-- Complete with Void type unique names and descriptions

-- Void type names and descriptions
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'Void Lord Nullifier'
                WHEN 1 THEN 'Shadow Emperor Abyssius'
                WHEN 2 THEN 'Darkness God Voidwalker'
                WHEN 3 THEN 'Cosmic Horror Netherspud'
                ELSE 'Infinite Void Mastermind'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'Captain Blackhole'
                WHEN 1 THEN 'Shadow Queen Umbra'
                WHEN 2 THEN 'Void General Darkstar'
                WHEN 3 THEN 'Abyss Guardian'
                WHEN 4 THEN 'Baron von Voidface'
                WHEN 5 THEN 'Duchess Nightfall'
                WHEN 6 THEN 'Eclipse Commander'
                ELSE 'Dark Matter Knight'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'Spooky McVoidface'
                WHEN 1 THEN 'Shadow Sniper'
                WHEN 2 THEN 'Void Ninja Spud'
                WHEN 3 THEN 'Black Hole Warrior'
                WHEN 4 THEN 'Darkness Defender'
                WHEN 5 THEN 'Eclipse Scout'
                WHEN 6 THEN 'Shadow Fighter'
                WHEN 7 THEN 'Midnight Explorer'
                WHEN 8 THEN 'Void Walker'
                WHEN 9 THEN 'Dark Energy Master'
                WHEN 10 THEN 'Space-Time Bender'
                ELSE 'Reality Glitch'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'Shadowy McGillicuddy'
                WHEN 1 THEN 'Voidbert the Mysterious'
                WHEN 2 THEN 'Darkling McBlackface'
                WHEN 3 THEN 'Empty Potato Pete'
                WHEN 4 THEN 'Spooky the Hollow'
                WHEN 5 THEN 'Black Out Patrick'
                WHEN 6 THEN 'Shadow Sammie'
                WHEN 7 THEN 'Midnight Sally'
                WHEN 8 THEN 'Dark Mode Dan'
                WHEN 9 THEN 'Void Side Barry'
                WHEN 10 THEN 'Night Light Larry'
                WHEN 11 THEN 'Eclipse Steve'
                WHEN 12 THEN 'Blackout Shane'
                WHEN 13 THEN 'Power Void Tom'
                ELSE 'Night Vision Wendy'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'Tiny Shadow Chip'
                WHEN 1 THEN 'Baby Darkness'
                WHEN 2 THEN 'Little Void Spark'
                WHEN 3 THEN 'Dark Spud Jr'
                WHEN 4 THEN 'Shadow Pip'
                WHEN 5 THEN 'Whisper Squeaky'
                WHEN 6 THEN 'Void Pop Penny'
                WHEN 7 THEN 'Shadowy McShade'
                WHEN 8 THEN 'Dark Thoughts Bob'
                WHEN 9 THEN 'Gloom Chip'
                WHEN 10 THEN 'Midnight Sammy'
                WHEN 11 THEN 'Black Hole Tim'
                WHEN 12 THEN 'Dark Veggie Val'
                WHEN 13 THEN 'Dim Down Phil'
                WHEN 14 THEN 'Shadow King Kevin'
                WHEN 15 THEN 'Dark Day Sophie'
                WHEN 16 THEN 'Void Space Rick'
                WHEN 17 THEN 'Shadow Baby'
                WHEN 18 THEN 'Blackened Fries Frank'
                ELSE 'Void Age Amy'
            END
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'An entity from the space between spaces, who nullifies existence itself and once accidentally erased Tuesday from the calendar.'
                WHEN 1 THEN 'The supreme ruler of the shadow realm, who commands darkness across infinite dimensions and has excellent night vision.'
                WHEN 2 THEN 'A cosmic wanderer who steps between realities and occasionally gets lost in the space between thoughts.'
                WHEN 3 THEN 'An incomprehensible horror from beyond the stars, who is actually quite friendly once you get to know them.'
                ELSE 'The living embodiment of the void, so empty that even emptiness feels jealous of their vast nothingness.'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'A fearless captain who navigates through black holes and has the most comprehensive travel insurance policy.'
                WHEN 1 THEN 'The enigmatic queen of the Shadow Court, famous for her mysterious smile and ability to hide in plain sight.'
                WHEN 2 THEN 'A strategic general who uses darkness for tactical advantage and has revolutionized stealth warfare.'
                WHEN 3 THEN 'The eternal guardian of the cosmic abyss, ensuring that infinite darkness remains properly organized.'
                WHEN 4 THEN 'A sophisticated nobleman who speaks in whispers and always knows more than he lets on.'
                WHEN 5 THEN 'The graceful duchess of the Twilight Realm, known for her evening soirées and excellent mood lighting.'
                WHEN 6 THEN 'A cosmic commander who can eclipse suns but always makes sure planets get adequate warning.'
                ELSE 'A noble knight of the Dark Matter Order, sworn to protect the universe from excessive brightness.'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'A perpetually spooky potato who hides in shadows and enjoys surprising people at appropriate times.'
                WHEN 1 THEN 'An expert marksman who shoots darkness bolts and can hit targets they can\'t even see.'
                WHEN 2 THEN 'A stealthy warrior trained in void-jutsu, can disappear into nothingness and sometimes forgets to come back.'
                WHEN 3 THEN 'A gravity-defying potato who escaped from a physics textbook and now bends space-time for fun.'
                WHEN 4 THEN 'A protective defender who absorbs negative energy and converts it into mild disappointment.'
                WHEN 5 THEN 'An astronomical potato who predicts eclipses and always brings proper viewing equipment.'
                WHEN 6 THEN 'A mysterious fighter who battles in complete darkness and somehow never trips over anything.'
                WHEN 7 THEN 'An adventurous explorer who travels through the void of space and collects interesting dark matter.'
                WHEN 8 THEN 'A dimensional traveler who walks between worlds and occasionally gets their address confused.'
                WHEN 9 THEN 'A physicist potato who studies dark energy and publishes papers that nobody can understand.'
                WHEN 10 THEN 'A reality-bending potato who can alter the fabric of space-time but mostly uses it to skip lines.'
                ELSE 'A glitchy potato who exists in the spaces between code and occasionally causes minor system errors.'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'A mysterious potato who lurks in shadows and gives cryptic advice that\'s surprisingly helpful.'
                WHEN 1 THEN 'A void-touched potato who phases in and out of existence and has trouble with solid food.'
                WHEN 2 THEN 'A dark potato who absorbs light and makes rooms mysteriously cozy and intimate.'
                WHEN 3 THEN 'A hollow potato who contains infinite space and often loses small objects in their personal void.'
                WHEN 4 THEN 'A spooky potato who enjoys Halloween year-round and has an impressive collection of ghost stories.'
                WHEN 5 THEN 'A practical potato who causes power outages during peak usage to help conserve energy.'
                WHEN 6 THEN 'A shadowy potato who follows people around and provides excellent dramatic silhouettes.'
                WHEN 7 THEN 'A nocturnal potato who prefers midnight activities and knows all the best 24-hour diners.'
                WHEN 8 THEN 'A tech-savvy potato who enables dark mode on everything and advocates for eye health.'
                WHEN 9 THEN 'a pessimistic potato who points out problems but also helps find solutions.'
                WHEN 10 THEN 'A dim potato who provides subtle ambient lighting for romantic dinners.'
                WHEN 11 THEN 'An astronomical potato who celebrates solar eclipses and organizes viewing parties.'
                WHEN 12 THEN 'A theatrical potato who creates dramatic blackouts for surprise reveals.'
                WHEN 13 THEN 'A space-conscious potato who creates pockets of void for convenient storage.'
                ELSE 'A practical potato who helps people see in the dark and navigate by starlight.'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'A tiny shadow that dreams of becoming a mighty black hole someday.'
                WHEN 1 THEN 'A baby darkness who\'s still learning how to be properly mysterious.'
                WHEN 2 THEN 'A little void spark who creates small pockets of nothingness and finds it very satisfying.'
                WHEN 3 THEN 'A junior dark potato who\'s practicing their shadowing techniques in bright corners.'
                WHEN 4 THEN 'A little shadow who follows people around and occasionally plays peek-a-boo.'
                WHEN 5 THEN 'A squeaky potato who whispers important secrets that only cats can hear.'
                WHEN 6 THEN 'A penny-sized potato who\'s dark, mysterious, and makes everything slightly more dramatic.'
                WHEN 7 THEN 'A shady potato who can\'t decide if they want to be a shadow or a really dark curtain.'
                WHEN 8 THEN 'A thoughtful potato who provides deep contemplation and occasional existential crises.'
                WHEN 9 THEN 'A gloomy potato who brings a touch of melancholy and surprising emotional depth.'
                WHEN 10 THEN 'A midnight potato who celebrates the quiet hours and provides excellent company for insomniacs.'
                WHEN 11 THEN 'A small black hole who\'s very polite and only absorbs things with permission.'
                WHEN 12 THEN 'A health-conscious potato who promotes the benefits of vitamin D deficiency (just kidding).'
                WHEN 13 THEN 'A dimming potato who helps people wind down for sleep and reduces screen brightness.'
                WHEN 14 THEN 'A small but mighty potato who insists they\'re the darkest thing around.'
                WHEN 15 THEN 'A gloomy potato who finds beauty in rainy days and excessive brooding.'
                WHEN 16 THEN 'A spacious potato who provides personal void storage and interdimensional parking.'
                WHEN 17 THEN 'An infant potato who\'s new to existing and finds the concept of nothingness fascinating.'
                WHEN 18 THEN 'A fast-food potato who achieved their dreams of being perfectly blackened.'
                ELSE 'A ancient potato who remembers the time before light and won\'t stop talking about how quiet it was.'
            END
    END
