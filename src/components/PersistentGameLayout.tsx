import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useLocation, useNavigate } from 'react-router-dom';
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

// Import page components
import FantasyMainMenuContent from '@/components/FantasyMainMenuContent';
import { FantasyCollection } from '@/components/FantasyCollection';
import { FantasyDeckBuilder } from '@/components/FantasyDeckBuilder';
import HeroHall from '@/components/HeroHall';
import { UnlockAnimation } from '@/components/UnlockAnimation';
import { checkUserHasValidDeck } from '@/lib/battleService';

type PageType = 'home' | 'collection' | 'deck-builder' | 'hero-hall';

interface PersistentGameLayoutProps {
  // Props from Index.tsx for battle functionality
  onStartBattle: () => void;
  onCancelMatchmaking?: () => void;
  isInMatchmaking: boolean;
  matchmakingStatus: string;
  showUnlockAnimation: boolean;
  unlockAnimationData: any;
  onUnlockAnimationComplete: () => void;
}

export const PersistentGameLayout: React.FC<PersistentGameLayoutProps> = ({
  onStartBattle,
  onCancelMatchmaking,
  isInMatchmaking,
  matchmakingStatus,
  showUnlockAnimation,
  unlockAnimationData,
  onUnlockAnimationComplete
}) => {
  const { user } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [showAuth, setShowAuth] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [hasValidDeck, setHasValidDeck] = useState(true);
  
  // Determine current page from URL
  const getCurrentPage = (): PageType => {
    const path = location.pathname;
    if (path === '/collection') return 'collection';
    if (path === '/deck-builder') return 'deck-builder';
    if (path === '/hero-hall') return 'hero-hall';
    return 'home';
  };
  
  const currentPage = getCurrentPage();

  // Check deck validity when user changes
  useEffect(() => {
    const checkDeckValidity = async () => {
      if (!user) {
        setHasValidDeck(false);
        return;
      }
      
      const deckValid = await checkUserHasValidDeck(user.id);
      setHasValidDeck(deckValid);
    };

    checkDeckValidity();
  }, [user]);

  // Menu items configuration
  const menuItems = [
    {
      id: 'home' as PageType,
      label: 'Home',
      icon: Home,
      gradient: 'from-slate-600 to-gray-700'
    },
    {
      id: 'collection' as PageType,
      label: 'Collection',
      icon: Library,
      gradient: 'from-emerald-600 to-teal-700'
    },
    {
      id: 'deck-builder' as PageType,
      label: 'Deck Builder',
      icon: Sparkles,
      gradient: 'from-blue-600 to-indigo-700'
    },
    {
      id: 'hero-hall' as PageType,
      label: 'Hero Hall',
      icon: Crown,
      gradient: 'from-yellow-600 to-amber-700'
    }
  ];

  const handleUnlock = (data: any) => {
    // This will be passed down to components that need it
  };

  const renderCurrentPage = () => {
    switch (currentPage) {
      case 'home':
        return (
          <FantasyMainMenuContent
            onStartBattle={onStartBattle}
            onCancelMatchmaking={onCancelMatchmaking}
            isInMatchmaking={isInMatchmaking}
            matchmakingStatus={matchmakingStatus}
            hasValidDeck={hasValidDeck}
          />
        );
      case 'collection':
        return (
          <div className="h-full flex flex-col">
            <FantasyCollection onUnlock={handleUnlock} />
          </div>
        );
      case 'deck-builder':
        return <FantasyDeckBuilder />;
      case 'hero-hall':
        return (
          <div className="container mx-auto px-4 py-8">
            <div className="mb-8">
              <h1 className="text-4xl font-bold text-white mb-2" style={{ fontFamily: "'Cinzel', serif" }}>
                Hero Hall
              </h1>
              <p className="text-gray-300 text-lg">
                Choose your champion for battle
              </p>
            </div>
            
            <div className="bg-white/5 backdrop-blur-sm rounded-2xl border border-white/10 p-6">
              <HeroHall standalone={true} />
            </div>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div 
      className="min-h-screen relative overflow-hidden bg-cover bg-center bg-no-repeat select-none"
      style={{ backgroundImage: `url(${mainPixelBackground})` }}
    >
      {/* Content */}
      <div className="relative z-10 min-h-screen flex flex-col">
        
        {/* Persistent Top Navigation Bar */}
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
                onClick={() => navigate(item.id === 'home' ? '/' : `/${item.id}`)}
                className="relative group"
              >
                <div className={`
                  px-4 py-2 rounded-lg bg-gradient-to-r ${item.gradient}
                  border ${currentPage === item.id ? 'border-white/40' : 'border-white/20'}
                  shadow-lg backdrop-blur-sm transition-all duration-300 hover:shadow-xl
                  flex items-center space-x-2
                  ${currentPage === item.id ? 'ring-2 ring-white/30' : ''}
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

        {/* Main Content Area - Changes based on current page */}
        <div className="flex-1 flex flex-col">
          {renderCurrentPage()}
        </div>
      </div>

      {/* Modals */}
      {showAuth && (
        <AuthModal open={showAuth} onOpenChange={setShowAuth} />
      )}

      {showSettings && (
        <SettingsModal open={showSettings} onOpenChange={setShowSettings} />
      )}

      {/* Unlock Animation */}
      {showUnlockAnimation && (
        <UnlockAnimation 
          isVisible={showUnlockAnimation}
          onComplete={onUnlockAnimationComplete}
        />
      )}
    </div>
  );
};

export default PersistentGameLayout;