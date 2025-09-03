-- Autonomous Server-Side Timer System
-- This creates a completely server-side timer that requires NO client interaction
-- Each battle manages its own timer using PostgreSQL triggers and background jobs

-- First, let's create a more robust timer function that processes individual battles
CREATE OR REPLACE FUNCTION process_battle_timer(battle_id UUID)
RETURNS void
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_record RECORD;
    current_game_state JSONB;
    time_remaining INTEGER;
    new_time_remaining INTEGER;
    current_player_id TEXT;
    next_player_id TEXT;
    new_turn_number INTEGER;
    is_player1_turn BOOLEAN;
BEGIN
    -- Get the specific battle
    SELECT id, player1_id, player2_id, game_state, status
    INTO battle_record
    FROM battle_sessions 
    WHERE id = battle_id 
    AND status = 'active'
    AND (game_state->>'current_turn_player_id') IS NOT NULL
    AND (game_state->>'turn_time_remaining')::INTEGER > 0;
    
    -- Exit if battle doesn't exist or doesn't need timer processing
    IF NOT FOUND THEN
        RETURN;
    END IF;
    
    current_game_state := battle_record.game_state;
    time_remaining := (current_game_state->>'turn_time_remaining')::INTEGER;
    new_time_remaining := GREATEST(0, time_remaining - 1);
    
    RAISE LOG 'Processing timer for battle %, time: % -> %', battle_id, time_remaining, new_time_remaining;
    
    -- Update the timer
    current_game_state := jsonb_set(current_game_state, '{turn_time_remaining}', to_jsonb(new_time_remaining));
    current_game_state := jsonb_set(current_game_state, '{last_action_time}', to_jsonb(NOW()::TEXT));
    
    -- Check if we need to auto-end the turn
    IF new_time_remaining <= 0 THEN
        RAISE LOG 'Auto-ending turn for battle %', battle_id;
        
        -- Get current and next players
        current_player_id := current_game_state->>'current_turn_player_id';
        next_player_id := CASE 
            WHEN current_player_id = battle_record.player1_id 
            THEN battle_record.player2_id 
            ELSE battle_record.player1_id 
        END;
        
        new_turn_number := COALESCE((current_game_state->>'turn_number')::INTEGER, 1) + 1;
        is_player1_turn := (next_player_id = battle_record.player1_id);
        
        -- Switch to next player and reset timer
        current_game_state := jsonb_set(current_game_state, '{current_turn_player_id}', to_jsonb(next_player_id));
        current_game_state := jsonb_set(current_game_state, '{turn_number}', to_jsonb(new_turn_number));
        current_game_state := jsonb_set(current_game_state, '{turn_time_remaining}', '60');
        current_game_state := jsonb_set(current_game_state, '{turn_phase}', '"start"');
        current_game_state := jsonb_set(current_game_state, '{turn_started_at}', to_jsonb(NOW()::TEXT));
        current_game_state := jsonb_set(current_game_state, '{last_action}', '"auto_end_turn_timeout"');
        
        -- Handle start of turn for next player
        IF is_player1_turn THEN
            -- Player 1's turn: Update mana, reset hero power, draw card
            DECLARE
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
                current_hero := current_game_state->'player1_hero';
                current_max_mana := COALESCE((current_hero->>'max_mana')::INTEGER, 1);
                new_max_mana := LEAST(current_max_mana + 1, 10);
                
                -- Update hero mana and reset hero power
                current_hero := jsonb_set(current_hero, '{max_mana}', to_jsonb(new_max_mana));
                current_hero := jsonb_set(current_hero, '{mana}', to_jsonb(new_max_mana));
                current_hero := jsonb_set(current_hero, '{hero_power_used_this_turn}', 'false');
                current_game_state := jsonb_set(current_game_state, '{player1_hero}', current_hero);
                
                -- Handle card draw or fatigue
                current_deck := current_game_state->'player1_deck';
                current_hand := current_game_state->'hands'->'player1_hand';
                
                IF jsonb_array_length(current_deck) > 0 THEN
                    -- Draw a card
                    drawn_card := current_deck->0;
                    remaining_deck := current_deck - 0;
                    new_hand := current_hand || jsonb_build_array(drawn_card);
                    current_game_state := jsonb_set(current_game_state, '{player1_deck}', remaining_deck);
                    current_game_state := jsonb_set(current_game_state, '{hands,player1_hand}', new_hand);
                ELSE
                    -- Fatigue damage
                    fatigue_damage := COALESCE((current_hero->>'fatigue_damage')::INTEGER, 0) + 1;
                    current_hp := COALESCE((current_hero->>'hp')::INTEGER, 30);
                    new_hp := GREATEST(0, current_hp - fatigue_damage);
                    current_hero := jsonb_set(current_hero, '{fatigue_damage}', to_jsonb(fatigue_damage));
                    current_hero := jsonb_set(current_hero, '{hp}', to_jsonb(new_hp));
                    current_game_state := jsonb_set(current_game_state, '{player1_hero}', current_hero);
                END IF;
            END;
        ELSE
            -- Player 2's turn: Update mana, reset hero power, draw card
            DECLARE
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
                current_hero := current_game_state->'player2_hero';
                current_max_mana := COALESCE((current_hero->>'max_mana')::INTEGER, 1);
                new_max_mana := LEAST(current_max_mana + 1, 10);
                
                -- Update hero mana and reset hero power
                current_hero := jsonb_set(current_hero, '{max_mana}', to_jsonb(new_max_mana));
                current_hero := jsonb_set(current_hero, '{mana}', to_jsonb(new_max_mana));
                current_hero := jsonb_set(current_hero, '{hero_power_used_this_turn}', 'false');
                current_game_state := jsonb_set(current_game_state, '{player2_hero}', current_hero);
                
                -- Handle card draw or fatigue
                current_deck := current_game_state->'player2_deck';
                current_hand := current_game_state->'hands'->'player2_hand';
                
                IF jsonb_array_length(current_deck) > 0 THEN
                    -- Draw a card
                    drawn_card := current_deck->0;
                    remaining_deck := current_deck - 0;
                    new_hand := current_hand || jsonb_build_array(drawn_card);
                    current_game_state := jsonb_set(current_game_state, '{player2_deck}', remaining_deck);
                    current_game_state := jsonb_set(current_game_state, '{hands,player2_hand}', new_hand);
                ELSE
                    -- Fatigue damage
                    fatigue_damage := COALESCE((current_hero->>'fatigue_damage')::INTEGER, 0) + 1;
                    current_hp := COALESCE((current_hero->>'hp')::INTEGER, 30);
                    new_hp := GREATEST(0, current_hp - fatigue_damage);
                    current_hero := jsonb_set(current_hero, '{fatigue_damage}', to_jsonb(fatigue_damage));
                    current_hero := jsonb_set(current_hero, '{hp}', to_jsonb(new_hp));
                    current_game_state := jsonb_set(current_game_state, '{player2_hero}', current_hero);
                END IF;
            END;
        END IF;
        
        RAISE LOG 'Turn switched to player % for battle %', next_player_id, battle_id;
    END IF;
    
    -- Update the battle session
    UPDATE battle_sessions 
    SET 
        game_state = current_game_state,
        updated_at = NOW()
    WHERE id = battle_id;
    
    RAISE LOG 'Battle % timer updated successfully', battle_id;
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE LOG 'Error processing timer for battle %: %', battle_id, SQLERRM;
END;
$$;

