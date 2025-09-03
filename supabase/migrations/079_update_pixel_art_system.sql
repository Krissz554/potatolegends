-- Update pixel art system to include elemental and rarity-based visual data
-- This ensures every card has unique, creative pixel art matching their element and rarity

-- Step 1: Add new fields for enhanced pixel art data
ALTER TABLE card_complete 
ADD COLUMN IF NOT EXISTS pixel_art_config JSONB DEFAULT '{}',
ADD COLUMN IF NOT EXISTS visual_effects JSONB DEFAULT '{}';

-- Step 2: Update pixel art configuration for each card based on element and rarity
UPDATE card_complete 
SET pixel_art_config = json_build_object(
    'element', potato_type,
    'rarity', rarity,
    'seed', id::text,
    'base_color', CASE potato_type
        WHEN 'ice' THEN 'hsl(200 80% 75%)'
        WHEN 'fire' THEN 'hsl(15 90% 65%)'
        WHEN 'lightning' THEN 'hsl(280 70% 70%)'
        WHEN 'light' THEN 'hsl(50 90% 80%)'
        WHEN 'void' THEN 'hsl(270 30% 25%)'
        ELSE 'hsl(45 90% 60%)'
    END,
    'accent_color', CASE potato_type
        WHEN 'ice' THEN 'hsl(180 90% 85%)'
        WHEN 'fire' THEN 'hsl(45 100% 70%)'
        WHEN 'lightning' THEN 'hsl(290 90% 80%)'
        WHEN 'light' THEN 'hsl(60 100% 90%)'
        WHEN 'void' THEN 'hsl(280 50% 35%)'
        ELSE 'hsl(35 65% 40%)'
    END
);

-- Step 3: Update visual effects based on element and rarity
UPDATE card_complete 
SET visual_effects = json_build_object(
    'element_effects', CASE potato_type
        WHEN 'ice' THEN json_build_object(
            'snowflakes', true,
            'crystals', (abs(hashtext(id::text)) % 2) = 0,
            'icy_glow', true
        )
        WHEN 'fire' THEN json_build_object(
            'flames', true,
            'embers', (abs(hashtext(id::text)) % 3) = 0,
            'heat_glow', true
        )
        WHEN 'lightning' THEN json_build_object(
            'sparks', true,
            'electric_field', (abs(hashtext(id::text)) % 2) = 0,
            'energy_glow', true
        )
        WHEN 'light' THEN json_build_object(
            'rays', true,
            'stars', (abs(hashtext(id::text)) % 3) = 0,
            'radiance', true
        )
        WHEN 'void' THEN json_build_object(
            'shadows', true,
            'void_portals', (abs(hashtext(id::text)) % 4) = 0,
            'ethereal', true
        )
        ELSE json_build_object('basic', true)
    END,
    'rarity_effects', CASE rarity
        WHEN 'exotic' THEN json_build_object(
            'crown', true,
            'aura', true,
            'eye_glow', true,
            'particles', true
        )
        WHEN 'legendary' THEN json_build_object(
            'hat', true,
            'aura', (abs(hashtext(id::text)) % 2) = 0,
            'eye_glow', (abs(hashtext(id::text)) % 3) = 0,
            'glow', true
        )
        WHEN 'rare' THEN json_build_object(
            'accessory', (abs(hashtext(id::text)) % 3) = 0,
            'glow', (abs(hashtext(id::text)) % 2) = 0
        )
        WHEN 'uncommon' THEN json_build_object(
            'accessory', (abs(hashtext(id::text)) % 5) = 0
        )
        ELSE json_build_object('basic', true)
    END,
    'unique_features', json_build_object(
        'eye_variation', (abs(hashtext(id::text || 'eyes')) % 3),
        'smile_variation', (abs(hashtext(id::text || 'smile')) % 2),
        'spot_pattern', (abs(hashtext(id::text || 'spots')) % 5),
        'personality', CASE 
            WHEN name ILIKE '%captain%' OR name ILIKE '%general%' THEN 'heroic'
            WHEN name ILIKE '%lord%' OR name ILIKE '%queen%' OR name ILIKE '%king%' THEN 'royal'
            WHEN name ILIKE '%little%' OR name ILIKE '%tiny%' OR name ILIKE '%baby%' THEN 'cute'
            WHEN name ILIKE '%mc%' OR name ILIKE '%face%' THEN 'quirky'
            WHEN name ILIKE '%eternal%' OR name ILIKE '%supreme%' OR name ILIKE '%god%' THEN 'divine'
            ELSE 'standard'
        END
    )
);

