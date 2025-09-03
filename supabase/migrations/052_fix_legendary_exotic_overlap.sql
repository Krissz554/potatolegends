-- Fix legendary cards that are incorrectly marked as exotic
-- Legendary cards should NOT be exotic - they are separate rarities

-- 1. Check current state of legendary cards with exotic flags
DO $$
DECLARE
    card_record RECORD;
BEGIN
    RAISE NOTICE '=== LEGENDARY CARDS WITH EXOTIC FLAGS ===';
    
    FOR card_record IN
        SELECT name, rarity, is_legendary, is_exotic
        FROM potato_registry 
        WHERE is_legendary = true AND is_exotic = true
        ORDER BY name
    LOOP
        RAISE NOTICE 'Card: % | Rarity: % | is_legendary: % | is_exotic: %', 
            card_record.name, card_record.rarity, card_record.is_legendary, card_record.is_exotic;
    END LOOP;
END
$$;

-- 2. Fix legendary cards that are incorrectly marked as exotic
-- Rule: If a card is legendary, it should NOT be exotic
UPDATE potato_registry 
SET is_exotic = false
WHERE is_legendary = true AND is_exotic = true;

-- 3. Show how many cards were fixed
DO $$
DECLARE
    updated_count INTEGER;
BEGIN
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Fixed % legendary cards that were incorrectly marked as exotic', updated_count;
END
$$;

-- 4. Verify the fix worked
DO $$
DECLARE
    remaining_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO remaining_count
    FROM potato_registry 
    WHERE is_legendary = true AND is_exotic = true;
    
    IF remaining_count = 0 THEN
        RAISE NOTICE '✅ SUCCESS: No legendary cards are marked as exotic anymore';
    ELSE
        RAISE NOTICE '❌ WARNING: % legendary cards still marked as exotic', remaining_count;
    END IF;
END
$$;

-- 5. Show final state of legendary and exotic cards
DO $$
DECLARE
    legendary_count INTEGER;
    exotic_count INTEGER;
    overlap_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO legendary_count FROM potato_registry WHERE is_legendary = true;
    SELECT COUNT(*) INTO exotic_count FROM potato_registry WHERE is_exotic = true;
    SELECT COUNT(*) INTO overlap_count FROM potato_registry WHERE is_legendary = true AND is_exotic = true;
    
    RAISE NOTICE '=== FINAL CARD COUNTS ===';
    RAISE NOTICE 'Legendary cards: %', legendary_count;
    RAISE NOTICE 'Exotic cards: %', exotic_count;
    RAISE NOTICE 'Overlap (should be 0): %', overlap_count;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== LEGENDARY/EXOTIC SEPARATION COMPLETED ===';
  RAISE NOTICE 'Legendary cards are no longer marked as exotic';
  RAISE NOTICE 'Each card now has only one special rarity flag';
  RAISE NOTICE 'UI will show only one badge per card';
END $$;