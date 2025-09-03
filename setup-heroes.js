// Script to manually set up the hero system if migrations haven't run
import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTYwNTg0MzIsImV4cCI6MjA3MTYzNDQzMn0.J8G45OdXxTbWuGO8N05_50qOVPq7BMSDo1xeegXQMW0'

const supabase = createClient(supabaseUrl, supabaseKey)

async function setupHeroSystem() {
  console.log('🔍 Checking if hero system exists...')
  
  try {
    // Test if heroes table exists
    const { data: heroes, error: heroesError } = await supabase
      .from('heroes')
      .select('hero_id')
      .limit(1)
    
    if (heroesError) {
      console.log('❌ Heroes table does not exist:', heroesError.message)
      console.log('📋 This means the migration 042_create_hero_system.sql has not been applied to the database.')
      console.log('💡 Solutions:')
      console.log('   1. Apply the migration manually in Supabase Dashboard')
      console.log('   2. Use Supabase CLI: supabase db push')
      console.log('   3. Copy the SQL from the migration file and run it in the SQL editor')
      return
    }
    
    console.log('✅ Heroes table exists!')
    
    // Check if Potato King exists
    const { data: potatoKing, error: kingError } = await supabase
      .from('heroes')
      .select('*')
      .eq('hero_id', 'potato_king')
      .single()
    
    if (kingError || !potatoKing) {
      console.log('❌ Potato King hero not found')
      console.log('💡 Need to run the hero data insertion from migration')
      return
    }
    
    console.log('✅ Potato King hero exists:', potatoKing.name)
    
    // Check if user_heroes table exists
    const { data: userHeroes, error: userHeroError } = await supabase
      .from('user_heroes')
      .select('id')
      .limit(1)
    
    if (userHeroError) {
      console.log('❌ User heroes table does not exist:', userHeroError.message)
      return
    }
    
    console.log('✅ User heroes table exists!')
    
    // Check if the RPC functions exist
    const { data: rpcResult, error: rpcError } = await supabase.rpc('unlock_starter_heroes_for_user', {
      user_uuid: '00000000-0000-0000-0000-000000000000' // Fake UUID just to test if function exists
    })
    
    if (rpcError && rpcError.message.includes('could not find function')) {
      console.log('❌ RPC functions not found:', rpcError.message)
      console.log('💡 The database functions from the migration are missing')
      return
    }
    
    console.log('✅ All hero system components are in place!')
    console.log('🎉 Hero system should be working correctly')
    
  } catch (error) {
    console.error('❌ Error checking hero system:', error)
  }
}

setupHeroSystem().catch(console.error)