import { serve } from "https://deno.land/std@0.168.0/http/server.ts"
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

interface SurrenderRequest {
  battleId: string
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
    const { battleId }: SurrenderRequest = await req.json()

    if (!battleId) {
      throw new Error('Missing battleId')
    }

    // Get battle session
    const { data: battle, error: battleError } = await supabaseClient
      .from('battle_sessions')
      .select('*')
      .eq('id', battleId)
      .single()

    if (battleError || !battle) {
      throw new Error('Battle not found or access denied')
    }

    // Verify user is part of this battle
    if (battle.player1_id !== user.id && battle.player2_id !== user.id) {
      throw new Error('User not part of this battle')
    }

    // Check if battle is already finished
    if (battle.status === 'finished') {
      throw new Error('Battle is already finished')
    }

    // Determine winner (the opponent)
    const winnerId = battle.player1_id === user.id ? battle.player2_id : battle.player1_id

    // Update game state
    const updatedGameState = {
      ...battle.game_state,
      phase: 'finished',
      end_reason: 'surrender',
      surrendered_by: user.id,
      winner_id: winnerId
    }

    // Update battle session as finished
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: updatedGameState,
        status: 'finished',
        winner_id: winnerId,
        finished_at: new Date().toISOString(),
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
          name: 'game_ended',
          data: {
            type: 'surrender',
            surrenderedBy: user.id,
            winnerId,
            battleSessionId: battleId,
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish surrender to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published surrender to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
    }

    // Create surrender action log
    await supabaseClient
      .from('battle_actions')
      .insert({
        battle_session_id: battleId,
        player_id: user.id,
        action_type: 'surrender',
        action_data: {
          reason: 'player_surrender',
          winner_id: winnerId,
          surrendered_by: user.id
        }
      })

    return new Response(
      JSON.stringify({ 
        success: true, 
        message: 'Battle surrendered successfully',
        winnerId: winnerId
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200,
      },
    )

  } catch (error) {
    console.error('Surrender battle error:', error)
    return new Response(
      JSON.stringify({ 
        success: false, 
        error: error.message 
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 400,
      },
    )
  }
})