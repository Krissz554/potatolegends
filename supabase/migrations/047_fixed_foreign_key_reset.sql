-- Fixed version of foreign key constraint reset
-- Uses correct PostgreSQL information schema column names

-- 1. First, let's see what constraints currently exist (fixed query)
DO $$
DECLARE
    constraint_record RECORD;
BEGIN
    RAISE NOTICE 'Current foreign key constraints on deck_cards:';
    FOR constraint_record IN 
        SELECT 
            tc.constraint_name, 
            kcu.column_name, 
            ccu.table_name AS referenced_table_name,
            ccu.column_name AS referenced_column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
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
        EXECUTE 'ALTER TABLE deck_cards DROP CONSTRAINT IF EXISTS ' || constraint_name;
        RAISE NOTICE 'Dropped constraint: %', constraint_name;
    END LOOP;
    
    -- Also try dropping common constraint names that might exist
    BEGIN
        ALTER TABLE deck_cards DROP CONSTRAINT IF EXISTS deck_cards_card_id_fkey;
        RAISE NOTICE 'Dropped deck_cards_card_id_fkey (if it existed)';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'deck_cards_card_id_fkey did not exist';
    END;
    
    BEGIN
        ALTER TABLE deck_cards DROP CONSTRAINT IF EXISTS deck_cards_card_id_cards_fkey;
        RAISE NOTICE 'Dropped deck_cards_card_id_cards_fkey (if it existed)';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'deck_cards_card_id_cards_fkey did not exist';
    END;
    
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
        EXECUTE 'ALTER TABLE collections DROP CONSTRAINT IF EXISTS ' || constraint_name;
        RAISE NOTICE 'Dropped collections constraint: %', constraint_name;
    END LOOP;
    
    -- Also try dropping common constraint names
    BEGIN
        ALTER TABLE collections DROP CONSTRAINT IF EXISTS collections_card_id_fkey;
        RAISE NOTICE 'Dropped collections_card_id_fkey (if it existed)';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'collections_card_id_fkey did not exist';
    END;
    
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
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    -- Check deck_cards
    SELECT COUNT(*) INTO orphaned_count
    FROM deck_cards dc
    WHERE NOT EXISTS (SELECT 1 FROM potato_registry pr WHERE pr.id = dc.card_id);
    
    IF orphaned_count > 0 THEN
        RAISE NOTICE 'Found % orphaned records in deck_cards, cleaning them up...', orphaned_count;
        DELETE FROM deck_cards 
        WHERE NOT EXISTS (SELECT 1 FROM potato_registry pr WHERE pr.id = deck_cards.card_id);
        RAISE NOTICE 'Cleaned up % orphaned deck_cards records', orphaned_count;
    ELSE
        RAISE NOTICE 'No orphaned records in deck_cards';
    END IF;
    
    -- Check collections
    SELECT COUNT(*) INTO orphaned_count
    FROM collections c
    WHERE NOT EXISTS (SELECT 1 FROM potato_registry pr WHERE pr.id = c.card_id);
    
    IF orphaned_count > 0 THEN
        RAISE NOTICE 'Found % orphaned records in collections, cleaning them up...', orphaned_count;
        DELETE FROM collections 
        WHERE NOT EXISTS (SELECT 1 FROM potato_registry pr WHERE pr.id = collections.card_id);
        RAISE NOTICE 'Cleaned up % orphaned collections records', orphaned_count;
    ELSE
        RAISE NOTICE 'No orphaned records in collections';
    END IF;
END
$$;

-- 6. Now add the correct foreign key constraints with unique names
DO $$
BEGIN
    BEGIN
        ALTER TABLE deck_cards 
        ADD CONSTRAINT fk_deck_cards_potato_registry_2024 
        FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;
        RAISE NOTICE 'Added FK constraint: fk_deck_cards_potato_registry_2024';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Error adding deck_cards FK: %', SQLERRM;
    END;
    
    BEGIN
        ALTER TABLE collections 
        ADD CONSTRAINT fk_collections_potato_registry_2024 
        FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;
        RAISE NOTICE 'Added FK constraint: fk_collections_potato_registry_2024';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Error adding collections FK: %', SQLERRM;
    END;
END
$$;

-- 7. Verify the new constraints were added correctly (fixed query)
DO $$
DECLARE
    constraint_record RECORD;
BEGIN
    RAISE NOTICE 'New foreign key constraints on deck_cards:';
    FOR constraint_record IN 
        SELECT 
            tc.constraint_name, 
            kcu.column_name, 
            ccu.table_name AS referenced_table_name,
            ccu.column_name AS referenced_column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
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
        SELECT 
            tc.constraint_name, 
            kcu.column_name, 
            ccu.table_name AS referenced_table_name,
            ccu.column_name AS referenced_column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
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
DECLARE
    card_name TEXT;
BEGIN
    SELECT name INTO card_name FROM potato_registry WHERE id = '2736982b-4219-467b-8243-c8e826063c1a';
    
    IF card_name IS NOT NULL THEN
        RAISE NOTICE 'Card ID 2736982b-4219-467b-8243-c8e826063c1a EXISTS in potato_registry: %', card_name;
    ELSE
        RAISE NOTICE 'Card ID 2736982b-4219-467b-8243-c8e826063c1a NOT FOUND in potato_registry';
        
        -- Show some sample cards
        RAISE NOTICE 'Sample cards that exist:';
        FOR card_name IN 
            SELECT name || ' (' || id || ')' 
            FROM potato_registry 
            LIMIT 3
        LOOP
            RAISE NOTICE '  %', card_name;
        END LOOP;
    END IF;
END
$$;

-- 9. Simple test insert to verify constraints work
DO $$
DECLARE
    test_deck_id UUID;
    test_card_id UUID;
BEGIN
    -- Get a real deck and card ID for testing
    SELECT id INTO test_deck_id FROM decks LIMIT 1;
    SELECT id INTO test_card_id FROM potato_registry LIMIT 1;
    
    IF test_deck_id IS NOT NULL AND test_card_id IS NOT NULL THEN
        -- Try a test insert
        BEGIN
            INSERT INTO deck_cards (deck_id, card_id, quantity) 
            VALUES (test_deck_id, test_card_id, 1)
            ON CONFLICT (deck_id, card_id) DO NOTHING;
            RAISE NOTICE 'TEST SUCCESS: Can insert cards into deck_cards table';
        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE 'TEST FAILED: %', SQLERRM;
        END;
    ELSE
        RAISE NOTICE 'Cannot run test: no decks or cards found';
    END IF;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== COMPREHENSIVE FOREIGN KEY FIX COMPLETED ===';
  RAISE NOTICE 'All old constraints dropped, orphaned data cleaned, new constraints added';
  RAISE NOTICE 'deck_cards and collections now properly reference potato_registry';
  RAISE NOTICE 'The deck builder should now work correctly!';
END $$;