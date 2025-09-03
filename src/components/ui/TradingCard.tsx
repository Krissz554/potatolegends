import React, { useState, useRef } from 'react';
import { Button } from '@/components/ui/button';
import { Download, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import html2canvas from 'html2canvas';
import ElementalPixelPotato from '../ElementalPixelPotato';

// Import card background images
import fireCard from '@/assets/pixel/cards/fire-card.png';
import iceCard from '@/assets/pixel/cards/ice-card.png';
import lightCard from '@/assets/pixel/cards/light-card.png';
import lightningCard from '@/assets/pixel/cards/lightning-card.png';
import voidCard from '@/assets/pixel/cards/void-card.png';
import exoticClassCard from '@/assets/pixel/cards/exotic-class-card.png';

// Import custom pixel icons
import hpIcon from '@/assets/pixel/icons/HP-icon.png';
import manaCrystalIcon from '@/assets/pixel/icons/mana-crystal-icon.png';
import atkIcon from '@/assets/pixel/icons/ATK-icon.png';

// Import custom rarity badges
import commonRarity from '@/assets/pixel/cards/common-rarity.png';
import uncommonRarity from '@/assets/pixel/cards/uncommon-rarity.png';
import rareRarity from '@/assets/pixel/cards/rare-rarity.png';
import legendaryRarity from '@/assets/pixel/cards/legendary-rarity.png';
import exoticRarity from '@/assets/pixel/cards/exotic-rarity.png';

// Import custom potato assets
import potato0001 from '@/assets/pixel-potatoes/potato_0001/potato_0001.png';
import potato0001DeploySprite from '@/assets/pixel-potatoes/potato_0001/potato_0001-deploy-sprite.png';

// Custom potato asset mapping
const customPotatoAssets = {
  potato_0001: {
    static: potato0001,
    deploySprite: potato0001DeploySprite
  }
};

interface TradingCardProps {
  card: {
    id: string;
    name: string;
    description?: string;
    flavor_text?: string;
    potato_type: 'ice' | 'fire' | 'lightning' | 'light' | 'void';
    rarity: 'common' | 'uncommon' | 'rare' | 'legendary' | 'exotic';
    card_type?: 'unit' | 'spell' | 'structure' | 'relic';
    unit_class?: 'warrior' | 'tank' | 'mage' | 'healer' | 'mixed';
    mana_cost: number;
    attack: number | null;
    hp: number | null;
    structure_hp?: number;
    spell_damage?: number;
    heal_amount?: number;
    health?: number; // Backend uses 'health' instead of 'hp'
    current_attack?: number; // For in-game modified stats
    current_health?: number; // For in-game modified stats
    ability_text?: string;
    keywords?: string[];
    target_type?: string;
    is_legendary?: boolean;
    exotic?: boolean;
    adjective?: string;
    trait?: string;
    registry_id?: string; // For custom potato assets
  };
  onClick?: () => void;
  onAddToDeck?: () => void;
  onRemoveFromDeck?: () => void;
  owned?: number;
  inDeck?: boolean;
  canAdd?: boolean;
  showInGameStats?: boolean;
  showDownloadButton?: boolean;
  className?: string;
}

// Rarity styling with enhanced gradients and borders
const rarityStyles = {
  common: {
    gradient: 'from-slate-50 via-slate-100 to-slate-200',
    border: 'border-slate-400',
    glow: 'shadow-slate-400/30',
    nameColor: 'text-slate-800'
  },
  uncommon: {
    gradient: 'from-emerald-50 via-emerald-100 to-emerald-200',
    border: 'border-emerald-500',
    glow: 'shadow-emerald-500/40',
    nameColor: 'text-emerald-900'
  },
  rare: {
    gradient: 'from-sky-50 via-sky-100 to-sky-200',
    border: 'border-sky-500',
    glow: 'shadow-sky-500/40',
    nameColor: 'text-sky-900'
  },
  legendary: {
    gradient: 'from-amber-50 via-amber-100 to-amber-200',
    border: 'border-amber-500',
    glow: 'shadow-amber-500/40',
    nameColor: 'text-amber-900'
  },
  exotic: {
    gradient: 'from-orange-50 via-orange-100 to-orange-200',
    border: 'border-orange-500',
    glow: 'shadow-orange-500/40',
    nameColor: 'text-orange-900'
  }
};

// Element colors for the type icon
const elementColors = {
  ice: 'text-blue-500',
  fire: 'text-red-500',
  lightning: 'text-purple-500',
  light: 'text-yellow-500',
  void: 'text-gray-800'
};

// Element text colors for potato names (readable against backgrounds)
const elementTextColors = {
  ice: 'text-cyan-100',      // Light cyan for ice backgrounds
  fire: 'text-orange-100',   // Light orange for fire backgrounds  
  lightning: 'text-purple-100', // Light purple for lightning backgrounds
  light: 'text-yellow-100',  // Light yellow for light backgrounds
  void: 'text-purple-100'    // Light purple for void backgrounds
};

// Rarity badge images
const rarityImages = {
  common: commonRarity,
  uncommon: uncommonRarity,
  rare: rareRarity,
  legendary: legendaryRarity,
  exotic: exoticRarity
};

// Element background images mapping
const elementBackgrounds = {
  fire: fireCard,
  ice: iceCard,
  light: lightCard,
  lightning: lightningCard,
  void: voidCard
};

const TradingCard: React.FC<TradingCardProps> = ({
  card,
  onClick,
  onAddToDeck,
  onRemoveFromDeck,
  owned = 0,
  inDeck = false,
  canAdd = true,
  showInGameStats = false,
  showDownloadButton = false,
  className = ''
}) => {
  const [isDownloading, setIsDownloading] = useState(false)
  const cardRef = useRef<HTMLDivElement>(null)
  const rarityStyle = rarityStyles[card.rarity] || rarityStyles.common;
  const elementColor = elementColors[card.potato_type] || elementColors.void;
  const elementTextColor = elementTextColors[card.potato_type] || elementTextColors.void;
  
  // Use exotic background only when exotic column is true, otherwise use element-based background
  const backgroundImage = card.exotic === true
    ? exoticClassCard 
    : (elementBackgrounds[card.potato_type] || elementBackgrounds.void);
  
  // Download function to create exact 1:1 PNG replica using html2canvas
  const handleDownload = async () => {
    if (isDownloading) return
    
    setIsDownloading(true)
    
    try {
      if (!cardRef.current) {
        throw new Error('Card element not found')
      }

      // Wait for fonts and images to fully load
      await new Promise(resolve => setTimeout(resolve, 300))
      await document.fonts.ready

      // Add temporary CSS to fix text positioning for html2canvas
      const style = document.createElement('style')
      style.textContent = `
        [data-card-id="${card.id}"] h3,
        [data-card-id="${card.id}"] p {
          transform: translateY(-4px) !important;
          line-height: 1.2 !important;
          max-height: none !important;
          overflow: visible !important;
        }
        [data-card-id="${card.id}"] .absolute.top-1 span {
          transform: translateY(-3px) !important;
          color: white !important;
          font-weight: bold !important;
          text-shadow: 2px 2px 0 #000, -2px -2px 0 #000, 2px -2px 0 #000, -2px 2px 0 #000 !important;
          z-index: 30 !important;
          position: relative !important;
          font-size: 1.25rem !important;
        }
        [data-card-id="${card.id}"] .absolute.bottom-3 span {
          transform: translateY(-3px) !important;
          color: white !important;
          font-weight: bold !important;
          text-shadow: 2px 2px 0 #000, -2px -2px 0 #000, 2px -2px 0 #000, -2px 2px 0 #000 !important;
          z-index: 30 !important;
          position: relative !important;
          font-size: 1.25rem !important;
        }
        [data-card-id="${card.id}"] [data-download-button="true"],
        [data-card-id="${card.id}"] [data-owned-badge="true"],
        [data-card-id="${card.id}"] [data-in-deck-badge="true"] {
          display: none !important;
        }
        [data-card-id="${card.id}"] .w-40.h-40 img {
          transform: scale(1.5) scaleX(1.05) scaleY(0.85) translateX(-5px) !important;
          transform-origin: center center !important;
        }
      `
      document.head.appendChild(style)

      // Force reflow
      cardRef.current.offsetHeight

      // Capture with transparent background
      const canvas = await html2canvas(cardRef.current, {
        scale: 2,
        logging: false,
        backgroundColor: null
      })

      // Remove the temporary CSS
      document.head.removeChild(style)

      // Download the exact replica
      const link = document.createElement('a')
      link.download = `${card.name.replace(/[^a-zA-Z0-9]/g, '_')}_card.png`
      link.href = canvas.toDataURL('image/png', 1.0)
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)

      toast.success(`Downloaded ${card.name} card!`)
    } catch (error) {
      console.error('Download failed:', error)
      toast.error(`Failed to download card: ${error.message}`)
    } finally {
      setIsDownloading(false)
    }
  }
  
  return (
    <div 
      ref={cardRef}
      className={`
        relative rounded-2xl cursor-pointer transition-all duration-300 
        hover:scale-105 bg-cover bg-center bg-no-repeat
        ${inDeck ? 'ring-4 ring-blue-400' : ''} 
        ${!canAdd && !inDeck ? 'opacity-50' : ''}
        overflow-hidden select-none
        ${className}
      `}
      style={{ 
        width: '18rem', // Fixed width in rem for consistent scaling
        height: '27rem', // Fixed height in rem (maintains 2:3 aspect ratio)
        aspectRatio: '2/3',
        backgroundImage: `url(${backgroundImage})`
      }}
      onClick={onClick}
      data-card-id={card.id}
    >
      {/* Card Type Badge (Top Left, Lower Position, More to Right) */}
      <div className="absolute top-6 left-6 z-20">
        <div className={`
          px-2 py-1 rounded-md text-xs font-bold uppercase tracking-wide
          border shadow-lg backdrop-blur-sm
          ${card.card_type === 'unit' ? 'bg-green-600/90 text-white border-green-400/50' :
            card.card_type === 'spell' ? 'bg-purple-600/90 text-white border-purple-400/50' :
            card.card_type === 'structure' ? 'bg-yellow-600/90 text-white border-yellow-400/50' :
            card.card_type === 'relic' ? 'bg-orange-600/90 text-white border-orange-400/50' :
            'bg-gray-600/90 text-white border-gray-400/50'
          }
        `}>
          {card.card_type || 'unit'}
        </div>
      </div>

      {/* Card Header */}
      <div className="relative p-4 pb-2">
        {/* Name (Top Left) - HIDDEN FOR CUSTOM CARDS */}
        {/* <div className="absolute top-3 left-3 max-w-40">
          <h3 className={`font-bold text-sm leading-tight ${rarityStyle.nameColor} truncate`}>
            {card.name || '?'}
          </h3>
          {!card.name && (
            <div className="text-xs text-red-500">
              Missing name
            </div>
          )}
        </div> */}
        
        {/* Mana Cost (Top Right) - HIDDEN FOR CUSTOM CARDS */}
        {/* <div className="absolute top-2 right-2">
          <div className="flex items-center gap-1">
            <span className="text-blue-900 font-bold text-xl drop-shadow-md">{card.mana_cost}</span>
            <CustomManaIcon size={18} className="text-blue-600 drop-shadow-md" />
          </div>
        </div> */}

        {/* Mana Cost (Top Right) */}
        <div className="absolute top-1 -right-8">
          <div className="relative flex items-center justify-center" style={{ width: '7rem', height: '5rem' }}>
            <img 
              src={manaCrystalIcon} 
              alt="Mana Cost" 
              className="absolute inset-0 w-full h-full drop-shadow-lg object-contain"
            />
            <span 
              className="relative z-10 text-white font-bold text-xl drop-shadow-md"
              style={{
                fontFamily: 'PixelFont, monospace',
                textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
                transform: 'translateY(2px)' // Move down slightly to align with icon
              }}
            >
              {card.mana_cost}
            </span>
          </div>
        </div>

        {/* Removed download button - download functionality completely removed */}
        
        {/* Download Button (Below Type Badge) */}
        {showDownloadButton && (
          <div className="absolute top-14 left-2 z-10">
            <Button
              onClick={(e) => {
                e.stopPropagation()
                handleDownload()
              }}
              disabled={isDownloading}
              size="sm"
              className="bg-blue-600 hover:bg-blue-700 text-white shadow-lg"
              data-download-button="true"
            >
              {isDownloading ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Download className="w-4 h-4" />
              )}
            </Button>
          </div>
        )}

        {/* Owned Badge (Top Center) */}
        {owned > 0 && (
          <div className="absolute top-2 left-1/2 transform -translate-x-1/2 z-10">
            <span 
              className="text-green-600 font-medium bg-green-50/90 backdrop-blur-sm px-3 py-1 rounded-full border border-green-200 shadow-md text-sm"
              data-owned-badge="true"
            >
              Owned: {owned}
            </span>
          </div>
        )}
        
        {/* Element Type Icon (Under Name) - HIDDEN FOR CUSTOM CARDS */}
        {/* <div className="absolute top-8 left-3 mt-1">
          <div className={`${elementColor} drop-shadow-lg`}>
            <ElementIcon element={card.potato_type} size={24} />
          </div>
        </div> */}
      </div>

      {/* Pixel Art (Center) - Custom art for specific potatoes, generated for others */}
      <div className="flex flex-col items-center" style={{ marginTop: '20px' }}>
        <div className="w-40 h-40">
          {card.registry_id && customPotatoAssets[card.registry_id as keyof typeof customPotatoAssets] ? (
            // Use custom potato art for specific registry IDs - bigger but same position
            <img 
              src={customPotatoAssets[card.registry_id as keyof typeof customPotatoAssets].static}
              alt={card.name}
              className="w-full h-full object-cover drop-shadow-lg"
              style={{ 
                imageRendering: 'pixelated',
                filter: 'drop-shadow(2px 2px 4px rgba(0,0,0,0.5))',
                transform: 'scale(1.7)', // Make image bigger but 15% smaller than previous (2.0 * 0.85 = 1.7)
                transformOrigin: 'center center'
              }}
            />
          ) : (
            // Use generated ElementalPixelPotato for cards without custom art
            <ElementalPixelPotato
              seed={card.id}
              potatoType={card.potato_type}
              rarity={card.rarity}
              cardName={card.name}
              size={160}
            />
          )}
        </div>
        
        {/* Potato Name */}
        <div className="mt-2 px-4">
          <h3 
            className={`${elementTextColor} font-bold text-center drop-shadow-lg leading-tight break-words`}
            style={{
              fontSize: '1.125rem', // Relative font size
              fontFamily: 'PixelFont, monospace',
              textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
              wordWrap: 'break-word',
              hyphens: 'auto'
            }}
          >
            {card.name}
          </h3>
        </div>
        
        {/* Card Flavor Text */}
        <div className="mt-1 px-12">
          <p 
            className="text-white text-xs text-center drop-shadow-md leading-tight line-clamp-3 break-all"
            style={{
              textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
              wordWrap: 'break-word',
              hyphens: 'auto',
              maxWidth: '100%'
            }}
          >
            {card.flavor_text || card.description || ''}
          </p>
        </div>

        {/* Card Ability Text - Positioned perfectly in the middle */}
        {card.ability_text && (
          <div className="absolute bottom-32 left-1/2 transform -translate-x-1/2 w-full px-8">
            <div 
              className="bg-black/30 backdrop-blur-sm rounded-lg px-3 py-2 border border-white/20"
              style={{
                background: 'rgba(0, 0, 0, 0.4)',
                backdropFilter: 'blur(4px)'
              }}
            >
              <p 
                className="text-white text-xs text-center leading-tight break-words"
                style={{
                  textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
                  wordWrap: 'break-word',
                  hyphens: 'auto'
                }}
              >
                {card.ability_text}
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Stats (Under Pixel Art) - HIDDEN FOR CUSTOM CARDS */}
      {/* <div className="flex justify-center gap-6 mb-3">
        <div className="flex items-center gap-1">
          <div className="w-8 h-8 rounded-lg bg-red-500 text-white font-bold text-sm flex items-center justify-center shadow-md border-2 border-red-600">
            ⚔
          </div>
          <span className="font-bold text-lg text-red-700">
            {showInGameStats ? (card.current_attack !== undefined ? card.current_attack : card.attack) : card.attack}
          </span>
        </div>
        
        <div className="flex items-center gap-1">
          <div className="w-8 h-8 rounded-lg bg-green-500 text-white font-bold text-sm flex items-center justify-center shadow-md border-2 border-green-600">
            ❤
          </div>
          <span className="font-bold text-lg text-green-700">
            {showInGameStats ? (card.current_health !== undefined ? card.current_health : (card.health || card.hp)) : (card.hp || card.health)}
          </span>
        </div>
      </div> */}

      {/* Rarity Badge - HIDDEN FOR CUSTOM CARDS */}
      {/* <div className="flex justify-center mb-3">
        {card.exotic ? (
          <Badge className="bg-orange-500 text-white border-orange-600 text-xs font-bold px-3 py-1 shadow-md flex items-center gap-1">
            <RarityIcon rarity="exotic" className="w-4 h-4" />
            EXOTIC
          </Badge>
        ) : card.is_legendary ? (
          <Badge className="bg-amber-500 text-white border-amber-600 text-xs font-bold px-3 py-1 shadow-md flex items-center gap-1">
            <RarityIcon rarity="legendary" className="w-4 h-4" />
            LEGENDARY
          </Badge>
        ) : (
          <Badge className={`text-xs font-medium px-3 py-1 shadow-sm border-2 bg-white/80 ${rarityStyle.border} ${rarityStyle.nameColor} flex items-center gap-1`}>
            <RarityIcon rarity={card.rarity || 'common'} className="w-3 h-3" />
            {(card.rarity || 'common').toUpperCase()}
          </Badge>
        )}
      </div> */}

      {/* Ability Text - HIDDEN FOR CUSTOM CARDS */}
      {/* {card.ability_text && (
        <div className="px-4 mb-2">
          <div className="bg-blue-50/80 border border-blue-200 rounded-lg p-2 backdrop-blur-sm">
            <p className="text-blue-800 text-xs italic text-center leading-relaxed">
              {card.ability_text}
            </p>
          </div>
        </div>
      )} */}

      {/* Description (Bottom) - HIDDEN FOR CUSTOM CARDS */}
      {/* <div className="px-4 mb-4">
        <p className="text-gray-700 text-xs text-center line-clamp-3 leading-relaxed">
          {card.flavor_text || card.description}
        </p>
      </div> */}

      {/* ATK Icon (Bottom Left) - Only for units (not structures/spells/relics) */}
      {(card.card_type === 'unit' || !card.card_type) && (
        <div className="absolute bottom-3 left-3">
          <div className="relative flex items-center justify-center" style={{ width: '5rem', height: '3.5rem' }}>
            <img 
              src={atkIcon} 
              alt="Attack Points" 
              className="absolute inset-0 w-full h-full drop-shadow-lg object-contain"
            />
            <span 
              className="relative z-10 text-white font-bold text-xl drop-shadow-md"
              style={{
                fontFamily: 'PixelFont, monospace',
                textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
                transform: 'translateY(2px)' // Move down slightly to align with icon
              }}
            >
              {showInGameStats ? (card.current_attack !== undefined ? card.current_attack : card.attack) : card.attack}
            </span>
          </div>
        </div>
      )}

      {/* Rarity Badge (Bottom Center) */}
      <div className="absolute left-1/2 transform -translate-x-1/2" style={{ bottom: '-8px' }}>
        <img 
          src={rarityImages[card.rarity]} 
          alt={`${card.rarity} rarity`}
          className="w-48 h-24 drop-shadow-lg object-contain"
        />
      </div>

      {/* HP Icon (Bottom Right) - For units and structures only (not relics) */}
      {(card.card_type === 'unit' || card.card_type === 'structure' || !card.card_type) && (
        <div className="absolute bottom-3 right-3">
          <div className="relative flex items-center justify-center" style={{ width: '5rem', height: '3.5rem' }}>
            <img 
              src={hpIcon} 
              alt="Health Points" 
              className="absolute inset-0 w-full h-full drop-shadow-lg object-contain"
            />
            <span 
              className="relative z-10 text-white font-bold text-xl drop-shadow-md"
              style={{
                fontFamily: 'PixelFont, monospace',
                textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
                transform: 'translateY(2px)' // Move down slightly to align with icon
              }}
            >
              {showInGameStats ? 
                (card.current_health !== undefined ? card.current_health : (card.health || card.hp || card.structure_hp)) : 
                (card.card_type === 'structure' ? (card.structure_hp || card.hp || card.health) : (card.hp || card.health || card.structure_hp))
              }
            </span>
          </div>
        </div>
      )}

      {/* Bottom Stats/Actions (moved above HP) */}
      <div className="absolute bottom-20 right-3">
        <div className="flex justify-end items-center text-xs gap-2">
          {inDeck && (
            <span 
              className="text-blue-600 font-medium bg-blue-50 px-2 py-1 rounded border border-blue-200"
              data-in-deck-badge="true"
            >
              In Deck
            </span>
          )}
        </div>
      </div>

      {/* Action Buttons */}
      {(onAddToDeck || onRemoveFromDeck) && (
        <div className="absolute bottom-0 left-0 right-0 p-3 bg-white/80 backdrop-blur-sm rounded-b-2xl border-t border-white/60">
          <div className="flex gap-2">
            {onAddToDeck && canAdd && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onAddToDeck();
                }}
                className="flex-1 px-3 py-2 bg-blue-600 text-white text-xs font-medium rounded-lg hover:bg-blue-700 transition-colors shadow-md"
              >
                Add to Deck
              </button>
            )}
            {onRemoveFromDeck && inDeck && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onRemoveFromDeck();
                }}
                className="flex-1 px-3 py-2 bg-red-600 text-white text-xs font-medium rounded-lg hover:bg-red-700 transition-colors shadow-md"
              >
                Remove
              </button>
            )}
          </div>
        </div>
      )}

      {/* Holographic effect overlay - REMOVED FOR CLEAN CUSTOM IMAGES */}
    </div>
  );
};

export default TradingCard;