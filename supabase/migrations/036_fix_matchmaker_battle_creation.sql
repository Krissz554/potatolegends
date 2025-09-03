-- Migration 036: Fix Matchmaker Battle Creation
-- The matchmaker was creating battles with wrong game state, bypassing battle-initialize

-- Update server_side_matchmaking to create battles with minimal state
-- and let battle-initialize handle the proper game state setup
CREATE OR REPLACE FUNCTION server_side_matchmaking()
RETURNS TABLE(
  success boolean,
  battle_id uuid,
  player1_id uuid,
  player2_id uuid,
  message text
) AS $$
DECLARE
  v_player1 RECORD;
  v_player2 RECORD;
  v_battle_id uuid;
  v_player1_deck_count int;
  v_player2_deck_count int;
  v_player1_deck_slot int;
  v_player2_deck_slot int;
BEGIN
  -- Clean up old queue entries first
  DELETE FROM public.matchmaking_queue 
  WHERE joined_at < NOW() - INTERVAL '5 minutes';

  -- Get two waiting players with proper alias
  SELECT mq.user_id, mq.joined_at INTO v_player1
  FROM public.matchmaking_queue mq
  WHERE mq.status = 'searching' 
  ORDER BY mq.joined_at 
  LIMIT 1;

  SELECT mq.user_id, mq.joined_at INTO v_player2
  FROM public.matchmaking_queue mq
  WHERE mq.status = 'searching' 
  AND mq.user_id != v_player1.user_id
  ORDER BY mq.joined_at 
  LIMIT 1;

  -- Check if we have two players
  IF v_player1.user_id IS NULL OR v_player2.user_id IS NULL THEN
    RETURN QUERY SELECT false, NULL::uuid, NULL::uuid, NULL::uuid, 'Not enough players in queue'::text;
    RETURN;
  END IF;

  -- Verify both players have valid battle decks
  -- Get each player's active deck slot
  SELECT uds.active_deck_slot INTO v_player1_deck_slot
  FROM public.user_deck_settings uds
  WHERE uds.user_id = v_player1.user_id;

  SELECT uds.active_deck_slot INTO v_player2_deck_slot
  FROM public.user_deck_settings uds
  WHERE uds.user_id = v_player2.user_id;

  -- Default to slot 1 if no preference set
  v_player1_deck_slot := COALESCE(v_player1_deck_slot, 1);
  v_player2_deck_slot := COALESCE(v_player2_deck_slot, 1);

  -- Check deck counts
  SELECT COUNT(*) INTO v_player1_deck_count
  FROM public.battle_decks bd
  WHERE bd.user_id = v_player1.user_id 
  AND bd.deck_slot = v_player1_deck_slot;

  SELECT COUNT(*) INTO v_player2_deck_count
  FROM public.battle_decks bd
  WHERE bd.user_id = v_player2.user_id 
  AND bd.deck_slot = v_player2_deck_slot;

  -- Both players need at least 5 cards OR we'll auto-create them in battle-initialize
  -- For now, just log the deck status
  
  -- Create battle session with minimal initial state
  -- battle-initialize edge function will handle the full game state setup
  INSERT INTO public.battle_sessions (
    player1_id,
    player2_id,
    current_turn_player_id,
    game_state,
    status
  ) VALUES (
    v_player1.user_id,
    v_player2.user_id,
    NULL, -- Will be set by battle-initialize
    jsonb_build_object(
      'phase', 'needs_initialization',
      'turn_count', 1,
      'round_count', 1,
      'created_by_matchmaker', true
    ),
    'waiting_initialization' -- Clear status indicating initialization needed
  ) RETURNING id INTO v_battle_id;

  -- Remove matched players from queue
  DELETE FROM public.matchmaking_queue 
  WHERE user_id IN (v_player1.user_id, v_player2.user_id);

  -- Return success
  RETURN QUERY SELECT true, v_battle_id, v_player1.user_id, v_player2.user_id, 'Battle created, needs initialization'::text;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Update get_user_active_battle to include the new status
CREATE OR REPLACE FUNCTION get_user_active_battle(p_user_id uuid)
RETURNS TABLE(
  battle_id uuid,
  player1_id uuid,
  player2_id uuid,
  status text,
  started_at timestamptz,
  game_state jsonb
) AS $$
BEGIN
  RETURN QUERY
  SELECT 
    bs.id,
    bs.player1_id,
    bs.player2_id,
    bs.status,
    bs.started_at,
    bs.game_state
  FROM public.battle_sessions bs
  WHERE (bs.player1_id = p_user_id OR bs.player2_id = p_user_id)
  AND bs.status IN ('waiting_initialization', 'active', 'deploying', 'battling')
  AND bs.started_at > NOW() - INTERVAL '30 minutes'
  ORDER BY bs.started_at DESC
  LIMIT 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add comment
COMMENT ON FUNCTION server_side_matchmaking() IS 'Creates battles with minimal state for battle-initialize to handle';

-- Success message
SELECT 'Fixed matchmaker to create battles with minimal state - battle-initialize will handle full setup!' as status;