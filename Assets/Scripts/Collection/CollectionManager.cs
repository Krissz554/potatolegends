using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using PotatoLegends.Data;
using PotatoLegends.Network;

namespace PotatoLegends.Collection
{
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance { get; private set; }

        [Header("Collection Data")]
        public List<CollectionItem> userCollection = new List<CollectionItem>();
        public List<CardData> allCards = new List<CardData>();
        public bool isCollectionLoaded = false;
        public bool isLoading = false;

        [Header("Events")]
        public System.Action<CollectionItem[]> OnCollectionLoaded;
        public System.Action<CardData[]> OnAllCardsLoaded;
        public System.Action<string> OnCollectionError;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public async Task LoadUserCollection()
        {
            if (isLoading) return;

            string userId = PlayerPrefs.GetString("user_id");
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("CollectionManager: No user ID found");
                OnCollectionError?.Invoke("No user ID found");
                return;
            }

            isLoading = true;
            Debug.Log($"CollectionManager: Loading collection for user {userId}");

            try
            {
                var (collection, error) = await SupabaseClient.Instance.GetUserCollection(userId);
                
                if (error != null)
                {
                    Debug.LogError($"CollectionManager: Failed to load collection: {error}");
                    OnCollectionError?.Invoke(error);
                    return;
                }

                userCollection.Clear();
                if (collection != null)
                {
                    userCollection.AddRange(collection);
                }

                isCollectionLoaded = true;
                OnCollectionLoaded?.Invoke(collection);
                Debug.Log($"CollectionManager: Loaded {userCollection.Count} collection items");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CollectionManager: Exception loading collection: {e.Message}");
                OnCollectionError?.Invoke($"Exception: {e.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        public async Task LoadAllCards()
        {
            if (isLoading) return;

            isLoading = true;
            Debug.Log("CollectionManager: Loading all cards");

            try
            {
                var (cards, error) = await SupabaseClient.Instance.GetAllCards();
                
                if (error != null)
                {
                    Debug.LogError($"CollectionManager: Failed to load cards: {error}");
                    OnCollectionError?.Invoke(error);
                    return;
                }

                allCards.Clear();
                if (cards != null)
                {
                    allCards.AddRange(cards);
                }

                OnAllCardsLoaded?.Invoke(cards);
                Debug.Log($"CollectionManager: Loaded {allCards.Count} cards");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CollectionManager: Exception loading cards: {e.Message}");
                OnCollectionError?.Invoke($"Exception: {e.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        public CollectionItem GetCollectionItem(string cardId)
        {
            return userCollection.Find(item => item.card.id == cardId);
        }

        public int GetCardQuantity(string cardId)
        {
            var item = GetCollectionItem(cardId);
            return item?.quantity ?? 0;
        }

        public List<CollectionItem> GetCardsByRarity(Rarity rarity)
        {
            return userCollection.FindAll(item => item.card.rarity == rarity);
        }

        public List<CollectionItem> GetCardsByType(CardType cardType)
        {
            return userCollection.FindAll(item => item.card.card_type == cardType);
        }

        public List<CollectionItem> GetCardsByElement(PotatoType elementType)
        {
            return userCollection.FindAll(item => item.card.potato_type == elementType);
        }

        public CollectionStats GetCollectionStats()
        {
            int totalCards = 0;
            int uniqueCards = userCollection.Count;
            int commonCards = 0;
            int uncommonCards = 0;
            int rareCards = 0;
            int legendaryCards = 0;
            int exoticCards = 0;

            foreach (var item in userCollection)
            {
                totalCards += item.quantity;
                
                switch (item.card.rarity)
                {
                    case Rarity.common:
                        commonCards++;
                        break;
                    case Rarity.uncommon:
                        uncommonCards++;
                        break;
                    case Rarity.rare:
                        rareCards++;
                        break;
                    case Rarity.legendary:
                        legendaryCards++;
                        break;
                    case Rarity.exotic:
                        exoticCards++;
                        break;
                }
            }

            float completionPercentage = allCards.Count > 0 ? (float)uniqueCards / allCards.Count * 100f : 0f;

            return new CollectionStats
            {
                totalCards = totalCards,
                uniqueCards = uniqueCards,
                commonCards = commonCards,
                uncommonCards = uncommonCards,
                rareCards = rareCards,
                legendaryCards = legendaryCards,
                exoticCards = exoticCards,
                completionPercentage = completionPercentage
            };
        }
    }

    [System.Serializable]
    public class CollectionStats
    {
        public int totalCards;
        public int uniqueCards;
        public int commonCards;
        public int uncommonCards;
        public int rareCards;
        public int legendaryCards;
        public int exoticCards;
        public float completionPercentage;
    }
}