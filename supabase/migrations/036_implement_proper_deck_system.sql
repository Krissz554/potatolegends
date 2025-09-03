-- Proper Deck Builder System Migration
-- Implements exactly 30-card decks with 2x copy limits (1x for Exotic/Legendary)

-- 1. Update potato_registry to include proper card metadata
ALTER TABLE potato_registry 
ADD COLUMN IF NOT EXISTS mana_cost INTEGER DEFAULT 1,
ADD COLUMN IF NOT EXISTS attack INTEGER DEFAULT 1,
ADD COLUMN IF NOT EXISTS hp INTEGER DEFAULT 1,
ADD COLUMN IF NOT EXISTS card_type TEXT DEFAULT 'unit',
ADD COLUMN IF NOT EXISTS keywords TEXT[] DEFAULT '{}',
ADD COLUMN IF NOT EXISTS is_exotic BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS is_legendary BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS format_legal BOOLEAN DEFAULT true;

-- Update existing cards with generated stats based on their data
UPDATE potato_registry 
SET 
  mana_cost = CASE 
    WHEN rarity = 'common' THEN 1 + (RANDOM() * 3)::INTEGER
    WHEN rarity = 'rare' THEN 2 + (RANDOM() * 4)::INTEGER  
    WHEN rarity = 'epic' THEN 4 + (RANDOM() * 3)::INTEGER
    WHEN rarity = 'legendary' THEN 6 + (RANDOM() * 2)::INTEGER
    ELSE 1
  END,
  attack = CASE 
    WHEN rarity = 'common' THEN 1 + (RANDOM() * 2)::INTEGER
    WHEN rarity = 'rare' THEN 2 + (RANDOM() * 3)::INTEGER
    WHEN rarity = 'epic' THEN 3 + (RANDOM() * 3)::INTEGER  
    WHEN rarity = 'legendary' THEN 4 + (RANDOM() * 4)::INTEGER
    ELSE 1
  END,
  hp = CASE 
    WHEN rarity = 'common' THEN 1 + (RANDOM() * 2)::INTEGER
    WHEN rarity = 'rare' THEN 2 + (RANDOM() * 3)::INTEGER
    WHEN rarity = 'epic' THEN 3 + (RANDOM() * 3)::INTEGER
    WHEN rarity = 'legendary' THEN 4 + (RANDOM() * 4)::INTEGER
    ELSE 1
  END,
  is_legendary = (rarity = 'legendary'),
  is_exotic = (rarity = 'legendary' OR (RANDOM() < 0.1 AND rarity = 'epic')),
  card_type = CASE 
    WHEN trait ILIKE '%spell%' OR trait ILIKE '%magic%' THEN 'spell'
    WHEN trait ILIKE '%structure%' OR trait ILIKE '%building%' THEN 'structure'
    ELSE 'unit'
  END,
  keywords = CASE 
    WHEN trait ILIKE '%protect%' OR trait ILIKE '%guard%' THEN ARRAY['Taunt']
    WHEN trait ILIKE '%quick%' OR trait ILIKE '%fast%' THEN ARRAY['Charge']
    WHEN trait ILIKE '%heal%' OR trait ILIKE '%life%' THEN ARRAY['Lifesteal']
    ELSE ARRAY[]::TEXT[]
  END;

-- 2. Create collections table (what cards each player owns)
CREATE TABLE IF NOT EXISTS collections (
  id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
  user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
  card_id UUID REFERENCES potato_registry(id) ON DELETE CASCADE,
  quantity INTEGER DEFAULT 1 CHECK (quantity >= 0),
  acquired_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  source TEXT DEFAULT 'starter', -- 'starter', 'pack', 'reward', etc.
  UNIQUE(user_id, card_id)
);

-- 3. Create decks table (player deck metadata)
CREATE TABLE IF NOT EXISTS decks (
  id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
  user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
  name TEXT NOT NULL,
  hero_id UUID NULL, -- For future hero system
  format TEXT DEFAULT 'standard',
  is_active BOOLEAN DEFAULT false,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  CONSTRAINT deck_name_length CHECK (length(name) >= 1 AND length(name) <= 50)
);

