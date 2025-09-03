import React, { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import ElementalPixelPotato from '@/components/ElementalPixelPotato'

interface UnitSpriteProps {
  card: any
  position: { x: number; y: number }
  isPlayer: boolean
  isSelected?: boolean
  isTargetable?: boolean
  isTargeted?: boolean
  animationState?: 'idle' | 'spawn' | 'attack' | 'hit' | 'death' | 'victory'
  onSelect?: () => void
  onAnimationComplete?: (state: string) => void
  className?: string
}

interface StatusBadgeProps {
  type: string
  value?: number
  position: 'top' | 'bottom'
  index: number
}

/**
 * UnitSprite: Animated potato units with overlays and status indicators
 * Features: Multiple animation states, stat overlays, status badges, pixel-perfect rendering
 */
export const UnitSprite: React.FC<UnitSpriteProps> = ({
  card,
  position,
  isPlayer,
  isSelected = false,
  isTargetable = false,
  isTargeted = false,
  animationState = 'idle',
  onSelect,
  onAnimationComplete,
  className = ''
}) => {
  const [currentState, setCurrentState] = useState(animationState)
  const [isAnimating, setIsAnimating] = useState(false)

  useEffect(() => {
    if (animationState !== currentState) {
      setCurrentState(animationState)
      setIsAnimating(true)
      
      // Auto-reset to idle after non-idle animations
      if (animationState !== 'idle' && animationState !== 'death') {
        const timeout = setTimeout(() => {
          setCurrentState('idle')
          setIsAnimating(false)
          onAnimationComplete?.(animationState)
        }, getAnimationDuration(animationState))
        
        return () => clearTimeout(timeout)
      }
    }
  }, [animationState, currentState, onAnimationComplete])

  // Get animation duration based on state
  const getAnimationDuration = (state: string): number => {
    switch (state) {
      case 'spawn': return 600
      case 'attack': return 800
      case 'hit': return 400
      case 'death': return 1000
      case 'victory': return 2000
      default: return 0
    }
  }

  // Get sprite scale based on animation state
  const getSpriteScale = (): number => {
    switch (currentState) {
      case 'spawn': return 1.2
      case 'attack': return 1.1
      case 'victory': return 1.15
      default: return 1.0
    }
  }

  // Status badges (shields, buffs, debuffs)
  const statusEffects = [
    ...(card.has_taunt ? [{ type: 'taunt', icon: '🛡️' }] : []),
    ...(card.has_charge ? [{ type: 'charge', icon: '⚡' }] : []),
    ...(card.is_frozen ? [{ type: 'frozen', icon: '❄️' }] : []),
    ...(card.is_poisoned ? [{ type: 'poison', icon: '☠️' }] : []),
    ...(card.is_stunned ? [{ type: 'stunned', icon: '😵' }] : []),
    ...(card.has_stealth ? [{ type: 'stealth', icon: '👻' }] : [])
  ]

  return (
    <motion.div
      className={`absolute pointer-events-auto cursor-pointer ${className}`}
              style={{
          left: position.x - 48, // Center the 96px sprite
          top: position.y - 48,
          zIndex: isSelected ? 20 : isTargeted ? 15 : 10,
          imageRendering: 'pixelated'
        }}
      onClick={onSelect}
      initial={{ scale: 0, opacity: 0, rotate: 180 }}
      animate={{ 
        scale: currentState === 'death' ? 0 : 1,
        opacity: currentState === 'death' ? 0 : 1,
        rotate: 0,
        x: currentState === 'attack' ? [0, 10, -5, 0] : 0,
        y: currentState === 'hit' ? [0, -5, 0] : 0
      }}
      exit={{ scale: 0, opacity: 0, rotate: 360 }}
      transition={{ 
        duration: currentState === 'spawn' ? 0.6 : 0.3,
        ease: currentState === 'spawn' ? 'easeOut' : 'easeInOut'
      }}
      whileHover={!isAnimating ? { scale: 1.05 } : {}}
      whileTap={!isAnimating ? { scale: 0.95 } : {}}
    >
      {/* Selection Ring */}
      <AnimatePresence>
        {isSelected && (
          <motion.div
            className="absolute inset-0 border-4 border-yellow-400 rounded-full opacity-80"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 0.8, rotate: 360 }}
            exit={{ scale: 0.8, opacity: 0 }}
            transition={{ 
              scale: { duration: 0.2 },
              opacity: { duration: 0.2 },
              rotate: { duration: 2, repeat: Infinity, ease: 'linear' }
            }}
            style={{
              imageRendering: 'pixelated'
            }}
          />
        )}
      </AnimatePresence>

      {/* Targeting Indicator */}
      <AnimatePresence>
        {isTargeted && (
          <motion.div
            className="absolute inset-0 border-4 border-red-500 rounded-full"
            initial={{ scale: 1.2, opacity: 0 }}
            animate={{ 
              scale: [1.2, 1, 1.2], 
              opacity: [0, 1, 0]
            }}
            exit={{ scale: 1.2, opacity: 0 }}
            transition={{ 
              duration: 1,
              repeat: Infinity,
              ease: 'easeInOut'
            }}
            style={{
              imageRendering: 'pixelated'
            }}
          />
        )}
      </AnimatePresence>

      {/* Targetable Glow */}
      <AnimatePresence>
        {isTargetable && !isSelected && (
          <motion.div
            className="absolute inset-0 border-2 border-green-400 rounded-full opacity-60"
            animate={{ 
              scale: [1, 1.1, 1],
              opacity: [0.6, 1, 0.6]
            }}
            transition={{ 
              duration: 1.5,
              repeat: Infinity,
              ease: 'easeInOut'
            }}
            style={{
              imageRendering: 'pixelated'
            }}
          />
        )}
      </AnimatePresence>

      {/* Potato Sprite Container */}
      <motion.div
        className="relative w-24 h-24"
        animate={{
          scale: getSpriteScale(),
          y: currentState === 'idle' ? [0, -2, 0] : 0,
          rotateY: currentState === 'victory' ? [0, 360] : 0
        }}
        transition={{
          y: { duration: 2, repeat: Infinity, ease: 'easeInOut' },
          rotateY: { duration: 2, repeat: Infinity, ease: 'easeInOut' },
          scale: { duration: 0.3 }
        }}
      >
        {/* Spawn Dust Effect */}
        <AnimatePresence>
          {currentState === 'spawn' && (
            <motion.div
              className="absolute inset-0 pointer-events-none"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
            >
              {[...Array(8)].map((_, i) => (
                <motion.div
                  key={i}
                  className="absolute w-2 h-2 bg-yellow-300 rounded-full"
                  style={{
                    left: `${50 + Math.cos(i * 45 * Math.PI / 180) * 30}%`,
                    top: `${50 + Math.sin(i * 45 * Math.PI / 180) * 30}%`,
                    imageRendering: 'pixelated'
                  }}
                  initial={{ scale: 0, opacity: 1 }}
                  animate={{ 
                    scale: [0, 1, 0],
                    opacity: [1, 1, 0],
                    x: Math.cos(i * 45 * Math.PI / 180) * 20,
                    y: Math.sin(i * 45 * Math.PI / 180) * 20
                  }}
                  transition={{ duration: 0.6, ease: 'easeOut' }}
                />
              ))}
            </motion.div>
          )}
        </AnimatePresence>

        {/* Main Potato Sprite */}
        <div className="relative w-full h-full">
          <ElementalPixelPotato
            seed={card.id}
            potatoType={card.potato_type}
            rarity={card.rarity}
            size={96}
            className={`w-full h-full ${
              currentState === 'hit' ? 'filter brightness-150 hue-rotate-90' : ''
            } ${
              currentState === 'death' ? 'filter grayscale brightness-50' : ''
            }`}
          />

          {/* Attack Flash */}
          <AnimatePresence>
            {currentState === 'attack' && (
              <motion.div
                className="absolute inset-0 bg-gradient-to-r from-transparent via-yellow-400 to-transparent opacity-70 transform rotate-45"
                initial={{ scale: 0, opacity: 0 }}
                animate={{ scale: [0, 1.5, 0], opacity: [0, 0.7, 0] }}
                transition={{ duration: 0.3, ease: 'easeOut' }}
                style={{
                  imageRendering: 'pixelated'
                }}
              />
            )}
          </AnimatePresence>

          {/* Death Poof */}
          <AnimatePresence>
            {currentState === 'death' && (
              <motion.div
                className="absolute inset-0 pointer-events-none"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
              >
                {[...Array(12)].map((_, i) => (
                  <motion.div
                    key={i}
                    className="absolute w-3 h-3 bg-gray-400 rounded-full"
                    style={{
                      left: '50%',
                      top: '50%',
                      imageRendering: 'pixelated'
                    }}
                    initial={{ scale: 0, x: 0, y: 0, opacity: 1 }}
                    animate={{ 
                      scale: [0, 1, 0],
                      x: Math.cos(i * 30 * Math.PI / 180) * 40,
                      y: Math.sin(i * 30 * Math.PI / 180) * 40,
                      opacity: [1, 1, 0]
                    }}
                    transition={{ duration: 1, ease: 'easeOut' }}
                  />
                ))}
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </motion.div>

      {/* Status Badges Above */}
      <div className="absolute -top-6 left-1/2 transform -translate-x-1/2 flex gap-1">
        {statusEffects.slice(0, 3).map((effect, index) => (
          <StatusBadge
            key={effect.type}
            type={effect.type}
            position="top"
            index={index}
          />
        ))}
      </div>

      {/* Stat Overlays */}
      <div className="absolute -bottom-4 left-1/2 transform -translate-x-1/2 flex gap-2">
        {/* Attack */}
        <motion.div
          className="flex items-center gap-1 bg-red-500 border-2 border-red-600 rounded px-2 py-1"
          animate={{ scale: [1, 1.1, 1] }}
          transition={{ duration: 0.5, repeat: Infinity, repeatDelay: 3 }}
          style={{
            imageRendering: 'pixelated'
          }}
        >
          <div className="w-3 h-3 text-white text-xs font-bold flex items-center justify-center">⚔</div>
          <span className="text-white text-xs font-bold" style={{ fontFamily: "'Press Start 2P', monospace" }}>
            {card.current_attack !== undefined ? card.current_attack : card.attack}
          </span>
        </motion.div>

        {/* Health */}
        <motion.div
          className="flex items-center gap-1 bg-green-500 border-2 border-green-600 rounded px-2 py-1"
          animate={{ scale: [1, 1.1, 1] }}
          transition={{ duration: 0.5, repeat: Infinity, repeatDelay: 3, delay: 0.1 }}
          style={{
            imageRendering: 'pixelated'
          }}
        >
          <div className="w-3 h-3 text-white text-xs font-bold flex items-center justify-center">❤</div>
          <span className="text-white text-xs font-bold" style={{ fontFamily: "'Press Start 2P', monospace" }}>
            {card.current_health !== undefined ? card.current_health : (card.hp || card.health)}
          </span>
        </motion.div>
      </div>

      {/* Victory Sparkles */}
      <AnimatePresence>
        {currentState === 'victory' && (
          <div className="absolute inset-0 pointer-events-none">
            {[...Array(6)].map((_, i) => (
              <motion.div
                key={i}
                className="absolute w-2 h-2 bg-yellow-300 rounded-full"
                style={{
                  left: `${20 + Math.random() * 60}%`,
                  top: `${20 + Math.random() * 60}%`,
                  imageRendering: 'pixelated'
                }}
                animate={{
                  scale: [0, 1, 0],
                  opacity: [0, 1, 0],
                  y: [0, -20]
                }}
                transition={{
                  duration: 1.5,
                  repeat: Infinity,
                  delay: i * 0.2
                }}
              />
            ))}
          </div>
        )}
      </AnimatePresence>
    </motion.div>
  )
}

/**
 * Status Badge Component for buffs/debuffs
 */
const StatusBadge: React.FC<StatusBadgeProps> = ({ type, value, position, index }) => {
  const getStatusIcon = (type: string): string => {
    switch (type) {
      case 'taunt': return '🛡️'
      case 'charge': return '⚡'
      case 'frozen': return '❄️'
      case 'poison': return '☠️'
      case 'stunned': return '😵'
      case 'stealth': return '👻'
      default: return '?'
    }
  }

  const getStatusColor = (type: string): string => {
    switch (type) {
      case 'taunt': return 'bg-blue-500 border-blue-600'
      case 'charge': return 'bg-yellow-500 border-yellow-600'
      case 'frozen': return 'bg-cyan-500 border-cyan-600'
      case 'poison': return 'bg-purple-500 border-purple-600'
      case 'stunned': return 'bg-gray-500 border-gray-600'
      case 'stealth': return 'bg-indigo-500 border-indigo-600'
      default: return 'bg-gray-500 border-gray-600'
    }
  }

  return (
    <motion.div
      className={`w-6 h-6 border-2 rounded-full flex items-center justify-center text-xs ${getStatusColor(type)}`}
      initial={{ scale: 0, y: position === 'top' ? 10 : -10 }}
      animate={{ scale: 1, y: 0 }}
      exit={{ scale: 0, y: position === 'top' ? 10 : -10 }}
      transition={{ delay: index * 0.1 }}
      style={{
        imageRendering: 'pixelated'
      }}
    >
      <span className="text-white">{getStatusIcon(type)}</span>
      {value && (
        <span className="absolute -bottom-1 -right-1 w-3 h-3 bg-white text-black rounded-full text-xs flex items-center justify-center">
          {value}
        </span>
      )}
    </motion.div>
  )
}

export default UnitSprite