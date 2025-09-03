-- =============================================
-- Enhanced Deck System for New Card Game
-- Creates proper 30-card decks using potato cards
-- =============================================

-- First, let's create a function to ensure users have enough potatoes for proper decks
CREATE OR REPLACE FUNCTION ensure_user_has_sufficient_potatoes(user_uuid UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  unlocked_count INTEGER;
BEGIN
  -- Count user's unlocked potatoes
  SELECT COUNT(*) INTO unlocked_count
  FROM potato_unlocks
  WHERE user_id = user_uuid;
  
  -- If user has fewer than 10 different potatoes, unlock more basics
  IF unlocked_count < 10 THEN
    -- Unlock all common and uncommon potatoes for deck building
    INSERT INTO potato_unlocks (user_id, potato_id, unlocked_at)
    SELECT 
      user_uuid,
      pr.id,
      NOW()
    FROM potato_registry pr
    WHERE pr.rarity IN ('common', 'uncommon')
      AND NOT EXISTS (
        SELECT 1 FROM potato_unlocks pu 
        WHERE pu.user_id = user_uuid AND pu.potato_id = pr.id
      )
    LIMIT (10 - unlocked_count);
  END IF;
  
  RETURN TRUE;
END;
$$;

-- Enhanced function to create proper 30-card battle decks
CREATE OR REPLACE FUNCTION ensure_user_battle_deck_30_cards(user_uuid UUID, deck_slot_num INTEGER DEFAULT 1)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  deck_count INTEGER;
  unlocked_potato RECORD;
  deck_position INTEGER := 1;
  cards_needed INTEGER := 30;
  current_cards INTEGER := 0;
BEGIN
  -- Validate deck slot
  IF deck_slot_num < 1 OR deck_slot_num > 3 THEN
    RAISE EXCEPTION 'Invalid deck slot. Must be 1, 2, or 3.';
  END IF;
  
  -- Check if user already has a 30-card deck in this slot
  SELECT COUNT(*) INTO deck_count
  FROM battle_decks
  WHERE user_id = user_uuid AND deck_slot = deck_slot_num;
  
  -- If deck already has 30 cards, return true
  IF deck_count >= 30 THEN
    RETURN TRUE;
  END IF;
  
  -- Ensure user has sufficient potatoes
  PERFORM ensure_user_has_sufficient_potatoes(user_uuid);
  
  -- Clear existing deck in this slot to rebuild properly
  DELETE FROM battle_decks 
  WHERE user_id = user_uuid AND deck_slot = deck_slot_num;
  
  -- Build 30-card deck using unlocked potatoes
  -- Strategy: Use each potato multiple times (up to 3 copies) to fill 30 cards
  FOR unlocked_potato IN 
    SELECT pr.id as potato_id, pr.rarity, pr.name
    FROM potato_unlocks pu
    JOIN potato_registry pr ON pu.potato_id = pr.id
    WHERE pu.user_id = user_uuid
    ORDER BY 
      CASE pr.rarity 
        WHEN 'common' THEN 1 
        WHEN 'uncommon' THEN 2 
        WHEN 'rare' THEN 3 
        WHEN 'legendary' THEN 4 
        WHEN 'exotic' THEN 5 
      END,
      pr.name
  LOOP
    -- Add up to 3 copies of each card
    FOR i IN 1..3 LOOP
      EXIT WHEN current_cards >= cards_needed;
      
      INSERT INTO battle_decks (user_id, potato_id, deck_position, deck_slot)
      VALUES (user_uuid, unlocked_potato.potato_id, deck_position, deck_slot_num);
      
      deck_position := deck_position + 1;
      current_cards := current_cards + 1;
    END LOOP;
    
    EXIT WHEN current_cards >= cards_needed;
  END LOOP;
  
  -- If we still don't have enough cards, fill with common potatoes
  WHILE current_cards < cards_needed LOOP
    -- Get a random common potato
    SELECT pr.id INTO unlocked_potato.potato_id
    FROM potato_unlocks pu
    JOIN potato_registry pr ON pu.potato_id = pr.id
    WHERE pu.user_id = user_uuid AND pr.rarity = 'common'
    ORDER BY RANDOM()
    LIMIT 1;
    
    INSERT INTO battle_decks (user_id, potato_id, deck_position, deck_slot)
    VALUES (user_uuid, unlocked_potato.potato_id, deck_position, deck_slot_num);
    
    deck_position := deck_position + 1;
    current_cards := current_cards + 1;
  END LOOP;
  
  RETURN TRUE;
END;
$$;

-- Function to get a player's deck with full potato data for battles
CREATE OR REPLACE FUNCTION get_player_battle_deck(user_uuid UUID, deck_slot_num INTEGER DEFAULT 1)
RETURNS TABLE (
  id UUID,
  user_id UUID,
  potato_id TEXT,
  deck_position INTEGER,
  deck_slot INTEGER,
  potato JSONB,
  attack INTEGER,
  current_hp INTEGER,
  max_hp INTEGER
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
  RETURN QUERY
  SELECT 
    bd.id,
    bd.user_id,
    bd.potato_id,
    bd.deck_position,
    bd.deck_slot,
    to_jsonb(pr.*) as potato,
    CASE pr.rarity
      WHEN 'common' THEN 1
      WHEN 'uncommon' THEN 2
      WHEN 'rare' THEN 3
      WHEN 'legendary' THEN 4
      WHEN 'exotic' THEN 5
      ELSE 1
    END as attack,
    CASE pr.rarity
      WHEN 'common' THEN 1
      WHEN 'uncommon' THEN 2
      WHEN 'rare' THEN 3
      WHEN 'legendary' THEN 4
      WHEN 'exotic' THEN 5
      ELSE 1
    END as current_hp,
    CASE pr.rarity
      WHEN 'common' THEN 1
      WHEN 'uncommon' THEN 2
      WHEN 'rare' THEN 3
      WHEN 'legendary' THEN 4
      WHEN 'exotic' THEN 5
      ELSE 1
    END as max_hp
  FROM battle_decks bd
  JOIN potato_registry pr ON bd.potato_id = pr.id
  WHERE bd.user_id = user_uuid AND bd.deck_slot = deck_slot_num
  ORDER BY bd.deck_position;
END;
$$;

-- Update the original function to use the new 30-card system
CREATE OR REPLACE FUNCTION ensure_user_battle_deck(user_uuid UUID, deck_slot_num INTEGER DEFAULT 1)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
  -- For backwards compatibility, create a 30-card deck
  RETURN ensure_user_battle_deck_30_cards(user_uuid, deck_slot_num);
END;
$$;

-- Grant permissions
GRANT EXECUTE ON FUNCTION ensure_user_has_sufficient_potatoes(UUID) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION ensure_user_battle_deck_30_cards(UUID, INTEGER) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION get_player_battle_deck(UUID, INTEGER) TO authenticated, service_role;

-- Update RLS policies to ensure deck access
DO $$
BEGIN
  -- Users can view their own decks
  DROP POLICY IF EXISTS "Users can view their own battle decks" ON battle_decks;
  CREATE POLICY "Users can view their own battle decks"
    ON battle_decks FOR SELECT
    USING (auth.uid() = user_id);
  
  -- Users can manage their own decks
  DROP POLICY IF EXISTS "Users can manage their own battle decks" ON battle_decks;
  CREATE POLICY "Users can manage their own battle decks"
    ON battle_decks FOR ALL
    USING (auth.uid() = user_id);
    
EXCEPTION WHEN OTHERS THEN
  RAISE NOTICE 'Policy creation had issues: %', SQLERRM;
END $$;

-- Add comments for documentation
COMMENT ON FUNCTION ensure_user_has_sufficient_potatoes(UUID) IS 'Ensures user has at least 10 different potato types for deck building';
COMMENT ON FUNCTION ensure_user_battle_deck_30_cards(UUID, INTEGER) IS 'Creates a proper 30-card battle deck for card game';
COMMENT ON FUNCTION get_player_battle_deck(UUID, INTEGER) IS 'Retrieves a player''s battle deck with full potato data and calculated stats';

-- Success message
SELECT 'Enhanced deck system for card game created successfully! Users will now get 30-card decks.' as status;