// =============================================
// Concede Edge Function
// Handles player surrender and match termination
// =============================================

import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

serve(async (req) => {
  const corsHeaders = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
  }

  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    console.log('🚀 Concede function started')
    
    const supabase = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    )
    console.log('✅ Supabase client created')

    // Get user from JWT
    console.log('🔐 Extracting user from JWT...')
    const authHeader = req.headers.get('Authorization')!
    const jwt = authHeader.replace('Bearer ', '')
    const { data: { user }, error: authError } = await supabase.auth.getUser(jwt)

    if (authError || !user) {
      console.error('❌ Authentication failed:', authError)
      return new Response(
        JSON.stringify({ success: false, error: 'Authentication failed' }),
        { status: 401, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }
    console.log('✅ User authenticated:', user.id)

    console.log('📦 Parsing request body...')
    const { matchId } = await req.json()
    console.log('🎯 Match ID received:', matchId)

    if (!matchId) {
      return new Response(
        JSON.stringify({ success: false, error: 'Missing match ID' }),
        { status: 400, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

    console.log('🏳️ Concede request from player:', user.id, 'in battle:', matchId)

    // Load current battle session
    const { data: battle, error: battleError } = await supabase
      .from('battle_sessions')
      .select('*')
      .eq('id', matchId)
      .single()

    if (battleError || !battle) {
      console.error('Error loading battle:', battleError)
      return new Response(
        JSON.stringify({ success: false, error: 'Battle not found' }),
        { status: 404, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

    // Check if battle is already finished
    if (battle.status === 'finished') {
      return new Response(
        JSON.stringify({ success: false, error: 'Battle is already finished' }),
        { status: 400, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

    const gameState = battle.game_state

    // Validate player is in the battle
    if (battle.player1_id !== user.id && battle.player2_id !== user.id) {
      return new Response(
        JSON.stringify({ success: false, error: 'Player not found in battle' }),
        { status: 400, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

    // Determine winner (the other player)
    const winnerId = battle.player1_id === user.id ? battle.player2_id : battle.player1_id

    if (!winnerId) {
      return new Response(
        JSON.stringify({ success: false, error: 'Could not determine winner' }),
        { status: 400, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

    console.log('✅ Concede validation passed, ending battle')
    console.log('🎮 Current game state:', JSON.stringify(gameState, null, 2))
    console.log('👤 Winner ID:', winnerId)
    console.log('💀 Conceding player:', user.id)

    // Create new game state
    const newGameState = { ...gameState }
    newGameState.game_status = 'completed'
    newGameState.winner_id = winnerId
    newGameState.phase = 'ended'

    // Mark conceding player's hero as defeated
    try {
      if (battle.player1_id === user.id && newGameState.player1_hero) {
        newGameState.player1_hero.hp = 0
        console.log('💀 Player 1 hero defeated')
      } else if (battle.player2_id === user.id && newGameState.player2_hero) {
        newGameState.player2_hero.hp = 0
        console.log('💀 Player 2 hero defeated')
      }
    } catch (heroUpdateError) {
      console.error('Error updating hero HP:', heroUpdateError)
      // Continue without hero HP update
    }

    const endTime = new Date().toISOString()
    console.log('⏰ End time:', endTime)

    // Update battle session
    console.log('💾 Updating battle session...')
    const battleUpdateResult = await supabase
      .from('battle_sessions')
      .update({
        status: 'finished',
        winner_id: winnerId,
        game_state: newGameState,
        updated_at: endTime
      })
      .eq('id', matchId)
    
    console.log('✅ Battle update result:', battleUpdateResult)

    if (battleUpdateResult.error) {
      console.error('Database update error:', battleUpdateResult.error)
      return new Response(
        JSON.stringify({ success: false, error: 'Failed to update battle state' }),
        { status: 500, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

    console.log('💾 Database updated successfully')

    // Publish to Ably for real-time updates
    try {
      const ablyResponse = await fetch('https://rest.ably.io/channels/match:' + matchId + '/messages', {
        method: 'POST',
        headers: {
          'Authorization': 'Basic ' + btoa('1Vf05w.bzqjGQ:6YuVnRKLTCKDlHR-tDzDZPGfA5pBP57vWP4nFRrFXm0'),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'game_ended',
          data: {
            type: 'concede',
            concededBy: user.id,
            winnerId,
            battleSessionId: matchId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish concede to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published concede to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
    }

    // Update hero battle stats
    try {
      const player1Hero = newGameState.player1_hero
      const player2Hero = newGameState.player2_hero
      
      if (player1Hero && player2Hero) {
        // Update winner's hero stats
        await supabase.rpc('update_hero_battle_stats', {
          user_uuid: winnerId,
          hero_id_param: winnerId === battle.player1_id ? player1Hero.id : player2Hero.id,
          won: true
        })
        
        // Update loser's hero stats (the one who conceded)
        await supabase.rpc('update_hero_battle_stats', {
          user_uuid: user.id,
          hero_id_param: user.id === battle.player1_id ? player1Hero.id : player2Hero.id,
          won: false
        })
        
        console.log('📊 Hero battle stats updated')
      }
    } catch (error) {
      console.error('Error updating hero battle stats:', error)
      // Don't fail the concede if stats update fails
    }

    // Get winner profile for notification
    let winnerName = 'Unknown'
    try {
      const { data: winnerProfile } = await supabase
        .from('user_profiles')
        .select('username, display_name')
        .eq('id', winnerId)
        .single()

      winnerName = winnerProfile?.display_name || winnerProfile?.username || 'Unknown'
    } catch (profileError) {
      console.error('Error fetching winner profile:', profileError)
      // Continue with default name
    }

    // Publish to realtime channel
    try {
      const matchDuration = battle.created_at ? 
        new Date().getTime() - new Date(battle.created_at).getTime() : 0
      
      await supabase
        .channel(`match:${matchId}`)
        .send({
          type: 'broadcast',
          event: 'match_ended',
          payload: {
            matchId,
            reason: 'concede',
            concedePlayerId: user.id,
            winnerId,
            winnerName,
            gameState: newGameState,
            matchDuration,
            totalTurns: gameState.turn_number || 0
          }
        })

      console.log('📡 Realtime update sent')
    } catch (broadcastError) {
      console.error('Error sending realtime broadcast:', broadcastError)
      // Don't fail the concede if broadcast fails
    }

    return new Response(
      JSON.stringify({ 
        success: true, 
        gameState: newGameState,
        winnerId,
        winnerName,
        reason: 'concede',
        matchFinished: true
      }),
      { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
    )

  } catch (error) {
    console.error('❌ Unexpected error in concede function:')
    console.error('Error name:', error.name)
    console.error('Error message:', error.message)
    console.error('Error stack:', error.stack)
    console.error('Full error object:', JSON.stringify(error, null, 2))
    
    return new Response(
      JSON.stringify({ 
        success: false, 
        error: 'Internal server error',
        details: error.message 
      }),
      { status: 500, headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
    )
  }
})