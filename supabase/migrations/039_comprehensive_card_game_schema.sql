-- =============================================
-- Comprehensive Card Game Database Schema
-- Building production-ready digital TCG system
-- =============================================

-- Enable necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- CARD SYSTEM TABLES
-- =============================================

-- Card sets for organizing card releases/expansions
CREATE TABLE IF NOT EXISTS public.card_sets (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  code VARCHAR(20) UNIQUE NOT NULL, -- 'base', 'expansion1', etc.
  name VARCHAR(100) NOT NULL,
  description TEXT,
  release_date DATE,
  is_active BOOLEAN DEFAULT true,
  sort_order INTEGER DEFAULT 0,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Keywords/abilities that can be applied to cards
CREATE TABLE IF NOT EXISTS public.keywords (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  code VARCHAR(50) UNIQUE NOT NULL, -- 'taunt', 'charge', 'lifesteal', etc.
  name VARCHAR(100) NOT NULL,
  description TEXT NOT NULL,
  keyword_type VARCHAR(50) NOT NULL, -- 'passive', 'triggered', 'activated'
  is_stackable BOOLEAN DEFAULT false,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Card definitions (master card data)
CREATE TABLE IF NOT EXISTS public.cards (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  card_set_id UUID REFERENCES public.card_sets(id) ON DELETE CASCADE,
  code VARCHAR(50) UNIQUE NOT NULL, -- 'russet_warrior', 'mana_spud', etc.
  name VARCHAR(100) NOT NULL,
  description TEXT,
  flavor_text TEXT,
  card_type VARCHAR(20) NOT NULL CHECK (card_type IN ('unit', 'spell', 'structure')),
  cost INTEGER NOT NULL CHECK (cost >= 0 AND cost <= 15), -- Mana cost
  attack INTEGER CHECK (attack >= 0), -- NULL for spells
  health INTEGER CHECK (health >= 0), -- NULL for spells
  rarity VARCHAR(20) NOT NULL CHECK (rarity IN ('common', 'uncommon', 'rare', 'legendary', 'exotic')),
  potato_type VARCHAR(50), -- Links to our potato types for theming
  image_url TEXT,
  is_collectible BOOLEAN DEFAULT true,
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Many-to-many relationship for card keywords
CREATE TABLE IF NOT EXISTS public.card_keywords (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  card_id UUID REFERENCES public.cards(id) ON DELETE CASCADE,
  keyword_id UUID REFERENCES public.keywords(id) ON DELETE CASCADE,
  value INTEGER DEFAULT NULL, -- For parameterized keywords (e.g., lifesteal amount)
  UNIQUE(card_id, keyword_id)
);

-- =============================================
-- USER COLLECTION & DECK SYSTEM
-- =============================================

-- User card collection (which cards they own)
CREATE TABLE IF NOT EXISTS public.collection (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
  card_id UUID REFERENCES public.cards(id) ON DELETE CASCADE,
  quantity INTEGER NOT NULL DEFAULT 1 CHECK (quantity >= 0),
  acquired_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  UNIQUE(user_id, card_id)
);

-- User deck definitions
CREATE TABLE IF NOT EXISTS public.decks (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
  name VARCHAR(100) NOT NULL,
  description TEXT,
  is_active BOOLEAN DEFAULT true,
  is_public BOOLEAN DEFAULT false,
  format VARCHAR(50) DEFAULT 'standard', -- 'standard', 'wild', etc.
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Cards in each deck
CREATE TABLE IF NOT EXISTS public.deck_cards (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  deck_id UUID REFERENCES public.decks(id) ON DELETE CASCADE,
  card_id UUID REFERENCES public.cards(id) ON DELETE CASCADE,
  quantity INTEGER NOT NULL DEFAULT 1 CHECK (quantity >= 1 AND quantity <= 3), -- Max 3 copies
  UNIQUE(deck_id, card_id)
);

-- =============================================
-- MATCH SYSTEM TABLES
-- =============================================

-- Match sessions
CREATE TABLE IF NOT EXISTS public.matches (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  match_type VARCHAR(50) NOT NULL DEFAULT 'ranked', -- 'ranked', 'casual', 'tournament'
  status VARCHAR(50) NOT NULL DEFAULT 'waiting' CHECK (status IN ('waiting', 'active', 'finished', 'abandoned')),
  winner_id UUID REFERENCES auth.users(id),
  start_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  end_time TIMESTAMP WITH TIME ZONE,
  total_turns INTEGER DEFAULT 0,
  rng_seed BIGINT NOT NULL, -- For deterministic randomness
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Players in each match
CREATE TABLE IF NOT EXISTS public.match_players (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  match_id UUID REFERENCES public.matches(id) ON DELETE CASCADE,
  user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
  deck_id UUID REFERENCES public.decks(id),
  player_order INTEGER NOT NULL CHECK (player_order IN (1, 2)), -- Player 1 or 2
  starting_health INTEGER NOT NULL DEFAULT 20,
  current_health INTEGER NOT NULL DEFAULT 20,
  current_mana INTEGER NOT NULL DEFAULT 0,
  max_mana INTEGER NOT NULL DEFAULT 0,
  has_conceded BOOLEAN DEFAULT false,
  UNIQUE(match_id, user_id),
  UNIQUE(match_id, player_order)
);

-- Match state snapshots (current game state)
CREATE TABLE IF NOT EXISTS public.match_states (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  match_id UUID REFERENCES public.matches(id) ON DELETE CASCADE,
  turn_number INTEGER NOT NULL DEFAULT 1,
  active_player_id UUID REFERENCES auth.users(id),
  phase VARCHAR(50) NOT NULL DEFAULT 'start' CHECK (phase IN ('start', 'main', 'battle', 'end')),
  game_state JSONB NOT NULL, -- Complete game state
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  UNIQUE(match_id) -- Only one current state per match
);

-- Event log for all match actions (append-only)
CREATE TABLE IF NOT EXISTS public.match_events (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  match_id UUID REFERENCES public.matches(id) ON DELETE CASCADE,
  player_id UUID REFERENCES auth.users(id),
  event_type VARCHAR(50) NOT NULL, -- 'play_card', 'attack', 'end_turn', etc.
  event_data JSONB NOT NULL,
  turn_number INTEGER NOT NULL,
  event_sequence INTEGER NOT NULL, -- Order within turn
  timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  UNIQUE(match_id, turn_number, event_sequence)
);

-- Turn tracking
CREATE TABLE IF NOT EXISTS public.turns (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  match_id UUID REFERENCES public.matches(id) ON DELETE CASCADE,
  turn_number INTEGER NOT NULL,
  player_id UUID REFERENCES auth.users(id),
  started_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  ended_at TIMESTAMP WITH TIME ZONE,
  actions_taken INTEGER DEFAULT 0,
  UNIQUE(match_id, turn_number)
);

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

-- Card system indexes
CREATE INDEX IF NOT EXISTS idx_cards_set_type ON public.cards(card_set_id, card_type);
CREATE INDEX IF NOT EXISTS idx_cards_cost_rarity ON public.cards(cost, rarity);
CREATE INDEX IF NOT EXISTS idx_cards_active ON public.cards(is_active) WHERE is_active = true;
CREATE INDEX IF NOT EXISTS idx_card_keywords_card ON public.card_keywords(card_id);
CREATE INDEX IF NOT EXISTS idx_card_keywords_keyword ON public.card_keywords(keyword_id);

-- Collection indexes  
CREATE INDEX IF NOT EXISTS idx_collection_user ON public.collection(user_id);
CREATE INDEX IF NOT EXISTS idx_collection_card ON public.collection(card_id);

-- Deck system indexes
CREATE INDEX IF NOT EXISTS idx_decks_user_active ON public.decks(user_id, is_active);
CREATE INDEX IF NOT EXISTS idx_deck_cards_deck ON public.deck_cards(deck_id);

-- Match system indexes
CREATE INDEX IF NOT EXISTS idx_matches_status ON public.matches(status);
CREATE INDEX IF NOT EXISTS idx_matches_players ON public.match_players(match_id, user_id);
CREATE INDEX IF NOT EXISTS idx_match_events_match_turn ON public.match_events(match_id, turn_number, event_sequence);
CREATE INDEX IF NOT EXISTS idx_match_states_match ON public.match_states(match_id);
CREATE INDEX IF NOT EXISTS idx_turns_match ON public.turns(match_id, turn_number);

-- =============================================
-- ROW LEVEL SECURITY POLICIES
-- =============================================

-- Enable RLS on all tables
ALTER TABLE public.card_sets ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.keywords ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.cards ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.card_keywords ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.collection ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.decks ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.deck_cards ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.matches ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.match_players ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.match_states ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.match_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.turns ENABLE ROW LEVEL SECURITY;

-- Card system policies (public read for active content)
CREATE POLICY "Card sets are publicly readable" ON public.card_sets FOR SELECT USING (is_active = true);
CREATE POLICY "Keywords are publicly readable" ON public.keywords FOR SELECT USING (true);
CREATE POLICY "Cards are publicly readable" ON public.cards FOR SELECT USING (is_active = true);
CREATE POLICY "Card keywords are publicly readable" ON public.card_keywords FOR SELECT USING (true);

-- Collection policies (users can only access their own)
CREATE POLICY "Users can view their own collection" ON public.collection 
  FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY "Users can manage their own collection" ON public.collection 
  FOR ALL USING (auth.uid() = user_id);

-- Deck policies
CREATE POLICY "Users can view their own decks" ON public.decks 
  FOR SELECT USING (auth.uid() = user_id OR is_public = true);
CREATE POLICY "Users can manage their own decks" ON public.decks 
  FOR ALL USING (auth.uid() = user_id);

CREATE POLICY "Users can view deck cards" ON public.deck_cards 
  FOR SELECT USING (
    EXISTS (
      SELECT 1 FROM public.decks d 
      WHERE d.id = deck_id AND (d.user_id = auth.uid() OR d.is_public = true)
    )
  );
CREATE POLICY "Users can manage their deck cards" ON public.deck_cards 
  FOR ALL USING (
    EXISTS (
      SELECT 1 FROM public.decks d 
      WHERE d.id = deck_id AND d.user_id = auth.uid()
    )
  );

-- Match policies (players can see their own matches)
CREATE POLICY "Players can view their matches" ON public.matches 
  FOR SELECT USING (
    EXISTS (
      SELECT 1 FROM public.match_players mp 
      WHERE mp.match_id = id AND mp.user_id = auth.uid()
    )
  );

CREATE POLICY "Players can view their match player data" ON public.match_players 
  FOR SELECT USING (
    user_id = auth.uid() OR 
    EXISTS (
      SELECT 1 FROM public.match_players mp2 
      WHERE mp2.match_id = match_id AND mp2.user_id = auth.uid()
    )
  );

CREATE POLICY "Players can view their match states" ON public.match_states 
  FOR SELECT USING (
    EXISTS (
      SELECT 1 FROM public.match_players mp 
      WHERE mp.match_id = match_id AND mp.user_id = auth.uid()
    )
  );

CREATE POLICY "Players can view their match events" ON public.match_events 
  FOR SELECT USING (
    EXISTS (
      SELECT 1 FROM public.match_players mp 
      WHERE mp.match_id = match_id AND mp.user_id = auth.uid()
    )
  );

CREATE POLICY "Players can view their match turns" ON public.turns 
  FOR SELECT USING (
    EXISTS (
      SELECT 1 FROM public.match_players mp 
      WHERE mp.match_id = match_id AND mp.user_id = auth.uid()
    )
  );

-- Service role full access for all tables
CREATE POLICY "Service role full access card_sets" ON public.card_sets FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access keywords" ON public.keywords FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access cards" ON public.cards FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access card_keywords" ON public.card_keywords FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access collection" ON public.collection FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access decks" ON public.decks FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access deck_cards" ON public.deck_cards FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access matches" ON public.matches FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access match_players" ON public.match_players FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access match_states" ON public.match_states FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access match_events" ON public.match_events FOR ALL TO service_role USING (true) WITH CHECK (true);
CREATE POLICY "Service role full access turns" ON public.turns FOR ALL TO service_role USING (true) WITH CHECK (true);

-- =============================================
-- REALTIME PUBLICATION
-- =============================================

-- Add tables to realtime publication
DO $$
BEGIN
  BEGIN
    ALTER PUBLICATION supabase_realtime ADD TABLE public.matches;
  EXCEPTION WHEN duplicate_object THEN
    NULL;
  END;
  
  BEGIN
    ALTER PUBLICATION supabase_realtime ADD TABLE public.match_players;
  EXCEPTION WHEN duplicate_object THEN
    NULL;
  END;
  
  BEGIN
    ALTER PUBLICATION supabase_realtime ADD TABLE public.match_states;
  EXCEPTION WHEN duplicate_object THEN
    NULL;
  END;
  
  BEGIN
    ALTER PUBLICATION supabase_realtime ADD TABLE public.match_events;
  EXCEPTION WHEN duplicate_object THEN
    NULL;
  END;
END $$;

-- =============================================
-- SEED DATA - Basic Keywords
-- =============================================

-- Insert core keywords
INSERT INTO public.keywords (code, name, description, keyword_type, is_stackable) VALUES
  ('taunt', 'Taunt', 'Enemies must attack this unit before other units', 'passive', false),
  ('charge', 'Charge', 'Can attack immediately when played', 'passive', false),
  ('lifesteal', 'Lifesteal', 'Damage dealt by this unit heals your hero', 'passive', false),
  ('onplay', 'OnPlay', 'Triggers an effect when the card is played', 'triggered', false),
  ('deathrattle', 'Deathrattle', 'Triggers an effect when this unit dies', 'triggered', false),
  ('stealth', 'Stealth', 'Cannot be attacked until this unit attacks', 'passive', false),
  ('divine_shield', 'Divine Shield', 'First damage taken is prevented', 'passive', false),
  ('windfury', 'Windfury', 'Can attack twice per turn', 'passive', false),
  ('poisonous', 'Poisonous', 'Destroys any unit damaged by this unit', 'passive', false),
  ('spell_damage', 'Spell Damage', 'Increases spell damage by this amount', 'passive', true)
ON CONFLICT (code) DO NOTHING;

-- Insert base card set
INSERT INTO public.card_sets (code, name, description, release_date, is_active, sort_order) VALUES
  ('base', 'Base Set', 'The core set of potato cards for What''s My Potato', CURRENT_DATE, true, 0),
  ('spells', 'Spell Collection', 'Magical potato spells and abilities', CURRENT_DATE, true, 1),
  ('structures', 'Potato Structures', 'Buildings and structures in the potato world', CURRENT_DATE, true, 2)
ON CONFLICT (code) DO NOTHING;

-- Grant necessary permissions
GRANT SELECT ON public.card_sets TO authenticated;
GRANT SELECT ON public.keywords TO authenticated;
GRANT SELECT ON public.cards TO authenticated;
GRANT SELECT ON public.card_keywords TO authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON public.collection TO authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON public.decks TO authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON public.deck_cards TO authenticated;
GRANT SELECT ON public.matches TO authenticated;
GRANT SELECT ON public.match_players TO authenticated;
GRANT SELECT ON public.match_states TO authenticated;
GRANT SELECT ON public.match_events TO authenticated;
GRANT SELECT ON public.turns TO authenticated;

-- =============================================
-- HELPER FUNCTIONS
-- =============================================

-- Function to get user's card collection
CREATE OR REPLACE FUNCTION get_user_collection(user_uuid UUID)
RETURNS TABLE (
  card_id UUID,
  card_name VARCHAR,
  card_type VARCHAR,
  cost INTEGER,
  attack INTEGER,
  health INTEGER,
  rarity VARCHAR,
  quantity INTEGER
) LANGUAGE sql SECURITY DEFINER
AS $$
  SELECT 
    c.id as card_id,
    c.name as card_name,
    c.card_type,
    c.cost,
    c.attack,
    c.health,
    c.rarity,
    col.quantity
  FROM public.collection col
  JOIN public.cards c ON c.id = col.card_id
  WHERE col.user_id = user_uuid AND c.is_active = true
  ORDER BY c.cost, c.name;
$$;

-- Function to validate deck composition
CREATE OR REPLACE FUNCTION validate_deck(deck_uuid UUID)
RETURNS TABLE (
  is_valid BOOLEAN,
  total_cards INTEGER,
  error_message TEXT
) LANGUAGE sql SECURITY DEFINER
AS $$
  WITH deck_stats AS (
    SELECT 
      COUNT(*) as total_cards,
      SUM(dc.quantity) as total_card_count
    FROM public.deck_cards dc
    WHERE dc.deck_id = deck_uuid
  )
  SELECT 
    CASE 
      WHEN ds.total_card_count = 30 THEN true
      ELSE false
    END as is_valid,
    ds.total_card_count::INTEGER as total_cards,
    CASE 
      WHEN ds.total_card_count < 30 THEN 'Deck must contain exactly 30 cards'
      WHEN ds.total_card_count > 30 THEN 'Deck cannot contain more than 30 cards'
      ELSE 'Deck is valid'
    END as error_message
  FROM deck_stats ds;
$$;

COMMENT ON TABLE public.cards IS 'Master card definitions with stats and metadata';
COMMENT ON TABLE public.matches IS 'Game match sessions with deterministic RNG seeds';
COMMENT ON TABLE public.match_events IS 'Append-only log of all game actions for replay capability';
COMMENT ON TABLE public.match_states IS 'Current game state snapshots for quick loading';