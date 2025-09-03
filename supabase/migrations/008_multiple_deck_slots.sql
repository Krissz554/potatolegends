-- =====================================================
-- MULTIPLE DECK SLOTS SYSTEM
-- =====================================================
-- Allow users to save up to 3 different battle decks

-- Add deck_slot column to battle_decks table
ALTER TABLE public.battle_decks 
ADD COLUMN deck_slot INTEGER DEFAULT 1 CHECK (deck_slot >= 1 AND deck_slot <= 3);

-- Update unique constraints to include deck_slot
-- Remove old constraints
ALTER TABLE public.battle_decks 
DROP CONSTRAINT IF EXISTS battle_decks_user_id_deck_position_key;

ALTER TABLE public.battle_decks 
DROP CONSTRAINT IF EXISTS battle_decks_user_id_potato_id_key;

-- Add new constraints that include deck_slot
-- Each user can only have one card per position per deck slot
ALTER TABLE public.battle_decks 
ADD CONSTRAINT battle_decks_user_deck_position_unique 
UNIQUE(user_id, deck_slot, deck_position);

-- Each user can only have one instance of each potato per deck slot
ALTER TABLE public.battle_decks 
ADD CONSTRAINT battle_decks_user_deck_potato_unique 
UNIQUE(user_id, deck_slot, potato_id);

-- Create user_deck_settings table to track active deck slot
CREATE TABLE IF NOT EXISTS public.user_deck_settings (
  id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  user_id UUID REFERENCES public.user_profiles(id) ON DELETE CASCADE NOT NULL UNIQUE,
  active_deck_slot INTEGER DEFAULT 1 CHECK (active_deck_slot >= 1 AND active_deck_slot <= 3),
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- RLS for user_deck_settings
ALTER TABLE public.user_deck_settings ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can manage their own deck settings" ON public.user_deck_settings
FOR ALL USING (auth.uid() = user_id);

-- Function to get or create user deck settings
CREATE OR REPLACE FUNCTION get_or_create_deck_settings(user_uuid UUID)
RETURNS TABLE(active_deck_slot INTEGER) AS $$
BEGIN
  -- Try to get existing settings
  RETURN QUERY 
  SELECT ds.active_deck_slot 
  FROM public.user_deck_settings ds 
  WHERE ds.user_id = user_uuid;
  
  -- If no settings exist, create default ones
  IF NOT FOUND THEN
    INSERT INTO public.user_deck_settings (user_id, active_deck_slot)
    VALUES (user_uuid, 1);
    
    RETURN QUERY 
    SELECT 1 as active_deck_slot;
  END IF;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Update existing battle_decks to use deck_slot 1
UPDATE public.battle_decks 
SET deck_slot = 1 
WHERE deck_slot IS NULL;

-- Make deck_slot NOT NULL after setting default values
ALTER TABLE public.battle_decks 
ALTER COLUMN deck_slot SET NOT NULL;

-- Add indexes for better performance
CREATE INDEX IF NOT EXISTS idx_battle_decks_user_deck_slot ON public.battle_decks (user_id, deck_slot);
CREATE INDEX IF NOT EXISTS idx_user_deck_settings_user_id ON public.user_deck_settings (user_id);

-- Comments
COMMENT ON TABLE public.user_deck_settings IS 'Stores user preferences for active battle deck slot';
COMMENT ON COLUMN public.battle_decks.deck_slot IS 'Deck slot number (1-3) for multiple saved decks';
COMMENT ON FUNCTION get_or_create_deck_settings(UUID) IS 'Gets existing deck settings or creates default ones';

-- Success message
SELECT 'Multiple deck slots system created successfully!' as status;