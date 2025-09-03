import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

interface PlayCardAction {
  battleId: string
  cardId: string
  cardIndex: number
  targetId?: string // For targeted spells/abilities
  position?: number // For unit placement
}

serve(async (req) => {
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    console.log('🃏 BATTLE-PLAY-CARD FUNCTION CALLED')
    
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

    // Parse request
    const requestBody = await req.json()
    const { battleId, cardId, cardIndex, targetId, position = -1 }: PlayCardAction = requestBody

    console.log(`🃏 Playing card: ${cardId} from index ${cardIndex} in battle: ${battleId}`)
    
    if (!battleId || !cardId || cardIndex < 0) {
      throw new Error('Missing required parameters: battleId, cardId, cardIndex')
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

    // Validate game state
    if (gameState.game_status !== 'active') {
      throw new Error('Game is not active')
    }

    if (gameState.current_turn_player_id !== user.id) {
      throw new Error('Not your turn')
    }

    if (gameState.turn_phase !== 'main') {
      throw new Error('Can only play cards during main phase')
    }

    // Process card play
    const updatedGameState = await playCard(gameState, battle, user.id, isPlayer1, cardId, cardIndex, targetId, position)

    // Update battle state
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: updatedGameState,
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
          name: 'card_played',
          data: {
            type: 'play_card',
            cardId,
            playerId: user.id,
            battleId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish card play to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published card play to Ably')
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
        action_type: 'play_card',
        action_data: { 
          cardId, 
          cardIndex, 
          targetId, 
          position,
          timestamp: new Date().toISOString() 
        }
      })

    console.log('✅ Card played successfully')

    return new Response(
      JSON.stringify({ success: true, gameState: updatedGameState }),
      { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
    )

  } catch (error) {
    console.error('Play card error:', error)
    return new Response(
      JSON.stringify({ success: false, error: error.message }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 400 
      }
    )
  }
})

async function playCard(
  gameState: any, 
  battle: any, 
  playerId: string, 
  isPlayer1: boolean, 
  cardId: string, 
  cardIndex: number, 
  targetId?: string, 
  position?: number
) {
  console.log('🎯 Processing card play...')
  
  const playerKey = isPlayer1 ? 'player1' : 'player2'
  const heroKey = `${playerKey}_hero`
  const handKey = `${playerKey}_hand`
  const deckKey = `${playerKey}_deck`
  const graveyardKey = `${playerKey}_graveyard`
  
  // Get player's hand and validate card
  // Check if hands are in gameState.hands or directly in gameState
  const hand = gameState.hands ? gameState.hands[handKey] : gameState[handKey]
  if (!hand || cardIndex >= hand.length) {
    console.log(`❌ Invalid card index: ${cardIndex}, hand length: ${hand?.length || 0}`)
    throw new Error('Invalid card index')
  }

  const card = hand[cardIndex]
  if (!card || card.id !== cardId) {
    throw new Error('Card mismatch or not found')
  }

  // Check mana cost
  const hero = gameState[heroKey]
  if (card.mana_cost > hero.mana) {
    throw new Error(`Not enough mana. Need ${card.mana_cost}, have ${hero.mana}`)
  }

  // Remove card from hand
  const newHand = [...hand]
  newHand.splice(cardIndex, 1)
  
  // Update hand in correct location
  if (gameState.hands) {
    gameState.hands[handKey] = newHand
  } else {
    gameState[handKey] = newHand
  }

  // Spend mana
  gameState[heroKey].mana -= card.mana_cost

  // Process card based on type
  switch (card.card_type) {
    case 'unit':
      await playUnitCard(gameState, playerKey, card, position)
      break
    case 'spell':
      await playSpellCard(gameState, battle, playerKey, card, targetId)
      // Spells go to graveyard immediately
      if (!gameState[graveyardKey]) {
        gameState[graveyardKey] = []
      }
      gameState[graveyardKey].push({
        ...card,
        played_at: new Date().toISOString()
      })
      break
    case 'structure':
      await playStructureCard(gameState, playerKey, card, position)
      break
    default:
      throw new Error(`Unknown card type: ${card.card_type}`)
  }

  console.log(`✅ ${card.card_type} card "${card.name}" played successfully`)
  return gameState
}

async function playUnitCard(gameState: any, playerKey: string, card: any, position: number) {
  console.log('👥 Playing unit card:', card.name)
  
  const unitsKey = `${playerKey}_units`
  const units = gameState.battlefield[unitsKey] || []
  
  // Check battlefield space (max 7 units)
  if (units.length >= 7) {
    throw new Error('Battlefield is full (maximum 7 units)')
  }

  // Create unit instance
  const unitInstance = {
    id: `${card.id}_${Date.now()}_${Math.random()}`,
    cardId: card.id,
    name: card.name,
    card_type: card.card_type,
    mana_cost: card.mana_cost,
    attack: card.attack,
    health: card.health,
    current_attack: card.attack,
    current_health: card.health,
    max_health: card.health,
    
    // Combat states
    summoning_sickness: true, // Can't attack the turn it's played
    can_attack: false,
    has_attacked: false,
    attacks_this_turn: 0,
    
    // Abilities and keywords
    abilities: card.abilities || [],
    keywords: card.keywords || [],
    
    // Status effects
    buffs: [],
    debuffs: [],
    
    // Metadata
    owner_id: playerKey,
    played_at: new Date().toISOString(),
    turn_played: gameState.turn_count
  }

  // Process keywords and abilities
  await processCardKeywords(gameState, playerKey, card, unitInstance)
  
  // Process battlecry effects
  if (card.keywords?.some((k: string) => k.startsWith('Battlecry:'))) {
    await processBattlecryEffects(gameState, playerKey, card, targetId)
  }

  // Insert at specified position or at the end
  if (position >= 0 && position <= units.length) {
    units.splice(position, 0, unitInstance)
  } else {
    units.push(unitInstance)
  }

  gameState.battlefield[unitsKey] = units

  // Trigger battlecry effects
  if (card.keywords?.some((k: any) => k.keyword === 'battlecry')) {
    await triggerBattlecry(gameState, playerKey, unitInstance)
  }
}

