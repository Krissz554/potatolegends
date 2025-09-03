import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

interface UnitAttackRequest {
  battleSessionId: string
  playerId: string
  attackerSlot: number
  targetSlot?: number // Optional for hero attacks
  targetType?: 'unit' | 'hero' // Specify what we're attacking
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

    const { battleSessionId, playerId, attackerSlot, targetSlot, targetType }: UnitAttackRequest = await req.json()

    console.log('⚔️ Processing attack:', {
      battleSessionId,
      playerId,
      attackerSlot,
      targetSlot,
      targetType: targetType || 'unit'
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

    const gameState = battleSession.game_state

    // Validate turn and phase
    if (gameState.current_turn_player_id !== playerId) {
      throw new Error(`Not your turn. Current turn: ${gameState.current_turn_player_id}`)
    }

    if (gameState.turn_phase !== 'main') {
      throw new Error(`Can only attack during main phase. Current phase: ${gameState.turn_phase}`)
    }

    // Get battlefield data
    const isPlayer1 = battleSession.player1_id === playerId
    const playerSlots = isPlayer1 ? gameState.battlefield.player1_slots : gameState.battlefield.player2_slots
    const opponentSlots = isPlayer1 ? gameState.battlefield.player2_slots : gameState.battlefield.player1_slots

    // Validate attacker
    const attacker = playerSlots[attackerSlot]
    if (!attacker) {
      throw new Error(`No unit in attacker slot ${attackerSlot}`)
    }

    if (!attacker.attack || attacker.attack === 0) {
      throw new Error(`Unit ${attacker.name} cannot attack (0 attack)`)
    }

    // Check summoning sickness (can't attack same turn as deployment, unless has Charge)
    if (attacker.deployed_turn === gameState.turn_number && !attacker.has_charge && attacker.summoning_sickness !== false) {
      throw new Error(`${attacker.name} cannot attack this turn (summoning sickness)`)
    }

    // Check if attacker has already attacked this turn (with Windfury exception)
    const hasWindfury = attacker.keywords && (
      Array.isArray(attacker.keywords) ? attacker.keywords : 
      (typeof attacker.keywords === 'string' ? JSON.parse(attacker.keywords) : [])
    ).includes('Windfury')
    
    const attacksThisTurn = attacker.attacks_this_turn || 0
    const maxAttacks = hasWindfury ? 2 : 1
    
    if (attacksThisTurn >= maxAttacks) {
      const windMessage = hasWindfury ? ' (Windfury: 2/2 attacks used)' : ''
      throw new Error(`${attacker.name} has already attacked this turn${windMessage}`)
    }
    
    // Check if attacker is frozen
    if (attacker.is_frozen) {
      throw new Error(`${attacker.name} is frozen and cannot attack`)
    }

    // Handle hero attacks
    if (targetType === 'hero') {
      // Check for Rush restriction - Rush units can't attack hero on deployment turn
      const hasRush = attacker.keywords && (
        Array.isArray(attacker.keywords) ? attacker.keywords : 
        (typeof attacker.keywords === 'string' ? JSON.parse(attacker.keywords) : [])
      ).includes('Rush')
      
      if (hasRush && attacker.deployed_turn === gameState.turn_number) {
        throw new Error(`${attacker.name} has Rush - cannot attack hero on deployment turn`)
      }
      
      // Check for Taunt units - cannot attack hero if enemy has Taunt units
      const tauntUnits = opponentSlots.filter(unit => unit && unit.has_taunt)
      if (tauntUnits.length > 0) {
        throw new Error(`Cannot attack hero while enemy has Taunt units: ${tauntUnits.map(u => u.name).join(', ')}`)
      }
      
      const opponentHeroKey = isPlayer1 ? 'player2_hero' : 'player1_hero'
      const playerHeroKey = isPlayer1 ? 'player1_hero' : 'player2_hero'
      const enemyHero = gameState[opponentHeroKey]
      
      if (!enemyHero) {
        throw new Error('Enemy hero not found')
      }

      console.log('🎯 Hero attack:', {
        attacker: `${attacker.name} (${attacker.attack} attack)`,
        target: `Enemy Hero (${enemyHero.hp} HP)`
      })

      // Apply damage to enemy hero (no counter-attack) with aura bonuses
      const attackPower = calculateEffectiveAttack(attacker, gameState, isPlayer1 ? 'player1' : 'player2')
      const actualDamageDealt = Math.min(attackPower, enemyHero.hp) // Actual damage = min(attack, current HP)
      const newHeroHp = Math.max(0, enemyHero.hp - attackPower)
      
      // Update hero HP and mark attacker as having attacked
      gameState[opponentHeroKey].hp = newHeroHp
      
      // Remove Stealth when attacking (stealth breaks)
      if (attacker.has_stealth) {
        attacker.has_stealth = false
        attacker.can_be_targeted = true
        console.log(`👤 ${attacker.name} loses Stealth after attacking`)
      }
      
      attacker.attacks_this_turn = (attacker.attacks_this_turn || 0) + 1
      
      // Mark as attacked if reached max attacks
      if (attacker.attacks_this_turn >= maxAttacks) {
        attacker.has_attacked_this_turn = true
      }
      
      // Process Lifesteal - heal for actual damage dealt, not full attack
      if (attacker.has_lifesteal && actualDamageDealt > 0) {
        const playerHero = gameState[playerHeroKey]
        const healAmount = actualDamageDealt
        playerHero.hp = Math.min(playerHero.max_hp, playerHero.hp + healAmount)
        console.log(`🩸 Lifesteal: Healed ${attacker.name}'s hero for ${healAmount} (actual damage dealt) (${playerHero.hp}/${playerHero.max_hp})`)
      }

      // Check for victory condition
      let gameEnded = false
      if (newHeroHp === 0) {
        gameState.status = 'completed'
        gameState.winner_id = playerId
        gameState.end_reason = 'hero_defeated'
        gameEnded = true
        console.log(`🏆 Game won! Enemy Hero defeated`)
      }

      // Update game state
      const newGameState = {
        ...gameState,
        battlefield: {
          ...gameState.battlefield,
          player1_slots: isPlayer1 ? playerSlots : opponentSlots,
          player2_slots: isPlayer1 ? opponentSlots : playerSlots
        }
      }

      // Save to database
      const { error: updateError } = await supabaseClient
        .from('battle_sessions')
        .update({
          game_state: newGameState,
          status: gameEnded ? 'completed' : battleSession.status,
          updated_at: new Date().toISOString()
        })
        .eq('id', battleSessionId)

      if (updateError) {
        throw new Error(`Failed to update battle: ${updateError.message}`)
      }

      // Log battle action
      await supabaseClient
        .from('battle_actions')
        .insert({
          battle_session_id: battleSessionId,
          player_id: playerId,
          action_type: 'hero_attack',
          action_data: {
            attackerSlot,
            attackerName: attacker.name,
            damage,
            heroNewHp: newHeroHp,
            gameEnded
          }
        })

      const heroAttackResult = {
        attackerDamage: damage,
        targetDamage: 0, // No counter-attack from hero
        heroNewHp: newHeroHp,
        heroDefeated: newHeroHp === 0,
        gameEnded,
        targetType: 'hero'
      }

      console.log('✅ Hero attack resolved:', heroAttackResult)

      return new Response(
        JSON.stringify({
          success: true,
          combat: heroAttackResult
        }),
        {
          headers: {
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': '*',
          },
        }
      )
    }

    // Check for Taunt units - can only attack non-Taunt units if no Taunt units exist
    const tauntUnits = opponentSlots.filter(unit => unit && unit.has_taunt)
    
    // Validate unit target
    const target = opponentSlots[targetSlot]
    if (!target) {
      throw new Error(`No unit in target slot ${targetSlot}`)
    }
    
    // Check for Stealth - cannot be targeted directly (except by AoE effects)
    if (target.has_stealth) {
      throw new Error(`Cannot target ${target.name} - unit has Stealth!`)
    }
    
    // Taunt validation - if enemy has Taunt units, must attack them first
    if (tauntUnits.length > 0 && !target.has_taunt) {
      throw new Error(`Cannot attack ${target.name}! Must attack Taunt units first: ${tauntUnits.map(u => u.name).join(', ')}`)
    }

    // Perform simultaneous damage exchange
    console.log('⚔️ Combat:', {
      attacker: `${attacker.name} (${attacker.attack}/${attacker.current_hp || attacker.hp})`,
      target: `${target.name} (${target.attack}/${target.current_hp || target.hp})`
    })

    // Calculate damage with aura bonuses
    const attackerDamage = calculateEffectiveAttack(attacker, gameState, isPlayer1 ? 'player1' : 'player2')
    const targetDamage = calculateEffectiveAttack(target, gameState, isPlayer1 ? 'player2' : 'player1')

    // Step 1: Apply attacker's damage to target (with damage reduction, Divine Shield, and Immune)
    const targetCurrentHp = target.current_hp || target.hp
    let reducedDamage = calculateEffectiveDamage(attackerDamage, target, gameState, isPlayer1 ? 'player2' : 'player1')
    
    // Process Immune - takes no damage
    if (target.has_immune) {
      console.log(`✨ Immune: ${target.name} takes no damage!`)
      reducedDamage = 0
    } else {
      // Process Divine Shield only if not immune
      reducedDamage = processDivineShieldDamage(target, reducedDamage)
    }
    
    const actualDamageDealt = Math.min(reducedDamage, targetCurrentHp) // Actual damage = min(reduced_damage, current HP)
    const newTargetHp = Math.max(0, targetCurrentHp - reducedDamage)
    target.current_hp = newTargetHp
    
    // Process combat reaction effects from target's structures
    await processCombatReactions(gameState, isPlayer1 ? 'player2' : 'player1', attacker, 'attacked')
    
    // Process attacker's OnAttack triggers
    await processOnAttackTriggers(gameState, attacker, target, isPlayer1 ? 'player1' : 'player2')
    
    // Process target's OnDamage triggers (if damage was dealt)
    if (actualDamageDealt > 0) {
      await processOnDamageTriggers(gameState, target, attacker, isPlayer1 ? 'player2' : 'player1', actualDamageDealt)
    }
    
    // Process attacker's AfterAttack triggers (after combat resolves)
    await processAfterAttackTriggers(gameState, attacker, target, isPlayer1 ? 'player1' : 'player2')
    
    // Process attacker abilities on damage dealt
    const playerHeroKey = isPlayer1 ? 'player1_hero' : 'player2_hero'
    
    // Lifesteal: Heal attacker's hero for actual damage dealt, not full attack
    if (attacker.has_lifesteal && actualDamageDealt > 0) {
      const playerHero = gameState[playerHeroKey]
      const healAmount = actualDamageDealt
      playerHero.hp = Math.min(playerHero.max_hp, playerHero.hp + healAmount)
      console.log(`🩸 Lifesteal: Healed ${attacker.name}'s hero for ${healAmount} (actual damage: ${actualDamageDealt}) (${playerHero.hp}/${playerHero.max_hp})`)
    }
    
    // Poison: Target dies if damaged by poison unit
    if ((attacker.has_poison || attacker.has_poison_touch) && attackerDamage > 0 && newTargetHp > 0) {
      target.current_hp = 0
      console.log(`☠️ Poison: ${target.name} destroyed by poison`)
    }
    
    // Step 2: Check if target dies - if so, no counter-attack
    let newAttackerHp = attacker.current_hp || attacker.hp
    let targetCounterAttacked = false
    
    if (target.current_hp > 0) {
      // Target is still alive, counter-attack happens (with damage reduction and Divine Shield)
      const attackerCurrentHp = newAttackerHp
      let reducedCounterDamage = calculateEffectiveDamage(targetDamage, attacker, gameState, isPlayer1 ? 'player1' : 'player2')
      
      // Process Divine Shield on counter-attack
      reducedCounterDamage = processDivineShieldDamage(attacker, reducedCounterDamage)
      
      const actualCounterDamage = Math.min(reducedCounterDamage, attackerCurrentHp) // Actual counter damage
      newAttackerHp = Math.max(0, newAttackerHp - reducedCounterDamage)
      targetCounterAttacked = true
      console.log(`🔄 ${target.name} counter-attacks for ${targetDamage} damage`)
      
      // Process target abilities on damage dealt (counter-attack)
      if (target.has_lifesteal && actualCounterDamage > 0) {
        const enemyHeroKey = isPlayer1 ? 'player2_hero' : 'player1_hero'
        const enemyHero = gameState[enemyHeroKey]
        const healAmount = actualCounterDamage
        enemyHero.hp = Math.min(enemyHero.max_hp, enemyHero.hp + healAmount)
        console.log(`🩸 Counter Lifesteal: Healed ${target.name}'s hero for ${healAmount} (actual damage: ${actualCounterDamage}) (${enemyHero.hp}/${enemyHero.max_hp})`)
      }
      
      // Poison on counter-attack
      if ((target.has_poison || target.has_poison_touch) && targetDamage > 0 && newAttackerHp > 0) {
        newAttackerHp = 0
        console.log(`☠️ Counter Poison: ${attacker.name} destroyed by poison`)
      }
    } else {
      console.log(`💀 ${target.name} destroyed before counter-attack`)
    }

    // Update attacker
    attacker.current_hp = newAttackerHp
    
    // Check for Double Strike - if attacker survives and has double strike, attack again
    const hasDoubleStrike = attacker.keywords && (
      Array.isArray(attacker.keywords) ? attacker.keywords : 
      (typeof attacker.keywords === 'string' ? JSON.parse(attacker.keywords) : [])
    ).includes('DoubleStrike')
    
    if (hasDoubleStrike && newAttackerHp > 0 && !attacker.has_double_struck) {
      console.log(`⚔️⚔️ Double Strike: ${attacker.name} attacks again!`)
      attacker.has_double_struck = true // Prevent infinite loops
      
      // Second attack - only if target still exists and alive
      if (opponentSlots[targetSlot] && opponentSlots[targetSlot].current_hp > 0) {
        await performDoubleStrikeAttack(gameState, attacker, opponentSlots[targetSlot], isPlayer1, targetSlot)
      }
    }
    
    // Remove Stealth when attacking (stealth breaks)
    if (attacker.has_stealth) {
      attacker.has_stealth = false
      attacker.can_be_targeted = true
      console.log(`👤 ${attacker.name} loses Stealth after attacking`)
    }
    
    // Update attack counter
    attacker.attacks_this_turn = (attacker.attacks_this_turn || 0) + 1
    
    // Mark as attacked if reached max attacks
    if (attacker.attacks_this_turn >= maxAttacks) {
      attacker.has_attacked_this_turn = true
    }

    // Remove dead units and process deathrattles
    if (newTargetHp === 0) {
      await processDeathrattle(gameState, target, isPlayer1 ? 'player2' : 'player1')
      opponentSlots[targetSlot] = null
      console.log(`💀 ${target.name} removed from battlefield`)
    }

    if (newAttackerHp === 0) {
      await processDeathrattle(gameState, attacker, isPlayer1 ? 'player1' : 'player2')
      playerSlots[attackerSlot] = null
      console.log(`💀 ${attacker.name} destroyed by counter-attack`)
    }

    // Update game state
    const newGameState = {
      ...gameState,
      battlefield: {
        ...gameState.battlefield,
        player1_slots: isPlayer1 ? playerSlots : opponentSlots,
        player2_slots: isPlayer1 ? opponentSlots : playerSlots
      }
    }

    // Save to database
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: newGameState,
        updated_at: new Date().toISOString()
      })
      .eq('id', battleSessionId)

