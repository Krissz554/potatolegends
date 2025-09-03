# 🎮 Potato Card Game - Unity Mobile Version

## 📱 Project Overview

This is the Unity mobile version of the "What's My Potato?" trading card game, converted from the original web-based React application. The game features real-time multiplayer battles, deck building, and card collection mechanics optimized for mobile devices.

## 🏗️ Project Structure

```
Unity-PotatoCardGame/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/          # Game managers and core systems
│   │   ├── Cards/         # Card display and interaction
│   │   ├── Battle/        # Combat system and battle logic
│   │   ├── UI/            # User interface controllers
│   │   ├── Network/       # Supabase integration and networking
│   │   ├── Data/          # ScriptableObjects and data models
│   │   └── Utils/         # Helper functions and utilities
│   ├── Prefabs/           # Reusable game objects
│   ├── UI/                # UI prefabs and layouts
│   ├── Art/               # Sprites, textures, and visual assets
│   ├── Audio/             # Sound effects and music
│   ├── Scenes/            # Game scenes
│   ├── Data/              # ScriptableObject assets
│   └── Resources/         # Runtime loadable assets
├── ProjectSettings/       # Unity project configuration
└── UserSettings/          # User-specific Unity settings
```

## 🎯 Key Features

### ✅ Implemented
- **Game Manager**: Core game state management and scene transitions
- **Supabase Integration**: Authentication and database communication
- **Card System**: ScriptableObject-based card data with full stats
- **Card Display**: Interactive card UI with touch controls and animations
- **Mobile Optimization**: Touch gestures, responsive UI, performance optimized

### 🚧 In Development
- Battle system and turn-based combat
- Deck builder interface
- Collection browser
- Real-time multiplayer integration
- Hero system
- Mobile-specific features (haptics, notifications)

## 🔌 Backend Integration

The Unity project connects to the same Supabase backend as the web version:
- **Database**: All card data, user accounts, and battle sessions
- **Authentication**: Supabase Auth with token management
- **Real-time**: WebSocket connections for live battles
- **Edge Functions**: Server-side battle logic and matchmaking

## 📱 Mobile Features

- **Touch Controls**: Drag and drop card playing
- **Responsive UI**: Adapts to different screen sizes
- **Performance Optimized**: Object pooling, efficient rendering
- **Platform Integration**: iOS/Android specific features
- **Offline Support**: Local data caching for smooth experience

## 🎨 Visual Design

- **Pixel Art Style**: Maintains the original game's aesthetic
- **Smooth Animations**: DOTween-powered card and UI animations
- **Modern UI**: Clean, mobile-optimized interface design
- **Visual Effects**: Particle systems and shader effects

## 🚀 Development Workflow

1. **Scripts are created/updated** in the repository
2. **Unity auto-refreshes** and compiles changes
3. **Test in Unity Editor** with mobile simulator
4. **Build and test** on actual mobile devices
5. **Deploy** through app stores

## 📋 Requirements

- **Unity 6000.2.2f1** (Unity 6) or newer
- **Mobile Build Modules** (Android/iOS)
- **Required Packages**:
  - Newtonsoft JSON
  - DOTween (Pro recommended)
  - TextMeshPro
  - Unity UI

## 🎮 Getting Started

1. Follow the **UNITY_GITHUB_SETUP.md** guide to connect your Unity project
2. Open the project in Unity Hub
3. Let Unity import all assets and compile scripts
4. Open the main scene and press Play to test
5. Switch to mobile platform in Build Settings for mobile testing

## 🔧 Configuration

Key settings to configure:
- **Supabase URL and Keys** in SupabaseClient.cs
- **Mobile platform settings** in Project Settings
- **Input system** for touch controls
- **Graphics settings** for mobile performance

## 📊 Performance Targets

- **60 FPS** on mid-range mobile devices
- **< 2 second** scene load times
- **< 100 MB** total app size
- **< 50 MB** RAM usage during gameplay
- **Minimal battery drain** during idle states

## 🐛 Debugging

- Use Unity Console for runtime errors
- Enable Development Build for detailed logs
- Use Unity Profiler for performance analysis
- Test on various device configurations

## 🎉 What's Next

The development roadmap includes:
1. **Phase 1**: Core battle system implementation
2. **Phase 2**: Deck builder and collection UI
3. **Phase 3**: Real-time multiplayer integration
4. **Phase 4**: Polish, optimization, and store release

---

*This Unity project maintains full compatibility with the existing Supabase backend, ensuring seamless cross-platform gameplay between web and mobile users!* 🚀