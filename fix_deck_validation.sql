-- URGENT FIX: Update deck validation for new deck system
-- Run this in Supabase SQL Editor to fix matchmaking

-- Drop the old function first
DROP FUNCTION IF EXISTS validate_deck_composition(UUID);

-- Create new secure deck validation function for the new deck system
CREATE OR REPLACE FUNCTION validate_deck_composition(user_uuid UUID)
RETURNS BOOLEAN AS $$
DECLARE
  active_deck_id UUID;
  total_cards INTEGER;
  card_record RECORD;
  user_collection_count INTEGER;
BEGIN
  -- Get user's active deck
  SELECT id INTO active_deck_id
  FROM public.decks
  WHERE user_id = user_uuid AND is_active = TRUE
  LIMIT 1;
  
  -- No active deck found
  IF active_deck_id IS NULL THEN
    RAISE LOG 'No active deck found for user %', user_uuid;
    RETURN FALSE;
  END IF;
  
  -- Check total cards in deck (should be exactly 30)
  SELECT COALESCE(SUM(quantity), 0) INTO total_cards
  FROM public.deck_cards
  WHERE deck_id = active_deck_id;
  
  IF total_cards != 30 THEN
    RAISE LOG 'Invalid deck size for user %: % cards (expected 30)', user_uuid, total_cards;
    RETURN FALSE;
  END IF;
  
  -- Validate each card in the deck
  FOR card_record IN
    SELECT dc.card_id, dc.quantity, cc.rarity, cc.exotic, cc.is_legendary
    FROM public.deck_cards dc
    JOIN public.card_complete cc ON dc.card_id = cc.id
    WHERE dc.deck_id = active_deck_id
  LOOP
    -- Check if user actually owns this card
    SELECT COALESCE(quantity, 0) INTO user_collection_count
    FROM public.collections
    WHERE user_id = user_uuid AND card_id = card_record.card_id;
    
    -- User doesn't own this card
    IF user_collection_count = 0 THEN
      RAISE LOG 'User % does not own card %', user_uuid, card_record.card_id;
      RETURN FALSE;
    END IF;
    
    -- Check deck quantity limits based on card rarity
    IF card_record.exotic = TRUE THEN
      -- Exotic cards: max 1 in deck, max 1 owned
      IF card_record.quantity > 1 OR user_collection_count > 1 THEN
        RAISE LOG 'Exotic card % limits violated for user %: deck=%, owned=%', 
          card_record.card_id, user_uuid, card_record.quantity, user_collection_count;
        RETURN FALSE;
      END IF;
    ELSIF card_record.is_legendary = TRUE THEN
      -- Legendary cards: max 2 in deck, max 2 owned
      IF card_record.quantity > 2 OR user_collection_count > 2 THEN
        RAISE LOG 'Legendary card % limits violated for user %: deck=%, owned=%', 
          card_record.card_id, user_uuid, card_record.quantity, user_collection_count;
        RETURN FALSE;
      END IF;
    ELSE
      -- Regular cards: max 2 in deck, max 4 owned
      IF card_record.quantity > 2 OR user_collection_count > 4 THEN
        RAISE LOG 'Regular card % limits violated for user %: deck=%, owned=%', 
          card_record.card_id, user_uuid, card_record.quantity, user_collection_count;
        RETURN FALSE;
      END IF;
    END IF;
    
    -- User cannot have more cards in deck than they own
    IF card_record.quantity > user_collection_count THEN
      RAISE LOG 'User % has more cards in deck than owned for card %: deck=%, owned=%', 
        user_uuid, card_record.card_id, card_record.quantity, user_collection_count;
      RETURN FALSE;
    END IF;
  END LOOP;
  
  RAISE LOG 'Deck validation successful for user %: % total cards', user_uuid, total_cards;
  RETURN TRUE;
  
EXCEPTION WHEN OTHERS THEN
  RAISE LOG 'Deck validation failed for user % with error: %', user_uuid, SQLERRM;
  RETURN FALSE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant necessary permissions
GRANT EXECUTE ON FUNCTION validate_deck_composition(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION validate_deck_composition(UUID) TO service_role;

-- Verification: Test that the function exists and works
SELECT 'Deck validation function updated successfully!' as status;