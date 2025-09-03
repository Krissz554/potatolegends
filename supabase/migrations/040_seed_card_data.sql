-- =============================================
-- Seed Card Data for Production Card Game
-- Creating diverse potato-themed cards across all types
-- =============================================

-- Get card set IDs for reference
DO $$
DECLARE
  base_set_id UUID;
  spells_set_id UUID;
  structures_set_id UUID;
  
  -- Keyword IDs
  taunt_id UUID;
  charge_id UUID;
  lifesteal_id UUID;
  onplay_id UUID;
  deathrattle_id UUID;
  stealth_id UUID;
  divine_shield_id UUID;
  windfury_id UUID;
  poisonous_id UUID;
  spell_damage_id UUID;
BEGIN
  -- Get set IDs
  SELECT id INTO base_set_id FROM public.card_sets WHERE code = 'base';
  SELECT id INTO spells_set_id FROM public.card_sets WHERE code = 'spells';
  SELECT id INTO structures_set_id FROM public.card_sets WHERE code = 'structures';
  
  -- Get keyword IDs
  SELECT id INTO taunt_id FROM public.keywords WHERE code = 'taunt';
  SELECT id INTO charge_id FROM public.keywords WHERE code = 'charge';
  SELECT id INTO lifesteal_id FROM public.keywords WHERE code = 'lifesteal';
  SELECT id INTO onplay_id FROM public.keywords WHERE code = 'onplay';
  SELECT id INTO deathrattle_id FROM public.keywords WHERE code = 'deathrattle';
  SELECT id INTO stealth_id FROM public.keywords WHERE code = 'stealth';
  SELECT id INTO divine_shield_id FROM public.keywords WHERE code = 'divine_shield';
  SELECT id INTO windfury_id FROM public.keywords WHERE code = 'windfury';
  SELECT id INTO poisonous_id FROM public.keywords WHERE code = 'poisonous';
  SELECT id INTO spell_damage_id FROM public.keywords WHERE code = 'spell_damage';

  -- =============================================
  -- UNIT CARDS (Base Set)
  -- =============================================
  
  -- Low cost units (1-3 mana)
  INSERT INTO public.cards (card_set_id, code, name, description, card_type, cost, attack, health, rarity, potato_type) VALUES
    (base_set_id, 'baby_spud', 'Baby Spud', 'A tiny but enthusiastic potato warrior.', 'unit', 1, 1, 1, 'common', 'fingerling'),
    (base_set_id, 'russet_scout', 'Russet Scout', 'Quick and nimble, perfect for reconnaissance.', 'unit', 1, 1, 2, 'common', 'russet'),
    (base_set_id, 'sweet_guardian', 'Sweet Guardian', 'Protects smaller potatoes with unwavering loyalty.', 'unit', 2, 1, 4, 'common', 'sweet'),
    (base_set_id, 'yukon_warrior', 'Yukon Warrior', 'A sturdy fighter from the northern fields.', 'unit', 2, 2, 2, 'common', 'yukon'),
    (base_set_id, 'red_berserker', 'Red Berserker', 'Charges into battle with reckless abandon.', 'unit', 2, 3, 1, 'common', 'red'),
    (base_set_id, 'purple_assassin', 'Purple Assassin', 'Strikes from the shadows with deadly precision.', 'unit', 3, 2, 2, 'uncommon', 'purple'),
    (base_set_id, 'maris_healer', 'Maris Healer', 'Channels life energy to sustain allies.', 'unit', 3, 2, 4, 'uncommon', 'maris');

  -- Mid cost units (4-6 mana)
  INSERT INTO public.cards (card_set_id, code, name, description, card_type, cost, attack, health, rarity, potato_type) VALUES
    (base_set_id, 'golden_knight', 'Golden Knight', 'A noble warrior clad in shimmering armor.', 'unit', 4, 4, 4, 'uncommon', 'golden'),
    (base_set_id, 'purple_majesty', 'Purple Majesty', 'Royalty among potatoes, commands respect.', 'unit', 4, 3, 6, 'rare', 'purple'),
    (base_set_id, 'russet_champion', 'Russet Champion', 'A battle-hardened veteran of many conflicts.', 'unit', 5, 5, 5, 'rare', 'russet'),
    (base_set_id, 'sweet_colossus', 'Sweet Colossus', 'Massive and imposing, blocks enemy advances.', 'unit', 6, 4, 8, 'rare', 'sweet'),
    (base_set_id, 'storm_spud', 'Storm Spud', 'Crackles with electrical energy.', 'unit', 5, 6, 4, 'rare', 'fingerling');

  -- High cost units (7+ mana)
  INSERT INTO public.cards (card_set_id, code, name, description, card_type, cost, attack, health, rarity, potato_type) VALUES
    (base_set_id, 'ancient_tuber', 'Ancient Tuber', 'An elder potato of immense wisdom and power.', 'unit', 7, 6, 8, 'legendary', 'russet'),
    (base_set_id, 'potato_overlord', 'Potato Overlord', 'The supreme ruler of all potato-kind.', 'unit', 8, 8, 8, 'legendary', 'golden'),
    (base_set_id, 'cosmic_spud', 'Cosmic Spud', 'Transcends earthly bounds with cosmic power.', 'unit', 9, 9, 9, 'exotic', 'purple'),
    (base_set_id, 'eternal_harvest', 'Eternal Harvest', 'The embodiment of endless growth and renewal.', 'unit', 10, 10, 10, 'exotic', 'sweet');

  -- =============================================
  -- SPELL CARDS
  -- =============================================
  
  INSERT INTO public.cards (card_set_id, code, name, description, card_type, cost, attack, health, rarity, potato_type) VALUES
    -- Damage spells
    (spells_set_id, 'spud_bolt', 'Spud Bolt', 'Deal 3 damage to any target.', 'spell', 1, NULL, NULL, 'common', NULL),
    (spells_set_id, 'mash_blast', 'Mash Blast', 'Deal 2 damage to all enemies.', 'spell', 2, NULL, NULL, 'common', NULL),
    (spells_set_id, 'volcanic_eruption', 'Volcanic Eruption', 'Deal 5 damage to all enemy units.', 'spell', 4, NULL, NULL, 'rare', NULL),
    (spells_set_id, 'potato_cannon', 'Potato Cannon', 'Deal 6 damage to target unit or hero.', 'spell', 3, NULL, NULL, 'uncommon', NULL),
    
    -- Buff spells
    (spells_set_id, 'growth_spurt', 'Growth Spurt', 'Give a unit +2/+2.', 'spell', 1, NULL, NULL, 'common', NULL),
    (spells_set_id, 'iron_skin', 'Iron Skin', 'Give a unit +0/+4 and Taunt.', 'spell', 2, NULL, NULL, 'common', NULL),
    (spells_set_id, 'battle_fury', 'Battle Fury', 'Give a unit +3/+0 and Charge.', 'spell', 2, NULL, NULL, 'uncommon', NULL),
    (spells_set_id, 'blessing_of_harvest', 'Blessing of Harvest', 'Give all friendly units +1/+1.', 'spell', 3, NULL, NULL, 'rare', NULL),
    
    -- Utility spells
    (spells_set_id, 'tuber_healing', 'Tuber Healing', 'Restore 5 health to your hero.', 'spell', 1, NULL, NULL, 'common', NULL),
    (spells_set_id, 'root_network', 'Root Network', 'Draw 2 cards.', 'spell', 2, NULL, NULL, 'uncommon', NULL),
    (spells_set_id, 'soil_enrichment', 'Soil Enrichment', 'Gain 2 mana crystals this turn only.', 'spell', 3, NULL, NULL, 'rare', NULL),
    (spells_set_id, 'harvest_time', 'Harvest Time', 'Summon two 1/1 Baby Spuds.', 'spell', 2, NULL, NULL, 'uncommon', NULL),
    
    -- Advanced spells
    (spells_set_id, 'metamorphosis', 'Metamorphosis', 'Transform a unit into a random unit that costs 2 more.', 'spell', 4, NULL, NULL, 'rare', NULL),
    (spells_set_id, 'potato_storm', 'Potato Storm', 'Deal 1 damage to all enemies. Repeat for each spell you''ve cast this turn.', 'spell', 5, NULL, NULL, 'legendary', NULL),
    (spells_set_id, 'time_harvest', 'Time Harvest', 'Take an extra turn after this one.', 'spell', 8, NULL, NULL, 'exotic', NULL);

  -- =============================================
  -- STRUCTURE CARDS
  -- =============================================
  
  INSERT INTO public.cards (card_set_id, code, name, description, card_type, cost, attack, health, rarity, potato_type) VALUES
    -- Defensive structures
    (structures_set_id, 'potato_wall', 'Potato Wall', 'A sturdy barrier that protects your side of the field.', 'structure', 2, 0, 6, 'common', NULL),
    (structures_set_id, 'sprouting_garden', 'Sprouting Garden', 'At the end of your turn, summon a 1/1 Baby Spud.', 'structure', 3, 0, 4, 'uncommon', NULL),
    (structures_set_id, 'watchtower', 'Watchtower', 'Your units have +1 Attack.', 'structure', 3, 0, 5, 'uncommon', NULL),
    (structures_set_id, 'mana_shrine', 'Mana Shrine', 'At the start of your turn, gain +1 mana this turn.', 'structure', 4, 0, 3, 'rare', NULL),
    
    -- Offensive structures
    (structures_set_id, 'catapult', 'Catapult', 'At the end of your turn, deal 2 damage to a random enemy.', 'structure', 4, 0, 4, 'uncommon', NULL),
    (structures_set_id, 'potato_forge', 'Potato Forge', 'Whenever you play a unit, give it +1/+1.', 'structure', 5, 0, 6, 'rare', NULL),
    (structures_set_id, 'arcane_laboratory', 'Arcane Laboratory', 'Your spells cost (1) less.', 'structure', 4, 0, 3, 'rare', NULL),
    
    -- Legendary structures
    (structures_set_id, 'great_potato_tree', 'Great Potato Tree', 'At the start of your turn, add a random potato card to your hand.', 'structure', 6, 0, 8, 'legendary', NULL),
    (structures_set_id, 'eternal_farm', 'Eternal Farm', 'Whenever a friendly unit dies, summon a 2/2 Potato Sprout.', 'structure', 7, 0, 5, 'legendary', NULL),
    (structures_set_id, 'nexus_of_roots', 'Nexus of Roots', 'All friendly units have +2/+2 and Taunt.', 'structure', 8, 0, 10, 'exotic', NULL);

  -- =============================================
  -- ASSIGN KEYWORDS TO CARDS
  -- =============================================
  
  -- Units with keywords
  INSERT INTO public.card_keywords (card_id, keyword_id, value) VALUES
    -- Taunt units
    ((SELECT id FROM public.cards WHERE code = 'sweet_guardian'), taunt_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'sweet_colossus'), taunt_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'potato_wall'), taunt_id, NULL),
    
    -- Charge units
    ((SELECT id FROM public.cards WHERE code = 'red_berserker'), charge_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'storm_spud'), charge_id, NULL),
    
    -- Lifesteal units
    ((SELECT id FROM public.cards WHERE code = 'maris_healer'), lifesteal_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'ancient_tuber'), lifesteal_id, NULL),
    
    -- Stealth units
    ((SELECT id FROM public.cards WHERE code = 'purple_assassin'), stealth_id, NULL),
    
    -- Divine Shield units
    ((SELECT id FROM public.cards WHERE code = 'golden_knight'), divine_shield_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'potato_overlord'), divine_shield_id, NULL),
    
    -- Windfury units
    ((SELECT id FROM public.cards WHERE code = 'storm_spud'), windfury_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'cosmic_spud'), windfury_id, NULL),
    
    -- Poisonous units
    ((SELECT id FROM public.cards WHERE code = 'purple_assassin'), poisonous_id, NULL),
    
    -- OnPlay effects (will be implemented in game engine)
    ((SELECT id FROM public.cards WHERE code = 'sprouting_garden'), onplay_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'harvest_time'), onplay_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'growth_spurt'), onplay_id, NULL),
    
    -- Deathrattle effects
    ((SELECT id FROM public.cards WHERE code = 'eternal_farm'), deathrattle_id, NULL),
    ((SELECT id FROM public.cards WHERE code = 'ancient_tuber'), deathrattle_id, NULL);

