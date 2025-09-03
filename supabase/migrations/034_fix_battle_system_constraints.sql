-- Migration 034: Fix Battle System Constraints and Ensure Data Integrity
-- This migration fixes database constraints and ensures all tables have proper structure

-- Ensure user_deck_settings table exists for storing active deck slots
DO $$ BEGIN
  CREATE TABLE IF NOT EXISTS user_deck_settings (
    id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
    user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL UNIQUE,
    active_deck_slot INTEGER DEFAULT 1 CHECK (active_deck_slot >= 1 AND active_deck_slot <= 3),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
  );
EXCEPTION
  WHEN duplicate_table THEN NULL;
END $$;

-- Add indexes for performance
DO $$ BEGIN
  CREATE INDEX IF NOT EXISTS idx_user_deck_settings_user_id ON user_deck_settings(user_id);
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

-- Ensure battle_decks table has proper constraints
DO $$ BEGIN
  -- Add deck_slot column if it doesn't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'battle_decks' AND column_name = 'deck_slot') THEN
    ALTER TABLE battle_decks ADD COLUMN deck_slot INTEGER DEFAULT 1 CHECK (deck_slot >= 1 AND deck_slot <= 3);
  END IF;
EXCEPTION
  WHEN others THEN NULL;
END $$;

-- Add indexes for battle_decks performance
DO $$ BEGIN
  CREATE INDEX IF NOT EXISTS idx_battle_decks_user_deck ON battle_decks(user_id, deck_slot);
  CREATE INDEX IF NOT EXISTS idx_battle_decks_position ON battle_decks(deck_position);
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

-- Ensure potato_unlocks table has all required columns
DO $$ BEGIN
  -- Add is_favorite column if it doesn't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'potato_unlocks' AND column_name = 'is_favorite') THEN
    ALTER TABLE potato_unlocks ADD COLUMN is_favorite BOOLEAN DEFAULT FALSE;
  END IF;
  
  -- Add view_count column if it doesn't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'potato_unlocks' AND column_name = 'view_count') THEN
    ALTER TABLE potato_unlocks ADD COLUMN view_count INTEGER DEFAULT 0;
  END IF;
EXCEPTION
  WHEN others THEN NULL;
END $$;

