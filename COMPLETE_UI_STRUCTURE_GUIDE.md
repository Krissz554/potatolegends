# Complete Mobile UI Structure Guide

## 🎮 Overview
This guide provides the complete UI structure for all scenes in the Unity mobile card game, based on the web version design patterns.

## 📱 Scene Structure Template

### Base Hierarchy (All Scenes)
```
📁 Scene Root
├── 📁 Game Managers
│   ├── 🎮 GameManager (Singleton)
│   ├── 🔊 AudioManager (Singleton) 
│   ├── 🎯 InputManager (Singleton)
│   ├── 🖥️ UIManager (Singleton)
│   └── 📋 Scene-Specific Manager
├── 📷 Camera
│   ├── Camera Component
│   └── AudioListener
└── 📱 UI
    ├── 📁 Background
    ├── 📁 MainContent (Scene-specific)
    ├── 📁 Overlay
    └── 📁 Loading
```

## 🔐 Auth Scene - Complete UI Structure

### Background Layer
- **Background Image**: Full-screen pixel art background
- **Gradient Overlay**: Dark overlay for readability

### AuthScreen Layer
```
📁 AuthScreen
├── 📁 AuthContainer (Center Panel)
│   ├── 🎨 LogoContainer
│   │   ├── 🥔 PotatoIcon (Image)
│   │   └── 📝 GameTitle (TextMeshPro)
│   ├── 📋 AuthTabs
│   │   ├── 🔘 SignInTab (Button)
│   │   └── 🔘 SignUpTab (Button)
│   ├── 📝 SignInForm
│   │   ├── 📧 EmailField (InputField)
│   │   ├── 🔒 PasswordField (InputField)
│   │   └── 🔘 SignInButton (Button)
│   ├── 📝 SignUpForm
│   │   ├── 📧 EmailField (InputField)
│   │   ├── 🔒 PasswordField (InputField)
│   │   └── 🔘 SignUpButton (Button)
│   └── 📄 TermsText (TextMeshPro)
├── 📁 LoadingIndicator (Hidden by default)
│   ├── 🔄 Spinner (Image)
│   └── 📝 LoadingText (TextMeshPro)
└── 📁 ErrorMessage (Hidden by default)
    └── 📝 ErrorText (TextMeshPro)
```

### Mobile Layout Specifications
- **Container**: 90% width, centered, max-width 400px
- **Logo**: 80x80px potato icon
- **Title**: "What's My Potato?" - 32px font
- **Tabs**: Full-width buttons with icons
- **Input Fields**: Full-width with padding
- **Buttons**: Full-width with gradient backgrounds

## 🏠 Main Menu Scene - Complete UI Structure

### Background Layer
- **Background Image**: Main pixel art background
- **Animated Elements**: Floating particles, glow effects

### MainMenu Layer
```
📁 MainMenu
├── 📁 TopBar
│   ├── 🎨 LogoSection
│   │   ├── 🥔 PotatoIcon (Image)
│   │   ├── 📝 GameTitle (TextMeshPro)
│   │   └── 📝 Subtitle (TextMeshPro)
│   ├── 📋 NavigationButtons
│   │   ├── 🔘 CollectionBtn (Button)
│   │   ├── 🔘 DeckBuilderBtn (Button)
│   │   ├── 🔘 HeroHallBtn (Button)
│   │   ├── 🔘 LeaderboardsBtn (Button)
│   │   └── 🔘 SocialBtn (Button)
│   └── 📁 UserSection
│       ├── 🔘 SettingsBtn (Button)
│       └── 📁 UserMenu (Dropdown)
├── 📁 CenterContent
│   ├── 📝 MainTitle (TextMeshPro) - "POTATO LEGENDS"
│   ├── 📝 Subtitle (TextMeshPro)
│   ├── 📝 EpicTagline (TextMeshPro)
│   ├── 📁 WelcomeBadge (Badge)
│   └── 📁 MatchmakingStatus (Panel)
├── 📁 BattleButton (Bottom Right)
│   ├── 🔘 BattleBtn (Button)
│   └── 📝 BattleText (TextMeshPro)
└── 📁 MobileMenu (Bottom Left - Mobile Only)
    ├── 🔘 CollectionBtn (Button)
    ├── 🔘 DeckBuilderBtn (Button)
    └── 🔘 HeroHallBtn (Button)
```

