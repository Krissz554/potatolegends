-- Fix collection quantity rules to match deck building rules
-- Currently: Collection gives 3 copies but decks only allow 2 copies (mismatch)
-- Solution: Align collection quantities with deck building limits

-- 1. Show current collection quantities by rarity
DO $$
DECLARE
    rarity_record RECORD;
BEGIN
    RAISE NOTICE '=== CURRENT COLLECTION QUANTITIES BY RARITY ===';
    FOR rarity_record IN
        SELECT 
            pr.rarity,
            AVG(c.quantity) as avg_quantity,
            MIN(c.quantity) as min_quantity,
            MAX(c.quantity) as max_quantity,
            COUNT(*) as card_count
        FROM collections c
        JOIN potato_registry pr ON c.card_id = pr.id
        GROUP BY pr.rarity
        ORDER BY avg_quantity DESC
    LOOP
        RAISE NOTICE 'Rarity: %, Avg: %, Min: %, Max: %, Cards: %', 
            rarity_record.rarity,
            rarity_record.avg_quantity,
            rarity_record.min_quantity,
            rarity_record.max_quantity,
            rarity_record.card_count;
    END LOOP;
END
$$;

-- 2. Update existing collection quantities to match deck rules
-- Legendary/Exotic: 1 copy max (already correct)
-- Regular cards: 2 copies max (currently 3, needs reduction)
UPDATE collections 
SET quantity = CASE 
    WHEN pr.is_legendary OR pr.exotic THEN 1  -- Legendary/Exotic: 1 copy
    ELSE LEAST(quantity, 2)                   -- Regular: max 2 copies
END
FROM potato_registry pr
WHERE collections.card_id = pr.id
AND quantity > (CASE 
    WHEN pr.is_legendary OR pr.exotic THEN 1
    ELSE 2
END);

-- 3. Get count of affected records
DO $$
DECLARE
    updated_count INTEGER;
BEGIN
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Updated % collection records to match deck building limits', updated_count;
END
$$;

-- 4. Update the unlock_all_cards_for_user function to give correct quantities
CREATE OR REPLACE FUNCTION unlock_all_cards_for_user(user_uuid UUID)
RETURNS JSON AS $$
DECLARE
  cards_unlocked INTEGER := 0;
  card_record RECORD;
BEGIN
  -- Loop through all cards and add them to the user's collection
  FOR card_record IN 
    SELECT id, rarity, is_legendary, exotic FROM potato_registry 
    WHERE format_legal = true
  LOOP
    INSERT INTO collections (user_id, card_id, quantity, source)
    VALUES (
      user_uuid, 
      card_record.id,
      CASE 
        WHEN card_record.is_legendary OR card_record.exotic THEN 1  -- Legendary/Exotic: 1 copy
        ELSE 2  -- Regular cards: 2 copies (matches deck limit)
      END,
      'unlock_all'
    )
    ON CONFLICT (user_id, card_id) 
    DO UPDATE SET 
      quantity = GREATEST(collections.quantity, EXCLUDED.quantity),
      source = EXCLUDED.source;
    
    cards_unlocked := cards_unlocked + 1;
  END LOOP;
  
  RETURN json_build_object(
    'success', true,
    'cards_unlocked', cards_unlocked,
    'message', 'All cards unlocked with correct quantities'
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 5. Create starter collection function with correct quantities
CREATE OR REPLACE FUNCTION create_starter_collection_for_user(user_uuid UUID)
RETURNS VOID AS $$
DECLARE
  card_record RECORD;
BEGIN
  -- Add starter cards (common and some rare)
  FOR card_record IN 
    SELECT id, rarity, is_legendary, exotic FROM potato_registry 
    WHERE rarity IN ('common', 'rare') 
    AND format_legal = true
    LIMIT 20  -- Give a reasonable starter set
  LOOP
    INSERT INTO collections (user_id, card_id, quantity, source)
    VALUES (
      user_uuid, 
      card_record.id,
      CASE 
        WHEN card_record.is_legendary OR card_record.exotic THEN 1  -- Legendary/Exotic: 1 copy
        ELSE 2  -- Regular cards: 2 copies
      END,
      'starter'
    )
    ON CONFLICT (user_id, card_id) DO NOTHING;
  END LOOP;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 6. Show updated collection quantities
DO $$
DECLARE
    rarity_record RECORD;
BEGIN
    RAISE NOTICE '=== UPDATED COLLECTION QUANTITIES BY RARITY ===';
    FOR rarity_record IN
        SELECT 
            pr.rarity,
            AVG(c.quantity) as avg_quantity,
            MIN(c.quantity) as min_quantity,
            MAX(c.quantity) as max_quantity,
            COUNT(*) as card_count
        FROM collections c
        JOIN potato_registry pr ON c.card_id = pr.id
        GROUP BY pr.rarity
        ORDER BY avg_quantity DESC
    LOOP
        RAISE NOTICE 'Rarity: %, Avg: %, Min: %, Max: %, Cards: %', 
            rarity_record.rarity,
            rarity_record.avg_quantity,
            rarity_record.min_quantity,
            rarity_record.max_quantity,
            rarity_record.card_count;
    END LOOP;
END
$$;

-- Grant permissions
GRANT EXECUTE ON FUNCTION unlock_all_cards_for_user(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION create_starter_collection_for_user(UUID) TO authenticated;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== COLLECTION QUANTITY RULES FIXED ===';
  RAISE NOTICE 'Collection quantities now match deck building limits:';
  RAISE NOTICE '• Legendary/Exotic cards: 1 copy max';
  RAISE NOTICE '• Regular cards: 2 copies max';
  RAISE NOTICE 'No more unused cards in collections!';
END $$;