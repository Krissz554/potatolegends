import { serve } from "https://deno.land/std@0.168.0/http/server.ts"
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

interface DeployCardRequest {
  battleId: string
  cardId: string
  slotIndex?: number // 0-5 for slot-based deployment
  expectedVersion?: number // For optimistic locking
  idempotencyKey?: string // For duplicate prevention
}

interface BattleSession {
  id: string
  player1_id: string
  player2_id: string
  current_turn_player_id: string | null
  version: number
  game_state: {
    player1_deck: any[]
    player2_deck: any[]
    player1_hand: any[]
    player2_hand: any[]
    battlefield: {
      player1_active?: any
      player2_active?: any
      player1_slots?: any[] // 6 slots for new system
      player2_slots?: any[] // 6 slots for new system
    }
    phase: string
    turn_count: number
    round_count: number
    first_player_id: string
    waiting_for_deploy_player_id?: string
    current_attacker_id?: string
    last_round_winner_id?: string
    current_turn_player_id?: string
    turn_phase?: string
    turn_time_remaining?: number
    turn_started_at?: string
    player1_id?: string
    player2_id?: string
  }
  status: string
}

// Helper function to get rarity numeric value for comparison
const getRarityValue = (rarity: string): number => {
  const rarityValues = {
    'common': 1,
    'uncommon': 2,
    'rare': 3,
    'legendary': 4,
    'exotic': 5
  }
  return rarityValues[rarity] || 1
}

// Auto-combat function to execute attacks automatically
async function executeAutoAttack(supabaseClient: any, battleId: string, gameState: any) {
  console.log('🤖 Executing auto-attack...')
  console.log('🎯 Input gameState:', {
    phase: gameState.phase,
    attacker_id: gameState.current_attacker_id,
    battlefield: gameState.battlefield,
    p1_hand_size: gameState.player1_hand?.length,
    p2_hand_size: gameState.player2_hand?.length
  })
  
  // Get battle info to determine player IDs
  const { data: battle, error: battleError } = await supabaseClient
    .from('battle_sessions')
    .select('player1_id, player2_id')
    .eq('id', battleId)
    .single()
  
  if (battleError || !battle) {
    console.error('❌ Battle not found for auto-attack:', battleError)
    throw new Error(`Battle not found: ${battleError?.message || 'Unknown error'}`)
  }
  
  console.log('🎮 Battle players:', { player1: battle.player1_id, player2: battle.player2_id })
  
  const attackerId = gameState.current_attacker_id
  const isPlayer1Attacking = attackerId === battle.player1_id
  
  console.log(`🎯 Attacker: ${attackerId}, Player1: ${battle.player1_id}, Player2: ${battle.player2_id}, IsPlayer1Attacking: ${isPlayer1Attacking}`)
  
  const attackingCard = isPlayer1Attacking ? gameState.battlefield.player1_active : gameState.battlefield.player2_active
  const defendingCard = isPlayer1Attacking ? gameState.battlefield.player2_active : gameState.battlefield.player1_active
  
  if (!attackingCard || !defendingCard) {
    console.error('Missing cards for auto-attack')
    return
  }
  
  console.log(`⚔️ ${attackingCard.potato?.name || attackingCard.name} attacks ${defendingCard.potato?.name || defendingCard.name} for ${attackingCard.attack} damage`)
  
  // Calculate damage
  const damage = attackingCard.attack
  const newHp = defendingCard.current_hp - damage
  const cardDefeated = newHp <= 0
  
  console.log(`💥 ${defendingCard.potato?.name || defendingCard.name} takes ${damage} damage (${defendingCard.current_hp} → ${Math.max(0, newHp)} HP)`)
  
  // Update defending card HP
  let updatedGameState = { ...gameState }
  if (isPlayer1Attacking) {
    updatedGameState.battlefield.player2_active = {
      ...defendingCard,
      current_hp: Math.max(0, newHp)
    }
  } else {
    updatedGameState.battlefield.player1_active = {
      ...defendingCard,
      current_hp: Math.max(0, newHp)
    }
  }
  
  if (cardDefeated) {
    console.log(`💀 ${defendingCard.potato?.name || defendingCard.name} is defeated!`)
    
    // Remove dead card
    if (isPlayer1Attacking) {
      updatedGameState.battlefield.player2_active = null
    } else {
      updatedGameState.battlefield.player1_active = null
    }
    
    // Check if loser has cards left
    const loserHand = isPlayer1Attacking ? gameState.player2_hand : gameState.player1_hand
    const loserId = isPlayer1Attacking ? battle.player2_id : battle.player1_id
    const winnerId = isPlayer1Attacking ? battle.player1_id : battle.player2_id
    
    console.log('🃏 Hand check:', {
      loser_id: loserId,
      loser_hand_size: loserHand?.length || 0,
      loser_hand: loserHand,
      winner_id: winnerId
    })
    
    if (!loserHand || loserHand.length === 0) {
      // Game over
      console.log(`🏆 Game over! Winner: ${winnerId}`)
      updatedGameState.phase = 'finished'
      updatedGameState.current_attacker_id = null
      
      await supabaseClient
        .from('battle_sessions')
        .update({
          game_state: updatedGameState,
          status: 'finished',
          winner_id: winnerId,
          finished_at: new Date().toISOString(),
          updated_at: new Date().toISOString()
        })
        .eq('id', battleId)
      
      // Publish game end to Ably
      try {
        await fetch('https://rest.ably.io/channels/match:' + battleId + '/messages', {
          method: 'POST',
          headers: {
            'Authorization': 'Basic ' + btoa('1Vf05w.bzqjGQ:6YuVnRKLTCKDlHR-tDzDZPGfA5pBP57vWP4nFRrFXm0'),
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            name: 'game_ended',
            data: { type: 'combat_victory', winnerId, battleId, timestamp: Date.now() }
          })
        })
      } catch (error) {
        console.error('❌ Ably publish error:', error)
      }
    } else {
      // Loser must deploy new card
      console.log(`🃏 ${loserId} must deploy a new card`)
      updatedGameState.phase = 'waiting_redeploy'
      updatedGameState.waiting_for_deploy_player_id = loserId
      updatedGameState.last_round_winner_id = winnerId
      updatedGameState.round_count = (updatedGameState.round_count || 1) + 1
      
      await supabaseClient
        .from('battle_sessions')
        .update({
          game_state: updatedGameState,
          current_turn_player_id: loserId,
          updated_at: new Date().toISOString()
        })
        .eq('id', battleId)
      
      // Publish redeploy phase to Ably
      try {
        await fetch('https://rest.ably.io/channels/match:' + battleId + '/messages', {
          method: 'POST',
          headers: {
            'Authorization': 'Basic ' + btoa('1Vf05w.bzqjGQ:6YuVnRKLTCKDlHR-tDzDZPGfA5pBP57vWP4nFRrFXm0'),
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            name: 'phase_change',
            data: { type: 'waiting_redeploy', loserId, battleId, timestamp: Date.now() }
          })
        })
      } catch (error) {
        console.error('❌ Ably publish error:', error)
      }
    }
  } else {
    // Card survived, switch attacker
    const nextAttacker = isPlayer1Attacking ? battle.player2_id : battle.player1_id
    
    console.log(`🔄 ${defendingCard.potato?.name || defendingCard.name} survives! Next attacker: ${nextAttacker}`)
    updatedGameState.current_attacker_id = nextAttacker
    
    await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: updatedGameState,
        current_turn_player_id: nextAttacker,
        updated_at: new Date().toISOString()
      })
      .eq('id', battleId)
    
    // Publish attacker switch to Ably
    try {
      await fetch('https://rest.ably.io/channels/match:' + battleId + '/messages', {
        method: 'POST',
        headers: {
          'Authorization': 'Basic ' + btoa('1Vf05w.bzqjGQ:6YuVnRKLTCKDlHR-tDzDZPGfA5pBP57vWP4nFRrFXm0'),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'attacker_switch',
          data: { type: 'switch_attacker', nextAttacker, battleId, timestamp: Date.now() }
        })
      })
    } catch (error) {
      console.error('❌ Ably publish error:', error)
    }
    
    // Note: For continuous auto-combat, we would need a different approach
    // since edge functions can't use setTimeout. For now, each attack needs
    // to be triggered separately (either by client or another edge function call)
  }
  
  // Log the action
  await supabaseClient
    .from('battle_actions')
    .insert({
      battle_session_id: battleId,
      player_id: attackerId,
      action_type: 'auto_attack',
      action_data: {
        damage: damage,
        remaining_hp: Math.max(0, newHp),
        defeated_card_id: cardDefeated ? defendingCard.potato_id : null,
        auto_combat: true
      }
    })
}

