-- Alternative Timer System using Database Functions
-- This creates a more reliable timer system that doesn't depend on pg_cron

-- Create a function to handle timer decrements
CREATE OR REPLACE FUNCTION decrement_turn_timers()
RETURNS void
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    battle_record RECORD;
    updated_game_state JSONB;
    current_player_id TEXT;
    next_player_id TEXT;
    new_turn_number INTEGER;
    is_player1_turn BOOLEAN;
BEGIN
    -- Process all active battles with active turns
    FOR battle_record IN 
        SELECT id, player1_id, player2_id, game_state, updated_at
        FROM battle_sessions 
        WHERE status = 'active' 
        AND (game_state->>'current_turn_player_id') IS NOT NULL
        AND (game_state->>'turn_time_remaining')::INTEGER > 0
    LOOP
        -- Decrement timer
        updated_game_state := jsonb_set(
            battle_record.game_state,
            '{turn_time_remaining}',
            ((battle_record.game_state->>'turn_time_remaining')::INTEGER - 1)::TEXT::JSONB
        );
        
        -- Check if timer reached 0
        IF (updated_game_state->>'turn_time_remaining')::INTEGER <= 0 THEN
            -- Auto-end turn
            current_player_id := battle_record.game_state->>'current_turn_player_id';
            next_player_id := CASE 
                WHEN current_player_id = battle_record.player1_id 
                THEN battle_record.player2_id 
                ELSE battle_record.player1_id 
            END;
            
            new_turn_number := COALESCE((battle_record.game_state->>'turn_number')::INTEGER, 1) + 1;
            is_player1_turn := (next_player_id = battle_record.player1_id);
            
            -- Update game state for turn switch
            updated_game_state := jsonb_set(updated_game_state, '{current_turn_player_id}', to_jsonb(next_player_id));
            updated_game_state := jsonb_set(updated_game_state, '{turn_number}', to_jsonb(new_turn_number));
            updated_game_state := jsonb_set(updated_game_state, '{turn_time_remaining}', '60');
            updated_game_state := jsonb_set(updated_game_state, '{turn_phase}', '"start"');
            updated_game_state := jsonb_set(updated_game_state, '{last_action}', '"auto_end_turn_timeout"');
            updated_game_state := jsonb_set(updated_game_state, '{last_action_time}', to_jsonb(NOW()::TEXT));
            
            -- Handle start of turn for next player (simplified version)
            IF is_player1_turn THEN
                -- Player 1's turn - increase mana and reset hero power
                updated_game_state := jsonb_set(
                    updated_game_state, 
                    '{player1_hero,max_mana}', 
                    to_jsonb(LEAST(COALESCE((updated_game_state->'player1_hero'->>'max_mana')::INTEGER, 0) + 1, 10))
                );
                updated_game_state := jsonb_set(
                    updated_game_state, 
                    '{player1_hero,mana}', 
                    updated_game_state->'player1_hero'->'max_mana'
                );
                updated_game_state := jsonb_set(
                    updated_game_state, 
                    '{player1_hero,hero_power_used_this_turn}', 
                    'false'
                );
            ELSE
                -- Player 2's turn - increase mana and reset hero power
                updated_game_state := jsonb_set(
                    updated_game_state, 
                    '{player2_hero,max_mana}', 
                    to_jsonb(LEAST(COALESCE((updated_game_state->'player2_hero'->>'max_mana')::INTEGER, 0) + 1, 10))
                );
                updated_game_state := jsonb_set(
                    updated_game_state, 
                    '{player2_hero,mana}', 
                    updated_game_state->'player2_hero'->'max_mana'
                );
                updated_game_state := jsonb_set(
                    updated_game_state, 
                    '{player2_hero,hero_power_used_this_turn}', 
                    'false'
                );
            END IF;
        END IF;
        
        -- Update the battle session
        UPDATE battle_sessions 
        SET 
            game_state = updated_game_state,
            updated_at = NOW()
        WHERE id = battle_record.id;
        
    END LOOP;
END;
$$;

-- Grant execute permissions
GRANT EXECUTE ON FUNCTION decrement_turn_timers() TO authenticated;
GRANT EXECUTE ON FUNCTION decrement_turn_timers() TO service_role;

-- Create a simple way to call this function
COMMENT ON FUNCTION decrement_turn_timers() IS 'Call this function every second to handle turn timers. Can be called from client-side or external cron job.';