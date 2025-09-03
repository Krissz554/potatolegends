-- =====================================================
-- BATTLE SYSTEM TABLES
-- =====================================================
-- Real-time multiplayer battle system

-- Battle Decks Table - Store user's selected 5-card battle decks
CREATE TABLE IF NOT EXISTS public.battle_decks (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  potato_id UUID REFERENCES public.potato_registry(id) ON DELETE CASCADE NOT NULL,
  deck_position INTEGER NOT NULL CHECK (deck_position >= 1 AND deck_position <= 5),
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  
  -- Ensure user can only have 5 cards max, one per position
  UNIQUE(user_id, deck_position),
  -- Prevent duplicate cards in same deck
  UNIQUE(user_id, potato_id)
);

-- Matchmaking Queue Table - Track users looking for battles
CREATE TABLE IF NOT EXISTS public.matchmaking_queue (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL UNIQUE,
  joined_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  status TEXT DEFAULT 'searching' CHECK (status IN ('searching', 'matched', 'cancelled')),
  
  -- Auto-cleanup old queue entries (5 minutes)
  CONSTRAINT valid_queue_time CHECK (joined_at > NOW() - INTERVAL '5 minutes')
);

-- Battle Sessions Table - Track active battles
CREATE TABLE IF NOT EXISTS public.battle_sessions (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  player1_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  player2_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  current_turn_player_id UUID REFERENCES public.user_profiles(id),
  game_state JSONB DEFAULT '{}', -- Store battle state, deployed cards, HP, etc.
  status TEXT DEFAULT 'deploying' CHECK (status IN ('deploying', 'battling', 'finished')),
  winner_id UUID REFERENCES public.user_profiles(id),
  started_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  finished_at TIMESTAMP WITH TIME ZONE,
  
  -- Ensure valid players
  CONSTRAINT different_players CHECK (player1_id != player2_id),
  -- Current turn must be one of the players
  CONSTRAINT valid_turn_player CHECK (
    current_turn_player_id IS NULL OR 
    current_turn_player_id = player1_id OR 
    current_turn_player_id = player2_id
  )
);

-- Battle Actions Table - Track all battle actions for replay/debugging
CREATE TABLE IF NOT EXISTS public.battle_actions (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  battle_session_id UUID REFERENCES public.battle_sessions(id) ON DELETE CASCADE NOT NULL,
  player_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  action_type TEXT NOT NULL CHECK (action_type IN ('deploy', 'attack', 'surrender')),
  action_data JSONB DEFAULT '{}', -- Card deployed, damage dealt, etc.
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_battle_decks_user_id ON public.battle_decks (user_id);
CREATE INDEX IF NOT EXISTS idx_matchmaking_queue_status ON public.matchmaking_queue (status, joined_at);
CREATE INDEX IF NOT EXISTS idx_battle_sessions_players ON public.battle_sessions (player1_id, player2_id);
CREATE INDEX IF NOT EXISTS idx_battle_sessions_status ON public.battle_sessions (status, started_at);
CREATE INDEX IF NOT EXISTS idx_battle_actions_session ON public.battle_actions (battle_session_id, created_at);

-- Row Level Security (RLS) Policies
ALTER TABLE public.battle_decks ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.matchmaking_queue ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.battle_sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.battle_actions ENABLE ROW LEVEL SECURITY;

-- Battle Decks Policies
CREATE POLICY "Users can manage their own battle decks" ON public.battle_decks
FOR ALL USING (auth.uid() = user_id);

-- Matchmaking Queue Policies  
CREATE POLICY "Users can manage their own queue entry" ON public.matchmaking_queue
FOR ALL USING (auth.uid() = user_id);

-- Battle Sessions Policies
CREATE POLICY "Players can view their own battle sessions" ON public.battle_sessions
FOR SELECT USING (auth.uid() = player1_id OR auth.uid() = player2_id);

CREATE POLICY "System can manage battle sessions" ON public.battle_sessions
FOR ALL USING (true); -- Will be restricted by application logic

-- Battle Actions Policies
CREATE POLICY "Players can view actions in their battles" ON public.battle_actions
FOR SELECT USING (
  EXISTS (
    SELECT 1 FROM public.battle_sessions 
    WHERE id = battle_session_id 
    AND (player1_id = auth.uid() OR player2_id = auth.uid())
  )
);

CREATE POLICY "Players can create actions in their battles" ON public.battle_actions
FOR INSERT WITH CHECK (
  auth.uid() = player_id AND
  EXISTS (
    SELECT 1 FROM public.battle_sessions 
    WHERE id = battle_session_id 
    AND (player1_id = auth.uid() OR player2_id = auth.uid())
  )
);

-- Realtime subscriptions
ALTER PUBLICATION supabase_realtime ADD TABLE public.matchmaking_queue;
ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_sessions;
ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_actions;

-- Auto-cleanup function for expired queue entries
CREATE OR REPLACE FUNCTION cleanup_expired_queue_entries()
RETURNS void AS $$
BEGIN
  DELETE FROM public.matchmaking_queue 
  WHERE joined_at < NOW() - INTERVAL '10 minutes' 
     OR status != 'searching';
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger matchmaking check when new players join
CREATE OR REPLACE FUNCTION trigger_matchmaking()
RETURNS trigger AS $$
BEGIN
  -- Only trigger on INSERT of searching status
  IF TG_OP = 'INSERT' AND NEW.status = 'searching' THEN
    -- Check if we have enough players for a match
    PERFORM pg_notify('matchmaking_check', 'check_for_matches');
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create trigger
DROP TRIGGER IF EXISTS matchmaking_trigger ON public.matchmaking_queue;
CREATE TRIGGER matchmaking_trigger
  AFTER INSERT ON public.matchmaking_queue
  FOR EACH ROW
  EXECUTE FUNCTION trigger_matchmaking();

-- Comments
COMMENT ON FUNCTION cleanup_expired_queue_entries() IS 'Removes expired matchmaking queue entries';
COMMENT ON FUNCTION trigger_matchmaking() IS 'Triggers matchmaking check when players join queue';

-- Success message
SELECT 'Battle system with matchmaking triggers created successfully!' as status;