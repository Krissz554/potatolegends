-- Create a table to show card rarity distribution from card_complete

-- 1. Create a temporary table to hold the results
CREATE TEMP TABLE rarity_distribution (
    rarity_type TEXT,
    card_count INTEGER,
    percentage DECIMAL(5,2)
);

-- 2. Insert the rarity counts
INSERT INTO rarity_distribution (rarity_type, card_count, percentage)
WITH rarity_counts AS (
    SELECT 
        rarity,
        COUNT(*) as count,
        ROUND((COUNT(*) * 100.0 / (SELECT COUNT(*) FROM card_complete)), 2) as pct
    FROM card_complete
    GROUP BY rarity
),
total_count AS (
    SELECT COUNT(*) as total FROM card_complete
),
exotic_flag_count AS (
    SELECT COUNT(*) as exotic_true_count FROM card_complete WHERE exotic = true
)
SELECT 
    rarity as rarity_type,
    count as card_count,
    pct as percentage
FROM rarity_counts
UNION ALL
SELECT 
    'TOTAL' as rarity_type,
    (SELECT total FROM total_count) as card_count,
    100.00 as percentage
UNION ALL
SELECT 
    'exotic_flag_check' as rarity_type,
    (SELECT exotic_true_count FROM exotic_flag_count) as card_count,
    0.00 as percentage
ORDER BY 
    CASE rarity_type
        WHEN 'exotic' THEN 1
        WHEN 'legendary' THEN 2
        WHEN 'rare' THEN 3
        WHEN 'uncommon' THEN 4
        WHEN 'common' THEN 5
        WHEN 'exotic_flag_check' THEN 6
        WHEN 'TOTAL' THEN 7
        ELSE 8
    END;

-- 3. Display the results
SELECT 
    rarity_type as "Rarity Type",
    card_count as "Card Count",
    percentage as "Percentage"
FROM rarity_distribution;

-- 4. Also show a summary table with just the main rarities
SELECT 
    'Card Distribution Summary' as "Summary";

SELECT 
    rarity_type as "Rarity",
    card_count as "Count",
    CASE 
        WHEN rarity_type = 'TOTAL' THEN '100.00%'
        WHEN rarity_type = 'exotic_flag_check' THEN 'exotic=true'
        ELSE percentage || '%'
    END as "Percentage/Note"
FROM rarity_distribution
WHERE rarity_type IN ('common', 'uncommon', 'rare', 'legendary', 'exotic', 'exotic_flag_check', 'TOTAL');

-- 5. Check for any unexpected rarities
SELECT 
    'Unexpected Rarities Check' as "Check";

SELECT 
    rarity as "Unknown Rarity",
    COUNT(*) as "Count"
FROM card_complete 
WHERE rarity NOT IN ('common', 'uncommon', 'rare', 'legendary', 'exotic')
GROUP BY rarity;

-- 6. Verification check
SELECT 
    'Exotic Consistency Check' as "Verification";

SELECT 
    'rarity=exotic cards' as "Type",
    COUNT(*) as "Count"
FROM card_complete 
WHERE rarity = 'exotic'
UNION ALL
SELECT 
    'exotic=true cards' as "Type",
    COUNT(*) as "Count"
FROM card_complete 
WHERE exotic = true
UNION ALL
SELECT 
    'both conditions' as "Type",
    COUNT(*) as "Count"
FROM card_complete 
WHERE rarity = 'exotic' AND exotic = true;