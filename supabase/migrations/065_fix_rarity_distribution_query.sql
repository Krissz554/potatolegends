-- Fixed version of card rarity distribution analysis

-- 1. Simple rarity distribution query
SELECT 
    'Card Rarity Distribution' as analysis_type;

SELECT 
    rarity as "Rarity Type",
    COUNT(*) as "Card Count",
    ROUND((COUNT(*) * 100.0 / (SELECT COUNT(*) FROM card_complete)), 2) as "Percentage"
FROM card_complete
GROUP BY rarity
ORDER BY 
    CASE rarity
        WHEN 'exotic' THEN 1
        WHEN 'legendary' THEN 2
        WHEN 'rare' THEN 3
        WHEN 'uncommon' THEN 4
        WHEN 'common' THEN 5
        ELSE 6
    END;

-- 2. Total count
SELECT 
    'Total Cards' as analysis_type;

SELECT 
    'TOTAL' as "Rarity Type",
    COUNT(*) as "Card Count",
    100.00 as "Percentage"
FROM card_complete;

-- 3. Exotic flag verification
SELECT 
    'Exotic Flag Verification' as analysis_type;

SELECT 
    'Cards with rarity = exotic' as "Check Type",
    COUNT(*) as "Count"
FROM card_complete 
WHERE rarity = 'exotic'

UNION ALL

SELECT 
    'Cards with exotic = true' as "Check Type",
    COUNT(*) as "Count"
FROM card_complete 
WHERE exotic = true

UNION ALL

SELECT 
    'Cards with both conditions' as "Check Type",
    COUNT(*) as "Count"
FROM card_complete 
WHERE rarity = 'exotic' AND exotic = true;

-- 4. Check for unexpected rarities
SELECT 
    'Unknown Rarities Check' as analysis_type;

SELECT 
    COALESCE(rarity, 'NULL') as "Unknown Rarity",
    COUNT(*) as "Count"
FROM card_complete 
WHERE rarity NOT IN ('common', 'uncommon', 'rare', 'legendary', 'exotic')
   OR rarity IS NULL
GROUP BY rarity;

-- 5. Summary statistics
SELECT 
    'Summary Statistics' as analysis_type;

SELECT 
    'Total Cards in Database' as "Statistic",
    COUNT(*)::text as "Value"
FROM card_complete

UNION ALL

SELECT 
    'Number of Different Rarities' as "Statistic",
    COUNT(DISTINCT rarity)::text as "Value"
FROM card_complete

UNION ALL

SELECT 
    'Most Common Rarity' as "Statistic",
    (SELECT rarity FROM card_complete GROUP BY rarity ORDER BY COUNT(*) DESC LIMIT 1) as "Value"

UNION ALL

SELECT 
    'Least Common Rarity' as "Statistic",
    (SELECT rarity FROM card_complete GROUP BY rarity ORDER BY COUNT(*) ASC LIMIT 1) as "Value";