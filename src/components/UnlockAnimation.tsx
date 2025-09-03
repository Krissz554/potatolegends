import React, { useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import PixelPotato from '@/components/PixelPotato';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { RARITY_COLORS, type Rarity } from '@/lib/potatoData';
import { PartyPopper, Star } from 'lucide-react';

interface UnlockAnimationProps {
  isVisible: boolean;
  onComplete: () => void;
  potatoData: {
    name: string;
    rarity: Rarity;
    seed: string;
    adjective: string;
    type: string;
    trait: string;
    description?: string;
    hp?: number;
    attack?: number;
  };
}

export const UnlockAnimation: React.FC<UnlockAnimationProps> = ({ 
  isVisible, 
  onComplete, 
  potatoData 
}) => {
  const [stage, setStage] = useState<'floating' | 'collecting' | 'complete'>('floating');

  useEffect(() => {
    if (!isVisible) return;

    const timer1 = setTimeout(() => setStage('collecting'), 1500);
    const timer2 = setTimeout(() => setStage('complete'), 2500);
    const timer3 = setTimeout(() => onComplete(), 3000);

    return () => {
      clearTimeout(timer1);
      clearTimeout(timer2);
      clearTimeout(timer3);
    };
  }, [isVisible, onComplete]);

  const rarityColorData = RARITY_COLORS[potatoData.rarity] || RARITY_COLORS.common;

  return (
    <AnimatePresence>
      {isVisible && (
        <>
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center"
          >
            {/* Celebration particles */}
            <div className="absolute inset-0 overflow-hidden pointer-events-none">
              {[...Array(20)].map((_, i) => (
                <motion.div
                  key={i}
                  initial={{ 
                    opacity: 0,
                    scale: 0,
                    x: Math.random() * window.innerWidth,
                    y: window.innerHeight + 100
                  }}
                  animate={{ 
                    opacity: [0, 1, 0],
                    scale: [0, 1, 0],
                    y: -100,
                    rotate: 360
                  }}
                  transition={{ 
                    duration: 3,
                    delay: i * 0.1,
                    ease: "easeOut"
                  }}
                  className="absolute"
                >
                  {i % 3 === 0 ? (
                    <PartyPopper className="w-6 h-6 text-yellow-400" />
                  ) : i % 3 === 1 ? (
                    <Star className="w-4 h-4 text-blue-400" />
                  ) : (
                    <div className="w-2 h-2 bg-purple-400 rounded-full" />
                  )}
                </motion.div>
              ))}
            </div>

            {/* Main card animation */}
            <motion.div
              initial={{ 
                scale: 0.5, 
                opacity: 0, 
                y: 50,
                rotateY: -15,
              }}
              animate={
                stage === 'floating' 
                  ? { 
                      scale: 1, 
                      opacity: 1, 
                      y: 0,
                      rotateY: 0,
                      rotateX: [0, 5, -5, 0],
                      transition: { 
                        duration: 0.8,
                        rotateX: { 
                          duration: 2, 
                          repeat: Infinity, 
                          ease: "easeInOut" 
                        }
                      }
                    }
                  : stage === 'collecting'
                  ? {
                      scale: 0.3,
                      opacity: 0.8,
                      x: window.innerWidth * 0.4,
                      y: -window.innerHeight * 0.3,
                      rotateY: 15,
                      transition: { 
                        duration: 0.8,
                        ease: "easeInOut"
                      }
                    }
                  : {
                      scale: 0,
                      opacity: 0,
                      transition: { duration: 0.3 }
                    }
              }
              className="relative"
            >
              <Card 
                className="w-80 bg-white/95 backdrop-blur-md shadow-2xl"
                style={{ 
                  borderColor: rarityColorData.border,
                  borderWidth: '3px',
                  boxShadow: `0 0 30px ${rarityColorData.glow}40`
                }}
              >
                <div className="p-6 text-center space-y-4">
                  {/* Rarity badge */}
                  <motion.div
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ delay: 0.3, duration: 0.5, type: "spring" }}
                    className="flex justify-center"
                  >
                    <Badge
                      variant="secondary"
                      className="text-sm font-bold px-3 py-1"
                      style={{
                        backgroundColor: rarityColorData.bg,
                        color: rarityColorData.text,
                        borderColor: rarityColorData.border,
                      }}
                    >
                      {potatoData.rarity.toUpperCase()}
                    </Badge>
                  </motion.div>

                  {/* Potato visual */}
                  <motion.div
                    initial={{ scale: 0, rotate: -180 }}
                    animate={{ scale: 1, rotate: 0 }}
                    transition={{ delay: 0.5, duration: 0.6, type: "spring" }}
                    className="flex justify-center"
                  >
                    <div className="w-32 h-32 bg-gradient-to-br from-amber-50 to-amber-100 rounded-2xl flex items-center justify-center shadow-xl border-2 border-amber-200">
                      <PixelPotato 
                        seed={potatoData.seed} 
                        potatoType={potatoData.type}
                        traitHints={[potatoData.trait]}
                        size={120} 
                      />
                    </div>
                  </motion.div>

                  {/* Potato name */}
                  <motion.h3
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.7, duration: 0.5 }}
                    className="text-xl font-bold text-gray-800"
                  >
                    {potatoData.name}
                  </motion.h3>

                  {/* Battle Stats */}
                  {potatoData.hp && potatoData.attack && (
                    <motion.div
                      initial={{ opacity: 0, scale: 0.8 }}
                      animate={{ opacity: 1, scale: 1 }}
                      transition={{ delay: 0.8, duration: 0.5 }}
                      className="flex justify-center gap-3"
                    >
                      <div className="flex items-center gap-1 px-3 py-1 bg-red-100 text-red-700 rounded-lg border border-red-300">
                        <span className="text-sm">⚔️</span>
                        <div className="text-center">
                          <div className="text-xs font-medium opacity-75">ATK</div>
                          <div className="text-sm font-bold">{potatoData.attack}</div>
                        </div>
                      </div>
                      <div className="flex items-center gap-1 px-3 py-1 bg-green-100 text-green-700 rounded-lg border border-green-300">
                        <span className="text-sm">❤️</span>
                        <div className="text-center">
                          <div className="text-xs font-medium opacity-75">HP</div>
                          <div className="text-sm font-bold">{potatoData.hp}</div>
                        </div>
                      </div>
                    </motion.div>
                  )}

                  {/* Unlock message */}
                  <motion.div
                    initial={{ opacity: 0, scale: 0.8 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ delay: 0.9, duration: 0.5 }}
                    className="text-center"
                  >
                    <div className="text-lg font-semibold text-green-600 mb-1">
                      🎉 NEW POTATO DISCOVERED!
                    </div>
                    <div className="text-sm text-gray-600">
                      Added to your collection
                    </div>
                  </motion.div>
                </div>
              </Card>

              {/* Glow effect */}
              <motion.div
                initial={{ opacity: 0, scale: 0.8 }}
                animate={{ 
                  opacity: [0, 0.6, 0], 
                  scale: [0.8, 1.2, 1.4],
                }}
                transition={{ 
                  duration: 2, 
                  repeat: Infinity,
                  ease: "easeInOut"
                }}
                className="absolute inset-0 rounded-lg -z-10"
                style={{
                  background: `radial-gradient(circle, ${rarityColorData.glow}30 0%, transparent 70%)`,
                }}
              />
            </motion.div>

            {/* Collection indicator */}
            {stage === 'collecting' && (
              <motion.div
                initial={{ opacity: 0, scale: 0 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0 }}
                className="absolute top-4 right-4 bg-green-500 text-white px-4 py-2 rounded-full shadow-lg flex items-center gap-2"
              >
                <PartyPopper className="w-5 h-5" />
                <span className="font-medium">Added to Collection!</span>
              </motion.div>
            )}
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
};