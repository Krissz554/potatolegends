import React from "react";

interface PixelPotatoProps {
  seed: string;
  potatoType: string; // used to style color/theme
  traitHints?: string[];
  size?: number; // px
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

// Map potato types to base colors (HSL) and garnish colors
const typeColors: Record<string, { base: string; spot: string }> = {
  russet: { base: "hsl(30 45% 42%)", spot: "hsl(30 35% 32%)" },
  golden: { base: "hsl(45 90% 60%)", spot: "hsl(35 65% 40%)" },
  "yukon gold": { base: "hsl(48 92% 62%)", spot: "hsl(38 60% 42%)" },
  sweet: { base: "hsl(22 85% 55%)", spot: "hsl(22 65% 40%)" },
  purple: { base: "hsl(275 50% 50%)", spot: "hsl(275 45% 40%)" },
  red: { base: "hsl(0 65% 55%)", spot: "hsl(0 55% 40%)" },
  fingerling: { base: "hsl(32 50% 50%)", spot: "hsl(32 40% 40%)" },
  tot: { base: "hsl(38 90% 62%)", spot: "hsl(32 70% 45%)" },
  waffle: { base: "hsl(36 85% 58%)", spot: "hsl(36 70% 45%)" },
  hashbrown: { base: "hsl(34 80% 55%)", spot: "hsl(34 60% 42%)" },
  // new types
  "maris piper": { base: "hsl(42 70% 58%)", spot: "hsl(35 55% 42%)" },
  desiree: { base: "hsl(0 62% 56%)", spot: "hsl(0 52% 42%)" },
  "king edward": { base: "hsl(38 60% 60%)", spot: "hsl(32 50% 45%)" },
  kennebec: { base: "hsl(36 55% 55%)", spot: "hsl(30 45% 40%)" },
  "red bliss": { base: "hsl(0 65% 55%)", spot: "hsl(0 55% 40%)" },
  "purple majesty": { base: "hsl(275 52% 50%)", spot: "hsl(275 45% 40%)" },
  "blue adirondack": { base: "hsl(220 50% 50%)", spot: "hsl(220 45% 40%)" },
  vitelotte: { base: "hsl(265 50% 46%)", spot: "hsl(265 45% 36%)" },
  "la ratte": { base: "hsl(32 50% 50%)", spot: "hsl(32 40% 40%)" },
  kipfler: { base: "hsl(34 52% 52%)", spot: "hsl(34 42% 40%)" },
  "new potato": { base: "hsl(50 85% 70%)", spot: "hsl(42 60% 50%)" },
  baked: { base: "hsl(30 50% 48%)", spot: "hsl(28 45% 36%)" },
  mashed: { base: "hsl(50 80% 85%)", spot: "hsl(48 65% 70%)" },
  "curly fry": { base: "hsl(38 85% 60%)", spot: "hsl(32 70% 45%)" },
  "crinkle fry": { base: "hsl(36 85% 58%)", spot: "hsl(34 70% 45%)" },
  chips: { base: "hsl(40 85% 62%)", spot: "hsl(35 70% 45%)" },
  "french fry": { base: "hsl(42 86% 60%)", spot: "hsl(36 72% 45%)" },
  gnocchi: { base: "hsl(52 60% 90%)", spot: "hsl(50 50% 75%)" },
  princess: { base: "hsl(320 70% 80%)", spot: "hsl(315 60% 65%)" },
  
  // Vibrant New Potato Colors
  "rainbow potato": { base: "hsl(280 80% 70%)", spot: "hsl(320 90% 60%)" },
  "neon green": { base: "hsl(120 100% 70%)", spot: "hsl(120 80% 50%)" },
  "electric blue": { base: "hsl(210 100% 70%)", spot: "hsl(210 80% 50%)" },
  "sunset orange": { base: "hsl(25 100% 65%)", spot: "hsl(15 90% 45%)" },
  "lime green": { base: "hsl(80 100% 60%)", spot: "hsl(80 80% 40%)" },
  "hot pink": { base: "hsl(330 100% 70%)", spot: "hsl(330 80% 50%)" },
  "cyber purple": { base: "hsl(270 100% 65%)", spot: "hsl(270 80% 45%)" },
  "acid yellow": { base: "hsl(60 100% 70%)", spot: "hsl(60 80% 50%)" },
  "turquoise": { base: "hsl(180 100% 65%)", spot: "hsl(180 80% 45%)" },
  "magenta": { base: "hsl(300 100% 70%)", spot: "hsl(300 80% 50%)" },
  
  // Metallic Potatoes
  "gold chrome": { base: "hsl(45 100% 75%)", spot: "hsl(35 90% 55%)" },
  "silver chrome": { base: "hsl(0 0% 80%)", spot: "hsl(0 0% 60%)" },
  "copper bronze": { base: "hsl(30 80% 60%)", spot: "hsl(20 70% 40%)" },
  "platinum": { base: "hsl(0 0% 85%)", spot: "hsl(0 0% 65%)" },
  "rose gold": { base: "hsl(15 70% 75%)", spot: "hsl(15 60% 55%)" },
  
  // Pastel Potatoes
  "pastel pink": { base: "hsl(350 60% 85%)", spot: "hsl(350 50% 70%)" },
  "pastel blue": { base: "hsl(200 60% 85%)", spot: "hsl(200 50% 70%)" },
  "pastel green": { base: "hsl(120 60% 85%)", spot: "hsl(120 50% 70%)" },
  "pastel purple": { base: "hsl(280 60% 85%)", spot: "hsl(280 50% 70%)" },
  "pastel yellow": { base: "hsl(60 60% 85%)", spot: "hsl(60 50% 70%)" },
  "mint green": { base: "hsl(150 60% 80%)", spot: "hsl(150 50% 65%)" },
  "lavender": { base: "hsl(260 60% 85%)", spot: "hsl(260 50% 70%)" },
  
  // Dark/Gothic Potatoes
  "midnight black": { base: "hsl(0 0% 15%)", spot: "hsl(0 0% 10%)" },
  "deep crimson": { base: "hsl(0 80% 25%)", spot: "hsl(0 70% 15%)" },
  "forest green": { base: "hsl(120 60% 25%)", spot: "hsl(120 50% 15%)" },
  "midnight blue": { base: "hsl(220 80% 25%)", spot: "hsl(220 70% 15%)" },
  "gothic purple": { base: "hsl(280 60% 25%)", spot: "hsl(280 50% 15%)" },
  "charcoal": { base: "hsl(0 0% 25%)", spot: "hsl(0 0% 15%)" },
  
  // Nature-Inspired Potatoes
  "ocean blue": { base: "hsl(200 80% 55%)", spot: "hsl(200 70% 35%)" },
  "grass green": { base: "hsl(100 70% 50%)", spot: "hsl(100 60% 30%)" },
  "sunset pink": { base: "hsl(340 80% 65%)", spot: "hsl(340 70% 45%)" },
  "sky blue": { base: "hsl(190 80% 70%)", spot: "hsl(190 70% 50%)" },
  "coral reef": { base: "hsl(15 90% 65%)", spot: "hsl(15 80% 45%)" },
  "autumn orange": { base: "hsl(35 90% 60%)", spot: "hsl(35 80% 40%)" },
  
  // Cosmic Potatoes
  "galaxy purple": { base: "hsl(260 90% 45%)", spot: "hsl(260 80% 25%)" },
  "nebula pink": { base: "hsl(320 90% 60%)", spot: "hsl(320 80% 40%)" },
  "starlight blue": { base: "hsl(220 90% 65%)", spot: "hsl(220 80% 45%)" },
  "cosmic green": { base: "hsl(140 90% 55%)", spot: "hsl(140 80% 35%)" },
  "alien green": { base: "hsl(90 100% 60%)", spot: "hsl(90 90% 40%)" },
  
  // Food-Inspired Potatoes
  "chocolate": { base: "hsl(25 80% 35%)", spot: "hsl(25 70% 25%)" },
  "vanilla cream": { base: "hsl(50 40% 90%)", spot: "hsl(50 30% 75%)" },
  "strawberry": { base: "hsl(350 90% 65%)", spot: "hsl(350 80% 45%)" },
  "blueberry": { base: "hsl(240 80% 55%)", spot: "hsl(240 70% 35%)" },
  "lemon": { base: "hsl(55 100% 70%)", spot: "hsl(55 90% 50%)" },
  "lime": { base: "hsl(75 100% 60%)", spot: "hsl(75 90% 40%)" },
  "grape": { base: "hsl(270 80% 50%)", spot: "hsl(270 70% 30%)" },
  "peach": { base: "hsl(25 90% 75%)", spot: "hsl(25 80% 55%)" },
  "cherry": { base: "hsl(0 90% 60%)", spot: "hsl(0 80% 40%)" },
  
  // Gemstone Potatoes
  "emerald": { base: "hsl(140 80% 45%)", spot: "hsl(140 70% 25%)" },
  "ruby": { base: "hsl(0 90% 50%)", spot: "hsl(0 80% 30%)" },
  "sapphire": { base: "hsl(220 90% 50%)", spot: "hsl(220 80% 30%)" },
  "diamond": { base: "hsl(0 0% 95%)", spot: "hsl(0 0% 80%)" },
  "amethyst": { base: "hsl(280 80% 60%)", spot: "hsl(280 70% 40%)" },
  "topaz": { base: "hsl(45 90% 65%)", spot: "hsl(45 80% 45%)" },
  "opal": { base: "hsl(200 60% 80%)", spot: "hsl(200 50% 65%)" },
};

// Smart color helper function to provide contrasting colors for accessories
function getAccessoryColors(potatoType: string) {
  const type = (potatoType || '').toLowerCase();
  
  // Red potatoes get cool colors
  if (type.includes('red') || type === 'desiree') {
    return {
      primary: 'hsl(210, 80%, 60%)', // Blue
      accent: 'hsl(120, 60%, 45%)',  // Green
      dark: 'hsl(240, 70%, 30%)',    // Dark blue
    };
  }
  // Purple potatoes get warm colors
  else if (type.includes('purple') || type === 'vitelotte') {
    return {
      primary: 'hsl(25, 85%, 55%)',  // Orange
      accent: 'hsl(45, 90%, 60%)',   // Yellow
      dark: 'hsl(30, 70%, 30%)',     // Brown
    };
  }
  // Blue potatoes get warm colors
  else if (type.includes('blue')) {
    return {
      primary: 'hsl(15, 85%, 55%)',  // Red-orange
      accent: 'hsl(45, 90%, 60%)',   // Yellow
      dark: 'hsl(30, 70%, 30%)',     // Brown
    };
  }
  // Golden/yellow potatoes get cool colors
  else if (type.includes('golden') || type.includes('yellow') || type === 'yukon gold' || 
           type === 'tot' || type === 'waffle' || type === 'new potato' || type === 'mashed') {
    return {
      primary: 'hsl(220, 70%, 50%)', // Blue
      accent: 'hsl(280, 60%, 50%)',  // Purple
      dark: 'hsl(200, 80%, 30%)',    // Dark blue
    };
  }
  // Princess gets complementary colors
  else if (type === 'princess') {
    return {
      primary: 'hsl(120, 60%, 45%)', // Green
      accent: 'hsl(200, 70%, 50%)',  // Blue
      dark: 'hsl(240, 70%, 30%)',    // Dark blue
    };
  }
  // Default for brown/neutral potatoes
  else {
    return {
      primary: 'hsl(220, 70%, 55%)', // Blue
      accent: 'hsl(280, 65%, 55%)',  // Purple
      dark: 'hsl(200, 80%, 30%)',    // Dark blue
    };
  }
}

// 16x16 potato silhouette using 'x' as filled squares
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

const PixelPotato: React.FC<PixelPotatoProps> = ({ seed, potatoType, traitHints = [], size = 256 }) => {
  const h = hashString((seed || '') + (potatoType || '') + (traitHints || []).join("|"));
  const rand = mulberry32(h);
  const cell = Math.floor(size / 16);
  const padding = Math.floor((size - cell * 16) / 2);

  const typeKey = (potatoType || '').toLowerCase();
  const theme = typeColors[typeKey] || typeColors["golden"];

  // Eyes positions (randomized a bit)
  const eyeRow = 6 + Math.floor(rand() * 2); // rows 6-7
  const leftEyeCol = 5 + Math.floor(rand() * 2); // 5-6
  const rightEyeCol = 9 + Math.floor(rand() * 2); // 9-10

  // Smile arch variation
  const smileRow = eyeRow + 3;

  // Random freckles/spots
  const spotCount = 4 + Math.floor(rand() * 5);
  const spots = Array.from({ length: spotCount }).map(() => ({
    r: 2 + Math.floor(rand() * 12),
    c: 2 + Math.floor(rand() * 12),
  }));

  // Accessory decisions based on traits + a bit of seeded randomness
  const t = (traitHints || []).join(" ").toLowerCase();
  const isPrincess = potatoType === "princess";
  
  // Primary accessories (mutually exclusive with umbrella)
  const withUmbrella = !isPrincess && /umbrella|rain|rainy|shower|drizzle/.test(t);
  const withMic = !withUmbrella && !isPrincess && /karaoke|sing|concert|microphone|\bmic\b/.test(t);
  
  // Face accessories
  const withGlasses = !isPrincess && /nerd|smart|glasses|reading|crosswords|coding/.test(t);
  const withSunglasses = !isPrincess && /sunglasses|cool|sunny|beach|summer|\bsun\b|aviator|designer/.test(t);
  const withMonocle = !isPrincess && /monocle|detective|magnifying|truth|probability/.test(t);
  
  // Head accessories
  const withBeret = !isPrincess && /beret|painting|bohemian|artistic|french/.test(t);
  const withFedora = !isPrincess && /fedora|fancy|detective|noir|classic/.test(t);
  const withBaseballCap = !isPrincess && /baseball|cap|backwards|sports/.test(t);
  const withBeanie = !isPrincess && /beanie|winter|warm|cozy/.test(t);
  const withCowboyHat = !isPrincess && /cowboy|western|ranch|boots/.test(t);
  const withTopHat = !isPrincess && /top hat|formal|gentleman|magician|cane/.test(t);
  const withGraduationCap = !isPrincess && /graduation|cap|tassel|student|scholar/.test(t);
  const withSailorCap = !isPrincess && /sailor|nautical|navy|adventure/.test(t);
  const withChefHat = !isPrincess && /chef|cooking|kitchen/.test(t);
  const withWizardHat = !isPrincess && /wizard|magical|mystical|sage/.test(t);
  const withConstructionHat = !isPrincess && /construction|hard hat|work|safety/.test(t);
  const withSpaceHelmet = !isPrincess && /space|helmet|astronaut|stargazing/.test(t);
  
  // Neck accessories
  const withBowTie = !isPrincess && /bow tie|formal|tuxedo|dinner|diplomatic/.test(t);
  const withScarf = !isPrincess && /scarf|winter|cozy|warm|eternal|reality/.test(t);
  const withNecklace = !isPrincess && /necklace|pearl|jewel|holy|neutron star/.test(t);
  const withTie = !isPrincess && /tie|polka|business|professional/.test(t);
  
  // Body accessories
  const withSuspenders = !isPrincess && /suspenders|vintage|support|universal/.test(t);
  const withCape = !isPrincess && /cape|heroic|wind|dimensional|singularity/.test(t);
  
  // Hand accessories
  const withGloves = !isPrincess && /gloves|antimatter|sacred|primordial/.test(t);
  
  // Hair accessories
  const withHeadband = !isPrincess && /headband|vintage|bohemian|festival|wisdom/.test(t);
  const withFlowerCrown = !isPrincess && /flower|crown|spring|nature/.test(t);
  const withHairPins = !isPrincess && /hair pins|elegant|kimono|traditional/.test(t);
  
  // Special accessories
  const withBandana = !isPrincess && /bandana|pirate|western/.test(t);
  const withLabCoat = !isPrincess && /lab coat|scientist|safety|research/.test(t);
  const withMask = !isPrincess && /mask|phases|flavor|ethereal/.test(t);

  // Occasional random accessories (deterministic per seed)
  const randomHat = rand() < 0.18;
  const randomShades = rand() < 0.15;
  const randomBowTie = rand() < 0.12;
  const randomScarf = rand() < 0.10;

  const canHaveExtras = !withUmbrella && !isPrincess;
  
  // Head accessories (only one at a time)
  const showCrown = isPrincess;
  const showBeret = canHaveExtras && withBeret && !showCrown;
  const showFedora = canHaveExtras && withFedora && !showCrown && !showBeret;
  const showBaseballCap = canHaveExtras && withBaseballCap && !showCrown && !showBeret && !showFedora;
  const showBeanie = canHaveExtras && withBeanie && !showCrown && !showBeret && !showFedora && !showBaseballCap;
  const showCowboyHat = canHaveExtras && withCowboyHat && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie;
  const showTopHat = canHaveExtras && withTopHat && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat;
  const showGraduationCap = canHaveExtras && withGraduationCap && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat;
  const showSailorCap = canHaveExtras && withSailorCap && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap;
  const showChefHat = canHaveExtras && withChefHat && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap;
  const showWizardHat = canHaveExtras && withWizardHat && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat;
  const showConstructionHat = canHaveExtras && withConstructionHat && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat && !showWizardHat;
  const showSpaceHelmet = canHaveExtras && withSpaceHelmet && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat && !showWizardHat && !showConstructionHat;
  
  const showHat = canHaveExtras && (/wizard|chef|pirate|captain|party/.test(t) || randomHat) && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat && !showWizardHat && !showConstructionHat && !showSpaceHelmet;
  
  // Face accessories (can combine some)
  const showSunglasses = canHaveExtras && (withSunglasses || randomShades);
  const showGlasses = canHaveExtras && withGlasses && !showSunglasses;
  const showMonocle = canHaveExtras && withMonocle && !showGlasses && !showSunglasses;
  
  // Neck accessories
  const showBowTie = canHaveExtras && (withBowTie || randomBowTie);
  const showScarf = canHaveExtras && (withScarf || randomScarf) && !showBowTie;
  const showNecklace = canHaveExtras && withNecklace && !showBowTie && !showScarf;
  const showTie = canHaveExtras && withTie && !showBowTie && !showScarf && !showNecklace;
  
  // Other accessories
  const showSuspenders = canHaveExtras && withSuspenders;
  const showCape = canHaveExtras && withCape;
  const showGloves = canHaveExtras && withGloves;
  const showHeadband = canHaveExtras && withHeadband && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat && !showWizardHat && !showConstructionHat && !showSpaceHelmet && !showHat;
  const showFlowerCrown = canHaveExtras && withFlowerCrown && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat && !showWizardHat && !showConstructionHat && !showSpaceHelmet && !showHat && !showHeadband;
  const showHairPins = canHaveExtras && withHairPins && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat && !showWizardHat && !showConstructionHat && !showSpaceHelmet && !showHat && !showHeadband && !showFlowerCrown;
  const showBandana = canHaveExtras && withBandana && !showCrown && !showBeret && !showFedora && !showBaseballCap && !showBeanie && !showCowboyHat && !showTopHat && !showGraduationCap && !showSailorCap && !showChefHat && !showWizardHat && !showConstructionHat && !showSpaceHelmet && !showHat && !showHeadband && !showFlowerCrown && !showHairPins;
  const showLabCoat = canHaveExtras && withLabCoat;
  const showMask = canHaveExtras && withMask;
  return (
    <svg
      width={size}
      height={size}
      viewBox={`0 0 ${size} ${size}`}
      role="img"
      aria-label={`${potatoType} pixel-art potato`}
      style={{
        imageRendering: "pixelated",
        shapeRendering: "crispEdges",
        filter: "drop-shadow(var(--shadow-elegant))",
      }}
    >
      {/* Background subtle circle */}
      <circle
        cx={size / 2}
        cy={size / 2}
        r={size / 2 - 8}
        fill="hsl(var(--secondary))"
      />

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

      {/* Random darker spots */}
      {spots.map((s, i) => (
        <rect
          key={`spot-${i}`}
          x={padding + s.c * cell}
          y={padding + s.r * cell}
          width={cell}
          height={cell}
          fill={theme.spot}
          opacity={0.65}
        />
      ))}

      {/* Eyes */}
      <rect
        x={padding + leftEyeCol * cell}
        y={padding + eyeRow * cell}
        width={cell}
        height={cell}
        fill="hsl(var(--foreground))"
      />
      <rect
        x={padding + rightEyeCol * cell}
        y={padding + eyeRow * cell}
        width={cell}
        height={cell}
        fill="hsl(var(--foreground))"
      />

      {/* Smile */}
      {isPrincess ? (
        // Special princess smile
        <>
          <rect
            x={padding + (leftEyeCol + 1) * cell}
            y={padding + smileRow * cell}
            width={cell}
            height={cell}
            fill="hsl(var(--foreground))"
          />
          <rect
            x={padding + (leftEyeCol + 2) * cell}
            y={padding + (smileRow + 1) * cell}
            width={cell}
            height={cell}
            fill="hsl(var(--foreground))"
          />
          <rect
            x={padding + (leftEyeCol + 3) * cell}
            y={padding + smileRow * cell}
            width={cell}
            height={cell}
            fill="hsl(var(--foreground))"
          />
        </>
      ) : (
        // Regular smile
        <rect
          x={padding + (leftEyeCol + 1) * cell}
          y={padding + smileRow * cell}
          width={cell * 2}
          height={cell}
          fill="hsl(var(--foreground))"
          opacity={0.9}
        />
      )}

      {/* Accessories */}
      {/* Monocle - Authentic Pixel Art Style */}


      {(showSunglasses || showGlasses) && (
        <>
          {showSunglasses ? (
            <>
              {/* Sunglasses (filled lenses) */}
              <rect
                x={padding + (leftEyeCol - 1) * cell}
                y={padding + (eyeRow - 1) * cell}
                width={cell * 3}
                height={cell * 3}
                fill="hsl(var(--foreground))"
                rx={2}
                ry={2}
                opacity={0.95}
              />
              <rect
                x={padding + (rightEyeCol - 1) * cell}
                y={padding + (eyeRow - 1) * cell}
                width={cell * 3}
                height={cell * 3}
                fill="hsl(var(--foreground))"
                rx={2}
                ry={2}
                opacity={0.95}
              />
              {/* Bridge */}
              <rect
                x={padding + (leftEyeCol + 2) * cell}
                y={padding + eyeRow * cell}
                width={cell}
                height={1}
                fill="hsl(var(--foreground))"
              />
            </>
          ) : (
            <>
              {/* Regular glasses (outlined) */}
              <rect
                x={padding + (leftEyeCol - 1) * cell}
                y={padding + (eyeRow - 1) * cell}
                width={cell * 3}
                height={cell * 3}
                fill="transparent"
                stroke="hsl(var(--foreground))"
                strokeWidth={2}
              />
              <rect
                x={padding + (rightEyeCol - 1) * cell}
                y={padding + (eyeRow - 1) * cell}
                width={cell * 3}
                height={cell * 3}
                fill="transparent"
                stroke="hsl(var(--foreground))"
                strokeWidth={2}
              />
              <rect
                x={padding + (leftEyeCol + 2) * cell}
                y={padding + eyeRow * cell}
                width={cell}
                height={1}
                fill="hsl(var(--foreground))"
              />
            </>
          )}
        </>
      )}

      {withMic && (
        <>
          {/* Simple mic on the right side */}
          <rect
            x={padding + (rightEyeCol + 4) * cell}
            y={padding + (eyeRow + 2) * cell}
            width={cell}
            height={cell * 3}
            fill="hsl(var(--foreground))"
          />
          <rect
            x={padding + (rightEyeCol + 3) * cell}
            y={padding + (eyeRow + 1) * cell}
            width={cell * 3}
            height={cell}
            fill="hsl(var(--accent))"
          />
        </>
      )}



      {showHat && (
        <>
          {/* Hair instead of hat (pixel fringe) */}
          {/* Top hair band */}
          <rect
            x={padding + 3 * cell}
            y={padding + 2 * cell}
            width={cell * 10}
            height={cell * 2}
            fill="hsl(var(--foreground))"
            opacity={0.95}
          />
          {/* Bangs */}
          <rect x={padding + 4 * cell} y={padding + 4 * cell} width={cell * 2} height={cell} fill="hsl(var(--foreground))" opacity={0.95} />
          <rect x={padding + 8 * cell} y={padding + 4 * cell} width={cell * 3} height={cell} fill="hsl(var(--foreground))" opacity={0.95} />
          <rect x={padding + 12 * cell} y={padding + 4 * cell} width={cell * 2} height={cell} fill="hsl(var(--foreground))" opacity={0.95} />
          {/* Sideburns */}
          <rect x={padding + 3 * cell} y={padding + 3 * cell} width={cell} height={cell * 3} fill="hsl(var(--foreground))" opacity={0.95} />
          <rect x={padding + 12 * cell} y={padding + 3 * cell} width={cell} height={cell * 3} fill="hsl(var(--foreground))" opacity={0.95} />
        </>
      )}

      {/* Crown for princess */}
      {showCrown && (
        <>
          {/* Crown base */}
          <rect
            x={padding + 4 * cell}
            y={padding + 1 * cell}
            width={cell * 8}
            height={cell}
            fill="gold"
          />
          <rect
            x={padding + 3 * cell}
            y={padding + 2 * cell}
            width={cell * 10}
            height={cell}
            fill="gold"
          />
          {/* Crown points */}
          <rect
            x={padding + 5 * cell}
            y={padding + 0 * cell}
            width={cell}
            height={cell}
            fill="gold"
          />
          <rect
            x={padding + 7 * cell}
            y={padding + 0 * cell}
            width={cell}
            height={cell}
            fill="gold"
          />
          <rect
            x={padding + 9 * cell}
            y={padding + 0 * cell}
            width={cell}
            height={cell}
            fill="gold"
          />
          {/* Crown jewels */}
          <rect
            x={padding + 6 * cell}
            y={padding + 1 * cell}
            width={cell}
            height={cell}
            fill="red"
          />
          <rect
            x={padding + 8 * cell}
            y={padding + 1 * cell}
            width={cell}
            height={cell}
            fill="blue"
          />
        </>
      )}













      {/* Headband */}
      {showHeadband && (
        <>
          <rect
            x={padding + 3 * cell}
            y={padding + 3 * cell}
            width={cell * 10}
            height={cell}
            fill="hsl(var(--accent))"
            rx={cell * 0.5}
          />
          {/* Headband pattern */}
          <rect
            x={padding + 6 * cell}
            y={padding + 3.2 * cell}
            width={cell * 0.5}
            height={cell * 0.6}
            fill="hsl(var(--primary))"
          />
          <rect
            x={padding + 8 * cell}
            y={padding + 3.2 * cell}
            width={cell * 0.5}
            height={cell * 0.6}
            fill="hsl(var(--primary))"
          />
          <rect
            x={padding + 10 * cell}
            y={padding + 3.2 * cell}
            width={cell * 0.5}
            height={cell * 0.6}
            fill="hsl(var(--primary))"
          />
        </>
      )}

      {/* Flower Crown - Authentic Pixel Art Style */}
      {showFlowerCrown && (
        <>
          {/* Simple green vine base */}
          <rect
            x={padding + 3 * cell}
            y={padding + 3 * cell}
            width={cell * 10}
            height={cell}
            fill="#228B22"
          />
          {/* Simple pixel flowers */}
          <rect x={padding + 5 * cell} y={padding + 2 * cell} width={cell} height={cell} fill="#FF69B4" />
          <rect x={padding + 7 * cell} y={padding + 1.5 * cell} width={cell} height={cell} fill="#FFD700" />
          <rect x={padding + 9 * cell} y={padding + 2 * cell} width={cell} height={cell} fill="#9370DB" />
          <rect x={padding + 11 * cell} y={padding + 1.5 * cell} width={cell} height={cell} fill="#FF8C00" />
          
          {/* Simple flower highlights */}
          <rect x={padding + 5 * cell} y={padding + 2 * cell} width={cell * 0.5} height={cell * 0.5} fill="white" opacity={0.5} />
          <rect x={padding + 7 * cell} y={padding + 1.5 * cell} width={cell * 0.5} height={cell * 0.5} fill="white" opacity={0.5} />
          <rect x={padding + 9 * cell} y={padding + 2 * cell} width={cell * 0.5} height={cell * 0.5} fill="white" opacity={0.5} />
          <rect x={padding + 11 * cell} y={padding + 1.5 * cell} width={cell * 0.5} height={cell * 0.5} fill="white" opacity={0.5} />
        </>
      )}

      {/* Bandana */}



      {withUmbrella && (
        <>
          {/* Umbrella canopy above left side */}
          <path
            d={`M ${padding + (leftEyeCol - 2) * cell} ${padding + (eyeRow - 4) * cell}
               Q ${size / 2} ${padding + (eyeRow - 7) * cell} ${padding + (rightEyeCol + 4) * cell} ${padding + (eyeRow - 4) * cell}
               L ${padding + (rightEyeCol + 4) * cell} ${padding + (eyeRow - 3) * cell}
               L ${padding + (leftEyeCol - 2) * cell} ${padding + (eyeRow - 3) * cell} Z`}
            fill="hsl(var(--accent))"
            opacity={0.9}
          />
          {/* Umbrella shaft */}
          <rect
            x={padding + Math.round((leftEyeCol + rightEyeCol) / 2) * cell}
            y={padding + (eyeRow - 3) * cell}
            width={Math.max(1, Math.floor(cell / 3))}
            height={cell * 6}
            fill="hsl(var(--foreground))"
          />
          {/* Handle */}
          <path
            d={`M ${padding + Math.round((leftEyeCol + rightEyeCol) / 2) * cell} ${padding + (eyeRow + 3) * cell}
               q ${cell} ${cell} 0 ${cell * 2}`}
            fill="none"
            stroke="hsl(var(--foreground))"
            strokeWidth={2}
          />
        </>
      )}



    </svg>
  );
};

export default PixelPotato;
