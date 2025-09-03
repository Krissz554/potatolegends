-- Manual Hero System Setup
-- Run this SQL in Supabase Dashboard > SQL Editor if the migrations didn't work

-- First, let's check if tables exist and create them if needed
DO $$ 
BEGIN
    -- Create heroes table if it doesn't exist
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'heroes') THEN
        CREATE TABLE heroes (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            hero_id TEXT UNIQUE NOT NULL,
            name TEXT NOT NULL,
            description TEXT,
            base_hp INTEGER NOT NULL DEFAULT 30,
            base_mana INTEGER NOT NULL DEFAULT 1,
            hero_power_id TEXT,
            hero_power_name TEXT,
            hero_power_description TEXT,
            hero_power_cost INTEGER DEFAULT 2,
            rarity TEXT NOT NULL DEFAULT 'common',
            element_type TEXT,
            unlock_condition TEXT,
            is_starter BOOLEAN DEFAULT false,
            art_url TEXT,
            created_at TIMESTAMPTZ DEFAULT NOW(),
            updated_at TIMESTAMPTZ DEFAULT NOW()
        );
        
        -- Enable RLS
        ALTER TABLE heroes ENABLE ROW LEVEL SECURITY;
        
        -- Create policy for public reading
        CREATE POLICY "Heroes are viewable by everyone" ON heroes
            FOR SELECT USING (true);
            
        RAISE NOTICE 'Created heroes table';
    END IF;

    -- Create user_heroes table if it doesn't exist
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'user_heroes') THEN
        CREATE TABLE user_heroes (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
            hero_id TEXT NOT NULL,
            unlocked_at TIMESTAMPTZ DEFAULT NOW(),
            is_active BOOLEAN DEFAULT false,
            total_wins INTEGER DEFAULT 0,
            total_losses INTEGER DEFAULT 0,
            total_games INTEGER DEFAULT 0,
            created_at TIMESTAMPTZ DEFAULT NOW(),
            updated_at TIMESTAMPTZ DEFAULT NOW(),
            
            UNIQUE(user_id, hero_id)
        );
        
        -- Create indexes
        CREATE INDEX idx_user_heroes_user_id ON user_heroes(user_id);
        CREATE INDEX idx_user_heroes_active ON user_heroes(user_id, is_active);
        
        -- Enable RLS
        ALTER TABLE user_heroes ENABLE ROW LEVEL SECURITY;
        
        -- Create RLS policies
        CREATE POLICY "Users can view their own heroes" ON user_heroes
            FOR SELECT USING (auth.uid() = user_id);

        CREATE POLICY "Users can update their own heroes" ON user_heroes
            FOR UPDATE USING (auth.uid() = user_id);

        CREATE POLICY "Users can insert their own heroes" ON user_heroes
            FOR INSERT WITH CHECK (auth.uid() = user_id);
            
        RAISE NOTICE 'Created user_heroes table';
    END IF;
END $$;

-- Insert hero data (will not create duplicates)
INSERT INTO heroes (hero_id, name, description, base_hp, base_mana, hero_power_name, hero_power_description, hero_power_cost, rarity, element_type, is_starter) VALUES
    ('potato_king', 'Potato King', 'The mighty ruler of all potatoes! A legendary hero with royal powers and supreme command over the potato realm.', 30, 1, 'Royal Decree', 'Deal 2 damage to any target, or restore 2 health to your hero.', 2, 'legendary', 'light', true),
    ('fire_spud', 'Fire Spud Warrior', 'A fierce potato warrior wielding the power of flames. Burns enemies with fiery attacks.', 30, 1, 'Flame Burst', 'Deal 2 damage to an enemy. If it dies, deal 1 damage to all enemies.', 2, 'rare', 'fire', false),
    ('ice_tater', 'Ice Tater Guardian', 'A cool and collected potato guardian with mastery over ice magic.', 30, 1, 'Frost Shield', 'Gain 5 armor. Draw a card.', 2, 'rare', 'ice', false),
    ('lightning_tuber', 'Lightning Tuber', 'A high-energy potato crackling with electrical power.', 25, 1, 'Chain Lightning', 'Deal 3 damage to an enemy. If you have 5+ mana, deal 1 damage to all enemies.', 3, 'epic', 'lightning', false),
    ('light_potato', 'Radiant Light Potato', 'A holy potato blessed with divine light energy.', 35, 1, 'Holy Light', 'Restore 4 health to your hero and draw a card.', 2, 'epic', 'light', false)
ON CONFLICT (hero_id) DO UPDATE SET
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    base_hp = EXCLUDED.base_hp,
    hero_power_name = EXCLUDED.hero_power_name,
    hero_power_description = EXCLUDED.hero_power_description,
    updated_at = NOW();

-- Create the unlock function
CREATE OR REPLACE FUNCTION unlock_starter_heroes_for_user(user_uuid UUID)
RETURNS VOID AS $$
BEGIN
    -- Insert all starter heroes for the user
    INSERT INTO user_heroes (user_id, hero_id, is_active)
    SELECT 
        user_uuid,
        hero_id,
        hero_id = 'potato_king' -- Make potato_king the default active hero
    FROM heroes 
    WHERE is_starter = true
    ON CONFLICT (user_id, hero_id) DO NOTHING;
    
    RAISE NOTICE 'Starter heroes unlocked for user %', user_uuid;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create the set active hero function
CREATE OR REPLACE FUNCTION set_active_hero(user_uuid UUID, target_hero_id TEXT)
RETURNS BOOLEAN AS $$
DECLARE
    hero_exists BOOLEAN;
BEGIN
    -- Check if user owns this hero
    SELECT EXISTS(
        SELECT 1 FROM user_heroes 
        WHERE user_id = user_uuid AND hero_id = target_hero_id
    ) INTO hero_exists;
    
    IF NOT hero_exists THEN
        RAISE EXCEPTION 'User does not own hero: %', target_hero_id;
    END IF;
    
    -- Deactivate all heroes for this user
    UPDATE user_heroes 
    SET is_active = false, updated_at = NOW()
    WHERE user_id = user_uuid;
    
    -- Activate the target hero
    UPDATE user_heroes 
    SET is_active = true, updated_at = NOW()
    WHERE user_id = user_uuid AND hero_id = target_hero_id;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Unlock starter heroes for all existing users
INSERT INTO user_heroes (user_id, hero_id, is_active)
SELECT 
    up.id as user_id,
    h.hero_id,
    h.hero_id = 'potato_king' as is_active
FROM user_profiles up
CROSS JOIN heroes h
WHERE h.is_starter = true
AND NOT EXISTS (
    SELECT 1 FROM user_heroes uh 
    WHERE uh.user_id = up.id AND uh.hero_id = h.hero_id
)
ON CONFLICT (user_id, hero_id) DO NOTHING;

-- Ensure every user has at least one active hero
UPDATE user_heroes 
SET is_active = true 
WHERE user_id IN (
    SELECT DISTINCT uh.user_id 
    FROM user_heroes uh
    WHERE NOT EXISTS (
        SELECT 1 FROM user_heroes uh2 
        WHERE uh2.user_id = uh.user_id AND uh2.is_active = true
    )
)
AND hero_id = 'potato_king';

-- Verify the setup
SELECT 'Heroes created:' as status, count(*) as count FROM heroes;
SELECT 'User heroes created:' as status, count(*) as count FROM user_heroes;
SELECT 'Users with Potato King:' as status, count(*) as count FROM user_heroes WHERE hero_id = 'potato_king';

-- Show the heroes that were created
SELECT hero_id, name, rarity, element_type, is_starter FROM heroes ORDER BY is_starter DESC, rarity;