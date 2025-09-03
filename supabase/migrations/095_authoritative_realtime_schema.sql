-- Create authoritative real-time multiplayer schema
-- Migration: 095_authoritative_realtime_schema.sql

-- Matches table holds canonical snapshot
CREATE TABLE IF NOT EXISTS public.matches (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  status TEXT NOT NULL CHECK (status IN ('pending','active','finished')) DEFAULT 'pending',
  p1 UUID NOT NULL,
  p2 UUID NOT NULL,
  current_player UUID,
  turn INT NOT NULL DEFAULT 1,
  version INT NOT NULL DEFAULT 0,        -- monotonic snapshot version
  state JSONB NOT NULL,                  -- canonical snapshot (hands, boards, timers, last_event_id)
  last_event_id BIGINT DEFAULT 0,        -- latest event id applied
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Append-only event log
CREATE TABLE IF NOT EXISTS public.game_events (
  id BIGSERIAL PRIMARY KEY,
  match_id UUID REFERENCES public.matches(id) ON DELETE CASCADE,
  actor UUID NOT NULL,
  type TEXT NOT NULL,
  payload JSONB NOT NULL,
  client_time TIMESTAMPTZ,
  server_time TIMESTAMPTZ DEFAULT NOW(),
  version_after INT NOT NULL,
  idempotency_key TEXT NOT NULL,
  processed BOOLEAN DEFAULT FALSE
);

-- Create indexes for performance
CREATE UNIQUE INDEX IF NOT EXISTS ux_game_events_match_idempotency 
ON public.game_events(match_id, idempotency_key);

CREATE INDEX IF NOT EXISTS idx_game_events_match_id 
ON public.game_events(match_id);

CREATE INDEX IF NOT EXISTS idx_game_events_version 
ON public.game_events(match_id, version_after);

CREATE INDEX IF NOT EXISTS idx_matches_players 
ON public.matches(p1, p2);

CREATE INDEX IF NOT EXISTS idx_matches_status 
ON public.matches(status);

-- Enable Row Level Security
ALTER TABLE public.matches ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.game_events ENABLE ROW LEVEL SECURITY;

-- RLS Policies for matches
CREATE POLICY "Users can view their own matches" ON public.matches
  FOR SELECT USING (auth.uid() = p1 OR auth.uid() = p2);

-- Deny direct client inserts/updates - only service role can modify
CREATE POLICY "Only service role can modify matches" ON public.matches
  FOR ALL USING (auth.role() = 'service_role');

-- RLS Policies for game_events  
CREATE POLICY "Users can view events for their matches" ON public.game_events
  FOR SELECT USING (
    EXISTS (
      SELECT 1 FROM public.matches 
      WHERE matches.id = game_events.match_id 
      AND (matches.p1 = auth.uid() OR matches.p2 = auth.uid())
    )
  );

-- Deny direct client inserts/updates - only service role can modify
CREATE POLICY "Only service role can modify game_events" ON public.game_events
  FOR ALL USING (auth.role() = 'service_role');

-- Grant permissions
GRANT SELECT ON public.matches TO authenticated;
GRANT SELECT ON public.game_events TO authenticated;
GRANT ALL ON public.matches TO service_role;
GRANT ALL ON public.game_events TO service_role;

-- Enable real-time for both tables
ALTER PUBLICATION supabase_realtime ADD TABLE public.matches;
ALTER PUBLICATION supabase_realtime ADD TABLE public.game_events;

-- Function to migrate existing battle_sessions to matches
CREATE OR REPLACE FUNCTION migrate_battle_to_matches()
RETURNS VOID AS $$
DECLARE
  battle RECORD;
  new_match_id UUID;
BEGIN
  -- Migrate active battle_sessions to matches table
  FOR battle IN 
    SELECT * FROM public.battle_sessions 
    WHERE status IN ('active', 'waiting_initialization', 'mulligan')
  LOOP
    -- Create corresponding match
    INSERT INTO public.matches (
      id,
      status,
      p1,
      p2,
      current_player,
      turn,
      version,
      state,
      last_event_id,
      created_at,
      updated_at
    ) VALUES (
      battle.id, -- Use same ID for compatibility
      CASE 
        WHEN battle.status = 'waiting_initialization' THEN 'pending'
        WHEN battle.status = 'mulligan' THEN 'pending'
        ELSE 'active'
      END,
      battle.player1_id,
      battle.player2_id,
      battle.current_turn_player_id,
      COALESCE(battle.turn_number, 1),
      COALESCE(battle.version, 1),
      battle.game_state,
      0, -- Initialize last_event_id
      battle.started_at,
      battle.updated_at
    )
    ON CONFLICT (id) DO NOTHING; -- Skip if already migrated
    
    RAISE NOTICE 'Migrated battle % to matches table', battle.id;
  END LOOP;
END;
$$ LANGUAGE plpgsql;

-- Run migration
SELECT migrate_battle_to_matches();

-- Success message
SELECT 'Authoritative real-time schema created successfully!' as status;