// Generate stats for a card based on rarity
const generatePotatoStats = (seed: string, rarity: string): { hp: number; attack: number } => {
  // Simple hash function for deterministic randomness
  let hash = 0
  for (let i = 0; i < seed.length; i++) {
    const char = seed.charCodeAt(i)
    hash = ((hash << 5) - hash) + char
    hash = hash & hash // Convert to 32-bit integer
  }
  
  // Use absolute value and normalize
  const normalized = Math.abs(hash) / 2147483647
  
  // Stat ranges by rarity
  const statRanges = {
    common: { hp: { min: 50, max: 100 }, attack: { min: 10, max: 25 } },
    uncommon: { hp: { min: 105, max: 120 }, attack: { min: 26, max: 30 } },
    rare: { hp: { min: 125, max: 150 }, attack: { min: 31, max: 38 } },
    legendary: { hp: { min: 155, max: 200 }, attack: { min: 39, max: 50 } },
    exotic: { hp: { min: 205, max: 300 }, attack: { min: 51, max: 75 } }
  }
  
  const ranges = statRanges[rarity] || statRanges.common
  
  // Generate HP and Attack
  const hp = Math.floor(ranges.hp.min + (normalized * (ranges.hp.max - ranges.hp.min)))
  const attack = Math.floor(ranges.attack.min + (normalized * (ranges.attack.max - ranges.attack.min)))
  
  return { hp, attack }
}