async function playSpellCard(gameState: any, battle: any, playerKey: string, card: any, targetId?: string) {
  console.log('✨ Playing spell card:', card.name)
  console.log('🔍 Spell keywords:', card.keywords)
  
  // Process spell effects based on keywords
  if (card.keywords && Array.isArray(card.keywords)) {
    for (const keyword of card.keywords) {
      await processSpellKeyword(gameState, battle, playerKey, keyword, targetId)
    }
  }
  
  // Fallback to legacy spell properties if no keywords
  if (!card.keywords || card.keywords.length === 0) {
    if (card.spell_damage && card.spell_damage > 0) {
      console.log(`💥 Legacy damage spell: ${card.spell_damage} damage`)
      const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
      const enemyHeroKey = `${enemyPlayerKey}_hero`
      
      gameState[enemyHeroKey].hp -= card.spell_damage
      
      if (gameState[enemyHeroKey].hp <= 0) {
        gameState.game_status = 'finished'
        gameState.winner_id = playerKey === 'player1' ? battle.player1_id : battle.player2_id
        gameState.phase = 'game_over'
        console.log(`🏆 ${playerKey} wins via spell damage!`)
      }
    } else if (card.heal_amount && card.heal_amount > 0) {
      console.log(`💚 Legacy healing spell: ${card.heal_amount} healing`)
      const heroKey = `${playerKey}_hero`
      gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + card.heal_amount)
    }
  }
  
  // Trigger spell-cast passive abilities from units on battlefield
  await processSpellCastTriggers(gameState, playerKey, card)
  
  console.log(`✅ Spell "${card.name}" effects applied`)
}

async function playStructureCard(gameState: any, playerKey: string, card: any, position: number) {
  console.log('🏗️ Playing structure card:', card.name)
  
  const structuresKey = `${playerKey}_structures`
  const structures = gameState.battlefield[structuresKey] || []
  
  // Check structure space (max 3 structures)
  if (structures.length >= 3) {
    throw new Error('Too many structures (maximum 3)')
  }

  // Create structure instance
  const structureInstance = {
    id: `${card.id}_${Date.now()}_${Math.random()}`,
    cardId: card.id,
    name: card.name,
    card_type: card.card_type,
    mana_cost: card.mana_cost,
    health: card.health,
    current_health: card.health,
    max_health: card.health,
    
    // Structure properties
    abilities: card.abilities || [],
    keywords: card.keywords || [],
    
    // Metadata
    owner_id: playerKey,
    played_at: new Date().toISOString(),
    turn_played: gameState.turn_count
  }

  structures.push(structureInstance)
  gameState.battlefield[structuresKey] = structures

  // Trigger structure effects
  await triggerStructureEffects(gameState, playerKey, structureInstance)
}

// Spell implementations
async function castLightningBolt(gameState: any, battle: any, playerKey: string, targetId?: string) {
  const damage = 3
  console.log(`⚡ Lightning Bolt: ${damage} damage`)
  
  if (!targetId) {
    throw new Error('Lightning Bolt requires a target')
  }

  await dealDamage(gameState, battle, targetId, damage)
}

async function castHealingPotion(gameState: any, playerKey: string, targetId?: string) {
  const healing = 5
  console.log(`💚 Healing Potion: ${healing} healing`)
  
  if (!targetId) {
    // Heal own hero if no target specified
    const heroKey = `${playerKey}_hero`
    gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + healing)
    return
  }

  await healTarget(gameState, targetId, healing)
}

async function castPotatoStorm(gameState: any, battle: any, playerKey: string) {
  const damage = 1
  console.log(`🌪️ Potato Storm: ${damage} damage to all enemies`)
  
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  const enemyUnitsKey = `${enemyPlayerKey}_units`
  
  // Damage enemy hero
  gameState[enemyHeroKey].hp -= damage
  
  // Damage all enemy units
  const enemyUnits = gameState.battlefield[enemyUnitsKey] || []
  for (const unit of enemyUnits) {
    unit.current_health -= damage
    if (unit.current_health <= 0) {
      await destroyUnit(gameState, enemyPlayerKey, unit)
    }
  }
  
  // Check for game end
  if (gameState[enemyHeroKey].hp <= 0) {
    gameState.game_status = 'finished'
    gameState.winner_id = playerKey === 'player1' ? battle.player1_id : battle.player2_id
    gameState.phase = 'game_over'
  }
}

async function castManaBoost(gameState: any, playerKey: string) {
  console.log('💎 Mana Boost: +2 mana this turn')
  
  const heroKey = `${playerKey}_hero`
  gameState[heroKey].mana = Math.min(10, gameState[heroKey].mana + 2)
}

