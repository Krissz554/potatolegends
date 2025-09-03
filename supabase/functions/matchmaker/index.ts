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

  console.log('🎯 Matchmaker function called')

  try {
    // Create supabase client with service role for full access
    const supabaseUrl = Deno.env.get('SUPABASE_URL') ?? ''
    const serviceRoleKey = Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    
    if (!supabaseUrl || !serviceRoleKey) {
      console.error('❌ Missing environment variables')
      return new Response(
        JSON.stringify({ error: 'Configuration error' }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' }, status: 500 }
      )
    }

    const supabaseClient = createClient(supabaseUrl, serviceRoleKey, {
      auth: {
        autoRefreshToken: false,
        persistSession: false
      }
    })

    console.log('✅ Supabase client created with service role')
    console.log('🌐 URL:', supabaseUrl.substring(0, 30) + '...')
    console.log('🔑 Service role key present:', serviceRoleKey.length > 0)

    // Use the improved server-side matchmaking function
    console.log('🎯 Calling server-side matchmaking function...')
    
    const { data: matchResult, error: matchError } = await supabaseClient
      .rpc('periodic_matchmaking_check')

    if (matchError) {
      console.error('❌ Server-side matchmaking error:', matchError)
      return new Response(
        JSON.stringify({ error: matchError.message }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' }, status: 500 }
      )
    }

    const result = matchResult[0] // RPC returns array, take first result
    console.log('📊 Matchmaking result:', result)

    if (result.matches_created > 0) {
      console.log(`🎉 Successfully created ${result.matches_created} matches!`)
      return new Response(
        JSON.stringify({ 
          success: true,
          matches_created: result.matches_created,
          queue_size: result.queue_size,
          message: result.message
        }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    } else {
      console.log(`⏳ No matches created. ${result.queue_size} players in queue.`)
      return new Response(
        JSON.stringify({ 
          success: false,
          matches_created: 0,
          queue_size: result.queue_size,
          message: result.message
        }),
        { headers: { ...corsHeaders, 'Content-Type': 'application/json' } }
      )
    }

  } catch (error) {
    console.error('Matchmaker error:', error)
    return new Response(
      JSON.stringify({ error: 'Internal server error' }),
      { headers: { ...corsHeaders, 'Content-Type': 'application/json' }, status: 500 }
    )
  }
})