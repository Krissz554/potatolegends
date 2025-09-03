import React from 'react'
import arenaDayImage from '@/assets/pixel/arena/day/arena-day.png'
import arenaNightImage from '@/assets/pixel/arena/night/arena-night.png'

interface ArenaStageProps {
  timeOfDay: 'day' | 'night'
  children: React.ReactNode
}

export const ArenaStage: React.FC<ArenaStageProps> = ({ timeOfDay, children }) => {
  const backgroundImage = timeOfDay === 'day' ? arenaDayImage : arenaNightImage

  return (
    <div className="absolute inset-0 overflow-hidden">
      {/* Fixed Background that doesn't change with zoom */}
      <img 
        src={backgroundImage}
        alt="Arena Background"
        className="fixed inset-0 z-0"
        style={{
          width: '100vw',
          height: '100vh',
          objectFit: 'contain',
          objectPosition: 'center',
          imageRendering: 'pixelated',
          maxWidth: '100%',
          maxHeight: '100%'
        }}
      />

      {/* Content Layer */}
      <div className="relative z-40">
        {children}
      </div>
    </div>
  )
}