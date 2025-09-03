using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Cards;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// REAL Collection Screen that actually loads and displays your cards
    /// Recreates the FantasyCollection component functionality
    /// </summary>
    public class RealCollectionScreen : MonoBehaviour
    {
        [Header("Collection UI")]
        [SerializeField] private Transform cardGrid;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown rarityFilter;
        [SerializeField] private TMP_Dropdown typeFilter;
        [SerializeField] private TMP_Dropdown elementFilter;
        [SerializeField] private Button clearFiltersButton;
        [SerializeField] private Button backButton;
        
        [Header("Collection Stats")]
        [SerializeField] private TextMeshProUGUI totalCardsText;
        [SerializeField] private TextMeshProUGUI uniqueCardsText;
        [SerializeField] private TextMeshProUGUI completionText;
        [SerializeField] private Slider completionSlider;
        
        [Header("Card Detail")]
        [SerializeField] private GameObject cardDetailPanel;
        [SerializeField] private TextMeshProUGUI detailCardName;
        [SerializeField] private TextMeshProUGUI detailCardDescription;
        [SerializeField] private TextMeshProUGUI detailCardStats;
        [SerializeField] private TextMeshProUGUI detailCardAbilities;
        [SerializeField] private Button closeDetailButton;
        
        // Collection data
        private List<RealSupabaseClient.CollectionItem> userCollection = new List<RealSupabaseClient.CollectionItem>();
        private List<RealSupabaseClient.EnhancedCard> filteredCards = new List<RealSupabaseClient.EnhancedCard>();
        private List<GameObject> cardDisplays = new List<GameObject>();
        
        // Filter state
        private string currentSearch = "";
        private string selectedRarity = "all";
        private string selectedType = "all";
        private string selectedElement = "all";
        
        private void Start()
        {
            CreateCollectionUI();
            SetupEventListeners();
            LoadCollection();
        }
        
        private void CreateCollectionUI()
        {
            Debug.Log("📚 Creating real collection UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            CreateCollectionInterface(canvas.transform);
            
            // Start hidden
            gameObject.SetActive(false);
            
            Debug.Log("✅ Real collection UI created");
        }
        
        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Collection Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        private void CreateCollectionInterface(Transform parent)
        {
            // Background
            GameObject background = CreatePanel("Collection Background", parent);
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.05f, 0.1f, 0.05f, 1f); // Dark green
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Top bar
            CreateTopBar(background.transform);
            
            // Collection stats
            CreateStatsPanel(background.transform);
            
            // Filters
            CreateFiltersPanel(background.transform);
            
            // Card grid
            CreateCardGrid(background.transform);
            
            // Card detail panel
            CreateCardDetailPanel(background.transform);
        }
        
        private void CreateTopBar(Transform parent)
        {
            GameObject topBar = CreatePanel("Top Bar", parent);
            Image topBarBg = topBar.GetComponent<Image>();
            topBarBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 0.9f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.offsetMin = Vector2.zero;
            topBarRect.offsetMax = Vector2.zero;
            
            // Back button
            GameObject backBtnObj = CreatePanel("Back Button", topBar.transform);
            backButton = backBtnObj.AddComponent<Button>();
            Image backBtnImg = backBtnObj.GetComponent<Image>();
            backBtnImg.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            GameObject backTextObj = new GameObject("Back Text");
            backTextObj.transform.SetParent(backBtnObj.transform, false);
            backTextObj.layer = 5;
            
            TextMeshProUGUI backText = backTextObj.AddComponent<TextMeshProUGUI>();
            backText.text = "← BACK";
            backText.fontSize = 24;
            backText.color = Color.white;
            backText.alignment = TextAlignmentOptions.Center;
            backText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(backTextObj.GetComponent<RectTransform>());
            
            RectTransform backBtnRect = backBtnObj.GetComponent<RectTransform>();
            backBtnRect.anchorMin = new Vector2(0.05f, 0.1f);
            backBtnRect.anchorMax = new Vector2(0.25f, 0.9f);
            backBtnRect.offsetMin = Vector2.zero;
            backBtnRect.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Collection Title");
            titleObj.transform.SetParent(topBar.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "📚 CARD COLLECTION";
            titleText.fontSize = 32;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0f);
            titleRect.anchorMax = new Vector2(0.7f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
        }
        
        private void CreateStatsPanel(Transform parent)
        {
            GameObject statsPanel = CreatePanel("Stats Panel", parent);
            Image statsBg = statsPanel.GetComponent<Image>();
            statsBg.color = new Color(0f, 0f, 0f, 0.6f);
            
            RectTransform statsRect = statsPanel.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.05f, 0.8f);
            statsRect.anchorMax = new Vector2(0.95f, 0.88f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // Total cards
            GameObject totalObj = new GameObject("Total Cards");
            totalObj.transform.SetParent(statsPanel.transform, false);
            totalObj.layer = 5;
            
            totalCardsText = totalObj.AddComponent<TextMeshProUGUI>();
            totalCardsText.text = "Total: 0";
            totalCardsText.fontSize = 20;
            totalCardsText.color = Color.white;
            totalCardsText.alignment = TextAlignmentOptions.Left;
            
            RectTransform totalRect = totalObj.GetComponent<RectTransform>();
            totalRect.anchorMin = new Vector2(0.05f, 0f);
            totalRect.anchorMax = new Vector2(0.3f, 1f);
            totalRect.offsetMin = Vector2.zero;
            totalRect.offsetMax = Vector2.zero;
            
            // Unique cards
            GameObject uniqueObj = new GameObject("Unique Cards");
            uniqueObj.transform.SetParent(statsPanel.transform, false);
            uniqueObj.layer = 5;
            
            uniqueCardsText = uniqueObj.AddComponent<TextMeshProUGUI>();
            uniqueCardsText.text = "Unique: 0";
            uniqueCardsText.fontSize = 20;
            uniqueCardsText.color = Color.white;
            uniqueCardsText.alignment = TextAlignmentOptions.Center;
            
            RectTransform uniqueRect = uniqueObj.GetComponent<RectTransform>();
            uniqueRect.anchorMin = new Vector2(0.35f, 0f);
            uniqueRect.anchorMax = new Vector2(0.65f, 1f);
            uniqueRect.offsetMin = Vector2.zero;
            uniqueRect.offsetMax = Vector2.zero;
            
            // Completion
            GameObject completionObj = new GameObject("Completion");
            completionObj.transform.SetParent(statsPanel.transform, false);
            completionObj.layer = 5;
            
            completionText = completionObj.AddComponent<TextMeshProUGUI>();
            completionText.text = "0%";
            completionText.fontSize = 20;
            completionText.color = new Color(1f, 0.8f, 0.2f, 1f);
            completionText.alignment = TextAlignmentOptions.Right;
            
            RectTransform completionRect = completionObj.GetComponent<RectTransform>();
            completionRect.anchorMin = new Vector2(0.7f, 0f);
            completionRect.anchorMax = new Vector2(0.95f, 1f);
            completionRect.offsetMin = Vector2.zero;
            completionRect.offsetMax = Vector2.zero;
        }
        
        private void CreateFiltersPanel(Transform parent)
        {
            GameObject filtersPanel = CreatePanel("Filters Panel", parent);
            Image filtersBg = filtersPanel.GetComponent<Image>();
            filtersBg.color = new Color(0f, 0f, 0f, 0.4f);
            
            RectTransform filtersRect = filtersPanel.GetComponent<RectTransform>();
            filtersRect.anchorMin = new Vector2(0.05f, 0.72f);
            filtersRect.anchorMax = new Vector2(0.95f, 0.78f);
            filtersRect.offsetMin = Vector2.zero;
            filtersRect.offsetMax = Vector2.zero;
            
            // Search field
            searchField = CreateSearchField(filtersPanel.transform);
            
            // TODO: Create dropdown filters for rarity, type, element
            // For now, just create placeholder text
            GameObject filtersText = new GameObject("Filters Text");
            filtersText.transform.SetParent(filtersPanel.transform, false);
            filtersText.layer = 5;
            
            TextMeshProUGUI filtersTMP = filtersText.AddComponent<TextMeshProUGUI>();
            filtersTMP.text = "🔍 Search and filter your cards";
            filtersTMP.fontSize = 18;
            filtersTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            filtersTMP.alignment = TextAlignmentOptions.Right;
            
            RectTransform filtersTextRect = filtersText.GetComponent<RectTransform>();
            filtersTextRect.anchorMin = new Vector2(0.5f, 0f);
            filtersTextRect.anchorMax = new Vector2(1f, 1f);
            filtersTextRect.offsetMin = Vector2.zero;
            filtersTextRect.offsetMax = Vector2.zero;
        }
        
        private TMP_InputField CreateSearchField(Transform parent)
        {
            GameObject searchObj = new GameObject("Search Field");
            searchObj.transform.SetParent(parent, false);
            searchObj.layer = 5;
            
            Image searchBg = searchObj.AddComponent<Image>();
            searchBg.color = new Color(1f, 1f, 1f, 0.9f);
            
            TMP_InputField inputField = searchObj.AddComponent<TMP_InputField>();
            
            // Text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(searchObj.transform, false);
            textArea.layer = 5;
            textArea.AddComponent<RectMask2D>();
            
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = new Vector2(0.05f, 0f);
            textAreaRect.anchorMax = new Vector2(0.95f, 1f);
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;
            
            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            placeholderObj.layer = 5;
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Search cards...";
            placeholderText.fontSize = 20;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.raycastTarget = false;
            
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());
            
            // Input text
            GameObject inputTextObj = new GameObject("Input Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 20;
            inputText.color = Color.black;
            inputText.raycastTarget = false;
            
            SetFullScreen(inputTextObj.GetComponent<RectTransform>());
            
            // Connect input field
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            // Position
            RectTransform searchRect = searchObj.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0.05f, 0.2f);
            searchRect.anchorMax = new Vector2(0.45f, 0.8f);
            searchRect.offsetMin = Vector2.zero;
            searchRect.offsetMax = Vector2.zero;
            
            return inputField;
        }
        
        private void CreateCardGrid(Transform parent)
        {
            // Scroll view
            GameObject scrollViewObj = new GameObject("Card Scroll View");
            scrollViewObj.transform.SetParent(parent, false);
            scrollViewObj.layer = 5;
            
            scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollViewObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform scrollViewRect = scrollViewObj.GetComponent<RectTransform>();
            scrollViewRect.anchorMin = new Vector2(0.05f, 0.1f);
            scrollViewRect.anchorMax = new Vector2(0.95f, 0.7f);
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<RectMask2D>();
            
            SetFullScreen(viewport.GetComponent<RectTransform>());
            this.scrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            this.scrollRect.content = contentRect;
            
            // Grid layout
            GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(150, 200); // Card size
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 6; // 6 cards per row
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            cardGrid = content.transform;
        }
        
        private void CreateCardDetailPanel(Transform parent)
        {
            cardDetailPanel = CreatePanel("Card Detail Panel", parent);
            Image detailBg = cardDetailPanel.GetComponent<Image>();
            detailBg.color = new Color(0f, 0f, 0f, 0.9f);
            
            SetFullScreen(cardDetailPanel.GetComponent<RectTransform>());
            cardDetailPanel.SetActive(false);
            
            // Detail content
            GameObject detailContent = CreatePanel("Detail Content", cardDetailPanel.transform);
            detailContent.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.2f, 0.95f);
            
            RectTransform detailRect = detailContent.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0.1f, 0.2f);
            detailRect.anchorMax = new Vector2(0.9f, 0.8f);
            detailRect.offsetMin = Vector2.zero;
            detailRect.offsetMax = Vector2.zero;
            
            // Card name
            GameObject nameObj = new GameObject("Card Name");
            nameObj.transform.SetParent(detailContent.transform, false);
            nameObj.layer = 5;
            
            detailCardName = nameObj.AddComponent<TextMeshProUGUI>();
            detailCardName.text = "Card Name";
            detailCardName.fontSize = 36;
            detailCardName.color = new Color(1f, 0.8f, 0.2f, 1f);
            detailCardName.alignment = TextAlignmentOptions.Center;
            detailCardName.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.1f, 0.8f);
            nameRect.anchorMax = new Vector2(0.9f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Card stats
            GameObject statsObj = new GameObject("Card Stats");
            statsObj.transform.SetParent(detailContent.transform, false);
            statsObj.layer = 5;
            
            detailCardStats = statsObj.AddComponent<TextMeshProUGUI>();
            detailCardStats.text = "Mana: 0 | Attack: 0 | Health: 0";
            detailCardStats.fontSize = 24;
            detailCardStats.color = Color.white;
            detailCardStats.alignment = TextAlignmentOptions.Center;
            
            RectTransform statsObjRect = statsObj.GetComponent<RectTransform>();
            statsObjRect.anchorMin = new Vector2(0.1f, 0.65f);
            statsObjRect.anchorMax = new Vector2(0.9f, 0.75f);
            statsObjRect.offsetMin = Vector2.zero;
            statsObjRect.offsetMax = Vector2.zero;
            
            // Card description
            GameObject descObj = new GameObject("Card Description");
            descObj.transform.SetParent(detailContent.transform, false);
            descObj.layer = 5;
            
            detailCardDescription = descObj.AddComponent<TextMeshProUGUI>();
            detailCardDescription.text = "Card description";
            detailCardDescription.fontSize = 20;
            detailCardDescription.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            detailCardDescription.alignment = TextAlignmentOptions.Center;
            
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.4f);
            descRect.anchorMax = new Vector2(0.9f, 0.6f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            
            // Card abilities
            GameObject abilitiesObj = new GameObject("Card Abilities");
            abilitiesObj.transform.SetParent(detailContent.transform, false);
            abilitiesObj.layer = 5;
            
            detailCardAbilities = abilitiesObj.AddComponent<TextMeshProUGUI>();
            detailCardAbilities.text = "Card abilities";
            detailCardAbilities.fontSize = 18;
            detailCardAbilities.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            detailCardAbilities.alignment = TextAlignmentOptions.Center;
            
            RectTransform abilitiesRect = abilitiesObj.GetComponent<RectTransform>();
            abilitiesRect.anchorMin = new Vector2(0.1f, 0.2f);
            abilitiesRect.anchorMax = new Vector2(0.9f, 0.35f);
            abilitiesRect.offsetMin = Vector2.zero;
            abilitiesRect.offsetMax = Vector2.zero;
            
            // Close button
            GameObject closeBtnObj = CreatePanel("Close Button", detailContent.transform);
            closeDetailButton = closeBtnObj.AddComponent<Button>();
            Image closeBtnImg = closeBtnObj.GetComponent<Image>();
            closeBtnImg.color = new Color(0.7f, 0.3f, 0.3f, 1f);
            
            GameObject closeTextObj = new GameObject("Close Text");
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            closeTextObj.layer = 5;
            
            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "✖ CLOSE";
            closeText.fontSize = 24;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(closeTextObj.GetComponent<RectTransform>());
            
            RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.3f, 0.05f);
            closeBtnRect.anchorMax = new Vector2(0.7f, 0.15f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;
        }
        
        private void SetupEventListeners()
        {
            // UI events
            if (backButton) backButton.onClick.AddListener(OnBackPressed);
            if (closeDetailButton) closeDetailButton.onClick.AddListener(OnCloseDetailPressed);
            if (searchField) searchField.onValueChanged.AddListener(OnSearchChanged);
            
            // Data events
            if (RealCardManager.Instance != null)
            {
                RealCardManager.Instance.OnCollectionLoaded += OnCollectionLoaded;
            }
            
            // Game flow events
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged += OnGameFlowChanged;
            }
        }
        
        private async void LoadCollection()
        {
            Debug.Log("🔄 Loading real collection data...");
            
            if (RealCardManager.Instance != null)
            {
                await RealCardManager.Instance.LoadUserCollectionFromDatabase();
            }
        }
        
        private void OnCollectionLoaded()
        {
            if (RealCardManager.Instance == null) return;
            
            userCollection = RealCardManager.Instance.GetUserCollection();
            UpdateCollectionStats();
            DisplayCards();
            
            Debug.Log($"✅ Collection UI updated with {userCollection.Count} cards");
        }
        
        private void UpdateCollectionStats()
        {
            if (RealCardManager.Instance == null) return;
            
            var stats = RealCardManager.Instance.GetCollectionStats();
            
            if (totalCardsText) totalCardsText.text = $"Total: {stats["total_cards"]}";
            if (uniqueCardsText) uniqueCardsText.text = $"Unique: {stats["unique_cards"]}";
            
            // Calculate completion percentage (assuming 236 total cards like web version)
            float completion = stats["unique_cards"] / 236f * 100f;
            if (completionText) completionText.text = $"{completion:F1}%";
            if (completionSlider) completionSlider.value = completion / 100f;
        }
        
        private void DisplayCards()
        {
            // Clear existing displays
            foreach (GameObject cardDisplay in cardDisplays)
            {
                if (cardDisplay != null) Destroy(cardDisplay);
            }
            cardDisplays.Clear();
            
            // Apply filters
            ApplyFilters();
            
            // Create card displays
            foreach (var collectionItem in filteredCards.Take(50)) // Limit for performance
            {
                if (RealCardManager.Instance != null)
                {
                    GameObject cardObj = RealCardManager.Instance.CreateCardDisplay(collectionItem, cardGrid, true);
                    if (cardObj != null)
                    {
                        cardDisplays.Add(cardObj);
                        
                        // Add click handler
                        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
                        if (cardDisplay != null)
                        {
                            cardDisplay.OnCardClicked += OnCardClicked;
                        }
                    }
                }
            }
            
            Debug.Log($"🎴 Displayed {cardDisplays.Count} cards");
        }
        
        private void ApplyFilters()
        {
            var ownedCards = userCollection.Where(item => item.quantity > 0).Select(item => item.card);
            
            filteredCards = ownedCards.Where(card =>
            {
                // Search filter
                if (!string.IsNullOrEmpty(currentSearch))
                {
                    bool matchesSearch = card.name.ToLower().Contains(currentSearch.ToLower()) ||
                                       card.description.ToLower().Contains(currentSearch.ToLower());
                    if (!matchesSearch) return false;
                }
                
                // Rarity filter
                if (selectedRarity != "all" && card.rarity != selectedRarity)
                    return false;
                
                // Type filter
                if (selectedType != "all" && card.card_type != selectedType)
                    return false;
                
                // Element filter
                if (selectedElement != "all" && card.potato_type != selectedElement)
                    return false;
                
                return true;
                
            }).ToList();
        }
        
        private void OnSearchChanged(string searchTerm)
        {
            currentSearch = searchTerm;
            DisplayCards();
        }
        
        private void OnCardClicked(CardDisplay cardDisplay)
        {
            var cardData = cardDisplay.GetCardData();
            if (cardData == null) return;
            
            // Find the enhanced card data
            var enhancedCard = userCollection.FirstOrDefault(item => item.card.id == cardData.cardId)?.card;
            if (enhancedCard != null)
            {
                ShowCardDetail(enhancedCard);
            }
        }
        
        private void ShowCardDetail(RealSupabaseClient.EnhancedCard card)
        {
            if (cardDetailPanel == null) return;
            
            // Update detail UI
            if (detailCardName) detailCardName.text = card.name;
            if (detailCardDescription) detailCardDescription.text = card.description;
            
            if (detailCardStats)
            {
                string stats = $"Mana: {card.mana_cost}";
                if (card.attack.HasValue) stats += $" | Attack: {card.attack.Value}";
                if (card.hp.HasValue) stats += $" | Health: {card.hp.Value}";
                detailCardStats.text = stats;
            }
            
            if (detailCardAbilities)
            {
                detailCardAbilities.text = card.ability_text ?? "No special abilities";
            }
            
            cardDetailPanel.SetActive(true);
        }
        
        private void OnCloseDetailPressed()
        {
            if (cardDetailPanel) cardDetailPanel.SetActive(false);
        }
        
        private void OnBackPressed()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.NavigateToMainMenu();
            }
        }
        
        private void OnGameFlowChanged(GameFlowManager.GameFlow newFlow)
        {
            bool shouldShow = newFlow == GameFlowManager.GameFlow.Collection;
            gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                LoadCollection(); // Refresh when shown
            }
        }
        
        #region Helper Methods
        
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.layer = 5;
            
            panel.AddComponent<RectTransform>();
            panel.AddComponent<CanvasRenderer>();
            panel.AddComponent<Image>();
            
            return panel;
        }
        
        private void SetFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (RealCardManager.Instance != null)
            {
                RealCardManager.Instance.OnCollectionLoaded -= OnCollectionLoaded;
            }
            
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged -= OnGameFlowChanged;
            }
        }
    }
}