import React from 'react'
import { motion } from 'framer-motion'

interface BoardProps {
  children: React.ReactNode
  cols?: number
  className?: string
}

/**
 * Board: Handles slot math and positioning for unit placement
 * Features: Semicircular layout, precise positioning, responsive scaling
 */
export const Board: React.FC<BoardProps> = ({ 
  children, 
  cols = 7, 
  className = '' 
}) => {
  return (
    <div className={`relative w-full h-full ${className}`}>
      {/* Board visualization guides (development only) */}
      {false && process.env.NODE_ENV === 'development' && (
        <div className="absolute inset-0 pointer-events-none opacity-20">
          {/* Enemy semicircle guides */}
          <div className="absolute top-4 left-1/2 transform -translate-x-1/2">
            {[...Array(cols)].map((_, i) => {
              const pos = getSlotPosition(i, 'enemy', cols)
              return (
                <div
                  key={`enemy-guide-${i}`}
                  className="absolute w-16 h-16 border border-red-400 rounded-full flex items-center justify-center text-red-400 text-xs"
                  style={{
                    left: pos.x - 32,
                    top: pos.y - 32,
                    imageRendering: 'pixelated'
                  }}
                >
                  E{i}
                </div>
              )
            })}
          </div>
          
          {/* Player semicircle guides */}
          <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2">
            {[...Array(cols)].map((_, i) => {
              const pos = getSlotPosition(i, 'player', cols)
              return (
                <div
                  key={`player-guide-${i}`}
                  className="absolute w-16 h-16 border border-blue-400 rounded-full flex items-center justify-center text-blue-400 text-xs"
                  style={{
                    left: pos.x - 32,
                    top: pos.y - 32,
                    imageRendering: 'pixelated'
                  }}
                >
                  P{i}
                </div>
              )
            })}
          </div>
        </div>
      )}

      {/* Game content */}
      {children}
    </div>
  )
}

/**
 * Calculate precise slot position for unit placement
 * @param col - Column index (0 to cols-1)
 * @param side - 'player' or 'enemy'
 * @param totalCols - Total number of columns (default 7)
 * @returns {x, y} position with integer rounding for crisp pixels
 */
export const getSlotPosition = (
  col: number, 
  side: 'player' | 'enemy', 
  totalCols: number = 7
): { x: number; y: number } => {
  // Arena dimensions (relative to board container)
  const centerX = 192 // Half of 384px arena width
  const radius = 120   // Semicircle radius
  
  // Calculate angle for this slot
  const angleSpan = Math.PI // 180 degrees for semicircle
  const angleStep = angleSpan / (totalCols - 1)
  const angle = col * angleStep
  
  // Calculate position on semicircle
  let x, y
  
  if (side === 'enemy') {
    // Enemy slots: top semicircle (inverted)
    x = centerX + Math.cos(Math.PI - angle) * radius
    y = 60 - Math.sin(Math.PI - angle) * radius
  } else {
    // Player slots: bottom semicircle
    x = centerX + Math.cos(angle) * radius
    y = 60 + Math.sin(angle) * radius
  }
  
  // Round to integers for crisp pixel rendering
  return {
    x: Math.round(x),
    y: Math.round(y)
  }
}

/**
 * Get slot index from position (for drag & drop)
 */
export const getSlotFromPosition = (
  x: number, 
  y: number, 
  side: 'player' | 'enemy',
  totalCols: number = 7
): number => {
  let closestSlot = 0
  let closestDistance = Infinity
  
  for (let i = 0; i < totalCols; i++) {
    const slotPos = getSlotPosition(i, side, totalCols)
    const distance = Math.sqrt(
      Math.pow(x - slotPos.x, 2) + Math.pow(y - slotPos.y, 2)
    )
    
    if (distance < closestDistance) {
      closestDistance = distance
      closestSlot = i
    }
  }
  
  return closestSlot
}

/**
 * Check if position is valid drop zone
 */
export const isValidDropZone = (
  x: number, 
  y: number, 
  side: 'player' | 'enemy',
  totalCols: number = 7
): boolean => {
  const slot = getSlotFromPosition(x, y, side, totalCols)
  const slotPos = getSlotPosition(slot, side, totalCols)
  const distance = Math.sqrt(
    Math.pow(x - slotPos.x, 2) + Math.pow(y - slotPos.y, 2)
  )
  
  // Valid if within 48px of slot center
  return distance <= 48
}

export default Board