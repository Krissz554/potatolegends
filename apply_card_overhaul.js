import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

// Target distribution
const TARGET_DISTRIBUTION = {
  'common': 90,
  'uncommon': 56, 
  'rare': 45,
  'legendary': 23,
  'exotic': 12
}

async function applyCardOverhaul() {
  try {
    console.log('🃏 Applying card overhaul to existing 226 cards...')
    
    // Get all cards
    const { data: cards, error: fetchError } = await supabase
      .from('card_complete')
      .select('*')
      .order('created_at')
    
    if (fetchError || !cards) {
      console.error('❌ Error fetching cards:', fetchError)
      return
    }
    
    console.log(`📋 Found ${cards.length} cards to update`)
    
    // Show current distribution
    const currentDist = cards.reduce((acc, card) => {
      acc[card.rarity] = (acc[card.rarity] || 0) + 1
      return acc
    }, {})
    console.log('📊 Current:', currentDist)
    console.log('🎯 Target:', TARGET_DISTRIBUTION)
    
    let updateCount = 0
    let cardIndex = 0
    
    // Update cards according to target distribution
    for (const [targetRarity, count] of Object.entries(TARGET_DISTRIBUTION)) {
      console.log(`\n🔄 Updating ${count} cards to ${targetRarity}...`)
      
      for (let i = 0; i < count && cardIndex < cards.length; i++) {
        const card = cards[cardIndex]
        
        // Determine card type and class
        let cardType = 'unit'
        let unitClass = 'warrior'
        let keywords = []
        let abilityText = ''
        
        // Smart card type assignment based on name patterns
        const name = card.name.toLowerCase()
        
        if (name.includes('bolt') || name.includes('blast') || name.includes('wave') || 
            name.includes('surge') || name.includes('storm') || name.includes('fire') ||
            (Math.random() < 0.15)) { // 15% chance to be spell
          cardType = 'spell'
          unitClass = null
        } else if (name.includes('wall') || name.includes('totem') || name.includes('shrine') ||
                   name.includes('barrier') || (Math.random() < 0.10)) { // 10% chance to be structure
          cardType = 'structure'
          unitClass = null
        } else if (name.includes('crown') || name.includes('relic') || name.includes('amulet') ||
                   (Math.random() < 0.05)) { // 5% chance to be relic
          cardType = 'relic'
          unitClass = null
        } else {
          // Unit - assign class based on name and stats
          if (name.includes('guard') || name.includes('tank') || name.includes('wall') ||
              (card.hp || 0) >= (card.attack || 0) + 2) {
            unitClass = 'tank'
            keywords = ['taunt']
            abilityText = 'Taunt - Enemies must attack this unit first.'
          } else if (name.includes('healer') || name.includes('priest') || name.includes('sage')) {
            unitClass = 'healer'
            keywords = ['lifesteal']
            abilityText = 'Lifesteal - Heal your hero for damage dealt.'
          } else if (name.includes('mage') || name.includes('wizard') || name.includes('alchemist') ||
                     (card.attack || 0) >= (card.hp || 0) + 1) {
            unitClass = 'mage'
          } else {
            unitClass = 'warrior'
            if (targetRarity === 'common' && Math.random() < 0.3) {
              keywords = ['charge']
              abilityText = 'Charge - Can attack immediately when played.'
            }
          }
        }
        
        // Build update data
        const updateData = {
          rarity: targetRarity,
          card_type: cardType,
          unit_class: unitClass,
          keywords: keywords,
          ability_text: abilityText
        }
        
        // Set type-specific fields
        if (cardType === 'spell') {
          updateData.spell_damage = Math.max(1, Math.min(5, card.attack || 2))
          updateData.target_type = 'enemy_unit'
          updateData.attack = null
          updateData.hp = null
        } else if (cardType === 'structure') {
          updateData.structure_hp = Math.max(1, Math.min(8, card.hp || 3))
          updateData.attack = null
          updateData.hp = null
        } else if (cardType === 'relic') {
          updateData.attack = null
          updateData.hp = null
        }
        
        // Apply the update
        try {
          const { error: updateError } = await supabase
            .from('card_complete')
            .update(updateData)
            .eq('id', card.id)
          
          if (updateError) {
            console.error(`❌ Error updating ${card.name}:`, updateError.message)
          } else {
            updateCount++
            if (updateCount % 25 === 0) {
              console.log(`  ✅ Updated ${updateCount}/${cards.length} cards...`)
            }
          }
        } catch (error) {
          console.error(`❌ Unexpected error updating ${card.name}:`, error)
        }
        
        cardIndex++
      }
    }
    
    console.log(`\n🎯 Card overhaul complete! Updated ${updateCount}/${cards.length} cards`)
    
    // Show final distribution
    const { data: updatedCards, error: finalFetchError } = await supabase
      .from('card_complete')
      .select('rarity, card_type, unit_class')
    
    if (!finalFetchError && updatedCards) {
      const finalDist = updatedCards.reduce((acc, card) => {
        acc[card.rarity] = (acc[card.rarity] || 0) + 1
        return acc
      }, {})
      
      const typeDist = updatedCards.reduce((acc, card) => {
        acc[card.card_type] = (acc[card.card_type] || 0) + 1
        return acc
      }, {})
      
      console.log('\n📊 Final rarity distribution:')
      Object.entries(finalDist).forEach(([rarity, count]) => {
        const target = TARGET_DISTRIBUTION[rarity] || 0
        const percentage = ((count / 226) * 100).toFixed(1)
        console.log(`  ${rarity}: ${count}/${target} (${percentage}%)`)
      })
      
      console.log('\n🎨 Card type distribution:')
      Object.entries(typeDist).forEach(([type, count]) => {
        const percentage = ((count / 226) * 100).toFixed(1)
        console.log(`  ${type}: ${count} (${percentage}%)`)
      })
    }
    
  } catch (error) {
    console.error('❌ Card overhaul failed:', error)
  }
}

applyCardOverhaul()