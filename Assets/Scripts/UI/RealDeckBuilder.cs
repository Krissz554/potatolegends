using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Cards;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// REAL Deck Builder that actually builds 30-card decks with validation
    /// Recreates the FantasyDeckBuilder component functionality
    /// </summary>
    public class RealDeckBuilder : MonoBehaviour
    {
        [Header("Deck Builder UI")]
        [SerializeField] private Transform availableCardsGrid;
        [SerializeField] private Transform deckCardsGrid;
        [SerializeField] private ScrollRect availableScrollRect;
        [SerializeField] private ScrollRect deckScrollRect;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Button backButton;
        [SerializeField] private Button saveDeckButton;
        [SerializeField] private Button newDeckButton;
        [SerializeField] private TMP_InputField deckNameField;
        
        [Header("Deck Info")]
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI manaCurveText;
        [SerializeField] private TextMeshProUGUI validationText;
        [SerializeField] private Slider deckProgressSlider;
        
        // Current deck data
        private RealSupabaseClient.Deck currentDeck;
        private Dictionary<string, int> deckCardCounts = new Dictionary<string, int>();
        private List<RealSupabaseClient.CollectionItem> availableCards = new List<RealSupabaseClient.CollectionItem>();
        private List<GameObject> availableCardDisplays = new List<GameObject>();
        private List<GameObject> deckCardDisplays = new List<GameObject>();
        
        // Filters
        private string currentSearch = "";
        
        private void Start()
        {
            CreateDeckBuilderUI();
            SetupEventListeners();
            LoadAvailableCards();
            CreateNewDeck();
        }
        
        private void CreateDeckBuilderUI()
        {
            Debug.Log("🔧 Creating real deck builder UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            CreateDeckBuilderInterface(canvas.transform);
            
            // Start hidden
            gameObject.SetActive(false);
            
            Debug.Log("✅ Real deck builder UI created");
        }
        
        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Deck Builder Canvas");
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
        
        private void CreateDeckBuilderInterface(Transform parent)
        {
            // Background
            GameObject background = CreatePanel("Deck Builder Background", parent);
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.05f, 0.15f, 1f); // Dark purple
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Top bar with title and controls
            CreateTopBar(background.transform);
            
            // Deck info panel
            CreateDeckInfoPanel(background.transform);
            
            // Split view: Available cards (left) | Current deck (right)
            CreateSplitView(background.transform);
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
            backText.fontSize = 20;
            backText.color = Color.white;
            backText.alignment = TextAlignmentOptions.Center;
            backText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(backTextObj.GetComponent<RectTransform>());
            
            RectTransform backBtnRect = backBtnObj.GetComponent<RectTransform>();
            backBtnRect.anchorMin = new Vector2(0.02f, 0.1f);
            backBtnRect.anchorMax = new Vector2(0.18f, 0.9f);
            backBtnRect.offsetMin = Vector2.zero;
            backBtnRect.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Deck Builder Title");
            titleObj.transform.SetParent(topBar.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🔧 DECK BUILDER";
            titleText.fontSize = 28;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0f);
            titleRect.anchorMax = new Vector2(0.6f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Save button
            GameObject saveBtnObj = CreatePanel("Save Button", topBar.transform);
            saveDeckButton = saveBtnObj.AddComponent<Button>();
            Image saveBtnImg = saveBtnObj.GetComponent<Image>();
            saveBtnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green
            
            GameObject saveTextObj = new GameObject("Save Text");
            saveTextObj.transform.SetParent(saveBtnObj.transform, false);
            saveTextObj.layer = 5;
            
            TextMeshProUGUI saveText = saveTextObj.AddComponent<TextMeshProUGUI>();
            saveText.text = "💾 SAVE";
            saveText.fontSize = 18;
            saveText.color = Color.white;
            saveText.alignment = TextAlignmentOptions.Center;
            saveText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(saveTextObj.GetComponent<RectTransform>());
            
            RectTransform saveBtnRect = saveBtnObj.GetComponent<RectTransform>();
            saveBtnRect.anchorMin = new Vector2(0.82f, 0.1f);
            saveBtnRect.anchorMax = new Vector2(0.98f, 0.9f);
            saveBtnRect.offsetMin = Vector2.zero;
            saveBtnRect.offsetMax = Vector2.zero;
        }
        
        private void CreateDeckInfoPanel(Transform parent)
        {
            GameObject infoPanel = CreatePanel("Deck Info Panel", parent);
            Image infoBg = infoPanel.GetComponent<Image>();
            infoBg.color = new Color(0f, 0f, 0f, 0.6f);
            
            RectTransform infoRect = infoPanel.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.05f, 0.8f);
            infoRect.anchorMax = new Vector2(0.95f, 0.88f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            
            // Deck name field
            GameObject nameFieldObj = CreatePanel("Deck Name Field", infoPanel.transform);
            nameFieldObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            
            RectTransform nameFieldRect = nameFieldObj.GetComponent<RectTransform>();
            nameFieldRect.anchorMin = new Vector2(0.05f, 0.1f);
            nameFieldRect.anchorMax = new Vector2(0.35f, 0.9f);
            nameFieldRect.offsetMin = Vector2.zero;
            nameFieldRect.offsetMax = Vector2.zero;
            
            deckNameField = nameFieldObj.AddComponent<TMP_InputField>();
            deckNameField.text = "New Deck";
            
            // Text component for input field
            GameObject nameTextObj = new GameObject("Name Text");
            nameTextObj.transform.SetParent(nameFieldObj.transform, false);
            nameTextObj.layer = 5;
            
            TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "New Deck";
            nameText.fontSize = 18;
            nameText.color = Color.black;
            
            SetFullScreen(nameTextObj.GetComponent<RectTransform>());
            deckNameField.textComponent = nameText;
            
            // Deck count
            GameObject countObj = new GameObject("Deck Count");
            countObj.transform.SetParent(infoPanel.transform, false);
            countObj.layer = 5;
            
            deckCountText = countObj.AddComponent<TextMeshProUGUI>();
            deckCountText.text = "Cards: 0/30";
            deckCountText.fontSize = 20;
            deckCountText.color = Color.white;
            deckCountText.alignment = TextAlignmentOptions.Center;
            deckCountText.fontStyle = FontStyles.Bold;
            
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.4f, 0f);
            countRect.anchorMax = new Vector2(0.6f, 1f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
            
            // Validation status
            GameObject validationObj = new GameObject("Validation");
            validationObj.transform.SetParent(infoPanel.transform, false);
            validationObj.layer = 5;
            
            validationText = validationObj.AddComponent<TextMeshProUGUI>();
            validationText.text = "❌ Invalid Deck";
            validationText.fontSize = 18;
            validationText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red
            validationText.alignment = TextAlignmentOptions.Right;
            
            RectTransform validationRect = validationObj.GetComponent<RectTransform>();
            validationRect.anchorMin = new Vector2(0.65f, 0f);
            validationRect.anchorMax = new Vector2(0.95f, 1f);
            validationRect.offsetMin = Vector2.zero;
            validationRect.offsetMax = Vector2.zero;
        }
        
        private void CreateSplitView(Transform parent)
        {
            // Available cards section (left)
            GameObject availableSection = CreatePanel("Available Cards Section", parent);
            availableSection.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.05f, 0.8f);
            
            RectTransform availableRect = availableSection.GetComponent<RectTransform>();
            availableRect.anchorMin = new Vector2(0.02f, 0.1f);
            availableRect.anchorMax = new Vector2(0.48f, 0.78f);
            availableRect.offsetMin = Vector2.zero;
            availableRect.offsetMax = Vector2.zero;
            
            CreateAvailableCardsArea(availableSection.transform);
            
            // Current deck section (right)
            GameObject deckSection = CreatePanel("Current Deck Section", parent);
            deckSection.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.1f, 0.8f);
            
            RectTransform deckRect = deckSection.GetComponent<RectTransform>();
            deckRect.anchorMin = new Vector2(0.52f, 0.1f);
            deckRect.anchorMax = new Vector2(0.98f, 0.78f);
            deckRect.offsetMin = Vector2.zero;
            deckRect.offsetMax = Vector2.zero;
            
            CreateCurrentDeckArea(deckSection.transform);
        }
        
        private void CreateAvailableCardsArea(Transform parent)
        {
            // Title
            GameObject titleObj = new GameObject("Available Cards Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "📚 YOUR COLLECTION";
            titleText.fontSize = 20;
            titleText.color = new Color(0.8f, 1f, 0.8f, 1f); // Light green
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Search field
            searchField = CreateSearchField(parent, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.88f));
            
            // Available cards grid
            availableCardsGrid = CreateCardGrid(parent, "Available Cards Grid", 
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.8f), 4); // 4 columns
        }
        
        private void CreateCurrentDeckArea(Transform parent)
        {
            // Title
            GameObject titleObj = new GameObject("Current Deck Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🃏 CURRENT DECK";
            titleText.fontSize = 20;
            titleText.color = new Color(1f, 0.8f, 1f, 1f); // Light purple
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Deck name field
            deckNameField = CreateDeckNameField(parent);
            
            // Current deck grid
            deckCardsGrid = CreateCardGrid(parent, "Deck Cards Grid", 
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.75f), 3); // 3 columns
        }
        
        private TMP_InputField CreateSearchField(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
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
            placeholderText.text = "Search your cards...";
            placeholderText.fontSize = 16;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.raycastTarget = false;
            
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());
            
            // Input text
            GameObject inputTextObj = new GameObject("Input Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 16;
            inputText.color = Color.black;
            inputText.raycastTarget = false;
            
            SetFullScreen(inputTextObj.GetComponent<RectTransform>());
            
            // Connect input field
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            // Position
            RectTransform searchRect = searchObj.GetComponent<RectTransform>();
            searchRect.anchorMin = anchorMin;
            searchRect.anchorMax = anchorMax;
            searchRect.offsetMin = Vector2.zero;
            searchRect.offsetMax = Vector2.zero;
            
            return inputField;
        }
        
        private TMP_InputField CreateDeckNameField(Transform parent)
        {
            GameObject nameFieldObj = CreatePanel("Deck Name Field", parent);
            nameFieldObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            
            RectTransform nameFieldRect = nameFieldObj.GetComponent<RectTransform>();
            nameFieldRect.anchorMin = new Vector2(0.05f, 0.82f);
            nameFieldRect.anchorMax = new Vector2(0.95f, 0.88f);
            nameFieldRect.offsetMin = Vector2.zero;
            nameFieldRect.offsetMax = Vector2.zero;
            
            TMP_InputField inputField = nameFieldObj.AddComponent<TMP_InputField>();
            inputField.text = "New Deck";
            
            // Text component
            GameObject textObj = new GameObject("Name Text");
            textObj.transform.SetParent(nameFieldObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "New Deck";
            text.fontSize = 18;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
            inputField.textComponent = text;
            
            return inputField;
        }
        
        private Transform CreateCardGrid(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int columns)
        {
            // Scroll view
            GameObject scrollViewObj = new GameObject($"{name} Scroll View");
            scrollViewObj.transform.SetParent(parent, false);
            scrollViewObj.layer = 5;
            
            ScrollRect scrollRectComponent = scrollViewObj.AddComponent<ScrollRect>();
            scrollViewObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform scrollViewRect = scrollViewObj.GetComponent<RectTransform>();
            scrollViewRect.anchorMin = anchorMin;
            scrollViewRect.anchorMax = anchorMax;
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;
            
            // Store reference
            if (name.Contains("Available"))
                availableScrollRect = scrollRectComponent;
            else
                deckScrollRect = scrollRectComponent;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<RectMask2D>();
            
            SetFullScreen(viewport.GetComponent<RectTransform>());
            scrollRectComponent.viewport = viewport.GetComponent<RectTransform>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            scrollRectComponent.content = contentRect;
            
            // Grid layout
            GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(120, 160); // Smaller cards for deck builder
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            return content.transform;
        }
        

        
        private void SetupEventListeners()
        {
            // UI events
            if (backButton) backButton.onClick.AddListener(OnBackPressed);
            if (saveDeckButton) saveDeckButton.onClick.AddListener(OnSaveDeckPressed);
            if (searchField) searchField.onValueChanged.AddListener(OnSearchChanged);
            if (deckNameField) deckNameField.onValueChanged.AddListener(OnDeckNameChanged);
            
            // Data events
            if (RealCardManager.Instance != null)
            {
                RealCardManager.Instance.OnCollectionLoaded += OnCollectionDataLoaded;
            }
            
            // Game flow events
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged += OnGameFlowChanged;
            }
        }
        
        private async void LoadAvailableCards()
        {
            Debug.Log("🔄 Loading available cards for deck building...");
            
            if (RealCardManager.Instance != null)
            {
                await RealCardManager.Instance.LoadUserCollectionFromDatabase();
            }
        }
        
        private void CreateNewDeck()
        {
            currentDeck = new RealSupabaseClient.Deck
            {
                id = System.Guid.NewGuid().ToString(),
                user_id = RealSupabaseClient.Instance?.UserId ?? "",
                name = "New Deck",
                is_active = false,
                cards = new List<RealSupabaseClient.DeckCard>(),
                total_cards = 0,
                created_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                updated_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            
            deckCardCounts.Clear();
            UpdateDeckDisplay();
        }
        
        private void OnCollectionDataLoaded()
        {
            if (RealCardManager.Instance == null) return;
            
            availableCards = RealCardManager.Instance.GetUserCollection();
            DisplayAvailableCards();
            
            Debug.Log($"✅ Loaded {availableCards.Count} cards for deck building");
        }
        
        private void DisplayAvailableCards()
        {
            // Clear existing displays
            foreach (GameObject cardDisplay in availableCardDisplays)
            {
                if (cardDisplay != null) Destroy(cardDisplay);
            }
            availableCardDisplays.Clear();
            
            // Apply search filter
            var filteredCards = availableCards.Where(item => 
            {
                if (string.IsNullOrEmpty(currentSearch)) return true;
                
                return item.card.name.ToLower().Contains(currentSearch.ToLower()) ||
                       item.card.description.ToLower().Contains(currentSearch.ToLower());
            }).ToList();
            
            // Create card displays
            foreach (var collectionItem in filteredCards.Take(40)) // Limit for performance
            {
                if (RealCardManager.Instance != null)
                {
                    GameObject cardObj = RealCardManager.Instance.CreateCardDisplay(collectionItem.card, availableCardsGrid, true);
                    if (cardObj != null)
                    {
                        availableCardDisplays.Add(cardObj);
                        
                        // Add click handler to add card to deck
                        Button cardButton = cardObj.GetComponent<Button>();
                        if (cardButton == null) cardButton = cardObj.AddComponent<Button>();
                        
                        string cardId = collectionItem.card.id;
                        cardButton.onClick.AddListener(() => AddCardToDeck(cardId));
                        
                        // Show quantity owned
                        AddQuantityDisplay(cardObj, collectionItem.quantity);
                    }
                }
            }
        }
        
        private void DisplayDeckCards()
        {
            // Clear existing displays
            foreach (GameObject cardDisplay in deckCardDisplays)
            {
                if (cardDisplay != null) Destroy(cardDisplay);
            }
            deckCardDisplays.Clear();
            
            // Create displays for cards in deck
            foreach (var kvp in deckCardCounts)
            {
                string cardId = kvp.Key;
                int quantity = kvp.Value;
                
                var card = RealCardManager.Instance?.GetCardById(cardId);
                if (card != null && quantity > 0)
                {
                    GameObject cardObj = RealCardManager.Instance.CreateCardDisplay(card, deckCardsGrid, false);
                    if (cardObj != null)
                    {
                        deckCardDisplays.Add(cardObj);
                        
                        // Add click handler to remove card from deck
                        Button cardButton = cardObj.GetComponent<Button>();
                        if (cardButton == null) cardButton = cardObj.AddComponent<Button>();
                        
                        cardButton.onClick.AddListener(() => RemoveCardFromDeck(cardId));
                        
                        // Show quantity in deck
                        AddQuantityDisplay(cardObj, quantity);
                    }
                }
            }
        }
        
        private void AddQuantityDisplay(GameObject cardObj, int quantity)
        {
            GameObject quantityObj = new GameObject("Quantity Display");
            quantityObj.transform.SetParent(cardObj.transform, false);
            quantityObj.layer = 5;
            
            Image quantityBg = quantityObj.AddComponent<Image>();
            quantityBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            TextMeshProUGUI quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
            quantityText.text = quantity.ToString();
            quantityText.fontSize = 16;
            quantityText.color = Color.white;
            quantityText.alignment = TextAlignmentOptions.Center;
            quantityText.fontStyle = FontStyles.Bold;
            
            RectTransform quantityRect = quantityObj.GetComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(0.8f, 0.8f);
            quantityRect.anchorMax = new Vector2(1f, 1f);
            quantityRect.offsetMin = Vector2.zero;
            quantityRect.offsetMax = Vector2.zero;
        }
        
        private void AddCardToDeck(string cardId)
        {
            var card = RealCardManager.Instance?.GetCardById(cardId);
            if (card == null) return;
            
            int currentInDeck = deckCardCounts.ContainsKey(cardId) ? deckCardCounts[cardId] : 0;
            int ownedQuantity = RealCardManager.Instance?.GetCardQuantity(cardId) ?? 0;
            
            // Check if we can add more
            int maxCopies = GetMaxCopiesByRarity(card.rarity);
            
            if (currentInDeck >= maxCopies)
            {
                Debug.Log($"❌ Cannot add more {card.name} - max {maxCopies} copies allowed");
                return;
            }
            
            if (currentInDeck >= ownedQuantity)
            {
                Debug.Log($"❌ Cannot add more {card.name} - only own {ownedQuantity} copies");
                return;
            }
            
            int totalCardsInDeck = deckCardCounts.Values.Sum();
            if (totalCardsInDeck >= 30)
            {
                Debug.Log("❌ Deck is full (30/30 cards)");
                return;
            }
            
            // Add card to deck
            deckCardCounts[cardId] = currentInDeck + 1;
            UpdateDeckDisplay();
            
            Debug.Log($"✅ Added {card.name} to deck ({currentInDeck + 1}/{maxCopies})");
        }
        
        private void RemoveCardFromDeck(string cardId)
        {
            if (!deckCardCounts.ContainsKey(cardId)) return;
            
            var card = RealCardManager.Instance?.GetCardById(cardId);
            if (card == null) return;
            
            int currentInDeck = deckCardCounts[cardId];
            
            if (currentInDeck <= 1)
            {
                deckCardCounts.Remove(cardId);
            }
            else
            {
                deckCardCounts[cardId] = currentInDeck - 1;
            }
            
            UpdateDeckDisplay();
            
            Debug.Log($"✅ Removed {card.name} from deck");
        }
        
        private int GetMaxCopiesByRarity(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => 3,
                "uncommon" => 2,
                "rare" => 2,
                "legendary" => 1,
                "exotic" => 1,
                _ => 2
            };
        }
        
        private void UpdateDeckDisplay()
        {
            DisplayDeckCards();
            
            int totalCards = deckCardCounts.Values.Sum();
            
            // Update deck count
            if (deckCountText)
            {
                deckCountText.text = $"Cards: {totalCards}/30";
                
                if (totalCards == 30)
                    deckCountText.color = new Color(0.2f, 1f, 0.2f, 1f); // Green
                else if (totalCards > 25)
                    deckCountText.color = new Color(1f, 0.8f, 0.2f, 1f); // Yellow
                else
                    deckCountText.color = Color.white;
            }
            
            // Update validation
            bool isValid = ValidateDeck();
            if (validationText)
            {
                if (isValid)
                {
                    validationText.text = "✅ Valid Deck";
                    validationText.color = new Color(0.2f, 1f, 0.2f, 1f);
                }
                else
                {
                    validationText.text = totalCards < 30 ? $"❌ Need {30 - totalCards} more cards" : "❌ Invalid Deck";
                    validationText.color = new Color(1f, 0.3f, 0.3f, 1f);
                }
            }
            
            // Update save button
            if (saveDeckButton)
            {
                saveDeckButton.interactable = isValid;
                Image saveImg = saveDeckButton.GetComponent<Image>();
                if (saveImg)
                {
                    saveImg.color = isValid ? new Color(0.2f, 0.7f, 0.3f, 1f) : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }
        
        private bool ValidateDeck()
        {
            int totalCards = deckCardCounts.Values.Sum();
            
            // Must have exactly 30 cards
            if (totalCards != 30) return false;
            
            // Check copy limits
            foreach (var kvp in deckCardCounts)
            {
                var card = RealCardManager.Instance?.GetCardById(kvp.Key);
                if (card == null) return false;
                
                int maxCopies = GetMaxCopiesByRarity(card.rarity);
                if (kvp.Value > maxCopies) return false;
            }
            
            return true;
        }
        
        private async void OnSaveDeckPressed()
        {
            if (!ValidateDeck())
            {
                Debug.Log("❌ Cannot save invalid deck");
                return;
            }
            
            try
            {
                // Update deck data
                currentDeck.name = deckNameField?.text ?? "New Deck";
                currentDeck.total_cards = deckCardCounts.Values.Sum();
                currentDeck.updated_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                
                // Convert deck cards
                currentDeck.cards = new List<RealSupabaseClient.DeckCard>();
                foreach (var kvp in deckCardCounts)
                {
                    currentDeck.cards.Add(new RealSupabaseClient.DeckCard
                    {
                        deck_id = currentDeck.id,
                        card_id = kvp.Key,
                        quantity = kvp.Value,
                        card = RealCardManager.Instance?.GetCardById(kvp.Key)
                    });
                }
                
                // Save to database
                bool success = await RealSupabaseClient.Instance.SaveDeck(currentDeck);
                
                if (success)
                {
                    Debug.Log($"✅ Deck '{currentDeck.name}' saved successfully!");
                    
                    // Create new deck for next build
                    CreateNewDeck();
                }
                else
                {
                    Debug.LogError("❌ Failed to save deck");
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error saving deck: {e.Message}");
            }
        }
        
        private void OnSearchChanged(string searchTerm)
        {
            currentSearch = searchTerm;
            DisplayAvailableCards();
        }
        
        private void OnDeckNameChanged(string newName)
        {
            if (currentDeck != null)
            {
                currentDeck.name = newName;
            }
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
            bool shouldShow = newFlow == GameFlowManager.GameFlow.DeckBuilder;
            gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                LoadAvailableCards(); // Refresh when shown
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
                RealCardManager.Instance.OnCollectionLoaded -= OnCollectionDataLoaded;
            }
            
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged -= OnGameFlowChanged;
            }
        }
    }
}