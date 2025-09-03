-- =====================================================
-- POTATO PERSONALITY GENERATOR - BASIC AUTH SETUP
-- =====================================================
-- Simplified migration for basic user authentication only
-- Run this in your Supabase SQL Editor to set up user accounts

-- Enable necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =====================================================
-- 1. USER PROFILES TABLE (BASIC)
-- =====================================================
-- Simple user profiles table that extends auth.users
CREATE TABLE IF NOT EXISTS public.user_profiles (
  id UUID REFERENCES auth.users(id) ON DELETE CASCADE PRIMARY KEY,
  email TEXT NOT NULL,
  display_name TEXT,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- 2. INDEXES FOR PERFORMANCE
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_user_profiles_email ON public.user_profiles(email);
CREATE INDEX IF NOT EXISTS idx_user_profiles_created_at ON public.user_profiles(created_at);

-- =====================================================
-- 3. ROW LEVEL SECURITY (RLS) POLICIES
-- =====================================================
-- Enable RLS on user profiles table
ALTER TABLE public.user_profiles ENABLE ROW LEVEL SECURITY;

-- User Profiles Policies - users can only access their own profile
CREATE POLICY "Users can view their own profile" ON public.user_profiles
  FOR SELECT USING (auth.uid() = id);

CREATE POLICY "Users can update their own profile" ON public.user_profiles
  FOR UPDATE USING (auth.uid() = id);

CREATE POLICY "Users can insert their own profile" ON public.user_profiles
  FOR INSERT WITH CHECK (auth.uid() = id);

-- =====================================================
-- 4. AUTOMATIC USER PROFILE CREATION
-- =====================================================
-- Function to automatically create user profile when user signs up
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO public.user_profiles (id, email, display_name)
  VALUES (
    NEW.id,
    NEW.email,
    SPLIT_PART(NEW.email, '@', 1)  -- Use email prefix as default display name
  );
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger to automatically create profile for new users
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- =====================================================
-- BASIC AUTH SETUP COMPLETE! 🥔
-- =====================================================

-- Summary of what was created:
-- ✅ Basic user profiles table
-- ✅ Row Level Security for data protection  
-- ✅ Automatic profile creation on user signup
-- ✅ Performance indexes
-- ✅ Secure policies for user data access

SELECT 'Basic authentication setup completed successfully! 🥔✨' as status;