using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FUNCTIONAL Deck Builder that actually creates and manages 30-card decks
    /// Recreates the FantasyDeckBuilder.tsx functionality from your web version
    /// </summary>
    public class FunctionalDeckBuilder : MonoBehaviour
    {
        [Header("🔧 Functional Deck Builder")]
        [SerializeField] private ScrollRect availableCardsScrollRect;
        [SerializeField] private ScrollRect deckScrollRect;
        [SerializeField] private GridLayoutGroup availableCardsGrid;
        [SerializeField] private GridLayoutGroup deckGrid;
        [SerializeField] private TMP_InputField deckNameField;
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI validationText;
        [SerializeField] private Button saveDeckButton;
        
        // Real deck data
        private List<RealSupabaseClient.Deck> userDecks = new List<RealSupabaseClient.Deck>();
        private RealSupabaseClient.Deck currentDeck;
        private Dictionary<string, int> deckCardCounts = new Dictionary<string, int>();
        private List<RealSupabaseClient.CollectionItem> availableCards = new List<RealSupabaseClient.CollectionItem>();
        private List<GameObject> availableCardDisplays = new List<GameObject>();
        private List<GameObject> deckCardDisplays = new List<GameObject>();
        
        // UI References
        private Canvas deckBuilderCanvas;
        private GameObject deckBuilderUI;
        
        private void Start()
        {
            CreateFunctionalDeckBuilderUI();
        }
        
        private void CreateFunctionalDeckBuilderUI()
        {
            Debug.Log("🔧 Creating FUNCTIONAL deck builder...");
            
            // Create deck builder canvas
            GameObject canvasObj = new GameObject("Deck Builder Canvas");
            deckBuilderCanvas = canvasObj.AddComponent<Canvas>();
            deckBuilderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            deckBuilderCanvas.sortingOrder = 200;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create deck builder interface
            CreateDeckBuilderInterface();
            
            // Start hidden
            deckBuilderCanvas.gameObject.SetActive(false);
            
            Debug.Log("✅ FUNCTIONAL deck builder UI created");
        }
        
        private void CreateDeckBuilderInterface()
        {
            // Main background
            deckBuilderUI = CreatePanel("Deck Builder Screen", deckBuilderCanvas.transform);
            Image bgImage = deckBuilderUI.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.05f, 0.15f, 1f); // Dark purple
            SetFullScreen(deckBuilderUI.GetComponent<RectTransform>());
            
            // Header
            CreateDeckBuilderHeader();
            
            // Deck info panel
            CreateDeckInfoPanel();
            
            // Split view: Available cards (left) | Current deck (right)
            CreateDeckBuilderSplitView();
            
            // Controls
            CreateDeckBuilderControls();
        }
        
        private void CreateDeckBuilderHeader()
        {
            GameObject headerPanel = CreatePanel("Deck Builder Header", deckBuilderUI.transform);
            Image headerBg = headerPanel.GetComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.7f);
            
            RectTransform headerRect = headerPanel.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.9f);
            headerRect.anchorMax = new Vector2(0.95f, 0.98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Deck Builder Title");
            titleObj.transform.SetParent(headerPanel.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DECK BUILDER";
            titleText.fontSize = 28;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(titleObj.GetComponent<RectTransform>());
        }
        
        private void CreateDeckInfoPanel()
        {
            GameObject infoPanel = CreatePanel("Deck Info Panel", deckBuilderUI.transform);
            Image infoBg = infoPanel.GetComponent<Image>();
            infoBg.color = new Color(0f, 0f, 0f, 0.5f);
            
            RectTransform infoRect = infoPanel.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.05f, 0.82f);
            infoRect.anchorMax = new Vector2(0.95f, 0.88f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            
            // Deck name field
            deckNameField = CreateDeckNameField(infoPanel.transform);
            
            // Deck count
            GameObject countObj = new GameObject("Deck Count");
            countObj.transform.SetParent(infoPanel.transform, false);
            countObj.layer = 5;
            
            deckCountText = countObj.AddComponent<TextMeshProUGUI>();
            deckCountText.text = "Cards: 0/30";
            deckCountText.fontSize = 18;
            deckCountText.color = Color.white;
            deckCountText.alignment = TextAlignmentOptions.Center;
            deckCountText.fontStyle = FontStyles.Bold;
            
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.4f, 0f);
            countRect.anchorMax = new Vector2(0.6f, 1f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
            
            // Validation status
            GameObject validationObj = new GameObject("Validation Status");
            validationObj.transform.SetParent(infoPanel.transform, false);
            validationObj.layer = 5;
            
            validationText = validationObj.AddComponent<TextMeshProUGUI>();
            validationText.text = "Need 30 cards";
            validationText.fontSize = 16;
            validationText.color = new Color(1f, 0.3f, 0.3f, 1f);
            validationText.alignment = TextAlignmentOptions.Right;
            
            RectTransform validationRect = validationObj.GetComponent<RectTransform>();
            validationRect.anchorMin = new Vector2(0.65f, 0f);
            validationRect.anchorMax = new Vector2(0.95f, 1f);
            validationRect.offsetMin = Vector2.zero;
            validationRect.offsetMax = Vector2.zero;
        }
        
        private TMP_InputField CreateDeckNameField(Transform parent)
        {
            GameObject nameFieldObj = CreatePanel("Deck Name Field", parent);
            nameFieldObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            
            RectTransform nameRect = nameFieldObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.2f);
            nameRect.anchorMax = new Vector2(0.35f, 0.8f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            TMP_InputField inputField = nameFieldObj.AddComponent<TMP_InputField>();
            inputField.text = "New Deck";
            
            // Input text
            GameObject inputTextObj = new GameObject("Deck Name Text");
            inputTextObj.transform.SetParent(nameFieldObj.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "New Deck";
            inputText.fontSize = 16;
            inputText.color = Color.black;
            inputText.alignment = TextAlignmentOptions.Center;
            
            SetFullScreen(inputTextObj.GetComponent<RectTransform>());
            inputField.textComponent = inputText;
            
            return inputField;
        }
        
        private void CreateDeckBuilderSplitView()
        {
            // Available cards panel (left)
            GameObject availablePanel = CreatePanel("Available Cards Panel", deckBuilderUI.transform);
            availablePanel.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.05f, 0.8f);
            
            RectTransform availableRect = availablePanel.GetComponent<RectTransform>();
            availableRect.anchorMin = new Vector2(0.02f, 0.15f);
            availableRect.anchorMax = new Vector2(0.48f, 0.8f);
            availableRect.offsetMin = Vector2.zero;
            availableRect.offsetMax = Vector2.zero;
            
            CreateAvailableCardsArea(availablePanel.transform);
            
            // Current deck panel (right)
            GameObject deckPanel = CreatePanel("Current Deck Panel", deckBuilderUI.transform);
            deckPanel.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.1f, 0.8f);
            
            RectTransform deckRect = deckPanel.GetComponent<RectTransform>();
            deckRect.anchorMin = new Vector2(0.52f, 0.15f);
            deckRect.anchorMax = new Vector2(0.98f, 0.8f);
            deckRect.offsetMin = Vector2.zero;
            deckRect.offsetMax = Vector2.zero;
            
            CreateCurrentDeckArea(deckPanel.transform);
        }
        
        private void CreateAvailableCardsArea(Transform parent)
        {
            // Title
            GameObject titleObj = new GameObject("Available Cards Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "YOUR COLLECTION";
            titleText.fontSize = 18;
            titleText.color = new Color(0.8f, 1f, 0.8f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Available cards scroll view
            CreateAvailableCardsScrollView(parent);
        }
        
        private void CreateCurrentDeckArea(Transform parent)
        {
            // Title
            GameObject titleObj = new GameObject("Current Deck Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "CURRENT DECK";
            titleText.fontSize = 18;
            titleText.color = new Color(1f, 0.8f, 1f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Current deck scroll view
            CreateCurrentDeckScrollView(parent);
        }
        
        private void CreateAvailableCardsScrollView(Transform parent)
        {
            GameObject scrollViewObj = new GameObject("Available Cards Scroll");
            scrollViewObj.transform.SetParent(parent, false);
            scrollViewObj.layer = 5;
            
            availableCardsScrollRect = scrollViewObj.AddComponent<ScrollRect>();
            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.05f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.88f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<RectMask2D>();
            
            SetFullScreen(viewport.GetComponent<RectTransform>());
            availableCardsScrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            availableCardsScrollRect.content = contentRect;
            
            // Grid for available cards
            availableCardsGrid = content.AddComponent<GridLayoutGroup>();
            availableCardsGrid.cellSize = new Vector2(120, 160);
            availableCardsGrid.spacing = new Vector2(5, 5);
            availableCardsGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            availableCardsGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            availableCardsGrid.childAlignment = TextAnchor.UpperCenter;
            availableCardsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            availableCardsGrid.constraintCount = 3; // 3 cards per row in available area
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        private void CreateCurrentDeckScrollView(Transform parent)
        {
            GameObject scrollViewObj = new GameObject("Current Deck Scroll");
            scrollViewObj.transform.SetParent(parent, false);
            scrollViewObj.layer = 5;
            
            deckScrollRect = scrollViewObj.AddComponent<ScrollRect>();
            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.05f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.88f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<RectMask2D>();
            
            SetFullScreen(viewport.GetComponent<RectTransform>());
            deckScrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            deckScrollRect.content = contentRect;
            
            // Grid for deck cards
            deckGrid = content.AddComponent<GridLayoutGroup>();
            deckGrid.cellSize = new Vector2(100, 140);
            deckGrid.spacing = new Vector2(5, 5);
            deckGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            deckGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            deckGrid.childAlignment = TextAnchor.UpperCenter;
            deckGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            deckGrid.constraintCount = 4; // 4 cards per row in deck area
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        private void CreateDeckBuilderControls()
        {
            // Back button
            GameObject backBtnObj = CreatePanel("Back Button", deckBuilderUI.transform);
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
            backText.fontSize = 16;
            backText.color = Color.white;
            backText.alignment = TextAlignmentOptions.Center;
            backText.fontStyle = FontStyles.Bold;
            backText.raycastTarget = false;
            
            SetFullScreen(backTextObj.GetComponent<RectTransform>());
            
            backButton.onClick.AddListener(() => {
                HideDeckBuilder();
            });
            
            // Save deck button
            GameObject saveBtnObj = CreatePanel("Save Deck Button", deckBuilderUI.transform);
            saveDeckButton = saveBtnObj.AddComponent<Button>();
            Image saveBtnImg = saveBtnObj.GetComponent<Image>();
            saveBtnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);
            
            RectTransform saveRect = saveBtnObj.GetComponent<RectTransform>();
            saveRect.anchorMin = new Vector2(0.7f, 0.05f);
            saveRect.anchorMax = new Vector2(0.95f, 0.12f);
            saveRect.offsetMin = Vector2.zero;
            saveRect.offsetMax = Vector2.zero;
            
            // Save text
            GameObject saveTextObj = new GameObject("Save Text");
            saveTextObj.transform.SetParent(saveBtnObj.transform, false);
            saveTextObj.layer = 5;
            
            TextMeshProUGUI saveText = saveTextObj.AddComponent<TextMeshProUGUI>();
            saveText.text = "SAVE DECK";
            saveText.fontSize = 16;
            saveText.color = Color.white;
            saveText.alignment = TextAlignmentOptions.Center;
            saveText.fontStyle = FontStyles.Bold;
            saveText.raycastTarget = false;
            
            SetFullScreen(saveTextObj.GetComponent<RectTransform>());
            
            saveDeckButton.onClick.AddListener(async () => {
                await SaveCurrentDeck();
            });
        }
        
        public async void ShowDeckBuilder()
        {
            Debug.Log("🔧 Showing functional deck builder...");
            
            deckBuilderCanvas.gameObject.SetActive(true);
            
            // Load real data
            await LoadDeckBuilderData();
        }
        
        public void HideDeckBuilder()
        {
            Debug.Log("🔧 Hiding deck builder...");
            deckBuilderCanvas.gameObject.SetActive(false);
            
            // Return to main menu
            var productionUI = FindFirstObjectByType<ProductionUIManager>();
            if (productionUI != null)
            {
                productionUI.ShowScreen(ProductionUIManager.GameScreen.MainMenu);
            }
        }
        
        private async Task LoadDeckBuilderData()
        {
            try
            {
                Debug.Log("🔄 Loading REAL deck builder data...");
                
                if (RealSupabaseClient.Instance == null)
                {
                    Debug.LogError("❌ RealSupabaseClient not found!");
                    return;
                }
                
                // Load user's collection
                availableCards = await RealSupabaseClient.Instance.LoadUserCollection();
                Debug.Log($"✅ Loaded {availableCards.Count} available cards");
                
                // Load user's decks
                userDecks = await RealSupabaseClient.Instance.LoadUserDecks();
                Debug.Log($"✅ Loaded {userDecks.Count} user decks");
                
                // Create new deck or load existing
                CreateNewDeck();
                
                // Display available cards
                DisplayAvailableCards();
                
                // Update deck display
                UpdateDeckDisplay();
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading deck builder data: {e.Message}");
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
            
            if (deckNameField) deckNameField.text = "New Deck";
            
            Debug.Log("✅ Created new deck");
        }
        
        private void DisplayAvailableCards()
        {
            // Clear existing displays
            ClearAvailableCardDisplays();
            
            // Create card displays for owned cards
            foreach (var collectionItem in availableCards.Take(30)) // Limit for performance
            {
                if (collectionItem.quantity > 0)
                {
                    GameObject cardObj = CreateAvailableCardDisplay(collectionItem);
                    if (cardObj != null)
                    {
                        availableCardDisplays.Add(cardObj);
                    }
                }
            }
            
            Debug.Log($"✅ Displayed {availableCardDisplays.Count} available cards");
        }
        
        private GameObject CreateAvailableCardDisplay(RealSupabaseClient.CollectionItem collectionItem)
        {
            var card = collectionItem.card;
            
            GameObject cardObj = new GameObject($"Available_{card.name}");
            cardObj.transform.SetParent(availableCardsGrid.transform, false);
            cardObj.layer = 5;
            
            // Card background with elemental theme
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
            CreateCardName(cardObj.transform, card.name);
            
            // Mana cost
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            
            // Attack/Health
            if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
            if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), new Color(0.3f, 1f, 0.3f, 1f), new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
            
            // Quantity owned
            CreateQuantityBadge(cardObj.transform, collectionItem.quantity);
            
            // Add to deck button
            Button addButton = cardObj.AddComponent<Button>();
            addButton.onClick.AddListener(() => {
                AddCardToDeck(card);
            });
            
            // Professional button colors
            ColorBlock colors = addButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            addButton.colors = colors;
            
            return cardObj;
        }
        
        private void AddCardToDeck(RealSupabaseClient.EnhancedCard card)
        {
            // Check deck size
            int totalCardsInDeck = deckCardCounts.Values.Sum();
            if (totalCardsInDeck >= 30)
            {
                Debug.Log("❌ Deck is full (30/30 cards)");
                return;
            }
            
            // Check copy limits
            int currentInDeck = deckCardCounts.ContainsKey(card.id) ? deckCardCounts[card.id] : 0;
            int maxCopies = GetMaxCopiesByRarity(card.rarity);
            
            if (currentInDeck >= maxCopies)
            {
                Debug.Log($"❌ Cannot add more {card.name} - max {maxCopies} copies allowed");
                return;
            }
            
            // Add card to deck
            deckCardCounts[card.id] = currentInDeck + 1;
            UpdateDeckDisplay();
            
            Debug.Log($"✅ Added {card.name} to deck ({currentInDeck + 1}/{maxCopies})");
        }
        
        private void RemoveCardFromDeck(string cardId)
        {
            if (!deckCardCounts.ContainsKey(cardId)) return;
            
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
            Debug.Log($"✅ Removed card from deck");
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
            // Clear existing deck displays
            ClearDeckCardDisplays();
            
            // Update deck count
            int totalCards = deckCardCounts.Values.Sum();
            
            if (deckCountText)
            {
                deckCountText.text = $"Cards: {totalCards}/30";
                deckCountText.color = totalCards == 30 ? new Color(0.2f, 1f, 0.2f, 1f) : Color.white;
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
                    validationText.text = totalCards < 30 ? $"Need {30 - totalCards} more" : "❌ Invalid";
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
            
            // Create deck card displays
            foreach (var kvp in deckCardCounts)
            {
                string cardId = kvp.Key;
                int quantity = kvp.Value;
                
                var card = GetCardById(cardId);
                if (card != null)
                {
                    GameObject deckCardObj = CreateDeckCardDisplay(card, quantity);
                    if (deckCardObj != null)
                    {
                        deckCardDisplays.Add(deckCardObj);
                    }
                }
            }
        }
        
        private GameObject CreateDeckCardDisplay(RealSupabaseClient.EnhancedCard card, int quantity)
        {
            GameObject cardObj = new GameObject($"Deck_{card.name}");
            cardObj.transform.SetParent(deckGrid.transform, false);
            cardObj.layer = 5;
            
            // Card background
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
            CreateCardName(cardObj.transform, card.name);
            
            // Mana cost
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            
            // Quantity in deck
            CreateQuantityBadge(cardObj.transform, quantity);
            
            // Remove from deck button
            Button removeButton = cardObj.AddComponent<Button>();
            removeButton.onClick.AddListener(() => {
                RemoveCardFromDeck(card.id);
            });
            
            // Professional button colors
            ColorBlock colors = removeButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            removeButton.colors = colors;
            
            return cardObj;
        }
        
        private bool ValidateDeck()
        {
            int totalCards = deckCardCounts.Values.Sum();
            
            // Must have exactly 30 cards
            if (totalCards != 30) return false;
            
            // Check copy limits
            foreach (var kvp in deckCardCounts)
            {
                var card = GetCardById(kvp.Key);
                if (card == null) return false;
                
                int maxCopies = GetMaxCopiesByRarity(card.rarity);
                if (kvp.Value > maxCopies) return false;
            }
            
            return true;
        }
        
        private async Task SaveCurrentDeck()
        {
            if (!ValidateDeck())
            {
                Debug.Log("❌ Cannot save invalid deck");
                return;
            }
            
            try
            {
                Debug.Log($"💾 Saving deck: {currentDeck.name}");
                
                // Update deck metadata
                currentDeck.name = deckNameField?.text ?? "New Deck";
                currentDeck.total_cards = deckCardCounts.Values.Sum();
                currentDeck.updated_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                
                // Convert to deck cards
                currentDeck.cards = new List<RealSupabaseClient.DeckCard>();
                foreach (var kvp in deckCardCounts)
                {
                    currentDeck.cards.Add(new RealSupabaseClient.DeckCard
                    {
                        deck_id = currentDeck.id,
                        card_id = kvp.Key,
                        quantity = kvp.Value,
                        card = GetCardById(kvp.Key)
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
        
        private RealSupabaseClient.EnhancedCard GetCardById(string cardId)
        {
            return availableCards.FirstOrDefault(item => item.card.id == cardId)?.card;
        }
        
        #region Helper Methods (Same as Collection)
        
        private void CreateCardName(Transform parent, string name)
        {
            GameObject nameObj = new GameObject("Card Name");
            nameObj.transform.SetParent(parent, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = name;
            nameText.fontSize = 10;
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
            
            GameObject textObj = new GameObject("Stat Text");
            textObj.transform.SetParent(statObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI statText = textObj.AddComponent<TextMeshProUGUI>();
            statText.text = value;
            statText.fontSize = 10;
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
        
        private void ClearAvailableCardDisplays()
        {
            foreach (GameObject cardDisplay in availableCardDisplays)
            {
                if (cardDisplay != null) Destroy(cardDisplay);
            }
            availableCardDisplays.Clear();
        }
        
        private void ClearDeckCardDisplays()
        {
            foreach (GameObject cardDisplay in deckCardDisplays)
            {
                if (cardDisplay != null) Destroy(cardDisplay);
            }
            deckCardDisplays.Clear();
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