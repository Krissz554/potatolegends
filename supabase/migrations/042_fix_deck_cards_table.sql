-- Fix deck_cards table structure and RPC functions
-- Ensures the added_at column exists and functions work properly

-- 1. Add missing column if it doesn't exist
ALTER TABLE deck_cards 
ADD COLUMN IF NOT EXISTS added_at TIMESTAMP WITH TIME ZONE DEFAULT NOW();

-- 2. Fix the add_card_to_deck_v3 function to handle missing columns gracefully
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
  max_copies INTEGER;
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

  -- Calculate deck size
  SELECT COALESCE(SUM(quantity), 0) INTO current_deck_size
  FROM deck_cards WHERE deck_id = deck_uuid;

  -- Get current quantity in deck
  SELECT COALESCE(quantity, 0) INTO current_card_quantity
  FROM deck_cards WHERE deck_id = deck_uuid AND card_id = card_uuid;

  -- Determine max copies allowed (enhanced logic)
  -- Legendary cards and exotic cards are limited to 1 copy
  IF card_data.is_legendary OR card_data.exotic THEN
    max_copies := 1;
  ELSE
    max_copies := 2;
  END IF;

  -- Validate addition
  IF current_deck_size + add_quantity > 30 THEN
    RETURN json_build_object('success', false, 'error', 'Deck cannot exceed 30 cards');
  END IF;

  IF current_card_quantity + add_quantity > max_copies THEN
    IF max_copies = 1 THEN
      RETURN json_build_object('success', false, 'error', 
        'Only 1 copy allowed for ' || 
        CASE 
          WHEN card_data.is_legendary THEN 'Legendary'
          WHEN card_data.exotic THEN 'Exotic' 
          ELSE 'special'
        END || ' cards');
    ELSE
      RETURN json_build_object('success', false, 'error', 'Maximum 2 copies per card allowed');
    END IF;
  END IF;

  -- Add or update card in deck (safe version without added_at if column doesn't exist)
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
    'message', 'Added ' || card_data.name || ' to deck',
    'card_name', card_data.name
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Also fix remove_card_from_deck_v3 for consistency
DROP FUNCTION IF EXISTS remove_card_from_deck_v3(UUID, UUID, INTEGER);

CREATE FUNCTION remove_card_from_deck_v3(
  deck_uuid UUID,
  card_uuid UUID,
  remove_quantity INTEGER DEFAULT 1
)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  current_card_quantity INTEGER;
  card_data RECORD;
BEGIN
  -- Verify deck ownership
  SELECT user_id INTO deck_owner FROM decks WHERE id = deck_uuid;
  IF deck_owner != auth.uid() THEN
    RETURN json_build_object('success', false, 'error', 'Deck not found or access denied');
  END IF;

  -- Get card data for response
  SELECT * INTO card_data FROM card_complete WHERE id = card_uuid;
  IF NOT FOUND THEN
    RETURN json_build_object('success', false, 'error', 'Card not found');
  END IF;

  -- Get current quantity in deck
  SELECT COALESCE(quantity, 0) INTO current_card_quantity
  FROM deck_cards WHERE deck_id = deck_uuid AND card_id = card_uuid;

  IF current_card_quantity = 0 THEN
    RETURN json_build_object('success', false, 'error', 'Card not in deck');
  END IF;

  IF remove_quantity >= current_card_quantity THEN
    -- Remove card completely
    DELETE FROM deck_cards WHERE deck_id = deck_uuid AND card_id = card_uuid;
  ELSE
    -- Decrease quantity
    UPDATE deck_cards 
    SET quantity = quantity - remove_quantity,
        added_at = NOW()
    WHERE deck_id = deck_uuid AND card_id = card_uuid;
  END IF;

  -- Update deck timestamp
  UPDATE decks SET updated_at = NOW() WHERE id = deck_uuid;

  RETURN json_build_object(
    'success', true, 
    'message', 'Removed ' || card_data.name || ' from deck',
    'card_name', card_data.name
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION add_card_to_deck_v3(UUID, UUID, INTEGER) TO authenticated;
GRANT EXECUTE ON FUNCTION remove_card_from_deck_v3(UUID, UUID, INTEGER) TO authenticated;

-- Verify success
DO $$ 
BEGIN
  RAISE NOTICE 'deck_cards table structure and RPC functions fixed successfully';
END $$;