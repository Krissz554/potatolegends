# 🎨 CUSTOM ASSET PLACEMENT GUIDE

## 📁 **WHERE TO PUT YOUR CUSTOM ASSETS:**

### **🎯 Exact File Locations:**

#### **💰 Currency Icons:**
```
Assets/UI/Icons/gold-icon.png         ← Your custom gold icon
Assets/UI/Icons/gems-icon.png         ← Your custom gems icon
```

#### **🔘 Navigation Icons:**
```
Assets/UI/Icons/battle-icon.png       ← Your custom battle icon
Assets/UI/Icons/settings-icon.png     ← Your custom settings icon
Assets/UI/Icons/shop-icon.png         ← Your custom shop icon
Assets/UI/Icons/logout-icon.png       ← Your custom logout icon
```

#### **🖼️ Background Images:**
```
Assets/UI/Backgrounds/main-menu-bg.png    ← Your custom main screen background
Assets/UI/Backgrounds/auth-bg.png         ← Auth screen background (optional)
Assets/UI/Backgrounds/collection-bg.png   ← Collection background (optional)
Assets/UI/Backgrounds/deck-builder-bg.png ← Deck builder background (optional)
Assets/UI/Backgrounds/hero-hall-bg.png    ← Hero hall background (optional)
```

#### **🎮 Button Assets (Optional):**
```
Assets/UI/Buttons/primary-button.png     ← Custom button sprite
Assets/UI/Buttons/battle-button.png      ← Special battle button
Assets/UI/Buttons/secondary-button.png   ← Secondary button style
```

---

## 🔧 **HOW TO ADD YOUR ASSETS:**

### **Step 1: Copy Files to Unity**
1. **Drag & drop** your custom images into the exact folders above
2. **Unity will automatically import** them

### **Step 2: Configure Import Settings**
For each image:
1. **Select the image** in Unity Project window
2. **Inspector → Texture Type** → **"Sprite (2D and UI)"**
3. **Click "Apply"**

### **Step 3: Connect to UI System**
I'll create an automatic connection system, or you can:
1. **Create UI Asset Library:** `Assets → Create → Potato Legends → UI Asset Library`
2. **Drag your sprites** to the appropriate fields
3. **Assign the library** to ProductionUIManager

---

## 🎯 **AUTOMATIC ASSET LOADING:**

Let me update the ProductionUIManager to automatically find and use your custom assets:

### **Smart Asset Detection:**
- **Automatically loads** `main-menu-bg.png` for main menu
- **Automatically loads** `gold-icon.png` for currency display
- **Automatically loads** `battle-icon.png` for battle button
- **Falls back to beautiful defaults** if assets not found

### **No Manual Assignment Needed:**
- Just drop files in the right folders
- The system automatically finds and uses them
- **Immediate visual upgrade!**

---

## 📐 **RECOMMENDED ASSET SIZES:**

### **🎨 Icons (Square, Power of 2):**
```
Gold Icon:     64x64px   or  128x128px
Gems Icon:     64x64px   or  128x128px
Battle Icon:   128x128px or  256x256px
Settings Icon: 64x64px   or  128x128px
Shop Icon:     64x64px   or  128x128px
Logout Icon:   64x64px   or  128x128px
```

### **🖼️ Backgrounds (16:9 or taller for mobile):**
```
Main Menu BG:  1080x1920px (or any 16:9+ ratio)
Auth BG:       1080x1920px (or any 16:9+ ratio)
Other BGs:     1080x1920px (optional)
```

### **🔘 Buttons (Rectangular, 9-slice ready):**
```
Primary Button:    300x100px (with 9-slice borders)
Battle Button:     400x120px (with 9-slice borders)
Secondary Button:  250x80px  (with 9-slice borders)
```

---

## 🎨 **ASSET PREPARATION TIPS:**

### **✅ For Best Results:**
- **PNG format** with transparency
- **High resolution** for crisp mobile display
- **Consistent art style** across all assets
- **Proper alpha channels** for transparency
- **Power-of-2 dimensions** for performance

### **🎭 Style Consistency:**
- **Match your web version** visual style
- **Consistent color palette** across icons
- **Similar line weights** and details
- **Unified fantasy theme**

---

## 🔄 **AFTER YOU ADD ASSETS:**

### **I'll Update the Code to Use Them:**

Once you place your assets, I'll modify ProductionUIManager to:
1. **Automatically load** your custom assets
2. **Apply them** to the appropriate UI elements
3. **Ensure proper scaling** and positioning
4. **Add any needed visual effects**

### **Or You Can Use the Visual System:**
1. **Create UI Asset Library** ScriptableObject
2. **Drag your assets** to the fields
3. **Assign to ProductionUIManager**
4. **Automatic integration!**

---

## 🚀 **IMMEDIATE BENEFITS:**

### **🎨 Visual Upgrade:**
- **Your custom gold icon** in currency display
- **Your custom battle icon** on the prominent battle button
- **Your custom background** for immersive main menu
- **Your custom navigation icons** for professional feel

### **🎮 Professional Polish:**
- **Consistent visual style** throughout the game
- **Custom branding** with your assets
- **Professional mobile game appearance**
- **Unified art direction**

---

## 📋 **QUICK CHECKLIST:**

- [ ] Drop `gold-icon.png` in `Assets/UI/Icons/`
- [ ] Drop `gems-icon.png` in `Assets/UI/Icons/`
- [ ] Drop `battle-icon.png` in `Assets/UI/Icons/`
- [ ] Drop `settings-icon.png` in `Assets/UI/Icons/`
- [ ] Drop `shop-icon.png` in `Assets/UI/Icons/`
- [ ] Drop `logout-icon.png` in `Assets/UI/Icons/`
- [ ] Drop `main-menu-bg.png` in `Assets/UI/Backgrounds/`
- [ ] Set all images to **"Sprite (2D and UI)"** type
- [ ] Test in game = **BEAUTIFUL CUSTOM UI!**

**After you add these assets, your game will have your EXACT custom visual style with professional mobile game polish!** 🎮✨🎨