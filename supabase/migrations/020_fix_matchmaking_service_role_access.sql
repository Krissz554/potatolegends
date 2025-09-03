-- =====================================================
-- FIX MATCHMAKING SERVICE ROLE ACCESS
-- =====================================================
-- This migration fixes RLS policies to allow service role
-- to properly manage matchmaking queue and battle creation

-- Update matchmaking queue policies to allow service role access
DROP POLICY IF EXISTS "Users can manage their own queue entry" ON public.matchmaking_queue;
DROP POLICY IF EXISTS "Service role can manage matchmaking queue" ON public.matchmaking_queue;

-- Allow users to manage their own entries
CREATE POLICY "Users can manage their own queue entry" ON public.matchmaking_queue
FOR ALL USING (auth.uid() = user_id);

-- Allow service role to manage all queue entries for matchmaking
CREATE POLICY "Service role can manage matchmaking queue" ON public.matchmaking_queue
FOR ALL USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Update battle sessions policies for service role
DROP POLICY IF EXISTS "Service role can manage battle sessions" ON public.battle_sessions;

CREATE POLICY "Service role can manage battle sessions" ON public.battle_sessions
FOR ALL USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Allow service role to read battle decks for matchmaking
DROP POLICY IF EXISTS "Service role can read battle decks" ON public.battle_decks;

CREATE POLICY "Service role can read battle decks" ON public.battle_decks
FOR SELECT USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Allow service role to read user deck settings
DROP POLICY IF EXISTS "Service role can read deck settings" ON public.user_deck_settings;

CREATE POLICY "Service role can read deck settings" ON public.user_deck_settings
FOR SELECT USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Allow service role to read potato registry for battle deck validation
DROP POLICY IF EXISTS "Service role can read potato registry" ON public.potato_registry;

CREATE POLICY "Service role can read potato registry" ON public.potato_registry
FOR SELECT USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Create improved server-side matchmaking function
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

  -- Get two waiting players
  SELECT user_id, joined_at INTO v_player1
  FROM public.matchmaking_queue 
  WHERE status = 'searching' 
  ORDER BY joined_at 
  LIMIT 1;

  SELECT user_id, joined_at INTO v_player2
  FROM public.matchmaking_queue 
  WHERE status = 'searching' 
  AND user_id != v_player1.user_id
  ORDER BY joined_at 
  LIMIT 1;

  -- Check if we have enough players
  IF v_player1.user_id IS NULL OR v_player2.user_id IS NULL THEN
    RETURN QUERY SELECT false, NULL::uuid, NULL::uuid, NULL::uuid, 'Not enough players in queue'::text;
    RETURN;
  END IF;

  -- Check if either player already has an active battle
  IF EXISTS (
    SELECT 1 FROM public.battle_sessions 
    WHERE (player1_id = v_player1.user_id OR player2_id = v_player1.user_id OR 
           player1_id = v_player2.user_id OR player2_id = v_player2.user_id)
    AND status IN ('deploying', 'battling')
    AND started_at > NOW() - INTERVAL '30 minutes'
  ) THEN
    -- Remove these players from queue
    DELETE FROM public.matchmaking_queue 
    WHERE user_id IN (v_player1.user_id, v_player2.user_id);
    
    RETURN QUERY SELECT false, NULL::uuid, NULL::uuid, NULL::uuid, 'Players already in active battle'::text;
    RETURN;
  END IF;

  -- Get active deck slots for both players
  SELECT COALESCE(active_deck_slot, 1) INTO v_player1_deck_slot
  FROM public.user_deck_settings 
  WHERE user_id = v_player1.user_id;
  
  SELECT COALESCE(active_deck_slot, 1) INTO v_player2_deck_slot
  FROM public.user_deck_settings 
  WHERE user_id = v_player2.user_id;

  -- Default to slot 1 if no settings found
  v_player1_deck_slot := COALESCE(v_player1_deck_slot, 1);
  v_player2_deck_slot := COALESCE(v_player2_deck_slot, 1);

  -- Validate both players have complete decks
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

-- Create automatic matchmaking trigger function
CREATE OR REPLACE FUNCTION auto_matchmaking_trigger()
RETURNS trigger AS $$
DECLARE
  queue_count int;
BEGIN
  -- Only trigger on INSERT of searching status
  IF TG_OP = 'INSERT' AND NEW.status = 'searching' THEN
    -- Check current queue size
    SELECT COUNT(*) INTO queue_count
    FROM public.matchmaking_queue 
    WHERE status = 'searching';
    
    -- If we have 2 or more players, trigger matchmaking
    IF queue_count >= 2 THEN
      PERFORM server_side_matchmaking();
    END IF;
  END IF;
  
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Update the trigger to use the new function
DROP TRIGGER IF EXISTS matchmaking_trigger ON public.matchmaking_queue;
CREATE TRIGGER auto_matchmaking_trigger
  AFTER INSERT ON public.matchmaking_queue
  FOR EACH ROW
  EXECUTE FUNCTION auto_matchmaking_trigger();

-- Create periodic matchmaking job function (to be called by edge function)
CREATE OR REPLACE FUNCTION periodic_matchmaking_check()
RETURNS TABLE(
  matches_created int,
  queue_size int,
  message text
) AS $$
DECLARE
  v_matches_created int := 0;
  v_queue_size int;
  v_result RECORD;
BEGIN
  -- Get current queue size
  SELECT COUNT(*) INTO v_queue_size
  FROM public.matchmaking_queue 
  WHERE status = 'searching';

  -- Keep matching players while we have 2 or more
  WHILE v_queue_size >= 2 LOOP
    -- Try to create a match
    SELECT * INTO v_result FROM server_side_matchmaking();
    
    -- If match was successful, increment counter
    IF v_result.success THEN
      v_matches_created := v_matches_created + 1;
    ELSE
      -- If match failed, stop trying
      EXIT;
    END IF;
    
    -- Update queue size for next iteration
    SELECT COUNT(*) INTO v_queue_size
    FROM public.matchmaking_queue 
    WHERE status = 'searching';
  END LOOP;

  RETURN QUERY SELECT 
    v_matches_created, 
    v_queue_size, 
    CASE 
      WHEN v_matches_created > 0 THEN format('Created %s matches, %s players remaining', v_matches_created, v_queue_size)
      ELSE format('No matches created, %s players in queue', v_queue_size)
    END;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permission to service role
GRANT EXECUTE ON FUNCTION periodic_matchmaking_check() TO service_role;

-- Create function to get user's active battle
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
    id,
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

-- Comments
COMMENT ON FUNCTION server_side_matchmaking() IS 'Server-side matchmaking that validates decks and creates battles';
COMMENT ON FUNCTION periodic_matchmaking_check() IS 'Checks queue and creates multiple matches if possible';
COMMENT ON FUNCTION get_user_active_battle(uuid) IS 'Gets active battle for a specific user';

-- Success message
SELECT 'Matchmaking service role access and server-side functions created successfully!' as status;