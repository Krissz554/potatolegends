-- Verify card data and test deck operations
-- This will help us understand what's happening with the specific card ID

-- 1. Check if the problematic card exists
DO $$
DECLARE
    card_exists BOOLEAN;
    card_info RECORD;
BEGIN
    -- Check if the card from the error exists
    SELECT EXISTS(SELECT 1 FROM potato_registry WHERE id = '2736982b-4219-467b-8243-c8e826063c1a') INTO card_exists;
    
    IF card_exists THEN
        -- Get card details
        SELECT name, registry_id, rarity INTO card_info 
        FROM potato_registry 
        WHERE id = '2736982b-4219-467b-8243-c8e826063c1a';
        
        RAISE NOTICE 'Card EXISTS: % (%, %)', card_info.name, card_info.registry_id, card_info.rarity;
    ELSE
        RAISE NOTICE 'Card with ID 2736982b-4219-467b-8243-c8e826063c1a NOT FOUND in potato_registry';
        
        -- Show some sample cards that do exist
        RAISE NOTICE 'Sample cards that DO exist:';
        FOR card_info IN 
            SELECT id, name, registry_id, rarity 
            FROM potato_registry 
            LIMIT 5
        LOOP
            RAISE NOTICE 'Card: % - % (%, %)', card_info.id, card_info.name, card_info.registry_id, card_info.rarity;
        END LOOP;
    END IF;
END
$$;

-- 2. Create a simple test function to add a card to deck (bypassing the complex v3 function)
CREATE OR REPLACE FUNCTION test_add_card_to_deck(
    test_deck_id UUID,
    test_card_id UUID
)
RETURNS TEXT AS $$
DECLARE
    result_message TEXT;
BEGIN
    -- Try to insert directly
    INSERT INTO deck_cards (deck_id, card_id, quantity)
    VALUES (test_deck_id, test_card_id, 1)
    ON CONFLICT (deck_id, card_id) DO UPDATE SET
        quantity = deck_cards.quantity + 1;
    
    result_message := 'SUCCESS: Added card to deck';
    RETURN result_message;
EXCEPTION
    WHEN foreign_key_violation THEN
        result_message := 'FK_ERROR: ' || SQLERRM;
        RETURN result_message;
    WHEN OTHERS THEN
        result_message := 'OTHER_ERROR: ' || SQLERRM;
        RETURN result_message;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION test_add_card_to_deck(UUID, UUID) TO authenticated;

-- 3. Show current foreign key constraints (for debugging)
DO $$
DECLARE
    constraint_record RECORD;
BEGIN
    RAISE NOTICE '=== CURRENT FOREIGN KEY CONSTRAINTS ===';
    
    -- deck_cards constraints
    RAISE NOTICE 'deck_cards foreign keys:';
    FOR constraint_record IN 
        SELECT tc.constraint_name, kcu.column_name, kcu.referenced_table_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'deck_cards' AND tc.constraint_type = 'FOREIGN KEY'
    LOOP
        RAISE NOTICE '  %: % -> %', 
            constraint_record.constraint_name,
            constraint_record.column_name,
            constraint_record.referenced_table_name;
    END LOOP;
    
    -- collections constraints  
    RAISE NOTICE 'collections foreign keys:';
    FOR constraint_record IN 
        SELECT tc.constraint_name, kcu.column_name, kcu.referenced_table_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'collections' AND tc.constraint_type = 'FOREIGN KEY'
    LOOP
        RAISE NOTICE '  %: % -> %', 
            constraint_record.constraint_name,
            constraint_record.column_name,
            constraint_record.referenced_table_name;
    END LOOP;
END
$$;

-- 4. Count records in key tables
DO $$
DECLARE
    potato_count INTEGER;
    collection_count INTEGER;
    deck_count INTEGER;
    deck_card_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO potato_count FROM potato_registry;
    SELECT COUNT(*) INTO collection_count FROM collections;
    SELECT COUNT(*) INTO deck_count FROM decks;
    SELECT COUNT(*) INTO deck_card_count FROM deck_cards;
    
    RAISE NOTICE '=== TABLE RECORD COUNTS ===';
    RAISE NOTICE 'potato_registry: % records', potato_count;
    RAISE NOTICE 'collections: % records', collection_count;
    RAISE NOTICE 'decks: % records', deck_count;
    RAISE NOTICE 'deck_cards: % records', deck_card_count;
END
$$;