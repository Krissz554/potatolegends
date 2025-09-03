# Supabase Database Configuration

This folder contains all Supabase-related database configurations for the Potato Personality Generator.

## 📁 Folder Structure

### `/migrations/`
Contains SQL migration files for database schema setup and updates.

- **`001_initial_setup.sql`** - ⚠️ Complex setup (has foreign key issues)
- **`002_basic_auth_only.sql`** - ✅ **RUN FIRST** - Simple user authentication setup
  - Basic user profiles table
  - Row Level Security policies
  - Automatic profile creation on signup
  - Performance indexes
  - Clean, working authentication system
- **`003_potato_collections.sql`** - ✅ **RUN SECOND** - Potato collection system
  - User potato collections table
  - Filtering and search capabilities
  - Favorites and statistics tracking
  - Full RLS security for user privacy
- **`004_potato_registry_table.sql`** - ✅ **RUN THIRD** - Complete potato registry database
  - Stores all discoverable potatoes in database
  - Proper unlock tracking with user relationships
  - Optimized queries with views and indexes
  - Replaces client-side registry generation
- **`005_fix_potato_registry_rls.sql`** - ✅ **RUN FOURTH** - Fix RLS policies for seeding
  - Fixes Row Level Security policies that block seeding
  - Allows authenticated users to populate registry
  - Maintains security while enabling proper functionality
  - Required for database seeding to work
- **`006_add_potato_stats.sql`** - ✅ **RUN FIFTH** - Add trading card battle stats
  - Adds HP and Attack columns to potato registry
  - Includes proper constraints and indexes for stats
  - Enables trading card game mechanics
  - Deterministic stat generation based on rarity
- **`007_battle_system.sql`** - ✅ **RUN SIXTH** - Battle system foundation
  - Creates battle_decks, matchmaking_queue, battle_sessions tables
  - Real-time subscriptions for multiplayer features
  - Battle action tracking and game state management
  - Complete PvP battle infrastructure
- **`008_multiple_deck_slots.sql`** - ✅ **RUN SEVENTH** - Multiple deck slots
  - Adds support for 3 different deck slots per user
  - User deck settings for active deck selection
  - Enhanced deck management capabilities
  - Improved battle preparation system
- **`009_server_side_matchmaking_safe.sql`** - ✅ **RUN EIGHTH** - Server-side matchmaking (Safe)
  - Automatic matchmaking triggers and functions (without pg_net dependency)
  - Safe migration that handles existing objects without conflicts
  - Queue cleanup and timeout handling
  - Matchmaking logs for debugging and monitoring
  - Complete server-side battle matching system
- **`010_fix_matchmaking_foreign_keys.sql`** - ✅ **RUN NINTH** - Fix foreign key references
  - Fixes foreign key constraints to reference auth.users instead of user_profiles
  - Resolves 400 Bad Request errors when joining matchmaking queue
  - Essential fix for proper authentication integration
- **`011_fix_matchmaking_rls_for_service_role.sql`** - ✅ **RUN TENTH** - Fix RLS for service role
  - Allows edge function service role to see all players in queue
  - Fixes "waiting: 1" issue when multiple players are searching
  - Essential for proper server-side matchmaking visibility
- **`012_comprehensive_matchmaking_fix.sql`** - ✅ **RUN ELEVENTH** - Comprehensive fix (REQUIRED)
  - Completely fixes RLS policies with proper service_role access
  - Cleans up stale battle sessions and queue entries
  - Uses explicit TO service_role policies for guaranteed access
  - Essential final fix for all matchmaking issues
- **`013_fix_realtime_battle_notifications_safe.sql`** - ✅ **RUN TWELFTH** - Fix real-time notifications (Safe)
  - Fixes RLS policies blocking real-time INSERT notifications
  - Safely handles existing realtime publication memberships
  - Allows users to receive battle session creation events
  - Essential for battle arena to open automatically
  - Fixes the "battle created but arena doesn't open" issue
- **`014_cleanup_stale_battles_and_improve_logic.sql`** - ✅ **RUN THIRTEENTH** - Cleanup stale battles (REQUIRED)
  - Removes stale battle sessions blocking new matches
  - Improves edge function logic to handle stale data
  - Adds emergency reset functions for stuck players
  - Fixes "Players already in active battle" when they're not
  - Essential for preventing matchmaking blockages

### `/functions/` 
Contains Supabase Edge Functions (serverless functions).

- **`matchmaker/`** - ✅ **Server-side matchmaking function**
  - Handles automatic player matching for battles
  - Validates deck completeness and player readiness
  - Creates battle sessions with proper game state
  - Includes comprehensive logging and error handling

Future edge functions ready for:
- Image generation endpoints
- Email notifications
- Analytics processing
- Third-party integrations

## 🚀 How to Apply Migrations

