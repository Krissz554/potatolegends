import React from 'react';
import { Card, CardContent } from './card';
import { Badge } from './badge';
import ElementalPixelPotato from '../ElementalPixelPotato';
import { Flame, Snowflake, Zap, Sun, Moon } from 'lucide-react';

interface EnhancedCard {
  id: string;
  name: string;
  description: string;
  potato_type: 'ice' | 'fire' | 'lightning' | 'light' | 'void';
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary' | 'exotic';
  mana_cost: number;
  attack: number;
  hp: number;
  ability_text?: string;
  flavor_text?: string;
  is_legendary?: boolean;
  exotic?: boolean;
}

interface EnhancedCardProps {
  card: EnhancedCard;
  owned?: number;
  inDeck?: boolean;
  canAdd?: boolean;
  onClick?: () => void;
  onAddToDeck?: () => void;
  onRemoveFromDeck?: () => void;
  className?: string;
}

// Rarity-based styling
const rarityStyles = {
  common: {
    gradient: 'from-gray-50 via-gray-100 to-gray-200',
    border: 'border-gray-400',
    textColor: 'text-gray-800',
    badgeColor: 'bg-gray-100 text-gray-800 border-gray-300',
    glow: 'shadow-gray-300/50'
  },
  uncommon: {
    gradient: 'from-green-50 via-green-100 to-green-200', 
    border: 'border-green-500',
    textColor: 'text-green-900',
    badgeColor: 'bg-green-100 text-green-800 border-green-300',
    glow: 'shadow-green-400/50'
  },
  rare: {
    gradient: 'from-blue-50 via-blue-100 to-blue-200',
    border: 'border-blue-500',
    textColor: 'text-blue-900', 
    badgeColor: 'bg-blue-100 text-blue-800 border-blue-300',
    glow: 'shadow-blue-400/50'
  },
  legendary: {
    gradient: 'from-yellow-50 via-yellow-100 to-yellow-200',
    border: 'border-yellow-500',
    textColor: 'text-yellow-900',
    badgeColor: 'bg-yellow-100 text-yellow-800 border-yellow-300',
    glow: 'shadow-yellow-400/50'
  },
  exotic: {
    gradient: 'from-orange-50 via-orange-100 to-orange-200',
    border: 'border-orange-500',
    textColor: 'text-orange-900',
    badgeColor: 'bg-orange-100 text-orange-800 border-orange-300',
    glow: 'shadow-orange-400/50'
  }
};

// Element icons and colors
const elementConfig = {
  ice: {
    icon: Snowflake,
    color: 'text-blue-600',
    bgColor: 'bg-blue-100',
    label: 'ICE'
  },
  fire: {
    icon: Flame,
    color: 'text-red-600', 
    bgColor: 'bg-red-100',
    label: 'FIRE'
  },
  lightning: {
    icon: Zap,
    color: 'text-purple-600',
    bgColor: 'bg-purple-100', 
    label: 'LIGHTNING'
  },
  light: {
    icon: Sun,
    color: 'text-yellow-600',
    bgColor: 'bg-yellow-100',
    label: 'LIGHT'
  },
  void: {
    icon: Moon,
    color: 'text-gray-800',
    bgColor: 'bg-gray-200',
    label: 'VOID'
  }
};

