-- Add 10 anti-relic cards for game balance
-- Migration: 098_add_anti_relic_cards.sql

-- Insert anti-relic cards following existing pattern
INSERT INTO public.card_complete (
  id, registry_id, name, description, potato_type, trait, adjective, rarity, palette_name,
  pixel_art_data, generation_seed, variation_index, sort_order, mana_cost, attack, hp,
  card_type, is_legendary, exotic, set_id, format_legalities, ability_text, passive_effects,
  triggered_effects, illustration_url, frame_style, flavor_text, release_date, tags,
  craft_cost, dust_value, alternate_skins, voice_line_url, level_up_conditions, token_spawns,
  created_at, pixel_art_config, visual_effects, unit_class, keywords, target_type,
  spell_damage, heal_amount, structure_hp, passive_effect
) VALUES
-- Common Cards (4)
(gen_random_uuid(), 'potato_0227', 'Purge Sigil', 'A brave little radiant potato who destroys enemy magical artifacts. Though small, they dream of great adventures and practice their light abilities every day, hoping to become a legendary hero someday.', 
 'light', 'who destroys enemy magical artifacts', 'purging', 'common', 'golden_yellow',
 '{"type":"light","rarity":"common","traits":["who destroys enemy magical artifacts"],"palette":"golden_yellow","enhanced":true,"generation_seed":"anti-relic-purge-sigil_common_light"}', 
 'anti-relic-purge-sigil_common_light', 0, 227, 3, NULL, NULL,
 'spell', false, false, 'relic_breakers', '{"standard"}', 'Destroy the enemy relic. If there is no enemy relic, draw a card.', '{}',
 '{}', 'https://example.com/potato-art/potato_0227.png', 'standard', '"Light purifies all corruption." - Purification Codex', '2025-01-24', '{"Spell","Relic-Breaker"}',
 40, 5, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0227","rarity":"common","element":"light","base_color":"hsl(50 90% 80%)","accent_color":"hsl(60 100% 90%)"}', 
 '{"rarity_effects":{"basic":true},"element_effects":{"rays":true,"stars":false,"radiance":true},"unique_features":{"personality":"cute","spot_pattern":1,"eye_variation":1,"smile_variation":1}}', 
 NULL, '{"Spell:DestroyEnemyRelic","Spell:ConditionalDraw"}', NULL, 0, 0, NULL, NULL),

(gen_random_uuid(), 'potato_0228', 'Rust Hex', 'A brave little shadowy potato who corrupts magical artifacts. Though small, they dream of great adventures and practice their void abilities every day, hoping to become a legendary hero someday.',
 'void', 'who corrupts magical artifacts', 'rusting', 'common', 'shadow_dark',
 '{"type":"void","rarity":"common","traits":["who corrupts magical artifacts"],"palette":"shadow_dark","enhanced":true,"generation_seed":"anti-relic-rust-hex_common_void"}',
 'anti-relic-rust-hex_common_void', 0, 228, 2, NULL, NULL,
 'spell', false, false, 'relic_breakers', '{"standard"}', 'Until your next turn, the enemy relic''s effects are disabled. Draw a card.', '{}',
 '{}', 'https://example.com/potato-art/potato_0228.png', 'standard', '"Even the mightiest artifacts rust in time." - Decay Wisdom', '2025-01-24', '{"Spell","Relic-Breaker"}',
 40, 5, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0228","rarity":"common","element":"void","base_color":"hsl(270 30% 25%)","accent_color":"hsl(280 50% 35%)"}',
 '{"rarity_effects":{"basic":true},"element_effects":{"shadows":true,"ethereal":true,"void_portals":false},"unique_features":{"personality":"cute","spot_pattern":2,"eye_variation":0,"smile_variation":1}}',
 NULL, '{"Spell:DisableEnemyRelic","Spell:Draw"}', NULL, 0, 0, NULL, NULL),