-- Create or replace function to ensure user has basic potatoes
CREATE OR REPLACE FUNCTION ensure_user_has_basic_potatoes(user_uuid UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  unlocked_count INTEGER;
  basic_potato RECORD;
BEGIN
  -- Check if user already has unlocked potatoes
  SELECT COUNT(*) INTO unlocked_count
  FROM potato_unlocks
  WHERE user_id = user_uuid;
  
  -- If user already has potatoes, return true
  IF unlocked_count > 0 THEN
    RETURN TRUE;
  END IF;
  
  -- Auto-unlock first 5 potatoes from registry
  FOR basic_potato IN 
    SELECT id FROM potato_registry 
    ORDER BY sort_order, id 
    LIMIT 5
  LOOP
    INSERT INTO potato_unlocks (user_id, potato_id, discovered_seed)
    VALUES (
      user_uuid, 
      basic_potato.id, 
      'auto-unlock-' || extract(epoch from now()) || '-' || substr(md5(random()::text), 1, 6)
    )
    ON CONFLICT (user_id, potato_id) DO NOTHING;
  END LOOP;
  
  RETURN TRUE;
END;
$$;

-- Create or replace function to ensure user has a battle deck
CREATE OR REPLACE FUNCTION ensure_user_battle_deck(user_uuid UUID, deck_slot_num INTEGER DEFAULT 1)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
  deck_count INTEGER;
  unlocked_potato RECORD;
  deck_position INTEGER := 1;
BEGIN
  -- Validate deck slot
  IF deck_slot_num < 1 OR deck_slot_num > 3 THEN
    RAISE EXCEPTION 'Invalid deck slot. Must be 1, 2, or 3.';
  END IF;
  
  -- Check if user already has a deck in this slot
  SELECT COUNT(*) INTO deck_count
  FROM battle_decks
  WHERE user_id = user_uuid AND deck_slot = deck_slot_num;
  
  -- If deck already exists, return true
  IF deck_count >= 5 THEN
    RETURN TRUE;
  END IF;
  
  -- Ensure user has basic potatoes first
  PERFORM ensure_user_has_basic_potatoes(user_uuid);
  
  -- Clear existing deck in this slot
  DELETE FROM battle_decks 
  WHERE user_id = user_uuid AND deck_slot = deck_slot_num;
  
  -- Create new deck with first 5 unlocked potatoes
  FOR unlocked_potato IN 
    SELECT pu.potato_id
    FROM potato_unlocks pu
    JOIN potato_registry pr ON pu.potato_id = pr.id
    WHERE pu.user_id = user_uuid
    ORDER BY pr.sort_order, pu.unlocked_at
    LIMIT 5
  LOOP
    INSERT INTO battle_decks (user_id, potato_id, deck_position, deck_slot)
    VALUES (user_uuid, unlocked_potato.potato_id, deck_position, deck_slot_num);
    
    deck_position := deck_position + 1;
  END LOOP;
  
  RETURN TRUE;
END;
$$;

-- Create or replace function to set user's active deck slot
CREATE OR REPLACE FUNCTION set_user_active_deck_slot(user_uuid UUID, slot_num INTEGER)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
  -- Validate slot number
  IF slot_num < 1 OR slot_num > 3 THEN
    RAISE EXCEPTION 'Invalid deck slot. Must be 1, 2, or 3.';
  END IF;
  
  -- Insert or update user deck settings
  INSERT INTO user_deck_settings (user_id, active_deck_slot)
  VALUES (user_uuid, slot_num)
  ON CONFLICT (user_id) 
  DO UPDATE SET 
    active_deck_slot = slot_num,
    updated_at = NOW();
  
  RETURN TRUE;
END;
$$;

-- Grant execute permissions to authenticated users and service role
GRANT EXECUTE ON FUNCTION ensure_user_has_basic_potatoes(UUID) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION ensure_user_battle_deck(UUID, INTEGER) TO authenticated, service_role;
GRANT EXECUTE ON FUNCTION set_user_active_deck_slot(UUID, INTEGER) TO authenticated, service_role;

-- Update RLS policies for new tables
DROP POLICY IF EXISTS "Users can manage their own deck settings" ON user_deck_settings;
CREATE POLICY "Users can manage their own deck settings"
  ON user_deck_settings FOR ALL
  USING (auth.uid() = user_id);

-- Ensure service_role can access all tables for edge functions
DROP POLICY IF EXISTS "Service role full access to battle_decks" ON battle_decks;
CREATE POLICY "Service role full access to battle_decks"
  ON battle_decks FOR ALL
  TO service_role
  USING (true);

DROP POLICY IF EXISTS "Service role full access to potato_unlocks" ON potato_unlocks;
CREATE POLICY "Service role full access to potato_unlocks"
  ON potato_unlocks FOR ALL
  TO service_role
  USING (true);

DROP POLICY IF EXISTS "Service role full access to user_deck_settings" ON user_deck_settings;
CREATE POLICY "Service role full access to user_deck_settings"
  ON user_deck_settings FOR ALL
  TO service_role
  USING (true);

-- Enable RLS on user_deck_settings
ALTER TABLE user_deck_settings ENABLE ROW LEVEL SECURITY;

-- Add helpful comments
COMMENT ON TABLE user_deck_settings IS 'Stores user preferences for active deck slots';
COMMENT ON FUNCTION ensure_user_has_basic_potatoes(UUID) IS 'Auto-unlocks basic potatoes for new users';
COMMENT ON FUNCTION ensure_user_battle_deck(UUID, INTEGER) IS 'Creates a battle deck for the user in the specified slot';
COMMENT ON FUNCTION set_user_active_deck_slot(UUID, INTEGER) IS 'Sets the user''s active deck slot preference';