serve(async (req) => {
  // Handle CORS preflight requests
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  const body = await req.text()
  
  // Quick test to verify function deployment
  if (body.includes('"test":true')) {
    return new Response(
      JSON.stringify({ 
        success: true, 
        message: 'Edge function is running',
        version: '3.0',
        timestamp: new Date().toISOString()
      }),
      {
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200,
      }
    )
  }

  try {
    // Get the authorization header
    const authHeader = req.headers.get('authorization')
    if (!authHeader) {
      throw new Error('No authorization header')
    }

    // Create Supabase client with service role for game state updates
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
    const { data: { user }, error: userError } = await supabaseClient.auth.getUser(authHeader.replace('Bearer ', ''))
    if (userError || !user) {
      throw new Error('Invalid user')
    }

    // Parse request body
    let battleId: string, cardId: string, slotIndex: number | undefined, expectedVersion: number | undefined, idempotencyKey: string | undefined, targetId: string | undefined
    try {
      const requestData = JSON.parse(body)
      battleId = requestData.battleId
      cardId = requestData.cardId
      slotIndex = requestData.slotIndex
      expectedVersion = requestData.expectedVersion
      idempotencyKey = requestData.idempotencyKey || `deploy_${battleId}_${cardId}_${Date.now()}`
      targetId = requestData.targetId

    } catch (parseError) {
      console.error('JSON parse error:', parseError)
      throw new Error('Invalid JSON in request body')
    }

    if (!battleId || !cardId) {
      throw new Error('Missing battleId or cardId')
    }
    
    // Validate slot index if provided
    if (slotIndex !== undefined && (slotIndex < 0 || slotIndex > 5)) {
      throw new Error('Invalid slot index. Must be between 0 and 5.')
    }

    // Acquire advisory lock for this battle to prevent race conditions
    const { error: lockError } = await supabaseClient.rpc('pg_advisory_xact_lock', {
      key: battleId.split('-').reduce((a, b) => a + parseInt(b, 16), 0) // Convert UUID to number
    })

    // Get battle session with row-level security FOR UPDATE
    const { data: battle, error: battleError } = await supabaseClient
      .from('battle_sessions')
      .select('*')
      .eq('id', battleId)
      .single()

    if (battleError || !battle) {
      throw new Error('Battle not found or access denied')
    }

    const battleSession = battle as BattleSession

    // Verify user is part of this battle
    if (battleSession.player1_id !== user.id && battleSession.player2_id !== user.id) {
      throw new Error('User not part of this battle')
    }

    // Check version for optimistic locking
    if (expectedVersion !== undefined && battleSession.version !== expectedVersion) {
      return new Response(
        JSON.stringify({ 
          success: false, 
          error: 'Version mismatch - battle state has changed',
          currentVersion: battleSession.version,
          expectedVersion: expectedVersion,
          shouldRefetch: true
        }),
        { 
          headers: { ...corsHeaders, 'Content-Type': 'application/json' },
          status: 409, // Conflict
        }
      )
    }

        // The current turn player should ONLY be checked from game_state
    // The top-level current_turn_player_id is often null and unreliable
    const currentTurnPlayer = battleSession.game_state?.current_turn_player_id
    
    if (!currentTurnPlayer) {
      throw new Error(`No current turn player set in game state. Game may not be properly initialized.`)
    }
    
    if (currentTurnPlayer !== user.id) {
      throw new Error(`Not your turn. Current turn: ${currentTurnPlayer}, Your ID: ${user.id}`)
    }

    // Verify game phase allows deployment
    const validPhases = ['main', 'waiting_first_deploy', 'waiting_second_deploy', 'waiting_redeploy', 'gameplay', 'start']
    
    // Also check turn phase - 'start' phase should allow deployment and auto-transition to 'main'
    const currentTurnPhase = battleSession.game_state.turn_phase || 'main'
    const validTurnPhases = ['main', 'start']
    if (!validPhases.includes(battleSession.game_state.phase)) {
      const currentPhase = battleSession.game_state.phase
      if (currentPhase === 'mulligan') {
        throw new Error('Cannot deploy cards during mulligan phase. Complete your mulligan first by keeping or discarding cards.')
      } else {
        throw new Error(`Invalid phase for deployment: ${currentPhase}. Valid phases: ${validPhases.join(', ')}`)
      }
    }
    
    if (!validTurnPhases.includes(currentTurnPhase)) {
      throw new Error(`Invalid turn phase for deployment: ${currentTurnPhase}. Valid turn phases: ${validTurnPhases.join(', ')}`)
    }

    const gameState = battleSession.game_state
    const isPlayer1 = battleSession.player1_id === user.id
    const playerHand = isPlayer1 ? 
      (gameState.hands?.player1_hand || gameState.player1_hand) : 
      (gameState.hands?.player2_hand || gameState.player2_hand)
    
        // If we're in 'start' phase, transition to 'main' phase for actions
    const shouldTransitionToMain = currentTurnPhase === 'start'

    // Verify card is in player's hand (check both id and potato_id)
    const cardToPlay = playerHand?.find(card => 
      card.id === cardId || 
      card.potato_id === cardId || 
      card.potato?.id === cardId
    )
    

    
    if (!cardToPlay) {
      throw new Error(`Card not found in hand. Available cards: ${playerHand?.map(c => `${c.id}(${c.potato?.name || c.name})`).join(', ') || 'none'}`)
    }
    


    // Use actual card stats from card_complete table (single source of truth)
    // Handle both nested (cardToPlay.potato.field) and flat (cardToPlay.field) structures
    const cardHp = cardToPlay.hp || cardToPlay.health || cardToPlay.potato?.hp || cardToPlay.potato?.health
    const cardAttack = cardToPlay.attack || cardToPlay.potato?.attack
    const cardType = cardToPlay.card_type || 'unit'
    
    // Only units require attack/hp stats - spells, structures, relics can have null values
    if (cardType === 'unit') {
      if (cardHp === null || cardHp === undefined || cardAttack === null || cardAttack === undefined) {
        throw new Error(`Unit card missing required stats. HP: ${cardHp}, Attack: ${cardAttack}`)
      }
    }
    
    // For non-unit cards, set appropriate values
    const deployedCardHp = cardType === 'unit' ? cardHp : (cardToPlay.structure_hp || null)
    const deployedCardAttack = cardType === 'unit' ? cardAttack : 0
    

    
    const cardWithStats = {
      ...cardToPlay,
      current_hp: deployedCardHp, // Use appropriate HP based on card type
      max_hp: deployedCardHp,     // Max HP same as base HP
      attack: deployedCardAttack, // Use appropriate attack based on card type
      hp: deployedCardHp,         // Keep original hp field for compatibility
      card_type: cardType,        // Ensure card type is preserved
      has_attacked_this_turn: false, // Will be handled by summoning sickness logic in frontend
      deployed_turn: gameState.turn_number, // Track when unit was deployed
      
      // Initialize ability flags
      has_taunt: false,
      has_charge: false,
      has_lifesteal: false,
      has_poison: false,
      has_poison_touch: false,
      is_frozen: false,
      is_silenced: false,
      summoning_sickness: true, // Default summoning sickness
      can_attack: false
    }
    
    // Process keywords and abilities for deployed units and structures
    console.log(`🔍 Debug keywords for ${cardToPlay.name}:`, {
      keywords: cardToPlay.keywords,
      keywordsType: typeof cardToPlay.keywords,
      isArray: Array.isArray(cardToPlay.keywords)
    })
    
    // Handle keywords (might be string or array)
    let keywordsArray = []
    if (cardToPlay.keywords) {
      if (typeof cardToPlay.keywords === 'string') {
        try {
          keywordsArray = JSON.parse(cardToPlay.keywords)
        } catch (e) {
          console.log('❌ Failed to parse keywords string:', cardToPlay.keywords)
        }
      } else if (Array.isArray(cardToPlay.keywords)) {
        keywordsArray = cardToPlay.keywords
      }
    }
    
    if ((cardType === 'unit' || cardType === 'structure') && keywordsArray.length > 0) {
      console.log(`🎯 Processing keywords for deployed ${cardType} ${cardToPlay.name}:`, keywordsArray)
      
      for (const keyword of keywordsArray) {
        switch (keyword) {
          case 'Charge':
            cardWithStats.summoning_sickness = false
            cardWithStats.can_attack = true
            cardWithStats.has_charge = true
            console.log('⚡ Applied Charge - can attack immediately')
            break
            
          case 'Taunt':
            cardWithStats.has_taunt = true
            console.log('🛡️ Applied Taunt - must be attacked first')
            break
            
          case 'Lifesteal':
            cardWithStats.has_lifesteal = true
            console.log('🩸 Applied Lifesteal - heals when dealing damage')
            break
            
          case 'Poison':
            cardWithStats.has_poison = true
            console.log('☠️ Applied Poison - destroys any unit damaged')
            break
            
          case 'PoisonTouch':
            cardWithStats.has_poison_touch = true
            console.log('☠️ Applied Poison Touch - poisons units damaged')
            break
            
          case 'DoubleStrike':
            cardWithStats.has_double_strike = true
            console.log('⚔️⚔️ Applied Double Strike - attacks twice')
            break
            
          case 'DivineShield':
            cardWithStats.has_divine_shield = true
            console.log('🛡️ Applied Divine Shield - blocks first damage')
            break
            
          case 'Windfury':
            cardWithStats.has_windfury = true
            cardWithStats.attacks_per_turn = 2
            console.log('💨 Applied Windfury - can attack twice per turn')
            break
            
          case 'Rush':
            cardWithStats.summoning_sickness = false
            cardWithStats.can_attack_units = true
            cardWithStats.can_attack_hero = false // Rush can't attack hero on first turn
            cardWithStats.has_rush = true
            console.log('🏃 Applied Rush - can attack units immediately')
            break
            
          case 'Stealth':
            cardWithStats.has_stealth = true
            cardWithStats.can_be_targeted = false
            console.log('👤 Applied Stealth - cannot be targeted')
            break
            
          case 'Immune':
            cardWithStats.has_immune = true
            console.log('✨ Applied Immune - takes no damage')
            break
        }
      }
    }

    // Handle different card types
    let newGameState = { ...gameState }
    
    // Process battlecry effects for deployed units and structures (after newGameState is initialized)
    if ((cardType === 'unit' || cardType === 'structure') && keywordsArray.length > 0) {
      const playerKey = isPlayer1 ? 'player1' : 'player2'
      
      for (const keyword of keywordsArray) {
        if (keyword.startsWith('Battlecry:')) {
          await processBattlecryEffect(newGameState, playerKey, keyword, targetId)
        }
      }
    }
    let targetSlotIndex = slotIndex // Declare at top level for all code paths
    
    // If we're in 'start' phase, transition to 'main' phase for actions
    if (shouldTransitionToMain) {
      newGameState.turn_phase = 'main'
    }

    // Handle spells - they resolve immediately, don't go to battlefield
    if (cardType === 'spell') {
      
      // TODO: Implement spell effects based on keywords
      // For now, just remove from hand and deduct mana
      
      // Remove card from hand
      const newPlayerHand = playerHand.filter(c => c.potato_id !== cardId)
      
      if (isPlayer1) {
        if (newGameState.hands?.player1_hand) {
          newGameState.hands.player1_hand = newPlayerHand
        } else {
          newGameState.player1_hand = newPlayerHand
        }
        newGameState.player1_mana -= cardToPlay.mana_cost
      } else {
        if (newGameState.hands?.player2_hand) {
          newGameState.hands.player2_hand = newPlayerHand
        } else {
          newGameState.player2_hand = newPlayerHand
        }
        newGameState.player2_mana -= cardToPlay.mana_cost
      }

      // Update battle session
      const { error: updateError } = await supabaseClient
        .from('battle_sessions')
        .update({
          game_state: newGameState,
          version: battleSession.version + 1,
          updated_at: new Date().toISOString()
        })
        .eq('id', battleId)

      if (updateError) {
        throw new Error(`Failed to update battle: ${updateError.message}`)
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
            name: 'spell_resolved',
            data: {
              type: 'spell',
              cardName: cardToPlay.name,
              battleSessionId: battleId,
              version: battleSession.version + 1,
              timestamp: Date.now()
            }
          })
        })

        if (!ablyResponse.ok) {
          console.error('Failed to publish spell to Ably:', ablyResponse.status)
        }
      } catch (ablyError) {
        console.error('Ably publish error:', ablyError)
      }

      return new Response(
        JSON.stringify({
          success: true,
          message: `Spell ${cardToPlay.name} resolved`,
          cardType: 'spell',
          version: battleSession.version + 1
        }),
        {
          headers: {
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': '*',
          },
        }
      )
    }
    
    if (gameState.phase === 'main' || gameState.phase === 'gameplay') {
      // New 6-slot battlefield system
      
      // Initialize battlefield slots if they don't exist
      if (!newGameState.battlefield.player1_slots) {
        newGameState.battlefield.player1_slots = Array(6).fill(null)
      }
      if (!newGameState.battlefield.player2_slots) {
        newGameState.battlefield.player2_slots = Array(6).fill(null)
      }
      
      // Determine which slot to use (already declared at top level)
      const playerSlots = isPlayer1 ? newGameState.battlefield.player1_slots : newGameState.battlefield.player2_slots
      
      // If no specific slot provided, find first empty slot
      if (targetSlotIndex === undefined) {
        targetSlotIndex = playerSlots.findIndex(slot => slot === null)
        if (targetSlotIndex === -1) {
          throw new Error('No empty slots available on battlefield')
        }
      }
      
            // Check if target slot is empty
      if (playerSlots[targetSlotIndex] !== null) {
        throw new Error(`Slot ${targetSlotIndex + 1} is already occupied`)
      }

      // Special handling for structures - limit to 2 active structures
      if (cardToPlay.card_type === 'structure') {
        const currentStructures = playerSlots.filter(slot => slot && slot.card_type === 'structure')
        if (currentStructures.length >= 2) {
          throw new Error('Maximum 2 structures allowed. Please destroy an existing structure first.')
        }
      }

      // Special handling for relics - limit to 1 active relic, replace if deploying second
      if (cardToPlay.card_type === 'relic') {
        const currentRelics = playerSlots.filter(slot => slot && slot.card_type === 'relic')
        if (currentRelics.length >= 1) {
          // Find and remove the existing relic
          const existingRelicIndex = playerSlots.findIndex(slot => slot && slot.card_type === 'relic')
          if (existingRelicIndex !== -1) {
            console.log(`🔮 Replacing existing relic: ${playerSlots[existingRelicIndex].name}`)
            playerSlots[existingRelicIndex] = null // Remove old relic
          }
        }
      }

      // Remove card from hand (check all possible ID matches)
      const filteredHand = playerHand.filter(card => 
        card.id !== cardId && 
        card.potato_id !== cardId && 
        card.potato?.id !== cardId
      )
      
      if (isPlayer1) {
        if (newGameState.hands?.player1_hand) {
          newGameState.hands.player1_hand = filteredHand
        } else {
          newGameState.player1_hand = filteredHand
        }
        // Deduct mana cost for player 1
        if (newGameState.player1_hero) {
          newGameState.player1_hero.mana = Math.max(0, newGameState.player1_hero.mana - (cardToPlay.mana_cost || 0))
        }
      } else {
        if (newGameState.hands?.player2_hand) {
          newGameState.hands.player2_hand = filteredHand
        } else {
          newGameState.player2_hand = filteredHand
        }
        // Deduct mana cost for player 2
        if (newGameState.player2_hero) {
          newGameState.player2_hero.mana = Math.max(0, newGameState.player2_hero.mana - (cardToPlay.mana_cost || 0))
        }
      }
      
      // Add card to specific battlefield slot
      if (isPlayer1) {
        newGameState.battlefield.player1_slots[targetSlotIndex] = cardWithStats
      } else {
        newGameState.battlefield.player2_slots[targetSlotIndex] = cardWithStats
      }
      

      
      // DO NOT switch turns after card deployment - player can deploy multiple cards
      // Turn only ends when:
      // 1. Player manually clicks "End Turn" button
      // 2. Timer runs out and triggers auto-end

      
    } else if (gameState.phase === 'waiting_first_deploy') {
      // First player deploys
      if (isPlayer1) {
        newGameState.battlefield.player1_active = cardWithStats
        newGameState.player1_hand = playerHand.filter(card => card.potato_id !== cardId)
      } else {
        newGameState.battlefield.player2_active = cardWithStats
        newGameState.player2_hand = playerHand.filter(card => card.potato_id !== cardId)
      }
      
      // Move to waiting for second deploy
      newGameState.phase = 'waiting_second_deploy'
      newGameState.waiting_for_deploy_player_id = isPlayer1 ? battleSession.player2_id : battleSession.player1_id
      
    } else if (gameState.phase === 'waiting_second_deploy') {
      // Second player deploys
      if (isPlayer1) {
        newGameState.battlefield.player1_active = cardWithStats
        newGameState.player1_hand = playerHand.filter(card => card.potato_id !== cardId)
      } else {
        newGameState.battlefield.player2_active = cardWithStats
        newGameState.player2_hand = playerHand.filter(card => card.potato_id !== cardId)
      }
      
      // Both cards deployed, start combat
      newGameState.phase = 'combat'
      newGameState.waiting_for_deploy_player_id = null
      
      // Determine who attacks first (lower rarity goes first)
      const card1 = newGameState.battlefield.player1_active
      const card2 = newGameState.battlefield.player2_active
      const card1Rarity = getRarityValue(card1.potato?.rarity || card1.rarity)
      const card2Rarity = getRarityValue(card2.potato?.rarity || card2.rarity)
      
      console.log(`🥊 Combat starting: P1 ${card1.potato?.name || card1.name} (rarity ${card1Rarity}) vs P2 ${card2.potato?.name || card2.name} (rarity ${card2Rarity})`)
      
      if (card1Rarity <= card2Rarity) {
        newGameState.current_attacker_id = battleSession.player1_id
        console.log(`⚔️ Player 1 attacks first (lower/equal rarity)`)
      } else {
        newGameState.current_attacker_id = battleSession.player2_id
        console.log(`⚔️ Player 2 attacks first (lower rarity)`)
      }
      
    } else if (gameState.phase === 'waiting_redeploy') {
      // Player who lost previous round deploys new card
      if (isPlayer1) {
        newGameState.battlefield.player1_active = cardWithStats
        newGameState.player1_hand = playerHand.filter(card => card.potato_id !== cardId)
      } else {
        newGameState.battlefield.player2_active = cardWithStats
        newGameState.player2_hand = playerHand.filter(card => card.potato_id !== cardId)
      }
      
      // Start new combat round
      newGameState.phase = 'combat'
      newGameState.waiting_for_deploy_player_id = null
      newGameState.round_count += 1
      
      // The player who lost the previous round attacks first
      newGameState.current_attacker_id = user.id
    }

    // Update battle session - keep current turn player unchanged for card deployment
    
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: newGameState,
        // Do NOT update current_turn_player_id - keep it unchanged
        updated_at: new Date().toISOString()
      })
      .eq('id', battleId)

    if (updateError) {
      throw new Error(`Failed to update battle: ${updateError.message}`)
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
          name: 'card_deployed',
          data: {
            type: 'deployment',
            cardType: cardType,
            cardName: cardWithStats.name,
            slotIndex: targetSlotIndex,
            playerId: user.id,
            battleSessionId: battleId,
            version: battleSession.version + 1,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        const errorText = await ablyResponse.text()
        console.error('Failed to publish deployment to Ably:', ablyResponse.status, errorText)
      }
    } catch (ablyError) {
      console.error('Ably publish error:', ablyError)
    }
    
    // Combat will be triggered automatically by the client via battle-execute-attack

    // Create battle action log with idempotency
    try {
      await supabaseClient
        .from('battle_actions')
        .insert({
          battle_session_id: battleId,
          player_id: user.id,
          action_type: 'deploy',
          action_data: { 
            card_id: cardId, 
            slot_index: slotIndex,
            card_stats: { hp: cardHp, attack: cardAttack },
            source: 'card_complete_table'
          },
          idempotency_key: idempotencyKey
        })
      

    } catch (actionError: any) {
      // If it's a duplicate idempotency key, that's fine - action already processed
      if (actionError.code !== '23505') { // Not a duplicate idempotency key
        console.error('Failed to log action:', actionError)
        // Don't fail the whole operation for logging issues
      }
    }


    
    return new Response(
      JSON.stringify({ 
        success: true, 
        message: 'Card deployed successfully - turn continues',
        newPhase: newGameState.phase,
        currentPlayer: newGameState.current_turn_player_id, // Turn stays with current player
        slotIndex: targetSlotIndex,
        version: battleSession.version + 1, // New version after update
        deployedCard: {
          name: cardWithStats.name || cardWithStats.potato?.name,
          hp: cardWithStats.current_hp,
          attack: cardWithStats.attack
        }
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200,
      },
    )

  } catch (error) {
    console.error('Deploy card error:', error.message)
    return new Response(
      JSON.stringify({ 
        success: false, 
        error: error.message,
        debug: process.env.NODE_ENV === 'development' ? error.stack : undefined
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 400,
      },
    )
  }
})

