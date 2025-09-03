import React, { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'

interface VFXEffect {
  id: string
  type: 'spawn_dust' | 'slash' | 'bolt' | 'shield' | 'poison' | 'freeze' | 'death_poof' | 'damage_number' | 'heal_number'
  position: { x: number; y: number }
  value?: number
  duration?: number
  color?: string
}

interface VFXPortalProps {
  children: React.ReactNode
  className?: string
}

interface TargetingLineProps {
  from: { x: number; y: number }
  to: { x: number; y: number }
  isValid: boolean
  className?: string
}

/**
 * VFXPortal: Manages particle effects and visual feedback
 * Features: Damage numbers, spell effects, targeting lines, particle systems
 */
export const VFXPortal: React.FC<VFXPortalProps> = ({ children, className = '' }) => {
  const [effects, setEffects] = useState<VFXEffect[]>([])

  // Global VFX trigger function (can be called from anywhere)
  useEffect(() => {
    const triggerVFX = (effect: Omit<VFXEffect, 'id'>) => {
      const id = Math.random().toString(36).substr(2, 9)
      const newEffect = { ...effect, id }
      
      setEffects(prev => [...prev, newEffect])
      
      // Auto-remove effect after duration
      setTimeout(() => {
        setEffects(prev => prev.filter(e => e.id !== id))
      }, effect.duration || 1000)
    }

    // Expose to global scope
    ;(window as any).triggerVFX = triggerVFX

    return () => {
      delete (window as any).triggerVFX
    }
  }, [])

  return (
    <div className={`relative ${className}`}>
      {children}
      
      {/* VFX Layer */}
      <div className="absolute inset-0 pointer-events-none overflow-hidden">
        <AnimatePresence>
          {effects.map(effect => (
            <VFXEffect key={effect.id} {...effect} />
          ))}
        </AnimatePresence>
      </div>
    </div>
  )
}

/**
 * Individual VFX Effect Component
 */
const VFXEffect: React.FC<VFXEffect> = ({ type, position, value, duration = 1000, color }) => {
  const renderEffect = () => {
    switch (type) {
      case 'spawn_dust':
        return <SpawnDust position={position} />
      
      case 'slash':
        return <SlashEffect position={position} />
      
      case 'bolt':
        return <BoltEffect position={position} />
      
      case 'shield':
        return <ShieldEffect position={position} />
      
      case 'poison':
        return <PoisonEffect position={position} />
      
      case 'freeze':
        return <FreezeEffect position={position} />
      
      case 'death_poof':
        return <DeathPoof position={position} />
      
      case 'damage_number':
        return <DamageNumber position={position} value={value || 0} color={color || '#E35748'} />
      
      case 'heal_number':
        return <DamageNumber position={position} value={value || 0} color={color || '#4CCB7F'} />
      
      default:
        return null
    }
  }

  return (
    <motion.div
      className="absolute"
      style={{ left: position.x, top: position.y }}
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      transition={{ duration: duration / 1000 }}
    >
      {renderEffect()}
    </motion.div>
  )
}

/**
 * Spawn Dust Effect
 */
const SpawnDust: React.FC<{ position: { x: number; y: number } }> = ({ position }) => (
  <div className="relative">
    {[...Array(8)].map((_, i) => (
      <motion.div
        key={i}
        className="absolute w-2 h-2 bg-yellow-300 rounded-full"
        style={{
          left: -4,
          top: -4,
          imageRendering: 'pixelated'
        }}
        initial={{ scale: 0, opacity: 1 }}
        animate={{
          scale: [0, 1, 0],
          opacity: [1, 1, 0],
          x: Math.cos(i * 45 * Math.PI / 180) * 20,
          y: Math.sin(i * 45 * Math.PI / 180) * 20
        }}
        transition={{ duration: 0.3, ease: 'easeOut' }}
      />
    ))}
  </div>
)

/**
 * Slash Effect
 */
const SlashEffect: React.FC<{ position: { x: number; y: number } }> = ({ position }) => (
  <motion.div
    className="w-16 h-1 bg-gradient-to-r from-transparent via-yellow-400 to-transparent transform rotate-45"
    initial={{ scale: 0, opacity: 0 }}
    animate={{ scale: [0, 1.5, 0], opacity: [0, 1, 0] }}
    transition={{ duration: 0.15, ease: 'easeOut' }}
    style={{
      imageRendering: 'pixelated'
    }}
  />
)

/**
 * Bolt/Beam Effect
 */
const BoltEffect: React.FC<{ position: { x: number; y: number } }> = ({ position }) => (
  <div className="relative">
    <motion.div
      className="w-1 h-24 bg-gradient-to-b from-cyan-400 via-blue-500 to-cyan-400"
      initial={{ scaleY: 0, opacity: 0 }}
      animate={{ scaleY: [0, 1, 0], opacity: [0, 1, 0] }}
      transition={{ duration: 0.2, ease: 'easeOut' }}
      style={{
        imageRendering: 'pixelated'
      }}
    />
    {/* Impact burst */}
    <motion.div
      className="absolute bottom-0 left-1/2 transform -translate-x-1/2 w-8 h-8 bg-cyan-400 rounded-full"
      initial={{ scale: 0, opacity: 1 }}
      animate={{ scale: [0, 1.5, 0], opacity: [1, 0.5, 0] }}
      transition={{ duration: 0.3, ease: 'easeOut' }}
      style={{
        imageRendering: 'pixelated'
      }}
    />
  </div>
)

/**
 * Shield Effect
 */
const ShieldEffect: React.FC<{ position: { x: number; y: number } }> = ({ position }) => (
  <motion.div
    className="w-24 h-24 border-4 border-yellow-400 rounded-full bg-yellow-400/20"
    initial={{ scale: 0, opacity: 0, rotate: 0 }}
    animate={{ 
      scale: [0, 1.2, 1],
      opacity: [0, 1, 0],
      rotate: [0, 90]
    }}
    transition={{ duration: 0.4, ease: 'easeOut' }}
    style={{
      imageRendering: 'pixelated'
    }}
  />
)

/**
 * Poison Bubbles Effect
 */
const PoisonEffect: React.FC<{ position: { x: number; y: number } }> = ({ position }) => (
  <div className="relative">
    {[...Array(6)].map((_, i) => (
      <motion.div
        key={i}
        className="absolute w-2 h-2 bg-green-500 rounded-full"
        style={{
          left: Math.random() * 20 - 10,
          top: Math.random() * 20 - 10,
          imageRendering: 'pixelated'
        }}
        initial={{ scale: 0, y: 0, opacity: 1 }}
        animate={{
          scale: [0, 1, 0],
          y: -30,
          opacity: [1, 1, 0]
        }}
        transition={{
          duration: 1.5,
          delay: i * 0.1,
          ease: 'easeOut'
        }}
      />
    ))}
  </div>
)

/**
 * Freeze Shards Effect
 */
const FreezeEffect: React.FC<{ position: { x: number; y: number } }> = ({ position }) => (
  <div className="relative">
    {[...Array(6)].map((_, i) => (
      <motion.div
        key={i}
        className="absolute w-1 h-4 bg-cyan-400"
        style={{
          left: Math.cos(i * 60 * Math.PI / 180) * 15,
          top: Math.sin(i * 60 * Math.PI / 180) * 15,
          rotate: `${i * 60}deg`,
          imageRendering: 'pixelated'
        }}
        initial={{ scale: 0, opacity: 0 }}
        animate={{ 
          scale: [0, 1, 0],
          opacity: [0, 1, 0]
        }}
        transition={{
          duration: 0.4,
          delay: i * 0.05,
          ease: 'easeOut'
        }}
      />
    ))}
    
    {/* Sparkle */}
    <motion.div
      className="absolute w-3 h-3 bg-white rounded-full"
      style={{ left: -6, top: -6 }}
      animate={{
        scale: [0, 1, 0],
        opacity: [0, 1, 0]
      }}
      transition={{
        duration: 0.6,
        delay: 0.2
      }}
    />
  </div>
)

/**
 * Death Poof Effect
 */
const DeathPoof: React.FC<{ position: { x: number; y: number } }> = ({ position }) => (
  <div className="relative">
    {[...Array(12)].map((_, i) => (
      <motion.div
        key={i}
        className="absolute w-3 h-3 bg-gray-500 rounded-full"
        style={{
          left: -6,
          top: -6,
          imageRendering: 'pixelated'
        }}
        initial={{ scale: 0, opacity: 1 }}
        animate={{
          scale: [0, 1, 0],
          opacity: [1, 1, 0],
          x: Math.cos(i * 30 * Math.PI / 180) * 40,
          y: Math.sin(i * 30 * Math.PI / 180) * 40
        }}
        transition={{
          duration: 0.6,
          ease: 'easeOut'
        }}
      />
    ))}
  </div>
)

/**
 * Damage/Heal Numbers
 */
const DamageNumber: React.FC<{ 
  position: { x: number; y: number }
  value: number
  color: string
}> = ({ position, value, color }) => (
  <motion.div
    className="absolute font-bold text-lg select-none pointer-events-none"
    style={{
      color,
      fontFamily: "'Press Start 2P', monospace",
      textShadow: '2px 2px 0px rgba(0, 0, 0, 0.8)',
      left: -20,
      top: -10,
      imageRendering: 'pixelated'
    }}
    initial={{ opacity: 1, scale: 1, y: 0 }}
    animate={{
      opacity: [1, 1, 0],
      scale: [1, 1.2, 0.8],
      y: [-10, -30, -50]
    }}
    transition={{
      duration: 1,
      ease: 'easeOut'
    }}
  >
    {value > 0 ? `+${value}` : value}
  </motion.div>
)

/**
 * Targeting Line Component
 */
export const TargetingLine: React.FC<TargetingLineProps> = ({ 
  from, 
  to, 
  isValid, 
  className = '' 
}) => {
  const length = Math.sqrt(Math.pow(to.x - from.x, 2) + Math.pow(to.y - from.y, 2))
  const angle = Math.atan2(to.y - from.y, to.x - from.x) * 180 / Math.PI

  return (
    <motion.div
      className={`absolute pointer-events-none ${className}`}
      style={{
        left: from.x,
        top: from.y,
        width: length,
        height: 2,
        background: isValid 
          ? 'linear-gradient(90deg, transparent, #10b981, transparent)'
          : 'linear-gradient(90deg, transparent, #ef4444, transparent)',
        transform: `rotate(${angle}deg)`,
        transformOrigin: '0 50%',
        imageRendering: 'pixelated'
      }}
      initial={{ scaleX: 0, opacity: 0 }}
      animate={{ scaleX: 1, opacity: 1 }}
      exit={{ scaleX: 0, opacity: 0 }}
      transition={{ duration: 0.2 }}
    >
      {/* Arrow head */}
      <div
        className={`absolute right-0 top-1/2 transform -translate-y-1/2 w-0 h-0`}
        style={{
          borderLeft: `6px solid ${isValid ? '#10b981' : '#ef4444'}`,
          borderTop: '3px solid transparent',
          borderBottom: '3px solid transparent'
        }}
      />
    </motion.div>
  )
}

// Utility functions for triggering VFX
export const triggerVFX = (effect: Omit<VFXEffect, 'id'>) => {
  if ((window as any).triggerVFX) {
    ;(window as any).triggerVFX(effect)
  }
}

export const triggerDamage = (position: { x: number; y: number }, damage: number) => {
  triggerVFX({
    type: 'damage_number',
    position,
    value: damage,
    color: '#E35748',
    duration: 1000
  })
}

export const triggerHeal = (position: { x: number; y: number }, heal: number) => {
  triggerVFX({
    type: 'heal_number',
    position,
    value: heal,
    color: '#4CCB7F',
    duration: 1000
  })
}

export const triggerSpawnDust = (position: { x: number; y: number }) => {
  triggerVFX({
    type: 'spawn_dust',
    position,
    duration: 300
  })
}

export const triggerSlash = (position: { x: number; y: number }) => {
  triggerVFX({
    type: 'slash',
    position,
    duration: 150
  })
}

export default VFXPortal