-- 4. Create deck_cards table (deck contents)
CREATE TABLE IF NOT EXISTS deck_cards (
  id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
  deck_id UUID REFERENCES decks(id) ON DELETE CASCADE,
  card_id UUID REFERENCES potato_registry(id) ON DELETE CASCADE,
  quantity INTEGER DEFAULT 1 CHECK (quantity >= 1 AND quantity <= 2),
  added_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  UNIQUE(deck_id, card_id)
);

-- 5. RLS Policies for security
ALTER TABLE collections ENABLE ROW LEVEL SECURITY;
ALTER TABLE decks ENABLE ROW LEVEL SECURITY;
ALTER TABLE deck_cards ENABLE ROW LEVEL SECURITY;

-- Collections policies
DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_policies 
    WHERE tablename = 'collections' 
    AND policyname = 'Users can view their own collections'
  ) THEN
    CREATE POLICY "Users can view their own collections" ON collections
      FOR SELECT USING (auth.uid() = user_id);
  END IF;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_policies 
    WHERE tablename = 'collections' 
    AND policyname = 'Users can manage their own collections'
  ) THEN
    CREATE POLICY "Users can manage their own collections" ON collections
      FOR ALL USING (auth.uid() = user_id);
  END IF;
END $$;

-- Decks policies  
DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_policies 
    WHERE tablename = 'decks' 
    AND policyname = 'Users can view their own decks'
  ) THEN
    CREATE POLICY "Users can view their own decks" ON decks
      FOR SELECT USING (auth.uid() = user_id);
  END IF;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_policies 
    WHERE tablename = 'decks' 
    AND policyname = 'Users can manage their own decks'
  ) THEN
    CREATE POLICY "Users can manage their own decks" ON decks
      FOR ALL USING (auth.uid() = user_id);
  END IF;
END $$;

-- Deck cards policies
DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_policies 
    WHERE tablename = 'deck_cards' 
    AND policyname = 'Users can view their deck cards'
  ) THEN
    CREATE POLICY "Users can view their deck cards" ON deck_cards
      FOR SELECT USING (
        EXISTS (
          SELECT 1 FROM decks 
          WHERE decks.id = deck_cards.deck_id 
          AND decks.user_id = auth.uid()
        )
      );
  END IF;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_policies 
    WHERE tablename = 'deck_cards' 
    AND policyname = 'Users can manage their deck cards'
  ) THEN
    CREATE POLICY "Users can manage their deck cards" ON deck_cards
      FOR ALL USING (
        EXISTS (
          SELECT 1 FROM decks 
          WHERE decks.id = deck_cards.deck_id 
          AND decks.user_id = auth.uid()
        )
      );
  END IF;
END $$;

-- 6. Create starter collection for all existing users
CREATE OR REPLACE FUNCTION create_starter_collection_for_user(user_uuid UUID)
RETURNS VOID AS $$
DECLARE
  card_record RECORD;
BEGIN
  -- Give each user 3 copies of all common cards, 2 copies of rare cards, 1 copy of epic/legendary
  FOR card_record IN 
    SELECT id, rarity FROM potato_registry 
    WHERE format_legal = true
  LOOP
    INSERT INTO collections (user_id, card_id, quantity, source)
    VALUES (
      user_uuid, 
      card_record.id,
      CASE 
        WHEN card_record.rarity = 'common' THEN 3
        WHEN card_record.rarity = 'rare' THEN 2  
        ELSE 1
      END,
      'starter'
    )
    ON CONFLICT (user_id, card_id) DO NOTHING;
  END LOOP;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 7. RPC Functions for deck operations

-- Add card to deck with validation
CREATE OR REPLACE FUNCTION add_card_to_deck_v2(
  deck_uuid UUID,
  card_uuid UUID,
  add_quantity INTEGER DEFAULT 1
)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  current_deck_size INTEGER;
  current_card_quantity INTEGER;
  card_rarity TEXT;
  card_is_legendary BOOLEAN;
  card_is_exotic BOOLEAN;
  owned_quantity INTEGER;
  max_copies INTEGER;
