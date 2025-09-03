import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function removeKeywordConstraint() {
  try {
    console.log('🔧 Removing keyword constraint to allow flexible ability tags...')
    
    // Test if we can update a card first
    const { data: testCard, error: fetchError } = await supabase
      .from('card_complete')
      .select('id, name')
      .limit(1)
      .single()
    
    if (fetchError || !testCard) {
      console.error('❌ Cannot fetch test card:', fetchError)
      return
    }
    
    console.log(`🧪 Testing with card: ${testCard.name}`)
    
    // Try to update with complex keywords
    const { error: updateError } = await supabase
      .from('card_complete')
      .update({ 
        keywords: ["Taunt", "Charge", "Battlecry:AOE3All", "Battlecry:FreezeRandom"]
      })
      .eq('id', testCard.id)
    
    if (updateError) {
      console.error('❌ Update failed:', updateError)
      
      if (updateError.message.includes('valid_keywords')) {
        console.log('🔧 Keyword constraint is the issue. You need to run this SQL:')
        console.log('')
        console.log('ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS valid_keywords;')
        console.log('')
        console.log('Go to Supabase Dashboard → SQL Editor and run that command.')
      }
    } else {
      console.log('✅ Keywords update successful!')
    }
    
  } catch (error) {
    console.error('❌ Test failed:', error)
  }
}

removeKeywordConstraint()