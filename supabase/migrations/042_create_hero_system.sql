-- Create hero system for battles
-- Users can unlock heroes and select which one to use in battles

-- Create heroes table (master list of all available heroes)
CREATE TABLE IF NOT EXISTS heroes (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  hero_id TEXT UNIQUE NOT NULL, -- e.g. 'basic_potato', 'warrior_potato'
  name TEXT NOT NULL,
  description TEXT,
  base_hp INTEGER NOT NULL DEFAULT 30,
  base_mana INTEGER NOT NULL DEFAULT 1,
  hero_power_id TEXT, -- Reference to hero power
  hero_power_name TEXT,
  hero_power_description TEXT,
  hero_power_cost INTEGER DEFAULT 2,
  rarity TEXT NOT NULL DEFAULT 'common', -- common, rare, epic, legendary
  element_type TEXT, -- fire, ice, lightning, light, void
  unlock_condition TEXT, -- How to unlock this hero
  is_starter BOOLEAN DEFAULT false, -- Starter heroes given to all users
  art_url TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Create user_heroes table (which heroes each user has unlocked)
CREATE TABLE IF NOT EXISTS user_heroes (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  hero_id TEXT NOT NULL REFERENCES heroes(hero_id) ON DELETE CASCADE,
  unlocked_at TIMESTAMPTZ DEFAULT NOW(),
  is_active BOOLEAN DEFAULT false, -- Which hero is currently selected for battles
  total_wins INTEGER DEFAULT 0,
  total_losses INTEGER DEFAULT 0,
  total_games INTEGER DEFAULT 0,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  
  UNIQUE(user_id, hero_id)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_user_heroes_user_id ON user_heroes(user_id);
CREATE INDEX IF NOT EXISTS idx_user_heroes_active ON user_heroes(user_id, is_active);
CREATE INDEX IF NOT EXISTS idx_heroes_starter ON heroes(is_starter);
CREATE INDEX IF NOT EXISTS idx_heroes_rarity ON heroes(rarity);

-- Enable RLS
ALTER TABLE heroes ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_heroes ENABLE ROW LEVEL SECURITY;

-- RLS Policies
-- Heroes table - readable by everyone (public hero list)
CREATE POLICY "Heroes are viewable by everyone" ON heroes
  FOR SELECT USING (true);

-- User heroes - users can only see their own heroes
CREATE POLICY "Users can view their own heroes" ON user_heroes
  FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can update their own heroes" ON user_heroes
  FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own heroes" ON user_heroes
  FOR INSERT WITH CHECK (auth.uid() = user_id);

-- Insert starter heroes
INSERT INTO heroes (hero_id, name, description, base_hp, base_mana, hero_power_name, hero_power_description, hero_power_cost, rarity, element_type, is_starter) VALUES
  ('potato_king', 'Potato King', 'The mighty ruler of all potatoes! A legendary hero with royal powers and supreme command over the potato realm.', 30, 1, 'Royal Decree', 'Deal 2 damage to any target, or restore 2 health to your hero.', 2, 'legendary', 'light', true),
  ('fire_spud', 'Fire Spud Warrior', 'A fierce potato warrior wielding the power of flames. Burns enemies with fiery attacks.', 30, 1, 'Flame Burst', 'Deal 2 damage to an enemy. If it dies, deal 1 damage to all enemies.', 2, 'rare', 'fire', false),
  ('ice_tater', 'Ice Tater Guardian', 'A cool and collected potato guardian with mastery over ice magic.', 30, 1, 'Frost Shield', 'Gain 5 armor. Draw a card.', 2, 'rare', 'ice', false),
  ('lightning_tuber', 'Lightning Tuber', 'A high-energy potato crackling with electrical power.', 25, 1, 'Chain Lightning', 'Deal 3 damage to an enemy. If you have 5+ mana, deal 1 damage to all enemies.', 3, 'epic', 'lightning', false),
  ('light_potato', 'Radiant Light Potato', 'A holy potato blessed with divine light energy.', 35, 1, 'Holy Light', 'Restore 4 health to your hero and draw a card.', 2, 'epic', 'light', false)
ON CONFLICT (hero_id) DO NOTHING;

-- Function to unlock starter heroes for new users
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

-- Function to set active hero for user
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
  total_games INTEGER
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
    uh.total_games
  FROM user_heroes uh
  JOIN heroes h ON uh.hero_id = h.hero_id
  WHERE uh.user_id = user_uuid AND uh.is_active = true
  LIMIT 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Update the user signup handler to include hero unlocking
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
  
  -- Unlock starter heroes
  PERFORM unlock_starter_heroes_for_user(NEW.id);
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Update trigger (if it doesn't exist, create it)
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE FUNCTION handle_new_user_signup();