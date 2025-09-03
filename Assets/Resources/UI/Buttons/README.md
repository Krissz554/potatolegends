# 🔘 CUSTOM BUTTONS FOLDER

## 📁 **Upload Your Custom Button Sprites Here:**

### **🎮 Button Types:**
- `primary-button.png` - Main action buttons (Login, Confirm, etc.)
- `battle-button.png` - **Special battle button sprite** (PRIORITY!)
- `secondary-button.png` - Secondary buttons (Back, Cancel, etc.)
- `utility-button.png` - Small utility buttons (Settings, Shop, etc.)

### **🎯 Button States (Advanced):**
- `primary-button-normal.png` - Normal state
- `primary-button-hover.png` - Hover/highlighted state
- `primary-button-pressed.png` - Pressed state
- `primary-button-disabled.png` - Disabled state

### **⚔️ Special Buttons:**
- `battle-button-idle.png` - Battle button normal state
- `battle-button-glow.png` - Battle button with glow effect
- `battle-button-pressed.png` - Battle button when pressed

## 📐 **Recommended Sizes:**
- **Primary Buttons:** 300x100px
- **Battle Button:** 400x120px (larger, more prominent)
- **Secondary Buttons:** 250x80px
- **Utility Buttons:** 120x60px

## 🎨 **Format & Design:**
- **PNG with transparency** (for rounded corners)
- **9-slice ready** (borders that can stretch)
- **Fantasy/game theme** styling
- **Consistent visual style** across all buttons
- **High contrast** for text readability

## 🔧 **Import Settings:**
After adding button sprites:
1. Select the button sprite in Unity
2. Inspector → Texture Type → "Sprite (2D and UI)"
3. Inspector → Sprite Mode → "Single"
4. Inspector → Mesh Type → "Full Rect"
5. Click "Apply"

## 🎯 **9-Slice Setup (For Scalable Buttons):**
1. Select your button sprite
2. Click "Sprite Editor"
3. Set border values (L, R, T, B) for corners that shouldn't stretch
4. Apply changes

## ✅ **Automatic Integration:**
The ProductionUIManager will automatically detect and use your custom button sprites when you add them to this folder!

## 🎮 **Priority Asset:**
**`battle-button.png`** is most important - this will make your battle button look exactly like your design!