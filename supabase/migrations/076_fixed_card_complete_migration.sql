-- Fixed migration: Convert card_complete from view to table as single source of truth
-- This replaces 074_convert_card_complete_to_table.sql with proper function handling

-- Step 1: Drop existing functions that will be recreated
DROP FUNCTION IF EXISTS unlock_all_cards_for_user(uuid);
DROP FUNCTION IF EXISTS get_user_collection(uuid);
DROP FUNCTION IF EXISTS create_starter_collection_for_user(uuid);
DROP FUNCTION IF EXISTS add_card_to_deck_v3(uuid, uuid, integer);
DROP FUNCTION IF EXISTS remove_card_from_deck_v3(uuid, uuid, integer);
DROP FUNCTION IF EXISTS validate_deck(uuid);

-- Step 2: Drop the existing card_complete view
DROP VIEW IF EXISTS card_complete CASCADE;

-- Step 3: Create card_complete as a proper table with all essential columns
CREATE TABLE card_complete (
  -- Core identifiers
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  registry_id VARCHAR(50) UNIQUE NOT NULL,
  
  -- Card identity
  name VARCHAR(255) NOT NULL,
  description TEXT NOT NULL,
  
  -- Card type and attributes  
  potato_type VARCHAR(50) NOT NULL,
  trait TEXT,
  adjective VARCHAR(100),
  rarity VARCHAR(20) NOT NULL CHECK (rarity IN ('common', 'uncommon', 'rare', 'legendary', 'exotic')),
  
  -- Visual data
  palette_name VARCHAR(50),
  pixel_art_data JSONB,
  generation_seed TEXT,
  variation_index INTEGER DEFAULT 0,
  sort_order INTEGER,
  
  -- Gameplay stats
  mana_cost INTEGER NOT NULL DEFAULT 1,
  attack INTEGER NOT NULL DEFAULT 1,
  hp INTEGER NOT NULL DEFAULT 1,
  card_type VARCHAR(20) NOT NULL DEFAULT 'unit',
  
  -- Special flags (mutually exclusive)
  is_legendary BOOLEAN NOT NULL DEFAULT false,
  exotic BOOLEAN NOT NULL DEFAULT false,
  
  -- Enhanced identifiers
  set_id VARCHAR(50) NOT NULL DEFAULT 'core',
  format_legalities TEXT[] NOT NULL DEFAULT ARRAY['standard'],
  
  -- Enhanced abilities & keywords
  keywords TEXT[] NOT NULL DEFAULT ARRAY['Charge'],
  ability_text TEXT NOT NULL DEFAULT '',
  passive_effects JSONB NOT NULL DEFAULT '{}',
  triggered_effects JSONB NOT NULL DEFAULT '{}',
  
  -- Enhanced metadata
  illustration_url TEXT NOT NULL DEFAULT '',
  frame_style VARCHAR(50) NOT NULL DEFAULT 'standard',
  flavor_text TEXT,
  
  -- Enhanced balance data
  release_date DATE NOT NULL DEFAULT CURRENT_DATE,
  tags TEXT[] NOT NULL DEFAULT ARRAY['core'],
  craft_cost INTEGER NOT NULL DEFAULT 40,
  dust_value INTEGER NOT NULL DEFAULT 10,
  
  -- Optional advanced features
  alternate_skins JSONB DEFAULT '{}',
  voice_line_url TEXT NOT NULL DEFAULT '',
  level_up_conditions JSONB NOT NULL DEFAULT '{}',
  token_spawns TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
  
  -- Timestamps
  created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
  
  -- Ensure legendary and exotic are mutually exclusive
  CONSTRAINT exclusive_legendary_exotic CHECK (NOT (is_legendary = true AND exotic = true))
);

-- Step 4: Migrate ALL current data from potato_registry to card_complete
INSERT INTO card_complete (
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
  mana_cost,
  attack,
  hp,
  card_type,
  is_legendary,
  exotic,
  set_id,
  format_legalities,
  keywords,
  ability_text,
  passive_effects,
  triggered_effects,
  illustration_url,
  frame_style,
  flavor_text,
  release_date,
  tags,
  craft_cost,
  dust_value,
  alternate_skins,
  voice_line_url,
  level_up_conditions,
  token_spawns
)
SELECT 
  -- Original core fields (preserve existing data)
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
  
  -- Gameplay stats with sensible defaults
  COALESCE(mana_cost, 1) as mana_cost,
  COALESCE(attack, 1) as attack,
  COALESCE(hp, 1) as hp,
  COALESCE(card_type, 'unit') as card_type,
  COALESCE(is_legendary, false) as is_legendary,
  
  -- Handle exotic field: rarity='exotic' OR is_exotic=true, but NOT both legendary AND exotic
  CASE 
    WHEN rarity = 'exotic' AND COALESCE(is_legendary, false) = false THEN true
    WHEN COALESCE(is_exotic, false) = true AND COALESCE(is_legendary, false) = false THEN true
    ELSE false
  END as exotic,
  
  -- Enhanced identifiers with defaults
  COALESCE(set_id, 'core') as set_id,
  COALESCE(format_legalities, ARRAY['standard']) as format_legalities,
  
  -- Enhanced abilities & keywords with defaults
  COALESCE(keywords, ARRAY['Charge']) as keywords,
  COALESCE(ability_text, '') as ability_text,
  COALESCE(passive_effects, '{}') as passive_effects,
  COALESCE(triggered_effects, '{}') as triggered_effects,
  
  -- Enhanced metadata with defaults
  COALESCE(illustration_url, '') as illustration_url,
  COALESCE(frame_style, 'standard') as frame_style,
  COALESCE(flavor_text, description) as flavor_text,
  
  -- Enhanced balance data with defaults
  COALESCE(release_date, CURRENT_DATE) as release_date,
  COALESCE(tags, ARRAY['core']) as tags,
  COALESCE(craft_cost, 40) as craft_cost,
  COALESCE(dust_value, 10) as dust_value,
  COALESCE(alternate_skins, '{}') as alternate_skins,
  COALESCE(voice_line_url, '') as voice_line_url,
  COALESCE(level_up_conditions, '{}') as level_up_conditions,
  COALESCE(token_spawns, ARRAY[]::TEXT[]) as token_spawns

