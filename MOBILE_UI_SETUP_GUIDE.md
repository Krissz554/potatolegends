# 📱 Mobile UI Setup Guide

## 🎯 **What You Need to Create in Unity**

I've created the scripts, but you need to set up the UI elements in Unity Editor. Here's exactly what to create:

### **1. Scene Structure (Already Created)**
```
MainScene
├── Main Camera (orthographic, size 10)
├── UI Canvas (Screen Space - Overlay)
│   ├── LoginScreen (starts active)
│   └── MainGameUI (starts inactive)
├── GameManager
├── SupabaseClient  
├── CardManager
├── BattleManager
└── MobileInputHandler
```

### **2. LoginScreen UI Layout**

**Create these UI elements as children of LoginScreen:**

```
LoginScreen (Panel - dark background)
├── Title Text: "What's My Potato?" (large, center top)
├── Status Text: "Welcome! Please sign in to continue." (center)
├── LoginPanel (active by default)
│   ├── Email InputField
│   ├── Password InputField  
│   ├── Login Button
│   └── "Don't have account? Sign up" Button
├── SignupPanel (inactive by default)
│   ├── Email InputField
│   ├── Password InputField
│   ├── Display Name InputField
│   ├── Signup Button
│   └── "Already have account? Sign in" Button
└── Loading Indicator (inactive by default)
```

### **3. MainGameUI Layout**

**Create these UI elements as children of MainGameUI:**

```
MainGameUI (Panel - transparent)
├── TopBar
│   ├── Screen Title Text (left)
│   ├── User Name Text (center)
│   ├── Settings Button (right)
│   └── Logout Button (right)
├── NavigationBar (bottom)
│   ├── Main Page Button (icon/text)
│   ├── Collection Button (icon/text)  
│   ├── Deck Builder Button (icon/text)
│   └── Hero Hall Button (icon/text)
├── Battle Button (bottom right corner, large)
├── Screen Panels
│   ├── MainPagePanel (active by default)
│   │   ├── Welcome Text
│   │   ├── Stats Text
│   │   ├── Quick Play Button
│   │   └── Matchmaking Panel (inactive)
│   │       ├── Match Status Text
│   │       └── Cancel Match Button
│   ├── CollectionPanel (inactive)
│   ├── DeckBuilderPanel (inactive)
│   └── HeroHallPanel (inactive)
```

## 🔧 **How to Set Up in Unity:**

### **Step 1: Create UI Canvas**
1. **Right-click in Hierarchy** → UI → Canvas
2. **Set Canvas Scaler**:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1080 x 1920 (mobile portrait)
   - Screen Match Mode: Match Width Or Height (0.5)

### **Step 2: Create LoginScreen**
1. **Right-click Canvas** → UI → Panel
2. **Rename to "LoginScreen"**
3. **Add LoginScreen.cs script**
4. **Create child UI elements** (InputFields, Buttons, Text)
5. **Assign references** in the LoginScreen script inspector

### **Step 3: Create MainGameUI**
1. **Right-click Canvas** → UI → Panel  
2. **Rename to "MainGameUI"**
3. **Set inactive** by default
4. **Add MobileMainUI.cs script**
5. **Create child UI elements** for navigation and screens
6. **Assign references** in the MobileMainUI script inspector

### **Step 4: Connect Scripts**
1. **LoginScreen script**: Assign mainGameUI reference
2. **MobileMainUI script**: Assign loginScreen reference
3. **Assign all UI element references** in both scripts

## 🎨 **UI Styling Tips:**

### **Colors (Potato Theme)**
- Background: Dark purple/brown (#2D1B35)
- Primary: Orange/gold (#FF8C42)
- Secondary: Green (#4ECDC4)
- Text: White/cream (#F7FFF7)

### **Mobile Optimization**
- **Button min size**: 120x120 pixels (good for touch)
- **Text min size**: 24pt for readability
- **Padding**: 20-40 pixels between elements
- **Safe areas**: Keep important UI away from screen edges

### **Navigation Icons**
For now, use **text labels**:
- Main: "🏠 Home"
- Collection: "📚 Cards" 
- Deck Builder: "🔧 Decks"
- Hero Hall: "🦸 Heroes"
- Battle: "⚔️ BATTLE"

## 🚀 **Expected Behavior:**

1. **Game starts** → LoginScreen appears automatically
2. **User logs in** → MainGameUI appears, LoginScreen hides
3. **Navigation works** → Tap buttons to switch screens
4. **Battle button** → Starts matchmaking
5. **Logout** → Returns to LoginScreen

## 🎯 **Test Flow:**

1. **Press Play** → Should see login screen
2. **Enter credentials** → Should connect to Supabase
3. **After login** → Should see main UI with navigation
4. **Test navigation** → Should switch between screens
5. **Test battle button** → Should start matchmaking

---

*The scripts are ready - you just need to create the UI layout in Unity Editor!* 🎮