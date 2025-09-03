-- Clean up stale battles and improve matchmaking logic
-- Fix the "already in active battle" issue

-- First, clean up any stale battle sessions
-- Remove battles that are older than 1 hour and not finished
DELETE FROM public.battle_sessions 
WHERE status IN ('deploying', 'battling') 
AND started_at < NOW() - INTERVAL '1 hour';

-- Clean up any old queue entries (older than 10 minutes)
DELETE FROM public.matchmaking_queue 
WHERE joined_at < NOW() - INTERVAL '10 minutes';

-- Update the cleanup function to be more aggressive with stale data
CREATE OR REPLACE FUNCTION cleanup_stale_matchmaking_data()
RETURNS void AS $$
BEGIN
    -- Remove old queue entries (older than 5 minutes)
    DELETE FROM public.matchmaking_queue 
    WHERE joined_at < NOW() - INTERVAL '5 minutes';
    
    -- Remove stale battle sessions (older than 30 minutes and not finished)
    DELETE FROM public.battle_sessions 
    WHERE status IN ('deploying', 'battling') 
    AND started_at < NOW() - INTERVAL '30 minutes';
    
    -- Also remove battles where both players haven't been active
    -- (This is a more aggressive cleanup for stuck battles)
    DELETE FROM public.battle_sessions 
    WHERE status IN ('deploying', 'battling') 
    AND started_at < NOW() - INTERVAL '10 minutes'
    AND (
        game_state->>'actions' IS NULL 
        OR jsonb_array_length(COALESCE(game_state->'actions', '[]'::jsonb)) = 0
    );
    
    -- Log the cleanup
    INSERT INTO public.matchmaking_logs (event_type, message, created_at)
    VALUES ('aggressive_cleanup', 'Cleaned up stale matchmaking data and inactive battles', NOW());
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create a function to force-reset a user's matchmaking state
CREATE OR REPLACE FUNCTION reset_user_matchmaking_state(target_user_id UUID)
RETURNS void AS $$
BEGIN
    -- Remove from queue
    DELETE FROM public.matchmaking_queue 
    WHERE user_id = target_user_id;
    
    -- Mark any active battles as finished (emergency cleanup)
    UPDATE public.battle_sessions 
    SET status = 'finished', 
        finished_at = NOW(),
        winner_id = CASE 
            WHEN player1_id = target_user_id THEN player2_id 
            ELSE player1_id 
        END
    WHERE (player1_id = target_user_id OR player2_id = target_user_id)
    AND status IN ('deploying', 'battling');
    
    -- Log the reset
    INSERT INTO public.matchmaking_logs (event_type, message, additional_data, created_at)
    VALUES ('user_reset', 'Force reset user matchmaking state', 
            jsonb_build_object('user_id', target_user_id), NOW());
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Run aggressive cleanup now
SELECT cleanup_stale_matchmaking_data();

-- Create a view to easily check current matchmaking state
CREATE OR REPLACE VIEW matchmaking_status AS
SELECT 
    'queue' as type,
    user_id,
    joined_at as created_at,
    status,
    NULL as battle_id
FROM public.matchmaking_queue
UNION ALL
SELECT 
    'battle' as type,
    player1_id as user_id,
    started_at as created_at,
    status,
    id as battle_id
FROM public.battle_sessions 
WHERE status IN ('deploying', 'battling')
UNION ALL
SELECT 
    'battle' as type,
    player2_id as user_id,
    started_at as created_at,
    status,
    id as battle_id
FROM public.battle_sessions 
WHERE status IN ('deploying', 'battling')
ORDER BY created_at DESC;

-- Grant access to the view
GRANT SELECT ON matchmaking_status TO authenticated;
GRANT SELECT ON matchmaking_status TO service_role;

-- Success message
SELECT 'Stale battles cleaned up and matchmaking logic improved!' as status;