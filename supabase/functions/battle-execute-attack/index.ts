import { serve } from "https://deno.land/std@0.168.0/http/server.ts"
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

interface ExecuteAttackRequest {
  battleId: string
}

interface BattleSession {
  id: string
  player1_id: string
  player2_id: string
  current_turn_player_id: string
  game_state: {
    player1_deck: any[]
    player2_deck: any[]
    player1_hand: any[]
    player2_hand: any[]
    battlefield: {
      player1_active?: any
      player2_active?: any
    }
    phase: string
    turn_count: number
    round_count: number
    first_player_id: string
    waiting_for_deploy_player_id?: string
    current_attacker_id?: string
    last_round_winner_id?: string
  }
  status: string
}

serve(async (req) => {
  // Handle CORS preflight requests
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
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
    const { battleId }: ExecuteAttackRequest = await req.json()

    if (!battleId) {
      throw new Error('Missing battleId')
    }

    // Get battle session with row-level security
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

    // For auto-combat, we allow the attack if the user is part of the battle
    // and the game is in combat phase, regardless of whose "turn" it is
    // The attack logic will use the actual current_attacker_id from game state
    
    // Verify game phase allows attacks
    if (battleSession.game_state.phase !== 'combat') {
      throw new Error('Not in combat phase')
    }

    // Use the current attacker from game state (not the requesting user)
    const actualAttackerId = battleSession.game_state.current_attacker_id
    if (!actualAttackerId) {
      throw new Error('No current attacker set')
    }

    console.log(`⚔️ Attack request: User ${user.id}, Actual attacker: ${actualAttackerId}`)
    
    // Allow the attack to proceed with the actual attacker
    // (This enables auto-combat where any player can trigger the attack)

    const gameState = battleSession.game_state
    const isPlayer1Attacking = battleSession.player1_id === actualAttackerId
    const attackingCard = isPlayer1Attacking ? gameState.battlefield.player1_active : gameState.battlefield.player2_active
    const defendingCard = isPlayer1Attacking ? gameState.battlefield.player2_active : gameState.battlefield.player1_active
    
    console.log(`🎯 Attack details:`, {
      attacker_id: actualAttackerId,
      is_player1_attacking: isPlayer1Attacking,
      attacking_card: attackingCard?.potato?.name || 'none',
      defending_card: defendingCard?.potato?.name || 'none'
    })

    // Verify both cards exist on battlefield
    if (!attackingCard || !defendingCard) {
      throw new Error('Missing cards on battlefield')
    }

    // Calculate damage (server-side validation)
    const damage = attackingCard.attack
    const newHp = defendingCard.current_hp - damage
    
    let newGameState = { ...gameState }
    let battleResult = {
      damage,
      remainingHp: Math.max(0, newHp),
      cardDefeated: newHp <= 0,
      roundWinner: null as string | null,
      gameWinner: null as string | null,
      newPhase: 'combat' as string,
      nextPlayer: null as string | null
    }
    
    // Update defending card HP
    if (isPlayer1Attacking) {
      newGameState.battlefield.player2_active = {
        ...defendingCard,
        current_hp: Math.max(0, newHp)
      }
    } else {
      newGameState.battlefield.player1_active = {
        ...defendingCard,
        current_hp: Math.max(0, newHp)
      }
    }

    // Check if defending card died
    if (newHp <= 0) {
      // Card died, round ends
      const winnerId = actualAttackerId
      const loserId = isPlayer1Attacking ? battleSession.player2_id : battleSession.player1_id
      
      battleResult.cardDefeated = true
      battleResult.roundWinner = winnerId
      
      // Remove dead card from battlefield
      if (isPlayer1Attacking) {
        newGameState.battlefield.player2_active = null
      } else {
        newGameState.battlefield.player1_active = null
      }
      
      // Check if loser has cards left
      const loserHand = isPlayer1Attacking ? gameState.player2_hand : gameState.player1_hand
      
      if (loserHand.length === 0) {
        // Game over
        newGameState.phase = 'finished'
        newGameState.current_attacker_id = null
        battleResult.gameWinner = winnerId
        battleResult.newPhase = 'finished'
        
        // Update battle session as finished
        await supabaseClient
          .from('battle_sessions')
          .update({
            game_state: newGameState,
            status: 'finished',
            winner_id: winnerId,
            finished_at: new Date().toISOString(),
            updated_at: new Date().toISOString()
          })
          .eq('id', battleId)
          
      } else {
        // Loser must deploy new card
        newGameState.phase = 'waiting_redeploy'
        newGameState.waiting_for_deploy_player_id = loserId
        newGameState.last_round_winner_id = winnerId
        battleResult.newPhase = 'waiting_redeploy'
        battleResult.nextPlayer = loserId
        
        // Update battle session
        await supabaseClient
          .from('battle_sessions')
          .update({
            game_state: newGameState,
            current_turn_player_id: loserId,
            updated_at: new Date().toISOString()
          })
          .eq('id', battleId)
      }
      
    } else {
      // Card survived, switch attacker
      newGameState.current_attacker_id = isPlayer1Attacking ? battleSession.player2_id : battleSession.player1_id
      battleResult.nextPlayer = newGameState.current_attacker_id
      
      // Update battle session
      await supabaseClient
        .from('battle_sessions')
        .update({
          game_state: newGameState,
          current_turn_player_id: newGameState.current_attacker_id,
          updated_at: new Date().toISOString()
        })
        .eq('id', battleId)
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
          name: 'attack_executed',
          data: {
            type: 'auto_attack',
            damage: battleResult.damage,
            remainingHp: battleResult.remainingHp,
            cardDefeated: battleResult.cardDefeated,
            roundWinner: battleResult.roundWinner,
            gameWinner: battleResult.gameWinner,
            battleId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish attack to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published attack execution to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
    }

    // Create battle action log
    await supabaseClient
      .from('battle_actions')
      .insert({
        battle_session_id: battleId,
        player_id: actualAttackerId,
        action_type: 'attack',
        action_data: {
          damage: battleResult.damage,
          remaining_hp: battleResult.remainingHp,
          defeated_card_id: battleResult.cardDefeated ? defendingCard.potato_id : null,
          round_winner: battleResult.roundWinner,
          game_winner: battleResult.gameWinner
        }
      })

    return new Response(
      JSON.stringify({ 
        success: true, 
        message: 'Attack executed successfully',
        result: battleResult
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200,
      },
    )

  } catch (error) {
    console.error('❌ Execute attack error:', error)
    console.error('❌ Error details:', {
      message: error.message,
      stack: error.stack,
      name: error.name
    })
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