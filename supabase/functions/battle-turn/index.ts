import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

async function updateHeroBattleStats(supabaseClient: any, winnerId: string, loserId: string, player1Id: string, player2Id: string, player1Hero: any, player2Hero: any) {
  try {
    // Update winner's hero stats
    if (winnerId === player1Id) {
      await supabaseClient.rpc('update_hero_battle_stats', {
        user_uuid: player1Id,
        hero_id_param: player1Hero.id,
        won: true
      })
      await supabaseClient.rpc('update_hero_battle_stats', {
        user_uuid: player2Id,
        hero_id_param: player2Hero.id,
        won: false
      })
      console.log(`📊 Battle stats updated: ${player1Hero.name} wins, ${player2Hero.name} loses`)
    } else {
      await supabaseClient.rpc('update_hero_battle_stats', {
        user_uuid: player2Id,
        hero_id_param: player2Hero.id,
        won: true
      })
      await supabaseClient.rpc('update_hero_battle_stats', {
        user_uuid: player1Id,
        hero_id_param: player1Hero.id,
        won: false
      })
      console.log(`📊 Battle stats updated: ${player2Hero.name} wins, ${player1Hero.name} loses`)
    }
  } catch (error) {
    console.error('Error updating hero battle stats:', error)
    // Don't throw - game should still end even if stats update fails
  }
}

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

interface TurnAction {
  battleId: string
  action: 'start_turn' | 'end_turn' | 'draw_card'
}

