using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PotatoCardGame.Data;
using PotatoCardGame.Network;
using Newtonsoft.Json;

namespace PotatoCardGame.Cards
{
    /// <summary>
    /// Manages all card-related operations including loading from database,
    /// creating card instances, and handling card collections
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [Header("Card Prefabs")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardContainer;
        
        [Header("Card Database")]
        [SerializeField] private List<CardData> localCardDatabase = new List<CardData>();
        
        // Singleton
        public static CardManager Instance { get; private set; }
        
        // Runtime data
        private Dictionary<string, CardData> cardDatabase = new Dictionary<string, CardData>();
        private List<CardData> userCollection = new List<CardData>();
        private Dictionary<string, int> userCardCounts = new Dictionary<string, int>();
        
        // Events
        public System.Action OnCollectionLoaded;
        public System.Action<CardData> OnCardUnlocked;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCardDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeCardDatabase()
        {
            Debug.Log("🃏 Initializing Card Database...");
            
            // Load local card data first
            foreach (CardData card in localCardDatabase)
            {
                if (!cardDatabase.ContainsKey(card.cardId))
                {
                    cardDatabase[card.cardId] = card;
                }
            }
            
            Debug.Log($"📚 Local card database loaded: {cardDatabase.Count} cards");
        }
        
        private void Start()
        {
            // Load cards from Supabase when ready
            LoadCardsFromDatabase();
        }
        
        #region Database Operations
        
        public async Task LoadCardsFromDatabase()
        {
            try
            {
                Debug.Log("🔄 Loading cards from Supabase...");
                
                if (SupabaseClient.Instance == null)
                {
                    Debug.LogError("❌ SupabaseClient not available");
                    return;
                }
                
                // Get all cards from card_complete table
                var cardsResponse = await SupabaseClient.Instance.GetData<List<DatabaseCard>>("card_complete?select=*");
                
                if (cardsResponse != null && cardsResponse.Count > 0)
                {
                    Debug.Log($"✅ Loaded {cardsResponse.Count} cards from database");
                    
                    // Convert database cards to Unity CardData
                    foreach (var dbCard in cardsResponse)
                    {
                        CardData unityCard = ConvertDatabaseCardToUnity(dbCard);
                        if (unityCard != null)
                        {
                            cardDatabase[unityCard.cardId] = unityCard;
                        }
                    }
                    
                    Debug.log($"🎯 Total cards in database: {cardDatabase.Count}");
                }
                
                // Load user's collection
                await LoadUserCollection();
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading cards: {e.Message}");
            }
        }
        
        public async Task LoadUserCollection()
        {
            try
            {
                if (!SupabaseClient.Instance.IsAuthenticated)
                {
                    Debug.LogWarning("⚠️ User not authenticated, cannot load collection");
                    return;
                }
                
                Debug.Log("🔄 Loading user collection...");
                
                // Get user's collection from database
                var collectionResponse = await SupabaseClient.Instance.GetData<List<UserCollectionItem>>(
                    $"user_collection?user_id=eq.{SupabaseClient.Instance.GetUserId()}&select=*"
                );
                
                if (collectionResponse != null)
                {
                    userCollection.Clear();
                    userCardCounts.Clear();
                    
                    foreach (var item in collectionResponse)
                    {
                        if (cardDatabase.ContainsKey(item.card_id))
                        {
                            CardData card = cardDatabase[item.card_id];
                            userCollection.Add(card);
                            userCardCounts[item.card_id] = item.quantity;
                        }
                    }
                    
                    Debug.Log($"✅ User collection loaded: {userCollection.Count} unique cards");
                    OnCollectionLoaded?.Invoke();
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading user collection: {e.Message}");
            }
        }
        
        #endregion
        
        #region Card Creation and Management
        
        public GameObject CreateCardObject(CardData cardData, Transform parent = null)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("❌ Card prefab not assigned!");
                return null;
            }
            
            Transform spawnParent = parent != null ? parent : cardContainer;
            GameObject cardObject = Instantiate(cardPrefab, spawnParent);
            
            CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Initialize(cardData, true);
            }
            
