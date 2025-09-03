import React from 'react';

// Import card back image
import backgroundCard from '@/assets/pixel/cards/background-card.png';

interface CardBackProps {
  className?: string;
  onClick?: () => void;
}

const CardBack: React.FC<CardBackProps> = ({ className = '', onClick }) => {
  return (
    <div 
      className={`
        relative rounded-2xl cursor-pointer transition-all duration-300 
        hover:scale-105 hover:shadow-2xl bg-cover bg-center bg-no-repeat
        border-4 border-gray-600 shadow-gray-600/30
        w-72 h-96 overflow-hidden
        ${className}
      `}
      style={{ 
        aspectRatio: '2/3',
        backgroundImage: `url(${backgroundCard})`
      }}
      onClick={onClick}
    >
      {/* Optional overlay for hover effects */}
      <div className="absolute inset-0 bg-black/10 hover:bg-black/5 transition-all duration-300 rounded-2xl"></div>
    </div>
  );
};

export default CardBack;