-- Fix Matchmaking Foreign Key References
-- The matchmaking tables should reference auth.users directly, not user_profiles

-- Drop existing foreign key constraints
ALTER TABLE public.matchmaking_queue 
DROP CONSTRAINT IF EXISTS matchmaking_queue_user_id_fkey;

ALTER TABLE public.battle_decks 
DROP CONSTRAINT IF EXISTS battle_decks_user_id_fkey;

ALTER TABLE public.battle_sessions
DROP CONSTRAINT IF EXISTS battle_sessions_player1_id_fkey;

ALTER TABLE public.battle_sessions
DROP CONSTRAINT IF EXISTS battle_sessions_player2_id_fkey;

ALTER TABLE public.battle_sessions
DROP CONSTRAINT IF EXISTS battle_sessions_current_turn_player_id_fkey;

ALTER TABLE public.battle_sessions
DROP CONSTRAINT IF EXISTS battle_sessions_winner_id_fkey;

-- Add new foreign key constraints that reference auth.users
ALTER TABLE public.matchmaking_queue 
ADD CONSTRAINT matchmaking_queue_user_id_fkey 
FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;

ALTER TABLE public.battle_decks 
ADD CONSTRAINT battle_decks_user_id_fkey 
FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;

ALTER TABLE public.battle_sessions
ADD CONSTRAINT battle_sessions_player1_id_fkey 
FOREIGN KEY (player1_id) REFERENCES auth.users(id) ON DELETE CASCADE;

ALTER TABLE public.battle_sessions
ADD CONSTRAINT battle_sessions_player2_id_fkey 
FOREIGN KEY (player2_id) REFERENCES auth.users(id) ON DELETE CASCADE;

ALTER TABLE public.battle_sessions
ADD CONSTRAINT battle_sessions_current_turn_player_id_fkey 
FOREIGN KEY (current_turn_player_id) REFERENCES auth.users(id);

ALTER TABLE public.battle_sessions
ADD CONSTRAINT battle_sessions_winner_id_fkey 
FOREIGN KEY (winner_id) REFERENCES auth.users(id);

-- Also need to fix user_deck_settings table
ALTER TABLE public.user_deck_settings
DROP CONSTRAINT IF EXISTS user_deck_settings_user_id_fkey;

ALTER TABLE public.user_deck_settings
ADD CONSTRAINT user_deck_settings_user_id_fkey 
FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;

-- Success message
SELECT 'Matchmaking foreign key references fixed! Now references auth.users correctly.' as status;