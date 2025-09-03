-- Continue with Lightning, Light, and Void type unique names and descriptions

-- Lightning type names and descriptions
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'Storm Lord Thunderspud'
                WHEN 1 THEN 'Lightning Emperor Voltaic'
                WHEN 2 THEN 'Thunder God Zapotron'
                WHEN 3 THEN 'Electric Overlord Shockwave'
                ELSE 'Tesla Coil Mastermind'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'Captain Thunderbolt'
                WHEN 1 THEN 'Storm Queen Electrica'
                WHEN 2 THEN 'Lightning General Sparks'
                WHEN 3 THEN 'Electric Grid Guardian'
                WHEN 4 THEN 'Baron von Buzzkill'
                WHEN 5 THEN 'Duchess Static'
                WHEN 6 THEN 'Hurricane Commander'
                ELSE 'Plasma Knight Voltaire'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'Zappy McShockface'
                WHEN 1 THEN 'Thunder Sniper'
                WHEN 2 THEN 'Lightning Ninja Spud'
                WHEN 3 THEN 'Electric Eel Warrior'
                WHEN 4 THEN 'Static Shock Defender'
                WHEN 5 THEN 'Storm Chaser Scout'
                WHEN 6 THEN 'Power Surge Fighter'
                WHEN 7 THEN 'Weather Watcher'
                WHEN 8 THEN 'Radio Wave Rider'
                WHEN 9 THEN 'Battery Pack Master'
                WHEN 10 THEN 'Electric Guitar Hero'
                ELSE 'Tesla Coil Technician'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'Buzzy McGillicuddy'
                WHEN 1 THEN 'Sparkard the Energetic'
                WHEN 2 THEN 'Crackling McZapface'
                WHEN 3 THEN 'Charged Potato Pete'
                WHEN 4 THEN 'Flashy the Bright'
                WHEN 5 THEN 'Power Cord Patrick'
                WHEN 6 THEN 'Lightning Bug Sammie'
                WHEN 7 THEN 'Storm Cloud Sally'
                WHEN 8 THEN 'High Voltage Dan'
                WHEN 9 THEN 'Live Wire Barry'
                WHEN 10 THEN 'Short Circuit Larry'
                WHEN 11 THEN 'Electric Bill Steve'
                WHEN 12 THEN 'Thunder Clap Shane'
                WHEN 13 THEN 'Power Outage Tom'
                ELSE 'Shock Therapy Wendy'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'Tiny Spark Chip'
                WHEN 1 THEN 'Baby Lightning'
                WHEN 2 THEN 'Little Buzzer'
                WHEN 3 THEN 'Electric Spud Jr'
                WHEN 4 THEN 'Static Pip'
                WHEN 5 THEN 'Thunder Squeaky'
                WHEN 6 THEN 'Shock Pop Penny'
                WHEN 7 THEN 'Crackling McZap'
                WHEN 8 THEN 'Current Events Bob'
                WHEN 9 THEN 'Lightning Bug Chip'
                WHEN 10 THEN 'Storm Front Sammy'
                WHEN 11 THEN 'Power Socket Tim'
                WHEN 12 THEN 'Electric Veggie Val'
                WHEN 13 THEN 'Amp Up Phil'
                WHEN 14 THEN 'Thunder King Kevin'
                WHEN 15 THEN 'Stormy Day Sophie'
                WHEN 16 THEN 'Electric Fence Rick'
                WHEN 17 THEN 'Sparking Baby'
                WHEN 18 THEN 'Charged Fries Frank'
                ELSE 'Electric Age Amy'
            END
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'An ancient storm deity who commands thunder and lightning across multiple dimensions, occasionally causing power outages in neighboring realities.'
                WHEN 1 THEN 'The supreme emperor of all electrical phenomena, who can power entire cities just by thinking about it.'
                WHEN 2 THEN 'A divine thunder god who creates lightning bolts for fun and considers thunderstorms a form of interpretive dance.'
                WHEN 3 THEN 'The ultimate overlord of electromagnetic forces, capable of controlling all electronics within a thousand-mile radius.'
                ELSE 'A genius inventor potato who discovered electricity before it was cool and still holds all the patents.'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'A fearless captain who rides lightning bolts like horses and has never been late to an appointment.'
                WHEN 1 THEN 'The electrifying queen of Storm Castle, famous for her shocking personality and electric blue hair.'
                WHEN 2 THEN 'A brilliant general who communicates through electromagnetic pulses and has revolutionized warfare.'
                WHEN 3 THEN 'The dedicated guardian of the power grid, ensuring everyone has electricity and reasonable rates.'
                WHEN 4 THEN 'A sophisticated nobleman who speaks in electrical terminology and always generates static when excited.'
                WHEN 5 THEN 'The elegant duchess of the Lightning Court, known for her electrifying dance moves.'
                WHEN 6 THEN 'A fierce commander who can summon hurricanes but always warns people about severe weather.'
                ELSE 'A noble knight of the Plasma Order, sworn to keep all electronics charged and functioning.'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'A perpetually energetic potato who bounces off walls and generates static electricity from excitement.'
                WHEN 1 THEN 'An expert marksman who shoots lightning bolts with pinpoint accuracy and excellent sound effects.'
                WHEN 2 THEN 'A stealthy warrior trained in electric-jitsu, can disappear in a flash of lightning.'
                WHEN 3 THEN 'A slippery potato who escaped from an aquarium and learned to generate bioelectricity.'
                WHEN 4 THEN 'A defensive specialist who creates electrical barriers and occasionally shorts out the Wi-Fi.'
                WHEN 5 THEN 'An adventurous potato who chases storms for fun and has been struck by lightning 47 times.'
                WHEN 6 THEN 'A powerful fighter who surges with energy and sometimes accidentally overloads household appliances.'
                WHEN 7 THEN 'A meteorologist potato who can predict storms with 100% accuracy but only tells their weather app.'
                WHEN 8 THEN 'A musical potato who communicates through radio frequencies and has their own talk show.'
                WHEN 9 THEN 'An engineering potato who keeps everyone\'s devices charged and never asks for payment.'
                WHEN 10 THEN 'A rockstar potato who plays electric guitar and once electrified an entire concert audience.'
                ELSE 'A technical support potato who fixes electrical problems and explains things very patiently.'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'An energetic potato who vibrates constantly and makes excellent coffee with built-in agitation.'
                WHEN 1 THEN 'A sparkly potato who lights up rooms both literally and figuratively with their enthusiasm.'
                WHEN 2 THEN 'A crackling potato who makes Rice Krispies sounds when they move and finds it hilarious.'
                WHEN 3 THEN 'A charged potato who builds up static and gives surprise hugs that are quite shocking.'
                WHEN 4 THEN 'A flashy potato who likes to make dramatic entrances with lightning effects.'
                WHEN 5 THEN 'A helpful potato who always carries charging cables and offers them to strangers.'
                WHEN 6 THEN 'A glowing potato who thinks they\'re a firefly and blinks in morse code.'
                WHEN 7 THEN 'A stormy potato who brings rain wherever they go but always carries an umbrella for others.'
                WHEN 8 THEN 'A high-energy potato who never sleeps and powers the local electrical grid through sheer enthusiasm.'
                WHEN 9 THEN 'A conducting potato who helps electricity flow smoothly and mediates electrical disputes.'
                WHEN 10 THEN 'A malfunction-prone potato who occasionally shorts out but always apologizes profusely.'
                WHEN 11 THEN 'A billing potato who keeps track of everyone\'s electrical usage and sends polite reminder notes.'
                WHEN 12 THEN 'A loud potato who announces their presence with thunder sounds and dramatic weather.'
                WHEN 13 THEN 'A power-outage potato who accidentally causes blackouts but brings backup generators.'
                ELSE 'A therapeutic potato who claims to cure everything with mild electrical stimulation.'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'A tiny spark that dreams of becoming a mighty lightning bolt someday.'
                WHEN 1 THEN 'A baby lightning bolt who\'s still learning how to zigzag properly.'
                WHEN 2 THEN 'A little buzzer who gets very excited about doorbells and fire alarms.'
                WHEN 3 THEN 'A junior electric potato who\'s practicing their shocking techniques on doorknobs.'
                WHEN 4 THEN 'A little static charge who clings to balloons and enjoys making people\'s hair stand up.'
                WHEN 5 THEN 'A squeaky potato who sounds like a smoke detector with a low battery.'
                WHEN 6 THEN 'A penny-sized potato who\'s energetic, crackling, and always fully charged.'
                WHEN 7 THEN 'A crackling potato who can\'t decide if they want to be a radio or a microwave.'
                WHEN 8 THEN 'A news-following potato who keeps everyone updated on current events, literally.'
                WHEN 9 THEN 'A quick-flashing potato who communicates in blinks and somehow everyone understands.'
                WHEN 10 THEN 'a stormy potato who brings weather updates and occasionally mild precipitation.'
                WHEN 11 THEN 'A socket-shaped potato who believes everything should be properly grounded.'
                WHEN 12 THEN 'A health-conscious potato who promotes the benefits of bioelectricity.'
                WHEN 13 THEN 'An amplifying potato who makes everything louder and more exciting.'
                WHEN 14 THEN 'A small but mighty potato who insists they\'re the most electrifying personality around.'
                WHEN 15 THEN 'A weather-watching potato who celebrates every thunderstorm with excessive enthusiasm.'
                WHEN 16 THEN 'A boundary-setting potato who keeps others safely separated from dangerous electrical areas.'
                WHEN 17 THEN 'An infant potato who\'s new to electricity and finds light switches absolutely magical.'
                WHEN 18 THEN 'A fast-food potato who achieved their dreams of being flash-fried with lightning.'
                ELSE 'A prehistoric potato who claims to have invented the first electrical storm.'
            END
    END
