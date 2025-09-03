-- Make all card names unique and update to elemental types
-- Create creative, funny, unique names and descriptions

-- 1. First, let's see what we're working with
DO $$
DECLARE
    total_cards INTEGER;
    unique_names INTEGER;
    duplicate_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO total_cards FROM card_complete;
    SELECT COUNT(DISTINCT name) INTO unique_names FROM card_complete;
    duplicate_count := total_cards - unique_names;
    
    -- This will show in query results if run directly
    RAISE NOTICE 'Total cards: %, Unique names: %, Duplicates: %', total_cards, unique_names, duplicate_count;
END
$$;

-- 2. Update potato_type to elemental types randomly
UPDATE potato_registry 
SET potato_type = CASE 
    WHEN RANDOM() < 0.2 THEN 'ice'
    WHEN RANDOM() < 0.4 THEN 'fire' 
    WHEN RANDOM() < 0.6 THEN 'lightning'
    WHEN RANDOM() < 0.8 THEN 'light'
    ELSE 'void'
END;

-- 3. Create unique names for all cards based on their rarity and new type
-- Ice type names and descriptions
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'Frostmourne the Eternal'
                WHEN 1 THEN 'Arctic Overlord Spud'
                WHEN 2 THEN 'Glacier Emperor Tater'
                WHEN 3 THEN 'Blizzard Supreme Commander'
                ELSE 'Absolute Zero Potato Lord'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'Captain Frostbite'
                WHEN 1 THEN 'Ice Queen Tuberia'
                WHEN 2 THEN 'Snowstorm General'
                WHEN 3 THEN 'Frozen Throne Guardian'
                WHEN 4 THEN 'Icicle Baron Von Spud'
                WHEN 5 THEN 'Permafrost Duchess'
                WHEN 6 THEN 'Hailstorm Commander'
                ELSE 'Glacier Knight Taterion'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'Chilly McFreeze'
                WHEN 1 THEN 'Snowball Sniper'
                WHEN 2 THEN 'Frost Ninja Spud'
                WHEN 3 THEN 'Ice Cube Warrior'
                WHEN 4 THEN 'Slush Puppy Defender'
                WHEN 5 THEN 'Blizzard Scout'
                WHEN 6 THEN 'Frozen Peas Fighter'
                WHEN 7 THEN 'Arctic Explorer Tate'
                WHEN 8 THEN 'Snowman Builder'
                WHEN 9 THEN 'Ice Cream Vendor'
                WHEN 10 THEN 'Winter Olympic Spud'
                ELSE 'Penguin Whisperer'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'Cool Guy McGillicuddy'
                WHEN 1 THEN 'Brrr-nard the Shivering'
                WHEN 2 THEN 'Slippy McIceface'
                WHEN 3 THEN 'Chilled Potato Pete'
                WHEN 4 THEN 'Frosty the Slowman'
                WHEN 5 THEN 'Ice Pack Patrick'
                WHEN 6 THEN 'Snowflake Sammie'
                WHEN 7 THEN 'Polar Bear Hugger'
                WHEN 8 THEN 'Frozen Dinner Dan'
                WHEN 9 THEN 'Cold Shoulder Sally'
                WHEN 10 THEN 'Icicle Licker Larry'
                WHEN 11 THEN 'Brain Freeze Barry'
                WHEN 12 THEN 'Shaved Ice Shane'
                WHEN 13 THEN 'Snowball Fight Steve'
                ELSE 'Winter Coat Wendy'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'Tiny Frost Chip'
                WHEN 1 THEN 'Baby Snowflake'
                WHEN 2 THEN 'Little Ice Cube'
                WHEN 3 THEN 'Chilly Willy Jr'
                WHEN 4 THEN 'Frost Bite Pip'
                WHEN 5 THEN 'Snow Angel Squeaky'
                WHEN 6 THEN 'Ice Pop Penny'
                WHEN 7 THEN 'Slushy McSlush'
                WHEN 8 THEN 'Freezer Burn Bob'
                WHEN 9 THEN 'Cold Snap Chip'
                WHEN 10 THEN 'Snowdrift Sammy'
                WHEN 11 THEN 'Ice Tray Tim'
                WHEN 12 THEN 'Frozen Veggie Val'
                WHEN 13 THEN 'Chill Pill Phil'
                WHEN 14 THEN 'Frost King Kevin'
                WHEN 15 THEN 'Snow Day Sophie'
                WHEN 16 THEN 'Ice Rink Rick'
                WHEN 17 THEN 'Blizzard Baby'
                WHEN 18 THEN 'Frozen Fries Frank'
                ELSE 'Ice Age Amy'
            END
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'An ancient ice potato of immeasurable power, said to have frozen entire kingdoms with a single glare. Legends speak of its ability to turn summer into eternal winter.'
                WHEN 1 THEN 'The supreme ruler of all frozen realms, this majestic potato commands blizzards and controls the very essence of cold itself.'
                WHEN 2 THEN 'A colossal ice emperor who sits upon a throne of permafrost, wielding the power to create glaciers with mere thoughts.'
                WHEN 3 THEN 'The ultimate military strategist of the frozen wastes, capable of commanding armies of ice soldiers in the great Potato Wars.'
                ELSE 'A potato so cold it defies the laws of physics, existing at absolute zero while somehow remaining alive and very, very grumpy about it.'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'A fearless captain who sailed the seven frozen seas and discovered the legendary Ice Potato Islands.'
                WHEN 1 THEN 'The elegant ruler of the Crystal Palace, known for her stunning ice sculptures and terrible dad jokes.'
                WHEN 2 THEN 'A brilliant military tactician who once defeated an army using nothing but strategically placed snowballs.'
                WHEN 3 THEN 'The stoic guardian of the ancient Frozen Throne, who has never lost a staring contest in 1000 years.'
                WHEN 4 THEN 'A distinguished nobleman who insists on wearing a monocle made of ice and speaks only in freeze puns.'
                WHEN 5 THEN 'The graceful duchess of the Snowflake Court, famous for her ice dancing and frozen tea parties.'
                WHEN 6 THEN 'A storm-commanding warrior who can summon hailstorms but always apologizes to anyone who gets hit.'
                ELSE 'A noble knight of the Glacier Order, sworn to protect all potatoes from the horrors of being served warm.'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'A perpetually cold potato who wears seventeen sweaters and still complains about the temperature.'
                WHEN 1 THEN 'An expert marksman with snowballs, holds the world record for longest-distance snowball fight victory.'
                WHEN 2 THEN 'A stealthy warrior trained in the ancient art of ice-jitsu, can disappear in a cloud of snow.'
                WHEN 3 THEN 'A cubic potato who rolled out of an ice tray and decided to become a professional wrestler.'
                WHEN 4 THEN 'A loyal companion who follows you around making slush sounds and occasionally trips people with ice patches.'
                WHEN 5 THEN 'A weather-tracking potato who can predict snow days with 100% accuracy but only tells their friends.'
                WHEN 6 THEN 'A brave fighter who believes all vegetables should stick together, especially when frozen.'
                WHEN 7 THEN 'An adventurous potato who once got lost in a freezer for three months and came back with amazing stories.'
                WHEN 8 THEN 'A creative potato who builds elaborate snow forts and hosts epic snowball tournaments.'
                WHEN 9 THEN 'A friendly vendor who sells the most delicious ice cream, despite being a potato themselves.'
                WHEN 10 THEN 'An athletic potato training for the Winter Olympics in the exciting sport of Synchronized Sliding.'
                ELSE 'A mysterious potato who communicates exclusively through penguin noises and somehow makes it work.'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'A laid-back potato who thinks everything is "totally chill, dude" and gives great life advice.'
                WHEN 1 THEN 'A nervous potato who shivers constantly but insists it\'s not from the cold, just excitement.'
                WHEN 2 THEN 'A clumsy potato who slides around on ice but somehow always lands on their feet... eventually.'
                WHEN 3 THEN 'A cool customer who never loses composure, even when literally frozen solid.'
                WHEN 4 THEN 'A slow-moving potato who takes their time with everything but gives the warmest (coldest?) hugs.'
                WHEN 5 THEN 'A helpful potato who always carries an ice pack and offers it to anyone with minor injuries.'
                WHEN 6 THEN 'A delicate potato who believes they\'re unique and special, just like every other snowflake.'
                WHEN 7 THEN 'A cuddly potato who gives bear hugs that are surprisingly refreshing on hot days.'
                WHEN 8 THEN 'A practical potato who\'s always prepared with frozen meals and zero cooking skills.'
                WHEN 9 THEN 'An aloof potato who literally gives everyone the cold shoulder but means well.'
                WHEN 10 THEN 'A sweet potato (literally) who enjoys frozen treats more than anyone should.'
                WHEN 11 THEN 'A confused potato who gets brain freeze from thinking too hard about simple problems.'
                WHEN 12 THEN 'A tropical-themed potato who moved north and deeply regrets all their life choices.'
                WHEN 13 THEN 'A competitive potato who challenges everyone to snowball fights and somehow always wins.'
                ELSE 'A fashionable potato who insists winter coats are a year-round necessity.'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'A tiny chip of ice that dreams of becoming a mighty glacier someday.'
                WHEN 1 THEN 'A baby snowflake who\'s still learning how to be unique and special.'
                WHEN 2 THEN 'A small ice cube who escaped from a drink and is now living their best life.'
                WHEN 3 THEN 'A junior version of a famous ice warrior, still working on their chilling techniques.'
                WHEN 4 THEN 'A little potato who gives tiny frost bites that are more like gentle cooling sensations.'
                WHEN 5 THEN 'A squeaky clean potato who makes snow angels and believes in the magic of winter.'
                WHEN 6 THEN 'A penny-sized potato who\'s sweet, cold, and disappears way too quickly.'
                WHEN 7 THEN 'A liquidy potato who can\'t decide if they want to be a drink or a dessert.'
                WHEN 8 THEN 'A slightly damaged potato who survived the freezer incident and has stories to tell.'
                WHEN 9 THEN 'A quick-tempered potato who causes sudden cold snaps when annoyed.'
                WHEN 10 THEN 'A drifty potato who goes wherever the wind takes them and seems happy about it.'
                WHEN 11 THEN 'A rectangular potato who\'s very organized and believes everything has its place.'
                WHEN 12 THEN 'A health-conscious potato who promotes the benefits of eating frozen vegetables.'
                WHEN 13 THEN 'A pharmaceutical potato who claims to cure everything but mostly just makes you cold.'
                WHEN 14 THEN 'A small but mighty potato who insists they\'re royalty despite all evidence to the contrary.'
                WHEN 15 THEN 'A school-loving potato who celebrates every snow day with excessive enthusiasm.'
                WHEN 16 THEN 'A sporty potato who spends all day at the ice rink and has the bruises to prove it.'
                WHEN 17 THEN 'A infant potato who\'s new to the world and finds everything absolutely fascinating.'
                WHEN 18 THEN 'A fast-food potato who achieved their dreams of being frozen for preservation.'
                ELSE 'A prehistoric potato who remembers the last ice age and won\'t stop talking about it.'
            END
    END