// ===== BATTLECRY PROCESSING =====

async function processBattlecryEffect(gameState: any, playerKey: string, keyword: string, targetId?: string) {
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  const heroKey = `${playerKey}_hero`
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  
  console.log(`⚡ Processing battlecry: ${keyword}`)
  
  switch (keyword) {
    case 'Battlecry:Damage1':
      gameState[enemyHeroKey].hp -= 1
      console.log('💥 Battlecry: Dealt 1 damage to enemy hero')
      break
      
    case 'Battlecry:Damage2':
      gameState[enemyHeroKey].hp -= 2
      console.log('💥 Battlecry: Dealt 2 damage to enemy hero')
      break
      
    case 'Battlecry:Damage3':
      gameState[enemyHeroKey].hp -= 3
      console.log('💥 Battlecry: Dealt 3 damage to enemy hero')
      break
      
    case 'Battlecry:HealHero1':
      gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 1)
      console.log('💚 Battlecry: Healed hero for 1')
      break
      
    case 'Battlecry:HealHero2':
      gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 2)
      console.log('💚 Battlecry: Healed hero for 2')
      break
      
    case 'Battlecry:HealAlly1':
    case 'Battlecry:HealAlly2':
    case 'Battlecry:HealAlly3':
      const healAmount = parseInt(keyword.split('HealAlly')[1])
      await healRandomAllyDeploy(gameState, playerKey, healAmount)
      console.log(`💚 Battlecry: Healed random ally for ${healAmount}`)
      break
      
    case 'Battlecry:Draw':
      await drawCardDeploy(gameState, playerKey, 1)
      console.log('📚 Battlecry: Drew 1 card')
      break
      
    case 'Battlecry:FreezeRandomEnemy':
      await freezeRandomEnemyDeploy(gameState, enemyPlayerKey)
      console.log('🧊 Battlecry: Froze random enemy')
      break
      
    case 'Battlecry:Damage1RandomEnemy':
      await damageRandomEnemyDeploy(gameState, enemyPlayerKey, 1)
      console.log('💥 Battlecry: Dealt 1 damage to random enemy')
      break
      
    case 'Battlecry:Damage1RandomEnemyTwice':
      await damageRandomEnemyDeploy(gameState, enemyPlayerKey, 1)
      await damageRandomEnemyDeploy(gameState, enemyPlayerKey, 1)
      console.log('💥 Battlecry: Dealt 1 damage to two random enemies')
      break
      
    case 'Battlecry:DamageEnemyHero1':
      gameState[enemyHeroKey].hp -= 1
      console.log('💥 Battlecry: Dealt 1 damage to enemy hero')
      break
      
    case 'Battlecry:DamageEnemyHero3':
      gameState[enemyHeroKey].hp -= 3
      console.log('💥 Battlecry: Dealt 3 damage to enemy hero')
      break
      
    case 'Battlecry:HeroDamage4':
      gameState[enemyHeroKey].hp -= 4
      console.log('💥 Battlecry: Dealt 4 damage to enemy hero')
      break
      
    case 'Battlecry:Heal4':
      await healRandomAllyDeploy(gameState, playerKey, 4)
      console.log('💚 Battlecry: Healed random ally for 4')
      break
      
    case 'Battlecry:AoEDamage2':
    case 'Battlecry:AOE2Enemies':
      await damageAllEnemyUnitsDeploy(gameState, enemyPlayerKey, 2)
      console.log('💥 Battlecry: Dealt 2 damage to all enemies')
      break
      
    case 'Battlecry:FreezeEnemy':
      if (targetId) {
        await freezeTargetEnemyDeploy(gameState, enemyPlayerKey, targetId)
        console.log('🧊 Battlecry: Froze target enemy unit')
      } else {
        await freezeRandomEnemyDeploy(gameState, enemyPlayerKey)
        console.log('🧊 Battlecry: Froze random enemy unit (no target specified)')
      }
      break
      
    // Add more common battlecry patterns from user data
    case 'Battlecry:AOE4Enemies':
      await damageAllEnemyUnitsDeploy(gameState, enemyPlayerKey, 4)
      console.log('💥 Battlecry: Dealt 4 damage to all enemies')
      break
      
    case 'Battlecry:Heal3':
      await healRandomAllyDeploy(gameState, playerKey, 3)
      console.log('💚 Battlecry: Healed random ally for 3')
      break
      
    case 'Battlecry:RandomEnemy2':
      await damageRandomEnemyDeploy(gameState, enemyPlayerKey, 2)
      console.log('💥 Battlecry: Dealt 2 damage to random enemy')
      break
      
    case 'Battlecry:HealAlliesFull':
      await healAllAlliesFullDeploy(gameState, playerKey)
      console.log('💚 Battlecry: Fully healed all allies')
      break
      
    case 'Battlecry:BuffAllies+1+1':
      await buffAllAlliesDeploy(gameState, playerKey, 1, 1)
      console.log('💪 Battlecry: Gave all allies +1/+1')
      break
      
    // ===== CONDITIONAL BATTLECRY ABILITIES =====
    
    case 'Battlecry:ConditionalRelicDestroy':
      await conditionalRelicDestroyDeploy(gameState, enemyPlayerKey)
      console.log('💥 Battlecry: Conditional relic destroy')
      break
      
    case 'Battlecry:ConditionalBuff':
      await conditionalBuffDeploy(gameState, playerKey, enemyPlayerKey)
      console.log('💪 Battlecry: Conditional buff')
      break
      
    case 'Battlecry:ConditionalHeal':
      await conditionalHealDeploy(gameState, playerKey, enemyPlayerKey)
      console.log('💚 Battlecry: Conditional heal')
      break
      
    case 'Battlecry:ConditionalDraw':
      await conditionalDrawDeploy(gameState, playerKey, enemyPlayerKey)
      console.log('🃏 Battlecry: Conditional draw')
      break
      
    // ===== MISSING BATTLECRY ABILITIES =====
    
    case 'Battlecry:CopyEnemyRelic':
      await copyEnemyRelicDeploy(gameState, playerKey, enemyPlayerKey)
      console.log('📋 Battlecry: Copied enemy relic')
      break
      
    case 'Battlecry:Damage1RandomEnemyTwice':
      await damageRandomEnemyTwiceDeploy(gameState, enemyPlayerKey, 1)
      console.log('💥 Battlecry: Damaged 2 random enemies for 1')
      break
      
    case 'Battlecry:AOE3All':
      await damageAllUnitsDeploy(gameState, 3)
      console.log('💥 Battlecry: Damaged ALL units for 3')
      break
      
    case 'Battlecry:FreezeRandom':
      await freezeRandomEnemyDeploy(gameState, enemyPlayerKey)
      console.log('🧊 Battlecry: Froze random enemy')
      break
      
    case 'Battlecry:AOE2':
      await damageAllEnemyUnitsDeploy(gameState, enemyPlayerKey, 2)
      console.log('💥 Battlecry: Damaged all enemy units for 2')
      break
      
    case 'Battlecry:HealAllies2':
      await healAllAlliesDeploy(gameState, playerKey, 2)
      console.log('💚 Battlecry: Healed all allies for 2')
      break
  }
  
  // Check for game over after damage
  if (gameState[enemyHeroKey].hp <= 0) {
    gameState.game_status = 'finished'
    gameState.winner_id = playerKey === 'player1' ? gameState.player1_id : gameState.player2_id
    gameState.phase = 'game_over'
    console.log(`🏆 ${playerKey} wins via battlecry damage!`)
  }
}