// Utility functions
async function dealDamage(gameState: any, battle: any, targetId: string, damage: number) {
  // Find target (hero or unit)
  let target = null
  let targetType = null
  let ownerKey = null

  // Check heroes
  if (targetId === 'player1_hero') {
    target = gameState.player1_hero
    targetType = 'hero'
    ownerKey = 'player1'
  } else if (targetId === 'player2_hero') {
    target = gameState.player2_hero
    targetType = 'hero'
    ownerKey = 'player2'
  } else {
    // Check units
    for (const playerKey of ['player1', 'player2']) {
      const unitsKey = `${playerKey}_units`
      const units = gameState.battlefield[unitsKey] || []
      target = units.find((unit: any) => unit.id === targetId)
      if (target) {
        targetType = 'unit'
        ownerKey = playerKey
        break
      }
    }
  }

  if (!target) {
    throw new Error(`Target ${targetId} not found`)
  }

  console.log(`💥 Dealing ${damage} damage to ${targetType}`)

  if (targetType === 'hero') {
    target.hp -= damage
    
    // Check for game end
    if (target.hp <= 0) {
      gameState.game_status = 'finished'
      gameState.winner_id = ownerKey === 'player1' ? battle.player2_id : battle.player1_id
      gameState.phase = 'game_over'
    }
  } else if (targetType === 'unit') {
    target.current_health -= damage
    
    // Check for unit death
    if (target.current_health <= 0) {
      await destroyUnit(gameState, ownerKey!, target)
    }
  }
}

async function healTarget(gameState: any, targetId: string, healing: number) {
  // Similar to dealDamage but for healing
  console.log(`💚 Healing ${healing} to ${targetId}`)
  // Implementation would be similar to dealDamage
}

async function destroyUnit(gameState: any, ownerKey: string, unit: any) {
  console.log(`💀 Unit destroyed: ${unit.name}`)
  
  const unitsKey = `${ownerKey}_units`
  const graveyardKey = `${ownerKey}_graveyard`
  
  // Remove from battlefield
  const units = gameState.battlefield[unitsKey] || []
  const index = units.findIndex((u: any) => u.id === unit.id)
  if (index !== -1) {
    units.splice(index, 1)
    gameState.battlefield[unitsKey] = units
  }
  
  // Add to graveyard
  unit.destroyed_at = new Date().toISOString()
  gameState[graveyardKey].push(unit)
  
  // Trigger deathrattle effects
  if (unit.keywords?.some((k: any) => k.keyword === 'deathrattle')) {
    await triggerDeathrattle(gameState, ownerKey, unit)
  }
}

async function triggerBattlecry(gameState: any, playerKey: string, unit: any) {
  console.log(`⚡ Triggering battlecry for ${unit.name}`)
  // Implement specific battlecry effects based on unit
}

async function triggerDeathrattle(gameState: any, playerKey: string, unit: any) {
  console.log(`💀 Triggering deathrattle for ${unit.name}`)
  // Implement specific deathrattle effects based on unit
}

async function triggerStructureEffects(gameState: any, playerKey: string, structure: any) {
  console.log(`🏗️ Triggering structure effects for ${structure.name}`)
  // Implement structure effects
}

// ===== ABILITY PROCESSING SYSTEM =====

async function processCardKeywords(gameState: any, playerKey: string, card: any, unitInstance: any) {
  if (!card.keywords || !Array.isArray(card.keywords)) return
  
  console.log(`🎯 Processing keywords for ${card.name}:`, card.keywords)
  
  for (const keyword of card.keywords) {
    switch (keyword) {
      case 'Charge':
        unitInstance.summoning_sickness = false
        unitInstance.can_attack = true
        unitInstance.has_charge = true
        console.log('⚡ Applied Charge - can attack immediately')
        break
        
      case 'Taunt':
        unitInstance.has_taunt = true
        console.log('🛡️ Applied Taunt - must be attacked first')
        break
        
      case 'Lifesteal':
        unitInstance.has_lifesteal = true
        console.log('🩸 Applied Lifesteal - heals when dealing damage')
        break
        
      case 'DoubleStrike':
        unitInstance.has_double_strike = true
        console.log('⚔️ Applied Double Strike - attacks twice')
        break
        
      case 'Poison':
        unitInstance.has_poison = true
        console.log('☠️ Applied Poison - destroys any unit damaged')
        break
        
      case 'PoisonTouch':
        unitInstance.has_poison_touch = true
        console.log('☠️ Applied Poison Touch - poisons units damaged')
        break
        
      default:
        // Handle complex keywords that start with specific prefixes
        if (keyword.startsWith('Battlecry:')) {
          await processBattlecryKeyword(gameState, playerKey, keyword)
        } else if (keyword.startsWith('Deathrattle:')) {
          // Store deathrattle for later processing
          if (!unitInstance.deathrattle_effects) unitInstance.deathrattle_effects = []
          unitInstance.deathrattle_effects.push(keyword)
          console.log(`💀 Stored deathrattle: ${keyword}`)
        }
        break
    }
  }
}

async function processBattlecryEffects(gameState: any, playerKey: string, card: any, targetId?: string) {
  if (!card.keywords || !Array.isArray(card.keywords)) return
  
  console.log(`⚡ Processing battlecry effects for ${card.name}`)
  
  for (const keyword of card.keywords) {
    if (keyword.startsWith('Battlecry:')) {
      await processBattlecryKeyword(gameState, playerKey, keyword)
    }
  }
}

