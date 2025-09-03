-- Simplified Hero System - Only Potato King
-- Clean setup with proper hero abilities

-- Drop existing functions first to avoid conflicts
DROP FUNCTION IF EXISTS get_user_active_hero(uuid);
DROP FUNCTION IF EXISTS give_potato_king_to_user(uuid);
DROP FUNCTION IF EXISTS update_hero_battle_stats(uuid, text, boolean);
DROP FUNCTION IF EXISTS unlock_starter_heroes_for_user(uuid);
DROP FUNCTION IF EXISTS handle_new_user_signup();

-- Drop existing tables to start fresh
DROP TABLE IF EXISTS user_heroes CASCADE;
DROP TABLE IF EXISTS heroes CASCADE;

-- Create heroes table (simplified)
CREATE TABLE heroes (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  hero_id TEXT UNIQUE NOT NULL,
  name TEXT NOT NULL,
  description TEXT,
  base_hp INTEGER NOT NULL DEFAULT 20,
  base_mana INTEGER NOT NULL DEFAULT 1,
  hero_power_name TEXT,
  hero_power_description TEXT,
  hero_power_cost INTEGER DEFAULT 2,
  rarity TEXT NOT NULL DEFAULT 'common',
  element_type TEXT DEFAULT 'light',
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Create user_heroes table (tracks user's hero stats)
CREATE TABLE user_heroes (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  hero_id TEXT NOT NULL REFERENCES heroes(hero_id) ON DELETE CASCADE,
  is_active BOOLEAN DEFAULT true, -- Only one hero for now, always active
  total_wins INTEGER DEFAULT 0,
  total_losses INTEGER DEFAULT 0,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  
  UNIQUE(user_id, hero_id)
);

-- Create indexes
CREATE INDEX idx_user_heroes_user_id ON user_heroes(user_id);
CREATE INDEX idx_user_heroes_active ON user_heroes(user_id, is_active);

-- Enable RLS
ALTER TABLE heroes ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_heroes ENABLE ROW LEVEL SECURITY;

-- RLS Policies - Heroes are public, user_heroes are private
CREATE POLICY "Heroes are viewable by everyone" ON heroes
  FOR SELECT USING (true);

CREATE POLICY "Users can view their own heroes" ON user_heroes
  FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can update their own heroes" ON user_heroes
  FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own heroes" ON user_heroes
  FOR INSERT WITH CHECK (auth.uid() = user_id);

-- Insert only Potato King
INSERT INTO heroes (hero_id, name, description, base_hp, base_mana, hero_power_name, hero_power_description, hero_power_cost, rarity, element_type) VALUES
  ('potato_king', 'Potato King', 'The mighty ruler of all potatoes! A legendary hero with royal powers and supreme command over the potato realm.', 20, 1, 'Royal Decree', 'Deal 2 damage to any target, or restore 2 health to your hero.', 2, 'common', 'light');

-- Function to give Potato King to user
CREATE OR REPLACE FUNCTION give_potato_king_to_user(user_uuid UUID)
RETURNS VOID AS $$
BEGIN
  -- Give the user Potato King
  INSERT INTO user_heroes (user_id, hero_id, is_active)
  VALUES (user_uuid, 'potato_king', true)
  ON CONFLICT (user_id, hero_id) DO UPDATE SET
    is_active = true,
    updated_at = NOW();
  
  RAISE NOTICE 'Potato King given to user %', user_uuid;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to update hero battle stats
CREATE OR REPLACE FUNCTION update_hero_battle_stats(user_uuid UUID, hero_id_param TEXT, won BOOLEAN)
RETURNS VOID AS $$
BEGIN
  IF won THEN
    UPDATE user_heroes 
    SET total_wins = total_wins + 1, updated_at = NOW()
    WHERE user_id = user_uuid AND hero_id = hero_id_param;
  ELSE
    UPDATE user_heroes 
    SET total_losses = total_losses + 1, updated_at = NOW()
    WHERE user_id = user_uuid AND hero_id = hero_id_param;
  END IF;
  
  RAISE NOTICE 'Updated battle stats for user % hero % (won: %)', user_uuid, hero_id_param, won;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Function to get user's active hero
CREATE OR REPLACE FUNCTION get_user_active_hero(user_uuid UUID)
RETURNS TABLE(
  hero_id TEXT,
  name TEXT,
  description TEXT,
  base_hp INTEGER,
  base_mana INTEGER,
  hero_power_name TEXT,
  hero_power_description TEXT,
  hero_power_cost INTEGER,
  rarity TEXT,
  element_type TEXT,
  total_wins INTEGER,
  total_losses INTEGER,
  total_games INTEGER,
  win_rate NUMERIC
) AS $$
BEGIN
  RETURN QUERY
  SELECT 
    h.hero_id,
    h.name,
    h.description,
    h.base_hp,
    h.base_mana,
    h.hero_power_name,
    h.hero_power_description,
    h.hero_power_cost,
    h.rarity,
    h.element_type,
    uh.total_wins,
    uh.total_losses,
    (uh.total_wins + uh.total_losses) as total_games,
    CASE 
      WHEN (uh.total_wins + uh.total_losses) = 0 THEN 0
      ELSE ROUND((uh.total_wins::NUMERIC / (uh.total_wins + uh.total_losses)) * 100, 1)
    END as win_rate
  FROM user_heroes uh
  JOIN heroes h ON uh.hero_id = h.hero_id
  WHERE uh.user_id = user_uuid AND uh.is_active = true
  LIMIT 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Give Potato King to all existing users
INSERT INTO user_heroes (user_id, hero_id, is_active)
SELECT up.id, 'potato_king', true
FROM user_profiles up
ON CONFLICT (user_id, hero_id) DO UPDATE SET
  is_active = true,
  updated_at = NOW();

-- Update the user signup trigger to give Potato King
CREATE OR REPLACE FUNCTION handle_new_user_signup()
RETURNS TRIGGER AS $$
BEGIN
  -- Create user profile
  INSERT INTO public.user_profiles (id, email, display_name, username)
  VALUES (
    NEW.id,
    NEW.email,
    COALESCE(NEW.raw_user_meta_data->>'display_name', split_part(NEW.email, '@', 1)),
    split_part(NEW.email, '@', 1)
  )
  ON CONFLICT (id) DO NOTHING;
  
  -- Give Potato King
  PERFORM give_potato_king_to_user(NEW.id);
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Update trigger
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE FUNCTION handle_new_user_signup();

-- Verify setup
SELECT 'Total heroes:' as info, count(*) as count FROM heroes;
SELECT 'Total user heroes:' as info, count(*) as count FROM user_heroes;
SELECT 'Users with Potato King:' as info, count(*) as count FROM user_heroes WHERE hero_id = 'potato_king';