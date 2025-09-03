-- =====================================================
-- BATTLE END AND VICTORY MANAGEMENT
-- =====================================================

-- Add function to end battle
CREATE OR REPLACE FUNCTION end_battle(
  p_battle_id uuid,
  p_winner_id uuid,
  p_reason text DEFAULT 'victory'
)
RETURNS boolean AS $$
DECLARE
  v_battle_exists boolean;
  v_loser_id uuid;
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
  
  -- Get the loser ID
  SELECT 
    CASE 
      WHEN player1_id = p_winner_id THEN player2_id 
      ELSE player1_id 
    END
  INTO v_loser_id
  FROM public.battle_sessions 
  WHERE id = p_battle_id;
  
  -- Update battle session
  UPDATE public.battle_sessions 
  SET 
    status = 'finished',
    winner_id = p_winner_id,
    finished_at = NOW(),
    updated_at = NOW(),
    game_state = COALESCE(game_state, '{}'::jsonb) || jsonb_build_object(
      'phase', 'finished',
      'end_reason', p_reason,
      'finished_at', NOW()::text,
      'winner_id', p_winner_id,
      'loser_id', v_loser_id
    )
  WHERE id = p_battle_id;
  
  -- Log the game end action
  INSERT INTO public.battle_actions (
    battle_session_id,
    player_id,
    action_type,
    action_data
  ) VALUES (
    p_battle_id,
    p_winner_id,
    'game_end',
    jsonb_build_object(
      'reason', p_reason,
      'winner_id', p_winner_id,
      'loser_id', v_loser_id,
      'ended_at', NOW()::text
    )
  );
  
  RETURN true;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add function to handle surrender
CREATE OR REPLACE FUNCTION surrender_battle(
  p_battle_id uuid,
  p_surrendering_player_id uuid
)
RETURNS boolean AS $$
DECLARE
  v_winner_id uuid;
  v_battle_active boolean;
BEGIN
  -- Check if battle exists and is active
  SELECT EXISTS(
    SELECT 1 FROM public.battle_sessions 
    WHERE id = p_battle_id 
    AND status IN ('deploying', 'battling')
    AND (player1_id = p_surrendering_player_id OR player2_id = p_surrendering_player_id)
  ) INTO v_battle_active;
  
  IF NOT v_battle_active THEN
    RETURN false;
  END IF;
  
  -- Determine winner (the other player)
  SELECT 
    CASE 
      WHEN player1_id = p_surrendering_player_id THEN player2_id 
      ELSE player1_id 
    END
  INTO v_winner_id
  FROM public.battle_sessions 
  WHERE id = p_battle_id;
  
  -- Log surrender action first
  INSERT INTO public.battle_actions (
    battle_session_id,
    player_id,
    action_type,
    action_data
  ) VALUES (
    p_battle_id,
    p_surrendering_player_id,
    'surrender',
    jsonb_build_object(
      'surrendered_at', NOW()::text,
      'winner_id', v_winner_id
    )
  );
  
  -- End the battle
  RETURN end_battle(p_battle_id, v_winner_id, 'surrender');
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add function to check if battle should auto-end (no cards remaining)
CREATE OR REPLACE FUNCTION check_battle_auto_end(p_battle_id uuid)
RETURNS boolean AS $$
DECLARE
  v_player1_cards integer;
  v_player2_cards integer;
  v_player1_id uuid;
  v_player2_id uuid;
  v_winner_id uuid;
BEGIN
  -- Get current card counts and player IDs
  SELECT 
    player1_cards_remaining, 
    player2_cards_remaining,
    player1_id,
    player2_id
  INTO v_player1_cards, v_player2_cards, v_player1_id, v_player2_id
  FROM public.battle_sessions 
  WHERE id = p_battle_id 
  AND status IN ('deploying', 'battling');
  
  -- Check if either player is out of cards
  IF v_player1_cards <= 0 THEN
    v_winner_id := v_player2_id;
  ELSIF v_player2_cards <= 0 THEN
    v_winner_id := v_player1_id;
  ELSE
    RETURN false; -- Battle continues
  END IF;
  
  -- End the battle
  RETURN end_battle(p_battle_id, v_winner_id, 'no_cards_remaining');
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permissions
GRANT EXECUTE ON FUNCTION end_battle(uuid, uuid, text) TO authenticated;
GRANT EXECUTE ON FUNCTION end_battle(uuid, uuid, text) TO service_role;
GRANT EXECUTE ON FUNCTION surrender_battle(uuid, uuid) TO authenticated;
GRANT EXECUTE ON FUNCTION surrender_battle(uuid, uuid) TO service_role;
GRANT EXECUTE ON FUNCTION check_battle_auto_end(uuid) TO authenticated;
GRANT EXECUTE ON FUNCTION check_battle_auto_end(uuid) TO service_role;

-- Success message
SELECT 'Battle end management functions created successfully!' as status;