using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// WORKING Screen Manager that actually loads and displays real cards
/// Simple, reliable approach that works with your Supabase data
/// </summary>
public class WorkingScreenManager : MonoBehaviour
{
    [Header("🎮 Working Game Screens")]
    [SerializeField] private bool autoStart = true;
    
    // Screen management
    private enum Screen { Auth, MainMenu, Collection, DeckBuilder, HeroHall }
    private Screen currentScreen = Screen.Auth;
    
    // UI References
    private Canvas gameCanvas;
    private GameObject currentScreenObject;
    private RealSupabaseClient supabaseClient;
    
    // Auth fields
    private TMP_InputField emailField;
    private TMP_InputField passwordField;
    private TextMeshProUGUI statusText;
    
    // Real data from database
    private List<RealSupabaseClient.EnhancedCard> allCards = new List<RealSupabaseClient.EnhancedCard>();
    private List<RealSupabaseClient.CollectionItem> userCollection = new List<RealSupabaseClient.CollectionItem>();
    private List<RealSupabaseClient.Hero> availableHeroes = new List<RealSupabaseClient.Hero>();
    private List<RealSupabaseClient.UserHero> userHeroes = new List<RealSupabaseClient.UserHero>();
    
    // Deck building
    private Dictionary<string, int> currentDeckCards = new Dictionary<string, int>();
    private string currentDeckName = "New Deck";
    
    private void Start()
    {
        if (autoStart)
        {
            StartWorkingGame();
        }
    }
    
    private void StartWorkingGame()
    {
        Debug.Log("🎮 Starting WORKING Potato Legends game...");
        
        CreateGameCanvas();
        CreateSupabaseClient();
        ShowAuthScreen();
        
        Debug.Log("✅ Working game started!");
    }
    
    private void CreateGameCanvas()
    {
        GameObject canvasObj = new GameObject("Working Game Canvas");
        gameCanvas = canvasObj.AddComponent<Canvas>();
        gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // EventSystem
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }
    
    private void CreateSupabaseClient()
    {
        GameObject supabaseObj = new GameObject("RealSupabaseClient");
        supabaseClient = supabaseObj.AddComponent<RealSupabaseClient>();
        supabaseClient.OnAuthenticationChanged += OnAuthChanged;
    }
    
    private void ShowAuthScreen()
    {
        currentScreen = Screen.Auth;
        ClearScreen();
        
        Debug.Log("🔐 Creating working auth screen...");
        
        currentScreenObject = CreatePanel("Auth Screen", gameCanvas.transform);
        Image bg = currentScreenObject.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.1f, 0.2f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        // Title
        CreateTitle("POTATO LEGENDS\nMOBILE CARD GAME");
        
        // Email field
        emailField = CreateInputField("Email Field", "Enter your email...", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.65f));
        
