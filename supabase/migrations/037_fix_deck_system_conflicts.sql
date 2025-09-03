-- Fix conflicts and ensure proper deck system setup
-- This migration handles existing objects gracefully

-- 1. Ensure potato_registry has all required columns
ALTER TABLE potato_registry 
ADD COLUMN IF NOT EXISTS mana_cost INTEGER DEFAULT 1,
ADD COLUMN IF NOT EXISTS attack INTEGER DEFAULT 1,
ADD COLUMN IF NOT EXISTS hp INTEGER DEFAULT 1,
ADD COLUMN IF NOT EXISTS card_type TEXT DEFAULT 'unit',
ADD COLUMN IF NOT EXISTS keywords TEXT[] DEFAULT '{}',
ADD COLUMN IF NOT EXISTS is_exotic BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS is_legendary BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS format_legal BOOLEAN DEFAULT true;

-- 2. Update existing cards with generated stats (only if they don't have them)
UPDATE potato_registry 
SET 
  mana_cost = CASE 
    WHEN mana_cost IS NULL OR mana_cost = 1 THEN
      CASE 
        WHEN rarity = 'common' THEN 1 + (RANDOM() * 3)::INTEGER
        WHEN rarity = 'rare' THEN 2 + (RANDOM() * 4)::INTEGER  
        WHEN rarity = 'epic' THEN 4 + (RANDOM() * 3)::INTEGER
        WHEN rarity = 'legendary' THEN 6 + (RANDOM() * 2)::INTEGER
        ELSE 1
      END
    ELSE mana_cost
  END,
  attack = CASE 
    WHEN attack IS NULL OR attack = 1 THEN
      CASE 
        WHEN rarity = 'common' THEN 1 + (RANDOM() * 2)::INTEGER
        WHEN rarity = 'rare' THEN 2 + (RANDOM() * 3)::INTEGER
        WHEN rarity = 'epic' THEN 3 + (RANDOM() * 3)::INTEGER  
        WHEN rarity = 'legendary' THEN 4 + (RANDOM() * 4)::INTEGER
        ELSE 1
      END
    ELSE attack
  END,
  hp = CASE 
    WHEN hp IS NULL OR hp = 1 THEN
      CASE 
        WHEN rarity = 'common' THEN 1 + (RANDOM() * 2)::INTEGER
        WHEN rarity = 'rare' THEN 2 + (RANDOM() * 3)::INTEGER
        WHEN rarity = 'epic' THEN 3 + (RANDOM() * 3)::INTEGER
        WHEN rarity = 'legendary' THEN 4 + (RANDOM() * 4)::INTEGER
        ELSE 1
      END
    ELSE hp
  END,
  is_legendary = CASE 
    WHEN is_legendary IS NULL THEN (rarity = 'legendary')
    ELSE is_legendary
  END,
  is_exotic = CASE 
    WHEN is_exotic IS NULL THEN (rarity = 'legendary' OR (RANDOM() < 0.1 AND rarity = 'epic'))
    ELSE is_exotic
  END,
  card_type = CASE 
    WHEN card_type = 'unit' OR card_type IS NULL THEN
      CASE 
        WHEN trait ILIKE '%spell%' OR trait ILIKE '%magic%' THEN 'spell'
        WHEN trait ILIKE '%structure%' OR trait ILIKE '%building%' THEN 'structure'
        ELSE 'unit'
      END
    ELSE card_type
  END,
  keywords = CASE 
    WHEN keywords = '{}' OR keywords IS NULL THEN
      CASE 
        WHEN trait ILIKE '%protect%' OR trait ILIKE '%guard%' THEN ARRAY['Taunt']
        WHEN trait ILIKE '%quick%' OR trait ILIKE '%fast%' THEN ARRAY['Charge']
        WHEN trait ILIKE '%heal%' OR trait ILIKE '%life%' THEN ARRAY['Lifesteal']
        ELSE ARRAY[]::TEXT[]
      END
    ELSE keywords
  END;

-- 3. Create tables if they don't exist
CREATE TABLE IF NOT EXISTS collections (
  id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
  user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
  card_id UUID REFERENCES potato_registry(id) ON DELETE CASCADE,
  quantity INTEGER DEFAULT 1 CHECK (quantity >= 0),
  acquired_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  source TEXT DEFAULT 'starter',
  UNIQUE(user_id, card_id)
);

CREATE TABLE IF NOT EXISTS decks (
  id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
  user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
  name TEXT NOT NULL,
  hero_id UUID NULL,
  format TEXT DEFAULT 'standard',
  is_active BOOLEAN DEFAULT false,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  CONSTRAINT deck_name_length CHECK (length(name) >= 1 AND length(name) <= 50)
);

CREATE TABLE IF NOT EXISTS deck_cards (
  id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
  deck_id UUID REFERENCES decks(id) ON DELETE CASCADE,
  card_id UUID REFERENCES potato_registry(id) ON DELETE CASCADE,
  quantity INTEGER DEFAULT 1 CHECK (quantity >= 1 AND quantity <= 2),
  added_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  UNIQUE(deck_id, card_id)
);

-- 4. Enable RLS if not already enabled
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'collections' AND rowsecurity = true) THEN
    ALTER TABLE collections ENABLE ROW LEVEL SECURITY;
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'decks' AND rowsecurity = true) THEN
    ALTER TABLE decks ENABLE ROW LEVEL SECURITY;
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'deck_cards' AND rowsecurity = true) THEN
    ALTER TABLE deck_cards ENABLE ROW LEVEL SECURITY;
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

