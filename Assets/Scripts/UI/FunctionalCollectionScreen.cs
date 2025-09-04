using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FUNCTIONAL Collection Screen that actually loads and displays your real cards
    /// Recreates the FantasyCollection.tsx functionality from your web version
    /// </summary>
    public class FunctionalCollectionScreen : MonoBehaviour
    {
        [Header("🃏 Functional Collection")]
        [SerializeField] private ScrollRect cardScrollRect;
        [SerializeField] private GridLayoutGroup cardGrid;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI collectionCountText;
        
        // Real data from your database
        private List<RealSupabaseClient.CollectionItem> userCollection = new List<RealSupabaseClient.CollectionItem>();
        private List<RealSupabaseClient.EnhancedCard> allCards = new List<RealSupabaseClient.EnhancedCard>();
        private List<RealSupabaseClient.EnhancedCard> filteredCards = new List<RealSupabaseClient.EnhancedCard>();
        private List<GameObject> cardDisplays = new List<GameObject>();
        
        // Filter state
        private string currentSearchQuery = "";
        private string currentRarityFilter = "all";
        private string currentElementFilter = "all";
        
        // UI References
        private Canvas collectionCanvas;
        private GameObject collectionUI;
        
        private void Start()
        {
            CreateFunctionalCollectionUI();
        }
        
        private void CreateFunctionalCollectionUI()
        {
            Debug.Log("📚 Creating FUNCTIONAL collection screen...");
            
            // Create collection canvas
            GameObject canvasObj = new GameObject("Collection Canvas");
            collectionCanvas = canvasObj.AddComponent<Canvas>();
            collectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            collectionCanvas.sortingOrder = 200;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create collection interface
            CreateCollectionInterface();
            
            // Start hidden
            collectionCanvas.gameObject.SetActive(false);
            
            Debug.Log("✅ FUNCTIONAL collection UI created");
        }
        
        private void CreateCollectionInterface()
        {
            // Main collection background
            collectionUI = CreatePanel("Collection Screen", collectionCanvas.transform);
            Image bgImage = collectionUI.GetComponent<Image>();
            bgImage.color = new Color(0.05f, 0.15f, 0.08f, 1f); // Dark green
            SetFullScreen(collectionUI.GetComponent<RectTransform>());
            
            // Header with title and stats
            CreateCollectionHeader();
            
            // Search and filters
            CreateSearchAndFilters();
            
            // Card grid (REAL CARDS)
            CreateRealCardGrid();
            
            // Back button
            CreateBackButton();
        }
        
        private void CreateCollectionHeader()
        {
            GameObject headerPanel = CreatePanel("Collection Header", collectionUI.transform);
            Image headerBg = headerPanel.GetComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.7f);
            
            RectTransform headerRect = headerPanel.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.88f);
            headerRect.anchorMax = new Vector2(0.95f, 0.98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Collection Title");
            titleObj.transform.SetParent(headerPanel.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "CARD COLLECTION";
            titleText.fontSize = 28;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.5f);
            titleRect.anchorMax = new Vector2(0.9f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Collection stats
            GameObject statsObj = new GameObject("Collection Stats");
            statsObj.transform.SetParent(headerPanel.transform, false);
            statsObj.layer = 5;
            
            collectionCountText = statsObj.AddComponent<TextMeshProUGUI>();
            collectionCountText.text = "Loading collection...";
            collectionCountText.fontSize = 16;
            collectionCountText.color = Color.white;
            collectionCountText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.1f, 0f);
            statsRect.anchorMax = new Vector2(0.9f, 0.5f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
        }
        
        private void CreateSearchAndFilters()
        {
            GameObject filterPanel = CreatePanel("Filter Panel", collectionUI.transform);
            Image filterBg = filterPanel.GetComponent<Image>();
            filterBg.color = new Color(0f, 0f, 0f, 0.5f);
            
            RectTransform filterRect = filterPanel.GetComponent<RectTransform>();
            filterRect.anchorMin = new Vector2(0.05f, 0.8f);
            filterRect.anchorMax = new Vector2(0.95f, 0.86f);
            filterRect.offsetMin = Vector2.zero;
            filterRect.offsetMax = Vector2.zero;
            
            // Search field
            searchField = CreateSearchField(filterPanel.transform);
            searchField.onValueChanged.AddListener(OnSearchChanged);
            
            // Filter buttons
            CreateFilterButtons(filterPanel.transform);
        }
        
        private TMP_InputField CreateSearchField(Transform parent)
        {
            GameObject searchObj = CreatePanel("Search Field", parent);
            searchObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            
            RectTransform searchRect = searchObj.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0.05f, 0.1f);
            searchRect.anchorMax = new Vector2(0.5f, 0.9f);
            searchRect.offsetMin = Vector2.zero;
            searchRect.offsetMax = Vector2.zero;
            
            TMP_InputField inputField = searchObj.AddComponent<TMP_InputField>();
            
            // Input text
            GameObject inputTextObj = new GameObject("Search Text");
            inputTextObj.transform.SetParent(searchObj.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 16;
            inputText.color = Color.black;
            inputText.alignment = TextAlignmentOptions.Left;
            
            RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
            inputTextRect.anchorMin = new Vector2(0.05f, 0f);
            inputTextRect.anchorMax = new Vector2(0.95f, 1f);
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;
            
            // Placeholder
            GameObject placeholderObj = new GameObject("Search Placeholder");
            placeholderObj.transform.SetParent(searchObj.transform, false);
            placeholderObj.layer = 5;
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Search cards...";
            placeholderText.fontSize = 16;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.alignment = TextAlignmentOptions.Left;
            placeholderText.raycastTarget = false;
            
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());
            
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            return inputField;
        }
        
        private void CreateFilterButtons(Transform parent)
        {
            // Rarity filters
            CreateFilterButton(parent, "ALL", new Vector2(0.55f, 0.1f), new Vector2(0.68f, 0.9f), () => SetRarityFilter("all"));
            CreateFilterButton(parent, "RARE", new Vector2(0.7f, 0.1f), new Vector2(0.82f, 0.9f), () => SetRarityFilter("rare"));
            CreateFilterButton(parent, "LEGEND", new Vector2(0.84f, 0.1f), new Vector2(0.95f, 0.9f), () => SetRarityFilter("legendary"));
        }
        
        private void CreateFilterButton(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
        {
            GameObject btnObj = CreatePanel($"Filter {text}", parent);
            Button button = btnObj.AddComponent<Button>();
            Image btnImage = btnObj.GetComponent<Image>();
            btnImage.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Button text
            GameObject textObj = new GameObject("Filter Text");
            textObj.transform.SetParent(btnObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 12;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.raycastTarget = false;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
            
            button.onClick.AddListener(() => onClick?.Invoke());
        }
        
        private void CreateRealCardGrid()
        {
            // Scroll view for real cards
            GameObject scrollViewObj = new GameObject("Real Card Scroll View");
            scrollViewObj.transform.SetParent(collectionUI.transform, false);
            scrollViewObj.layer = 5;
            
            cardScrollRect = scrollViewObj.AddComponent<ScrollRect>();
            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.78f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<RectMask2D>();
            
            SetFullScreen(viewport.GetComponent<RectTransform>());
            cardScrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            // Content with grid
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            cardScrollRect.content = contentRect;
            
            // Grid layout for real cards
            cardGrid = content.AddComponent<GridLayoutGroup>();
            cardGrid.cellSize = new Vector2(150, 200); // Professional card size
            cardGrid.spacing = new Vector2(10, 10);
            cardGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            cardGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            cardGrid.childAlignment = TextAnchor.UpperCenter;
            cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cardGrid.constraintCount = 6; // 6 cards per row for mobile
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        private void CreateBackButton()
        {
            GameObject backBtnObj = CreatePanel("Back Button", collectionUI.transform);
            Button backButton = backBtnObj.AddComponent<Button>();
            Image backBtnImg = backBtnObj.GetComponent<Image>();
            backBtnImg.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            RectTransform backRect = backBtnObj.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.05f, 0.05f);
            backRect.anchorMax = new Vector2(0.25f, 0.12f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            
            // Back text
            GameObject backTextObj = new GameObject("Back Text");
            backTextObj.transform.SetParent(backBtnObj.transform, false);
            backTextObj.layer = 5;
            
            TextMeshProUGUI backText = backTextObj.AddComponent<TextMeshProUGUI>();
            backText.text = "← BACK";
            backText.fontSize = 18;
            backText.color = Color.white;
            backText.alignment = TextAlignmentOptions.Center;
            backText.fontStyle = FontStyles.Bold;
            backText.raycastTarget = false;
            
            SetFullScreen(backTextObj.GetComponent<RectTransform>());
            
            backButton.onClick.AddListener(() => {
                HideCollection();
            });
        }
        
        public async void ShowCollection()
        {
            Debug.Log("📚 Showing functional collection...");
            
            collectionCanvas.gameObject.SetActive(true);
            
            // Load real collection data
            await LoadRealCollectionData();
        }
        
        public void HideCollection()
        {
            Debug.Log("📚 Hiding collection...");
            collectionCanvas.gameObject.SetActive(false);
            
            // Return to main menu
            var productionUI = FindFirstObjectByType<ProductionUIManager>();
            if (productionUI != null)
            {
                productionUI.ShowScreen(ProductionUIManager.GameScreen.MainMenu);
            }
        }
        
        private async Task LoadRealCollectionData()
        {
            try
            {
                Debug.Log("🔄 Loading REAL collection data from Supabase...");
                
                if (RealSupabaseClient.Instance == null)
                {
                    Debug.LogError("❌ RealSupabaseClient not found!");
                    return;
                }
                
                // Load all cards from database (like web version)
                allCards = await RealSupabaseClient.Instance.LoadAllCards();
                Debug.Log($"✅ Loaded {allCards.Count} cards from database");
                
                // Load user's collection
                userCollection = await RealSupabaseClient.Instance.LoadUserCollection();
                Debug.Log($"✅ Loaded user collection: {userCollection.Count} unique cards");
                
                // Update collection stats
                UpdateCollectionStats();
                
                // Display cards
                DisplayRealCards();
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading collection: {e.Message}");
                
                if (collectionCountText)
                {
                    collectionCountText.text = "Error loading collection";
                    collectionCountText.color = Color.red;
                }
            }
        }
        
        private void UpdateCollectionStats()
        {
            int totalCards = userCollection.Sum(item => item.quantity);
            int uniqueCards = userCollection.Count(item => item.quantity > 0);
            float completionPercentage = (float)uniqueCards / allCards.Count * 100f;
            
            if (collectionCountText)
            {
                collectionCountText.text = $"Total: {totalCards} cards • Unique: {uniqueCards} • Completion: {completionPercentage:F1}%";
            }
            
            Debug.Log($"📊 Collection stats: {totalCards} total, {uniqueCards} unique, {completionPercentage:F1}% complete");
        }
        
        private void DisplayRealCards()
        {
            Debug.Log("🎴 Displaying real cards...");
            
            // Clear existing card displays
            ClearCardDisplays();
            
            // Apply filters
            ApplyFilters();
            
            // Create real card displays (limit for performance)
            int cardsToShow = Mathf.Min(filteredCards.Count, 50);
            
            for (int i = 0; i < cardsToShow; i++)
            {
                var card = filteredCards[i];
                var collectionItem = userCollection.FirstOrDefault(item => item.card.id == card.id);
                int quantity = collectionItem?.quantity ?? 0;
                
                if (quantity > 0) // Only show owned cards
                {
                    GameObject cardObj = CreateRealCardDisplay(card, quantity);
                    if (cardObj != null)
                    {
                        cardDisplays.Add(cardObj);
                    }
                }
            }
            
            Debug.Log($"✅ Displayed {cardDisplays.Count} real cards from collection");
        }
        
        private GameObject CreateRealCardDisplay(RealSupabaseClient.EnhancedCard card, int quantity)
        {
            GameObject cardObj = new GameObject($"Card_{card.name}");
            cardObj.transform.SetParent(cardGrid.transform, false);
            cardObj.layer = 5;
            
            // Card background with REAL elemental background
            Image cardImage = cardObj.AddComponent<Image>();
            Sprite elementalBg = GetElementalBackground(card.potato_type, card.exotic || card.is_exotic);
            
            if (elementalBg != null)
            {
                cardImage.sprite = elementalBg;
                cardImage.type = Image.Type.Simple;
            }
            else
            {
                cardImage.color = GetElementalColor(card.potato_type);
            }
            
            // Card name
            GameObject nameObj = new GameObject("Card Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = card.name;
            nameText.fontSize = 11;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            nameText.outlineColor = Color.black;
            nameText.outlineWidth = 0.2f;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.95f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Mana cost
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            
            // Attack (if unit)
            if (card.attack.HasValue)
            {
                CreateCardStat(cardObj.transform, card.attack.Value.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
            }
            
            // Health (if unit)
            if (card.hp.HasValue)
            {
                CreateCardStat(cardObj.transform, card.hp.Value.ToString(), new Color(0.3f, 1f, 0.3f, 1f), new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
            }
            
            // Quantity badge
            if (quantity > 1)
            {
                CreateQuantityBadge(cardObj.transform, quantity);
            }
            
            // Rarity indicator
            CreateRarityIndicator(cardObj.transform, card.rarity);
            
            // Click handler for card details
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                ShowCardDetails(card);
            });
            
            // Professional button colors
            ColorBlock colors = cardButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            cardButton.colors = colors;
            
            return cardObj;
        }
        
        private void CreateCardStat(Transform parent, string value, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject statObj = CreatePanel("Card Stat", parent);
            Image statBg = statObj.GetComponent<Image>();
            statBg.color = color;
            
            RectTransform statRect = statObj.GetComponent<RectTransform>();
            statRect.anchorMin = anchorMin;
            statRect.anchorMax = anchorMax;
            statRect.offsetMin = Vector2.zero;
            statRect.offsetMax = Vector2.zero;
            
            // Stat text
            GameObject textObj = new GameObject("Stat Text");
            textObj.transform.SetParent(statObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI statText = textObj.AddComponent<TextMeshProUGUI>();
            statText.text = value;
            statText.fontSize = 12;
            statText.color = Color.white;
            statText.alignment = TextAlignmentOptions.Center;
            statText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
        }
        
        private void CreateQuantityBadge(Transform parent, int quantity)
        {
            GameObject quantityObj = CreatePanel("Quantity Badge", parent);
            Image quantityBg = quantityObj.GetComponent<Image>();
            quantityBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform quantityRect = quantityObj.GetComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(0.75f, 0.75f);
            quantityRect.anchorMax = new Vector2(0.95f, 0.9f);
            quantityRect.offsetMin = Vector2.zero;
            quantityRect.offsetMax = Vector2.zero;
            
            // Quantity text
            GameObject textObj = new GameObject("Quantity Text");
            textObj.transform.SetParent(quantityObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI quantityText = textObj.AddComponent<TextMeshProUGUI>();
            quantityText.text = quantity.ToString();
            quantityText.fontSize = 10;
            quantityText.color = Color.white;
            quantityText.alignment = TextAlignmentOptions.Center;
            quantityText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
        }
        
        private void CreateRarityIndicator(Transform parent, string rarity)
        {
            GameObject rarityObj = CreatePanel("Rarity Indicator", parent);
            Image rarityBg = rarityObj.GetComponent<Image>();
            rarityBg.color = GetRarityColor(rarity);
            
            RectTransform rarityRect = rarityObj.GetComponent<RectTransform>();
            rarityRect.anchorMin = new Vector2(0.75f, 0.8f);
            rarityRect.anchorMax = new Vector2(0.95f, 0.95f);
            rarityRect.offsetMin = Vector2.zero;
            rarityRect.offsetMax = Vector2.zero;
        }
        
        private void ApplyFilters()
        {
            // Start with owned cards only
            var ownedCards = userCollection.Where(item => item.quantity > 0).Select(item => item.card);
            
            filteredCards = ownedCards.Where(card =>
            {
                // Search filter
                if (!string.IsNullOrEmpty(currentSearchQuery))
                {
                    bool matchesSearch = card.name.ToLower().Contains(currentSearchQuery.ToLower()) ||
                                       (card.description?.ToLower().Contains(currentSearchQuery.ToLower()) ?? false);
                    if (!matchesSearch) return false;
                }
                
                // Rarity filter
                if (currentRarityFilter != "all" && card.rarity != currentRarityFilter)
                    return false;
                
                // Element filter
                if (currentElementFilter != "all" && card.potato_type != currentElementFilter)
                    return false;
                
                return true;
                
            }).ToList();
            
            Debug.Log($"🔍 Filtered to {filteredCards.Count} cards");
        }
        
        private void OnSearchChanged(string searchText)
        {
            currentSearchQuery = searchText;
            DisplayRealCards();
        }
        
        private void SetRarityFilter(string rarity)
        {
            currentRarityFilter = rarity;
            DisplayRealCards();
            Debug.Log($"🔍 Set rarity filter: {rarity}");
        }
        
        private void ShowCardDetails(RealSupabaseClient.EnhancedCard card)
        {
            Debug.Log($"🃏 Showing details for: {card.name}");
            // TODO: Create card detail popup
        }
        
        private void ClearCardDisplays()
        {
            foreach (GameObject cardDisplay in cardDisplays)
            {
                if (cardDisplay != null) Destroy(cardDisplay);
            }
            cardDisplays.Clear();
        }
        
        #region Helper Methods
        
        private Sprite GetElementalBackground(string potatoType, bool isExotic)
        {
            if (isExotic)
            {
                return Resources.Load<Sprite>("ElementalBackgrounds/exotic-class-card");
            }
            
            string backgroundName = potatoType?.ToLower() switch
            {
                "fire" => "fire-card",
                "ice" => "ice-card",
                "light" => "light-card",
                "lightning" => "lightning-card",
                "void" => "void-card",
                _ => "void-card"
            };
            
            return Resources.Load<Sprite>($"ElementalBackgrounds/{backgroundName}");
        }
        
        private Color GetElementalColor(string potatoType)
        {
            return potatoType?.ToLower() switch
            {
                "fire" => new Color(0.9f, 0.3f, 0.2f, 1f),
                "ice" => new Color(0.2f, 0.6f, 0.9f, 1f),
                "lightning" => new Color(1f, 0.9f, 0.2f, 1f),
                "light" => new Color(1f, 0.95f, 0.7f, 1f),
                "void" => new Color(0.5f, 0.2f, 0.7f, 1f),
                _ => new Color(0.5f, 0.5f, 0.5f, 1f)
            };
        }
        
        private Color GetRarityColor(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => Color.white,
                "uncommon" => Color.green,
                "rare" => Color.blue,
                "legendary" => Color.yellow,
                "exotic" => new Color(1f, 0.2f, 1f, 1f),
                _ => Color.white
            };
        }
        
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
    }
}