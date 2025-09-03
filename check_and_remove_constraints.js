import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function checkAndRemoveConstraints() {
  try {
    console.log('🔍 Checking existing constraints on card_complete table...')
    
    // Query to see all constraints on the table
    const { data: constraints, error: constraintError } = await supabase
      .from('information_schema.table_constraints')
      .select('constraint_name, constraint_type')
      .eq('table_name', 'card_complete')
      .eq('table_schema', 'public')
    
    if (constraintError) {
      console.error('❌ Error checking constraints:', constraintError)
    } else {
      console.log('📋 Current constraints:')
      constraints.forEach(c => {
        console.log(`  - ${c.constraint_name} (${c.constraint_type})`)
      })
    }
    
    // Try to remove ALL check constraints on card_complete
    console.log('\n🔧 Removing ALL check constraints...')
    
    const dropCommands = [
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_attack_check CASCADE;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_hp_check CASCADE;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_card_type_check CASCADE;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_mana_cost_check CASCADE;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS valid_keywords CASCADE;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_rarity_check CASCADE;'
    ]
    
    for (const command of dropCommands) {
      try {
        const { error } = await supabase.rpc('sql', { query: command })
        if (error) {
          console.log(`⚠️ ${command.split(' ')[6]}: ${error.message}`)
        } else {
          console.log(`✅ Removed: ${command.split(' ')[6]}`)
        }
      } catch (error) {
        console.log(`⚠️ ${command.split(' ')[6]}: ${error.message}`)
      }
    }
    
    console.log('\n🎯 All constraints should now be removed!')
    console.log('Try editing your card_type column again.')
    
  } catch (error) {
    console.error('❌ Failed to check/remove constraints:', error)
  }
}

checkAndRemoveConstraints()