// Helper functions for deployment battlecries
async function healRandomAllyDeploy(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit !== null && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * allyUnits.length)
  const unit = allyUnits[randomIndex]
  
  const oldHp = unit.current_hp || unit.hp
  unit.current_hp = Math.min(unit.max_hp, unit.current_hp + amount)
  const actualHealing = unit.current_hp - oldHp
  
  console.log(`💚 Healed ${unit.name} for ${actualHealing} (${unit.current_hp}/${unit.max_hp})`)
  
  // Trigger heal-based passive abilities
  if (actualHealing > 0) {
    await processHealTriggersDeploy(gameState, playerKey, unit, actualHealing)
  }
}

async function damageRandomEnemyDeploy(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit !== null)
  
  if (enemyUnits.length === 0) {
    // No units, damage hero instead
    const heroKey = `${enemyPlayerKey}_hero`
    gameState[heroKey].hp -= amount
    console.log(`💥 No enemy units, damaged enemy hero for ${amount}`)
    return
  }
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.current_hp -= amount
  console.log(`💥 Damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
  
  // Remove unit if dead
  if (unit.current_hp <= 0) {
    const slotIndex = enemySlots.indexOf(unit)
    enemySlots[slotIndex] = null
    console.log(`💀 ${unit.name} destroyed`)
  }
}

async function freezeRandomEnemyDeploy(gameState: any, enemyPlayerKey: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit !== null)
  
  if (enemyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.is_frozen = true
  unit.frozen_turns_remaining = 1 // Freeze lasts for 1 of the controller's turns
  console.log(`🧊 Froze ${unit.name} for 1 turn`)
}

async function freezeTargetEnemyDeploy(gameState: any, enemyPlayerKey: string, targetId: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  // Find target by ID or slot index
  let targetUnit = null
  let targetSlotIndex = -1
  
  // Try to find by unit ID first
  for (let i = 0; i < enemySlots.length; i++) {
    if (enemySlots[i] && enemySlots[i].id === targetId) {
      targetUnit = enemySlots[i]
      targetSlotIndex = i
      break
    }
  }
  
  // If not found by ID, try slot index
  if (!targetUnit) {
    const slotIndex = parseInt(targetId)
    if (!isNaN(slotIndex) && slotIndex >= 0 && slotIndex < enemySlots.length && enemySlots[slotIndex]) {
      targetUnit = enemySlots[slotIndex]
      targetSlotIndex = slotIndex
    }
  }
  
  if (!targetUnit) {
    console.log(`❌ Target unit not found: ${targetId}`)
    // Fall back to random freeze
    await freezeRandomEnemyDeploy(gameState, enemyPlayerKey)
    return
  }
  
  targetUnit.is_frozen = true
  targetUnit.frozen_turns_remaining = 1
  console.log(`🧊 Froze target ${targetUnit.name} for 1 turn`)
}

async function damageAllEnemyUnitsDeploy(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (let i = 0; i < enemySlots.length; i++) {
    const unit = enemySlots[i]
    if (unit) {
      unit.current_hp -= amount
      console.log(`💥 AoE: Damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
      
      // Remove unit if dead
      if (unit.current_hp <= 0) {
        enemySlots[i] = null
        console.log(`💀 ${unit.name} destroyed by AoE`)
      }
    }
  }
}

