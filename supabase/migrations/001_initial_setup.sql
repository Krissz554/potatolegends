-- =====================================================
-- POTATO PERSONALITY GENERATOR - DATABASE MIGRATION
-- =====================================================
-- Run this in your Supabase SQL Editor to set up the complete database
-- for user accounts and potato collections

-- Enable necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =====================================================
-- 1. USER PROFILES TABLE
-- =====================================================
-- Extends the default auth.users with additional profile information
CREATE TABLE IF NOT EXISTS public.user_profiles (
  id UUID REFERENCES auth.users(id) ON DELETE CASCADE PRIMARY KEY,
  email TEXT NOT NULL,
  display_name TEXT,
  avatar_url TEXT,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  -- Potato-specific user stats
  total_potatoes_generated INTEGER DEFAULT 0,
  favorite_rarity TEXT DEFAULT 'common',
  last_generated_at TIMESTAMP WITH TIME ZONE,
  -- User preferences
  preferred_palette TEXT DEFAULT 'orange',
  email_notifications BOOLEAN DEFAULT TRUE
);

-- =====================================================
-- 2. POTATO COLLECTIONS TABLE
-- =====================================================
-- Stores all generated potatoes that users want to save
CREATE TABLE IF NOT EXISTS public.potato_collections (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  
  -- Potato Generation Data
  seed TEXT NOT NULL,
  potato_name TEXT NOT NULL,
  rarity TEXT NOT NULL CHECK (rarity IN ('common', 'uncommon', 'rare', 'legendary', 'exotic')),
  adjective TEXT NOT NULL,
  potato_type TEXT NOT NULL,
  trait TEXT NOT NULL,
  full_description TEXT NOT NULL,
  palette_name TEXT NOT NULL,
  
  -- Metadata
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  is_favorite BOOLEAN DEFAULT FALSE,
  view_count INTEGER DEFAULT 0,
  shared_count INTEGER DEFAULT 0,
  
  -- Make sure each user can only save a potato with a specific seed once
  UNIQUE(user_id, seed)
);

-- =====================================================
-- 3. POTATO SHARES TABLE
-- =====================================================
-- Track when users share potatoes (for analytics and social features)
CREATE TABLE IF NOT EXISTS public.potato_shares (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  potato_id UUID REFERENCES public.potato_collections(id) ON DELETE CASCADE NOT NULL,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  share_method TEXT NOT NULL CHECK (share_method IN ('link', 'download', 'social', 'email')),
  shared_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  shared_to TEXT, -- Optional: email, social platform, etc.
  
  -- Analytics data
  ip_address INET,
  user_agent TEXT
);

-- =====================================================
-- 4. POTATO RATINGS TABLE
-- =====================================================
-- Allow users to rate potatoes (for future recommendation features)
CREATE TABLE IF NOT EXISTS public.potato_ratings (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  potato_id UUID REFERENCES public.potato_collections(id) ON DELETE CASCADE NOT NULL,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  
  -- Each user can only rate a potato once
  UNIQUE(potato_id, user_id)
);

-- =====================================================
-- 5. USER ACHIEVEMENTS TABLE
-- =====================================================
-- Track user achievements and milestones
CREATE TABLE IF NOT EXISTS public.user_achievements (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  achievement_type TEXT NOT NULL,
  achievement_name TEXT NOT NULL,
  description TEXT,
  earned_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  
  -- Metadata for the achievement
  metadata JSONB DEFAULT '{}',
  
  -- Prevent duplicate achievements
  UNIQUE(user_id, achievement_type)
);

-- =====================================================
-- 6. INDEXES FOR PERFORMANCE
-- =====================================================

-- User profiles indexes
CREATE INDEX IF NOT EXISTS idx_user_profiles_email ON public.user_profiles(email);
CREATE INDEX IF NOT EXISTS idx_user_profiles_created_at ON public.user_profiles(created_at);

-- Potato collections indexes
CREATE INDEX IF NOT EXISTS idx_potato_collections_user_id ON public.potato_collections(user_id);
CREATE INDEX IF NOT EXISTS idx_potato_collections_created_at ON public.potato_collections(created_at);
CREATE INDEX IF NOT EXISTS idx_potato_collections_rarity ON public.potato_collections(rarity);
CREATE INDEX IF NOT EXISTS idx_potato_collections_is_favorite ON public.potato_collections(is_favorite);
CREATE INDEX IF NOT EXISTS idx_potato_collections_seed ON public.potato_collections(seed);

-- Potato shares indexes
CREATE INDEX IF NOT EXISTS idx_potato_shares_potato_id ON public.potato_shares(potato_id);
CREATE INDEX IF NOT EXISTS idx_potato_shares_user_id ON public.potato_shares(user_id);
CREATE INDEX IF NOT EXISTS idx_potato_shares_shared_at ON public.potato_shares(shared_at);