    if (updateError) {
      throw new Error(`Failed to update battle: ${updateError.message}`)
    }

    // Prepare combat result data for Ably and response
    const combatResult = {
      attackerDamage,
      targetDamage: targetCounterAttacked ? targetDamage : 0,
      attackerNewHp: newAttackerHp,
      targetNewHp: newTargetHp,
      attackerDestroyed: newAttackerHp === 0,
      targetDestroyed: newTargetHp === 0,
      attackerName: attacker.name,
      targetName: targetType === 'hero' ? 'Enemy Hero' : target.name,
      targetCounterAttacked
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
          name: 'combat_result',
          data: {
            type: targetType === 'hero' ? 'hero_attack' : 'unit_attack',
            combat: combatResult,
            battleSessionId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published combat result to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
      // Don't fail the entire operation if Ably fails
    }

    // Log battle action
    await supabaseClient
      .from('battle_actions')
      .insert({
        battle_session_id: battleSessionId,
        player_id: playerId,
        action_type: 'unit_attack',
        action_data: {
          attackerSlot,
          targetSlot,
          attackerDamage,
          targetDamage: targetCounterAttacked ? targetDamage : 0,
          attackerName: attacker.name,
          targetName: target.name,
          attackerNewHp: newAttackerHp,
          targetNewHp: newTargetHp,
          targetCounterAttacked
        }
      })

    console.log('✅ Combat resolved:', combatResult)

    return new Response(
      JSON.stringify({
        success: true,
        combat: combatResult
      }),
      {
        headers: {
          'Content-Type': 'application/json',
          'Access-Control-Allow-Origin': '*',
        },
      }
    )

  } catch (error) {
    console.error('❌ Unit attack error:', error)
    return new Response(
      JSON.stringify({
        error: error.message,
        success: false
      }),
      {
        status: 400,
        headers: {
          'Content-Type': 'application/json',
          'Access-Control-Allow-Origin': '*',
        },
      }
    )
  }
})

