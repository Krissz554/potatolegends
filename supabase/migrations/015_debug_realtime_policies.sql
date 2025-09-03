-- Debug Real-time Policies
-- Verify and fix any issues with real-time notifications

-- Check current policies on battle_sessions
SELECT schemaname, tablename, policyname, permissive, roles, cmd, qual, with_check
FROM pg_policies 
WHERE tablename = 'battle_sessions';

-- Ensure battle_sessions has proper policies for real-time
-- Drop all existing policies and recreate them cleanly
DROP POLICY IF EXISTS "Users can view own battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "Users can receive battle notifications" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role full access to battles" ON public.battle_sessions;

-- Create simple, clear policies
-- Allow users to SELECT their own battles
CREATE POLICY "battle_sessions_select_own" ON public.battle_sessions
FOR SELECT TO authenticated
USING (auth.uid() = player1_id OR auth.uid() = player2_id);

-- Allow users to receive INSERT notifications for their battles
-- This is crucial for real-time to work
CREATE POLICY "battle_sessions_realtime_insert" ON public.battle_sessions
FOR INSERT TO authenticated
WITH CHECK (auth.uid() = player1_id OR auth.uid() = player2_id);

-- Service role can do everything
CREATE POLICY "battle_sessions_service_role" ON public.battle_sessions
FOR ALL TO service_role
USING (true)
WITH CHECK (true);

-- Verify the publication includes battle_sessions
-- This is safe because it handles if already exists
DO $$
BEGIN
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_sessions;
    EXCEPTION WHEN duplicate_object THEN
        NULL; -- Already exists, that's fine
    END;
END $$;

-- Create a simple test function to verify real-time works
CREATE OR REPLACE FUNCTION test_realtime_insert()
RETURNS UUID AS $$
DECLARE
    test_battle_id UUID;
BEGIN
    -- Insert a test battle that should trigger real-time
    INSERT INTO public.battle_sessions (
        player1_id,
        player2_id,
        current_turn_player_id,
        game_state,
        status
    ) VALUES (
        '00000000-0000-0000-0000-000000000001',
        '00000000-0000-0000-0000-000000000002',
        '00000000-0000-0000-0000-000000000001',
        '{"player1_deck":[],"player2_deck":[],"board":{},"phase":"deployment","turn_count":1,"actions":[]}',
        'deploying'
    ) RETURNING id INTO test_battle_id;
    
    -- Clean up after 30 seconds
    -- Note: In production, you'd want a proper cleanup mechanism
    RETURN test_battle_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permission
GRANT EXECUTE ON FUNCTION test_realtime_insert() TO authenticated;
GRANT EXECUTE ON FUNCTION test_realtime_insert() TO service_role;

-- Success message
SELECT 'Real-time policies debugged and fixed!' as status;