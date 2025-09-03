-- =====================================================
-- Fix Edge Function Table References and Battle System
-- =====================================================

-- Ensure potato_unlocks table has all necessary features
ALTER TABLE public.potato_unlocks 
ADD COLUMN IF NOT EXISTS is_favorite BOOLEAN DEFAULT FALSE;

ALTER TABLE public.potato_unlocks 
ADD COLUMN IF NOT EXISTS view_count INTEGER DEFAULT 0;

-- Ensure battle_decks table has the deck_slot column for multiple deck support
ALTER TABLE public.battle_decks 
ADD COLUMN IF NOT EXISTS deck_slot INTEGER DEFAULT 1 CHECK (deck_slot >= 1 AND deck_slot <= 5);

-- Create function to auto-unlock basic potatoes for new users
CREATE OR REPLACE FUNCTION auto_unlock_basic_potatoes_for_user(user_uuid UUID)
RETURNS BOOLEAN AS $$
DECLARE
  basic_potato RECORD;
  unlock_count INTEGER;
BEGIN
  -- Check if user already has unlocked potatoes
  SELECT COUNT(*) INTO unlock_count
  FROM public.potato_unlocks 
  WHERE user_id = user_uuid;
  
  -- If user has no unlocked potatoes, unlock first 5 from registry
  IF unlock_count = 0 THEN
    FOR basic_potato IN 
      SELECT id FROM public.potato_registry 
      ORDER BY sort_order, id 
      LIMIT 5
    LOOP
      INSERT INTO public.potato_unlocks (user_id, potato_id, discovered_seed)
      VALUES (
        user_uuid, 
        basic_potato.id, 
        'auto-unlock-' || user_uuid::text || '-' || basic_potato.id::text
      )
      ON CONFLICT (user_id, potato_id) DO NOTHING;
    END LOOP;
    
    RETURN TRUE;
  END IF;
  
  RETURN FALSE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create function to ensure user has battle deck
CREATE OR REPLACE FUNCTION ensure_user_battle_deck(user_uuid UUID, deck_slot_num INTEGER DEFAULT 1)
RETURNS BOOLEAN AS $$
DECLARE
  deck_count INTEGER;
  unlocked_potato RECORD;
  position_counter INTEGER := 1;
BEGIN
  -- Check if user already has a deck in this slot
  SELECT COUNT(*) INTO deck_count
  FROM public.battle_decks 
  WHERE user_id = user_uuid AND deck_slot = deck_slot_num;
  
  -- If no deck exists, create one
  IF deck_count = 0 THEN
    -- First ensure user has unlocked potatoes
    PERFORM auto_unlock_basic_potatoes_for_user(user_uuid);
    
    -- Create deck from unlocked potatoes
    FOR unlocked_potato IN 
      SELECT potato_id FROM public.potato_unlocks 
      WHERE user_id = user_uuid 
      ORDER BY unlocked_at 
      LIMIT 5
    LOOP
      INSERT INTO public.battle_decks (user_id, potato_id, deck_position, deck_slot)
      VALUES (user_uuid, unlocked_potato.potato_id, position_counter, deck_slot_num)
      ON CONFLICT (user_id, deck_position, deck_slot) DO UPDATE 
      SET potato_id = EXCLUDED.potato_id;
      
      position_counter := position_counter + 1;
      
      -- Stop at 5 cards
      IF position_counter > 5 THEN
        EXIT;
      END IF;
    END LOOP;
    
    RETURN TRUE;
  END IF;
  
  RETURN FALSE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant necessary permissions
GRANT EXECUTE ON FUNCTION auto_unlock_basic_potatoes_for_user(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION ensure_user_battle_deck(UUID, INTEGER) TO authenticated;
GRANT EXECUTE ON FUNCTION auto_unlock_basic_potatoes_for_user(UUID) TO service_role;
GRANT EXECUTE ON FUNCTION ensure_user_battle_deck(UUID, INTEGER) TO service_role;

-- Update RLS policies to allow service role access for battle initialization
-- Drop existing policies if they exist, then create new ones
DROP POLICY IF EXISTS "Service role can manage potato unlocks" ON public.potato_unlocks;
CREATE POLICY "Service role can manage potato unlocks" ON public.potato_unlocks
  FOR ALL USING (current_setting('role') = 'service_role');

DROP POLICY IF EXISTS "Service role can manage battle decks" ON public.battle_decks;
CREATE POLICY "Service role can manage battle decks" ON public.battle_decks
  FOR ALL USING (current_setting('role') = 'service_role');

-- Ensure indexes exist for performance
CREATE INDEX IF NOT EXISTS idx_battle_decks_user_slot ON public.battle_decks(user_id, deck_slot);
CREATE INDEX IF NOT EXISTS idx_potato_unlocks_user_unlocked ON public.potato_unlocks(user_id, unlocked_at);

-- Success message
SELECT 'Edge function table references and battle system functions updated successfully!' as status;