### Mobile Layout Specifications
- **Top Bar**: Fixed height 80px, responsive
- **Logo**: 60x60px with title
- **Navigation**: Horizontal scroll on mobile
- **Battle Button**: Fixed bottom-right, 120x60px
- **Mobile Menu**: Bottom-left grid 3x1

## 📚 Collection Scene - Complete UI Structure

### Collection Layer
```
📁 Collection
├── 📁 Header
│   ├── 📝 Title (TextMeshPro) - "Collection"
│   ├── 📊 StatsPanel
│   │   ├── 📝 TotalCards (TextMeshPro)
│   │   ├── 📝 UniqueCards (TextMeshPro)
│   │   └── 📝 Completion (TextMeshPro)
│   └── 🔘 BackButton (Button)
├── 📁 Filters
│   ├── 🔍 SearchField (InputField)
│   ├── 📋 RarityFilter (Dropdown)
│   ├── 📋 TypeFilter (Dropdown)
│   ├── 📋 ElementFilter (Dropdown)
│   └── 🔘 ClearFiltersBtn (Button)
├── 📁 ViewControls
│   ├── 🔘 GridViewBtn (Button)
│   ├── 🔘 ListViewBtn (Button)
│   └── 🔘 ShowOwnedToggle (Toggle)
├── 📁 CardGrid
│   ├── 📁 ScrollView
│   │   ├── 📁 Viewport
│   │   │   └── 📁 Content
│   │   │       └── 📁 CardContainer (Grid Layout)
│   │   │           ├── 🃏 CardPrefab1
│   │   │           ├── 🃏 CardPrefab2
│   │   │           └── ... (Dynamic Cards)
│   │   └── 📜 Scrollbar
│   └── 📁 Pagination
│       ├── 🔘 FirstPageBtn (Button)
│       ├── 🔘 PrevPageBtn (Button)
│       ├── 📝 PageInfo (TextMeshPro)
│       ├── 🔘 NextPageBtn (Button)
│       └── 🔘 LastPageBtn (Button)
└── 📁 CardDetails (Modal)
    ├── 🃏 CardImage (Image)
    ├── 📝 CardName (TextMeshPro)
    ├── 📝 CardDescription (TextMeshPro)
    ├── 📊 CardStats (Panel)
    └── 🔘 CloseBtn (Button)
```

### Mobile Layout Specifications
- **Header**: Fixed top, 60px height
- **Filters**: Horizontal scroll, 50px height
- **Card Grid**: 2 columns on mobile, 3 on tablet
- **Card Size**: 120x180px on mobile
- **Pagination**: Bottom center, 40px height

## 🃏 Deck Builder Scene - Complete UI Structure

### DeckBuilder Layer
```
📁 DeckBuilder
├── 📁 Header
│   ├── 📝 Title (TextMeshPro) - "Deck Builder"
│   ├── 📊 DeckStats
│   │   ├── 📝 CardCount (TextMeshPro)
│   │   ├── 📝 ManaCurve (TextMeshPro)
│   │   └── 📝 DeckRating (TextMeshPro)
│   └── 🔘 BackButton (Button)
├── 📁 MainContent
│   ├── 📁 LeftPanel (Card Library)
│   │   ├── 📁 LibraryHeader
│   │   │   ├── 🔍 SearchField (InputField)
│   │   │   ├── 📋 RarityFilter (Dropdown)
│   │   │   └── 📋 TypeFilter (Dropdown)
│   │   ├── 📁 LibraryGrid
│   │   │   └── 📁 ScrollView
│   │   │       ├── 📁 Viewport
│   │   │       │   └── 📁 Content
│   │   │       │       └── 📁 CardContainer (Grid Layout)
│   │   │       └── 📜 Scrollbar
│   │   └── 📁 LibraryStats
│   │       └── 📝 LibraryCount (TextMeshPro)
│   └── 📁 RightPanel (Deck Area)
│       ├── 📁 DeckHeader
│       │   ├── 📝 DeckName (InputField)
│       │   ├── 🔘 SaveBtn (Button)
│       │   └── 🔘 ClearBtn (Button)
│       ├── 📁 DeckSlots
│       │   ├── 🃏 Slot1 (DeckSlot)
│       │   ├── 🃏 Slot2 (DeckSlot)
│       │   └── ... (30 slots total)
│       └── 📁 DeckActions
│           ├── 🔘 ValidateBtn (Button)
│           ├── 🔘 TestBtn (Button)
│           └── 🔘 ShareBtn (Button)
└── 📁 DeckValidation (Modal)
    ├── 📝 ValidationTitle (TextMeshPro)
    ├── 📝 ValidationMessage (TextMeshPro)
    ├── 📊 ValidationDetails (Panel)
    └── 🔘 CloseBtn (Button)
```

