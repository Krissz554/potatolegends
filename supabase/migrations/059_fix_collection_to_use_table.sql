-- Fix collection queries to use card_complete table directly

-- 1. First check if we're dealing with a table or view
DO $$
DECLARE
    is_table BOOLEAN;
    is_view BOOLEAN;
BEGIN
    -- Check what card_complete actually is
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'card_complete'
    ) INTO is_table;
    
    SELECT EXISTS (
        SELECT FROM information_schema.views 
        WHERE table_schema = 'public' AND table_name = 'card_complete'
    ) INTO is_view;
    
    RAISE NOTICE '=== CARD_COMPLETE STRUCTURE ===';
    RAISE NOTICE 'card_complete is a table: %', is_table;
    RAISE NOTICE 'card_complete is a view: %', is_view;
END
$$;

-- 2. Update get_user_collection to query card_complete table directly
CREATE OR REPLACE FUNCTION get_user_collection(user_uuid UUID)
RETURNS JSON AS $$
DECLARE
    result JSON;
BEGIN
    -- Query collections joined with card_complete TABLE (not view)
    WITH collection_with_cards AS (
        SELECT 
            col.quantity,
            col.acquired_at,
            col.source,
            -- Get ALL fields from card_complete table
            cc.*
        FROM collections col
        JOIN card_complete cc ON col.card_id = cc.id
        WHERE col.user_id = user_uuid
    )
    SELECT json_agg(
        json_build_object(
            'card', row_to_json(collection_with_cards.*) - 'quantity' - 'acquired_at' - 'source',
            'quantity', quantity,
            'acquired_at', acquired_at,
            'source', source
        )
    ) INTO result
    FROM collection_with_cards;
    
    RETURN COALESCE(result, '[]'::json);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 3. Test the updated function
DO $$
DECLARE
    test_result JSON;
    collection_count INTEGER;
    exotic_count INTEGER;
BEGIN
    RAISE NOTICE '=== TESTING UPDATED COLLECTION FUNCTION ===';
    
    -- Get collection count
    SELECT COUNT(*) INTO collection_count FROM collections;
    RAISE NOTICE 'Total collection entries: %', collection_count;
    
    -- Test the function (using first user if any)
    IF collection_count > 0 THEN
        SELECT get_user_collection(user_id) INTO test_result
        FROM collections 
        LIMIT 1;
        
        RAISE NOTICE 'Collection function returned data length: %', 
            CASE WHEN test_result IS NULL THEN 0 
                 ELSE json_array_length(test_result) 
            END;
    END IF;
    
    -- Check for exotic cards in card_complete table
    BEGIN
        SELECT COUNT(*) INTO exotic_count FROM card_complete WHERE exotic = true;
        RAISE NOTICE 'Exotic cards in card_complete (exotic field): %', exotic_count;
    EXCEPTION
        WHEN undefined_column THEN
            BEGIN
                SELECT COUNT(*) INTO exotic_count FROM card_complete WHERE is_exotic = true;
                RAISE NOTICE 'Exotic cards in card_complete (is_exotic field): %', exotic_count;
            EXCEPTION
                WHEN undefined_column THEN
                    RAISE NOTICE 'Neither exotic nor is_exotic field found in card_complete';
            END;
    END;
END
$$;

-- Grant permissions
GRANT EXECUTE ON FUNCTION get_user_collection(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION get_user_collection(UUID) TO anon;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== COLLECTION FUNCTION UPDATED ===';
  RAISE NOTICE 'Now queries card_complete table directly';
  RAISE NOTICE 'Should preserve all exotic field data';
END $$;