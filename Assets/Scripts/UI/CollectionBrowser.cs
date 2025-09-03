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
    /// Mobile collection browser for viewing and managing card collection
    /// Optimized for touch interaction and mobile performance
    /// </summary>
    public class CollectionBrowser : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private ScrollRect cardScrollView;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardUIPrefab;
        
        [Header("Search and Filter")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TMP_Dropdown rarityFilter;
        [SerializeField] private TMP_Dropdown typeFilter;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Button clearFiltersButton;
        
        [Header("Collection Stats")]
        [SerializeField] private TextMeshProUGUI totalCardsText;
        [SerializeField] private TextMeshProUGUI uniqueCardsText;
        [SerializeField] private TextMeshProUGUI collectionProgressText;
        [SerializeField] private Slider collectionProgressBar;
        
        [Header("Card Detail View")]
        [SerializeField] private GameObject cardDetailPanel;
        [SerializeField] private MobileCardUI detailCardDisplay;
        [SerializeField] private Button closeDetailButton;
        
        [Header("Mobile Settings")]
        [SerializeField] private int cardsPerRow = 3;
        [SerializeField] private float cardSpacing = 10f;
        [SerializeField] private float loadBatchSize = 20f;
        
        // Data
        private List<GameCard> allCards = new List<GameCard>();
        private List<CollectionCard> userCollection = new List<CollectionCard>();
        private List<GameCard> filteredCards = new List<GameCard>();
        private List<GameObject> cardUIObjects = new List<GameObject>();
        
        // State
        private bool isLoading = false;
        private string currentSearchTerm = "";
        private string currentRarityFilter = "";
        private string currentTypeFilter = "";
        private int currentSortMode = 0; // 0: Name, 1: Rarity, 2: Mana Cost, 3: Attack
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            LoadCollection();
        }
        
        private void InitializeUI()
        {
            // Hide collection panel initially
            if (collectionPanel) collectionPanel.SetActive(false);
            if (cardDetailPanel) cardDetailPanel.SetActive(false);
            
            // Setup filter dropdowns
            SetupFilterDropdowns();
            
            // Setup grid layout for mobile
            SetupMobileGrid();
        }
        
        private void SetupFilterDropdowns()
        {
            // Setup rarity filter
            if (rarityFilter)
            {
                rarityFilter.ClearOptions();
                var rarityOptions = new List<string> { "All Rarities", "Common", "Uncommon", "Rare", "Legendary", "Exotic" };
                rarityFilter.AddOptions(rarityOptions);
            }
            
            // Setup type filter
            if (typeFilter)
            {
                typeFilter.ClearOptions();
                var typeOptions = new List<string> { "All Types", "Unit", "Spell", "Structure" };
                typeFilter.AddOptions(typeOptions);
            }
            
            // Setup sort dropdown
            if (sortDropdown)
            {
                sortDropdown.ClearOptions();
                var sortOptions = new List<string> { "Name", "Rarity", "Mana Cost", "Attack", "Health" };
                sortDropdown.AddOptions(sortOptions);
            }
        }
        
        private void SetupMobileGrid()
        {
            if (cardContainer)
            {
                var gridLayout = cardContainer.GetComponent<GridLayoutGroup>();
                if (gridLayout == null)
                    gridLayout = cardContainer.gameObject.AddComponent<GridLayoutGroup>();
                
                // Calculate card size based on screen width
                float screenWidth = Screen.width;
                float cardWidth = (screenWidth - (cardSpacing * (cardsPerRow + 1))) / cardsPerRow;
                float cardHeight = cardWidth * 1.4f; // Card aspect ratio
                
                gridLayout.cellSize = new Vector2(cardWidth, cardHeight);
                gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = cardsPerRow;
                gridLayout.childAlignment = TextAnchor.UpperCenter;
            }
        }
        
        private void SetupEventListeners()
        {
            // Search and filter events
            if (searchInput) searchInput.onValueChanged.AddListener(OnSearchChanged);
            if (rarityFilter) rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);
            if (typeFilter) typeFilter.onValueChanged.AddListener(OnTypeFilterChanged);
            if (sortDropdown) sortDropdown.onValueChanged.AddListener(OnSortChanged);
            if (clearFiltersButton) clearFiltersButton.onClick.AddListener(OnClearFilters);
            
            // Detail view events
            if (closeDetailButton) closeDetailButton.onClick.AddListener(OnCloseDetail);
            
            // Card service events
            if (CardService.Instance)
            {
                CardService.Instance.OnAllCardsLoaded += OnAllCardsLoaded;
                CardService.Instance.OnUserCollectionLoaded += OnUserCollectionLoaded;
                CardService.Instance.OnError += OnCardServiceError;
            }
        }
        
        #region Collection Loading
        
        private async void LoadCollection()
        {
            if (isLoading) return;
            
            isLoading = true;
            Debug.Log("📚 Loading card collection...");
            
            // Load all cards first
            allCards = await CardService.Instance.LoadAllCards();
            
            // Load user collection if authenticated
            if (SupabaseClient.Instance && SupabaseClient.Instance.IsAuthenticated)
            {
                // TODO: Get user ID from authentication
                // userCollection = await CardService.Instance.LoadUserCollection(userId);
            }
            
            // Apply current filters and display
            ApplyFiltersAndDisplay();
            UpdateCollectionStats();
            
            isLoading = false;
        }
        
        private void OnAllCardsLoaded(List<GameCard> cards)
        {
            allCards = cards;
            ApplyFiltersAndDisplay();
            UpdateCollectionStats();
            
            Debug.Log($"✅ Collection loaded: {cards.Count} total cards");
        }
        
        private void OnUserCollectionLoaded(List<CollectionCard> collection)
        {
            userCollection = collection;
            ApplyFiltersAndDisplay();
            UpdateCollectionStats();
            
            Debug.Log($"✅ User collection loaded: {collection.Count} owned cards");
        }
        
        #endregion
        
        #region Filtering and Sorting
        
        private void ApplyFiltersAndDisplay()
        {
            // Start with all cards
            filteredCards = new List<GameCard>(allCards);
            
            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchTerm))
            {
                filteredCards = filteredCards.Where(card => 
                    card.name.ToLower().Contains(currentSearchTerm.ToLower()) ||
                    card.description.ToLower().Contains(currentSearchTerm.ToLower()) ||
                    card.ability_text.ToLower().Contains(currentSearchTerm.ToLower())
                ).ToList();
            }
            
            // Apply rarity filter
            if (!string.IsNullOrEmpty(currentRarityFilter) && currentRarityFilter != "All Rarities")
            {
                filteredCards = filteredCards.Where(card => 
                    card.rarity.ToLower() == currentRarityFilter.ToLower()
                ).ToList();
            }
            
            // Apply type filter
            if (!string.IsNullOrEmpty(currentTypeFilter) && currentTypeFilter != "All Types")
            {
                filteredCards = filteredCards.Where(card => 
                    card.card_type.ToLower() == currentTypeFilter.ToLower()
                ).ToList();
            }
            
            // Apply sorting
            ApplySorting();
            
            // Display filtered cards
            DisplayCards();
            
            Debug.Log($"🔍 Filtered to {filteredCards.Count} cards");
        }
        
        private void ApplySorting()
        {
            switch (currentSortMode)
            {
                case 0: // Name
                    filteredCards = filteredCards.OrderBy(card => card.name).ToList();
                    break;
                case 1: // Rarity
                    filteredCards = filteredCards.OrderBy(card => GetRarityOrder(card.rarity)).ThenBy(card => card.name).ToList();
                    break;
                case 2: // Mana Cost
                    filteredCards = filteredCards.OrderBy(card => card.mana_cost).ThenBy(card => card.name).ToList();
                    break;
                case 3: // Attack
                    filteredCards = filteredCards.OrderByDescending(card => card.attack).ThenBy(card => card.name).ToList();
                    break;
                case 4: // Health
                    filteredCards = filteredCards.OrderByDescending(card => card.hp).ThenBy(card => card.name).ToList();
                    break;
            }
        }
        
        private int GetRarityOrder(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => 0,
                "uncommon" => 1,
                "rare" => 2,
                "legendary" => 3,
                "exotic" => 4,
                _ => 0
            };
        }
        
        #endregion
        
        #region UI Display
        
        private void DisplayCards()
        {
            // Clear existing card UI objects
            ClearCardDisplay();
            
            // Create card UI objects for filtered cards
            foreach (var card in filteredCards)
            {
                CreateCardUI(card);
            }
            
            // Update scroll view
            if (cardScrollView) cardScrollView.verticalNormalizedPosition = 1f;
        }
        
        private void ClearCardDisplay()
        {
            foreach (var cardUI in cardUIObjects)
            {
                if (cardUI) DestroyImmediate(cardUI);
            }
            cardUIObjects.Clear();
        }
        
        private void CreateCardUI(GameCard card)
        {
            if (!cardUIPrefab || !cardContainer) return;
            
            GameObject cardUIObj = Instantiate(cardUIPrefab, cardContainer);
            var cardUI = cardUIObj.GetComponent<MobileCardUI>();
            
            if (cardUI)
            {
                cardUI.SetCardData(card);
                cardUI.OnCardTapped += OnCardUITapped;
                cardUI.OnCardLongPressed += OnCardUILongPressed;
                cardUI.OnCardDoubleTapped += OnCardUIDoubleTapped;
                
                // Check if user owns this card
                bool isOwned = userCollection.Any(c => c.card.id == card.id);
                cardUI.SetInteractable(isOwned);
            }
            
            cardUIObjects.Add(cardUIObj);
        }
        
        private void UpdateCollectionStats()
        {
            int totalCards = allCards.Count;
            int ownedCards = userCollection.Count;
            int totalOwnedQuantity = userCollection.Sum(c => c.quantity);
            
            if (totalCardsText) totalCardsText.text = $"Total: {totalOwnedQuantity}";
            if (uniqueCardsText) uniqueCardsText.text = $"Unique: {ownedCards}";
            if (collectionProgressText) collectionProgressText.text = $"{ownedCards}/{totalCards}";
            
            if (collectionProgressBar)
            {
                float progress = totalCards > 0 ? (float)ownedCards / totalCards : 0f;
                collectionProgressBar.value = progress;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSearchChanged(string searchTerm)
        {
            currentSearchTerm = searchTerm;
            ApplyFiltersAndDisplay();
        }
        
        private void OnRarityFilterChanged(int index)
        {
            currentRarityFilter = rarityFilter.options[index].text;
            ApplyFiltersAndDisplay();
        }
        
        private void OnTypeFilterChanged(int index)
        {
            currentTypeFilter = typeFilter.options[index].text;
            ApplyFiltersAndDisplay();
        }
        
        private void OnSortChanged(int index)
        {
            currentSortMode = index;
            ApplyFiltersAndDisplay();
        }
        
        private void OnClearFilters()
        {
            currentSearchTerm = "";
            currentRarityFilter = "";
            currentTypeFilter = "";
            
            if (searchInput) searchInput.text = "";
            if (rarityFilter) rarityFilter.value = 0;
            if (typeFilter) typeFilter.value = 0;
            
            ApplyFiltersAndDisplay();
        }
        
        private void OnCardUITapped(MobileCardUI cardUI)
        {
            ShowCardDetail(cardUI.GetCardData());
        }
        
        private void OnCardUILongPressed(MobileCardUI cardUI)
        {
            // Long press could show card actions (add to deck, etc.)
            Debug.Log($"🔧 Long press actions for: {cardUI.GetCardData()?.name}");
        }
        
        private void OnCardUIDoubleTapped(MobileCardUI cardUI)
        {
            // Double tap could quick-add to active deck
            Debug.Log($"➕ Quick add to deck: {cardUI.GetCardData()?.name}");
        }
        
        private void OnCloseDetail()
        {
            if (cardDetailPanel) cardDetailPanel.SetActive(false);
        }
        
        private void OnCardServiceError(string errorMessage)
        {
            Debug.LogError($"❌ Card Service Error: {errorMessage}");
            // TODO: Show error toast to user
        }
        
        #endregion
        
        #region Public Methods
        
        public void ShowCollection()
        {
            if (collectionPanel) collectionPanel.SetActive(true);
            
            // Refresh collection if needed
            if (!isLoading && allCards.Count == 0)
            {
                LoadCollection();
            }
        }
        
        public void HideCollection()
        {
            if (collectionPanel) collectionPanel.SetActive(false);
            if (cardDetailPanel) cardDetailPanel.SetActive(false);
        }
        
        public void RefreshCollection()
        {
            LoadCollection();
        }
        
        private void ShowCardDetail(GameCard card)
        {
            if (!cardDetailPanel || !detailCardDisplay) return;
            
            cardDetailPanel.SetActive(true);
            detailCardDisplay.SetCardData(card);
            
            Debug.Log($"🔍 Showing detail for: {card.name}");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Cleanup event listeners
            if (CardService.Instance)
            {
                CardService.Instance.OnAllCardsLoaded -= OnAllCardsLoaded;
                CardService.Instance.OnUserCollectionLoaded -= OnUserCollectionLoaded;
                CardService.Instance.OnError -= OnCardServiceError;
            }
        }
    }
}