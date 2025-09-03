import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { 
  Search, 
  Filter, 
  Plus, 
  Minus, 
  Save, 
  Trash2, 
  Eye,
  Sparkles,
  Sword,
  Shield,
  Crown,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  Gift,
  RotateCcw
} from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/contexts/AuthContext';
import { supabase } from '@/lib/supabase';
import { getUserCollection, getAllCards, unlockAllCardsForUser, type CollectionItem, type EnhancedCard } from '@/lib/newCollectionService';
import TradingCard from '@/components/ui/TradingCard';
import RarityIcon from '@/components/ui/RarityIcon';
import ElementIcon from '@/components/ui/ElementIcon';

interface DeckCard {
  card: EnhancedCard;
  quantity: number;
}

interface Deck {
  id: string;
  name: string;
  is_active: boolean;
  cards: DeckCard[];
  total_cards: number;
  created_at: string;
  updated_at: string;
}

interface ValidationResult {
  valid: boolean;
  errors: string[];
  total_cards: number;
}

export const FantasyDeckBuilder: React.FC = () => {
  const { user } = useAuth();
  const { toast } = useToast();

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

  // State
  const [collection, setCollection] = useState<CollectionItem[]>([]);
  const [allCards, setAllCards] = useState<EnhancedCard[]>([]);
  const [filteredCards, setFilteredCards] = useState<EnhancedCard[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [rarityFilter, setRarityFilter] = useState<string>('all');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [showOwnedOnly, setShowOwnedOnly] = useState(false);

  // Deck state
  const [decks, setDecks] = useState<Deck[]>([]);
  const [currentDeck, setCurrentDeck] = useState<Deck | null>(null);
  const [deckCards, setDeckCards] = useState<DeckCard[]>([]);
  const [showCreateDeck, setShowCreateDeck] = useState(false);
  const [newDeckName, setNewDeckName] = useState('');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const cardsPerPage = 25;

  // Helper functions
  const getOwnedQuantity = (cardId: string): number => {
    if (!Array.isArray(collection) || !cardId) return 0;
    
    const collectionItem = collection.find(item => {
      // Handle both nested card object and direct card_id
      const itemCardId = item?.card?.id || item?.card_id;
      return itemCardId === cardId;
    });
    
    return collectionItem?.quantity || 0;
  };

  const getCardInDeck = (cardId: string): number => {
    return deckCards.find(d => d.card.id === cardId)?.quantity || 0;
  };

  const canAddCard = (card: EnhancedCard): boolean => {
    const owned = getOwnedQuantity(card.id);
    const inDeck = getCardInDeck(card.id);
    const currentTotal = deckCards.reduce((sum, deck_card) => sum + deck_card.quantity, 0);
    
    if (owned === 0) return false;
    
    // Check 30-card deck limit
    if (currentTotal >= 30) return false;
    
    // Deck rules based on rarity:
    // Common: 3 copies, Uncommon: 2 copies, Rare: 2 copies, Legendary: 1 copy, Exotic: 1 copy
    const getMaxCopiesByRarity = (rarity: string): number => {
      switch (rarity) {
        case 'common': return 3;
        case 'uncommon': return 2;
        case 'rare': return 2;
        case 'legendary': return 1;
        case 'exotic': return 1;
        default: return 2; // fallback for unknown rarities
      }
    };
    
    const maxInDeck = getMaxCopiesByRarity(card.rarity);
    
    return inDeck < maxInDeck && inDeck < owned;
  };

  const validateDeck = (): ValidationResult => {
    const total = deckCards.reduce((sum, deck_card) => sum + deck_card.quantity, 0);
    const errors: string[] = [];

    if (total !== 30) {
      errors.push(`Deck must have exactly 30 cards (currently ${total})`);
    }

    // Check copy limits for each card
    const getMaxCopiesByRarity = (rarity: string): number => {
      switch (rarity) {
        case 'common': return 3;
        case 'uncommon': return 2;
        case 'rare': return 2;
        case 'legendary': return 1;
        case 'exotic': return 1;
        default: return 2;
      }
    };

    deckCards.forEach(deckCard => {
      const maxAllowed = getMaxCopiesByRarity(deckCard.card.rarity);
      if (deckCard.quantity > maxAllowed) {
        errors.push(`${deckCard.card.name} (${deckCard.card.rarity}): ${deckCard.quantity} copies exceeds limit of ${maxAllowed}`);
      }
    });

    return {
      valid: errors.length === 0,
      errors,
      total_cards: total
    };
  };

  // Function to refresh collection data
  const refreshCollectionData = async () => {
    if (!user) return;
    
    setLoading(true);
    try {
      const collectionResult = await getUserCollection(user.id);
      const collectionData = Array.isArray(collectionResult?.data) ? collectionResult.data : 
                           Array.isArray(collectionResult) ? collectionResult : [];
      
      const validCollection = collectionData.filter(item => item && (item.card?.id || item.card_id));
      setCollection(validCollection);
      
      console.log('🔄 Deck Builder - Collection refreshed:', validCollection.length, 'items');
      
      toast({
        title: "Collection Updated",
        description: `Collection refreshed with ${validCollection.length} items`,
      });
    } catch (error) {
      console.error('Error refreshing collection:', error);
      toast({
        title: "Error",
        description: "Failed to refresh collection",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  // Function to unlock all cards
  const handleUnlockAllCards = async () => {
    if (!user) return;

    setLoading(true);
    try {
      const result = await unlockAllCardsForUser(user.id);
      
      if (result.success) {
        toast({
          title: "Cards Unlocked!",
          description: `Successfully unlocked ${result.unlockedCount || 'all'} cards!`,
        });
        
        // Refresh the collection data
        await refreshCollectionData();
      } else {
        throw new Error(result.error || 'Failed to unlock cards');
      }
    } catch (error: any) {
      console.error('Error unlocking cards:', error);
      toast({
        title: "Error",
        description: error.message || 'Failed to unlock cards',
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  // Load data
  useEffect(() => {
    const loadData = async () => {
      if (!user || allCards.length > 0) return;
      
      setLoading(true);
      try {
        const [collectionResult, cardsResult] = await Promise.all([
          getUserCollection(user.id),
          getAllCards()
        ]);

        const collectionData = Array.isArray(collectionResult?.data) ? collectionResult.data : 
                             Array.isArray(collectionResult) ? collectionResult : [];
        const cardsData = Array.isArray(cardsResult?.data) ? cardsResult.data : 
                         Array.isArray(cardsResult) ? cardsResult : [];

        console.log('🔍 Deck Builder - Collection Result:', collectionResult);
        console.log('📊 Deck Builder - Collection Data:', collectionData);
        console.log('🃏 Deck Builder - Cards Data Count:', cardsData.length);
        
        // Filter out invalid items and test ownership lookup
        const validCollection = collectionData.filter(item => item && (item.card?.id || item.card_id));
        const validCards = cardsData.filter(card => card && card.id);
        
        console.log('✅ Valid collection items:', validCollection.length);
        console.log('✅ Valid cards:', validCards.length);
        console.log('🔢 Deck Builder - Sample owned quantities:', 
          validCards.slice(0, 5).map(card => {
            const collectionItem = validCollection.find(item => {
              const itemCardId = item?.card?.id || item?.card_id;
              return itemCardId === card.id;
            });
            return {
              id: card.id,
              name: card.name,
              owned: collectionItem?.quantity || 0
            };
          })
        );

        setCollection(validCollection);
        setAllCards(validCards);
        setFilteredCards(validCards);
      } catch (error) {
        console.error('Error loading data:', error);
        toast({
          title: "Error",
          description: "Failed to load cards and collection",
          variant: "destructive"
        });
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [user]);

  // Load decks
  useEffect(() => {
    const loadDecks = async () => {
      if (!user) return;

      try {
        const { data, error } = await supabase
          .from('decks')
          .select('*')
          .eq('user_id', user.id)
          .order('created_at', { ascending: false });

        if (error) throw error;
        setDecks(data || []);
      } catch (error) {
        console.error('Error loading decks:', error);
      }
    };

    loadDecks();
  }, [user]);

  // Filter cards
  useEffect(() => {
    let filtered = [...allCards];

    // Search filter
    if (searchQuery) {
      filtered = filtered.filter(card =>
        card.name?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        card.description?.toLowerCase().includes(searchQuery.toLowerCase())
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

    // Owned only filter
    if (showOwnedOnly) {
      filtered = filtered.filter(card => getOwnedQuantity(card.id) > 0);
    }

    setFilteredCards(filtered);
    setCurrentPage(1);
  }, [allCards, collection, searchQuery, rarityFilter, typeFilter, showOwnedOnly]);

  // Pagination calculations
  const totalPages = Math.ceil(filteredCards.length / cardsPerPage);
  const startIndex = (currentPage - 1) * cardsPerPage;
  const endIndex = startIndex + cardsPerPage;
  const paginatedCards = filteredCards.slice(startIndex, endIndex);

  const goToPage = (page: number) => {
    setCurrentPage(Math.max(1, Math.min(page, totalPages)));
  };

  // Deck management functions
  const addCardToDeck = async (card: EnhancedCard) => {
    if (!currentDeck || !canAddCard(card)) return;

    const existingCard = deckCards.find(d => d.card.id === card.id);
    const newDeckCards = existingCard
      ? deckCards.map(d => d.card.id === card.id ? { ...d, quantity: d.quantity + 1 } : d)
      : [...deckCards, { card, quantity: 1 }];

    setDeckCards(newDeckCards);
    
    toast({
      title: "Card Added",
      description: `Added ${card.name} to deck`,
    });
  };

  const removeCardFromDeck = async (card: EnhancedCard) => {
    if (!currentDeck) return;

    const existingCard = deckCards.find(d => d.card.id === card.id);
    if (!existingCard) return;

    const newDeckCards = existingCard.quantity > 1
      ? deckCards.map(d => d.card.id === card.id ? { ...d, quantity: d.quantity - 1 } : d)
      : deckCards.filter(d => d.card.id !== card.id);

    setDeckCards(newDeckCards);
    
    toast({
      title: "Card Removed",
      description: `Removed ${card.name} from deck`,
    });
  };

  const createDeck = async () => {
    if (!user || !newDeckName.trim()) return;

    try {
      // Check if this will be the user's first deck
      const isFirstDeck = decks.length === 0;

      const { data, error } = await supabase
        .from('decks')
        .insert({
          user_id: user.id,
          name: newDeckName.trim(),
          is_active: isFirstDeck // Make first deck active automatically
        })
        .select()
        .single();

      if (error) throw error;

      const newDeck: Deck = {
        ...data,
        cards: [],
        total_cards: 0
      };

      setDecks([newDeck, ...decks]);
      setCurrentDeck(newDeck);
      setDeckCards([]);
      setNewDeckName('');
      setShowCreateDeck(false);

      toast({
        title: "Deck Created",
        description: `Created deck "${newDeckName}"${isFirstDeck ? ' and set as active for battles' : ''}`,
      });
    } catch (error) {
      console.error('Error creating deck:', error);
      toast({
        title: "Error",
        description: "Failed to create deck",
        variant: "destructive"
      });
    }
  };

  const saveDeck = async () => {
    if (!currentDeck || !user) return;

    const validation = validateDeck();
    if (!validation.valid) {
      toast({
        title: "Invalid Deck",
        description: validation.errors.join(', '),
        variant: "destructive"
      });
      return;
    }

    try {
      // Save deck cards
      const deckCardData = deckCards.map(dc => ({
        deck_id: currentDeck.id,
        card_id: dc.card.id,
        quantity: dc.quantity
      }));

      // Delete existing cards and insert new ones
      await supabase.from('deck_cards').delete().eq('deck_id', currentDeck.id);
      
      if (deckCardData.length > 0) {
        const { error } = await supabase.from('deck_cards').insert(deckCardData);
        if (error) throw error;
      }

      toast({
        title: "Deck Saved",
        description: `Saved "${currentDeck.name}" with ${validation.total_cards} cards`,
      });
    } catch (error) {
      console.error('Error saving deck:', error);
      toast({
        title: "Error",
        description: "Failed to save deck",
        variant: "destructive"
      });
    }
  };

  const loadDeck = async (deck: Deck) => {
    try {
      const { data, error } = await supabase
        .from('deck_cards')
        .select(`
          quantity,
          card_id,
          cards:card_complete(*)
        `)
        .eq('deck_id', deck.id);

      if (error) throw error;

      const loadedCards: DeckCard[] = (data || []).map(dc => ({
        card: dc.cards as EnhancedCard,
        quantity: dc.quantity
      }));

      setCurrentDeck(deck);
      setDeckCards(loadedCards);
      
      toast({
        title: "Deck Loaded",
        description: `Loaded "${deck.name}" with ${loadedCards.reduce((sum, dc) => sum + dc.quantity, 0)} cards`,
      });
    } catch (error) {
      console.error('Error loading deck:', error);
      toast({
        title: "Error",
        description: "Failed to load deck",
        variant: "destructive"
      });
    }
  };

  const setActiveDeck = async (deck: Deck) => {
    if (!user) return;

    try {
      // First, set all decks to inactive
      await supabase
        .from('decks')
        .update({ is_active: false })
        .eq('user_id', user.id);

      // Then set the selected deck as active
      const { error } = await supabase
        .from('decks')
        .update({ is_active: true })
        .eq('id', deck.id);

      if (error) throw error;

      // Update local state
      const updatedDecks = decks.map(d => ({
        ...d,
        is_active: d.id === deck.id
      }));
      setDecks(updatedDecks);

      // Update current deck if it's the one being activated
      if (currentDeck?.id === deck.id) {
        setCurrentDeck({ ...deck, is_active: true });
      }

      toast({
        title: "Active Deck Set",
        description: `"${deck.name}" is now your active deck for battles`,
      });
    } catch (error) {
      console.error('Error setting active deck:', error);
      toast({
        title: "Error",
        description: "Failed to set active deck",
        variant: "destructive"
      });
    }
  };

  const deleteDeck = async (deck: Deck) => {
    if (!user) return;

    try {
      // Delete deck cards first (foreign key constraint)
      await supabase.from('deck_cards').delete().eq('deck_id', deck.id);
      
      // Delete the deck itself
      const { error } = await supabase.from('decks').delete().eq('id', deck.id);
      
      if (error) throw error;

      // Update local state
      const updatedDecks = decks.filter(d => d.id !== deck.id);
      setDecks(updatedDecks);
      
      // If we deleted the current deck, clear it
      if (currentDeck?.id === deck.id) {
        setCurrentDeck(null);
        setDeckCards([]);
      }

      toast({
        title: "Deck Deleted",
        description: `Deleted deck "${deck.name}"`,
      });
    } catch (error) {
      console.error('Error deleting deck:', error);
      toast({
        title: "Error",
        description: "Failed to delete deck",
        variant: "destructive"
      });
    }
  };

  if (loading) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="text-center">
          <Sparkles className="w-12 h-12 text-blue-400 mx-auto mb-4 animate-spin" />
          <p className="text-white text-lg">Loading deck builder...</p>
        </div>
      </div>
    );
  }

  const validation = validateDeck();

  return (
    <div className="h-full flex flex-col p-4 space-y-4">
      {/* Top Section - Deck Management */}
      <div className="bg-black/20 backdrop-blur-sm rounded-lg border border-white/20 p-4">
        <div className="flex flex-wrap items-center gap-4 mb-4">
          <div className="flex items-center gap-2">
            <Sword className="w-5 h-5 text-blue-400" />
            <h2 className="text-xl font-bold text-white" style={{ fontFamily: "'Cinzel', serif" }}>
              Deck Builder
            </h2>
          </div>

          {/* Active Deck Info */}
          {decks.find(deck => deck.is_active) && (
            <div className="bg-yellow-900/20 border border-yellow-500/30 rounded-lg px-3 py-2 flex items-center gap-2">
              <Crown className="w-4 h-4 text-yellow-400" />
              <span className="text-yellow-200 text-sm">
                Active: <span className="font-semibold">{decks.find(deck => deck.is_active)?.name}</span>
              </span>
            </div>
          )}

          <Select value={currentDeck?.id || ''} onValueChange={(value) => {
            const deck = decks.find(d => d.id === value);
            if (deck) loadDeck(deck);
          }}>
            <SelectTrigger className="w-64 bg-gray-900/50 border-white/30 text-white">
              <SelectValue placeholder="Select a deck" />
            </SelectTrigger>
            <SelectContent className="bg-gray-900/95 backdrop-blur-sm border-white/30 text-white">
              {decks.map(deck => (
                <SelectItem 
                  key={deck.id} 
                  value={deck.id}
                  className="text-white hover:bg-white/10 focus:bg-white/10"
                >
                  <div className="flex items-center gap-2 w-full">
                    {deck.name}
                    {deck.is_active && (
                      <Badge className="bg-yellow-600/80 text-yellow-100 text-xs">
                        <Crown className="w-3 h-3 mr-1" />
                        Active
                      </Badge>
                    )}
                  </div>
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Dialog open={showCreateDeck} onOpenChange={setShowCreateDeck}>
            <DialogTrigger asChild>
              <Button className="bg-blue-600/80 hover:bg-blue-500 text-white border border-blue-400/50">
                <Plus className="w-4 h-4 mr-2" />
                New Deck
              </Button>
            </DialogTrigger>
            <DialogContent className="bg-gray-900/95 backdrop-blur-sm border-white/30 text-white">
              <DialogHeader>
                <DialogTitle className="text-white">Create New Deck</DialogTitle>
              </DialogHeader>
              <div className="space-y-4">
                <div>
                  <Label htmlFor="deck-name" className="text-white">Deck Name</Label>
                  <Input
                    id="deck-name"
                    value={newDeckName}
                    onChange={(e) => setNewDeckName(e.target.value)}
                    className="bg-gray-800/50 border-white/30 text-white placeholder-gray-400"
                    placeholder="Enter deck name..."
                  />
                </div>
                <div className="flex gap-2">
                  <Button onClick={createDeck} className="flex-1 bg-blue-600/80 hover:bg-blue-500">
                    Create
                  </Button>
                  <Button onClick={() => setShowCreateDeck(false)} variant="outline" className="flex-1 border-white/30 text-white hover:bg-white/10">
                    Cancel
                  </Button>
                </div>
              </div>
            </DialogContent>
          </Dialog>

          {currentDeck && (
            <>
              <Button 
                onClick={saveDeck} 
                disabled={!validation.valid}
                className={`border ${validation.valid ? 'bg-green-600/80 hover:bg-green-500 text-white border-green-400/50' : 'bg-gray-600/50 text-gray-400 border-gray-500/50 cursor-not-allowed'}`}
                title={validation.valid ? 'Save deck' : validation.errors.join('; ')}
              >
                <Save className="w-4 h-4 mr-2" />
                Save Deck
              </Button>

              {!currentDeck.is_active ? (
                <Button 
                  onClick={() => setActiveDeck(currentDeck)}
                  disabled={!validation.valid}
                  className={`border ${validation.valid ? 'bg-yellow-600/80 hover:bg-yellow-500 text-white border-yellow-400/50' : 'bg-gray-600/50 text-gray-400 border-gray-500/50 cursor-not-allowed'}`}
                  title={validation.valid ? 'Set as active deck for battles' : 'Fix deck issues before setting as active'}
                >
                  <Crown className="w-4 h-4 mr-2" />
                  Set as Active
                </Button>
              ) : (
                <Badge className="bg-yellow-600/80 text-yellow-100 flex items-center gap-1 px-3 py-2">
                  <Crown className="w-4 h-4" />
                  Active Battle Deck
                </Badge>
              )}
              
              <Button 
                onClick={() => {
                  if (confirm(`Are you sure you want to delete "${currentDeck.name}"? This action cannot be undone.`)) {
                    deleteDeck(currentDeck);
                  }
                }}
                className="bg-red-600/80 hover:bg-red-500 text-white border border-red-400/50"
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Delete Deck
              </Button>
              
              <div className="flex items-center gap-2">
                <Badge 
                  variant={validation.valid ? "default" : "destructive"}
                  className={validation.valid ? "bg-green-600/80" : ""}
                >
                  {validation.total_cards}/30 cards
                </Badge>
                {!validation.valid && (
                  <div className="text-red-400 text-sm">
                    {validation.errors[0]}
                  </div>
                )}
              </div>
            </>
          )}
        </div>

        {/* Deck Cards Display */}
        {currentDeck && deckCards.length > 0 && (
          <div className="bg-black/30 rounded-lg p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-white font-semibold flex items-center gap-2">
                <Shield className="w-5 h-5" />
                Current Deck - {currentDeck.name}
                {currentDeck.is_active && (
                  <Badge className="bg-yellow-600/80 text-yellow-100 text-xs">
                    <Crown className="w-3 h-3 mr-1" />
                    Active
                  </Badge>
                )}
              </h3>
              <Button
                onClick={() => {
                  if (confirm('Remove all cards from this deck?')) {
                    setDeckCards([]);
                    toast({
                      title: "Deck Cleared",
                      description: "Removed all cards from deck",
                    });
                  }
                }}
                size="sm"
                className="bg-orange-600/80 hover:bg-orange-500 text-white"
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Clear Deck
              </Button>
            </div>
            <div className="grid grid-cols-[repeat(auto-fill,minmax(240px,1fr))] gap-6 max-h-96 overflow-y-auto custom-scrollbar">
              {deckCards.map(({ card, quantity }) => (
                <motion.div
                  key={card.id}
                  initial={{ opacity: 0, scale: 0.8 }}
                  animate={{ opacity: 1, scale: 1 }}
                  whileHover={{ scale: 1.05 }}
                  className="transition-all duration-300 w-full max-w-[280px] mx-auto hover:drop-shadow-xl relative"
                >
                  <TradingCard
                    card={card}
                    owned={getOwnedQuantity(card.id)}
                    className="border-white/30 hover:border-white/60 !w-full !max-w-none aspect-[10/14] !min-w-0 !min-h-0"
                  />
                  <div className="absolute -top-2 -right-2 bg-blue-600 text-white text-base font-bold rounded-full w-10 h-10 flex items-center justify-center border-2 border-white shadow-lg z-10">
                    {quantity}
                  </div>
                  <div className="absolute -top-2 -left-2 flex flex-col gap-1 z-10">
                    <Button
                      size="sm"
                      onClick={() => removeCardFromDeck(card)}
                      className="w-8 h-8 p-0 bg-red-600/90 hover:bg-red-500 text-white shadow-lg"
                      title="Remove one copy"
                    >
                      <Minus className="w-4 h-4" />
                    </Button>
                  </div>
                </motion.div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Filters Section */}
      <div className="bg-black/20 backdrop-blur-sm rounded-lg border border-white/20 p-4">
        <div className="flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2">
            <Search className="w-4 h-4 text-gray-400" />
            <Input
              placeholder="Search cards..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-64 bg-gray-900/50 border-white/30 text-white placeholder-gray-400"
            />
          </div>

          <Select value={rarityFilter} onValueChange={setRarityFilter}>
            <SelectTrigger className="w-40 bg-gray-900/50 border-white/30 text-white">
              <SelectValue placeholder="All Rarities" />
            </SelectTrigger>
            <SelectContent className="bg-gray-900/95 backdrop-blur-sm border-white/30 text-white">
              <SelectItem value="all" className="text-white hover:bg-white/10 focus:bg-white/10">All Rarities</SelectItem>
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
            <SelectTrigger className="w-40 bg-gray-900/50 border-white/30 text-white">
              <SelectValue placeholder="All Types" />
            </SelectTrigger>
            <SelectContent className="bg-gray-900/95 backdrop-blur-sm border-white/30 text-white">
              <SelectItem value="all" className="text-white hover:bg-white/10 focus:bg-white/10">All Types</SelectItem>
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

          <Button
            onClick={() => setShowOwnedOnly(!showOwnedOnly)}
            variant={showOwnedOnly ? "default" : "outline"}
            className={showOwnedOnly 
              ? "bg-blue-600/80 hover:bg-blue-500 text-white border border-blue-400/50" 
              : "border-white/30 text-white hover:bg-white/10"
            }
          >
            <Eye className="w-4 h-4 mr-2" />
            {showOwnedOnly ? 'Owned Only' : 'Show All'}
          </Button>

          {/* Unlock All Button */}
          <Button
            onClick={handleUnlockAllCards}
            disabled={loading}
            className="bg-gradient-to-r from-yellow-600 to-orange-600 hover:from-yellow-700 hover:to-orange-700 text-white border border-yellow-500/30 disabled:opacity-50"
          >
            <Gift className="w-4 h-4 mr-2" />
            Unlock All Cards
          </Button>

          {/* Refresh Collection Button */}
          <Button
            onClick={refreshCollectionData}
            disabled={loading}
            variant="outline"
            className="border-white/30 text-white hover:bg-white/10 disabled:opacity-50"
          >
            <RotateCcw className="w-4 h-4 mr-2" />
            Refresh Collection
          </Button>
        </div>
      </div>

      {/* Cards Grid */}
      <div className="flex-1 overflow-y-auto custom-scrollbar min-h-0">
        {filteredCards.length === 0 ? (
          <div className="text-center py-20">
            <Sparkles className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h3 className="text-xl font-semibold text-gray-300 mb-2">No cards found</h3>
            <p className="text-gray-400">Try adjusting your filters or search term</p>
          </div>
        ) : (
          <motion.div
            layout
            className="grid grid-cols-[repeat(auto-fit,minmax(280px,1fr))] gap-6"
          >
            <AnimatePresence>
              {paginatedCards.map((card, index) => {
                const owned = getOwnedQuantity(card.id);
                const inDeck = getCardInDeck(card.id);
                const currentTotal = deckCards.reduce((sum, deck_card) => sum + deck_card.quantity, 0);
                const canAdd = canAddCard(card);
                
                // Get max copies allowed for this rarity
                const getMaxCopiesByRarity = (rarity: string): number => {
                  switch (rarity) {
                    case 'common': return 3;
                    case 'uncommon': return 2;
                    case 'rare': return 2;
                    case 'legendary': return 1;
                    case 'exotic': return 1;
                    default: return 2;
                  }
                };
                const maxCopies = getMaxCopiesByRarity(card.rarity);
                
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
                      transition-all duration-300 w-full max-w-[360px] mx-auto hover:drop-shadow-2xl
                      ${owned === 0 ? 'opacity-50 grayscale' : ''}
                    `}
                  >
                    <div className="relative">
                      <TradingCard
                        card={card}
                        owned={owned}
                        className={`
                          ${owned === 0 ? 'border-gray-600' : 'border-white/20'}
                          backdrop-blur-sm hover:border-white/40 !w-full !max-w-none aspect-[10/14] 
                          !min-w-0 !min-h-0
                        `}
                      />
                      
                      {/* Deck Actions */}
                      {currentDeck && (
                        <div className="absolute bottom-4 right-4 flex items-center gap-2">
                          {inDeck > 0 && (
                            <Button
                              size="sm"
                              onClick={() => removeCardFromDeck(card)}
                              className="w-8 h-8 p-0 bg-red-600/80 hover:bg-red-500 text-white"
                            >
                              <Minus className="w-4 h-4" />
                            </Button>
                          )}
                          
                          {inDeck > 0 && (
                            <Badge className="bg-blue-600/80 text-white">
                              {inDeck}
                            </Badge>
                          )}
                          
                          <Button
                            size="sm"
                            onClick={() => owned > 0 ? addCardToDeck(card) : null}
                            disabled={!canAdd || owned === 0}
                            className={`w-8 h-8 p-0 text-white ${
                              owned === 0 ? 'bg-gray-600/50 cursor-not-allowed' : 
                              canAdd ? 'bg-green-600/80 hover:bg-green-500' : 
                              'bg-gray-600/80 cursor-not-allowed'
                            }`}
                            title={owned === 0 ? 'You don\'t own this card' : 
                                   !canAdd ? (
                                     currentTotal >= 30 ? 'Deck is full (30/30 cards)' :
                                     `Cannot add more - ${card.rarity} cards limited to ${maxCopies} copies (${inDeck}/${maxCopies})`
                                   ) : 
                                   `Add to deck (${inDeck}/${maxCopies} copies)`}
                          >
                            <Plus className="w-4 h-4" />
                          </Button>
                        </div>
                      )}
                    </div>
                  </motion.div>
                );
              })}
            </AnimatePresence>
          </motion.div>
        )}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <motion.div
          layout
          className="flex items-center justify-center gap-2 py-4"
        >
          <Button
            onClick={() => goToPage(1)}
            disabled={currentPage === 1}
            size="sm"
            variant="outline"
            className="border-white/30 text-white hover:bg-white/10 disabled:opacity-50"
          >
            <ChevronsLeft className="w-4 h-4" />
          </Button>
          
          <Button
            onClick={() => goToPage(currentPage - 1)}
            disabled={currentPage === 1}
            size="sm"
            variant="outline"
            className="border-white/30 text-white hover:bg-white/10 disabled:opacity-50"
          >
            <ChevronLeft className="w-4 h-4" />
          </Button>

          <div className="flex items-center gap-1">
            {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
              const page = Math.max(1, Math.min(totalPages - 4, currentPage - 2)) + i;
              if (page > totalPages) return null;
              
              return (
                <Button
                  key={page}
                  onClick={() => goToPage(page)}
                  size="sm"
                  variant={currentPage === page ? "default" : "outline"}
                  className={currentPage === page 
                    ? "bg-blue-600/80 hover:bg-blue-500 text-white"
                    : "border-white/30 text-white hover:bg-white/10"
                  }
                >
                  {page}
                </Button>
              );
            })}
          </div>

          <Button
            onClick={() => goToPage(currentPage + 1)}
            disabled={currentPage === totalPages}
            size="sm"
            variant="outline"
            className="border-white/30 text-white hover:bg-white/10 disabled:opacity-50"
          >
            <ChevronRight className="w-4 h-4" />
          </Button>
          
          <Button
            onClick={() => goToPage(totalPages)}
            disabled={currentPage === totalPages}
            size="sm"
            variant="outline"
            className="border-white/30 text-white hover:bg-white/10 disabled:opacity-50"
          >
            <ChevronsRight className="w-4 h-4" />
          </Button>
          
          <span className="text-gray-400 text-sm ml-4">
            Page {currentPage} of {totalPages}
          </span>
        </motion.div>
      )}
    </div>
  );
};