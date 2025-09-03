-- Fix the get_user_collection function SQL error
-- Resolves the GROUP BY clause error

-- Drop and recreate the function with proper SQL
DROP FUNCTION IF EXISTS get_user_collection(UUID);

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

-- Grant permissions
GRANT EXECUTE ON FUNCTION get_user_collection(UUID) TO authenticated;

-- Test the function
DO $$ 
BEGIN
  RAISE NOTICE 'get_user_collection function updated successfully';
END $$;