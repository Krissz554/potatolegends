-- Check the actual database structure to see where cards are stored

-- 1. Check if card_complete is a table or view
DO $$
DECLARE
    table_type TEXT;
    table_exists BOOLEAN := false;
    view_exists BOOLEAN := false;
BEGIN
    RAISE NOTICE '=== DATABASE STRUCTURE CHECK ===';
    
    -- Check if card_complete exists as a table
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'card_complete'
    ) INTO table_exists;
    
    -- Check if card_complete exists as a view
    SELECT EXISTS (
        SELECT FROM information_schema.views 
        WHERE table_schema = 'public' 
        AND table_name = 'card_complete'
    ) INTO view_exists;
    
    RAISE NOTICE 'card_complete exists as table: %', table_exists;
    RAISE NOTICE 'card_complete exists as view: %', view_exists;
    
    -- List all tables that might contain card data
    RAISE NOTICE '=== AVAILABLE TABLES ===';
    FOR table_type IN
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND (table_name LIKE '%card%' OR table_name LIKE '%potato%')
        ORDER BY table_name
    LOOP
        RAISE NOTICE 'Table: %', table_type;
    END LOOP;
    
    -- List all views that might contain card data
    RAISE NOTICE '=== AVAILABLE VIEWS ===';
    FOR table_type IN
        SELECT table_name 
        FROM information_schema.views 
        WHERE table_schema = 'public' 
        AND (table_name LIKE '%card%' OR table_name LIKE '%potato%')
        ORDER BY table_name
    LOOP
        RAISE NOTICE 'View: %', table_type;
    END LOOP;
END
$$;

-- 2. Check columns in card_complete (whether table or view)
DO $$
DECLARE
    col_record RECORD;
    total_cards INTEGER;
    exotic_cards INTEGER;
BEGIN
    RAISE NOTICE '=== CARD_COMPLETE COLUMNS ===';
    
    -- List all columns in card_complete
    FOR col_record IN
        SELECT column_name, data_type 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'card_complete'
        ORDER BY ordinal_position
    LOOP
        RAISE NOTICE 'Column: % (type: %)', col_record.column_name, col_record.data_type;
    END LOOP;
    
    -- Check if exotic field exists and get counts
    BEGIN
        EXECUTE 'SELECT COUNT(*) FROM card_complete' INTO total_cards;
        RAISE NOTICE 'Total cards in card_complete: %', total_cards;
        
        -- Try to check exotic field
        BEGIN
            EXECUTE 'SELECT COUNT(*) FROM card_complete WHERE exotic = true' INTO exotic_cards;
            RAISE NOTICE 'Exotic cards (exotic = true): %', exotic_cards;
        EXCEPTION
            WHEN undefined_column THEN
                RAISE NOTICE 'Column "exotic" does not exist in card_complete';
        END;
        
        -- Try to check is_exotic field
        BEGIN
            EXECUTE 'SELECT COUNT(*) FROM card_complete WHERE is_exotic = true' INTO exotic_cards;
            RAISE NOTICE 'Exotic cards (is_exotic = true): %', exotic_cards;
        EXCEPTION
            WHEN undefined_column THEN
                RAISE NOTICE 'Column "is_exotic" does not exist in card_complete';
        END;
        
    EXCEPTION
        WHEN undefined_table THEN
            RAISE NOTICE 'card_complete does not exist or is not accessible';
    END;
END
$$;

-- 3. Show sample data from card_complete
DO $$
DECLARE
    card_record RECORD;
BEGIN
    RAISE NOTICE '=== SAMPLE CARD DATA ===';
    
    -- Show first 3 cards with all their fields
    FOR card_record IN
        SELECT *
        FROM card_complete 
        ORDER BY name
        LIMIT 3
    LOOP
        RAISE NOTICE 'Sample Card: % | Rarity: %', card_record.name, card_record.rarity;
    END LOOP;
    
EXCEPTION
    WHEN undefined_table THEN
        RAISE NOTICE 'Cannot access card_complete for sample data';
END
$$;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE '=== DATABASE STRUCTURE CHECK COMPLETED ===';
  RAISE NOTICE 'Check the logs above to see actual table structure';
END $$;