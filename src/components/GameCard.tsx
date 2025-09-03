// =============================================
// Beautiful Game Card Component
// Stunning card design with animations and interactions
// =============================================

import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { 
  Zap, 
  Heart, 
  Sword, 
  Shield, 
  Crown, 
  Sparkles, 
  Eye, 
  Wind,
  Skull,
  Target,
  Castle,
  Wand2
} from 'lucide-react';
import PixelPotato from './PixelPotato';

interface CardKeyword {
  code: string;
  name: string;
  description: string;
}

interface CardDef {
  id: string;
  code: string;
  name: string;
  description: string;
  cardType: 'unit' | 'spell' | 'structure';
  cost: number;
  attack?: number;
  health?: number;
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary' | 'exotic';
  keywords: { keyword: CardKeyword; value?: number }[];
  // Potato data for art rendering
  potatoData?: {
    type: string;
    generation_seed: string;
    rarity: string;
  };
}

interface GameCardProps {
  card: {
    id: string;
    cardDef: CardDef;
    currentAttack?: number;
    currentHealth?: number;
    maxHealth?: number;
    canAttack?: boolean;
    hasAttackedThisTurn?: boolean;
    summonningSickness?: boolean;
    hasTaunt?: boolean;
    hasCharge?: boolean;
    hasLifesteal?: boolean;
    hasStealth?: boolean;
    hasDivineShield?: boolean;
    hasWindfury?: boolean;
    hasPoisonous?: boolean;
    tempAttackBonus?: number;
    tempHealthBonus?: number;
  };
  zone: 'hand' | 'battlefield' | 'graveyard';
  isPlayable?: boolean;
  isTargetable?: boolean;
  isSelected?: boolean;
  isHighlighted?: boolean;
  isDragging?: boolean;
  size?: 'small' | 'medium' | 'large';
  onClick?: () => void;
  onDoubleClick?: () => void;
  onRightClick?: (e: React.MouseEvent) => void;
  children?: React.ReactNode;
}

const rarityColors = {
  common: 'from-gray-400 to-gray-600',
  uncommon: 'from-green-400 to-green-600',
  rare: 'from-blue-400 to-blue-600',
  legendary: 'from-purple-400 to-purple-600',
  exotic: 'from-orange-400 via-pink-500 to-purple-600'
};

const rarityGlows = {
  common: 'shadow-gray-500/20',
  uncommon: 'shadow-green-500/30',
  rare: 'shadow-blue-500/30',
  legendary: 'shadow-purple-500/40',
  exotic: 'shadow-pink-500/50'
};

const typeColors = {
  unit: 'from-emerald-600 to-emerald-700',
  spell: 'from-violet-600 to-violet-700',
  structure: 'from-amber-600 to-amber-700'
};

const typeIcons = {
  unit: Sword,
  spell: Wand2,
  structure: Castle
};