BEGIN
  -- Verify deck ownership
  SELECT user_id INTO deck_owner FROM decks WHERE id = deck_uuid;
  IF deck_owner != auth.uid() THEN
    RETURN json_build_object('success', false, 'error', 'Deck not found or access denied');
  END IF;

  -- Get card info
  SELECT rarity, is_legendary, is_exotic INTO card_rarity, card_is_legendary, card_is_exotic
  FROM potato_registry WHERE id = card_uuid;

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

  -- Determine max copies allowed
  IF card_is_legendary OR card_is_exotic THEN
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
      RETURN json_build_object('success', false, 'error', 'Only 1 copy allowed for Exotic/Legendary cards');
    ELSE
      RETURN json_build_object('success', false, 'error', 'Maximum 2 copies per card allowed');
    END IF;
  END IF;

  -- Add or update card in deck
  INSERT INTO deck_cards (deck_id, card_id, quantity)
  VALUES (deck_uuid, card_uuid, add_quantity)
  ON CONFLICT (deck_id, card_id) 
  DO UPDATE SET 
    quantity = deck_cards.quantity + add_quantity,
    added_at = NOW();

  -- Update deck timestamp
  UPDATE decks SET updated_at = NOW() WHERE id = deck_uuid;

  RETURN json_build_object('success', true, 'message', 'Card added successfully');
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Remove card from deck
CREATE OR REPLACE FUNCTION remove_card_from_deck_v2(
  deck_uuid UUID,
  card_uuid UUID,
  remove_quantity INTEGER DEFAULT 1
)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  current_quantity INTEGER;
BEGIN
  -- Verify deck ownership
  SELECT user_id INTO deck_owner FROM decks WHERE id = deck_uuid;
  IF deck_owner != auth.uid() THEN
    RETURN json_build_object('success', false, 'error', 'Deck not found or access denied');
  END IF;

  -- Get current quantity
  SELECT quantity INTO current_quantity
  FROM deck_cards WHERE deck_id = deck_uuid AND card_id = card_uuid;

  IF current_quantity IS NULL OR current_quantity = 0 THEN
    RETURN json_build_object('success', false, 'error', 'Card not in deck');
  END IF;

  IF current_quantity <= remove_quantity THEN
    -- Remove card entirely
    DELETE FROM deck_cards WHERE deck_id = deck_uuid AND card_id = card_uuid;
  ELSE
    -- Reduce quantity
    UPDATE deck_cards 
    SET quantity = quantity - remove_quantity
    WHERE deck_id = deck_uuid AND card_id = card_uuid;
  END IF;

  -- Update deck timestamp
  UPDATE decks SET updated_at = NOW() WHERE id = deck_uuid;

  RETURN json_build_object('success', true, 'message', 'Card removed successfully');
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Validate deck completeness
CREATE OR REPLACE FUNCTION validate_deck(deck_uuid UUID)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  total_cards INTEGER;
  validation_errors TEXT[] := ARRAY[]::TEXT[];
  card_violations TEXT;
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
    validation_errors := array_append(validation_errors, 
      'Deck must have exactly 30 cards (currently has ' || total_cards || ')');
  END IF;

  -- Check for copy limit violations
  SELECT string_agg(violation, ', ') INTO card_violations FROM (
    SELECT 
      CASE 
        WHEN pr.is_legendary OR pr.is_exotic THEN
          pr.name || ' (max 1 copy, has ' || dc.quantity || ')'
        ELSE
          pr.name || ' (max 2 copies, has ' || dc.quantity || ')'
      END as violation
    FROM deck_cards dc
    JOIN potato_registry pr ON dc.card_id = pr.id
    WHERE dc.deck_id = deck_uuid 
    AND (
      (pr.is_legendary OR pr.is_exotic) AND dc.quantity > 1
      OR (NOT pr.is_legendary AND NOT pr.is_exotic) AND dc.quantity > 2
    )
  ) violations;

  IF card_violations IS NOT NULL THEN
    validation_errors := array_append(validation_errors, 'Copy limit violations: ' || card_violations);
  END IF;

  -- Return validation result
  RETURN json_build_object(
    'valid', array_length(validation_errors, 1) IS NULL,
    'errors', COALESCE(validation_errors, ARRAY[]::TEXT[]),
    'total_cards', total_cards
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create default starter deck for new users
CREATE OR REPLACE FUNCTION create_starter_deck_for_user(user_uuid UUID)
RETURNS UUID AS $$
DECLARE
  new_deck_id UUID;
  card_record RECORD;
  cards_added INTEGER := 0;
BEGIN
  -- Create starter collection first
  PERFORM create_starter_collection_for_user(user_uuid);

  -- Create starter deck
  INSERT INTO decks (user_id, name, is_active)
  VALUES (user_uuid, 'Starter Deck', true)
  RETURNING id INTO new_deck_id;

  -- Add 30 common cards to make a legal deck
  FOR card_record IN 
    SELECT pr.id, pr.name 
    FROM potato_registry pr
    JOIN collections c ON pr.id = c.card_id
    WHERE c.user_id = user_uuid 
    AND pr.rarity = 'common'
    AND pr.format_legal = true
    ORDER BY RANDOM()
    LIMIT 15 -- 15 different cards, 2 copies each = 30 total
  LOOP
    INSERT INTO deck_cards (deck_id, card_id, quantity)
    VALUES (new_deck_id, card_record.id, 2);
    
    cards_added := cards_added + 2;
    EXIT WHEN cards_added >= 30;
  END LOOP;

  RETURN new_deck_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 8. Create starter collections for all existing users
DO $$
DECLARE
  user_record RECORD;
BEGIN
  -- Only run if auth.users table exists and has users
  IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'users' AND table_schema = 'auth') THEN
    FOR user_record IN SELECT id FROM auth.users LOOP
      BEGIN
        PERFORM create_starter_collection_for_user(user_record.id);
      EXCEPTION
        WHEN OTHERS THEN
          -- Log the error but continue with other users
          RAISE NOTICE 'Failed to create starter collection for user %: %', user_record.id, SQLERRM;
      END;
    END LOOP;
  END IF;