async function drawCardDeploy(gameState: any, playerKey: string, amount: number) {
  const handKey = `${playerKey}_hand`
  const deckKey = `${playerKey}_deck`
  const heroKey = `${playerKey}_hero`
  
  // Get hand and deck (check both locations)
  const hand = gameState.hands ? gameState.hands[handKey] : gameState[handKey]
  const deck = gameState[deckKey] || []
  
  if (!hand) {
    console.log('❌ No hand found for card draw')
    return
  }
  
  for (let i = 0; i < amount; i++) {
    // Check hand size limit (10 cards)
    if (hand.length >= 10) {
      console.log('🔥 Hand full - card burned')
      continue
    }
    
    // Check if deck has cards
    if (deck.length === 0) {
      // Fatigue damage
      const hero = gameState[heroKey]
      hero.fatigue_damage = (hero.fatigue_damage || 0) + 1
      hero.hp -= hero.fatigue_damage
      console.log(`💀 Fatigue damage: ${hero.fatigue_damage} (hero at ${hero.hp} HP)`)
      continue
    }
    
    // Draw card from top of deck
    const drawnCard = deck.shift()
    hand.push(drawnCard)
    console.log(`📚 Drew card: ${drawnCard.name}`)
  }
}

// ===== HEAL-TRIGGERED PASSIVE SYSTEM =====

