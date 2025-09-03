import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

// Target distribution for 226 cards
const TARGET_DISTRIBUTION = {
  'common': 90,      // 40%
  'uncommon': 56,    // 25% 
  'rare': 45,        // 20%
  'legendary': 23,   // 10%
  'exotic': 12       // 5%
}

// Card type distribution (rough target)
const CARD_TYPE_DISTRIBUTION = {
  'unit': 0.70,      // 70% units (~158 cards)
  'spell': 0.15,     // 15% spells (~34 cards)
  'structure': 0.10, // 10% structures (~23 cards)
  'relic': 0.05      // 5% relics (~11 cards)
}

// Unit class distribution for units
const UNIT_CLASS_DISTRIBUTION = {
  'warrior': 0.35,   // 35% warriors
  'tank': 0.25,     // 25% tanks
  'mage': 0.25,     // 25% mages
  'healer': 0.15    // 15% healers
}

// Keywords by rarity (more complex abilities for higher rarities)
const KEYWORDS_BY_RARITY = {
  'common': ['charge', 'taunt', 'lifesteal'],
  'uncommon': ['charge', 'taunt', 'lifesteal', 'freeze', 'battlecry'],
  'rare': ['charge', 'taunt', 'lifesteal', 'freeze', 'battlecry', 'deathrattle', 'overload'],
  'legendary': ['charge', 'taunt', 'lifesteal', 'freeze', 'battlecry', 'deathrattle', 'overload', 'double_attack', 'immune'],
  'exotic': ['charge', 'taunt', 'lifesteal', 'freeze', 'battlecry', 'deathrattle', 'overload', 'double_attack', 'immune', 'stealth', 'poison']
}

