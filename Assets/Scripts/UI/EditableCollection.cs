using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FULLY EDITABLE Collection Screen
    /// Customize card collection UI completely in Inspector!
    /// </summary>
    public class EditableCollection : MonoBehaviour
    {
        [Header("📚 COLLECTION SCREEN - EDIT EVERYTHING!")]
        [Space(10)]
        
        [Header("🎨 Background & Layout")]
        [Tooltip("Custom background image")]
        public Sprite collectionBackground;
        
        [Tooltip("Background color tint")]
        public Color backgroundTint = Color.white;
        
        [Space(10)]
        [Header("📱 Layout Areas - Drag to Reposition")]
        [Tooltip("Header area (title, back button)")]
        public RectTransform headerArea;
        
        [Tooltip("Search and filters area")]
        public RectTransform filtersArea;
        
        [Tooltip("Card grid area")]
        public RectTransform cardGridArea;
        
        [Tooltip("Collection stats area")]
        public RectTransform statsArea;
        
        [Space(10)]
        [Header("🎨 Visual Styling")]
        [Tooltip("Screen title")]
        public string screenTitle = "📚 YOUR COLLECTION";
        
        [Range(18, 48)]
        [Tooltip("Title font size")]
        public int titleFontSize = 32;
        
        [Tooltip("Title color")]
        public Color titleColor = new Color(1f, 0.9f, 0.3f, 1f);
        
        [Tooltip("Filter panel color")]
        public Color filterPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        [Tooltip("Card background color")]
        public Color cardBackgroundColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        
        [Space(10)]
        [Header("🃏 Card Display Settings")]
        [Range(80, 200)]
        [Tooltip("Card width")]
        public float cardWidth = 120f;
        
        [Range(100, 250)]
        [Tooltip("Card height")]
        public float cardHeight = 160f;
        
        [Range(2, 6)]
        [Tooltip("Cards per row")]
        public int cardsPerRow = 4;
        
        [Range(5, 20)]
        [Tooltip("Spacing between cards")]
        public float cardSpacing = 8f;
        
        [Range(50, 500)]
        [Tooltip("Maximum cards to display (for performance)")]
        public int maxCardsToShow = 200;
        
        [Space(10)]
        [Header("🔍 Filter Settings")]
        [Tooltip("Show search bar")]
        public bool showSearchBar = true;
        
        [Tooltip("Show element filters")]
        public bool showElementFilters = true;
        
        [Tooltip("Show rarity filters")]
        public bool showRarityFilters = true;
        
        [Tooltip("Show mana cost filter")]
        public bool showManaCostFilter = true;
        
        [Space(10)]
        [Header("📊 Collection Stats")]
        [Tooltip("Show total cards owned")]
        public bool showTotalCards = true;
        
        [Tooltip("Show collection completion percentage")]
        public bool showCompletionPercentage = true;
        
        [Tooltip("Show rarity breakdown")]
        public bool showRarityBreakdown = true;
        
        [Space(10)]
        [Header("🔄 Quick Actions")]
        [Tooltip("Refresh collection display")]
        public bool refreshScreen = false;
        
        [Tooltip("Reset to defaults")]
        public bool resetToDefaults = false;
        
        [Tooltip("Reload card data from database")]
        public bool reloadCardData = false;
        
        // Runtime data
        private List<RealSupabaseClient.CollectionItem> userCollection;
        private List<RealSupabaseClient.CollectionItem> filteredCollection;
        private bool isInitialized = false;
        
        void Start()
        {
            InitializeCollection();
        }
        
        void OnValidate()
        {
            if (Application.isPlaying && isInitialized)
            {
                if (refreshScreen)
                {
                    refreshScreen = false;
                    RefreshScreen();
                }
                
                if (resetToDefaults)
                {
                    resetToDefaults = false;
                    ResetToDefaults();
                }
                
                if (reloadCardData)
                {
                    reloadCardData = false;
                    _ = LoadCollectionData();
                }
                
                ApplyVisualSettings();
            }
        }
        
        private async void InitializeCollection()
        {
            Debug.Log("📚 Initializing EDITABLE Collection...");
            
            // Create layout
            CreateEditableLayout();
            
            // Load collection data
            await LoadCollectionData();
            
            // Create content
            CreateCollectionContent();
            
            // Apply settings
            ApplyVisualSettings();
            
            isInitialized = true;
            Debug.Log("✅ Editable Collection ready!");
        }
        
        private void CreateEditableLayout()
        {
            // Background
            CreateEditableBackground();
            
            // Create areas
            if (headerArea == null) headerArea = CreateEditableArea("Header Area", new Vector2(0f, 0.9f), new Vector2(1f, 1f));
            if (filtersArea == null) filtersArea = CreateEditableArea("Filters Area", new Vector2(0f, 0.8f), new Vector2(1f, 0.88f));
            if (statsArea == null) statsArea = CreateEditableArea("Stats Area", new Vector2(0f, 0.72f), new Vector2(1f, 0.78f));
            if (cardGridArea == null) cardGridArea = CreateEditableArea("Card Grid Area", new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.7f));
        }
        
        private void CreateEditableBackground()
        {
            GameObject bgObj = new GameObject("🎨 Collection Background");
            bgObj.transform.SetParent(transform, false);
            bgObj.layer = 5;
            
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImg = bgObj.AddComponent<Image>();
            if (collectionBackground != null)
            {
                bgImg.sprite = collectionBackground;
                bgImg.color = backgroundTint;
            }
            else
            {
                bgImg.color = new Color(0.05f, 0.1f, 0.15f, 1f);
            }
        }
        
        private RectTransform CreateEditableArea(string areaName, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject areaObj = new GameObject(areaName);
            areaObj.transform.SetParent(transform, false);
            areaObj.layer = 5;
            
            RectTransform areaRect = areaObj.AddComponent<RectTransform>();
            areaRect.anchorMin = anchorMin;
            areaRect.anchorMax = anchorMax;
            areaRect.offsetMin = Vector2.zero;
            areaRect.offsetMax = Vector2.zero;
            
            Image areaImg = areaObj.AddComponent<Image>();
            areaImg.color = new Color(0f, 0f, 1f, 0.05f); // Very transparent blue
            
            return areaRect;
        }
        
        private async System.Threading.Tasks.Task LoadCollectionData()
        {
            // Wait for RealSupabaseClient
            int attempts = 0;
            while (RealSupabaseClient.Instance == null && attempts < 50)
            {
                await System.Threading.Tasks.Task.Delay(100);
                attempts++;
            }
            
            if (RealSupabaseClient.Instance == null)
            {
                Debug.LogError("❌ Could not load collection data - Supabase not ready");
                return;
            }
            
            userCollection = await RealSupabaseClient.Instance.LoadUserCollection();
            filteredCollection = userCollection?.Where(item => item.quantity > 0).ToList() ?? new List<RealSupabaseClient.CollectionItem>();
            
            Debug.Log($"📚 Loaded {filteredCollection?.Count ?? 0} cards in collection");
        }
        
        private void CreateCollectionContent()
        {
            // Header
            if (headerArea != null)
            {
                CreateCollectionHeader();
            }
            
            // Filters
            if (filtersArea != null)
            {
                CreateFilters();
            }
            
            // Stats
            if (statsArea != null)
            {
                CreateCollectionStats();
            }
            
            // Card grid
            if (cardGridArea != null)
            {
                CreateCardGrid();
            }
        }
        
        private void CreateCollectionHeader()
        {
            // Back button
            CreateButton("← BACK", headerArea, new Vector2(0.02f, 0.1f), new Vector2(0.15f, 0.9f), new Color(0.8f, 0.2f, 0.2f, 1f), () => {
                EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                uiManager?.GoToMainMenu();
            });
            
            // Title
            CreateText("Collection Title", screenTitle, headerArea, new Vector2(0.2f, 0f), new Vector2(0.8f, 1f), titleFontSize, titleColor);
        }
        
        private void CreateFilters()
        {
            // Search bar
            if (showSearchBar)
            {
                CreateInputField("Search Cards", "Search your collection...", filtersArea, new Vector2(0.05f, 0.2f), new Vector2(0.4f, 0.8f));
            }
            
            // Element filter buttons
            if (showElementFilters)
            {
                CreateFilterButton("🔥", new Vector2(0.45f, 0.2f), new Vector2(0.52f, 0.8f), new Color(1f, 0.3f, 0.1f, 1f));
                CreateFilterButton("🧊", new Vector2(0.54f, 0.2f), new Vector2(0.61f, 0.8f), new Color(0.1f, 0.6f, 1f, 1f));
                CreateFilterButton("⚡", new Vector2(0.63f, 0.2f), new Vector2(0.7f, 0.8f), new Color(1f, 1f, 0.2f, 1f));
                CreateFilterButton("☀️", new Vector2(0.72f, 0.2f), new Vector2(0.79f, 0.8f), new Color(1f, 1f, 0.8f, 1f));
                CreateFilterButton("🌑", new Vector2(0.81f, 0.2f), new Vector2(0.88f, 0.8f), new Color(0.4f, 0.1f, 0.6f, 1f));
                CreateFilterButton("✨", new Vector2(0.9f, 0.2f), new Vector2(0.97f, 0.8f), new Color(1f, 0.5f, 1f, 1f));
            }
        }
        
        private void CreateCollectionStats()
        {
            if (showTotalCards)
            {
                int totalOwned = filteredCollection?.Count ?? 0;
                CreateText("Total Cards", $"Cards: {totalOwned}/236", statsArea, new Vector2(0.05f, 0f), new Vector2(0.3f, 1f), 14, Color.white);
            }
            
            if (showCompletionPercentage)
            {
                float completion = filteredCollection != null ? (filteredCollection.Count / 236f) * 100f : 0f;
                CreateText("Completion", $"Collection: {completion:F1}%", statsArea, new Vector2(0.35f, 0f), new Vector2(0.65f, 1f), 14, Color.green);
            }
        }
        
        private void CreateCardGrid()
        {
            if (filteredCollection == null || filteredCollection.Count == 0)
            {
                CreateText("No Cards", "No cards in collection", cardGridArea, Vector2.zero, Vector2.one, 20, Color.gray);
                return;
            }
            
            // Create scroll view
            GameObject scrollView = new GameObject("Collection Scroll View");
            scrollView.transform.SetParent(cardGridArea, false);
            scrollView.layer = 5;
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = Color.clear;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            viewport.layer = 5;
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            // Grid layout
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cardWidth, cardHeight);
            grid.spacing = new Vector2(cardSpacing, cardSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cardsPerRow;
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            // Create cards
            int cardsCreated = 0;
            foreach (var collectionItem in filteredCollection.Take(maxCardsToShow))
            {
                CreateCollectionCard(collectionItem, content.transform);
                cardsCreated++;
            }
            
            Debug.Log($"✅ Created {cardsCreated} collection cards");
        }
        
        private void CreateCollectionCard(RealSupabaseClient.CollectionItem collectionItem, Transform parent)
        {
            GameObject cardObj = new GameObject($"Card: {collectionItem.card.name}");
            cardObj.transform.SetParent(parent, false);
            cardObj.layer = 5;
            
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = GetElementalColor(collectionItem.card.potato_type);
            
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                Debug.Log($"🃏 Clicked card: {collectionItem.card.name}");
                // TODO: Show card details
            });
            
            // Card frame
            if (cardBackgroundColor.a > 0)
            {
                cardImg.color = cardBackgroundColor;
            }
            
            // Card name
            CreateText("Name", collectionItem.card.name, cardObj.transform, new Vector2(0.05f, 0f), new Vector2(0.95f, 0.2f), 8, Color.white);
            
            // Card stats
            CreateText("Mana", $"⚡{collectionItem.card.mana_cost}", cardObj.transform, new Vector2(0.05f, 0.75f), new Vector2(0.35f, 0.95f), 8, Color.blue);
            CreateText("Attack", $"⚔️{collectionItem.card.attack}", cardObj.transform, new Vector2(0.35f, 0.75f), new Vector2(0.65f, 0.95f), 8, Color.red);
            CreateText("Health", $"❤️{collectionItem.card.hp}", cardObj.transform, new Vector2(0.65f, 0.75f), new Vector2(0.95f, 0.95f), 8, Color.green);
            
            // Quantity badge
            if (collectionItem.quantity > 1)
            {
                CreateQuantityBadge(collectionItem.quantity, cardObj.transform);
            }
            
            // Rarity indicator
            CreateRarityIndicator(collectionItem.card.rarity, cardObj.transform);
        }
        
        private void CreateQuantityBadge(int quantity, Transform parent)
        {
            GameObject badgeObj = new GameObject($"x{quantity}");
            badgeObj.transform.SetParent(parent, false);
            badgeObj.layer = 5;
            
            Image badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.color = new Color(1f, 0.8f, 0f, 0.9f);
            
            RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.7f, 0.8f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;
            
            CreateText("Badge Text", $"x{quantity}", badgeObj.transform, Vector2.zero, Vector2.one, 8, Color.black);
        }
        
        private void CreateRarityIndicator(string rarity, Transform parent)
        {
            Color rarityColor = rarity?.ToLower() switch
            {
                "common" => Color.gray,
                "uncommon" => Color.green,
                "rare" => Color.blue,
                "legendary" => new Color(0.6f, 0.3f, 1f, 1f),
                "exotic" => new Color(1f, 0.8f, 0f, 1f),
                _ => Color.white
            };
            
            GameObject rarityObj = new GameObject($"Rarity: {rarity}");
            rarityObj.transform.SetParent(parent, false);
            rarityObj.layer = 5;
            
            Image rarityImg = rarityObj.AddComponent<Image>();
            rarityImg.color = rarityColor;
            
            RectTransform rarityRect = rarityObj.GetComponent<RectTransform>();
            rarityRect.anchorMin = new Vector2(0f, 0.9f);
            rarityRect.anchorMax = new Vector2(1f, 1f);
            rarityRect.offsetMin = Vector2.zero;
            rarityRect.offsetMax = Vector2.zero;
        }
        
        private void CreateInputField(string name, string placeholder, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);
            inputObj.layer = 5;
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.onValueChanged.AddListener((value) => FilterCards(value));
            
            // Placeholder and text setup
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            placeholderObj.layer = 5;
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
            
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0.05f, 0f);
            placeholderRect.anchorMax = new Vector2(0.95f, 1f);
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            inputField.placeholder = placeholderText;
        }
        
        private void CreateFilterButton(string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject filterBtn = new GameObject($"Filter: {text}");
            filterBtn.transform.SetParent(filtersArea, false);
            filterBtn.layer = 5;
            
            Image filterImg = filterBtn.AddComponent<Image>();
            filterImg.color = color;
            
            Button button = filterBtn.AddComponent<Button>();
            button.onClick.AddListener(() => FilterByElement(text));
            
            RectTransform filterRect = filterBtn.GetComponent<RectTransform>();
            filterRect.anchorMin = anchorMin;
            filterRect.anchorMax = anchorMax;
            filterRect.offsetMin = Vector2.zero;
            filterRect.offsetMax = Vector2.zero;
            
            CreateText("Filter Text", text, filterBtn.transform, Vector2.zero, Vector2.one, 16, Color.white);
        }
        
        private void CreateButton(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, System.Action onClick)
        {
            GameObject btnObj = new GameObject($"Button: {text}");
            btnObj.transform.SetParent(parent, false);
            btnObj.layer = 5;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;
            
            Button button = btnObj.AddComponent<Button>();
            if (onClick != null) button.onClick.AddListener(() => onClick());
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            CreateText("Button Text", text, btnObj.transform, Vector2.zero, Vector2.one, 14, Color.white);
        }
        
        private void CreateText(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            textObj.layer = 5;
            
            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        private Color GetElementalColor(string element)
        {
            return element?.ToLower() switch
            {
                "fire" => new Color(1f, 0.3f, 0.1f, 1f),
                "ice" => new Color(0.1f, 0.6f, 1f, 1f),
                "lightning" => new Color(1f, 1f, 0.2f, 1f),
                "light" => new Color(1f, 1f, 0.8f, 1f),
                "void" => new Color(0.4f, 0.1f, 0.6f, 1f),
                "exotic" => new Color(1f, 0.5f, 1f, 1f),
                _ => new Color(0.5f, 0.5f, 0.5f, 1f)
            };
        }
        
        private void FilterCards(string searchTerm)
        {
            if (userCollection == null) return;
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                filteredCollection = userCollection.Where(item => item.quantity > 0).ToList();
            }
            else
            {
                filteredCollection = userCollection.Where(item => 
                    item.quantity > 0 && 
                    item.card.name.ToLower().Contains(searchTerm.ToLower())
                ).ToList();
            }
            
            RefreshCardGrid();
        }
        
        private void FilterByElement(string element)
        {
            if (userCollection == null) return;
            
            string elementName = element switch
            {
                "🔥" => "fire",
                "🧊" => "ice", 
                "⚡" => "lightning",
                "☀️" => "light",
                "🌑" => "void",
                "✨" => "exotic",
                _ => ""
            };
            
            if (string.IsNullOrEmpty(elementName))
            {
                filteredCollection = userCollection.Where(item => item.quantity > 0).ToList();
            }
            else
            {
                filteredCollection = userCollection.Where(item => 
                    item.quantity > 0 && 
                    item.card.potato_type?.ToLower() == elementName
                ).ToList();
            }
            
            RefreshCardGrid();
        }
        
        private void RefreshCardGrid()
        {
            // Remove existing card grid
            Transform existingGrid = cardGridArea.Find("Collection Scroll View");
            if (existingGrid != null)
            {
                DestroyImmediate(existingGrid.gameObject);
            }
            
            // Recreate grid
            CreateCardGrid();
        }
        
        public void RefreshScreen()
        {
            Debug.Log("🔄 Refreshing collection...");
            ApplyVisualSettings();
            _ = LoadCollectionData();
        }
        
        public void ResetToDefaults()
        {
            Debug.Log("🔄 Resetting collection to defaults...");
            
            cardWidth = 120f;
            cardHeight = 160f;
            cardsPerRow = 4;
            cardSpacing = 8f;
            titleFontSize = 32;
            titleColor = new Color(1f, 0.9f, 0.3f, 1f);
            
            ApplyVisualSettings();
        }
        
        private void ApplyVisualSettings()
        {
            // Update grid layout if it exists
            GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>();
            if (grid != null)
            {
                grid.cellSize = new Vector2(cardWidth, cardHeight);
                grid.spacing = new Vector2(cardSpacing, cardSpacing);
                grid.constraintCount = cardsPerRow;
            }
            
            // Update title
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.name == "Collection Title")
                {
                    text.text = screenTitle;
                    text.fontSize = titleFontSize;
                    text.color = titleColor;
                }
            }
        }
    }
}