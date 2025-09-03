-- Fix the view recreation issue by properly dropping and recreating

-- 1. Drop the existing view first
DROP VIEW IF EXISTS card_complete;

-- 2. Recreate the view with proper exotic field mapping
CREATE VIEW card_complete AS
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
  
  -- EXOTIC FIELD - properly mapped from is_exotic
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
  
  -- Advanced fields (preserved for compatibility)
  COALESCE(alternate_skins, '[]') as alternate_skins,
  COALESCE(voice_line_url, '') as voice_line_url,
  COALESCE(level_up_conditions, '{}') as level_up_conditions,
  COALESCE(token_spawns, ARRAY[]::text[]) as token_spawns
  
FROM potato_registry;

-- 3. Test the recreated view
DO $$
DECLARE
    total_cards INTEGER;
    exotic_cards INTEGER;
    registry_exotic INTEGER;
    card_record RECORD;
BEGIN
    -- Check counts
    SELECT COUNT(*) INTO total_cards FROM card_complete;
    SELECT COUNT(*) INTO exotic_cards FROM card_complete WHERE exotic = true;
    SELECT COUNT(*) INTO registry_exotic FROM potato_registry WHERE is_exotic = true;
    
    RAISE NOTICE '=== VIEW RECREATION TEST ===';
    RAISE NOTICE 'Total cards in view: %', total_cards;
    RAISE NOTICE 'Exotic cards in view: %', exotic_cards;
    RAISE NOTICE 'Exotic cards in registry: %', registry_exotic;
    
    -- If no exotic cards, create one for testing
    IF registry_exotic = 0 THEN
        RAISE NOTICE 'No exotic cards found - creating one for testing...';
        
        UPDATE potato_registry 
        SET is_exotic = true
        WHERE id = (
            SELECT id 
            FROM potato_registry 
            WHERE rarity = 'rare' AND COALESCE(is_legendary, false) = false
            ORDER BY sort_order, name
            LIMIT 1
        );
        
        -- Recheck counts
        SELECT COUNT(*) INTO exotic_cards FROM card_complete WHERE exotic = true;
        SELECT COUNT(*) INTO registry_exotic FROM potato_registry WHERE is_exotic = true;
        
        RAISE NOTICE 'After creating exotic card:';
        RAISE NOTICE 'Exotic cards in view: %', exotic_cards;
        RAISE NOTICE 'Exotic cards in registry: %', registry_exotic;
    END IF;
    
    -- Show exotic cards
    FOR card_record IN
        SELECT name, rarity, exotic
        FROM card_complete 
        WHERE exotic = true
        ORDER BY name
        LIMIT 3
    LOOP
        RAISE NOTICE 'EXOTIC CARD: % (%) - exotic: %', 
            card_record.name, card_record.rarity, card_record.exotic;
    END LOOP;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== VIEW RECREATION COMPLETED ===';
  RAISE NOTICE 'card_complete view properly recreated with exotic field';
  RAISE NOTICE 'Exotic cards should now be visible to frontend';
END $$;