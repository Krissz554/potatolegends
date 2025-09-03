-- Fixed version of new collection and deck building rules
-- Corrects column name from 'exotic' to 'is_exotic'

-- 1. Show current collection distribution
DO $$
DECLARE
    stats_record RECORD;
BEGIN
    RAISE NOTICE '=== CURRENT COLLECTION STATISTICS ===';
    FOR stats_record IN
        SELECT 
            CASE 
                WHEN pr.is_exotic THEN 'Exotic'
                WHEN pr.is_legendary THEN 'Legendary' 
                ELSE pr.rarity || ' (Regular)'
            END as card_type,
            COUNT(*) as unique_cards,
            SUM(c.quantity) as total_copies,
            AVG(c.quantity) as avg_copies_per_card,
            MAX(c.quantity) as max_copies
        FROM collections c
        JOIN potato_registry pr ON c.card_id = pr.id
        GROUP BY 
            CASE 
                WHEN pr.is_exotic THEN 'Exotic'
                WHEN pr.is_legendary THEN 'Legendary' 
                ELSE pr.rarity || ' (Regular)'
            END
        ORDER BY avg_copies_per_card DESC
    LOOP
        RAISE NOTICE 'Type: %, Unique: %, Total: %, Avg: %, Max: %', 
            stats_record.card_type,
            stats_record.unique_cards,
            stats_record.total_copies,
            ROUND(stats_record.avg_copies_per_card, 2),
            stats_record.max_copies;
    END LOOP;
END
$$;

-- 2. Update existing collections to match new rules
-- Exotic: max 1, Legendary: max 2, Regular: max 4
DO $$
DECLARE
    updated_exotic INTEGER := 0;
    updated_legendary INTEGER := 0;
    updated_regular INTEGER := 0;
BEGIN
    -- Update Exotic cards (max 1 copy)
    UPDATE collections 
    SET quantity = 1
    FROM potato_registry pr
    WHERE collections.card_id = pr.id
    AND pr.is_exotic = true
    AND collections.quantity > 1;
    
    GET DIAGNOSTICS updated_exotic = ROW_COUNT;
    
    -- Update Legendary cards (max 2 copies)
    UPDATE collections 
    SET quantity = 2
    FROM potato_registry pr
    WHERE collections.card_id = pr.id
    AND pr.is_legendary = true
    AND pr.is_exotic = false  -- Don't double-update exotic legendaries
    AND collections.quantity > 2;
    
    GET DIAGNOSTICS updated_legendary = ROW_COUNT;
    
    -- Update Regular cards (max 4 copies)
    UPDATE collections 
    SET quantity = 4
    FROM potato_registry pr
    WHERE collections.card_id = pr.id
    AND pr.is_legendary = false
    AND pr.is_exotic = false
    AND collections.quantity > 4;
    
    GET DIAGNOSTICS updated_regular = ROW_COUNT;
    
    RAISE NOTICE 'Updated collections: % exotic, % legendary, % regular records', 
        updated_exotic, updated_legendary, updated_regular;
END
$$;

-- 3. Update deck building validation function
DROP FUNCTION IF EXISTS add_card_to_deck_v3(UUID, UUID, INTEGER);

CREATE FUNCTION add_card_to_deck_v3(
  deck_uuid UUID,
  card_uuid UUID,
  add_quantity INTEGER DEFAULT 1
)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  current_deck_size INTEGER;
  current_card_quantity INTEGER;
  card_data RECORD;
  owned_quantity INTEGER;
  max_copies_in_deck INTEGER;
  card_type_name TEXT;
