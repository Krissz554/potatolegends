-- Complete Migration to New Card System Only
-- This migration enhances the card data model and removes old systems

-- 1. Enhanced potato_registry with complete card data model
ALTER TABLE potato_registry 
-- Core Identifiers (some already exist)
ADD COLUMN IF NOT EXISTS set_id TEXT DEFAULT 'core',
ADD COLUMN IF NOT EXISTS format_legalities TEXT[] DEFAULT ARRAY['standard'],

-- Gameplay Stats (some already exist)
-- mana_cost, attack, hp already added in previous migrations
-- card_type already exists
-- rarity already exists
-- is_legendary, is_exotic already exist

-- Abilities & Keywords (some already exist)
-- keywords already exists
ADD COLUMN IF NOT EXISTS ability_text TEXT DEFAULT '',
ADD COLUMN IF NOT EXISTS passive_effects JSONB DEFAULT '{}',
ADD COLUMN IF NOT EXISTS triggered_effects JSONB DEFAULT '{}',

-- Metadata / Cosmetic
ADD COLUMN IF NOT EXISTS illustration_url TEXT DEFAULT '',
ADD COLUMN IF NOT EXISTS frame_style TEXT DEFAULT 'standard',
ADD COLUMN IF NOT EXISTS flavor_text TEXT DEFAULT '',

-- Balance / Dev Data  
-- exotic already exists as is_exotic
ADD COLUMN IF NOT EXISTS release_date DATE DEFAULT CURRENT_DATE,
ADD COLUMN IF NOT EXISTS tags TEXT[] DEFAULT ARRAY['core'],

-- Optional / Advanced (future-proofing)
ADD COLUMN IF NOT EXISTS alternate_skins JSONB DEFAULT '[]',
ADD COLUMN IF NOT EXISTS voice_line_url TEXT DEFAULT '',
ADD COLUMN IF NOT EXISTS craft_cost INTEGER DEFAULT 0,
ADD COLUMN IF NOT EXISTS dust_value INTEGER DEFAULT 0,
ADD COLUMN IF NOT EXISTS level_up_conditions JSONB DEFAULT '{}',
ADD COLUMN IF NOT EXISTS token_spawns TEXT[] DEFAULT ARRAY[]::TEXT[];

-- 2. Update existing cards with enhanced data (preserving all existing data)
UPDATE potato_registry SET
  -- Set metadata (only if not already set)
  set_id = COALESCE(set_id, 'core'),
  format_legalities = COALESCE(format_legalities, ARRAY['standard']),
  
  -- Set frame and flavor text (preserve existing description)
  frame_style = COALESCE(frame_style, 'standard'),
  flavor_text = COALESCE(NULLIF(flavor_text, ''), description, 'A mighty potato ready for battle!'),
  
  -- Set abilities and keywords (preserve existing or set default)
  ability_text = COALESCE(ability_text, ''),
  keywords = CASE 
    WHEN keywords IS NULL OR keywords = '{}' OR array_length(keywords, 1) IS NULL THEN ARRAY['Charge']
    ELSE keywords
  END,
  
  -- Set tags and release info (preserve existing)
  tags = COALESCE(tags, ARRAY['core', 'starter']),
  release_date = COALESCE(release_date, CURRENT_DATE),
  
  -- Set craft costs based on rarity (only if not set)
  craft_cost = COALESCE(craft_cost, CASE
    WHEN rarity = 'common' THEN 40
    WHEN rarity = 'rare' THEN 100  
    WHEN rarity = 'epic' THEN 400
    WHEN rarity = 'legendary' THEN 1600
    ELSE 40
  END),
  
  dust_value = COALESCE(dust_value, CASE
    WHEN rarity = 'common' THEN 5
    WHEN rarity = 'rare' THEN 20
    WHEN rarity = 'epic' THEN 100  
    WHEN rarity = 'legendary' THEN 400
    ELSE 5
  END),
  
  -- Set illustration URL (preserve existing or use pixel art reference)
  illustration_url = COALESCE(
    NULLIF(illustration_url, ''),
    'https://example.com/potato-art/' || registry_id || '.png'
  )
WHERE 
  -- Only update rows that need updating (avoid unnecessary changes)
  set_id IS NULL OR 
  format_legalities IS NULL OR 
  frame_style IS NULL OR 
  flavor_text IS NULL OR flavor_text = '' OR
  ability_text IS NULL OR
  (keywords IS NULL OR keywords = '{}' OR array_length(keywords, 1) IS NULL) OR
  tags IS NULL OR
  release_date IS NULL OR
  craft_cost IS NULL OR craft_cost = 0 OR
  dust_value IS NULL OR dust_value = 0 OR
  illustration_url IS NULL OR illustration_url = '';

