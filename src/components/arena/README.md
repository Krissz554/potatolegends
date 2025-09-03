# Pixel Arena System

A high-resolution pixel-art fantasy battle arena with responsive design and smooth 60fps performance.

## Overview

The Pixel Arena System replaces the previous battle UI with a modern pixel-art implementation featuring:
- **Circular stone dueling platform** on a mountaintop vista
- **Day/night cycle** with dynamic lighting and effects
- **Responsive 16:9 letterboxed design** that scales across all devices
- **Pixel-perfect rendering** with crisp edges and integer scaling
- **All existing game logic preserved** - only visuals and layout changed

## Component Architecture

### Core Components

#### `ArenaStage`
- **Purpose**: Main arena background with parallax layers
- **Features**: Day/night themes, mountain vistas, animated runes, fireflies
- **Props**: `timeOfDay: 'day' | 'night'`, `children`, `className`

#### `Board`
- **Purpose**: Handles slot math and unit positioning
- **Features**: Semicircular 7-slot layout, precise positioning math
- **Functions**: `getSlotPosition(col, side, totalCols)`, `getSlotFromPosition()`, `isValidDropZone()`

#### `UnitSprite`
- **Purpose**: Animated potato units with overlays
- **Features**: 5 animation states, stat overlays, status badges, VFX integration
- **States**: `idle`, `spawn`, `attack`, `hit`, `death`, `victory`

#### `HeroHUD`
- **Purpose**: Pixel-styled hero interface
- **Features**: Ornate frames, segmented health bars, crystal mana, cooldown indicators

#### `Hand`
- **Purpose**: Card hand with fanning and details modal
- **Features**: Responsive fanning, hover magnification, pixel parchment details

#### `VFXPortal`
- **Purpose**: Particle effects and visual feedback system
- **Features**: Damage numbers, spell effects, targeting lines, particle systems

### Utility Components

#### `UIComponents`
- `EndTurnButton`: Magical rune button with state animations
- `DeckCounter`: Pixel stack with count display
- `TurnTimer`: Hourglass with animated sand
- `ConcedeMenu`: Compact icon menu
- `Tooltip`: Pixel-styled tooltips

## Responsive Scaling System

### `useResponsiveScale` Hook

Manages the `--ui-scale` CSS variable for pixel-perfect scaling across devices:

```typescript
const { scale, breakpoint, scaleValue, isMobile } = useResponsiveScale()
```

#### Breakpoints
- **Mobile**: ≤768px (scale × 0.8)
- **Tablet**: 769-1023px (scale × 0.9)  
- **Desktop**: 1024-1279px (scale × 1.0)
- **Ultrawide**: ≥1280px (scale × 1.1)

#### Features
- 16:9 letterboxing maintains aspect ratio
- Integer scaling prevents pixel blur
- CSS custom properties for dynamic scaling
- Utility functions for responsive values

### CSS Custom Properties

The system automatically sets these CSS variables:
- `--ui-scale`: Current scale factor
- `--container-width`: Letterboxed width
- `--container-height`: Letterboxed height

## Slot Math & Positioning

### Board Geometry

Units are positioned on two semicircles (7 slots each):
- **Enemy slots**: Top semicircle (inverted)
- **Player slots**: Bottom semicircle

### Position Calculation

```typescript
const getSlotPosition = (col: number, side: 'player' | 'enemy', totalCols: number = 7) => {
  const centerX = 192 // Half of 384px arena width
  const radius = 120   // Semicircle radius
  
  const angleSpan = Math.PI // 180 degrees
  const angleStep = angleSpan / (totalCols - 1)
  const angle = col * angleStep
  
  let x, y
  if (side === 'enemy') {
    x = centerX + Math.cos(Math.PI - angle) * radius
    y = 60 - Math.sin(Math.PI - angle) * radius
  } else {
    x = centerX + Math.cos(angle) * radius
    y = 60 + Math.sin(angle) * radius
  }
  
  return { x: Math.round(x), y: Math.round(y) }
}
```

## Layer Order (Z-Index)

From back to front:
1. **Sky** (z-0): Parallax sky, mountains, clouds
2. **Mid Mountains** (z-10): Trees, parallax layers
3. **Parapet** (z-20): Stone wall background
4. **Platform** (z-30): Arena base and runes
5. **Board Slots** (z-40): Unit positioning guides
6. **Units** (z-50): Potato sprites with overlays
7. **VFX** (z-60): Particles, targeting lines
8. **HUD** (z-70): Heroes, cards, UI elements

## VFX System

### Triggering Effects

