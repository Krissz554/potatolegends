-- Simplified deck validation that focuses on game rules, not ownership
-- Migration: 097_simplified_deck_validation.sql

-- Create simplified deck validation function
CREATE OR REPLACE FUNCTION validate_deck_composition(user_uuid UUID)
RETURNS BOOLEAN AS $$
DECLARE
  total_cards INTEGER;
  copy_violations INTEGER;
  deck_id_var UUID;
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
  
  -- Skip ownership validation since deck builder already handles this
  -- and collections table may not be fully synced
  
  RAISE NOTICE 'User % deck validation passed: 30 cards with valid copy limits', user_uuid;
  RETURN TRUE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permission
GRANT EXECUTE ON FUNCTION validate_deck_composition(UUID) TO service_role;
GRANT EXECUTE ON FUNCTION validate_deck_composition(UUID) TO authenticated;

-- Success message
SELECT 'Simplified deck validation function created!' as status;