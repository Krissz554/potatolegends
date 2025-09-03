-- Create some exotic cards by upgrading some rare cards
-- Exotic cards are the highest rarity tier (purple, 1 copy max in deck)

-- 1. First, let's see what rare cards we have
DO $$
DECLARE
    card_record RECORD;
    rare_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO rare_count FROM potato_registry WHERE rarity = 'rare';
    RAISE NOTICE '=== CURRENT RARE CARDS (%): ===', rare_count;
    
    FOR card_record IN
        SELECT name, rarity, is_legendary, is_exotic
        FROM potato_registry 
        WHERE rarity = 'rare'
        ORDER BY name
        LIMIT 10
    LOOP
        RAISE NOTICE 'Card: % | Rarity: % | is_legendary: % | is_exotic: %', 
            card_record.name, card_record.rarity, card_record.is_legendary, card_record.is_exotic;
    END LOOP;
END
$$;

-- 2. Create exotic cards by promoting some rare cards
-- Select approximately 20% of rare cards to be exotic (keep them rare rarity but add exotic flag)
UPDATE potato_registry 
SET 
  is_exotic = true,
  craft_cost = 2000,  -- Higher craft cost for exotic cards
  dust_value = 500    -- Higher dust value for exotic cards
WHERE 
  rarity = 'rare' 
  AND is_exotic = false 
  AND is_legendary = false
  AND id IN (
    SELECT id 
    FROM potato_registry 
    WHERE rarity = 'rare' AND is_exotic = false AND is_legendary = false
    ORDER BY RANDOM()
    LIMIT (
      SELECT GREATEST(2, CAST(COUNT(*) * 0.2 AS INTEGER))
      FROM potato_registry 
      WHERE rarity = 'rare' AND is_exotic = false AND is_legendary = false
    )
  );

-- 3. Show what cards became exotic
DO $$
DECLARE
    card_record RECORD;
    exotic_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO exotic_count FROM potato_registry WHERE is_exotic = true;
    RAISE NOTICE '=== NEW EXOTIC CARDS (%): ===', exotic_count;
    
    FOR card_record IN
        SELECT name, rarity, is_legendary, is_exotic, craft_cost
        FROM potato_registry 
        WHERE is_exotic = true
        ORDER BY name
    LOOP
        RAISE NOTICE 'EXOTIC: % | Rarity: % | Craft Cost: %', 
            card_record.name, card_record.rarity, card_record.craft_cost;
    END LOOP;
END
$$;

-- 4. Verify card counts
DO $$
DECLARE
    legendary_count INTEGER;
    exotic_count INTEGER;
    rare_count INTEGER;
    uncommon_count INTEGER;
    common_count INTEGER;
    overlap_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO legendary_count FROM potato_registry WHERE is_legendary = true;
    SELECT COUNT(*) INTO exotic_count FROM potato_registry WHERE is_exotic = true;
    SELECT COUNT(*) INTO rare_count FROM potato_registry WHERE rarity = 'rare';
    SELECT COUNT(*) INTO uncommon_count FROM potato_registry WHERE rarity = 'uncommon';
    SELECT COUNT(*) INTO common_count FROM potato_registry WHERE rarity = 'common';
    SELECT COUNT(*) INTO overlap_count FROM potato_registry WHERE is_legendary = true AND is_exotic = true;
    
    RAISE NOTICE '=== FINAL RARITY DISTRIBUTION ===';
    RAISE NOTICE 'Common cards: %', common_count;
    RAISE NOTICE 'Uncommon cards: %', uncommon_count;
    RAISE NOTICE 'Rare cards: %', rare_count;
    RAISE NOTICE 'Legendary cards: %', legendary_count;
    RAISE NOTICE 'Exotic cards: %', exotic_count;
    RAISE NOTICE 'Legendary+Exotic overlap (should be 0): %', overlap_count;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== EXOTIC CARDS CREATED ===';
  RAISE NOTICE 'Some rare cards have been promoted to exotic status';
  RAISE NOTICE 'Exotic cards have higher craft/dust values';
  RAISE NOTICE 'Exotic filter will now show cards!';
END $$;