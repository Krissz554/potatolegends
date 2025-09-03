-- Fix card_complete view to properly map exotic rarity to exotic flag
-- Ensure cards with rarity='exotic' automatically have exotic=true

-- 1. First check what we currently have
DO $$
DECLARE
    total_cards INTEGER;
    exotic_rarity_cards INTEGER;
    exotic_flag_cards INTEGER;
BEGIN
    RAISE NOTICE '=== CURRENT STATE CHECK ===';
    
    SELECT COUNT(*) INTO total_cards FROM card_complete;
    SELECT COUNT(*) INTO exotic_rarity_cards FROM card_complete WHERE rarity = 'exotic';
    SELECT COUNT(*) INTO exotic_flag_cards FROM card_complete WHERE exotic = true;
    
    RAISE NOTICE 'Total cards in view: %', total_cards;
    RAISE NOTICE 'Cards with rarity = exotic: %', exotic_rarity_cards;
    RAISE NOTICE 'Cards with exotic = true: %', exotic_flag_cards;
END
$$;

-- 2. Drop and recreate the view with proper exotic mapping
DROP VIEW IF EXISTS card_complete;

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
  
  -- FIXED EXOTIC FIELD - true if rarity='exotic' OR is_exotic=true
  CASE 
    WHEN rarity = 'exotic' THEN true
    ELSE COALESCE(is_exotic, false)
  END as exotic,
  
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

-- 3. Test the fixed view
DO $$
DECLARE
    total_cards INTEGER;
    exotic_rarity_cards INTEGER;
    exotic_flag_cards INTEGER;
    both_conditions INTEGER;
    card_record RECORD;
BEGIN
    RAISE NOTICE '=== TESTING FIXED VIEW ===';
    
    SELECT COUNT(*) INTO total_cards FROM card_complete;
    SELECT COUNT(*) INTO exotic_rarity_cards FROM card_complete WHERE rarity = 'exotic';
    SELECT COUNT(*) INTO exotic_flag_cards FROM card_complete WHERE exotic = true;
    SELECT COUNT(*) INTO both_conditions FROM card_complete WHERE rarity = 'exotic' AND exotic = true;
    
    RAISE NOTICE 'Total cards: %', total_cards;
    RAISE NOTICE 'Cards with rarity = exotic: %', exotic_rarity_cards;
    RAISE NOTICE 'Cards with exotic = true: %', exotic_flag_cards;
    RAISE NOTICE 'Cards with both (should equal exotic rarity): %', both_conditions;
    
    -- Show sample exotic cards
    RAISE NOTICE '=== SAMPLE EXOTIC CARDS ===';
    FOR card_record IN
        SELECT name, rarity, exotic
        FROM card_complete 
        WHERE rarity = 'exotic'
        ORDER BY name
        LIMIT 5
    LOOP
        RAISE NOTICE 'EXOTIC: % | rarity: % | exotic: %', 
            card_record.name, card_record.rarity, card_record.exotic;
    END LOOP;
    
    -- Verify consistency
    IF exotic_rarity_cards = both_conditions THEN
        RAISE NOTICE '✅ SUCCESS: All exotic rarity cards now have exotic=true';
    ELSE
        RAISE NOTICE '❌ ISSUE: Mismatch between exotic rarity and exotic flag';
    END IF;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== VIEW EXOTIC MAPPING FIXED ===';
  RAISE NOTICE 'card_complete view now properly maps:';
  RAISE NOTICE '• rarity=exotic → exotic=true automatically';
  RAISE NOTICE '• All 66+ exotic cards should now have exotic=true';
  RAISE NOTICE '• Frontend filters will find all exotic cards';
END $$;