import { serve } from "https://deno.land/std@0.168.0/http/server.ts"
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

serve(async (req) => {
  // Handle CORS preflight requests
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  console.log('🔍 Get Active Battle function called')

  try {
    // Get auth header
    const authHeader = req.headers.get('Authorization')
    if (!authHeader) {
      return new Response(
        JSON.stringify({ error: 'No authorization header' }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' }, status: 401 }
      )
    }

    // Create supabase client with the user's token
    const supabaseUrl = Deno.env.get('SUPABASE_URL') ?? ''
    const supabaseAnonKey = Deno.env.get('SUPABASE_ANON_KEY') ?? ''
    
    const supabaseClient = createClient(supabaseUrl, supabaseAnonKey, {
      global: {
        headers: { Authorization: authHeader },
      },
    })

    // Get user from token
    const { data: { user }, error: userError } = await supabaseClient.auth.getUser()
    
    if (userError || !user) {
      console.error('❌ User authentication error:', userError)
      return new Response(
        JSON.stringify({ error: 'User not authenticated' }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' }, status: 401 }
      )
    }

    console.log('👤 Getting active battle for user:', user.id)

    // Use the server-side function to get active battle
    const serviceRoleKey = Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    const serviceClient = createClient(supabaseUrl, serviceRoleKey)

    const { data: activeBattle, error: battleError } = await serviceClient
      .rpc('get_user_active_battle', { p_user_id: user.id })

    if (battleError) {
      console.error('❌ Error getting active battle:', battleError)
      return new Response(
        JSON.stringify({ error: battleError.message }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' }, status: 500 }
      )
    }

    if (activeBattle && activeBattle.length > 0) {
      const battle = activeBattle[0]
      console.log('🎮 Found active battle:', battle.battle_id)
      
      return new Response(
        JSON.stringify({ 
          success: true,
          activeBattle: {
            id: battle.battle_id,
            player1_id: battle.player1_id,
            player2_id: battle.player2_id,
            status: battle.status,
            started_at: battle.started_at,
            game_state: battle.game_state
          }
        }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    } else {
      console.log('✅ No active battle found')
      return new Response(
        JSON.stringify({ 
          success: true,
          activeBattle: null
        }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

  } catch (error) {
    console.error('Get Active Battle error:', error)
    return new Response(
      JSON.stringify({ error: 'Internal server error' }),
      { headers: { ...corsHeaders, 'Content-Type': 'application/json' }, status: 500 }
    )
  }
})