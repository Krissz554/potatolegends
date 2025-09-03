-- FULLY REAL-TIME SERVER-SIDE TIMER SYSTEM
-- Uses database timestamps and real-time calculations for perfect synchronization

-- Drop old timer functions
DROP FUNCTION IF EXISTS get_current_timer_value(TIMESTAMP WITH TIME ZONE, INTEGER);
DROP FUNCTION IF EXISTS auto_end_expired_turns();
DROP FUNCTION IF EXISTS update_timer_displays();

-- Function to calculate real-time timer value
CREATE OR REPLACE FUNCTION get_realtime_timer(
    turn_started_at TIMESTAMP WITH TIME ZONE
)
RETURNS INTEGER
LANGUAGE SQL
STABLE
AS $$
    SELECT CASE 
        WHEN turn_started_at IS NULL THEN 60
        ELSE GREATEST(0, 60 - EXTRACT(EPOCH FROM (NOW() - turn_started_at))::INTEGER)
    END;
$$;

-- Function to check if a turn has expired and auto-end it
CREATE OR REPLACE FUNCTION check_and_auto_end_turn(battle_id UUID)
RETURNS TABLE(turn_ended BOOLEAN, new_timer INTEGER)
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
BEGIN
    -- Get the battle session
    SELECT id, player1_id, player2_id, game_state, status
    INTO battle_record
    FROM battle_sessions
    WHERE id = battle_id AND status = 'active';

    IF NOT FOUND THEN
        RETURN QUERY SELECT FALSE, 60;
        RETURN;
    END IF;

    current_game_state := battle_record.game_state;
    
    -- Check if there's an active turn
    current_player_id := current_game_state->>'current_turn_player_id';
    IF current_player_id IS NULL THEN
        RETURN QUERY SELECT FALSE, 60;
        RETURN;
    END IF;

    -- Get turn start time
    turn_start_time := (current_game_state->>'turn_started_at')::TIMESTAMP WITH TIME ZONE;
    IF turn_start_time IS NULL THEN
        RETURN QUERY SELECT FALSE, 60;
        RETURN;
    END IF;

    -- Calculate elapsed time
    elapsed_seconds := EXTRACT(EPOCH FROM (NOW() - turn_start_time))::INTEGER;

    -- If turn hasn't expired yet, return current timer
    IF elapsed_seconds < 60 THEN
        RETURN QUERY SELECT FALSE, GREATEST(0, 60 - elapsed_seconds);
        RETURN;
    END IF;

    -- Turn has expired - auto-end it
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
        -- Player 1's turn: increment mana, reset hero power, draw card
        DECLARE
            current_hero JSONB := updated_game_state->'player1_hero';
            current_max_mana INTEGER := COALESCE((current_hero->>'max_mana')::INTEGER, 0);
            new_max_mana INTEGER := LEAST(current_max_mana + 1, 10);
            current_deck JSONB := updated_game_state->'player1_deck';
            current_hand JSONB := updated_game_state->'hands'->'player1_hand';
        BEGIN
            -- Update hero mana and power
            current_hero := jsonb_set(current_hero, '{max_mana}', to_jsonb(new_max_mana));
            current_hero := jsonb_set(current_hero, '{mana}', to_jsonb(new_max_mana));
            current_hero := jsonb_set(current_hero, '{hero_power_used_this_turn}', 'false');
            updated_game_state := jsonb_set(updated_game_state, '{player1_hero}', current_hero);

            -- Draw card if deck has cards
            IF jsonb_array_length(current_deck) > 0 THEN
                DECLARE
                    drawn_card JSONB := current_deck->0;
                    remaining_deck JSONB := current_deck - 0;
                    new_hand JSONB := current_hand || jsonb_build_array(drawn_card);
                BEGIN
                    updated_game_state := jsonb_set(updated_game_state, '{player1_deck}', remaining_deck);
                    updated_game_state := jsonb_set(updated_game_state, '{hands,player1_hand}', new_hand);
                END;
            ELSE
                -- Handle fatigue
                DECLARE
                    fatigue_damage INTEGER := COALESCE((current_hero->>'fatigue_damage')::INTEGER, 0) + 1;
                    current_hp INTEGER := COALESCE((current_hero->>'hp')::INTEGER, 30);
                    new_hp INTEGER := GREATEST(0, current_hp - fatigue_damage);
                BEGIN
                    current_hero := jsonb_set(current_hero, '{fatigue_damage}', to_jsonb(fatigue_damage));
                    current_hero := jsonb_set(current_hero, '{hp}', to_jsonb(new_hp));
                    updated_game_state := jsonb_set(updated_game_state, '{player1_hero}', current_hero);
                END;
            END IF;
        END;
    ELSE
        -- Player 2's turn: same logic but for player2
        DECLARE
            current_hero JSONB := updated_game_state->'player2_hero';
            current_max_mana INTEGER := COALESCE((current_hero->>'max_mana')::INTEGER, 0);
            new_max_mana INTEGER := LEAST(current_max_mana + 1, 10);
            current_deck JSONB := updated_game_state->'player2_deck';
            current_hand JSONB := updated_game_state->'hands'->'player2_hand';
        BEGIN
            -- Update hero mana and power
            current_hero := jsonb_set(current_hero, '{max_mana}', to_jsonb(new_max_mana));
            current_hero := jsonb_set(current_hero, '{mana}', to_jsonb(new_max_mana));
            current_hero := jsonb_set(current_hero, '{hero_power_used_this_turn}', 'false');
            updated_game_state := jsonb_set(updated_game_state, '{player2_hero}', current_hero);

            -- Draw card if deck has cards
            IF jsonb_array_length(current_deck) > 0 THEN
                DECLARE
                    drawn_card JSONB := current_deck->0;
                    remaining_deck JSONB := current_deck - 0;
                    new_hand JSONB := current_hand || jsonb_build_array(drawn_card);
                BEGIN
                    updated_game_state := jsonb_set(updated_game_state, '{player2_deck}', remaining_deck);
                    updated_game_state := jsonb_set(updated_game_state, '{hands,player2_hand}', new_hand);
                END;
            ELSE
                -- Handle fatigue
                DECLARE
                    fatigue_damage INTEGER := COALESCE((current_hero->>'fatigue_damage')::INTEGER, 0) + 1;
                    current_hp INTEGER := COALESCE((current_hero->>'hp')::INTEGER, 30);
                    new_hp INTEGER := GREATEST(0, current_hp - fatigue_damage);
                BEGIN
                    current_hero := jsonb_set(current_hero, '{fatigue_damage}', to_jsonb(fatigue_damage));
                    current_hero := jsonb_set(current_hero, '{hp}', to_jsonb(new_hp));
                    updated_game_state := jsonb_set(updated_game_state, '{player2_hero}', current_hero);
                END;
            END IF;
        END;
    END IF;

    -- Update the battle session
    UPDATE battle_sessions
    SET 
        game_state = updated_game_state,
        updated_at = NOW()
    WHERE id = battle_id;

    -- Return that turn ended with new timer
    RETURN QUERY SELECT TRUE, 60;
