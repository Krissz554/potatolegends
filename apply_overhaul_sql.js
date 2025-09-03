import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function applyOverhaulSQL() {
  try {
    console.log('🃏 Applying card overhaul with direct SQL...')
    
    // Get all cards first to see what we're working with
    const { data: cards, error: fetchError } = await supabase
      .from('card_complete')
      .select('id, name, rarity')
      .order('created_at')
      .limit(10) // Just check first 10
    
    if (fetchError || !cards) {
      console.error('❌ Error fetching cards:', fetchError)
      return
    }
    
    console.log('📋 Sample cards:', cards.map(c => `${c.name} (${c.rarity})`))
    
    // Try a simple update with direct SQL
    console.log('\n🔄 Testing single card update...')
    
    const testCard = cards[0]
    console.log(`Testing update on: ${testCard.name} (ID: ${testCard.id})`)
    
    const { error: testError } = await supabase
      .rpc('exec', {
        sql: `UPDATE public.card_complete 
              SET rarity = 'common', 
                  card_type = 'unit', 
                  unit_class = 'warrior'
              WHERE id = '${testCard.id}';`
      })
    
    if (testError) {
      console.error('❌ Test update failed:', testError)
    } else {
      console.log('✅ Test update successful!')
      
      // Verify the update
      const { data: verifyCard, error: verifyError } = await supabase
        .from('card_complete')
        .select('name, rarity, card_type, unit_class')
        .eq('id', testCard.id)
        .single()
      
      if (!verifyError && verifyCard) {
        console.log('✅ Verified update:', verifyCard)
      }
    }
    
  } catch (error) {
    console.error('❌ SQL overhaul failed:', error)
  }
}

applyOverhaulSQL()