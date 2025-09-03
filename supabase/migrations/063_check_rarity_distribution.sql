-- Check the current rarity distribution in card_complete table

DO $$
DECLARE
    total_cards INTEGER;
    common_cards INTEGER;
    uncommon_cards INTEGER;
    rare_cards INTEGER;
    legendary_cards INTEGER;
    exotic_cards INTEGER;
    other_rarities INTEGER;
    rarity_record RECORD;
BEGIN
    RAISE NOTICE '=== CARD RARITY DISTRIBUTION ANALYSIS ===';
    
    -- Get total cards
    SELECT COUNT(*) INTO total_cards FROM card_complete;
    RAISE NOTICE 'Total cards in card_complete: %', total_cards;
    
    -- Count each rarity type
    SELECT COUNT(*) INTO common_cards FROM card_complete WHERE rarity = 'common';
    SELECT COUNT(*) INTO uncommon_cards FROM card_complete WHERE rarity = 'uncommon';
    SELECT COUNT(*) INTO rare_cards FROM card_complete WHERE rarity = 'rare';
    SELECT COUNT(*) INTO legendary_cards FROM card_complete WHERE rarity = 'legendary';
    SELECT COUNT(*) INTO exotic_cards FROM card_complete WHERE rarity = 'exotic';
    
    RAISE NOTICE '';
    RAISE NOTICE '=== RARITY BREAKDOWN ===';
    RAISE NOTICE 'Common cards: %', common_cards;
    RAISE NOTICE 'Uncommon cards: %', uncommon_cards;
    RAISE NOTICE 'Rare cards: %', rare_cards;
    RAISE NOTICE 'Legendary cards: %', legendary_cards;
    RAISE NOTICE 'Exotic cards: %', exotic_cards;
    
    -- Check for any other rarities
    SELECT COUNT(*) INTO other_rarities 
    FROM card_complete 
    WHERE rarity NOT IN ('common', 'uncommon', 'rare', 'legendary', 'exotic');
    
    IF other_rarities > 0 THEN
        RAISE NOTICE 'Other rarities: %', other_rarities;
        RAISE NOTICE '=== UNKNOWN RARITIES ===';
        FOR rarity_record IN
            SELECT DISTINCT rarity, COUNT(*) as count
            FROM card_complete 
            WHERE rarity NOT IN ('common', 'uncommon', 'rare', 'legendary', 'exotic')
            GROUP BY rarity
            ORDER BY count DESC
        LOOP
            RAISE NOTICE 'Unknown rarity: % (% cards)', rarity_record.rarity, rarity_record.count;
        END LOOP;
    END IF;
    
    -- Verify totals add up
    DECLARE
        calculated_total INTEGER;
    BEGIN
        calculated_total := common_cards + uncommon_cards + rare_cards + legendary_cards + exotic_cards + other_rarities;
        RAISE NOTICE '';
        RAISE NOTICE '=== VERIFICATION ===';
        RAISE NOTICE 'Sum of all rarities: %', calculated_total;
        RAISE NOTICE 'Total cards: %', total_cards;
        
        IF calculated_total = total_cards THEN
            RAISE NOTICE '✅ All cards accounted for';
        ELSE
            RAISE NOTICE '❌ Mismatch in counts';
        END IF;
    END;
    
    -- Show percentage distribution
    RAISE NOTICE '';
    RAISE NOTICE '=== PERCENTAGE DISTRIBUTION ===';
    RAISE NOTICE 'Common: %% (%)', ROUND((common_cards * 100.0 / total_cards), 1), common_cards;
    RAISE NOTICE 'Uncommon: %% (%)', ROUND((uncommon_cards * 100.0 / total_cards), 1), uncommon_cards;
    RAISE NOTICE 'Rare: %% (%)', ROUND((rare_cards * 100.0 / total_cards), 1), rare_cards;
    RAISE NOTICE 'Legendary: %% (%)', ROUND((legendary_cards * 100.0 / total_cards), 1), legendary_cards;
    RAISE NOTICE 'Exotic: %% (%)', ROUND((exotic_cards * 100.0 / total_cards), 1), exotic_cards;
    
    -- Also check the exotic flag consistency
    DECLARE
        exotic_flag_cards INTEGER;
        exotic_both INTEGER;
    BEGIN
        SELECT COUNT(*) INTO exotic_flag_cards FROM card_complete WHERE exotic = true;
        SELECT COUNT(*) INTO exotic_both FROM card_complete WHERE rarity = 'exotic' AND exotic = true;
        
        RAISE NOTICE '';
        RAISE NOTICE '=== EXOTIC FLAG VERIFICATION ===';
        RAISE NOTICE 'Cards with rarity = exotic: %', exotic_cards;
        RAISE NOTICE 'Cards with exotic = true: %', exotic_flag_cards;
        RAISE NOTICE 'Cards with both: %', exotic_both;
        
        IF exotic_cards = exotic_flag_cards AND exotic_cards = exotic_both THEN
            RAISE NOTICE '✅ Exotic mapping is consistent';
        ELSE
            RAISE NOTICE '❌ Exotic mapping has inconsistencies';
        END IF;
    END;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '';
  RAISE NOTICE '=== RARITY DISTRIBUTION ANALYSIS COMPLETE ===';
  RAISE NOTICE 'Check the logs above for complete breakdown';
END $$;