# 🃏 DECK BUILDER CARD ASSETS

Upload your custom card-related images here!

## 📁 Required Card Assets:

### **Card Frames:**
- `card-frame-collection.png` - Frame for cards in collection view (left panel)
- `card-frame-deck.png` - Frame for cards in deck view (right panel)
- `card-slot-empty.png` - Empty deck slot placeholder (for slots 16-30)
- `card-quantity-badge.png` - Badge background for quantities (2x, 3x copies)

### **📏 Recommended Specifications:**
- **Format:** PNG with transparency
- **Card Frame Size:** 120x160 pixels (or proportional)
- **Empty Slot Size:** 100x140 pixels
- **Quantity Badge:** 32x32 pixels (small circular badge)

### **🎯 How They're Used:**
```
Collection Panel:                 Deck Panel:
┌─[card-frame-collection.png]─┐   ┌─[card-frame-deck.png]─┐
│ 🔥 Fire Potato              │   │ 🔥 Fire Potato        │
│ Mana: 3  ATK: 4  HP: 5      │   │ [quantity-badge] 2x    │
│ [+] Add to Deck             │   │ [-] Remove from Deck   │
└─────────────────────────────┘   └────────────────────────┘

Empty Deck Slots:
┌─[card-slot-empty.png]─┐
│        [16]           │  ← Slot numbers for empty spaces
│                       │
└───────────────────────┘
```

### **🌟 Rarity Borders (Optional):**
Upload to `card-rarity-borders/` subfolder:
- `common-border.png` - Gray/white border
- `uncommon-border.png` - Green border  
- `rare-border.png` - Blue border
- `legendary-border.png` - Purple border
- `exotic-border.png` - Gold/rainbow border

### **🚀 After Upload:**
Your cards will look professional with custom frames and visual effects!