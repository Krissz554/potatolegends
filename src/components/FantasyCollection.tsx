import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { 
  Search, 
  Filter, 
  Star, 
  Grid3X3,
  List,
  Eye,
  EyeOff,
  Trophy,
  Sparkles,
  Crown,
  Zap,
  Package,
  Gift,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight
} from 'lucide-react';
import TradingCard from '@/components/ui/TradingCard';
import ElementIcon from '@/components/ui/ElementIcon';
import RarityIcon from '@/components/ui/RarityIcon';
import { getUserCollection, getCollectionStats, getAllCards, type CollectionItem, type EnhancedCard } from '@/lib/newCollectionService';
import { unlockAllCardsForUser } from '@/lib/newCollectionService';
import { useAuth } from '@/contexts/AuthContext';
import { toast } from 'sonner';

interface FantasyCollectionProps {
  onUnlock?: (data: any) => void;
}

export const FantasyCollection: React.FC<FantasyCollectionProps> = ({ onUnlock }) => {
  const { user } = useAuth();

  // Add custom scrollbar styles
  React.useEffect(() => {
    const style = document.createElement('style');
    style.textContent = `
      .custom-scrollbar {
        scrollbar-width: thin;
        scrollbar-color: rgba(148, 163, 184, 0.6) rgba(0, 0, 0, 0.1);
      }
      
      .custom-scrollbar::-webkit-scrollbar {
        width: 8px;
      }
      
      .custom-scrollbar::-webkit-scrollbar-track {
        background: rgba(0, 0, 0, 0.1);
        border-radius: 4px;
        border: 1px solid rgba(255, 255, 255, 0.1);
      }
      
      .custom-scrollbar::-webkit-scrollbar-thumb {
        background: linear-gradient(180deg, rgba(59, 130, 246, 0.8) 0%, rgba(147, 51, 234, 0.8) 100%);
        border-radius: 4px;
        border: 1px solid rgba(255, 255, 255, 0.2);
        transition: all 0.3s ease;
      }
      
      .custom-scrollbar::-webkit-scrollbar-thumb:hover {
        background: linear-gradient(180deg, rgba(59, 130, 246, 1) 0%, rgba(147, 51, 234, 1) 100%);
        box-shadow: 0 0 10px rgba(59, 130, 246, 0.5);
      }
      
      .custom-scrollbar::-webkit-scrollbar-corner {
        background: rgba(0, 0, 0, 0.1);
      }
    `;
    document.head.appendChild(style);
    
    return () => {
      document.head.removeChild(style);
    };
  }, []);
  
  // Collection state
  const [collection, setCollection] = useState<CollectionItem[]>([]);
  const [allCards, setAllCards] = useState<EnhancedCard[]>([]);
  const [filteredCards, setFilteredCards] = useState<EnhancedCard[]>([]);
  const [loading, setLoading] = useState(false);
  const [stats, setStats] = useState({
    totalCards: 0,
    uniqueCards: 0,
    completionPercentage: 0,
    exoticCards: 0,
    legendaryCards: 0,
    rareCards: 0
  });

  // UI state
  const [searchQuery, setSearchQuery] = useState('');
  const [rarityFilter, setRarityFilter] = useState('all');
  const [typeFilter, setTypeFilter] = useState('all');
  const [showOwnedOnly, setShowOwnedOnly] = useState(false);
  const [sortBy, setSortBy] = useState('name');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const cardsPerPage = 25;

  // Load collection data once - prevent reloads on page visibility changes
  useEffect(() => {
    if (!user) return;
    
    // Only load if we don't have data yet
    if (allCards.length > 0 || loading) return;

    const loadCollectionData = async () => {
      setLoading(true);
      try {
        const [collectionResult, cardsResult] = await Promise.all([
          getUserCollection(user.id),
          getAllCards()
        ]);

        // Extract data from service responses
        const collectionData = collectionResult?.data || collectionResult || [];
        const cardsData = cardsResult?.data || cardsResult || [];

        console.log('🔍 Collection result:', collectionResult);
        console.log('🔍 Cards result:', cardsResult);
        console.log('🔍 Collection data loaded:', collectionData.length, 'items');
        console.log('🔍 Cards data loaded:', cardsData.length, 'cards');

        // Ensure we have arrays and filter out invalid items
        const validCollection = Array.isArray(collectionData) ? collectionData.filter(item => {
          if (!item) return false;
          // Check if we have either a card object with id or direct card_id
          const hasValidCard = (item.card && item.card.id) || item.card_id;
          if (!hasValidCard) {
            console.warn('⚠️ Invalid collection item:', item);
          }
          return hasValidCard;
        }) : [];
        
        const validCards = Array.isArray(cardsData) ? cardsData.filter(card => {
          if (!card || !card.id) {
            console.warn('⚠️ Invalid card:', card);
            return false;
          }
          return true;
        }) : [];
        
        console.log('✅ Valid collection items:', validCollection.length);
        console.log('✅ Valid cards:', validCards.length);
        
        setCollection(validCollection);
        setAllCards(validCards);
      } catch (error) {
        console.error('Error loading collection:', error);
        toast.error('Failed to load collection');
      } finally {
        setLoading(false);
      }
    };

    loadCollectionData();
  }, [user, allCards.length, loading]);

  // Recalculate stats when collection or cards change
  useEffect(() => {
    if (Array.isArray(allCards) && Array.isArray(collection)) {
      const calculatedStats = calculateStats();
      setStats(calculatedStats);
      console.log('📊 Calculated stats:', calculatedStats);
    }
  }, [collection, allCards]);

  // Filter and sort cards
  useEffect(() => {
    // Ensure allCards is an array before filtering
    if (!Array.isArray(allCards) || allCards.length === 0) {
      setFilteredCards([]);
      return;
    }
    
    let filtered = [...allCards];

    // Search filter
    if (searchQuery) {
      filtered = filtered.filter(card => 
        card.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        card.description.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    // Rarity filter
    if (rarityFilter !== 'all') {
      filtered = filtered.filter(card => card.rarity === rarityFilter);
    }

    // Type filter
    if (typeFilter !== 'all') {
      filtered = filtered.filter(card => card.potato_type === typeFilter);
    }

    // Owned filter
    if (showOwnedOnly) {
      filtered = filtered.filter(card => getOwnedQuantity(card.id) > 0);
    }

    // Sort
    filtered.sort((a, b) => {
      switch (sortBy) {
        case 'name':
          return a.name.localeCompare(b.name);
        case 'rarity':
          const rarityOrder = { common: 0, uncommon: 1, rare: 2, legendary: 3, exotic: 4 };
          const getRarityScore = (card: EnhancedCard) => {
            return rarityOrder[card.rarity] || 0;
          };
          return getRarityScore(b) - getRarityScore(a);
        case 'mana_cost':
          return a.mana_cost - b.mana_cost;
        case 'type':
          return a.card_type.localeCompare(b.card_type);
        default:
          return 0;
      }
    });

    setFilteredCards(filtered);
    
    // Reset to first page when filters change
    setCurrentPage(1);
  }, [allCards, collection, searchQuery, rarityFilter, typeFilter, showOwnedOnly, sortBy]);

  // Get paginated cards
  const totalPages = Math.ceil(filteredCards.length / cardsPerPage);
  const startIndex = (currentPage - 1) * cardsPerPage;
  const endIndex = startIndex + cardsPerPage;
  const paginatedCards = filteredCards.slice(startIndex, endIndex);

  // Pagination navigation functions
  const goToPage = (page: number) => {
    setCurrentPage(Math.max(1, Math.min(page, totalPages)));
  };

  const nextPage = () => goToPage(currentPage + 1);
  const prevPage = () => goToPage(currentPage - 1);

  // Get owned quantity for a card
  const getOwnedQuantity = (cardId: string): number => {
    if (!Array.isArray(collection) || !cardId) return 0;
    
    const collectionItem = collection.find(item => {
      // Handle both nested card object and direct card_id
      const itemCardId = item?.card?.id || item?.card_id;
      return itemCardId === cardId;
    });
    
    return collectionItem?.quantity || 0;
  };

  // Calculate stats from loaded data
  const calculateStats = () => {
    if (!Array.isArray(allCards) || !Array.isArray(collection)) {
      return {
        totalCards: 0,
        uniqueCards: 0,
        completionPercentage: 0,
        exoticCards: 0,
        legendaryCards: 0,
        rareCards: 0
      };
    }

    const totalCards = collection.reduce((sum, item) => sum + (item.quantity || 0), 0);
    const uniqueCards = collection.length;
    const totalAvailableCards = allCards.length;
    const completionPercentage = totalAvailableCards > 0 ? Math.round((uniqueCards / totalAvailableCards) * 100) : 0;
    
    const exoticCards = collection.filter(item => 
      item.card?.exotic === true || item.card?.is_exotic === true || item.card?.rarity === 'exotic'
    ).length;
    
    const legendaryCards = collection.filter(item => 
      item.card?.is_legendary === true || item.card?.rarity === 'legendary'
    ).length;
    
    const rareCards = collection.filter(item => 
      item.card?.rarity === 'rare'
    ).length;

    return {
      totalCards,
      uniqueCards,
      completionPercentage,
      exoticCards,
      legendaryCards,
      rareCards
    };
  };

  // Handle unlock all cards
  const handleUnlockAllCards = async () => {
    if (!user) return;

    try {
      setLoading(true);
      const result = await unlockAllCardsForUser(user.id);
      
      if (result.success) {
        toast.success(`Unlocked ${result.unlockedCount} new cards!`);
        
        // Trigger unlock animation if callback provided
        if (onUnlock && result.unlockedCards && Array.isArray(result.unlockedCards) && result.unlockedCards.length > 0) {
          onUnlock({
            type: 'multiple',
            cards: result.unlockedCards
          });
        }

        // Reload collection data
        const collectionResult = await getUserCollection(user.id);
        const collectionData = collectionResult?.data || collectionResult || [];
        setCollection(Array.isArray(collectionData) ? collectionData : []);
        // Stats will be automatically recalculated by the useEffect
      }
    } catch (error) {
      console.error('Error unlocking cards:', error);
      toast.error('Failed to unlock cards');
    } finally {
      setLoading(false);
    }
  };

  // Rarity color mapping for fantasy theme
  const getRarityGradient = (rarity: string) => {
    switch (rarity) {
      case 'common': return 'from-gray-500/20 to-gray-600/20';
      case 'uncommon': return 'from-green-500/20 to-emerald-600/20';
      case 'rare': return 'from-blue-500/20 to-cyan-600/20';
      case 'legendary': return 'from-yellow-500/20 to-orange-600/20';
      case 'exotic': return 'from-purple-500/20 to-pink-600/20';
      default: return 'from-gray-500/20 to-gray-600/20';
    }
  };

  const getRarityTextColor = (rarity: string) => {
    switch (rarity) {
      case 'common': return 'text-gray-300';
      case 'uncommon': return 'text-green-300';
      case 'rare': return 'text-blue-300';
      case 'legendary': return 'text-yellow-300';
      case 'exotic': return 'text-purple-300';
      default: return 'text-gray-300';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <motion.div
          animate={{ rotate: 360 }}
          transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
          className="w-16 h-16 border-4 border-blue-400/30 border-t-blue-400 rounded-full"
        />
        <span className="ml-4 text-white text-lg">Loading your collection...</span>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col p-4 space-y-4">
      {/* Stats Overview - Compact */}
      <div className="grid grid-cols-4 gap-3">
        {[
          { label: 'Unique Cards', value: stats.uniqueCards, icon: Package, color: 'blue' },
          { label: 'Total Cards', value: stats.totalCards, icon: Star, color: 'green' },
          { label: 'Collection', value: `${stats.completionPercentage}%`, icon: Trophy, color: 'purple' },
          { label: 'Exotics', value: stats.exoticCards, icon: Crown, color: 'orange' }
        ].map((stat, index) => (
          <motion.div
            key={stat.label}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.1 }}
            className={`
              bg-gradient-to-br ${getRarityGradient('common')}
              backdrop-blur-sm border border-white/20 rounded-lg p-3
              hover:border-white/30 transition-all duration-300
            `}
          >
            <div className="flex items-center justify-between mb-2">
              <stat.icon className={`w-6 h-6 ${getRarityTextColor(stat.color)}`} />
              <span className="text-2xl font-bold text-white">{stat.value}</span>
            </div>
            <p className="text-gray-300 text-sm font-medium">{stat.label}</p>
            {stat.label === 'Collection' && (
              <div className="mt-2 h-2 bg-black/20 rounded-full overflow-hidden">
                <motion.div
                  initial={{ width: 0 }}
                  animate={{ width: `${stats.completionPercentage}%` }}
                  transition={{ delay: 0.5, duration: 1 }}
                  className="h-full bg-gradient-to-r from-purple-500 to-pink-500"
                />
              </div>
            )}
          </motion.div>
        ))}
      </div>

      {/* Controls Bar - Compact */}
      <div className="bg-black/20 backdrop-blur-sm border border-white/20 rounded-lg p-4">
        <div className="flex flex-wrap items-center gap-4 mb-4">
          {/* Search */}
          <div className="relative flex-1 min-w-64">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            <Input
              placeholder="Search cards..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10 bg-white/10 border-white/20 text-white placeholder:text-gray-400 focus:border-blue-400"
            />
          </div>

          {/* Unlock All Button */}
          <motion.button
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            onClick={handleUnlockAllCards}
            disabled={loading}
            className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-yellow-600 to-orange-600 text-white font-medium rounded-lg border border-yellow-500/30 hover:border-yellow-400/50 transition-all duration-300 disabled:opacity-50"
          >
            <Gift className="w-4 h-4" />
            Unlock All Cards
          </motion.button>
        </div>

        <div className="flex flex-wrap items-center gap-4">
          {/* Filters */}
          <Select value={rarityFilter} onValueChange={setRarityFilter}>
            <SelectTrigger className="w-40 bg-white/10 border-white/20 text-white">
              <SelectValue placeholder="Rarity" />
            </SelectTrigger>
            <SelectContent className="bg-gray-900/95 backdrop-blur-sm border-white/30 text-white">
              <SelectItem value="all" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <Filter className="w-4 h-4" />
                  All Rarities
                </div>
              </SelectItem>
              <SelectItem value="common" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <RarityIcon rarity="common" size={16} />
                  Common
                </div>
              </SelectItem>
              <SelectItem value="uncommon" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <RarityIcon rarity="uncommon" size={16} />
                  Uncommon
                </div>
              </SelectItem>
              <SelectItem value="rare" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <RarityIcon rarity="rare" size={16} />
                  Rare
                </div>
              </SelectItem>
              <SelectItem value="legendary" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <RarityIcon rarity="legendary" size={16} />
                  Legendary
                </div>
              </SelectItem>
              <SelectItem value="exotic" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <RarityIcon rarity="exotic" size={16} />
                  Exotic
                </div>
              </SelectItem>
            </SelectContent>
          </Select>

          <Select value={typeFilter} onValueChange={setTypeFilter}>
            <SelectTrigger className="w-40 bg-white/10 border-white/20 text-white">
              <SelectValue placeholder="Type" />
            </SelectTrigger>
            <SelectContent className="bg-gray-900/95 backdrop-blur-sm border-white/30 text-white">
              <SelectItem value="all" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <Sparkles className="w-4 h-4" />
                  All Types
                </div>
              </SelectItem>
              <SelectItem value="fire" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <ElementIcon element="fire" size={16} />
                  Fire
                </div>
              </SelectItem>
              <SelectItem value="ice" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <ElementIcon element="ice" size={16} />
                  Ice
                </div>
              </SelectItem>
              <SelectItem value="lightning" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <ElementIcon element="lightning" size={16} />
                  Lightning
                </div>
              </SelectItem>
              <SelectItem value="light" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <ElementIcon element="light" size={16} />
                  Light
                </div>
              </SelectItem>
              <SelectItem value="void" className="text-white hover:bg-white/10 focus:bg-white/10">
                <div className="flex items-center gap-2">
                  <ElementIcon element="void" size={16} />
                  Void
                </div>
              </SelectItem>
            </SelectContent>
          </Select>

          <Select value={sortBy} onValueChange={setSortBy}>
            <SelectTrigger className="w-40 bg-white/10 border-white/20 text-white">
              <SelectValue placeholder="Sort by" />
            </SelectTrigger>
            <SelectContent className="bg-gray-900/95 backdrop-blur-sm border-white/30 text-white">
              <SelectItem value="name" className="text-white hover:bg-white/10 focus:bg-white/10">Name</SelectItem>
              <SelectItem value="rarity" className="text-white hover:bg-white/10 focus:bg-white/10">Rarity</SelectItem>
              <SelectItem value="mana_cost" className="text-white hover:bg-white/10 focus:bg-white/10">Mana Cost</SelectItem>
              <SelectItem value="type" className="text-white hover:bg-white/10 focus:bg-white/10">Type</SelectItem>
            </SelectContent>
          </Select>

          {/* Toggle Buttons */}
          <Button
            variant={showOwnedOnly ? "default" : "outline"}
            onClick={() => setShowOwnedOnly(!showOwnedOnly)}
            className={`
              ${showOwnedOnly ? 'bg-blue-600 hover:bg-blue-700' : 'bg-white/10 hover:bg-white/20'}
              border-white/20 text-white
            `}
          >
            {showOwnedOnly ? <Eye className="w-4 h-4 mr-2" /> : <EyeOff className="w-4 h-4 mr-2" />}
            {showOwnedOnly ? 'Owned Only' : 'Show All'}
          </Button>

          <Button
            variant="outline"
            onClick={() => setViewMode(viewMode === 'grid' ? 'list' : 'grid')}
            className="bg-white/10 hover:bg-white/20 border-white/20 text-white"
          >
            {viewMode === 'grid' ? <List className="w-4 h-4" /> : <Grid3X3 className="w-4 h-4" />}
          </Button>
        </div>
      </div>

      {/* Cards Display - Flexible Height */}
      <div className="flex-1 bg-black/10 backdrop-blur-sm border border-white/20 rounded-lg p-4 flex flex-col" style={{ minHeight: '800px' }}>
        <div className="flex items-center justify-between mb-6">
          <h3 className="text-xl font-bold text-white">
            {filteredCards.length} Cards {searchQuery && `matching "${searchQuery}"`}
            {totalPages > 1 && (
              <span className="text-sm font-normal text-gray-300 ml-2">
                (Page {currentPage} of {totalPages})
              </span>
            )}
          </h3>
          <div className="flex items-center gap-2 text-gray-300">
            <Filter className="w-4 h-4" />
            <span className="text-sm">
              {rarityFilter !== 'all' && `${rarityFilter} • `}
              {typeFilter !== 'all' && `${typeFilter} • `}
              {showOwnedOnly && 'Owned Only'}
            </span>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto custom-scrollbar" style={{ minHeight: '700px' }}>
          {filteredCards.length === 0 ? (
            <div className="text-center py-20">
              <Sparkles className="w-16 h-16 text-gray-400 mx-auto mb-4" />
              <h3 className="text-xl font-semibold text-gray-300 mb-2">No cards found</h3>
              <p className="text-gray-400">Try adjusting your filters or search term</p>
            </div>
          ) : (
            <motion.div
              layout
              className={`grid gap-8 p-4 ${
                viewMode === 'grid'
                  ? 'grid-cols-[repeat(auto-fit,minmax(320px,1fr))]'
                  : 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3'
              }`}
              style={{ minHeight: '600px' }}
            >
            <AnimatePresence>
              {paginatedCards.filter(card => card && card.id).map((card, index) => {
                if (!card || !card.id) return null;
                
                const owned = getOwnedQuantity(card.id);
                return (
                  <motion.div
                    key={card.id}
                    layout
                    initial={{ opacity: 0, scale: 0.8 }}
                    animate={{ opacity: 1, scale: 1 }}
                    exit={{ opacity: 0, scale: 0.8 }}
                    transition={{ delay: index * 0.02 }}
                    whileHover={{ scale: 1.05 }}
                    className={`
                      transition-all duration-300 w-full max-w-[360px] mx-auto
                      ${owned === 0 ? 'opacity-50 grayscale' : 'hover:drop-shadow-2xl'}
                    `}
                    style={{ transform: 'scale(0.95)' }}
                  >
                    <TradingCard
                      card={card}
                      owned={owned}
                      showDownloadButton={owned > 0} // Only show download for owned cards
                      className={`
                        ${owned === 0 ? 'border-gray-600' : 'border-white/20'}
                        backdrop-blur-sm hover:border-white/40 !w-full !max-w-none
                        !min-w-0 !h-auto !min-h-[500px] overflow-visible
                      `}
                    />
                  </motion.div>
                );
              })}
            </AnimatePresence>
            </motion.div>
          )}
        </div>

        {/* Pagination Controls */}
        {totalPages > 1 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="flex items-center justify-center mt-4 gap-2 flex-shrink-0"
          >
            {/* First Page */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => goToPage(1)}
              disabled={currentPage === 1}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 disabled:opacity-50"
            >
              <ChevronsLeft className="w-4 h-4" />
            </Button>

            {/* Previous Page */}
            <Button
              variant="outline"
              size="sm"
              onClick={prevPage}
              disabled={currentPage === 1}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 disabled:opacity-50"
            >
              <ChevronLeft className="w-4 h-4" />
              Previous
            </Button>

            {/* Page Numbers */}
            <div className="flex items-center gap-1">
              {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                let pageNum;
                if (totalPages <= 5) {
                  pageNum = i + 1;
                } else {
                  // Show pages around current page
                  const start = Math.max(1, currentPage - 2);
                  const end = Math.min(totalPages, start + 4);
                  pageNum = start + i;
                  if (pageNum > end) return null;
                }

                return (
                  <Button
                    key={pageNum}
                    variant={currentPage === pageNum ? "default" : "outline"}
                    size="sm"
                    onClick={() => goToPage(pageNum)}
                    className={`
                      w-10 h-10 p-0
                      ${currentPage === pageNum 
                        ? 'bg-blue-600 hover:bg-blue-700 text-white border-blue-500' 
                        : 'bg-white/10 border-white/20 text-white hover:bg-white/20'
                      }
                    `}
                  >
                    {pageNum}
                  </Button>
                );
              })}
            </div>

            {/* Next Page */}
            <Button
              variant="outline"
              size="sm"
              onClick={nextPage}
              disabled={currentPage === totalPages}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 disabled:opacity-50"
            >
              Next
              <ChevronRight className="w-4 h-4" />
            </Button>

            {/* Last Page */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => goToPage(totalPages)}
              disabled={currentPage === totalPages}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 disabled:opacity-50"
            >
              <ChevronsRight className="w-4 h-4" />
            </Button>
          </motion.div>
        )}
      </div>
    </div>
  );
};