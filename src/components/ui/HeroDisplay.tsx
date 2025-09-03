import React from 'react';
import { motion } from 'framer-motion';
import ElementalPixelPotato from '@/components/ElementalPixelPotato';

// Import pixel art assets
import heroBorderIcon from '@/assets/pixel/icons/hero-border-icon.png';
import manaCrystalIcon from '@/assets/pixel/icons/mana-crystal-icon.png';
import hpIcon from '@/assets/pixel/icons/HP-icon.png';
import heroAbilityIcon from '@/assets/pixel/icons/hero-ability-icon.png';

interface Hero {
  id: string;
  hero_id: string;
  name: string;
  description: string;
  base_hp: number;
  base_mana: number;
  hero_power_name: string;
  hero_power_description: string;
  hero_power_cost: number;
  rarity: string;
  element_type: string;
}

interface HeroDisplayProps {
  hero: Hero;
  currentHp: number;
  maxHp: number;
  currentMana: number;
  maxMana: number;
  isOpponent?: boolean;
  heroPowerAvailable?: boolean;
  heroPowerOnCooldown?: boolean;
  onHeroPowerClick?: () => void;
  className?: string;
}

export const HeroDisplay: React.FC<HeroDisplayProps> = ({
  hero,
  currentHp,
  maxHp,
  currentMana,
  maxMana,
  isOpponent = false,
  heroPowerAvailable = true,
  heroPowerOnCooldown = false,
  onHeroPowerClick,
  className = ''
}) => {
  // Determine hero power button state
  const canUseHeroPower = heroPowerAvailable && !heroPowerOnCooldown && currentMana >= hero.hero_power_cost;

  return (
    <div className={`relative select-none ${className}`}>
      {/* Hero Portrait with overlaid stats - Centered */}
      <div className="relative flex items-center justify-center">
        <img 
          src={heroBorderIcon} 
          alt="Hero Border"
          className="pixel-art relative z-10"
          style={{
            imageRendering: 'pixelated',
            filter: isOpponent ? 'hue-rotate(180deg)' : 'none',
            width: 'auto',
            height: 'auto',
            maxWidth: '180px',
            maxHeight: '180px'
          }}
        />

        {/* Hero Pixel Art Inside Border */}
        <div className="absolute inset-0 flex items-center justify-center z-5">
          <div className="w-24 h-24 rounded-full bg-gradient-to-br from-white/10 to-white/5 border border-white/20 shadow-lg overflow-hidden backdrop-blur-sm">
            <ElementalPixelPotato
              seed={hero.hero_id}
              potatoType={hero.element_type as any}
              rarity={hero.rarity as any}
              cardName={hero.name}
              size={96}
            />
          </div>
        </div>

        {/* HP Display - Bottom Left Corner */}
        <div className="absolute bottom-0 left-0 transform translate-x-2 translate-y-2 z-20">
          <div className="flex flex-col items-center">
            <div className="relative">
              <img 
                src={hpIcon} 
                alt="HP" 
                className="pixel-art"
                style={{ 
                  imageRendering: 'pixelated',
                  width: 'auto',
                  height: 'auto',
                  maxWidth: '80px',
                  maxHeight: '80px'
                }}
              />
              {/* HP Number Overlay */}
              <div className="absolute inset-0 flex items-center justify-center">
                <span 
                  className="text-white font-bold drop-shadow-lg"
                  style={{ 
                    fontFamily: "'Press Start 2P', monospace",
                    fontSize: currentHp >= 100 ? '12px' : '14px',
                    textShadow: '1px 1px 0px #000, -1px -1px 0px #000, 1px -1px 0px #000, -1px 1px 0px #000'
                  }}
                >
                  {currentHp}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Mana Display - Bottom Right Corner */}
        <div className="absolute bottom-0 right-0 transform translate-x-0 translate-y-3 z-20">
          <div className="flex flex-col items-center">
            <div className="relative">
              <img 
                src={manaCrystalIcon} 
                alt="Mana" 
                className="pixel-art"
                style={{ 
                  imageRendering: 'pixelated',
                  width: 'auto',
                  height: 'auto',
                  maxWidth: '96px', // 20% bigger (80px * 1.2 = 96px)
                  maxHeight: '96px' // 20% bigger (80px * 1.2 = 96px)
                }}
              />
              {/* Mana Number Overlay */}
              <div className="absolute inset-0 flex items-center justify-center">
                <span 
                  className="text-white font-bold drop-shadow-lg"
                  style={{ 
                    fontFamily: "'Press Start 2P', monospace",
                    fontSize: maxMana >= 10 ? '12px' : '14px',
                    textShadow: '1px 1px 0px #000, -1px -1px 0px #000, 1px -1px 0px #000, -1px 1px 0px #000'
                  }}
                >
                  {currentMana}/{maxMana}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Hero Power Button - Positioned absolutely to the right */}
      <div className="absolute left-full ml-8 top-1/2 transform -translate-y-1/2 flex items-center">

        {/* Hero Power Button */}
        {!isOpponent && (
          <motion.button
            onClick={onHeroPowerClick}
            disabled={!canUseHeroPower}
            className={`relative rounded-lg border-2 transition-all duration-200 p-2 ${
              canUseHeroPower 
                ? 'border-yellow-400 bg-yellow-600/20 hover:bg-yellow-500/30 cursor-pointer shadow-lg shadow-yellow-400/50' 
                : 'border-gray-600 bg-gray-800/50 cursor-not-allowed'
            }`}
            whileHover={canUseHeroPower ? { scale: 1.05 } : {}}
            whileTap={canUseHeroPower ? { scale: 0.95 } : {}}
            title={`${hero.hero_power_name}: ${hero.hero_power_description} (Cost: ${hero.hero_power_cost} mana)`}
          >
            <img 
              src={heroAbilityIcon} 
              alt="Hero Power" 
              className={`pixel-art ${
                canUseHeroPower ? 'opacity-100' : 'opacity-50'
              }`}
              style={{ 
                imageRendering: 'pixelated',
                width: 'auto',
                height: 'auto',
                maxWidth: '40px',
                maxHeight: '40px'
              }}
            />
            
            {/* Glow effect when available */}
            {canUseHeroPower && (
              <motion.div
                className="absolute inset-0 rounded-lg bg-yellow-400/20"
                animate={{ 
                  opacity: [0.3, 0.6, 0.3],
                  scale: [1, 1.02, 1]
                }}
                transition={{ 
                  duration: 2, 
                  repeat: Infinity,
                  ease: "easeInOut"
                }}
              />
            )}

            {/* Cooldown overlay */}
            {heroPowerOnCooldown && (
              <div className="absolute inset-0 bg-gray-900/70 rounded-lg flex items-center justify-center">
                <span 
                  className="text-white text-xs"
                  style={{ fontFamily: "'Press Start 2P', monospace" }}
                >
                  CD
                </span>
              </div>
            )}

            {/* Mana cost indicator */}
            <div 
              className="absolute -bottom-2 -right-2 bg-blue-600 text-white rounded-full w-5 h-5 flex items-center justify-center text-xs"
              style={{ fontFamily: "'Press Start 2P', monospace" }}
            >
              {hero.hero_power_cost}
            </div>
          </motion.button>
        )}

        {/* Opponent Hero Power Indicator */}
        {isOpponent && (
          <div className="relative rounded-lg border-2 border-gray-600 bg-gray-800/50 p-2">
            <img 
              src={heroAbilityIcon} 
              alt="Hero Power" 
              className="pixel-art opacity-75"
              style={{ 
                imageRendering: 'pixelated',
                width: 'auto',
                height: 'auto',
                maxWidth: '40px',
                maxHeight: '40px'
              }}
            />
            <div 
              className="absolute -bottom-2 -right-2 bg-blue-600 text-white rounded-full w-5 h-5 flex items-center justify-center text-xs"
              style={{ fontFamily: "'Press Start 2P', monospace" }}
            >
              {hero.hero_power_cost}
            </div>
          </div>
        )}
      </div>

      {/* Hero Power Description (on hover for player) */}
      {!isOpponent && (
        <div className="absolute top-full mt-2 left-1/2 transform -translate-x-1/2 bg-black bg-opacity-90 text-white p-2 rounded text-xs max-w-48 text-center opacity-0 hover:opacity-100 transition-opacity duration-200 pointer-events-none z-50"
             style={{ fontFamily: "'Press Start 2P', monospace" }}>
          <div className="font-bold text-yellow-400 mb-1">{hero.hero_power_name}</div>
          <div>{hero.hero_power_description}</div>
        </div>
      )}
    </div>
  );
};

export default HeroDisplay;