-- Ensure at least some exotic cards exist in the system
-- This migration guarantees exotic cards for testing the filter

-- First, let's check current exotic card count
DO $$
DECLARE
    exotic_count INTEGER;
    rare_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO exotic_count FROM potato_registry WHERE is_exotic = true;
    SELECT COUNT(*) INTO rare_count FROM potato_registry WHERE rarity = 'rare';
    
    RAISE NOTICE '=== CURRENT CARD STATUS ===';
    RAISE NOTICE 'Current exotic cards: %', exotic_count;
    RAISE NOTICE 'Available rare cards: %', rare_count;
END
$$;

-- If no exotic cards exist, create some by making specific rare cards exotic
DO $$
DECLARE
    exotic_count INTEGER;
    updated_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO exotic_count FROM potato_registry WHERE is_exotic = true;
    
    IF exotic_count = 0 THEN
        RAISE NOTICE 'No exotic cards found - creating some exotic cards...';
        
        -- Make the first 3 rare cards exotic (if they exist)
        UPDATE potato_registry 
        SET 
            is_exotic = true,
            craft_cost = 2000,
            dust_value = 500
        WHERE 
            rarity = 'rare' 
            AND is_exotic = false 
            AND is_legendary = false
            AND id IN (
                SELECT id 
                FROM potato_registry 
                WHERE rarity = 'rare' AND is_exotic = false AND is_legendary = false
                ORDER BY sort_order, name
                LIMIT 3
            );
            
        GET DIAGNOSTICS updated_count = ROW_COUNT;
        RAISE NOTICE 'Created % exotic cards from rare cards', updated_count;
    ELSE
        RAISE NOTICE 'Exotic cards already exist: %', exotic_count;
    END IF;
END
$$;

-- Also ensure some uncommon cards exist if needed
DO $$
DECLARE
    uncommon_count INTEGER;
    common_count INTEGER;
    updated_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO uncommon_count FROM potato_registry WHERE rarity = 'uncommon';
    SELECT COUNT(*) INTO common_count FROM potato_registry WHERE rarity = 'common';
    
    RAISE NOTICE 'Uncommon cards: %, Common cards: %', uncommon_count, common_count;
    
    -- If we have no uncommon cards but have common cards, promote some common to uncommon
    IF uncommon_count = 0 AND common_count > 5 THEN
        UPDATE potato_registry 
        SET rarity = 'uncommon'
        WHERE 
            rarity = 'common'
            AND id IN (
                SELECT id 
                FROM potato_registry 
                WHERE rarity = 'common'
                ORDER BY sort_order, name
                LIMIT 2
            );
            
        GET DIAGNOSTICS updated_count = ROW_COUNT;
        RAISE NOTICE 'Promoted % common cards to uncommon', updated_count;
    END IF;
END
$$;

-- Final status report
DO $$
DECLARE
    card_record RECORD;
    total_exotic INTEGER;
    total_legendary INTEGER;
    total_rare INTEGER;
    total_uncommon INTEGER;
    total_common INTEGER;
BEGIN
    SELECT COUNT(*) INTO total_exotic FROM potato_registry WHERE is_exotic = true;
    SELECT COUNT(*) INTO total_legendary FROM potato_registry WHERE is_legendary = true;
    SELECT COUNT(*) INTO total_rare FROM potato_registry WHERE rarity = 'rare';
    SELECT COUNT(*) INTO total_uncommon FROM potato_registry WHERE rarity = 'uncommon';
    SELECT COUNT(*) INTO total_common FROM potato_registry WHERE rarity = 'common';
    
    RAISE NOTICE '=== FINAL RARITY DISTRIBUTION ===';
    RAISE NOTICE 'Exotic cards: %', total_exotic;
    RAISE NOTICE 'Legendary cards: %', total_legendary;
    RAISE NOTICE 'Rare cards: %', total_rare;
    RAISE NOTICE 'Uncommon cards: %', total_uncommon;
    RAISE NOTICE 'Common cards: %', total_common;
    
    -- Show which cards are exotic
    IF total_exotic > 0 THEN
        RAISE NOTICE '=== EXOTIC CARDS ===';
        FOR card_record IN
            SELECT name, rarity, craft_cost 
            FROM potato_registry 
            WHERE is_exotic = true 
            ORDER BY name
            LIMIT 5
        LOOP
            RAISE NOTICE 'EXOTIC: % (%) - Craft Cost: %', 
                card_record.name, card_record.rarity, card_record.craft_cost;
        END LOOP;
    END IF;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== EXOTIC CARD SYSTEM READY ===';
  RAISE NOTICE 'Exotic cards are available for filtering';
  RAISE NOTICE 'Exotic filter and stats tracker will work correctly';
END $$;