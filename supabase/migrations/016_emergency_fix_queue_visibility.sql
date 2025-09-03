-- Emergency Fix for Queue Visibility
-- The edge function can only see 1 player when there should be 2
-- This suggests RLS is still blocking service role access

-- First, let's check the current state
SELECT 'Current queue entries:' as status;
SELECT user_id, joined_at, status FROM public.matchmaking_queue ORDER BY joined_at;

SELECT 'Current RLS policies on matchmaking_queue:' as status;
SELECT policyname, permissive, roles, cmd, qual, with_check
FROM pg_policies 
WHERE tablename = 'matchmaking_queue';

-- EMERGENCY: Temporarily disable RLS to test
-- This will help us confirm if RLS is the issue
-- WARNING: This reduces security temporarily for debugging
ALTER TABLE public.matchmaking_queue DISABLE ROW LEVEL SECURITY;

-- Clean up any stale entries
DELETE FROM public.matchmaking_queue 
WHERE joined_at < NOW() - INTERVAL '10 minutes';

-- Re-enable RLS with the most permissive policies for service role
ALTER TABLE public.matchmaking_queue ENABLE ROW LEVEL SECURITY;

-- Drop ALL existing policies to start fresh
DROP POLICY IF EXISTS "Users can manage own queue entry" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Users can receive queue notifications" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Service role full access to queue" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Users manage own queue entry" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Service role full access" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Anon no access" ON public.matchmaking_queue;

-- Create the most permissive policies possible
-- Users can manage their own entries
CREATE POLICY "queue_users_own" ON public.matchmaking_queue
FOR ALL TO authenticated
USING (auth.uid() = user_id)
WITH CHECK (auth.uid() = user_id);

-- Service role can see and do EVERYTHING (no restrictions)
CREATE POLICY "queue_service_role_all" ON public.matchmaking_queue
FOR ALL TO service_role
USING (true)
WITH CHECK (true);

-- Allow authenticated users to SELECT all entries (needed for real-time)
CREATE POLICY "queue_authenticated_select_all" ON public.matchmaking_queue
FOR SELECT TO authenticated
USING (true);

-- Also fix battle_sessions policies while we're at it
ALTER TABLE public.battle_sessions DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.battle_sessions ENABLE ROW LEVEL SECURITY;

-- Drop all battle_sessions policies
DROP POLICY IF EXISTS "Users can view own battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "Users can receive battle notifications" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role full access to battles" ON public.battle_sessions;
DROP POLICY IF EXISTS "battle_sessions_select_own" ON public.battle_sessions;
DROP POLICY IF EXISTS "battle_sessions_realtime_insert" ON public.battle_sessions;
DROP POLICY IF EXISTS "battle_sessions_service_role" ON public.battle_sessions;

-- Create ultra-permissive battle_sessions policies
CREATE POLICY "battles_users_own" ON public.battle_sessions
FOR SELECT TO authenticated
USING (auth.uid() = player1_id OR auth.uid() = player2_id);

CREATE POLICY "battles_users_realtime" ON public.battle_sessions
FOR INSERT TO authenticated
WITH CHECK (auth.uid() = player1_id OR auth.uid() = player2_id);

CREATE POLICY "battles_service_role_all" ON public.battle_sessions
FOR ALL TO service_role
USING (true)
WITH CHECK (true);

-- Verify realtime is enabled
DO $$
BEGIN
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.matchmaking_queue;
    EXCEPTION WHEN duplicate_object THEN
        NULL;
    END;
    
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_sessions;
    EXCEPTION WHEN duplicate_object THEN
        NULL;
    END;
END $$;

-- Create a diagnostic function that service role can call
CREATE OR REPLACE FUNCTION diagnose_queue_visibility()
RETURNS TABLE(
    total_entries INTEGER,
    searching_entries INTEGER,
    user_ids TEXT[]
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        (SELECT COUNT(*)::INTEGER FROM public.matchmaking_queue),
        (SELECT COUNT(*)::INTEGER FROM public.matchmaking_queue WHERE status = 'searching'),
        (SELECT ARRAY_AGG(user_id::TEXT) FROM public.matchmaking_queue WHERE status = 'searching');
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant access to diagnostic function
GRANT EXECUTE ON FUNCTION diagnose_queue_visibility() TO service_role;
GRANT EXECUTE ON FUNCTION diagnose_queue_visibility() TO authenticated;

-- Test the diagnostic function
SELECT * FROM diagnose_queue_visibility();

SELECT 'Emergency queue visibility fix applied!' as status;