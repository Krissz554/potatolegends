# 🎨 DECK BUILDER CUSTOM ASSETS - COMPLETE GUIDE

## 📁 **COMPLETE FOLDER STRUCTURE CREATED:**

```
Assets/Resources/
├── UI/
│   ├── Backgrounds/
│   │   ├── deck-builder-bg.png           ← Main deck builder background
│   │   ├── collection-panel-bg.png       ← Left panel (Your Collection)
│   │   ├── deck-panel-bg.png             ← Right panel (Current Deck)
│   │   └── deck-management-bar-bg.png    ← Top management bar
│   │
│   ├── Cards/
│   │   ├── card-frame-collection.png     ← Collection card frame
│   │   ├── card-frame-deck.png           ← Deck card frame
│   │   ├── card-slot-empty.png           ← Empty deck slot
│   │   ├── card-quantity-badge.png       ← Quantity badge (2x, 3x)
│   │   └── card-rarity-borders/
│   │       ├── common-border.png         ← Gray border
│   │       ├── uncommon-border.png       ← Green border
│   │       ├── rare-border.png           ← Blue border
│   │       ├── legendary-border.png      ← Purple border
│   │       └── exotic-border.png         ← Gold border
│   │
│   ├── Buttons/
│   │   ├── create-deck-btn.png           ← "New Deck" button
│   │   ├── deck-selector-btn.png         ← "Decks" dropdown
│   │   ├── save-deck-btn.png             ← Save button
│   │   ├── delete-deck-btn.png           ← Delete button
│   │   ├── set-active-deck-btn.png       ← "Set Active" button
│   │   ├── add-to-deck-btn.png           ← "+" add card button
│   │   └── remove-from-deck-btn.png      ← "-" remove card button
│   │
│   └── Icons/
│       ├── mana-crystal-icon.png         ← Mana cost icon
│       ├── attack-icon.png               ← Attack stat icon
│       ├── health-icon.png               ← Health stat icon
│       ├── deck-count-icon.png           ← Card count icon
│       ├── active-deck-crown.png         ← Active deck crown
│       ├── deck-validation-check.png     ← Valid deck checkmark
│       └── deck-validation-error.png     ← Invalid deck X mark
│
└── ElementalBackgrounds/
    ├── fire-card-bg.png                 ← Fire element background
    ├── ice-card-bg.png                  ← Ice element background
    ├── lightning-card-bg.png            ← Lightning element background
    ├── light-card-bg.png                ← Light element background
    ├── void-card-bg.png                 ← Void element background
    └── exotic-card-bg.png               ← Exotic element background
```

## 🎯 **FINAL DECK BUILDER APPEARANCE:**

```
┌─[deck-management-bar-bg.png]─────────────────────────────────────────────┐
│ 🔧 DECK MANAGEMENT   👑[crown] Active: MyDeck  [New][Select][Save][Delete]│
├─[collection-panel-bg.png]───────────┬─[deck-panel-bg.png]───────────────┤
│ YOUR COLLECTION (226 cards)          │ MyDeck (15/30) [count-icon]       │
│                                       │                                   │
│ ┌─[card-frame-collection]─┐          │ ┌─[card-frame-deck]─┐             │
│ │[fire-bg] 🔥 Fire Potato │          │ │[fire-bg] 🔥 Fire  │ [2x badge]  │
│ │[mana]3 [atk]4 [hp]5     │          │ │ Potato [remove-btn]│             │
│ │         [add-btn]       │          │ └────────────────────┘             │
│ └─────────────────────────┘          │                                   │
│                                       │ ┌─[card-slot-empty]─┐             │
│ ┌─[card-frame-collection]─┐          │ │        [16]        │             │
│ │[ice-bg] ❄️ Ice Warrior  │          │ └────────────────────┘             │
│ │[mana]2 [atk]2 [hp]6     │          │                                   │
│ │         [add-btn]       │          │ (Continues to slot 30...)         │
│ └─────────────────────────┘          │                                   │
│                                       │                                   │
│ (Scrollable - all your cards...)     │ [validation-check] Deck Valid ✓   │
└───────────────────────────────────────┴───────────────────────────────────┘
```

## 🚀 **HOW TO USE:**

### **1. Upload Your Assets:**
- Place your custom images in the exact folders shown above
- Use the exact filenames specified
- PNG format recommended for transparency

### **2. Automatic Loading:**
- The system automatically detects and loads your custom assets
- Fallback colors are used if custom assets aren't found
- No code changes needed!

### **3. Instant Results:**
- Restart Unity project after uploading assets
- Your custom deck builder UI will appear immediately
- Professional, branded appearance matching your game style

## 📏 **RECOMMENDED SPECIFICATIONS:**

### **Backgrounds:**
- **Resolution:** 1024x768+ for panels, 1920x1080+ for main background
- **Format:** PNG with transparency support
- **Style:** Match your game's art direction

### **Cards & Buttons:**
- **Card Frames:** 120x160 pixels
- **Buttons:** 128x64 pixels (rectangular), 48x48 pixels (square)
- **Icons:** 32x32 pixels (stats), 24x24 pixels (UI)

### **Elemental Backgrounds:**
- **Size:** 120x160 pixels (card size)
- **Theme:** Clear visual distinction between elements
- **Colors:** Fire=Red, Ice=Blue, Lightning=Yellow, Light=White, Void=Purple, Exotic=Rainbow

## ✨ **RESULT:**
Your deck builder will transform from basic shapes to a professional, polished interface that matches your game's visual style perfectly!

**Upload your assets and watch your deck builder come to life!** 🎮🃏✨