-- Migration 035: Fix Edge Function Access to Game State
-- Allows edge functions (service_role) to update game state while still preventing client tampering

-- Drop the old trigger that was too restrictive
DROP TRIGGER IF EXISTS prevent_game_state_tampering_trigger ON public.battle_sessions;

-- Create a more permissive function that allows service_role updates
CREATE OR REPLACE FUNCTION prevent_game_state_tampering()
RETURNS TRIGGER AS $$
BEGIN
  -- Allow service_role (used by edge functions) to update game state
  IF current_setting('role') = 'service_role' THEN
    RETURN NEW;
  END IF;
  
  -- Allow updates from authenticated edge functions
  IF current_setting('request.jwt.claims', true) IS NOT NULL THEN
    -- Check if this is a legitimate Supabase edge function call
    IF current_setting('request.jwt.claims', true)::json->>'iss' = 'supabase' THEN
      RETURN NEW;
    END IF;
  END IF;
  
  -- Block direct client manipulation
  RAISE EXCEPTION 'Direct game state manipulation not allowed';
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Recreate the trigger with the updated function
CREATE TRIGGER prevent_game_state_tampering_trigger
  BEFORE UPDATE OF game_state ON public.battle_sessions
  FOR EACH ROW
  EXECUTE FUNCTION prevent_game_state_tampering();

-- Ensure service_role can fully access battle_sessions
DROP POLICY IF EXISTS "Service role full access" ON public.battle_sessions;
CREATE POLICY "Service role full access" ON public.battle_sessions
  FOR ALL TO service_role
  USING (true)
  WITH CHECK (true);

-- Add comments for clarity
COMMENT ON FUNCTION prevent_game_state_tampering() IS 'Prevents direct client manipulation of game state while allowing edge functions to update it';
COMMENT ON TRIGGER prevent_game_state_tampering_trigger ON public.battle_sessions IS 'Protects game state from client tampering while allowing server-side updates';