-- =====================================================
-- SECURE BATTLE SYSTEM - ANTI-CHEAT & SERVER-SIDE VALIDATION
-- =====================================================

-- Create function to validate battle actions
CREATE OR REPLACE FUNCTION validate_battle_action()
RETURNS TRIGGER AS $$
DECLARE
  battle_record RECORD;
  user_in_battle BOOLEAN := FALSE;
BEGIN
  -- Get battle session
  SELECT * INTO battle_record 
  FROM public.battle_sessions 
  WHERE id = NEW.battle_session_id;
  
  -- Check if battle exists
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Battle session not found';
  END IF;
  
  -- Check if user is part of the battle
  IF battle_record.player1_id = NEW.player_id OR battle_record.player2_id = NEW.player_id THEN
    user_in_battle := TRUE;
  END IF;
  
  IF NOT user_in_battle THEN
    RAISE EXCEPTION 'User not part of this battle';
  END IF;
  
  -- Check if battle is still active
  IF battle_record.status = 'finished' THEN
    RAISE EXCEPTION 'Battle is already finished';
  END IF;
  
  -- Validate turn order for non-initialization actions
  IF NEW.action_type != 'start_game' AND battle_record.current_turn_player_id != NEW.player_id THEN
    RAISE EXCEPTION 'Not your turn';
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create trigger for battle action validation
DROP TRIGGER IF EXISTS validate_battle_action_trigger ON public.battle_actions;
CREATE TRIGGER validate_battle_action_trigger
  BEFORE INSERT ON public.battle_actions
  FOR EACH ROW
  EXECUTE FUNCTION validate_battle_action();

-- Create function to prevent direct battle state manipulation
CREATE OR REPLACE FUNCTION prevent_game_state_tampering()
RETURNS TRIGGER AS $$
BEGIN
  -- Only allow updates from service role or specific edge functions
  IF current_setting('role') != 'service_role' THEN
    -- Check if this is coming from an authorized edge function
    IF current_setting('request.jwt.claims', true)::json->>'iss' != 'supabase' THEN
      RAISE EXCEPTION 'Direct game state manipulation not allowed';
    END IF;
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create trigger to prevent tampering
DROP TRIGGER IF EXISTS prevent_game_state_tampering_trigger ON public.battle_sessions;
CREATE TRIGGER prevent_game_state_tampering_trigger
  BEFORE UPDATE OF game_state ON public.battle_sessions
  FOR EACH ROW
  EXECUTE FUNCTION prevent_game_state_tampering();

-- Enhanced RLS policies for battle sessions
DROP POLICY IF EXISTS "Enhanced battle session access" ON public.battle_sessions;
CREATE POLICY "Enhanced battle session access" ON public.battle_sessions
  FOR ALL USING (
    -- Users can only access battles they're part of
    (auth.uid() = player1_id OR auth.uid() = player2_id) OR
    -- Service role can access all
    auth.role() = 'service_role'
  );

-- Enhanced RLS policies for battle actions  
DROP POLICY IF EXISTS "Enhanced battle action access" ON public.battle_actions;
CREATE POLICY "Enhanced battle action access" ON public.battle_actions
  FOR ALL USING (
    -- Users can only see actions from their battles
    EXISTS (
      SELECT 1 FROM public.battle_sessions 
      WHERE id = battle_session_id 
      AND (player1_id = auth.uid() OR player2_id = auth.uid())
    ) OR
    -- Service role can access all
    auth.role() = 'service_role'
  );

-- Create function to validate deck composition
CREATE OR REPLACE FUNCTION validate_deck_composition(user_uuid UUID)
RETURNS BOOLEAN AS $$
DECLARE
  deck_count INTEGER;
  unique_cards INTEGER;
  unlocked_count INTEGER;
BEGIN
  -- Check deck size (should be exactly 5 cards)
  SELECT COUNT(*) INTO deck_count
  FROM public.battle_decks
  WHERE user_id = user_uuid;
  
  IF deck_count != 5 THEN
    RETURN FALSE;
  END IF;
  
  -- Check for duplicate cards (each card should be unique)
  SELECT COUNT(DISTINCT potato_id) INTO unique_cards
  FROM public.battle_decks
  WHERE user_id = user_uuid;
  
  IF unique_cards != deck_count THEN
    RETURN FALSE;
  END IF;
  
  -- Check if all cards are actually unlocked by the user
  SELECT COUNT(*) INTO unlocked_count
  FROM public.battle_decks bd
  JOIN public.potato_unlocks pu ON bd.potato_id = pu.potato_id
  WHERE bd.user_id = user_uuid AND pu.user_id = user_uuid;
  
  IF unlocked_count != deck_count THEN
    RETURN FALSE;
  END IF;
  
  RETURN TRUE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create function to validate battle initialization