export const GameCard: React.FC<GameCardProps> = ({
  card,
  zone,
  isPlayable = false,
  isTargetable = false,
  isSelected = false,
  isHighlighted = false,
  isDragging = false,
  size = 'medium',
  onClick,
  onDoubleClick,
  onRightClick,
  children
}) => {
  const [isHovered, setIsHovered] = useState(false);
  const [showTooltip, setShowTooltip] = useState(false);

  const { cardDef } = card;
  const rarity = cardDef.rarity;
  const cardType = cardDef.cardType;

  // Size variants - Responsive
  const sizeClasses = {
    small: 'w-12 h-18 sm:w-16 sm:h-24',
    medium: 'w-20 h-30 sm:w-28 sm:h-42 md:w-32 md:h-48',
    large: 'w-28 h-42 sm:w-36 sm:h-54 md:w-40 md:h-60'
  };

  const textSizes = {
    small: 'text-[6px] sm:text-[8px]',
    medium: 'text-[8px] sm:text-[10px] md:text-xs',
    large: 'text-[10px] sm:text-xs md:text-sm'
  };

  // Card states
  const isInPlay = zone === 'battlefield';
  const isDead = isInPlay && (card.currentHealth ?? 0) <= 0;
  const isExhausted = isInPlay && card.hasAttackedThisTurn && !card.hasWindfury;
  const isSick = isInPlay && card.summonningSickness && !card.hasCharge;
  const isBuffed = (card.tempAttackBonus ?? 0) > 0 || (card.tempHealthBonus ?? 0) > 0;

  // Keyword icons
  const keywordIcons = {
    taunt: Shield,
    charge: Zap,
    lifesteal: Heart,
    stealth: Eye,
    divine_shield: Crown,
    windfury: Wind,
    poisonous: Skull
  };

  const getKeywordIcon = (code: string) => keywordIcons[code as keyof typeof keywordIcons];

  const TypeIcon = typeIcons[cardType];

  return (
    <div
      className={cn(
        'relative group cursor-pointer transition-all duration-300 ease-out',
        sizeClasses[size],
        {
          // Hover effects
          'transform hover:-translate-y-2 hover:scale-105': !isDragging && zone === 'hand',
          'transform hover:scale-110': !isDragging && isInPlay,
          
          // Selection states
          'transform -translate-y-1 scale-105': isSelected,
          'ring-2 ring-yellow-400 ring-opacity-60': isSelected,
          
          // Playability
          'hover:shadow-2xl hover:shadow-green-500/30 cursor-pointer': isPlayable,
          'opacity-60': !isPlayable && zone === 'hand',
          
          // Targeting
          'ring-2 ring-red-400 ring-opacity-80 shadow-lg shadow-red-500/30': isTargetable,
          'ring-2 ring-blue-400 ring-opacity-60': isHighlighted,
          
          // Battlefield states
          'opacity-50 grayscale': isDead,
          'opacity-75': isExhausted,
          'animate-pulse ring-1 ring-yellow-300': isSick,
          
          // Dragging
          'scale-110 rotate-3 z-50': isDragging,
        }
      )}
      onClick={onClick}
      onDoubleClick={onDoubleClick}
      onContextMenu={onRightClick}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Card Frame */}
      <div className={cn(
        'w-full h-full rounded-lg border-2 overflow-hidden relative',
        'bg-gradient-to-b from-slate-100 to-slate-200',
        'border-slate-300 shadow-lg',
        rarityGlows[rarity],
        {
          'shadow-2xl border-opacity-80': isHovered || isSelected,
          'shadow-xl': isInPlay,
        }
      )}>
        
        {/* Rarity Border */}
        <div className={cn(
          'absolute inset-0 rounded-lg opacity-30',
          'bg-gradient-to-r',
          rarityColors[rarity]
        )} />

        {/* Type Header */}
        <div className={cn(
          'relative flex items-center justify-between p-1',
          'bg-gradient-to-r text-white text-xs font-bold',
          typeColors[cardType]
        )}>
          <div className="flex items-center gap-1">
            <TypeIcon className="w-3 h-3" />
            <span className={textSizes[size]}>{cardType.toUpperCase()}</span>
          </div>
          
          {/* Mana Cost */}
          <div className="flex items-center justify-center w-6 h-6 bg-blue-600 rounded-full text-white font-bold text-xs border-2 border-blue-300">
            {cardDef.cost}
          </div>
        </div>

        {/* Card Art Area */}
        <div className="relative h-1/3 bg-gradient-to-b from-green-200 to-green-400 flex items-center justify-center overflow-hidden">
          {/* Actual Potato Pixel Art */}
          {cardDef.potatoData ? (
            <div className="w-full h-full flex items-center justify-center scale-110">
              <PixelPotato
                seed={cardDef.potatoData.generation_seed}
                potatoType={cardDef.potatoData.type}
                size={size === 'small' ? 24 : size === 'medium' ? 32 : 40}
              />
            </div>
          ) : (
            // Fallback for cards without potato data
            <div className="text-4xl opacity-60">🥔</div>
          )}
          
          {/* Keywords Overlay */}
          {card.hasDivineShield && (
            <div className="absolute top-1 left-1">
              <Crown className="w-3 h-3 text-yellow-400 drop-shadow" />
            </div>
          )}
          
          {card.hasStealth && (
            <div className="absolute top-1 right-1">
              <Eye className="w-3 h-3 text-purple-400 drop-shadow opacity-60" />
            </div>
          )}
        </div>

        {/* Card Name */}
        <div className="px-2 py-1 bg-white bg-opacity-90">
          <h3 className={cn(
            'font-bold text-center leading-tight text-slate-800',
            textSizes[size]
          )}>
            {cardDef.name}
          </h3>
        </div>

        {/* Stats Area */}
        {(cardDef.attack !== undefined || cardDef.health !== undefined) && (
          <div className="absolute bottom-1 left-1 right-1 flex justify-between items-center">
            {/* Attack */}
            {cardDef.attack !== undefined && (
              <div className={cn(
                'flex items-center justify-center w-6 h-6 rounded-full font-bold text-xs',
                'bg-red-500 text-white border-2 border-red-300',
                {
                  'bg-green-500 border-green-300': (card.tempAttackBonus ?? 0) > 0,
                  'scale-110': isBuffed
                }
              )}>
                {(card.currentAttack ?? cardDef.attack) + (card.tempAttackBonus ?? 0)}
              </div>
            )}

            {/* Health */}
            {cardDef.health !== undefined && (
              <div className={cn(
                'flex items-center justify-center w-6 h-6 rounded-full font-bold text-xs',
                'bg-green-600 text-white border-2 border-green-300',
                {
                  'bg-red-500 border-red-300': (card.currentHealth ?? cardDef.health) < cardDef.health,
                  'bg-blue-500 border-blue-300': (card.tempHealthBonus ?? 0) > 0,
                  'scale-110': isBuffed
                }
              )}>
                {(card.currentHealth ?? cardDef.health) + (card.tempHealthBonus ?? 0)}
              </div>
            )}
          </div>
        )}

        {/* Keywords Bar */}
        {cardDef.keywords.length > 0 && (
          <div className="absolute bottom-0 left-0 right-0 bg-slate-800 bg-opacity-80 p-1">
            <div className="flex flex-wrap gap-1 justify-center">
              {cardDef.keywords.map(({ keyword }) => {
                const IconComponent = getKeywordIcon(keyword.code);
                const isActive = {
                  taunt: card.hasTaunt,
                  charge: card.hasCharge,
                  lifesteal: card.hasLifesteal,
                  stealth: card.hasStealth,
                  divine_shield: card.hasDivineShield,
                  windfury: card.hasWindfury,
                  poisonous: card.hasPoisonous,
                }[keyword.code];

                return IconComponent ? (
                  <div
                    key={keyword.code}
                    className={cn(
                      'p-0.5 rounded',
                      {
                        'bg-yellow-500': isActive,
                        'bg-gray-500': !isActive
                      }
                    )}
                    title={keyword.description}
                  >
                    <IconComponent className="w-2 h-2 text-white" />
                  </div>
                ) : null;
              })}
            </div>
          </div>
        )}

        {/* Special Effects */}
        {card.canAttack && !card.hasAttackedThisTurn && isInPlay && (
          <div className="absolute inset-0 border-2 border-green-400 rounded-lg animate-pulse pointer-events-none" />
        )}

        {card.hasTaunt && (
          <div className="absolute inset-0 border-2 border-yellow-400 rounded-lg pointer-events-none" />
        )}

        {/* Targeting Indicator */}
        {isTargetable && (
          <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
            <Target className="w-8 h-8 text-red-500 animate-ping" />
          </div>
        )}

        {/* Rarity Sparkles */}
        {(rarity === 'legendary' || rarity === 'exotic') && isHovered && (
          <div className="absolute inset-0 pointer-events-none">
            <Sparkles className="absolute top-2 right-2 w-4 h-4 text-yellow-400 animate-pulse" />
            <Sparkles className="absolute bottom-2 left-2 w-3 h-3 text-purple-400 animate-pulse delay-300" />
          </div>
        )}
      </div>

      {/* Children (drag previews, etc.) */}
      {children}

      {/* Tooltip */}
      {showTooltip && (
        <div className="absolute z-50 bottom-full left-1/2 transform -translate-x-1/2 mb-2 p-2 bg-slate-900 text-white text-xs rounded shadow-lg max-w-xs">
          <div className="font-bold">{cardDef.name}</div>
          <div className="text-slate-300">{cardDef.description}</div>
          {cardDef.keywords.length > 0 && (
            <div className="mt-1">
              {cardDef.keywords.map(({ keyword }) => (
                <div key={keyword.code} className="text-yellow-300 text-xs">
                  <span className="font-semibold">{keyword.name}:</span> {keyword.description}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};