-- 2.1. Fix legendary and exotic flags based on rarity
UPDATE potato_registry SET
  is_legendary = CASE 
    WHEN rarity = 'legendary' THEN true
    ELSE COALESCE(is_legendary, false)
  END,
  is_exotic = CASE 
    WHEN rarity = 'legendary' THEN true  -- Legendary cards are also exotic (1 copy max)
    WHEN rarity = 'epic' AND (RANDOM() < 0.2) THEN true  -- 20% of epic cards are exotic
    ELSE COALESCE(is_exotic, false)
  END
WHERE 
  -- Update legendary cards or cards that don't have proper flags set
  (rarity = 'legendary' AND is_legendary != true) OR
  (rarity = 'legendary' AND is_exotic != true) OR
  (is_legendary IS NULL) OR
  (is_exotic IS NULL);

-- 3. Create view for complete card data (preserving ALL original data)
CREATE OR REPLACE VIEW card_complete AS
SELECT 
  -- Original core fields (PRESERVED)
  id,
  registry_id,
  name,
  description,
  potato_type,
  trait,
  adjective,
  rarity,
  palette_name,
  pixel_art_data,
  generation_seed,
  variation_index,
  sort_order,
  created_at,
  
  -- Original gameplay stats (PRESERVED - from previous migrations)
  COALESCE(mana_cost, 1) as mana_cost,
  COALESCE(attack, 1) as attack,
  COALESCE(hp, 1) as hp,
  COALESCE(card_type, 'unit') as card_type,
  COALESCE(is_legendary, false) as is_legendary,
  COALESCE(is_exotic, false) as exotic,
  
  -- Enhanced identifiers
  COALESCE(set_id, 'core') as set_id,
  COALESCE(format_legalities, ARRAY['standard']) as format_legalities,
  
  -- Enhanced abilities & keywords
  COALESCE(keywords, ARRAY['Charge']) as keywords,
  COALESCE(ability_text, '') as ability_text,
  COALESCE(passive_effects, '{}') as passive_effects,
  COALESCE(triggered_effects, '{}') as triggered_effects,
  
  -- Enhanced metadata
  COALESCE(illustration_url, '') as illustration_url,
  COALESCE(frame_style, 'standard') as frame_style,
  COALESCE(flavor_text, description) as flavor_text,
  
  -- Enhanced balance data
  COALESCE(release_date, CURRENT_DATE) as release_date,
  COALESCE(tags, ARRAY['core']) as tags,
  COALESCE(craft_cost, 40) as craft_cost,
  COALESCE(dust_value, 5) as dust_value,
  
  -- Advanced features
  COALESCE(alternate_skins, '[]') as alternate_skins,
  COALESCE(voice_line_url, '') as voice_line_url,
  COALESCE(level_up_conditions, '{}') as level_up_conditions,
  COALESCE(token_spawns, ARRAY[]::TEXT[]) as token_spawns
  
FROM potato_registry
WHERE COALESCE(format_legal, true) = true;

-- 4. Enhanced collection management functions

-- Function to unlock all cards for a user (for testing)
CREATE OR REPLACE FUNCTION unlock_all_cards_for_user(user_uuid UUID)
RETURNS JSON AS $$
DECLARE
  card_record RECORD;
  cards_unlocked INTEGER := 0;