(gen_random_uuid(), 'potato_0229', 'Scrapling Saboteur', 'A brave little fiery potato who sabotages enemy artifacts. Though small, they dream of great adventures and practice their fire abilities every day, hoping to become a legendary hero someday.',
 'fire', 'who sabotages enemy artifacts', 'sabotaging', 'common', 'flame_orange',
 '{"type":"fire","rarity":"common","traits":["who sabotages enemy artifacts"],"palette":"flame_orange","enhanced":true,"generation_seed":"anti-relic-scrapling-saboteur_common_fire"}',
 'anti-relic-scrapling-saboteur_common_fire', 0, 229, 2, 2, 2,
 'unit', false, false, 'relic_breakers', '{"standard"}', 'Battlecry: Destroy an enemy relic that costs (3) or less. If you destroy a relic, this gains +1/+1. Otherwise, draw a card.', '{}',
 '{}', 'https://example.com/potato-art/potato_0229.png', 'standard', '"Where there''s fire, there''s destruction." - Saboteur Motto', '2025-01-24', '{"Battlecry","Relic-Breaker"}',
 40, 5, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0229","rarity":"common","element":"fire","base_color":"hsl(15 90% 65%)","accent_color":"hsl(45 100% 70%)"}',
 '{"rarity_effects":{"basic":true},"element_effects":{"embers":false,"flames":true,"heat_glow":true},"unique_features":{"personality":"cute","spot_pattern":3,"eye_variation":2,"smile_variation":0}}',
 'warrior', '{"Battlecry:ConditionalRelicDestroy","Battlecry:ConditionalBuff"}', NULL, 0, 0, NULL, NULL),

(gen_random_uuid(), 'potato_0230', 'Frostbind Edict', 'A brave little crystalline potato who freezes enemies and disrupts magic. Though small, they dream of great adventures and practice their ice abilities every day, hoping to become a legendary hero someday.',
 'ice', 'who freezes enemies and disrupts magic', 'frostbinding', 'common', 'frost_blue',
 '{"type":"ice","rarity":"common","traits":["who freezes enemies and disrupts magic"],"palette":"frost_blue","enhanced":true,"generation_seed":"anti-relic-frostbind-edict_common_ice"}',
 'anti-relic-frostbind-edict_common_ice', 0, 230, 2, NULL, NULL,
 'spell', false, false, 'relic_breakers', '{"standard"}', 'Freeze an enemy unit. If the enemy controls a relic, that relic''s effects are disabled until your next turn.', '{}',
 '{}', 'https://example.com/potato-art/potato_0230.png', 'standard', '"Ice binds both flesh and magic." - Frost Binding', '2025-01-24', '{"Spell","Relic-Breaker"}',
 40, 5, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0230","rarity":"common","element":"ice","base_color":"hsl(200 80% 75%)","accent_color":"hsl(180 90% 85%)"}',
 '{"rarity_effects":{"basic":true},"element_effects":{"crystals":true,"icy_glow":true,"snowflakes":true},"unique_features":{"personality":"cute","spot_pattern":0,"eye_variation":1,"smile_variation":1}}',
 NULL, '{"Spell:FreezeTarget","Spell:ConditionalDisableRelic"}', NULL, 0, 0, NULL, NULL),

-- Uncommon Cards (2)
(gen_random_uuid(), 'potato_0231', 'Static Disruptor', 'A talented voltaic potato who disrupts magical energies. While still learning the deeper mysteries of lightning magic, they show great promise and help their community with enthusiasm.',
 'lightning', 'who disrupts magical energies', 'disrupting', 'uncommon', 'storm_purple',
 '{"type":"lightning","rarity":"uncommon","traits":["who disrupts magical energies"],"palette":"storm_purple","enhanced":true,"generation_seed":"anti-relic-static-disruptor_uncommon_lightning"}',
 'anti-relic-static-disruptor_uncommon_lightning', 0, 231, 4, NULL, NULL,
 'spell', false, false, 'relic_breakers', '{"standard"}', 'The enemy relic''s effects are disabled until the end of your next turn. Deal 1 damage to all enemy units.', '{}',
 '{}', 'https://example.com/potato-art/potato_0231.png', 'standard', '"Lightning disrupts all forms of magic." - Storm Theory', '2025-01-24', '{"Spell","Relic-Breaker"}',
 100, 20, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0231","rarity":"uncommon","element":"lightning","base_color":"hsl(280 70% 70%)","accent_color":"hsl(290 90% 80%)"}',
 '{"rarity_effects":{"accessory":false},"element_effects":{"sparks":true,"energy_glow":true,"electric_field":true},"unique_features":{"personality":"standard","spot_pattern":2,"eye_variation":1,"smile_variation":0}}',
 NULL, '{"Spell:DisableEnemyRelicExtended","Spell:AoE1AllEnemies"}', NULL, 1, 0, NULL, NULL),

