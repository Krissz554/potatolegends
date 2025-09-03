-- =====================================================
-- POTATO COLLECTIONS TABLE
-- =====================================================
-- Add potato collections table for saving user's potatoes

-- Create the potato collections table
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

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_potato_collections_user_id ON public.potato_collections(user_id);
CREATE INDEX IF NOT EXISTS idx_potato_collections_created_at ON public.potato_collections(created_at);
CREATE INDEX IF NOT EXISTS idx_potato_collections_rarity ON public.potato_collections(rarity);
CREATE INDEX IF NOT EXISTS idx_potato_collections_is_favorite ON public.potato_collections(is_favorite);
CREATE INDEX IF NOT EXISTS idx_potato_collections_seed ON public.potato_collections(seed);

-- Enable Row Level Security
ALTER TABLE public.potato_collections ENABLE ROW LEVEL SECURITY;

-- RLS Policies - users can only access their own potatoes
CREATE POLICY "Users can view their own potatoes" ON public.potato_collections
  FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own potatoes" ON public.potato_collections
  FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update their own potatoes" ON public.potato_collections
  FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Users can delete their own potatoes" ON public.potato_collections
  FOR DELETE USING (auth.uid() = user_id);

-- Success message
SELECT 'Potato collections table created successfully! 🥔✨' as status;