WHERE potato_type = 'ice';

-- 4. Fire type names and descriptions
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'Inferno Lord Blazewick'
                WHEN 1 THEN 'Phoenix Emperor Scorchus'
                WHEN 2 THEN 'Volcano King Magmatron'
                WHEN 3 THEN 'Solar Flare Overlord'
                ELSE 'Eternal Flame Master'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'Captain Burnface'
                WHEN 1 THEN 'Fire Queen Cinderia'
                WHEN 2 THEN 'Blaze General Smokestack'
                WHEN 3 THEN 'Molten Core Guardian'
                WHEN 4 THEN 'Baron von Sizzle'
                WHEN 5 THEN 'Duchess Flamesworth'
                WHEN 6 THEN 'Wildfire Commander'
                ELSE 'Lava Knight Ignitius'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'Spicy McFireFace'
                WHEN 1 THEN 'Ember Sniper'
                WHEN 2 THEN 'Flame Ninja Potato'
                WHEN 3 THEN 'Hot Sauce Warrior'
                WHEN 4 THEN 'Chili Pepper Defender'
                WHEN 5 THEN 'Bonfire Scout'
                WHEN 6 THEN 'Jalapeño Fighter'
                WHEN 7 THEN 'Desert Explorer Burns'
                WHEN 8 THEN 'Campfire Storyteller'
                WHEN 9 THEN 'BBQ Master Chef'
                WHEN 10 THEN 'Summer Olympic Torch'
                ELSE 'Dragon Tamer Tate'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'Toasty McGillicuddy'
                WHEN 1 THEN 'Burnard the Crispy'
                WHEN 2 THEN 'Sizzly McHotface'
                WHEN 3 THEN 'Heated Potato Pete'
                WHEN 4 THEN 'Smoky the Coalman'
                WHEN 5 THEN 'Heat Pack Patrick'
                WHEN 6 THEN 'Spark Plug Sammie'
                WHEN 7 THEN 'Sun Lover Sally'
                WHEN 8 THEN 'Oven Fresh Dan'
                WHEN 9 THEN 'Warm Heart Barry'
                WHEN 10 THEN 'Fire Starter Larry'
                WHEN 11 THEN 'Hot Head Steve'
                WHEN 12 THEN 'Flame Grilled Shane'
                WHEN 13 THEN 'Torch Bearer Tom'
                ELSE 'Summer Dress Wendy'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'Tiny Ember Chip'
                WHEN 1 THEN 'Baby Flame'
                WHEN 2 THEN 'Little Fire Starter'
                WHEN 3 THEN 'Hot Potato Jr'
                WHEN 4 THEN 'Spark Bite Pip'
                WHEN 5 THEN 'Fire Angel Squeaky'
                WHEN 6 THEN 'Hot Pop Penny'
                WHEN 7 THEN 'Steamy McSteam'
                WHEN 8 THEN 'Crispy Burn Bob'
                WHEN 9 THEN 'Heat Wave Chip'
                WHEN 10 THEN 'Smoke Signal Sammy'
                WHEN 11 THEN 'Fire Pit Tim'
                WHEN 12 THEN 'Grilled Veggie Val'
                WHEN 13 THEN 'Hot Sauce Phil'
                WHEN 14 THEN 'Flame King Kevin'
                WHEN 15 THEN 'Sunny Day Sophie'
                WHEN 16 THEN 'Fire Ring Rick'
                WHEN 17 THEN 'Blazing Baby'
                WHEN 18 THEN 'Flaming Fries Frank'
                ELSE 'Fire Age Amy'
            END
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'A primordial fire potato whose flames have burned since the dawn of time, capable of igniting entire worlds with a single thought.'
                WHEN 1 THEN 'The magnificent ruler of all phoenixes, this legendary potato rises from its own ashes daily just to prove it can.'
                WHEN 2 THEN 'A colossal volcano deity who creates new mountains by sneezing and considers earthquakes a form of light exercise.'
                WHEN 3 THEN 'The supreme commander of solar energy, capable of creating new stars but prefers to just make really good baked potatoes.'
                ELSE 'An eternal flame that has never been extinguished, not even that one time it went swimming in the ocean.'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'A fearless captain whose face is literally on fire but who somehow still manages to look distinguished.'
                WHEN 1 THEN 'The radiant queen of the Ember Court, famous for her fire dancing and ability to cook perfect s\'mores.'
                WHEN 2 THEN 'A brilliant strategist who uses smoke signals for military communications and has never lost a battle.'
                WHEN 3 THEN 'The unwavering guardian of the Molten Core, who has been keeping the Earth\'s center warm for millennia.'
                WHEN 4 THEN 'A sophisticated nobleman who speaks with a refined accent despite his mouth being full of lava.'
                WHEN 5 THEN 'The elegant duchess of the Fire Court, known for her blazing personality and literally blazing hair.'
                WHEN 6 THEN 'A fierce commander who can control wildfires but always makes sure forest animals evacuate first.'
                ELSE 'A noble knight of the Lava Order, sworn to keep all potatoes at the perfect serving temperature.'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'A perpetually spicy potato who sweats hot sauce and thinks mild salsa is for beginners.'
                WHEN 1 THEN 'An expert marksman who shoots flaming arrows and has never missed a target, though some were accidentally set on fire.'
                WHEN 2 THEN 'A stealthy warrior trained in the ancient art of flame-jitsu, can disappear in a puff of smoke.'
                WHEN 3 THEN 'A saucy potato who escaped from a bottle and decided to become a professional wrestler.'
                WHEN 4 THEN 'A loyal defender who protects the vegetable garden from frost and occasionally overcooks the tomatoes.'
                WHEN 5 THEN 'A camping expert who can start fires with just two sticks and a positive attitude.'
                WHEN 6 THEN 'A brave fighter with a fiery temper who challenges everyone to spicy food contests.'
                WHEN 7 THEN 'An adventurous explorer who once crossed the Sahara Desert and complained it wasn\'t hot enough.'
                WHEN 8 THEN 'A charismatic storyteller who gathers crowds around the campfire with tales of adventure.'
                WHEN 9 THEN 'A culinary master who can grill anything to perfection, including things that shouldn\'t be grilled.'
                WHEN 10 THEN 'An athletic potato training for the Summer Olympics in the exciting sport of Torch Carrying.'
                ELSE 'A legendary trainer who teaches dragons proper fire-breathing etiquette and safety.'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'A warm-hearted potato who gives great hugs and only occasionally sets people on fire.'
                WHEN 1 THEN 'A crispy potato who got a little too excited about sunbathing and now permanently radiates warmth.'
                WHEN 2 THEN 'A sizzling potato who makes cooking sounds when they walk and smells deliciously of barbecue.'
                WHEN 3 THEN 'A hot-tempered potato who literally heats up when angry but cools down quickly with kind words.'
                WHEN 4 THEN 'A smoky potato who worked as a chimney sweep and developed a love for dramatic entrances.'
                WHEN 5 THEN 'A helpful potato who always carries heat packs and offers them to anyone feeling chilly.'
                WHEN 6 THEN 'An energetic potato who sparks with excitement and occasionally shorts out electronics.'
                WHEN 7 THEN 'A cheerful potato who loves sunny days and considers winter a personal insult.'
                WHEN 8 THEN 'A fresh-baked potato who just came out of the oven and is very proud of their golden-brown color.'
                WHEN 9 THEN 'A kind-hearted potato whose warmth comes from genuine caring, not just internal combustion.'
                WHEN 10 THEN 'A fire-starting potato who helps with campfires but sometimes gets carried away.'
                WHEN 11 THEN 'A hot-headed potato who makes rash decisions but always means well in the end.'
                WHEN 12 THEN 'A perfectly grilled potato who achieved their life goal and now helps others reach theirs.'
                WHEN 13 THEN 'A torch-bearing potato who lights the way for others and gives inspiring speeches.'
                ELSE 'A fashionable potato who insists that summer clothes are appropriate in all seasons.'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'A tiny ember that dreams of becoming a mighty bonfire someday.'
                WHEN 1 THEN 'A baby flame who\'s still learning how to flicker properly without going out.'
                WHEN 2 THEN 'A small fire starter who gets very excited about helping with barbecues.'
                WHEN 3 THEN 'A junior hot potato who\'s practicing their heating techniques on unsuspecting hands.'
                WHEN 4 THEN 'A little spark who gives tiny burns that are more like gentle warming sensations.'
                WHEN 5 THEN 'A squeaky clean potato who makes fire angels in ash and believes in the magic of summer.'
                WHEN 6 THEN 'A penny-sized potato who\'s hot, spicy, and disappears way too quickly when you need warmth.'
                WHEN 7 THEN 'A steamy potato who can\'t decide if they want to be a sauna or a humidifier.'
                WHEN 8 THEN 'A slightly charred potato who survived the BBQ incident and has stories to tell.'
                WHEN 9 THEN 'A quick-tempered potato who causes sudden heat waves when annoyed.'
                WHEN 10 THEN 'A smoky potato who sends signals to distant friends and somehow they always understand.'
                WHEN 11 THEN 'A circular potato who believes everything should revolve around the fire pit.'
                WHEN 12 THEN 'A health-conscious potato who promotes the benefits of grilling vegetables.'
                WHEN 13 THEN 'A saucy potato who claims to spice up everyone\'s life, literally.'
                WHEN 14 THEN 'A small but mighty potato who insists they\'re the hottest thing around.'
                WHEN 15 THEN 'A cheerful potato who celebrates every sunny day with excessive enthusiasm.'
                WHEN 16 THEN 'A sporty potato who spends all day around fire rings and has the tan to prove it.'
                WHEN 17 THEN 'An infant potato who\'s new to combustion and finds everything absolutely fascinating.'
                WHEN 18 THEN 'A fast-food potato who achieved their dreams of being flame-grilled to perfection.'
                ELSE 'A prehistoric potato who remembers the first discovery of fire and takes full credit for it.'
            END
    END
WHERE potato_type = 'fire';

-- Continue with lightning, light, and void types in the next part...
-- This is getting long, so I'll split this into multiple migrations