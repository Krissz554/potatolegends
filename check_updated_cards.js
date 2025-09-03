import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function checkUpdatedCards() {
  try {
    console.log('🃏 Checking your updated card_complete table...')
    
    // Get overall stats
    const { data: allCards, error: fetchError } = await supabase
      .from('card_complete')
      .select('*')
    
    if (fetchError) {
      console.error('❌ Error fetching cards:', fetchError)
      return
    }
    
    console.log(`📊 Total cards: ${allCards.length}`)
    
    // Analyze rarity distribution
    const rarityDist = allCards.reduce((acc, card) => {
      acc[card.rarity] = (acc[card.rarity] || 0) + 1
      return acc
    }, {})
    
    console.log('\n📈 Rarity Distribution:')
    console.log(`  Common: ${rarityDist.common || 0}/90 (${((rarityDist.common || 0) / allCards.length * 100).toFixed(1)}%)`)
    console.log(`  Uncommon: ${rarityDist.uncommon || 0}/56 (${((rarityDist.uncommon || 0) / allCards.length * 100).toFixed(1)}%)`)
    console.log(`  Rare: ${rarityDist.rare || 0}/45 (${((rarityDist.rare || 0) / allCards.length * 100).toFixed(1)}%)`)
    console.log(`  Legendary: ${rarityDist.legendary || 0}/23 (${((rarityDist.legendary || 0) / allCards.length * 100).toFixed(1)}%)`)
    console.log(`  Exotic: ${rarityDist.exotic || 0}/12 (${((rarityDist.exotic || 0) / allCards.length * 100).toFixed(1)}%)`)
    
    // Analyze card types
    const typeDist = allCards.reduce((acc, card) => {
      acc[card.card_type || 'unit'] = (acc[card.card_type || 'unit'] || 0) + 1
      return acc
    }, {})
    
    console.log('\n🎨 Card Type Distribution:')
    Object.entries(typeDist).forEach(([type, count]) => {
      console.log(`  ${type}: ${count} (${(count / allCards.length * 100).toFixed(1)}%)`)
    })
    
    // Analyze unit classes (for units only)
    const units = allCards.filter(card => card.card_type === 'unit')
    const classDist = units.reduce((acc, card) => {
      acc[card.unit_class || 'unknown'] = (acc[card.unit_class || 'unknown'] || 0) + 1
      return acc
    }, {})
    
    console.log('\n⚔️ Unit Class Distribution:')
    Object.entries(classDist).forEach(([unitClass, count]) => {
      console.log(`  ${unitClass}: ${count} (${(count / units.length * 100).toFixed(1)}%)`)
    })
    
    // Analyze potato types
    const elementDist = allCards.reduce((acc, card) => {
      acc[card.potato_type] = (acc[card.potato_type] || 0) + 1
      return acc
    }, {})
    
    console.log('\n🥔 Element Distribution:')
    Object.entries(elementDist).forEach(([element, count]) => {
      console.log(`  ${element}: ${count} (${(count / allCards.length * 100).toFixed(1)}%)`)
    })
    
    // Show some example cards from each type
    console.log('\n🎯 Sample Cards by Type:')
    
    const samplesByType = {}
    allCards.forEach(card => {
      const type = card.card_type || 'unit'
      if (!samplesByType[type]) samplesByType[type] = []
      if (samplesByType[type].length < 3) {
        samplesByType[type].push(card)
      }
    })
    
    Object.entries(samplesByType).forEach(([type, cards]) => {
      console.log(`\n  ${type.toUpperCase()}S:`)
      cards.forEach(card => {
        const stats = card.card_type === 'unit' 
          ? `${card.attack || 0}/${card.hp || 0}` 
          : card.card_type === 'spell' 
            ? `Spell Damage: ${card.spell_damage || 0}`
            : card.card_type === 'structure'
              ? `Structure HP: ${card.structure_hp || 0}`
              : 'Relic'
        
        console.log(`    ${card.name} (${card.rarity}) - ${stats}`)
        if (card.tags && card.tags.length > 0) {
          console.log(`      Tags: ${JSON.stringify(card.tags)}`)
        }
        if (card.ability_text) {
          console.log(`      Ability: ${card.ability_text.substring(0, 60)}${card.ability_text.length > 60 ? '...' : ''}`)
        }
      })
    })
    
    // Check for cards with interesting abilities
    const cardsWithAbilities = allCards.filter(card => 
      card.ability_text && card.ability_text.length > 10
    )
    
    console.log(`\n✨ Cards with abilities: ${cardsWithAbilities.length}/${allCards.length}`)
    
    if (cardsWithAbilities.length > 0) {
      console.log('\n🎮 Sample abilities:')
      cardsWithAbilities.slice(0, 5).forEach(card => {
        console.log(`  ${card.name}: "${card.ability_text}"`)
      })
    }
    
  } catch (error) {
    console.error('❌ Check failed:', error)
  }
}

checkUpdatedCards()