        // Password field
        passwordField = CreateInputField("Password Field", "Enter your password...", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f));
        passwordField.inputType = TMP_InputField.InputType.Password;
        
        // Login button
        Button loginButton = CreateButton("Login Button", "ENTER GAME", new Color(0.2f, 0.7f, 0.3f, 1f), new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.35f));
        loginButton.onClick.AddListener(async () => await HandleLogin());
        
        // Status text
        statusText = CreateStatusText("Enter your Supabase credentials to play!");
        
        Debug.Log("✅ Working auth screen created");
    }
    
    private void ShowMainMenu()
    {
        currentScreen = Screen.MainMenu;
        ClearScreen();
        
        Debug.Log("🏠 Creating working main menu...");
        
        currentScreenObject = CreatePanel("Main Menu", gameCanvas.transform);
        Image bg = currentScreenObject.GetComponent<Image>();
        bg.color = new Color(0.1f, 0.15f, 0.3f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        // Title
        CreateTitle("POTATO LEGENDS\nWelcome to the arena!");
        
        // Navigation buttons
        CreateButton("Collection Button", "COLLECTION", new Color(0.2f, 0.7f, 0.3f, 1f), new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.65f)).onClick.AddListener(() => ShowCollection());
        CreateButton("Deck Builder Button", "DECK BUILDER", new Color(0.7f, 0.3f, 0.7f, 1f), new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f)).onClick.AddListener(() => ShowDeckBuilder());
        CreateButton("Hero Hall Button", "HERO HALL", new Color(0.9f, 0.6f, 0.2f, 1f), new Vector2(0.1f, 0.29f), new Vector2(0.9f, 0.39f)).onClick.AddListener(() => ShowHeroHall());
        
        // Battle button
        CreateButton("Battle Button", "BATTLE", new Color(1f, 0.2f, 0.2f, 1f), new Vector2(0.7f, 0.05f), new Vector2(0.95f, 0.2f));
        
        // Logout button
        CreateButton("Logout Button", "LOGOUT", new Color(0.7f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.15f)).onClick.AddListener(async () => {
            await supabaseClient.SignOut();
            ShowAuthScreen();
        });
        
        Debug.Log("✅ Working main menu created");
    }
    
    private async void ShowCollection()
    {
        currentScreen = Screen.Collection;
        ClearScreen();
        
        Debug.Log("📚 Creating WORKING collection...");
        
        currentScreenObject = CreatePanel("Collection Screen", gameCanvas.transform);
        Image bg = currentScreenObject.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.15f, 0.08f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        // Header
        CreateTitle("CARD COLLECTION");
        
        // Loading message
        TextMeshProUGUI loadingText = CreateStatusText("Loading your cards from database...");
        
        // Back button
        CreateButton("Back Button", "← BACK", new Color(0.3f, 0.6f, 0.9f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.12f)).onClick.AddListener(() => ShowMainMenu());
        
        // Load real data
        await LoadCollectionData(loadingText);
        
        Debug.Log("✅ Working collection created");
    }
    
    private async Task LoadCollectionData(TextMeshProUGUI loadingText)
    {
        try
        {
            Debug.Log("🔄 Loading REAL collection data...");
            
            if (RealSupabaseClient.Instance == null)
            {
                Debug.LogError("❌ RealSupabaseClient not found!");
                if (loadingText) loadingText.text = "Error: Supabase not connected";
                return;
            }
            
            // Load all cards
            allCards = await RealSupabaseClient.Instance.LoadAllCards();
            Debug.Log($"✅ Loaded {allCards.Count} total cards");
            
            // Load user collection
            userCollection = await RealSupabaseClient.Instance.LoadUserCollection();
            Debug.Log($"✅ Loaded user collection: {userCollection.Count} unique cards");
            
            // Update UI
            if (loadingText)
            {
                int totalCards = userCollection.Sum(item => item.quantity);
                int uniqueCards = userCollection.Count(item => item.quantity > 0);
                float completion = (float)uniqueCards / allCards.Count * 100f;
                
                loadingText.text = $"Collection loaded!\nTotal cards: {totalCards}\nUnique cards: {uniqueCards}\nCompletion: {completion:F1}%";
                loadingText.color = new Color(0.2f, 1f, 0.2f, 1f); // Green for success
            }
            
            // Create card grid
            await CreateRealCardGrid();
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error loading collection: {e.Message}");
            if (loadingText)
            {
                loadingText.text = $"Error loading collection: {e.Message}";
                loadingText.color = Color.red;
            }
        }
    }
    
    private async Task CreateRealCardGrid()
    {
        // Create scroll view for cards
        GameObject scrollView = CreatePanel("Card Scroll View", currentScreenObject.transform);
        scrollView.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
        
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.05f, 0.2f);
        scrollRect.anchorMax = new Vector2(0.95f, 0.7f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        
        ScrollRect scrollComponent = scrollView.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        viewport.layer = 5;
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<RectMask2D>();
        
        SetFullScreen(viewport.GetComponent<RectTransform>());
        scrollComponent.viewport = viewport.GetComponent<RectTransform>();
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        content.layer = 5;
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        
        scrollComponent.content = contentRect;
        
        // Grid layout
        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(140, 180);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 6;
        
        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create real card displays
        int cardsDisplayed = 0;
        foreach (var collectionItem in userCollection.Take(30)) // Limit for performance
        {
            if (collectionItem.quantity > 0)
            {
                GameObject cardObj = CreateRealCard(collectionItem, content.transform);
                if (cardObj != null) cardsDisplayed++;
            }
        }
        
        Debug.Log($"✅ Created {cardsDisplayed} real card displays");
        
        await Task.Yield();
    }
    
    private async void ShowDeckBuilder()
    {
        currentScreen = Screen.DeckBuilder;
        ClearScreen();
        
        Debug.Log("🔧 Creating WORKING deck builder...");
        
        currentScreenObject = CreatePanel("Deck Builder Screen", gameCanvas.transform);
        Image bg = currentScreenObject.GetComponent<Image>();
        bg.color = new Color(0.1f, 0.05f, 0.15f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        // Header
        CreateTitle("DECK BUILDER");
        
        // Deck info
        TextMeshProUGUI deckInfo = CreateStatusText($"Current deck: {currentDeckName} ({currentDeckCards.Values.Sum()}/30 cards)");
        
        // Loading message
        TextMeshProUGUI loadingText = CreateStatusText("Loading your cards for deck building...");
        loadingText.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.5f);
        loadingText.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 0.6f);
        
        // Back button
        CreateButton("Back Button", "← BACK", new Color(0.3f, 0.6f, 0.9f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.12f)).onClick.AddListener(() => ShowMainMenu());
        
        // Save deck button
        Button saveDeckButton = CreateButton("Save Deck Button", "SAVE DECK", new Color(0.2f, 0.7f, 0.3f, 1f), new Vector2(0.7f, 0.05f), new Vector2(0.95f, 0.12f));
        saveDeckButton.onClick.AddListener(async () => await SaveCurrentDeck());
        
        // Load real data
        await LoadDeckBuilderData(loadingText, deckInfo);
        
        Debug.Log("✅ Working deck builder created");
    }
    
    private async Task LoadDeckBuilderData(TextMeshProUGUI loadingText, TextMeshProUGUI deckInfo)
    {
        try
        {
            Debug.Log("🔄 Loading REAL deck builder data...");
            
            if (RealSupabaseClient.Instance == null)
            {
                Debug.LogError("❌ RealSupabaseClient not found!");
                if (loadingText) loadingText.text = "Error: Supabase not connected";
                return;
            }
            
            // Load user collection
            userCollection = await RealSupabaseClient.Instance.LoadUserCollection();
            Debug.Log($"✅ Loaded collection: {userCollection.Count} unique cards");
            
            // Update loading text
            if (loadingText)
            {
                int ownedCards = userCollection.Count(item => item.quantity > 0);
                loadingText.text = $"Collection loaded: {ownedCards} cards available for deck building";
                loadingText.color = new Color(0.2f, 1f, 0.2f, 1f);
            }
            
            // Create deck building interface
            await CreateDeckBuildingInterface();
            
            // Update deck info
            UpdateDeckInfo(deckInfo);
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error loading deck builder data: {e.Message}");
            if (loadingText)
            {
                loadingText.text = $"Error: {e.Message}";
                loadingText.color = Color.red;
            }
        }
    }
    
    private async Task CreateDeckBuildingInterface()
    {
        // Available cards section
        GameObject availableSection = CreatePanel("Available Cards", currentScreenObject.transform);
        availableSection.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.05f, 0.8f);
        
        RectTransform availableRect = availableSection.GetComponent<RectTransform>();
        availableRect.anchorMin = new Vector2(0.02f, 0.2f);
        availableRect.anchorMax = new Vector2(0.48f, 0.8f);
        availableRect.offsetMin = Vector2.zero;
        availableRect.offsetMax = Vector2.zero;
        
        // Available cards title
        GameObject availableTitleObj = new GameObject("Available Title");
        availableTitleObj.transform.SetParent(availableSection.transform, false);
        availableTitleObj.layer = 5;
        
        TextMeshProUGUI availableTitle = availableTitleObj.AddComponent<TextMeshProUGUI>();
        availableTitle.text = "YOUR COLLECTION";
        availableTitle.fontSize = 18;
        availableTitle.color = new Color(0.8f, 1f, 0.8f, 1f);
        availableTitle.alignment = TextAlignmentOptions.Center;
        availableTitle.fontStyle = FontStyles.Bold;
        
        RectTransform availableTitleRect = availableTitleObj.GetComponent<RectTransform>();
        availableTitleRect.anchorMin = new Vector2(0.05f, 0.9f);
        availableTitleRect.anchorMax = new Vector2(0.95f, 1f);
        availableTitleRect.offsetMin = Vector2.zero;
        availableTitleRect.offsetMax = Vector2.zero;
        
        // Available cards grid
        await CreateAvailableCardsGrid(availableSection.transform);
        
        // Current deck section
        GameObject deckSection = CreatePanel("Current Deck", currentScreenObject.transform);
        deckSection.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.1f, 0.8f);
        
        RectTransform deckRect = deckSection.GetComponent<RectTransform>();
        deckRect.anchorMin = new Vector2(0.52f, 0.2f);
        deckRect.anchorMax = new Vector2(0.98f, 0.8f);
        deckRect.offsetMin = Vector2.zero;
        deckRect.offsetMax = Vector2.zero;
        
        // Current deck title
        GameObject deckTitleObj = new GameObject("Deck Title");
        deckTitleObj.transform.SetParent(deckSection.transform, false);
        deckTitleObj.layer = 5;
        
        TextMeshProUGUI deckTitle = deckTitleObj.AddComponent<TextMeshProUGUI>();
        deckTitle.text = "CURRENT DECK";
        deckTitle.fontSize = 18;
        deckTitle.color = new Color(1f, 0.8f, 1f, 1f);
        deckTitle.alignment = TextAlignmentOptions.Center;
        deckTitle.fontStyle = FontStyles.Bold;
        
        RectTransform deckTitleRect = deckTitleObj.GetComponent<RectTransform>();
        deckTitleRect.anchorMin = new Vector2(0.05f, 0.9f);
        deckTitleRect.anchorMax = new Vector2(0.95f, 1f);
        deckTitleRect.offsetMin = Vector2.zero;
        deckTitleRect.offsetMax = Vector2.zero;
        
        // Current deck grid
        CreateCurrentDeckGrid(deckSection.transform);
        
        await Task.Yield();
    }
    
    private async Task CreateAvailableCardsGrid(Transform parent)
    {
        // Scroll view for available cards
        GameObject scrollView = CreatePanel("Available Cards Scroll", parent);
        scrollView.GetComponent<Image>().color = Color.clear;
        
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.05f, 0.05f);
        scrollRect.anchorMax = new Vector2(0.95f, 0.88f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        
        ScrollRect scrollComponent = scrollView.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        viewport.layer = 5;
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<RectMask2D>();
        
        SetFullScreen(viewport.GetComponent<RectTransform>());
        scrollComponent.viewport = viewport.GetComponent<RectTransform>();
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        content.layer = 5;
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        
        scrollComponent.content = contentRect;
        
        // Grid layout
        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(100, 140);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;
        
        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create available card displays
        int cardsCreated = 0;
        foreach (var collectionItem in userCollection.Take(20)) // Limit for performance
        {
            if (collectionItem.quantity > 0)
            {
                GameObject cardObj = CreateDeckBuilderCard(collectionItem, content.transform, true);
                if (cardObj != null) cardsCreated++;
            }
        }
        
        Debug.Log($"✅ Created {cardsCreated} available cards for deck building");
        
        await Task.Yield();
    }
    
    private void CreateCurrentDeckGrid(Transform parent)
    {
        // Current deck display area
        GameObject deckArea = CreatePanel("Current Deck Area", parent);
        deckArea.GetComponent<Image>().color = Color.clear;
        
        RectTransform deckRect = deckArea.GetComponent<RectTransform>();
        deckRect.anchorMin = new Vector2(0.05f, 0.05f);
        deckRect.anchorMax = new Vector2(0.95f, 0.88f);
        deckRect.offsetMin = Vector2.zero;
        deckRect.offsetMax = Vector2.zero;
        
        // Grid layout for deck cards
        GridLayoutGroup deckGrid = deckArea.AddComponent<GridLayoutGroup>();
        deckGrid.cellSize = new Vector2(80, 110);
        deckGrid.spacing = new Vector2(5, 5);
        deckGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        deckGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        deckGrid.childAlignment = TextAnchor.UpperCenter;
        deckGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        deckGrid.constraintCount = 4;
        
        // Show current deck cards
        UpdateCurrentDeckDisplay(deckArea.transform);
    }
    
    private void UpdateCurrentDeckDisplay(Transform deckParent)
    {
        // Clear existing deck displays
        for (int i = deckParent.childCount - 1; i >= 0; i--)
        {
            if (deckParent.GetChild(i).name.StartsWith("DeckCard_"))
            {
                DestroyImmediate(deckParent.GetChild(i).gameObject);
            }
        }
        
        // Create displays for cards in deck
        foreach (var kvp in currentDeckCards)
        {
            string cardId = kvp.Key;
            int quantity = kvp.Value;
            
            var collectionItem = userCollection.FirstOrDefault(item => item.card.id == cardId);
            if (collectionItem != null)
            {
                for (int i = 0; i < quantity; i++)
                {
                    CreateDeckBuilderCard(collectionItem, deckParent, false);
                }
            }
        }
    }
    
    private GameObject CreateDeckBuilderCard(RealSupabaseClient.CollectionItem collectionItem, Transform parent, bool isAvailable)
    {
        var card = collectionItem.card;
        
        GameObject cardObj = new GameObject($"{(isAvailable ? "Available" : "DeckCard")}_{card.name}");
        cardObj.transform.SetParent(parent, false);
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
        
        // Attack/Health for units
        if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
        if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), new Color(0.3f, 1f, 0.3f, 1f), new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
        
        // Quantity badge
        if (isAvailable && collectionItem.quantity > 1)
        {
            CreateQuantityBadge(cardObj.transform, collectionItem.quantity);
        }
        
        // Click handler
        Button cardButton = cardObj.AddComponent<Button>();
        
        if (isAvailable)
        {
            // Add to deck
            cardButton.onClick.AddListener(() => {
                AddCardToDeck(card);
            });
        }
        else
        {
            // Remove from deck
            cardButton.onClick.AddListener(() => {
                RemoveCardFromDeck(card.id);
            });
        }
        
        // Professional button colors
        ColorBlock colors = cardButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cardButton.colors = colors;
        
        return cardObj;
    }
    
    private void AddCardToDeck(RealSupabaseClient.EnhancedCard card)
    {
        // Check deck size
        int totalCards = currentDeckCards.Values.Sum();
        if (totalCards >= 30)
        {
            Debug.Log("❌ Deck is full (30/30 cards)");
            return;
        }
        
        // Check copy limits
        int currentInDeck = currentDeckCards.ContainsKey(card.id) ? currentDeckCards[card.id] : 0;
        int maxCopies = GetMaxCopiesByRarity(card.rarity);
        
        if (currentInDeck >= maxCopies)
        {
            Debug.Log($"❌ Cannot add more {card.name} - max {maxCopies} allowed");
            return;
        }
        
        // Add to deck
        currentDeckCards[card.id] = currentInDeck + 1;
        
        Debug.Log($"✅ Added {card.name} to deck ({currentInDeck + 1}/{maxCopies})");
        
        // Refresh deck display
        RefreshDeckBuilder();
    }
    
    private void RemoveCardFromDeck(string cardId)
    {
        if (!currentDeckCards.ContainsKey(cardId)) return;
        
        int currentInDeck = currentDeckCards[cardId];
        
        if (currentInDeck <= 1)
        {
            currentDeckCards.Remove(cardId);
        }
        else
        {
            currentDeckCards[cardId] = currentInDeck - 1;
        }
        
        Debug.Log($"✅ Removed card from deck");
        
        // Refresh deck display
        RefreshDeckBuilder();
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
    
    private void RefreshDeckBuilder()
    {
        // Find deck info text and update it
        var deckInfoText = currentScreenObject.GetComponentInChildren<TextMeshProUGUI>();
        UpdateDeckInfo(deckInfoText);
        
        // Refresh current deck display
        var deckArea = currentScreenObject.transform.Find("Current Deck")?.Find("Current Deck Area");
        if (deckArea != null)
        {
            UpdateCurrentDeckDisplay(deckArea);
        }
    }
    
    private void UpdateDeckInfo(TextMeshProUGUI deckInfo)
    {
        if (deckInfo == null) return;
        
        int totalCards = currentDeckCards.Values.Sum();
        bool isValid = totalCards == 30;
        
        deckInfo.text = $"Deck: {currentDeckName} ({totalCards}/30) {(isValid ? "✅ Valid" : "❌ Invalid")}";
        deckInfo.color = isValid ? new Color(0.2f, 1f, 0.2f, 1f) : Color.white;
    }
    
    private async Task SaveCurrentDeck()
    {
        int totalCards = currentDeckCards.Values.Sum();
        if (totalCards != 30)
        {
            Debug.Log("❌ Cannot save - deck must have exactly 30 cards");
            return;
        }
        
        try
        {
            Debug.Log($"💾 Saving deck: {currentDeckName}");
            
            // Create deck object
            var deck = new RealSupabaseClient.Deck
            {
                id = System.Guid.NewGuid().ToString(),
                user_id = RealSupabaseClient.Instance?.UserId ?? "",
                name = currentDeckName,
                is_active = true, // Make this deck active
                cards = new List<RealSupabaseClient.DeckCard>(),
                total_cards = totalCards,
                created_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                updated_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            
            // Add deck cards
            foreach (var kvp in currentDeckCards)
            {
                var card = userCollection.FirstOrDefault(item => item.card.id == kvp.Key)?.card;
                if (card != null)
                {
                    deck.cards.Add(new RealSupabaseClient.DeckCard
                    {
                        deck_id = deck.id,
                        card_id = kvp.Key,
                        quantity = kvp.Value,
                        card = card
                    });
                }
            }
            
            // Save to database
            bool success = await RealSupabaseClient.Instance.SaveDeck(deck);
            
            if (success)
            {
                Debug.Log($"✅ Deck '{currentDeckName}' saved successfully!");
                
                // Reset for new deck
                currentDeckCards.Clear();
                currentDeckName = "New Deck";
                
                // Refresh display
                RefreshDeckBuilder();
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
    
    private async void ShowHeroHall()
    {
        currentScreen = Screen.HeroHall;
        ClearScreen();
        
        Debug.Log("🦸 Creating WORKING hero hall...");
        
        currentScreenObject = CreatePanel("Hero Hall Screen", gameCanvas.transform);
        Image bg = currentScreenObject.GetComponent<Image>();
        bg.color = new Color(0.15f, 0.1f, 0.05f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        // Header
        CreateTitle("HERO HALL");
        
        // Loading message
        TextMeshProUGUI loadingText = CreateStatusText("Loading heroes from database...");
        
        // Back button
        CreateButton("Back Button", "← BACK", new Color(0.3f, 0.6f, 0.9f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.12f)).onClick.AddListener(() => ShowMainMenu());
        
        // Load real hero data
        await LoadHeroData(loadingText);
        
        Debug.Log("✅ Working hero hall created");
    }
    
    private async Task LoadHeroData(TextMeshProUGUI loadingText)
    {
        try
        {
            Debug.Log("🔄 Loading REAL hero data...");
            
            if (RealSupabaseClient.Instance == null)
            {
                Debug.LogError("❌ RealSupabaseClient not found!");
                if (loadingText) loadingText.text = "Error: Supabase not connected";
                return;
            }
            
            // Load heroes
            availableHeroes = await RealSupabaseClient.Instance.LoadAvailableHeroes();
            userHeroes = await RealSupabaseClient.Instance.LoadUserHeroes();
            
            Debug.Log($"✅ Loaded {availableHeroes.Count} available heroes, {userHeroes.Count} user heroes");
            
            // Update loading text
            if (loadingText)
            {
                var activeHero = userHeroes.FirstOrDefault(uh => uh.is_active);
                string activeHeroName = activeHero?.hero?.name ?? "None";
                
                loadingText.text = $"Heroes loaded!\nAvailable: {availableHeroes.Count}\nOwned: {userHeroes.Count}\nActive: {activeHeroName}";
                loadingText.color = new Color(0.2f, 1f, 0.2f, 1f);
            }
            
            // Create hero grid
            await CreateRealHeroGrid();
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error loading heroes: {e.Message}");
            if (loadingText)
            {
                loadingText.text = $"Error loading heroes: {e.Message}";
                loadingText.color = Color.red;
            }
        }
    }
    
    private async Task CreateRealHeroGrid()
    {
        // Create hero grid
        GameObject heroGrid = CreatePanel("Hero Grid", currentScreenObject.transform);
        heroGrid.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
        
        RectTransform heroRect = heroGrid.GetComponent<RectTransform>();
        heroRect.anchorMin = new Vector2(0.1f, 0.25f);
        heroRect.anchorMax = new Vector2(0.9f, 0.7f);
        heroRect.offsetMin = Vector2.zero;
        heroRect.offsetMax = Vector2.zero;
        
        // Grid layout
        GridLayoutGroup gridLayout = heroGrid.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(180, 240);
        gridLayout.spacing = new Vector2(15, 15);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4;
        
        // Create hero displays
        foreach (var hero in availableHeroes)
        {
            CreateRealHeroDisplay(hero, heroGrid.transform);
        }
        
        Debug.Log($"✅ Created {availableHeroes.Count} hero displays");
        
        await Task.Yield();
    }
    
    private void CreateRealHeroDisplay(RealSupabaseClient.Hero hero, Transform parent)
    {
        GameObject heroObj = new GameObject($"Hero_{hero.name}");
        heroObj.transform.SetParent(parent, false);
        heroObj.layer = 5;
        
        // Hero background
        Image heroImage = heroObj.AddComponent<Image>();
        heroImage.color = GetHeroElementColor(hero.element_type);
        
        // Hero name
        CreateCardName(heroObj.transform, hero.name);
        
        // Hero stats
        CreateCardStat(heroObj.transform, hero.base_hp.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.65f), new Vector2(0.45f, 0.8f));
        CreateCardStat(heroObj.transform, hero.base_mana.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.55f, 0.65f), new Vector2(0.95f, 0.8f));
        
        // Hero power
        GameObject powerObj = new GameObject("Hero Power");
        powerObj.transform.SetParent(heroObj.transform, false);
        powerObj.layer = 5;
        
        TextMeshProUGUI powerText = powerObj.AddComponent<TextMeshProUGUI>();
        powerText.text = hero.hero_power_name ?? "No Power";
        powerText.fontSize = 10;
        powerText.color = new Color(0.8f, 0.8f, 1f, 1f);
        powerText.alignment = TextAlignmentOptions.Center;
        
        RectTransform powerRect = powerObj.GetComponent<RectTransform>();
        powerRect.anchorMin = new Vector2(0.05f, 0.45f);
        powerRect.anchorMax = new Vector2(0.95f, 0.6f);
        powerRect.offsetMin = Vector2.zero;
        powerRect.offsetMax = Vector2.zero;
        
        // Ownership status
        bool isOwned = userHeroes.Any(uh => uh.hero_id == hero.id);
        bool isActive = userHeroes.Any(uh => uh.hero_id == hero.id && uh.is_active);
        
        GameObject statusObj = CreatePanel("Status", heroObj.transform);
        Image statusBg = statusObj.GetComponent<Image>();
        statusBg.color = isActive ? new Color(1f, 0.8f, 0.2f, 0.9f) : 
                        isOwned ? new Color(0.2f, 1f, 0.2f, 0.9f) : 
                        new Color(0.5f, 0.5f, 0.5f, 0.9f);
        
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.1f, 0.05f);
        statusRect.anchorMax = new Vector2(0.9f, 0.2f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
        
        GameObject statusTextObj = new GameObject("Status Text");
        statusTextObj.transform.SetParent(statusObj.transform, false);
        statusTextObj.layer = 5;
        
        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.text = isActive ? "⭐ ACTIVE" : isOwned ? "✅ OWNED" : "🔒 LOCKED";
        statusText.fontSize = 10;
        statusText.color = Color.white;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontStyle = FontStyles.Bold;
        
        SetFullScreen(statusTextObj.GetComponent<RectTransform>());
        
        // Click handler for hero selection
        if (isOwned)
        {
            Button heroButton = heroObj.AddComponent<Button>();
            heroButton.onClick.AddListener(async () => {
                await SelectHero(hero);
            });
            
            ColorBlock colors = heroButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            heroButton.colors = colors;
        }
    }
    
    private async Task SelectHero(RealSupabaseClient.Hero hero)
    {
        try
        {
            Debug.Log($"⭐ Selecting hero: {hero.name}");
            
            // Update user heroes (set all to inactive, then set selected to active)
            foreach (var userHero in userHeroes)
            {
                userHero.is_active = userHero.hero_id == hero.id;
            }
            
            Debug.Log($"✅ {hero.name} is now your active hero!");
            
            // Refresh hero hall display
            ShowHeroHall();
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error selecting hero: {e.Message}");
        }
    }
    
    private GameObject CreateRealCard(RealSupabaseClient.CollectionItem collectionItem, Transform parent)
    {
        var card = collectionItem.card;
        
        GameObject cardObj = new GameObject($"Card_{card.name}");
        cardObj.transform.SetParent(parent, false);
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
        CreateCardName(cardObj.transform, card.name);
        
        // Mana cost
        CreateCardStat(cardObj.transform, card.mana_cost.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
        
        // Attack/Health (for units)
        if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
        if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), new Color(0.3f, 1f, 0.3f, 1f), new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
        
        // Quantity badge
        if (collectionItem.quantity > 1)
        {
            CreateQuantityBadge(cardObj.transform, collectionItem.quantity);
        }
        
        // Rarity indicator
        CreateRarityIndicator(cardObj.transform, card.rarity);
        
        return cardObj;
    }
    
    #region Helper Methods
    
    private async Task HandleLogin()
    {
        string email = emailField.text.Trim();
        string password = passwordField.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateStatus("Please enter email and password", Color.red);
            return;
        }
        
        UpdateStatus("Signing in...", Color.yellow);
        
        try
        {
            bool success = await supabaseClient.SignIn(email, password);
            
            if (success)
            {
                UpdateStatus("Login successful!", Color.green);
                await Task.Delay(1000);
                ShowMainMenu();
            }
            else
            {
                UpdateStatus("Invalid email or password", Color.red);
            }
        }
        catch (System.Exception e)
        {
            UpdateStatus($"Login error: {e.Message}", Color.red);
        }
    }
    
    private void OnAuthChanged(bool isAuthenticated)
    {
        if (isAuthenticated && currentScreen == Screen.Auth)
        {
            ShowMainMenu();
        }
        else if (!isAuthenticated && currentScreen != Screen.Auth)
        {
            ShowAuthScreen();
        }
    }
    
    private void UpdateStatus(string message, Color color)
    {
        if (statusText)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"📱 Status: {message}");
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
    
    private Color GetHeroElementColor(string elementType)
    {
        return elementType?.ToLower() switch
        {
            "fire" => new Color(1f, 0.3f, 0.2f, 0.9f),
            "ice" => new Color(0.2f, 0.6f, 1f, 0.9f),
            "lightning" => new Color(1f, 1f, 0.2f, 0.9f),
            "light" => new Color(1f, 0.9f, 0.7f, 0.9f),
            "void" => new Color(0.4f, 0.2f, 0.6f, 0.9f),
            _ => new Color(0.5f, 0.5f, 0.5f, 0.9f)
        };
    }
    
    private void CreateTitle(string titleText)
    {
        GameObject titleObj = new GameObject("Screen Title");
        titleObj.transform.SetParent(currentScreenObject.transform, false);
        titleObj.layer = 5;
        
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = titleText;
        title.fontSize = 32;
        title.color = new Color(1f, 0.8f, 0.2f, 1f);
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    private TMP_InputField CreateInputField(string name, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject fieldObj = CreatePanel(name, currentScreenObject.transform);
        fieldObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
        
        RectTransform fieldRect = fieldObj.GetComponent<RectTransform>();
        fieldRect.anchorMin = anchorMin;
        fieldRect.anchorMax = anchorMax;
        fieldRect.offsetMin = Vector2.zero;
        fieldRect.offsetMax = Vector2.zero;
        
        TMP_InputField inputField = fieldObj.AddComponent<TMP_InputField>();
        
        // Input text
        GameObject inputTextObj = new GameObject("Input Text");
        inputTextObj.transform.SetParent(fieldObj.transform, false);
        inputTextObj.layer = 5;
        
        TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.text = "";
        inputText.fontSize = 18;
        inputText.color = Color.black;
        inputText.alignment = TextAlignmentOptions.Left;
        
        RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = new Vector2(0.05f, 0f);
        inputTextRect.anchorMax = new Vector2(0.95f, 1f);
        inputTextRect.offsetMin = Vector2.zero;
        inputTextRect.offsetMax = Vector2.zero;
        
        // Placeholder
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(fieldObj.transform, false);
        placeholderObj.layer = 5;
        
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        placeholderText.raycastTarget = false;
        
        SetFullScreen(placeholderObj.GetComponent<RectTransform>());
        
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        
        return inputField;
    }
    
    private Button CreateButton(string name, string text, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject btnObj = CreatePanel(name, currentScreenObject.transform);
        Button button = btnObj.AddComponent<Button>();
        Image btnImage = btnObj.GetComponent<Image>();
        btnImage.color = color;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = anchorMin;
        btnRect.anchorMax = anchorMax;
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        
        // Button text
        GameObject textObj = new GameObject("Button Text");
        textObj.transform.SetParent(btnObj.transform, false);
        textObj.layer = 5;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;
        
        SetFullScreen(textObj.GetComponent<RectTransform>());
        
        return button;
    }
    
    private TextMeshProUGUI CreateStatusText(string text)
    {
        GameObject statusObj = new GameObject("Status Text");
        statusObj.transform.SetParent(currentScreenObject.transform, false);
        statusObj.layer = 5;
        
        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = text;
        statusText.fontSize = 18;
        statusText.color = Color.white;
        statusText.alignment = TextAlignmentOptions.Center;
        
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.1f, 0.1f);
        statusRect.anchorMax = new Vector2(0.9f, 0.2f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
        
        return statusText;
    }
    
    private void CreateCardName(Transform parent, string name)
    {
        GameObject nameObj = new GameObject("Card Name");
        nameObj.transform.SetParent(parent, false);
        nameObj.layer = 5;
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = name;
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
    
    private void CreateRarityIndicator(Transform parent, string rarity)
    {
        GameObject rarityObj = CreatePanel("Rarity Indicator", parent);
        Image rarityBg = rarityObj.GetComponent<Image>();
        
        Color rarityColor = rarity?.ToLower() switch
        {
            "common" => Color.white,
            "uncommon" => Color.green,
            "rare" => Color.blue,
            "legendary" => Color.yellow,
            "exotic" => new Color(1f, 0.2f, 1f, 1f),
            _ => Color.white
        };
        
        rarityBg.color = rarityColor;
        
        RectTransform rarityRect = rarityObj.GetComponent<RectTransform>();
        rarityRect.anchorMin = new Vector2(0.75f, 0.8f);
        rarityRect.anchorMax = new Vector2(0.95f, 0.95f);
        rarityRect.offsetMin = Vector2.zero;
        rarityRect.offsetMax = Vector2.zero;
    }
    
    private void ClearScreen()
    {
        if (currentScreenObject != null)
        {
            Destroy(currentScreenObject);
            currentScreenObject = null;
        }
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
    
    private void OnDestroy()
    {
        if (supabaseClient != null)
        {
            supabaseClient.OnAuthenticationChanged -= OnAuthChanged;
        }
    }
}