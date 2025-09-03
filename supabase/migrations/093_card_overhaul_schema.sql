-- Card Overhaul: Complete schema update for 226-card strategic system
-- Migration: 093_card_overhaul_schema.sql

-- Add new card types beyond just 'unit'
ALTER TABLE public.card_complete 
DROP CONSTRAINT IF EXISTS card_complete_card_type_check;

ALTER TABLE public.card_complete 
ADD CONSTRAINT card_complete_card_type_check 
CHECK (card_type = ANY (ARRAY['unit', 'spell', 'structure', 'relic']));

-- Add class/role system for units
ALTER TABLE public.card_complete 
ADD COLUMN IF NOT EXISTS unit_class TEXT CHECK (unit_class = ANY (ARRAY['warrior', 'tank', 'mage', 'healer', 'mixed']));

-- Add keyword abilities system
ALTER TABLE public.card_complete 
DROP COLUMN IF EXISTS keywords CASCADE;

ALTER TABLE public.card_complete 
ADD COLUMN keywords TEXT[] DEFAULT ARRAY[]::TEXT[];

-- Update keywords constraint to include all new abilities
-- Common keywords: charge, taunt, lifesteal, freeze, overload, deathrattle, battlecry, poison, silence
ALTER TABLE public.card_complete 
ADD CONSTRAINT valid_keywords CHECK (
  keywords <@ ARRAY[
    'charge', 'taunt', 'lifesteal', 'freeze', 'overload', 'deathrattle', 
    'battlecry', 'poison', 'silence', 'double_attack', 'immune', 'stealth'
  ]
);

-- Add targeting and effect data for spells/abilities
ALTER TABLE public.card_complete 
ADD COLUMN IF NOT EXISTS target_type TEXT CHECK (target_type = ANY (ARRAY['none', 'enemy_unit', 'ally_unit', 'any_unit', 'enemy_hero', 'ally_hero', 'all_enemies', 'all_allies', 'all_units', 'random_enemy', 'random_ally']));

ALTER TABLE public.card_complete 
ADD COLUMN IF NOT EXISTS spell_damage INTEGER DEFAULT 0;

ALTER TABLE public.card_complete 
ADD COLUMN IF NOT EXISTS heal_amount INTEGER DEFAULT 0;

-- Add structure-specific fields
ALTER TABLE public.card_complete 
ADD COLUMN IF NOT EXISTS structure_hp INTEGER; -- HP for structures that can be attacked

ALTER TABLE public.card_complete 
ADD COLUMN IF NOT EXISTS passive_effect TEXT; -- Ongoing effects for structures/relics

-- Add mana cost validation (0-10 mana)
ALTER TABLE public.card_complete 
DROP CONSTRAINT IF EXISTS card_complete_mana_cost_check;

ALTER TABLE public.card_complete 
ADD CONSTRAINT card_complete_mana_cost_check 
CHECK (mana_cost >= 0 AND mana_cost <= 10);

-- Add attack/hp validation for units only
ALTER TABLE public.card_complete 
DROP CONSTRAINT IF EXISTS card_complete_attack_check;

ALTER TABLE public.card_complete 
ADD CONSTRAINT card_complete_attack_check 
CHECK (
  (card_type = 'unit' AND attack >= 0 AND attack <= 15) OR 
  (card_type != 'unit' AND attack IS NULL)
);

ALTER TABLE public.card_complete 
DROP CONSTRAINT IF EXISTS card_complete_hp_check;

ALTER TABLE public.card_complete 
ADD CONSTRAINT card_complete_hp_check 
CHECK (
  (card_type = 'unit' AND hp >= 1 AND hp <= 20) OR 
  (card_type = 'structure' AND structure_hp >= 1 AND structure_hp <= 15) OR 
  (card_type IN ('spell', 'relic') AND hp IS NULL)
);

-- Update rarity distribution tracking
CREATE TABLE IF NOT EXISTS public.rarity_distribution (
  rarity TEXT PRIMARY KEY,
  target_count INTEGER NOT NULL,
  current_count INTEGER DEFAULT 0,
  percentage DECIMAL(5,2) NOT NULL
);

-- Insert target distribution for 226 cards
INSERT INTO public.rarity_distribution (rarity, target_count, percentage) VALUES
  ('common', 90, 40.0),
  ('uncommon', 56, 25.0),
  ('rare', 45, 20.0),
  ('legendary', 23, 10.0),
  ('exotic', 12, 5.0)
ON CONFLICT (rarity) DO UPDATE SET
  target_count = EXCLUDED.target_count,
  percentage = EXCLUDED.percentage;

-- Create function to update rarity counts
CREATE OR REPLACE FUNCTION update_rarity_counts()
RETURNS TRIGGER AS $$
BEGIN
  -- Recalculate all rarity counts
  UPDATE public.rarity_distribution 
  SET current_count = (
    SELECT COUNT(*) 
    FROM public.card_complete 
    WHERE rarity = rarity_distribution.rarity
  );
  
  RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create trigger to auto-update rarity counts
DROP TRIGGER IF EXISTS rarity_count_trigger ON public.card_complete;
CREATE TRIGGER rarity_count_trigger
  AFTER INSERT OR UPDATE OR DELETE ON public.card_complete
  FOR EACH STATEMENT
  EXECUTE FUNCTION update_rarity_counts();

-- Add indexes for better performance
CREATE INDEX IF NOT EXISTS idx_card_complete_card_type ON public.card_complete (card_type);
CREATE INDEX IF NOT EXISTS idx_card_complete_unit_class ON public.card_complete (unit_class);
CREATE INDEX IF NOT EXISTS idx_card_complete_keywords ON public.card_complete USING GIN (keywords);
CREATE INDEX IF NOT EXISTS idx_card_complete_potato_type_rarity ON public.card_complete (potato_type, rarity);

-- Update battlefield structure to support different card types
ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS battlefield_structures JSONB DEFAULT '{}'::jsonb;

ALTER TABLE public.battle_sessions 
ADD COLUMN IF NOT EXISTS active_relics JSONB DEFAULT '{}'::jsonb;

-- Grant permissions
GRANT SELECT ON public.rarity_distribution TO authenticated;
GRANT EXECUTE ON FUNCTION update_rarity_counts() TO service_role;

-- Success message
SELECT 'Card overhaul schema ready! Support for Units, Spells, Structures, Relics, Classes, and Keywords added!' as status;