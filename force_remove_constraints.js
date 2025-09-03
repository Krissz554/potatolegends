import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function forceRemoveConstraints() {
  try {
    console.log('🔧 Force removing ALL check constraints from card_complete...')
    
    // First, let's see what constraints exist by querying the pg_constraint table directly
    const { data: constraints, error: queryError } = await supabase
      .rpc('sql', { 
        query: `
          SELECT conname as constraint_name 
          FROM pg_constraint 
          WHERE conrelid = 'public.card_complete'::regclass 
          AND contype = 'c';
        `
      })
    
    if (queryError) {
      console.log('⚠️ Could not query constraints, trying direct drops...')
    } else {
      console.log('📋 Found constraints:', constraints)
    }
    
    // Try multiple ways to drop the constraint
    const dropAttempts = [
      "ALTER TABLE public.card_complete DROP CONSTRAINT card_complete_attack_check;",
      "ALTER TABLE public.card_complete DROP CONSTRAINT IF EXISTS card_complete_attack_check;",
      "ALTER TABLE card_complete DROP CONSTRAINT card_complete_attack_check;",
      "ALTER TABLE card_complete DROP CONSTRAINT IF EXISTS card_complete_attack_check;"
    ]
    
    for (const command of dropAttempts) {
      try {
        console.log(`Trying: ${command}`)
        const { error } = await supabase.rpc('sql', { query: command })
        if (error) {
          console.log(`  ❌ ${error.message}`)
        } else {
          console.log(`  ✅ Success!`)
          break
        }
      } catch (error) {
        console.log(`  ❌ ${error.message}`)
      }
    }
    
    console.log('\n🎯 Try editing your card now!')
    
  } catch (error) {
    console.error('❌ Failed to remove constraints:', error)
  }
}

forceRemoveConstraints()