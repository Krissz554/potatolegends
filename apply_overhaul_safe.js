import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function applyOverhaulSafe() {
  try {
    console.log('🃏 Applying card overhaul safely...')
    
    // Get all cards
    const { data: cards, error: fetchError } = await supabase
      .from('card_complete')
      .select('*')
      .order('created_at')
    
    if (fetchError || !cards) {
      console.error('❌ Error fetching cards:', fetchError)
      return
    }
    
    console.log(`📋 Found ${cards.length} cards`)
    
    // Target distribution
    const targets = [
      { rarity: 'common', count: 90 },
      { rarity: 'uncommon', count: 56 },
      { rarity: 'rare', count: 45 },
      { rarity: 'legendary', count: 23 },
      { rarity: 'exotic', count: 12 }
    ]
    
    let cardIndex = 0
    let successCount = 0
    
    for (const { rarity, count } of targets) {
      console.log(`\n🎯 Processing ${count} ${rarity} cards...`)
      
      for (let i = 0; i < count && cardIndex < cards.length; i++) {
        const card = cards[cardIndex]
        
        try {
          // Simple update: just rarity and card_type for now
          const { error: updateError } = await supabase
            .from('card_complete')
            .update({ 
              rarity: rarity,
              card_type: 'unit', // Start with all units
              unit_class: 'warrior' // Default class
            })
            .eq('id', card.id)
          
          if (updateError) {
            console.error(`❌ ${card.name}:`, updateError.message)
          } else {
            successCount++
            if (successCount % 20 === 0) {
              console.log(`  ✅ ${successCount} cards updated...`)
            }
          }
        } catch (error) {
          console.error(`❌ ${card.name}:`, error.message)
        }
        
        cardIndex++
      }
    }
    
    console.log(`\n🎯 Overhaul complete! Updated ${successCount}/${cards.length} cards`)
    
    // Check final distribution
    const { data: finalCards, error: finalError } = await supabase
      .from('card_complete')
      .select('rarity')
    
    if (!finalError && finalCards) {
      const finalDist = finalCards.reduce((acc, card) => {
        acc[card.rarity] = (acc[card.rarity] || 0) + 1
        return acc
      }, {})
      
      console.log('\n📊 Final distribution:')
      console.log('  Common:', finalDist.common || 0, '/ 90 target')
      console.log('  Uncommon:', finalDist.uncommon || 0, '/ 56 target')
      console.log('  Rare:', finalDist.rare || 0, '/ 45 target')
      console.log('  Legendary:', finalDist.legendary || 0, '/ 23 target')
      console.log('  Exotic:', finalDist.exotic || 0, '/ 12 target')
    }
    
  } catch (error) {
    console.error('❌ Overhaul failed:', error)
  }
}

applyOverhaulSafe()