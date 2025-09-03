import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

interface HeroPowerRequest {
  battleSessionId: string
  playerId: string
  targetType: 'enemy_unit' | 'enemy_hero' | 'own_hero'
  targetSlotIndex?: number
}

serve(async (req) => {
  try {
    // CORS headers
    if (req.method === 'OPTIONS') {
      return new Response('ok', {
        headers: {
          'Access-Control-Allow-Origin': '*',
          'Access-Control-Allow-Methods': 'POST',
          'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
        },
      })
    }

    const supabaseClient = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    )

    const { battleSessionId, playerId, targetType, targetSlotIndex }: HeroPowerRequest = await req.json()

    console.log('🔥 Processing hero power:', {
      battleSessionId,
      playerId,
      targetType,
      targetSlotIndex
    })

    // Get current battle session
    const { data: battleSession, error: fetchError } = await supabaseClient
      .from('battle_sessions')
      .select('*')
      .eq('id', battleSessionId)
      .single()

    if (fetchError || !battleSession) {
      throw new Error(`Battle session not found: ${fetchError?.message}`)
    }

    const gameState = battleSession.game_state || {}
    const isPlayer1 = playerId === battleSession.player1_id

    // Validate it's the player's turn
    const currentTurnPlayerId = gameState.current_turn_player_id || battleSession.current_turn_player_id
    if (currentTurnPlayerId !== playerId) {
      throw new Error(`Not your turn. Current turn: ${currentTurnPlayerId}`)
    }

    // Get player hero state
    const playerHeroState = isPlayer1 ? gameState.player1_hero : gameState.player2_hero
    const opponentHeroState = isPlayer1 ? gameState.player2_hero : gameState.player1_hero

    if (!playerHeroState) {
      throw new Error('Player hero state not found')
    }

    // Validate hero power availability
    if (playerHeroState.hero_power_used_this_turn === true) {
      throw new Error('Hero power already used this turn')
    }

    if ((playerHeroState.mana || 0) < 2) {
      throw new Error(`Not enough mana. Need 2, have ${playerHeroState.mana || 0}`)
    }

    // Get battlefield state
    const battlefield = gameState.battlefield || {}
    const opponentSlots = isPlayer1 ? battlefield.player2_slots : battlefield.player1_slots

    let targetDescription = ''
    let damageDealt = 0
    let healingDone = 0

    // Execute hero power based on target type
    if (targetType === 'enemy_unit') {
      if (targetSlotIndex === undefined || targetSlotIndex < 0 || targetSlotIndex >= 6) {
        throw new Error('Invalid target slot index')
      }

      const targetUnit = opponentSlots?.[targetSlotIndex]
      if (!targetUnit) {
        throw new Error('No unit at target slot')
      }

      // Deal 2 damage to target unit
      const newHp = Math.max(0, targetUnit.hp - 2)
      damageDealt = targetUnit.hp - newHp

      if (newHp <= 0) {
        // Unit dies
        opponentSlots[targetSlotIndex] = null
        targetDescription = `${targetUnit.name} (destroyed)`
      } else {
        // Unit survives
        targetUnit.hp = newHp
        opponentSlots[targetSlotIndex] = targetUnit
        targetDescription = `${targetUnit.name} (${newHp}/${targetUnit.max_hp} HP)`
      }

    } else if (targetType === 'enemy_hero') {
      // Target enemy hero
      const newHp = Math.max(0, opponentHeroState.hp - 2)
      damageDealt = opponentHeroState.hp - newHp
      opponentHeroState.hp = newHp
      targetDescription = `Enemy Hero (${newHp}/${opponentHeroState.max_hp} HP)`

    } else if (targetType === 'own_hero') {
      // Heal own hero
      const newHp = Math.min(playerHeroState.max_hp, playerHeroState.hp + 2)
      healingDone = newHp - playerHeroState.hp
      playerHeroState.hp = newHp
      targetDescription = `Your Hero (${newHp}/${playerHeroState.max_hp} HP)`

    } else {
      throw new Error('Invalid target type')
    }

    // Deduct mana cost and mark hero power as used
    playerHeroState.mana = (playerHeroState.mana || 0) - 2
    playerHeroState.hero_power_used_this_turn = true

    // Update battlefield state
    if (isPlayer1) {
      gameState.battlefield.player2_slots = opponentSlots
      gameState.player1_hero = playerHeroState
      gameState.player2_hero = opponentHeroState
    } else {
      gameState.battlefield.player1_slots = opponentSlots
      gameState.player2_hero = playerHeroState
      gameState.player1_hero = opponentHeroState
    }

    // Update battle session
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: gameState,
        updated_at: new Date().toISOString()
      })
      .eq('id', battleSessionId)

    if (updateError) {
      console.error('❌ Error updating battle session:', updateError)
      throw new Error('Failed to update battle session')
    }

    console.log('✅ Hero power used successfully')

    return new Response(JSON.stringify({ 
      success: true,
      message: `Royal Decree used on ${targetDescription}`,
      damageDealt,
      healingDone
    }), {
      status: 200,
      headers: {
        'Content-Type': 'application/json',
        'Access-Control-Allow-Origin': '*',
      }
    })

  } catch (error) {
    console.error('❌ Hero power error:', error)
    return new Response(JSON.stringify({ 
      error: error.message 
    }), {
      status: 400,
      headers: {
        'Content-Type': 'application/json',
        'Access-Control-Allow-Origin': '*',
      }
    })
  }
})