// ===== AURA SYSTEM =====

function calculateEffectiveAttack(unit: any, gameState: any, playerKey: string): number {
  if (!unit || !unit.attack) return 0
  
  let baseAttack = unit.attack
  let auraBonus = 0
  
  // Get player's structures and relics for aura calculations
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const auraProviders = playerSlots.filter((card: any) => 
    card && (card.card_type === 'structure' || card.card_type === 'relic')
  )
  
  // Calculate aura bonuses
  auraProviders.forEach((provider: any) => {
    if (!provider.keywords) return
    
    const keywords = Array.isArray(provider.keywords) ? provider.keywords : 
      (typeof provider.keywords === 'string' ? JSON.parse(provider.keywords) : [])
    
    keywords.forEach((keyword: string) => {
      switch (keyword) {
        case 'Structure:BuffTribeFireAtk+1':
          if (unit.potato_type === 'fire') {
            auraBonus += 1
            console.log(`🔥 ${unit.name} gets +1 ATK from Fire Totem`)
          }
          break
        case 'Structure:BuffTribeLightningAtk+1':
          if (unit.potato_type === 'lightning') {
            auraBonus += 1
            console.log(`⚡ ${unit.name} gets +1 ATK from Lightning Totem`)
          }
          break
        case 'Structure:BuffHealersAtk+1':
          if (unit.unit_class === 'healer') {
            auraBonus += 1
            console.log(`💚 ${unit.name} gets +1 ATK from Healer Totem`)
          }
          break
        case 'Structure:BuffLightningUnitsAtk+1':
          if (unit.potato_type === 'lightning') {
            auraBonus += 1
            console.log(`⚡ ${unit.name} gets +1 ATK from Lightning Totem`)
          }
          break
      }
    })
  })
  
  const effectiveAttack = baseAttack + auraBonus
  
  if (auraBonus > 0) {
    console.log(`💪 ${unit.name} effective attack: ${baseAttack} + ${auraBonus} = ${effectiveAttack}`)
  }
  
  return effectiveAttack
}