(gen_random_uuid(), 'potato_0232', 'Dawn Acolyte', 'A talented luminous potato who purifies corruption. While still learning the deeper mysteries of light magic, they show great promise and help their community with enthusiasm.',
 'light', 'who purifies corruption', 'purifying', 'uncommon', 'golden_yellow',
 '{"type":"light","rarity":"uncommon","traits":["who purifies corruption"],"palette":"golden_yellow","enhanced":true,"generation_seed":"anti-relic-dawn-acolyte_uncommon_light"}',
 'anti-relic-dawn-acolyte_uncommon_light', 0, 232, 3, 3, 3,
 'unit', false, false, 'relic_breakers', '{"standard"}', 'Battlecry: If the enemy controls a relic, destroy it and restore 2 Health to your hero. Otherwise, draw a card.', '{}',
 '{}', 'https://example.com/potato-art/potato_0232.png', 'standard', '"Dawn breaks all darkness." - Morning Prayer', '2025-01-24', '{"Battlecry","Relic-Breaker"}',
 100, 20, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0232","rarity":"uncommon","element":"light","base_color":"hsl(50 90% 80%)","accent_color":"hsl(60 100% 90%)"}',
 '{"rarity_effects":{"accessory":false},"element_effects":{"rays":true,"stars":true,"radiance":true},"unique_features":{"personality":"standard","spot_pattern":1,"eye_variation":0,"smile_variation":1}}',
 'healer', '{"Battlecry:ConditionalRelicDestroy","Battlecry:ConditionalHeal"}', NULL, 0, 2, NULL, NULL),

-- Rare Cards (2)
(gen_random_uuid(), 'potato_0233', 'Shatter Lens', 'A skilled crystalline potato warrior who shatters magical focus. Known throughout the kingdom for their expertise in ice combat and their unwavering dedication to protecting the innocent.',
 'ice', 'who shatters magical focus', 'shattering', 'rare', 'frost_blue',
 '{"type":"ice","rarity":"rare","traits":["who shatters magical focus"],"palette":"frost_blue","enhanced":true,"generation_seed":"anti-relic-shatter-lens_rare_ice"}',
 'anti-relic-shatter-lens_rare_ice', 0, 233, 4, NULL, NULL,
 'structure', false, false, 'relic_breakers', '{"standard"}', 'While this is alive, the enemy relic''s effects are disabled.', '{}',
 '{}', 'https://example.com/potato-art/potato_0233.png', 'standard', '"Through shattered lens, magic becomes mundane." - Ice Sage', '2025-01-24', '{"Structure","Relic-Breaker"}',
 400, 100, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0233","rarity":"rare","element":"ice","base_color":"hsl(200 80% 75%)","accent_color":"hsl(180 90% 85%)"}',
 '{"rarity_effects":{"glow":true,"accessory":false},"element_effects":{"crystals":true,"icy_glow":true,"snowflakes":true},"unique_features":{"personality":"standard","spot_pattern":3,"eye_variation":2,"smile_variation":1}}',
 NULL, '{"Structure:DisableEnemyRelic"}', NULL, 0, 0, 6, NULL),

(gen_random_uuid(), 'potato_0234', 'Molten Crusher', 'A skilled blazing potato warrior who crushes magical artifacts. Known throughout the kingdom for their expertise in fire combat and their unwavering dedication to protecting the innocent.',
 'fire', 'who crushes magical artifacts', 'molten', 'rare', 'flame_orange',
 '{"type":"fire","rarity":"rare","traits":["who crushes magical artifacts"],"palette":"flame_orange","enhanced":true,"generation_seed":"anti-relic-molten-crusher_rare_fire"}',
 'anti-relic-molten-crusher_rare_fire', 0, 234, 5, 5, 5,
 'unit', false, false, 'relic_breakers', '{"standard"}', 'Charge. After this attacks, destroy the enemy relic. If there is no enemy relic, this gains +2 ATK this turn instead.', '{}',
 '{}', 'https://example.com/potato-art/potato_0234.png', 'standard', '"Molten fury melts all obstacles." - Crusher''s Creed', '2025-01-24', '{"Relic-Breaker"}',
 400, 100, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0234","rarity":"rare","element":"fire","base_color":"hsl(15 90% 65%)","accent_color":"hsl(45 100% 70%)"}',
 '{"rarity_effects":{"glow":true,"accessory":true},"element_effects":{"embers":true,"flames":true,"heat_glow":true},"unique_features":{"personality":"standard","spot_pattern":0,"eye_variation":1,"smile_variation":0}}',
 'warrior', '{"Charge","AfterAttack:DestroyEnemyRelic","AfterAttack:ConditionalBuff"}', NULL, 0, 0, NULL, NULL),

