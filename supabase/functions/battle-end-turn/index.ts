import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

interface EndTurnRequest {
  battleSessionId: string
  playerId: string
  isAutoEnd?: boolean
}

serve(async (req) => {
  // Handle CORS preflight requests
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    const supabaseClient = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    )

    const { battleSessionId, playerId, isAutoEnd = false }: EndTurnRequest = await req.json()

    console.log(`🏁 Processing end turn request:`, {
      battleSessionId,
      playerId,
      isAutoEnd
    })

    // Get current battle session
    const { data: battle, error: battleError } = await supabaseClient
      .from('battle_sessions')
      .select('*')
      .eq('id', battleSessionId)
      .single()

    if (battleError || !battle) {
      throw new Error(`Battle not found: ${battleError?.message}`)
    }

    // Verify it's the player's turn
    if (battle.game_state?.current_turn_player_id !== playerId) {
      throw new Error('Not your turn!')
    }

    // Get current game state
    const gameState = battle.game_state

    // Determine next player
    const nextPlayerId = gameState.current_turn_player_id === battle.player1_id 
      ? battle.player2_id 
      : battle.player1_id

    console.log(`🔄 Switching turn from ${playerId} to ${nextPlayerId}`)

    // Update turn counter and current player
    const newTurnNumber = (gameState.turn_number || 1) + 1
    const isPlayer1Turn = nextPlayerId === battle.player1_id

    // Execute start of turn for next player
    const updatedGameState = {
      ...gameState,
      current_turn_player_id: nextPlayerId,
      turn_number: newTurnNumber,
      turn_phase: 'start',
      turn_time_remaining: 60, // Reset timer to 60 seconds
      turn_started_at: new Date().toISOString(),
      last_action: isAutoEnd ? 'auto_end_turn' : 'end_turn',
      last_action_time: new Date().toISOString()
    }

    // Start of turn logic for next player
    if (isPlayer1Turn) {
      // Player 1's turn
      const currentHero = updatedGameState.player1_hero
      const currentMana = currentHero?.mana || 0
      const currentMaxMana = currentHero?.max_mana || 0
      
      // +1 max mana (up to 10)
      const newMaxMana = Math.min(currentMaxMana + 1, 10)
      
      updatedGameState.player1_hero = {
        ...currentHero,
        mana: newMaxMana, // Refresh to max
        max_mana: newMaxMana,
        hero_power_used_this_turn: false // Reset hero power
      }

      // Draw a card (if deck has cards)
      const currentDeck = updatedGameState.player1_deck || []
      const currentHand = updatedGameState.hands?.player1_hand || []
      
      if (currentDeck.length > 0) {
        const drawnCard = currentDeck[0]
        const remainingDeck = currentDeck.slice(1)
        const newHand = [...currentHand, drawnCard]
        
        updatedGameState.player1_deck = remainingDeck
        updatedGameState.hands = {
          ...updatedGameState.hands,
          player1_hand: newHand
        }
        
        console.log(`🎴 Player 1 drew card: ${drawnCard.name}`)
      } else {
        // Fatigue damage
        const fatigueDamage = (currentHero?.fatigue_damage || 0) + 1
        updatedGameState.player1_hero = {
          ...updatedGameState.player1_hero,
          hp: Math.max(0, currentHero.hp - fatigueDamage),
          fatigue_damage: fatigueDamage
        }
        console.log(`💀 Player 1 takes ${fatigueDamage} fatigue damage`)
      }
    } else {
      // Player 2's turn
      const currentHero = updatedGameState.player2_hero
      const currentMana = currentHero?.mana || 0
      const currentMaxMana = currentHero?.max_mana || 0
      
      // +1 max mana (up to 10)
      const newMaxMana = Math.min(currentMaxMana + 1, 10)
      
      updatedGameState.player2_hero = {
        ...currentHero,
        mana: newMaxMana, // Refresh to max
        max_mana: newMaxMana,
        hero_power_used_this_turn: false // Reset hero power
      }

      // Draw a card (if deck has cards)
      const currentDeck = updatedGameState.player2_deck || []
      const currentHand = updatedGameState.hands?.player2_hand || []
      
      if (currentDeck.length > 0) {
        const drawnCard = currentDeck[0]
        const remainingDeck = currentDeck.slice(1)
        const newHand = [...currentHand, drawnCard]
        
        updatedGameState.player2_deck = remainingDeck
        updatedGameState.hands = {
          ...updatedGameState.hands,
          player2_hand: newHand
        }
        
        console.log(`🎴 Player 2 drew card: ${drawnCard.name}`)
      } else {
        // Fatigue damage
        const fatigueDamage = (currentHero?.fatigue_damage || 0) + 1
        updatedGameState.player2_hero = {
          ...updatedGameState.player2_hero,
          hp: Math.max(0, currentHero.hp - fatigueDamage),
          fatigue_damage: fatigueDamage
        }
        console.log(`💀 Player 2 takes ${fatigueDamage} fatigue damage`)
      }
    }

    // Automatically process start turn for next player
    console.log('🌅 Auto-processing start turn for next player:', nextPlayerId)
    
    // 1. Increase max mana (up to 10)
    const playerKey = nextPlayerId === battle.player1_id ? 'player1' : 'player2'
    const heroKey = `${playerKey}_hero`
    
    if (updatedGameState[heroKey].max_mana < 10) {
      updatedGameState[heroKey].max_mana += 1
    }
    
    // 2. Refresh mana to full
    updatedGameState[heroKey].mana = updatedGameState[heroKey].max_mana
    
    // 3. Reset hero power for new turn
    updatedGameState[heroKey].hero_power_used_this_turn = false
    
    // 4. Reset attack flags for ALL units on battlefield (both players)
    if (updatedGameState.battlefield?.player1_slots) {
      updatedGameState.battlefield.player1_slots.forEach((unit: any) => {
        if (unit) {
          unit.has_attacked_this_turn = false
        }
      })
    }
    
    if (updatedGameState.battlefield?.player2_slots) {
      updatedGameState.battlefield.player2_slots.forEach((unit: any) => {
        if (unit) {
          unit.has_attacked_this_turn = false
        }
      })
    }
    
    // 5. Draw a card (with fatigue check) - Skip on turn 1
    if (newTurnNumber > 1) {
      const deckKey = `${playerKey}_deck`
      const handKey = `${playerKey}_hand`
      
      if (updatedGameState[deckKey] && updatedGameState[deckKey].length > 0) {
        // Draw from deck
        const drawnCard = updatedGameState[deckKey].shift()
        if (updatedGameState[handKey]) {
          updatedGameState[handKey].push(drawnCard)
        }
        console.log('🃏 Card drawn from deck for new turn')
      } else {
        // Fatigue damage
        const fatigueDamage = (updatedGameState[heroKey].fatigue_damage || 0) + 1
        updatedGameState[heroKey].fatigue_damage = fatigueDamage
        updatedGameState[heroKey].hp = Math.max(0, updatedGameState[heroKey].hp - fatigueDamage)
        console.log(`💀 Fatigue damage: ${fatigueDamage}`)
      }
    }
    
    // 6. Automatically transition to main phase (start actions complete)
    updatedGameState.turn_phase = 'main'
    updatedGameState.phase = 'gameplay'
    console.log('➡️ Auto-transitioned to main phase - player can now take actions')

    // Update battle session - sync both game_state and top-level current_turn_player_id
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({ 
        game_state: updatedGameState,
        current_turn_player_id: nextPlayerId, // Sync with game state
        updated_at: new Date().toISOString()
      })
      .eq('id', battleSessionId)

    if (updateError) {
      throw new Error(`Failed to update battle: ${updateError.message}`)
    }

    // Publish to Ably for real-time updates
    try {
      const ablyResponse = await fetch('https://rest.ably.io/channels/match:' + battleSessionId + '/messages', {
        method: 'POST',
        headers: {
          'Authorization': 'Basic ' + btoa('1Vf05w.bzqjGQ:6YuVnRKLTCKDlHR-tDzDZPGfA5pBP57vWP4nFRrFXm0'),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'turn_change',
          data: {
            type: 'turn_end',
            newTurn: newTurnNumber,
            currentPlayer: nextPlayerId,
            turnPhase: 'main', // Now in main phase after auto-processing start
            battleSessionId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish turn change to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published turn change to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
    }

    console.log(`✅ Turn ended successfully. New turn: ${newTurnNumber}`)

    return new Response(
      JSON.stringify({ 
        success: true, 
        newTurnPlayer: nextPlayerId,
        turnNumber: newTurnNumber,
        message: isAutoEnd ? 'Turn ended automatically' : 'Turn ended successfully'
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200 
      }
    )

  } catch (error: any) {
    console.error('❌ End turn error:', error)
    
    return new Response(
      JSON.stringify({ 
        success: false, 
        error: error.message || 'Failed to end turn' 
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 400 
      }
    )
  }
})