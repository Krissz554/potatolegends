import React from 'react';

interface CustomManaIconProps {
  size?: number;
  className?: string;
}

const CustomManaIcon: React.FC<CustomManaIconProps> = ({ size = 24, className = '' }) => {
  return (
    <svg 
      width={size} 
      height={size} 
      className={className} 
      viewBox="0 0 24 24" 
      fill="none"
    >
      {/* Outer diamond/gem shape */}
      <path
        d="M12 2 L19 9 L12 22 L5 9 Z"
        fill="currentColor"
        opacity={0.9}
      />
      
      {/* Inner facets for depth */}
      <path
        d="M12 2 L16 7 L12 12 L8 7 Z"
        fill="currentColor"
        opacity={0.6}
      />
      <path
        d="M12 12 L19 9 L12 22 Z"
        fill="currentColor"
        opacity={0.4}
      />
      <path
        d="M12 12 L5 9 L12 22 Z"
        fill="currentColor"
        opacity={0.3}
      />
      
      {/* Top highlight */}
      <path
        d="M12 2 L14 5 L12 8 L10 5 Z"
        fill="white"
        opacity={0.8}
      />
      
      {/* Center shine */}
      <circle 
        cx={12} 
        cy={9} 
        r={1.5} 
        fill="white" 
        opacity={0.6} 
      />
    </svg>
  );
};

export default CustomManaIcon;