-- 5. Create policies only if they don't exist
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'collections' AND policyname = 'Users can view their own collections') THEN
    CREATE POLICY "Users can view their own collections" ON collections FOR SELECT USING (auth.uid() = user_id);
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'collections' AND policyname = 'Users can manage their own collections') THEN
    CREATE POLICY "Users can manage their own collections" ON collections FOR ALL USING (auth.uid() = user_id);
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'decks' AND policyname = 'Users can view their own decks') THEN
    CREATE POLICY "Users can view their own decks" ON decks FOR SELECT USING (auth.uid() = user_id);
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'decks' AND policyname = 'Users can manage their own decks') THEN
    CREATE POLICY "Users can manage their own decks" ON decks FOR ALL USING (auth.uid() = user_id);
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'deck_cards' AND policyname = 'Users can view their deck cards') THEN
    CREATE POLICY "Users can view their deck cards" ON deck_cards FOR SELECT USING (
      EXISTS (SELECT 1 FROM decks WHERE decks.id = deck_cards.deck_id AND decks.user_id = auth.uid())
    );
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = 'deck_cards' AND policyname = 'Users can manage their deck cards') THEN
    CREATE POLICY "Users can manage their deck cards" ON deck_cards FOR ALL USING (
      EXISTS (SELECT 1 FROM decks WHERE decks.id = deck_cards.deck_id AND decks.user_id = auth.uid())
    );
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

-- 6. Create or replace functions (always safe to replace)
CREATE OR REPLACE FUNCTION create_starter_collection_for_user(user_uuid UUID)
RETURNS VOID AS $$
DECLARE
  card_record RECORD;
BEGIN
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

CREATE OR REPLACE FUNCTION create_starter_deck_for_user(user_uuid UUID)
RETURNS UUID AS $$
DECLARE
  new_deck_id UUID;
  card_record RECORD;
  cards_added INTEGER := 0;
BEGIN
  -- Create starter collection first
  PERFORM create_starter_collection_for_user(user_uuid);

  -- Check if user already has a starter deck
  SELECT id INTO new_deck_id FROM decks 
  WHERE user_id = user_uuid AND name = 'Starter Deck';
  
  IF new_deck_id IS NOT NULL THEN
    RETURN new_deck_id;
  END IF;

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
    VALUES (new_deck_id, card_record.id, 2)
    ON CONFLICT (deck_id, card_id) DO NOTHING;
    
    cards_added := cards_added + 2;
    EXIT WHEN cards_added >= 30;
  END LOOP;

  RETURN new_deck_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 7. Create indexes if they don't exist
CREATE INDEX IF NOT EXISTS idx_collections_user_id ON collections(user_id);
CREATE INDEX IF NOT EXISTS idx_collections_card_id ON collections(card_id);
CREATE INDEX IF NOT EXISTS idx_decks_user_id ON decks(user_id);
CREATE INDEX IF NOT EXISTS idx_decks_is_active ON decks(user_id, is_active);
CREATE INDEX IF NOT EXISTS idx_deck_cards_deck_id ON deck_cards(deck_id);
CREATE INDEX IF NOT EXISTS idx_deck_cards_card_id ON deck_cards(card_id);
CREATE INDEX IF NOT EXISTS idx_potato_registry_rarity ON potato_registry(rarity);
CREATE INDEX IF NOT EXISTS idx_potato_registry_mana_cost ON potato_registry(mana_cost);
CREATE INDEX IF NOT EXISTS idx_potato_registry_card_type ON potato_registry(card_type);

-- 8. Set up triggers
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Drop and recreate trigger (safe way)
DROP TRIGGER IF EXISTS set_decks_updated_at ON decks;
CREATE TRIGGER set_decks_updated_at
  BEFORE UPDATE ON decks
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- 9. Grant necessary permissions
GRANT USAGE ON SCHEMA public TO anon, authenticated;
GRANT ALL ON TABLE collections TO authenticated;
GRANT ALL ON TABLE decks TO authenticated;
GRANT ALL ON TABLE deck_cards TO authenticated;

-- 10. Success message
DO $$ BEGIN
  RAISE NOTICE 'Deck system migration completed successfully! You can now use the new deck builder.';
END $$;