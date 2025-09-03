-- Efficient Turn Timer System
-- This creates a simple, reliable timer system that works automatically

-- Drop any existing timer functions to start fresh
DROP FUNCTION IF EXISTS process_battle_timer(UUID);
DROP FUNCTION IF EXISTS process_all_battle_timers();
DROP FUNCTION IF EXISTS run_autonomous_timer();
DROP TABLE IF EXISTS battle_timer_logs;

-- Create a simple function to decrement a battle's timer
CREATE OR REPLACE FUNCTION decrement_battle_timer(battle_id UUID)
RETURNS JSONB
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_record RECORD;
    current_game_state JSONB;
    time_remaining INTEGER;
    new_time INTEGER;
    turn_ended BOOLEAN := false;
BEGIN
    -- Get the battle
    SELECT game_state INTO current_game_state
    FROM battle_sessions 
    WHERE id = battle_id 
    AND status = 'active'
    AND (game_state->>'current_turn_player_id') IS NOT NULL;
    
    -- Exit if battle doesn't exist or no active turn
    IF current_game_state IS NULL THEN
        RETURN jsonb_build_object('success', false, 'reason', 'no_active_battle');
    END IF;
    
    -- Get current timer
    time_remaining := COALESCE((current_game_state->>'turn_time_remaining')::INTEGER, 60);
    new_time := GREATEST(0, time_remaining - 1);
    
    -- Update the timer
    current_game_state := jsonb_set(current_game_state, '{turn_time_remaining}', to_jsonb(new_time));
    current_game_state := jsonb_set(current_game_state, '{last_action_time}', to_jsonb(NOW()::TEXT));
    
    -- Check if turn should auto-end
    IF new_time <= 0 THEN
        -- Get player IDs for turn switching
        SELECT player1_id, player2_id INTO battle_record
        FROM battle_sessions WHERE id = battle_id;
        
        DECLARE
            current_player_id TEXT := current_game_state->>'current_turn_player_id';
            next_player_id TEXT;
            new_turn_number INTEGER;
        BEGIN
            -- Determine next player
            next_player_id := CASE 
                WHEN current_player_id = battle_record.player1_id 
                THEN battle_record.player2_id 
                ELSE battle_record.player1_id 
            END;
            
            new_turn_number := COALESCE((current_game_state->>'turn_number')::INTEGER, 1) + 1;
            
            -- Switch turn and reset timer
            current_game_state := jsonb_set(current_game_state, '{current_turn_player_id}', to_jsonb(next_player_id));
            current_game_state := jsonb_set(current_game_state, '{turn_number}', to_jsonb(new_turn_number));
            current_game_state := jsonb_set(current_game_state, '{turn_time_remaining}', '60');
            current_game_state := jsonb_set(current_game_state, '{turn_phase}', '"start"');
            current_game_state := jsonb_set(current_game_state, '{turn_started_at}', to_jsonb(NOW()::TEXT));
            current_game_state := jsonb_set(current_game_state, '{last_action}', '"auto_end_turn_timeout"');
            
            turn_ended := true;
        END;
    END IF;
    
    -- Update the battle
    UPDATE battle_sessions 
    SET 
        game_state = current_game_state,
        updated_at = NOW()
    WHERE id = battle_id;
    
    -- Return result
    RETURN jsonb_build_object(
        'success', true, 
        'time_remaining', new_time,
        'turn_ended', turn_ended,
        'battle_id', battle_id
    );
END;
$$;

-- Create a function to start a new turn (resets timer to 60)
CREATE OR REPLACE FUNCTION start_new_turn(battle_id UUID, player_id TEXT)
RETURNS JSONB
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    current_game_state JSONB;
BEGIN
    -- Get current game state
    SELECT game_state INTO current_game_state
    FROM battle_sessions 
    WHERE id = battle_id 
    AND status = 'active';
    
    IF current_game_state IS NULL THEN
        RETURN jsonb_build_object('success', false, 'reason', 'battle_not_found');
    END IF;
    
    -- Set the new turn state
    current_game_state := jsonb_set(current_game_state, '{current_turn_player_id}', to_jsonb(player_id));
    current_game_state := jsonb_set(current_game_state, '{turn_time_remaining}', '60');
    current_game_state := jsonb_set(current_game_state, '{turn_started_at}', to_jsonb(NOW()::TEXT));
    current_game_state := jsonb_set(current_game_state, '{turn_phase}', '"main"');
    current_game_state := jsonb_set(current_game_state, '{last_action_time}', to_jsonb(NOW()::TEXT));
    
    -- Update the battle
    UPDATE battle_sessions 
    SET 
        game_state = current_game_state,
        updated_at = NOW()
    WHERE id = battle_id;
    
    RETURN jsonb_build_object('success', true, 'new_timer', 60);
END;
$$;

-- Grant permissions
GRANT EXECUTE ON FUNCTION decrement_battle_timer(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION decrement_battle_timer(UUID) TO service_role;
GRANT EXECUTE ON FUNCTION start_new_turn(UUID, TEXT) TO authenticated;
GRANT EXECUTE ON FUNCTION start_new_turn(UUID, TEXT) TO service_role;