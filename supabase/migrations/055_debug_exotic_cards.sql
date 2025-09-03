-- Debug exotic cards - make sure some cards are exotic and visible

-- 1. Check current database state
DO $$
DECLARE
    total_cards INTEGER;
    exotic_count INTEGER;
    card_record RECORD;
BEGIN
    SELECT COUNT(*) INTO total_cards FROM potato_registry;
    SELECT COUNT(*) INTO exotic_count FROM potato_registry WHERE is_exotic = true;
    
    RAISE NOTICE '=== DATABASE STATE DEBUG ===';
    RAISE NOTICE 'Total cards in potato_registry: %', total_cards;
    RAISE NOTICE 'Cards marked as exotic (is_exotic=true): %', exotic_count;
    
    -- Show first few cards with their exotic status
    RAISE NOTICE '=== FIRST 5 CARDS WITH EXOTIC STATUS ===';
    FOR card_record IN
        SELECT name, rarity, is_exotic, is_legendary
        FROM potato_registry 
        ORDER BY sort_order, name
        LIMIT 5
    LOOP
        RAISE NOTICE 'Card: % | Rarity: % | is_exotic: % | is_legendary: %', 
            card_record.name, card_record.rarity, card_record.is_exotic, card_record.is_legendary;
    END LOOP;
END
$$;

-- 2. Force create at least one exotic card for testing
DO $$
DECLARE
    updated_count INTEGER;
    card_record RECORD;
BEGIN
    -- Make the first rare card exotic (force it)
    UPDATE potato_registry 
    SET is_exotic = true
    WHERE id = (
        SELECT id 
        FROM potato_registry 
        WHERE rarity = 'rare' AND is_legendary = false
        ORDER BY sort_order, name
        LIMIT 1
    );
    
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Forced % cards to be exotic', updated_count;
    
    -- Show the card that was made exotic
    FOR card_record IN
        SELECT name, rarity, is_exotic, registry_id
        FROM potato_registry 
        WHERE is_exotic = true
        ORDER BY name
        LIMIT 3
    LOOP
        RAISE NOTICE 'EXOTIC CARD: % (%) - Registry ID: %', 
            card_record.name, card_record.rarity, card_record.registry_id;
    END LOOP;
END
$$;

-- 3. Test the card_complete view
DO $$
DECLARE
    view_exotic_count INTEGER;
    card_record RECORD;
BEGIN
    SELECT COUNT(*) INTO view_exotic_count FROM card_complete WHERE exotic = true;
    RAISE NOTICE '=== CARD_COMPLETE VIEW TEST ===';
    RAISE NOTICE 'Exotic cards visible in card_complete view: %', view_exotic_count;
    
    -- Show exotic cards from the view
    FOR card_record IN
        SELECT name, rarity, exotic, is_legendary
        FROM card_complete 
        WHERE exotic = true
        ORDER BY name
        LIMIT 3
    LOOP
        RAISE NOTICE 'VIEW EXOTIC: % | Rarity: % | exotic: % | is_legendary: %', 
            card_record.name, card_record.rarity, card_record.exotic, card_record.is_legendary;
    END LOOP;
END
$$;

-- 4. Final verification
DO $$
DECLARE
    registry_exotic INTEGER;
    view_exotic INTEGER;
BEGIN
    SELECT COUNT(*) INTO registry_exotic FROM potato_registry WHERE is_exotic = true;
    SELECT COUNT(*) INTO view_exotic FROM card_complete WHERE exotic = true;
    
    RAISE NOTICE '=== FINAL VERIFICATION ===';
    RAISE NOTICE 'Exotic cards in potato_registry (is_exotic=true): %', registry_exotic;
    RAISE NOTICE 'Exotic cards in card_complete view (exotic=true): %', view_exotic;
    
    IF registry_exotic > 0 AND view_exotic > 0 THEN
        RAISE NOTICE '✅ SUCCESS: Exotic cards are properly set up';
    ELSE
        RAISE NOTICE '❌ ISSUE: Mismatch between registry and view';
    END IF;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== EXOTIC CARD DEBUG COMPLETED ===';
  RAISE NOTICE 'Check the logs above to verify exotic cards exist';
  RAISE NOTICE 'Frontend should now be able to see exotic cards';
END $$;