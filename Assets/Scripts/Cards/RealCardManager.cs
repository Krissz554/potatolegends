using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using PotatoCardGame.Network;
using PotatoCardGame.Data;

namespace PotatoCardGame.Cards
{
    /// <summary>
    /// REAL Card Manager that loads actual cards from your Supabase database
    /// Handles all 236+ cards with real data, stats, and abilities
    /// </summary>
    public class RealCardManager : MonoBehaviour
    {
        [Header("Card System")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardContainer;
        
        // Singleton
        public static RealCardManager Instance { get; private set; }
        
        // Real card data from database
        private List<RealSupabaseClient.EnhancedCard> allCardsFromDB = new List<RealSupabaseClient.EnhancedCard>();
        private List<RealSupabaseClient.CollectionItem> userCollection = new List<RealSupabaseClient.CollectionItem>();
        private Dictionary<string, RealSupabaseClient.EnhancedCard> cardLookup = new Dictionary<string, RealSupabaseClient.EnhancedCard>();
        
        // Events
        public System.Action OnCardsLoaded;
        public System.Action OnCollectionLoaded;
        public System.Action<RealSupabaseClient.EnhancedCard> OnCardUnlocked;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            Debug.Log("🃏 Real Card Manager Initialized");
            
            // Subscribe to Supabase events
            if (RealSupabaseClient.Instance != null)
            {
                RealSupabaseClient.Instance.OnCardsLoaded += OnDatabaseCardsLoaded;
                RealSupabaseClient.Instance.OnCollectionLoaded += OnDatabaseCollectionLoaded;
                RealSupabaseClient.Instance.OnAuthenticationChanged += OnAuthChanged;
            }
        }
        
        private async void Start()
        {
            // Load all cards from database
            await LoadAllCardsFromDatabase();
        }
        
        public async Task LoadAllCardsFromDatabase()
        {
            try
            {
                Debug.Log("🔄 Loading all cards from Supabase database...");
                
                var cards = await RealSupabaseClient.Instance.LoadAllCards();
                
                if (cards != null && cards.Count > 0)
                {
                    allCardsFromDB = cards;
                    
                    // Create lookup dictionary
                    cardLookup.Clear();
                    foreach (var card in allCardsFromDB)
                    {
                        cardLookup[card.id] = card;
                    }
                    
                    Debug.Log($"✅ Loaded {allCardsFromDB.Count} cards from database");
                    OnCardsLoaded?.Invoke();
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading cards: {e.Message}");
            }
        }
        
        public async Task LoadUserCollectionFromDatabase()
        {
            try
            {
                if (!RealSupabaseClient.Instance.IsAuthenticated)
                {
                    Debug.LogWarning("⚠️ Cannot load collection - user not authenticated");
                    return;
                }
                
                Debug.Log("📚 Loading user collection from database...");
                
                var collection = await RealSupabaseClient.Instance.LoadUserCollection();
                
                if (collection != null)
                {
                    userCollection = collection;
                    Debug.Log($"✅ Loaded user collection: {userCollection.Count} unique cards");
                    OnCollectionLoaded?.Invoke();
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading collection: {e.Message}");
            }
        }
        
        private void OnDatabaseCardsLoaded(List<RealSupabaseClient.EnhancedCard> cards)
        {
            allCardsFromDB = cards;
            
            // Create lookup dictionary
            cardLookup.Clear();
            foreach (var card in allCardsFromDB)
            {
                cardLookup[card.id] = card;
            }
            
            OnCardsLoaded?.Invoke();
        }
        
        private void OnDatabaseCollectionLoaded(List<RealSupabaseClient.CollectionItem> collection)
        {
            userCollection = collection;
            OnCollectionLoaded?.Invoke();
        }
        
        private void OnAuthChanged(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                // Load user collection when authenticated
                _ = LoadUserCollectionFromDatabase();
            }
            else
            {
                // Clear collection when logged out
                userCollection.Clear();
            }
        }
        
        #region Card Access Methods
        
        public List<RealSupabaseClient.EnhancedCard> GetAllCards()
        {
            return new List<RealSupabaseClient.EnhancedCard>(allCardsFromDB);
        }
        
        public List<RealSupabaseClient.CollectionItem> GetUserCollection()
        {
            return new List<RealSupabaseClient.CollectionItem>(userCollection);
        }
        
        public RealSupabaseClient.EnhancedCard GetCardById(string cardId)
        {
            return cardLookup.ContainsKey(cardId) ? cardLookup[cardId] : null;
        }
        
        public int GetCardQuantity(string cardId)
        {
            var collectionItem = userCollection.FirstOrDefault(item => item.card.id == cardId);
            return collectionItem?.quantity ?? 0;
        }
        
        public bool HasCard(string cardId)
        {
            return GetCardQuantity(cardId) > 0;
        }
        
        public List<RealSupabaseClient.EnhancedCard> GetCardsByRarity(string rarity)
        {
            return allCardsFromDB.Where(card => card.rarity == rarity).ToList();
        }
        
        public List<RealSupabaseClient.EnhancedCard> GetCardsByType(string cardType)
        {
            return allCardsFromDB.Where(card => card.card_type == cardType).ToList();
        }
        
        public List<RealSupabaseClient.EnhancedCard> GetCardsByElement(string potatoType)
        {
            return allCardsFromDB.Where(card => card.potato_type == potatoType).ToList();
        }
        
        public List<RealSupabaseClient.CollectionItem> GetOwnedCards()
        {
            return userCollection.Where(item => item.quantity > 0).ToList();
        }
        
        public Dictionary<string, int> GetCollectionStats()
        {
            var stats = new Dictionary<string, int>
            {
                ["total_cards"] = userCollection.Sum(item => item.quantity),
                ["unique_cards"] = userCollection.Count(item => item.quantity > 0),
                ["common_cards"] = userCollection.Count(item => item.card.rarity == "common" && item.quantity > 0),
                ["uncommon_cards"] = userCollection.Count(item => item.card.rarity == "uncommon" && item.quantity > 0),
                ["rare_cards"] = userCollection.Count(item => item.card.rarity == "rare" && item.quantity > 0),
                ["legendary_cards"] = userCollection.Count(item => item.card.rarity == "legendary" && item.quantity > 0),
                ["exotic_cards"] = userCollection.Count(item => item.card.rarity == "exotic" && item.quantity > 0)
            };
            
            return stats;
        }
        
        #endregion
        
        #region Card Display
        
        public GameObject CreateCardDisplay(RealSupabaseClient.EnhancedCard card, Transform parent = null, bool showQuantity = false)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("❌ Card prefab not assigned!");
                return null;
            }
            
