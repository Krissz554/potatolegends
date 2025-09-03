using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections;

/// <summary>
/// PROFESSIONAL Mobile Game UI - Clean, polished, and beautiful like Clash Royale
/// Modern mobile game design with animations, gradients, and professional styling
/// </summary>
public class ProfessionalMobileUI : MonoBehaviour
{
    [Header("🎮 Professional Mobile Game UI")]
    [SerializeField] private bool autoStart = true;
    
    // Game state
    private enum Screen { Auth, MainMenu, Collection, DeckBuilder, HeroHall, Battle }
    private Screen currentScreen = Screen.Auth;
    
    // UI References
    private Canvas gameCanvas;
    private GameObject currentScreenObject;
    private RealSupabaseClient supabaseClient;
    
    // Auth fields
    private TMP_InputField emailField;
    private TMP_InputField passwordField;
    private TextMeshProUGUI statusText;
    private Button loginButton;
    
    // Colors (Professional mobile game palette)
    private readonly Color primaryBlue = new Color(0.2f, 0.4f, 0.8f, 1f);
    private readonly Color primaryGreen = new Color(0.2f, 0.7f, 0.3f, 1f);
    private readonly Color primaryRed = new Color(0.9f, 0.2f, 0.2f, 1f);
    private readonly Color primaryGold = new Color(1f, 0.8f, 0.2f, 1f);
    private readonly Color darkBackground = new Color(0.08f, 0.12f, 0.2f, 1f);
    private readonly Color cardBackground = new Color(0.15f, 0.2f, 0.3f, 0.95f);
    private readonly Color buttonNormal = new Color(0.25f, 0.35f, 0.55f, 1f);
    private readonly Color buttonHighlight = new Color(0.35f, 0.45f, 0.65f, 1f);
    private readonly Color textPrimary = new Color(0.95f, 0.95f, 0.95f, 1f);
    private readonly Color textSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);
    
    private void Start()
    {
        if (autoStart)
        {
            StartProfessionalGame();
        }
    }
    
    [ContextMenu("Start Professional Game")]
    public void StartProfessionalGame()
    {
        Debug.Log("🎮 Starting PROFESSIONAL Potato Legends Mobile Game...");
        
        CreateProfessionalCanvas();
        CreateSupabaseClient();
        ShowProfessionalAuthScreen();
        
        Debug.Log("✅ Professional mobile game started!");
    }
    
    private void CreateProfessionalCanvas()
    {
        // Main canvas with proper setup
        GameObject canvasObj = new GameObject("Professional Game Canvas");
        gameCanvas = canvasObj.AddComponent<Canvas>();
        gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameCanvas.sortingOrder = 100;
        
        // Professional mobile scaling
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); // Modern mobile resolution
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // EventSystem for input
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        
        Debug.Log("🎨 Professional canvas created");
    }
    
    private void CreateSupabaseClient()
    {
        GameObject supabaseObj = new GameObject("RealSupabaseClient");
        supabaseClient = supabaseObj.AddComponent<RealSupabaseClient>();
        supabaseClient.OnAuthenticationChanged += OnAuthChanged;
        
        Debug.Log("🔌 Professional Supabase client created");
    }
    
    private void ShowProfessionalAuthScreen()
    {
        currentScreen = Screen.Auth;
        ClearScreen();
        
        Debug.Log("🔐 Creating PROFESSIONAL auth screen...");
        
        // Main background with gradient effect
        currentScreenObject = CreateGradientBackground(
            "Professional Auth Screen",
            new Color(0.05f, 0.1f, 0.15f, 1f), // Dark blue-gray
            new Color(0.1f, 0.15f, 0.25f, 1f)  // Slightly lighter blue
        );
        
        // Auth card panel (floating card style)
        GameObject authCard = CreateProfessionalCard(
            "Auth Card", 
            currentScreenObject.transform,
            new Vector2(0.08f, 0.25f), 
            new Vector2(0.92f, 0.75f)
        );
        
        // Game logo/title
        CreateProfessionalTitle(authCard.transform, "POTATO LEGENDS", "Enter the Arena");
        
        // Input fields with professional styling
        emailField = CreateProfessionalInputField(
            "Email Input", 
            authCard.transform, 
            "Email Address", 
            new Vector2(0.08f, 0.55f), 
            new Vector2(0.92f, 0.65f)
        );
        
        passwordField = CreateProfessionalInputField(
            "Password Input", 
            authCard.transform, 
            "Password", 
            new Vector2(0.08f, 0.42f), 
            new Vector2(0.92f, 0.52f)
        );
        passwordField.inputType = TMP_InputField.InputType.Password;
        
        // Professional login button
        loginButton = CreateProfessionalButton(
            "Login Button", 
            authCard.transform, 
            "ENTER GAME", 
            primaryGreen,
            new Vector2(0.15f, 0.25f), 
            new Vector2(0.85f, 0.35f)
        );
        
        loginButton.onClick.AddListener(async () => await HandleLogin());
        
        // Status text
        statusText = CreateProfessionalStatusText(
            authCard.transform, 
            "Welcome! Enter your credentials to begin your journey.",
            new Vector2(0.08f, 0.08f), 
            new Vector2(0.92f, 0.18f)
        );
        
        // Animate auth card entrance
        StartCoroutine(AnimateCardEntrance(authCard));
        
        Debug.Log("✅ PROFESSIONAL auth screen created");
    }
    
    private void ShowProfessionalMainMenu()
    {
        currentScreen = Screen.MainMenu;
        ClearScreen();
        
        Debug.Log("🏠 Creating PROFESSIONAL main menu...");
        
        // Main background with gradient
        currentScreenObject = CreateGradientBackground(
            "Professional Main Menu",
            new Color(0.08f, 0.12f, 0.18f, 1f), // Dark blue
            new Color(0.12f, 0.18f, 0.28f, 1f)  // Medium blue
        );
        
        // Top header panel
        CreateMainMenuHeader();
        
        // Navigation cards (like Clash Royale style)
        CreateNavigationCards();
        
        // Battle button (prominent, bottom right)
        CreateProfessionalBattleButton();
        
        // User info panel
        CreateUserInfoPanel();
        
        Debug.Log("✅ PROFESSIONAL main menu created");
    }
    
    private void CreateMainMenuHeader()
    {
        GameObject headerPanel = CreateProfessionalCard(
            "Header Panel",
            currentScreenObject.transform,
            new Vector2(0.05f, 0.85f),
            new Vector2(0.95f, 0.97f)
        );
        
        // Game title
        GameObject titleObj = new GameObject("Game Title");
        titleObj.transform.SetParent(headerPanel.transform, false);
        titleObj.layer = 5;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "POTATO LEGENDS";
        titleText.fontSize = 32;
        titleText.color = primaryGold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0f);
        titleRect.anchorMax = new Vector2(0.7f, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Logout button (professional style)
        Button logoutBtn = CreateProfessionalButton(
            "Logout Button",
            headerPanel.transform,
            "LOGOUT",
            primaryRed,
            new Vector2(0.75f, 0.2f),
            new Vector2(0.95f, 0.8f)
        );
        
        logoutBtn.onClick.AddListener(async () => {
            await supabaseClient.SignOut();
            ShowProfessionalAuthScreen();
        });
    }
    
    private void CreateNavigationCards()
    {
        // Navigation cards container
        GameObject navContainer = new GameObject("Navigation Container");
        navContainer.transform.SetParent(currentScreenObject.transform, false);
        navContainer.layer = 5;
        
        RectTransform navRect = navContainer.AddComponent<RectTransform>();
        navRect.anchorMin = new Vector2(0.05f, 0.45f);
        navRect.anchorMax = new Vector2(0.95f, 0.8f);
        navRect.offsetMin = Vector2.zero;
        navRect.offsetMax = Vector2.zero;
        
        // Grid layout for cards
        GridLayoutGroup gridLayout = navContainer.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(280, 160); // Professional card size
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3; // 3 cards per row
        
        // Collection card
        CreateNavigationCard(navContainer.transform, "COLLECTION", "Browse Cards", primaryGreen, () => ShowCollection());
        
        // Deck Builder card  
        CreateNavigationCard(navContainer.transform, "DECK BUILDER", "Build Decks", new Color(0.7f, 0.3f, 0.9f, 1f), () => ShowDeckBuilder());
        
        // Hero Hall card
        CreateNavigationCard(navContainer.transform, "HERO HALL", "Choose Hero", new Color(0.9f, 0.5f, 0.2f, 1f), () => ShowHeroHall());
    }
    
    private void CreateNavigationCard(Transform parent, string title, string subtitle, Color accentColor, System.Action onClick)
    {
        GameObject card = CreateProfessionalCard("Nav Card", parent, Vector2.zero, Vector2.one);
        
        // Add gradient overlay
        CreateCardGradientOverlay(card.transform, accentColor);
        
        // Title
        GameObject titleObj = new GameObject("Card Title");
        titleObj.transform.SetParent(card.transform, false);
        titleObj.layer = 5;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 20;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.6f);
        titleRect.anchorMax = new Vector2(0.95f, 0.85f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Subtitle
        GameObject subtitleObj = new GameObject("Card Subtitle");
        subtitleObj.transform.SetParent(card.transform, false);
        subtitleObj.layer = 5;
        
        TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = subtitle;
        subtitleText.fontSize = 14;
        subtitleText.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.05f, 0.35f);
        subtitleRect.anchorMax = new Vector2(0.95f, 0.55f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
        
        // Button functionality
        Button cardButton = card.AddComponent<Button>();
        cardButton.onClick.AddListener(() => {
            StartCoroutine(AnimateCardPress(card));
            onClick?.Invoke();
        });
        
        // Professional button colors
        ColorBlock colors = cardButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        cardButton.colors = colors;
        
        // Add shadow effect
        CreateCardShadow(card.transform);
    }
    
    private void CreateProfessionalBattleButton()
    {
        // Battle button container with glow effect
        GameObject battleContainer = new GameObject("Battle Container");
        battleContainer.transform.SetParent(currentScreenObject.transform, false);
        battleContainer.layer = 5;
        
        RectTransform battleContainerRect = battleContainer.AddComponent<RectTransform>();
        battleContainerRect.anchorMin = new Vector2(0.65f, 0.05f);
        battleContainerRect.anchorMax = new Vector2(0.95f, 0.25f);
        battleContainerRect.offsetMin = Vector2.zero;
        battleContainerRect.offsetMax = Vector2.zero;
        
        // Glow background
        GameObject glowObj = CreatePanel("Battle Glow", battleContainer.transform);
        Image glowImage = glowObj.GetComponent<Image>();
        glowImage.color = new Color(1f, 0.3f, 0.1f, 0.3f); // Orange glow
        SetFullScreen(glowObj.GetComponent<RectTransform>());
        
        // Main battle button
        Button battleButton = CreateProfessionalButton(
            "Battle Button",
            battleContainer.transform,
            "BATTLE",
            primaryRed,
            new Vector2(0.1f, 0.1f),
            new Vector2(0.9f, 0.9f)
        );
        
        // Make battle button extra prominent
        TextMeshProUGUI battleText = battleButton.GetComponentInChildren<TextMeshProUGUI>();
        battleText.fontSize = 28;
        battleText.fontStyle = FontStyles.Bold;
        
        battleButton.onClick.AddListener(() => {
            Debug.Log("⚔️ BATTLE clicked - starting professional matchmaking...");
            StartCoroutine(AnimateBattlePress(battleButton.gameObject));
        });
        
        // Animate glow effect
        StartCoroutine(AnimateBattleGlow(glowObj));
    }
    
    private void CreateUserInfoPanel()
    {
        GameObject userPanel = CreateProfessionalCard(
            "User Info Panel",
            currentScreenObject.transform,
            new Vector2(0.05f, 0.25f),
            new Vector2(0.6f, 0.4f)
        );
        
        // User welcome text
        GameObject welcomeObj = new GameObject("User Welcome");
        welcomeObj.transform.SetParent(userPanel.transform, false);
        welcomeObj.layer = 5;
        
        TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
        welcomeText.text = "Welcome back, Champion!";
        welcomeText.fontSize = 18;
        welcomeText.color = textPrimary;
        welcomeText.alignment = TextAlignmentOptions.Center;
        welcomeText.fontStyle = FontStyles.Bold;
        
        RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
        welcomeRect.anchorMin = new Vector2(0.05f, 0.6f);
        welcomeRect.anchorMax = new Vector2(0.95f, 0.9f);
        welcomeRect.offsetMin = Vector2.zero;
        welcomeRect.offsetMax = Vector2.zero;
        
        // Stats text
        GameObject statsObj = new GameObject("User Stats");
        statsObj.transform.SetParent(userPanel.transform, false);
        statsObj.layer = 5;
        
        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = "Ready for battle • Deck validated";
        statsText.fontSize = 14;
        statsText.color = textSecondary;
        statsText.alignment = TextAlignmentOptions.Center;
        
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.05f, 0.2f);
        statsRect.anchorMax = new Vector2(0.95f, 0.5f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;
    }
    
    private async Task HandleLogin()
    {
        string email = emailField.text.Trim();
        string password = passwordField.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateProfessionalStatus("Please enter your credentials", Color.red);
            StartCoroutine(ShakeElement(loginButton.transform));
            return;
        }
        
        UpdateProfessionalStatus("Connecting to arena...", primaryBlue);
        StartCoroutine(AnimateLoadingButton(loginButton));
        
        try
        {
            bool success = await supabaseClient.SignIn(email, password);
            
            if (success)
            {
                UpdateProfessionalStatus("Welcome to the arena!", primaryGreen);
                StartCoroutine(AnimateSuccessAndTransition());
            }
            else
            {
                UpdateProfessionalStatus("Invalid credentials", primaryRed);
                StartCoroutine(ShakeElement(loginButton.transform));
            }
        }
        catch (System.Exception e)
        {
            UpdateProfessionalStatus("Connection failed", primaryRed);
            StartCoroutine(ShakeElement(loginButton.transform));
        }
    }
    
    private void ShowCollection()
    {
        currentScreen = Screen.Collection;
        ClearScreen();
        
        currentScreenObject = CreateGradientBackground(
            "Collection Screen",
            new Color(0.05f, 0.15f, 0.08f, 1f), // Dark green
            new Color(0.08f, 0.2f, 0.12f, 1f)   // Medium green
        );
        
        CreateProfessionalScreenHeader("COLLECTION", "Browse your cards", primaryGreen);
        CreateProfessionalScreenContent("Your collection of 236+ unique cards\nwith beautiful elemental backgrounds\n\nCards loading from database...");
        CreateProfessionalBackButton();
        
        Debug.Log("✅ PROFESSIONAL collection screen created");
    }
    
    private void ShowDeckBuilder()
    {
        currentScreen = Screen.DeckBuilder;
        ClearScreen();
        
        currentScreenObject = CreateGradientBackground(
            "Deck Builder Screen",
            new Color(0.15f, 0.05f, 0.15f, 1f), // Dark purple
            new Color(0.2f, 0.08f, 0.2f, 1f)    // Medium purple
        );
        
        CreateProfessionalScreenHeader("DECK BUILDER", "Craft your strategy", new Color(0.7f, 0.3f, 0.9f, 1f));
        CreateProfessionalScreenContent("Build powerful 30-card decks\nwith strategic copy limits\n\nCommon: 3 copies\nLegendary: 1 copy");
        CreateProfessionalBackButton();
        
        Debug.Log("✅ PROFESSIONAL deck builder screen created");
    }
    
    private void ShowHeroHall()
    {
        currentScreen = Screen.HeroHall;
        ClearScreen();
        
        currentScreenObject = CreateGradientBackground(
            "Hero Hall Screen",
            new Color(0.15f, 0.1f, 0.05f, 1f), // Dark bronze
            new Color(0.2f, 0.15f, 0.08f, 1f)  // Medium bronze
        );
        
        CreateProfessionalScreenHeader("HERO HALL", "Choose your champion", new Color(0.9f, 0.6f, 0.2f, 1f));
        CreateProfessionalScreenContent("Select your hero with unique abilities\nand powerful special attacks\n\nHeroes loading from database...");
        CreateProfessionalBackButton();
        
        Debug.Log("✅ PROFESSIONAL hero hall screen created");
    }
    
    #region Professional UI Creation Methods
    
    private GameObject CreateGradientBackground(string name, Color topColor, Color bottomColor)
    {
        GameObject bg = CreatePanel(name, gameCanvas.transform);
        SetFullScreen(bg.GetComponent<RectTransform>());
        
        // Create gradient effect using multiple panels
        Image topGradient = bg.GetComponent<Image>();
        topGradient.color = topColor;
        
        GameObject bottomGradient = CreatePanel("Bottom Gradient", bg.transform);
        Image bottomImage = bottomGradient.GetComponent<Image>();
        bottomImage.color = bottomColor;
        
        RectTransform bottomRect = bottomGradient.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0f, 0f);
        bottomRect.anchorMax = new Vector2(1f, 0.5f);
        bottomRect.offsetMin = Vector2.zero;
        bottomRect.offsetMax = Vector2.zero;
        
        return bg;
    }
    
    private GameObject CreateProfessionalCard(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject card = CreatePanel(name, parent);
        Image cardImage = card.GetComponent<Image>();
        cardImage.color = cardBackground;
        
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = anchorMin;
        cardRect.anchorMax = anchorMax;
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;
        
        return card;
    }
    
    private void CreateCardShadow(Transform cardTransform)
    {
        GameObject shadow = CreatePanel("Card Shadow", cardTransform.parent);
        Image shadowImage = shadow.GetComponent<Image>();
        shadowImage.color = new Color(0f, 0f, 0f, 0.3f);
        
        RectTransform shadowRect = shadow.GetComponent<RectTransform>();
        RectTransform cardRect = cardTransform.GetComponent<RectTransform>();
        
        shadowRect.anchorMin = cardRect.anchorMin;
        shadowRect.anchorMax = cardRect.anchorMax;
        shadowRect.offsetMin = new Vector2(8, -8); // Offset for shadow effect
        shadowRect.offsetMax = new Vector2(8, -8);
        
        shadow.transform.SetSiblingIndex(cardTransform.GetSiblingIndex()); // Behind card
    }
    
    private void CreateCardGradientOverlay(Transform cardTransform, Color accentColor)
    {
        GameObject overlay = CreatePanel("Card Overlay", cardTransform);
        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.2f);
        SetFullScreen(overlay.GetComponent<RectTransform>());
    }
    
    private void CreateProfessionalTitle(Transform parent, string mainTitle, string subtitle)
    {
        // Main title
        GameObject titleObj = new GameObject("Main Title");
        titleObj.transform.SetParent(parent, false);
        titleObj.layer = 5;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = mainTitle;
        titleText.fontSize = 32;
        titleText.color = primaryGold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.75f);
        titleRect.anchorMax = new Vector2(0.95f, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Subtitle
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(parent, false);
        subtitleObj.layer = 5;
        
        TextMeshProUGUI subtitleTextComp = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleTextComp.text = subtitle;
        subtitleTextComp.fontSize = 16;
        subtitleTextComp.color = textSecondary;
        subtitleTextComp.alignment = TextAlignmentOptions.Center;
        
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.05f, 0.68f);
        subtitleRect.anchorMax = new Vector2(0.95f, 0.75f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
    }
    
    private TMP_InputField CreateProfessionalInputField(string name, Transform parent, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject fieldObj = CreatePanel(name, parent);
        Image fieldImage = fieldObj.GetComponent<Image>();
        fieldImage.color = new Color(0.95f, 0.95f, 0.95f, 0.95f); // Almost white
        
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
        inputText.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark text
        inputText.alignment = TextAlignmentOptions.Left;
        
        RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = new Vector2(0.08f, 0f);
        inputTextRect.anchorMax = new Vector2(0.92f, 1f);
        inputTextRect.offsetMin = Vector2.zero;
        inputTextRect.offsetMax = Vector2.zero;
        
        // Placeholder with professional styling
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(fieldObj.transform, false);
        placeholderObj.layer = 5;
        
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        placeholderText.raycastTarget = false;
        
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0.08f, 0f);
        placeholderRect.anchorMax = new Vector2(0.92f, 1f);
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        
        return inputField;
    }
    
    private Button CreateProfessionalButton(string name, Transform parent, string text, Color baseColor, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject btnObj = CreatePanel(name, parent);
        Button button = btnObj.AddComponent<Button>();
        Image buttonImage = btnObj.GetComponent<Image>();
        buttonImage.color = baseColor;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = anchorMin;
        btnRect.anchorMax = anchorMax;
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        
        // Professional button colors
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        button.colors = colors;
        
        // Button text
        GameObject textObj = new GameObject("Button Text");
        textObj.transform.SetParent(btnObj.transform, false);
        textObj.layer = 5;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;
        
        SetFullScreen(textObj.GetComponent<RectTransform>());
        
        return button;
    }
    
    private TextMeshProUGUI CreateProfessionalStatusText(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject statusObj = new GameObject("Status Text");
        statusObj.transform.SetParent(parent, false);
        statusObj.layer = 5;
        
        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = text;
        statusText.fontSize = 16;
        statusText.color = textSecondary;
        statusText.alignment = TextAlignmentOptions.Center;
        
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = anchorMin;
        statusRect.anchorMax = anchorMax;
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
        
        return statusText;
    }
    
    private void CreateProfessionalScreenHeader(string title, string subtitle, Color accentColor)
    {
        GameObject headerCard = CreateProfessionalCard(
            "Screen Header",
            currentScreenObject.transform,
            new Vector2(0.05f, 0.85f),
            new Vector2(0.95f, 0.97f)
        );
        
        CreateCardGradientOverlay(headerCard.transform, accentColor);
        
        // Title
        GameObject titleObj = new GameObject("Screen Title");
        titleObj.transform.SetParent(headerCard.transform, false);
        titleObj.layer = 5;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 28;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.3f);
        titleRect.anchorMax = new Vector2(0.9f, 0.8f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Subtitle
        GameObject subtitleObj = new GameObject("Screen Subtitle");
        subtitleObj.transform.SetParent(headerCard.transform, false);
        subtitleObj.layer = 5;
        
        TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = subtitle;
        subtitleText.fontSize = 14;
        subtitleText.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.1f, 0.1f);
        subtitleRect.anchorMax = new Vector2(0.9f, 0.4f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
    }
    
    private void CreateProfessionalScreenContent(string content)
    {
        GameObject contentCard = CreateProfessionalCard(
            "Content Card",
            currentScreenObject.transform,
            new Vector2(0.08f, 0.3f),
            new Vector2(0.92f, 0.8f)
        );
        
        GameObject contentObj = new GameObject("Content Text");
        contentObj.transform.SetParent(contentCard.transform, false);
        contentObj.layer = 5;
        
        TextMeshProUGUI contentText = contentObj.AddComponent<TextMeshProUGUI>();
        contentText.text = content;
        contentText.fontSize = 20;
        contentText.color = textPrimary;
        contentText.alignment = TextAlignmentOptions.Center;
        
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.1f);
        contentRect.anchorMax = new Vector2(0.9f, 0.9f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
    }
    
    private void CreateProfessionalBackButton()
    {
        Button backBtn = CreateProfessionalButton(
            "Professional Back Button",
            currentScreenObject.transform,
            "BACK",
            primaryBlue,
            new Vector2(0.05f, 0.05f),
            new Vector2(0.3f, 0.15f)
        );
        
        backBtn.onClick.AddListener(() => {
            StartCoroutine(AnimateCardPress(backBtn.gameObject));
            ShowProfessionalMainMenu();
        });
    }
    
    #endregion
    
    #region Professional Animations
    
    private IEnumerator AnimateCardEntrance(GameObject card)
    {
        Transform cardTransform = card.transform;
        Vector3 originalScale = Vector3.one;
        
        // Start small and grow
        cardTransform.localScale = Vector3.zero;
        
        float time = 0f;
        float duration = 0.5f;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            
            // Smooth ease-out animation
            float easedProgress = 1f - (1f - progress) * (1f - progress);
            cardTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easedProgress);
            
            yield return null;
        }
        
        cardTransform.localScale = originalScale;
    }
    
    private IEnumerator AnimateCardPress(GameObject card)
    {
        Transform cardTransform = card.transform;
        Vector3 originalScale = cardTransform.localScale;
        Vector3 pressedScale = originalScale * 0.95f;
        
        // Press down
        float time = 0f;
        float duration = 0.1f;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            cardTransform.localScale = Vector3.Lerp(originalScale, pressedScale, progress);
            yield return null;
        }
        
        // Release back
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            cardTransform.localScale = Vector3.Lerp(pressedScale, originalScale, progress);
            yield return null;
        }
        
        cardTransform.localScale = originalScale;
    }
    
    private IEnumerator AnimateBattleGlow(GameObject glowObj)
    {
        Image glowImage = glowObj.GetComponent<Image>();
        Color originalColor = glowImage.color;
        
        while (glowObj != null)
        {
            // Pulse glow effect
            float pulse = (Mathf.Sin(Time.time * 2f) + 1f) / 2f; // 0 to 1
            Color glowColor = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * pulse);
            glowImage.color = glowColor;
            
            yield return null;
        }
    }
    
    private IEnumerator AnimateBattlePress(GameObject battleButton)
    {
        // Special battle button animation
        yield return StartCoroutine(AnimateCardPress(battleButton));
        
        // Add extra effect for battle
        Debug.Log("🔥 Battle button pressed with style!");
    }
    
    private IEnumerator ShakeElement(Transform element)
    {
        Vector3 originalPosition = element.localPosition;
        float shakeDuration = 0.5f;
        float shakeIntensity = 10f;
        
        float time = 0f;
        
        while (time < shakeDuration)
        {
            time += Time.deltaTime;
            
            float x = Random.Range(-shakeIntensity, shakeIntensity);
            element.localPosition = originalPosition + new Vector3(x, 0, 0);
            
            yield return null;
        }
        
        element.localPosition = originalPosition;
    }
    
    private IEnumerator AnimateLoadingButton(Button button)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        string originalText = buttonText.text;
        
        // Loading animation
        string[] loadingStates = { "CONNECTING", "CONNECTING.", "CONNECTING..", "CONNECTING..." };
        int currentState = 0;
        
        for (int i = 0; i < 8; i++) // 2 seconds of loading animation
        {
            buttonText.text = loadingStates[currentState];
            currentState = (currentState + 1) % loadingStates.Length;
            yield return new WaitForSeconds(0.25f);
        }
        
        buttonText.text = originalText;
    }
    
    private IEnumerator AnimateSuccessAndTransition()
    {
        // Success animation
        yield return new WaitForSeconds(1f);
        
        // Fade out auth screen and show main menu
        ShowProfessionalMainMenu();
    }
    
    #endregion
    
    private void UpdateProfessionalStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
            
            // Animate status text
            StartCoroutine(AnimateStatusText());
        }
        
        Debug.Log($"📱 Status: {message}");
    }
    
    private IEnumerator AnimateStatusText()
    {
        if (statusText == null) yield break;
        
        Transform textTransform = statusText.transform;
        Vector3 originalScale = textTransform.localScale;
        
        // Quick scale animation
        textTransform.localScale = Vector3.zero;
        
        float time = 0f;
        float duration = 0.3f;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            textTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }
        
        textTransform.localScale = originalScale;
    }
    
    private void OnAuthChanged(bool isAuthenticated)
    {
        Debug.Log($"🔐 Professional auth changed: {isAuthenticated}");
        
        if (isAuthenticated && currentScreen == Screen.Auth)
        {
            ShowProfessionalMainMenu();
        }
        else if (!isAuthenticated && currentScreen != Screen.Auth)
        {
            ShowProfessionalAuthScreen();
        }
    }
    
    private void ClearScreen()
    {
        if (currentScreenObject != null)
        {
            Destroy(currentScreenObject);
            currentScreenObject = null;
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
        if (supabaseClient != null)
        {
            supabaseClient.OnAuthenticationChanged -= OnAuthChanged;
        }
    }
}