async function processBattlecryKeyword(gameState: any, playerKey: string, keyword: string) {
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  const heroKey = `${playerKey}_hero`
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  
  console.log(`⚡ Processing battlecry: ${keyword}`)
  
  switch (keyword) {
    case 'Battlecry:Damage1':
      // Deal 1 damage to enemy hero (for now, can add targeting later)
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
      // Heal random ally for now (can add targeting later)
      const healAmount = parseInt(keyword.split('HealAlly')[1])
      await healRandomAlly(gameState, playerKey, healAmount)
      console.log(`💚 Battlecry: Healed random ally for ${healAmount}`)
      break
      
    case 'Battlecry:Draw':
      await drawCard(gameState, playerKey, 1)
      console.log('📚 Battlecry: Drew 1 card')
      break
      
    case 'Battlecry:FreezeRandomEnemy':
      await freezeRandomEnemy(gameState, enemyPlayerKey)
      console.log('🧊 Battlecry: Froze random enemy')
      break
      
    case 'Battlecry:Damage1RandomEnemy':
      await damageRandomEnemy(gameState, enemyPlayerKey, 1)
      console.log('💥 Battlecry: Dealt 1 damage to random enemy')
      break
      
    case 'Battlecry:Damage1RandomEnemyTwice':
      await damageRandomEnemy(gameState, enemyPlayerKey, 1)
      await damageRandomEnemy(gameState, enemyPlayerKey, 1)
      console.log('💥 Battlecry: Dealt 1 damage to two random enemies')
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

async function processSpellKeyword(gameState: any, battle: any, playerKey: string, keyword: string, targetId?: string) {
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  const heroKey = `${playerKey}_hero`
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  
  console.log(`🪄 Processing spell keyword: ${keyword}`)
  
  switch (keyword) {
    case 'Spell:Damage1':
      gameState[enemyHeroKey].hp -= 1
      console.log('💥 Spell: Dealt 1 damage to enemy hero')
      break
      
    case 'Spell:Damage2':
      gameState[enemyHeroKey].hp -= 2
      console.log('💥 Spell: Dealt 2 damage to enemy hero')
      break
      
    case 'Spell:Damage3':
      gameState[enemyHeroKey].hp -= 3
      console.log('💥 Spell: Dealt 3 damage to enemy hero')
      break
      
    case 'Spell:Heal2':
      gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 2)
      console.log('💚 Spell: Healed hero for 2')
      break
      
    case 'Spell:HealHero2':
      gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 2)
      console.log('💚 Spell: Healed hero for 2')
      break
      
    case 'Spell:HealHero3':
      gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 3)
      console.log('💚 Spell: Healed hero for 3')
      break
      
    case 'Spell:FreezeTarget':
      // Freeze target unit (requires targeting system)
      console.log('🧊 Spell: Freeze target (targeting needed)')
      break
      
    case 'Spell:AoE1AllEnemies':
      await damageAllEnemyUnits(gameState, enemyPlayerKey, 1)
      console.log('💥 Spell: Dealt 1 damage to all enemy units')
      break
      
    case 'Spell:AoE2AllEnemies':
      await damageAllEnemyUnits(gameState, enemyPlayerKey, 2)
      console.log('💥 Spell: Dealt 2 damage to all enemy units')
      break
      
    case 'Spell:AoEHealAllies1':
      await healAllAllies(gameState, playerKey, 1)
      console.log('💚 Spell: Healed all allies for 1')
      break
      
    case 'Spell:Draw':
      await drawCard(gameState, playerKey, 1)
      console.log('📚 Spell: Drew 1 card')
      break
      
    case 'Spell:SilenceEnemy':
      // Silence random enemy unit
      await silenceRandomEnemy(gameState, enemyPlayerKey)
      console.log('🔇 Spell: Silenced random enemy')
      break
      
    // ===== CONDITIONAL SPELL ABILITIES =====
    
    case 'Spell:DestroyEnemyRelic':
      await destroyEnemyRelic(gameState, enemyPlayerKey)
      console.log('💥 Spell: Destroyed enemy relic')
      break
      
    case 'Spell:ConditionalDraw':
      // Draw card if no enemy relic was destroyed
      await conditionalDrawCard(gameState, playerKey, enemyPlayerKey)
      console.log('🃏 Spell: Conditional card draw')
      break
      
    case 'Spell:ConditionalDisableRelic':
      await conditionalDisableRelic(gameState, enemyPlayerKey)
      console.log('🔒 Spell: Conditionally disabled enemy relic')
      break
      
    case 'Spell:FreezeTarget':
      if (targetId) {
        await freezeTargetUnit(gameState, targetId)
        console.log('🧊 Spell: Froze target unit')
      } else {
        await freezeRandomEnemy(gameState, enemyPlayerKey)
        console.log('🧊 Spell: Froze random enemy (no target)')
      }
      break
      
    case 'Spell:ChoiceEffect':
      // Complex choice-based spell (implement specific logic per card)
      await processChoiceEffect(gameState, playerKey, targetId)
      console.log('🔀 Spell: Choice effect processed')
      break
      
    // ===== MISSING SPELL ABILITIES =====
    
    case 'Spell:AoEBuffAtk+1':
      await buffAllAlliesAttack(gameState, playerKey, 1)
      console.log('💪 Spell: Gave all allies +1 ATK')
      break
      
    case 'Spell:AoEDamageAllies1':
      await damageAllAllies(gameState, playerKey, 1)
      console.log('💥 Spell: Damaged all allies for 1')
      break
      
    case 'Spell:Damage2VoidOnly':
      await damageVoidUnitsOnly(gameState, enemyPlayerKey, 2)
      console.log('💥 Spell: Damaged Void units for 2')
      break
      
    case 'Spell:Damage3VoidOnly':
      await damageVoidUnitsOnly(gameState, enemyPlayerKey, 3)
      console.log('💥 Spell: Damaged Void units for 3')
      break
      
    case 'Spell:Damage3IfFrozen':
      await damageIfFrozen(gameState, enemyPlayerKey, 3, targetId)
      console.log('🧊💥 Spell: Damaged frozen unit for 3')
      break
      
    case 'Spell:DamageUnit5':
      await damageTargetUnit(gameState, targetId, 5)
      console.log('💥 Spell: Damaged unit for 5')
      break
      
    case 'Spell:Damage5RandomEnemy':
      await damageRandomEnemy(gameState, enemyPlayerKey, 5)
      console.log('💥 Spell: Damaged random enemy for 5')
      break
      
    case 'Spell:ChainDamage1Twice':
      await chainDamageTwice(gameState, enemyPlayerKey, 1)
      console.log('⚡ Spell: Chain damage 1 to two enemies')
      break
      
    case 'Spell:ChainKillDamage1':
      await chainKillDamage(gameState, enemyPlayerKey, 1)
      console.log('⚡💀 Spell: Chain kill damage')
      break
      
    case 'Spell:SummonHusks2':
      await summonMultipleHusks(gameState, playerKey, 2)
      console.log('👻 Spell: Summoned 2 Husks')
      break
      
    case 'Spell:DestroyAlly':
      await destroyAlly(gameState, playerKey)
      console.log('💀 Spell: Destroyed ally')
      break
      
    case 'Spell:SacrificeLowestAlly':
      await sacrificeLowestAlly(gameState, playerKey)
      console.log('💀 Spell: Sacrificed lowest HP ally')
      break
      
    case 'Spell:HealHeroFull':
      await healHeroFull(gameState, playerKey)
      console.log('💚 Spell: Fully healed hero')
      break
      
    case 'Spell:Drain1':
      await drainSpell(gameState, playerKey, enemyPlayerKey, 1)
      console.log('🩸 Spell: Drained 1 HP')
      break
      
    case 'Spell:DrainHero2':
      await drainHeroSpell(gameState, playerKey, enemyPlayerKey, 2)
      console.log('🩸 Spell: Drained hero for 2')
      break
      
    case 'Spell:DamageAllUnits7':
      await damageAllUnits(gameState, 7)
      console.log('💥 Spell: Damaged ALL units for 7')
      break
      
    case 'Spell:RandomSplitDamage6':
      await randomSplitDamage(gameState, enemyPlayerKey, 6)
      console.log('⚡ Spell: Split 6 damage randomly')
      break
      
    case 'Spell:PoisonAllEnemies':
      await poisonAllEnemies(gameState, enemyPlayerKey)
      console.log('☠️ Spell: Poisoned all enemies')
      break
      
    case 'Spell:FreezeAll(2Turns)':
      await freezeAllEnemiesExtended(gameState, enemyPlayerKey, 2)
      console.log('🧊 Spell: Froze all enemies for 2 turns')
      break
  }
  
  // Check for game over after damage
  if (gameState[enemyHeroKey].hp <= 0) {
    gameState.game_status = 'finished'
    gameState.winner_id = playerKey === 'player1' ? battle.player1_id : battle.player2_id
    gameState.phase = 'game_over'
    console.log(`🏆 ${playerKey} wins via spell damage!`)
  }
}

