# Unity Mobile Game Hierarchy Structure

## 🎮 Complete Mobile Game Hierarchy

This document outlines the proper hierarchy structure for mobile Unity games, following industry best practices.

## 📁 Scene Hierarchy Structure

### **MainMenu Scene Hierarchy:**

```
📁 MainMenu
├── 📁 Game Managers
│   ├── 🎮 GameManager (Singleton)
│   ├── 🔊 AudioManager (Singleton)
│   ├── 🎯 InputManager (Singleton)
│   └── 🖥️ UIManager (Singleton)
├── 📷 Camera
│   ├── Camera Component
│   └── AudioListener
└── 📱 UI
    ├── 📁 Background
    │   └── Background Image/Panel
    ├── 📁 MainMenu
    │   ├── 🏷️ Title Text
    │   ├── 🔘 Battle Button
    │   ├── 🔘 Collection Button
    │   ├── 🔘 Deck Builder Button
    │   ├── 🔘 Hero Hall Button
    │   ├── 🔘 Settings Button
    │   └── 🔘 Logout Button
    ├── 📁 Overlay
    │   ├── 🔔 Notifications
    │   ├── ⚠️ Error Messages
    │   └── ℹ️ Info Panels
    └── 📁 Loading
        ├── 🔄 Loading Spinner
        └── 📊 Progress Bar
```

## 🏗️ Hierarchy Best Practices

### **1. Game Managers (Top Level)**
- **Purpose**: Core game systems that persist across scenes
- **Structure**: Organized under "Game Managers" parent
- **Components**: Singleton managers for core functionality

### **2. Camera System**
- **Purpose**: Main camera with AudioListener
- **Structure**: Separate from UI for better organization
- **Components**: Camera + AudioListener

### **3. UI Hierarchy (Layered)**
- **Background**: Static background elements
- **MainMenu**: Primary UI content
- **Overlay**: Pop-ups, notifications, modals
- **Loading**: Loading screens and progress indicators

## 📱 Mobile-Specific Considerations

### **UI Scaling**
- Canvas Scaler: Scale With Screen Size
- Reference Resolution: 1920x1080
- Match Width or Height: 0.5 (balanced scaling)

### **Touch Optimization**
- Large touch targets (minimum 44x44 pixels)
- Proper spacing between interactive elements
- Touch-friendly button sizes

### **Performance**
- UI elements organized by update frequency
- Static elements separated from dynamic ones
- Proper layering to minimize draw calls

## 🎯 Scene-Specific Hierarchies

### **Collection Scene:**
```
📁 Collection
├── 📁 Game Managers (same as MainMenu)
├── 📷 Camera
└── 📱 UI
    ├── 📁 Background
    ├── 📁 Collection
    │   ├── 🏷️ Title
    │   ├── 🔍 Search Bar
    │   ├── 📋 Filter Buttons
    │   └── 📱 Card Grid
    ├── 📁 Overlay
    └── 📁 Loading
```

### **DeckBuilder Scene:**
```
📁 DeckBuilder
├── 📁 Game Managers
├── 📷 Camera
└── 📱 UI
    ├── 📁 Background
    ├── 📁 DeckBuilder
    │   ├── 🏷️ Title
    │   ├── 📋 Deck Slots (7 slots)
    │   ├── 📚 Card Library
    │   ├── 💾 Save Button
    │   └── 🗑️ Clear Button
    ├── 📁 Overlay
    └── 📁 Loading
```

### **Battle Scene:**
```
📁 Battle
├── 📁 Game Managers
│   ├── GameManager
│   ├── AudioManager
│   ├── InputManager
│   ├── UIManager
│   └── BattleManager (Battle-specific)
├── 📷 Camera
├── 🎮 Gameplay
│   ├── 📁 Battlefield
│   │   ├── 👤 Player Slots (7 slots)
│   │   └── 🤖 Enemy Slots (7 slots)
│   ├── 📁 Heroes
│   │   ├── 👤 Player Hero
│   │   └── 🤖 Enemy Hero
│   └── 📁 Effects
└── 📱 UI
    ├── 📁 Background
    ├── 📁 BattleUI
    │   ├── ❤️ Health Bars
    │   ├── ⚡ Mana Display
    │   ├── 🃏 Hand Cards
    │   ├── ⏭️ End Turn Button
    │   └── 📊 Battle Log
    ├── 📁 Overlay
    └── 📁 Loading
```

## 🔧 Implementation Notes

### **Singleton Pattern**
- All managers use Singleton pattern
- Persistent across scene changes
- Easy access from anywhere in code

### **UI Layering**
- Background: Layer 0 (furthest back)
- MainMenu: Layer 1 (main content)
- Overlay: Layer 2 (pop-ups, modals)
- Loading: Layer 3 (topmost, when active)

### **Mobile Optimization**
- Touch-friendly button sizes
- Proper spacing for thumb navigation
- Responsive design for different screen sizes
- Performance-optimized UI updates

## 🚀 Benefits of This Structure

1. **Organization**: Clear separation of concerns
2. **Scalability**: Easy to add new features
3. **Maintainability**: Logical hierarchy structure
4. **Performance**: Optimized for mobile devices
5. **Team Development**: Multiple developers can work efficiently
6. **Debugging**: Easy to locate and fix issues

## 📋 Next Steps

1. **Create Prefabs**: Make reusable UI components
2. **Script Integration**: Connect scripts to UI elements
3. **Animation Setup**: Add smooth transitions
4. **Testing**: Test on various mobile devices
5. **Optimization**: Profile and optimize performance

This hierarchy structure provides a solid foundation for mobile Unity game development! 🎮📱