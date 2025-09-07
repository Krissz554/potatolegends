using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PotatoLegends.Collection
{
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance { get; private set; }

        public List<CardData> UserCollection { get; private set; } = new List<CardData>();
        public List<CardData> AllAvailableCards { get; private set; } = new List<CardData>();
        public List<CardData> CurrentDeck { get; private set; } = new List<CardData>();

        public event System.Action OnCollectionUpdated;
        public event System.Action OnDeckUpdated;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public async Task LoadUserCollection(string userId)
        {
            Debug.Log($"CollectionManager: Loading collection for user: {userId}");
            var (cards, error) = await SupabaseClient.Instance.GetUserCollection(userId);

            if (error != null)
            {
                Debug.LogError($"Failed to load user collection: {error}");
                UserCollection.Clear();
            }
            else
            {
                UserCollection = new List<CardData>(cards);
                Debug.Log($"CollectionManager: Loaded {UserCollection.Count} cards for user {userId}.");
            }
            OnCollectionUpdated?.Invoke();
        }

        public async Task LoadAllCards()
        {
            Debug.Log("CollectionManager: Loading all available cards.");
            await Task.Delay(500);
            AllAvailableCards.Clear();
            
            CardData dummyCard1 = ScriptableObject.CreateInstance<CardData>();
            dummyCard1.cardId = "dummy_001";
            dummyCard1.cardName = "Basic Potato";
            dummyCard1.description = "A humble potato, ready for battle.";
            dummyCard1.manaCost = 1;
            dummyCard1.attack = 1;
            dummyCard1.health = 2;
            dummyCard1.cardType = CardData.CardType.Unit;
            dummyCard1.rarity = CardData.Rarity.Common;
            AllAvailableCards.Add(dummyCard1);

            CardData dummyCard2 = ScriptableObject.CreateInstance<CardData>();
            dummyCard2.cardId = "dummy_002";
            dummyCard2.cardName = "Spud Warrior";
            dummyCard2.description = "A potato trained in the art of combat.";
            dummyCard2.manaCost = 2;
            dummyCard2.attack = 3;
            dummyCard2.health = 2;
            dummyCard2.cardType = CardData.CardType.Unit;
            dummyCard2.rarity = CardData.Rarity.Uncommon;
            AllAvailableCards.Add(dummyCard2);

            CardData dummyCard3 = ScriptableObject.CreateInstance<CardData>();
            dummyCard3.cardId = "dummy_003";
            dummyCard3.cardName = "Mana Sprout";
            dummyCard3.description = "Generates extra mana.";
            dummyCard3.manaCost = 0;
            dummyCard3.cardType = CardData.CardType.Spell;
            dummyCard3.rarity = CardData.Rarity.Common;
            AllAvailableCards.Add(dummyCard3);

            Debug.Log($"CollectionManager: Loaded {AllAvailableCards.Count} total cards.");
        }

        public void AddCardToDeck(CardData card)
        {
            if (CurrentDeck.Count < 30)
            {
                CurrentDeck.Add(card);
                OnDeckUpdated?.Invoke();
                Debug.Log($"Added {card.cardName} to deck. Current size: {CurrentDeck.Count}");
            }
            else
            {
                Debug.LogWarning("Deck is full!");
            }
        }

        public void RemoveCardFromDeck(CardData card)
        {
            if (CurrentDeck.Remove(card))
            {
                OnDeckUpdated?.Invoke();
                Debug.Log($"Removed {card.cardName} from deck. Current size: {CurrentDeck.Count}");
            }
        }

        public void ClearDeck()
        {
            CurrentDeck.Clear();
            OnDeckUpdated?.Invoke();
            Debug.Log("Deck cleared.");
        }

        public async Task SaveCurrentDeck(string userId, string deckName = "Default Deck")
        {
            Debug.Log($"CollectionManager: Saving deck for user {userId}.");
            await Task.Delay(1000);
            Debug.Log("Deck saved successfully (simulated).");
        }

        public async Task LoadActiveDeck(string userId)
        {
            Debug.Log($"CollectionManager: Loading active deck for user {userId}.");
            await Task.Delay(1000);
            CurrentDeck.Clear();
            if (AllAvailableCards.Count >= 2)
            {
                CurrentDeck.Add(AllAvailableCards[0]);
                CurrentDeck.Add(AllAvailableCards[1]);
            }
            Debug.Log($"CollectionManager: Loaded dummy active deck with {CurrentDeck.Count} cards.");
            OnDeckUpdated?.Invoke();
        }
    }
}