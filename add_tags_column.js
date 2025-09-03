import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://xsknbbvyagngljxkftkd.supabase.co'
const supabaseServiceKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q'

const supabase = createClient(supabaseUrl, supabaseServiceKey)

async function addTagsColumn() {
  try {
    console.log('🔧 Adding tags column to card_complete table...')
    
    // Check if tags column already exists
    const { data: columns, error: columnError } = await supabase
      .from('information_schema.columns')
      .select('column_name')
      .eq('table_name', 'card_complete')
      .eq('table_schema', 'public')
    
    if (columnError) {
      console.log('⚠️ Could not check existing columns, proceeding with add...')
    } else {
      const hasTagsColumn = columns.some(col => col.column_name === 'tags')
      if (hasTagsColumn) {
        console.log('✅ Tags column already exists!')
        return
      }
    }
    
    console.log('📝 Adding tags column...')
    
    // You'll need to run this SQL in your Supabase Dashboard → SQL Editor:
    const sqlCommand = `
-- Add tags column before ability_text
ALTER TABLE public.card_complete 
ADD COLUMN IF NOT EXISTS tags TEXT[] DEFAULT ARRAY[]::TEXT[];

-- Add comment to explain the column
COMMENT ON COLUMN public.card_complete.tags IS 'Keyword tags like ["Taunt", "Charge", "Battlecry"] - displayed as badges on cards';
COMMENT ON COLUMN public.card_complete.ability_text IS 'Detailed ability description - displayed as card text';
`
    
    console.log('🎯 Run this SQL in your Supabase Dashboard → SQL Editor:')
    console.log('')
    console.log(sqlCommand)
    console.log('')
    
    console.log('📋 Example usage for your Eternal Stormborn Colossus:')
    console.log('  tags: ["Taunt", "Charge", "Battlecry"]')
    console.log('  ability_text: "Deal 3 damage to all enemies and Freeze one of them."')
    
  } catch (error) {
    console.error('❌ Failed to add tags column:', error)
  }
}

addTagsColumn()