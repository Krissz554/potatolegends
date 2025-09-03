import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey, {
  auth: {
    autoRefreshToken: false,
    persistSession: false
  }
})

async function testSingleUpdate() {
  try {
    console.log('🧪 Testing single card update...')
    
    // Get one card
    const { data: cards, error: fetchError } = await supabase
      .from('card_complete')
      .select('id, name, rarity, card_type, attack, hp')
      .limit(1)
    
    if (fetchError || !cards || cards.length === 0) {
      console.error('❌ Error fetching card:', fetchError)
      return
    }
    
    const card = cards[0]
    console.log('📋 Testing with card:', card)
    
    // Try the update with explicit WHERE clause
    console.log('🔄 Attempting update...')
    
    const { data: updateResult, error: updateError } = await supabase
      .from('card_complete')
      .update({ 
        card_type: 'unit',
        unit_class: 'warrior'
      })
      .eq('id', card.id)
      .select()
    
    if (updateError) {
      console.error('❌ Update failed:', updateError)
      
      // Check if it's an RLS issue
      if (updateError.message.includes('requires a WHERE clause')) {
        console.log('🔍 This is likely an RLS policy issue')
        
        // Try with service role permissions
        console.log('🔧 Checking service role permissions...')
        
        const { data: permCheck, error: permError } = await supabase
          .from('card_complete')
          .select('id')
          .limit(1)
        
        if (permError) {
          console.error('❌ Service role cannot even SELECT:', permError)
        } else {
          console.log('✅ Service role can SELECT')
        }
      }
    } else {
      console.log('✅ Update successful!', updateResult)
    }
    
  } catch (error) {
    console.error('❌ Test failed:', error)
  }
}

testSingleUpdate()