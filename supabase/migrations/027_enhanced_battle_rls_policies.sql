-- =====================================================
-- ENHANCED RLS POLICIES FOR REAL-TIME BATTLE
-- =====================================================

-- Drop existing conflicting policies first
DROP POLICY IF EXISTS "Service role can manage battle actions" ON public.battle_actions;
DROP POLICY IF EXISTS "Users can read battle actions for their battles" ON public.battle_actions;

-- Allow service role to manage battle actions for real-time sync
CREATE POLICY "Service role can manage battle actions" ON public.battle_actions
FOR ALL USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Allow authenticated users to read battle actions for their battles
CREATE POLICY "Users can read battle actions for their battles" ON public.battle_actions
FOR SELECT USING (
  EXISTS (
    SELECT 1 FROM public.battle_sessions bs
    WHERE bs.id = battle_session_id 
    AND (bs.player1_id = auth.uid() OR bs.player2_id = auth.uid())
  )
);

-- Allow users to create actions in their own battles
CREATE POLICY "Users can create battle actions in their battles" ON public.battle_actions
FOR INSERT WITH CHECK (
  auth.uid() = player_id AND
  EXISTS (
    SELECT 1 FROM public.battle_sessions bs
    WHERE bs.id = battle_session_id 
    AND (bs.player1_id = auth.uid() OR bs.player2_id = auth.uid())
    AND bs.status IN ('deploying', 'battling')
  )
);

-- Enhanced battle sessions policies
DROP POLICY IF EXISTS "Service role can update battle sessions" ON public.battle_sessions;

CREATE POLICY "Service role can update battle sessions" ON public.battle_sessions
FOR UPDATE USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Allow players to update battle sessions they're part of (for game actions)
CREATE POLICY "Players can update their own battle sessions" ON public.battle_sessions
FOR UPDATE USING (
  auth.uid() = player1_id OR auth.uid() = player2_id
);

-- Enhanced battle deck policies for real-time access
DROP POLICY IF EXISTS "Service role can read all battle decks" ON public.battle_decks;

CREATE POLICY "Service role can read all battle decks" ON public.battle_decks
FOR SELECT USING (
  current_setting('request.jwt.claim.role', true) = 'service_role'
);

-- Allow players to read battle decks of their opponents during active battles
CREATE POLICY "Players can read opponent decks in active battles" ON public.battle_decks
FOR SELECT USING (
  EXISTS (
    SELECT 1 FROM public.battle_sessions bs
    WHERE (bs.player1_id = auth.uid() OR bs.player2_id = auth.uid())
    AND bs.status IN ('deploying', 'battling')
    AND (bs.player1_id = battle_decks.user_id OR bs.player2_id = battle_decks.user_id)
  )
);

-- Success message
SELECT 'Enhanced RLS policies for real-time battle created successfully!' as status;