-- Unlock starter heroes for all existing users who don't have them yet

-- First, make sure all existing users get the Potato King hero
DO $$
DECLARE
    user_record RECORD;
BEGIN
    -- Loop through all users who don't have any heroes unlocked
    FOR user_record IN 
        SELECT DISTINCT up.id 
        FROM user_profiles up
        LEFT JOIN user_heroes uh ON up.id = uh.user_id
        WHERE uh.user_id IS NULL
    LOOP
        -- Unlock starter heroes for this user
        PERFORM unlock_starter_heroes_for_user(user_record.id);
        RAISE NOTICE 'Unlocked starter heroes for existing user: %', user_record.id;
    END LOOP;
END $$;

-- Also handle users who might have some heroes but not the starter ones
INSERT INTO user_heroes (user_id, hero_id, is_active)
SELECT 
    up.id as user_id,
    h.hero_id,
    h.hero_id = 'potato_king' as is_active
FROM user_profiles up
CROSS JOIN heroes h
WHERE h.is_starter = true
AND NOT EXISTS (
    SELECT 1 FROM user_heroes uh 
    WHERE uh.user_id = up.id AND uh.hero_id = h.hero_id
)
ON CONFLICT (user_id, hero_id) DO NOTHING;

-- Ensure every user has at least one active hero
UPDATE user_heroes 
SET is_active = true 
WHERE user_id IN (
    SELECT DISTINCT uh.user_id 
    FROM user_heroes uh
    WHERE NOT EXISTS (
        SELECT 1 FROM user_heroes uh2 
        WHERE uh2.user_id = uh.user_id AND uh2.is_active = true
    )
)
AND hero_id = 'potato_king';