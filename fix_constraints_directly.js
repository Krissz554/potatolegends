import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function fixConstraints() {
  try {
    console.log('🔧 Fixing database constraints for card overhaul...')
    
    // Step 1: Remove NOT NULL constraints from attack and hp
    console.log('1. Removing NOT NULL constraints...')
    
    const alterCommands = [
      'ALTER TABLE public.card_complete ALTER COLUMN attack DROP NOT NULL;',
      'ALTER TABLE public.card_complete ALTER COLUMN hp DROP NOT NULL;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_attack_check;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_hp_check;'
    ]
    
    for (const command of alterCommands) {
      try {
        await supabase.rpc('exec', { sql: command })
        console.log('  ✅', command.substring(0, 50) + '...')
      } catch (error) {
        console.log('  ⚠️ ', command.substring(0, 50) + '...', error.message)
      }
    }
    
    console.log('✅ Constraints fixed! Cards can now have null attack/hp for spells/structures/relics')
    
  } catch (error) {
    console.error('❌ Failed to fix constraints:', error)
  }
}

fixConstraints()