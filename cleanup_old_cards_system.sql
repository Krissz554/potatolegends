-- =====================================================
-- CLEANUP: Remove old cards system entirely
-- =====================================================
-- Since the old 'cards' table has completely different cards
-- than 'card_complete', we need to clean up the old system

-- First, remove all deck_cards since they reference non-existent cards
-- in the new card_complete system
DELETE FROM deck_cards;

-- Drop the old cards table entirely
DROP TABLE IF EXISTS cards CASCADE;

-- Optional: Reset all decks to be empty so users can rebuild them
-- with the new card_complete system
UPDATE decks SET updated_at = NOW();

-- Add some helpful notices
SELECT 'Old cards system cleaned up successfully' as status;
SELECT COUNT(*) as remaining_card_complete_cards FROM card_complete;
SELECT COUNT(*) as remaining_deck_cards FROM deck_cards;
SELECT COUNT(*) as total_decks FROM decks;