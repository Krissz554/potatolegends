// Rarity system
export type Rarity = 'common' | 'uncommon' | 'rare' | 'legendary' | 'exotic';

export const RARITY_WEIGHTS = {
  common: 50,
  uncommon: 30,
  rare: 15,
  legendary: 4,
  exotic: 1
};

export const RARITY_COLORS = {
  common: 'text-gray-600',
  uncommon: 'text-green-600',
  rare: 'text-blue-600',
  legendary: 'text-purple-600',
  exotic: 'text-orange-600'
};

// Stat ranges by rarity for trading card battle system
export const STAT_RANGES = {
  common: { hp: { min: 50, max: 100 }, attack: { min: 10, max: 25 } },
  uncommon: { hp: { min: 105, max: 120 }, attack: { min: 26, max: 30 } },
  rare: { hp: { min: 125, max: 150 }, attack: { min: 31, max: 38 } },
  legendary: { hp: { min: 155, max: 200 }, attack: { min: 39, max: 50 } },
  exotic: { hp: { min: 205, max: 300 }, attack: { min: 51, max: 75 } }
};

export const ADJECTIVES_BY_RARITY = {
  common: [
    "crunchy", "fluffy", "cozy", "earthy", "buttery", "rustic", "smooth", "tender", "warm", "crispy"
  ],
  uncommon: [
    "chaotic", "sparkly", "mischievous", "zesty", "whimsical", "sassy", "velvety", "silky", "peppery", "sun-kissed", "bouncy", "fizzy"
  ],
  rare: [
    "noble", "melodramatic", "adventurous", "philosophical", "glamorous", "mysterious", "radiant", "caramelized", "golden-hearted", "enchanted", "mystical", "luminous"
  ],
  legendary: [
    "cosmic", "legendary", "transcendent", "otherworldly", "divine", "celestial", "ethereal", "ancient", "majestic", "timeless"
  ],
  exotic: [
    "interdimensional", "quantum", "crystalline", "prismatic", "aurora-blessed", "stardust-infused", "time-warped", "reality-bending", "galactic", "void-touched"
  ]
};

export const POTATO_TYPES_BY_RARITY = {
  common: [
    "russet", "golden", "sweet", "red", "tot", "baked", "mashed", "chips", "french fry", "hash brown"
  ],
  uncommon: [
    "yukon gold", "fingerling", "waffle", "hashbrown", "maris piper", "desiree", "king edward", "kennebec", "new potato", "curly fry", "crinkle fry", "gnocchi", "roasted", "steamed"
  ],
  rare: [
    "purple", "red bliss", "purple majesty", "blue adirondack", "vitelotte", "la ratte", "kipfler", "duchess", "hasselback", "fondant", "gratin", "scalloped"
  ],
  legendary: [
    "rainbow", "diamond", "platinum", "moonstone", "sapphire", "emerald", "crystal", "nebula", "aurora", "phoenix"
  ],
  exotic: [
    "void", "antimatter", "plasma", "quantum", "tesseract", "singularity", "hyperdimensional", "cosmic dust", "neutron star", "black hole", "princess"
  ]
};

