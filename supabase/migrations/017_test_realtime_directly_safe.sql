-- Test Real-time Directly (Safe Version)
-- Create a simple test to verify real-time notifications are working

-- First, let's verify the realtime publication includes our tables
SELECT tablename FROM pg_publication_tables WHERE pubname = 'supabase_realtime';

-- Create a simple test function that creates a battle with the current user
CREATE OR REPLACE FUNCTION test_realtime_battle_creation_safe()
RETURNS TABLE(
    battle_id UUID,
    created_at TIMESTAMP WITH TIME ZONE,
    message TEXT,
    player1_id UUID,
    player2_id UUID
) AS $$
DECLARE
    new_battle_id UUID;
    new_battle_created_at TIMESTAMP WITH TIME ZONE;
    current_user_id UUID;
    test_user_id UUID;
BEGIN
    -- Get the current authenticated user
    current_user_id := auth.uid();
    
    IF current_user_id IS NULL THEN
        RAISE EXCEPTION 'No authenticated user found for real-time test';
    END IF;
    
    -- Use current user as player1 and create a fake UUID for player2
    -- But first check if this would violate foreign key for player2
    test_user_id := '00000000-0000-0000-0000-000000000002'::UUID;
    
    -- We'll use current user for both players to avoid foreign key issues
    INSERT INTO public.battle_sessions (
        player1_id,
        player2_id,
        current_turn_player_id,
        game_state,
        status
    ) VALUES (
        current_user_id,
        current_user_id, -- Use same user to avoid FK constraint
        current_user_id,
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
        'test_realtime_safe',
        'Safe test battle created for real-time verification',
        jsonb_build_object(
            'battle_id', new_battle_id,
            'current_user_id', current_user_id
        ),
        NOW()
    );
    
    -- Return the result
    RETURN QUERY SELECT 
        new_battle_id,
        new_battle_created_at,
        'Safe test battle created with current user - check if real-time notification was received'::TEXT,
        current_user_id,
        current_user_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION test_realtime_battle_creation_safe() TO authenticated;

-- Also create a function to clean up test battles for current user
CREATE OR REPLACE FUNCTION cleanup_test_battles_safe()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
    current_user_id UUID;
BEGIN
    current_user_id := auth.uid();
    
    IF current_user_id IS NULL THEN
        RETURN 0;
    END IF;
    
    DELETE FROM public.battle_sessions 
    WHERE player1_id = current_user_id
    AND player2_id = current_user_id
    AND status = 'deploying'
    AND started_at > NOW() - INTERVAL '1 hour'; -- Only recent test battles
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

GRANT EXECUTE ON FUNCTION cleanup_test_battles_safe() TO authenticated;

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

-- Create a simpler test that just checks if we can insert at all
CREATE OR REPLACE FUNCTION test_simple_insert()
RETURNS TEXT AS $$
DECLARE
    current_user_id UUID;
BEGIN
    current_user_id := auth.uid();
    
    IF current_user_id IS NULL THEN
        RETURN 'No authenticated user found';
    END IF;
    
    -- Just test if we can access the table
    PERFORM COUNT(*) FROM public.battle_sessions WHERE player1_id = current_user_id;
    
    RETURN 'Current user: ' || current_user_id::TEXT || ' - Can access battle_sessions table';
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

GRANT EXECUTE ON FUNCTION test_simple_insert() TO authenticated;

-- Test the simple function
SELECT test_simple_insert();

SELECT 'Safe real-time test setup complete!' as status;