BEGIN
  -- Clear existing collection first
  DELETE FROM collections WHERE user_id = user_uuid;
  
  -- Add all cards to collection
  FOR card_record IN 
    SELECT id, rarity FROM potato_registry 
    WHERE format_legal = true
  LOOP
    INSERT INTO collections (user_id, card_id, quantity, source)
    VALUES (
      user_uuid, 
      card_record.id,
      CASE 
        WHEN card_record.rarity = 'legendary' THEN 1 -- Max 1 legendary
        ELSE 3 -- Max 3 of others for deck building
      END,
      'unlock_all'
    );
    
    cards_unlocked := cards_unlocked + 1;
  END LOOP;
  
  RETURN json_build_object(
    'success', true,
    'cards_unlocked', cards_unlocked,
    'message', 'All cards unlocked successfully'
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Drop existing function if it exists to allow return type change
DROP FUNCTION IF EXISTS get_user_collection(UUID);

-- Function to get user's complete collection
CREATE FUNCTION get_user_collection(user_uuid UUID)
RETURNS JSON AS $$
BEGIN
  RETURN (
    SELECT COALESCE(
      json_agg(
        json_build_object(
          'card', json_build_object(
            'id', c.id,
            'registry_id', c.registry_id,
            'name', c.name,
            'description', c.description,
            'potato_type', c.potato_type,
            'trait', c.trait,
            'adjective', c.adjective,
            'rarity', c.rarity,
            'palette_name', c.palette_name,
            'pixel_art_data', c.pixel_art_data,
            'mana_cost', c.mana_cost,
            'attack', c.attack,
            'hp', c.hp,
            'card_type', c.card_type,
            'is_legendary', c.is_legendary,
            'exotic', c.exotic,
            'set_id', c.set_id,
            'format_legalities', c.format_legalities,
            'keywords', c.keywords,
            'ability_text', c.ability_text,
            'flavor_text', c.flavor_text,
            'craft_cost', c.craft_cost,
            'dust_value', c.dust_value
          ),
          'quantity', col.quantity,
          'acquired_at', col.acquired_at,
          'source', col.source
        )
      ),
      '[]'::json
    )
    FROM collections col
    JOIN card_complete c ON col.card_id = c.id
    WHERE col.user_id = user_uuid
    ORDER BY 
      CASE c.rarity 
        WHEN 'legendary' THEN 1
        WHEN 'epic' THEN 2
        WHEN 'rare' THEN 3
        WHEN 'common' THEN 4
        ELSE 5
      END,
      c.name
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 5. Drop old tables (after confirming migration)
-- We'll comment these out for safety - uncomment after confirming everything works
-- DROP TABLE IF EXISTS potato_unlocks CASCADE;
-- DROP TABLE IF EXISTS potato_collections CASCADE;
-- DROP TABLE IF EXISTS player_decks CASCADE;
-- DROP TABLE IF EXISTS player_deck_cards CASCADE;

-- 6. Update RPC functions to use new system only

-- Enhanced deck functions - drop existing to avoid conflicts
DROP FUNCTION IF EXISTS add_card_to_deck_v3(UUID, UUID, INTEGER);
DROP FUNCTION IF EXISTS remove_card_from_deck_v3(UUID, UUID, INTEGER);

-- Enhanced add card to deck with new validation
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

  -- Add or update card in deck
  INSERT INTO deck_cards (deck_id, card_id, quantity)
  VALUES (deck_uuid, card_uuid, add_quantity)
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

-- Enhanced remove card from deck
CREATE FUNCTION remove_card_from_deck_v3(
  deck_uuid UUID,
  card_uuid UUID,
  remove_quantity INTEGER DEFAULT 1
)
RETURNS JSON AS $$
DECLARE
  deck_owner UUID;
  current_quantity INTEGER;
  card_data RECORD;
BEGIN
  -- Verify deck ownership
  SELECT user_id INTO deck_owner FROM decks WHERE id = deck_uuid;
  IF deck_owner != auth.uid() THEN
    RETURN json_build_object('success', false, 'error', 'Deck not found or access denied');
  END IF;

  -- Get card data
  SELECT * INTO card_data FROM card_complete WHERE id = card_uuid;
  IF NOT FOUND THEN
    RETURN json_build_object('success', false, 'error', 'Card not found');
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

  RETURN json_build_object(
    'success', true, 
    'message', 'Removed ' || card_data.name || ' from deck',
    'card_name', card_data.name
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 7. Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_potato_registry_set_id ON potato_registry(set_id);
CREATE INDEX IF NOT EXISTS idx_potato_registry_tags ON potato_registry USING GIN(tags);
CREATE INDEX IF NOT EXISTS idx_potato_registry_format_legalities ON potato_registry USING GIN(format_legalities);
CREATE INDEX IF NOT EXISTS idx_potato_registry_rarity_mana ON potato_registry(rarity, mana_cost);
CREATE INDEX IF NOT EXISTS idx_collections_user_card ON collections(user_id, card_id);

-- 8. Grant permissions
GRANT SELECT ON card_complete TO anon, authenticated;
GRANT EXECUTE ON FUNCTION unlock_all_cards_for_user(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION get_user_collection(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION add_card_to_deck_v3(UUID, UUID, INTEGER) TO authenticated;
GRANT EXECUTE ON FUNCTION remove_card_from_deck_v3(UUID, UUID, INTEGER) TO authenticated;

-- 9. Success message
DO $$ BEGIN
  RAISE NOTICE 'Enhanced card system migration completed successfully!';
  RAISE NOTICE 'New features: Complete card data model, enhanced collection system, improved deck building';
  RAISE NOTICE 'Use unlock_all_cards_for_user() function for testing';
END $$;