export const TRAITS_BY_RARITY = {
  common: [
    "who loves karaoke", "who fears spoons (a little)", "who dreams of becoming fries", "who naps in ovens (but only when they're off)", "who only travels by rolling", "who speedruns peeling", "who bakes sourdough on weekends", "who knits tiny scarves", "who packs extra napkins", "who hums elevator bops", "who wears a cozy scarf in winter", "who sports a stylish bow tie to dinner", "who never leaves home without sunglasses", "who collects vintage headbands", "who always wears a lucky hat", "who rocks a monocle for reading", "who loves wearing berets on Tuesdays", "who has a collection of tiny bandanas", "who wears reading glasses for crosswords", "who sports a fedora when feeling fancy", "who loves wearing suspenders", "who collects polka-dot ties", "who wears a chef's hat while cooking", "who sports a baseball cap backwards", "who loves wearing pearl necklaces", "who always has a flower in their hair", "who wears cowboy boots everywhere", "who loves sporting rainbow socks", "who collects vintage brooches", "who wears a warm winter beanie"
  ],
  uncommon: [
    "who organizes midnight snack heists", "who writes haikus about butter", "who always brings a tiny umbrella", "who moonlights as a salsa critic", "who collects stickers of other potatoes", "who believes in ketchup astrology", "who hosts a couch podcast", "who tells knock-knock jokes in starch", "who prefers dramatic entrances", "who brews artisanal gravy", "who teaches yoga for tubers", "who writes fan mail to farmers", "who DJs at farmers' markets", "who collects rare salts", "who is a midnight fridge philosopher", "who paints in ketchup", "who does stand-up about starch", "who is a selfie connoisseur", "who composts compliments", "who runs a tiny book club", "who is training for couch marathons", "who forecasts gravy weather", "who prefers dramatic seasoning", "who solves mysteries with a spork", "who wears designer sunglasses indoors", "who sports a tuxedo bow tie for formal occasions", "who carries a vintage umbrella even on sunny days", "who wears a detective's magnifying monocle", "who sports a pirate bandana with pride", "who wears nerd glasses while coding", "who rocks a bohemian headband at festivals", "who dons a wizard hat for magical moments", "who wears a stylish beret while painting", "who sports a sailor's cap on adventures", "who wears a flower crown in spring", "who rocks aviator sunglasses like a pilot", "who wears a lab coat and safety goggles", "who sports a cowboy hat and bandana combo", "who wears a graduation cap with tassel", "who rocks a punk leather jacket and spikes", "who wears a kimono with elegant hair pins", "who sports a top hat and cane ensemble", "who wears a space helmet for stargazing", "who rocks a construction hard hat at work"
  ],
  rare: [
    "who trains for the Mashed Olympics", "who speaks fluent carbohydrate", "who conducts symphony orchestras with forks", "who writes poetry in steam", "who runs underground spice clubs", "who mentors baby vegetables", "who time travels through taste buds", "who architects flavor bridges", "who translates for confused condiments", "who choreographs kitchen dances", "who wears ancient crown jewels from lost potato kingdoms", "who sports enchanted sunglasses that see through time", "who dons a mystical cloak woven from starlight", "who wears armor forged from crystallized flavor", "who rocks a ceremonial headdress of cosmic herbs", "who sports legendary boots that walk on clouds", "who wears a cape that billows with dimensional winds", "who dons spectacles that reveal hidden truths", "who sports a collar made of compressed moonbeams", "who wears gauntlets infused with primordial spice", "who rocks a tiara crafted from crystallized dreams", "who sports wings made of golden french fry magic", "who wears a medallion containing bottled thunder", "who dons robes that shift through all flavors of reality", "who sports a mask that phases between dimensions"
  ],
  legendary: [
    "who commands armies of utensils", "who forged the first golden spoon", "who discovered the secret of eternal crispiness", "who founded the University of Umami", "who tamed the legendary Salt Dragon", "who holds the ancient recipe of flavor", "who built bridges between dimensions using starch", "who whispers secrets to the moon about perfect seasoning", "who once arm-wrestled a mountain for the perfect growing soil", "who wears the Crown of Infinite Seasons atop their mighty head", "who sports the Legendary Sunglasses of Solar Fusion", "who dons the Mythical Scarf of Eternal Warmth", "who wears the Fabled Bow Tie of Diplomatic Immunity", "who rocks the Epic Monocle of Truth Seeing", "who sports the Heroic Cape of Wind Mastery", "who wears the Divine Headband of Infinite Wisdom", "who dons the Sacred Gloves of Flavor Channeling", "who rocks the Blessed Boots of Dimensional Walking", "who sports the Holy Necklace of Taste Transcendence", "who wears the Celestial Glasses of Reality Perception", "who dons the Cosmic Hat of Interdimensional Authority", "who rocks the Astral Umbrella of Weather Dominion", "who sports the Ethereal Mask of Flavor Incarnation", "who wears the Primordial Suspenders of Universal Support"
  ],
  exotic: [
    "who transcends the physical realm through pure flavor", "who exists in seventeen dimensions simultaneously", "who rewrote the laws of physics using quantum seasoning", "who once high-fived the Big Bang", "who taught black holes how to properly season themselves", "who knits reality together with cosmic butter", "who solved the universal equation of taste", "who farts galaxies into existence", "who uses supernovas as night lights", "who plays poker with abstract concepts every Tuesday", "who farts gracefully while ruling potato kingdoms", "who commands armies of tater tots with royal elegance", "who wears the Infinite Crown that exists in all realities simultaneously", "who sports the Quantum Monocle of Probability Sight", "who dons the Hyperdimensional Scarf of Reality Weaving", "who wears the Cosmic Bow Tie of Universal Harmony", "who rocks the Void Sunglasses that see beyond existence", "who sports the Omniversal Headband of Thought Transcendence", "who wears the Antimatter Gloves of Creation Destruction", "who dons the Singularity Cape of Spacetime Mastery", "who rocks the Tesseract Boots that walk through dimensions", "who sports the Neutron Star Necklace of Infinite Mass", "who wears the Plasma Glasses of Pure Energy Vision", "who dons the Black Hole Hat that consumes lesser accessories", "who rocks the Stardust Umbrella of Cosmic Protection", "who sports the Galaxy Suspenders that hold up reality itself"
  ]
};

