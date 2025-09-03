-- Comprehensive Matchmaking Fix
-- This migration completely fixes RLS and stale data issues

-- First, let's disable RLS temporarily on matchmaking_queue to test
-- We'll re-enable it with proper policies
ALTER TABLE public.matchmaking_queue DISABLE ROW LEVEL SECURITY;

-- Clean up any stale battle sessions that might be blocking new matches
DELETE FROM public.battle_sessions 
WHERE status IN ('deploying', 'battling') 
AND started_at < NOW() - INTERVAL '10 minutes';

-- Clean up any old queue entries
DELETE FROM public.matchmaking_queue 
WHERE joined_at < NOW() - INTERVAL '10 minutes';

-- Re-enable RLS with comprehensive policies
ALTER TABLE public.matchmaking_queue ENABLE ROW LEVEL SECURITY;

-- Drop all existing policies to start fresh
DROP POLICY IF EXISTS "Users can manage their own queue entry" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Service role can view all queue entries" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Service role can delete queue entries" ON public.matchmaking_queue;

-- Create comprehensive policies for matchmaking_queue
-- Users can manage their own entries
CREATE POLICY "Users can manage own queue entry" ON public.matchmaking_queue
FOR ALL USING (auth.uid() = user_id);

-- Service role has full access (this is crucial for the edge function)
CREATE POLICY "Service role full access" ON public.matchmaking_queue
FOR ALL TO service_role USING (true);

-- Anon role can't access (security)
CREATE POLICY "Anon no access" ON public.matchmaking_queue
FOR ALL TO anon USING (false);

-- Fix battle_sessions policies too
ALTER TABLE public.battle_sessions DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.battle_sessions ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Players can view their own battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "Service role can manage battle sessions" ON public.battle_sessions;
DROP POLICY IF EXISTS "System can manage battle sessions" ON public.battle_sessions;

-- Battle sessions policies
CREATE POLICY "Players view own battles" ON public.battle_sessions
FOR SELECT USING (auth.uid() = player1_id OR auth.uid() = player2_id);

CREATE POLICY "Service role full access battles" ON public.battle_sessions
FOR ALL TO service_role USING (true);

-- Fix battle_decks policies
ALTER TABLE public.battle_decks DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.battle_decks ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Users can manage their own battle decks" ON public.battle_decks;
DROP POLICY IF EXISTS "Service role can view all battle decks" ON public.battle_decks;

CREATE POLICY "Users manage own decks" ON public.battle_decks
FOR ALL USING (auth.uid() = user_id);

CREATE POLICY "Service role full access decks" ON public.battle_decks
FOR ALL TO service_role USING (true);

-- Fix user_deck_settings policies
ALTER TABLE public.user_deck_settings DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.user_deck_settings ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Users can view their own deck settings" ON public.user_deck_settings;
DROP POLICY IF EXISTS "Users can insert their own deck settings" ON public.user_deck_settings;
DROP POLICY IF EXISTS "Users can update their own deck settings" ON public.user_deck_settings;
DROP POLICY IF EXISTS "Service role can view user deck settings" ON public.user_deck_settings;

CREATE POLICY "Users manage own deck settings" ON public.user_deck_settings
FOR ALL USING (auth.uid() = user_id);

CREATE POLICY "Service role full access deck settings" ON public.user_deck_settings
FOR ALL TO service_role USING (true);

-- Create a function to clean up stale data
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
    
    -- Log the cleanup
    INSERT INTO public.matchmaking_logs (event_type, message, created_at)
    VALUES ('cleanup', 'Cleaned up stale matchmaking data', NOW());
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Run initial cleanup
SELECT cleanup_stale_matchmaking_data();

-- Success message
SELECT 'Comprehensive matchmaking fix applied! RLS policies updated and stale data cleaned.' as status;