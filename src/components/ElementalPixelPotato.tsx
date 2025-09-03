import React from "react";

interface ElementalPixelPotatoProps {
  seed: string;
  potatoType: 'ice' | 'fire' | 'lightning' | 'light' | 'void';
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary' | 'exotic';
  cardName: string;
  size?: number;
}

// Simple hash to number for seeded randomness
function hashString(str: string) {
  let h = 2166136261 >>> 0;
  for (let i = 0; i < str.length; i++) {
    h ^= str.charCodeAt(i);
    h += (h << 1) + (h << 4) + (h << 7) + (h << 8) + (h << 24);
  }
  return h >>> 0;
}

function mulberry32(a: number) {
  return function () {
    let t = (a += 0x6d2b79f5);
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

// Element-based color schemes
const elementColors = {
  ice: {
    base: "hsl(200 80% 75%)",
    spot: "hsl(200 70% 60%)",
    accent: "hsl(180 90% 85%)",
    highlight: "hsl(220 100% 90%)",
    shadow: "hsl(200 60% 45%)"
  },
  fire: {
    base: "hsl(15 90% 65%)",
    spot: "hsl(0 85% 55%)",
    accent: "hsl(45 100% 70%)",
    highlight: "hsl(60 100% 85%)",
    shadow: "hsl(0 70% 35%)"
  },
  lightning: {
    base: "hsl(280 70% 70%)",
    spot: "hsl(270 80% 60%)",
    accent: "hsl(290 90% 80%)",
    highlight: "hsl(300 100% 90%)",
    shadow: "hsl(270 60% 40%)"
  },
  light: {
    base: "hsl(50 90% 80%)",
    spot: "hsl(45 85% 70%)",
    accent: "hsl(60 100% 90%)",
    highlight: "hsl(55 100% 95%)",
    shadow: "hsl(40 70% 50%)"
  },
  void: {
    base: "hsl(270 30% 25%)",
    spot: "hsl(260 40% 15%)",
    accent: "hsl(280 50% 35%)",
    highlight: "hsl(290 60% 45%)",
    shadow: "hsl(250 20% 10%)"
  }
};

// 16x16 potato silhouette
const shape: string[] = [
  ".......xx.......",
  ".....xxxxxx.....",
  "....xxxxxxxx....",
  "...xxxxxxxxxx...",
  "..xxxxxxxxxxxx..",
  "..xxxxxxxxxxxx..",
  ".xxxxxxxxxxxxxx.",
  ".xxxxxxxxxxxxxx.",
  ".xxxxxxxxxxxxxx.",
  ".xxxxxxxxxxxxxx.",
  "..xxxxxxxxxxxx..",
  "..xxxxxxxxxxxx..",
  "...xxxxxxxxxx...",
  "....xxxxxxxx....",
  ".....xxxxxx.....",
  "......xxxx......",
];

const ElementalPixelPotato: React.FC<ElementalPixelPotatoProps> = ({ 
  seed, 
  potatoType, 
  rarity, 
  cardName,
  size = 256 
}) => {
  const h = hashString((seed || '') + (potatoType || '') + (cardName || '') + (rarity || ''));
  const rand = mulberry32(h);
  const cell = Math.floor(size / 16);
  const padding = Math.floor((size - cell * 16) / 2);

  const theme = elementColors[potatoType] || elementColors.light;

  // More varied facial features based on card identity
  const cardHash = hashString(cardName + seed + rarity);
  const faceRand = mulberry32(cardHash);
  
  // Eyes: much more variation
  const eyeVariation = Math.floor(faceRand() * 8); // 8 different eye styles
  const eyeRow = 6 + Math.floor(faceRand() * 3); // More eye row variation
  const leftEyeCol = 4 + Math.floor(faceRand() * 3); // More horizontal variation
  const rightEyeCol = 9 + Math.floor(faceRand() * 3);
  
  // Multiple smile/expression types
  const expressionType = Math.floor(faceRand() * 6); // 6 different expressions
  const smileRow = eyeRow + 2 + Math.floor(faceRand() * 2);

  // Unique features based on card identity
  const personalityHash = hashString(cardName + 'personality');
  const personalityRand = mulberry32(personalityHash);
  
  // Face shape variations
  const faceShape = Math.floor(personalityRand() * 4); // Different face shapes
  const hasBeard = personalityRand() > 0.7; // Some cards have beards
  const hasEyebrows = personalityRand() > 0.4; // Eyebrow variations
  const hasCheeks = personalityRand() > 0.6; // Rosy cheeks
  const hasScars = personalityRand() > 0.85; // Battle scars for tough cards
  const hasGlasses = (cardName || '').toLowerCase().includes('professor') || (cardName || '').toLowerCase().includes('scholar') || personalityRand() > 0.9;
  const hasHat = (cardName || '').toLowerCase().includes('captain') || (cardName || '').toLowerCase().includes('general') || (cardName || '').toLowerCase().includes('lord');
  const hasMustache = personalityRand() > 0.8;
  
  // Element-specific unique features
  const hasElementalAura = rarity === 'exotic' || rarity === 'legendary';
  const elementalIntensity = Math.floor(personalityRand() * 3) + 1;
  
  // Dynamic spots based on personality and element
  const spotCount = 2 + Math.floor(rand() * 5);
  const spots = Array.from({ length: spotCount }).map(() => ({
    r: 2 + Math.floor(rand() * 12),
    c: 2 + Math.floor(rand() * 12),
    intensity: Math.floor(rand() * 3) + 1
  }));

  // Element-specific features
  const getElementalFeatures = () => {
    switch (potatoType) {
      case 'ice':
        return {
          hasSnowflakes: rand() > 0.3,
          hasCrystals: rand() > 0.5,
          hasIcyGlow: true
        };
      case 'fire':
        return {
          hasFlames: rand() > 0.4,
          hasEmbers: rand() > 0.3,
          hasGlow: true
        };
      case 'lightning':
        return {
          hasSparks: rand() > 0.3,
          hasElectricField: rand() > 0.5,
          hasGlow: true
        };
      case 'light':
        return {
          hasRays: rand() > 0.2,
          hasGlow: true,
          hasStars: rand() > 0.4
        };
      case 'void':
        return {
          hasShadows: rand() > 0.3,
          hasVoidPortals: rand() > 0.6,
          hasEthereal: true
        };
      default:
        return {};
    }
  };

  const features = getElementalFeatures();

  // Rarity-based accessories
  const getRarityFeatures = () => {
    switch (rarity) {
      case 'exotic':
        return {
          hasCrown: true,
          hasAura: true,
          eyeGlow: true
        };
      case 'legendary':
        return {
          hasHat: true,
          hasAura: rand() > 0.3,
          eyeGlow: rand() > 0.5
        };
      case 'rare':
        return {
          hasAccessory: rand() > 0.4,
          hasGlow: rand() > 0.6
        };
      case 'uncommon':
        return {
          hasAccessory: rand() > 0.7
        };
      default:
        return {};
    }
  };

  const rarityFeatures = getRarityFeatures();

  return (
    <svg
      width={size}
      height={size}
      viewBox={`0 0 ${size} ${size}`}
      role="img"
      aria-label={`${potatoType} ${rarity} pixel-art potato: ${cardName}`}
      data-card-id={seed}
      style={{
        imageRendering: "pixelated",
        shapeRendering: "crispEdges",
      }}
    >
      {/* Background with element-specific effects - REMOVED FOR CLEAN CARDS */}
      {/* Aura for high rarity cards - REMOVED FOR CLEAN CARDS */}

      {/* Element-specific background effects - REMOVED FOR CLEAN CARDS */}

      {/* Potato body */}
      {shape.map((row, r) =>
        row.split("").map((ch, c) =>
          ch === "x" ? (
            <rect
              key={`${r}-${c}`}
              x={padding + c * cell}
              y={padding + r * cell}
              width={cell}
              height={cell}
              fill={theme.base}
            />
          ) : null
        )
      )}

      {/* Element-specific spots/patterns with intensity */}
      {spots.map((s, i) => (
        <rect
          key={`spot-${i}`}
          x={padding + s.c * cell}
          y={padding + s.r * cell}
          width={cell}
          height={cell}
          fill={theme.spot}
          opacity={0.3 + (s.intensity * 0.2)}
        />
      ))}

      {/* Eyebrows (if character has them) */}
      {hasEyebrows && (
        <>
          <rect
            x={padding + (leftEyeCol - 0.5) * cell}
            y={padding + (eyeRow - 1) * cell}
            width={cell * 1.5}
            height={cell * 0.5}
            fill={theme.shadow}
          />
          <rect
            x={padding + (rightEyeCol - 0.5) * cell}
            y={padding + (eyeRow - 1) * cell}
            width={cell * 1.5}
            height={cell * 0.5}
            fill={theme.shadow}
          />
        </>
      )}

      {/* Eyes with different styles based on eyeVariation */}
      {eyeVariation === 0 && (
        // Normal round eyes
        <>
          <rect
            x={padding + leftEyeCol * cell}
            y={padding + eyeRow * cell}
            width={cell}
            height={cell}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
          <rect
            x={padding + rightEyeCol * cell}
            y={padding + eyeRow * cell}
            width={cell}
            height={cell}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
        </>
      )}
      
      {eyeVariation === 1 && (
        // Wide eyes
        <>
          <rect
            x={padding + leftEyeCol * cell}
            y={padding + eyeRow * cell}
            width={cell * 1.5}
            height={cell}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
          <rect
            x={padding + (rightEyeCol - 0.5) * cell}
            y={padding + eyeRow * cell}
            width={cell * 1.5}
            height={cell}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
        </>
      )}
      
      {eyeVariation === 2 && (
        // Sleepy/tired eyes
        <>
          <rect
            x={padding + leftEyeCol * cell}
            y={padding + (eyeRow + 0.3) * cell}
            width={cell}
            height={cell * 0.4}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
          <rect
            x={padding + rightEyeCol * cell}
            y={padding + (eyeRow + 0.3) * cell}
            width={cell}
            height={cell * 0.4}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
        </>
      )}
      
      {eyeVariation === 3 && (
        // Angry eyes
        <>
          <rect
            x={padding + (leftEyeCol + 0.2) * cell}
            y={padding + (eyeRow - 0.2) * cell}
            width={cell * 0.8}
            height={cell * 1.4}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
          <rect
            x={padding + rightEyeCol * cell}
            y={padding + (eyeRow - 0.2) * cell}
            width={cell * 0.8}
            height={cell * 1.4}
            fill={rarityFeatures.eyeGlow ? theme.highlight : "#000"}
          />
        </>
      )}
      
      {eyeVariation >= 4 && (
        // Star eyes (for special cards)
        <>
          {/* Left star eye */}
          <rect x={padding + leftEyeCol * cell} y={padding + (eyeRow + 0.2) * cell} width={cell} height={cell * 0.6} fill={theme.highlight} />
          <rect x={padding + (leftEyeCol + 0.2) * cell} y={padding + eyeRow * cell} width={cell * 0.6} height={cell} fill={theme.highlight} />
          {/* Right star eye */}
          <rect x={padding + rightEyeCol * cell} y={padding + (eyeRow + 0.2) * cell} width={cell} height={cell * 0.6} fill={theme.highlight} />
          <rect x={padding + (rightEyeCol + 0.2) * cell} y={padding + eyeRow * cell} width={cell * 0.6} height={cell} fill={theme.highlight} />
        </>
      )}

      {/* Cheeks (if character has them) */}
      {hasCheeks && (
        <>
          <rect
            x={padding + (leftEyeCol - 1) * cell}
            y={padding + (eyeRow + 1) * cell}
            width={cell * 0.8}
            height={cell * 0.8}
            fill={theme.accent}
            opacity={0.6}
            rx={cell * 0.4}
          />
          <rect
            x={padding + (rightEyeCol + 1.2) * cell}
            y={padding + (eyeRow + 1) * cell}
            width={cell * 0.8}
            height={cell * 0.8}
            fill={theme.accent}
            opacity={0.6}
            rx={cell * 0.4}
          />
        </>
      )}

      {/* Different expressions based on expressionType */}
      {expressionType === 0 && (
        // Happy smile
        <rect
          x={padding + (leftEyeCol + 1) * cell}
          y={padding + smileRow * cell}
          width={cell * 2}
          height={cell}
          fill="#000"
          opacity={0.9}
        />
      )}
      
      {expressionType === 1 && (
        // Big grin
        <rect
          x={padding + (leftEyeCol + 0.5) * cell}
          y={padding + smileRow * cell}
          width={cell * 3}
          height={cell}
          fill="#000"
          opacity={0.9}
        />
      )}
      
      {expressionType === 2 && (
        // Smirk
        <rect
          x={padding + (leftEyeCol + 1.5) * cell}
          y={padding + smileRow * cell}
          width={cell * 1.5}
          height={cell * 0.5}
          fill="#000"
          opacity={0.9}
        />
      )}
      
      {expressionType === 3 && (
        // Serious line mouth
        <rect
          x={padding + (leftEyeCol + 1) * cell}
          y={padding + smileRow * cell}
          width={cell * 2}
          height={cell * 0.3}
          fill="#000"
          opacity={0.9}
        />
      )}
      
      {expressionType === 4 && (
        // Surprised O mouth
        <rect
          x={padding + (leftEyeCol + 1.5) * cell}
          y={padding + smileRow * cell}
          width={cell}
          height={cell}
          fill="#000"
          opacity={0.9}
          rx={cell * 0.5}
        />
      )}
      
      {expressionType === 5 && (
        // Sad frown
        <>
          <rect
            x={padding + (leftEyeCol + 1) * cell}
            y={padding + (smileRow + 0.3) * cell}
            width={cell}
            height={cell * 0.4}
            fill="#000"
            opacity={0.9}
          />
          <rect
            x={padding + (leftEyeCol + 2) * cell}
            y={padding + (smileRow + 0.3) * cell}
            width={cell}
            height={cell * 0.4}
            fill="#000"
            opacity={0.9}
          />
        </>
      )}

      {/* Mustache (if character has one) */}
      {hasMustache && (
        <rect
          x={padding + (leftEyeCol + 1) * cell}
          y={padding + (smileRow - 0.5) * cell}
          width={cell * 2}
          height={cell * 0.6}
          fill={theme.shadow}
          opacity={0.8}
        />
      )}

      {/* Beard (if character has one) */}
      {hasBeard && (
        <>
          <rect
            x={padding + (leftEyeCol + 0.5) * cell}
            y={padding + (smileRow + 1) * cell}
            width={cell * 3}
            height={cell * 2}
            fill={theme.shadow}
            opacity={0.7}
          />
          <rect
            x={padding + (leftEyeCol + 1) * cell}
            y={padding + (smileRow + 2) * cell}
            width={cell * 2}
            height={cell}
            fill={theme.shadow}
            opacity={0.7}
          />
        </>
      )}

      {/* Battle scars (for tough characters) */}
      {hasScars && (
        <>
          <rect
            x={padding + (leftEyeCol - 0.5) * cell}
            y={padding + (eyeRow - 1) * cell}
            width={cell * 0.2}
            height={cell * 3}
            fill={theme.shadow}
            opacity={0.8}
          />
          <rect
            x={padding + (rightEyeCol + 1) * cell}
            y={padding + (eyeRow + 0.5) * cell}
            width={cell * 2}
            height={cell * 0.2}
            fill={theme.shadow}
            opacity={0.8}
          />
        </>
      )}

      {/* Rarity-based accessories */}
      {rarityFeatures.hasCrown && (
        <>
          {/* Exotic crown */}
          <rect
            x={padding + 4 * cell}
            y={padding + 1 * cell}
            width={cell * 8}
            height={cell}
            fill={theme.highlight}
          />
          <rect
            x={padding + 5 * cell}
            y={padding + 0 * cell}
            width={cell}
            height={cell}
            fill={theme.accent}
          />
          <rect
            x={padding + 7 * cell}
            y={padding + 0 * cell}
            width={cell}
            height={cell}
            fill={theme.accent}
          />
          <rect
            x={padding + 9 * cell}
            y={padding + 0 * cell}
            width={cell}
            height={cell}
            fill={theme.accent}
          />
        </>
      )}

      {rarityFeatures.hasHat && (
        <>
          {/* Legendary hat */}
          <rect
            x={padding + 3 * cell}
            y={padding + 2 * cell}
            width={cell * 10}
            height={cell * 2}
            fill={theme.accent}
          />
          <rect
            x={padding + 6 * cell}
            y={padding + 1 * cell}
            width={cell * 4}
            height={cell}
            fill={theme.spot}
          />
        </>
      )}

      {/* Element-specific visual effects */}
      {features.hasSnowflakes && (
        <>
          <rect x={padding + 2 * cell} y={padding + 3 * cell} width={cell * 0.5} height={cell * 0.5} fill={theme.highlight} />
          <rect x={padding + 13 * cell} y={padding + 5 * cell} width={cell * 0.5} height={cell * 0.5} fill={theme.highlight} />
          <rect x={padding + 1 * cell} y={padding + 8 * cell} width={cell * 0.5} height={cell * 0.5} fill={theme.highlight} />
          <rect x={padding + 14 * cell} y={padding + 11 * cell} width={cell * 0.5} height={cell * 0.5} fill={theme.highlight} />
        </>
      )}

      {features.hasFlames && (
        <>
          {/* Flame effect on top */}
          <rect x={padding + 6 * cell} y={padding + 0 * cell} width={cell} height={cell} fill={theme.accent} opacity={0.8} />
          <rect x={padding + 8 * cell} y={padding + 0 * cell} width={cell} height={cell} fill={theme.highlight} opacity={0.8} />
          <rect x={padding + 10 * cell} y={padding + 1 * cell} width={cell} height={cell} fill={theme.accent} opacity={0.8} />
        </>
      )}

      {features.hasSparks && (
        <>
          {/* Lightning sparks */}
          <rect x={padding + 1 * cell} y={padding + 4 * cell} width={cell * 0.3} height={cell * 2} fill={theme.highlight} />
          <rect x={padding + 14 * cell} y={padding + 7 * cell} width={cell * 0.3} height={cell * 2} fill={theme.highlight} />
          <rect x={padding + 2 * cell} y={padding + 10 * cell} width={cell * 2} height={cell * 0.3} fill={theme.highlight} />
          <rect x={padding + 12 * cell} y={padding + 12 * cell} width={cell * 2} height={cell * 0.3} fill={theme.highlight} />
        </>
      )}

      {features.hasRays && (
        <>
          {/* Light rays */}
          <rect x={padding + size/2 - cell*0.15} y={padding + 0} width={cell * 0.3} height={cell * 3} fill={theme.highlight} opacity={0.7} />
          <rect x={padding + 0} y={padding + size/2 - cell*0.15} width={cell * 3} height={cell * 0.3} fill={theme.highlight} opacity={0.7} />
          <rect x={padding + size - cell*3} y={padding + size/2 - cell*0.15} width={cell * 3} height={cell * 0.3} fill={theme.highlight} opacity={0.7} />
          <rect x={padding + size/2 - cell*0.15} y={padding + size - cell*3} width={cell * 0.3} height={cell * 3} fill={theme.highlight} opacity={0.7} />
        </>
      )}

      {/* Void shadows - REMOVED FOR CLEAN CARDS */}
    </svg>
  );
};

export default ElementalPixelPotato;