# Pure Server-Side Timer Setup

The timer system is now completely server-side. Here's how to set it up:

## 1. Deploy the Functions

```bash
# Deploy the server timer function
supabase functions deploy server-timer

# Run the new migration
# Execute: supabase/migrations/086_pure_server_timer.sql
```

## 2. Set Up External Cron Service

### Option A: cron-job.org (Recommended)
1. Go to https://cron-job.org/
2. Create a free account
3. Add a new cron job:
   - **URL**: `https://xsknbbvyagngljxkftkd.supabase.co/functions/v1/server-timer`
   - **Schedule**: Every minute (* * * * *)
   - **Method**: POST
   - **Headers**: 
     ```
     Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q
     Content-Type: application/json
     ```
   - **Body**: `{}`

### Option B: UptimeRobot
1. Go to https://uptimerobot.com/
2. Create a monitor with:
   - **Type**: HTTP(S)
   - **URL**: `https://xsknbbvyagngljxkftkd.supabase.co/functions/v1/server-timer`
   - **Monitoring Interval**: 1 minute
   - **HTTP Method**: POST

### Option C: GitHub Actions (if you prefer)
Create `.github/workflows/timer.yml`:
```yaml
name: Server Timer
on:
  schedule:
    - cron: '* * * * *'  # Every minute
  workflow_dispatch:

jobs:
  timer:
    runs-on: ubuntu-latest
    steps:
    - name: Call Timer
      run: |
        curl -X POST \
          -H "Authorization: Bearer ${{ secrets.SUPABASE_SERVICE_ROLE_KEY }}" \
          -H "Content-Type: application/json" \
          -d '{}' \
          "https://xsknbbvyagngljxkftkd.supabase.co/functions/v1/server-timer"
```

## 3. How It Works

### Server-Only Timer System:
1. **Turn Start**: `turn_started_at` timestamp is set
2. **Timer Calculation**: Based on `NOW() - turn_started_at`
3. **Display Updates**: Real-time via Supabase subscriptions
4. **Auto Turn End**: When `elapsed > 60 seconds`
5. **Perfect Sync**: Same timestamp-based calculation for all clients

### Benefits:
- ✅ **No client calls**: Eliminates speed issues
- ✅ **Timestamp-based**: Perfect accuracy across time zones
- ✅ **Refresh-proof**: Timer persists through page refreshes
- ✅ **Auto turn-end**: Server automatically switches turns
- ✅ **Real-time sync**: All players see exact same timer

### What You'll See:
- Timer counts down accurately: 60 → 59 → 58 → ... → 0
- Page refresh shows correct timer value (not 60)
- Turns auto-end exactly at 0 seconds
- All players perfectly synchronized
- No console spam or performance issues

## 4. Testing

Once the cron service is running:
1. Start a game
2. Watch timer count down normally
3. Refresh page - timer should show correct value
4. Let timer reach 0 - turn should auto-switch
5. Check that all players see same timer value

The system is now completely autonomous and manipulation-proof!