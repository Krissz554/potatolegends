-- Migration 038: Fix Real-time Battle Updates
-- Ensures battle sessions update in real-time for both players

-- First, ensure real-time is enabled for battle_sessions (skip if already exists)
DO $$
BEGIN
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_sessions;
    EXCEPTION 
        WHEN duplicate_object THEN 
            RAISE NOTICE 'Table battle_sessions already in publication supabase_realtime';
    END;
END $$;

-- Clean up conflicting RLS policies for battle_sessions
DROP POLICY IF EXISTS "Enhanced battle session access" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role can update battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "Players can update their own battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role full access" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role can manage battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "Players view own battles" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role full access battles" ON public.battle_sessions;
DROP POLICY IF EXISTS "Players can view their own battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "System can manage battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "battle_sessions_select_own" ON public.battle_sessions;
DROP POLICY IF EXISTS "battle_sessions_realtime_insert" ON public.battle_sessions;
DROP POLICY IF EXISTS "battle_sessions_service_role" ON public.battle_sessions;
DROP POLICY IF EXISTS "Users can view own battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "Users can receive battle notifications" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role full access to battles" ON public.battle_sessions;
DROP POLICY IF EXISTS "battles_users_own" ON public.battle_sessions;
DROP POLICY IF EXISTS "battles_users_realtime" ON public.battle_sessions;
DROP POLICY IF EXISTS "battles_service_role_all" ON public.battle_sessions;

-- Create clean, simple RLS policies for real-time (skip if they already exist)
-- Allow authenticated users to SELECT their own battles
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'battle_sessions' AND policyname = 'battle_sessions_select_policy') THEN
        CREATE POLICY "battle_sessions_select_policy" ON public.battle_sessions
          FOR SELECT TO authenticated
          USING (auth.uid() = player1_id OR auth.uid() = player2_id);
        RAISE NOTICE 'Created policy battle_sessions_select_policy';
    ELSE
        RAISE NOTICE 'Policy battle_sessions_select_policy already exists';
    END IF;
END $$;

-- Allow service role full access
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'battle_sessions' AND policyname = 'battle_sessions_service_role_policy') THEN
        CREATE POLICY "battle_sessions_service_role_policy" ON public.battle_sessions
          FOR ALL TO service_role
          USING (true)
          WITH CHECK (true);
        RAISE NOTICE 'Created policy battle_sessions_service_role_policy';
    ELSE
        RAISE NOTICE 'Policy battle_sessions_service_role_policy already exists';
    END IF;
END $$;

-- Allow authenticated users to receive real-time updates (needed for subscriptions)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'battle_sessions' AND policyname = 'battle_sessions_realtime_policy') THEN
        CREATE POLICY "battle_sessions_realtime_policy" ON public.battle_sessions
          FOR UPDATE TO authenticated
          USING (auth.uid() = player1_id OR auth.uid() = player2_id)
          WITH CHECK (auth.uid() = player1_id OR auth.uid() = player2_id);
        RAISE NOTICE 'Created policy battle_sessions_realtime_policy';
    ELSE
        RAISE NOTICE 'Policy battle_sessions_realtime_policy already exists';
    END IF;
END $$;

-- Also ensure battle_actions have proper real-time access (skip if already exists)
DO $$
BEGIN
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_actions;
    EXCEPTION 
        WHEN duplicate_object THEN 
            RAISE NOTICE 'Table battle_actions already in publication supabase_realtime';
    END;
END $$;

-- Ensure matchmaking_queue is in real-time publication for instant match detection
DO $$
BEGIN
    BEGIN
        ALTER PUBLICATION supabase_realtime ADD TABLE public.matchmaking_queue;
    EXCEPTION 
        WHEN duplicate_object THEN 
            RAISE NOTICE 'Table matchmaking_queue already in publication supabase_realtime';
    END;
END $$;

-- Clean up battle_actions policies
DROP POLICY IF EXISTS "Enhanced battle action access" ON public.battle_actions;
DROP POLICY IF EXISTS "Service role can manage battle actions" ON public.battle_actions;
DROP POLICY IF EXISTS "Users can read battle actions for their battles" ON public.battle_actions;
DROP POLICY IF EXISTS "Users can create battle actions in their battles" ON public.battle_actions;

-- Create clean battle_actions policies (skip if they already exist)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'battle_actions' AND policyname = 'battle_actions_select_policy') THEN
        CREATE POLICY "battle_actions_select_policy" ON public.battle_actions
          FOR SELECT TO authenticated
          USING (
            EXISTS (
              SELECT 1 FROM public.battle_sessions bs
              WHERE bs.id = battle_session_id 
              AND (bs.player1_id = auth.uid() OR bs.player2_id = auth.uid())
            )
          );
        RAISE NOTICE 'Created policy battle_actions_select_policy';
    ELSE
        RAISE NOTICE 'Policy battle_actions_select_policy already exists';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'battle_actions' AND policyname = 'battle_actions_service_role_policy') THEN
        CREATE POLICY "battle_actions_service_role_policy" ON public.battle_actions
          FOR ALL TO service_role
          USING (true)
          WITH CHECK (true);
        RAISE NOTICE 'Created policy battle_actions_service_role_policy';
    ELSE
        RAISE NOTICE 'Policy battle_actions_service_role_policy already exists';
    END IF;
END $$;

-- Add indexes to improve real-time performance
CREATE INDEX IF NOT EXISTS idx_battle_sessions_players ON public.battle_sessions(player1_id, player2_id);
CREATE INDEX IF NOT EXISTS idx_battle_sessions_updated_at ON public.battle_sessions(updated_at);
CREATE INDEX IF NOT EXISTS idx_battle_actions_session ON public.battle_actions(battle_session_id);

-- Add matchmaking_queue real-time policies (skip if they already exist)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'matchmaking_queue' AND policyname = 'matchmaking_queue_select_policy') THEN
        CREATE POLICY "matchmaking_queue_select_policy" ON public.matchmaking_queue
          FOR SELECT TO authenticated
          USING (auth.uid() = user_id);
        RAISE NOTICE 'Created policy matchmaking_queue_select_policy';
    ELSE
        RAISE NOTICE 'Policy matchmaking_queue_select_policy already exists';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'matchmaking_queue' AND policyname = 'matchmaking_queue_service_role_policy') THEN
        CREATE POLICY "matchmaking_queue_service_role_policy" ON public.matchmaking_queue
          FOR ALL TO service_role
          USING (true)
          WITH CHECK (true);
        RAISE NOTICE 'Created policy matchmaking_queue_service_role_policy';
    ELSE
        RAISE NOTICE 'Policy matchmaking_queue_service_role_policy already exists';
    END IF;
END $$;

-- Grant necessary permissions for real-time
GRANT SELECT ON public.battle_sessions TO authenticated;
GRANT SELECT ON public.battle_actions TO authenticated;
GRANT SELECT ON public.matchmaking_queue TO authenticated;

-- Success message
SELECT 'Real-time battle updates configuration completed!' as status;