EXCEPTION
  WHEN undefined_table THEN
    -- auth.users table doesn't exist, skip this step
    RAISE NOTICE 'auth.users table not found, skipping starter collection creation';
END $$;

-- 9. Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_collections_user_id ON collections(user_id);
CREATE INDEX IF NOT EXISTS idx_collections_card_id ON collections(card_id);
CREATE INDEX IF NOT EXISTS idx_decks_user_id ON decks(user_id);
CREATE INDEX IF NOT EXISTS idx_decks_is_active ON decks(user_id, is_active);
CREATE INDEX IF NOT EXISTS idx_deck_cards_deck_id ON deck_cards(deck_id);
CREATE INDEX IF NOT EXISTS idx_deck_cards_card_id ON deck_cards(card_id);
CREATE INDEX IF NOT EXISTS idx_potato_registry_rarity ON potato_registry(rarity);
CREATE INDEX IF NOT EXISTS idx_potato_registry_mana_cost ON potato_registry(mana_cost);
CREATE INDEX IF NOT EXISTS idx_potato_registry_card_type ON potato_registry(card_type);

-- 10. Triggers for automatic starter collection/deck creation
CREATE OR REPLACE FUNCTION handle_new_user_signup()
RETURNS TRIGGER AS $$
BEGIN
  -- Create starter collection and deck for new user
  PERFORM create_starter_collection_for_user(NEW.id);
  PERFORM create_starter_deck_for_user(NEW.id);
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create trigger for new user signups (if it doesn't exist)
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
DO $$ BEGIN
  -- Only create trigger if auth.users table exists (might not in local dev)
  IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'users' AND table_schema = 'auth') THEN
    CREATE TRIGGER on_auth_user_created
      AFTER INSERT ON auth.users
      FOR EACH ROW EXECUTE FUNCTION handle_new_user_signup();
  END IF;
EXCEPTION
  WHEN undefined_table THEN
    -- auth.users table doesn't exist, skip trigger creation
    NULL;
END $$;

-- Set updated_at trigger for decks
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS set_decks_updated_at ON decks;
CREATE TRIGGER set_decks_updated_at
  BEFORE UPDATE ON decks
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();