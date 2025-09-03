-- =====================================================
-- MIGRATION: Remove old 'cards' table, use 'card_complete'
-- =====================================================
-- This migration removes the old 'cards' table and updates
-- all references to use the newer 'card_complete' table

-- First, let's check what's referencing the cards table
DO $$
BEGIN
  RAISE NOTICE 'Starting migration from cards table to card_complete table...';
  
  -- Check current row counts
  RAISE NOTICE 'Current cards table rows: %', (SELECT COUNT(*) FROM cards);
  RAISE NOTICE 'Current card_complete table rows: %', (SELECT COUNT(*) FROM card_complete);
  RAISE NOTICE 'Current deck_cards table rows: %', (SELECT COUNT(*) FROM deck_cards);
END $$;

-- Step 1: Create a mapping table to match old card IDs to new card_complete IDs
-- This helps preserve existing deck compositions
CREATE TABLE IF NOT EXISTS temp_card_id_mapping (
  old_card_id UUID,
  new_card_id UUID,
  card_name TEXT,
  mapping_basis TEXT -- 'name_match', 'manual', etc.
);

-- Step 2: Try to map cards by name (best effort)
INSERT INTO temp_card_id_mapping (old_card_id, new_card_id, card_name, mapping_basis)
SELECT 
  c.id as old_card_id,
  cc.id as new_card_id,
  c.name as card_name,
  'name_match' as mapping_basis
FROM cards c
JOIN card_complete cc ON LOWER(TRIM(c.name)) = LOWER(TRIM(cc.name))
WHERE NOT EXISTS (
  SELECT 1 FROM temp_card_id_mapping tm 
  WHERE tm.old_card_id = c.id
);

-- Step 3: Show mapping results
DO $$
BEGIN
  RAISE NOTICE 'Successfully mapped % cards by name', (SELECT COUNT(*) FROM temp_card_id_mapping WHERE mapping_basis = 'name_match');
  RAISE NOTICE 'Unmapped cards from old table: %', (SELECT COUNT(*) FROM cards WHERE id NOT IN (SELECT old_card_id FROM temp_card_id_mapping));
END $$;

-- Step 4: Update deck_cards to reference card_complete instead of cards
-- First, add a new temporary column
ALTER TABLE deck_cards ADD COLUMN IF NOT EXISTS new_card_id UUID;

-- Update the new column with mapped card IDs
UPDATE deck_cards 
SET new_card_id = tm.new_card_id
FROM temp_card_id_mapping tm
WHERE deck_cards.card_id = tm.old_card_id;

-- Check how many deck_cards were successfully mapped
DO $$
BEGIN
  RAISE NOTICE 'Deck cards successfully mapped: %', (SELECT COUNT(*) FROM deck_cards WHERE new_card_id IS NOT NULL);
  RAISE NOTICE 'Deck cards that could not be mapped: %', (SELECT COUNT(*) FROM deck_cards WHERE new_card_id IS NULL);
END $$;

-- Step 5: Remove deck cards that couldn't be mapped (they reference non-existent cards)
DELETE FROM deck_cards WHERE new_card_id IS NULL;

-- Step 6: Drop the old foreign key constraint to cards table
ALTER TABLE deck_cards DROP CONSTRAINT IF EXISTS deck_cards_card_id_fkey;

-- Step 7: Drop the old card_id column and rename new_card_id to card_id
ALTER TABLE deck_cards DROP COLUMN card_id;
ALTER TABLE deck_cards RENAME COLUMN new_card_id TO card_id;

-- Step 8: Add foreign key constraint to card_complete
ALTER TABLE deck_cards 
ADD CONSTRAINT deck_cards_card_id_fkey 
FOREIGN KEY (card_id) 
REFERENCES card_complete(id) 
ON DELETE CASCADE;

-- Step 9: Drop the old cards table
DROP TABLE IF EXISTS cards CASCADE;

-- Step 10: Clean up temporary mapping table
DROP TABLE IF EXISTS temp_card_id_mapping;

-- Step 11: Update any views or functions that might reference the old cards table
-- (Add specific updates here if needed)

-- Step 12: Create an index on card_complete.id for performance
CREATE INDEX IF NOT EXISTS idx_card_complete_id ON card_complete(id);
CREATE INDEX IF NOT EXISTS idx_card_complete_name ON card_complete(name);
CREATE INDEX IF NOT EXISTS idx_card_complete_rarity ON card_complete(rarity);

-- Final verification
DO $$
BEGIN
  RAISE NOTICE 'Migration completed successfully!';
  RAISE NOTICE 'Remaining card_complete rows: %', (SELECT COUNT(*) FROM card_complete);
  RAISE NOTICE 'Remaining deck_cards rows: %', (SELECT COUNT(*) FROM deck_cards);
  RAISE NOTICE 'All deck_cards now reference card_complete table';
END $$;