FROM potato_registry
WHERE COALESCE(format_legal, true) = true;

-- Step 5: Create performance indexes
CREATE INDEX idx_card_complete_rarity ON card_complete(rarity);
CREATE INDEX idx_card_complete_potato_type ON card_complete(potato_type);
CREATE INDEX idx_card_complete_exotic ON card_complete(exotic);
CREATE INDEX idx_card_complete_is_legendary ON card_complete(is_legendary);
CREATE INDEX idx_card_complete_mana_cost ON card_complete(mana_cost);
CREATE INDEX idx_card_complete_registry_id ON card_complete(registry_id);
CREATE INDEX idx_card_complete_name ON card_complete(name);

-- Step 6: Update foreign key constraints to point to card_complete
-- Update collections table
ALTER TABLE collections 
DROP CONSTRAINT IF EXISTS collections_card_id_fkey,
DROP CONSTRAINT IF EXISTS collections_card_id_potato_registry_fkey;

ALTER TABLE collections 
ADD CONSTRAINT collections_card_id_fkey 
FOREIGN KEY (card_id) REFERENCES card_complete(id) ON DELETE CASCADE;

-- Update deck_cards table  
ALTER TABLE deck_cards
DROP CONSTRAINT IF EXISTS deck_cards_card_id_fkey,
DROP CONSTRAINT IF EXISTS deck_cards_card_id_potato_registry_fkey;

ALTER TABLE deck_cards
ADD CONSTRAINT deck_cards_card_id_fkey 
FOREIGN KEY (card_id) REFERENCES card_complete(id) ON DELETE CASCADE;

-- Step 7: Recreate RPC functions to use card_complete
CREATE OR REPLACE FUNCTION get_user_collection(user_uuid UUID)
RETURNS JSON AS $$
DECLARE
    result JSON;
BEGIN
    WITH collection_with_cards AS (
        SELECT 
            col.quantity,
            col.acquired_at,
            col.source,
            cc.*
        FROM collections col
        JOIN card_complete cc ON col.card_id = cc.id
        WHERE col.user_id = user_uuid
    )
    SELECT json_agg(
        json_build_object(
            'card', json_build_object(
                'id', id,
                'registry_id', registry_id,
                'name', name,
                'description', description,
                'potato_type', potato_type,
                'trait', trait,
                'adjective', adjective,
                'rarity', rarity,
                'palette_name', palette_name,
                'pixel_art_data', pixel_art_data,
                'generation_seed', generation_seed,
                'variation_index', variation_index,
                'sort_order', sort_order,
                'created_at', created_at,
                'mana_cost', mana_cost,
                'attack', attack,
                'hp', hp,
                'card_type', card_type,
                'is_legendary', is_legendary,
                'exotic', exotic,
                'set_id', set_id,
                'format_legalities', format_legalities,
                'keywords', keywords,
                'ability_text', ability_text,
                'passive_effects', passive_effects,
                'triggered_effects', triggered_effects,
                'illustration_url', illustration_url,
                'frame_style', frame_style,
                'flavor_text', flavor_text,
                'release_date', release_date,
                'tags', tags,
                'craft_cost', craft_cost,
                'dust_value', dust_value,
                'alternate_skins', alternate_skins,
                'voice_line_url', voice_line_url,
                'level_up_conditions', level_up_conditions,
                'token_spawns', token_spawns
            ),
            'quantity', quantity,
            'acquired_at', acquired_at,
            'source', source
        )
    ) INTO result
    FROM collection_with_cards;
    
    RETURN COALESCE(result, '[]'::json);
END;
$$ LANGUAGE plpgsql;