const EnhancedCardComponent: React.FC<EnhancedCardProps> = ({
  card,
  owned = 0,
  inDeck = false,
  canAdd = true,
  onClick,
  onAddToDeck,
  onRemoveFromDeck,
  className = ''
}) => {
  const rarity = card.rarity as keyof typeof rarityStyles;
  const element = card.potato_type as keyof typeof elementConfig;
  
  const rarityStyle = rarityStyles[rarity] || rarityStyles.common;
  const elementInfo = elementConfig[element] || elementConfig.light;
  const ElementIcon = elementInfo.icon;

  return (
    <Card 
      className={`
        group relative transition-all duration-300 hover:shadow-xl hover:scale-105 
        bg-gradient-to-br ${rarityStyle.gradient} 
        border-3 ${rarityStyle.border} 
        ${rarityStyle.glow} hover:shadow-xl
        ${inDeck ? 'ring-2 ring-blue-400' : ''} 
        ${!canAdd && !inDeck ? 'opacity-50' : ''}
        ${className}
      `}
      onClick={onClick}
    >
      <CardContent className="p-4">
        {/* Header with Name and Element */}
        <div className="flex justify-between items-start mb-3">
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <h3 className={`font-bold text-lg leading-tight ${rarityStyle.textColor} truncate`}>
                {card.name}
              </h3>
              
              {/* Mana Cost */}
              <div className="flex-shrink-0 w-8 h-8 rounded-full bg-blue-600 text-white font-bold text-sm flex items-center justify-center shadow-md">
                {card.mana_cost}
              </div>
            </div>
            
            {/* Element Label */}
            <div className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${elementInfo.bgColor} ${elementInfo.color} shadow-sm`}>
              <ElementIcon className="w-3 h-3" />
              <span>{elementInfo.label}</span>
            </div>
          </div>
        </div>

        {/* Pixel Art */}
        <div className="flex justify-center mb-3">
          <div className="w-24 h-24 rounded-lg overflow-hidden shadow-md">
            <ElementalPixelPotato
              seed={card.id}
              potatoType={card.potato_type}
              rarity={card.rarity}
              cardName={card.name}
              size={96}
            />
          </div>
        </div>

        {/* Stats */}
        <div className="flex justify-between items-center mb-3">
          <div className="flex items-center gap-3">
            {/* Attack */}
            <div className="flex items-center gap-1">
              <div className="w-6 h-6 rounded bg-red-500 text-white text-xs font-bold flex items-center justify-center">
                ⚔
              </div>
              <span className="font-semibold text-sm">{card.attack}</span>
            </div>
            
            {/* Health */}
            <div className="flex items-center gap-1">
              <div className="w-6 h-6 rounded bg-green-500 text-white text-xs font-bold flex items-center justify-center">
                ❤
              </div>
              <span className="font-semibold text-sm">{card.hp}</span>
            </div>
          </div>

          {/* Rarity Badge */}
          <div className="flex items-center gap-2">
            {/* Special rarity badges for legendary/exotic */}
            {card.exotic && (
              <Badge className="bg-orange-100 text-orange-800 border-orange-300 text-xs font-bold">
                ✨ EXOTIC
              </Badge>
            )}
            {card.is_legendary && !card.exotic && (
              <Badge className="bg-yellow-100 text-yellow-800 border-yellow-300 text-xs font-bold">
                👑 LEGENDARY
              </Badge>
            )}
            {/* Regular rarity badge for non-special cards */}
            {!card.is_legendary && !card.exotic && (
              <Badge className={`text-xs ${rarityStyle.badgeColor}`}>
                {card.rarity.toUpperCase()}
              </Badge>
            )}
          </div>
        </div>

        {/* Ability Text */}
        {card.ability_text && (
          <div className="mb-3 p-2 bg-blue-50 border border-blue-200 rounded text-xs">
            <span className="text-blue-800 italic">{card.ability_text}</span>
          </div>
        )}

        {/* Description */}
        <p className="text-sm text-gray-700 mb-3 line-clamp-3 leading-relaxed">
          {card.flavor_text || card.description}
        </p>

        {/* Owned Status */}
        {owned > 0 && (
          <div className="flex justify-between items-center text-xs">
            <span className="text-green-600 font-medium">
              Owned: {owned}
            </span>
            {inDeck && (
              <span className="text-blue-600 font-medium">
                In Deck
              </span>
            )}
          </div>
        )}

        {/* Action Buttons */}
        {(onAddToDeck || onRemoveFromDeck) && (
          <div className="flex gap-2 mt-3">
            {onAddToDeck && canAdd && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onAddToDeck();
                }}
                className="flex-1 px-3 py-2 bg-blue-600 text-white text-xs font-medium rounded hover:bg-blue-700 transition-colors"
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
                className="flex-1 px-3 py-2 bg-red-600 text-white text-xs font-medium rounded hover:bg-red-700 transition-colors"
              >
                Remove
              </button>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export default EnhancedCardComponent;