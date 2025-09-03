-- =====================================================
-- UPDATE BATTLE SYSTEM FOR NEW TURN-BASED CARD GAME
-- =====================================================

-- Add updated_at column to battle_sessions if it doesn't exist
ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW();

-- Ensure the game_state structure supports the new fields
-- This is handled by the application logic, but we can add some comments

COMMENT ON COLUMN public.battle_sessions.game_state IS 'Stores battle state including player hands, battlefield, phase, round count, and turn management';

-- Add deck_slot column to battle_decks if it doesn't exist (for multiple deck support)
ALTER TABLE public.battle_decks 
ADD COLUMN IF NOT EXISTS deck_slot INTEGER DEFAULT 1;

-- Update the constraint to include deck_slot
DROP CONSTRAINT IF EXISTS battle_decks_user_id_deck_position_key;
ALTER TABLE public.battle_decks 
ADD CONSTRAINT battle_decks_user_deck_position_unique 
UNIQUE(user_id, deck_position, deck_slot);

-- Add an index for better performance on deck queries
CREATE INDEX IF NOT EXISTS idx_battle_decks_user_slot ON public.battle_decks (user_id, deck_slot);

-- Update the battle_actions table to support new action types
-- This is already flexible with the action_data JSONB field

-- Create a function to initialize a battle with proper game state
CREATE OR REPLACE FUNCTION initialize_battle_game_state(
  p_battle_id UUID,
  p_player1_id UUID,
  p_player2_id UUID,
  p_first_player_id UUID
) RETURNS BOOLEAN AS $$
BEGIN
  -- Update battle session with initial game state
  UPDATE public.battle_sessions 
  SET 
    game_state = jsonb_build_object(
      'player1_deck', '[]'::jsonb,
      'player2_deck', '[]'::jsonb,
      'player1_hand', '[]'::jsonb,
      'player2_hand', '[]'::jsonb,
      'battlefield', '{}'::jsonb,
      'phase', 'waiting_first_deploy',
      'turn_count', 1,
      'round_count', 1,
      'first_player_id', p_first_player_id,
      'waiting_for_deploy_player_id', p_first_player_id,
      'actions', '[]'::jsonb
    ),
    current_turn_player_id = p_first_player_id,
    status = 'deploying',
    updated_at = NOW()
  WHERE id = p_battle_id;
  
  RETURN FOUND;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Grant permissions
GRANT EXECUTE ON FUNCTION initialize_battle_game_state(UUID, UUID, UUID, UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION initialize_battle_game_state(UUID, UUID, UUID, UUID) TO service_role;

-- Success message
SELECT 'Battle system schema updated for new turn-based mechanics!' as status;