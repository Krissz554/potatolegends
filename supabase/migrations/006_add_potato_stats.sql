-- =====================================================
-- ADD POTATO STATS (HP AND ATTACK)
-- =====================================================
-- Add trading card stats to potato registry

-- Add HP and Attack columns to potato_registry table
ALTER TABLE public.potato_registry 
ADD COLUMN IF NOT EXISTS hp INTEGER,
ADD COLUMN IF NOT EXISTS attack INTEGER;

-- Update existing potatoes with stats (will be populated by seeding process)
-- The stats will be generated deterministically based on seed + rarity

-- Add indexes for potential stat-based queries
CREATE INDEX IF NOT EXISTS idx_potato_registry_hp ON public.potato_registry (hp);
CREATE INDEX IF NOT EXISTS idx_potato_registry_attack ON public.potato_registry (attack);
CREATE INDEX IF NOT EXISTS idx_potato_registry_stats ON public.potato_registry (hp, attack);

-- Add constraint to ensure stats are reasonable (optional safety check)
ALTER TABLE public.potato_registry 
ADD CONSTRAINT chk_hp_range CHECK (hp IS NULL OR (hp >= 1 AND hp <= 500)),
ADD CONSTRAINT chk_attack_range CHECK (attack IS NULL OR (attack >= 1 AND attack <= 100));