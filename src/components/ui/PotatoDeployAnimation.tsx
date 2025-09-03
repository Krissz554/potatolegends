import React, { useState, useEffect } from 'react';

interface PotatoDeployAnimationProps {
  spriteSheet: string;
  frameCount?: number;
  frameWidth?: number;
  frameHeight?: number;
  animationSpeed?: number;
  onComplete?: () => void;
  className?: string;
}

const PotatoDeployAnimation: React.FC<PotatoDeployAnimationProps> = ({
  spriteSheet,
  frameCount = 8,
  frameWidth = 64,
  frameHeight = 64,
  animationSpeed = 150,
  onComplete,
  className = ''
}) => {
  const [currentFrame, setCurrentFrame] = useState(0);
  const [isPlaying, setIsPlaying] = useState(true);

  useEffect(() => {
    if (!isPlaying) return;

    const interval = setInterval(() => {
      setCurrentFrame(prev => {
        const nextFrame = prev + 1;
        if (nextFrame >= frameCount) {
          setIsPlaying(false);
          onComplete?.();
          return 0; // Reset to first frame
        }
        return nextFrame;
      });
    }, animationSpeed);

    return () => clearInterval(interval);
  }, [isPlaying, frameCount, animationSpeed, onComplete]);

  const backgroundPositionX = -(currentFrame * frameWidth);

  return (
    <div 
      className={`${className}`}
      style={{
        width: frameWidth,
        height: frameHeight,
        backgroundImage: `url(${spriteSheet})`,
        backgroundPosition: `${backgroundPositionX}px 0px`,
        backgroundRepeat: 'no-repeat',
        imageRendering: 'pixelated'
      }}
    />
  );
};

export default PotatoDeployAnimation;