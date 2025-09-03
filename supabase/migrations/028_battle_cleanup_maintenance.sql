-- =====================================================
-- CLEANUP AND MAINTENANCE FOR BATTLES
-- =====================================================

-- Add function to cleanup old battles
CREATE OR REPLACE FUNCTION cleanup_old_battles()
RETURNS TABLE(
  cleaned_battles integer,
  deleted_battles integer,
  message text
) AS $$
DECLARE
  v_cleaned_count integer := 0;
  v_deleted_count integer := 0;
BEGIN
  -- Mark battles older than 2 hours as abandoned
  UPDATE public.battle_sessions 
  SET 
    status = 'finished',
    finished_at = NOW(),
    updated_at = NOW(),
    game_state = COALESCE(game_state, '{}'::jsonb) || jsonb_build_object(
      'end_reason', 'abandoned',
      'abandoned_at', NOW()::text
    )
  WHERE status IN ('deploying', 'battling')
  AND started_at < NOW() - INTERVAL '2 hours';
  
  GET DIAGNOSTICS v_cleaned_count = ROW_COUNT;
  
  -- Clean up very old finished battles (older than 7 days)
  -- Keep the battle_sessions but delete old battle_actions to save space
  DELETE FROM public.battle_actions 
  WHERE battle_session_id IN (
    SELECT id FROM public.battle_sessions 
    WHERE status = 'finished' 
    AND finished_at < NOW() - INTERVAL '7 days'
  );
  
  GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
  
  RETURN QUERY SELECT 
    v_cleaned_count,
    v_deleted_count,
    format('Cleaned %s abandoned battles, deleted %s old battle actions', v_cleaned_count, v_deleted_count);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add function to get battle statistics
CREATE OR REPLACE FUNCTION get_battle_statistics()
RETURNS TABLE(
  total_battles integer,
  active_battles integer,
  finished_battles integer,
  abandoned_battles integer,
  avg_battle_duration interval
) AS $$
BEGIN
  RETURN QUERY
  SELECT 
    COUNT(*)::integer as total_battles,
    COUNT(CASE WHEN status IN ('deploying', 'battling') THEN 1 END)::integer as active_battles,
    COUNT(CASE WHEN status = 'finished' AND (game_state->>'end_reason') != 'abandoned' THEN 1 END)::integer as finished_battles,
    COUNT(CASE WHEN status = 'finished' AND (game_state->>'end_reason') = 'abandoned' THEN 1 END)::integer as abandoned_battles,
    AVG(CASE 
      WHEN finished_at IS NOT NULL THEN finished_at - started_at 
      ELSE NULL 
    END) as avg_battle_duration
  FROM public.battle_sessions;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add function to force end stale battles (for admin use)
CREATE OR REPLACE FUNCTION force_end_stale_battles(p_max_duration_hours integer DEFAULT 1)
RETURNS integer AS $$
DECLARE
  v_ended_count integer := 0;
  v_battle_record RECORD;
BEGIN
  -- Find stale battles
  FOR v_battle_record IN 
    SELECT id, player1_id, player2_id
    FROM public.battle_sessions 
    WHERE status IN ('deploying', 'battling')
    AND started_at < NOW() - (p_max_duration_hours || ' hours')::interval
  LOOP
    -- End each stale battle (player1 wins by default)
    PERFORM end_battle(v_battle_record.id, v_battle_record.player1_id, 'force_ended_stale');
    v_ended_count := v_ended_count + 1;
  END LOOP;
  
  RETURN v_ended_count;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Add updated_at trigger for battle_sessions
CREATE OR REPLACE FUNCTION update_battle_sessions_updated_at()
RETURNS trigger AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger for auto-updating updated_at
DROP TRIGGER IF EXISTS battle_sessions_updated_at_trigger ON public.battle_sessions;
CREATE TRIGGER battle_sessions_updated_at_trigger
  BEFORE UPDATE ON public.battle_sessions
  FOR EACH ROW
  EXECUTE FUNCTION update_battle_sessions_updated_at();

-- Grant execute permissions
GRANT EXECUTE ON FUNCTION cleanup_old_battles() TO service_role;
GRANT EXECUTE ON FUNCTION get_battle_statistics() TO service_role;
GRANT EXECUTE ON FUNCTION get_battle_statistics() TO authenticated;
GRANT EXECUTE ON FUNCTION force_end_stale_battles(integer) TO service_role;

-- Create indexes for better cleanup performance
CREATE INDEX IF NOT EXISTS idx_battle_sessions_cleanup ON public.battle_sessions (status, started_at, finished_at);
CREATE INDEX IF NOT EXISTS idx_battle_actions_cleanup ON public.battle_actions (battle_session_id, created_at);

-- Success message
SELECT 'Battle cleanup and maintenance functions created successfully!' as status;