            return cardObject;
        }
        
        public List<CardData> GetUserCollection()
        {
            return new List<CardData>(userCollection);
        }
        
        public int GetCardCount(string cardId)
        {
            return userCardCounts.ContainsKey(cardId) ? userCardCounts[cardId] : 0;
        }
        
        public bool HasCard(string cardId)
        {
            return userCardCounts.ContainsKey(cardId) && userCardCounts[cardId] > 0;
        }
        
        public CardData GetCardData(string cardId)
        {
            return cardDatabase.ContainsKey(cardId) ? cardDatabase[cardId] : null;
        }
        
        public List<CardData> GetCardsByRarity(Rarity rarity)
        {
            List<CardData> result = new List<CardData>();
            foreach (CardData card in userCollection)
            {
                if (card.rarity == rarity)
                {
                    result.Add(card);
                }
            }
            return result;
        }
        
        public List<CardData> GetCardsByElement(ElementType element)
        {
            List<CardData> result = new List<CardData>();
            foreach (CardData card in userCollection)
            {
                if (card.elementType == element)
                {
                    result.Add(card);
                }
            }
            return result;
        }
        
        #endregion
        
        #region Database Models
        
        [System.Serializable]
        public class DatabaseCard
        {
            public string id;
            public string registry_id;
            public string name;
            public string description;
            public string potato_type;
            public string trait;
            public string adjective;
            public string rarity;
            public string palette_name;
            public string pixel_art_data;
            public int generation_seed;
            public int variation_index;
            public int sort_order;
            public int mana_cost;
            public int attack;
            public int hp;
            public string card_type;
            public bool is_legendary;
            public bool exotic;
            public string set_id;
            public List<string> format_legalities;
            public string ability_text;
            public string passive_effects;
            public string triggered_effects;
            public string illustration_url;
            public string frame_style;
            public string flavor_text;
            public string release_date;
            public List<string> tags;
            public int craft_cost;
            public int dust_value;
            public string alternate_skins;
            public string voice_line_url;
            public string level_up_conditions;
            public string token_spawns;
            public string created_at;
            public string pixel_art_config;
            public string visual_effects;
            public string unit_class;
            public List<string> keywords;
            public string target_type;
            public int spell_damage;
            public int heal_amount;
            public int structure_hp;
            public string passive_effect;
        }
        
        [System.Serializable]
        public class UserCollectionItem
        {
            public string id;
            public string user_id;
            public string card_id;
            public int quantity;
            public bool is_favorite;
            public string created_at;
            public string updated_at;
        }
        
        #endregion
        
        private CardData ConvertDatabaseCardToUnity(DatabaseCard dbCard)
        {
            // Create a new CardData ScriptableObject instance
            CardData unityCard = ScriptableObject.CreateInstance<CardData>();
            
            // Basic properties
            unityCard.cardId = dbCard.id;
            unityCard.cardName = dbCard.name;
            unityCard.description = dbCard.description;
            unityCard.manaCost = dbCard.mana_cost;
            unityCard.attack = dbCard.attack;
            unityCard.health = dbCard.hp;
            unityCard.abilityText = dbCard.ability_text;
            
            // Potato properties
            unityCard.potatoType = dbCard.potato_type;
            unityCard.trait = dbCard.trait;
            unityCard.adjective = dbCard.adjective;
            
            // Parse rarity
            unityCard.rarity = ParseRarity(dbCard.rarity);
            
            // Parse card type
            unityCard.cardType = ParseCardType(dbCard.card_type);
            
            // Parse element type (you'll need to add logic based on your data)
            unityCard.elementType = ParseElementType(dbCard.tags);
            
            // Collection info
            unityCard.isLegendary = dbCard.is_legendary;
            unityCard.isExotic = dbCard.exotic;
            unityCard.setId = dbCard.set_id;
            unityCard.formatLegalities = dbCard.format_legalities ?? new List<string>();
            
            // Crafting
            unityCard.craftCost = dbCard.craft_cost;
            unityCard.dustValue = dbCard.dust_value;
            
            // Advanced properties
            unityCard.keywords = dbCard.keywords ?? new List<string>();
            unityCard.targetType = ParseTargetType(dbCard.target_type);
            unityCard.spellDamage = dbCard.spell_damage;
            unityCard.healAmount = dbCard.heal_amount;
            unityCard.structureHp = dbCard.structure_hp;
            
            return unityCard;
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
                "hero" => CardType.Hero,
                "token" => CardType.Token,
                _ => CardType.Unit
            };
        }
        
        private ElementType ParseElementType(List<string> tags)
        {
            if (tags == null) return ElementType.Neutral;
            
            foreach (string tag in tags)
            {
                switch (tag.ToLower())
                {
                    case "fire": return ElementType.Fire;
                    case "water": return ElementType.Water;
                    case "earth": return ElementType.Earth;
                    case "air": return ElementType.Air;
                    case "light": return ElementType.Light;
                    case "dark": return ElementType.Dark;
                    case "lightning": return ElementType.Lightning;
                    case "void": return ElementType.Void;
                }
            }
            
            return ElementType.Neutral;
        }
        
        private TargetType ParseTargetType(string targetTypeString)
        {
            return targetTypeString?.ToLower() switch
            {
                "enemy" => TargetType.Enemy,
                "friendly" => TargetType.Friendly,
                "any" => TargetType.Any,
                "self" => TargetType.Self,
                "all" => TargetType.All,
                "random" => TargetType.Random,
                _ => TargetType.None
            };
        }
    }
}