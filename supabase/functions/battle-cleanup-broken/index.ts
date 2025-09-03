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

  try {
    // Get the authorization header
    const authHeader = req.headers.get('authorization')
    if (!authHeader) {
      throw new Error('No authorization header')
    }

    // Create Supabase client
    const supabaseClient = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_ANON_KEY') ?? '',
      { global: { headers: { Authorization: authHeader } } }
    )

    // Get user from auth header
    const { data: { user }, error: userError } = await supabaseClient.auth.getUser()
    if (userError || !user) {
      throw new Error('Invalid user')
    }

    console.log('🧹 Cleaning up broken battles for user:', user.id)

    // Find all active battles for this user
    const { data: userBattles, error: battleError } = await supabaseClient
      .from('battle_sessions')
      .select('*')
      .or(`player1_id.eq.${user.id},player2_id.eq.${user.id}`)
      .in('status', ['deploying', 'battling'])

    if (battleError) {
      console.error('Error finding user battles:', battleError)
      throw new Error('Failed to find user battles')
    }

    console.log('🔍 Found battles:', userBattles?.length || 0)

    let cleanedCount = 0
    
    if (userBattles && userBattles.length > 0) {
      for (const battle of userBattles) {
        console.log('🗑️ Cleaning battle:', battle.id)
        
        // Mark battle as finished/abandoned
        await supabaseClient
          .from('battle_sessions')
          .update({
            status: 'finished',
            finished_at: new Date().toISOString(),
            game_state: {
              ...battle.game_state,
              phase: 'finished',
              end_reason: 'cleaned_up',
              cleaned_at: new Date().toISOString()
            }
          })
          .eq('id', battle.id)

        cleanedCount++
      }
    }

    // Also clean up any stale matchmaking entries
    await supabaseClient
      .from('matchmaking_queue')
      .delete()
      .eq('user_id', user.id)

    console.log('✅ Cleanup complete:', cleanedCount, 'battles cleaned')

    return new Response(
      JSON.stringify({ 
        success: true, 
        message: 'Cleanup completed successfully',
        cleanedBattles: cleanedCount
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200,
      },
    )

  } catch (error) {
    console.error('Cleanup error:', error)
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