WHERE potato_type = 'void';

-- Final verification and summary
DO $$
DECLARE
    total_cards INTEGER;
    unique_names INTEGER;
    ice_count INTEGER;
    fire_count INTEGER;
    lightning_count INTEGER;
    light_count INTEGER;
    void_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO total_cards FROM card_complete;
    SELECT COUNT(DISTINCT name) INTO unique_names FROM card_complete;
    SELECT COUNT(*) INTO ice_count FROM card_complete WHERE potato_type = 'ice';
    SELECT COUNT(*) INTO fire_count FROM card_complete WHERE potato_type = 'fire';
    SELECT COUNT(*) INTO lightning_count FROM card_complete WHERE potato_type = 'lightning';
    SELECT COUNT(*) INTO light_count FROM card_complete WHERE potato_type = 'light';
    SELECT COUNT(*) INTO void_count FROM card_complete WHERE potato_type = 'void';
    
    RAISE NOTICE '=== CARD UNIQUENESS & ELEMENTAL DISTRIBUTION ===';
    RAISE NOTICE 'Total cards: %', total_cards;
    RAISE NOTICE 'Unique names: %', unique_names;
    RAISE NOTICE 'Ice cards: %', ice_count;
    RAISE NOTICE 'Fire cards: %', fire_count;
    RAISE NOTICE 'Lightning cards: %', lightning_count;
    RAISE NOTICE 'Light cards: %', light_count;
    RAISE NOTICE 'Void cards: %', void_count;
    
    IF unique_names = total_cards THEN
        RAISE NOTICE '✅ SUCCESS: All card names are now unique!';
    ELSE
        RAISE NOTICE '❌ WARNING: % duplicate names still exist', total_cards - unique_names;
    END IF;
END
$$;