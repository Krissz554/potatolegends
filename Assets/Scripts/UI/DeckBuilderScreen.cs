using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PotatoLegends.UI
{
    public class DeckBuilderScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public Transform availableCardsGridParent;
        public GameObject cardDisplayPrefab;
        public Transform deckSlotsParent;
        public GameObject deckSlotPrefab;
        public Button saveDeckButton;
        public Button clearDeckButton;
        public TextMeshProUGUI deckCountText;

        private List<CardDisplay> availableCardDisplays = new List<CardDisplay>();
        private List<DeckSlot> deckSlots = new List<DeckSlot>();

        void OnEnable()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCollectionUpdated += DisplayAvailableCards;
                CollectionManager.Instance.OnDeckUpdated += UpdateDeckDisplay;
                
                CollectionManager.Instance.LoadAllCards();
                if (SupabaseClient.Instance != null && !string.IsNullOrEmpty(SupabaseClient.Instance.GetAccessToken()))
                {
                    CollectionManager.Instance.LoadActiveDeck(SupabaseClient.Instance.GetAccessToken());
                }
            }

            saveDeckButton.onClick.AddListener(OnSaveDeckButtonClicked);
            clearDeckButton.onClick.AddListener(OnClearDeckButtonClicked);

            DisplayAvailableCards();
            SetupDeckSlots();
            UpdateDeckDisplay();
        }

        void OnDisable()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCollectionUpdated -= DisplayAvailableCards;
                CollectionManager.Instance.OnDeckUpdated -= UpdateDeckDisplay;
            }
            saveDeckButton.onClick.RemoveAllListeners();
            clearDeckButton.onClick.RemoveAllListeners();
            ClearAvailableCards();
            ClearDeckSlots();
        }

        private void DisplayAvailableCards()
        {
            ClearAvailableCards();

            if (CollectionManager.Instance == null || CollectionManager.Instance.AllAvailableCards.Count == 0)
            {
                return;
            }

            foreach (CardData card in CollectionManager.Instance.AllAvailableCards)
            {
                GameObject cardObj = Instantiate(cardDisplayPrefab, availableCardsGridParent);
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                if (display != null)
                {
                    display.SetupCard(card);
                    display.GetComponent<Button>().onClick.AddListener(() => OnCardClickedInCollection(card));
                    availableCardDisplays.Add(display);
                }
            }
        }

        private void SetupDeckSlots()
        {
            ClearDeckSlots();
            for (int i = 0; i < 30; i++)
            {
                GameObject slotObj = Instantiate(deckSlotPrefab, deckSlotsParent);
                DeckSlot slot = slotObj.GetComponent<DeckSlot>();
                if (slot != null)
                {
                    slot.slotIndex = i;
                    slot.OnCardRemovedFromSlot += OnCardRemovedFromDeckSlot;
                    deckSlots.Add(slot);
                }
            }
        }

        private void UpdateDeckDisplay()
        {
            foreach (DeckSlot slot in deckSlots)
            {
                slot.ClearCard();
            }

            if (CollectionManager.Instance != null)
            {
                for (int i = 0; i < CollectionManager.Instance.CurrentDeck.Count && i < deckSlots.Count; i++)
                {
                    deckSlots[i].SetCard(CollectionManager.Instance.CurrentDeck[i]);
                }
                deckCountText.text = $"Deck: {CollectionManager.Instance.CurrentDeck.Count}/30";
            }
        }

        private void OnCardClickedInCollection(CardData card)
        {
            CollectionManager.Instance.AddCardToDeck(card);
        }

        private void OnCardRemovedFromDeckSlot(CardData card)
        {
            CollectionManager.Instance.RemoveCardFromDeck(card);
        }

        private void OnSaveDeckButtonClicked()
        {
            if (CollectionManager.Instance != null && SupabaseClient.Instance != null && !string.IsNullOrEmpty(SupabaseClient.Instance.GetAccessToken()))
            {
                CollectionManager.Instance.SaveCurrentDeck(SupabaseClient.Instance.GetAccessToken());
            }
        }

        private void OnClearDeckButtonClicked()
        {
            CollectionManager.Instance.ClearDeck();
        }

        private void ClearAvailableCards()
        {
            foreach (CardDisplay display in availableCardDisplays)
            {
                Destroy(display.gameObject);
            }
            availableCardDisplays.Clear();
        }

        private void ClearDeckSlots()
        {
            foreach (DeckSlot slot in deckSlots)
            {
                slot.OnCardRemovedFromSlot -= OnCardRemovedFromDeckSlot;
                Destroy(slot.gameObject);
            }
            deckSlots.Clear();
        }
    }
}