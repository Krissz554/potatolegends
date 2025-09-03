-- =====================================================
-- ENHANCED BATTLE SYSTEM FOR REAL-TIME GAMEPLAY
-- =====================================================

-- Add more detailed game state tracking to battle_sessions
ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS round_number INTEGER DEFAULT 1;

ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS turn_number INTEGER DEFAULT 1;

-- Add battle statistics tracking
ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS player1_cards_remaining INTEGER DEFAULT 5;

ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS player2_cards_remaining INTEGER DEFAULT 5;

-- Add updated_at column if it doesn't exist
ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW();

-- Ensure battle_actions table supports all action types
ALTER TABLE public.battle_actions 
DROP CONSTRAINT IF EXISTS battle_actions_action_type_check;

ALTER TABLE public.battle_actions 
ADD CONSTRAINT battle_actions_action_type_check 
CHECK (action_type IN ('deploy', 'attack', 'surrender', 'card_select', 'damage_dealt', 'card_destroyed', 'game_end'));

-- Add indexes for better performance on battle queries
CREATE INDEX IF NOT EXISTS idx_battle_sessions_players ON public.battle_sessions (player1_id, player2_id);
CREATE INDEX IF NOT EXISTS idx_battle_sessions_status_started ON public.battle_sessions (status, started_at);
CREATE INDEX IF NOT EXISTS idx_battle_actions_battle_session ON public.battle_actions (battle_session_id, created_at);

-- Add function to update battle statistics
CREATE OR REPLACE FUNCTION update_battle_stats()
RETURNS trigger AS $$
BEGIN
  -- Update turn counter when actions are created
  IF NEW.action_type IN ('deploy', 'attack', 'card_select') THEN
    UPDATE public.battle_sessions 
    SET turn_number = turn_number + 1,
        updated_at = NOW()
    WHERE id = NEW.battle_session_id;
  END IF;
  
  -- Update cards remaining when cards are destroyed
  IF NEW.action_type = 'card_destroyed' THEN
    IF NEW.player_id = (SELECT player1_id FROM public.battle_sessions WHERE id = NEW.battle_session_id) THEN
      UPDATE public.battle_sessions 
      SET player1_cards_remaining = GREATEST(0, player1_cards_remaining - 1)
      WHERE id = NEW.battle_session_id;
    ELSE
      UPDATE public.battle_sessions 
      SET player2_cards_remaining = GREATEST(0, player2_cards_remaining - 1)
      WHERE id = NEW.battle_session_id;
    END IF;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create trigger for battle statistics
DROP TRIGGER IF EXISTS battle_stats_trigger ON public.battle_actions;
CREATE TRIGGER battle_stats_trigger
  AFTER INSERT ON public.battle_actions
  FOR EACH ROW
  EXECUTE FUNCTION update_battle_stats();

-- Grant execute permission to service role and authenticated users
GRANT EXECUTE ON FUNCTION update_battle_stats() TO service_role;
GRANT EXECUTE ON FUNCTION update_battle_stats() TO authenticated;

-- Success message
SELECT 'Enhanced battle system created successfully!' as status;