-- Pure Server-Side Timer System
-- This creates a completely autonomous timer using PostgreSQL triggers and timestamps

-- Drop the old functions
DROP FUNCTION IF EXISTS decrement_battle_timer(UUID);
DROP FUNCTION IF EXISTS start_new_turn(UUID, TEXT);

-- Create a function that calculates current timer based on server timestamp
CREATE OR REPLACE FUNCTION get_current_timer_value(
    turn_started_at TIMESTAMP WITH TIME ZONE,
    initial_time INTEGER DEFAULT 60
)
RETURNS INTEGER
LANGUAGE plpgsql
STABLE
AS $$
BEGIN
    -- If no start time, return initial time
    IF turn_started_at IS NULL THEN
        RETURN initial_time;
    END IF;
    
    -- Calculate elapsed seconds since turn started
    DECLARE
        elapsed_seconds INTEGER := EXTRACT(EPOCH FROM (NOW() - turn_started_at))::INTEGER;
        remaining_time INTEGER := initial_time - elapsed_seconds;
    BEGIN
        -- Return remaining time, minimum 0
        RETURN GREATEST(0, remaining_time);
    END;
END;
$$;

-- Create a function to check and auto-end expired turns
CREATE OR REPLACE FUNCTION auto_end_expired_turns()
RETURNS void
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_record RECORD;
    elapsed_seconds INTEGER;
    updated_game_state JSONB;
    current_player_id TEXT;
    next_player_id TEXT;
    new_turn_number INTEGER;
BEGIN
    -- Find battles with expired turns (over 60 seconds)
    FOR battle_record IN 
        SELECT id, player1_id, player2_id, game_state
        FROM battle_sessions 
        WHERE status = 'active'
        AND (game_state->>'current_turn_player_id') IS NOT NULL
        AND (game_state->>'turn_started_at') IS NOT NULL
        AND EXTRACT(EPOCH FROM (NOW() - (game_state->>'turn_started_at')::TIMESTAMP WITH TIME ZONE)) > 60
    LOOP
        -- Get current game state
        updated_game_state := battle_record.game_state;
        current_player_id := updated_game_state->>'current_turn_player_id';
        
        -- Determine next player
        next_player_id := CASE 
            WHEN current_player_id = battle_record.player1_id 
            THEN battle_record.player2_id 
            ELSE battle_record.player1_id 
        END;
        
        new_turn_number := COALESCE((updated_game_state->>'turn_number')::INTEGER, 1) + 1;
        
        -- Switch to next player's turn
        updated_game_state := jsonb_set(updated_game_state, '{current_turn_player_id}', to_jsonb(next_player_id));
        updated_game_state := jsonb_set(updated_game_state, '{turn_number}', to_jsonb(new_turn_number));
        updated_game_state := jsonb_set(updated_game_state, '{turn_started_at}', to_jsonb(NOW()::TEXT));
        updated_game_state := jsonb_set(updated_game_state, '{turn_phase}', '"start"');
        updated_game_state := jsonb_set(updated_game_state, '{last_action}', '"auto_end_turn_timeout"');
        updated_game_state := jsonb_set(updated_game_state, '{last_action_time}', to_jsonb(NOW()::TEXT));
        
        -- Update the battle
        UPDATE battle_sessions 
        SET 
            game_state = updated_game_state,
            updated_at = NOW()
        WHERE id = battle_record.id;
        
        RAISE LOG 'Auto-ended expired turn for battle %, switched to player %', battle_record.id, next_player_id;
    END LOOP;
END;
$$;

-- Create a function to update timer display values in real-time
CREATE OR REPLACE FUNCTION update_timer_displays()
RETURNS void
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_record RECORD;
    current_timer INTEGER;
    updated_game_state JSONB;
BEGIN
    -- Update timer display for all active battles
    FOR battle_record IN 
        SELECT id, game_state
        FROM battle_sessions 
        WHERE status = 'active'
        AND (game_state->>'current_turn_player_id') IS NOT NULL
        AND (game_state->>'turn_started_at') IS NOT NULL
    LOOP
        -- Calculate current timer value
        current_timer := get_current_timer_value(
            (battle_record.game_state->>'turn_started_at')::TIMESTAMP WITH TIME ZONE,
            60
        );
        
        -- Only update if timer value changed
        IF current_timer != COALESCE((battle_record.game_state->>'turn_time_remaining')::INTEGER, 60) THEN
            updated_game_state := jsonb_set(
                battle_record.game_state, 
                '{turn_time_remaining}', 
                to_jsonb(current_timer)
            );
            
            -- Update the battle session
            UPDATE battle_sessions 
            SET 
                game_state = updated_game_state,
                updated_at = NOW()
            WHERE id = battle_record.id;
        END IF;
    END LOOP;
END;
$$;

-- Grant permissions
GRANT EXECUTE ON FUNCTION get_current_timer_value(TIMESTAMP WITH TIME ZONE, INTEGER) TO authenticated;
GRANT EXECUTE ON FUNCTION get_current_timer_value(TIMESTAMP WITH TIME ZONE, INTEGER) TO service_role;
GRANT EXECUTE ON FUNCTION auto_end_expired_turns() TO service_role;
GRANT EXECUTE ON FUNCTION update_timer_displays() TO service_role;