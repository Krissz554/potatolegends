import React from 'react'

interface RarityIconProps {
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary' | 'exotic'
  size?: number
  className?: string
}

export const RarityIcon: React.FC<RarityIconProps> = ({ 
  rarity, 
  size = 16, 
  className = '' 
}) => {
  const commonStyles = `inline-block ${className}`
  
  switch (rarity) {
    case 'exotic':
      return (
        <svg 
          width={size} 
          height={size} 
          viewBox="0 0 24 24" 
          className={commonStyles}
          style={{ color: '#8b5cf6' }}
        >
          {/* Exotic: Purple Diamond with Sparkles */}
          <path 
            d="M12 2L15.5 8.5L22 12L15.5 15.5L12 22L8.5 15.5L2 12L8.5 8.5Z" 
            fill="currentColor" 
            stroke="#a855f7" 
            strokeWidth="1"
          />
          <circle cx="6" cy="6" r="1" fill="#a855f7" opacity="0.8" />
          <circle cx="18" cy="6" r="1" fill="#a855f7" opacity="0.8" />
          <circle cx="6" cy="18" r="1" fill="#a855f7" opacity="0.8" />
          <circle cx="18" cy="18" r="1" fill="#a855f7" opacity="0.8" />
        </svg>
      )
    
    case 'legendary':
      return (
        <svg 
          width={size} 
          height={size} 
          viewBox="0 0 24 24" 
          className={commonStyles}
          style={{ color: '#f59e0b' }}
        >
          {/* Legendary: Golden Crown */}
          <path 
            d="M5 16L3 12L7 8L12 11L17 8L21 12L19 16L12 14L5 16Z" 
            fill="currentColor" 
            stroke="#d97706" 
            strokeWidth="1"
          />
          <circle cx="7" cy="8" r="1.5" fill="#fbbf24" />
          <circle cx="12" cy="6" r="1.5" fill="#fbbf24" />
          <circle cx="17" cy="8" r="1.5" fill="#fbbf24" />
        </svg>
      )
    
    case 'rare':
      return (
        <svg 
          width={size} 
          height={size} 
          viewBox="0 0 24 24" 
          className={commonStyles}
          style={{ color: '#3b82f6' }}
        >
          {/* Rare: Blue Gem */}
          <path 
            d="M12 2L16 8L12 14L8 8Z" 
            fill="currentColor" 
            stroke="#2563eb" 
            strokeWidth="1"
          />
          <path 
            d="M8 8L12 14L16 8L20 12L12 22L4 12Z" 
            fill="currentColor" 
            opacity="0.7"
            stroke="#2563eb" 
            strokeWidth="1"
          />
        </svg>
      )
    
    case 'uncommon':
      return (
        <svg 
          width={size} 
          height={size} 
          viewBox="0 0 24 24" 
          className={commonStyles}
          style={{ color: '#10b981' }}
        >
          {/* Uncommon: Green Shield */}
          <path 
            d="M12 3L20 7V13C20 17 16 21 12 21C8 21 4 17 4 13V7L12 3Z" 
            fill="currentColor" 
            stroke="#059669" 
            strokeWidth="1"
            opacity="0.8"
          />
          <path 
            d="M12 7L16 10V14C16 16 14 18 12 18C10 18 8 16 8 14V10L12 7Z" 
            fill="#34d399" 
            opacity="0.6"
          />
        </svg>
      )
    
    case 'common':
      return (
        <svg 
          width={size} 
          height={size} 
          viewBox="0 0 24 24" 
          className={commonStyles}
          style={{ color: '#6b7280' }}
        >
          {/* Common: Gray Circle */}
          <circle 
            cx="12" 
            cy="12" 
            r="8" 
            fill="currentColor" 
            stroke="#4b5563" 
            strokeWidth="1"
            opacity="0.7"
          />
          <circle 
            cx="12" 
            cy="12" 
            r="4" 
            fill="#9ca3af" 
            opacity="0.5"
          />
        </svg>
      )
    
    default:
      return null
  }
}

export default RarityIcon