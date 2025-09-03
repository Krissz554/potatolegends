import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function runMigration() {
  try {
    console.log('🔧 Running card constraint fixes...')
    
    // Fix attack constraint
    console.log('1. Fixing attack constraint...')
    await supabase.rpc('exec', {
      sql: `
        ALTER TABLE public.card_complete 
        DROP CONSTRAINT IF EXISTS card_complete_attack_check;
        
        ALTER TABLE public.card_complete 
        ADD CONSTRAINT card_complete_attack_check 
        CHECK (
          (card_type = 'unit' AND attack >= 0 AND attack <= 15) OR 
          (card_type IN ('spell', 'structure', 'relic') AND (attack IS NULL OR attack = 0))
        );
      `
    })
    
    // Fix HP constraint
    console.log('2. Fixing HP constraint...')
    await supabase.rpc('exec', {
      sql: `
        ALTER TABLE public.card_complete 
        DROP CONSTRAINT IF EXISTS card_complete_hp_check;
        
        ALTER TABLE public.card_complete 
        ADD CONSTRAINT card_complete_hp_check 
        CHECK (
          (card_type = 'unit' AND hp >= 1 AND hp <= 20) OR 
          (card_type = 'structure' AND structure_hp >= 1 AND structure_hp <= 15) OR 
          (card_type IN ('spell', 'relic') AND (hp IS NULL OR hp = 0))
        );
      `
    })
    
    console.log('✅ Card constraints fixed!')
    
  } catch (error) {
    console.error('❌ Migration failed:', error)
  }
}

runMigration()