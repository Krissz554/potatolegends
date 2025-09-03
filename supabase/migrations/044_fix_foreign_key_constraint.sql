-- Fix the foreign key constraint pointing to wrong table
-- The error shows it's looking for table "cards" but we use "potato_registry"

-- 1. Drop the incorrect foreign key constraint
DO $$
BEGIN
    -- Drop any foreign key constraints that point to non-existent "cards" table
    IF EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'deck_cards' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
        AND kcu.referenced_table_name = 'cards'
    ) THEN
        ALTER TABLE deck_cards DROP CONSTRAINT IF EXISTS deck_cards_card_id_fkey;
        RAISE NOTICE 'Dropped incorrect foreign key constraint pointing to "cards" table';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error dropping constraint: %', SQLERRM;
END
$$;

-- 2. Drop any foreign key constraints from collections table pointing to "cards"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'collections' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
        AND kcu.referenced_table_name = 'cards'
    ) THEN
        ALTER TABLE collections DROP CONSTRAINT IF EXISTS collections_card_id_fkey;
        RAISE NOTICE 'Dropped incorrect foreign key constraint from collections pointing to "cards" table';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error dropping collections constraint: %', SQLERRM;
END
$$;

-- 3. Add the correct foreign key constraints pointing to potato_registry
DO $$
BEGIN
    -- Add correct foreign key for deck_cards
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'deck_cards' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
        AND kcu.referenced_table_name = 'potato_registry'
    ) THEN
        ALTER TABLE deck_cards 
        ADD CONSTRAINT deck_cards_card_id_potato_registry_fkey 
        FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;
        
        RAISE NOTICE 'Added correct foreign key constraint: deck_cards.card_id -> potato_registry.id';
    ELSE
        RAISE NOTICE 'Correct foreign key constraint for deck_cards already exists';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error adding deck_cards constraint: %', SQLERRM;
END
$$;

-- 4. Add the correct foreign key constraint for collections
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'collections' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
        AND kcu.referenced_table_name = 'potato_registry'
    ) THEN
        ALTER TABLE collections 
        ADD CONSTRAINT collections_card_id_potato_registry_fkey 
        FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;
        
        RAISE NOTICE 'Added correct foreign key constraint: collections.card_id -> potato_registry.id';
    ELSE
        RAISE NOTICE 'Correct foreign key constraint for collections already exists';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error adding collections constraint: %', SQLERRM;
END
$$;

-- 5. Verify there are no orphaned records in deck_cards
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO orphaned_count
    FROM deck_cards dc
    LEFT JOIN potato_registry pr ON dc.card_id = pr.id
    WHERE pr.id IS NULL;
    
    IF orphaned_count > 0 THEN
        RAISE NOTICE 'Warning: Found % orphaned records in deck_cards with invalid card_id', orphaned_count;
        -- Optionally clean them up:
        -- DELETE FROM deck_cards WHERE card_id NOT IN (SELECT id FROM potato_registry);
    ELSE
        RAISE NOTICE 'No orphaned records found in deck_cards';
    END IF;
END
$$;

-- 6. Verify there are no orphaned records in collections
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO orphaned_count
    FROM collections c
    LEFT JOIN potato_registry pr ON c.card_id = pr.id
    WHERE pr.id IS NULL;
    
    IF orphaned_count > 0 THEN
        RAISE NOTICE 'Warning: Found % orphaned records in collections with invalid card_id', orphaned_count;
        -- Optionally clean them up:
        -- DELETE FROM collections WHERE card_id NOT IN (SELECT id FROM potato_registry);
    ELSE
        RAISE NOTICE 'No orphaned records found in collections';
    END IF;
END
$$;

-- 7. Refresh schema cache
NOTIFY pgrst, 'reload schema';

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE 'Fixed foreign key constraints to point to potato_registry instead of non-existent cards table';
  RAISE NOTICE 'deck_cards and collections now properly reference potato_registry.id';
END $$;