END $$;

-- =============================================
-- STARTER COLLECTION FOR NEW USERS
-- =============================================

-- Function to give new users a starter collection
CREATE OR REPLACE FUNCTION give_starter_collection(user_uuid UUID)
RETURNS VOID LANGUAGE plpgsql SECURITY DEFINER
AS $$
DECLARE
  card_record RECORD;
BEGIN
  -- Give basic cards to new users
  FOR card_record IN (
    SELECT id FROM public.cards 
    WHERE rarity = 'common' AND is_collectible = true
    LIMIT 20
  ) LOOP
    INSERT INTO public.collection (user_id, card_id, quantity)
    VALUES (user_uuid, card_record.id, 2)
    ON CONFLICT (user_id, card_id) DO UPDATE SET quantity = collection.quantity + 2;
  END LOOP;
  
  -- Give some uncommon cards
  FOR card_record IN (
    SELECT id FROM public.cards 
    WHERE rarity = 'uncommon' AND is_collectible = true
    LIMIT 10
  ) LOOP
    INSERT INTO public.collection (user_id, card_id, quantity)
    VALUES (user_uuid, card_record.id, 1)
    ON CONFLICT (user_id, card_id) DO UPDATE SET quantity = collection.quantity + 1;
  END LOOP;
  
  -- Give a few rare cards
  FOR card_record IN (
    SELECT id FROM public.cards 
    WHERE rarity = 'rare' AND is_collectible = true
    LIMIT 3
  ) LOOP
    INSERT INTO public.collection (user_id, card_id, quantity)
    VALUES (user_uuid, card_record.id, 1)
    ON CONFLICT (user_id, card_id) DO UPDATE SET quantity = collection.quantity + 1;
  END LOOP;
