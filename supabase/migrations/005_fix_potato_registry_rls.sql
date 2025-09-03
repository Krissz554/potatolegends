-- =====================================================
-- FIX POTATO REGISTRY RLS POLICIES
-- =====================================================
-- Fix RLS policies to allow seeding of potato registry

-- Drop existing restrictive policies
DROP POLICY IF EXISTS "Anyone can view potato registry" ON public.potato_registry;

-- Disable RLS temporarily for seeding operations
ALTER TABLE public.potato_registry DISABLE ROW LEVEL SECURITY;

-- For potato_unlocks, we keep RLS enabled since it's user-specific data
-- The potato_registry should be readable by everyone and insertable by the system

-- Add a more permissive policy for potato_registry reads
-- Since this is reference data (all potatoes available), everyone should be able to read it
CREATE POLICY "Public can read potato registry" ON public.potato_registry
  FOR SELECT USING (true);

-- Allow authenticated users to insert into registry (for seeding)
-- In production, you might want to restrict this to admin users only
CREATE POLICY "Authenticated users can seed registry" ON public.potato_registry
  FOR INSERT WITH CHECK (auth.uid() IS NOT NULL);

-- Re-enable RLS with the new policies
ALTER TABLE public.potato_registry ENABLE ROW LEVEL SECURITY;

-- Update the view to work without RLS issues
DROP VIEW IF EXISTS public.potato_registry_with_unlocks;

CREATE VIEW public.potato_registry_with_unlocks AS
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

-- Grant necessary permissions
GRANT SELECT ON public.potato_registry TO anon, authenticated;
GRANT INSERT ON public.potato_registry TO authenticated;
GRANT SELECT ON public.potato_registry_with_unlocks TO anon, authenticated;

-- Success message
SELECT 'Potato registry RLS policies fixed! Ready for seeding. 🥔🔓' as status;