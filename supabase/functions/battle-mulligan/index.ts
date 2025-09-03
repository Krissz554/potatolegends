import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

interface MulliganRequest {
  battleSessionId: string
  playerId: string
  cardsToRedraw: string[] // Card IDs to send back to deck and redraw
}

serve(async (req) => {
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    const supabaseClient = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    )

    const { battleSessionId, playerId, cardsToRedraw }: MulliganRequest = await req.json()

    console.log(`🃏 Processing mulligan for player ${playerId}:`, {
      battleSessionId,
      cardsToRedraw: cardsToRedraw.length
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

    const gameState = battle.game_state

    // Verify we're in mulligan phase
    if (gameState.phase !== 'mulligan') {
      throw new Error('Battle is not in mulligan phase')
    }

    // Verify player is part of this battle
    if (battle.player1_id !== playerId && battle.player2_id !== playerId) {
      throw new Error('Player not part of this battle')
    }

    // Check if player already completed mulligan
    const mulliganCompleted = gameState.mulligan_completed || {}
    if (mulliganCompleted[playerId]) {
      throw new Error('Player already completed mulligan')
    }

    const isPlayer1 = playerId === battle.player1_id
    const playerHandKey = isPlayer1 ? 'player1_hand' : 'player2_hand'
    const playerDeckKey = isPlayer1 ? 'player1_deck' : 'player2_deck'

    const currentHand = gameState.hands?.[playerHandKey] || []
    const currentDeck = gameState[playerDeckKey] || []

    // Create sets for faster lookup
    const cardIdsToRedraw = new Set(cardsToRedraw)
    
    // Validate that all cards to redraw are actually in hand
    for (const cardId of cardsToRedraw) {
      const cardInHand = currentHand.find(card => card.id === cardId)
      if (!cardInHand) {
        throw new Error(`Card ${cardId} not found in player hand`)
      }
    }

    console.log(`🔄 Player ${playerId} redrawing ${cardsToRedraw.length} cards`)

    // Separate cards to keep vs cards to redraw
    const cardsToKeep = currentHand.filter(card => !cardIdsToRedraw.has(card.id))
    const cardsBeingRedrawn = currentHand.filter(card => cardIdsToRedraw.has(card.id))

    // PROPER MULLIGAN RULES:
    // 1. Mulliganed cards go back to deck
    // 2. Replacement cards come from remaining deck (excluding mulliganed cards from the draw)
    // 3. Guarantee at least one playable card (1-2 mana)

    // Add redrawn cards back to deck
    const deckWithMulligans = [...currentDeck, ...cardsBeingRedrawn]
    
    // Simple shuffle function
    const shuffleDeck = (deck: any[]) => {
      const shuffled = [...deck]
      for (let i = shuffled.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1))
        ;[shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]]
      }
      return shuffled
    }

    const shuffledDeck = shuffleDeck(deckWithMulligans)

    // Create set of mulliganed card IDs to avoid redrawing them
    const mulliganedCardIds = new Set(cardsBeingRedrawn.map(card => card.id))
    
    // Filter deck to exclude mulliganed cards for drawing replacements
    const availableForDraw = shuffledDeck.filter(card => !mulliganedCardIds.has(card.id))
    
    // Draw replacement cards (cannot be the same as mulliganed cards)
    const newCards = availableForDraw.slice(0, cardsToRedraw.length)
    
    // Create new hand with kept cards + newly drawn cards
    let newHand = [...cardsToKeep, ...newCards]
    
    // HAND SMOOTHING: Guarantee at least one playable card (1-2 mana)
    const hasPlayableCard = newHand.some(card => (card.mana_cost || 0) <= 2)
    
    if (!hasPlayableCard && availableForDraw.length > cardsToRedraw.length) {
      // Find a low-cost card in remaining deck
      const lowCostCards = availableForDraw.slice(cardsToRedraw.length)
        .filter(card => (card.mana_cost || 0) <= 2)
      
      if (lowCostCards.length > 0) {
        // Replace the highest cost card with a low cost one
        const highestCostIndex = newHand.reduce((maxIdx, card, idx) => 
          (card.mana_cost || 0) > (newHand[maxIdx].mana_cost || 0) ? idx : maxIdx, 0)
        
        // Swap highest cost card with a random low cost card
        const randomLowCost = lowCostCards[Math.floor(Math.random() * lowCostCards.length)]
        newHand[highestCostIndex] = randomLowCost
        
        console.log(`🎯 Hand smoothing: Replaced high-cost card with ${randomLowCost.name} (${randomLowCost.mana_cost} mana)`)
      }
    }
    
    // Remove all drawn cards (including smoothing adjustments) from deck
    const drawnCardIds = new Set(newCards.map(card => card.id))
    if (!hasPlayableCard) {
      // Also remove the smoothing card if one was added
      const smoothingCard = newHand.find(card => !cardsToKeep.includes(card) && !drawnCardIds.has(card.id))
      if (smoothingCard) {
        drawnCardIds.add(smoothingCard.id)
      }
    }
    
    const finalDeck = shuffledDeck.filter(card => !drawnCardIds.has(card.id))

    console.log(`✅ Mulligan complete - Hand: ${newHand.length}, Deck: ${finalDeck.length}`)

    // Update game state
    const updatedGameState = {
      ...gameState,
      [playerDeckKey]: finalDeck,
      hands: {
        ...gameState.hands,
        [playerHandKey]: newHand
      },
      mulligan_completed: {
        ...mulliganCompleted,
        [playerId]: true
      },
      last_action: 'mulligan',
      last_action_time: new Date().toISOString()
    }

    // Check if both players completed mulligan
    const player1Done = updatedGameState.mulligan_completed[battle.player1_id]
    const player2Done = updatedGameState.mulligan_completed[battle.player2_id]

    if (player1Done && player2Done) {
      console.log('🚀 Both players completed mulligan - starting first turn!')
      
      // Randomly decide who goes first
      const firstPlayer = Math.random() < 0.5 ? battle.player1_id : battle.player2_id
      
      console.log(`🎯 First player selected: ${firstPlayer}`)

      // Transition to gameplay and start first turn
      updatedGameState.phase = 'gameplay'
      updatedGameState.current_turn_player_id = firstPlayer
      updatedGameState.turn_number = 1
      updatedGameState.turn_phase = 'start'
      updatedGameState.turn_time_remaining = 60
      updatedGameState.turn_started_at = new Date().toISOString()
      updatedGameState.first_player_id = firstPlayer

      // Execute start of first turn with proper hand sizes
      const isFirstPlayerPlayer1 = firstPlayer === battle.player1_id

      // CORRECT HEARTHSTONE RULES:
      // - First player: 4 cards (no extra card)
      // - Second player: 5 cards (gets +1 extra card for going second)
      
      if (isFirstPlayerPlayer1) {
        // Player 1 goes first - they keep their 4 cards from mulligan
        const currentHero = updatedGameState.player1_hero
        updatedGameState.player1_hero = {
          ...currentHero,
          mana: 1, // First turn starts with 1 mana
          max_mana: 1,
          hero_power_used_this_turn: false
        }
        
        // Player 2 goes second - they get an extra card (4 + 1 = 5 total)
        const currentDeck2 = updatedGameState.player2_deck
        const currentHand2 = updatedGameState.hands.player2_hand
        
        if (currentDeck2.length > 0) {
          const drawnCard = currentDeck2[0]
          updatedGameState.player2_deck = currentDeck2.slice(1)
          updatedGameState.hands.player2_hand = [...currentHand2, drawnCard]
          console.log(`🎴 Player 2 (second) gets extra card: ${drawnCard.name} (total: ${currentHand2.length + 1} cards)`)
        }
        
        console.log(`🎯 Player 1 goes first with 4 cards, Player 2 goes second with 5 cards`)
      } else {
        // Player 2 goes first - they keep their 4 cards from mulligan
        const currentHero = updatedGameState.player2_hero
        updatedGameState.player2_hero = {
          ...currentHero,
          mana: 1, // First turn starts with 1 mana
          max_mana: 1,
          hero_power_used_this_turn: false
        }
        
        // Player 1 goes second - they get an extra card (4 + 1 = 5 total)
        const currentDeck1 = updatedGameState.player1_deck
        const currentHand1 = updatedGameState.hands.player1_hand
        
        if (currentDeck1.length > 0) {
          const drawnCard = currentDeck1[0]
          updatedGameState.player1_deck = currentDeck1.slice(1)
          updatedGameState.hands.player1_hand = [...currentHand1, drawnCard]
          console.log(`🎴 Player 1 (second) gets extra card: ${drawnCard.name} (total: ${currentHand1.length + 1} cards)`)
        }
        
        console.log(`🎯 Player 2 goes first with 4 cards, Player 1 goes second with 5 cards`)
      }
    }

    // Update battle session
    const updateData: any = { 
      game_state: updatedGameState,
      updated_at: new Date().toISOString()
    }
    
    // If both players completed mulligan, also update the top-level current_turn_player_id for compatibility
    if (player1Done && player2Done && updatedGameState.current_turn_player_id) {
      updateData.current_turn_player_id = updatedGameState.current_turn_player_id
      updateData.status = 'active' // Mark battle as active when game starts
      console.log('🚀 Setting top-level current_turn_player_id to:', updatedGameState.current_turn_player_id)
    }
    
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update(updateData)
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
          name: player1Done && player2Done ? 'game_started' : 'mulligan_completed',
          data: {
            type: 'mulligan',
            playerId,
            gameStarted: player1Done && player2Done,
            firstPlayer: player1Done && player2Done ? updatedGameState.first_player_id : null,
            battleSessionId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish mulligan to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published mulligan completion to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
    }

    const responseMessage = player1Done && player2Done 
      ? 'Mulligan completed - Game started!' 
      : 'Mulligan completed - Waiting for opponent'

    console.log(`✅ ${responseMessage}`)

    return new Response(
      JSON.stringify({ 
        success: true,
        message: responseMessage,
        gameStarted: player1Done && player2Done,
        firstPlayer: player1Done && player2Done ? updatedGameState.first_player_id : null
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200 
      }
    )

  } catch (error: any) {
    console.error('❌ Mulligan error:', error)
    
    return new Response(
      JSON.stringify({ 
        success: false, 
        error: error.message || 'Mulligan failed' 
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 400 
      }
    )
  }
})