import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

// Sample cards from the overhaul plan
const sampleCards = [
  // Common Ice Cards
  {
    registry_id: 'ice_frozen_sprout_001',
    name: 'Frozen Sprout',
    description: 'A young potato that has embraced the cold.',
    potato_type: 'ice',
    trait: 'frozen',
    adjective: 'Frozen',
    rarity: 'common',
    card_type: 'unit',
    unit_class: 'tank',
    mana_cost: 1,
    attack: 1,
    hp: 3,
    keywords: ['taunt'],
    ability_text: 'Taunt - Enemies must attack this unit first.',
    flavor_text: 'Small but stubborn, like an icy doorstop.',
    generation_seed: 'ice_frozen_sprout_001_seed'
  },
  {
    registry_id: 'ice_frost_peeler_002',
    name: 'Frost Peeler',
    description: 'A hardy ice warrior with sharp edges.',
    potato_type: 'ice',
    trait: 'sharp',
    adjective: 'Frost',
    rarity: 'common',
    card_type: 'unit',
    unit_class: 'warrior',
    mana_cost: 2,
    attack: 2,
    hp: 2,
    keywords: [],
    ability_text: '',
    flavor_text: 'Peels enemies like vegetables.',
    generation_seed: 'ice_frost_peeler_002_seed'
  },
  {
    registry_id: 'ice_chip_spell_003',
    name: 'Ice Chip',
    description: 'A small shard of pure cold.',
    potato_type: 'ice',
    trait: 'frozen',
    adjective: 'Icy',
    rarity: 'common',
    card_type: 'spell',
    unit_class: null,
    mana_cost: 1,
    attack: null,
    hp: null,
    spell_damage: 0,
    heal_amount: 0,
    target_type: 'enemy_unit',
    keywords: ['freeze'],
    ability_text: 'Freeze target enemy unit.',
    flavor_text: 'One small chip can stop a giant.',
    generation_seed: 'ice_chip_spell_003_seed'
  },
  
  // Common Fire Cards
  {
    registry_id: 'fire_charred_spudling_004',
    name: 'Charred Spudling',
    description: 'A small but fierce fire warrior.',
    potato_type: 'fire',
    trait: 'charred',
    adjective: 'Charred',
    rarity: 'common',
    card_type: 'unit',
    unit_class: 'warrior',
    mana_cost: 2,
    attack: 2,
    hp: 1,
    keywords: ['charge'],
    ability_text: 'Charge - Can attack immediately when played.',
    flavor_text: 'Burns bright and fast.',
    generation_seed: 'fire_charred_spudling_004_seed'
  },
  {
    registry_id: 'fire_potato_bomb_005',
    name: 'Potato Bomb',
    description: 'An explosive spell that damages all enemies.',
    potato_type: 'fire',
    trait: 'explosive',
    adjective: 'Explosive',
    rarity: 'common',
    card_type: 'spell',
    unit_class: null,
    mana_cost: 2,
    attack: null,
    hp: null,
    spell_damage: 2,
    heal_amount: 0,
    target_type: 'all_enemies',
    keywords: [],
    ability_text: 'Deal 2 damage to all enemy units.',
    flavor_text: 'When potatoes go boom.',
    generation_seed: 'fire_potato_bomb_005_seed'
  },
  
  // Legendary Examples
  {
    registry_id: 'ice_cryoknight_taterius_legendary',
    name: 'Cryoknight Taterius',
    description: 'The legendary ice knight who freezes all who dare attack.',
    potato_type: 'ice',
    trait: 'legendary',
    adjective: 'Cryoknight',
    rarity: 'legendary',
    card_type: 'unit',
    unit_class: 'warrior',
    mana_cost: 6,
    attack: 6,
    hp: 8,
    keywords: ['taunt', 'freeze'],
    ability_text: 'Taunt. Whenever this unit is attacked, freeze the attacker.',
    flavor_text: 'The eternal guardian of the frozen realm.',
    generation_seed: 'ice_cryoknight_taterius_legendary_seed'
  },
  
  // Exotic Example
  {
    registry_id: 'exotic_molten_frostbite_colossus',
    name: 'Molten Frostbite Colossus',
    description: 'A massive hybrid of ice and fire elements.',
    potato_type: 'ice', // Primary type
    trait: 'hybrid',
    adjective: 'Molten Frostbite',
    rarity: 'exotic',
    card_type: 'unit',
    unit_class: 'tank',
    mana_cost: 8,
    attack: 8,
    hp: 12,
    keywords: ['taunt', 'freeze', 'lifesteal'],
    ability_text: 'Taunt. When attacked, freeze the attacker and deal 2 damage to them.',
    flavor_text: 'Born from the clash of opposing elements.',
    generation_seed: 'exotic_molten_frostbite_colossus_seed'
  }
]

async function generateCardOverhaul() {
  try {
    console.log('🃏 Starting card overhaul generation...')
    
    // Get existing cards to update
    console.log('📊 Analyzing existing 226 cards...')
    const { data: existingCards, error: fetchError } = await supabase
      .from('card_complete')
      .select('id, registry_id, name, rarity, potato_type')
      .order('created_at')
    
    if (fetchError || !existingCards) {
      console.error('❌ Error fetching existing cards:', fetchError)
      return
    }
    
    console.log(`📋 Found ${existingCards.length} existing cards to update`)
    
    // Analyze current distribution
    const currentDistribution = existingCards.reduce((acc, card) => {
      acc[card.rarity] = (acc[card.rarity] || 0) + 1
      return acc
    }, {})
    
    console.log('📊 Current distribution:', currentDistribution)
    
    // Plan the updates based on target distribution
    const targetDistribution = {
      'common': 90,
      'uncommon': 56, 
      'rare': 45,
      'legendary': 23,
      'exotic': 12
    }
    
    console.log('🎯 Target distribution:', targetDistribution)
    
    // For now, just update a few sample cards to test the new schema
    console.log('🔄 Updating sample cards with new schema...')
    
    // Update first few cards as examples
    if (existingCards.length > 0) {
      const updates = [
        {
          id: existingCards[0].id,
          card_type: 'unit',
          unit_class: 'tank',
          keywords: ['taunt'],
          ability_text: 'Taunt - Enemies must attack this unit first.'
        },
        {
          id: existingCards[1].id,
          card_type: 'unit', 
          unit_class: 'warrior',
          keywords: ['charge'],
          ability_text: 'Charge - Can attack immediately when played.'
        }
      ]
      
      for (const update of updates) {
        const { id, ...updateData } = update
        const { error: updateError } = await supabase
          .from('card_complete')
          .update(updateData)
          .eq('id', id)
        
        if (updateError) {
          console.error('❌ Error updating card:', updateError)
        } else {
          console.log('✅ Updated card:', update.id)
        }
      }
    }
    
    // Check rarity distribution
    const { data: rarityStats, error: statsError } = await supabase
      .from('rarity_distribution')
      .select('*')
      .order('target_count', { ascending: false })
    
    if (!statsError && rarityStats) {
      console.log('\n📊 Current Rarity Distribution:')
      rarityStats.forEach(stat => {
        console.log(`${stat.rarity}: ${stat.current_count}/${stat.target_count} (${stat.percentage}%)`)
      })
    }
    
    console.log('\n🎯 Card overhaul sample generation complete!')
    console.log('Next steps:')
    console.log('1. Run the database migration: npx supabase db push')
    console.log('2. Deploy updated edge functions')
    console.log('3. Generate remaining cards to reach 226 total')
    
  } catch (error) {
    console.error('❌ Card overhaul generation failed:', error)
  }
}

generateCardOverhaul()