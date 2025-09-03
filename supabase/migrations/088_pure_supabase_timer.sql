-- PURE SUPABASE SERVER-SIDE REAL-TIME TIMER SYSTEM
-- Uses PostgreSQL triggers and background workers for 100% server-side operation

-- Drop previous functions
DROP FUNCTION IF EXISTS get_realtime_timer(TIMESTAMP WITH TIME ZONE);
DROP FUNCTION IF EXISTS check_and_auto_end_turn(UUID);
DROP VIEW IF EXISTS active_battles_with_timer;

-- Enable pg_cron extension if available (for hosted Supabase)
-- This will fail gracefully if not available
DO $$ 
BEGIN
    CREATE EXTENSION IF NOT EXISTS pg_cron;
    RAISE NOTICE 'pg_cron extension enabled';
EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE 'pg_cron not available, using alternative approach';
END $$;

-- Function to calculate current timer value from timestamp
CREATE OR REPLACE FUNCTION calculate_turn_timer(turn_started_at TIMESTAMP WITH TIME ZONE)
RETURNS INTEGER
LANGUAGE SQL
IMMUTABLE
AS $$
    SELECT CASE 
        WHEN turn_started_at IS NULL THEN 60
        ELSE GREATEST(0, 60 - EXTRACT(EPOCH FROM (NOW() - turn_started_at))::INTEGER)
    END;
$$;