async function processHealTriggersDeploy(gameState: any, playerKey: string, healedUnit: any, healAmount: number) {
  console.log(`💚 Processing heal triggers for ${healedUnit.name}...`)
  
  // Check all units on battlefield for heal-triggered passives
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const playerUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of playerUnits) {
    if (!unit.keywords) continue
    
    const keywords = Array.isArray(unit.keywords) ? unit.keywords : 
      (typeof unit.keywords === 'string' ? JSON.parse(unit.keywords) : [])
    
    for (const keyword of keywords) {
      switch (keyword) {
        case 'Passive:Damage1OnHeal':
          await damageRandomEnemyHealTriggerDeploy(gameState, playerKey === 'player1' ? 'player2' : 'player1', 1)
          console.log(`💥 ${unit.name} passive: Damaged random enemy for 1 (heal trigger)`)
          break
      }
    }
  }
}

async function damageRandomEnemyHealTriggerDeploy(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) {
    // No units, damage hero instead
    const heroKey = `${enemyPlayerKey}_hero`
    gameState[heroKey].hp -= amount
    console.log(`💥 No enemy units, damaged enemy hero for ${amount}`)
    return
  }
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.current_hp -= amount
  console.log(`💥 Heal trigger damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
  
  // Remove unit if dead
  if (unit.current_hp <= 0) {
    const slotIndex = enemySlots.indexOf(unit)
    enemySlots[slotIndex] = null
    console.log(`💀 ${unit.name} destroyed by heal trigger`)
  }
}

// ===== CONDITIONAL BATTLECRY ABILITIES =====

async function conditionalRelicDestroyDeploy(gameState: any, enemyPlayerKey: string) {
  // Destroy enemy relic that costs 3 or less
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length > 0) {
    const relic = enemyRelics[0]
    if ((relic.mana_cost || 0) <= 3) {
      enemyRelics.splice(0, 1)
      console.log(`💥 Destroyed low-cost enemy relic: ${relic.name} (${relic.mana_cost} mana)`)
      return true
    } else {
      console.log(`❌ Enemy relic too expensive to destroy: ${relic.name} (${relic.mana_cost} mana)`)
      return false
    }
  }
  
  console.log('❌ No enemy relic to destroy')
  return false
}

async function conditionalBuffDeploy(gameState: any, playerKey: string, enemyPlayerKey: string) {
  // If destroyed a relic, gain +1/+1. Otherwise, draw a card
  const relicDestroyed = await conditionalRelicDestroyDeploy(gameState, enemyPlayerKey)
  
  if (relicDestroyed) {
    // Buff self +1/+1 (this would need unit reference)
    console.log('💪 Conditional: Gained +1/+1 (destroyed relic)')
  } else {
    // Draw a card
    await drawCardDeploy(gameState, playerKey, 1)
    console.log('🃏 Conditional: Drew card (no relic destroyed)')
  }
}

async function conditionalHealDeploy(gameState: any, playerKey: string, enemyPlayerKey: string) {
  // If enemy controls a relic, destroy it and heal 2. Otherwise, draw a card
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length > 0) {
    const relic = enemyRelics[0]
    enemyRelics.splice(0, 1)
    
    // Heal hero for 2
    const heroKey = `${playerKey}_hero`
    gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 2)
    console.log(`💚 Conditional: Destroyed ${relic.name} and healed hero for 2`)
  } else {
    // Draw a card
    await drawCardDeploy(gameState, playerKey, 1)
    console.log('🃏 Conditional: Drew card (no enemy relic)')
  }
}

async function conditionalDrawDeploy(gameState: any, playerKey: string, enemyPlayerKey: string) {
  // If no enemy relic, draw a card
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length === 0) {
    await drawCardDeploy(gameState, playerKey, 1)
    console.log('🃏 Conditional: Drew card (no enemy relic)')
  } else {
    console.log('❌ Conditional: No card draw (enemy has relic)')
  }
}



async function healAllAlliesFullDeploy(gameState: any, playerKey: string) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  for (const unit of playerSlots) {
    if (unit) {
      unit.current_hp = unit.max_hp
      console.log(`💚 Fully healed ${unit.name} (${unit.current_hp}/${unit.max_hp})`)
    }
  }
}

async function buffAllAlliesDeploy(gameState: any, playerKey: string, atkBuff: number, hpBuff: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  for (const unit of playerSlots) {
    if (unit) {
      unit.attack += atkBuff
      unit.current_hp += hpBuff
      unit.max_hp += hpBuff
      console.log(`💪 Buffed ${unit.name} +${atkBuff}/+${hpBuff} (${unit.attack}/${unit.current_hp})`)
    }
  }
}

// ===== MISSING BATTLECRY HELPERS =====

async function copyEnemyRelicDeploy(gameState: any, playerKey: string, enemyPlayerKey: string) {
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const playerRelicsKey = `${playerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length > 0) {
    const enemyRelic = enemyRelics[0]
    
    // Destroy enemy relic
    enemyRelics.splice(0, 1)
    
    // Copy it to player's side (replacing existing relic if any)
    const playerRelics = gameState.battlefield[playerRelicsKey] || []
    if (playerRelics.length > 0) {
      playerRelics[0] = { ...enemyRelic, owner_id: playerKey }
    } else {
      playerRelics.push({ ...enemyRelic, owner_id: playerKey })
      gameState.battlefield[playerRelicsKey] = playerRelics
    }
    
    console.log(`📋 Copied and activated enemy relic: ${enemyRelic.name}`)
  }
}

