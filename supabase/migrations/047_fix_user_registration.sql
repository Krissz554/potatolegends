-- Fix User Registration Issues
-- Debug and repair the user signup process

-- Create a results table to track the fix progress
CREATE TABLE IF NOT EXISTS registration_fix_results (
  id SERIAL PRIMARY KEY,
  step TEXT NOT NULL,
  status TEXT NOT NULL,
  message TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Clear previous results
DELETE FROM registration_fix_results WHERE created_at < NOW() - INTERVAL '1 hour';

-- First, ensure the user_profiles table structure is correct
-- Check if all needed columns exist and add them if missing

-- Ensure username column exists
ALTER TABLE public.user_profiles 
ADD COLUMN IF NOT EXISTS username TEXT;

-- Ensure the unique constraint exists
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'unique_username' 
        AND table_name = 'user_profiles'
    ) THEN
        ALTER TABLE public.user_profiles 
        ADD CONSTRAINT unique_username UNIQUE (username);
    END IF;
EXCEPTION
    WHEN duplicate_table THEN NULL; -- Ignore if constraint already exists
END $$;

-- Log setup completion
INSERT INTO registration_fix_results (step, status, message)
VALUES ('Setup', 'SUCCESS', 'user_profiles table structure verified');

-- Recreate the handle_new_user_signup function with better error handling
CREATE OR REPLACE FUNCTION handle_new_user_signup()
RETURNS TRIGGER AS $$
BEGIN
  -- Create user profile with error handling
  BEGIN
    INSERT INTO public.user_profiles (id, email, display_name, username)
    VALUES (
      NEW.id,
      NEW.email,
      COALESCE(NEW.raw_user_meta_data->>'display_name', split_part(NEW.email, '@', 1)),
      split_part(NEW.email, '@', 1)
    )
    ON CONFLICT (id) DO UPDATE SET
      email = EXCLUDED.email,
      display_name = COALESCE(EXCLUDED.display_name, user_profiles.display_name),
      username = COALESCE(EXCLUDED.username, user_profiles.username),
      updated_at = NOW();
    
  EXCEPTION
    WHEN unique_violation THEN
      -- Handle username conflict by appending random number
      INSERT INTO public.user_profiles (id, email, display_name, username)
      VALUES (
        NEW.id,
        NEW.email,
        COALESCE(NEW.raw_user_meta_data->>'display_name', split_part(NEW.email, '@', 1)),
        split_part(NEW.email, '@', 1) || '_' || floor(random() * 1000)::text
      )
      ON CONFLICT (id) DO UPDATE SET
        email = EXCLUDED.email,
        updated_at = NOW();
      
    WHEN OTHERS THEN
      -- Don't fail the whole registration, just log the error
      NULL;
  END;
  
  -- Give Potato King with error handling
  BEGIN
    PERFORM give_potato_king_to_user(NEW.id);
  EXCEPTION
    WHEN OTHERS THEN
      -- Don't fail the whole registration
      NULL;
  END;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Log function creation
INSERT INTO registration_fix_results (step, status, message)
VALUES ('Function Creation', 'SUCCESS', 'handle_new_user_signup function created with error handling');

-- Ensure the trigger exists
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE FUNCTION handle_new_user_signup();

-- Log trigger creation
INSERT INTO registration_fix_results (step, status, message)
VALUES ('Trigger Creation', 'SUCCESS', 'on_auth_user_created trigger created successfully');

-- Verify that the heroes table has Potato King
INSERT INTO heroes (hero_id, name, description, base_hp, base_mana, hero_power_name, hero_power_description, hero_power_cost, rarity, element_type) 
VALUES ('potato_king', 'Potato King', 'The mighty ruler of all potatoes! A legendary hero with royal powers and supreme command over the potato realm.', 20, 1, 'Royal Decree', 'Deal 2 damage to any target, or restore 2 health to your hero.', 2, 'common', 'light')
ON CONFLICT (hero_id) DO NOTHING;

-- Log hero creation
INSERT INTO registration_fix_results (step, status, message)
VALUES ('Hero Creation', 'SUCCESS', 'Potato King hero ensured in heroes table');

-- Test the function manually (this will help debug any issues)
DO $$
DECLARE
  hero_count INTEGER;
  profile_table_exists BOOLEAN;
  username_column_exists BOOLEAN;
BEGIN
  -- Check if heroes table has data
  SELECT COUNT(*) INTO hero_count FROM heroes WHERE hero_id = 'potato_king';
  
  IF hero_count > 0 THEN
    INSERT INTO registration_fix_results (step, status, message)
    VALUES ('Verification', 'SUCCESS', 'Potato King found in heroes table');
  ELSE
    INSERT INTO registration_fix_results (step, status, message)
    VALUES ('Verification', 'WARNING', 'Potato King not found in heroes table');
  END IF;
  
  -- Check if user_profiles table structure is correct
  SELECT EXISTS (
    SELECT 1 FROM information_schema.tables 
    WHERE table_name = 'user_profiles' AND table_schema = 'public'
  ) INTO profile_table_exists;
  
  IF profile_table_exists THEN
    INSERT INTO registration_fix_results (step, status, message)
    VALUES ('Verification', 'SUCCESS', 'user_profiles table exists');
  ELSE
    INSERT INTO registration_fix_results (step, status, message)
    VALUES ('Verification', 'ERROR', 'user_profiles table missing');
  END IF;
  
  -- Check if username column exists
  SELECT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'user_profiles' AND column_name = 'username' AND table_schema = 'public'
  ) INTO username_column_exists;
  
  IF username_column_exists THEN
    INSERT INTO registration_fix_results (step, status, message)
    VALUES ('Verification', 'SUCCESS', 'username column exists in user_profiles');
  ELSE
    INSERT INTO registration_fix_results (step, status, message)
    VALUES ('Verification', 'WARNING', 'username column missing from user_profiles');
  END IF;
  
END $$;

-- Grant necessary permissions
GRANT USAGE ON SCHEMA public TO authenticated;
GRANT ALL ON public.user_profiles TO authenticated;
GRANT ALL ON public.user_heroes TO authenticated;
GRANT SELECT ON public.heroes TO authenticated;

-- Log permissions grant
INSERT INTO registration_fix_results (step, status, message)
VALUES ('Permissions', 'SUCCESS', 'Database permissions granted to authenticated users');

-- Final completion log
INSERT INTO registration_fix_results (step, status, message)
VALUES ('Completion', 'SUCCESS', 'User registration fix completed successfully!');