import { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import PixelPotato from "@/components/PixelPotato";
import { AuthModal } from "@/components/AuthModal";
import { UserMenu } from "@/components/UserMenu";

import PixelArena from "@/components/PixelArena";
import { UnlockAnimation } from "@/components/UnlockAnimation";
import { SettingsModal } from "@/components/SettingsModal";
import PersistentGameLayout from "@/components/PersistentGameLayout";
import { 
  Sparkles, 
  Users, 
  Trophy, 
  Swords, 
  Library, 
  Settings, 
  LogIn,
  Play,
  Shuffle,
  Star,
  Crown,
  Zap,
  Package,
  Gift,
  Plus
} from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { checkForActiveBattle, joinMatchmaking, leaveMatchmaking, stopMatchmakingListener, stopBackgroundMatchmakingPolling, initializeBattle, cleanupBrokenBattles, checkUserHasValidDeck, type BattleSession } from "@/lib/battleService";
import { generatePotatoFromRegistry, type RegistryPotato } from "@/lib/registryGenerator";
import { getUserCollection, unlockAllCardsForUser, getCollectionStats, type CollectionItem, type EnhancedCard } from "@/lib/newCollectionService";
import { RARITY_COLORS } from '@/lib/potatoData';
import HeroHall from "@/components/HeroHall";

// SEO JSON-LD
const JsonLd = () => (
  <script
    type="application/ld+json"
    dangerouslySetInnerHTML={{
      __html: JSON.stringify({
        "@context": "https://schema.org",
        "@type": "WebSite",
        name: "What's My Potato?",
        description:
          "Discover your hilarious potato persona and battle with friends in this epic potato card game!",
      }),
    }}
  />
);

const Index = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  
  // State management
  const [showAuth, setShowAuth] = useState(false);
  const [showCollection, setShowCollection] = useState(false);
  const [showHeroHall, setShowHeroHall] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [showBattleArena, setShowBattleArena] = useState(false);
  const [currentBattle, setCurrentBattle] = useState<BattleSession | null>(null);
  
  // Potato generation state
  const [currentPotato, setCurrentPotato] = useState<RegistryPotato | null>(null);
  const [generating, setGenerating] = useState(false);
  const [unlockAnimationData, setUnlockAnimationData] = useState<any>(null);
  const [showUnlockAnimation, setShowUnlockAnimation] = useState(false);
  const [justUnlocked, setJustUnlocked] = useState(false);
  
  // Battle state
  const [isInMatchmaking, setIsInMatchmaking] = useState(false);
  const [matchmakingStatus, setMatchmakingStatus] = useState('Looking for opponents...');
  const [hasValidDeck, setHasValidDeck] = useState(true);
  
  // Collection state
  const [collectionStats, setCollectionStats] = useState({
    totalCards: 0,
    uniqueCards: 0,
    completionPercentage: 0
  });

  // Refresh collection data
  const refreshCollection = async () => {
    if (!user) return;
    
    try {
      const result = await getCollectionStats(user.id);
      if (result.success && result.stats) {
        setCollectionStats({
          totalCards: result.stats.totalCards,
          uniqueCards: result.stats.uniqueCards,
          completionPercentage: result.stats.completionPercentage
        });
      }
    } catch (error) {
      console.error('Error refreshing collection:', error);
    }
  };

  // Load collection stats on mount and user change
  useEffect(() => {
    if (user) {
      refreshCollection();
    }
  }, [user]);

  // Generate random potato (disabled - using card system now)
  const generateRandomPotato = async () => {
    if (generating) return;
    
    console.log('🃏 Potato generation disabled - using card-based system now');
    // Old potato generation system disabled
    // Users now get cards through the collection system
  };

  // Test function to unlock all potatoes
  const handleUnlockAllPotatoes = async () => {
    if (!user) {
      toast.error('Please sign in to unlock potatoes');
      return;
    }

    try {
      toast.loading('Unlocking all cards...', { id: 'unlock-all' });
      
      const result = await unlockAllCardsForUser(user.id);
      
      if (result.success) {
        if (result.cardsUnlocked === 0) {
          toast.success('All cards already unlocked!', { id: 'unlock-all' });
        } else {
          toast.success(`🎉 Unlocked ${result.cardsUnlocked} cards! Check your collection.`, { id: 'unlock-all' });
        }
        
        // Refresh collection data
        await refreshCollection();
      } else {
        toast.error(result.error || 'Failed to unlock cards', { id: 'unlock-all' });
      }
      
    } catch (error) {
      console.error('Error unlocking all potatoes:', error);
      toast.error('An unexpected error occurred', { id: 'unlock-all' });
    }
  };

  // Handle battle matchmaking
  const handleStartBattle = async () => {
    if (!user) {
      setAuthModalOpen(true);
      return;
    }

    setIsInMatchmaking(true);
    try {
      const result = await joinMatchmaking();
      
      if (!result.success) {
        // Handle specific errors
        if (result.error?.includes('battle deck')) {
          toast.error(result.error);
        } else {
          toast.error(result.error || 'Failed to join matchmaking');
        }
        setIsInMatchmaking(false);
        return;
      }
      
      if (result.activeBattle) {
        // User already has an active battle, open it immediately
        setCurrentBattle(result.activeBattle);
        setShowBattleArena(true);
        setIsInMatchmaking(false);
        return;
      }

      toast.success('Searching for opponent...');
      
      // Set a timeout to cancel matchmaking after 60 seconds
      const matchmakingTimeout = setTimeout(async () => {
        console.log('⏰ Matchmaking timeout reached');
        await handleCancelMatchmaking();
        toast.info('Matchmaking timed out. Try again!');
      }, 60000);

      // Store timeout ID for cleanup
      (window as any).matchmakingTimeout = matchmakingTimeout;
      
      // Real-time events will handle battle found via the global event listener
      // No polling needed - everything is handled by real-time subscriptions

    } catch (error: any) {
      console.error('Error joining matchmaking:', error);
      toast.error('Failed to join matchmaking');
      setIsInMatchmaking(false);
    }
  };

  // Handle cancel matchmaking
  const handleCancelMatchmaking = async () => {
    setIsInMatchmaking(false);
    
    // Clear any existing timeout
    if ((window as any).matchmakingTimeout) {
      clearTimeout((window as any).matchmakingTimeout);
      (window as any).matchmakingTimeout = null;
    }
    
    try {
      await leaveMatchmaking();
      toast.info('Cancelled matchmaking');
    } catch (error) {
      console.error('Error leaving matchmaking:', error);
      toast.error('Failed to cancel matchmaking');
    }
  };

  // Handle unlock animation completion
  const handleUnlockAnimationComplete = () => {
    setShowUnlockAnimation(false);
    setUnlockAnimationData(null);
  };

  // Check for active battle and deck validity on page load/refresh
  useEffect(() => {
    const checkForExistingBattle = async () => {
      if (!user) {
        // Stop real-time listener if user is not authenticated
        stopMatchmakingListener();
        setHasValidDeck(false);
        return;
      }
      
      // Check if user has a valid deck
      const deckValid = await checkUserHasValidDeck(user.id);
      setHasValidDeck(deckValid);
      
      if (!deckValid) {
        console.log('⚠️ User does not have a valid deck for battles');
      }

      const { activeBattle } = await checkForActiveBattle();
      
      if (activeBattle) {
        setCurrentBattle(activeBattle);
        setShowBattleArena(true);
      }

      // Real-time listener will be started only when user joins matchmaking
      // Background polling is not needed unless actively matchmaking
    };

    checkForExistingBattle();

    // Cleanup on unmount
    return () => {
      // Always stop listeners on unmount to prevent memory leaks
      stopMatchmakingListener();
      stopBackgroundMatchmakingPolling();
    };
  }, [user]);

  // Initialize with a random potato (disabled - using card system now)
  useEffect(() => {
    // generateRandomPotato(); // Disabled - old potato system

  }, []);

  // Global event listener for real-time battle notifications
  useEffect(() => {
    const handleBattleFound = (event: CustomEvent) => {
      console.log('🎯 Battle found event received:', event.detail);
      
      // Handle both old and new event formats
      const activeBattle = event.detail?.activeBattle || event.detail;
      
      if (!activeBattle || !activeBattle.id) {
        console.log('❌ No valid battle in event detail:', { activeBattle, hasId: !!activeBattle?.id });
        return;
      }
      
      console.log('🏟️ Opening battle arena for battle:', activeBattle.id);
      
      // Clear matchmaking timeout
      if ((window as any).matchmakingTimeout) {
        clearTimeout((window as any).matchmakingTimeout);
        (window as any).matchmakingTimeout = null;
      }
      
      setCurrentBattle(activeBattle);
      setShowBattleArena(true);
      setIsInMatchmaking(false);
    };

    // Add event listener
    window.addEventListener('battleFound', handleBattleFound as EventListener);

    // Cleanup
    return () => {
      window.removeEventListener('battleFound', handleBattleFound as EventListener);
    };
  }, []);

  // Cleanup broken battles function
  const handleCleanupBrokenBattles = async () => {
    if (!user) return;
    
    try {
      toast.loading('Cleaning up broken battles...', { id: 'cleanup' });
      const result = await cleanupBrokenBattles();
      if (result.success) {
        toast.success(`✅ Cleaned up ${result.cleanedBattles || 0} broken battles!`, { id: 'cleanup' });
        // Close any open battle arena
        setShowBattleArena(false);
        setCurrentBattle(null);
      } else {
        toast.error(result.error || 'Failed to cleanup battles', { id: 'cleanup' });
      }
    } catch (error) {
      console.error('Error cleaning up battles:', error);
      toast.error('Failed to cleanup battles', { id: 'cleanup' });
    }
  };

  // Cleanup matchmaking on page unload
  useEffect(() => {
    const handleBeforeUnload = async () => {
      if (isInMatchmaking) {
        await leaveMatchmaking();
      }
    };

    const handleVisibilityChange = async () => {
      if (document.hidden && isInMatchmaking) {
        // User switched tabs or minimized browser while matchmaking
        // Keep them in queue but clear local timeout
        if ((window as any).matchmakingTimeout) {
          clearTimeout((window as any).matchmakingTimeout);
          (window as any).matchmakingTimeout = null;
        }
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [isInMatchmaking]);

  // If there's an active battle, render the full-page battle arena

  if (currentBattle && ['waiting_initialization', 'active', 'deploying', 'battling'].includes(currentBattle.status)) {
    return (
      <div className="h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 overflow-hidden">
        <PixelArena 
          battleSessionId={currentBattle.id}
        />
      </div>
    );
  }

  return (
    <PersistentGameLayout
      onStartBattle={handleStartBattle}
      onCancelMatchmaking={handleCancelMatchmaking}
      isInMatchmaking={isInMatchmaking}
      matchmakingStatus={matchmakingStatus}
      showUnlockAnimation={showUnlockAnimation}
      unlockAnimationData={unlockAnimationData}
      onUnlockAnimationComplete={() => setShowUnlockAnimation(false)}
    />
  );
};

export default Index;
