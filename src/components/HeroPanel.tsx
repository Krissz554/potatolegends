// =============================================
// Beautiful Hero Panel Component
// Player health, avatar, and status display
// =============================================

import React, { useState, useEffect } from 'react';
import { cn } from '@/lib/utils';
import { 
  Heart, 
  Shield, 
  Crown, 
  Skull, 
  Zap, 
  Users,
  Clock,
  Target
} from 'lucide-react';

interface HeroPanelProps {
  playerId: string;
  playerName: string;
  health: number;
  maxHealth: number;
  isMyHero?: boolean;
  isActivePlayer?: boolean;
  isTargetable?: boolean;
  deckCount?: number;
  handCount?: number;
  avatarUrl?: string;
  className?: string;
  onHeroClick?: () => void;
  children?: React.ReactNode;
}

export const HeroPanel: React.FC<HeroPanelProps> = ({
  playerId,
  playerName,
  health,
  maxHealth,
  isMyHero = false,
  isActivePlayer = false,
  isTargetable = false,
  deckCount = 0,
  handCount = 0,
  avatarUrl,
  className,
  onHeroClick,
  children
}) => {
  const [isHovered, setIsHovered] = useState(false);
  const [damageAnimation, setDamageAnimation] = useState<number | null>(null);
  const [healAnimation, setHealAnimation] = useState<number | null>(null);
  const [previousHealth, setPreviousHealth] = useState(health);

  // Animate health changes
  useEffect(() => {
    if (health !== previousHealth) {
      const diff = health - previousHealth;
      if (diff < 0) {
        setDamageAnimation(Math.abs(diff));
        setTimeout(() => setDamageAnimation(null), 1500);
      } else if (diff > 0) {
        setHealAnimation(diff);
        setTimeout(() => setHealAnimation(null), 1500);
      }
      setPreviousHealth(health);
    }
  }, [health, previousHealth]);

  const healthPercentage = Math.max(0, (health / maxHealth) * 100);
  const isLowHealth = healthPercentage <= 30;
  const isCriticalHealth = healthPercentage <= 15;
  const isDead = health <= 0;

  return (
    <div
      className={cn(
        'relative group transition-all duration-300 ease-out',
        'bg-gradient-to-br from-slate-100 to-slate-200',
        'border-2 border-slate-300 rounded-xl shadow-lg',
        'p-4 min-w-64',
        {
          // My hero styling
          'border-blue-400 shadow-blue-500/20': isMyHero,
          'border-red-400 shadow-red-500/20': !isMyHero,
          
          // Active turn indicator
          'ring-2 ring-yellow-400 ring-opacity-60 shadow-lg shadow-yellow-500/20': isActivePlayer,
          
          // Targetable state
          'cursor-pointer hover:scale-105 hover:shadow-xl': isTargetable,
          'ring-2 ring-red-500 ring-opacity-80': isTargetable && isHovered,
          
          // Health-based states
          'border-orange-400 shadow-orange-500/30': isLowHealth && !isCriticalHealth,
          'border-red-500 shadow-red-500/40 animate-pulse': isCriticalHealth && !isDead,
          'grayscale opacity-60': isDead,
          
          // Hover effects
          'hover:shadow-xl': !isDead,
        },
        className
      )}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onClick={onHeroClick}
    >
      {/* Background Pattern */}
      <div className="absolute inset-0 rounded-xl opacity-5">
        <div className="w-full h-full bg-gradient-to-br from-transparent via-white to-transparent" />
      </div>

      {/* Player Info Header */}
      <div className="relative flex items-center justify-between mb-3">
        <div className="flex items-center gap-3">
          {/* Avatar */}
          <div className={cn(
            'relative w-12 h-12 rounded-full overflow-hidden border-2 shadow-md',
            {
              'border-blue-400': isMyHero,
              'border-red-400': !isMyHero,
              'ring-2 ring-yellow-400 ring-opacity-60': isActivePlayer,
            }
          )}>
            {avatarUrl ? (
              <img 
                src={avatarUrl} 
                alt={playerName}
                className="w-full h-full object-cover"
              />
            ) : (
              <div className={cn(
                'w-full h-full flex items-center justify-center text-xl font-bold',
                {
                  'bg-blue-500 text-white': isMyHero,
                  'bg-red-500 text-white': !isMyHero,
                }
              )}>
                {playerName.charAt(0).toUpperCase()}
              </div>
            )}
            
            {/* Active turn indicator */}
            {isActivePlayer && (
              <div className="absolute -top-1 -right-1 w-4 h-4 bg-yellow-400 rounded-full border-2 border-white animate-pulse">
                <Crown className="w-2 h-2 text-yellow-800 ml-0.5 mt-0.5" />
              </div>
            )}
          </div>

          {/* Name and Status */}
          <div className="flex flex-col">
            <h3 className="font-bold text-slate-800 text-lg">
              {playerName}
            </h3>
            <div className="flex items-center gap-2 text-sm text-slate-600">
              {isMyHero && <span className="text-blue-600 font-medium">You</span>}
              {isActivePlayer && (
                <span className="flex items-center gap-1 text-yellow-600">
                  <Clock className="w-3 h-3" />
                  Your Turn
                </span>
              )}
              {isDead && (
                <span className="flex items-center gap-1 text-red-600">
                  <Skull className="w-3 h-3" />
                  Defeated
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Deck Info */}
        <div className="flex flex-col items-end gap-1 text-sm text-slate-600">
          <div className="flex items-center gap-1">
            <div className="w-4 h-6 bg-blue-800 rounded border border-blue-600" />
            <span>{deckCount}</span>
          </div>
          <div className="flex items-center gap-1">
            <Users className="w-4 h-4" />
            <span>{handCount}</span>
          </div>
        </div>
      </div>

      {/* Health Display */}
      <div className="relative">
        {/* Health Bar Background */}
        <div className="w-full h-8 bg-slate-300 rounded-lg overflow-hidden shadow-inner">
          {/* Health Bar Fill */}
          <div
            className={cn(
              'h-full transition-all duration-500 ease-out',
              'bg-gradient-to-r',
              {
                'from-green-400 to-green-500': healthPercentage > 60,
                'from-yellow-400 to-orange-500': healthPercentage > 30 && healthPercentage <= 60,
                'from-orange-500 to-red-500': healthPercentage > 15 && healthPercentage <= 30,
                'from-red-500 to-red-700 animate-pulse': healthPercentage <= 15,
              }
            )}
            style={{ width: `${healthPercentage}%` }}
          />
          
          {/* Health Text */}
          <div className="absolute inset-0 flex items-center justify-center">
            <div className="flex items-center gap-1 font-bold text-white drop-shadow-lg">
              <Heart className="w-4 h-4" />
              <span>{health}</span>
              <span className="text-white/80">/ {maxHealth}</span>
            </div>
          </div>
        </div>

        {/* Damage Animation */}
        {damageAnimation && (
          <div className="absolute -top-8 left-1/2 transform -translate-x-1/2 animate-bounce">
            <div className="bg-red-500 text-white px-2 py-1 rounded font-bold text-sm shadow-lg">
              -{damageAnimation}
            </div>
          </div>
        )}

        {/* Heal Animation */}
        {healAnimation && (
          <div className="absolute -top-8 left-1/2 transform -translate-x-1/2 animate-bounce">
            <div className="bg-green-500 text-white px-2 py-1 rounded font-bold text-sm shadow-lg">
              +{healAnimation}
            </div>
          </div>
        )}
      </div>

      {/* Special Effects */}
      {isTargetable && (
        <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
          <Target className="w-16 h-16 text-red-500 animate-ping opacity-50" />
        </div>
      )}

      {/* Low Health Warning */}
      {isCriticalHealth && !isDead && (
        <div className="absolute -top-2 -right-2 animate-pulse">
          <div className="w-6 h-6 bg-red-500 rounded-full flex items-center justify-center">
            <Skull className="w-4 h-4 text-white" />
          </div>
        </div>
      )}

      {/* Children (additional content) */}
      {children}

      {/* Hover Tooltip */}
      {isHovered && !isDead && (
        <div className="absolute top-full left-1/2 transform -translate-x-1/2 mt-2 p-2 bg-slate-900 text-white text-xs rounded shadow-lg z-50 whitespace-nowrap">
          <div className="flex items-center gap-2">
            <Heart className="w-3 h-3 text-red-400" />
            <span>{health} / {maxHealth} Health</span>
          </div>
          {isTargetable && (
            <div className="flex items-center gap-2 mt-1 text-red-300">
              <Target className="w-3 h-3" />
              <span>Click to attack</span>
            </div>
          )}
        </div>
      )}
    </div>
  );
};