// ===== HELPER FUNCTIONS =====

async function healRandomAlly(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const units = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (units.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * units.length)
  const unit = units[randomIndex]
  
  const oldHp = unit.current_hp || unit.hp
  unit.current_hp = Math.min(unit.max_hp, unit.current_hp + amount)
  const actualHealing = unit.current_hp - oldHp
  
  console.log(`💚 Healed ${unit.name} for ${actualHealing} (${unit.current_hp}/${unit.max_hp})`)
  
  // Trigger heal-based passive abilities
  if (actualHealing > 0) {
    await processHealTriggers(gameState, playerKey, unit, actualHealing)
  }
}

async function damageRandomEnemy(gameState: any, enemyPlayerKey: string, amount: number) {
  const unitsKey = `${enemyPlayerKey}_units`
  const units = gameState.battlefield[unitsKey] || []
  
  if (units.length === 0) {
    // No units, damage hero instead
    const heroKey = `${enemyPlayerKey}_hero`
    gameState[heroKey].hp -= amount
    console.log(`💥 No enemy units, damaged enemy hero for ${amount}`)
    return
  }
  
  const randomIndex = Math.floor(Math.random() * units.length)
  const unit = units[randomIndex]
  
  unit.current_health -= amount
  console.log(`💥 Damaged ${unit.name} for ${amount} (${unit.current_health}/${unit.max_health})`)
  
  // Remove unit if dead
  if (unit.current_health <= 0) {
    units.splice(randomIndex, 1)
    console.log(`💀 ${unit.name} destroyed`)
  }
}

async function damageAllEnemyUnits(gameState: any, enemyPlayerKey: string, amount: number) {
  const unitsKey = `${enemyPlayerKey}_units`
  const units = gameState.battlefield[unitsKey] || []
  
  for (let i = units.length - 1; i >= 0; i--) {
    const unit = units[i]
    unit.current_health -= amount
    console.log(`💥 Damaged ${unit.name} for ${amount} (${unit.current_health}/${unit.max_health})`)
    
    // Remove unit if dead
    if (unit.current_health <= 0) {
      units.splice(i, 1)
      console.log(`💀 ${unit.name} destroyed`)
    }
  }
}

async function healAllAllies(gameState: any, playerKey: string, amount: number) {
  const unitsKey = `${playerKey}_units`
  const units = gameState.battlefield[unitsKey] || []
  
  for (const unit of units) {
    unit.current_health = Math.min(unit.max_health, unit.current_health + amount)
    console.log(`💚 Healed ${unit.name} for ${amount} (${unit.current_health}/${unit.max_health})`)
  }
}

async function freezeRandomEnemy(gameState: any, enemyPlayerKey: string) {
  const unitsKey = `${enemyPlayerKey}_units`
  const units = gameState.battlefield[unitsKey] || []
  
  if (units.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * units.length)
  const unit = units[randomIndex]
  
  unit.is_frozen = true
  unit.frozen_turns_remaining = 1 // Freeze lasts for 1 of the controller's turns
  console.log(`🧊 Froze ${unit.name} for 1 turn`)
}