CREATE OR REPLACE FUNCTION validate_battle_initialization()
RETURNS TRIGGER AS $$
BEGIN
  -- Only allow if both players have valid decks
  IF NOT validate_deck_composition(NEW.player1_id) THEN
    RAISE EXCEPTION 'Player 1 has invalid deck composition';
  END IF;
  
  IF NOT validate_deck_composition(NEW.player2_id) THEN
    RAISE EXCEPTION 'Player 2 has invalid deck composition';
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create trigger for battle initialization validation
DROP TRIGGER IF EXISTS validate_battle_initialization_trigger ON public.battle_sessions;
CREATE TRIGGER validate_battle_initialization_trigger
  BEFORE INSERT ON public.battle_sessions
  FOR EACH ROW
  EXECUTE FUNCTION validate_battle_initialization();

-- Create function to prevent duplicate active battles
CREATE OR REPLACE FUNCTION prevent_duplicate_battles()
RETURNS TRIGGER AS $$
DECLARE
  existing_battles INTEGER;
BEGIN
  -- Check if either player already has an active battle
  SELECT COUNT(*) INTO existing_battles
  FROM public.battle_sessions
  WHERE (player1_id = NEW.player1_id OR player2_id = NEW.player1_id OR 
         player1_id = NEW.player2_id OR player2_id = NEW.player2_id)
  AND status IN ('deploying', 'battling');
  
  IF existing_battles > 0 THEN
    RAISE EXCEPTION 'Player already has an active battle';
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create trigger to prevent duplicate battles
DROP TRIGGER IF EXISTS prevent_duplicate_battles_trigger ON public.battle_sessions;
CREATE TRIGGER prevent_duplicate_battles_trigger
  BEFORE INSERT ON public.battle_sessions
  FOR EACH ROW
  EXECUTE FUNCTION prevent_duplicate_battles();

-- Create function to auto-cleanup abandoned battles
CREATE OR REPLACE FUNCTION auto_cleanup_battles()
RETURNS void AS $$
BEGIN
  -- Mark battles as abandoned if no activity for 10 minutes
  UPDATE public.battle_sessions 
  SET 
    status = 'finished',
    finished_at = NOW(),
    game_state = COALESCE(game_state, '{}'::jsonb) || jsonb_build_object(
      'end_reason', 'abandoned_timeout',
      'abandoned_at', NOW()::text
    )
  WHERE status IN ('deploying', 'battling')
  AND updated_at < NOW() - INTERVAL '10 minutes';
  
  -- Log cleanup action
  INSERT INTO public.battle_actions (
    battle_session_id,
    player_id,
    action_type,
    action_data
  )
  SELECT 
    id,
    player1_id, -- Attribute to player1 for logging
    'surrender',
    jsonb_build_object('reason', 'auto_cleanup_timeout')
  FROM public.battle_sessions
  WHERE status = 'finished'
  AND (game_state->>'end_reason') = 'abandoned_timeout'
  AND finished_at > NOW() - INTERVAL '1 minute'; -- Only recently cleaned up
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create function for rate limiting battle actions
CREATE OR REPLACE FUNCTION check_action_rate_limit(player_uuid UUID, battle_uuid UUID)
RETURNS BOOLEAN AS $$
DECLARE
  recent_actions INTEGER;
BEGIN
  -- Count actions in the last 10 seconds
  SELECT COUNT(*) INTO recent_actions
  FROM public.battle_actions
  WHERE player_id = player_uuid
  AND battle_session_id = battle_uuid
  AND created_at > NOW() - INTERVAL '10 seconds';
  
  -- Allow max 5 actions per 10 seconds (prevents spam/automation)
  RETURN recent_actions < 5;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant necessary permissions
GRANT EXECUTE ON FUNCTION validate_deck_composition(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION check_action_rate_limit(UUID, UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION auto_cleanup_battles() TO service_role;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_battle_sessions_active ON public.battle_sessions (status, updated_at) WHERE status IN ('deploying', 'battling');
CREATE INDEX IF NOT EXISTS idx_battle_actions_rate_limit ON public.battle_actions (player_id, battle_session_id, created_at);

-- Success message
SELECT 'Secure battle system with anti-cheat measures implemented!' as status;