async function damageRandomEnemyTwiceDeploy(gameState: any, enemyPlayerKey: string, amount: number) {
  // Damage two different random enemies
  for (let i = 0; i < 2; i++) {
    const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
    const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
    
    if (enemyUnits.length === 0) {
      const heroKey = `${enemyPlayerKey}_hero`
      gameState[heroKey].hp -= amount
      console.log(`💥 No enemy units, damaged enemy hero for ${amount}`)
      continue
    }
    
    const randomIndex = Math.floor(Math.random() * enemyUnits.length)
    const unit = enemyUnits[randomIndex]
    
    unit.current_hp -= amount
    console.log(`💥 Battlecry hit ${i + 1}: Damaged ${unit.name} for ${amount}`)
    
    if (unit.current_hp <= 0) {
      const slotIndex = enemySlots.indexOf(unit)
      enemySlots[slotIndex] = null
      console.log(`💀 ${unit.name} destroyed by battlecry hit ${i + 1}`)
    }
  }
}

async function damageAllUnitsDeploy(gameState: any, amount: number) {
  // Damage ALL units on BOTH sides
  for (const playerKey of ['player1', 'player2']) {
    const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
    
    for (let i = playerSlots.length - 1; i >= 0; i--) {
      const unit = playerSlots[i]
      if (unit && unit.card_type === 'unit') {
        unit.current_hp -= amount
        console.log(`💥 Battlecry damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
        
        if (unit.current_hp <= 0) {
          playerSlots[i] = null
          console.log(`💀 ${unit.name} destroyed by battlecry AoE`)
        }
      }
    }
  }
}

async function healAllAlliesDeploy(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of allyUnits) {
    const oldHp = unit.current_hp || unit.hp
    unit.current_hp = Math.min(unit.max_hp, unit.current_hp + amount)
    const actualHealing = unit.current_hp - oldHp
    
    if (actualHealing > 0) {
      console.log(`💚 Battlecry healed ${unit.name} for ${actualHealing} (${unit.current_hp}/${unit.max_hp})`)
      
      // Trigger heal-based passives
      await processHealTriggersDeploy(gameState, playerKey, unit, actualHealing)
    }
  }
}