import React from 'react';

interface ElementIconProps {
  element: 'ice' | 'fire' | 'lightning' | 'light' | 'void';
  size?: number;
  className?: string;
}

const ElementIcon: React.FC<ElementIconProps> = ({ element, size = 16, className = '' }) => {
  const iconStyle = {
    width: size,
    height: size,
    display: 'inline-block'
  };

  switch (element) {
    case 'ice':
      return (
        <svg style={iconStyle} className={className} viewBox="0 0 24 24" fill="none">
          {/* Simple ice crystal */}
          <path
            d="M12 4 L16 8 L12 12 L8 8 Z"
            fill="currentColor"
            opacity={0.8}
          />
          <path
            d="M12 12 L16 16 L12 20 L8 16 Z"
            fill="currentColor"
            opacity={0.6}
          />
          <path
            d="M4 12 L8 8 L12 12 L8 16 Z"
            fill="currentColor"
            opacity={0.4}
          />
          <path
            d="M12 12 L16 8 L20 12 L16 16 Z"
            fill="currentColor"
            opacity={0.4}
          />
          <circle cx={12} cy={12} r={1.5} fill="currentColor" />
        </svg>
      );
      
    case 'fire':
      return (
        <svg style={iconStyle} className={className} viewBox="0 0 24 24" fill="none">
          {/* Simple flame */}
          <path
            d="M12 20 C8 20 6 17 6 14 C6 11 8 9 10 8 C10.5 10 11.5 11 13 10 C14 11 18 12 18 16 C18 18.5 15 20 12 20 Z"
            fill="currentColor"
          />
          <path
            d="M12 17 C10 17 9 15.5 9 14 C9 12.5 10 11.5 11 11 C11.2 12 11.8 12.5 12.5 12 C13 12.5 15 13 15 15 C15 16.5 13.5 17 12 17 Z"
            fill="white"
            opacity={0.7}
          />
        </svg>
      );
      
    case 'lightning':
      return (
        <svg style={iconStyle} className={className} viewBox="0 0 24 24" fill="none">
          {/* Simple lightning bolt */}
          <path
            d="M13 3 L6 12 L10 12 L9 21 L16 12 L12 12 L13 3 Z"
            fill="currentColor"
          />
          <path
            d="M12.5 4 L8 10 L10.5 10 L9.5 18 L14 12 L11.5 12 L12.5 4 Z"
            fill="white"
            opacity={0.8}
          />
        </svg>
      );
      
    case 'light':
      return (
        <svg style={iconStyle} className={className} viewBox="0 0 24 24" fill="none">
          {/* Simple sun */}
          <circle cx={12} cy={12} r={4} fill="currentColor" />
          <circle cx={12} cy={12} r={2} fill="white" opacity={0.8} />
          <path
            d="M12 3 L12 5 M12 19 L12 21 M3 12 L5 12 M19 12 L21 12 M5.5 5.5 L7 7 M17 17 L18.5 18.5 M5.5 18.5 L7 17 M17 7 L18.5 5.5"
            stroke="currentColor"
            strokeWidth={2}
            strokeLinecap="round"
          />
        </svg>
      );
      
    case 'void':
      return (
        <svg style={iconStyle} className={className} viewBox="0 0 24 24" fill="none">
          {/* Simple void/dark orb */}
          <circle cx={12} cy={12} r={8} fill="currentColor" opacity={0.2} />
          <circle cx={12} cy={12} r={5} fill="currentColor" opacity={0.5} />
          <circle cx={12} cy={12} r={2.5} fill="currentColor" />
          <circle cx={10} cy={10} r={0.8} fill="white" opacity={0.3} />
        </svg>
      );
      
    default:
      return <div style={iconStyle} className={className}>?</div>;
  }
};

export default ElementIcon;