import React from 'react'
import { motion } from 'framer-motion'
import ElementalPixelPotato from '@/components/ElementalPixelPotato'

interface HeroHUDProps {
  hero: {
    hp: number
    max_hp: number
    mana: number
    max_mana: number
    hero_power_available: boolean
    hero_power_cost: number
  }
  isPlayer: boolean
  isActive: boolean
  onHeroPowerClick?: () => void
  heroPowerCooldown?: number
  className?: string
}

interface ManaBarProps {
  current: number
  max: number
  isPlayer: boolean
}

interface HealthBarProps {
  current: number
  max: number
  isPlayer: boolean
}

/**
 * HeroHUD: Pixel-styled hero interface with ornate frames and crystal mana
 * Features: Segmented health bars, crystal mana orbs, cooldown indicators
 */
export const HeroHUD: React.FC<HeroHUDProps> = ({
  hero,
  isPlayer,
  isActive,
  onHeroPowerClick,
  heroPowerCooldown = 0,
  className = ''
}) => {
  if (!hero) {
    return <div className="text-white">Loading Hero...</div>
  }
  
  const healthPercent = (hero.hp / hero.max_hp) * 100
  const isCritical = healthPercent < 25
  const heroType = isPlayer ? 'light' : 'void'

  return (
    <div className={`flex items-center gap-4 select-none ${className}`}>
      {/* Hero Portrait Frame */}
      <motion.div
        className="relative"
        animate={isActive ? {
          boxShadow: ['0 0 0 rgba(59, 130, 246, 0)', '0 0 20px rgba(59, 130, 246, 0.8)', '0 0 0 rgba(59, 130, 246, 0)']
        } : {}}
        transition={{ duration: 2, repeat: Infinity }}
      >
        {/* Ornate Frame */}
        <div 
          className={`relative w-20 h-20 rounded-2xl border-4 overflow-hidden ${
            isActive
              ? 'border-yellow-400 shadow-xl shadow-yellow-400/50'
              : isPlayer
                ? 'border-blue-400 shadow-lg shadow-blue-400/30'
                : 'border-red-400 shadow-lg shadow-red-400/30'
          } bg-gradient-to-br from-slate-700 to-slate-800`}
          style={{
            imageRendering: 'pixelated'
          }}
        >
          {/* Decorative Corner Gems */}
          <div className="absolute top-1 left-1 w-2 h-2 bg-yellow-400 rounded-full"></div>
          <div className="absolute top-1 right-1 w-2 h-2 bg-yellow-400 rounded-full"></div>
          <div className="absolute bottom-1 left-1 w-2 h-2 bg-yellow-400 rounded-full"></div>
          <div className="absolute bottom-1 right-1 w-2 h-2 bg-yellow-400 rounded-full"></div>

          {/* Health Bar Background Overlay */}
          <div className="absolute inset-0 rounded-2xl overflow-hidden">
            <div 
              className={`absolute bottom-0 left-0 right-0 transition-all duration-500 ${
                isCritical ? 'bg-red-500' : 'bg-green-500'
              }`}
              style={{ height: `${healthPercent}%`, opacity: 0.3 }}
            />
          </div>

          {/* Hero Avatar */}
          <div className="absolute inset-2 rounded-xl overflow-hidden">
            <ElementalPixelPotato 
              variant="hero"
              seed={`hero-${isPlayer ? 'player' : 'enemy'}`}
              rarity="legendary"
              potatoType={heroType}
              size={64}
              className={`w-full h-full ${isCritical ? 'animate-pulse' : ''} ${hero.hp <= 0 ? 'grayscale' : ''}`}
            />
          </div>

          {/* Active Turn Crown */}
          {isActive && (
            <motion.div
              className="absolute -top-2 -right-2 bg-yellow-500 rounded-full p-1 shadow-lg"
              animate={{ scale: [1, 1.1, 1], rotate: [0, 5, 0] }}
              transition={{ duration: 2, repeat: Infinity }}
            >
              <div className="w-4 h-4 text-yellow-900 text-xs flex items-center justify-center">👑</div>
            </motion.div>
          )}

          {/* Hero Power Indicator */}
          {hero.hero_power_available && heroPowerCooldown === 0 && (
            <motion.div
              className="absolute -bottom-1 left-1/2 transform -translate-x-1/2 cursor-pointer"
              onClick={onHeroPowerClick}
              whileHover={{ scale: 1.1 }}
              whileTap={{ scale: 0.9 }}
            >
              <div className="bg-purple-500 rounded-full p-1 shadow-lg animate-pulse border-2 border-purple-300">
                <div className="w-3 h-3 text-white text-xs flex items-center justify-center">⚡</div>
              </div>
            </motion.div>
          )}

          {/* Hero Power Cooldown */}
          {heroPowerCooldown > 0 && (
            <div className="absolute -bottom-1 left-1/2 transform -translate-x-1/2">
              <div className="bg-gray-600 rounded-full p-1 shadow-lg border-2 border-gray-400 relative">
                <div className="w-3 h-3 text-gray-300 text-xs flex items-center justify-center">⚡</div>
                {/* Cooldown Overlay */}
                <motion.div
                  className="absolute inset-0 bg-red-500 rounded-full"
                  initial={{ scale: 1 }}
                  animate={{ scale: 0 }}
                  transition={{ duration: heroPowerCooldown }}
                />
                <div className="absolute inset-0 flex items-center justify-center text-white text-xs font-bold">
                  {heroPowerCooldown}
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Health Display */}
        <div className="absolute -bottom-3 left-1/2 transform -translate-x-1/2 bg-black/80 rounded-lg px-2 py-1 border border-white/20">
          <div className="flex items-center gap-1 text-xs">
            <div className="w-3 h-3 text-red-400 text-xs flex items-center justify-center">❤</div>
            <span className="text-white font-bold" style={{ fontFamily: "'Press Start 2P', monospace" }}>
              {hero.hp}
            </span>
            <span className="text-gray-400">/</span>
            <span className="text-gray-400">{hero.max_hp}</span>
          </div>
        </div>
      </motion.div>

      {/* Health Bar */}
      <HealthBar current={hero.hp} max={hero.max_hp} isPlayer={isPlayer} />

      {/* Mana Bar */}
      <ManaBar current={hero.mana} max={hero.max_mana} isPlayer={isPlayer} />
    </div>
  )
}

/**
 * Pixel-style segmented health bar
 */
const HealthBar: React.FC<HealthBarProps> = ({ current, max, isPlayer }) => {
  const segments = Math.min(max, 10) // Cap display at 10 segments
  const healthPerSegment = max / segments
  const isCritical = (current / max) < 0.25

  return (
    <div className="flex flex-col gap-1">
      <div className="text-xs text-white/60" style={{ fontFamily: "'Press Start 2P', monospace" }}>
        HEALTH
      </div>
      <div className="flex gap-1">
        {[...Array(segments)].map((_, i) => {
          const segmentHealth = Math.min(healthPerSegment, Math.max(0, current - (i * healthPerSegment)))
          const fillPercent = (segmentHealth / healthPerSegment) * 100
          
          return (
            <motion.div
              key={i}
              className={`w-3 h-8 border border-gray-600 relative overflow-hidden`}
              style={{
                imageRendering: 'pixelated',
                background: 'linear-gradient(to top, #374151, #4b5563)'
              }}
              animate={isCritical && fillPercent > 0 ? {
                boxShadow: ['0 0 0 rgba(239, 68, 68, 0)', '0 0 8px rgba(239, 68, 68, 0.8)', '0 0 0 rgba(239, 68, 68, 0)']
              } : {}}
              transition={{ duration: 1, repeat: Infinity }}
            >
              {/* Health Fill */}
              <motion.div
                className={`absolute bottom-0 left-0 right-0 ${
                  isCritical ? 'bg-red-500' : 'bg-green-500'
                }`}
                style={{ height: `${fillPercent}%` }}
                initial={{ height: 0 }}
                animate={{ height: `${fillPercent}%` }}
                transition={{ duration: 0.5, ease: 'easeOut' }}
              />
              
              {/* Shine Effect */}
              <motion.div
                className="absolute inset-0 bg-gradient-to-t from-transparent via-white/30 to-transparent"
                animate={{ y: ['-100%', '100%'] }}
                transition={{ duration: 2, repeat: Infinity, repeatDelay: 3 }}
              />
            </motion.div>
          )
        })}
      </div>
      <div className="text-xs text-center text-white/80" style={{ fontFamily: "'Press Start 2P', monospace" }}>
        {current}/{max}
      </div>
    </div>
  )
}

/**
 * Crystal mana orbs with swirling energy
 */
const ManaBar: React.FC<ManaBarProps> = ({ current, max, isPlayer }) => {
  return (
    <div className="flex flex-col gap-1">
      <div className="text-xs text-white/60" style={{ fontFamily: "'Press Start 2P', monospace" }}>
        MANA
      </div>
      <div className="flex gap-1">
        {[...Array(Math.max(max, 1))].map((_, i) => {
          const isFilled = i < current
          
          return (
            <motion.div
              key={i}
              className={`relative w-6 h-6 border-2 transition-all duration-300 ${
                isFilled 
                  ? 'bg-gradient-to-br from-blue-400 to-cyan-600 border-cyan-400 shadow-lg shadow-cyan-400/50' 
                  : 'bg-gradient-to-br from-slate-600 to-slate-700 border-slate-500'
              }`}
              style={{
                clipPath: 'polygon(50% 0%, 100% 25%, 100% 75%, 50% 100%, 0% 75%, 0% 25%)',
                imageRendering: 'pixelated'
              }}
              initial={{ scale: 0, rotate: -180 }}
              animate={{ scale: 1, rotate: 0 }}
              transition={{ delay: i * 0.1, type: "spring" }}
            >
              {/* Crystal shine */}
              <div className="absolute inset-0 bg-gradient-to-t from-white/20 to-transparent" 
                style={{ clipPath: 'polygon(50% 0%, 100% 25%, 100% 75%, 50% 100%, 0% 75%, 0% 25%)' }}
              />
              
              {/* Lightning icon */}
              <div className={`absolute inset-0 flex items-center justify-center text-xs ${
                isFilled ? 'text-white' : 'text-slate-400'
              }`}>
                ⚡
              </div>
              
              {/* Energy swirl for filled crystals */}
              {isFilled && (
                <motion.div
                  className="absolute inset-0 bg-cyan-400/30 rounded-lg"
                  animate={{ 
                    scale: [1, 1.1, 1],
                    opacity: [0.3, 0.6, 0.3]
                  }}
                  transition={{ 
                    duration: 2, 
                    repeat: Infinity, 
                    delay: i * 0.3 
                  }}
                  style={{ clipPath: 'polygon(50% 0%, 100% 25%, 100% 75%, 50% 100%, 0% 75%, 0% 25%)' }}
                />
              )}
            </motion.div>
          )
        })}
      </div>
      <div className="text-xs text-center text-white/80" style={{ fontFamily: "'Press Start 2P', monospace" }}>
        {current}/{max}
      </div>
    </div>
  )
}

export default HeroHUD