-- Potato ratings indexes
CREATE INDEX IF NOT EXISTS idx_potato_ratings_potato_id ON public.potato_ratings(potato_id);
CREATE INDEX IF NOT EXISTS idx_potato_ratings_user_id ON public.potato_ratings(user_id);

-- User achievements indexes
CREATE INDEX IF NOT EXISTS idx_user_achievements_user_id ON public.user_achievements(user_id);
CREATE INDEX IF NOT EXISTS idx_user_achievements_type ON public.user_achievements(achievement_type);

-- =====================================================
-- 7. ROW LEVEL SECURITY (RLS) POLICIES
-- =====================================================

-- Enable RLS on all tables
ALTER TABLE public.user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.potato_collections ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.potato_shares ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.potato_ratings ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.user_achievements ENABLE ROW LEVEL SECURITY;

-- User Profiles Policies
CREATE POLICY "Users can view their own profile" ON public.user_profiles
  FOR SELECT USING (auth.uid() = id);

CREATE POLICY "Users can update their own profile" ON public.user_profiles
  FOR UPDATE USING (auth.uid() = id);

CREATE POLICY "Users can insert their own profile" ON public.user_profiles
  FOR INSERT WITH CHECK (auth.uid() = id);

-- Potato Collections Policies
CREATE POLICY "Users can view their own potatoes" ON public.potato_collections
  FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own potatoes" ON public.potato_collections
  FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update their own potatoes" ON public.potato_collections
  FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Users can delete their own potatoes" ON public.potato_collections
  FOR DELETE USING (auth.uid() = user_id);

-- Potato Shares Policies
CREATE POLICY "Users can view their own shares" ON public.potato_shares
  FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own shares" ON public.potato_shares
  FOR INSERT WITH CHECK (auth.uid() = user_id);

-- Potato Ratings Policies
CREATE POLICY "Users can view all ratings" ON public.potato_ratings
  FOR SELECT USING (true);

CREATE POLICY "Users can insert their own ratings" ON public.potato_ratings
  FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update their own ratings" ON public.potato_ratings
  FOR UPDATE USING (auth.uid() = user_id);

-- User Achievements Policies
CREATE POLICY "Users can view their own achievements" ON public.user_achievements
  FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own achievements" ON public.user_achievements
  FOR INSERT WITH CHECK (auth.uid() = user_id);

-- =====================================================
-- 8. FUNCTIONS AND TRIGGERS
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

-- Function to update user stats when potato is saved
CREATE OR REPLACE FUNCTION public.update_user_potato_stats()
RETURNS TRIGGER AS $$
BEGIN
  -- Update user's total potatoes count and last generated time
  UPDATE public.user_profiles
  SET 
    total_potatoes_generated = total_potatoes_generated + 1,
    last_generated_at = NOW(),
    updated_at = NOW()
  WHERE id = NEW.user_id;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger to update user stats when potato is added
DROP TRIGGER IF EXISTS on_potato_created ON public.potato_collections;
CREATE TRIGGER on_potato_created
  AFTER INSERT ON public.potato_collections
  FOR EACH ROW EXECUTE FUNCTION public.update_user_potato_stats();

-- Function to update share count when potato is shared
CREATE OR REPLACE FUNCTION public.update_potato_share_count()
RETURNS TRIGGER AS $$
BEGIN
  -- Update share count for the potato
  UPDATE public.potato_collections
  SET shared_count = shared_count + 1
  WHERE id = NEW.potato_id;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger to update share count
DROP TRIGGER IF EXISTS on_potato_shared ON public.potato_shares;
CREATE TRIGGER on_potato_shared
  AFTER INSERT ON public.potato_shares
  FOR EACH ROW EXECUTE FUNCTION public.update_potato_share_count();

-- Function to check and award achievements
CREATE OR REPLACE FUNCTION public.check_user_achievements()
RETURNS TRIGGER AS $$
DECLARE
  user_potato_count INTEGER;
  rare_potato_count INTEGER;