-- Function to auto-end expired turns
CREATE OR REPLACE FUNCTION auto_end_expired_turn(battle_id UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_record RECORD;
    current_game_state JSONB;
    turn_start_time TIMESTAMP WITH TIME ZONE;
    elapsed_seconds INTEGER;
    current_player_id TEXT;
    next_player_id TEXT;
    new_turn_number INTEGER;
    updated_game_state JSONB;
    current_hero JSONB;
    current_max_mana INTEGER;
    new_max_mana INTEGER;
    current_deck JSONB;
    current_hand JSONB;
    drawn_card JSONB;
    remaining_deck JSONB;
    new_hand JSONB;
    fatigue_damage INTEGER;
    current_hp INTEGER;
    new_hp INTEGER;
BEGIN
    -- Get the battle session
    SELECT id, player1_id, player2_id, game_state, status
    INTO battle_record
    FROM battle_sessions
    WHERE id = battle_id AND status = 'active';

    IF NOT FOUND THEN
        RETURN FALSE;
    END IF;

    current_game_state := battle_record.game_state;
    
    -- Check if there's an active turn
    current_player_id := current_game_state->>'current_turn_player_id';
    IF current_player_id IS NULL THEN
        RETURN FALSE;
    END IF;

    -- Get turn start time and check if expired
    turn_start_time := (current_game_state->>'turn_started_at')::TIMESTAMP WITH TIME ZONE;
    IF turn_start_time IS NULL THEN
        RETURN FALSE;
    END IF;

    elapsed_seconds := EXTRACT(EPOCH FROM (NOW() - turn_start_time))::INTEGER;

    -- If turn hasn't expired (less than 60 seconds), do nothing
    IF elapsed_seconds < 60 THEN
        RETURN FALSE;
    END IF;

    -- Turn has expired - auto-end it
    RAISE NOTICE 'Auto-ending expired turn for battle % (elapsed: %s)', battle_id, elapsed_seconds;

    next_player_id := CASE 
        WHEN current_player_id = battle_record.player1_id 
        THEN battle_record.player2_id 
        ELSE battle_record.player1_id 
    END;

    new_turn_number := COALESCE((current_game_state->>'turn_number')::INTEGER, 1) + 1;

    -- Create updated game state with new turn
    updated_game_state := current_game_state;
    updated_game_state := jsonb_set(updated_game_state, '{current_turn_player_id}', to_jsonb(next_player_id));
    updated_game_state := jsonb_set(updated_game_state, '{turn_number}', to_jsonb(new_turn_number));
    updated_game_state := jsonb_set(updated_game_state, '{turn_started_at}', to_jsonb(NOW()::TEXT));
    updated_game_state := jsonb_set(updated_game_state, '{turn_phase}', '"start"');
    updated_game_state := jsonb_set(updated_game_state, '{last_action}', '"auto_end_turn_timeout"');
    updated_game_state := jsonb_set(updated_game_state, '{last_action_time}', to_jsonb(NOW()::TEXT));

    -- Handle start of turn logic for next player
    IF next_player_id = battle_record.player1_id THEN
        -- Player 1's turn
        current_hero := updated_game_state->'player1_hero';
        current_max_mana := COALESCE((current_hero->>'max_mana')::INTEGER, 0);
        new_max_mana := LEAST(current_max_mana + 1, 10);

        -- Update hero mana and power
        current_hero := jsonb_set(current_hero, '{max_mana}', to_jsonb(new_max_mana));
        current_hero := jsonb_set(current_hero, '{mana}', to_jsonb(new_max_mana));
        current_hero := jsonb_set(current_hero, '{hero_power_used_this_turn}', 'false');
        updated_game_state := jsonb_set(updated_game_state, '{player1_hero}', current_hero);

        -- Handle card draw
        current_deck := updated_game_state->'player1_deck';
        current_hand := updated_game_state->'hands'->'player1_hand';

        IF jsonb_array_length(current_deck) > 0 THEN
            drawn_card := current_deck->0;
            remaining_deck := current_deck - 0;
            new_hand := current_hand || jsonb_build_array(drawn_card);
            updated_game_state := jsonb_set(updated_game_state, '{player1_deck}', remaining_deck);
            updated_game_state := jsonb_set(updated_game_state, '{hands,player1_hand}', new_hand);
        ELSE
            -- Handle fatigue
            fatigue_damage := COALESCE((current_hero->>'fatigue_damage')::INTEGER, 0) + 1;
            current_hp := COALESCE((current_hero->>'hp')::INTEGER, 30);
            new_hp := GREATEST(0, current_hp - fatigue_damage);
            current_hero := jsonb_set(current_hero, '{fatigue_damage}', to_jsonb(fatigue_damage));
            current_hero := jsonb_set(current_hero, '{hp}', to_jsonb(new_hp));
            updated_game_state := jsonb_set(updated_game_state, '{player1_hero}', current_hero);
        END IF;
    ELSE
        -- Player 2's turn (same logic)
        current_hero := updated_game_state->'player2_hero';
        current_max_mana := COALESCE((current_hero->>'max_mana')::INTEGER, 0);
        new_max_mana := LEAST(current_max_mana + 1, 10);

        -- Update hero mana and power
        current_hero := jsonb_set(current_hero, '{max_mana}', to_jsonb(new_max_mana));
        current_hero := jsonb_set(current_hero, '{mana}', to_jsonb(new_max_mana));
        current_hero := jsonb_set(current_hero, '{hero_power_used_this_turn}', 'false');
        updated_game_state := jsonb_set(updated_game_state, '{player2_hero}', current_hero);

        -- Handle card draw
        current_deck := updated_game_state->'player2_deck';
        current_hand := updated_game_state->'hands'->'player2_hand';

        IF jsonb_array_length(current_deck) > 0 THEN
            drawn_card := current_deck->0;
            remaining_deck := current_deck - 0;
            new_hand := current_hand || jsonb_build_array(drawn_card);
            updated_game_state := jsonb_set(updated_game_state, '{player2_deck}', remaining_deck);
            updated_game_state := jsonb_set(updated_game_state, '{hands,player2_hand}', new_hand);
        ELSE
            -- Handle fatigue
            fatigue_damage := COALESCE((current_hero->>'fatigue_damage')::INTEGER, 0) + 1;
            current_hp := COALESCE((current_hero->>'hp')::INTEGER, 30);
            new_hp := GREATEST(0, current_hp - fatigue_damage);
            current_hero := jsonb_set(current_hero, '{fatigue_damage}', to_jsonb(fatigue_damage));
            current_hero := jsonb_set(current_hero, '{hp}', to_jsonb(new_hp));
            updated_game_state := jsonb_set(updated_game_state, '{player2_hero}', current_hero);
        END IF;
    END IF;

    -- Update the battle session
    UPDATE battle_sessions
    SET 
        game_state = updated_game_state,
        updated_at = NOW()
    WHERE id = battle_id;

    RETURN TRUE;
END;
$$;

-- Function to process all active battles and auto-end expired turns
CREATE OR REPLACE FUNCTION process_battle_timers()
RETURNS TABLE(battles_checked INTEGER, turns_ended INTEGER)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_record RECORD;
    battles_count INTEGER := 0;
    ended_count INTEGER := 0;
    turn_ended BOOLEAN;
BEGIN
    FOR battle_record IN
        SELECT id
        FROM battle_sessions
        WHERE status = 'active'
        AND (game_state->>'current_turn_player_id') IS NOT NULL
        AND (game_state->>'turn_started_at') IS NOT NULL
    LOOP
        battles_count := battles_count + 1;
        
        SELECT auto_end_expired_turn(battle_record.id) INTO turn_ended;
        
        IF turn_ended THEN
            ended_count := ended_count + 1;
        END IF;
    END LOOP;

    RETURN QUERY SELECT battles_count, ended_count;
END;
$$;

-- Create a table to track timer jobs if pg_cron is not available
CREATE TABLE IF NOT EXISTS timer_heartbeat (
    id SERIAL PRIMARY KEY,
    last_check TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    battles_processed INTEGER DEFAULT 0,
    turns_ended INTEGER DEFAULT 0
);

-- Function to update timer heartbeat and trigger processing
CREATE OR REPLACE FUNCTION update_timer_heartbeat()
RETURNS void
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    result_record RECORD;
BEGIN
    -- Process timers
    SELECT * FROM process_battle_timers() INTO result_record;
    
    -- Update heartbeat
    INSERT INTO timer_heartbeat (last_check, battles_processed, turns_ended)
    VALUES (NOW(), result_record.battles_checked, result_record.turns_ended);
    
    -- Keep only last 100 entries
    DELETE FROM timer_heartbeat 
    WHERE id NOT IN (
        SELECT id FROM timer_heartbeat 
        ORDER BY last_check DESC 
        LIMIT 100
    );
    
    -- Log activity
    IF result_record.turns_ended > 0 THEN
        RAISE NOTICE 'Timer heartbeat: processed % battles, ended % turns', 
            result_record.battles_checked, result_record.turns_ended;
    END IF;
END;
$$;

-- Try to set up pg_cron job (will fail gracefully if not available)
DO $$
BEGIN
    -- Schedule timer processing every 10 seconds
    PERFORM cron.schedule('battle-timer-processor', '*/10 * * * * *', 'SELECT update_timer_heartbeat();');
    RAISE NOTICE 'Scheduled pg_cron job for timer processing';
EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE 'pg_cron not available, timer will need external triggering';
END $$;

-- Create a trigger to automatically update timer values on battle_sessions updates
CREATE OR REPLACE FUNCTION sync_battle_timer()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    -- If turn_started_at exists, calculate and store current timer
    IF NEW.game_state ? 'turn_started_at' AND NEW.game_state->>'turn_started_at' IS NOT NULL THEN
        NEW.game_state := jsonb_set(
            NEW.game_state,
            '{current_timer}',
            to_jsonb(calculate_turn_timer((NEW.game_state->>'turn_started_at')::TIMESTAMP WITH TIME ZONE))
        );
    END IF;
    
    RETURN NEW;
END;
$$;

-- Create trigger on battle_sessions
DROP TRIGGER IF EXISTS battle_timer_sync_trigger ON battle_sessions;
CREATE TRIGGER battle_timer_sync_trigger
    BEFORE UPDATE ON battle_sessions
    FOR EACH ROW
    EXECUTE FUNCTION sync_battle_timer();

-- Grant permissions
GRANT EXECUTE ON FUNCTION calculate_turn_timer(TIMESTAMP WITH TIME ZONE) TO authenticated;
GRANT EXECUTE ON FUNCTION calculate_turn_timer(TIMESTAMP WITH TIME ZONE) TO service_role;
GRANT EXECUTE ON FUNCTION auto_end_expired_turn(UUID) TO service_role;
GRANT EXECUTE ON FUNCTION process_battle_timers() TO service_role;
GRANT EXECUTE ON FUNCTION update_timer_heartbeat() TO service_role;
GRANT SELECT ON timer_heartbeat TO authenticated;
GRANT SELECT ON timer_heartbeat TO service_role;

-- Initial timer heartbeat
SELECT update_timer_heartbeat();