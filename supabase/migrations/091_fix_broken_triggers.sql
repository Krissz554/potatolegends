-- Fix broken triggers and functions after table cleanup
-- Migration: 091_fix_broken_triggers.sql

-- Remove triggers and functions that reference removed tables
DROP TRIGGER IF EXISTS matchmaker_trigger ON public.matchmaking_queue CASCADE;
DROP FUNCTION IF EXISTS trigger_matchmaker() CASCADE;
DROP FUNCTION IF EXISTS cleanup_old_queue_entries() CASCADE;

-- Remove any other functions that might reference removed tables
DROP FUNCTION IF EXISTS update_battle_stats() CASCADE;
DROP TRIGGER IF EXISTS battle_stats_trigger ON public.battle_actions CASCADE;

-- Success message
SELECT 'Fixed broken triggers and functions after table cleanup!' as status;