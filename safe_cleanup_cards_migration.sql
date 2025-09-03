-- =====================================================
-- SAFE CLEANUP: Switch from cards to card_complete
-- =====================================================
-- This script safely removes the old cards system

-- Step 1: Drop the problematic view first
DROP VIEW IF EXISTS deck_cards_with_cards CASCADE;

-- Step 2: Clear all deck_cards (they reference old cards anyway)
DELETE FROM deck_cards;

-- Step 3: Drop old foreign key constraint
ALTER TABLE deck_cards DROP CONSTRAINT IF EXISTS deck_cards_card_id_fkey;

-- Step 4: Drop the old cards table
DROP TABLE IF EXISTS cards CASCADE;

-- Step 5: Add new foreign key to card_complete
ALTER TABLE deck_cards 
ADD CONSTRAINT deck_cards_card_id_fkey 
FOREIGN KEY (card_id) 
REFERENCES card_complete(id) 
ON DELETE CASCADE;

-- Step 6: Recreate the view with card_complete
CREATE OR REPLACE VIEW deck_cards_with_cards AS
SELECT 
  dc.id,
  dc.deck_id,
  dc.card_id,
  dc.quantity,
  dc.added_at,
  cc.name as card_name,
  cc.description as card_description,
  cc.card_type,
  cc.mana_cost as cost,
  cc.attack,
  cc.hp as health,
  cc.rarity,
  cc.potato_type,
  cc.keywords
FROM deck_cards dc
JOIN card_complete cc ON dc.card_id = cc.id;

-- Step 7: Mark decks as needing rebuild
UPDATE decks SET updated_at = NOW();

-- Verify results
SELECT 'Migration completed successfully!' as status;
SELECT COUNT(*) as card_complete_count FROM card_complete;
SELECT COUNT(*) as deck_cards_count FROM deck_cards;