-- Step 4: Update the traditional pixel_art_data field to be compatible
UPDATE card_complete 
SET pixel_art_data = json_build_object(
    'type', potato_type,
    'rarity', rarity,
    'palette', CASE potato_type
        WHEN 'ice' THEN 'frost_blue'
        WHEN 'fire' THEN 'flame_orange'
        WHEN 'lightning' THEN 'storm_purple'
        WHEN 'light' THEN 'golden_yellow'
        WHEN 'void' THEN 'shadow_dark'
        ELSE 'default'
    END,
    'traits', ARRAY[trait],
    'generation_seed', generation_seed || '_' || rarity || '_' || potato_type,
    'enhanced', true
);

-- Step 5: Create index for pixel art queries
CREATE INDEX IF NOT EXISTS idx_card_complete_pixel_config ON card_complete USING GIN(pixel_art_config);
CREATE INDEX IF NOT EXISTS idx_card_complete_visual_effects ON card_complete USING GIN(visual_effects);

-- Step 6: Update illustration URLs to reference the new system
UPDATE card_complete 
SET illustration_url = CASE 
    WHEN illustration_url = '' OR illustration_url IS NULL THEN
        '/api/pixel-art/' || id::text || '/' || potato_type || '/' || rarity
    ELSE illustration_url
END;

-- Step 7: Create sample queries for verification
-- Show element distribution with visual effects
CREATE TEMPORARY TABLE element_visual_summary AS
SELECT 
    potato_type,
    rarity,
    COUNT(*) as card_count,
    COUNT(*) FILTER (WHERE visual_effects->'element_effects'->>'icy_glow' = 'true') as ice_glow_count,
    COUNT(*) FILTER (WHERE visual_effects->'element_effects'->>'flames' = 'true') as flame_count,
    COUNT(*) FILTER (WHERE visual_effects->'element_effects'->>'sparks' = 'true') as spark_count,
    COUNT(*) FILTER (WHERE visual_effects->'element_effects'->>'rays' = 'true') as ray_count,
    COUNT(*) FILTER (WHERE visual_effects->'element_effects'->>'shadows' = 'true') as shadow_count,
    COUNT(*) FILTER (WHERE visual_effects->'rarity_effects'->>'crown' = 'true') as crown_count,
    COUNT(*) FILTER (WHERE visual_effects->'rarity_effects'->>'aura' = 'true') as aura_count
FROM card_complete
GROUP BY potato_type, rarity
ORDER BY 
    CASE potato_type
        WHEN 'ice' THEN 1
        WHEN 'fire' THEN 2
        WHEN 'lightning' THEN 3
        WHEN 'light' THEN 4
        WHEN 'void' THEN 5
        ELSE 6
    END,
    CASE rarity 
        WHEN 'exotic' THEN 5
        WHEN 'legendary' THEN 4
        WHEN 'rare' THEN 3
        WHEN 'uncommon' THEN 2
        ELSE 1
    END;

-- Final verification
SELECT 
    'Pixel Art System Update' as step,
    COUNT(*) as total_cards,
    COUNT(*) FILTER (WHERE pixel_art_config IS NOT NULL) as cards_with_config,
    COUNT(*) FILTER (WHERE visual_effects IS NOT NULL) as cards_with_effects,
    COUNT(DISTINCT pixel_art_config->>'element') as unique_elements,
    COUNT(DISTINCT pixel_art_config->>'rarity') as unique_rarities
FROM card_complete;

-- Show sample of enhanced pixel art data
SELECT 
    'Sample Enhanced Pixel Art' as step,
    name,
    potato_type,
    rarity,
    pixel_art_config->>'base_color' as base_color,
    visual_effects->'element_effects' as element_effects,
    visual_effects->'rarity_effects' as rarity_effects
FROM card_complete 
WHERE rarity IN ('exotic', 'legendary')
ORDER BY rarity DESC, potato_type
LIMIT 10;