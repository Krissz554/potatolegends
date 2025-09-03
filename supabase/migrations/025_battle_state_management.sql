-- =====================================================
-- REAL-TIME BATTLE STATE MANAGEMENT
-- =====================================================

-- Add function to get current battle state
CREATE OR REPLACE FUNCTION get_battle_state(p_battle_id uuid)
RETURNS TABLE(
  battle_id uuid,
  phase text,
  current_turn uuid,
  round_number integer,
  turn_number integer,
  player1_id uuid,
  player2_id uuid,
  player1_cards_remaining integer,
  player2_cards_remaining integer,
  game_state jsonb,
  status text,
  started_at timestamptz,
  updated_at timestamptz
) AS $$
BEGIN
  RETURN QUERY
  SELECT 
    bs.id,
    COALESCE((bs.game_state->>'phase')::text, 'deployment'),
    bs.current_turn_player_id,
    bs.round_number,
    bs.turn_number,
    bs.player1_id,
    bs.player2_id,
    bs.player1_cards_remaining,
    bs.player2_cards_remaining,
    bs.game_state,
    bs.status,
    bs.started_at,
    bs.updated_at
  FROM public.battle_sessions bs
  WHERE bs.id = p_battle_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add function to update battle phase
CREATE OR REPLACE FUNCTION update_battle_phase(
  p_battle_id uuid,
  p_phase text,
  p_current_turn uuid DEFAULT NULL
)
RETURNS boolean AS $$
DECLARE
  v_battle_exists boolean;
BEGIN
  -- Check if battle exists and is active
  SELECT EXISTS(
    SELECT 1 FROM public.battle_sessions 
    WHERE id = p_battle_id 
    AND status IN ('deploying', 'battling')
  ) INTO v_battle_exists;
  
  IF NOT v_battle_exists THEN
    RETURN false;
  END IF;
  
  -- Update battle session
  UPDATE public.battle_sessions 
  SET 
    game_state = COALESCE(game_state, '{}'::jsonb) || jsonb_build_object('phase', p_phase),
    current_turn_player_id = COALESCE(p_current_turn, current_turn_player_id),
    status = CASE 
      WHEN p_phase = 'deployment' THEN 'deploying'
      WHEN p_phase = 'combat' THEN 'battling'
      WHEN p_phase = 'finished' THEN 'finished'
      ELSE status
    END,
    updated_at = NOW()
  WHERE id = p_battle_id;
  
  RETURN true;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add function to switch turns
CREATE OR REPLACE FUNCTION switch_battle_turn(p_battle_id uuid)
RETURNS uuid AS $$
DECLARE
  v_current_turn uuid;
  v_player1_id uuid;
  v_player2_id uuid;
  v_next_turn uuid;
BEGIN
  -- Get current battle info
  SELECT current_turn_player_id, player1_id, player2_id 
  INTO v_current_turn, v_player1_id, v_player2_id
  FROM public.battle_sessions 
  WHERE id = p_battle_id;
  
  -- Determine next turn
  IF v_current_turn = v_player1_id THEN
    v_next_turn := v_player2_id;
  ELSE
    v_next_turn := v_player1_id;
  END IF;
  
  -- Update battle session
  UPDATE public.battle_sessions 
  SET 
    current_turn_player_id = v_next_turn,
    updated_at = NOW()
  WHERE id = p_battle_id;
  
  RETURN v_next_turn;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permissions
GRANT EXECUTE ON FUNCTION get_battle_state(uuid) TO authenticated;
GRANT EXECUTE ON FUNCTION get_battle_state(uuid) TO service_role;
GRANT EXECUTE ON FUNCTION update_battle_phase(uuid, text, uuid) TO authenticated;
GRANT EXECUTE ON FUNCTION update_battle_phase(uuid, text, uuid) TO service_role;
GRANT EXECUTE ON FUNCTION switch_battle_turn(uuid) TO authenticated;
GRANT EXECUTE ON FUNCTION switch_battle_turn(uuid) TO service_role;

-- Success message
SELECT 'Battle state management functions created successfully!' as status;