serve(async (req) => {
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    console.log('🎯 BATTLE-TURN FUNCTION CALLED - VERSION 2.0')
    
    // Create Supabase client
    const supabaseClient = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? '',
      {
        auth: {
          autoRefreshToken: false,
          persistSession: false
        }
      }
    )

    // Get user from auth header
    const authHeader = req.headers.get('authorization')
    if (!authHeader) {
      throw new Error('No authorization header')
    }

    const { data: { user }, error: userError } = await supabaseClient.auth.getUser(authHeader.replace('Bearer ', ''))
    if (userError || !user) {
      throw new Error('Invalid user')
    }

    // Parse request with detailed logging
    const requestBody = await req.json()
    console.log('📦 Raw request body:', JSON.stringify(requestBody))
    
    const { battleId, action }: TurnAction = requestBody

    console.log(`🎮 Processing turn action: ${action} for battle: ${battleId}`)
    
    // Validate required fields
    if (!battleId) {
      throw new Error('Missing battleId in request')
    }
    
    if (!action) {
      throw new Error('Missing action in request')
    }
    
    if (!['start_turn', 'end_turn', 'draw_card'].includes(action)) {
      throw new Error(`Invalid action: ${action}`)
    }

    // Get battle session
    const { data: battle, error: battleError } = await supabaseClient
      .from('battle_sessions')
      .select('*')
      .eq('id', battleId)
      .single()

    if (battleError || !battle) {
      throw new Error('Battle not found')
    }

    // Verify user is part of battle
    if (battle.player1_id !== user.id && battle.player2_id !== user.id) {
      throw new Error('User not part of this battle')
    }

    const gameState = battle.game_state
    const isPlayer1 = battle.player1_id === user.id

    // Process turn action
    let updatedGameState = { ...gameState }

    switch (action) {
      case 'start_turn':
        updatedGameState = await processStartTurn(updatedGameState, user.id, supabaseClient, battle)
        break
      case 'end_turn':
        updatedGameState = await processEndTurn(updatedGameState, user.id, supabaseClient, battle)
        break
      case 'draw_card':
        updatedGameState = await processDrawCard(updatedGameState, user.id, supabaseClient, battle)
        break
      default:
        throw new Error(`Unknown turn action: ${action}`)
    }

    // Update battle state
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: updatedGameState,
        current_turn_player_id: updatedGameState.current_turn_player_id,
        updated_at: new Date().toISOString()
      })
      .eq('id', battleId)

    if (updateError) {
      throw new Error(`Failed to update battle state: ${updateError.message}`)
    }

    // Publish to Ably for real-time updates
    try {
      const ablyResponse = await fetch('https://rest.ably.io/channels/match:' + battleId + '/messages', {
        method: 'POST',
        headers: {
          'Authorization': 'Basic ' + btoa('1Vf05w.bzqjGQ:6YuVnRKLTCKDlHR-tDzDZPGfA5pBP57vWP4nFRrFXm0'),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'turn_action',
          data: {
            type: action,
            playerId: user.id,
            currentPlayer: updatedGameState.current_turn_player_id,
            turnPhase: updatedGameState.turn_phase,
            battleSessionId: battleId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish turn action to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published turn action to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
    }

    // Log action
    await supabaseClient
      .from('battle_actions')
      .insert({
        battle_session_id: battleId,
        player_id: user.id,
        action_type: action,
        action_data: { action, timestamp: new Date().toISOString() }
      })

    return new Response(
      JSON.stringify({ success: true, gameState: updatedGameState }),
      { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
    )

  } catch (error) {
    console.error('Turn action error:', error)
    return new Response(
      JSON.stringify({ success: false, error: error.message }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 400 
      }
    )
  }
})

// Process start of turn phase
async function processStartTurn(gameState: any, playerId: string, supabaseClient: any, battle: any) {
  console.log('🌅 Processing start turn for player:', playerId)
  
  if (gameState.current_turn_player_id !== playerId) {
    throw new Error('Not your turn')
  }

  if (gameState.turn_phase !== 'start') {
    throw new Error('Not in start turn phase')
  }

  const isPlayer1 = battle.player1_id === playerId
  const playerKey = isPlayer1 ? 'player1' : 'player2'
  const heroKey = `${playerKey}_hero`
  
  // 1. Increase max mana (up to 10) - Always increase on turn start
  if (gameState[heroKey].max_mana < 10) {
    gameState[heroKey].max_mana += 1
  }
  
  // 2. Refresh mana to full
  gameState[heroKey].mana = gameState[heroKey].max_mana
  
  // 3. Reset hero power for new turn
  gameState[heroKey].hero_power_used_this_turn = false
  
  // 4. Reset attack flags for ALL units on battlefield (both players)
  // This ensures units can attack every turn they're alive
  if (gameState.battlefield?.player1_slots) {
    gameState.battlefield.player1_slots.forEach((unit: any) => {
      if (unit) {
        unit.has_attacked_this_turn = false
        unit.attacks_this_turn = 0
        unit.has_double_struck = false // Reset double strike flag
        console.log(`🔄 Reset attack flags for Player 1 unit: ${unit.name}`)
      }
    })
  }
  
  if (gameState.battlefield?.player2_slots) {
    gameState.battlefield.player2_slots.forEach((unit: any) => {
      if (unit) {
        unit.has_attacked_this_turn = false
        unit.attacks_this_turn = 0
        unit.has_double_struck = false // Reset double strike flag
        console.log(`🔄 Reset attack flags for Player 2 unit: ${unit.name}`)
      }
    })
  }
  
  // 5. Process freeze duration - unfreeze units controlled by current player
  const playerSlots = isPlayer1 ? gameState.battlefield?.player1_slots : gameState.battlefield?.player2_slots
  if (playerSlots) {
    playerSlots.forEach((unit: any) => {
      if (unit && unit.is_frozen) {
        // Reduce freeze duration
        if (unit.frozen_turns_remaining) {
          unit.frozen_turns_remaining -= 1
          console.log(`🧊 ${unit.name} freeze duration: ${unit.frozen_turns_remaining} turns remaining`)
          
          // Unfreeze if duration expired
          if (unit.frozen_turns_remaining <= 0) {
            unit.is_frozen = false
            unit.frozen_turns_remaining = 0
            console.log(`❄️ ${unit.name} is no longer frozen!`)
          }
        } else {
          // Legacy freeze (no duration tracking) - unfreeze after 1 turn
          unit.is_frozen = false
          console.log(`❄️ ${unit.name} is no longer frozen (legacy)!`)
        }
      }
    })
  }
  
  console.log(`⚡ Mana refreshed: ${gameState[heroKey].mana}/${gameState[heroKey].max_mana}`)
  console.log(`👑 Hero power reset for new turn`)
  console.log(`⚔️ Attack flags reset for all units`)
  
  // 6. Process turn-based aura effects (structures/relics with turn triggers)
  await processTurnBasedAuras(gameState, playerKey)
  
  // 7. Process turn-start triggered abilities
  await processTurnStartTriggers(gameState, playerKey)
  
  // 4. Draw a card (with fatigue check) - Skip on turn 1 since starting hand already drawn
  if (gameState.turn_count > 1) {
    const deckKey = `${playerKey}_deck`
    const handKey = `${playerKey}_hand`
    
    if (gameState[deckKey].length > 0) {
      // Check hand size limit (10 cards max)
      if (gameState[handKey].length >= 10) {
        // Hand is full - burn the card (discard without effect)
        const burnedCard = gameState[deckKey].shift()
        console.log(`🔥 Card burned due to full hand (10/10): ${burnedCard?.name || 'Unknown Card'}`)
      } else {
        // Draw from deck normally
        const drawnCard = gameState[deckKey].shift()
        gameState[handKey].push(drawnCard)
        console.log('🃏 Card drawn from deck')
      }
    } else {
      // Fatigue damage
      gameState[heroKey].fatigue_damage += 1
      gameState[heroKey].hp -= gameState[heroKey].fatigue_damage
      console.log(`💀 Fatigue damage: ${gameState[heroKey].fatigue_damage} (HP: ${gameState[heroKey].hp})`)
      
      // Check if player died from fatigue
      if (gameState[heroKey].hp <= 0) {
        gameState.game_status = 'finished'
        gameState.winner_id = gameState.current_turn_player_id === battle.player1_id ? battle.player2_id : battle.player1_id
        gameState.phase = 'game_over'
        console.log('💀 Player died from fatigue!')
        return gameState
      }
    }
  }
  
  // 5. Apply start-of-turn triggers
  applyStartOfTurnTriggers(gameState, playerKey)
  
  // 6. Remove summoning sickness from units
  const unitsKey = `${playerKey}_units`
  if (gameState.battlefield[unitsKey]) {
    gameState.battlefield[unitsKey].forEach((unit: any) => {
      unit.summoning_sickness = false
      unit.can_attack = true
      unit.has_attacked = false
    })
  }
  
  // 7. Set turn timer (60 seconds from now)
  const turnDuration = 60 // seconds
  const turnStartTime = new Date()
  const turnEndTime = new Date(turnStartTime.getTime() + turnDuration * 1000)
  
  gameState.turn_start_time = turnStartTime.toISOString()
  gameState.turn_end_time = turnEndTime.toISOString()
  gameState.turn_duration = turnDuration
  
  // 7. Transition to main phase
  if (gameState.game_status === 'active') {
    gameState.turn_phase = 'main'
    gameState.phase = 'main_phase'
    console.log('➡️ Entering main phase')
  }
  
  // Check victory conditions after turn start
  await checkVictoryConditions(gameState, battle, supabaseClient)
  
  return gameState
}

// Apply start-of-turn triggers
function applyStartOfTurnTriggers(gameState: any, playerKey: string) {
  console.log('🔥 Applying start-of-turn triggers')
  
  const unitsKey = `${playerKey}_units`
  const structuresKey = `${playerKey}_structures`
  
  // Process unit triggers
  if (gameState.battlefield[unitsKey]) {
    gameState.battlefield[unitsKey].forEach((unit: any) => {
      // Example: Healing units, damage dealers, etc.
      if (unit.abilities?.includes('heal_all_potatoes')) {
        console.log('💚 Healing all friendly potatoes +1')
        gameState.battlefield[unitsKey].forEach((friendlyUnit: any) => {
          friendlyUnit.current_health = Math.min(friendlyUnit.max_health, friendlyUnit.current_health + 1)
        })
      }
    })
  }
  
  // Process structure triggers
  if (gameState.battlefield[structuresKey]) {
    gameState.battlefield[structuresKey].forEach((structure: any) => {
      // Example: Mana generation, card draw, etc.
      if (structure.abilities?.includes('mana_generation')) {
        console.log('💎 Structure providing extra mana')
        const heroKey = `${playerKey}_hero`
        gameState[heroKey].mana = Math.min(10, gameState[heroKey].mana + 1)
      }
    })
  }
}

// Process end of turn phase
async function processEndTurn(gameState: any, playerId: string, supabaseClient: any, battle: any) {
  console.log('🌙 Processing end turn for player:', playerId)
  console.log('🔍 Game state check:', {
    currentTurnPlayerId: gameState.current_turn_player_id,
    requestingPlayerId: playerId,
    turnPhase: gameState.turn_phase,
    gamePhase: gameState.phase,
    gameStatus: gameState.game_status
  })
  
  if (!gameState.current_turn_player_id) {
    throw new Error('No current turn player set (game might be in mulligan phase)')
  }
  
  if (gameState.current_turn_player_id !== playerId) {
    throw new Error(`Not your turn. Current turn player: ${gameState.current_turn_player_id}, Requesting player: ${playerId}`)
  }

  if (gameState.phase === 'mulligan') {
    throw new Error('Cannot end turn during mulligan phase')
  }

  if (gameState.turn_phase !== 'main') {
    throw new Error(`Can only end turn during main phase. Current phase: ${gameState.turn_phase}, Game phase: ${gameState.phase}`)
  }
  
  // 1. Apply end-of-turn triggers
  const playerKey = gameState.current_turn_player_id === battle.player1_id ? 'player1' : 'player2'
  await processEndOfTurnTriggers(gameState, playerKey)
  
  // 2. Reset unit states for current player
  const unitsKey = `${playerKey}_units`
  if (gameState.battlefield[unitsKey]) {
    gameState.battlefield[unitsKey].forEach((unit: any) => {
      unit.has_attacked = false
      unit.attacks_this_turn = 0
    })
  }
  
  // 3. Switch to next player
  const nextPlayerId = gameState.current_turn_player_id === battle.player1_id 
    ? battle.player2_id 
    : battle.player1_id
    
  gameState.current_turn_player_id = nextPlayerId
  
  // 4. Increment turn count when it comes back to first player
  const isBackToFirstPlayer = nextPlayerId === battle.player1_id
  if (isBackToFirstPlayer) {
    gameState.turn_count += 1
  }
  
  // 5. Set next turn phase
  gameState.turn_phase = 'start'
  gameState.phase = 'start_turn'
  
  console.log(`🔄 Turn ended. Next player: ${nextPlayerId}, Turn: ${gameState.turn_count}`)
  
  // Check victory conditions after turn end
  await checkVictoryConditions(gameState, battle, supabaseClient)
  
  return gameState
}

// Apply end-of-turn triggers
function applyEndOfTurnTriggers(gameState: any, playerKey: string) {
  console.log('🌙 Applying end-of-turn triggers')
  
  const unitsKey = `${playerKey}_units`
  const structuresKey = `${playerKey}_structures`
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  
  // Process unit triggers
  if (gameState.battlefield[unitsKey]) {
    gameState.battlefield[unitsKey].forEach((unit: any) => {
      // Example: End-of-turn damage
      if (unit.abilities?.includes('deal_1_damage_end_turn')) {
        console.log('🔥 Dealing 1 damage to enemy hero at end of turn')
        gameState[enemyHeroKey].hp -= 1
        
        // Check for death
        if (gameState[enemyHeroKey].hp <= 0) {
          gameState.game_status = 'finished'
          gameState.winner_id = playerKey === 'player1' ? gameState.player1_id : gameState.player2_id
          gameState.phase = 'game_over'
        }
      }
    })
  }
  
  // Process structure triggers
  if (gameState.battlefield[structuresKey]) {
    gameState.battlefield[structuresKey].forEach((structure: any) => {
      // Example: End-of-turn effects
      if (structure.abilities?.includes('draw_card_end_turn')) {
        console.log('🎴 Drawing a card at end of turn')
        const deckKey = `${playerKey}_deck`
        const handKey = `${playerKey}_hand`
        
        if (gameState[deckKey].length > 0 && gameState[handKey].length < 10) {
          const drawnCard = gameState[deckKey].shift()
          gameState[handKey].push(drawnCard)
        }
      }
    })
  }
}

// Process drawing a card (for effects/spells)
async function processDrawCard(gameState: any, playerId: string, supabaseClient: any, battle: any) {
  console.log('🃏 Processing draw card for player:', playerId)
  
  const isPlayer1 = battle.player1_id === playerId
  const playerKey = isPlayer1 ? 'player1' : 'player2'
  const deckKey = `${playerKey}_deck`
  const handKey = `${playerKey}_hand`
  const heroKey = `${playerKey}_hero`
  
  if (gameState[deckKey].length > 0) {
    // Check hand size limit (10 cards max)
    if (gameState[handKey].length >= 10) {
      // Hand is full - burn the card (discard without effect)
      const burnedCard = gameState[deckKey].shift()
      console.log(`🔥 Card burned due to full hand (10/10): ${burnedCard?.name || 'Unknown Card'}`)
    } else {
      // Draw card normally
      const drawnCard = gameState[deckKey].shift()
      gameState[handKey].push(drawnCard)
      console.log('🃏 Card drawn via effect')
    }
  } else {
    // Fatigue damage for additional draws
    gameState[heroKey].fatigue_damage += 1
    gameState[heroKey].hp -= gameState[heroKey].fatigue_damage
    console.log(`💀 Fatigue damage from card draw: ${gameState[heroKey].fatigue_damage}`)
    
    if (gameState[heroKey].hp <= 0) {
      gameState.game_status = 'finished'
      gameState.winner_id = playerId === battle.player1_id ? battle.player2_id : battle.player1_id
      gameState.phase = 'game_over'
    }
  }
  
  return gameState
}

// Check victory conditions
async function checkVictoryConditions(gameState: any, battle: any, supabaseClient: any) {
  const player1Hero = gameState.player1_hero
  const player2Hero = gameState.player2_hero
  
  // 1. Check if either player's health <= 0
  if (player1Hero.hp <= 0) {
    gameState.game_status = 'finished'
    gameState.winner_id = battle.player2_id
    gameState.phase = 'game_over'
    console.log('🏆 Player 2 wins - Player 1 health reached 0')
    
    // Update hero battle stats
    await updateHeroBattleStats(supabaseClient, battle.player2_id, battle.player1_id, battle.player1_id, battle.player2_id, player1Hero, player2Hero)
    
    return true
  }
  
  if (player2Hero.hp <= 0) {
    gameState.game_status = 'finished'
    gameState.winner_id = battle.player1_id
    gameState.phase = 'game_over'
    console.log('🏆 Player 1 wins - Player 2 health reached 0')
    
    // Update hero battle stats
    await updateHeroBattleStats(supabaseClient, battle.player1_id, battle.player2_id, battle.player1_id, battle.player2_id, player1Hero, player2Hero)
    
    return true
  }
  
  // 2. Check if either player has no cards left and cannot draw
  const player1NoCards = gameState.player1_deck.length === 0 && 
                         gameState.player1_hand.length === 0 && 
                         gameState.battlefield.player1_units.length === 0
  
  const player2NoCards = gameState.player2_deck.length === 0 && 
                         gameState.player2_hand.length === 0 && 
                         gameState.battlefield.player2_units.length === 0
  
  if (player1NoCards) {
    gameState.game_status = 'finished'
    gameState.winner_id = battle.player2_id
    gameState.phase = 'game_over'
    console.log('🏆 Player 2 wins - Player 1 has no cards left')
    
    // Update hero battle stats
    await updateHeroBattleStats(supabaseClient, battle.player2_id, battle.player1_id, battle.player1_id, battle.player2_id, player1Hero, player2Hero)
    
    return true
  }
  
  if (player2NoCards) {
    gameState.game_status = 'finished'
    gameState.winner_id = battle.player1_id
    gameState.phase = 'game_over'
    console.log('🏆 Player 1 wins - Player 2 has no cards left')
    
    // Update hero battle stats
    await updateHeroBattleStats(supabaseClient, battle.player1_id, battle.player2_id, battle.player1_id, battle.player2_id, player1Hero, player2Hero)
    
    return true
  }
  
  return false
}

// ===== TURN-BASED AURA PROCESSING =====

async function processTurnBasedAuras(gameState: any, playerKey: string) {
  const heroKey = `${playerKey}_hero`
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  // Find structures and relics with turn-based effects
  const auraProviders = playerSlots.filter((card: any) => 
    card && (card.card_type === 'structure' || card.card_type === 'relic')
  )
  
  for (const provider of auraProviders) {
    if (!provider.keywords) continue
    
    const keywords = Array.isArray(provider.keywords) ? provider.keywords : 
      (typeof provider.keywords === 'string' ? JSON.parse(provider.keywords) : [])
    
    for (const keyword of keywords) {
      switch (keyword) {
        case 'Structure:HealHero2EachTurn':
          const hero = gameState[heroKey]
          const healAmount = 2
          hero.hp = Math.min(hero.max_hp, hero.hp + healAmount)
          console.log(`💚 ${provider.name} healed hero for ${healAmount} (${hero.hp}/${hero.max_hp})`)
          break
          
        case 'Structure:DamageEnemyHero1EachTurn':
          const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
          const enemyHeroKey = `${enemyPlayerKey}_hero`
          const enemyHero = gameState[enemyHeroKey]
          enemyHero.hp -= 1
          console.log(`💥 ${provider.name} damaged enemy hero for 1 (${enemyHero.hp}/${enemyHero.max_hp})`)
          break
          
        case 'Relic:BuffRandomAlly+1+1':
          await buffRandomAllyTurn(gameState, playerKey)
          console.log(`💪 ${provider.name} buffed random ally +1/+1`)
          break
          
        case 'Relic:PingEnemyHeroEachTurn1':
        case 'Structure:PingEnemyHeroEachTurn1':
          const enemyPlayerKey2 = playerKey === 'player1' ? 'player2' : 'player1'
          const enemyHeroKey2 = `${enemyPlayerKey2}_hero`
          const enemyHero2 = gameState[enemyHeroKey2]
          enemyHero2.hp -= 1
          console.log(`💥 ${provider.name} pinged enemy hero for 1 (${enemyHero2.hp}/${enemyHero2.max_hp})`)
          break
          
        case 'Structure:HeroRegen1':
          const hero2 = gameState[heroKey]
          hero2.hp = Math.min(hero2.max_hp, hero2.hp + 1)
          console.log(`💚 ${provider.name} healed hero for 1 (${hero2.hp}/${hero2.max_hp})`)
          break
          
        case 'Structure:FreezeWeakestEnemyEachTurn':
          await freezeWeakestEnemyTurn(gameState, playerKey === 'player1' ? 'player2' : 'player1')
          console.log(`🧊 ${provider.name} froze weakest enemy`)
          break
          
        case 'Structure:FreezeRandomEnemyEachTurn':
          await freezeRandomEnemyTurn(gameState, playerKey === 'player1' ? 'player2' : 'player1')
          console.log(`🧊 ${provider.name} froze random enemy`)
          break
          
        case 'Structure:DebuffEnemyAtk-1':
          await debuffAllEnemyAttackTurn(gameState, playerKey === 'player1' ? 'player2' : 'player1')
          console.log(`💀 ${provider.name} reduced all enemy ATK by 1`)
          break
          
        case 'EndOfTurn:SummonHusk1_1':
          await summonHuskTurn(gameState, playerKey, 1, 1)
          console.log(`👻 ${provider.name} summoned 1/1 Husk`)
          break
          
        case 'EndOfTurn:HeroDamage1':
          const enemyPlayerKey3 = playerKey === 'player1' ? 'player2' : 'player1'
          const enemyHeroKey3 = `${enemyPlayerKey3}_hero`
          const enemyHero3 = gameState[enemyHeroKey3]
          enemyHero3.hp -= 1
          console.log(`💥 ${provider.name} damaged enemy hero for 1 (${enemyHero3.hp}/${enemyHero3.max_hp})`)
          break
          
        case 'Structure:ManaRamp3Turns':
          // Check if it's the right turn for mana ramp (every 3 turns)
          if (gameState.turn_count % 3 === 0) {
            const hero3 = gameState[heroKey]
            if (hero3.max_mana < 10) {
              hero3.max_mana += 1
              hero3.mana = hero3.max_mana // Refresh to new max
              console.log(`💎 ${provider.name} granted +1 max mana (${hero3.mana}/${hero3.max_mana})`)
            }
          }
          break
          
        case 'Structure:ReviveLastAlly1HP':
          await reviveLastAllyTurn(gameState, playerKey)
          console.log(`⚰️ ${provider.name} attempted to revive last ally`)
          break
      }
    }
  }
}

async function buffRandomAllyTurn(gameState: any, playerKey: string) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter((unit: any) => unit && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * allyUnits.length)
  const unit = allyUnits[randomIndex]
  
  unit.attack += 1
  unit.current_hp += 1
  unit.max_hp += 1
  
  console.log(`💪 Buffed ${unit.name} +1/+1 (${unit.attack}/${unit.current_hp})`)
}

// ===== END-OF-TURN TRIGGER SYSTEM =====

async function processEndOfTurnTriggers(gameState: any, playerKey: string) {
  console.log('🌙 Processing end-of-turn triggers...')
  
  const heroKey = `${playerKey}_hero`
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  
  // Check all units on current player's battlefield for end-of-turn triggers
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const playerUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of playerUnits) {
    if (!unit.keywords) continue
    
    const keywords = Array.isArray(unit.keywords) ? unit.keywords : 
      (typeof unit.keywords === 'string' ? JSON.parse(unit.keywords) : [])
    
    for (const keyword of keywords) {
      switch (keyword) {
        case 'EndTurn:HealHero2':
          const hero = gameState[heroKey]
          hero.hp = Math.min(hero.max_hp, hero.hp + 2)
          console.log(`💚 ${unit.name} end-turn: Healed hero for 2 (${hero.hp}/${hero.max_hp})`)
          break
          
        case 'EndOfTurn:HealHero3':
          const hero2 = gameState[heroKey]
          hero2.hp = Math.min(hero2.max_hp, hero2.hp + 3)
          console.log(`💚 ${unit.name} end-turn: Healed hero for 3 (${hero2.hp}/${hero2.max_hp})`)
          break
          
        case 'EndOfTurn:FreezeRandom':
          await freezeRandomEnemyEndTurn(gameState, enemyPlayerKey)
          console.log(`🧊 ${unit.name} end-turn: Froze random enemy`)
          break
          
        case 'EndOfTurn:HealAllies2':
          await healAllAlliesEndTurn(gameState, playerKey, 2)
          console.log(`💚 ${unit.name} end-turn: Healed all allies for 2`)
          break
          
        case 'EndOfTurn:HealAllies3':
          await healAllAlliesEndTurn(gameState, playerKey, 3)
          console.log(`💚 ${unit.name} end-turn: Healed all allies for 3`)
          break
          
        case 'EndOfTurn:AOE2Enemies':
          await damageAllEnemiesEndTurn(gameState, enemyPlayerKey, 2)
          console.log(`💥 ${unit.name} end-turn: Damaged all enemies for 2`)
          break
          
        case 'Passive:HealHero2EachTurn':
          const hero3 = gameState[heroKey]
          hero3.hp = Math.min(hero3.max_hp, hero3.hp + 2)
          console.log(`💚 ${unit.name} passive: Healed hero for 2 (each turn)`)
          break
      }
    }
  }
}

