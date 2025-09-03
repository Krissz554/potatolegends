# 🎮 Potato Card Game - Unity Mobile Version

## 📱 Unity 6 Mobile Trading Card Game

This is the Unity mobile version of "What's My Potato?" - a real-time multiplayer trading card game with potato-themed cards, battles, and collection mechanics.

## 🚀 Quick Start

### Requirements
- **Unity 6000.2.2f1** (Unity 6) or newer
- **Mobile Build Modules** (Android/iOS)
- **Git** for version control

### Opening the Project
1. **Clone this repository** to your local machine
2. **Switch to the `unity-mobile-game` branch**
3. **Open Unity Hub**
4. **Add project** and select this folder
5. **Unity will automatically import** all assets and scripts

## 🏗️ Project Structure

```
Assets/
├── Scripts/
│   ├── Core/          # GameManager, core systems
│   ├── Cards/         # Card display and interaction
│   ├── Battle/        # Combat system
│   ├── UI/            # User interface
│   ├── Network/       # Supabase integration
│   ├── Data/          # ScriptableObjects
│   └── Utils/         # Helper functions
├── Scenes/            # Game scenes
├── Prefabs/           # Reusable objects
├── Art/               # Sprites and textures
└── Audio/             # Sound effects
```

## 🔌 Backend Integration

- **Database**: Supabase PostgreSQL (same as web version)
- **Authentication**: Supabase Auth with Unity integration
- **Real-time**: WebSocket connections for live battles
- **API**: RESTful endpoints for game data

## 📱 Mobile Features

- **Touch Controls**: Drag and drop card playing
- **Responsive UI**: Adapts to all screen sizes
- **Performance Optimized**: 60+ FPS on mobile devices
- **Battery Efficient**: Optimized for extended gameplay
- **Cross-Platform**: Android and iOS support

## 🎯 Core Systems

### ✅ Implemented
- **GameManager**: Core game state management
- **SupabaseClient**: Backend authentication and API calls
- **CardData**: ScriptableObject system for 236+ cards
- **CardDisplay**: Interactive card UI with animations

### 🚧 In Development
- Battle system and turn-based combat
- Deck builder for mobile
- Collection browser
- Real-time multiplayer
- Hero system integration

## 🎨 Visual Style

- **Pixel Art**: Retro-inspired card designs
- **Modern UI**: Clean, mobile-optimized interface
- **Smooth Animations**: DOTween-powered transitions
- **Visual Effects**: Particle systems and shaders

## 🔧 Development

### Required Packages
- **Newtonsoft JSON**: For API communication
- **DOTween**: For smooth animations
- **TextMeshPro**: For text rendering
- **Unity UI**: For interface elements

### Build Targets
- **Android**: API Level 24+ (Android 7.0+)
- **iOS**: iOS 12.0+
- **Development**: Unity Editor for testing

## 🎮 Game Features

- **236+ Unique Cards** with different rarities and abilities
- **Real-time Battles** with turn-based strategy
- **Deck Building** with 30-card decks and copy limits
- **Collection System** for discovering and managing cards
- **Hero Powers** for strategic gameplay
- **Cross-platform Play** with web version users

## 🚀 Getting Started Development

1. **Open MainScene** in `Assets/Scenes/`
2. **Check GameManager** is in the scene
3. **Configure SupabaseClient** with your credentials
4. **Press Play** to test core systems
5. **Switch to mobile platform** in Build Settings

## 📊 Performance Targets

- **60+ FPS** on mid-range mobile devices
- **< 2 second** scene transitions
- **< 100 MB** total app size
- **Efficient memory usage** for smooth gameplay

---

*Built with Unity 6 for optimal mobile performance and modern development features!* 🚀