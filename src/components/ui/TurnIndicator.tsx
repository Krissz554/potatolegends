import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Clock, Crown, Sword } from 'lucide-react';

interface TurnIndicatorProps {
  isPlayerTurn: boolean;
  timeRemaining: number; // seconds (from server)
  onEndTurn?: () => void;
  className?: string;
}

export const TurnIndicator: React.FC<TurnIndicatorProps> = ({
  isPlayerTurn,
  timeRemaining,
  onEndTurn,
  className = ''
}) => {
  const [isAnimating, setIsAnimating] = useState(false);

  // Trigger animation when turn changes
  useEffect(() => {
    setIsAnimating(true);
    const timer = setTimeout(() => setIsAnimating(false), 1000);
    return () => clearTimeout(timer);
  }, [isPlayerTurn]);

  const getTimeColor = () => {
    if (timeRemaining > 20) return 'text-green-400';
    if (timeRemaining > 10) return 'text-yellow-400';
    return 'text-red-400';
  };

  const getTimeBorderColor = () => {
    if (timeRemaining > 20) return 'border-green-400/50';
    if (timeRemaining > 10) return 'border-yellow-400/50';
    return 'border-red-400/50';
  };

  return (
    <div className={`fixed top-4 right-4 z-50 ${className}`}>
      <AnimatePresence mode="wait">
        <motion.div
          key={isPlayerTurn ? 'player' : 'enemy'}
          initial={{ scale: 0.8, opacity: 0, y: -20 }}
          animate={{ scale: 1, opacity: 1, y: 0 }}
          exit={{ scale: 0.8, opacity: 0, y: 20 }}
          transition={{ duration: 0.5, ease: "easeOut" }}
          className="relative"
        >
          {/* Turn Indicator Card */}
          <div className={`
            rounded-xl border-2 shadow-2xl backdrop-blur-md px-6 py-4 min-w-[200px]
            ${isPlayerTurn 
              ? 'bg-gradient-to-br from-blue-500/20 to-cyan-500/20 border-blue-400/50' 
              : 'bg-gradient-to-br from-red-500/20 to-orange-500/20 border-red-400/50'
            }
          `}>
            
            {/* Turn Status */}
            <div className="flex items-center gap-3 mb-3">
              <motion.div
                animate={isAnimating ? { rotate: 360, scale: [1, 1.2, 1] } : {}}
                transition={{ duration: 0.6 }}
              >
                {isPlayerTurn ? (
                  <Crown className="w-6 h-6 text-blue-400" />
                ) : (
                  <Sword className="w-6 h-6 text-red-400" />
                )}
              </motion.div>
              
              <div>
                <h3 className={`
                  font-bold text-lg leading-none
                  ${isPlayerTurn ? 'text-blue-200' : 'text-red-200'}
                `} style={{ fontFamily: "'Press Start 2P', monospace" }}>
                  {isPlayerTurn ? 'YOUR TURN' : "ENEMY'S TURN"}
                </h3>
              </div>
            </div>

            {/* Timer */}
            <div className={`
              flex items-center gap-2 p-2 rounded-lg border
              ${getTimeBorderColor()}
              bg-black/30
            `}>
              <Clock className={`w-4 h-4 ${getTimeColor()}`} />
              <div className="flex-1">
                <div className={`
                  text-sm font-bold ${getTimeColor()}
                `} style={{ fontFamily: "'Press Start 2P', monospace" }}>
                  {Math.max(0, timeRemaining)}s
                </div>
                
                {/* Timer Progress Bar */}
                <div className="w-full bg-gray-700 rounded-full h-1.5 mt-1">
                  <motion.div
                    className={`
                      h-1.5 rounded-full transition-all duration-1000
                      ${timeRemaining > 20 ? 'bg-green-400' : 
                        timeRemaining > 10 ? 'bg-yellow-400' : 'bg-red-400'}
                    `}
                    style={{ width: `${(timeRemaining / 60) * 100}%` }}
                    animate={timeRemaining <= 10 ? { opacity: [1, 0.5, 1] } : {}}
                    transition={{ duration: 0.5, repeat: timeRemaining <= 10 ? Infinity : 0 }}
                  />
                </div>
              </div>
            </div>

            {/* End Turn Button */}
            {isPlayerTurn && onEndTurn && (
              <motion.button
                onClick={onEndTurn}
                className="
                  w-full mt-3 bg-gradient-to-r from-blue-600 to-cyan-600
                  hover:from-blue-700 hover:to-cyan-700
                  text-white font-bold py-2 px-4 rounded-lg
                  transition-all duration-200 transform hover:scale-105
                  border border-blue-400/50 shadow-lg
                "
                style={{ fontFamily: "'Press Start 2P', monospace" }}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
              >
                END TURN
              </motion.button>
            )}
          </div>

          {/* Glow Effect */}
          <div className={`
            absolute inset-0 rounded-xl blur-xl opacity-30 -z-10
            ${isPlayerTurn 
              ? 'bg-gradient-to-br from-blue-500 to-cyan-500' 
              : 'bg-gradient-to-br from-red-500 to-orange-500'
            }
          `} />
        </motion.div>
      </AnimatePresence>
    </div>
  );
};

export default TurnIndicator;