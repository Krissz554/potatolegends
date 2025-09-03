-- Simple unique names for all cards with fixed syntax
-- Update potato types to elemental types and assign unique names

-- 1. Update potato_type to elemental types randomly
UPDATE potato_registry 
SET potato_type = CASE 
    WHEN RANDOM() < 0.2 THEN 'ice'
    WHEN RANDOM() < 0.4 THEN 'fire' 
    WHEN RANDOM() < 0.6 THEN 'lightning'
    WHEN RANDOM() < 0.8 THEN 'light'
    ELSE 'void'
END;

-- 2. Create unique names for ICE cards
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 'Frostmourne the Eternal ' || id::text
        WHEN rarity = 'legendary' THEN 'Captain Frostbite ' || id::text
        WHEN rarity = 'rare' THEN 'Chilly McFreeze ' || id::text
        WHEN rarity = 'uncommon' THEN 'Cool Guy ' || id::text
        ELSE 'Tiny Frost Chip ' || id::text
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 'An ancient ice potato of immeasurable power who commands winter itself.'
        WHEN rarity = 'legendary' THEN 'A fearless captain who sailed the seven frozen seas.'
        WHEN rarity = 'rare' THEN 'A perpetually cold potato who wears seventeen sweaters.'
        WHEN rarity = 'uncommon' THEN 'A laid-back potato who thinks everything is totally chill.'
        ELSE 'A tiny chip of ice that dreams of becoming a mighty glacier.'
    END
WHERE potato_type = 'ice';

-- 3. Create unique names for FIRE cards  
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 'Inferno Lord ' || id::text
        WHEN rarity = 'legendary' THEN 'Captain Burnface ' || id::text
        WHEN rarity = 'rare' THEN 'Spicy McFireFace ' || id::text
        WHEN rarity = 'uncommon' THEN 'Toasty McGillicuddy ' || id::text
        ELSE 'Tiny Ember Chip ' || id::text
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 'A primordial fire potato whose flames have burned since the dawn of time.'
        WHEN rarity = 'legendary' THEN 'A fearless captain whose face is literally on fire but remains distinguished.'
        WHEN rarity = 'rare' THEN 'A perpetually spicy potato who sweats hot sauce.'
        WHEN rarity = 'uncommon' THEN 'A warm-hearted potato who gives great hugs and only occasionally sets people on fire.'
        ELSE 'A tiny ember that dreams of becoming a mighty bonfire someday.'
    END
WHERE potato_type = 'fire';

-- 4. Create unique names for LIGHTNING cards
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 'Storm Lord ' || id::text
        WHEN rarity = 'legendary' THEN 'Captain Thunderbolt ' || id::text
        WHEN rarity = 'rare' THEN 'Zappy McShockface ' || id::text
        WHEN rarity = 'uncommon' THEN 'Buzzy McGillicuddy ' || id::text
        ELSE 'Tiny Spark Chip ' || id::text
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 'An ancient storm deity who commands thunder and lightning across multiple dimensions.'
        WHEN rarity = 'legendary' THEN 'A fearless captain who rides lightning bolts like horses.'
        WHEN rarity = 'rare' THEN 'A perpetually energetic potato who bounces off walls and generates static electricity.'
        WHEN rarity = 'uncommon' THEN 'An energetic potato who vibrates constantly and makes excellent coffee.'
        ELSE 'A tiny spark that dreams of becoming a mighty lightning bolt someday.'
    END
WHERE potato_type = 'lightning';

-- 5. Create unique names for LIGHT cards
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 'Radiance Lord ' || id::text
        WHEN rarity = 'legendary' THEN 'Captain Daybreak ' || id::text
        WHEN rarity = 'rare' THEN 'Glowy McShineface ' || id::text
        WHEN rarity = 'uncommon' THEN 'Shiny McGillicuddy ' || id::text
        ELSE 'Tiny Glow Chip ' || id::text
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 'A transcendent being of pure light who illuminates truth across the cosmos.'
        WHEN rarity = 'legendary' THEN 'A heroic captain who brings the dawn each day.'
        WHEN rarity = 'rare' THEN 'A perpetually glowing potato who lights up rooms.'
        WHEN rarity = 'uncommon' THEN 'A polished potato who reflects light beautifully and gives everyone a healthy glow.'
        ELSE 'A tiny glow that dreams of becoming a mighty searchlight someday.'
    END
WHERE potato_type = 'light';

-- 6. Create unique names for VOID cards
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 'Void Lord ' || id::text
        WHEN rarity = 'legendary' THEN 'Captain Blackhole ' || id::text
        WHEN rarity = 'rare' THEN 'Spooky McVoidface ' || id::text
        WHEN rarity = 'uncommon' THEN 'Shadowy McGillicuddy ' || id::text
        ELSE 'Tiny Shadow Chip ' || id::text
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 'An entity from the space between spaces who nullifies existence itself.'
        WHEN rarity = 'legendary' THEN 'A fearless captain who navigates through black holes.'
        WHEN rarity = 'rare' THEN 'A perpetually spooky potato who hides in shadows.'
        WHEN rarity = 'uncommon' THEN 'A mysterious potato who lurks in shadows and gives cryptic advice.'
        ELSE 'A tiny shadow that dreams of becoming a mighty black hole someday.'
    END
WHERE potato_type = 'void';

-- 7. Verification
SELECT 
    'Card Uniqueness Check' as check_type,
    COUNT(*) as total_cards,
    COUNT(DISTINCT name) as unique_names,
    COUNT(*) - COUNT(DISTINCT name) as duplicates
FROM card_complete;

SELECT 
    'Elemental Distribution' as check_type,
    potato_type,
    COUNT(*) as count
FROM card_complete
GROUP BY potato_type
ORDER BY count DESC;