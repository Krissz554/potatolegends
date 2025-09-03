using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Data;
using PotatoCardGame.Cards;
using PotatoCardGame.Network;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Manages the card collection interface for mobile
    /// Displays user's cards with filtering, search, and detailed view capabilities
    /// </summary>
    public class CollectionController : MonoBehaviour
    {
        [Header("Collection Display")]
        [SerializeField] private Transform cardGrid;
        [SerializeField] private GameObject cardDisplayPrefab;
        [SerializeField] private ScrollRect scrollRect;
        
        [Header("Search and Filter")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown rarityFilter;
        [SerializeField] private TMP_Dropdown elementFilter;
        [SerializeField] private TMP_Dropdown typeFilter;
        [SerializeField] private Button clearFiltersButton;
        
        [Header("Collection Stats")]
        [SerializeField] private TextMeshProUGUI totalCardsText;
        [SerializeField] private TextMeshProUGUI uniqueCardsText;
        [SerializeField] private TextMeshProUGUI collectionProgressText;
        [SerializeField] private Slider collectionProgressSlider;
        
        [Header("Card Detail View")]
        [SerializeField] private GameObject cardDetailPanel;
        [SerializeField] private Image detailCardArt;
        [SerializeField] private TextMeshProUGUI detailCardName;
        [SerializeField] private TextMeshProUGUI detailCardDescription;
        [SerializeField] private TextMeshProUGUI detailCardStats;
        [SerializeField] private TextMeshProUGUI detailCardAbilities;
        [SerializeField] private Button closeDetailButton;
        
        [Header("Collection Actions")]
        [SerializeField] private Button favoriteButton;
        [SerializeField] private Button addToDeckButton;
        [SerializeField] private TextMeshProUGUI cardCountText;
        
        [Header("Loading")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingText;
        
        // Collection data
        private List<CardData> userCollection = new List<CardData>();
        private List<CardData> filteredCollection = new List<CardData>();
        private List<GameObject> cardDisplayObjects = new List<GameObject>();
        private CardData selectedCard;
        
        // Filter state
        private string currentSearchTerm = "";
        private Rarity selectedRarity = (Rarity)(-1); // -1 means all
        private ElementType selectedElement = (ElementType)(-1);
        private CardType selectedType = (CardType)(-1);
        
        private void Start()
        {
            SetupUI();
            LoadCollection();
        }
        
        private void SetupUI()
        {
            // Setup search
            if (searchField)
            {
                searchField.onValueChanged.AddListener(OnSearchChanged);
            }
            
            // Setup filters
            if (rarityFilter)
            {
                PopulateRarityFilter();
                rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);
            }
            
            if (elementFilter)
            {
                PopulateElementFilter();
                elementFilter.onValueChanged.AddListener(OnElementFilterChanged);
            }
            
            if (typeFilter)
            {
                PopulateTypeFilter();
                typeFilter.onValueChanged.AddListener(OnTypeFilterChanged);
            }
            
            // Setup buttons
            if (clearFiltersButton) clearFiltersButton.onClick.AddListener(ClearAllFilters);
            if (closeDetailButton) closeDetailButton.onClick.AddListener(CloseCardDetail);
            if (favoriteButton) favoriteButton.onClick.AddListener(ToggleFavorite);
            if (addToDeckButton) addToDeckButton.onClick.AddListener(AddSelectedCardToDeck);
            
            // Hide detail panel initially
            if (cardDetailPanel) cardDetailPanel.SetActive(false);
        }
        
        private async void LoadCollection()
        {
            ShowLoading(true, "Loading your collection...");
            
            try
            {
                if (CardManager.Instance != null)
                {
                    // Subscribe to collection events
                    CardManager.Instance.OnCollectionLoaded += OnCollectionLoaded;
                    
                    // Load collection from database
                    await CardManager.Instance.LoadUserCollection();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading collection: {e.Message}");
                ShowLoading(false);
            }
        }
        
        private void OnCollectionLoaded()
        {
            Debug.Log("📚 Collection loaded in UI");
            
            userCollection = CardManager.Instance.GetUserCollection();
            ApplyFilters();
            UpdateCollectionStats();
            ShowLoading(false);
        }
        
        #region Filtering and Search
        
        private void OnSearchChanged(string searchTerm)
        {
            currentSearchTerm = searchTerm.ToLower();
            ApplyFilters();
        }
        
        private void OnRarityFilterChanged(int index)
        {
            selectedRarity = index == 0 ? (Rarity)(-1) : (Rarity)(index - 1);
            ApplyFilters();
        }
        
        private void OnElementFilterChanged(int index)
        {
            selectedElement = index == 0 ? (ElementType)(-1) : (ElementType)(index - 1);
            ApplyFilters();
        }
        
        private void OnTypeFilterChanged(int index)
        {
            selectedType = index == 0 ? (CardType)(-1) : (CardType)(index - 1);
            ApplyFilters();
        }
        
        private void ApplyFilters()
        {
            filteredCollection = userCollection.Where(card =>
            {
                // Search filter
                if (!string.IsNullOrEmpty(currentSearchTerm))
                {
                    bool matchesSearch = card.cardName.ToLower().Contains(currentSearchTerm) ||
                                       card.description.ToLower().Contains(currentSearchTerm) ||
                                       card.potatoType.ToLower().Contains(currentSearchTerm);
                    if (!matchesSearch) return false;
                }
                
                // Rarity filter
                if (selectedRarity != (Rarity)(-1) && card.rarity != selectedRarity)
                    return false;
                
                // Element filter
                if (selectedElement != (ElementType)(-1) && card.elementType != selectedElement)
                    return false;
                
                // Type filter
                if (selectedType != (CardType)(-1) && card.cardType != selectedType)
                    return false;
                
                return true;
            }).ToList();
            
            DisplayFilteredCards();
            
            Debug.Log($"🔍 Filtered collection: {filteredCollection.Count}/{userCollection.Count} cards");
        }
        
        private void ClearAllFilters()
        {
            currentSearchTerm = "";
            selectedRarity = (Rarity)(-1);
            selectedElement = (ElementType)(-1);
            selectedType = (CardType)(-1);
            
            if (searchField) searchField.text = "";
            if (rarityFilter) rarityFilter.value = 0;
            if (elementFilter) elementFilter.value = 0;
            if (typeFilter) typeFilter.value = 0;
            
            ApplyFilters();
        }
        
        #endregion
        
        #region Card Display
        
        private void DisplayFilteredCards()
        {
            // Clear existing card displays
            foreach (GameObject cardObj in cardDisplayObjects)
            {
                if (cardObj != null) Destroy(cardObj);
            }
            cardDisplayObjects.Clear();
            
            // Create new card displays
            foreach (CardData card in filteredCollection)
            {
                CreateCardDisplay(card);
            }
            
            // Reset scroll position
            if (scrollRect) scrollRect.normalizedPosition = Vector2.up;
        }
        
        private void CreateCardDisplay(CardData card)
        {
            if (cardDisplayPrefab == null || cardGrid == null) return;
            
            GameObject cardObj = Instantiate(cardDisplayPrefab, cardGrid);
            cardDisplayObjects.Add(cardObj);
            
            CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Initialize(card, false); // Not in hand
                cardDisplay.OnCardClicked += OnCardClicked;
                
                // Show card count
                int cardCount = CardManager.Instance.GetCardCount(card.cardId);
                // TODO: Display card count on the card UI
            }
        }
        
        private void OnCardClicked(CardDisplay cardDisplay)
        {
            selectedCard = cardDisplay.GetCardData();
            ShowCardDetail(selectedCard);
            
            Debug.Log($"🃏 Card selected: {selectedCard.cardName}");
        }
        
        private void ShowCardDetail(CardData card)
        {
            if (cardDetailPanel == null) return;
            
            // Update detail UI
            if (detailCardName) detailCardName.text = card.cardName;
            if (detailCardDescription) detailCardDescription.text = card.description;
            if (detailCardArt && card.cardArt) detailCardArt.sprite = card.cardArt;
            
            if (detailCardStats)
            {
                detailCardStats.text = $"Mana: {card.manaCost} | Attack: {card.attack} | Health: {card.health}";
            }
            
            if (detailCardAbilities)
            {
                detailCardAbilities.text = card.abilityText;
            }
            
            if (cardCountText)
            {
                int count = CardManager.Instance.GetCardCount(card.cardId);
                cardCountText.text = $"Owned: {count}";
            }
            
            // Show panel with animation
            cardDetailPanel.SetActive(true);
            
            CanvasGroup canvasGroup = cardDetailPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = cardDetailPanel.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0f;
            StartCoroutine(AnimateFade(canvasGroup, 1f, 0.3f));
            
            cardDetailPanel.transform.localScale = Vector3.one * 0.8f;
            StartCoroutine(AnimateScale(cardDetailPanel.transform, Vector3.one, 0.3f));
        }
        
        private void CloseCardDetail()
        {
            if (cardDetailPanel == null) return;
            
            CanvasGroup canvasGroup = cardDetailPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(AnimateFadeAndClose(canvasGroup, cardDetailPanel, 0.2f));
            }
            else
            {
                cardDetailPanel.SetActive(false);
            }
        }
        
        #endregion
        
        #region Collection Stats
        
        private void UpdateCollectionStats()
        {
            int totalCards = userCollection.Sum(card => CardManager.Instance.GetCardCount(card.cardId));
            int uniqueCards = userCollection.Count;
            
            // TODO: Get total possible cards from database
            int totalPossibleCards = 236; // From your web game
            float collectionProgress = (float)uniqueCards / totalPossibleCards;
            
            if (totalCardsText) totalCardsText.text = $"Total Cards: {totalCards}";
            if (uniqueCardsText) uniqueCardsText.text = $"Unique Cards: {uniqueCards}";
            if (collectionProgressText) collectionProgressText.text = $"Collection: {uniqueCards}/{totalPossibleCards} ({collectionProgress:P0})";
            if (collectionProgressSlider) collectionProgressSlider.value = collectionProgress;
        }
        
        #endregion
        
        #region Filter Setup
        
        private void PopulateRarityFilter()
        {
            if (rarityFilter == null) return;
            
            rarityFilter.ClearOptions();
            List<string> options = new List<string> { "All Rarities" };
            
            foreach (Rarity rarity in System.Enum.GetValues(typeof(Rarity)))
            {
                options.Add(rarity.ToString());
            }
            
            rarityFilter.AddOptions(options);
        }
        
        private void PopulateElementFilter()
        {
            if (elementFilter == null) return;
            
            elementFilter.ClearOptions();
            List<string> options = new List<string> { "All Elements" };
            
            foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
            {
                options.Add(element.ToString());
            }
            
            elementFilter.AddOptions(options);
        }
        
        private void PopulateTypeFilter()
        {
            if (typeFilter == null) return;
            
            typeFilter.ClearOptions();
            List<string> options = new List<string> { "All Types" };
            
            foreach (CardType type in System.Enum.GetValues(typeof(CardType)))
            {
                options.Add(type.ToString());
            }
            
            typeFilter.AddOptions(options);
        }
        
        #endregion
        
        #region Card Actions
        
        private void ToggleFavorite()
        {
            if (selectedCard == null) return;
            
            // TODO: Implement favorite functionality with database
            Debug.Log($"⭐ Toggle favorite: {selectedCard.cardName}");
        }
        
        private void AddSelectedCardToDeck()
        {
            if (selectedCard == null) return;
            
            // TODO: Implement add to deck functionality
            Debug.Log($"➕ Add to deck: {selectedCard.cardName}");
            
            // For now, just close the detail panel
            CloseCardDetail();
        }
        
        #endregion
        
        #region Loading
        
        private void ShowLoading(bool show, string message = "Loading...")
        {
            if (loadingPanel) loadingPanel.SetActive(show);
            if (loadingText) loadingText.text = message;
        }
        
        #endregion
        
        private void OnEnable()
        {
            // Subscribe to card manager events
            if (CardManager.Instance != null)
            {
                CardManager.Instance.OnCollectionLoaded += OnCollectionLoaded;
                CardManager.Instance.OnCardUnlocked += OnCardUnlocked;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            if (CardManager.Instance != null)
            {
                CardManager.Instance.OnCollectionLoaded -= OnCollectionLoaded;
                CardManager.Instance.OnCardUnlocked -= OnCardUnlocked;
            }
        }
        
        private void OnCardUnlocked(CardData newCard)
        {
            Debug.Log($"🎉 New card unlocked: {newCard.cardName}");
            
            // Refresh collection display
            LoadCollection();
            
            // TODO: Show unlock animation
        }
        
        #region Animation Helpers
        
        private IEnumerator AnimateFade(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
        }
        
        private IEnumerator AnimateScale(Transform transform, Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = Mathf.SmoothStep(0f, 1f, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
        }
        
        private IEnumerator AnimateFadeAndClose(CanvasGroup canvasGroup, GameObject panel, float duration)
        {
            yield return StartCoroutine(AnimateFade(canvasGroup, 0f, duration));
            panel.SetActive(false);
        }
        
        #endregion
    }
}