END;
$$;

-- Create a view that shows real-time timer values for all active battles
CREATE OR REPLACE VIEW active_battles_with_timer AS
SELECT 
    bs.id,
    bs.player1_id,
    bs.player2_id,
    bs.status,
    bs.game_state,
    bs.created_at,
    bs.updated_at,
    CASE 
        WHEN (bs.game_state->>'turn_started_at') IS NULL THEN 60
        ELSE get_realtime_timer((bs.game_state->>'turn_started_at')::TIMESTAMP WITH TIME ZONE)
    END as current_timer,
    (bs.game_state->>'current_turn_player_id') as current_turn_player_id,
    (bs.game_state->>'turn_number')::INTEGER as turn_number
FROM battle_sessions bs
WHERE bs.status = 'active';

-- Grant permissions
GRANT EXECUTE ON FUNCTION get_realtime_timer(TIMESTAMP WITH TIME ZONE) TO authenticated;
GRANT EXECUTE ON FUNCTION get_realtime_timer(TIMESTAMP WITH TIME ZONE) TO service_role;
GRANT EXECUTE ON FUNCTION check_and_auto_end_turn(UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION check_and_auto_end_turn(UUID) TO service_role;
GRANT SELECT ON active_battles_with_timer TO authenticated;
GRANT SELECT ON active_battles_with_timer TO service_role;