# Unity Mobile Game Setup Guide

## 🎮 Complete Setup Instructions

### 1. **Configure Backend Settings**

1. **Create GameConfig Asset:**
   - Right-click in Project window → Create → Potato Legends → Game Config
   - Name it "GameConfig"
   - Set your Supabase URL and API key
   - Set your Ably API key (optional for real-time features)

2. **Assign GameConfig to SupabaseClient:**
   - Find the SupabaseClient GameObject in your scene
   - Drag the GameConfig asset to the "Game Config" field

### 2. **Scene Setup**

All scenes are already created and configured:
- ✅ **Auth** - Login/Register screen
- ✅ **MainMenu** - Main navigation hub
- ✅ **Collection** - Card collection viewer
- ✅ **DeckBuilder** - Deck creation and management
- ✅ **HeroHall** - Hero selection and management
- ✅ **Battle** - Battle arena

### 3. **Backend Integration**

#### **Supabase Setup:**
1. Create a Supabase project at https://supabase.com
2. Get your project URL and anon key
3. Set up these tables:
   ```sql
   -- Users table (handled by Supabase Auth)
   -- Cards table
   CREATE TABLE cards (
     id TEXT PRIMARY KEY,
     name TEXT NOT NULL,
     mana_cost INTEGER NOT NULL,
     hp INTEGER,
     attack INTEGER,
     card_type TEXT NOT NULL,
     rarity TEXT NOT NULL,
     potato_type TEXT NOT NULL,
     flavor_text TEXT,
     illustration_url TEXT
   );
   
   -- User collection table
   CREATE TABLE user_collection (
     id SERIAL PRIMARY KEY,
     user_id TEXT NOT NULL,
     card_id TEXT NOT NULL,
     quantity INTEGER DEFAULT 1,
     acquired_at TIMESTAMP DEFAULT NOW(),
     source TEXT DEFAULT 'default'
   );
   
   -- User decks table
   CREATE TABLE user_decks (
     id SERIAL PRIMARY KEY,
     user_id TEXT NOT NULL,
     deck_name TEXT NOT NULL,
     deck_data JSONB NOT NULL,
     is_active BOOLEAN DEFAULT FALSE,
     created_at TIMESTAMP DEFAULT NOW()
   );
   ```

4. Create RPC function for getting user collection:
   ```sql
   CREATE OR REPLACE FUNCTION get_user_collection(user_id TEXT)
   RETURNS TABLE (
     card_id TEXT,
     name TEXT,
     mana_cost INTEGER,
     hp INTEGER,
     attack INTEGER,
     card_type TEXT,
     rarity TEXT,
     potato_type TEXT,
     flavor_text TEXT,
     illustration_url TEXT,
     quantity INTEGER,
     acquired_at TIMESTAMP,
     source TEXT
   ) AS $$
   BEGIN
     RETURN QUERY
     SELECT 
       c.id as card_id,
       c.name,
       c.mana_cost,
       c.hp,
       c.attack,
       c.card_type,
       c.rarity,
       c.potato_type,
       c.flavor_text,
       c.illustration_url,
       uc.quantity,
       uc.acquired_at,
       uc.source
     FROM user_collection uc
     JOIN cards c ON uc.card_id = c.id
     WHERE uc.user_id = $1
     ORDER BY c.name;
   END;
   $$ LANGUAGE plpgsql;
   ```

#### **Ably Setup (Optional):**
1. Create an Ably account at https://ably.com
2. Get your API key
3. Add it to your GameConfig

### 4. **Asset Setup**

#### **Card Assets:**
1. Create folder: `Assets/Resources/Cards/`
2. Add card images with naming: `card_[id].png`
3. Card IDs should match your database

#### **Hero Assets:**
1. Create folder: `Assets/Resources/Heroes/`
2. Add hero images with naming: `hero_[id].png`

#### **UI Assets:**
1. Create folder: `Assets/Resources/UI/`
2. Add icons for mana, health, attack, etc.

### 5. **Testing**

#### **Authentication:**
1. Run the game
2. You should see the Auth scene
3. Try registering a new account
4. Try logging in with existing account

#### **Collection:**
1. After login, go to Collection
2. Should load your cards from database
3. Search and filter should work

#### **Deck Builder:**
1. Go to Deck Builder
2. Drag cards to create deck
3. Save deck should work

#### **Battle:**
1. Click Battle button in Main Menu
2. Should start matchmaking
3. After match found, loads Battle scene

### 6. **Mobile Build**

1. **Build Settings:**
   - File → Build Settings
   - Switch Platform to Android/iOS
   - Add all scenes to build

2. **Player Settings:**
   - Company Name: Your company
   - Product Name: Potato Legends
   - Package Name: com.yourcompany.potatolegends

3. **Build:**
   - Click Build
   - Choose output folder
   - Build APK/IPA

### 7. **Troubleshooting**

#### **Common Issues:**

1. **"GameConfig is not assigned":**
   - Create GameConfig asset
   - Assign it to SupabaseClient

2. **Authentication fails:**
   - Check Supabase URL and API key
   - Check internet connection
   - Check Supabase project is active

3. **Collection not loading:**
   - Check database tables exist
   - Check RPC function is created
   - Check user is authenticated

4. **UI not responding:**
   - Check EventSystem exists in scene
   - Check Canvas settings
   - Check button references

### 8. **Development Tips**

1. **Debug Logging:**
   - Enable in GameConfig
   - Check Console for detailed logs

2. **Testing:**
   - Use Unity Remote for mobile testing
   - Test on actual devices

3. **Performance:**
   - Optimize images for mobile
   - Use object pooling for cards
   - Minimize draw calls

## 🎉 **You're Ready!**

Your Unity mobile game is now fully functional with:
- ✅ Real Supabase backend integration
- ✅ Complete authentication system
- ✅ Card collection and deck building
- ✅ Hero management
- ✅ Battle system with matchmaking
- ✅ Mobile-optimized UI
- ✅ Real-time features (with Ably)

**Happy coding!** 🚀