BEGIN
  -- Get user's current potato count
  SELECT total_potatoes_generated INTO user_potato_count
  FROM public.user_profiles
  WHERE id = NEW.user_id;
  
  -- Award "First Potato" achievement
  IF user_potato_count = 1 THEN
    INSERT INTO public.user_achievements (user_id, achievement_type, achievement_name, description)
    VALUES (NEW.user_id, 'first_potato', 'First Potato', 'Generated your very first potato!')
    ON CONFLICT (user_id, achievement_type) DO NOTHING;
  END IF;
  
  -- Award milestone achievements
  IF user_potato_count IN (10, 50, 100, 500, 1000) THEN
    INSERT INTO public.user_achievements (user_id, achievement_type, achievement_name, description)
    VALUES (
      NEW.user_id, 
      'potato_count_' || user_potato_count,
      user_potato_count || ' Potatoes',
      'Generated ' || user_potato_count || ' amazing potatoes!'
    )
    ON CONFLICT (user_id, achievement_type) DO NOTHING;
  END IF;
  
  -- Award rarity-based achievements
  IF NEW.rarity = 'legendary' THEN
    INSERT INTO public.user_achievements (user_id, achievement_type, achievement_name, description)
    VALUES (NEW.user_id, 'legendary_potato', 'Legendary Discovery', 'Found a legendary potato!')
    ON CONFLICT (user_id, achievement_type) DO NOTHING;
  END IF;
  
  IF NEW.rarity = 'exotic' THEN
    INSERT INTO public.user_achievements (user_id, achievement_type, achievement_name, description)
    VALUES (NEW.user_id, 'exotic_potato', 'Exotic Explorer', 'Discovered an exotic potato!')
    ON CONFLICT (user_id, achievement_type) DO NOTHING;
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger to check achievements when potato is added
DROP TRIGGER IF EXISTS on_potato_check_achievements ON public.potato_collections;
CREATE TRIGGER on_potato_check_achievements
  AFTER INSERT ON public.potato_collections
  FOR EACH ROW EXECUTE FUNCTION public.check_user_achievements();

-- =====================================================
-- 9. HELPFUL VIEWS
-- =====================================================

-- View for user dashboard statistics
CREATE OR REPLACE VIEW public.user_dashboard_stats AS
SELECT 
  up.id as user_id,
  up.display_name,
  up.total_potatoes_generated,
  up.last_generated_at,
  up.created_at as joined_at,
  COUNT(DISTINCT pc.id) as saved_potatoes,
  COUNT(DISTINCT CASE WHEN pc.is_favorite = true THEN pc.id END) as favorite_potatoes,
  COUNT(DISTINCT ua.id) as achievements_earned,
  COALESCE(SUM(pc.shared_count), 0) as total_shares,
  -- Rarity breakdown
  COUNT(DISTINCT CASE WHEN pc.rarity = 'common' THEN pc.id END) as common_potatoes,
  COUNT(DISTINCT CASE WHEN pc.rarity = 'uncommon' THEN pc.id END) as uncommon_potatoes,
  COUNT(DISTINCT CASE WHEN pc.rarity = 'rare' THEN pc.id END) as rare_potatoes,
  COUNT(DISTINCT CASE WHEN pc.rarity = 'legendary' THEN pc.id END) as legendary_potatoes,
  COUNT(DISTINCT CASE WHEN pc.rarity = 'exotic' THEN pc.id END) as exotic_potatoes
FROM public.user_profiles up
LEFT JOIN public.potato_collections pc ON up.id = pc.user_id
LEFT JOIN public.user_achievements ua ON up.id = ua.user_id
GROUP BY up.id, up.display_name, up.total_potatoes_generated, up.last_generated_at, up.created_at;

-- View for popular potatoes (most shared/favorited)
CREATE OR REPLACE VIEW public.popular_potatoes AS
SELECT 
  pc.*,
  up.display_name as creator_name,
  COALESCE(pc.shared_count, 0) + (CASE WHEN pc.is_favorite THEN 10 ELSE 0 END) as popularity_score
FROM public.potato_collections pc
JOIN public.user_profiles up ON pc.user_id = up.id
ORDER BY popularity_score DESC, pc.created_at DESC;

-- =====================================================
-- 10. SAMPLE DATA (OPTIONAL)
-- =====================================================

-- Insert some sample achievement types for reference
INSERT INTO public.user_achievements (user_id, achievement_type, achievement_name, description, earned_at)
VALUES 
  ('00000000-0000-0000-0000-000000000000', 'sample', 'Sample Achievement', 'This is just a sample for reference', NOW())
ON CONFLICT DO NOTHING;

-- Clean up the sample data
DELETE FROM public.user_achievements WHERE achievement_type = 'sample';

-- =====================================================
-- MIGRATION COMPLETE! 🥔
-- =====================================================

-- Summary of what was created:
-- ✅ User profiles table with potato-specific stats
-- ✅ Potato collections table for saved potatoes  
-- ✅ Potato shares table for analytics
-- ✅ Potato ratings table for future features
-- ✅ User achievements table for gamification
-- ✅ Performance indexes on all tables
-- ✅ Row Level Security policies for data protection
-- ✅ Automatic triggers for user creation and stats
-- ✅ Achievement system with automatic rewards
-- ✅ Helpful views for dashboard data

SELECT 'Database migration completed successfully! 🥔✨' as status;