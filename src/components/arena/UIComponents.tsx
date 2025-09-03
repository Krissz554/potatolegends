import React from 'react'
import { motion } from 'framer-motion'
import { pixelPerfectStyles } from '@/hooks/useResponsiveScale'

interface EndTurnButtonProps {
  isActive: boolean
  disabled?: boolean
  onClick: () => void
  className?: string
}

interface DeckCounterProps {
  count: number
  isPlayer: boolean
  onClick?: () => void
  className?: string
}

interface TurnTimerProps {
  timeLeft: number
  maxTime: number
  isActive: boolean
  className?: string
}

interface ConcedeMenuProps {
  onConcede: () => void
  onSettings?: () => void
  className?: string
}

/**
 * EndTurnButton: Large magical rune button with state animations
 */
export const EndTurnButton: React.FC<EndTurnButtonProps> = ({
  isActive,
  disabled = false,
  onClick,
  className = ''
}) => {
  return (
    <motion.button
      className={`
        relative px-6 py-4 border-4 rounded-xl font-bold text-lg transition-all duration-200
        ${isActive && !disabled
          ? 'bg-gradient-to-br from-green-500 to-emerald-600 border-green-600 text-white shadow-lg shadow-green-500/50'
          : disabled
            ? 'bg-gradient-to-br from-gray-500 to-gray-600 border-gray-600 text-gray-300 cursor-not-allowed'
            : 'bg-gradient-to-br from-blue-500 to-blue-600 border-blue-600 text-white'
        }
        ${className}
      `}
      style={{
        ...pixelPerfectStyles.pixelFont,
        fontSize: '14px',
        imageRendering: 'pixelated'
      }}
      onClick={isActive && !disabled ? onClick : undefined}
      disabled={disabled}
      animate={isActive && !disabled ? {
        boxShadow: [
          '0 0 0 rgba(34, 197, 94, 0)',
          '0 0 20px rgba(34, 197, 94, 0.8)',
          '0 0 0 rgba(34, 197, 94, 0)'
        ]
      } : {}}
      transition={{ duration: 2, repeat: Infinity }}
      whileHover={isActive && !disabled ? { scale: 1.05 } : {}}
      whileTap={isActive && !disabled ? { scale: 0.95 } : {}}
    >
      {/* Rune Symbol */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
        <motion.div
          className="text-2xl opacity-20"
          animate={isActive && !disabled ? { rotate: 360 } : {}}
          transition={{ duration: 4, repeat: Infinity, ease: 'linear' }}
        >
          ⭕
        </motion.div>
      </div>
      
      {/* Button Text */}
      <span className="relative z-10">
        {isActive && !disabled ? 'END TURN' : disabled ? 'WAITING...' : 'ENEMY TURN'}
      </span>
      
      {/* Ripple Effect */}
      {isActive && !disabled && (
        <motion.div
          className="absolute inset-0 rounded-xl border-4 border-white/30"
          animate={{
            scale: [1, 1.1, 1],
            opacity: [0.5, 0, 0.5]
          }}
          transition={{ duration: 1.5, repeat: Infinity }}
        />
      )}
    </motion.button>
  )
}

/**
 * DeckCounter: Pixel stack with count and click interaction
 */
export const DeckCounter: React.FC<DeckCounterProps> = ({
  count,
  isPlayer,
  onClick,
  className = ''
}) => {
  return (
    <motion.div
      className={`
        relative cursor-pointer bg-gradient-to-br border-2 rounded-lg p-3 backdrop-blur-sm
        ${isPlayer
          ? 'from-blue-900/80 to-blue-800/80 border-blue-500'
          : 'from-red-900/80 to-red-800/80 border-red-500'
        }
        ${className}
      `}
      onClick={onClick}
      whileHover={{ scale: 1.05 }}
      whileTap={{ scale: 0.95 }}
      style={pixelPerfectStyles.crisp}
    >
      {/* Stack Visualization */}
      <div className="relative">
        {/* Multiple card layers for stack effect */}
        {[...Array(Math.min(3, Math.ceil(count / 10)))].map((_, i) => (
          <div
            key={i}
            className={`
              absolute w-8 h-12 border rounded
              ${isPlayer ? 'bg-blue-700 border-blue-500' : 'bg-red-700 border-red-500'}
            `}
            style={{
              left: i * 2,
              top: i * -1,
              zIndex: 3 - i
            }}
          />
        ))}
        
        {/* Main card */}
        <div className={`
          w-8 h-12 border-2 rounded flex items-center justify-center relative z-10
          ${isPlayer ? 'bg-blue-600 border-blue-400' : 'bg-red-600 border-red-400'}
        `}>
          <div className="text-xs text-white">📚</div>
        </div>
      </div>
      
      {/* Count Display */}
      <div className="flex items-center gap-2 mt-2">
        <div className="text-white text-sm font-bold" style={pixelPerfectStyles.pixelFont}>
          {count}
        </div>
      </div>
      
      <div 
        className={`text-xs opacity-70 ${isPlayer ? 'text-blue-300' : 'text-red-300'}`}
        style={pixelPerfectStyles.pixelFont}
      >
        {isPlayer ? 'DECK' : 'ENEMY'}
      </div>
    </motion.div>
  )
}

/**
 * TurnTimer: Pixel hourglass with animated sand
 */
export const TurnTimer: React.FC<TurnTimerProps> = ({
  timeLeft,
  maxTime,
  isActive,
  className = ''
}) => {
  const percentage = (timeLeft / maxTime) * 100
  const isUrgent = percentage <= 25

  return (
    <div className={`flex items-center gap-3 ${className}`}>
      {/* Hourglass Icon */}
      <div className="relative">
        <motion.div
          className={`text-2xl ${isUrgent ? 'text-red-400' : 'text-yellow-400'}`}
          animate={isUrgent ? { scale: [1, 1.1, 1] } : {}}
          transition={{ duration: 0.5, repeat: Infinity }}
        >
          ⏳
        </motion.div>
        
        {/* Falling sand animation */}
        {isActive && (
          <motion.div
            className="absolute top-1 left-1/2 w-0.5 h-4 bg-yellow-400 opacity-60"
            animate={{ height: [0, 16, 0] }}
            transition={{ duration: 2, repeat: Infinity, ease: 'linear' }}
          />
        )}
      </div>
      
      {/* Timer Display */}
      <div className="flex flex-col">
        <div 
          className={`text-lg font-bold ${isUrgent ? 'text-red-400' : 'text-white'}`}
          style={pixelPerfectStyles.pixelFont}
        >
          {Math.floor(timeLeft / 60)}:{(timeLeft % 60).toString().padStart(2, '0')}
        </div>
        
        {/* Progress Bar */}
        <div className="w-16 h-2 bg-gray-600 border border-gray-500 rounded overflow-hidden">
          <motion.div
            className={`h-full ${isUrgent ? 'bg-red-500' : 'bg-yellow-400'}`}
            style={{ width: `${percentage}%` }}
            animate={isUrgent ? {
              backgroundColor: ['#ef4444', '#dc2626', '#ef4444']
            } : {}}
            transition={{ duration: 1, repeat: Infinity }}
          />
        </div>
      </div>
    </div>
  )
}

/**
 * ConcedeMenu: Compact icon menu with flag and settings
 */
export const ConcedeMenu: React.FC<ConcedeMenuProps> = ({
  onConcede,
  onSettings,
  className = ''
}) => {
  const [isOpen, setIsOpen] = React.useState(false)

  return (
    <div className={`relative ${className}`}>
      {/* Menu Toggle */}
      <motion.button
        className="w-10 h-10 bg-gray-800 border-2 border-gray-600 rounded-lg flex items-center justify-center text-white hover:bg-gray-700"
        onClick={() => setIsOpen(!isOpen)}
        whileHover={{ scale: 1.05 }}
        whileTap={{ scale: 0.95 }}
        style={pixelPerfectStyles.crisp}
      >
        <div className="text-sm">☰</div>
      </motion.button>
      
      {/* Menu Items */}
      {isOpen && (
        <motion.div
          className="absolute bottom-12 right-0 bg-gray-800 border-2 border-gray-600 rounded-lg p-2 space-y-2"
          initial={{ scale: 0, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0, opacity: 0 }}
          style={pixelPerfectStyles.crisp}
        >
          {/* Concede Button */}
          <motion.button
            className="flex items-center gap-2 px-3 py-2 bg-red-600 border border-red-500 rounded text-white text-xs hover:bg-red-500"
            onClick={() => {
              onConcede()
              setIsOpen(false)
            }}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            style={pixelPerfectStyles.pixelFont}
          >
            <span>🏳️</span>
            <span>CONCEDE</span>
          </motion.button>
          
          {/* Settings Button */}
          {onSettings && (
            <motion.button
              className="flex items-center gap-2 px-3 py-2 bg-blue-600 border border-blue-500 rounded text-white text-xs hover:bg-blue-500"
              onClick={() => {
                onSettings()
                setIsOpen(false)
              }}
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              style={pixelPerfectStyles.pixelFont}
            >
              <span>⚙️</span>
              <span>SETTINGS</span>
            </motion.button>
          )}
        </motion.div>
      )}
      
      {/* Click outside to close */}
      {isOpen && (
        <div
          className="fixed inset-0 z-[-1]"
          onClick={() => setIsOpen(false)}
        />
      )}
    </div>
  )
}

/**
 * Tooltip: Pixel-styled tooltip with crisp edges
 */
export const Tooltip: React.FC<{
  children: React.ReactNode
  content: string
  position?: 'top' | 'bottom' | 'left' | 'right'
  className?: string
}> = ({ children, content, position = 'top', className = '' }) => {
  const [isVisible, setIsVisible] = React.useState(false)

  return (
    <div 
      className={`relative inline-block ${className}`}
      onMouseEnter={() => setIsVisible(true)}
      onMouseLeave={() => setIsVisible(false)}
    >
      {children}
      
      {isVisible && (
        <motion.div
          className={`
            absolute z-50 px-2 py-1 bg-gray-900 border border-gray-600 rounded text-white text-xs
            ${position === 'top' ? 'bottom-full left-1/2 transform -translate-x-1/2 mb-2' : ''}
            ${position === 'bottom' ? 'top-full left-1/2 transform -translate-x-1/2 mt-2' : ''}
            ${position === 'left' ? 'right-full top-1/2 transform -translate-y-1/2 mr-2' : ''}
            ${position === 'right' ? 'left-full top-1/2 transform -translate-y-1/2 ml-2' : ''}
          `}
          style={{
            ...pixelPerfectStyles.pixelFont,
            fontSize: '8px',
            whiteSpace: 'nowrap'
          }}
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.1 }}
        >
          {content}
          
          {/* Arrow */}
          <div
            className={`
              absolute w-0 h-0
              ${position === 'top' ? 'top-full left-1/2 transform -translate-x-1/2 border-l-4 border-r-4 border-t-4 border-transparent border-t-gray-900' : ''}
              ${position === 'bottom' ? 'bottom-full left-1/2 transform -translate-x-1/2 border-l-4 border-r-4 border-b-4 border-transparent border-b-gray-900' : ''}
              ${position === 'left' ? 'left-full top-1/2 transform -translate-y-1/2 border-t-4 border-b-4 border-l-4 border-transparent border-l-gray-900' : ''}
              ${position === 'right' ? 'right-full top-1/2 transform -translate-y-1/2 border-t-4 border-b-4 border-r-4 border-transparent border-r-gray-900' : ''}
            `}
          />
        </motion.div>
      )}
    </div>
  )
}

export default {
  EndTurnButton,
  DeckCounter,
  TurnTimer,
  ConcedeMenu,
  Tooltip
}