async function freezeRandomEnemyEndTurn(gameState: any, enemyPlayerKey: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.is_frozen = true
  unit.frozen_turns_remaining = 1
  console.log(`🧊 End-turn froze ${unit.name}`)
}

async function healAllAlliesEndTurn(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of allyUnits) {
    const oldHp = unit.current_hp || unit.hp
    unit.current_hp = Math.min(unit.max_hp, unit.current_hp + amount)
    const actualHealing = unit.current_hp - oldHp
    
    if (actualHealing > 0) {
      console.log(`💚 End-turn healed ${unit.name} for ${actualHealing} (${unit.current_hp}/${unit.max_hp})`)
      
      // Trigger heal-based passives (chain reactions)
      await processHealTriggersEndTurn(gameState, playerKey, unit, actualHealing)
    }
  }
}

async function damageAllEnemiesEndTurn(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (let i = enemySlots.length - 1; i >= 0; i--) {
    const unit = enemySlots[i]
    if (unit && unit.card_type === 'unit') {
      unit.current_hp -= amount
      console.log(`💥 End-turn damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
      
      // Check if unit dies from end-turn damage
      if (unit.current_hp <= 0) {
        // Process deathrattle if unit dies
        await processDeathrattleEndTurn(gameState, unit, enemyPlayerKey)
        enemySlots[i] = null
        console.log(`💀 ${unit.name} destroyed by end-turn damage`)
      }
    }
  }
}

async function processHealTriggersEndTurn(gameState: any, playerKey: string, healedUnit: any, healAmount: number) {
  // Check all units on battlefield for heal-triggered passives (same as spell triggers)
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const playerUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of playerUnits) {
    if (!unit.keywords) continue
    
    const keywords = Array.isArray(unit.keywords) ? unit.keywords : 
      (typeof unit.keywords === 'string' ? JSON.parse(unit.keywords) : [])
    
    for (const keyword of keywords) {
      if (keyword === 'Passive:Damage1OnHeal') {
        await damageRandomEnemyHealTriggerEndTurn(gameState, playerKey === 'player1' ? 'player2' : 'player1', 1)
        console.log(`💥 ${unit.name} passive: Damaged random enemy for 1 (heal trigger from end-turn)`)
      }
    }
  }
}

async function damageRandomEnemyHealTriggerEndTurn(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) {
    const heroKey = `${enemyPlayerKey}_hero`
    gameState[heroKey].hp -= amount
    console.log(`💥 No enemy units, damaged enemy hero for ${amount}`)
    return
  }
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.current_hp -= amount
  console.log(`💥 Heal trigger damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
  
  if (unit.current_hp <= 0) {
    const slotIndex = enemySlots.indexOf(unit)
    enemySlots[slotIndex] = null
    console.log(`💀 ${unit.name} destroyed by heal trigger`)
  }
}

async function processDeathrattleEndTurn(gameState: any, dyingUnit: any, playerKey: string) {
  // Simple deathrattle processing for end-turn deaths
  if (!dyingUnit || !dyingUnit.keywords) return
  
  const keywords = Array.isArray(dyingUnit.keywords) ? dyingUnit.keywords : 
    (typeof dyingUnit.keywords === 'string' ? JSON.parse(dyingUnit.keywords) : [])
  
  for (const keyword of keywords) {
    if (keyword === 'Deathrattle:SummonHusk1') {
      await summonHuskEndTurn(gameState, playerKey, 1, 1)
      console.log(`👻 ${dyingUnit.name} deathrattle: Summoned 1/1 Husk (end-turn death)`)
    }
  }
}

async function summonHuskEndTurn(gameState: any, playerKey: string, attack: number, hp: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  let emptySlotIndex = -1
  for (let i = 0; i < playerSlots.length; i++) {
    if (!playerSlots[i]) {
      emptySlotIndex = i
      break
    }
  }
  
  if (emptySlotIndex === -1) {
    console.log('❌ No empty slots for end-turn Husk')
    return
  }
  
  const huskToken = {
    id: `husk_endturn_${Date.now()}_${Math.random()}`,
    name: 'Husk',
    card_type: 'unit',
    potato_type: 'void',
    attack: attack,
    hp: hp,
    current_hp: hp,
    max_hp: hp,
    deployed_turn: gameState.turn_number,
    summoning_sickness: true,
    has_attacked_this_turn: false,
    owner_id: playerKey,
    is_token: true
  }
  
  playerSlots[emptySlotIndex] = huskToken
  console.log(`👻 End-turn summoned ${attack}/${hp} Husk in slot ${emptySlotIndex}`)
}

// ===== TURN-START TRIGGER SYSTEM =====

async function processTurnStartTriggers(gameState: any, playerKey: string) {
  console.log('🌅 Processing turn-start triggers...')
  
  const heroKey = `${playerKey}_hero`
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  
  // Check all units on current player's battlefield for turn-start triggers
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const playerUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of playerUnits) {
    if (!unit.keywords) continue
    
    const keywords = Array.isArray(unit.keywords) ? unit.keywords : 
      (typeof unit.keywords === 'string' ? JSON.parse(unit.keywords) : [])
    
    for (const keyword of keywords) {
      switch (keyword) {
        case 'TurnStart:HealHero2':
          const hero = gameState[heroKey]
          hero.hp = Math.min(hero.max_hp, hero.hp + 2)
          console.log(`💚 ${unit.name} turn-start: Healed hero for 2 (${hero.hp}/${hero.max_hp})`)
          break
          
        case 'TurnStart:DrawCard':
          await drawCardTurnStart(gameState, playerKey)
          console.log(`🃏 ${unit.name} turn-start: Drew extra card`)
          break
          
        case 'TurnStart:BuffRandomAlly+1+1':
          await buffRandomAllyTurnStart(gameState, playerKey)
          console.log(`💪 ${unit.name} turn-start: Buffed random ally +1/+1`)
          break
          
        case 'TurnStart:DamageRandomEnemy2':
          await damageRandomEnemyTurnStart(gameState, enemyPlayerKey, 2)
          console.log(`💥 ${unit.name} turn-start: Damaged random enemy for 2`)
          break
          
        case 'TurnStart:FreezeRandomEnemy':
          await freezeRandomEnemyTurnStart(gameState, enemyPlayerKey)
          console.log(`🧊 ${unit.name} turn-start: Froze random enemy`)
          break
          
        case 'TurnStart:SummonHusk1':
          await summonHuskTurnStart(gameState, playerKey, 1, 1)
          console.log(`👻 ${unit.name} turn-start: Summoned 1/1 Husk`)
          break
          
        case 'TurnStart:GainMana1':
          const hero2 = gameState[heroKey]
          hero2.mana = Math.min(10, hero2.mana + 1)
          console.log(`💎 ${unit.name} turn-start: Gained +1 mana (${hero2.mana})`)
          break
          
        case 'TurnStart:HealAllAllies1':
          await healAllAlliesTurnStart(gameState, playerKey, 1)
          console.log(`💚 ${unit.name} turn-start: Healed all allies for 1`)
          break
      }
    }
  }
}

async function drawCardTurnStart(gameState: any, playerKey: string) {
  const deckKey = `${playerKey}_deck`
  const handKey = `${playerKey}_hand`
  
  if (gameState[deckKey].length > 0) {
    // Check hand size limit (10 cards max)
    if (gameState[handKey].length >= 10) {
      // Hand is full - burn the card
      const burnedCard = gameState[deckKey].shift()
      console.log(`🔥 Turn-start card burned due to full hand: ${burnedCard?.name || 'Unknown Card'}`)
    } else {
      // Draw from deck normally
      const drawnCard = gameState[deckKey].shift()
      gameState[handKey].push(drawnCard)
      console.log('🃏 Turn-start extra card drawn')
    }
  }
}

async function buffRandomAllyTurnStart(gameState: any, playerKey: string) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * allyUnits.length)
  const unit = allyUnits[randomIndex]
  
  unit.attack += 1
  unit.current_hp += 1
  unit.max_hp += 1
  
  console.log(`💪 Turn-start buffed ${unit.name} +1/+1 (${unit.attack}/${unit.current_hp})`)
}

