import React from 'react'
import { cn } from '@/lib/utils'

interface ManaIconProps {
  className?: string
  size?: 'sm' | 'md' | 'lg'
}

export const ManaIcon: React.FC<ManaIconProps> = ({ 
  className, 
  size = 'md' 
}) => {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-5 h-5', 
    lg: 'w-6 h-6'
  }

  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={cn(sizeClasses[size], className)}
    >
      {/* Outer crystal shape */}
      <path
        d="M12 2L19 8L17 18L12 22L7 18L5 8L12 2Z"
        fill="url(#manaGradient)"
        stroke="url(#manaStroke)"
        strokeWidth="1.5"
        strokeLinejoin="round"
      />
      
      {/* Inner facets for crystal effect */}
      <path
        d="M12 2L7 8L12 12L17 8L12 2Z"
        fill="url(#manaHighlight)"
        opacity="0.8"
      />
      
      <path
        d="M7 8L12 12L7 18L7 8Z"
        fill="url(#manaShadow)"
        opacity="0.6"
      />
      
      <path
        d="M17 8L12 12L17 18L17 8Z"
        fill="url(#manaShadow)"
        opacity="0.4"
      />
      
      {/* Central spark */}
      <circle
        cx="12"
        cy="11"
        r="2"
        fill="url(#manaSpark)"
        opacity="0.9"
      />

      {/* Gradient definitions */}
      <defs>
        <linearGradient id="manaGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#3B82F6" />
          <stop offset="50%" stopColor="#1D4ED8" />
          <stop offset="100%" stopColor="#1E40AF" />
        </linearGradient>
        
        <linearGradient id="manaStroke" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#1E40AF" />
          <stop offset="100%" stopColor="#1E3A8A" />
        </linearGradient>
        
        <linearGradient id="manaHighlight" x1="0%" y1="0%" x2="0%" y2="100%">
          <stop offset="0%" stopColor="#DBEAFE" />
          <stop offset="100%" stopColor="#3B82F6" />
        </linearGradient>
        
        <linearGradient id="manaShadow" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#1E40AF" />
          <stop offset="100%" stopColor="#1E3A8A" />
        </linearGradient>
        
        <radialGradient id="manaSpark" cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="#FFFFFF" />
          <stop offset="50%" stopColor="#DBEAFE" />
          <stop offset="100%" stopColor="#3B82F6" />
        </radialGradient>
      </defs>
    </svg>
  )
}