-- Legendary Card (1)
(gen_random_uuid(), 'potato_0235', 'Eternal Nullifier', 'A legendary storm-born potato hero who nullifies all magic. Their deeds are sung in epic ballads across the land, inspiring countless others to greatness in the art of lightning mastery.',
 'lightning', 'who nullifies all magic', 'nullifying', 'legendary', 'storm_purple',
 '{"type":"lightning","rarity":"legendary","traits":["who nullifies all magic"],"palette":"storm_purple","enhanced":true,"generation_seed":"anti-relic-eternal-nullifier_legendary_lightning"}',
 'anti-relic-eternal-nullifier_legendary_lightning', 0, 235, 7, 6, 8,
 'unit', true, false, 'relic_breakers', '{"standard"}', 'Taunt. While this is alive, the enemy relic''s effects are disabled. Battlecry: Draw a card.', '{}',
 '{}', 'https://example.com/potato-art/potato_0235.png', 'standard', '"In the presence of eternity, all magic fades." - Nullifier''s Truth', '2025-01-24', '{"Taunt","Battlecry","Relic-Breaker"}',
 1600, 400, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0235","rarity":"legendary","element":"lightning","base_color":"hsl(280 70% 70%)","accent_color":"hsl(290 90% 80%)"}',
 '{"rarity_effects":{"hat":true,"aura":true,"glow":true,"eye_glow":false},"element_effects":{"sparks":true,"energy_glow":true,"electric_field":true},"unique_features":{"personality":"heroic","spot_pattern":2,"eye_variation":0,"smile_variation":1}}',
 'tank', '{"Taunt","Structure:DisableEnemyRelic","Battlecry:Draw"}', NULL, 0, 0, NULL, NULL),

-- Exotic Card (1)
(gen_random_uuid(), 'potato_0236', 'Sanctified Devourer', 'A transcendent radiant potato of immeasurable power, who devours and sanctifies artifacts. Legends say this ancient being shaped the very foundations of the light realm and commands forces beyond mortal comprehension.',
 'light', 'who devours and sanctifies artifacts', 'sanctified', 'exotic', 'golden_yellow',
 '{"type":"light","rarity":"exotic","traits":["who devours and sanctifies artifacts"],"palette":"golden_yellow","enhanced":true,"generation_seed":"anti-relic-sanctified-devourer_exotic_light"}',
 'anti-relic-sanctified-devourer_exotic_light', 0, 236, 6, 6, 6,
 'unit', false, true, 'relic_breakers', '{"standard"}', 'Battlecry: Destroy the enemy relic. Then create a copy of it under your control and activate it (replacing your relic if you already have one). Restore 3 Health to your hero.', '{}',
 '{}', 'https://example.com/potato-art/potato_0236.png', 'standard', '"What was corrupt becomes pure. What was enemy becomes ally." - Sanctification Codex', '2025-01-24', '{"Battlecry","Relic-Breaker","Dual-Type"}',
 3200, 1600, '[]', '', '{}', '{}',
 now(), '{"seed":"potato_0236","rarity":"exotic","element":"light","base_color":"hsl(50 90% 80%)","accent_color":"hsl(60 100% 90%)"}',
 '{"rarity_effects":{"aura":true,"crown":true,"eye_glow":true,"particles":true},"element_effects":{"rays":true,"stars":true,"radiance":true},"unique_features":{"personality":"divine","spot_pattern":1,"eye_variation":2,"smile_variation":0}}',
 'warrior', '{"Battlecry:DestroyEnemyRelic","Battlecry:CopyEnemyRelic","Battlecry:Heal3"}', NULL, 0, 3, NULL, NULL);

-- Update rarity counts
UPDATE public.rarity_distribution 
SET current_count = (SELECT COUNT(*) FROM public.card_complete WHERE rarity = rarity_distribution.rarity);

-- Success message
SELECT 'Anti-relic card expansion added! 10 new cards for relic counterplay.' as status;