function calculateEffectiveDamage(damage: number, target: any, gameState: any, playerKey: string): number {
  if (!target || damage <= 0) return damage
  
  let damageReduction = 0
  
  // Get player's structures and relics for damage reduction
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const auraProviders = playerSlots.filter((card: any) => 
    card && (card.card_type === 'structure' || card.card_type === 'relic')
  )
  
  // Calculate damage reduction
  auraProviders.forEach((provider: any) => {
    if (!provider.keywords) return
    
    const keywords = Array.isArray(provider.keywords) ? provider.keywords : 
      (typeof provider.keywords === 'string' ? JSON.parse(provider.keywords) : [])
    
    keywords.forEach((keyword: string) => {
      switch (keyword) {
        case 'Structure:DamageReductionAllies-1':
          damageReduction += 1
          console.log(`🛡️ ${target.name} gets -1 damage from Light Totem`)
          break
        case 'Structure:DamageReductionHero1':
          // This is for hero damage reduction, not unit damage
          break
      }
    })
  })
  
  const effectiveDamage = Math.max(1, damage - damageReduction) // Minimum 1 damage
  
  if (damageReduction > 0) {
    console.log(`🛡️ ${target.name} damage reduced: ${damage} - ${damageReduction} = ${effectiveDamage}`)
  }
  
  return effectiveDamage
}

// ===== COMBAT REACTION SYSTEM =====

async function processCombatReactions(gameState: any, defenderPlayerKey: string, attacker: any, triggerType: string) {
  const defenderSlots = gameState.battlefield[`${defenderPlayerKey}_slots`] || []
  const structures = defenderSlots.filter((card: any) => card && card.card_type === 'structure')
  
  for (const structure of structures) {
    if (!structure.keywords) continue
    
    const keywords = Array.isArray(structure.keywords) ? structure.keywords : 
      (typeof structure.keywords === 'string' ? JSON.parse(structure.keywords) : [])
    
    for (const keyword of keywords) {
      switch (keyword) {
        case 'Structure:FreezeAttacker':
          if (triggerType === 'attacked') {
            attacker.is_frozen = true
            attacker.frozen_turns_remaining = 1
            console.log(`🧊 ${structure.name} froze attacker ${attacker.name}`)
          }
          break
          
        case 'Structure:ZapAttacker1':
          if (triggerType === 'attacked') {
            const attackerCurrentHp = attacker.current_hp || attacker.hp
            attacker.current_hp = Math.max(0, attackerCurrentHp - 1)
            console.log(`⚡ ${structure.name} zapped attacker ${attacker.name} for 1 damage`)
          }
          break
          
        case 'Structure:Thorns1':
          if (triggerType === 'attacked') {
            const attackerCurrentHp = attacker.current_hp || attacker.hp
            attacker.current_hp = Math.max(0, attackerCurrentHp - 1)
            console.log(`🌹 ${structure.name} thorns damaged attacker ${attacker.name} for 1`)
          }
          break
          
        case 'Structure:ThornsDamage2':
          if (triggerType === 'attacked') {
            const attackerCurrentHp = attacker.current_hp || attacker.hp
            attacker.current_hp = Math.max(0, attackerCurrentHp - 2)
            console.log(`🌹 ${structure.name} thorns damaged attacker ${attacker.name} for 2`)
          }
          break
      }
    }
  }
}

