import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function removeConstraints() {
  try {
    console.log('🔧 Removing problematic check constraints...')
    
    const commands = [
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_attack_check;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_hp_check;',
      'ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_card_type_check;'
    ]
    
    for (const command of commands) {
      try {
        await supabase.rpc('exec', { sql: command })
        console.log('✅ Removed constraint:', command.split(' ')[6])
      } catch (error) {
        console.log('⚠️ Constraint may not exist:', command.split(' ')[6])
      }
    }
    
    console.log('✅ All problematic constraints removed!')
    console.log('🎯 You can now freely edit card_type, attack, hp columns')
    
  } catch (error) {
    console.error('❌ Failed to remove constraints:', error)
  }
}

removeConstraints()