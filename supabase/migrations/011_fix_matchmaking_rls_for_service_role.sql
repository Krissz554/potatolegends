-- Fix Matchmaking RLS for Service Role Access
-- Allow the service role to see all queue entries for matchmaking

-- Add a policy that allows service role to select all queue entries
CREATE POLICY "Service role can view all queue entries" ON public.matchmaking_queue
FOR SELECT USING (auth.role() = 'service_role');

-- Add a policy that allows service role to delete queue entries (for cleanup)
CREATE POLICY "Service role can delete queue entries" ON public.matchmaking_queue
FOR DELETE USING (auth.role() = 'service_role');

-- Add a policy that allows service role to manage battle sessions
DROP POLICY IF EXISTS "System can manage battle sessions" ON public.battle_sessions;
CREATE POLICY "Service role can manage battle sessions" ON public.battle_sessions
FOR ALL USING (auth.role() = 'service_role');

-- Also ensure battle_decks can be read by service role for deck validation
CREATE POLICY "Service role can view all battle decks" ON public.battle_decks
FOR SELECT USING (auth.role() = 'service_role');

-- Allow service role to read user deck settings
CREATE POLICY "Service role can view user deck settings" ON public.user_deck_settings
FOR SELECT USING (auth.role() = 'service_role');

-- Success message
SELECT 'Matchmaking RLS policies updated for service role access!' as status;