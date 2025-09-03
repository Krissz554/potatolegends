import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function checkTagsColumn() {
  try {
    console.log('🔍 Checking current tags column usage...')
    
    // Get sample cards to see what tags look like
    const { data: sampleCards, error: fetchError } = await supabase
      .from('card_complete')
      .select('id, name, tags')
      .limit(10)
    
    if (fetchError) {
      console.error('❌ Error fetching cards:', fetchError)
      return
    }
    
    console.log('📋 Sample cards with current tags:')
    sampleCards.forEach(card => {
      console.log(`  ${card.name}: ${JSON.stringify(card.tags)}`)
    })
    
    // Check unique tag values across all cards
    const { data: allCards, error: allError } = await supabase
      .from('card_complete')
      .select('tags')
    
    if (!allError && allCards) {
      const allTags = new Set()
      allCards.forEach(card => {
        if (card.tags && Array.isArray(card.tags)) {
          card.tags.forEach(tag => allTags.add(tag))
        }
      })
      
      console.log('\n🏷️ All unique tag values currently used:')
      console.log(Array.from(allTags))
      
      // Check if all cards just have ["core"]
      const onlyCore = allCards.every(card => 
        JSON.stringify(card.tags) === JSON.stringify(["core"])
      )
      
      if (onlyCore) {
        console.log('\n✅ All cards only have ["core"] tag - safe to repurpose!')
        console.log('🎯 We can safely repurpose this column for ability tags.')
        
        console.log('\n📝 To repurpose the tags column, run this SQL:')
        console.log('')
        console.log('-- Clear current tags and repurpose for ability tags')
        console.log("UPDATE public.card_complete SET tags = ARRAY[]::TEXT[];")
        console.log('')
        console.log('-- Update column comment')
        console.log("COMMENT ON COLUMN public.card_complete.tags IS 'Ability keyword tags like [\"Taunt\", \"Charge\", \"Battlecry\"] - displayed as badges on cards';")
        
      } else {
        console.log('\n⚠️ Tags column has diverse values - might need to keep it')
        console.log('🔧 Consider adding a separate ability_tags column instead')
      }
    }
    
  } catch (error) {
    console.error('❌ Check failed:', error)
  }
}

checkTagsColumn()