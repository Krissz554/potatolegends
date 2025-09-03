-- Simple fix for collection function - remove complex aggregation
-- This fixes the GROUP BY clause error definitively

-- Drop the problematic function
DROP FUNCTION IF EXISTS get_user_collection(UUID);

-- Create a much simpler version that works
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
            c.id,
            c.registry_id,
            c.name,
            c.description,
            c.potato_type,
            c.trait,
            c.adjective,
            c.rarity,
            c.palette_name,
            c.pixel_art_data,
            c.mana_cost,
            c.attack,
            c.hp,
            c.card_type,
            c.is_legendary,
            c.exotic,
            c.set_id,
            c.format_legalities,
            c.keywords,
            c.ability_text,
            c.flavor_text,
            c.craft_cost,
            c.dust_value
        FROM collections col
        JOIN card_complete c ON col.card_id = c.id
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
                'dust_value', dust_value
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

-- Verify the function works
DO $$ 
BEGIN
  RAISE NOTICE 'get_user_collection function recreated with CTE approach';
END $$;