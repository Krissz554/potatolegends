-- =====================================================
-- ADD USERNAME SUPPORT TO USER PROFILES
-- =====================================================

-- Add username column to user_profiles table
ALTER TABLE public.user_profiles 
ADD COLUMN IF NOT EXISTS username TEXT;

-- Add unique constraint for usernames (only if it doesn't exist)
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
END $$;

-- Create index for faster username lookups
CREATE INDEX IF NOT EXISTS idx_user_profiles_username ON public.user_profiles (username);

-- Add updated_at column if it doesn't exist
ALTER TABLE public.user_profiles 
ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW();

-- Update RLS policies to allow users to update their own usernames (only if doesn't exist)
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_policies 
        WHERE tablename = 'user_profiles' 
        AND policyname = 'Users can update their own profile'
    ) THEN
        CREATE POLICY "Users can update their own profile" ON public.user_profiles
        FOR UPDATE USING (auth.uid() = id);
    END IF;
END $$;

-- Success message
SELECT 'Username support added to user_profiles successfully!' as status;