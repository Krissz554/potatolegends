# Resources Folder - Custom Assets

This folder contains all your custom assets for the Unity mobile game. The Resources folder allows Unity to load assets at runtime using `Resources.Load()`.

## 📁 Folder Structure

```
Assets/Resources/
├── UI/           # UI sprites, icons, buttons
├── Audio/        # Sound effects, music, voice clips
├── Textures/     # Background images, textures
├── Sprites/      # 2D sprites for cards, characters, effects
├── Prefabs/      # Reusable prefabs
└── Data/         # JSON files, configuration data
```

## 🎨 How to Add Your Assets

### 1. **UI Assets** (`UI/` folder)
- **Button sprites**: Normal, pressed, disabled states
- **Icons**: Menu icons, action icons, status icons
- **UI backgrounds**: Panels, frames, borders
- **Loading sprites**: Loading animations, progress bars

### 2. **Audio Assets** (`Audio/` folder)
- **Music**: Background music, battle themes
- **Sound Effects**: Button clicks, card flips, battle sounds
- **Voice**: Character voices, announcer clips

### 3. **Textures** (`Textures/` folder)
- **Backgrounds**: Scene backgrounds, menu backgrounds
- **Textures**: Material textures, environment textures

### 4. **Sprites** (`Sprites/` folder)
- **Card Art**: All trading card images
- **Character Sprites**: Hero portraits, character art
- **Effects**: Particle effects, visual effects
- **Icons**: Game icons, status indicators

### 5. **Prefabs** (`Prefabs/` folder)
- **Card Prefabs**: Reusable card prefabs
- **UI Prefabs**: Custom UI components
- **Effect Prefabs**: Particle effects, animations

### 6. **Data** (`Data/` folder)
- **Card Data**: JSON files with card information
- **Game Config**: Configuration files
- **Localization**: Text files for different languages

## 🚀 Loading Assets in Code

### Loading Sprites
```csharp
Sprite cardSprite = Resources.Load<Sprite>("Sprites/Cards/Card_001");
```

### Loading Audio
```csharp
AudioClip buttonSound = Resources.Load<AudioClip>("Audio/SFX/ButtonClick");
```

### Loading Prefabs
```csharp
GameObject cardPrefab = Resources.Load<GameObject>("Prefabs/CardPrefab");
```

### Loading Text Files
```csharp
TextAsset cardData = Resources.Load<TextAsset>("Data/CardData");
string jsonData = cardData.text;
```

## 📱 Mobile Optimization Tips

### **Image Optimization**
- Use **Power of 2** dimensions (256x256, 512x512, 1024x1024)
- Use **PNG** for transparency, **JPG** for photos
- Keep file sizes small for mobile performance

### **Audio Optimization**
- Use **OGG** format for music, **WAV** for short sound effects
- Compress audio files appropriately
- Use **AudioSource** pooling for repeated sounds

### **Sprite Optimization**
- Use **Sprite Atlas** for better performance
- Group related sprites together
- Use appropriate **Filter Mode** (Point for pixel art, Bilinear for smooth)

## 🎯 Asset Naming Convention

### **UI Assets**
- `Button_Normal.png`
- `Button_Pressed.png`
- `Button_Disabled.png`
- `Icon_Collection.png`
- `Icon_Battle.png`

### **Card Assets**
- `Card_001_Common.png`
- `Card_002_Rare.png`
- `Card_003_Epic.png`
- `Card_004_Legendary.png`

### **Audio Assets**
- `SFX_ButtonClick.ogg`
- `SFX_CardFlip.wav`
- `Music_MainMenu.ogg`
- `Music_Battle.ogg`

## 🔧 Integration with Scene Generator

The scene generator will automatically look for assets in the Resources folder when creating UI elements. Make sure to:

1. **Place your assets** in the appropriate Resources subfolder
2. **Name them correctly** according to the naming convention
3. **Update the scene generator** if you add new asset types

## 📋 Asset Checklist

- [ ] **UI Sprites**: Buttons, icons, backgrounds
- [ ] **Card Art**: All trading card images
- [ ] **Character Art**: Hero portraits and sprites
- [ ] **Audio**: Music and sound effects
- [ ] **Effects**: Particle effects and animations
- [ ] **Data Files**: Card data, configuration

## 🎮 Ready to Use

Once you add your assets to the Resources folder, the scene generator and UI scripts will automatically use them to create a fully customized mobile game experience!

**Happy asset creation!** 🎨📱🎮