### Mobile Layout Specifications
- **Layout**: Vertical stack on mobile
- **Left Panel**: Full width, 40% height
- **Right Panel**: Full width, 60% height
- **Deck Slots**: 3 columns, scrollable
- **Card Library**: 2 columns on mobile

## 👑 Hero Hall Scene - Complete UI Structure

### HeroHall Layer
```
📁 HeroHall
├── 📁 Header
│   ├── 📝 Title (TextMeshPro) - "Hero Hall"
│   ├── 📊 PlayerStats
│   │   ├── 📝 PlayerLevel (TextMeshPro)
│   │   ├── 📝 PlayerXP (TextMeshPro)
│   │   └── 📝 PlayerRank (TextMeshPro)
│   └── 🔘 BackButton (Button)
├── 📁 HeroSelection
│   ├── 📁 SelectedHero
│   │   ├── 👑 HeroImage (Image)
│   │   ├── 📝 HeroName (TextMeshPro)
│   │   ├── 📝 HeroDescription (TextMeshPro)
│   │   ├── 📊 HeroStats (Panel)
│   │   │   ├── 📝 Health (TextMeshPro)
│   │   │   ├── 📝 Attack (TextMeshPro)
│   │   │   └── 📝 Ability (TextMeshPro)
│   │   └── 🔘 SelectHeroBtn (Button)
│   └── 📁 HeroGrid
│       ├── 📁 ScrollView
│       │   ├── 📁 Viewport
│       │   │   └── 📁 Content
│       │   │       └── 📁 HeroContainer (Grid Layout)
│       │   │           ├── 👑 HeroCard1
│       │   │           ├── 👑 HeroCard2
│       │   │           └── ... (Dynamic Heroes)
│       │   └── 📜 Scrollbar
│       └── 📁 HeroFilters
│           ├── 📋 ElementFilter (Dropdown)
│           ├── 📋 RarityFilter (Dropdown)
│           └── 🔘 OwnedOnlyToggle (Toggle)
└── 📁 HeroDetails (Modal)
    ├── 👑 HeroImage (Image)
    ├── 📝 HeroName (TextMeshPro)
    ├── 📝 HeroDescription (TextMeshPro)
    ├── 📊 HeroAbilities (Panel)
    └── 🔘 CloseBtn (Button)
```

### Mobile Layout Specifications
- **Selected Hero**: Top section, 200px height
- **Hero Grid**: 2 columns on mobile
- **Hero Cards**: 150x200px on mobile
- **Filters**: Horizontal scroll

## ⚔️ Battle Scene - Complete UI Structure