async function silenceRandomEnemy(gameState: any, enemyPlayerKey: string) {
  const unitsKey = `${enemyPlayerKey}_units`
  const units = gameState.battlefield[unitsKey] || []
  
  if (units.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * units.length)
  const unit = units[randomIndex]
  
  // Remove all abilities and keywords
  unit.keywords = []
  unit.abilities = []
  unit.has_taunt = false
  unit.has_lifesteal = false
  unit.has_poison = false
  unit.has_poison_touch = false
  unit.is_silenced = true
  
  console.log(`🔇 Silenced ${unit.name}`)
}

async function drawCard(gameState: any, playerKey: string, amount: number) {
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

// ===== SPELL-TRIGGERED PASSIVE SYSTEM =====

async function processSpellCastTriggers(gameState: any, casterPlayerKey: string, spellCard: any) {
  console.log('🪄 Processing spell-cast triggers...')
  
  // Check all units on caster's battlefield for spell-triggered passives
  const playerSlots = gameState.battlefield[`${casterPlayerKey}_slots`] || []
  const playerUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of playerUnits) {
    if (!unit.keywords) continue
    
    const keywords = Array.isArray(unit.keywords) ? unit.keywords : 
      (typeof unit.keywords === 'string' ? JSON.parse(unit.keywords) : [])
    
    for (const keyword of keywords) {
      switch (keyword) {
        case 'Passive:HealAlly2OnSpell':
          await healRandomAllySpellTrigger(gameState, casterPlayerKey, 2)
          console.log(`💚 ${unit.name} passive: Healed ally for 2 (spell trigger)`)
          break
          
        case 'Passive:DrawOnSpellCast':
          await drawCard(gameState, casterPlayerKey, 1)
          console.log(`📚 ${unit.name} passive: Drew 1 card (spell trigger)`)
          break
          
        case 'Passive:SummonHuskOnSpellCast':
          await summonHuskSpellTrigger(gameState, casterPlayerKey, 1, 1)
          console.log(`👻 ${unit.name} passive: Summoned 1/1 Husk (spell trigger)`)
          break
          
        case 'Trigger:OnSpellCastSummonHusk1_1':
          await summonHuskSpellTrigger(gameState, casterPlayerKey, 1, 1)
          console.log(`👻 ${unit.name} trigger: Summoned 1/1 Husk (spell trigger)`)
          break
      }
    }
  }
}

async function healRandomAllySpellTrigger(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * allyUnits.length)
  const unit = allyUnits[randomIndex]
  
  unit.current_hp = Math.min(unit.max_hp, unit.current_hp + amount)
  console.log(`💚 Spell trigger healed ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
}

async function summonHuskSpellTrigger(gameState: any, playerKey: string, attack: number, hp: number) {
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
    console.log('❌ No empty slots for spell-triggered Husk')
    return
  }
  
  // Create Husk token
  const huskToken = {
    id: `husk_spell_${Date.now()}_${Math.random()}`,
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
  console.log(`👻 Spell trigger summoned ${attack}/${hp} Husk in slot ${emptySlotIndex}`)
}

// ===== HEAL-TRIGGERED PASSIVE SYSTEM =====

async function processHealTriggers(gameState: any, playerKey: string, healedUnit: any, healAmount: number) {
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
          await damageRandomEnemyHealTrigger(gameState, playerKey === 'player1' ? 'player2' : 'player1', 1)
          console.log(`💥 ${unit.name} passive: Damaged random enemy for 1 (heal trigger)`)
          break
      }
    }
  }
}

async function damageRandomEnemyHealTrigger(gameState: any, enemyPlayerKey: string, amount: number) {
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

// ===== CONDITIONAL SPELL ABILITIES =====

async function destroyEnemyRelic(gameState: any, enemyPlayerKey: string) {
  // Check if enemy has an active relic
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length > 0) {
    // Destroy the first active relic
    const relic = enemyRelics[0]
    enemyRelics.splice(0, 1)
    console.log(`💥 Destroyed enemy relic: ${relic.name}`)
    return true // Relic was destroyed
  }
  
  console.log('❌ No enemy relic to destroy')
  return false // No relic to destroy
}

async function conditionalDrawCard(gameState: any, playerKey: string, enemyPlayerKey: string) {
  // Check if enemy has a relic
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length === 0) {
    // No enemy relic - draw a card
    await drawCard(gameState, playerKey, 1)
    console.log('🃏 Conditional: Drew card (no enemy relic)')
  } else {
    console.log('❌ Conditional: No card draw (enemy has relic)')
  }
}

async function conditionalDisableRelic(gameState: any, enemyPlayerKey: string) {
  // Disable enemy relic effects for 1 turn
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length > 0) {
    const relic = enemyRelics[0]
    relic.disabled_until_turn = (gameState.turn_number || 1) + 1
    console.log(`🔒 Disabled enemy relic ${relic.name} until turn ${relic.disabled_until_turn}`)
  }
}

async function freezeTargetUnit(gameState: any, targetId: string) {
  // Find target unit by ID
  for (const playerKey of ['player1', 'player2']) {
    const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
    
    for (const unit of playerSlots) {
      if (unit && unit.id === targetId) {
        unit.is_frozen = true
        unit.frozen_turns_remaining = 1
        console.log(`🧊 Froze target unit: ${unit.name}`)
        return
      }
    }
  }
  
  console.log('❌ Target unit not found for freeze')
}

async function processChoiceEffect(gameState: any, playerKey: string, targetId?: string) {
  // Placeholder for complex choice-based spells
  // Each card would have its own specific implementation
  console.log('🔀 Processing choice effect (placeholder)')
  
  // Example: Choose one of multiple effects
  // - Deal 3 damage to target
  // - Heal 3 to ally
  // - Draw 2 cards
  // Implementation would depend on client-side choice selection
}

// ===== MISSING SPELL HELPER FUNCTIONS =====

async function buffAllAlliesAttack(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  for (const unit of allyUnits) {
    unit.attack += amount
    console.log(`💪 Buffed ${unit.name} ATK +${amount} (${unit.attack}/${unit.current_hp})`)
  }
}

async function damageAllAllies(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  for (let i = playerSlots.length - 1; i >= 0; i--) {
    const unit = playerSlots[i]
    if (unit && unit.card_type === 'unit') {
      unit.current_hp -= amount
      console.log(`💥 Damaged ally ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
      
      if (unit.current_hp <= 0) {
        playerSlots[i] = null
        console.log(`💀 ${unit.name} destroyed by friendly spell`)
      }
    }
  }
}

