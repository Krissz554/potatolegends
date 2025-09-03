# Edge Functions Deployment Instructions

## Manual Deployment via Supabase Dashboard

Since the CLI isn't available in this environment, you'll need to deploy the updated edge functions manually:

### 1. Go to Supabase Dashboard
- Visit: https://supabase.com/dashboard/project/xsknbbvyagngljxkftkd
- Navigate to: Edge Functions section

### 2. Deploy battle-mulligan (VERSION 3.0)
- Click "Create a new function" or "Update existing function"
- Function name: `battle-mulligan`
- Copy the entire content from: `/workspace/supabase/functions/battle-mulligan/index.ts`

### 3. Deploy battle-deploy-card (VERSION 2.0)
- Click "Create a new function" or "Update existing function"  
- Function name: `battle-deploy-card`
- Copy the entire content from: `/workspace/supabase/functions/battle-deploy-card/index.ts`

### 4. Deploy battle-initialize
- Function name: `battle-initialize`
- Copy the entire content from: `/workspace/supabase/functions/battle-initialize/index.ts`

## Key Updates in VERSION 3.0:

### battle-mulligan:
- ✅ Enhanced logging with version identifier
- ✅ Comprehensive request body logging
- ✅ Better error messages with stack traces
- ✅ Request validation and parsing
- ✅ Detailed authentication logging

### battle-deploy-card:
- ✅ Version 2.0 identifier for deployment verification
- ✅ Better mulligan phase error messages
- ✅ Enhanced phase validation

## After Deployment:

1. Try to complete mulligan again
2. Check the Supabase Edge Functions logs for detailed error messages
3. The new version should show "VERSION 3.0" in the logs
4. More specific error messages will help identify the exact issue

## Test Commands:
- The functions now have test modes for version verification
- Look for "VERSION 3.0" in the logs to confirm deployment