async function damageRandomEnemyTurnStart(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) {
    const heroKey = `${enemyPlayerKey}_hero`
    gameState[heroKey].hp -= amount
    console.log(`💥 No enemy units, turn-start damaged enemy hero for ${amount}`)
    return
  }
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.current_hp -= amount
  console.log(`💥 Turn-start damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
  
  if (unit.current_hp <= 0) {
    const slotIndex = enemySlots.indexOf(unit)
    enemySlots[slotIndex] = null
    console.log(`💀 ${unit.name} destroyed by turn-start damage`)
  }
}

async function freezeRandomEnemyTurnStart(gameState: any, enemyPlayerKey: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.is_frozen = true
  unit.frozen_turns_remaining = 1
  console.log(`🧊 Turn-start froze ${unit.name}`)
}

async function summonHuskTurnStart(gameState: any, playerKey: string, attack: number, hp: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  let emptySlotIndex = -1
  for (let i = 0; i < playerSlots.length; i++) {
    if (!playerSlots[i]) {
      emptySlotIndex = i
      break
    }
  }
  
  if (emptySlotIndex === -1) {
    console.log('❌ No empty slots for turn-start Husk')
    return
  }
  
  const huskToken = {
    id: `husk_turnstart_${Date.now()}_${Math.random()}`,
    name: 'Husk',
    card_type: 'unit',
    potato_type: 'void',
    attack: attack,
    hp: hp,
    current_hp: hp,
    max_hp: hp,
    deployed_turn: gameState.turn_number,
    summoning_sickness: true,
    has_attacked_this_turn: false,
    owner_id: playerKey,
    is_token: true
  }
  
  playerSlots[emptySlotIndex] = huskToken
  console.log(`👻 Turn-start summoned ${attack}/${hp} Husk in slot ${emptySlotIndex}`)
}

async function healAllAlliesTurnStart(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of allyUnits) {
    const oldHp = unit.current_hp || unit.hp
    unit.current_hp = Math.min(unit.max_hp, unit.current_hp + amount)
    const actualHealing = unit.current_hp - oldHp
    
    if (actualHealing > 0) {
      console.log(`💚 Turn-start healed ${unit.name} for ${actualHealing} (${unit.current_hp}/${unit.max_hp})`)
      
      // Trigger heal-based passives
      await processHealTriggersTurnStart(gameState, playerKey, unit, actualHealing)
    }
  }
}

async function processHealTriggersTurnStart(gameState: any, playerKey: string, healedUnit: any, healAmount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const playerUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of playerUnits) {
    if (!unit.keywords) continue
    
    const keywords = Array.isArray(unit.keywords) ? unit.keywords : 
      (typeof unit.keywords === 'string' ? JSON.parse(unit.keywords) : [])
    
    for (const keyword of keywords) {
      if (keyword === 'Passive:Damage1OnHeal') {
        await damageRandomEnemyHealTriggerTurnStart(gameState, playerKey === 'player1' ? 'player2' : 'player1', 1)
        console.log(`💥 ${unit.name} passive: Damaged random enemy for 1 (heal trigger from turn-start)`)
      }
    }
  }
}

async function damageRandomEnemyHealTriggerTurnStart(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) {
    const heroKey = `${enemyPlayerKey}_hero`
    gameState[heroKey].hp -= amount
    console.log(`💥 No enemy units, damaged enemy hero for ${amount}`)
    return
  }
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.current_hp -= amount
  console.log(`💥 Turn-start heal trigger damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
  
  if (unit.current_hp <= 0) {
    const slotIndex = enemySlots.indexOf(unit)
    enemySlots[slotIndex] = null
    console.log(`💀 ${unit.name} destroyed by turn-start heal trigger`)
  }
}

