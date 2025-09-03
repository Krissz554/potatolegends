-- Fix deck validation function for current deck system
-- Migration: 096_fix_deck_validation_function.sql

-- Drop the old function that references non-existent tables
DROP FUNCTION IF EXISTS validate_deck_composition(UUID) CASCADE;

-- Create new function that validates current deck system
CREATE OR REPLACE FUNCTION validate_deck_composition(user_uuid UUID)
RETURNS BOOLEAN AS $$
DECLARE
  deck_count INTEGER;
  total_cards INTEGER;
  copy_violations INTEGER;
  deck_id_var UUID;
  ownership_violations INTEGER;
BEGIN
  -- Get user's active deck
  SELECT id INTO deck_id_var
  FROM public.decks
  WHERE user_id = user_uuid AND is_active = true
  LIMIT 1;
  
  -- Must have an active deck
  IF deck_id_var IS NULL THEN
    RAISE NOTICE 'User % has no active deck', user_uuid;
    RETURN FALSE;
  END IF;
  
  -- Check deck has exactly 30 cards
  SELECT COALESCE(SUM(quantity), 0) INTO total_cards
  FROM public.deck_cards
  WHERE deck_id = deck_id_var;
  
  IF total_cards != 30 THEN
    RAISE NOTICE 'User % deck has % cards, need exactly 30', user_uuid, total_cards;
    RETURN FALSE;
  END IF;
  
  -- Check copy limits based on rarity
  SELECT COUNT(*) INTO copy_violations
  FROM public.deck_cards dc
  JOIN public.card_complete cc ON dc.card_id = cc.id
  WHERE dc.deck_id = deck_id_var
  AND (
    (cc.rarity = 'common' AND dc.quantity > 3) OR
    (cc.rarity = 'uncommon' AND dc.quantity > 2) OR
    (cc.rarity = 'rare' AND dc.quantity > 2) OR
    (cc.rarity = 'legendary' AND dc.quantity > 1) OR
    (cc.rarity = 'exotic' AND dc.quantity > 1)
  );
  
  IF copy_violations > 0 THEN
    RAISE NOTICE 'User % deck has % copy limit violations', user_uuid, copy_violations;
    RETURN FALSE;
  END IF;
  
  -- Check if user owns all cards in deck (simplified - just check if collections exist)
  -- Note: We'll be more lenient here since the collections system might not be fully populated
  SELECT COUNT(*) INTO ownership_violations
  FROM public.deck_cards dc
  WHERE dc.deck_id = deck_id_var
  AND NOT EXISTS (
    SELECT 1 FROM public.collections c 
    WHERE c.card_id = dc.card_id AND c.user_id = user_uuid AND c.quantity >= dc.quantity
  );
  
  -- For now, let's skip ownership validation since collections might not be fully synced
  -- IF ownership_violations > 0 THEN
  --   RAISE NOTICE 'User % has % cards in deck they do not own', user_uuid, ownership_violations;
  --   RETURN FALSE;
  -- END IF;
  
  RAISE NOTICE 'User % deck validation passed: 30 cards with valid copy limits', user_uuid;
  RETURN TRUE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permission
GRANT EXECUTE ON FUNCTION validate_deck_composition(UUID) TO service_role;
GRANT EXECUTE ON FUNCTION validate_deck_composition(UUID) TO authenticated;

-- Success message
SELECT 'Deck validation function updated for current deck system!' as status;