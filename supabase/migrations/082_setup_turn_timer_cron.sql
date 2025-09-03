-- Setup Turn Timer System
-- Note: pg_cron extension may not be available in all Supabase plans
-- The client-side useTurnTimer hook will handle timer functionality instead

-- Try to enable pg_cron extension (will fail gracefully if not available)
DO $$
BEGIN
  -- Try to create the extension
  CREATE EXTENSION IF NOT EXISTS pg_cron;
  
  -- If successful, set up the cron job
  PERFORM cron.schedule(
    'battle-turn-timer',           -- job name
    '* * * * * *',                 -- run every second (cron format)
    $$
    SELECT
      net.http_post(
        url:='https://xsknbbvyagngljxkftkd.supabase.co/functions/v1/battle-turn-timer',
        headers:='{"Content-Type": "application/json", "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjA1ODQzMiwiZXhwIjoyMDcxNjM0NDMyfQ.0RQlYUYT9hGAm9lIV7RMJOxYgSj-JKex6VCEILaR4_Q"}'::jsonb,
        body:='{}'::jsonb
      ) as request_id;
    $$
  );
  
  RAISE NOTICE 'pg_cron extension enabled and timer job scheduled successfully';
  
EXCEPTION 
  WHEN OTHERS THEN
    RAISE NOTICE 'pg_cron extension not available. Timer will run client-side via useTurnTimer hook.';
END
$$;

-- Comment explaining fallback approach
COMMENT ON SCHEMA public IS 'Turn timer runs via client-side useTurnTimer hook if pg_cron is unavailable';