// ===== DEATHRATTLE SYSTEM =====

async function processDeathrattle(gameState: any, dyingUnit: any, playerKey: string) {
  if (!dyingUnit || !dyingUnit.keywords) return
  
  const keywords = Array.isArray(dyingUnit.keywords) ? dyingUnit.keywords : 
    (typeof dyingUnit.keywords === 'string' ? JSON.parse(dyingUnit.keywords) : [])
  
  console.log(`💀 Processing deathrattle for ${dyingUnit.name}:`, keywords)
  
  for (const keyword of keywords) {
    switch (keyword) {
      case 'Deathrattle:SummonHusk1':
        await summonHuskDeathrattle(gameState, playerKey, 1, 1)
        console.log(`👻 ${dyingUnit.name} deathrattle: Summoned 1/1 Husk`)
        break
        
      case 'Deathrattle:Summon2Husks':
        await summonHuskDeathrattle(gameState, playerKey, 1, 1)
        await summonHuskDeathrattle(gameState, playerKey, 1, 1)
        console.log(`👻 ${dyingUnit.name} deathrattle: Summoned two 1/1 Husks`)
        break
        
      case 'Deathrattle:AoE1AllEnemies':
        const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
        await damageAllEnemyUnitsDeathrattle(gameState, enemyPlayerKey, 1)
        console.log(`💥 ${dyingUnit.name} deathrattle: Damaged all enemies for 1`)
        break
        
      case 'Deathrattle:DrainHero2':
        const enemyPlayerKey2 = playerKey === 'player1' ? 'player2' : 'player1'
        const enemyHeroKey = `${enemyPlayerKey2}_hero`
        const enemyHero = gameState[enemyHeroKey]
        enemyHero.hp -= 2
        console.log(`💀 ${dyingUnit.name} deathrattle: Drained enemy hero for 2`)
        break
        
      // ===== MISSING DEATHRATTLE ABILITIES =====
      
      case 'Deathrattle:SummonIceShards2':
        await summonIceShardsDeathrattle(gameState, playerKey, 2)
        console.log(`🧊 ${dyingUnit.name} deathrattle: Summoned 2 Ice Shards`)
        break
        
      case 'Deathrattle:HealEnemyHero1':
        const enemyPlayerKey3 = playerKey === 'player1' ? 'player2' : 'player1'
        const enemyHeroKey2 = `${enemyPlayerKey3}_hero`
        gameState[enemyHeroKey2].hp = Math.min(gameState[enemyHeroKey2].max_hp, gameState[enemyHeroKey2].hp + 1)
        console.log(`💚 ${dyingUnit.name} deathrattle: Healed enemy hero for 1`)
        break
        
      case 'Deathrattle:SummonHusk1_1x3':
        await summonHuskDeathrattle(gameState, playerKey, 1, 1, 3)
        console.log(`👻 ${dyingUnit.name} deathrattle: Summoned 3 Husks`)
        break
        
      case 'Deathrattle:Draw1':
        await drawCardDeathrattle(gameState, playerKey)
        console.log(`🃏 ${dyingUnit.name} deathrattle: Drew card`)
        break
        
      case 'Deathrattle:HealRandomAlly1':
        await healRandomAllyDeathrattle(gameState, playerKey, 1)
        console.log(`💚 ${dyingUnit.name} deathrattle: Healed random ally for 1`)
        break
    }
  }
}

async function summonHuskDeathrattle(gameState: any, playerKey: string, attack: number, hp: number) {
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
    console.log('❌ No empty slots for deathrattle Husk')
    return
  }
  
  // Create Husk token
  const huskToken = {
    id: `husk_deathrattle_${Date.now()}_${Math.random()}`,
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
  console.log(`👻 Deathrattle summoned ${attack}/${hp} Husk in slot ${emptySlotIndex}`)
}

// ===== MISSING DEATHRATTLE HELPERS =====

async function summonIceShardsDeathrattle(gameState: any, playerKey: string, count: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  let summoned = 0
  for (let i = 0; i < playerSlots.length && summoned < count; i++) {
    if (!playerSlots[i]) {
      const iceShardToken = {
        id: `ice_shard_${Date.now()}_${Math.random()}`,
        name: 'Ice Shard',
        card_type: 'unit',
        potato_type: 'ice',
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
      
      playerSlots[i] = iceShardToken
      summoned++
      console.log(`🧊 Deathrattle summoned Ice Shard ${summoned} in slot ${i}`)
    }
  }
}

async function drawCardDeathrattle(gameState: any, playerKey: string) {
  const deckKey = `${playerKey}_deck`
  const handKey = `${playerKey}_hand`
  
  if (gameState[deckKey].length > 0) {
    if (gameState[handKey].length >= 10) {
      const burnedCard = gameState[deckKey].shift()
      console.log(`🔥 Deathrattle card burned due to full hand: ${burnedCard?.name || 'Unknown Card'}`)
    } else {
      const drawnCard = gameState[deckKey].shift()
      gameState[handKey].push(drawnCard)
      console.log('🃏 Deathrattle card drawn')
    }
  }
}

async function healRandomAllyDeathrattle(gameState: any, playerKey: string, amount: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * allyUnits.length)
  const unit = allyUnits[randomIndex]
  
  const oldHp = unit.current_hp || unit.hp
  unit.current_hp = Math.min(unit.max_hp, unit.current_hp + amount)
  const actualHealing = unit.current_hp - oldHp
  
  console.log(`💚 Deathrattle healed ${unit.name} for ${actualHealing} (${unit.current_hp}/${unit.max_hp})`)
}