END;
$$;

-- Function to create a basic starter deck
CREATE OR REPLACE FUNCTION create_starter_deck(user_uuid UUID)
RETURNS UUID LANGUAGE plpgsql SECURITY DEFINER
AS $$
DECLARE
  deck_uuid UUID;
  card_record RECORD;
  cards_added INTEGER := 0;
BEGIN
  -- Create the deck
  INSERT INTO public.decks (user_id, name, description, is_active)
  VALUES (user_uuid, 'Starter Deck', 'A basic deck to get you started', true)
  RETURNING id INTO deck_uuid;
  
  -- Add units (aim for 20 units)
  FOR card_record IN (
    SELECT c.id, 
           CASE 
             WHEN c.cost <= 2 THEN 3
             WHEN c.cost <= 4 THEN 2
             ELSE 1
           END as quantity
    FROM public.cards c
    JOIN public.collection col ON col.card_id = c.id
    WHERE col.user_id = user_uuid 
      AND c.card_type = 'unit' 
      AND c.cost <= 6
      AND col.quantity > 0
    ORDER BY c.cost, RANDOM()
    LIMIT 15
  ) LOOP
    INSERT INTO public.deck_cards (deck_id, card_id, quantity)
    VALUES (deck_uuid, card_record.id, LEAST(card_record.quantity, 2));
    cards_added := cards_added + LEAST(card_record.quantity, 2);
    
    IF cards_added >= 20 THEN EXIT; END IF;
  END LOOP;
  
  -- Add spells (aim for 8 spells)
  cards_added := 0;
  FOR card_record IN (
    SELECT c.id, 1 as quantity
    FROM public.cards c
    JOIN public.collection col ON col.card_id = c.id
    WHERE col.user_id = user_uuid 
      AND c.card_type = 'spell' 
      AND c.cost <= 5
      AND col.quantity > 0
    ORDER BY c.cost, RANDOM()
    LIMIT 8
  ) LOOP
    INSERT INTO public.deck_cards (deck_id, card_id, quantity)
    VALUES (deck_uuid, card_record.id, 1);
    cards_added := cards_added + 1;
    
    IF cards_added >= 8 THEN EXIT; END IF;
  END LOOP;
  
  -- Add structures (aim for 2 structures)
  cards_added := 0;
  FOR card_record IN (
    SELECT c.id, 1 as quantity
    FROM public.cards c
    JOIN public.collection col ON col.card_id = c.id
    WHERE col.user_id = user_uuid 
      AND c.card_type = 'structure' 
      AND c.cost <= 6
      AND col.quantity > 0
    ORDER BY c.cost, RANDOM()
    LIMIT 2
  ) LOOP
    INSERT INTO public.deck_cards (deck_id, card_id, quantity)
    VALUES (deck_uuid, card_record.id, 1);
    cards_added := cards_added + 1;
    
    IF cards_added >= 2 THEN EXIT; END IF;
  END LOOP;
  
  RETURN deck_uuid;
END;
$$;

-- Function to initialize new user with collection and deck
CREATE OR REPLACE FUNCTION initialize_new_user(user_uuid UUID)
RETURNS UUID LANGUAGE plpgsql SECURITY DEFINER
AS $$
DECLARE
  starter_deck_uuid UUID;
BEGIN
  -- Give starter collection
  PERFORM give_starter_collection(user_uuid);
  
  -- Create starter deck
  SELECT create_starter_deck(user_uuid) INTO starter_deck_uuid;
  
  RETURN starter_deck_uuid;
END;
$$;

COMMENT ON FUNCTION give_starter_collection IS 'Gives new users a starter collection of cards';
COMMENT ON FUNCTION create_starter_deck IS 'Creates a balanced 30-card starter deck for new users';
COMMENT ON FUNCTION initialize_new_user IS 'Complete initialization for new users - collection and deck';