-- Create a function to process all active battle timers
CREATE OR REPLACE FUNCTION process_all_battle_timers()
RETURNS void
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_id UUID;
    processed_count INTEGER := 0;
    error_count INTEGER := 0;
BEGIN
    RAISE LOG 'Starting autonomous timer processing at %', NOW();
    
    -- Process each active battle individually
    FOR battle_id IN 
        SELECT id 
        FROM battle_sessions 
        WHERE status = 'active' 
        AND (game_state->>'current_turn_player_id') IS NOT NULL
        AND (game_state->>'turn_time_remaining')::INTEGER > 0
    LOOP
        BEGIN
            PERFORM process_battle_timer(battle_id);
            processed_count := processed_count + 1;
        EXCEPTION
            WHEN OTHERS THEN
                error_count := error_count + 1;
                RAISE LOG 'Error processing battle %: %', battle_id, SQLERRM;
        END;
    END LOOP;
    
    RAISE LOG 'Timer processing complete: % battles processed, % errors', processed_count, error_count;
END;
$$;

-- Create a table to track timer jobs (for monitoring)
CREATE TABLE IF NOT EXISTS battle_timer_logs (
    id SERIAL PRIMARY KEY,
    processed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    battles_processed INTEGER DEFAULT 0,
    errors_count INTEGER DEFAULT 0
);

-- Create a function that will be called by external scheduler
CREATE OR REPLACE FUNCTION run_autonomous_timer()
RETURNS void
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    processed_count INTEGER := 0;
    error_count INTEGER := 0;
BEGIN
    -- Count battles before processing
    SELECT COUNT(*) INTO processed_count
    FROM battle_sessions 
    WHERE status = 'active' 
    AND (game_state->>'current_turn_player_id') IS NOT NULL
    AND (game_state->>'turn_time_remaining')::INTEGER > 0;
    
    -- Process timers
    PERFORM process_all_battle_timers();
    
    -- Log the execution
    INSERT INTO battle_timer_logs (battles_processed, errors_count)
    VALUES (processed_count, 0);
    
EXCEPTION
    WHEN OTHERS THEN
        INSERT INTO battle_timer_logs (battles_processed, errors_count)
        VALUES (0, 1);
        RAISE LOG 'Autonomous timer execution failed: %', SQLERRM;
END;
$$;

-- Grant necessary permissions
GRANT EXECUTE ON FUNCTION process_battle_timer(UUID) TO service_role;
GRANT EXECUTE ON FUNCTION process_all_battle_timers() TO service_role;
GRANT EXECUTE ON FUNCTION run_autonomous_timer() TO service_role;

-- Insert monitoring data
COMMENT ON FUNCTION run_autonomous_timer() IS 'Autonomous timer function - call this every second via external scheduler for completely server-side timer management';
COMMENT ON TABLE battle_timer_logs IS 'Logs for monitoring autonomous timer execution';

-- Create index for efficient timer queries
CREATE INDEX IF NOT EXISTS idx_battle_sessions_active_timers 
ON battle_sessions (status, ((game_state->>'current_turn_player_id')), ((game_state->>'turn_time_remaining')::INTEGER))
WHERE status = 'active';