async function freezeWeakestEnemyTurn(gameState: any, enemyPlayerKey: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) return
  
  // Find weakest enemy (lowest current HP)
  let weakestUnit = enemyUnits[0]
  enemyUnits.forEach(unit => {
    if ((unit.current_hp || unit.hp) < (weakestUnit.current_hp || weakestUnit.hp)) {
      weakestUnit = unit
    }
  })
  
  weakestUnit.is_frozen = true
  weakestUnit.frozen_turns_remaining = 1
  console.log(`🧊 Froze weakest enemy ${weakestUnit.name}`)
}

async function freezeRandomEnemyTurn(gameState: any, enemyPlayerKey: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.is_frozen = true
  unit.frozen_turns_remaining = 1
  console.log(`🧊 Froze random enemy ${unit.name}`)
}

async function debuffAllEnemyAttackTurn(gameState: any, enemyPlayerKey: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  enemySlots.forEach(unit => {
    if (unit && unit.card_type === 'unit') {
      // Apply temporary ATK debuff (can be permanent or temporary)
      unit.attack = Math.max(0, (unit.attack || 0) - 1)
      console.log(`💀 Reduced ${unit.name} ATK by 1 (now ${unit.attack})`)
    }
  })
}

async function summonHuskTurn(gameState: any, playerKey: string, attack: number, hp: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  // Find empty slot for summoned unit
  let emptySlotIndex = -1
  for (let i = 0; i < playerSlots.length; i++) {
    if (!playerSlots[i]) {
      emptySlotIndex = i
      break
    }
  }
  
  if (emptySlotIndex === -1) {
    console.log('❌ No empty slots for summoned Husk')
    return
  }
  
  // Create Husk token
  const huskToken = {
    id: `husk_${Date.now()}_${Math.random()}`,
    name: 'Husk',
    card_type: 'unit',
    potato_type: 'void',
    attack: attack,
    hp: hp,
    current_hp: hp,
    max_hp: hp,
    deployed_turn: gameState.turn_number,
    summoning_sickness: true,
    has_attacked_this_turn: false,
    owner_id: playerKey,
    is_token: true
  }
  
  playerSlots[emptySlotIndex] = huskToken
  console.log(`👻 Summoned ${attack}/${hp} Husk in slot ${emptySlotIndex}`)
}

