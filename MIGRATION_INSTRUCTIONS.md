# Migration from `cards` to `card_complete` Table

## Summary
Your project has two separate card systems:
- **Old system**: `cards` table (41 cards) - basic card game cards
- **New system**: `card_complete` table (226 cards) - potato-based cards with rich data

The systems are incompatible, so we need a clean migration.

## Current Status ✅
- **Edge Function updated**: `start-match/index.ts` now uses `card_complete`
- **App already uses new system**: `newCollectionService.ts` uses `card_complete`
- **Migration script ready**: Cleanup script created

## Manual Steps Required

### 1. Run SQL Cleanup (Required)
**ISSUE FOUND**: The original migration fails due to a view dependency.

**SOLUTION**: Use the corrected migration script:
- Execute `safe_cleanup_cards_migration.sql` in your Supabase SQL Editor
- OR use `082_fixed_migrate_from_cards_to_card_complete.sql` 

This properly handles the `deck_cards_with_cards` view dependency.

### 2. Verify Changes
After running the SQL:
- Confirm `cards` table is deleted
- Confirm `deck_cards` is empty
- Confirm `card_complete` still has 226 cards

### 3. User Impact
- **Existing decks**: Will be empty, users need to rebuild them
- **Card collection**: Uses the new potato card system (226 cards)
- **Battle system**: Will use the new card system

## Files Updated ✅
- `supabase/functions/start-match/index.ts` - Now uses `card_complete`
- `src/lib/newCollectionService.ts` - Already used `card_complete`

## Benefits of Migration
- Single card system using rich potato card data
- 226 cards vs 41 cards (5x more content)
- Rich metadata: pixel art, abilities, effects, rarities
- Consistent data structure throughout application

## Next Steps After SQL Cleanup
1. Test deck building with new cards
2. Test battle system with new cards
3. Notify users about deck reset (optional)
4. Consider providing starter decks with new cards