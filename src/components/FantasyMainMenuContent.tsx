import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '@/contexts/AuthContext';
import { Badge } from '@/components/ui/badge';
import { 
  Swords, 
  Shield,
  Sparkles,
  X
} from 'lucide-react';

interface FantasyMainMenuContentProps {
  onStartBattle: () => void;
  onCancelMatchmaking?: () => void;
  isInMatchmaking: boolean;
  matchmakingStatus: string;
  hasValidDeck?: boolean;
}

export const FantasyMainMenuContent: React.FC<FantasyMainMenuContentProps> = ({
  onStartBattle,
  onCancelMatchmaking,
  isInMatchmaking,
  matchmakingStatus,
  hasValidDeck = true
}) => {
  const { user } = useAuth();

  return (
    <div className="flex-1 flex items-center justify-center p-8">
      <div className="text-center max-w-4xl">
        
        {/* Game Title */}
        <motion.div
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ delay: 0.3, duration: 0.8 }}
          className="mb-12"
        >
          <h1 className="text-6xl md:text-8xl font-bold text-white mb-4 drop-shadow-2xl" style={{ fontFamily: "'Cinzel', serif" }}>
            <motion.span
              animate={{
                textShadow: [
                  '0 0 20px rgba(255, 215, 0, 0.5)',
                  '0 0 40px rgba(255, 215, 0, 0.8)',
                  '0 0 20px rgba(255, 215, 0, 0.5)'
                ]
              }}
              transition={{ duration: 3, repeat: Infinity }}
            >
              POTATO LEGENDS
            </motion.span>
          </h1>
          <p className="text-xl md:text-2xl text-gray-300 mb-8">
            Enter the mystical realm of trading card warfare
          </p>
          
          {/* Epic subtitle */}
          <motion.div
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ delay: 0.8, duration: 0.6 }}
            className="flex items-center justify-center space-x-2 text-yellow-400"
          >
            <Sparkles className="w-5 h-5" />
            <span className="text-lg font-medium">Forge Your Destiny • Command Your Army • Claim Victory</span>
            <Sparkles className="w-5 h-5" />
          </motion.div>
        </motion.div>

        {/* Status Messages */}
        {user && (
          <motion.div
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ delay: 1, duration: 0.6 }}
            className="mb-8"
          >
            <Badge className="bg-gradient-to-r from-green-600 to-emerald-700 text-white px-4 py-2 text-lg border border-green-400/30">
              <Shield className="w-4 h-4 mr-2" />
              Welcome back, {user.email?.split('@')[0]}!
            </Badge>
          </motion.div>
        )}

        {/* Matchmaking Status */}
        {isInMatchmaking && (
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.9, opacity: 0 }}
            className="mb-8 p-6 bg-blue-900/60 backdrop-blur-sm rounded-2xl border border-blue-400/50 shadow-2xl relative"
          >
            {/* Cancel Button (X) */}
            <motion.button
              whileHover={{ scale: 1.1 }}
              whileTap={{ scale: 0.9 }}
              onClick={onCancelMatchmaking}
              className="absolute top-3 right-3 w-8 h-8 bg-red-600/80 hover:bg-red-500 text-white rounded-full flex items-center justify-center transition-colors shadow-lg border border-red-400/50"
              title="Cancel matchmaking"
            >
              <X className="w-4 h-4" />
            </motion.button>

            <div className="flex items-center justify-center mb-4">
              <motion.div
                animate={{ rotate: 360 }}
                transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
                className="w-8 h-8 border-4 border-blue-400 border-t-transparent rounded-full mr-4"
              />
              <span className="text-xl font-bold text-blue-300">Finding Opponent...</span>
            </div>
            <p className="text-gray-300">{matchmakingStatus}</p>
          </motion.div>
        )}
      </div>

      {/* Battle Button */}
      <motion.div
        initial={{ x: 100, opacity: 0 }}
        animate={{ x: 0, opacity: 1 }}
        transition={{ delay: 0.5, duration: 0.8 }}
        className="fixed bottom-8 right-8 z-20"
      >
        <motion.button
          whileHover={{ scale: 1.1, y: -5 }}
          whileTap={{ scale: 0.95 }}
          onClick={onStartBattle}
          disabled={!user || !hasValidDeck}
          className={`
            relative group overflow-hidden
            ${user && hasValidDeck
              ? 'bg-gradient-to-r from-red-600 via-orange-600 to-yellow-600' 
              : 'bg-gray-600 cursor-not-allowed'
            }
            text-white font-bold text-2xl px-8 py-4 rounded-2xl
            shadow-2xl border-4 border-white/20 backdrop-blur-sm
            transition-all duration-300
            ${user && hasValidDeck ? 'hover:shadow-red-500/50 hover:border-red-400/50' : ''}
          `}
        >
          {/* Animated background */}
          {user && hasValidDeck && (
            <motion.div
              className="absolute inset-0 bg-gradient-to-r from-white/0 via-white/20 to-white/0"
              animate={{ x: ['-100%', '100%'] }}
              transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
            />
          )}
          
          <div className="relative flex items-center space-x-3">
            <Swords className="w-8 h-8" />
            <span style={{ fontFamily: "'Cinzel', serif" }}>
              {!user ? 'LOGIN' : 
               !hasValidDeck ? 'INVALID DECK' :
               isInMatchmaking ? 'SEARCHING...' : 'BATTLE'}
            </span>
          </div>

          {/* Glow effect */}
          {user && hasValidDeck && !isInMatchmaking && (
            <motion.div
              className="absolute inset-0 rounded-2xl"
              animate={{
                boxShadow: [
                  '0 0 20px rgba(239, 68, 68, 0.5)',
                  '0 0 40px rgba(239, 68, 68, 0.8)',
                  '0 0 20px rgba(239, 68, 68, 0.5)'
                ]
              }}
              transition={{ duration: 2, repeat: Infinity }}
            />
          )}
        </motion.button>
      </motion.div>
    </div>
  );
};

export default FantasyMainMenuContent;