async function damageVoidUnitsOnly(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (let i = enemySlots.length - 1; i >= 0; i--) {
    const unit = enemySlots[i]
    if (unit && unit.card_type === 'unit' && unit.potato_type === 'void') {
      unit.current_hp -= amount
      console.log(`💥 Damaged Void unit ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
      
      if (unit.current_hp <= 0) {
        enemySlots[i] = null
        console.log(`💀 ${unit.name} destroyed by anti-Void spell`)
      }
    }
  }
}

async function damageIfFrozen(gameState: any, enemyPlayerKey: string, amount: number, targetId?: string) {
  if (targetId) {
    // Target specific unit
    const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
    for (const unit of enemySlots) {
      if (unit && unit.id === targetId && unit.is_frozen) {
        unit.current_hp -= amount
        console.log(`🧊💥 Damaged frozen ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
        
        if (unit.current_hp <= 0) {
          const slotIndex = enemySlots.indexOf(unit)
          enemySlots[slotIndex] = null
          console.log(`💀 ${unit.name} destroyed by freeze damage`)
        }
        return
      }
    }
  } else {
    // Damage all frozen enemies
    const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
    for (let i = enemySlots.length - 1; i >= 0; i--) {
      const unit = enemySlots[i]
      if (unit && unit.card_type === 'unit' && unit.is_frozen) {
        unit.current_hp -= amount
        console.log(`🧊💥 Damaged frozen ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
        
        if (unit.current_hp <= 0) {
          enemySlots[i] = null
          console.log(`💀 ${unit.name} destroyed by freeze damage`)
        }
      }
    }
  }
}

async function damageTargetUnit(gameState: any, targetId: string, amount: number) {
  if (!targetId) {
    console.log('❌ No target specified for unit damage')
    return
  }
  
  for (const playerKey of ['player1', 'player2']) {
    const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
    
    for (let i = 0; i < playerSlots.length; i++) {
      const unit = playerSlots[i]
      if (unit && unit.id === targetId) {
        unit.current_hp -= amount
        console.log(`💥 Damaged target ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
        
        if (unit.current_hp <= 0) {
          playerSlots[i] = null
          console.log(`💀 ${unit.name} destroyed by targeted spell`)
        }
        return
      }
    }
  }
  
  console.log('❌ Target unit not found')
}

async function chainDamageTwice(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) return
  
  // Damage first random enemy
  let randomIndex = Math.floor(Math.random() * enemyUnits.length)
  let unit = enemyUnits[randomIndex]
  unit.current_hp -= amount
  console.log(`⚡ Chain 1: Damaged ${unit.name} for ${amount}`)
  
  if (unit.current_hp <= 0) {
    const slotIndex = enemySlots.indexOf(unit)
    enemySlots[slotIndex] = null
    console.log(`💀 ${unit.name} destroyed by chain 1`)
  }
  
  // Damage second random enemy (if any left)
  const remainingEnemies = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  if (remainingEnemies.length > 0) {
    randomIndex = Math.floor(Math.random() * remainingEnemies.length)
    unit = remainingEnemies[randomIndex]
    unit.current_hp -= amount
    console.log(`⚡ Chain 2: Damaged ${unit.name} for ${amount}`)
    
    if (unit.current_hp <= 0) {
      const slotIndex = enemySlots.indexOf(unit)
      enemySlots[slotIndex] = null
      console.log(`💀 ${unit.name} destroyed by chain 2`)
    }
  }
}

async function chainKillDamage(gameState: any, enemyPlayerKey: string, amount: number) {
  let canChain = true
  let chainCount = 0
  
  while (canChain && chainCount < 10) { // Prevent infinite loops
    const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
    const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
    
    if (enemyUnits.length === 0) {
      // No units - damage hero
      const heroKey = `${enemyPlayerKey}_hero`
      gameState[heroKey].hp -= amount
      console.log(`⚡ Chain ${chainCount + 1}: Damaged enemy hero for ${amount}`)
      break
    }
    
    // Find a unit that will die from this damage
    let targetUnit = null
    let targetSlotIndex = -1
    
    for (let i = 0; i < enemySlots.length; i++) {
      const unit = enemySlots[i]
      if (unit && unit.card_type === 'unit' && unit.current_hp <= amount) {
        targetUnit = unit
        targetSlotIndex = i
        break
      }
    }
    
    if (!targetUnit) {
      // No unit will die - pick random and stop chain
      const randomIndex = Math.floor(Math.random() * enemyUnits.length)
      targetUnit = enemyUnits[randomIndex]
      targetSlotIndex = enemySlots.indexOf(targetUnit)
      canChain = false
    }
    
    targetUnit.current_hp -= amount
    console.log(`⚡ Chain ${chainCount + 1}: Damaged ${targetUnit.name} for ${amount}`)
    
    if (targetUnit.current_hp <= 0) {
      enemySlots[targetSlotIndex] = null
      console.log(`💀 ${targetUnit.name} destroyed - chain continues`)
      chainCount++
    } else {
      canChain = false
    }
  }
}

