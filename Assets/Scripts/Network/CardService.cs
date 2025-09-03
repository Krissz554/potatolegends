using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using PotatoCardGame.Data;

namespace PotatoCardGame.Network
{
    /// <summary>
    /// Handles all card-related API calls to Supabase
    /// Manages card collection, deck operations, and card database queries
    /// </summary>
    public class CardService : MonoBehaviour
    {
        public static CardService Instance { get; private set; }
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Cached data
        private List<GameCard> allCards = new List<GameCard>();
        private List<CollectionCard> userCollection = new List<CollectionCard>();
        private bool cardsLoaded = false;
        
        // Events
        public System.Action<List<GameCard>> OnAllCardsLoaded;
        public System.Action<List<CollectionCard>> OnUserCollectionLoaded;
        public System.Action<string> OnError;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #region Card Database Operations
        
        /// <summary>
        /// Load all cards from the card_complete table
        /// </summary>
        public async Task<List<GameCard>> LoadAllCards()
        {
            try
            {
                if (enableDebugLogs) Debug.Log("🃏 Loading all cards from database...");
                
                // Get all cards from card_complete table
                var cards = await SupabaseClient.Instance.GetData<List<GameCard>>("card_complete?select=*");
                
                if (cards != null && cards.Count > 0)
                {
                    allCards = cards;
                    cardsLoaded = true;
                    
                    if (enableDebugLogs) Debug.Log($"✅ Loaded {cards.Count} cards from database");
                    OnAllCardsLoaded?.Invoke(allCards);
                    
                    return allCards;
                }
                else
                {
                    Debug.LogWarning("⚠️ No cards found in database");
                    return new List<GameCard>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading cards: {e.Message}");
                OnError?.Invoke($"Failed to load cards: {e.Message}");
                return new List<GameCard>();
            }
        }
        
        /// <summary>
        /// Get user's card collection from database
        /// </summary>
        public async Task<List<CollectionCard>> LoadUserCollection(string userId)
        {
            try
            {
                if (enableDebugLogs) Debug.Log($"📚 Loading collection for user: {userId}");
                
                // Get user's collection with card details
                string query = $"user_collections?select=*,card_complete(*)&user_id=eq.{userId}";
                var collectionData = await SupabaseClient.Instance.GetData<List<dynamic>>(query);
                
                userCollection = new List<CollectionCard>();
                
                if (collectionData != null)
                {
                    foreach (var item in collectionData)
                    {
                        // Parse the collection item and associated card
                        // This would need proper JSON parsing based on Supabase response structure
                        // For now, we'll create a placeholder implementation
                    }
                }
                
                if (enableDebugLogs) Debug.Log($"✅ Loaded user collection: {userCollection.Count} unique cards");
                OnUserCollectionLoaded?.Invoke(userCollection);
                
                return userCollection;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading user collection: {e.Message}");
                OnError?.Invoke($"Failed to load collection: {e.Message}");
                return new List<CollectionCard>();
            }
        }
        
        /// <summary>
        /// Search cards by name, type, rarity, etc.
        /// </summary>
        public List<GameCard> SearchCards(string searchTerm, string rarityFilter = "", string typeFilter = "")
        {
            if (!cardsLoaded) return new List<GameCard>();
            
            var filteredCards = allCards.FindAll(card => {
                bool matchesSearch = string.IsNullOrEmpty(searchTerm) || 
                                   card.name.ToLower().Contains(searchTerm.ToLower()) ||
                                   card.description.ToLower().Contains(searchTerm.ToLower());
                
                bool matchesRarity = string.IsNullOrEmpty(rarityFilter) || 
                                    card.rarity.ToLower() == rarityFilter.ToLower();
                
                bool matchesType = string.IsNullOrEmpty(typeFilter) || 
                                  card.card_type.ToLower() == typeFilter.ToLower();
                
                return matchesSearch && matchesRarity && matchesType;
            });
            
            if (enableDebugLogs) Debug.Log($"🔍 Search '{searchTerm}' found {filteredCards.Count} cards");
            return filteredCards;
        }
        
        /// <summary>
        /// Get card by ID from loaded cards
        /// </summary>
        public GameCard GetCardById(string cardId)
        {
            return allCards.Find(card => card.id == cardId);
        }
        
        /// <summary>
        /// Get cards by rarity
        /// </summary>
        public List<GameCard> GetCardsByRarity(string rarity)
        {
            return allCards.FindAll(card => card.rarity.ToLower() == rarity.ToLower());
        }
        
        #endregion
        
        #region Collection Management
        
        /// <summary>
        /// Add card to user's collection
        /// </summary>
        public async Task<bool> AddCardToCollection(string userId, string cardId, int quantity = 1)
        {
            try
            {
                var collectionItem = new
                {
                    user_id = userId,
                    card_id = cardId,
                    quantity = quantity,
                    unlocked_at = DateTime.UtcNow
                };
                
                bool success = await SupabaseClient.Instance.PostData("user_collections", collectionItem);
                
                if (success)
                {
                    if (enableDebugLogs) Debug.Log($"✅ Added card {cardId} to collection");
                    // Refresh user collection
                    await LoadUserCollection(userId);
                }
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error adding card to collection: {e.Message}");
                OnError?.Invoke($"Failed to add card: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Check if user owns enough copies of a card
        /// </summary>
        public bool HasCard(string cardId, int requiredQuantity = 1)
        {
            var collectionCard = userCollection.Find(c => c.card.id == cardId);
            return collectionCard != null && collectionCard.quantity >= requiredQuantity;
        }
        
        /// <summary>
        /// Get user's quantity of a specific card
        /// </summary>
        public int GetCardQuantity(string cardId)
        {
            var collectionCard = userCollection.Find(c => c.card.id == cardId);
            return collectionCard?.quantity ?? 0;
        }
        
        #endregion
        
        #region Deck Operations
        
        /// <summary>
        /// Load user's decks from database
        /// </summary>
        public async Task<List<Deck>> LoadUserDecks(string userId)
        {
            try
            {
                if (enableDebugLogs) Debug.Log($"🃏 Loading decks for user: {userId}");
                
                // Get user's decks with card details
                string query = $"decks?select=*,deck_cards(quantity,card_complete(*))&user_id=eq.{userId}";
                var deckData = await SupabaseClient.Instance.GetData<List<dynamic>>(query);
                
                var decks = new List<Deck>();
                
                // Parse deck data (implementation depends on exact Supabase response structure)
                // This is a placeholder - would need actual JSON parsing
                
                if (enableDebugLogs) Debug.Log($"✅ Loaded {decks.Count} decks");
                return decks;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading decks: {e.Message}");
                OnError?.Invoke($"Failed to load decks: {e.Message}");
                return new List<Deck>();
            }
        }
        
        /// <summary>
        /// Save deck to database
        /// </summary>
        public async Task<bool> SaveDeck(string userId, Deck deck)
        {
            try
            {
                if (enableDebugLogs) Debug.Log($"💾 Saving deck: {deck.name}");
                
                // Create deck record
                var deckData = new
                {
                    user_id = userId,
                    name = deck.name,
                    is_active = deck.is_active,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };
                
                bool success = await SupabaseClient.Instance.PostData("decks", deckData);
                
                if (success && enableDebugLogs)
                {
                    Debug.Log($"✅ Deck saved successfully: {deck.name}");
                }
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error saving deck: {e.Message}");
                OnError?.Invoke($"Failed to save deck: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate deck meets game rules (30 cards, copy limits, etc.)
        /// </summary>
        public bool ValidateDeck(List<DeckCard> deckCards, out string errorMessage)
        {
            errorMessage = "";
            
            // Calculate total cards
            int totalCards = 0;
            foreach (var deckCard in deckCards)
            {
                totalCards += deckCard.quantity;
            }
            
            // Check deck size
            if (totalCards != 30)
            {
                errorMessage = $"Deck must have exactly 30 cards (currently has {totalCards})";
                return false;
            }
            
            // Check copy limits
            foreach (var deckCard in deckCards)
            {
                int maxCopies = deckCard.card.GetMaxCopiesAllowed();
                if (deckCard.quantity > maxCopies)
                {
                    errorMessage = $"{deckCard.card.name} ({deckCard.card.rarity}) can only have {maxCopies} copies (currently has {deckCard.quantity})";
                    return false;
                }
            }
            
            if (enableDebugLogs) Debug.Log("✅ Deck validation passed");
            return true;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get all available rarities
        /// </summary>
        public List<string> GetAvailableRarities()
        {
            return new List<string> { "common", "uncommon", "rare", "legendary", "exotic" };
        }
        
        /// <summary>
        /// Get all available card types
        /// </summary>
        public List<string> GetAvailableCardTypes()
        {
            return new List<string> { "unit", "spell", "structure" };
        }
        
        /// <summary>
        /// Generate random cards for testing (when database is unavailable)
        /// </summary>
        public List<GameCard> GenerateTestCards(int count = 10)
        {
            var testCards = new List<GameCard>();
            var rarities = GetAvailableRarities();
            var types = GetAvailableCardTypes();
            
            for (int i = 0; i < count; i++)
            {
                var card = new GameCard
                {
                    id = Guid.NewGuid().ToString(),
                    name = $"Test Potato {i + 1}",
                    description = $"A test potato card for development",
                    mana_cost = UnityEngine.Random.Range(1, 10),
                    attack = UnityEngine.Random.Range(1, 8),
                    hp = UnityEngine.Random.Range(1, 8),
                    rarity = rarities[UnityEngine.Random.Range(0, rarities.Count)],
                    card_type = types[UnityEngine.Random.Range(0, types.Count)],
                    ability_text = "Test ability",
                    potato_type = "test",
                    trait = "testing",
                    adjective = "experimental"
                };
                
                testCards.Add(card);
            }
            
            if (enableDebugLogs) Debug.Log($"🧪 Generated {count} test cards");
            return testCards;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a complete deck
    /// </summary>
    [Serializable]
    public class Deck
    {
        public string id;
        public string name;
        public string user_id;
        public bool is_active;
        public List<DeckCard> cards;
        public DateTime created_at;
        public DateTime updated_at;
        
        public Deck()
        {
            cards = new List<DeckCard>();
            created_at = DateTime.Now;
            updated_at = DateTime.Now;
        }
        
        public int GetTotalCards()
        {
            int total = 0;
            foreach (var deckCard in cards)
            {
                total += deckCard.quantity;
            }
            return total;
        }
        
        public bool IsValid()
        {
            return CardService.Instance.ValidateDeck(cards, out _);
        }
    }
}