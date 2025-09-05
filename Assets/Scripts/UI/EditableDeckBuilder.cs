using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// EDITABLE Deck Builder - Fully customizable in Unity Inspector
    /// Drag, resize, and style everything visually without code changes!
    /// </summary>
    public class EditableDeckBuilder : MonoBehaviour
    {
        [Header("🎨 DECK BUILDER LAYOUT - EDIT IN INSPECTOR!")]
        
        [Header("📱 Main Layout Settings")]
        [Tooltip("Top management bar area")]
        public RectTransform managementBarArea;
        
        [Tooltip("Left collection panel area")]
        public RectTransform collectionPanelArea;
        
        [Tooltip("Right deck panel area")]
        public RectTransform deckPanelArea;
        
        [Header("🎨 Visual Styling")]
        [Tooltip("Collection panel background image")]
        public Sprite collectionPanelBackground;
        
        [Tooltip("Deck panel background image")]
        public Sprite deckPanelBackground;
        
        [Tooltip("Management bar background image")]
        public Sprite managementBarBackground;
        
        [Header("🃏 Card Display Settings")]
        [Tooltip("Card size in collection")]
        public Vector2 collectionCardSize = new Vector2(120, 160);
        
        [Tooltip("Card size in deck")]
        public Vector2 deckCardSize = new Vector2(100, 140);
        
        [Tooltip("Spacing between cards")]
        public Vector2 cardSpacing = new Vector2(5, 5);
        
        [Tooltip("Cards per row in collection")]
        [Range(2, 6)]
        public int collectionColumnsPerRow = 3;
        
        [Tooltip("Cards per row in deck")]
        [Range(3, 8)]
        public int deckColumnsPerRow = 4;
        
        [Header("📝 Text Styling")]
        [Tooltip("Title text component")]
        public TextMeshProUGUI titleText;
        
        [Tooltip("Active deck indicator text")]
        public TextMeshProUGUI activeDeckText;
        
        [Tooltip("Collection title text")]
        public TextMeshProUGUI collectionTitleText;
        
        [Tooltip("Deck title text")]
        public TextMeshProUGUI deckTitleText;
        
        [Header("🔘 Button Settings")]
        [Tooltip("Create new deck button")]
        public Button createDeckButton;
        
        [Tooltip("Deck selector button")]
        public Button deckSelectorButton;
        
        [Tooltip("Save deck button")]
        public Button saveDeckButton;
        
        [Tooltip("Back button")]
        public Button backButton;
        
        [Header("🎮 Card Container Settings")]
        [Tooltip("Collection cards scroll view")]
        public ScrollRect collectionScrollView;
        
        [Tooltip("Collection cards grid")]
        public GridLayoutGroup collectionGrid;
        
        [Tooltip("Deck cards area")]
        public GridLayoutGroup deckGrid;
        
        [Header("🎨 Color Scheme")]
        [Tooltip("Collection panel tint color")]
        public Color collectionPanelTint = Color.white;
        
        [Tooltip("Deck panel tint color")]
        public Color deckPanelTint = Color.white;
        
        [Tooltip("Management bar tint color")]
        public Color managementBarTint = Color.white;
        
        [Tooltip("Card background tint")]
        public Color cardTint = Color.white;
        
        // Runtime data
        private List<RealSupabaseClient.CollectionItem> userCollection;
        private List<RealSupabaseClient.Deck> userDecks;
        private RealSupabaseClient.Deck currentDeck;
        
        void Start()
        {
            // Don't hide immediately - wait for proper initialization
            InitializeEditableDeckBuilder();
        }
        
        private async void InitializeEditableDeckBuilder()
        {
            Debug.Log("🎮 Initializing EDITABLE Deck Builder...");
            
            // Apply visual settings from Inspector
            ApplyInspectorSettings();
            
            // Add back button for navigation
            CreateBackButton();
            
            // Load data from database
            await LoadDeckBuilderData();
            
            // Create UI content
            CreateEditableUI();
            
            // Create the actual UI content
            CreateDeckBuilderContent();
            
            Debug.Log("✅ Editable Deck Builder ready! Edit in Inspector for instant changes.");
        }
        
        private void CreateDeckBuilderContent()
        {
            Debug.Log("🎨 Creating deck builder content with your custom backgrounds...");
            
            // Create collection panel with your custom background
            if (collectionPanelArea != null)
            {
                // Apply your custom collection background
                Image collectionImg = collectionPanelArea.GetComponent<Image>();
                if (collectionImg == null) collectionImg = collectionPanelArea.gameObject.AddComponent<Image>();
                
                if (collectionPanelBackground != null)
                {
                    collectionImg.sprite = collectionPanelBackground;
                    collectionImg.color = Color.white;
                    Debug.Log("✅ Applied your custom collection panel background!");
                }
                else
                {
                    collectionImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Fallback
                }
                
                // Create collection content
                CreateCollectionCards(collectionPanelArea);
            }
            
            // Create deck panel with your custom background  
            if (deckPanelArea != null)
            {
                // Apply your custom deck background
                Image deckImg = deckPanelArea.GetComponent<Image>();
                if (deckImg == null) deckImg = deckPanelArea.gameObject.AddComponent<Image>();
                
                if (deckPanelBackground != null)
                {
                    deckImg.sprite = deckPanelBackground;
                    deckImg.color = Color.white;
                    Debug.Log("✅ Applied your custom deck panel background!");
                }
                else
                {
                    deckImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Fallback
                }
                
                // Create deck content
                CreateDeckCards(deckPanelArea);
            }
        }
        
        private void CreateCollectionCards(RectTransform parent)
        {
            if (userCollection == null || userCollection.Count == 0)
            {
                Debug.LogWarning("⚠️ No collection data available for display");
                return;
            }
            
            Debug.Log($"🃏 Creating collection display with {userCollection.Count} cards");
            
            // Create scroll view for collection
            GameObject scrollView = new GameObject("Collection Scroll View");
            scrollView.transform.SetParent(parent, false);
            scrollView.layer = 5;
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.1f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.9f);
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
            grid.cellSize = collectionCardSize;
            grid.spacing = cardSpacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = collectionColumnsPerRow;
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            // Create card displays
            int cardsCreated = 0;
            foreach (var collectionItem in userCollection.Where(item => item.quantity > 0))
            {
                CreateCollectionCard(collectionItem, content.transform);
                cardsCreated++;
                
                if (cardsCreated >= 50) break; // Limit for performance
            }
            
            Debug.Log($"✅ Created {cardsCreated} collection cards");
        }
        
        private void CreateDeckCards(RectTransform parent)
        {
            if (currentDeck == null || currentDeck.cards == null)
            {
                Debug.LogWarning("⚠️ No deck data available for display");
                CreateDeckPlaceholders(parent);
                return;
            }
            
            Debug.Log($"🃏 Creating deck display with {currentDeck.cards.Count} unique cards");
            
            // Create 30-slot grid for deck
            GameObject deckGrid = new GameObject("Deck Grid");
            deckGrid.transform.SetParent(parent, false);
            deckGrid.layer = 5;
            
            RectTransform deckGridRect = deckGrid.AddComponent<RectTransform>();
            deckGridRect.anchorMin = new Vector2(0.05f, 0.1f);
            deckGridRect.anchorMax = new Vector2(0.95f, 0.9f);
            deckGridRect.offsetMin = Vector2.zero;
            deckGridRect.offsetMax = Vector2.zero;
            
            GridLayoutGroup grid = deckGrid.AddComponent<GridLayoutGroup>();
            grid.cellSize = deckCardSize;
            grid.spacing = new Vector2(3, 3);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = deckColumnsPerRow;
            
            // Create deck cards
            int totalCards = 0;
            foreach (var deckCard in currentDeck.cards)
            {
                for (int i = 0; i < deckCard.quantity; i++)
                {
                    CreateDeckCard(deckCard.card, deckGrid.transform);
                    totalCards++;
                    
                    if (totalCards >= 30) break;
                }
                if (totalCards >= 30) break;
            }
            
            // Fill remaining slots with placeholders
            for (int i = totalCards; i < 30; i++)
            {
                CreateEmptyDeckSlot(deckGrid.transform);
            }
            
            Debug.Log($"✅ Created deck display with {totalCards} cards");
        }
        
        private void CreateDeckPlaceholders(RectTransform parent)
        {
            Debug.Log("📋 Creating deck placeholders (no active deck)");
            
            GameObject deckGrid = new GameObject("Empty Deck Grid");
            deckGrid.transform.SetParent(parent, false);
            deckGrid.layer = 5;
            
            RectTransform deckGridRect = deckGrid.AddComponent<RectTransform>();
            deckGridRect.anchorMin = new Vector2(0.05f, 0.1f);
            deckGridRect.anchorMax = new Vector2(0.95f, 0.9f);
            deckGridRect.offsetMin = Vector2.zero;
            deckGridRect.offsetMax = Vector2.zero;
            
            GridLayoutGroup grid = deckGrid.AddComponent<GridLayoutGroup>();
            grid.cellSize = deckCardSize;
            grid.spacing = new Vector2(3, 3);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = deckColumnsPerRow;
            
            // Create 30 empty slots
            for (int i = 0; i < 30; i++)
            {
                CreateEmptyDeckSlot(deckGrid.transform);
            }
        }
        
        private void CreateCollectionCard(RealSupabaseClient.CollectionItem collectionItem, Transform parent)
        {
            GameObject cardObj = new GameObject($"Card: {collectionItem.card.name}");
            cardObj.transform.SetParent(parent, false);
            cardObj.layer = 5;
            
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = GetElementalColor(collectionItem.card.potato_type);
            
            // Card name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = collectionItem.card.name;
            nameText.fontSize = 8;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.2f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Stats
            CreateCardStats(collectionItem.card, cardObj.transform);
            
            // Quantity badge if multiple
            if (collectionItem.quantity > 1)
            {
                CreateQuantityBadge(collectionItem.quantity, cardObj.transform);
            }
        }
        
        private void CreateDeckCard(RealSupabaseClient.EnhancedCard card, Transform parent)
        {
            GameObject cardObj = new GameObject($"Deck Card: {card.name}");
            cardObj.transform.SetParent(parent, false);
            cardObj.layer = 5;
            
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = GetElementalColor(card.potato_type);
            
            // Card name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = card.name;
            nameText.fontSize = 7;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.2f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Stats
            CreateCardStats(card, cardObj.transform);
        }
        
        private void CreateEmptyDeckSlot(Transform parent)
        {
            GameObject slotObj = new GameObject("Empty Slot");
            slotObj.transform.SetParent(parent, false);
            slotObj.layer = 5;
            
            Image slotImg = slotObj.AddComponent<Image>();
            slotImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Gray empty slot
            
            // Plus icon for empty slots
            GameObject plusObj = new GameObject("Plus");
            plusObj.transform.SetParent(slotObj.transform, false);
            plusObj.layer = 5;
            
            TextMeshProUGUI plusText = plusObj.AddComponent<TextMeshProUGUI>();
            plusText.text = "+";
            plusText.fontSize = 20;
            plusText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            plusText.alignment = TextAlignmentOptions.Center;
            plusText.fontStyle = FontStyles.Bold;
            
            RectTransform plusRect = plusObj.GetComponent<RectTransform>();
            plusRect.anchorMin = Vector2.zero;
            plusRect.anchorMax = Vector2.one;
            plusRect.offsetMin = Vector2.zero;
            plusRect.offsetMax = Vector2.zero;
        }
        
        private void CreateCardStats(RealSupabaseClient.EnhancedCard card, Transform parent)
        {
            // Mana cost
            CreateStatDisplay("MANA", card.mana_cost.ToString(), new Vector2(0f, 0.7f), new Vector2(0.3f, 0.9f), Color.blue, parent);
            
            // Attack
            CreateStatDisplay("ATK", card.attack.ToString(), new Vector2(0.35f, 0.7f), new Vector2(0.65f, 0.9f), Color.red, parent);
            
            // Health
            CreateStatDisplay("HP", card.hp.ToString(), new Vector2(0.7f, 0.7f), new Vector2(1f, 0.9f), Color.green, parent);
        }
        
        private void CreateStatDisplay(string label, string value, Vector2 anchorMin, Vector2 anchorMax, Color color, Transform parent)
        {
            GameObject statObj = new GameObject($"{label}: {value}");
            statObj.transform.SetParent(parent, false);
            statObj.layer = 5;
            
            TextMeshProUGUI statText = statObj.AddComponent<TextMeshProUGUI>();
            statText.text = $"{label}:{value}";
            statText.fontSize = 6;
            statText.color = color;
            statText.alignment = TextAlignmentOptions.Center;
            statText.fontStyle = FontStyles.Bold;
            
            RectTransform statRect = statObj.GetComponent<RectTransform>();
            statRect.anchorMin = anchorMin;
            statRect.anchorMax = anchorMax;
            statRect.offsetMin = Vector2.zero;
            statRect.offsetMax = Vector2.zero;
        }
        
        private void CreateQuantityBadge(int quantity, Transform parent)
        {
            GameObject badgeObj = new GameObject($"x{quantity}");
            badgeObj.transform.SetParent(parent, false);
            badgeObj.layer = 5;
            
            Image badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.color = new Color(1f, 0.8f, 0f, 0.9f); // Gold badge
            
            RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.7f, 0.8f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;
            
            GameObject badgeText = new GameObject("Badge Text");
            badgeText.transform.SetParent(badgeObj.transform, false);
            badgeText.layer = 5;
            
            TextMeshProUGUI badgeTextComp = badgeText.AddComponent<TextMeshProUGUI>();
            badgeTextComp.text = $"x{quantity}";
            badgeTextComp.fontSize = 8;
            badgeTextComp.color = Color.black;
            badgeTextComp.alignment = TextAlignmentOptions.Center;
            badgeTextComp.fontStyle = FontStyles.Bold;
            
            RectTransform badgeTextRect = badgeText.GetComponent<RectTransform>();
            badgeTextRect.anchorMin = Vector2.zero;
            badgeTextRect.anchorMax = Vector2.one;
            badgeTextRect.offsetMin = Vector2.zero;
            badgeTextRect.offsetMax = Vector2.zero;
        }
        
        private Color GetElementalColor(string element)
        {
            return element?.ToLower() switch
            {
                "fire" => new Color(1f, 0.3f, 0.1f, 1f),      // Red-orange
                "ice" => new Color(0.1f, 0.6f, 1f, 1f),       // Light blue
                "lightning" => new Color(1f, 1f, 0.2f, 1f),   // Yellow
                "light" => new Color(1f, 1f, 0.8f, 1f),       // Light yellow
                "void" => new Color(0.4f, 0.1f, 0.6f, 1f),    // Purple
                "exotic" => new Color(1f, 0.5f, 1f, 1f),      // Pink
                _ => new Color(0.5f, 0.5f, 0.5f, 1f)          // Gray default
            };
        }
        
        private void ApplyInspectorSettings()
        {
            // Apply background tints to Image components (not sprites directly)
            // Tints will be applied when sprites are assigned to Image components
            // in CreateDeckBuilderContent() method
            
            // Apply card grid settings
            if (collectionGrid != null)
            {
                collectionGrid.cellSize = collectionCardSize;
                collectionGrid.spacing = cardSpacing;
                collectionGrid.constraintCount = collectionColumnsPerRow;
            }
            
            if (deckGrid != null)
            {
                deckGrid.cellSize = deckCardSize;
                deckGrid.spacing = cardSpacing;
                deckGrid.constraintCount = deckColumnsPerRow;
            }
            
            Debug.Log("✅ Applied Inspector settings to UI");
        }
        
        private async System.Threading.Tasks.Task LoadDeckBuilderData()
        {
            // Wait for RealSupabaseClient to be ready
            int attempts = 0;
            while (RealSupabaseClient.Instance == null && attempts < 50) // Wait up to 5 seconds
            {
                await System.Threading.Tasks.Task.Delay(100);
                attempts++;
            }
            
            if (RealSupabaseClient.Instance == null)
            {
                Debug.LogError("❌ RealSupabaseClient not found after waiting!");
                return;
            }
            
            // Load user collection and decks
            userCollection = await RealSupabaseClient.Instance.LoadUserCollection();
            userDecks = await RealSupabaseClient.Instance.LoadUserDecks();
            
            // Load active deck
            var activeDeck = userDecks.FirstOrDefault(d => d.is_active);
            if (activeDeck != null)
            {
                currentDeck = await RealSupabaseClient.Instance.LoadDeckCards(activeDeck.id);
            }
            
            Debug.Log($"✅ Loaded {userCollection.Count} cards and {userDecks.Count} decks");
        }
        
        private void CreateEditableUI()
        {
            // Update title texts
            if (titleText != null)
                titleText.text = "🔧 DECK BUILDER";
            
            if (activeDeckText != null)
                activeDeckText.text = currentDeck != null ? $"👑 Active: {currentDeck.name}" : "👑 No Active Deck";
            
            if (collectionTitleText != null)
                collectionTitleText.text = $"YOUR COLLECTION ({userCollection.Count})";
            
            if (deckTitleText != null)
                deckTitleText.text = currentDeck != null ? $"{currentDeck.name} ({currentDeck.total_cards}/30)" : "No Deck Selected";
            
            // Create collection cards
            CreateCollectionCards();
            
            // Create deck cards
            CreateDeckCards();
            
            // Setup button events
            SetupButtonEvents();
        }
        
        private void CreateCollectionCards()
        {
            if (collectionGrid == null || userCollection == null) return;
            
            // Clear existing cards
            foreach (Transform child in collectionGrid.transform)
            {
                if (child.name.StartsWith("CollectionCard_"))
                    DestroyImmediate(child.gameObject);
            }
            
            // Create cards for owned items
            var ownedCards = userCollection.Where(item => item.quantity > 0);
            
            foreach (var collectionItem in ownedCards)
            {
                GameObject cardObj = CreateEditableCollectionCard(collectionItem);
                cardObj.transform.SetParent(collectionGrid.transform, false);
            }
            
            Debug.Log($"✅ Created {ownedCards.Count()} collection cards");
        }
        
        private void CreateDeckCards()
        {
            if (deckGrid == null) return;
            
            // Clear existing cards
            foreach (Transform child in deckGrid.transform)
            {
                if (child.name.StartsWith("DeckCard_") || child.name.StartsWith("DeckSlot_"))
                    DestroyImmediate(child.gameObject);
            }
            
            if (currentDeck?.cards != null)
            {
                // Create deck cards
                int cardCount = 0;
                foreach (var deckCard in currentDeck.cards)
                {
                    for (int i = 0; i < deckCard.quantity; i++)
                    {
                        GameObject cardObj = CreateEditableDeckCard(deckCard.card);
                        cardObj.transform.SetParent(deckGrid.transform, false);
                        cardCount++;
                    }
                }
                
                // Fill remaining slots with placeholders
                for (int i = cardCount; i < 30; i++)
                {
                    GameObject slotObj = CreateEditableDeckSlot(i + 1);
                    slotObj.transform.SetParent(deckGrid.transform, false);
                }
                
                Debug.Log($"✅ Created deck display with {cardCount} cards");
            }
            else
            {
                // Create 30 empty slots
                for (int i = 0; i < 30; i++)
                {
                    GameObject slotObj = CreateEditableDeckSlot(i + 1);
                    slotObj.transform.SetParent(deckGrid.transform, false);
                }
            }
        }
        
        private GameObject CreateEditableCollectionCard(RealSupabaseClient.CollectionItem collectionItem)
        {
            var card = collectionItem.card;
            
            GameObject cardObj = new GameObject($"CollectionCard_{card.name}");
            cardObj.layer = 5;
            
            // Card background
            Image cardImage = cardObj.AddComponent<Image>();
            cardImage.color = GetElementalColor(card.potato_type) * cardTint;
            
            // Card name
            GameObject nameObj = new GameObject("CardName");
            nameObj.transform.SetParent(cardObj.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = card.name;
            nameText.fontSize = 10;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.95f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Mana cost
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), Color.cyan, new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            
            // Attack/Health
            if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), Color.red, new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
            if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), Color.green, new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
            
            // Add button
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => AddCardToDeck(card));
            
            return cardObj;
        }
        
        private GameObject CreateEditableDeckCard(RealSupabaseClient.EnhancedCard card)
        {
            GameObject cardObj = new GameObject($"DeckCard_{card.name}");
            cardObj.layer = 5;
            
            // Card background
            Image cardImage = cardObj.AddComponent<Image>();
            cardImage.color = GetElementalColor(card.potato_type) * cardTint;
            
            // Card name (smaller for deck view)
            GameObject nameObj = new GameObject("CardName");
            nameObj.transform.SetParent(cardObj.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = card.name;
            nameText.fontSize = 8;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.95f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Stats (smaller)
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), Color.cyan, new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), Color.red, new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
            if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), Color.green, new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
            
            // Remove button
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => RemoveCardFromDeck(card));
            
            // Visual feedback for removal
            ColorBlock colors = cardButton.colors;
            colors.highlightedColor = new Color(1f, 0.8f, 0.8f, 1f);
            cardButton.colors = colors;
            
            return cardObj;
        }
        
        private GameObject CreateEditableDeckSlot(int slotNumber)
        {
            GameObject slotObj = new GameObject($"DeckSlot_{slotNumber}");
            slotObj.layer = 5;
            
            Image slotImage = slotObj.AddComponent<Image>();
            slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            
            // Slot number
            GameObject numberObj = new GameObject("SlotNumber");
            numberObj.transform.SetParent(slotObj.transform, false);
            numberObj.layer = 5;
            
            TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
            numberText.text = slotNumber.ToString();
            numberText.fontSize = 12;
            numberText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            numberText.alignment = TextAlignmentOptions.Center;
            
            RectTransform numberRect = numberObj.GetComponent<RectTransform>();
            numberRect.anchorMin = Vector2.zero;
            numberRect.anchorMax = Vector2.one;
            numberRect.offsetMin = Vector2.zero;
            numberRect.offsetMax = Vector2.zero;
            
            return slotObj;
        }
        
        private void CreateCardStat(Transform parent, string value, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject statObj = new GameObject($"Stat_{value}");
            statObj.transform.SetParent(parent, false);
            statObj.layer = 5;
            
            Image statBg = statObj.AddComponent<Image>();
            statBg.color = color;
            
            TextMeshProUGUI statText = statObj.AddComponent<TextMeshProUGUI>();
            statText.text = value;
            statText.fontSize = 10;
            statText.color = Color.white;
            statText.alignment = TextAlignmentOptions.Center;
            statText.fontStyle = FontStyles.Bold;
            
            RectTransform statRect = statObj.GetComponent<RectTransform>();
            statRect.anchorMin = anchorMin;
            statRect.anchorMax = anchorMax;
            statRect.offsetMin = Vector2.zero;
            statRect.offsetMax = Vector2.zero;
        }
        
        public void RefreshScreen()
        {
            Debug.Log("🔄 Refreshing editable deck builder...");
            ApplyInspectorSettings();
            
            // Reload data and recreate content
            _ = LoadDeckBuilderData();
            
            // Recreate content if areas exist
            if (collectionPanelArea != null && deckPanelArea != null)
            {
                CreateDeckBuilderContent();
            }
        }
        
        public void ResetToDefaults()
        {
            Debug.Log("🔄 Resetting deck builder to defaults...");
            
            // Reset visual settings
            collectionCardSize = new Vector2(100f, 140f);
            deckCardSize = new Vector2(80f, 110f);
            collectionColumnsPerRow = 3;
            deckColumnsPerRow = 5;
            cardTint = Color.white;
            
            // Reset layout if areas exist
            if (managementBarArea != null)
            {
                managementBarArea.anchorMin = new Vector2(0.02f, 0.9f);
                managementBarArea.anchorMax = new Vector2(0.98f, 0.98f);
            }
            
            if (collectionPanelArea != null)
            {
                collectionPanelArea.anchorMin = new Vector2(0.02f, 0.15f);
                collectionPanelArea.anchorMax = new Vector2(0.48f, 0.88f);
            }
            
            if (deckPanelArea != null)
            {
                deckPanelArea.anchorMin = new Vector2(0.52f, 0.15f);
                deckPanelArea.anchorMax = new Vector2(0.98f, 0.88f);
            }
            
            ApplyInspectorSettings();
        }
        
        private void SetupButtonEvents()
        {
            if (createDeckButton != null)
                createDeckButton.onClick.AddListener(() => Debug.Log("🃏 Create new deck - TODO: Implement"));
            
            if (deckSelectorButton != null)
                deckSelectorButton.onClick.AddListener(() => Debug.Log("🃏 Deck selector - TODO: Implement"));
            
            if (saveDeckButton != null)
                saveDeckButton.onClick.AddListener(() => Debug.Log("🃏 Save deck - TODO: Implement"));
            
            if (backButton != null)
                backButton.onClick.AddListener(() => {
                    var uiManager = FindObjectOfType<ProductionUIManager>();
                    if (uiManager != null) uiManager.ShowScreen(ProductionUIManager.GameScreen.MainMenu);
                });
        }
        
        private void AddCardToDeck(RealSupabaseClient.EnhancedCard card)
        {
            Debug.Log($"🃏 Adding {card.name} to deck - TODO: Implement deck building logic");
            // TODO: Implement add to deck functionality
        }
        
        private void RemoveCardFromDeck(RealSupabaseClient.EnhancedCard card)
        {
            Debug.Log($"🃏 Removing {card.name} from deck - TODO: Implement deck building logic");
            // TODO: Implement remove from deck functionality
        }
        
        // Inspector button to refresh the UI
        [ContextMenu("🔄 Refresh Deck Builder UI")]
        public void RefreshUI()
        {
            if (Application.isPlaying)
            {
                ApplyInspectorSettings();
                CreateEditableUI();
                Debug.Log("🔄 Deck Builder UI refreshed with Inspector settings!");
            }
        }
        
        // Inspector button to apply settings in real-time
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyInspectorSettings();
            }
        }
        
        private void CreateBackButton()
        {
            // Create back button
            GameObject backButton = new GameObject("🔙 Back to Main Menu");
            backButton.transform.SetParent(transform, false);
            backButton.layer = 5;
            
            RectTransform backRect = backButton.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.02f, 0.92f);
            backRect.anchorMax = new Vector2(0.15f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            
            Button backBtn = backButton.AddComponent<Button>();
            Image backImg = backButton.AddComponent<Image>();
            backImg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Red back button
            
            // Back button text
            GameObject backText = new GameObject("Back Text");
            backText.transform.SetParent(backButton.transform, false);
            backText.layer = 5;
            
            TextMeshProUGUI backTextComp = backText.AddComponent<TextMeshProUGUI>();
            backTextComp.text = "← BACK";
            backTextComp.fontSize = 14;
            backTextComp.color = Color.white;
            backTextComp.alignment = TextAlignmentOptions.Center;
            backTextComp.fontStyle = FontStyles.Bold;
            
            RectTransform backTextRect = backText.GetComponent<RectTransform>();
            backTextRect.anchorMin = Vector2.zero;
            backTextRect.anchorMax = Vector2.one;
            backTextRect.offsetMin = Vector2.zero;
            backTextRect.offsetMax = Vector2.zero;
            
            // Back button click handler
            backBtn.onClick.AddListener(() => {
                Debug.Log("🔙 Back button clicked - returning to main menu");
                
                // Find ProductionUIManager and call return method
                ProductionUIManager uiManager = FindObjectOfType<ProductionUIManager>();
                if (uiManager != null)
                {
                    uiManager.ReturnToMainMenuFromCustomDeckBuilder();
                }
                else
                {
                    Debug.LogError("❌ ProductionUIManager not found!");
                }
            });
            
            Debug.Log("✅ Back button added to editable deck builder");
        }
    }
}