WHERE potato_type = 'lightning';

-- Light type names and descriptions
UPDATE potato_registry 
SET 
    name = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'Radiance Lord Illuminus'
                WHEN 1 THEN 'Solar Emperor Brilliance'
                WHEN 2 THEN 'Photon God Luminaire'
                WHEN 3 THEN 'Celestial Overlord Beacon'
                ELSE 'Pure Light Mastermind'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'Captain Daybreak'
                WHEN 1 THEN 'Light Queen Celestia'
                WHEN 2 THEN 'Sunshine General Aurora'
                WHEN 3 THEN 'Prism Guardian'
                WHEN 4 THEN 'Baron von Brightside'
                WHEN 5 THEN 'Duchess Moonbeam'
                WHEN 6 THEN 'Rainbow Commander'
                ELSE 'Starlight Knight Lumina'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'Glowy McShineface'
                WHEN 1 THEN 'Beam Sniper'
                WHEN 2 THEN 'Light Ninja Spud'
                WHEN 3 THEN 'Laser Pointer Warrior'
                WHEN 4 THEN 'Flashlight Defender'
                WHEN 5 THEN 'Spotlight Scout'
                WHEN 6 THEN 'Glow Stick Fighter'
                WHEN 7 THEN 'Sunrise Watcher'
                WHEN 8 THEN 'Lighthouse Keeper'
                WHEN 9 THEN 'Solar Panel Master'
                WHEN 10 THEN 'Disco Ball Hero'
                ELSE 'Photographer\'s Flash'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'Shiny McGillicuddy'
                WHEN 1 THEN 'Glowbert the Radiant'
                WHEN 2 THEN 'Sparkling McBeamface'
                WHEN 3 THEN 'Bright Potato Pete'
                WHEN 4 THEN 'Sunny the Cheerful'
                WHEN 5 THEN 'Night Light Patrick'
                WHEN 6 THEN 'Glowing Sammie'
                WHEN 7 THEN 'Spotlight Sally'
                WHEN 8 THEN 'High Beam Dan'
                WHEN 9 THEN 'Bright Side Barry'
                WHEN 10 THEN 'Mood Light Larry'
                WHEN 11 THEN 'Daylight Steve'
                WHEN 12 THEN 'Brilliant Shane'
                WHEN 13 THEN 'Light Switch Tom'
                ELSE 'Reading Lamp Wendy'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'Tiny Glow Chip'
                WHEN 1 THEN 'Baby Sunbeam'
                WHEN 2 THEN 'Little Firefly'
                WHEN 3 THEN 'Bright Spud Jr'
                WHEN 4 THEN 'Shimmer Pip'
                WHEN 5 THEN 'Twinkle Squeaky'
                WHEN 6 THEN 'Glow Pop Penny'
                WHEN 7 THEN 'Shining McShine'
                WHEN 8 THEN 'Bright Ideas Bob'
                WHEN 9 THEN 'Glimmer Chip'
                WHEN 10 THEN 'Sunrise Sammy'
                WHEN 11 THEN 'Light Bulb Tim'
                WHEN 12 THEN 'Glowing Veggie Val'
                WHEN 13 THEN 'Brighten Up Phil'
                WHEN 14 THEN 'Sunshine King Kevin'
                WHEN 15 THEN 'Bright Day Sophie'
                WHEN 16 THEN 'Street Light Rick'
                WHEN 17 THEN 'Glowing Baby'
                WHEN 18 THEN 'Golden Fries Frank'
                ELSE 'Enlightened Amy'
            END
    END,
    description = CASE 
        WHEN rarity = 'exotic' THEN 
            CASE (id::text || registry_id)::hash % 5
                WHEN 0 THEN 'An transcendent being of pure light who illuminates truth and occasionally causes temporary blindness in those unprepared for enlightenment.'
                WHEN 1 THEN 'The supreme emperor of all solar phenomena, who can create miniature suns and has never experienced a cloudy day.'
                WHEN 2 THEN 'A divine photon deity who travels at light speed and still somehow arrives fashionably late to every appointment.'
                WHEN 3 THEN 'The ultimate beacon of hope and guidance, visible from space and used by lost travelers across galaxies.'
                ELSE 'The embodiment of pure illumination, so bright that sunglasses were invented specifically for looking at them.'
            END
        WHEN rarity = 'legendary' THEN
            CASE (id::text || registry_id)::hash % 8
                WHEN 0 THEN 'A heroic captain who brings the dawn each day and has never overslept in their entire existence.'
                WHEN 1 THEN 'The luminous queen of the Crystal Palace, famous for her radiant smile and ability to brighten anyone\'s day.'
                WHEN 2 THEN 'A brilliant general who uses light signals for communication and has revolutionized military tactics.'
                WHEN 3 THEN 'The steadfast guardian of the rainbow bridge, ensuring all colors stay in proper order.'
                WHEN 4 THEN 'A distinguished nobleman who always looks on the bright side and illuminates every conversation.'
                WHEN 5 THEN 'The graceful duchess of the Starlight Court, known for her moonlit garden parties.'
                WHEN 6 THEN 'A colorful commander who can summon rainbows and always brings good luck after storms.'
                ELSE 'A noble knight of the Radiance Order, sworn to banish darkness and provide excellent mood lighting.'
            END
        WHEN rarity = 'rare' THEN
            CASE (id::text || registry_id)::hash % 12
                WHEN 0 THEN 'A perpetually glowing potato who lights up rooms and has never needed a flashlight.'
                WHEN 1 THEN 'An expert marksman who shoots laser beams with perfect accuracy and makes "pew pew" sounds.'
                WHEN 2 THEN 'A stealthy warrior trained in light-jutsu, can become invisible by bending light around themselves.'
                WHEN 3 THEN 'A precise potato who escaped from a presentation and now helps people point at important things.'
                WHEN 4 THEN 'A reliable defender who illuminates dark corners and has an excellent battery life.'
                WHEN 5 THEN 'A performance potato who creates dramatic lighting effects and takes stage direction very seriously.'
                WHEN 6 THEN 'A party-loving fighter who glows in fun colors and makes every gathering more festive.'
                WHEN 7 THEN 'An early-rising potato who never misses a sunrise and provides daily weather updates.'
                WHEN 8 THEN 'A maritime potato who guides ships safely to shore and has prevented countless shipwrecks.'
                WHEN 9 THEN 'An eco-friendly potato who converts sunlight to energy and powers sustainable technology.'
                WHEN 10 THEN 'A dancing potato who reflects light in spectacular patterns and loves 70s music.'
                ELSE 'A helpful photographer potato who provides perfect lighting for every photo opportunity.'
            END
        WHEN rarity = 'uncommon' THEN
            CASE (id::text || registry_id)::hash % 15
                WHEN 0 THEN 'A polished potato who reflects light beautifully and gives everyone a healthy glow.'
                WHEN 1 THEN 'A radiant potato who emanates warmth and positivity, making everyone feel welcome.'
                WHEN 2 THEN 'A sparkling potato who shimmers constantly and believes every moment deserves to shine.'
                WHEN 3 THEN 'A luminous potato who brightens dark places and always sees the positive side of situations.'
                WHEN 4 THEN 'A cheerful potato who brings sunshine to cloudy days and has an infectious laugh.'
                WHEN 5 THEN 'A comforting potato who provides gentle illumination and helps with bedtime stories.'
                WHEN 6 THEN 'A glowing potato who thinks they\'re a lamp and sits very still to provide ambient lighting.'
                WHEN 7 THEN 'A dramatic potato who creates spotlight effects for important announcements.'
                WHEN 8 THEN 'A high-intensity potato who provides excellent visibility and always follows safety protocols.'
                WHEN 9 THEN 'an optimistic potato who always looks on the bright side and shares encouraging thoughts.'
                WHEN 10 THEN 'An atmospheric potato who creates the perfect lighting for any mood or occasion.'
                WHEN 11 THEN 'A dependable potato who provides consistent illumination and never flickers.'
                WHEN 12 THEN 'A brilliant potato who has great ideas and literally lights up when thinking.'
                WHEN 13 THEN 'A convenient potato who helps people find light switches in the dark.'
                ELSE 'A studious potato who provides excellent reading light and encourages education.'
            END
        ELSE -- common
            CASE (id::text || registry_id)::hash % 20
                WHEN 0 THEN 'A tiny glow that dreams of becoming a mighty searchlight someday.'
                WHEN 1 THEN 'A baby sunbeam who\'s still learning how to shine without being too bright.'
                WHEN 2 THEN 'A little firefly who blinks in patterns and believes they\'re sending important messages.'
                WHEN 3 THEN 'A junior bright potato who\'s practicing their illumination techniques on dark corners.'
                WHEN 4 THEN 'A little shimmer who makes everything slightly more magical just by being there.'
                WHEN 5 THEN 'A squeaky potato who twinkles like a star and makes gentle chiming sounds.'
                WHEN 6 THEN 'A penny-sized potato who\'s bright, cheerful, and makes everything more visible.'
                WHEN 7 THEN 'A shining potato who can\'t decide if they want to be a mirror or a light bulb.'
                WHEN 8 THEN 'An idea-generating potato who provides inspiration and occasional illumination.'
                WHEN 9 THEN 'A quick-blinking potato who creates interesting light patterns and hypnotizes cats.'
                WHEN 10 THEN 'A morning potato who celebrates each sunrise with enthusiasm and gentle glowing.'
                WHEN 11 THEN 'A bulb-shaped potato who believes in efficient lighting and proper electrical safety.'
                WHEN 12 THEN 'A health-conscious potato who promotes the benefits of natural sunlight.'
                WHEN 13 THEN 'An encouraging potato who helps people brighten up their day and their attitude.'
                WHEN 14 THEN 'A small but mighty potato who insists they\'re the brightest thing around.'
                WHEN 15 THEN 'A cheerful potato who celebrates every sunny day with excessive gleaming.'
                WHEN 16 THEN 'A municipal potato who provides public lighting and takes civic duty very seriously.'
                WHEN 17 THEN 'An infant potato who\'s new to glowing and finds light switches absolutely fascinating.'
                WHEN 18 THEN 'A fast-food potato who achieved their dreams of being golden and perfectly illuminated.'
                ELSE 'A enlightened potato who has achieved inner peace and shares wisdom through gentle glowing.'
            END
    END
WHERE potato_type = 'light';

-- Continue with void types in next migration...