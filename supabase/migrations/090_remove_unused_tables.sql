-- Remove unused/old tables to clean up database
-- Migration: 090_remove_unused_tables.sql

-- Remove old match system tables (replaced by battle_sessions)
DROP TABLE IF EXISTS public.match_events CASCADE;
DROP TABLE IF EXISTS public.match_players CASCADE;
DROP TABLE IF EXISTS public.match_states CASCADE;
DROP TABLE IF EXISTS public.matches CASCADE;
DROP TABLE IF EXISTS public.turns CASCADE;

-- Remove old deck system tables (replaced by decks + deck_cards)
DROP TABLE IF EXISTS public.player_deck_cards CASCADE;
DROP TABLE IF EXISTS public.player_decks CASCADE;
DROP TABLE IF EXISTS public.battle_decks CASCADE;

-- Remove old potato system tables (replaced by card_complete)
DROP TABLE IF EXISTS public.potato_collections CASCADE;
DROP TABLE IF EXISTS public.potato_registry CASCADE;
DROP TABLE IF EXISTS public.potato_unlocks CASCADE;

-- Remove duplicate collection table (keep collections, remove collection)
DROP TABLE IF EXISTS public.collection CASCADE;

-- Remove unused feature tables
DROP TABLE IF EXISTS public.deck_templates CASCADE;
DROP TABLE IF EXISTS public.deck_template_cards CASCADE;
DROP TABLE IF EXISTS public.user_deck_settings CASCADE;

-- Remove unused keyword system
DROP TABLE IF EXISTS public.card_keywords CASCADE;
DROP TABLE IF EXISTS public.keywords CASCADE;
DROP TABLE IF EXISTS public.card_sets CASCADE;

-- Remove debug/cleanup tables
DROP TABLE IF EXISTS public.registration_fix_results CASCADE;
DROP TABLE IF EXISTS public.timer_heartbeat CASCADE;
DROP TABLE IF EXISTS public.matchmaking_logs CASCADE;

-- Drop sequences if they exist
DROP SEQUENCE IF EXISTS registration_fix_results_id_seq CASCADE;
DROP SEQUENCE IF EXISTS timer_heartbeat_id_seq CASCADE;

-- Remove triggers and functions that reference removed tables
DROP TRIGGER IF EXISTS matchmaker_trigger ON public.matchmaking_queue CASCADE;
DROP FUNCTION IF EXISTS trigger_matchmaker() CASCADE;
DROP FUNCTION IF EXISTS cleanup_old_queue_entries() CASCADE;

-- Remove any other functions that might reference removed tables
DROP FUNCTION IF EXISTS update_battle_stats() CASCADE;
DROP TRIGGER IF EXISTS battle_stats_trigger ON public.battle_actions CASCADE;

-- Success message
SELECT 'Cleaned up unused database tables, triggers, and functions! Removed old match system, potato system, and debug tables.' as status;