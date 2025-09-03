-- =====================================================
-- FIXED MIGRATION: Remove old 'cards' table, use 'card_complete'
-- =====================================================
-- This fixed migration properly handles view dependencies

DO $$
BEGIN
  RAISE NOTICE 'Starting FIXED migration from cards table to card_complete table...';
  
  -- Check current row counts
  RAISE NOTICE 'Current cards table rows: %', (SELECT COUNT(*) FROM cards);
  RAISE NOTICE 'Current card_complete table rows: %', (SELECT COUNT(*) FROM card_complete);
  RAISE NOTICE 'Current deck_cards table rows: %', (SELECT COUNT(*) FROM deck_cards);
END $$;

-- Step 1: Drop the view that depends on deck_cards.card_id
DROP VIEW IF EXISTS deck_cards_with_cards CASCADE;

-- Step 2: Clear all deck_cards since they reference old card IDs
DELETE FROM deck_cards;

-- Step 3: Drop foreign key constraint to old cards table
ALTER TABLE deck_cards DROP CONSTRAINT IF EXISTS deck_cards_card_id_fkey;

-- Step 4: Drop the old cards table entirely
DROP TABLE IF EXISTS cards CASCADE;

-- Step 5: Add foreign key constraint to card_complete
ALTER TABLE deck_cards 
ADD CONSTRAINT deck_cards_card_id_fkey 
FOREIGN KEY (card_id) 
REFERENCES card_complete(id) 
ON DELETE CASCADE;

-- Step 6: Recreate the view using card_complete instead of cards
CREATE OR REPLACE VIEW deck_cards_with_cards AS
SELECT 
  dc.id,
  dc.deck_id,
  dc.card_id,
  dc.quantity,
  dc.added_at,
  -- Card details from card_complete
  cc.name as card_name,
  cc.description as card_description,
  cc.card_type,
  cc.mana_cost as cost,
  cc.attack,
  cc.hp as health,
  cc.rarity,
  cc.potato_type,
  cc.keywords,
  cc.ability_text
FROM deck_cards dc
JOIN card_complete cc ON dc.card_id = cc.id;

-- Step 7: Create useful indexes for performance
CREATE INDEX IF NOT EXISTS idx_card_complete_id ON card_complete(id);
CREATE INDEX IF NOT EXISTS idx_card_complete_name ON card_complete(name);
CREATE INDEX IF NOT EXISTS idx_card_complete_rarity ON card_complete(rarity);
CREATE INDEX IF NOT EXISTS idx_card_complete_card_type ON card_complete(card_type);

-- Step 8: Update deck timestamps to indicate they need rebuilding
UPDATE decks SET updated_at = NOW();

-- Final verification
DO $$
BEGIN
  RAISE NOTICE 'FIXED migration completed successfully!';
  RAISE NOTICE 'Remaining card_complete rows: %', (SELECT COUNT(*) FROM card_complete);
  RAISE NOTICE 'Remaining deck_cards rows: %', (SELECT COUNT(*) FROM deck_cards);
  RAISE NOTICE 'View deck_cards_with_cards recreated using card_complete';
  RAISE NOTICE 'All decks marked for rebuilding (updated_at set to now)';
END $$;