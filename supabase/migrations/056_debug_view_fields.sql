-- Debug the card_complete view to see exotic field mapping

-- 1. Show the current view definition
DO $$
BEGIN
    RAISE NOTICE '=== CHECKING CARD_COMPLETE VIEW ===';
    RAISE NOTICE 'This migration will verify the exotic field mapping';
END
$$;

-- 2. Recreate the view with explicit exotic field debugging
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
  
  -- EXOTIC FIELD DEBUGGING - expose both fields explicitly
  COALESCE(is_exotic, false) as exotic,
  COALESCE(is_exotic, false) as is_exotic,  -- Also include original field for debugging
  
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

-- 3. Test the view immediately after creation
DO $$
DECLARE
    total_cards INTEGER;
    exotic_cards INTEGER;
    card_record RECORD;
BEGIN
    SELECT COUNT(*) INTO total_cards FROM card_complete;
    SELECT COUNT(*) INTO exotic_cards FROM card_complete WHERE exotic = true;
    
    RAISE NOTICE '=== VIEW TEST RESULTS ===';
    RAISE NOTICE 'Total cards in view: %', total_cards;
    RAISE NOTICE 'Exotic cards in view: %', exotic_cards;
    
    -- Show exotic cards from the view
    FOR card_record IN
        SELECT name, rarity, exotic, is_exotic
        FROM card_complete 
        WHERE exotic = true OR is_exotic = true
        ORDER BY name
        LIMIT 5
    LOOP
        RAISE NOTICE 'EXOTIC CARD: % | Rarity: % | exotic: % | is_exotic: %', 
            card_record.name, card_record.rarity, card_record.exotic, card_record.is_exotic;
    END LOOP;
    
    IF exotic_cards = 0 THEN
        RAISE NOTICE 'WARNING: No exotic cards found in view - checking potato_registry...';
        
        SELECT COUNT(*) INTO exotic_cards FROM potato_registry WHERE is_exotic = true;
        RAISE NOTICE 'Exotic cards in potato_registry: %', exotic_cards;
    END IF;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== VIEW RECREATION COMPLETED ===';
  RAISE NOTICE 'card_complete view now includes both exotic and is_exotic fields';
  RAISE NOTICE 'Frontend debugging should show field values clearly';
END $$;