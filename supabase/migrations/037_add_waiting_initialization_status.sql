-- Migration 037: Add waiting_initialization status to battle_sessions
-- Fixes the check constraint violation when matchmaker creates battles

-- Drop the existing status check constraint
ALTER TABLE public.battle_sessions 
DROP CONSTRAINT IF EXISTS battle_sessions_status_check;

-- Add the new constraint with the additional status
ALTER TABLE public.battle_sessions 
ADD CONSTRAINT battle_sessions_status_check 
CHECK (status IN ('waiting_initialization', 'deploying', 'active', 'battling', 'finished'));

-- Also add 'active' status which we use after initialization
-- Update any existing battles with old status to use the new flow
UPDATE public.battle_sessions 
SET status = 'waiting_initialization' 
WHERE status = 'deploying' 
AND game_state->>'phase' IN ('deployment', 'needs_initialization')
AND started_at > NOW() - INTERVAL '1 hour';

-- Add comment
COMMENT ON CONSTRAINT battle_sessions_status_check ON public.battle_sessions 
IS 'Allows waiting_initialization, deploying, active, battling, and finished statuses';

-- Success message
SELECT 'Added waiting_initialization status to battle_sessions constraint!' as status;