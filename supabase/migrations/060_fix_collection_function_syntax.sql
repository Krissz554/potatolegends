-- Fix the collection function with proper PostgreSQL syntax

-- Drop and recreate the function with correct syntax
DROP FUNCTION IF EXISTS get_user_collection(UUID);

CREATE OR REPLACE FUNCTION get_user_collection(user_uuid UUID)
RETURNS JSON AS $$
DECLARE
    result JSON;
BEGIN
    -- Query collections joined with card_complete table directly
    WITH collection_with_cards AS (
        SELECT 
            col.quantity,
            col.acquired_at,
            col.source,
            -- Explicitly select all card fields from card_complete table
            cc.id,
            cc.registry_id,
            cc.name,
            cc.description,
            cc.potato_type,
            cc.trait,
            cc.adjective,
            cc.rarity,
            cc.palette_name,
            cc.pixel_art_data,
            cc.mana_cost,
            cc.attack,
            cc.hp,
            cc.card_type,
            cc.is_legendary,
            cc.exotic,
            cc.set_id,
            cc.format_legalities,
            cc.keywords,
            cc.ability_text,
            cc.flavor_text,
            cc.craft_cost,
            cc.dust_value,
            cc.generation_seed,
            cc.variation_index,
            cc.sort_order,
            cc.created_at,
            cc.passive_effects,
            cc.triggered_effects,
            cc.illustration_url,
            cc.frame_style,
            cc.release_date,
            cc.tags,
            cc.alternate_skins,
            cc.voice_line_url,
            cc.level_up_conditions,
            cc.token_spawns
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
                'flavor_text', flavor_text,
                'craft_cost', craft_cost,
                'dust_value', dust_value,
                'generation_seed', generation_seed,
                'variation_index', variation_index,
                'sort_order', sort_order,
                'created_at', created_at,
                'passive_effects', passive_effects,
                'triggered_effects', triggered_effects,
                'illustration_url', illustration_url,
                'frame_style', frame_style,
                'release_date', release_date,
                'tags', tags,
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
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION get_user_collection(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION get_user_collection(UUID) TO anon;

-- Test the function
DO $$
DECLARE
    test_result JSON;
    collection_count INTEGER;
    card_sample RECORD;
BEGIN
    RAISE NOTICE '=== TESTING FIXED COLLECTION FUNCTION ===';
    
    -- Check how many collection entries exist
    SELECT COUNT(*) INTO collection_count FROM collections;
    RAISE NOTICE 'Total collection entries: %', collection_count;
    
    -- Check what fields exist in card_complete
    RAISE NOTICE '=== CHECKING CARD_COMPLETE FIELDS ===';
    FOR card_sample IN
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'card_complete'
        AND column_name IN ('exotic', 'is_exotic', 'rarity')
        ORDER BY column_name
    LOOP
        RAISE NOTICE 'Found field: %', card_sample.column_name;
    END LOOP;
    
    -- Test with sample data if collections exist
    IF collection_count > 0 THEN
        SELECT get_user_collection(user_id) INTO test_result
        FROM collections 
        LIMIT 1;
        
        RAISE NOTICE 'Function test successful - returned % items', 
            CASE WHEN test_result IS NULL THEN 0 
                 ELSE json_array_length(test_result) 
            END;
    ELSE
        RAISE NOTICE 'No collection entries found to test with';
    END IF;
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== COLLECTION FUNCTION FIXED ===';
  RAISE NOTICE 'Now properly queries card_complete table';
  RAISE NOTICE 'Uses correct PostgreSQL JSON syntax';
  RAISE NOTICE 'Preserves all exotic field data';
END $$;