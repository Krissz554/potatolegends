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
        public Image collectionPanelBackground;
        
        [Tooltip("Deck panel background image")]
        public Image deckPanelBackground;
        
        [Tooltip("Management bar background image")]
        public Image managementBarBackground;
        
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
            // Hide initially - only show when deck builder button is clicked
            gameObject.SetActive(false);
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
            
            Debug.Log("✅ Editable Deck Builder ready! Edit in Inspector for instant changes.");
        }
        
        private void ApplyInspectorSettings()
        {
            // Apply background tints
            if (collectionPanelBackground != null)
                collectionPanelBackground.color = collectionPanelTint;
            
            if (deckPanelBackground != null)
                deckPanelBackground.color = deckPanelTint;
            
            if (managementBarBackground != null)
                managementBarBackground.color = managementBarTint;
            
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
            if (RealSupabaseClient.Instance == null)
            {
                Debug.LogError("❌ RealSupabaseClient not found!");
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
        
        private Color GetElementalColor(string potatoType)
        {
            switch (potatoType?.ToLower())
            {
                case "fire": return new Color(1f, 0.3f, 0.2f, 1f);
                case "ice": return new Color(0.3f, 0.7f, 1f, 1f);
                case "lightning": return new Color(1f, 1f, 0.3f, 1f);
                case "light": return new Color(1f, 0.95f, 0.7f, 1f);
                case "void": return new Color(0.5f, 0.3f, 0.8f, 1f);
                default: return new Color(0.7f, 0.7f, 0.7f, 1f);
            }
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