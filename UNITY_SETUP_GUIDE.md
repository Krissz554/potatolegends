# Unity Mobile Game Setup Guide

## 🚀 Quick Start

1. **Open Unity 6** (6000.2.2f1)
2. **Open Project** - Select the `unity-mobile` folder
3. **Wait for packages** to load (should be automatic)
4. **Create scenes** as needed

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, InputManager
│   ├── Data/           # CardData, HeroData ScriptableObjects
│   ├── Network/        # SupabaseClient
│   ├── UI/             # All UI screens and components
│   ├── Battle/         # BattleManager
│   ├── Cards/          # CardDisplay
│   ├── Collection/     # CollectionManager
│   └── Utils/          # JsonHelper
├── Scenes/
│   └── MainMenu.unity  # Sample scene
└── InputActions/
    └── GameInput.inputactions  # Input System actions
```

## 🎮 Essential Packages

- **TextMeshPro**: UI text rendering
- **Input System**: Modern input handling
- **2D Sprite**: Sprite rendering
- **2D Tilemap**: Tile-based graphics
- **2D PSD Importer**: Photoshop import

## 📱 Mobile Features Ready

- Touch input with drag-and-drop
- Mobile UI scaling
- Haptic feedback support
- Responsive canvas settings

## 🔧 Next Steps

1. Create additional scenes (Collection, DeckBuilder, HeroHall, Battle)
2. Set up Canvas hierarchy
3. Link scripts to GameObjects
4. Create card and hero assets
5. Test mobile controls

## ⚠️ Troubleshooting

If packages fail to load:
1. Close Unity
2. Delete `Library` folder
3. Reopen Unity
4. Let packages reinstall

The project is now fully functional for Unity 6 mobile development!