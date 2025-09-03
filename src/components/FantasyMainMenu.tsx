import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { AuthModal } from '@/components/AuthModal';
import { UserMenu } from '@/components/UserMenu';
import { SettingsModal } from '@/components/SettingsModal';
import { 
  Swords, 
  Library, 
  Crown, 
  Trophy, 
  Users,
  Settings,
  Sparkles,
  Zap,
  Shield,
  Star,
  LogIn,
  Play
} from 'lucide-react';
import { toast } from 'sonner';

// Import main menu background image
import mainPixelBackground from '@/assets/pixel/art/main-pixel.jpg';

interface FantasyMainMenuProps {
  onStartBattle: () => void;
  onShowCollection: () => void;
  onShowHeroHall: () => void;
  isInMatchmaking: boolean;
  matchmakingStatus: string;
  hasValidDeck?: boolean;
}

export const FantasyMainMenu: React.FC<FantasyMainMenuProps> = ({
  onStartBattle,
  onShowCollection,
  onShowHeroHall,
  isInMatchmaking,
  matchmakingStatus,
  hasValidDeck = true
}) => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [showAuth, setShowAuth] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [hoveredMenuItem, setHoveredMenuItem] = useState<string | null>(null);



  // Menu items configuration
  const menuItems = [
    {
      id: 'collection',
      label: 'Collection',
      icon: Library,
      description: 'View your cards',
      action: onShowCollection,
      gradient: 'from-emerald-600 to-teal-700'
    },
    {
      id: 'deck-builder',
      label: 'Deck Builder',
      icon: Sparkles,
      description: 'Build your deck',
      action: () => navigate('/deck-builder'),
      gradient: 'from-blue-600 to-indigo-700'
    },
    {
      id: 'hero-hall',
      label: 'Hero Hall',
      icon: Crown,
      description: 'Choose your hero',
      action: onShowHeroHall,
      gradient: 'from-yellow-600 to-orange-700'
    },
    {
      id: 'leaderboards',
      label: 'Leaderboards',
      icon: Trophy,
      description: 'Global rankings',
      action: () => toast.info('Leaderboards coming soon!'),
      gradient: 'from-purple-600 to-pink-700'
    },
    {
      id: 'social',
      label: 'Social',
      icon: Users,
      description: 'Friends & guilds',
      action: () => toast.info('Social features coming soon!'),
      gradient: 'from-cyan-600 to-blue-700'
    }
  ];

  return (
    <div 
      className="min-h-screen relative overflow-hidden bg-cover bg-center bg-no-repeat select-none"
      style={{ backgroundImage: `url(${mainPixelBackground})` }}
    >



      {/* Main Content */}
      <div className="relative z-10 min-h-screen flex flex-col">
        
        {/* Top Navigation Bar */}
        <motion.nav
          initial={{ y: -100, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ duration: 0.8, ease: "easeOut" }}
          className="flex justify-between items-center p-6 bg-black/20 backdrop-blur-md border-b border-white/10"
        >
          {/* Logo/Title */}
          <motion.div
            whileHover={{ scale: 1.05 }}
            className="flex items-center space-x-3"
          >
            <div className="w-12 h-12 bg-gradient-to-br from-yellow-400 to-orange-500 rounded-full flex items-center justify-center text-2xl font-bold text-white shadow-lg">
              🥔
            </div>
            <div>
              <h1 className="text-2xl font-bold text-white" style={{ fontFamily: "'Cinzel', serif" }}>
                What's My Potato?
              </h1>
              <p className="text-sm text-gray-300">Trading Card Adventure</p>
            </div>
          </motion.div>

          {/* Menu Items */}
          <div className="hidden md:flex items-center space-x-2">
            {menuItems.map((item, index) => (
              <motion.button
                key={item.id}
                initial={{ y: -50, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: index * 0.1, duration: 0.5 }}
                whileHover={{ scale: 1.05, y: -2 }}
                whileTap={{ scale: 0.95 }}
                onMouseEnter={() => setHoveredMenuItem(item.id)}
                onMouseLeave={() => setHoveredMenuItem(null)}
                onClick={item.action}
                className="relative group"
              >
                <div className={`
                  px-4 py-2 rounded-lg bg-gradient-to-r ${item.gradient}
                  border border-white/20 shadow-lg backdrop-blur-sm
                  transition-all duration-300 hover:shadow-xl
                  flex items-center space-x-2
                `}>
                  <item.icon className="w-5 h-5 text-white" />
                  <span className="text-white font-medium">{item.label}</span>
                </div>
                
                {/* Hover tooltip */}
                <AnimatePresence>
                  {hoveredMenuItem === item.id && (
                    <motion.div
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: 10 }}
                      className="absolute top-full mt-2 left-1/2 transform -translate-x-1/2 z-50"
                    >
                      <div className="bg-black/80 text-white text-sm px-3 py-1 rounded-md backdrop-blur-sm border border-white/20">
                        {item.description}
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.button>
            ))}
          </div>

          {/* User Menu / Auth */}
          <div className="flex items-center space-x-3">
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={() => setShowSettings(true)}
              className="p-2 bg-gray-700/50 hover:bg-gray-600/50 rounded-lg backdrop-blur-sm border border-white/20 transition-colors"
            >
              <Settings className="w-5 h-5 text-white" />
            </motion.button>

            {user ? (
              <UserMenu />
            ) : (
              <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                onClick={() => setShowAuth(true)}
                className="flex items-center space-x-2 px-4 py-2 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg text-white font-medium shadow-lg border border-white/20 backdrop-blur-sm"
              >
                <LogIn className="w-4 h-4" />
                <span>Sign In</span>
              </motion.button>
            )}
          </div>
        </motion.nav>

        {/* Main Content Area */}
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
                <Star className="w-5 h-5" />
                <span className="text-lg font-medium">Forge Your Destiny • Command Your Army • Claim Victory</span>
                <Star className="w-5 h-5" />
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
                initial={{ scale: 0.8, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                className="mb-8"
              >
                <div className="bg-gradient-to-r from-blue-600/20 to-purple-600/20 border border-blue-400/30 rounded-xl p-6 backdrop-blur-sm">
                  <div className="flex items-center justify-center space-x-3 mb-3">
                    <motion.div
                      animate={{ rotate: 360 }}
                      transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
                    >
                      <Zap className="w-6 h-6 text-blue-400" />
                    </motion.div>
                    <span className="text-xl font-bold text-blue-300">Finding Opponent...</span>
                  </div>
                  <p className="text-gray-300">{matchmakingStatus}</p>
                </div>
              </motion.div>
            )}
          </div>
        </div>

        {/* Battle Button - Bottom Right */}
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
          
          {!user && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 1 }}
              className="absolute top-full mt-2 right-0 bg-black/80 text-white text-sm px-3 py-1 rounded-md backdrop-blur-sm border border-white/20 whitespace-nowrap"
            >
              Sign in to battle!
            </motion.div>
          )}
        </motion.div>

        {/* Mobile Menu (simplified for mobile) */}
        <div className="md:hidden fixed bottom-8 left-8 right-20 z-20">
          <div className="bg-black/80 backdrop-blur-md rounded-2xl border border-white/20 p-4">
            <div className="grid grid-cols-3 gap-3">
              {menuItems.slice(0, 3).map((item) => (
                <motion.button
                  key={item.id}
                  whileTap={{ scale: 0.95 }}
                  onClick={item.action}
                  className={`
                    flex flex-col items-center space-y-1 p-3 rounded-lg
                    bg-gradient-to-r ${item.gradient} text-white
                    border border-white/20 shadow-lg
                  `}
                >
                  <item.icon className="w-5 h-5" />
                  <span className="text-xs font-medium">{item.label}</span>
                </motion.button>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Modals */}
      {showAuth && (
        <AuthModal 
          open={showAuth} 
          onOpenChange={(open) => setShowAuth(open)} 
        />
      )}
      
      {showSettings && (
        <SettingsModal 
          isOpen={showSettings} 
          onClose={() => setShowSettings(false)} 
        />
      )}
    </div>
  );
};