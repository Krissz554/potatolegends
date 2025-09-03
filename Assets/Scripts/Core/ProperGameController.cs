using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using PotatoCardGame.Network;

/// <summary>
/// PROPER Game Controller that starts with Auth screen and manages flow correctly
/// This replaces the complex system and works exactly like your web version
/// </summary>
public class ProperGameController : MonoBehaviour
{
    [Header("🥔 Potato Legends Mobile Game")]
    [SerializeField] private bool debugMode = true;
    
    // Game state
    private enum GameState
    {
        Loading,
        Authentication,
        MainMenu,
        Collection,
        DeckBuilder,
        HeroHall,
        Battle
    }
    
    private GameState currentState = GameState.Loading;
    private RealSupabaseClient supabaseClient;
    
    // UI References
    private Canvas gameCanvas;
    private GameObject currentScreen;
    
    private void Start()
    {
        Debug.Log("🥔 Starting Potato Legends Mobile Game...");
        InitializeGame();
    }
    
    private async void InitializeGame()
    {
        // Create game canvas
        CreateGameCanvas();
        
        // Create Supabase client
        CreateSupabaseClient();
        
        // Wait a moment for initialization
        await Task.Delay(100);
        
        // Start with authentication screen
        ShowAuthenticationScreen();
        
        Debug.Log("✅ Game initialized - showing login screen");
    }
    