### Complete Setup (RECOMMENDED)
**Step 1: Basic Authentication**
1. Go to your Supabase Dashboard → SQL Editor
2. Copy the contents of `migrations/002_basic_auth_only.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Basic authentication setup completed successfully! 🥔✨"

**Step 2: Potato Collections**
1. In the same SQL Editor
2. Copy the contents of `migrations/003_potato_collections.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Potato collections table created successfully! 🥔✨"

**Step 3: Potato Registry Database**
1. In the same SQL Editor
2. Copy the contents of `migrations/004_potato_registry_table.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Potato registry database tables created successfully! 🥔📚"

**Step 4: Fix RLS Policies**
1. In the same SQL Editor
2. Copy the contents of `migrations/005_fix_potato_registry_rls.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Potato registry RLS policies fixed! Ready for seeding. 🥔🔓"

**Step 5: Add Battle Stats**
1. In the same SQL Editor
2. Copy the contents of `migrations/006_add_potato_stats.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify that HP and Attack columns are added to potato_registry table

**Step 6: Battle System Foundation**
1. In the same SQL Editor
2. Copy the contents of `migrations/007_battle_system.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Battle system tables and functions created successfully!"

**Step 7: Multiple Deck Slots**
1. In the same SQL Editor
2. Copy the contents of `migrations/008_multiple_deck_slots.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Multiple deck slots and user deck settings added successfully!"

**Step 8: Server-side Matchmaking**
1. In the same SQL Editor
2. Copy the contents of `migrations/009_server_side_matchmaking_safe.sql` (use the safe version)
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Server-side matchmaking system created successfully! (Safe Version - No Conflicts)"

**Step 9: Fix Foreign Key References**
1. In the same SQL Editor
2. Copy the contents of `migrations/010_fix_matchmaking_foreign_keys.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Matchmaking foreign key references fixed! Now references auth.users correctly."

**Step 10: Fix RLS for Service Role**
1. In the same SQL Editor
2. Copy the contents of `migrations/011_fix_matchmaking_rls_for_service_role.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Matchmaking RLS policies updated for service role access!"

**Step 11: Comprehensive Matchmaking Fix (REQUIRED)**
1. In the same SQL Editor
2. Copy the contents of `migrations/012_comprehensive_matchmaking_fix.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Comprehensive matchmaking fix applied! RLS policies updated and stale data cleaned."

**Step 12: Fix Real-time Battle Notifications (REQUIRED)**
1. In the same SQL Editor
2. Copy the contents of `migrations/013_fix_realtime_battle_notifications_safe.sql` (use the safe version)
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Real-time battle notifications fixed! (Safe Version - No Conflicts)"

**Step 13: Cleanup Stale Battles (REQUIRED)**
1. In the same SQL Editor
2. Copy the contents of `migrations/014_cleanup_stale_battles_and_improve_logic.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Stale battles cleaned up and matchmaking logic improved!"

**Step 14: Debug Real-time Policies**
1. In the same SQL Editor
2. Copy the contents of `migrations/015_debug_realtime_policies.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Real-time policies debugged and fixed!"

**Step 15: Emergency Queue Visibility Fix (CRITICAL)**
1. In the same SQL Editor
2. Copy the contents of `migrations/016_emergency_fix_queue_visibility.sql`
3. Paste into the SQL Editor
4. Click "Run" to execute
5. Verify success message: "Emergency queue visibility fix applied!"

**Step 16: Deploy Edge Functions**
1. Install Supabase CLI if not already installed
2. Run: `npx supabase functions deploy matchmaker`
3. Run: `npx supabase functions deploy test-queue` (for debugging)
4. Verify both functions are deployed in your Supabase Dashboard → Edge Functions

### Alternative (Complex Setup - Has Issues)
If you want the full-featured setup, use `001_initial_setup.sql` but be aware it may have foreign key constraint issues.

### Future Migrations
When adding new migrations:
1. Create files with incremental numbers: `002_feature_name.sql`, `003_update_name.sql`, etc.
2. Always include rollback instructions in comments
3. Test on development database first
4. Document changes in this README

## 🗄️ Database Schema Overview

### Core Tables
- **`user_profiles`** - Extended user information and stats
- **`potato_collections`** - Saved potato personalities
- **`potato_shares`** - Sharing analytics and tracking
- **`potato_ratings`** - Community rating system
- **`user_achievements`** - Gamification and milestones

### Security
- Row Level Security (RLS) enabled on all tables
- Users can only access their own data
- Secure by default with comprehensive policies

### Performance
- Strategic indexes for fast queries
- Optimized for mobile app performance
- Efficient data relationships
- Server-side matchmaking with automatic triggers

## 🔧 Development Notes

- All tables use UUID primary keys for security
- Automatic user profile creation on signup
- Real-time achievement system
- Comprehensive analytics tracking
- Server-side matchmaking for real-time battles
- Future-ready for social features

## 📊 Views Available

- **`user_dashboard_stats`** - Complete user statistics
- **`popular_potatoes`** - Most shared and favorited potatoes

Ready for production use! 🥔✨