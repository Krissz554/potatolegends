-- Test Real-time Directly
-- Create a simple test to verify real-time notifications are working

-- First, let's verify the realtime publication includes our tables
SELECT tablename FROM pg_publication_tables WHERE pubname = 'supabase_realtime';

-- Create a simple test function that both creates a battle and logs it
CREATE OR REPLACE FUNCTION test_realtime_battle_creation(
    test_player1_id UUID DEFAULT '00000000-0000-0000-0000-000000000001',
    test_player2_id UUID DEFAULT '00000000-0000-0000-0000-000000000002'
)
RETURNS TABLE(
    battle_id UUID,
    created_at TIMESTAMP WITH TIME ZONE,
    message TEXT
) AS $$
DECLARE
    new_battle_id UUID;
    new_battle_created_at TIMESTAMP WITH TIME ZONE;
BEGIN
    -- Insert a test battle
    INSERT INTO public.battle_sessions (
        player1_id,
        player2_id,
        current_turn_player_id,
        game_state,
        status
    ) VALUES (
        test_player1_id,
        test_player2_id,
        test_player1_id,
        jsonb_build_object(
            'player1_deck', '[]'::jsonb,
            'player2_deck', '[]'::jsonb,
            'board', '{}'::jsonb,
            'phase', 'deployment',
            'turn_count', 1,
            'actions', '[]'::jsonb
        ),
        'deploying'
    ) RETURNING id, started_at INTO new_battle_id, new_battle_created_at;
    
    -- Log it
    INSERT INTO public.matchmaking_logs (event_type, message, additional_data, created_at)
    VALUES (
        'test_realtime',
        'Test battle created for real-time verification',
        jsonb_build_object(
            'battle_id', new_battle_id,
            'player1_id', test_player1_id,
            'player2_id', test_player2_id
        ),
        NOW()
    );
    
    -- Return the result
    RETURN QUERY SELECT 
        new_battle_id,
        new_battle_created_at,
        'Test battle created - check if real-time notification was received'::TEXT;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION test_realtime_battle_creation(UUID, UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION test_realtime_battle_creation(UUID, UUID) TO service_role;

-- Also create a function to clean up test battles
CREATE OR REPLACE FUNCTION cleanup_test_battles()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM public.battle_sessions 
    WHERE player1_id = '00000000-0000-0000-0000-000000000001'
    AND player2_id = '00000000-0000-0000-0000-000000000002';
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

GRANT EXECUTE ON FUNCTION cleanup_test_battles() TO authenticated;
GRANT EXECUTE ON FUNCTION cleanup_test_battles() TO service_role;

-- Let's also check if there are any issues with the current user authentication in real-time
-- Create a view to see current battle sessions that should trigger real-time
CREATE OR REPLACE VIEW current_active_battles AS
SELECT 
    id,
    player1_id,
    player2_id,
    status,
    started_at,
    'Battle that should trigger real-time for player1' as note
FROM public.battle_sessions 
WHERE status IN ('deploying', 'battling')
AND started_at > NOW() - INTERVAL '10 minutes'
ORDER BY started_at DESC;

GRANT SELECT ON current_active_battles TO authenticated;
GRANT SELECT ON current_active_battles TO service_role;

-- Run a test right now
SELECT * FROM test_realtime_battle_creation();

SELECT 'Real-time test setup complete! Check if notification was received.' as status;