// Playful color palettes (mapped to CSS variables in index.css)
export const PALETTES = {
  blue: { primary: 'var(--palette-blue-primary)', accent: 'var(--palette-blue-accent)' },
  green: { primary: 'var(--palette-green-primary)', accent: 'var(--palette-green-accent)' },
  teal: { primary: 'var(--palette-teal-primary)', accent: 'var(--palette-teal-accent)' },
  lime: { primary: 'var(--palette-lime-primary)', accent: 'var(--palette-lime-accent)' },
  purple: { primary: 'var(--palette-purple-primary)', accent: 'var(--palette-purple-accent)' },
  pink: { primary: 'var(--palette-pink-primary)', accent: 'var(--palette-pink-accent)' },
  orange: { primary: 'var(--palette-orange-primary)', accent: 'var(--palette-orange-accent)' },
  red: { primary: 'var(--palette-red-primary)', accent: 'var(--palette-red-accent)' },
  cyan: { primary: 'var(--palette-cyan-primary)', accent: 'var(--palette-cyan-accent)' },
  amber: { primary: 'var(--palette-amber-primary)', accent: 'var(--palette-amber-accent)' },
  emerald: { primary: 'var(--palette-emerald-primary)', accent: 'var(--palette-emerald-accent)' },
  indigo: { primary: 'var(--palette-indigo-primary)', accent: 'var(--palette-indigo-accent)' },
  rose: { primary: 'var(--palette-rose-primary)', accent: 'var(--palette-rose-accent)' },
  violet: { primary: 'var(--palette-violet-primary)', accent: 'var(--palette-violet-accent)' },
  gold: { primary: 'var(--palette-gold-primary)', accent: 'var(--palette-gold-accent)' },
  rainbow: { primary: 'var(--palette-rainbow-primary)', accent: 'var(--palette-rainbow-accent)' },
} as const;

export type PaletteName = keyof typeof PALETTES;

// Map potato types to a harmonious palette
export const TYPE_TO_PALETTE: Record<string, PaletteName> = {
  // Common
  russet: 'orange', golden: 'orange', sweet: 'orange', red: 'red', tot: 'orange', baked: 'orange', mashed: 'orange', chips: 'orange', 'french fry': 'orange', 'hash brown': 'orange',
  // Uncommon  
  'yukon gold': 'amber', fingerling: 'lime', waffle: 'amber', hashbrown: 'orange', 'maris piper': 'orange', desiree: 'red', 'king edward': 'orange', kennebec: 'orange', 'new potato': 'lime', 'curly fry': 'orange', 'crinkle fry': 'orange', gnocchi: 'teal', roasted: 'amber', steamed: 'green',
  // Rare
  purple: 'purple', 'red bliss': 'red', 'purple majesty': 'purple', 'blue adirondack': 'blue', vitelotte: 'purple', 'la ratte': 'lime', kipfler: 'lime', duchess: 'emerald', hasselback: 'amber', fondant: 'rose', gratin: 'amber', scalloped: 'teal',
  // Legendary
  rainbow: 'rainbow', diamond: 'cyan', platinum: 'cyan', moonstone: 'violet', sapphire: 'blue', emerald: 'emerald', crystal: 'cyan', nebula: 'indigo', aurora: 'violet', phoenix: 'rose',
  // Exotic
  void: 'purple', antimatter: 'violet', plasma: 'cyan', quantum: 'rainbow', tesseract: 'indigo', singularity: 'purple', hyperdimensional: 'rainbow', 'cosmic dust': 'gold', 'neutron star': 'cyan', 'black hole': 'purple', princess: 'pink',
};

// Utility functions for deterministic stat generation
function hashString(str: string): number {
  let hash = 0;
  if (str.length === 0) return hash;
  for (let i = 0; i < str.length; i++) {
    const char = str.charCodeAt(i);
    hash = ((hash << 5) - hash) + char;
    hash = hash & hash; // Convert to 32bit integer
  }
  return Math.abs(hash);
}

function mulberry32(a: number) {
  return function() {
    let t = a += 0x6D2B79F5;
    t = Math.imul(t ^ t >>> 15, t | 1);
    t ^= t + Math.imul(t ^ t >>> 7, t | 61);
    return ((t ^ t >>> 14) >>> 0) / 4294967296;
  };
}

// Generate deterministic stats based on potato seed and rarity
export function generatePotatoStats(seed: string, rarity: Rarity): { hp: number; attack: number } {
  const statRanges = STAT_RANGES[rarity];
  const hash = hashString(seed + '_stats');
  const rng = mulberry32(hash);
  
  // Generate HP within rarity range
  const hpRange = statRanges.hp.max - statRanges.hp.min;
  const hp = statRanges.hp.min + Math.floor(rng() * (hpRange + 1));
  
  // Generate Attack within rarity range  
  const attackRange = statRanges.attack.max - statRanges.attack.min;
  const attack = statRanges.attack.min + Math.floor(rng() * (attackRange + 1));
  
  return { hp, attack };
}