    private void CreateGameCanvas()
    {
        GameObject canvasObj = new GameObject("Game Canvas");
        gameCanvas = canvasObj.AddComponent<Canvas>();
        gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("✅ Created EventSystem with NEW Input System");
        }
    }
    
    private void CreateSupabaseClient()
    {
        GameObject supabaseObj = new GameObject("RealSupabaseClient");
        supabaseObj.transform.SetParent(transform);
        supabaseClient = supabaseObj.AddComponent<RealSupabaseClient>();
        
        // Subscribe to auth events
        supabaseClient.OnAuthenticationChanged += OnAuthenticationChanged;
        
        Debug.Log("✅ Supabase client created");
    }
    
    private void ShowAuthenticationScreen()
    {
        currentState = GameState.Authentication;
        
        // Clear any existing screen
        ClearCurrentScreen();
        
        // Create auth screen
        currentScreen = CreateAuthScreen();
        
        Debug.Log("🔐 Showing authentication screen");
    }
    
    private GameObject CreateAuthScreen()
    {
        // Background
        GameObject authScreen = CreatePanel("Auth Screen", gameCanvas.transform);
        Image bg = authScreen.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.1f, 0.2f, 1f); // Dark blue
        SetFullScreen(authScreen.GetComponent<RectTransform>());
        
        // Title
        GameObject titleObj = CreateText("Game Title", authScreen.transform);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "POTATO LEGENDS\nMOBILE CARD GAME";
        titleText.fontSize = 40;
        titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.7f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Email field
        TMP_InputField emailField = CreateInputField("Email Field", authScreen.transform, "Enter your email...");
        RectTransform emailRect = emailField.GetComponent<RectTransform>();
        emailRect.anchorMin = new Vector2(0.1f, 0.55f);
        emailRect.anchorMax = new Vector2(0.9f, 0.65f);
        emailRect.offsetMin = Vector2.zero;
        emailRect.offsetMax = Vector2.zero;
        
        // Password field
        TMP_InputField passwordField = CreateInputField("Password Field", authScreen.transform, "Enter your password...");
        passwordField.inputType = TMP_InputField.InputType.Password;
        RectTransform passwordRect = passwordField.GetComponent<RectTransform>();
        passwordRect.anchorMin = new Vector2(0.1f, 0.42f);
        passwordRect.anchorMax = new Vector2(0.9f, 0.52f);
        passwordRect.offsetMin = Vector2.zero;
        passwordRect.offsetMax = Vector2.zero;
        
        // Login button
        Button loginButton = CreateButton("Login Button", authScreen.transform, "LOGIN");
        loginButton.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 1f);
        RectTransform loginRect = loginButton.GetComponent<RectTransform>();
        loginRect.anchorMin = new Vector2(0.1f, 0.25f);
        loginRect.anchorMax = new Vector2(0.9f, 0.35f);
        loginRect.offsetMin = Vector2.zero;
        loginRect.offsetMax = Vector2.zero;
        
        // Status text
        GameObject statusObj = CreateText("Status Text", authScreen.transform);
        TextMeshProUGUI statusText = statusObj.GetComponent<TextMeshProUGUI>();
        statusText.text = "Enter your credentials to play!";
        statusText.fontSize = 18;
        statusText.color = Color.white;
        statusText.alignment = TextAlignmentOptions.Center;
        
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.1f, 0.1f);
        statusRect.anchorMax = new Vector2(0.9f, 0.2f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
        
        // Add login functionality
        loginButton.onClick.AddListener(async () => {
            string email = emailField.text.Trim();
            string password = passwordField.text;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                statusText.text = "Please enter email and password";
                statusText.color = Color.red;
                return;
            }
            
            statusText.text = "Signing in...";
            statusText.color = Color.yellow;
            
            bool success = await supabaseClient.SignIn(email, password);
            
            if (success)
            {
                statusText.text = "Login successful!";
                statusText.color = Color.green;
                ShowMainMenu();
            }
            else
            {
                statusText.text = "Invalid email or password";
                statusText.color = Color.red;
            }
        });
        
        return authScreen;
    }
    
    private void ShowMainMenu()
    {
        currentState = GameState.MainMenu;
        
        // Clear current screen
        ClearCurrentScreen();
        
        // Create main menu
        currentScreen = CreateMainMenuScreen();
        
        Debug.Log("🏠 Showing main menu");
    }
    
    private GameObject CreateMainMenuScreen()
    {
        // Background
        GameObject mainScreen = CreatePanel("Main Menu Screen", gameCanvas.transform);
        Image bg = mainScreen.GetComponent<Image>();
        bg.color = new Color(0.1f, 0.15f, 0.3f, 1f); // Dark blue-purple
        SetFullScreen(mainScreen.GetComponent<RectTransform>());
        
        // Welcome message
        GameObject welcomeObj = CreateText("Welcome Text", mainScreen.transform);
        TextMeshProUGUI welcomeText = welcomeObj.GetComponent<TextMeshProUGUI>();
        welcomeText.text = "POTATO LEGENDS\nWelcome to the arena!";
        welcomeText.fontSize = 32;
        welcomeText.color = new Color(1f, 0.9f, 0.7f, 1f);
        welcomeText.alignment = TextAlignmentOptions.Center;
        welcomeText.fontStyle = FontStyles.Bold;
        
        RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
        welcomeRect.anchorMin = new Vector2(0.1f, 0.7f);
        welcomeRect.anchorMax = new Vector2(0.9f, 0.9f);
        welcomeRect.offsetMin = Vector2.zero;
        welcomeRect.offsetMax = Vector2.zero;
        
        // Top navigation buttons
        CreateTopNavigation(mainScreen.transform);
        
        // BATTLE button (bottom right)
        Button battleButton = CreateButton("Battle Button", mainScreen.transform, "BATTLE");
        battleButton.GetComponent<Image>().color = new Color(1f, 0.2f, 0.2f, 1f);
        RectTransform battleRect = battleButton.GetComponent<RectTransform>();
        battleRect.anchorMin = new Vector2(0.7f, 0.05f);
        battleRect.anchorMax = new Vector2(0.95f, 0.2f);
        battleRect.offsetMin = Vector2.zero;
        battleRect.offsetMax = Vector2.zero;
        
        battleButton.onClick.AddListener(() => {
            Debug.Log("⚔️ Battle button clicked - starting matchmaking...");
            // TODO: Add matchmaking logic
        });
        
        // Logout button
        Button logoutButton = CreateButton("Logout Button", mainScreen.transform, "LOGOUT");
        logoutButton.GetComponent<Image>().color = new Color(0.7f, 0.3f, 0.3f, 1f);
        RectTransform logoutRect = logoutButton.GetComponent<RectTransform>();
        logoutRect.anchorMin = new Vector2(0.05f, 0.05f);
        logoutRect.anchorMax = new Vector2(0.25f, 0.15f);
        logoutRect.offsetMin = Vector2.zero;
        logoutRect.offsetMax = Vector2.zero;
        
        logoutButton.onClick.AddListener(async () => {
            await supabaseClient.SignOut();
            ShowAuthenticationScreen();
        });
        
        return mainScreen;
    }
    
    private void CreateTopNavigation(Transform parent)
    {
        // Navigation panel
        GameObject navPanel = CreatePanel("Navigation Panel", parent);
        Image navBg = navPanel.GetComponent<Image>();
        navBg.color = new Color(0f, 0f, 0f, 0.5f);
        
        RectTransform navRect = navPanel.GetComponent<RectTransform>();
        navRect.anchorMin = new Vector2(0.05f, 0.5f);
        navRect.anchorMax = new Vector2(0.95f, 0.65f);
        navRect.offsetMin = Vector2.zero;
        navRect.offsetMax = Vector2.zero;
        
        // Collection button
        Button collectionBtn = CreateButton("Collection Button", navPanel.transform, "COLLECTION");
        collectionBtn.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 1f);
        RectTransform collectionRect = collectionBtn.GetComponent<RectTransform>();
        collectionRect.anchorMin = new Vector2(0.05f, 0.1f);
        collectionRect.anchorMax = new Vector2(0.3f, 0.9f);
        collectionRect.offsetMin = Vector2.zero;
        collectionRect.offsetMax = Vector2.zero;
        
        collectionBtn.onClick.AddListener(() => {
            Debug.Log("📚 Opening collection...");
            ShowCollection();
        });
        
        // Deck Builder button
        Button deckBtn = CreateButton("Deck Builder Button", navPanel.transform, "DECK BUILDER");
        deckBtn.GetComponent<Image>().color = new Color(0.7f, 0.3f, 0.7f, 1f);
        RectTransform deckRect = deckBtn.GetComponent<RectTransform>();
        deckRect.anchorMin = new Vector2(0.35f, 0.1f);
        deckRect.anchorMax = new Vector2(0.65f, 0.9f);
        deckRect.offsetMin = Vector2.zero;
        deckRect.offsetMax = Vector2.zero;
        
        deckBtn.onClick.AddListener(() => {
            Debug.Log("🔧 Opening deck builder...");
            ShowDeckBuilder();
        });
        
        // Hero Hall button
        Button heroBtn = CreateButton("Hero Hall Button", navPanel.transform, "HERO HALL");
        heroBtn.GetComponent<Image>().color = new Color(0.9f, 0.6f, 0.2f, 1f);
        RectTransform heroRect = heroBtn.GetComponent<RectTransform>();
        heroRect.anchorMin = new Vector2(0.7f, 0.1f);
        heroRect.anchorMax = new Vector2(0.95f, 0.9f);
        heroRect.offsetMin = Vector2.zero;
        heroRect.offsetMax = Vector2.zero;
        
        heroBtn.onClick.AddListener(() => {
            Debug.Log("🦸 Opening hero hall...");
            ShowHeroHall();
        });
    }
    
    private void ShowCollection()
    {
        currentState = GameState.Collection;
        ClearCurrentScreen();
        currentScreen = CreateSimpleCollectionScreen();
    }
    
    private void ShowDeckBuilder()
    {
        currentState = GameState.DeckBuilder;
        ClearCurrentScreen();
        currentScreen = CreateSimpleDeckBuilderScreen();
    }
    
    private void ShowHeroHall()
    {
        currentState = GameState.HeroHall;
        ClearCurrentScreen();
        currentScreen = CreateSimpleHeroHallScreen();
    }
    
    private GameObject CreateSimpleCollectionScreen()
    {
        GameObject screen = CreatePanel("Collection Screen", gameCanvas.transform);
        screen.GetComponent<Image>().color = new Color(0.05f, 0.15f, 0.05f, 1f);
        SetFullScreen(screen.GetComponent<RectTransform>());
        
        // Title
        GameObject titleObj = CreateText("Collection Title", screen.transform);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "CARD COLLECTION";
        titleText.fontSize = 36;
        titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Cards info
        GameObject infoObj = CreateText("Cards Info", screen.transform);
        TextMeshProUGUI infoText = infoObj.GetComponent<TextMeshProUGUI>();
        infoText.text = "Loading your 236+ cards from database...";
        infoText.fontSize = 24;
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Center;
        
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.1f, 0.4f);
        infoRect.anchorMax = new Vector2(0.9f, 0.6f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
        
        // Back button
        Button backBtn = CreateButton("Back Button", screen.transform, "BACK TO MENU");
        backBtn.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.9f, 1f);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.1f, 0.05f);
        backRect.anchorMax = new Vector2(0.4f, 0.15f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        
        backBtn.onClick.AddListener(() => ShowMainMenu());
        
        return screen;
    }
    
    private GameObject CreateSimpleDeckBuilderScreen()
    {
        GameObject screen = CreatePanel("Deck Builder Screen", gameCanvas.transform);
        screen.GetComponent<Image>().color = new Color(0.15f, 0.05f, 0.15f, 1f);
        SetFullScreen(screen.GetComponent<RectTransform>());
        
        // Title
        GameObject titleObj = CreateText("Deck Builder Title", screen.transform);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "DECK BUILDER";
        titleText.fontSize = 36;
        titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Deck info
        GameObject infoObj = CreateText("Deck Info", screen.transform);
        TextMeshProUGUI infoText = infoObj.GetComponent<TextMeshProUGUI>();
        infoText.text = "Build your 30-card deck\nwith copy limits by rarity";
        infoText.fontSize = 24;
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Center;
        
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.1f, 0.4f);
        infoRect.anchorMax = new Vector2(0.9f, 0.6f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
        
        // Back button
        Button backBtn = CreateButton("Back Button", screen.transform, "BACK TO MENU");
        backBtn.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.9f, 1f);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.1f, 0.05f);
        backRect.anchorMax = new Vector2(0.4f, 0.15f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        
        backBtn.onClick.AddListener(() => ShowMainMenu());
        
        return screen;
    }
    
    private GameObject CreateSimpleHeroHallScreen()
    {
        GameObject screen = CreatePanel("Hero Hall Screen", gameCanvas.transform);
        screen.GetComponent<Image>().color = new Color(0.15f, 0.1f, 0.05f, 1f);
        SetFullScreen(screen.GetComponent<RectTransform>());
        
        // Title
        GameObject titleObj = CreateText("Hero Hall Title", screen.transform);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "HERO HALL";
        titleText.fontSize = 36;
        titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Hero info
        GameObject infoObj = CreateText("Hero Info", screen.transform);
        TextMeshProUGUI infoText = infoObj.GetComponent<TextMeshProUGUI>();
        infoText.text = "Select your hero\nwith unique powers and abilities";
        infoText.fontSize = 24;
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Center;
        
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.1f, 0.4f);
        infoRect.anchorMax = new Vector2(0.9f, 0.6f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
        
        // Back button
        Button backBtn = CreateButton("Back Button", screen.transform, "BACK TO MENU");
        backBtn.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.9f, 1f);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.1f, 0.05f);
        backRect.anchorMax = new Vector2(0.4f, 0.15f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        
        backBtn.onClick.AddListener(() => ShowMainMenu());
        
        return screen;
    }
    
    private void OnAuthenticationChanged(bool isAuthenticated)
    {
        Debug.Log($"🔐 Authentication changed: {isAuthenticated}");
        
        if (isAuthenticated && currentState == GameState.Authentication)
        {
            ShowMainMenu();
        }
        else if (!isAuthenticated && currentState != GameState.Authentication)
        {
            ShowAuthenticationScreen();
        }
    }
    
    private void ClearCurrentScreen()
    {
        if (currentScreen != null)
        {
            Destroy(currentScreen);
            currentScreen = null;
        }
    }
    
    #region UI Helper Methods
    
    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.layer = 5;
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        panel.AddComponent<Image>();
        
        return panel;
    }
    
    private GameObject CreateText(string name, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        textObj.layer = 5;
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        
        return textObj;
    }
    
    private TMP_InputField CreateInputField(string name, Transform parent, string placeholder)
    {
        GameObject fieldObj = CreatePanel(name, parent);
        fieldObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
        
        TMP_InputField inputField = fieldObj.AddComponent<TMP_InputField>();
        
        // Text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(fieldObj.transform, false);
        textArea.layer = 5;
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textArea.AddComponent<RectMask2D>();
        
        textAreaRect.anchorMin = new Vector2(0.05f, 0f);
        textAreaRect.anchorMax = new Vector2(0.95f, 1f);
        textAreaRect.offsetMin = Vector2.zero;
        textAreaRect.offsetMax = Vector2.zero;
        
        // Placeholder
        GameObject placeholderObj = CreateText("Placeholder", textArea.transform);
        TextMeshProUGUI placeholderText = placeholderObj.GetComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.raycastTarget = false;
        SetFullScreen(placeholderObj.GetComponent<RectTransform>());
        
        // Input text
        GameObject inputTextObj = CreateText("Input Text", textArea.transform);
        TextMeshProUGUI inputText = inputTextObj.GetComponent<TextMeshProUGUI>();
        inputText.text = "";
        inputText.fontSize = 18;
        inputText.color = Color.black;
        inputText.raycastTarget = false;
        SetFullScreen(inputTextObj.GetComponent<RectTransform>());
        
        // Connect input field
        inputField.textViewport = textAreaRect;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        
        return inputField;
    }
    
    private Button CreateButton(string name, Transform parent, string text)
    {
        GameObject btnObj = CreatePanel(name, parent);
        Button button = btnObj.AddComponent<Button>();
        
        GameObject textObj = CreateText("Button Text", btnObj.transform);
        TextMeshProUGUI buttonText = textObj.GetComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;
        
        SetFullScreen(textObj.GetComponent<RectTransform>());
        
        return button;
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
            supabaseClient.OnAuthenticationChanged -= OnAuthenticationChanged;
        }
    }
    
    [ContextMenu("Test Authentication")]
    public void TestAuth()
    {
        ShowAuthenticationScreen();
    }
    
    [ContextMenu("Test Main Menu")]
    public void TestMainMenu()
    {
        ShowMainMenu();
    }
}