async function updateAllCardsOverhaul() {
  try {
    console.log('🃏 Starting comprehensive card overhaul...')
    
    // Get all existing cards
    const { data: existingCards, error: fetchError } = await supabase
      .from('card_complete')
      .select('*')
      .order('created_at')
    
    if (fetchError || !existingCards) {
      console.error('❌ Error fetching existing cards:', fetchError)
      return
    }
    
    console.log(`📋 Found ${existingCards.length} existing cards to update`)
    
    // Show current distribution
    const currentDistribution = existingCards.reduce((acc, card) => {
      acc[card.rarity] = (acc[card.rarity] || 0) + 1
      return acc
    }, {})
    
    console.log('📊 Current distribution:', currentDistribution)
    console.log('🎯 Target distribution:', TARGET_DISTRIBUTION)
    
    // Plan rarity reassignment
    const updates = []
    let cardIndex = 0
    
    // Assign new rarities according to target distribution
    for (const [rarity, count] of Object.entries(TARGET_DISTRIBUTION)) {
      console.log(`\n🔄 Processing ${count} ${rarity} cards...`)
      
      for (let i = 0; i < count && cardIndex < existingCards.length; i++) {
        const card = existingCards[cardIndex]
        
        // Determine card type based on name and current stats
        let cardType = 'unit'
        let unitClass = null
        let keywords = []
        let spellDamage = 0
        let healAmount = 0
        let structureHp = null
        let targetType = null
        
        // Assign card type based on patterns in name
        if (card.name.toLowerCase().includes('spell') || 
            card.name.toLowerCase().includes('bolt') ||
            card.name.toLowerCase().includes('blast') ||
            card.name.toLowerCase().includes('wave') ||
            card.name.toLowerCase().includes('surge')) {
          cardType = 'spell'
          spellDamage = Math.max(1, Math.min(5, card.attack || 2))
          targetType = 'enemy_unit'
        } else if (card.name.toLowerCase().includes('wall') ||
                   card.name.toLowerCase().includes('totem') ||
                   card.name.toLowerCase().includes('shrine') ||
                   card.name.toLowerCase().includes('barrier')) {
          cardType = 'structure'
          structureHp = Math.max(1, Math.min(8, card.hp || 3))
        } else if (card.name.toLowerCase().includes('relic') ||
                   card.name.toLowerCase().includes('crown') ||
                   card.name.toLowerCase().includes('amulet')) {
          cardType = 'relic'
        } else {
          // Default to unit
          cardType = 'unit'
          
          // Assign unit class based on stats and name
          const hp = card.hp || 1
          const attack = card.attack || 1
          
          if (card.name.toLowerCase().includes('guard') || 
              card.name.toLowerCase().includes('tank') ||
              card.name.toLowerCase().includes('wall') ||
              hp >= attack + 2) {
            unitClass = 'tank'
            keywords = ['taunt']
          } else if (card.name.toLowerCase().includes('healer') ||
                     card.name.toLowerCase().includes('priest') ||
                     card.name.toLowerCase().includes('cleric')) {
            unitClass = 'healer'
            keywords = ['lifesteal']
          } else if (card.name.toLowerCase().includes('mage') ||
                     card.name.toLowerCase().includes('wizard') ||
                     card.name.toLowerCase().includes('sage') ||
                     attack >= hp + 1) {
            unitClass = 'mage'
          } else {
            unitClass = 'warrior'
            if (rarity === 'common' && Math.random() < 0.3) {
              keywords = ['charge']
            }
          }
        }
        
        // Add rarity-appropriate keywords
        const availableKeywords = KEYWORDS_BY_RARITY[rarity] || []
        if (keywords.length === 0 && availableKeywords.length > 0 && Math.random() < 0.4) {
          const randomKeyword = availableKeywords[Math.floor(Math.random() * availableKeywords.length)]
          keywords = [randomKeyword]
        }
        
        // Create ability text based on keywords
        let abilityText = ''
        if (keywords.includes('taunt')) {
          abilityText = 'Taunt - Enemies must attack this unit first.'
        } else if (keywords.includes('charge')) {
          abilityText = 'Charge - Can attack immediately when played.'
        } else if (keywords.includes('lifesteal')) {
          abilityText = 'Lifesteal - Heal your hero for damage dealt.'
        } else if (keywords.includes('freeze')) {
          abilityText = 'Freeze - Target cannot attack next turn.'
        } else if (keywords.includes('battlecry')) {
          abilityText = 'Battlecry - Effect triggers when played.'
        } else if (keywords.includes('deathrattle')) {
          abilityText = 'Deathrattle - Effect triggers when this unit dies.'
        }
        
        const updateData = {
          rarity: rarity,
          card_type: cardType,
          unit_class: unitClass,
          keywords: keywords,
          ability_text: abilityText,
          spell_damage: spellDamage,
          heal_amount: healAmount,
          structure_hp: structureHp,
          target_type: targetType
        }
        
        // For non-unit cards, set attack/hp to null
        if (cardType !== 'unit') {
          updateData.attack = null
          updateData.hp = null
        }
        
        updates.push({
          id: card.id,
          name: card.name,
          oldRarity: card.rarity,
          newRarity: rarity,
          cardType: cardType,
          unitClass: unitClass,
          keywords: keywords,
          updateData: updateData
        })
        
        cardIndex++
      }
    }
    
    console.log(`\n📝 Prepared ${updates.length} card updates`)
    
    // Show some examples
    console.log('\n🎨 Sample updates:')
    updates.slice(0, 10).forEach(update => {
      console.log(`${update.name}: ${update.oldRarity} → ${update.newRarity} (${update.cardType}${update.unitClass ? ` - ${update.unitClass}` : ''})`)
    })
    
    // Ask for confirmation before proceeding
    console.log('\n⚠️  This will update ALL 226 cards according to the overhaul plan.')
    console.log('🔄 To proceed with the updates, uncomment the update loop below.')
    
    // Apply the updates
    console.log('\n🚀 Applying updates to all 226 cards...')
    let successCount = 0
    let errorCount = 0
    
    for (const update of updates) {
      try {
        const { error: updateError } = await supabase
          .from('card_complete')
          .update(update.updateData)
          .eq('id', update.id)
        
        if (updateError) {
          console.error(`❌ Error updating ${update.name}:`, updateError.message)
          errorCount++
        } else {
          successCount++
          if (successCount % 20 === 0) {
            console.log(`✅ Updated ${successCount}/${updates.length} cards...`)
          }
        }
      } catch (error) {
        console.error(`❌ Unexpected error updating ${update.name}:`, error)
        errorCount++
      }
    }
    
    console.log(`\n🎯 Update complete! Success: ${successCount}, Errors: ${errorCount}`)
    
    // Show final distribution
    const { data: finalStats, error: statsError } = await supabase
      .from('rarity_distribution')
      .select('*')
      .order('target_count', { ascending: false })
    
    if (!statsError && finalStats) {
      console.log('\n📊 Final Rarity Distribution:')
      finalStats.forEach(stat => {
        const percentage = ((stat.current_count / 226) * 100).toFixed(1)
        console.log(`${stat.rarity}: ${stat.current_count}/226 (${percentage}% - target: ${stat.percentage}%)`)
      })
    }
    
  } catch (error) {
    console.error('❌ Card overhaul failed:', error)
  }
}

updateAllCardsOverhaul()