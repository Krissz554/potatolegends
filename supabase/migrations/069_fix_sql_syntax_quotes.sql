-- Fix SQL syntax errors with escaped quotes in descriptions
-- Replace all the problematic descriptions with properly escaped quotes

-- First, let's update ice type cards with fixed syntax
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
                WHEN 0 THEN 'An ancient ice potato of immeasurable power, said to have frozen entire kingdoms with a single glare.'
                WHEN 1 THEN 'The supreme ruler of all frozen realms, this majestic potato commands blizzards and controls the essence of cold.'
                WHEN 2 THEN 'A colossal ice emperor who sits upon a throne of permafrost, wielding the power to create glaciers.'
                WHEN 3 THEN 'The ultimate military strategist of the frozen wastes, commanding armies of ice soldiers in great wars.'
                ELSE 'A potato so cold it defies physics, existing at absolute zero while remaining alive and very grumpy about it.'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'A fearless captain who sailed the seven frozen seas and discovered the legendary Ice Potato Islands.'
                WHEN 1 THEN 'The elegant ruler of the Crystal Palace, known for stunning ice sculptures and terrible dad jokes.'
                WHEN 2 THEN 'A brilliant military tactician who once defeated an army using strategically placed snowballs.'
                WHEN 3 THEN 'The stoic guardian of the ancient Frozen Throne, who has never lost a staring contest in 1000 years.'
                WHEN 4 THEN 'A distinguished nobleman who insists on wearing a monocle made of ice and speaks only in freeze puns.'
                WHEN 5 THEN 'The graceful duchess of the Snowflake Court, famous for ice dancing and frozen tea parties.'
                WHEN 6 THEN 'A storm-commanding warrior who can summon hailstorms but always apologizes to those who get hit.'
                ELSE 'A noble knight of the Glacier Order, sworn to protect all potatoes from being served warm.'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'A perpetually cold potato who wears seventeen sweaters and still complains about the temperature.'
                WHEN 1 THEN 'An expert marksman with snowballs, holds the world record for longest-distance snowball fight victory.'
                WHEN 2 THEN 'A stealthy warrior trained in the ancient art of ice-jitsu, can disappear in a cloud of snow.'
                WHEN 3 THEN 'A cubic potato who rolled out of an ice tray and decided to become a professional wrestler.'
                WHEN 4 THEN 'A loyal companion who follows you around making slush sounds and occasionally trips people.'
                WHEN 5 THEN 'A weather-tracking potato who can predict snow days with perfect accuracy but only tells friends.'
                WHEN 6 THEN 'A brave fighter who believes all vegetables should stick together, especially when frozen.'
                WHEN 7 THEN 'An adventurous potato who once got lost in a freezer and came back with amazing stories.'
                WHEN 8 THEN 'A creative potato who builds elaborate snow forts and hosts epic snowball tournaments.'
                WHEN 9 THEN 'A friendly vendor who sells the most delicious ice cream, despite being a potato themselves.'
                WHEN 10 THEN 'An athletic potato training for the Winter Olympics in the exciting sport of Synchronized Sliding.'
                ELSE 'A mysterious potato who communicates exclusively through penguin noises and makes it work.'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'A laid-back potato who thinks everything is totally chill and gives great life advice.'
                WHEN 1 THEN 'A nervous potato who shivers constantly but insists its not from the cold, just excitement.'
                WHEN 2 THEN 'A clumsy potato who slides around on ice but somehow always lands on their feet eventually.'
                WHEN 3 THEN 'A cool customer who never loses composure, even when literally frozen solid.'
                WHEN 4 THEN 'A slow-moving potato who takes their time with everything but gives the warmest coldest hugs.'
                WHEN 5 THEN 'A helpful potato who always carries an ice pack and offers it to anyone with minor injuries.'
                WHEN 6 THEN 'A delicate potato who believes they are unique and special, just like every other snowflake.'
                WHEN 7 THEN 'A cuddly potato who gives bear hugs that are surprisingly refreshing on hot days.'
                WHEN 8 THEN 'A practical potato who is always prepared with frozen meals and zero cooking skills.'
                WHEN 9 THEN 'An aloof potato who literally gives everyone the cold shoulder but means well.'
                WHEN 10 THEN 'A sweet potato who enjoys frozen treats more than anyone should.'
                WHEN 11 THEN 'A confused potato who gets brain freeze from thinking too hard about simple problems.'
                WHEN 12 THEN 'A tropical-themed potato who moved north and deeply regrets all their life choices.'
                WHEN 13 THEN 'A competitive potato who challenges everyone to snowball fights and somehow always wins.'
                ELSE 'A fashionable potato who insists winter coats are a year-round necessity.'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'A tiny chip of ice that dreams of becoming a mighty glacier someday.'
                WHEN 1 THEN 'A baby snowflake who is still learning how to be unique and special.'
                WHEN 2 THEN 'A small ice cube who escaped from a drink and is now living their best life.'
                WHEN 3 THEN 'A junior version of a famous ice warrior, still working on their chilling techniques.'
                WHEN 4 THEN 'A little potato who gives tiny frost bites that are more like gentle cooling sensations.'
                WHEN 5 THEN 'A squeaky clean potato who makes snow angels and believes in the magic of winter.'
                WHEN 6 THEN 'A penny-sized potato who is sweet, cold, and disappears way too quickly.'
                WHEN 7 THEN 'A liquidy potato who cannot decide if they want to be a drink or a dessert.'
                WHEN 8 THEN 'A slightly damaged potato who survived the freezer incident and has stories to tell.'
                WHEN 9 THEN 'A quick-tempered potato who causes sudden cold snaps when annoyed.'
                WHEN 10 THEN 'A drifty potato who goes wherever the wind takes them and seems happy about it.'
                WHEN 11 THEN 'A rectangular potato who is very organized and believes everything has its place.'
                WHEN 12 THEN 'A health-conscious potato who promotes the benefits of eating frozen vegetables.'
                WHEN 13 THEN 'A pharmaceutical potato who claims to cure everything but mostly just makes you cold.'
                WHEN 14 THEN 'A small but mighty potato who insists they are royalty despite all evidence to the contrary.'
                WHEN 15 THEN 'A school-loving potato who celebrates every snow day with excessive enthusiasm.'
                WHEN 16 THEN 'A sporty potato who spends all day at the ice rink and has the bruises to prove it.'
                WHEN 17 THEN 'An infant potato who is new to the world and finds everything absolutely fascinating.'
                WHEN 18 THEN 'A fast-food potato who achieved their dreams of being frozen for preservation.'
                ELSE 'A prehistoric potato who remembers the last ice age and will not stop talking about it.'
            END
    END
WHERE potato_type = 'ice';