            Transform spawnParent = parent ?? cardContainer;
            GameObject cardObject = Instantiate(cardPrefab, spawnParent);
            
            // Configure card display
            CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                // Convert EnhancedCard to CardData
                CardData cardData = ConvertEnhancedCardToCardData(card);
                cardDisplay.Initialize(cardData, parent == playerHandArea);
                
                // Show quantity if requested
                if (showQuantity)
                {
                    int quantity = GetCardQuantity(card.id);
                    // TODO: Display quantity on card UI
                }
            }
            
            return cardObject;
        }
        
        private CardData ConvertEnhancedCardToCardData(RealSupabaseClient.EnhancedCard enhancedCard)
        {
            CardData cardData = ScriptableObject.CreateInstance<CardData>();
            
            // Basic info
            cardData.cardId = enhancedCard.id;
            cardData.cardName = enhancedCard.name;
            cardData.description = enhancedCard.description;
            cardData.manaCost = enhancedCard.mana_cost;
            cardData.attack = enhancedCard.attack ?? 0;
            cardData.health = enhancedCard.hp ?? 0;
            cardData.abilityText = enhancedCard.ability_text ?? "";
            
            // Potato properties
            cardData.potatoType = enhancedCard.potato_type ?? "";
            cardData.trait = enhancedCard.trait ?? "";
            cardData.adjective = enhancedCard.adjective ?? "";
            
            // Parse enums
            cardData.rarity = ParseRarity(enhancedCard.rarity);
            cardData.cardType = ParseCardType(enhancedCard.card_type);
            cardData.elementType = ParseElementType(enhancedCard.potato_type);
            cardData.targetType = ParseTargetType(enhancedCard.target_type);
            
            // Collection info
            cardData.isLegendary = enhancedCard.is_legendary;
            cardData.isExotic = enhancedCard.is_exotic || enhancedCard.exotic;
            cardData.setId = enhancedCard.set_id ?? "";
            cardData.formatLegalities = enhancedCard.format_legalities ?? new List<string>();
            
            // Advanced
            cardData.keywords = enhancedCard.keywords ?? new List<string>();
            cardData.spellDamage = enhancedCard.spell_damage;
            cardData.healAmount = enhancedCard.heal_amount;
            cardData.structureHp = enhancedCard.structure_hp ?? 0;
            
            // Crafting
            cardData.craftCost = enhancedCard.craft_cost;
            cardData.dustValue = enhancedCard.dust_value;
            
            return cardData;
        }
        
        private Rarity ParseRarity(string rarityString)
        {
            return rarityString?.ToLower() switch
            {
                "common" => Rarity.Common,
                "uncommon" => Rarity.Uncommon,
                "rare" => Rarity.Rare,
                "legendary" => Rarity.Legendary,
                "exotic" => Rarity.Exotic,
                _ => Rarity.Common
            };
        }
        
        private CardType ParseCardType(string cardTypeString)
        {
            return cardTypeString?.ToLower() switch
            {
                "unit" => CardType.Unit,
                "spell" => CardType.Spell,
                "structure" => CardType.Structure,
                "relic" => CardType.Hero, // Map relic to Hero for now
                _ => CardType.Unit
            };
        }
        
        private ElementType ParseElementType(string potatoType)
        {
            return potatoType?.ToLower() switch
            {
                "fire" => ElementType.Fire,
                "ice" => ElementType.Water, // Map ice to water
                "lightning" => ElementType.Lightning,
                "light" => ElementType.Light,
                "void" => ElementType.Void,
                _ => ElementType.Neutral
            };
        }
        
        private TargetType ParseTargetType(string targetTypeString)
        {
            return targetTypeString?.ToLower() switch
            {
                "enemy_unit" => TargetType.Enemy,
                "ally_unit" => TargetType.Friendly,
                "any_unit" => TargetType.Any,
                "enemy_hero" => TargetType.Enemy,
                "ally_hero" => TargetType.Self,
                "all_enemies" => TargetType.All,
                "all_allies" => TargetType.All,
                "all_units" => TargetType.All,
                "random_enemy" => TargetType.Random,
                "random_ally" => TargetType.Random,
                _ => TargetType.None
            };
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (RealSupabaseClient.Instance != null)
            {
                RealSupabaseClient.Instance.OnCardsLoaded -= OnDatabaseCardsLoaded;
                RealSupabaseClient.Instance.OnCollectionLoaded -= OnDatabaseCollectionLoaded;
                RealSupabaseClient.Instance.OnAuthenticationChanged -= OnAuthChanged;
            }
        }
    }
}