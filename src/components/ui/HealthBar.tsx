import React from 'react'

interface HealthBarProps {
  currentHp: number
  maxHp: number
  className?: string
}

export const HealthBar: React.FC<HealthBarProps> = ({ currentHp, maxHp, className = '' }) => {
  // Calculate how many segments should be filled
  const filledSegments = Math.max(0, Math.min(maxHp, currentHp))
  const emptySegments = maxHp - filledSegments
  
  // Create array of segments
  const segments = []
  
  // Add filled segments (green)
  for (let i = 0; i < filledSegments; i++) {
    segments.push({ filled: true, key: `filled-${i}` })
  }
  
  // Add empty segments (dark)
  for (let i = 0; i < emptySegments; i++) {
    segments.push({ filled: false, key: `empty-${i}` })
  }
  
  return (
    <div className={`flex w-full h-2 bg-black/60 rounded-sm border border-gray-600/50 overflow-hidden ${className}`}>
      {segments.map((segment, index) => (
        <div
          key={segment.key}
          className={`flex-1 transition-all duration-300 ${
            segment.filled 
              ? 'bg-green-500 shadow-sm' 
              : 'bg-red-900/40'
          }`}
          style={{
            borderRight: index < segments.length - 1 ? '1px solid rgba(0,0,0,0.3)' : 'none'
          }}
        />
      ))}
    </div>
  )
}