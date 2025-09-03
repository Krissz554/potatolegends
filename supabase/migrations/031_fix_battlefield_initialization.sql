-- =====================================================
-- FIX BATTLEFIELD INITIALIZATION FOR EXISTING BATTLES
-- =====================================================

-- Update existing battles that have null or malformed battlefield objects
UPDATE public.battle_sessions 
SET game_state = COALESCE(game_state, '{}'::jsonb) || jsonb_build_object(
  'battlefield', jsonb_build_object(
    'player1_active', null,
    'player2_active', null
  )
)
WHERE 
  status IN ('deploying', 'battling') 
  AND (
    game_state->>'battlefield' IS NULL 
    OR game_state->'battlefield' IS NULL
    OR NOT (game_state->'battlefield' ? 'player1_active')
    OR NOT (game_state->'battlefield' ? 'player2_active')
  );

-- Ensure all battles have proper hands initialized
UPDATE public.battle_sessions 
SET game_state = COALESCE(game_state, '{}'::jsonb) || jsonb_build_object(
  'player1_hand', COALESCE(game_state->'player1_hand', '[]'::jsonb),
  'player2_hand', COALESCE(game_state->'player2_hand', '[]'::jsonb)
)
WHERE 
  status IN ('deploying', 'battling') 
  AND (
    game_state->>'player1_hand' IS NULL 
    OR game_state->>'player2_hand' IS NULL
    OR game_state->'player1_hand' IS NULL
    OR game_state->'player2_hand' IS NULL
  );

-- Add function to validate and fix battle state structure
CREATE OR REPLACE FUNCTION fix_battle_state_structure(battle_uuid UUID)
RETURNS BOOLEAN AS $$
DECLARE
  current_state JSONB;
  fixed_state JSONB;
BEGIN
  -- Get current game state
  SELECT game_state INTO current_state
  FROM public.battle_sessions
  WHERE id = battle_uuid;
  
  IF current_state IS NULL THEN
    current_state := '{}'::jsonb;
  END IF;
  
  -- Build fixed state with all required fields
  fixed_state := current_state || jsonb_build_object(
    'battlefield', COALESCE(current_state->'battlefield', jsonb_build_object(
      'player1_active', null,
      'player2_active', null
    )),
    'player1_hand', COALESCE(current_state->'player1_hand', '[]'::jsonb),
    'player2_hand', COALESCE(current_state->'player2_hand', '[]'::jsonb),
    'phase', COALESCE(current_state->>'phase', 'waiting_first_deploy'),
    'turn_count', COALESCE((current_state->>'turn_count')::integer, 1),
    'round_count', COALESCE((current_state->>'round_count')::integer, 1)
  );
  
  -- Update the battle
  UPDATE public.battle_sessions
  SET game_state = fixed_state, updated_at = NOW()
  WHERE id = battle_uuid;
  
  RETURN TRUE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION fix_battle_state_structure(UUID) TO service_role;
GRANT EXECUTE ON FUNCTION fix_battle_state_structure(UUID) TO authenticated;

-- Success message
SELECT 'Battlefield initialization fixed for existing battles!' as status;