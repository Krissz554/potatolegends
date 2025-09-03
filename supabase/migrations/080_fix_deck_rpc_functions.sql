-- Fix missing RPC functions for deck operations
-- These functions should have been created in earlier migrations but seem to be missing

-- Drop existing functions if they exist to ensure clean recreation
DROP FUNCTION IF EXISTS add_card_to_deck_v3(UUID, UUID, INTEGER);
DROP FUNCTION IF EXISTS remove_card_from_deck_v3(UUID, UUID, INTEGER);
DROP FUNCTION IF EXISTS validate_deck(UUID);

-- Create add_card_to_deck_v3 function
CREATE OR REPLACE FUNCTION add_card_to_deck_v3(
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
    'message', 'Added ' || card_data.name || ' to deck',
    'card_name', card_data.name
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create remove_card_from_deck_v3 function
CREATE OR REPLACE FUNCTION remove_card_from_deck_v3(
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

-- Create validate_deck function
CREATE OR REPLACE FUNCTION validate_deck(deck_uuid UUID)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  total_cards INTEGER;
  duplicate_check RECORD;
  errors TEXT[] := '{}';
BEGIN
  -- Verify deck ownership
  SELECT user_id INTO deck_owner FROM decks WHERE id = deck_uuid;
  IF deck_owner != auth.uid() THEN
    RETURN json_build_object('valid', false, 'errors', ARRAY['Deck not found or access denied']);
  END IF;

  -- Check total card count
  SELECT COALESCE(SUM(quantity), 0) INTO total_cards
  FROM deck_cards WHERE deck_id = deck_uuid;

  IF total_cards != 30 THEN
    errors := array_append(errors, 'Deck must contain exactly 30 cards (currently has ' || total_cards || ')');
  END IF;

  -- Check for card copy limits
  FOR duplicate_check IN
    SELECT 
      dc.card_id,
      dc.quantity,
      cc.name,
      cc.is_legendary,
      cc.exotic
    FROM deck_cards dc
    JOIN card_complete cc ON dc.card_id = cc.id
    WHERE dc.deck_id = deck_uuid
  LOOP
    IF duplicate_check.is_legendary OR duplicate_check.exotic THEN
      IF duplicate_check.quantity > 1 THEN
        errors := array_append(errors, 
          'Card "' || duplicate_check.name || '" can only have 1 copy (has ' || duplicate_check.quantity || ')');
      END IF;
    ELSE
      IF duplicate_check.quantity > 2 THEN
        errors := array_append(errors, 
          'Card "' || duplicate_check.name || '" can only have 2 copies (has ' || duplicate_check.quantity || ')');
      END IF;
    END IF;
  END LOOP;

  RETURN json_build_object(
    'valid', array_length(errors, 1) IS NULL,
    'errors', errors,
    'total_cards', total_cards
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION add_card_to_deck_v3(UUID, UUID, INTEGER) TO authenticated;
GRANT EXECUTE ON FUNCTION remove_card_from_deck_v3(UUID, UUID, INTEGER) TO authenticated;
GRANT EXECUTE ON FUNCTION validate_deck(UUID) TO authenticated;

-- Verify functions were created
DO $$ 
BEGIN
  RAISE NOTICE 'RPC functions created successfully:';
  RAISE NOTICE '- add_card_to_deck_v3';
  RAISE NOTICE '- remove_card_from_deck_v3'; 
  RAISE NOTICE '- validate_deck';
END $$;