```typescript
// Global VFX functions
triggerDamage(position, damage)
triggerHeal(position, heal)
triggerSpawnDust(position)
triggerSlash(position)

// Custom effects
triggerVFX({
  type: 'bolt',
  position: { x, y },
  duration: 300
})
```

### Available Effects
- **spawn_dust**: Particle burst on unit spawn
- **slash**: Diagonal slash for attacks
- **bolt**: Lightning beam for spells
- **shield**: Circular protection effect
- **poison**: Rising green bubbles
- **freeze**: Ice shard burst
- **death_poof**: Expanding smoke cloud
- **damage_number**: Floating red numbers
- **heal_number**: Floating green numbers

## Pixel-Perfect Rendering

### CSS Classes

```css
.pixel-art {
  image-rendering: pixelated;
  image-rendering: -moz-crisp-edges;
  image-rendering: crisp-edges;
}

.pixel-font {
  font-family: 'Press Start 2P', monospace;
  image-rendering: pixelated;
}
```

### Transform Utility

```typescript
import { roundTransform } from '@/hooks/useResponsiveScale'

const transform = roundTransform({
  x: 10.7,  // becomes 11
  y: 5.3,   // becomes 5
  scale: 1.0
})
```

## Performance Optimizations

### Rendering
- Integer scaling only (1.0, 1.5, 2.0)
- Sprite sheet support for animations
- Texture reuse across components
- No heavy box-shadows (painted glows instead)

### Animation
- 60fps cap with RequestAnimationFrame
- Framer Motion with hardware acceleration
- Efficient state management
- VFX pooling and cleanup

### Memory
- Component lazy loading
- Effect cleanup on unmount
- Minimal DOM updates
- Optimized re-renders

## Game State Integration

All existing game logic is preserved:
- Battle session management
- Real-time multiplayer sync
- Turn system and timers
- Card playing and targeting
- Hero powers and abilities
- Mulligan system
- Victory/defeat handling

The new UI subscribes to the same game state and triggers the same actions, ensuring complete compatibility with existing backend systems.

## Mobile Adaptations

### Layout Changes
- **≤767px**: Single-column HUD layout
- **768-1023px**: Tighter spacing, scrollable hand
- **≥1024px**: Full desktop layout

### Touch Interactions
- Larger tap targets on mobile
- Swipe gestures for hand navigation
- Full-screen modals for tooltips
- Simplified gesture controls

### Text Scaling
- Minimum 12-14px for readability
- Responsive font scaling with `--ui-scale`
- Fallback fonts for pixel font readability

## Accessibility Features

### Visual
- Colorblind-safe status indicators (shapes + colors)
- High contrast pixel styling
- Clear focus states and outlines

### Interaction
- Keyboard navigation support
- Screen reader friendly labels
- Hover/focus state consistency
- Touch-friendly sizing

### Reduced Motion
- Respects `prefers-reduced-motion`
- Optional animation disable
- Essential animations only

## Asset Guidelines

### Directory Structure
```
src/assets/pixel/
├── arena/
│   ├── day/          # Day theme assets
│   └── night/        # Night theme assets
├── units/            # Unit sprite sheets
├── icons/            # UI icons
└── ui/               # Interface elements
```

### Export Settings
- **Base size**: 96×96px for units
- **Export scale**: 2× or 3× for HiDPI
- **Format**: PNG with transparency
- **Compression**: Lossless for crisp pixels
- **Naming**: `element_state_frame.png`

### Sprite Sheets
- Horizontal strips preferred
- Power-of-2 dimensions when possible
- Consistent frame timing
- Alpha channel for transparency

## Development Notes

### Environment Setup
The system automatically detects development mode and shows:
- Slot positioning guides
- Debug overlays
- Performance metrics
- Component boundaries

### Hot Reload
All components support hot module replacement:
- State preservation during development
- Instant visual feedback
- No game session interruption

### Testing
- Unit tests for slot math
- Visual regression tests
- Performance benchmarks
- Cross-device testing

## Browser Support

### Minimum Requirements
- **Chrome**: 88+
- **Firefox**: 85+
- **Safari**: 14+
- **Edge**: 88+

### Pixel Rendering Support
- `image-rendering: pixelated` (Chrome, Edge)
- `image-rendering: crisp-edges` (Firefox)
- `-moz-crisp-edges` (Firefox fallback)
- Automatic fallback to nearest-neighbor

## Future Enhancements

### Planned Features
- Animated backgrounds
- Weather effects
- Seasonal themes
- Custom arena unlocks
- Spectator mode support

### Performance Targets
- 60fps on mid-range devices
- <100ms input latency
- <2MB initial bundle size
- <50ms component render time