async function reviveLastAllyTurn(gameState: any, playerKey: string) {
  const graveyardKey = `${playerKey}_graveyard`
  const graveyard = gameState[graveyardKey] || []
  
  if (graveyard.length === 0) {
    console.log('❌ No units in graveyard to revive')
    return
  }
  
  // Find last ally that died (most recent in graveyard)
  const lastAlly = graveyard[graveyard.length - 1]
  
  // Find empty slot
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  let emptySlotIndex = -1
  for (let i = 0; i < playerSlots.length; i++) {
    if (!playerSlots[i]) {
      emptySlotIndex = i
      break
    }
  }
  
  if (emptySlotIndex === -1) {
    console.log('❌ No empty slots to revive unit')
    return
  }
  
  // Revive with 1 HP
  const revivedUnit = {
    ...lastAlly,
    current_hp: 1,
    max_hp: lastAlly.hp || lastAlly.max_hp,
    deployed_turn: gameState.turn_number,
    summoning_sickness: true,
    has_attacked_this_turn: false,
    id: `${lastAlly.id}_revived_${Date.now()}`
  }
  
  playerSlots[emptySlotIndex] = revivedUnit
  
  // Remove from graveyard (one-time use)
  graveyard.pop()
  
  console.log(`⚰️ Revived ${lastAlly.name} with 1 HP in slot ${emptySlotIndex}`)
}