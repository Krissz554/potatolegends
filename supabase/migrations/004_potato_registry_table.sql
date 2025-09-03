-- =====================================================
-- POTATO REGISTRY TABLE
-- =====================================================
-- Store all available/discoverable potatoes in the database

-- Create the potato registry table
CREATE TABLE IF NOT EXISTS public.potato_registry (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  registry_id TEXT UNIQUE NOT NULL, -- e.g., "potato_0001", "potato_0002"
  
  -- Core potato attributes
  name TEXT NOT NULL,
  adjective TEXT NOT NULL,
  potato_type TEXT NOT NULL,
  trait TEXT NOT NULL,
  rarity TEXT NOT NULL CHECK (rarity IN ('common', 'uncommon', 'rare', 'legendary', 'exotic')),
  description TEXT NOT NULL, -- Full "You are a..." sentence
  
  -- Visual/Generation attributes
  palette_name TEXT NOT NULL,
  variation_index INTEGER NOT NULL DEFAULT 0,
  
  -- Generation parameters for pixel art
  generation_seed TEXT, -- Deterministic seed for this specific potato
  pixel_art_data JSONB, -- Store any specific visual parameters
  
  -- Metadata
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  sort_order INTEGER, -- For consistent ordering in registry
  
  -- Unique constraint to prevent duplicates
  UNIQUE(adjective, potato_type, trait, rarity)
);

-- Create the potato unlocks junction table
CREATE TABLE IF NOT EXISTS public.potato_unlocks (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL,
  potato_id UUID REFERENCES public.potato_registry(id) ON DELETE CASCADE NOT NULL,
  
  -- Unlock details
  unlocked_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  discovered_seed TEXT NOT NULL, -- The actual seed that unlocked this potato
  is_favorite BOOLEAN DEFAULT FALSE,
  view_count INTEGER DEFAULT 0,
  
  -- Prevent duplicate unlocks
  UNIQUE(user_id, potato_id)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_potato_registry_rarity ON public.potato_registry(rarity);
CREATE INDEX IF NOT EXISTS idx_potato_registry_sort_order ON public.potato_registry(sort_order);
CREATE INDEX IF NOT EXISTS idx_potato_registry_registry_id ON public.potato_registry(registry_id);

CREATE INDEX IF NOT EXISTS idx_potato_unlocks_user_id ON public.potato_unlocks(user_id);
CREATE INDEX IF NOT EXISTS idx_potato_unlocks_potato_id ON public.potato_unlocks(potato_id);
CREATE INDEX IF NOT EXISTS idx_potato_unlocks_unlocked_at ON public.potato_unlocks(unlocked_at);

-- Enable Row Level Security
ALTER TABLE public.potato_registry ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.potato_unlocks ENABLE ROW LEVEL SECURITY;

-- RLS Policies for potato_registry - everyone can read the registry
CREATE POLICY "Anyone can view potato registry" ON public.potato_registry
  FOR SELECT USING (true);

-- RLS Policies for potato_unlocks - users can only access their own unlocks
CREATE POLICY "Users can view their own unlocks" ON public.potato_unlocks
  FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own unlocks" ON public.potato_unlocks
  FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update their own unlocks" ON public.potato_unlocks
  FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Users can delete their own unlocks" ON public.potato_unlocks
  FOR DELETE USING (auth.uid() = user_id);

-- Create a view for easy registry queries with unlock status
CREATE OR REPLACE VIEW public.potato_registry_with_unlocks AS
SELECT 
  pr.*,
  CASE 
    WHEN pu.id IS NOT NULL THEN true 
    ELSE false 
  END as is_unlocked,
  pu.unlocked_at,
  pu.discovered_seed,
  pu.is_favorite,
  pu.view_count
FROM public.potato_registry pr
LEFT JOIN public.potato_unlocks pu ON pr.id = pu.potato_id 
  AND pu.user_id = auth.uid();

-- Success message
SELECT 'Potato registry database tables created successfully! 🥔📚' as status;