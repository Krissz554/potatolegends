-- Server-side Matchmaking System
-- This migration sets up automatic matchmaking triggers and queue management

-- Create a simple logging function (no external calls)
CREATE OR REPLACE FUNCTION trigger_matchmaker()
RETURNS TRIGGER AS $$
DECLARE
    queue_count INTEGER;
BEGIN
    -- Check if we have at least 2 players in queue
    SELECT COUNT(*) INTO queue_count
    FROM public.matchmaking_queue
    WHERE status = 'searching';
    
    -- Log the queue state for monitoring
    INSERT INTO public.matchmaking_logs (event_type, message, player_count, created_at)
    VALUES ('queue_update', 'Player joined queue', queue_count, NOW());
    
    -- Note: The client-side periodic calls to the edge function will handle the actual matching
    -- This trigger just provides logging and monitoring
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create a logs table for matchmaking events
CREATE TABLE IF NOT EXISTS public.matchmaking_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_type VARCHAR(50) NOT NULL,
    message TEXT,
    player_count INTEGER,
    additional_data JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Add RLS for logs table
ALTER TABLE public.matchmaking_logs ENABLE ROW LEVEL SECURITY;

-- Policy for logs (readable by authenticated users, insertable by system)
CREATE POLICY "Allow system to insert logs" ON public.matchmaking_logs
FOR INSERT WITH CHECK (true);

CREATE POLICY "Allow authenticated users to read logs" ON public.matchmaking_logs
FOR SELECT USING (auth.role() = 'authenticated');

-- Create the trigger on matchmaking_queue
DROP TRIGGER IF EXISTS matchmaker_trigger ON public.matchmaking_queue;
CREATE TRIGGER matchmaker_trigger
    AFTER INSERT ON public.matchmaking_queue
    FOR EACH ROW
    WHEN (NEW.status = 'searching')
    EXECUTE FUNCTION trigger_matchmaker();

-- Create a function to clean up old queue entries (optional but recommended)
CREATE OR REPLACE FUNCTION cleanup_old_queue_entries()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    -- Remove queue entries older than 5 minutes
    DELETE FROM public.matchmaking_queue
    WHERE joined_at < NOW() - INTERVAL '5 minutes';
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    -- Log cleanup if any entries were removed
    IF deleted_count > 0 THEN
        INSERT INTO public.matchmaking_logs (event_type, message, player_count, created_at)
        VALUES ('cleanup', 'Removed old queue entries', deleted_count, NOW());
    END IF;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Alternative: Advanced trigger function with pg_net (requires extension)
-- This is commented out because it requires pg_net extension which may not be available
/*
CREATE OR REPLACE FUNCTION advanced_matchmaker_trigger()
RETURNS TRIGGER AS $$
DECLARE
    queue_count INTEGER;
BEGIN
    -- Check if we have at least 2 players in queue
    SELECT COUNT(*) INTO queue_count
    FROM public.matchmaking_queue
    WHERE status = 'searching';
    
    -- If we have 2 or more players, call the edge function
    IF queue_count >= 2 THEN
        PERFORM net.http_post(
            url := current_setting('app.settings.supabase_url', true) || '/functions/v1/matchmaker',
            headers := jsonb_build_object(
                'Content-Type', 'application/json',
                'Authorization', 'Bearer ' || current_setting('app.settings.service_role_key', true)
            ),
            body := jsonb_build_object('trigger', 'auto', 'timestamp', EXTRACT(EPOCH FROM NOW()))
        );
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;
*/

-- Comments for documentation
COMMENT ON FUNCTION trigger_matchmaker() IS 'Logs queue events when players join (client handles edge function calls)';
COMMENT ON FUNCTION cleanup_old_queue_entries() IS 'Removes old queue entries to prevent stale players';
COMMENT ON TABLE public.matchmaking_logs IS 'Logs matchmaking events for debugging and monitoring';

-- Add indexes for better performance
CREATE INDEX IF NOT EXISTS idx_matchmaking_logs_created_at ON public.matchmaking_logs (created_at DESC);
CREATE INDEX IF NOT EXISTS idx_matchmaking_logs_event_type ON public.matchmaking_logs (event_type);

-- Success message
SELECT 'Server-side matchmaking system created successfully!' as status;