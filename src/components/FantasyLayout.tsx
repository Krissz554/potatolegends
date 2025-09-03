import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
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
  LogIn,
  Home
} from 'lucide-react';

// Import main menu background image
import mainPixelBackground from '@/assets/pixel/art/main-pixel.jpg';

interface FantasyLayoutProps {
  children: React.ReactNode;
  onStartBattle?: () => void;
  isInMatchmaking?: boolean;
  showBattleButton?: boolean;
}

export const FantasyLayout: React.FC<FantasyLayoutProps> = ({
  children,
  onStartBattle,
  isInMatchmaking = false,
  showBattleButton = true
}) => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [showAuth, setShowAuth] = useState(false);
  const [showSettings, setShowSettings] = useState(false);

  // Menu items configuration
  const menuItems = [
    {
      id: 'home',
      label: 'Home',
      icon: Home,
      path: '/',
      gradient: 'from-slate-600 to-gray-700'
    },
    {
      id: 'collection',
      label: 'Collection',
      icon: Library,
      path: '/collection',
      gradient: 'from-emerald-600 to-teal-700'
    },
    {
      id: 'deck-builder',
      label: 'Deck Builder',
      icon: Sparkles,
      path: '/deck-builder',
      gradient: 'from-blue-600 to-indigo-700'
    },
    {
      id: 'hero-hall',
      label: 'Hero Hall',
      icon: Crown,
      path: '/hero-hall',
      gradient: 'from-yellow-600 to-orange-700'
    },
    {
      id: 'leaderboards',
      label: 'Leaderboards',
      icon: Trophy,
      path: '#',
      gradient: 'from-purple-600 to-pink-700'
    }
  ];

  const isCurrentPath = (path: string) => {
    if (path === '/') return location.pathname === '/';
    return location.pathname.startsWith(path);
  };

  return (
    <div 
      className="min-h-screen relative overflow-hidden bg-cover bg-center bg-no-repeat select-none"
      style={{ backgroundImage: `url(${mainPixelBackground})` }}
    >

      {/* Content */}
      <div className="relative z-10 min-h-screen flex flex-col">
        
        {/* Top Navigation Bar */}
        <motion.nav
          initial={{ y: -100, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ duration: 0.8, ease: "easeOut" }}
          className="flex justify-between items-center p-6 bg-black/20 backdrop-blur-md border-b border-white/10 sticky top-0 z-50"
        >
          {/* Logo/Title */}
          <motion.div
            whileHover={{ scale: 1.05 }}
            className="flex items-center space-x-3 cursor-pointer"
            onClick={() => navigate('/')}
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
                onClick={() => {
                  if (item.path === '#') {
                    // Placeholder for future features
                    return;
                  }
                  navigate(item.path);
                }}
                className="relative group"
              >
                <div className={`
                  px-4 py-2 rounded-lg bg-gradient-to-r ${item.gradient}
                  border ${isCurrentPath(item.path) ? 'border-white/40' : 'border-white/20'}
                  shadow-lg backdrop-blur-sm transition-all duration-300 hover:shadow-xl
                  flex items-center space-x-2
                  ${isCurrentPath(item.path) ? 'ring-2 ring-white/30' : ''}
                `}>
                  <item.icon className="w-5 h-5 text-white" />
                  <span className="text-white font-medium">{item.label}</span>
                </div>
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

        {/* Main Content */}
        <div className="flex-1">
          {children}
        </div>

        {/* Battle Button - Bottom Right (only on main page) */}
        {showBattleButton && onStartBattle && (
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
              disabled={!user}
              className={`
                relative group overflow-hidden
                ${user 
                  ? 'bg-gradient-to-r from-red-600 via-orange-600 to-yellow-600' 
                  : 'bg-gray-600 cursor-not-allowed'
                }
                text-white font-bold text-2xl px-8 py-4 rounded-2xl
                shadow-2xl border-4 border-white/20 backdrop-blur-sm
                transition-all duration-300
                ${user ? 'hover:shadow-red-500/50 hover:border-red-400/50' : ''}
              `}
            >
              {/* Animated background */}
              {user && (
                <motion.div
                  className="absolute inset-0 bg-gradient-to-r from-white/0 via-white/20 to-white/0"
                  animate={{ x: ['-100%', '100%'] }}
                  transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
                />
              )}
              
              <div className="relative flex items-center space-x-3">
                <Swords className="w-8 h-8" />
                <span style={{ fontFamily: "'Cinzel', serif" }}>
                  {isInMatchmaking ? 'SEARCHING...' : 'BATTLE'}
                </span>
              </div>

              {/* Glow effect */}
              {user && !isInMatchmaking && (
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
        )}

        {/* Mobile Menu (simplified for mobile) */}
        <div className="md:hidden fixed bottom-8 left-8 right-20 z-20">
          <div className="bg-black/80 backdrop-blur-md rounded-2xl border border-white/20 p-4">
            <div className="grid grid-cols-4 gap-3">
              {menuItems.slice(0, 4).map((item) => (
                <motion.button
                  key={item.id}
                  whileTap={{ scale: 0.95 }}
                  onClick={() => {
                    if (item.path === '#') return;
                    navigate(item.path);
                  }}
                  className={`
                    flex flex-col items-center space-y-1 p-3 rounded-lg
                    bg-gradient-to-r ${item.gradient} text-white
                    border ${isCurrentPath(item.path) ? 'border-white/40' : 'border-white/20'}
                    shadow-lg ${isCurrentPath(item.path) ? 'ring-1 ring-white/30' : ''}
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