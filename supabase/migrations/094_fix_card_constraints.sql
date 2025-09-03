-- Fix card constraints for the overhaul system
-- Migration: 094_fix_card_constraints.sql

-- Fix attack constraint to allow null for non-unit cards
ALTER TABLE public.card_complete 
DROP CONSTRAINT IF EXISTS card_complete_attack_check;

ALTER TABLE public.card_complete 
ADD CONSTRAINT card_complete_attack_check 
CHECK (
  (card_type = 'unit' AND attack >= 0 AND attack <= 15) OR 
  (card_type IN ('spell', 'structure', 'relic') AND (attack IS NULL OR attack = 0))
);

-- Fix HP constraint to allow null for non-unit cards  
ALTER TABLE public.card_complete 
DROP CONSTRAINT IF EXISTS card_complete_hp_check;

ALTER TABLE public.card_complete 
ADD CONSTRAINT card_complete_hp_check 
CHECK (
  (card_type = 'unit' AND hp >= 1 AND hp <= 20) OR 
  (card_type = 'structure' AND structure_hp >= 1 AND structure_hp <= 15) OR 
  (card_type IN ('spell', 'relic') AND (hp IS NULL OR hp = 0))
);

-- Allow service role to perform bulk updates
GRANT UPDATE ON public.card_complete TO service_role;

-- Success message
SELECT 'Card constraints fixed for overhaul system!' as status;