// Script to manually insert hero data
import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTYwNTg0MzIsImV4cCI6MjA3MTYzNDQzMn0.J8G45OdXxTbWuGO8N05_50qOVPq7BMSDo1xeegXQMW0'

const supabase = createClient(supabaseUrl, supabaseKey)

async function insertHeroes() {
  console.log('🦸 Inserting hero data...')
  
  const heroes = [
    {
      hero_id: 'potato_king',
      name: 'Potato King',
      description: 'The mighty ruler of all potatoes! A legendary hero with royal powers and supreme command over the potato realm.',
      base_hp: 30,
      base_mana: 1,
      hero_power_name: 'Royal Decree',
      hero_power_description: 'Deal 2 damage to any target, or restore 2 health to your hero.',
      hero_power_cost: 2,
      rarity: 'legendary',
      element_type: 'light',
      is_starter: true
    },
    {
      hero_id: 'fire_spud',
      name: 'Fire Spud Warrior',
      description: 'A fierce potato warrior wielding the power of flames. Burns enemies with fiery attacks.',
      base_hp: 30,
      base_mana: 1,
      hero_power_name: 'Flame Burst',
      hero_power_description: 'Deal 2 damage to an enemy. If it dies, deal 1 damage to all enemies.',
      hero_power_cost: 2,
      rarity: 'rare',
      element_type: 'fire',
      is_starter: false
    },
    {
      hero_id: 'ice_tater',
      name: 'Ice Tater Guardian',
      description: 'A cool and collected potato guardian with mastery over ice magic.',
      base_hp: 30,
      base_mana: 1,
      hero_power_name: 'Frost Shield',
      hero_power_description: 'Gain 5 armor. Draw a card.',
      hero_power_cost: 2,
      rarity: 'rare',
      element_type: 'ice',
      is_starter: false
    },
    {
      hero_id: 'lightning_tuber',
      name: 'Lightning Tuber',
      description: 'A high-energy potato crackling with electrical power.',
      base_hp: 25,
      base_mana: 1,
      hero_power_name: 'Chain Lightning',
      hero_power_description: 'Deal 3 damage to an enemy. If you have 5+ mana, deal 1 damage to all enemies.',
      hero_power_cost: 3,
      rarity: 'epic',
      element_type: 'lightning',
      is_starter: false
    },
    {
      hero_id: 'light_potato',
      name: 'Radiant Light Potato',
      description: 'A holy potato blessed with divine light energy.',
      base_hp: 35,
      base_mana: 1,
      hero_power_name: 'Holy Light',
      hero_power_description: 'Restore 4 health to your hero and draw a card.',
      hero_power_cost: 2,
      rarity: 'epic',
      element_type: 'light',
      is_starter: false
    }
  ]
  
  try {
    const { data, error } = await supabase
      .from('heroes')
      .upsert(heroes, { 
        onConflict: 'hero_id',
        ignoreDuplicates: false 
      })
      .select()
    
    if (error) {
      console.error('❌ Error inserting heroes:', error)
      return
    }
    
    console.log('✅ Heroes inserted successfully!')
    console.log(`📊 Inserted ${data?.length || heroes.length} heroes`)
    
    // Show the heroes
    data?.forEach(hero => {
      console.log(`  👑 ${hero.name} (${hero.hero_id}) - ${hero.rarity} ${hero.element_type}`)
    })
    
    // Now try to unlock starter heroes for existing users
    console.log('\n🔓 Checking if we can unlock starter heroes for users...')
    
    // Get a list of users
    const { data: profiles, error: profileError } = await supabase
      .from('user_profiles')
      .select('id')
      .limit(5)
    
    if (profileError) {
      console.log('❌ Could not access user profiles:', profileError.message)
      return
    }
    
    console.log(`👥 Found ${profiles?.length || 0} users`)
    
    if (profiles && profiles.length > 0) {
      console.log('💡 You can now manually unlock starter heroes by:')
      console.log('   1. Opening the Hero Hall in the app')
      console.log('   2. Clicking "Unlock Starter Heroes" button')
      console.log('   3. This will give users the Potato King hero!')
    }
    
  } catch (error) {
    console.error('❌ Unexpected error:', error)
  }
}

insertHeroes().catch(console.error)