async function summonMultipleHusks(gameState: any, playerKey: string, count: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  let summoned = 0
  for (let i = 0; i < playerSlots.length && summoned < count; i++) {
    if (!playerSlots[i]) {
      const huskToken = {
        id: `husk_spell_${Date.now()}_${Math.random()}`,
        name: 'Husk',
        card_type: 'unit',
        potato_type: 'void',
        attack: 1,
        hp: 1,
        current_hp: 1,
        max_hp: 1,
        deployed_turn: gameState.turn_number,
        summoning_sickness: true,
        has_attacked_this_turn: false,
        owner_id: playerKey,
        is_token: true
      }
      
      playerSlots[i] = huskToken
      summoned++
      console.log(`👻 Summoned Husk ${summoned} in slot ${i}`)
    }
  }
  
  if (summoned < count) {
    console.log(`❌ Only summoned ${summoned}/${count} Husks (board full)`)
  }
}

async function destroyAlly(gameState: any, playerKey: string) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * allyUnits.length)
  const unit = allyUnits[randomIndex]
  const slotIndex = playerSlots.indexOf(unit)
  
  playerSlots[slotIndex] = null
  console.log(`💀 Destroyed ally: ${unit.name}`)
}

async function sacrificeLowestAlly(gameState: any, playerKey: string) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  // Find lowest HP ally
  let lowestUnit = allyUnits[0]
  for (const unit of allyUnits) {
    if (unit.current_hp < lowestUnit.current_hp) {
      lowestUnit = unit
    }
  }
  
  const slotIndex = playerSlots.indexOf(lowestUnit)
  playerSlots[slotIndex] = null
  console.log(`💀 Sacrificed lowest ally: ${lowestUnit.name} (${lowestUnit.current_hp} HP)`)
}

async function healHeroFull(gameState: any, playerKey: string) {
  const heroKey = `${playerKey}_hero`
  const hero = gameState[heroKey]
  hero.hp = hero.max_hp
  console.log(`💚 Fully healed hero (${hero.hp}/${hero.max_hp})`)
}

async function drainSpell(gameState: any, playerKey: string, enemyPlayerKey: string, amount: number) {
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  const playerHeroKey = `${playerKey}_hero`
  
  gameState[enemyHeroKey].hp -= amount
  gameState[playerHeroKey].hp = Math.min(gameState[playerHeroKey].max_hp, gameState[playerHeroKey].hp + amount)
  
  console.log(`🩸 Drained ${amount} from enemy hero, healed own hero`)
}

async function drainHeroSpell(gameState: any, playerKey: string, enemyPlayerKey: string, amount: number) {
  const enemyHeroKey = `${enemyPlayerKey}_hero`
  const playerHeroKey = `${playerKey}_hero`
  
  gameState[enemyHeroKey].hp -= amount
  gameState[playerHeroKey].hp = Math.min(gameState[playerHeroKey].max_hp, gameState[playerHeroKey].hp + amount)
  
  console.log(`🩸 Drained ${amount} from enemy hero to own hero`)
}

async function damageAllUnits(gameState: any, amount: number) {
  // Damage ALL units on BOTH sides
  for (const playerKey of ['player1', 'player2']) {
    const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
    
    for (let i = playerSlots.length - 1; i >= 0; i--) {
      const unit = playerSlots[i]
      if (unit && unit.card_type === 'unit') {
        unit.current_hp -= amount
        console.log(`💥 Apocalypse damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
        
        if (unit.current_hp <= 0) {
          playerSlots[i] = null
          console.log(`💀 ${unit.name} destroyed by Apocalypse`)
        }
      }
    }
  }
}

async function randomSplitDamage(gameState: any, enemyPlayerKey: string, totalDamage: number) {
  let remainingDamage = totalDamage
  
  while (remainingDamage > 0) {
    const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
    const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
    
    if (enemyUnits.length === 0) {
      // No units - all remaining damage to hero
      const heroKey = `${enemyPlayerKey}_hero`
      gameState[heroKey].hp -= remainingDamage
      console.log(`⚡ Split damage: ${remainingDamage} to enemy hero`)
      break
    }
    
    const randomIndex = Math.floor(Math.random() * enemyUnits.length)
    const unit = enemyUnits[randomIndex]
    
    const damageToUnit = Math.min(1, remainingDamage) // 1 damage per hit
    unit.current_hp -= damageToUnit
    remainingDamage -= damageToUnit
    
    console.log(`⚡ Split damage: ${damageToUnit} to ${unit.name} (${unit.current_hp}/${unit.max_hp})`)
    
    if (unit.current_hp <= 0) {
      const slotIndex = enemySlots.indexOf(unit)
      enemySlots[slotIndex] = null
      console.log(`💀 ${unit.name} destroyed by split damage`)
    }
  }
}

async function poisonAllEnemies(gameState: any, enemyPlayerKey: string) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (let i = enemySlots.length - 1; i >= 0; i--) {
    const unit = enemySlots[i]
    if (unit && unit.card_type === 'unit') {
      unit.current_hp = 0 // Poison kills instantly
      enemySlots[i] = null
      console.log(`☠️ Poisoned and destroyed ${unit.name}`)
    }
  }
}

async function freezeAllEnemiesExtended(gameState: any, enemyPlayerKey: string, turns: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (const unit of enemySlots) {
    if (unit && unit.card_type === 'unit') {
      unit.is_frozen = true
      unit.frozen_turns_remaining = turns
      console.log(`🧊 Froze ${unit.name} for ${turns} turns`)
    }
  }
}