-- Function to unlock all cards for a user (testing)
CREATE OR REPLACE FUNCTION unlock_all_cards_for_user(user_uuid UUID)
RETURNS VOID AS $$
BEGIN
    -- Clear existing collection for clean slate
    DELETE FROM collections WHERE user_id = user_uuid;
    
    -- Add all cards from card_complete to user's collection with proper limits
    INSERT INTO collections (user_id, card_id, quantity, source)
    SELECT 
        user_uuid,
        cc.id,
        CASE 
            WHEN cc.exotic = true THEN 1      -- Exotic: 1 copy max
            WHEN cc.is_legendary = true THEN 2 -- Legendary: 2 copies max  
            ELSE 4                             -- Regular: 4 copies max
        END as quantity,
        'unlock_all'
    FROM card_complete cc;
    
    RAISE NOTICE 'Unlocked % cards for user %', 
        (SELECT COUNT(*) FROM card_complete), 
        user_uuid;
END;
$$ LANGUAGE plpgsql;

-- Function to validate deck composition
CREATE OR REPLACE FUNCTION validate_deck(deck_uuid UUID)
RETURNS JSON AS $$
DECLARE
    deck_info RECORD;
    validation_result JSON;
    total_cards INTEGER;
    errors TEXT[] := '{}';
    card_counts RECORD;
BEGIN
    -- Get deck info
    SELECT user_id INTO deck_info FROM player_decks WHERE id = deck_uuid;
    
    -- Count total cards in deck
    SELECT COUNT(*) INTO total_cards FROM deck_cards WHERE deck_id = deck_uuid;
    
    -- Check deck size
    IF total_cards != 30 THEN
        errors := array_append(errors, 'Deck must contain exactly 30 cards (currently has ' || total_cards || ')');
    END IF;
    
    -- Check copy limits for each card
    FOR card_counts IN
        SELECT 
            dc.card_id,
            cc.name,
            cc.exotic,
            cc.is_legendary,
            COUNT(*) as copies_in_deck
        FROM deck_cards dc
        JOIN card_complete cc ON dc.card_id = cc.id
        WHERE dc.deck_id = deck_uuid
        GROUP BY dc.card_id, cc.name, cc.exotic, cc.is_legendary
    LOOP
        -- Check copy limits based on card type
        IF card_counts.exotic = true AND card_counts.copies_in_deck > 1 THEN
            errors := array_append(errors, 'Exotic card "' || card_counts.name || '" can only have 1 copy (has ' || card_counts.copies_in_deck || ')');
        ELSIF card_counts.is_legendary = true AND card_counts.copies_in_deck > 2 THEN
            errors := array_append(errors, 'Legendary card "' || card_counts.name || '" can only have 2 copies (has ' || card_counts.copies_in_deck || ')');
        ELSIF card_counts.exotic = false AND card_counts.is_legendary = false AND card_counts.copies_in_deck > 2 THEN
            errors := array_append(errors, 'Regular card "' || card_counts.name || '" can only have 2 copies (has ' || card_counts.copies_in_deck || ')');
        END IF;
    END LOOP;
    
    -- Build result
    validation_result := json_build_object(
        'valid', array_length(errors, 1) IS NULL,
        'errors', errors,
        'total_cards', total_cards
    );
    
    RETURN validation_result;
END;
$$ LANGUAGE plpgsql;

-- Step 8: Create starter collection function
CREATE OR REPLACE FUNCTION create_starter_collection_for_user(user_uuid UUID)
RETURNS VOID AS $$
BEGIN
    -- Clear existing collection
    DELETE FROM collections WHERE user_id = user_uuid;
    
    -- Add starter cards (common and uncommon cards with basic quantities)
    INSERT INTO collections (user_id, card_id, quantity, source)
    SELECT 
        user_uuid,
        cc.id,
        CASE 
            WHEN cc.rarity = 'common' THEN 2
            WHEN cc.rarity = 'uncommon' THEN 1
            ELSE 0 -- No rare, legendary, or exotic in starter
        END as quantity,
        'starter_pack'
    FROM card_complete cc
    WHERE cc.rarity IN ('common', 'uncommon')
    AND quantity > 0;
    
    RAISE NOTICE 'Created starter collection for user %', user_uuid;
END;
$$ LANGUAGE plpgsql;

-- Step 9: Verification and data integrity check
SELECT 
    'Migration Verification' as step,
    COUNT(*) as total_cards_migrated,
    COUNT(DISTINCT name) as unique_names,
    COUNT(*) FILTER (WHERE exotic = true) as exotic_cards,
    COUNT(*) FILTER (WHERE is_legendary = true) as legendary_cards,
    COUNT(*) FILTER (WHERE rarity = 'common') as common_cards,
    COUNT(*) FILTER (WHERE rarity = 'uncommon') as uncommon_cards,
    COUNT(*) FILTER (WHERE rarity = 'rare') as rare_cards
FROM card_complete;

-- Check that no card is both legendary and exotic
SELECT 
    'Mutual Exclusivity Check' as step,
    COUNT(*) FILTER (WHERE is_legendary = true AND exotic = true) as illegal_legendary_exotic_cards,
    CASE 
        WHEN COUNT(*) FILTER (WHERE is_legendary = true AND exotic = true) = 0 
        THEN 'PASS: No cards are both legendary and exotic'
        ELSE 'FAIL: Some cards are both legendary and exotic'
    END as result
FROM card_complete;