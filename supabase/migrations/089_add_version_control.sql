-- Add version control and idempotency support for authoritative multiplayer
-- Migration: 089_add_version_control.sql

-- Add version column to battle_sessions for optimistic locking
ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS version INTEGER DEFAULT 1;

-- Add idempotency support to battle_actions
ALTER TABLE public.battle_actions 
ADD COLUMN IF NOT EXISTS idempotency_key TEXT;

-- Create unique index on idempotency_key to prevent duplicates
CREATE UNIQUE INDEX IF NOT EXISTS idx_battle_actions_idempotency 
ON public.battle_actions (battle_session_id, idempotency_key) 
WHERE idempotency_key IS NOT NULL;

-- Add indexes for better real-time performance
CREATE INDEX IF NOT EXISTS idx_battle_sessions_version 
ON public.battle_sessions (id, version);

CREATE INDEX IF NOT EXISTS idx_battle_actions_realtime 
ON public.battle_actions (battle_session_id, created_at DESC);

-- Function to increment version on battle updates
CREATE OR REPLACE FUNCTION increment_battle_version()
RETURNS TRIGGER AS $$
BEGIN
  -- Only increment version for actual game state changes
  IF OLD.game_state IS DISTINCT FROM NEW.game_state THEN
    NEW.version = COALESCE(OLD.version, 0) + 1;
  END IF;
  
  -- Always update the timestamp
  NEW.updated_at = NOW();
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger to auto-increment version
DROP TRIGGER IF EXISTS battle_version_trigger ON public.battle_sessions;
CREATE TRIGGER battle_version_trigger
  BEFORE UPDATE ON public.battle_sessions
  FOR EACH ROW
  EXECUTE FUNCTION increment_battle_version();

-- Grant necessary permissions
GRANT SELECT, UPDATE ON public.battle_sessions TO authenticated;
GRANT SELECT, INSERT ON public.battle_actions TO authenticated;

-- Ensure real-time is enabled for both tables (skip if already exists)
DO $$
BEGIN
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_sessions;
    EXCEPTION WHEN duplicate_object THEN
        RAISE NOTICE 'Table battle_sessions already in publication supabase_realtime';
    END;
END $$;

DO $$
BEGIN
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_actions;
    EXCEPTION WHEN duplicate_object THEN
        RAISE NOTICE 'Table battle_actions already in publication supabase_realtime';
    END;
END $$;

-- Success message
SELECT 'Version control and idempotency added to battle system!' as status;