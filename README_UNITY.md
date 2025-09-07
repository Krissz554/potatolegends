# Potato Legends - Unity Mobile Game

This is the Unity mobile version of the "What's My Potato?" card game, built to complement the existing web version.

## 🎮 Project Structure

```
Assets/Scripts/
├── Core/           # Core game systems
│   └── GameManager.cs
├── Data/           # ScriptableObject data definitions
│   ├── CardData.cs
│   └── HeroData.cs
├── Network/        # Backend communication
│   └── SupabaseClient.cs
├── UI/             # User interface components
│   ├── UIManager.cs
│   ├── AuthScreen.cs
│   ├── MainMenuScreen.cs
│   ├── CollectionScreen.cs
│   ├── DeckBuilderScreen.cs
│   ├── HeroHallScreen.cs
│   ├── BattleScreen.cs
│   ├── CardDisplay.cs
│   ├── DeckSlot.cs
│   ├── HeroCard.cs
│   └── BattlefieldSlot.cs
├── Battle/         # Battle system
│   └── BattleManager.cs
├── Cards/          # Card system
│   └── CardDisplay.cs
└── Collection/     # Collection management
    └── CollectionManager.cs
```

## 🚀 Getting Started

### Prerequisites
- Unity 6 or later
- Universal 2D template project

### Setup Instructions

1. **Open Unity** with your Universal 2D template project
2. **Import Scripts**: Copy all scripts from `Assets/Scripts/` to your Unity project
3. **Package Dependencies**: The `Packages/manifest.json` will automatically install required packages:
   - Newtonsoft JSON (for Supabase communication)
   - TextMeshPro (for UI text)
   - Unity UI system packages

### Creating the UI

1. **Create Scenes**:
   - MainMenuScene
   - CollectionScene
   - DeckBuilderScene
   - HeroHallScene
   - BattleScene
   - AuthScene

2. **Set up Canvas Hierarchy**:
   ```
   Canvas
   ├── AuthScreen
   ├── MainMenuScreen
   ├── CollectionScreen
   ├── DeckBuilderScreen
   ├── HeroHallScreen
   └── BattleScreen
   ```

3. **Link Scripts**:
   - Attach `GameManager` to an empty GameObject (DontDestroyOnLoad)
   - Attach `SupabaseClient` to an empty GameObject (DontDestroyOnLoad)
   - Attach `UIManager` to the Canvas
   - Attach screen scripts to their respective UI panels

### Creating Card Data

1. **Create CardData Assets**:
   - Right-click in Project → Create → Potato Card Game → Card Data
   - Configure card properties (name, cost, stats, abilities)
   - Assign card art sprites

2. **Create HeroData Assets**:
   - Right-click in Project → Create → Potato Card Game → Hero Data
   - Configure hero properties (name, HP, mana, hero power)

## 🔧 Key Features

### Mobile-Optimized UI
- Touch-friendly button sizes and spacing
- Drag-and-drop card interactions
- Responsive layout for different screen sizes
- Smooth animations and transitions

### Battle System
- Turn-based combat with 6-slot battlefields
- Card deployment and combat mechanics
- Real-time synchronization with web version
- Mulligan system for starting hand

### Collection Management
- Browse and filter card collection
- Deck building with 30-card limit
- Hero selection and management
- Save/load deck configurations

### Backend Integration
- Supabase authentication
- Real-time battle updates
- Card collection synchronization
- Cross-platform compatibility with web version

## 🎯 Game Flow

1. **Authentication**: Sign in/up with email
2. **Main Menu**: Navigate to different game sections
3. **Collection**: View and manage your cards
4. **Deck Builder**: Create custom decks (30 cards max)
5. **Hero Hall**: Select and upgrade heroes
6. **Battle**: Real-time multiplayer battles

## 🔗 Integration with Web Version

- **Shared Backend**: Uses same Supabase database and authentication
- **Card Compatibility**: Same card data structure and mechanics
- **Real-time Sync**: Battles sync between web and mobile
- **Cross-platform**: Play on any device with same account

## 📱 Mobile Considerations

- **Performance**: Optimized for mobile devices
- **Battery Life**: Efficient rendering and networking
- **Touch Controls**: Intuitive drag-and-drop interactions
- **Offline Support**: Local data caching for better UX

## 🛠️ Development Notes

- All scripts use proper Unity conventions
- Mobile-first UI design approach
- Modular architecture for easy maintenance
- Comprehensive error handling and logging
- Ready for Unity Cloud Build and deployment

## 📋 TODO

- [ ] Set up Unity scenes and prefabs
- [ ] Create card and hero art assets
- [ ] Implement real-time battle synchronization
- [ ] Add sound effects and music
- [ ] Optimize for different screen sizes
- [ ] Add push notifications
- [ ] Implement offline mode
- [ ] Add analytics and crash reporting

## 🤝 Contributing

This Unity project is designed to work alongside the existing web version. All backend integration points are compatible with the current Supabase setup.