BEGIN
  -- Verify deck ownership
  SELECT user_id INTO deck_owner FROM decks WHERE id = deck_uuid;
  IF deck_owner != auth.uid() THEN
    RETURN json_build_object('success', false, 'error', 'Deck not found or access denied');
  END IF;

  -- Get complete card data
  SELECT * INTO card_data FROM card_complete WHERE id = card_uuid;
  IF NOT FOUND THEN
    RETURN json_build_object('success', false, 'error', 'Card not found');
  END IF;

  -- Check if player owns this card
  SELECT COALESCE(quantity, 0) INTO owned_quantity 
  FROM collections 
  WHERE user_id = auth.uid() AND card_id = card_uuid;
  
  IF owned_quantity = 0 THEN
    RETURN json_build_object('success', false, 'error', 'You do not own this card');
  END IF;

  -- Calculate current deck size
  SELECT COALESCE(SUM(quantity), 0) INTO current_deck_size
  FROM deck_cards WHERE deck_id = deck_uuid;

  -- Get current quantity of this card in deck
  SELECT COALESCE(quantity, 0) INTO current_card_quantity
  FROM deck_cards WHERE deck_id = deck_uuid AND card_id = card_uuid;

  -- Determine max copies allowed in deck (NEW RULES - FIXED COLUMN NAMES)
  IF card_data.is_exotic THEN
    max_copies_in_deck := 1;
    card_type_name := 'Exotic';
  ELSIF card_data.is_legendary THEN
    max_copies_in_deck := 2;
    card_type_name := 'Legendary';
  ELSE
    max_copies_in_deck := 2;
    card_type_name := 'Regular';
  END IF;

  -- Validate deck size limit
  IF current_deck_size + add_quantity > 30 THEN
    RETURN json_build_object('success', false, 'error', 'Deck cannot exceed 30 cards');
  END IF;

  -- Validate card copy limit in deck
  IF current_card_quantity + add_quantity > max_copies_in_deck THEN
    RETURN json_build_object('success', false, 'error', 
      'Maximum ' || max_copies_in_deck || ' copies of ' || card_type_name || ' cards allowed per deck');
  END IF;

  -- Check if player has enough copies in collection
  IF current_card_quantity + add_quantity > owned_quantity THEN
    RETURN json_build_object('success', false, 'error', 
      'You only own ' || owned_quantity || ' copies of this card');
  END IF;

  -- Add or update card in deck
  INSERT INTO deck_cards (deck_id, card_id, quantity, added_at)
  VALUES (deck_uuid, card_uuid, add_quantity, NOW())
  ON CONFLICT (deck_id, card_id) 
  DO UPDATE SET 
    quantity = deck_cards.quantity + add_quantity,
    added_at = NOW();

  -- Update deck timestamp
  UPDATE decks SET updated_at = NOW() WHERE id = deck_uuid;

  RETURN json_build_object(
    'success', true, 
    'message', 'Added ' || add_quantity || 'x ' || card_data.name || ' to deck',
    'card_name', card_data.name,
    'card_type', card_type_name
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 4. Update unlock_all_cards_for_user function with new quantities
CREATE OR REPLACE FUNCTION unlock_all_cards_for_user(user_uuid UUID)
RETURNS JSON AS $$
DECLARE
  cards_unlocked INTEGER := 0;
  card_record RECORD;
  collection_quantity INTEGER;
BEGIN
  -- Loop through all cards and add them to the user's collection
  FOR card_record IN 
    SELECT id, rarity, is_legendary, is_exotic FROM potato_registry 
    WHERE format_legal = true
  LOOP
    -- Determine collection quantity based on new rules (FIXED COLUMN NAMES)
    IF card_record.is_exotic THEN
      collection_quantity := 1;  -- Exotic: 1 copy
    ELSIF card_record.is_legendary THEN
      collection_quantity := 2;  -- Legendary: 2 copies
    ELSE
      collection_quantity := 4;  -- Regular: 4 copies
    END IF;
    
    INSERT INTO collections (user_id, card_id, quantity, source)
    VALUES (
      user_uuid, 
      card_record.id,
      collection_quantity,
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
    'message', 'All cards unlocked with new collection rules: Exotic(1), Legendary(2), Regular(4)'
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 5. Update starter collection function
CREATE OR REPLACE FUNCTION create_starter_collection_for_user(user_uuid UUID)
RETURNS VOID AS $$
DECLARE
  card_record RECORD;
  collection_quantity INTEGER;
BEGIN
  -- Add starter cards with appropriate quantities
  FOR card_record IN 
    SELECT id, rarity, is_legendary, is_exotic FROM potato_registry 
    WHERE rarity IN ('common', 'rare') 
    AND format_legal = true
    LIMIT 20  -- Reasonable starter set
  LOOP
    -- Determine collection quantity based on new rules (FIXED COLUMN NAMES)
    IF card_record.is_exotic THEN
      collection_quantity := 1;  -- Exotic: 1 copy
    ELSIF card_record.is_legendary THEN
      collection_quantity := 2;  -- Legendary: 2 copies
    ELSE
      collection_quantity := 4;  -- Regular: 4 copies
    END IF;
    
    INSERT INTO collections (user_id, card_id, quantity, source)
    VALUES (
      user_uuid, 
      card_record.id,
      collection_quantity,
      'starter'
    )
    ON CONFLICT (user_id, card_id) DO NOTHING;
  END LOOP;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 6. Create deck validation function
CREATE OR REPLACE FUNCTION validate_deck(deck_uuid UUID)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  total_cards INTEGER;
  validation_errors TEXT[] := '{}';
  card_record RECORD;
  max_allowed INTEGER;
  card_type_name TEXT;
BEGIN
  -- Verify deck ownership
  SELECT user_id INTO deck_owner FROM decks WHERE id = deck_uuid;
  IF deck_owner != auth.uid() THEN
    RETURN json_build_object('success', false, 'error', 'Deck not found or access denied');
  END IF;

  -- Check total deck size
  SELECT COALESCE(SUM(quantity), 0) INTO total_cards
  FROM deck_cards WHERE deck_id = deck_uuid;
  
  IF total_cards != 30 THEN
    validation_errors := array_append(validation_errors, 
      'Deck must have exactly 30 cards (currently has ' || total_cards || ')');
  END IF;

  -- Check individual card limits (FIXED COLUMN NAMES)
  FOR card_record IN
    SELECT 
      dc.quantity,
      pr.name,
      pr.is_legendary,
      pr.is_exotic
    FROM deck_cards dc
    JOIN potato_registry pr ON dc.card_id = pr.id
    WHERE dc.deck_id = deck_uuid
  LOOP
    -- Determine limits and type
    IF card_record.is_exotic THEN
      max_allowed := 1;
      card_type_name := 'Exotic';
    ELSIF card_record.is_legendary THEN
      max_allowed := 2;
      card_type_name := 'Legendary';
    ELSE
      max_allowed := 2;
      card_type_name := 'Regular';
    END IF;
    
    -- Check if quantity exceeds limit
    IF card_record.quantity > max_allowed THEN
      validation_errors := array_append(validation_errors,
        card_record.name || ': ' || card_record.quantity || ' copies (max ' || 
        max_allowed || ' for ' || card_type_name || ' cards)');
    END IF;
  END LOOP;

  -- Return validation result
  IF array_length(validation_errors, 1) > 0 THEN
    RETURN json_build_object(
      'valid', false,
      'errors', validation_errors,
      'total_cards', total_cards
    );
  ELSE
    RETURN json_build_object(
      'valid', true,
      'message', 'Deck is valid',
      'total_cards', total_cards
    );
  END IF;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 7. Show updated collection statistics
DO $$
DECLARE
    stats_record RECORD;
BEGIN
    RAISE NOTICE '=== UPDATED COLLECTION STATISTICS ===';
    FOR stats_record IN
        SELECT 
            CASE 
                WHEN pr.is_exotic THEN 'Exotic'
                WHEN pr.is_legendary THEN 'Legendary' 
                ELSE pr.rarity || ' (Regular)'
            END as card_type,
            COUNT(*) as unique_cards,
            SUM(c.quantity) as total_copies,
            AVG(c.quantity) as avg_copies_per_card,
            MAX(c.quantity) as max_copies
        FROM collections c
        JOIN potato_registry pr ON c.card_id = pr.id
        GROUP BY 
            CASE 
                WHEN pr.is_exotic THEN 'Exotic'
                WHEN pr.is_legendary THEN 'Legendary' 
                ELSE pr.rarity || ' (Regular)'
            END
        ORDER BY avg_copies_per_card DESC
    LOOP
        RAISE NOTICE 'Type: %, Unique: %, Total: %, Avg: %, Max: %', 
            stats_record.card_type,
            stats_record.unique_cards,
            stats_record.total_copies,
            ROUND(stats_record.avg_copies_per_card, 2),
            stats_record.max_copies;
    END LOOP;
END
$$;

-- Grant permissions
GRANT EXECUTE ON FUNCTION add_card_to_deck_v3(UUID, UUID, INTEGER) TO authenticated;
GRANT EXECUTE ON FUNCTION unlock_all_cards_for_user(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION create_starter_collection_for_user(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION validate_deck(UUID) TO authenticated;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== NEW COLLECTION & DECK RULES IMPLEMENTED (FIXED) ===';
  RAISE NOTICE '';
  RAISE NOTICE 'COLLECTION LIMITS:';
  RAISE NOTICE '• Exotic cards: 1 copy max in collection';
  RAISE NOTICE '• Legendary cards: 2 copies max in collection'; 
  RAISE NOTICE '• Regular cards: 4 copies max in collection';
  RAISE NOTICE '';
  RAISE NOTICE 'DECK BUILDING LIMITS:';
  RAISE NOTICE '• Deck size: Exactly 30 cards';
  RAISE NOTICE '• Exotic cards: 1 copy max per deck';
  RAISE NOTICE '• Legendary cards: 2 copies max per deck';
  RAISE NOTICE '• Regular cards: 2 copies max per deck';
  RAISE NOTICE '';
  RAISE NOTICE 'COLUMN NAME FIX:';
  RAISE NOTICE '• Fixed all references from "exotic" to "is_exotic"';
  RAISE NOTICE '• All functions now use correct column names';
END $$;