### BattleUI Layer
```
📁 BattleUI
├── 📁 TopHUD
│   ├── 📁 PlayerHUD
│   │   ├── 👑 PlayerHero (Image)
│   │   ├── 📊 PlayerHealth (Slider)
│   │   ├── 📝 PlayerHealthText (TextMeshPro)
│   │   ├── 💎 PlayerMana (Panel)
│   │   │   ├── 💎 Mana1 (Image)
│   │   │   ├── 💎 Mana2 (Image)
│   │   │   └── ... (10 mana crystals)
│   │   └── 📝 PlayerName (TextMeshPro)
│   ├── 📁 BattleInfo
│   │   ├── 📝 TurnCounter (TextMeshPro)
│   │   ├── 📝 TurnTimer (TextMeshPro)
│   │   └── 📝 BattlePhase (TextMeshPro)
│   └── 📁 EnemyHUD
│       ├── 👑 EnemyHero (Image)
│       ├── 📊 EnemyHealth (Slider)
│       ├── 📝 EnemyHealthText (TextMeshPro)
│       ├── 💎 EnemyMana (Panel)
│       └── 📝 EnemyName (TextMeshPro)
├── 📁 Battlefield
│   ├── 📁 PlayerSide
│   │   ├── 🃏 BattlefieldSlot1
│   │   ├── 🃏 BattlefieldSlot2
│   │   ├── 🃏 BattlefieldSlot3
│   │   ├── 🃏 BattlefieldSlot4
│   │   └── 🃏 BattlefieldSlot5
│   └── 📁 EnemySide
│       ├── 🃏 BattlefieldSlot1
│       ├── 🃏 BattlefieldSlot2
│       ├── 🃏 BattlefieldSlot3
│       ├── 🃏 BattlefieldSlot4
│       └── 🃏 BattlefieldSlot5
├── 📁 HandArea
│   ├── 📁 PlayerHand
│   │   ├── 🃏 HandCard1
│   │   ├── 🃏 HandCard2
│   │   ├── 🃏 HandCard3
│   │   ├── 🃏 HandCard4
│   │   ├── 🃏 HandCard5
│   │   ├── 🃏 HandCard6
│   │   └── 🃏 HandCard7
│   └── 📁 HandActions
│       ├── 🔘 EndTurnBtn (Button)
│       ├── 🔘 ConcedeBtn (Button)
│       └── 🔘 SettingsBtn (Button)
├── 📁 ActionButtons
│   ├── 🔘 AttackBtn (Button)
│   ├── 🔘 DefendBtn (Button)
│   ├── 🔘 AbilityBtn (Button)
│   └── 🔘 PassBtn (Button)
└── 📁 BattleLog
    ├── 📁 LogContainer
    │   └── 📁 ScrollView
    │       ├── 📁 Viewport
    │       │   └── 📁 Content
    │       │       └── 📝 LogEntries (TextMeshPro)
    │       └── 📜 Scrollbar
    └── 🔘 ClearLogBtn (Button)
```

### Mobile Layout Specifications
- **Top HUD**: Fixed top, 100px height
- **Battlefield**: Center area, responsive
- **Hand Area**: Bottom, 120px height
- **Action Buttons**: Bottom-right overlay
- **Battle Log**: Collapsible side panel

## 🎨 UI Component Specifications

### Colors & Themes
- **Primary**: #3B82F6 (Blue)
- **Secondary**: #8B5CF6 (Purple)
- **Success**: #10B981 (Green)
- **Warning**: #F59E0B (Orange)
- **Error**: #EF4444 (Red)
- **Background**: #1F2937 (Dark Gray)
- **Surface**: #374151 (Medium Gray)
- **Text**: #F9FAFB (Light Gray)

### Typography
- **Title Font**: Cinzel (Fantasy)
- **Body Font**: Inter (Modern)
- **UI Font**: Roboto (Clean)
- **Sizes**: 12px, 14px, 16px, 18px, 24px, 32px, 48px

### Spacing & Layout
- **Padding**: 8px, 16px, 24px, 32px
- **Margins**: 4px, 8px, 16px, 24px
- **Border Radius**: 4px, 8px, 12px, 16px
- **Grid Gaps**: 8px, 16px, 24px

### Mobile Responsive Breakpoints
- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: > 1024px

## 🔧 Implementation Notes

### Script Integration
1. **Connect UI Scripts**: Link existing scripts to UI elements
2. **Event Handlers**: Set up button click events
3. **Data Binding**: Connect UI to game data
4. **Animation**: Add smooth transitions

### Performance Optimization
1. **Object Pooling**: For card instances
2. **Lazy Loading**: For large lists
3. **Texture Compression**: For mobile
4. **UI Batching**: Group similar elements

### Accessibility
1. **Touch Targets**: Minimum 44px
2. **Text Contrast**: WCAG AA compliant
3. **Screen Reader**: Proper labels
4. **Color Blind**: Alternative indicators

## 📱 Mobile-Specific Features

### Touch Interactions
- **Tap**: Single touch
- **Long Press**: Context menus
- **Swipe**: Navigation
- **Pinch**: Zoom (where applicable)
- **Drag**: Card movement

### Gesture Support
- **Swipe Left/Right**: Navigate between screens
- **Swipe Up**: Show additional options
- **Swipe Down**: Refresh content
- **Double Tap**: Quick actions

### Performance Considerations
- **60 FPS**: Smooth animations
- **Memory**: Efficient texture usage
- **Battery**: Optimize updates
- **Network**: Smart data loading

This guide provides the complete structure for implementing all UI scenes in Unity. Each scene follows mobile-first design principles and maintains consistency with the web version while being optimized for touch interactions and mobile performance.