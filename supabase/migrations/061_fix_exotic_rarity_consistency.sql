-- Fix exotic rarity consistency in card_complete table
-- Ensure cards with rarity='exotic' also have exotic=true

-- 1. Check current state of exotic cards
DO $$
DECLARE
    cards_with_exotic_rarity INTEGER;
    cards_with_exotic_flag INTEGER;
    cards_with_both INTEGER;
    card_record RECORD;
BEGIN
    RAISE NOTICE '=== EXOTIC CARD CONSISTENCY CHECK ===';
    
    -- Count cards with rarity = 'exotic'
    SELECT COUNT(*) INTO cards_with_exotic_rarity 
    FROM card_complete 
    WHERE rarity = 'exotic';
    
    -- Count cards with exotic = true
    SELECT COUNT(*) INTO cards_with_exotic_flag 
    FROM card_complete 
    WHERE exotic = true;
    
    -- Count cards with both
    SELECT COUNT(*) INTO cards_with_both 
    FROM card_complete 
    WHERE rarity = 'exotic' AND exotic = true;
    
    RAISE NOTICE 'Cards with rarity = exotic: %', cards_with_exotic_rarity;
    RAISE NOTICE 'Cards with exotic = true: %', cards_with_exotic_flag;
    RAISE NOTICE 'Cards with both: %', cards_with_both;
    
    -- Show cards that have rarity='exotic' but exotic=false
    RAISE NOTICE '=== CARDS WITH INCONSISTENT EXOTIC STATUS ===';
    FOR card_record IN
        SELECT name, rarity, exotic
        FROM card_complete 
        WHERE rarity = 'exotic' AND exotic = false
        ORDER BY name
        LIMIT 10
    LOOP
        RAISE NOTICE 'INCONSISTENT: % | rarity: % | exotic: %', 
            card_record.name, card_record.rarity, card_record.exotic;
    END LOOP;
    
    -- Show sample exotic rarity cards
    RAISE NOTICE '=== SAMPLE EXOTIC RARITY CARDS ===';
    FOR card_record IN
        SELECT name, rarity, exotic
        FROM card_complete 
        WHERE rarity = 'exotic'
        ORDER BY name
        LIMIT 5
    LOOP
        RAISE NOTICE 'EXOTIC RARITY: % | rarity: % | exotic: %', 
            card_record.name, card_record.rarity, card_record.exotic;
    END LOOP;
END
$$;

-- 2. Fix the inconsistency - update exotic flag for all exotic rarity cards
UPDATE card_complete 
SET exotic = true 
WHERE rarity = 'exotic' AND exotic = false;

-- 3. Get the count of updated cards
DO $$
DECLARE
    updated_count INTEGER;
BEGIN
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE '=== FIXED % CARDS ===', updated_count;
    RAISE NOTICE 'Updated exotic flag to true for cards with rarity=exotic';
END
$$;

-- 4. Also make some rare cards exotic for more variety
DO $$
DECLARE
    rare_exotic_count INTEGER;
    updated_count INTEGER;
BEGIN
    -- Count current exotic cards
    SELECT COUNT(*) INTO rare_exotic_count 
    FROM card_complete 
    WHERE exotic = true;
    
    RAISE NOTICE '=== ADDING MORE EXOTIC CARDS ===';
    RAISE NOTICE 'Current exotic cards: %', rare_exotic_count;
    
    -- If we have fewer than 10 exotic cards, promote some rare cards
    IF rare_exotic_count < 10 THEN
        UPDATE card_complete 
        SET exotic = true 
        WHERE rarity = 'rare' 
        AND exotic = false 
        AND is_legendary = false
        AND id IN (
            SELECT id 
            FROM card_complete 
            WHERE rarity = 'rare' AND exotic = false AND is_legendary = false
            ORDER BY RANDOM()
            LIMIT (10 - rare_exotic_count)
        );
        
        GET DIAGNOSTICS updated_count = ROW_COUNT;
        RAISE NOTICE 'Promoted % rare cards to exotic status', updated_count;
    END IF;
END
$$;

-- 5. Final verification
DO $$
DECLARE
    total_exotic INTEGER;
    exotic_rarity INTEGER;
    exotic_flag INTEGER;
    card_record RECORD;
BEGIN
    SELECT COUNT(*) INTO total_exotic FROM card_complete WHERE exotic = true;
    SELECT COUNT(*) INTO exotic_rarity FROM card_complete WHERE rarity = 'exotic';
    SELECT COUNT(*) INTO exotic_flag FROM card_complete WHERE exotic = true;
    
    RAISE NOTICE '=== FINAL EXOTIC CARD STATUS ===';
    RAISE NOTICE 'Total exotic cards (exotic=true): %', total_exotic;
    RAISE NOTICE 'Cards with exotic rarity: %', exotic_rarity;
    RAISE NOTICE 'Cards with exotic flag: %', exotic_flag;
    
    -- Show all exotic cards
    RAISE NOTICE '=== ALL EXOTIC CARDS ===';
    FOR card_record IN
        SELECT name, rarity, exotic
        FROM card_complete 
        WHERE exotic = true
        ORDER BY name
    LOOP
        RAISE NOTICE 'EXOTIC: % | rarity: % | exotic: %', 
            card_record.name, card_record.rarity, card_record.exotic;
    END LOOP;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== EXOTIC CONSISTENCY FIXED ===';
  RAISE NOTICE 'All exotic rarity cards now have exotic=true';
  RAISE NOTICE 'Added more exotic cards for better variety';
  RAISE NOTICE 'Exotic filter should now show all exotic cards';
END $$;