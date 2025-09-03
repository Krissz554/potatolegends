-- Fix legendary and exotic flags based on rarity
-- This ensures legendary cards have is_legendary = true and proper exotic handling

-- Update legendary and exotic flags
UPDATE potato_registry SET
  is_legendary = CASE 
    WHEN rarity = 'legendary' THEN true
    ELSE COALESCE(is_legendary, false)
  END,
  is_exotic = CASE 
    WHEN rarity = 'legendary' THEN true  -- All legendary cards are exotic (1 copy max)
    WHEN rarity = 'epic' AND (
      -- Make some epic cards exotic based on deterministic criteria
      -- Using registry_id hash for consistency
      (hashtext(registry_id) % 5) = 0  -- 20% of epic cards become exotic
    ) THEN true
    ELSE COALESCE(is_exotic, false)
  END;

-- Verify the changes
DO $$ 
DECLARE
  legendary_count INTEGER;
  exotic_count INTEGER;
  legendary_exotic_count INTEGER;
BEGIN
  -- Count legendary cards
  SELECT COUNT(*) INTO legendary_count 
  FROM potato_registry 
  WHERE rarity = 'legendary';
  
  -- Count cards marked as legendary
  SELECT COUNT(*) INTO legendary_exotic_count 
  FROM potato_registry 
  WHERE is_legendary = true;
  
  -- Count exotic cards
  SELECT COUNT(*) INTO exotic_count 
  FROM potato_registry 
  WHERE is_exotic = true;
  
  RAISE NOTICE 'Legendary cards (rarity): %', legendary_count;
  RAISE NOTICE 'Cards marked as legendary (is_legendary): %', legendary_exotic_count;
  RAISE NOTICE 'Total exotic cards (is_exotic): %', exotic_count;
  
  IF legendary_count = legendary_exotic_count THEN
    RAISE NOTICE '✅ All legendary rarity cards now have is_legendary = true';
  ELSE
    RAISE WARNING '❌ Mismatch: % legendary rarity cards but % marked as legendary', legendary_count, legendary_exotic_count;
  END IF;
END $$;

-- Show examples
DO $$
DECLARE
  rec RECORD;
BEGIN
  RAISE NOTICE 'Examples of legendary cards:';
  FOR rec IN 
    SELECT name, rarity, is_legendary, is_exotic 
    FROM potato_registry 
    WHERE rarity = 'legendary' 
    LIMIT 3
  LOOP
    RAISE NOTICE '  % - rarity: %, is_legendary: %, is_exotic: %', 
      rec.name, rec.rarity, rec.is_legendary, rec.is_exotic;
  END LOOP;
  
  RAISE NOTICE 'Examples of exotic epic cards:';
  FOR rec IN 
    SELECT name, rarity, is_legendary, is_exotic 
    FROM potato_registry 
    WHERE rarity = 'epic' AND is_exotic = true
    LIMIT 3
  LOOP
    RAISE NOTICE '  % - rarity: %, is_legendary: %, is_exotic: %', 
      rec.name, rec.rarity, rec.is_legendary, rec.is_exotic;
  END LOOP;
END $$;