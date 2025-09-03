-- Migration 035: Implement 30-Card Deck System
-- Complete overhaul from 5-card to 30-card deck system for proper card game mechanics

-- Create new table for 30-card deck system
CREATE TABLE IF NOT EXISTS player_decks (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  deck_name VARCHAR(50) NOT NULL DEFAULT 'New Deck',
  deck_slot INTEGER NOT NULL CHECK (deck_slot >= 1 AND deck_slot <= 10), -- Support up to 10 deck slots
  is_active BOOLEAN DEFAULT FALSE, -- Mark which deck is currently active
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  
  -- Unique constraint: one deck per user per slot
  UNIQUE(user_id, deck_slot)
);

-- Create table for individual deck cards with copy limits
CREATE TABLE IF NOT EXISTS player_deck_cards (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  deck_id UUID REFERENCES player_decks(id) ON DELETE CASCADE NOT NULL,
  potato_id UUID REFERENCES potato_registry(id) ON DELETE CASCADE NOT NULL,
  quantity INTEGER NOT NULL DEFAULT 1 CHECK (quantity >= 1 AND quantity <= 3), -- Max 3 copies per card
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  
  -- Unique constraint: one entry per card per deck
  UNIQUE(deck_id, potato_id)
);

-- Create deck presets/templates table
CREATE TABLE IF NOT EXISTS deck_templates (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  name VARCHAR(50) NOT NULL,
  description TEXT,
  is_starter BOOLEAN DEFAULT FALSE, -- Mark as starter deck template
  created_by UUID REFERENCES public.user_profiles(id) ON DELETE SET NULL,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create deck template cards
CREATE TABLE IF NOT EXISTS deck_template_cards (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  template_id UUID REFERENCES deck_templates(id) ON DELETE CASCADE NOT NULL,
  potato_id UUID REFERENCES potato_registry(id) ON DELETE CASCADE NOT NULL,
  quantity INTEGER NOT NULL DEFAULT 1 CHECK (quantity >= 1 AND quantity <= 3),
  
  UNIQUE(template_id, potato_id)
);

-- Add indexes for performance
CREATE INDEX IF NOT EXISTS idx_player_decks_user_id ON player_decks(user_id);
CREATE INDEX IF NOT EXISTS idx_player_decks_user_active ON player_decks(user_id, is_active);
CREATE INDEX IF NOT EXISTS idx_player_deck_cards_deck_id ON player_deck_cards(deck_id);
CREATE INDEX IF NOT EXISTS idx_player_deck_cards_potato_id ON player_deck_cards(potato_id);
CREATE INDEX IF NOT EXISTS idx_deck_template_cards_template_id ON deck_template_cards(template_id);

-- Function to validate deck has exactly 30 cards
CREATE OR REPLACE FUNCTION validate_deck_size(deck_uuid UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
  total_cards INTEGER;
BEGIN
  SELECT COALESCE(SUM(quantity), 0) INTO total_cards
  FROM player_deck_cards
  WHERE deck_id = deck_uuid;
  
  RETURN total_cards = 30;
END;
$$;

-- Function to get total cards in deck
CREATE OR REPLACE FUNCTION get_deck_card_count(deck_uuid UUID)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
  total_cards INTEGER;
BEGIN
  SELECT COALESCE(SUM(quantity), 0) INTO total_cards
  FROM player_deck_cards
  WHERE deck_id = deck_uuid;
  
  RETURN total_cards;
END;
$$;

-- Function to create starter deck for new users
CREATE OR REPLACE FUNCTION create_starter_deck(user_uuid UUID)
RETURNS UUID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  new_deck_id UUID;
  basic_potato RECORD;
  cards_added INTEGER := 0;
BEGIN
  -- Ensure user has basic potatoes unlocked
  PERFORM ensure_user_has_basic_potatoes(user_uuid);
  
  -- Create new deck in slot 1
  INSERT INTO player_decks (user_id, deck_name, deck_slot, is_active)
  VALUES (user_uuid, 'Starter Deck', 1, TRUE)
  RETURNING id INTO new_deck_id;
  
  -- Add cards to reach exactly 30 cards
  -- Strategy: Add 6 copies of each of the first 5 potatoes (6x5=30)
  FOR basic_potato IN 
    SELECT pu.potato_id
    FROM potato_unlocks pu
    JOIN potato_registry pr ON pu.potato_id = pr.id
    WHERE pu.user_id = user_uuid
    ORDER BY pr.sort_order, pu.unlocked_at
    LIMIT 5
  LOOP
    -- Add 6 copies of each card (but max 3 per entry, so need 2 entries)
    INSERT INTO player_deck_cards (deck_id, potato_id, quantity)
    VALUES (new_deck_id, basic_potato.potato_id, 3);
    
    INSERT INTO player_deck_cards (deck_id, potato_id, quantity)
    VALUES (new_deck_id, basic_potato.potato_id, 3)
    ON CONFLICT (deck_id, potato_id) 
    DO UPDATE SET quantity = player_deck_cards.quantity + 3;
    
    cards_added := cards_added + 6;
    
    -- Stop when we have 30 cards
    EXIT WHEN cards_added >= 30;
  END LOOP;
  
  -- If we still don't have 30 cards, fill with the first potato
  IF cards_added < 30 THEN
    DECLARE
      first_potato_id UUID;
      remaining_cards INTEGER;
    BEGIN
      SELECT pu.potato_id INTO first_potato_id
      FROM potato_unlocks pu
      JOIN potato_registry pr ON pu.potato_id = pr.id
      WHERE pu.user_id = user_uuid
      ORDER BY pr.sort_order, pu.unlocked_at
      LIMIT 1;
      
      remaining_cards := 30 - cards_added;
      
      -- Add remaining cards as a new entry or update existing
      INSERT INTO player_deck_cards (deck_id, potato_id, quantity)
      VALUES (new_deck_id, first_potato_id, remaining_cards)
      ON CONFLICT (deck_id, potato_id) 
      DO UPDATE SET quantity = LEAST(player_deck_cards.quantity + remaining_cards, 30);
    END;
  END IF;
  
  RETURN new_deck_id;
END;
$$;

-- Function to copy deck to new slot
CREATE OR REPLACE FUNCTION copy_deck_to_slot(source_deck_id UUID, target_slot INTEGER, new_deck_name VARCHAR DEFAULT NULL)
RETURNS UUID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  source_deck RECORD;
  new_deck_id UUID;
  deck_card RECORD;
BEGIN
  -- Get source deck info
  SELECT * INTO source_deck FROM player_decks WHERE id = source_deck_id;
  
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Source deck not found';
  END IF;
  
  -- Validate target slot
  IF target_slot < 1 OR target_slot > 10 THEN
    RAISE EXCEPTION 'Invalid deck slot. Must be 1-10.';
  END IF;
  
  -- Delete existing deck in target slot
  DELETE FROM player_decks WHERE user_id = source_deck.user_id AND deck_slot = target_slot;
  
  -- Create new deck
  INSERT INTO player_decks (user_id, deck_name, deck_slot, is_active)
  VALUES (
    source_deck.user_id, 
    COALESCE(new_deck_name, source_deck.deck_name || ' Copy'),
    target_slot,
    FALSE
  ) RETURNING id INTO new_deck_id;
  
  -- Copy all cards
  FOR deck_card IN 
    SELECT potato_id, quantity FROM player_deck_cards WHERE deck_id = source_deck_id
  LOOP
    INSERT INTO player_deck_cards (deck_id, potato_id, quantity)
    VALUES (new_deck_id, deck_card.potato_id, deck_card.quantity);
  END LOOP;
  
  RETURN new_deck_id;
END;
$$;

-- Function to set active deck
CREATE OR REPLACE FUNCTION set_active_deck(user_uuid UUID, deck_slot_num INTEGER)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
  -- Validate deck slot
  IF deck_slot_num < 1 OR deck_slot_num > 10 THEN
    RAISE EXCEPTION 'Invalid deck slot. Must be 1-10.';
  END IF;
  
  -- Check if deck exists
  IF NOT EXISTS (SELECT 1 FROM player_decks WHERE user_id = user_uuid AND deck_slot = deck_slot_num) THEN
    RAISE EXCEPTION 'No deck found in slot %', deck_slot_num;
  END IF;
  
  -- Deactivate all user's decks
  UPDATE player_decks 
  SET is_active = FALSE, updated_at = NOW()
  WHERE user_id = user_uuid;
  
  -- Activate target deck
  UPDATE player_decks 
  SET is_active = TRUE, updated_at = NOW()
  WHERE user_id = user_uuid AND deck_slot = deck_slot_num;
  
  RETURN TRUE;
END;
$$;

-- Function to add card to deck
CREATE OR REPLACE FUNCTION add_card_to_deck(deck_uuid UUID, potato_uuid UUID, card_quantity INTEGER DEFAULT 1)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  current_quantity INTEGER := 0;
  total_cards INTEGER;
BEGIN
  -- Validate quantity
  IF card_quantity < 1 OR card_quantity > 3 THEN
    RAISE EXCEPTION 'Invalid card quantity. Must be 1-3.';
  END IF;
  
  -- Check current deck size
  SELECT get_deck_card_count(deck_uuid) INTO total_cards;
  
  IF total_cards + card_quantity > 30 THEN
    RAISE EXCEPTION 'Cannot add cards. Deck would exceed 30 cards.';
  END IF;
  
  -- Get current quantity of this card
  SELECT COALESCE(quantity, 0) INTO current_quantity
  FROM player_deck_cards
  WHERE deck_id = deck_uuid AND potato_id = potato_uuid;
  
  -- Check copy limit
  IF current_quantity + card_quantity > 3 THEN
    RAISE EXCEPTION 'Cannot add more copies. Maximum 3 copies per card.';
  END IF;
  
  -- Add or update card
  INSERT INTO player_deck_cards (deck_id, potato_id, quantity)
  VALUES (deck_uuid, potato_uuid, card_quantity)
  ON CONFLICT (deck_id, potato_id)
  DO UPDATE SET 
    quantity = player_deck_cards.quantity + card_quantity,
    deck_id = EXCLUDED.deck_id; -- Touch record to update timestamp
  
  RETURN TRUE;
END;
$$;

-- Function to remove card from deck
CREATE OR REPLACE FUNCTION remove_card_from_deck(deck_uuid UUID, potato_uuid UUID, card_quantity INTEGER DEFAULT 1)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  current_quantity INTEGER := 0;
BEGIN
  -- Get current quantity
  SELECT COALESCE(quantity, 0) INTO current_quantity
  FROM player_deck_cards
  WHERE deck_id = deck_uuid AND potato_id = potato_uuid;
  
  IF current_quantity = 0 THEN
    RAISE EXCEPTION 'Card not found in deck.';
  END IF;
  
  IF card_quantity >= current_quantity THEN
    -- Remove card completely
    DELETE FROM player_deck_cards
    WHERE deck_id = deck_uuid AND potato_id = potato_uuid;
  ELSE
    -- Reduce quantity
    UPDATE player_deck_cards
    SET quantity = quantity - card_quantity
    WHERE deck_id = deck_uuid AND potato_id = potato_uuid;
  END IF;
  
  RETURN TRUE;
END;
$$;

-- Create some starter deck templates
INSERT INTO deck_templates (name, description, is_starter) VALUES
('Basic Starter', 'A simple deck for new players with balanced cards', TRUE),
('Aggressive Rush', 'Fast-paced deck focused on quick victories', FALSE),
('Control Deck', 'Defensive deck that wins through patience', FALSE);

-- Grant permissions
GRANT EXECUTE ON FUNCTION validate_deck_size(UUID) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION get_deck_card_count(UUID) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION create_starter_deck(UUID) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION copy_deck_to_slot(UUID, INTEGER, VARCHAR) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION set_active_deck(UUID, INTEGER) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION add_card_to_deck(UUID, UUID, INTEGER) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION remove_card_from_deck(UUID, UUID, INTEGER) TO authenticated, service_role;

-- RLS Policies
ALTER TABLE player_decks ENABLE ROW LEVEL SECURITY;
ALTER TABLE player_deck_cards ENABLE ROW LEVEL SECURITY;
ALTER TABLE deck_templates ENABLE ROW LEVEL SECURITY;
ALTER TABLE deck_template_cards ENABLE ROW LEVEL SECURITY;

-- Players can manage their own decks
CREATE POLICY "Users can manage their own decks"
  ON player_decks FOR ALL
  USING (auth.uid() = user_id);

-- Players can manage their own deck cards
CREATE POLICY "Users can manage their own deck cards"
  ON player_deck_cards FOR ALL
  USING (
    EXISTS (
      SELECT 1 FROM player_decks 
      WHERE player_decks.id = player_deck_cards.deck_id 
      AND player_decks.user_id = auth.uid()
    )
  );

-- Everyone can read deck templates
CREATE POLICY "Everyone can read deck templates"
  ON deck_templates FOR SELECT
  USING (true);

CREATE POLICY "Everyone can read deck template cards"
  ON deck_template_cards FOR SELECT
  USING (true);

-- Service role has full access
CREATE POLICY "Service role full access to player_decks"
  ON player_decks FOR ALL
  TO service_role
  USING (true);

CREATE POLICY "Service role full access to player_deck_cards"
  ON player_deck_cards FOR ALL
  TO service_role
  USING (true);

CREATE POLICY "Service role full access to deck_templates"
  ON deck_templates FOR ALL
  TO service_role
  USING (true);

CREATE POLICY "Service role full access to deck_template_cards"
  ON deck_template_cards FOR ALL
  TO service_role
  USING (true);

-- Add helpful comments
COMMENT ON TABLE player_decks IS 'Player deck configurations supporting 30-card format';
COMMENT ON TABLE player_deck_cards IS 'Individual cards in player decks with quantities';
COMMENT ON TABLE deck_templates IS 'Predefined deck templates for easy deck creation';
COMMENT ON TABLE deck_template_cards IS 'Cards in deck templates';
COMMENT ON FUNCTION validate_deck_size(UUID) IS 'Checks if deck has exactly 30 cards';
COMMENT ON FUNCTION get_deck_card_count(UUID) IS 'Returns total number of cards in deck';
COMMENT ON FUNCTION create_starter_deck(UUID) IS 'Creates a 30-card starter deck for new users';
COMMENT ON FUNCTION set_active_deck(UUID, INTEGER) IS 'Sets which deck slot is active for battles';
COMMENT ON FUNCTION add_card_to_deck(UUID, UUID, INTEGER) IS 'Adds cards to deck with validation';
COMMENT ON FUNCTION remove_card_from_deck(UUID, UUID, INTEGER) IS 'Removes cards from deck';