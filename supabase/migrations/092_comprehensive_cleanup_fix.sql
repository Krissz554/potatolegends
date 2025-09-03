-- Comprehensive cleanup of all references to removed tables
-- Migration: 092_comprehensive_cleanup_fix.sql

-- Remove all functions that reference removed tables
DROP FUNCTION IF EXISTS get_or_create_deck_settings(UUID) CASCADE;
DROP FUNCTION IF EXISTS set_user_active_deck_slot(UUID, INTEGER) CASCADE;
DROP FUNCTION IF EXISTS ensure_user_battle_deck(UUID, INTEGER) CASCADE;
DROP FUNCTION IF EXISTS get_user_active_deck_slot(UUID) CASCADE;
DROP FUNCTION IF EXISTS periodic_matchmaking_check() CASCADE;
DROP FUNCTION IF EXISTS server_side_matchmaking() CASCADE;
DROP FUNCTION IF EXISTS auto_matchmaking_trigger() CASCADE;

-- Remove any remaining triggers
DROP TRIGGER IF EXISTS matchmaker_trigger ON public.matchmaking_queue CASCADE;
DROP TRIGGER IF EXISTS battle_stats_trigger ON public.battle_actions CASCADE;

-- Remove any remaining functions
DROP FUNCTION IF EXISTS trigger_matchmaker() CASCADE;
DROP FUNCTION IF EXISTS cleanup_old_queue_entries() CASCADE;
DROP FUNCTION IF EXISTS update_battle_stats() CASCADE;
DROP FUNCTION IF EXISTS cleanup_stale_matchmaking_data() CASCADE;

-- Remove any views that might reference removed tables
DROP VIEW IF EXISTS public.user_battle_deck_view CASCADE;
DROP VIEW IF EXISTS public.active_deck_view CASCADE;

-- Remove any remaining policies on removed tables (in case they still exist)
DO $$
BEGIN
    -- These will fail silently if tables don't exist
    DROP POLICY IF EXISTS "Users can manage their own deck settings" ON public.user_deck_settings;
    DROP POLICY IF EXISTS "Service role full access deck settings" ON public.user_deck_settings;
    DROP POLICY IF EXISTS "Users can view actions in their battles" ON public.battle_actions;
    DROP POLICY IF EXISTS "Users can create actions in their battles" ON public.battle_actions;
EXCEPTION WHEN OTHERS THEN
    NULL; -- Ignore all errors
END $$;

-- Ensure clean RLS policies for remaining tables
DROP POLICY IF EXISTS "battle_actions_select_policy" ON public.battle_actions;
DROP POLICY IF EXISTS "battle_actions_service_role_policy" ON public.battle_actions;

-- Recreate clean battle_actions policies
CREATE POLICY "battle_actions_select_policy" ON public.battle_actions
  FOR SELECT TO authenticated
  USING (
    EXISTS (
      SELECT 1 FROM public.battle_sessions bs 
      WHERE bs.id = battle_session_id 
      AND (bs.player1_id = auth.uid() OR bs.player2_id = auth.uid())
    )
  );

CREATE POLICY "battle_actions_service_role_policy" ON public.battle_actions
  FOR ALL TO service_role
  USING (true)
  WITH CHECK (true);

-- Create simplified matchmaking function that works with current table structure
CREATE OR REPLACE FUNCTION periodic_matchmaking_check()
RETURNS TABLE(
  matches_created int,
  queue_size int,
  message text
) AS $$
DECLARE
  v_matches_created int := 0;
  v_queue_size int;
  v_player1 RECORD;
  v_player2 RECORD;
  v_new_battle_id uuid;
BEGIN
  -- Get current queue size
  SELECT COUNT(*) INTO v_queue_size
  FROM public.matchmaking_queue
  WHERE status = 'searching';
  
  -- Try to match players (simplified - no deck slot checking)
  WHILE v_queue_size >= 2 LOOP
    -- Get two players from queue
    SELECT * INTO v_player1
    FROM public.matchmaking_queue
    WHERE status = 'searching'
    ORDER BY joined_at ASC
    LIMIT 1;
    
    SELECT * INTO v_player2
    FROM public.matchmaking_queue
    WHERE status = 'searching'
    AND user_id != v_player1.user_id
    ORDER BY joined_at ASC
    LIMIT 1;
    
    IF v_player1.user_id IS NULL OR v_player2.user_id IS NULL THEN
      EXIT;
    END IF;
    
    -- Create battle session
    INSERT INTO public.battle_sessions (
      player1_id,
      player2_id,
      status,
      game_state
    ) VALUES (
      v_player1.user_id,
      v_player2.user_id,
      'waiting_initialization',
      '{}'::jsonb
    ) RETURNING id INTO v_new_battle_id;
    
    -- Remove players from queue
    UPDATE public.matchmaking_queue
    SET status = 'matched'
    WHERE user_id IN (v_player1.user_id, v_player2.user_id);
    
    v_matches_created := v_matches_created + 1;
    v_queue_size := v_queue_size - 2;
  END LOOP;
  
  RETURN QUERY SELECT 
    v_matches_created,
    v_queue_size,
    CASE 
      WHEN v_matches_created > 0 THEN format('Created %s matches, %s players remaining in queue', v_matches_created, v_queue_size)
      ELSE format('No matches created, %s players in queue', v_queue_size)
    END;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant execute permission to service role
GRANT EXECUTE ON FUNCTION periodic_matchmaking_check() TO service_role;

-- Success message
SELECT 'Comprehensive cleanup completed! Removed all references to deleted tables and recreated simplified matchmaking.' as status;