async function damageAllEnemyUnitsDeathrattle(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (let i = enemySlots.length - 1; i >= 0; i--) {
    const unit = enemySlots[i]
    if (unit && unit.card_type === 'unit') {
      unit.current_hp -= amount
      console.log(`💥 Deathrattle AoE: Damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
      
      // Check if unit dies from deathrattle damage (chain reaction)
      if (unit.current_hp <= 0) {
        await processDeathrattle(gameState, unit, enemyPlayerKey)
        enemySlots[i] = null
        console.log(`💀 ${unit.name} destroyed by deathrattle AoE (chain reaction)`)
      }
    }
  }
}

// ===== ADVANCED KEYWORD SYSTEM =====

async function performDoubleStrikeAttack(gameState: any, attacker: any, target: any, isPlayer1: boolean, targetSlot: number) {
  console.log(`⚔️⚔️ DOUBLE STRIKE: Second attack from ${attacker.name}`)
  
  // Calculate damage for second attack
  const attackerDamage = calculateEffectiveAttack(attacker, gameState, isPlayer1 ? 'player1' : 'player2')
  const targetCurrentHp = target.current_hp || target.hp
  
  // Check for Divine Shield first
  if (target.has_divine_shield) {
    console.log(`🛡️ Divine Shield: ${target.name} blocks second attack!`)
    target.has_divine_shield = false // Shield is consumed
    return // No damage dealt
  }
  
  // Apply damage with reduction
  const reducedDamage = calculateEffectiveDamage(attackerDamage, target, gameState, isPlayer1 ? 'player2' : 'player1')
  const actualDamageDealt = Math.min(reducedDamage, targetCurrentHp)
  const newTargetHp = Math.max(0, targetCurrentHp - reducedDamage)
  target.current_hp = newTargetHp
  
  console.log(`⚔️⚔️ Double Strike damage: ${attackerDamage} → ${reducedDamage} (after reduction) → ${actualDamageDealt} (actual)`)
  
  // Lifesteal on second attack
  if (attacker.has_lifesteal && actualDamageDealt > 0) {
    const playerHeroKey = isPlayer1 ? 'player1_hero' : 'player2_hero'
    const playerHero = gameState[playerHeroKey]
    const healAmount = actualDamageDealt
    playerHero.hp = Math.min(playerHero.max_hp, playerHero.hp + healAmount)
    console.log(`🩸 Double Strike Lifesteal: Healed for ${healAmount} (${playerHero.hp}/${playerHero.max_hp})`)
  }
  
  // Poison on second attack
  if ((attacker.has_poison || attacker.has_poison_touch) && attackerDamage > 0 && newTargetHp > 0) {
    target.current_hp = 0
    console.log(`☠️ Double Strike Poison: ${target.name} destroyed by poison`)
  }
  
  // No counter-attack on second strike (already happened)
  
  // Remove target if killed by second attack
  if (target.current_hp <= 0) {
    const opponentSlots = isPlayer1 ? gameState.battlefield.player2_slots : gameState.battlefield.player1_slots
    await processDeathrattle(gameState, target, isPlayer1 ? 'player2' : 'player1')
    opponentSlots[targetSlot] = null
    console.log(`💀 ${target.name} destroyed by Double Strike`)
  }
}

function applyDivineShield(unit: any) {
  // Initialize Divine Shield based on keywords
  if (!unit.keywords) return
  
  const keywords = Array.isArray(unit.keywords) ? unit.keywords : 
    (typeof unit.keywords === 'string' ? JSON.parse(unit.keywords) : [])
  
  if (keywords.includes('DivineShield')) {
    unit.has_divine_shield = true
    console.log(`🛡️ ${unit.name} gains Divine Shield`)
  }
}

function processDivineShieldDamage(unit: any, incomingDamage: number): number {
  // If unit has Divine Shield, block the first damage and remove shield
  if (unit.has_divine_shield && incomingDamage > 0) {
    console.log(`🛡️ Divine Shield: ${unit.name} blocks ${incomingDamage} damage!`)
    unit.has_divine_shield = false // Shield is consumed
    return 0 // No damage dealt
  }
  
  return incomingDamage // Normal damage
}

// ===== COMBAT TRIGGER SYSTEM =====

async function processOnAttackTriggers(gameState: any, attacker: any, target: any, playerKey: string) {
  if (!attacker.keywords) return
  
  const keywords = Array.isArray(attacker.keywords) ? attacker.keywords : 
    (typeof attacker.keywords === 'string' ? JSON.parse(attacker.keywords) : [])
  
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  
  for (const keyword of keywords) {
    switch (keyword) {
      case 'OnAttack:Heal2':
        const heroKey = `${playerKey}_hero`
        gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 2)
        console.log(`💚 ${attacker.name} OnAttack: Healed hero for 2`)
        break
        
      case 'OnAttack:DrawCard':
        await drawCardOnAttack(gameState, playerKey)
        console.log(`🃏 ${attacker.name} OnAttack: Drew card`)
        break
        
      case 'OnAttack:BuffSelf+1+0':
        attacker.attack += 1
        console.log(`💪 ${attacker.name} OnAttack: Gained +1 ATK (${attacker.attack}/${attacker.current_hp})`)
        break
        
      case 'OnAttack:FreezeTarget':
        if (target.card_type === 'unit') {
          target.is_frozen = true
          target.frozen_turns_remaining = 1
          console.log(`🧊 ${attacker.name} OnAttack: Froze ${target.name}`)
        }
        break
        
      case 'OnAttack:SummonHusk1':
        await summonHuskOnAttack(gameState, playerKey, 1, 1)
        console.log(`👻 ${attacker.name} OnAttack: Summoned 1/1 Husk`)
        break
    }
  }
}

async function processOnDamageTriggers(gameState: any, damagedUnit: any, damageSource: any, playerKey: string, damageAmount: number) {
  if (!damagedUnit.keywords) return
  
  const keywords = Array.isArray(damagedUnit.keywords) ? damagedUnit.keywords : 
    (typeof damagedUnit.keywords === 'string' ? JSON.parse(damagedUnit.keywords) : [])
  
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  
  for (const keyword of keywords) {
    switch (keyword) {
      case 'OnDamage:Heal1':
        const heroKey = `${playerKey}_hero`
        gameState[heroKey].hp = Math.min(gameState[heroKey].max_hp, gameState[heroKey].hp + 1)
        console.log(`💚 ${damagedUnit.name} OnDamage: Healed hero for 1`)
        break
        
      case 'OnDamage:DamageAttacker1':
        if (damageSource.card_type === 'unit') {
          damageSource.current_hp -= 1
          console.log(`💥 ${damagedUnit.name} OnDamage: Damaged attacker for 1 (${damageSource.current_hp}/${damageSource.max_hp})`)
          
          if (damageSource.current_hp <= 0) {
            console.log(`💀 ${damageSource.name} destroyed by OnDamage trigger`)
          }
        }
        break
        
      case 'OnDamage:DrawCard':
        await drawCardOnDamage(gameState, playerKey)
        console.log(`🃏 ${damagedUnit.name} OnDamage: Drew card`)
        break
        
      case 'OnDamage:BuffRandomAlly+1+1':
        await buffRandomAllyOnDamage(gameState, playerKey)
        console.log(`💪 ${damagedUnit.name} OnDamage: Buffed random ally +1/+1`)
        break
        
      case 'OnDamage:FreezeAttacker':
        if (damageSource.card_type === 'unit') {
          damageSource.is_frozen = true
          damageSource.frozen_turns_remaining = 1
          console.log(`🧊 ${damagedUnit.name} OnDamage: Froze attacker ${damageSource.name}`)
        }
        break
        
      // ===== MISSING COMBAT TRIGGERS =====
      
      case 'OnDamageTaken:AOE1Enemies':
        await damageAllEnemiesOnDamage(gameState, playerKey === 'player1' ? 'player2' : 'player1', 1)
        console.log(`💥 ${damagedUnit.name} OnDamageTaken: Damaged all enemies for 1`)
        break
        
      case 'OnDamageTaken:FreezeAttacker':
        if (damageSource.card_type === 'unit') {
          damageSource.is_frozen = true
          damageSource.frozen_turns_remaining = 1
          console.log(`🧊 ${damagedUnit.name} OnDamageTaken: Froze attacker ${damageSource.name}`)
        }
        break
        
      case 'OnDamageTaken:DamageAttacker2':
        if (damageSource.card_type === 'unit') {
          damageSource.current_hp -= 2
          console.log(`💥 ${damagedUnit.name} OnDamageTaken: Damaged attacker for 2 (${damageSource.current_hp}/${damageSource.max_hp})`)
          
          if (damageSource.current_hp <= 0) {
            console.log(`💀 ${damageSource.name} destroyed by OnDamageTaken trigger`)
          }
        }
        break
        
      case 'OnDamage:Freeze':
        if (damageSource.card_type === 'unit') {
          damageSource.is_frozen = true
          damageSource.frozen_turns_remaining = 1
          console.log(`🧊 ${damagedUnit.name} OnDamage: Froze attacker ${damageSource.name}`)
        }
        break
        
      case 'OnDamage:Burn':
        if (damageSource.card_type === 'unit') {
          damageSource.current_hp -= 1
          console.log(`🔥 ${damagedUnit.name} OnDamage: Burned attacker for 1 (${damageSource.current_hp}/${damageSource.max_hp})`)
          
          if (damageSource.current_hp <= 0) {
            console.log(`💀 ${damageSource.name} destroyed by burn`)
          }
        }
        break
        
      case 'OnDamageEnemy:Freeze':
        if (damageSource.card_type === 'unit') {
          damageSource.is_frozen = true
          damageSource.frozen_turns_remaining = 1
          console.log(`🧊 ${damagedUnit.name} OnDamageEnemy: Froze enemy ${damageSource.name}`)
        }
        break
    }
  }
}

async function damageAllEnemiesOnDamage(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (let i = enemySlots.length - 1; i >= 0; i--) {
    const unit = enemySlots[i]
    if (unit && unit.card_type === 'unit') {
      unit.current_hp -= amount
      console.log(`💥 OnDamage AoE damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
      
      if (unit.current_hp <= 0) {
        enemySlots[i] = null
        console.log(`💀 ${unit.name} destroyed by OnDamage AoE`)
      }
    }
  }
}

async function drawCardOnAttack(gameState: any, playerKey: string) {
  const deckKey = `${playerKey}_deck`
  const handKey = `${playerKey}_hand`
  
  if (gameState[deckKey].length > 0) {
    if (gameState[handKey].length >= 10) {
      const burnedCard = gameState[deckKey].shift()
      console.log(`🔥 OnAttack card burned due to full hand: ${burnedCard?.name || 'Unknown Card'}`)
    } else {
      const drawnCard = gameState[deckKey].shift()
      gameState[handKey].push(drawnCard)
      console.log('🃏 OnAttack card drawn')
    }
  }
}

async function drawCardOnDamage(gameState: any, playerKey: string) {
  const deckKey = `${playerKey}_deck`
  const handKey = `${playerKey}_hand`
  
  if (gameState[deckKey].length > 0) {
    if (gameState[handKey].length >= 10) {
      const burnedCard = gameState[deckKey].shift()
      console.log(`🔥 OnDamage card burned due to full hand: ${burnedCard?.name || 'Unknown Card'}`)
    } else {
      const drawnCard = gameState[deckKey].shift()
      gameState[handKey].push(drawnCard)
      console.log('🃏 OnDamage card drawn')
    }
  }
}

async function summonHuskOnAttack(gameState: any, playerKey: string, attack: number, hp: number) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  
  let emptySlotIndex = -1
  for (let i = 0; i < playerSlots.length; i++) {
    if (!playerSlots[i]) {
      emptySlotIndex = i
      break
    }
  }
  
  if (emptySlotIndex === -1) {
    console.log('❌ No empty slots for OnAttack Husk')
    return
  }
  
  const huskToken = {
    id: `husk_onattack_${Date.now()}_${Math.random()}`,
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
  console.log(`👻 OnAttack summoned ${attack}/${hp} Husk in slot ${emptySlotIndex}`)
}

async function buffRandomAllyOnDamage(gameState: any, playerKey: string) {
  const playerSlots = gameState.battlefield[`${playerKey}_slots`] || []
  const allyUnits = playerSlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (allyUnits.length === 0) return
  
  const randomIndex = Math.floor(Math.random() * allyUnits.length)
  const unit = allyUnits[randomIndex]
  
  unit.attack += 1
  unit.current_hp += 1
  unit.max_hp += 1
  
  console.log(`💪 OnDamage buffed ${unit.name} +1/+1 (${unit.attack}/${unit.current_hp})`)
}

// ===== AFTER-ATTACK TRIGGER SYSTEM =====

async function processAfterAttackTriggers(gameState: any, attacker: any, target: any, playerKey: string) {
  if (!attacker.keywords) return
  
  const keywords = Array.isArray(attacker.keywords) ? attacker.keywords : 
    (typeof attacker.keywords === 'string' ? JSON.parse(attacker.keywords) : [])
  
  const enemyPlayerKey = playerKey === 'player1' ? 'player2' : 'player1'
  
  for (const keyword of keywords) {
    switch (keyword) {
      case 'AfterAttack:DestroyEnemyRelic':
        await destroyEnemyRelicAfterAttack(gameState, enemyPlayerKey)
        console.log(`💥 ${attacker.name} AfterAttack: Destroyed enemy relic`)
        break
        
      case 'AfterAttack:ConditionalBuff':
        await conditionalBuffAfterAttack(gameState, attacker, enemyPlayerKey)
        console.log(`💪 ${attacker.name} AfterAttack: Conditional buff`)
        break
        
      case 'AfterAttack:AOE2Enemies':
        await damageAllEnemiesAfterAttack(gameState, enemyPlayerKey, 2)
        console.log(`💥 ${attacker.name} AfterAttack: Damaged all enemies for 2`)
        break
        
      case 'AfterAttack:RandomEnemy2':
        await damageRandomEnemyAfterAttack(gameState, enemyPlayerKey, 2)
        console.log(`💥 ${attacker.name} AfterAttack: Damaged random enemy for 2`)
        break
    }
  }
}

async function destroyEnemyRelicAfterAttack(gameState: any, enemyPlayerKey: string) {
  const enemyRelicsKey = `${enemyPlayerKey}_relics`
  const enemyRelics = gameState.battlefield[enemyRelicsKey] || []
  
  if (enemyRelics.length > 0) {
    const relic = enemyRelics[0]
    enemyRelics.splice(0, 1)
    console.log(`💥 AfterAttack destroyed enemy relic: ${relic.name}`)
    return true
  }
  
  console.log('❌ No enemy relic to destroy')
  return false
}

async function conditionalBuffAfterAttack(gameState: any, attacker: any, enemyPlayerKey: string) {
  const relicDestroyed = await destroyEnemyRelicAfterAttack(gameState, enemyPlayerKey)
  
  if (!relicDestroyed) {
    // No relic destroyed - gain +2 ATK this turn
    attacker.attack += 2
    attacker.temp_attack_bonus = (attacker.temp_attack_bonus || 0) + 2
    console.log(`💪 AfterAttack: ${attacker.name} gained +2 ATK this turn (no relic to destroy)`)
  }
}

async function damageAllEnemiesAfterAttack(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  
  for (let i = enemySlots.length - 1; i >= 0; i--) {
    const unit = enemySlots[i]
    if (unit && unit.card_type === 'unit') {
      unit.current_hp -= amount
      console.log(`💥 AfterAttack AoE damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
      
      if (unit.current_hp <= 0) {
        enemySlots[i] = null
        console.log(`💀 ${unit.name} destroyed by AfterAttack AoE`)
      }
    }
  }
}

async function damageRandomEnemyAfterAttack(gameState: any, enemyPlayerKey: string, amount: number) {
  const enemySlots = gameState.battlefield[`${enemyPlayerKey}_slots`] || []
  const enemyUnits = enemySlots.filter(unit => unit && unit.card_type === 'unit')
  
  if (enemyUnits.length === 0) {
    const heroKey = `${enemyPlayerKey}_hero`
    gameState[heroKey].hp -= amount
    console.log(`💥 No enemy units, AfterAttack damaged enemy hero for ${amount}`)
    return
  }
  
  const randomIndex = Math.floor(Math.random() * enemyUnits.length)
  const unit = enemyUnits[randomIndex]
  
  unit.current_hp -= amount
  console.log(`💥 AfterAttack damaged ${unit.name} for ${amount} (${unit.current_hp}/${unit.max_hp})`)
  
  if (unit.current_hp <= 0) {
    const slotIndex = enemySlots.indexOf(unit)
    enemySlots[slotIndex] = null
    console.log(`💀 ${unit.name} destroyed by AfterAttack`)
  }
}