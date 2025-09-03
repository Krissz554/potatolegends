-- =====================================================
-- FIX AMBIGUOUS COLUMN REFERENCES IN MATCHMAKING (CORRECTED)
-- =====================================================

-- Drop triggers before dropping functions to avoid dependency issues
DROP TRIGGER IF EXISTS auto_matchmaking_trigger ON public.matchmaking_queue;

-- Now we can safely drop the functions
DROP FUNCTION IF EXISTS auto_matchmaking_trigger();
DROP FUNCTION IF EXISTS server_side_matchmaking();
DROP FUNCTION IF EXISTS get_user_active_battle(uuid);

-- Recreate server_side_matchmaking function with proper table aliases
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

  -- Check if we have enough players
  IF v_player1.user_id IS NULL OR v_player2.user_id IS NULL THEN
    RETURN QUERY SELECT false, NULL::uuid, NULL::uuid, NULL::uuid, 'Not enough players in queue'::text;
    RETURN;
  END IF;

  -- Check if either player already has an active battle (use table aliases)
  IF EXISTS (
    SELECT 1 FROM public.battle_sessions bs
    WHERE (bs.player1_id = v_player1.user_id OR bs.player2_id = v_player1.user_id OR 
           bs.player1_id = v_player2.user_id OR bs.player2_id = v_player2.user_id)
    AND bs.status IN ('deploying', 'battling')
    AND bs.started_at > NOW() - INTERVAL '30 minutes'
  ) THEN
    -- Remove these players from queue
    DELETE FROM public.matchmaking_queue 
    WHERE user_id IN (v_player1.user_id, v_player2.user_id);
    
    RETURN QUERY SELECT false, NULL::uuid, NULL::uuid, NULL::uuid, 'Players already in active battle'::text;
    RETURN;
  END IF;

  -- Get active deck slots for both players with proper alias
  SELECT COALESCE(uds.active_deck_slot, 1) INTO v_player1_deck_slot
  FROM public.user_deck_settings uds
  WHERE uds.user_id = v_player1.user_id;
  
  SELECT COALESCE(uds.active_deck_slot, 1) INTO v_player2_deck_slot
  FROM public.user_deck_settings uds
  WHERE uds.user_id = v_player2.user_id;

  -- Default to slot 1 if no settings found
  v_player1_deck_slot := COALESCE(v_player1_deck_slot, 1);
  v_player2_deck_slot := COALESCE(v_player2_deck_slot, 1);

  -- Validate both players have complete decks (use proper table aliases)
  SELECT COUNT(*) INTO v_player1_deck_count
  FROM public.battle_decks bd
  JOIN public.potato_registry pr ON bd.potato_id = pr.id
  WHERE bd.user_id = v_player1.user_id 
  AND bd.deck_slot = v_player1_deck_slot;

  SELECT COUNT(*) INTO v_player2_deck_count
  FROM public.battle_decks bd
  JOIN public.potato_registry pr ON bd.potato_id = pr.id
  WHERE bd.user_id = v_player2.user_id 
  AND bd.deck_slot = v_player2_deck_slot;

  IF v_player1_deck_count < 5 OR v_player2_deck_count < 5 THEN
    -- Remove players with incomplete decks from queue
    DELETE FROM public.matchmaking_queue 
    WHERE user_id IN (v_player1.user_id, v_player2.user_id);
    
    RETURN QUERY SELECT false, NULL::uuid, NULL::uuid, NULL::uuid, 'Players have incomplete battle decks'::text;
    RETURN;
  END IF;

  -- Create battle session
  INSERT INTO public.battle_sessions (
    player1_id,
    player2_id,
    current_turn_player_id,
    game_state,
    status
  ) VALUES (
    v_player1.user_id,
    v_player2.user_id,
    v_player1.user_id, -- Player 1 starts
    jsonb_build_object(
      'phase', 'deployment',
      'turn_count', 1,
      'board', '{}',
      'actions', '[]'
    ),
    'deploying'
  ) RETURNING id INTO v_battle_id;

  -- Remove matched players from queue
  DELETE FROM public.matchmaking_queue 
  WHERE user_id IN (v_player1.user_id, v_player2.user_id);

  -- Return success
  RETURN QUERY SELECT true, v_battle_id, v_player1.user_id, v_player2.user_id, 'Battle created successfully'::text;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permission to service role
GRANT EXECUTE ON FUNCTION server_side_matchmaking() TO service_role;

-- Recreate get_user_active_battle function with proper aliases
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
  AND bs.status IN ('deploying', 'battling')
  AND bs.started_at > NOW() - INTERVAL '30 minutes'
  ORDER BY bs.started_at DESC
  LIMIT 1;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permissions
GRANT EXECUTE ON FUNCTION get_user_active_battle(uuid) TO service_role;
GRANT EXECUTE ON FUNCTION get_user_active_battle(uuid) TO authenticated;

-- Recreate the auto_matchmaking_trigger function with proper aliases
CREATE OR REPLACE FUNCTION auto_matchmaking_trigger()
RETURNS trigger AS $$
DECLARE
  queue_count int;
BEGIN
  -- Only trigger on INSERT of searching status
  IF TG_OP = 'INSERT' AND NEW.status = 'searching' THEN
    -- Check current queue size with proper alias
    SELECT COUNT(*) INTO queue_count
    FROM public.matchmaking_queue mq
    WHERE mq.status = 'searching';
    
    -- If we have 2 or more players, trigger matchmaking
    IF queue_count >= 2 THEN
      PERFORM server_side_matchmaking();
    END IF;
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create the trigger (after function is created)
CREATE TRIGGER auto_matchmaking_trigger
  AFTER INSERT ON public.matchmaking_queue
  FOR EACH ROW
  EXECUTE FUNCTION auto_matchmaking_trigger();

-- Success message
SELECT 'Fixed ambiguous column references in matchmaking functions (corrected order)!' as status;