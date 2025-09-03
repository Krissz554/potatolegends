-- Fix deck builder query issues
-- 1. Ensure proper foreign key constraint exists between deck_cards and potato_registry
-- 2. Fix the relationship so Supabase can understand the join

-- First, check if foreign key constraint exists and add it if missing
DO $$
BEGIN
    -- Check if foreign key constraint exists
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'deck_cards' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
        AND kcu.referenced_table_name = 'potato_registry'
    ) THEN
        -- Add the foreign key constraint
        ALTER TABLE deck_cards 
        ADD CONSTRAINT deck_cards_card_id_fkey 
        FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;
        
        RAISE NOTICE 'Added foreign key constraint: deck_cards.card_id -> potato_registry.id';
    ELSE
        RAISE NOTICE 'Foreign key constraint already exists';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error adding foreign key constraint: %', SQLERRM;
END
$$;

-- Similarly for collections table
DO $$
BEGIN
    -- Check if foreign key constraint exists
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_name = 'collections' 
        AND tc.constraint_type = 'FOREIGN KEY'
        AND kcu.column_name = 'card_id'
        AND kcu.referenced_table_name = 'potato_registry'
    ) THEN
        -- Add the foreign key constraint
        ALTER TABLE collections 
        ADD CONSTRAINT collections_card_id_fkey 
        FOREIGN KEY (card_id) REFERENCES potato_registry(id) ON DELETE CASCADE;
        
        RAISE NOTICE 'Added foreign key constraint: collections.card_id -> potato_registry.id';
    ELSE
        RAISE NOTICE 'Foreign key constraint already exists';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error adding foreign key constraint: %', SQLERRM;
END
$$;

-- Refresh the schema cache to ensure Supabase recognizes the relationships
NOTIFY pgrst, 'reload schema';

-- Create a helpful view for deck cards with proper joins
CREATE OR REPLACE VIEW deck_cards_with_cards AS
SELECT 
    dc.id,
    dc.deck_id,
    dc.card_id,
    dc.quantity,
    dc.added_at,
    pr.registry_id,
    pr.name,
    pr.description,
    pr.potato_type,
    pr.trait,
    pr.adjective,
    pr.rarity,
    pr.palette_name,
    pr.pixel_art_data,
    pr.mana_cost,
    pr.attack,
    pr.hp,
    pr.card_type,
    pr.keywords,
    pr.is_legendary,
    pr.is_exotic,
    pr.set_id,
    pr.format_legalities,
    pr.ability_text,
    pr.flavor_text,
    pr.craft_cost,
    pr.dust_value
FROM deck_cards dc
JOIN potato_registry pr ON dc.card_id = pr.id;

-- Grant permissions on the view
GRANT SELECT ON deck_cards_with_cards TO authenticated;
GRANT SELECT ON deck_cards_with_cards TO anon;

-- Create a similar view for collections
CREATE OR REPLACE VIEW collections_with_cards AS
SELECT 
    c.id,
    c.user_id,
    c.card_id,
    c.quantity,
    c.acquired_at,
    c.source,
    pr.registry_id,
    pr.name,
    pr.description,
    pr.potato_type,
    pr.trait,
    pr.adjective,
    pr.rarity,
    pr.palette_name,
    pr.pixel_art_data,
    pr.mana_cost,
    pr.attack,
    pr.hp,
    pr.card_type,
    pr.keywords,
    pr.is_legendary,
    pr.is_exotic,
    pr.set_id,
    pr.format_legalities,
    pr.ability_text,
    pr.flavor_text,
    pr.craft_cost,
    pr.dust_value
FROM collections c
JOIN potato_registry pr ON c.card_id = pr.id;

-- Grant permissions on the view
GRANT SELECT ON collections_with_cards TO authenticated;
GRANT SELECT ON collections_with_cards TO anon;

-- Success message
DO $$ 
BEGIN
  RAISE NOTICE 'Fixed deck builder foreign key relationships and created helper views';
  RAISE NOTICE 'Views available: deck_cards_with_cards, collections_with_cards';
END $$;