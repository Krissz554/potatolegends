using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Data;
using PotatoCardGame.Network;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Mobile deck builder for creating and editing 30-card decks
    /// Features touch-optimized interface with drag-and-drop and tap-to-add
    /// </summary>
    public class DeckBuilder : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject deckBuilderPanel;
        [SerializeField] private GameObject deckListPanel;
        [SerializeField] private GameObject cardBrowserPanel;
        [SerializeField] private GameObject deckEditPanel;
        
        [Header("Deck List UI")]
        [SerializeField] private Transform deckListContainer;
        [SerializeField] private GameObject deckItemPrefab;
        [SerializeField] private Button createNewDeckButton;
        [SerializeField] private Button backToMenuButton;
        
        [Header("Card Browser UI")]
        [SerializeField] private ScrollRect cardBrowserScrollView;
        [SerializeField] private Transform cardBrowserContainer;
        [SerializeField] private GameObject cardBrowserItemPrefab;
        [SerializeField] private TMP_InputField cardSearchInput;
        [SerializeField] private TMP_Dropdown cardRarityFilter;
        [SerializeField] private TMP_Dropdown cardTypeFilter;
        [SerializeField] private Button clearCardFiltersButton;
        
        [Header("Deck Edit UI")]
        [SerializeField] private ScrollRect deckScrollView;
        [SerializeField] private Transform deckContainer;
        [SerializeField] private GameObject deckCardPrefab;
        [SerializeField] private TMP_InputField deckNameInput;
        [SerializeField] private Button saveDeckButton;
        [SerializeField] private Button deleteDeckButton;
        [SerializeField] private Button setActiveDeckButton;
        
        [Header("Deck Stats")]
        [SerializeField] private TextMeshProUGUI deckCardCountText;
        [SerializeField] private TextMeshProUGUI manaCurveText;
        [SerializeField] private Slider deckProgressBar;
        [SerializeField] private Image deckValidIcon;
        [SerializeField] private TextMeshProUGUI deckValidText;
        
        [Header("Mobile Settings")]
        [SerializeField] private int cardBrowserColumns = 3;
        [SerializeField] private int deckColumns = 2;
        [SerializeField] private float cardSpacing = 5f;
        
        // Data
        private List<Deck> userDecks = new List<Deck>();
        private List<GameCard> availableCards = new List<GameCard>();
        private List<CollectionCard> userCollection = new List<CollectionCard>();
        private Deck currentEditingDeck;
        private List<GameCard> filteredCards = new List<GameCard>();
        
        // State
        private bool isLoading = false;
        private string currentCardSearch = "";
        private string currentCardRarityFilter = "";
        private string currentCardTypeFilter = "";
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            LoadDecksAndCards();
        }
        
        private void InitializeUI()
        {
            // Show deck list by default
            ShowDeckList();
            
            // Setup grid layouts for mobile
            SetupMobileGridLayouts();
            
            // Setup filter dropdowns
            SetupFilterDropdowns();
        }
        
        private void SetupMobileGridLayouts()
        {
            // Card browser grid
            if (cardBrowserContainer)
            {
                var gridLayout = cardBrowserContainer.GetComponent<GridLayoutGroup>();
                if (!gridLayout) gridLayout = cardBrowserContainer.gameObject.AddComponent<GridLayoutGroup>();
                
                float screenWidth = Screen.width;
                float cardWidth = (screenWidth - (cardSpacing * (cardBrowserColumns + 1))) / cardBrowserColumns;
                
                gridLayout.cellSize = new Vector2(cardWidth, cardWidth * 1.4f);
                gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = cardBrowserColumns;
            }
            
            // Deck grid
            if (deckContainer)
            {
                var gridLayout = deckContainer.GetComponent<GridLayoutGroup>();
                if (!gridLayout) gridLayout = deckContainer.gameObject.AddComponent<GridLayoutGroup>();
                
                float screenWidth = Screen.width;
                float cardWidth = (screenWidth - (cardSpacing * (deckColumns + 1))) / deckColumns;
                
                gridLayout.cellSize = new Vector2(cardWidth, cardWidth * 1.4f);
                gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = deckColumns;
            }
        }
        
        private void SetupFilterDropdowns()
        {
            if (cardRarityFilter)
            {
                cardRarityFilter.ClearOptions();
                cardRarityFilter.AddOptions(new List<string> { "All", "Common", "Uncommon", "Rare", "Legendary", "Exotic" });
            }
            
            if (cardTypeFilter)
            {
                cardTypeFilter.ClearOptions();
                cardTypeFilter.AddOptions(new List<string> { "All", "Unit", "Spell", "Structure" });
            }
        }
        
        private void SetupEventListeners()
        {
            // Navigation buttons
            if (createNewDeckButton) createNewDeckButton.onClick.AddListener(OnCreateNewDeck);
            if (backToMenuButton) backToMenuButton.onClick.AddListener(OnBackToMenu);
            
            // Deck management buttons
            if (saveDeckButton) saveDeckButton.onClick.AddListener(OnSaveDeck);
            if (deleteDeckButton) deleteDeckButton.onClick.AddListener(OnDeleteDeck);
            if (setActiveDeckButton) setActiveDeckButton.onClick.AddListener(OnSetActiveDeck);
            
            // Card browser filters
            if (cardSearchInput) cardSearchInput.onValueChanged.AddListener(OnCardSearchChanged);
            if (cardRarityFilter) cardRarityFilter.onValueChanged.AddListener(OnCardRarityFilterChanged);
            if (cardTypeFilter) cardTypeFilter.onValueChanged.AddListener(OnCardTypeFilterChanged);
            if (clearCardFiltersButton) clearCardFiltersButton.onClick.AddListener(OnClearCardFilters);
            
            // Card service events
            if (CardService.Instance)
            {
                CardService.Instance.OnAllCardsLoaded += OnCardsLoaded;
                CardService.Instance.OnUserCollectionLoaded += OnCollectionLoaded;
            }
        }
        
        #region Data Loading
        
        private async void LoadDecksAndCards()
        {
            if (isLoading) return;
            isLoading = true;
            
            Debug.Log("🔧 Loading deck builder data...");
            
            // Load all cards
            availableCards = await CardService.Instance.LoadAllCards();
            
            // Load user collection
            if (SupabaseClient.Instance && SupabaseClient.Instance.IsAuthenticated)
            {
                // TODO: Get user ID and load collection
                // userCollection = await CardService.Instance.LoadUserCollection(userId);
                // userDecks = await CardService.Instance.LoadUserDecks(userId);
            }
            
            // Display deck list
            DisplayDeckList();
            
            isLoading = false;
        }
        
        #endregion
        
        #region UI Navigation
        
        private void ShowDeckList()
        {
            if (deckListPanel) deckListPanel.SetActive(true);
            if (cardBrowserPanel) cardBrowserPanel.SetActive(false);
            if (deckEditPanel) deckEditPanel.SetActive(false);
        }
        
        private void ShowCardBrowser()
        {
            if (deckListPanel) deckListPanel.SetActive(false);
            if (cardBrowserPanel) cardBrowserPanel.SetActive(true);
            if (deckEditPanel) deckEditPanel.SetActive(false);
            
            DisplayCardBrowser();
        }
        
        private void ShowDeckEdit()
        {
            if (deckListPanel) deckListPanel.SetActive(false);
            if (cardBrowserPanel) cardBrowserPanel.SetActive(false);
            if (deckEditPanel) deckEditPanel.SetActive(true);
            
            DisplayCurrentDeck();
        }
        
        #endregion
        
        #region Deck List Display
        
        private void DisplayDeckList()
        {
            // Clear existing deck items
            foreach (Transform child in deckListContainer)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // Create deck items
            foreach (var deck in userDecks)
            {
                CreateDeckListItem(deck);
            }
        }
        
        private void CreateDeckListItem(Deck deck)
        {
            if (!deckItemPrefab || !deckListContainer) return;
            
            GameObject deckItem = Instantiate(deckItemPrefab, deckListContainer);
            
            // Setup deck item UI (would need proper prefab with these components)
            var nameText = deckItem.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText) nameText.text = deck.name;
            
            var editButton = deckItem.GetComponentInChildren<Button>();
            if (editButton)
            {
                editButton.onClick.AddListener(() => EditDeck(deck));
            }
            
            // Show active deck indicator
            var activeIndicator = deckItem.transform.Find("ActiveIndicator");
            if (activeIndicator) activeIndicator.gameObject.SetActive(deck.is_active);
        }
        
        #endregion
        
        #region Card Browser Display
        
        private void DisplayCardBrowser()
        {
            ApplyCardFilters();
            
            // Clear existing cards
            foreach (Transform child in cardBrowserContainer)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // Display filtered cards
            foreach (var card in filteredCards)
            {
                CreateCardBrowserItem(card);
            }
        }
        
        private void ApplyCardFilters()
        {
            filteredCards = availableCards.Where(card => {
                // Check if user owns this card
                bool isOwned = userCollection.Any(c => c.card.id == card.id);
                if (!isOwned) return false;
                
                // Apply search filter
                if (!string.IsNullOrEmpty(currentCardSearch))
                {
                    bool matchesSearch = card.name.ToLower().Contains(currentCardSearch.ToLower()) ||
                                       card.description.ToLower().Contains(currentCardSearch.ToLower());
                    if (!matchesSearch) return false;
                }
                
                // Apply rarity filter
                if (!string.IsNullOrEmpty(currentCardRarityFilter) && currentCardRarityFilter != "All")
                {
                    if (card.rarity.ToLower() != currentCardRarityFilter.ToLower()) return false;
                }
                
                // Apply type filter
                if (!string.IsNullOrEmpty(currentCardTypeFilter) && currentCardTypeFilter != "All")
                {
                    if (card.card_type.ToLower() != currentCardTypeFilter.ToLower()) return false;
                }
                
                return true;
            }).OrderBy(card => card.mana_cost).ThenBy(card => card.name).ToList();
        }
        
        private void CreateCardBrowserItem(GameCard card)
        {
            if (!cardBrowserItemPrefab || !cardBrowserContainer) return;
            
            GameObject cardItem = Instantiate(cardBrowserItemPrefab, cardBrowserContainer);
            var cardUI = cardItem.GetComponent<MobileCardUI>();
            
            if (cardUI)
            {
                cardUI.SetCardData(card);
                cardUI.OnCardTapped += (ui) => AddCardToDeck(card);
                cardUI.OnCardDoubleTapped += (ui) => AddCardToDeck(card, 2);
                
                // Show quantity owned
                int ownedQuantity = GetOwnedCardQuantity(card.id);
                int usedInDeck = GetCardQuantityInDeck(card.id);
                int available = ownedQuantity - usedInDeck;
                
                cardUI.SetInteractable(available > 0);
            }
        }
        
        #endregion
        
        #region Deck Management
        
        private void EditDeck(Deck deck)
        {
            currentEditingDeck = deck;
            
            if (deckNameInput) deckNameInput.text = deck.name;
            
            ShowDeckEdit();
            UpdateDeckStats();
        }
        
        private void CreateNewDeck()
        {
            currentEditingDeck = new Deck
            {
                id = System.Guid.NewGuid().ToString(),
                name = "New Deck",
                user_id = "", // TODO: Get from authentication
                is_active = false,
                cards = new List<DeckCard>()
            };
            
            EditDeck(currentEditingDeck);
        }
        
        private void AddCardToDeck(GameCard card, int quantity = 1)
        {
            if (currentEditingDeck == null) return;
            
            // Check if card is already in deck
            var existingCard = currentEditingDeck.cards.Find(dc => dc.card.id == card.id);
            
            if (existingCard != null)
            {
                // Check copy limits
                int maxCopies = card.GetMaxCopiesAllowed();
                int newQuantity = existingCard.quantity + quantity;
                
                if (newQuantity <= maxCopies)
                {
                    existingCard.quantity = newQuantity;
                }
                else
                {
                    Debug.LogWarning($"⚠️ Cannot add more copies of {card.name}. Max: {maxCopies}");
                    return;
                }
            }
            else
            {
                // Add new card to deck
                if (currentEditingDeck.GetTotalCards() + quantity <= 30)
                {
                    currentEditingDeck.cards.Add(new DeckCard(card, quantity));
                }
                else
                {
                    Debug.LogWarning("⚠️ Deck is full (30 cards max)");
                    return;
                }
            }
            
            DisplayCurrentDeck();
            UpdateDeckStats();
            
            Debug.Log($"➕ Added {quantity}x {card.name} to deck");
        }
        
        private void RemoveCardFromDeck(GameCard card, int quantity = 1)
        {
            if (currentEditingDeck == null) return;
            
            var deckCard = currentEditingDeck.cards.Find(dc => dc.card.id == card.id);
            if (deckCard != null)
            {
                deckCard.quantity -= quantity;
                
                if (deckCard.quantity <= 0)
                {
                    currentEditingDeck.cards.Remove(deckCard);
                }
                
                DisplayCurrentDeck();
                UpdateDeckStats();
                
                Debug.Log($"➖ Removed {quantity}x {card.name} from deck");
            }
        }
        
        #endregion
        
        #region Deck Display
        
        private void DisplayCurrentDeck()
        {
            if (currentEditingDeck == null) return;
            
            // Clear existing deck cards
            foreach (Transform child in deckContainer)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // Sort deck cards by mana cost, then name
            var sortedCards = currentEditingDeck.cards
                .OrderBy(dc => dc.card.mana_cost)
                .ThenBy(dc => dc.card.name)
                .ToList();
            
            // Create deck card items
            foreach (var deckCard in sortedCards)
            {
                CreateDeckCardItem(deckCard);
            }
        }
        
        private void CreateDeckCardItem(DeckCard deckCard)
        {
            if (!deckCardPrefab || !deckContainer) return;
            
            GameObject cardItem = Instantiate(deckCardPrefab, deckContainer);
            var cardUI = cardItem.GetComponent<MobileCardUI>();
            
            if (cardUI)
            {
                cardUI.SetCardData(deckCard.card);
                cardUI.OnCardTapped += (ui) => RemoveCardFromDeck(deckCard.card, 1);
                cardUI.OnCardLongPressed += (ui) => ShowCardOptions(deckCard);
                
                // Show quantity if more than 1
                if (deckCard.quantity > 1)
                {
                    // TODO: Add quantity display to MobileCardUI
                }
            }
        }
        
        private void ShowCardOptions(DeckCard deckCard)
        {
            // TODO: Show options popup (remove 1, remove all, etc.)
            Debug.Log($"🔧 Card options for: {deckCard.card.name} (x{deckCard.quantity})");
        }
        
        #endregion
        
        #region Deck Statistics
        
        private void UpdateDeckStats()
        {
            if (currentEditingDeck == null) return;
            
            int totalCards = currentEditingDeck.GetTotalCards();
            bool isValid = currentEditingDeck.IsValid();
            
            // Update card count
            if (deckCardCountText) deckCardCountText.text = $"{totalCards}/30";
            
            // Update progress bar
            if (deckProgressBar) deckProgressBar.value = totalCards / 30f;
            
            // Update validity indicator
            if (deckValidIcon) deckValidIcon.color = isValid ? Color.green : Color.red;
            if (deckValidText) deckValidText.text = isValid ? "Valid" : "Invalid";
            
            // Update mana curve
            UpdateManaCurveDisplay();
            
            // Enable/disable save button based on validity
            if (saveDeckButton) saveDeckButton.interactable = isValid;
        }
        
        private void UpdateManaCurveDisplay()
        {
            if (!manaCurveText) return;
            
            // Calculate mana curve
            var manaCounts = new int[11]; // 0-10 mana
            
            foreach (var deckCard in currentEditingDeck.cards)
            {
                int mana = Mathf.Clamp(deckCard.card.mana_cost, 0, 10);
                manaCounts[mana] += deckCard.quantity;
            }
            
            // Display as text (could be enhanced with visual chart)
            string curveText = "Mana Curve: ";
            for (int i = 0; i <= 10; i++)
            {
                if (manaCounts[i] > 0)
                {
                    curveText += $"{i}:{manaCounts[i]} ";
                }
            }
            
            manaCurveText.text = curveText;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnCreateNewDeck()
        {
            CreateNewDeck();
        }
        
        private void OnBackToMenu()
        {
            // TODO: Check for unsaved changes
            gameObject.SetActive(false);
        }
        
        private async void OnSaveDeck()
        {
            if (currentEditingDeck == null) return;
            
            // Update deck name
            currentEditingDeck.name = deckNameInput?.text ?? "Unnamed Deck";
            
            // Validate deck
            if (!currentEditingDeck.IsValid())
            {
                Debug.LogWarning("⚠️ Cannot save invalid deck");
                return;
            }
            
            // Save to database
            bool success = await CardService.Instance.SaveDeck(currentEditingDeck.user_id, currentEditingDeck);
            
            if (success)
            {
                Debug.Log($"✅ Deck saved: {currentEditingDeck.name}");
                
                // Update local deck list
                var existingDeck = userDecks.Find(d => d.id == currentEditingDeck.id);
                if (existingDeck != null)
                {
                    userDecks[userDecks.IndexOf(existingDeck)] = currentEditingDeck;
                }
                else
                {
                    userDecks.Add(currentEditingDeck);
                }
                
                ShowDeckList();
                DisplayDeckList();
            }
            else
            {
                Debug.LogError("❌ Failed to save deck");
            }
        }
        
        private void OnDeleteDeck()
        {
            // TODO: Show confirmation dialog
            Debug.Log($"🗑️ Delete deck: {currentEditingDeck?.name}");
        }
        
        private void OnSetActiveDeck()
        {
            if (currentEditingDeck == null || !currentEditingDeck.IsValid()) return;
            
            // Set as active deck
            foreach (var deck in userDecks)
            {
                deck.is_active = (deck.id == currentEditingDeck.id);
            }
            
            currentEditingDeck.is_active = true;
            
            Debug.Log($"⭐ Set active deck: {currentEditingDeck.name}");
            
            // TODO: Save active deck status to database
        }
        
        private void OnCardSearchChanged(string searchTerm)
        {
            currentCardSearch = searchTerm;
            if (cardBrowserPanel.activeInHierarchy)
            {
                DisplayCardBrowser();
            }
        }
        
        private void OnCardRarityFilterChanged(int index)
        {
            currentCardRarityFilter = cardRarityFilter.options[index].text;
            if (cardBrowserPanel.activeInHierarchy)
            {
                DisplayCardBrowser();
            }
        }
        
        private void OnCardTypeFilterChanged(int index)
        {
            currentCardTypeFilter = cardTypeFilter.options[index].text;
            if (cardBrowserPanel.activeInHierarchy)
            {
                DisplayCardBrowser();
            }
        }
        
        private void OnClearCardFilters()
        {
            currentCardSearch = "";
            currentCardRarityFilter = "";
            currentCardTypeFilter = "";
            
            if (cardSearchInput) cardSearchInput.text = "";
            if (cardRarityFilter) cardRarityFilter.value = 0;
            if (cardTypeFilter) cardTypeFilter.value = 0;
            
            if (cardBrowserPanel.activeInHierarchy)
            {
                DisplayCardBrowser();
            }
        }
        
        private void OnCardsLoaded(List<GameCard> cards)
        {
            availableCards = cards;
            if (cardBrowserPanel.activeInHierarchy)
            {
                DisplayCardBrowser();
            }
        }
        
        private void OnCollectionLoaded(List<CollectionCard> collection)
        {
            userCollection = collection;
            if (cardBrowserPanel.activeInHierarchy)
            {
                DisplayCardBrowser();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private int GetOwnedCardQuantity(string cardId)
        {
            var collectionCard = userCollection.Find(c => c.card.id == cardId);
            return collectionCard?.quantity ?? 0;
        }
        
        private int GetCardQuantityInDeck(string cardId)
        {
            if (currentEditingDeck == null) return 0;
            
            var deckCard = currentEditingDeck.cards.Find(dc => dc.card.id == cardId);
            return deckCard?.quantity ?? 0;
        }
        
        #endregion
        
        #region Public Methods
        
        public void ShowDeckBuilder()
        {
            if (deckBuilderPanel) deckBuilderPanel.SetActive(true);
            ShowDeckList();
        }
        
        public void HideDeckBuilder()
        {
            if (deckBuilderPanel) deckBuilderPanel.SetActive(false);
        }
        
        #endregion
    }
}