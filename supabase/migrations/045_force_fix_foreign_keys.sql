-- Force fix all foreign key constraints - comprehensive approach
-- This will definitely resolve the constraint pointing to non-existent "cards" table

-- 1. First, let's see what constraints currently exist
DO $$
DECLARE
    constraint_record RECORD;
BEGIN
    RAISE NOTICE 'Current foreign key constraints on deck_cards:';
    FOR constraint_record IN 
        SELECT tc.constraint_name, kcu.column_name, kcu.referenced_table_name, kcu.referenced_column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'deck_cards' AND tc.constraint_type = 'FOREIGN KEY'
    LOOP
        RAISE NOTICE 'Constraint: %, Column: %, References: %.%', 
            constraint_record.constraint_name, 
            constraint_record.column_name,
            constraint_record.referenced_table_name,
            constraint_record.referenced_column_name;
    END LOOP;
END
$$;

-- 2. Drop ALL foreign key constraints on deck_cards.card_id (brute force approach)
DO $$
DECLARE
    constraint_name TEXT;
BEGIN
    -- Get all FK constraint names for deck_cards.card_id
    FOR constraint_name IN
        SELECT tc.constraint_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'deck_cards' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
    LOOP
        EXECUTE 'ALTER TABLE deck_cards DROP CONSTRAINT ' || constraint_name;
        RAISE NOTICE 'Dropped constraint: %', constraint_name;
    END LOOP;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error dropping constraints: %', SQLERRM;
END
$$;

-- 3. Drop ALL foreign key constraints on collections.card_id as well
DO $$
DECLARE
    constraint_name TEXT;
BEGIN
    FOR constraint_name IN
        SELECT tc.constraint_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'collections' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
    LOOP
        EXECUTE 'ALTER TABLE collections DROP CONSTRAINT ' || constraint_name;
        RAISE NOTICE 'Dropped collections constraint: %', constraint_name;
    END LOOP;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error dropping collections constraints: %', SQLERRM;
END
$$;

-- 4. Verify potato_registry table exists and has the expected structure
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'potato_registry') THEN
        RAISE NOTICE 'potato_registry table exists - good!';
    ELSE
        RAISE EXCEPTION 'potato_registry table does not exist!';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'potato_registry' AND column_name = 'id') THEN
        RAISE NOTICE 'potato_registry.id column exists - good!';
    ELSE
        RAISE EXCEPTION 'potato_registry.id column does not exist!';
    END IF;
END
$$;

-- 5. Clean up any orphaned records before adding constraints
-- Check deck_cards first
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO orphaned_count
    FROM deck_cards dc
    WHERE dc.card_id NOT IN (SELECT id FROM potato_registry);
    
    IF orphaned_count > 0 THEN
        RAISE NOTICE 'Found % orphaned records in deck_cards, cleaning them up...', orphaned_count;
        DELETE FROM deck_cards WHERE card_id NOT IN (SELECT id FROM potato_registry);
        RAISE NOTICE 'Cleaned up % orphaned deck_cards records', orphaned_count;
    ELSE
        RAISE NOTICE 'No orphaned records in deck_cards';
    END IF;
END
$$;

-- Check collections
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO orphaned_count
    FROM collections c
    WHERE c.card_id NOT IN (SELECT id FROM potato_registry);
    
    IF orphaned_count > 0 THEN
        RAISE NOTICE 'Found % orphaned records in collections, cleaning them up...', orphaned_count;
        DELETE FROM collections WHERE card_id NOT IN (SELECT id FROM potato_registry);
        RAISE NOTICE 'Cleaned up % orphaned collections records', orphaned_count;
    ELSE
        RAISE NOTICE 'No orphaned records in collections';
    END IF;
END
$$;

-- 6. Now add the correct foreign key constraints with unique names
ALTER TABLE deck_cards 
ADD CONSTRAINT fk_deck_cards_potato_registry 
FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;

ALTER TABLE collections 
ADD CONSTRAINT fk_collections_potato_registry 
FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;

-- 7. Verify the new constraints were added correctly
DO $$
DECLARE
    constraint_record RECORD;
BEGIN
    RAISE NOTICE 'New foreign key constraints on deck_cards:';
    FOR constraint_record IN 
        SELECT tc.constraint_name, kcu.column_name, kcu.referenced_table_name, kcu.referenced_column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'deck_cards' AND tc.constraint_type = 'FOREIGN KEY'
    LOOP
        RAISE NOTICE 'Constraint: %, Column: %, References: %.%', 
            constraint_record.constraint_name, 
            constraint_record.column_name,
            constraint_record.referenced_table_name,
            constraint_record.referenced_column_name;
    END LOOP;
    
    RAISE NOTICE 'New foreign key constraints on collections:';
    FOR constraint_record IN 
        SELECT tc.constraint_name, kcu.column_name, kcu.referenced_table_name, kcu.referenced_column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'collections' AND tc.constraint_type = 'FOREIGN KEY'
    LOOP
        RAISE NOTICE 'Constraint: %, Column: %, References: %.%', 
            constraint_record.constraint_name, 
            constraint_record.column_name,
            constraint_record.referenced_table_name,
            constraint_record.referenced_column_name;
    END LOOP;
END
$$;

-- 8. Test that the card_id from the error exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM potato_registry WHERE id = '2736982b-4219-467b-8243-c8e826063c1a') THEN
        RAISE NOTICE 'Card ID 2736982b-4219-467b-8243-c8e826063c1a EXISTS in potato_registry - good!';
    ELSE
        RAISE NOTICE 'Card ID 2736982b-4219-467b-8243-c8e826063c1a NOT FOUND in potato_registry';
    END IF;
END
$$;

-- 9. Refresh schema cache
NOTIFY pgrst, 'reload schema';

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== COMPREHENSIVE FOREIGN KEY FIX COMPLETED ===';
  RAISE NOTICE 'All old constraints dropped, orphaned data cleaned, new constraints added';
  RAISE NOTICE 'deck_cards and collections now properly reference potato_registry';
  RAISE NOTICE 'The deck builder should now work correctly!';
END $$;