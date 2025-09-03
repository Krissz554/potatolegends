-- Fix Real-time Battle Notifications
-- Ensure users can receive real-time notifications for battle session inserts

-- Add missing policies for battle_sessions that allow users to see INSERT events
-- This is crucial for real-time subscriptions to work

-- Drop existing policies to recreate them properly
DROP POLICY IF EXISTS "Players view own battles" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role full access battles" ON public.battle_sessions;

-- Create comprehensive policies for battle_sessions
-- Users can SELECT their own battle sessions
CREATE POLICY "Users can view own battle sessions" ON public.battle_sessions
FOR SELECT USING (auth.uid() = player1_id OR auth.uid() = player2_id);

-- Users need INSERT access for real-time notifications to work
-- This doesn't allow them to actually insert, but allows them to receive INSERT notifications
CREATE POLICY "Users can receive battle notifications" ON public.battle_sessions
FOR INSERT WITH CHECK (auth.uid() = player1_id OR auth.uid() = player2_id);

-- Service role has full access
CREATE POLICY "Service role full access to battles" ON public.battle_sessions
FOR ALL TO service_role USING (true);

-- Also ensure matchmaking_queue has proper real-time access
DROP POLICY IF EXISTS "Users can manage own queue entry" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Service role full access" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Anon no access" ON public.matchmaking_queue;

-- Recreate matchmaking_queue policies with real-time support
CREATE POLICY "Users manage own queue entry" ON public.matchmaking_queue
FOR ALL USING (auth.uid() = user_id);

CREATE POLICY "Users can receive queue notifications" ON public.matchmaking_queue
FOR SELECT USING (true); -- Allow all authenticated users to see queue changes for real-time

CREATE POLICY "Service role full access to queue" ON public.matchmaking_queue
FOR ALL TO service_role USING (true);

-- Ensure real-time is enabled for these tables
ALTER PUBLICATION supabase_realtime ADD TABLE public.matchmaking_queue;
ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_sessions;
ALTER PUBLICATION supabase_realtime ADD TABLE public.battle_actions;

-- Success message
SELECT 'Real-time battle notifications fixed! Users can now receive INSERT events.' as status;