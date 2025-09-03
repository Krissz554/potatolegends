using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// CLEAN Game Starter - This actually works and starts with AUTH screen!
/// Delete all other GameStarter scripts and use ONLY this one
/// </summary>
public class CleanGameStarter : MonoBehaviour
{
    [Header("🥔 Clean Potato Legends Game")]
    [SerializeField] private bool autoStart = true;
    
    // Game state
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
    
    private void Start()
    {
        if (autoStart)
        {
            StartCleanGame();
        }
    }
    
    [ContextMenu("Start Clean Game")]
    public void StartCleanGame()
    {
        Debug.Log("🥔 Starting CLEAN Potato Legends game...");
        
        // Create basic systems
        CreateCanvas();
        CreateSupabaseClient();
        
        // Show AUTH screen first (like web version)
        ShowAuthScreen();
        
        Debug.Log("✅ Clean game started - showing AUTH screen first!");
    }
    
    private void CreateCanvas()
    {
        // Create main canvas
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
        }
        
        Debug.Log("✅ Canvas and EventSystem created");
    }
    
    private void CreateSupabaseClient()
    {
        GameObject supabaseObj = new GameObject("RealSupabaseClient");
        supabaseClient = supabaseObj.AddComponent<RealSupabaseClient>();
        supabaseClient.OnAuthenticationChanged += OnAuthChanged;
        
        Debug.Log("✅ Supabase client created");
    }
    
    private void ShowAuthScreen()
    {
        currentScreen = Screen.Auth;
        ClearScreen();
        
        Debug.Log("🔐 Creating AUTH screen...");
        
        // Create auth screen
        currentScreenObject = CreatePanel("Auth Screen", gameCanvas.transform);
        Image bg = currentScreenObject.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.1f, 0.2f, 1f); // Dark blue
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        // Title
        CreateTitle("POTATO LEGENDS\nMOBILE CARD GAME");
        
        // Email field
        emailField = CreateEmailField();
        
        // Password field  
        passwordField = CreatePasswordField();
        
        // Login button
        CreateLoginButton();
        
        // Status text
        statusText = CreateStatusText("Enter your Supabase credentials to play!");
        
        Debug.Log("✅ AUTH screen created and shown");
    }
    
    private void CreateTitle(string text)
    {
        GameObject titleObj = new GameObject("Game Title");
        titleObj.transform.SetParent(currentScreenObject.transform, false);
        titleObj.layer = 5;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = text;
        titleText.fontSize = 40;
        titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.7f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    private TMP_InputField CreateEmailField()
    {
        return CreateInputField("Email Field", "Enter your email...", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.65f));
    }
    
    private TMP_InputField CreatePasswordField()
    {
        TMP_InputField field = CreateInputField("Password Field", "Enter your password...", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f));
        field.inputType = TMP_InputField.InputType.Password;
        return field;
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
        inputText.fontSize = 20;
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
        placeholderText.fontSize = 20;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        placeholderText.raycastTarget = false;
        
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0.05f, 0f);
        placeholderRect.anchorMax = new Vector2(0.95f, 1f);
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        
        // Connect input field
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        
        return inputField;
    }
    
    private void CreateLoginButton()
    {
        Button loginButton = CreateButton("Login Button", "LOGIN", new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.35f));
        loginButton.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green
        
        loginButton.onClick.AddListener(async () => {
            await TryLogin();
        });
    }
    
    private async Task TryLogin()
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
                await Task.Delay(1000); // Show success message
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
    
    private void UpdateStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"📱 Status: {message}");
    }
    
    private void ShowMainMenu()
    {
        currentScreen = Screen.MainMenu;
        ClearScreen();
        
        Debug.Log("🏠 Creating MAIN MENU...");
        
        // Create main menu screen
        currentScreenObject = CreatePanel("Main Menu Screen", gameCanvas.transform);
        Image bg = currentScreenObject.GetComponent<Image>();
        bg.color = new Color(0.1f, 0.15f, 0.3f, 1f); // Dark blue-purple
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        // Welcome title
        GameObject welcomeObj = new GameObject("Welcome Title");
        welcomeObj.transform.SetParent(currentScreenObject.transform, false);
        welcomeObj.layer = 5;
        
        TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
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
        
        // Top navigation (like web version)
        CreateTopNavigation();
        
        // BATTLE button (bottom right like web version)
        CreateBattleButton();
        
        // Logout button
        CreateLogoutButton();
        
        Debug.Log("✅ MAIN MENU created");
    }
    
    private void CreateTopNavigation()
    {
        // Navigation panel
        GameObject navPanel = CreatePanel("Navigation Panel", currentScreenObject.transform);
        Image navBg = navPanel.GetComponent<Image>();
        navBg.color = new Color(0f, 0f, 0f, 0.5f);
        
        RectTransform navRect = navPanel.GetComponent<RectTransform>();
        navRect.anchorMin = new Vector2(0.05f, 0.5f);
        navRect.anchorMax = new Vector2(0.95f, 0.65f);
        navRect.offsetMin = Vector2.zero;
        navRect.offsetMax = Vector2.zero;
        
        // Collection button
        Button collectionBtn = CreateButton("Collection Button", "COLLECTION", new Vector2(0.05f, 0.1f), new Vector2(0.3f, 0.9f));
        collectionBtn.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 1f);
        collectionBtn.transform.SetParent(navPanel.transform, false);
        collectionBtn.onClick.AddListener(() => ShowCollection());
        
        // Deck Builder button
        Button deckBtn = CreateButton("Deck Builder Button", "DECK BUILDER", new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.9f));
        deckBtn.GetComponent<Image>().color = new Color(0.7f, 0.3f, 0.7f, 1f);
        deckBtn.transform.SetParent(navPanel.transform, false);
        deckBtn.onClick.AddListener(() => ShowDeckBuilder());
        
        // Hero Hall button
        Button heroBtn = CreateButton("Hero Hall Button", "HERO HALL", new Vector2(0.7f, 0.1f), new Vector2(0.95f, 0.9f));
        heroBtn.GetComponent<Image>().color = new Color(0.9f, 0.6f, 0.2f, 1f);
        heroBtn.transform.SetParent(navPanel.transform, false);
        heroBtn.onClick.AddListener(() => ShowHeroHall());
    }
    
    private void CreateBattleButton()
    {
        Button battleBtn = CreateButton("Battle Button", "BATTLE", new Vector2(0.7f, 0.05f), new Vector2(0.95f, 0.2f));
        battleBtn.GetComponent<Image>().color = new Color(1f, 0.2f, 0.2f, 1f); // Red
        battleBtn.transform.SetParent(currentScreenObject.transform, false);
        battleBtn.onClick.AddListener(() => {
            Debug.Log("⚔️ BATTLE clicked - starting matchmaking...");
            UpdateStatus("Starting matchmaking...", Color.yellow);
        });
    }
    
    private void CreateLogoutButton()
    {
        Button logoutBtn = CreateButton("Logout Button", "LOGOUT", new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.15f));
        logoutBtn.GetComponent<Image>().color = new Color(0.7f, 0.3f, 0.3f, 1f);
        logoutBtn.transform.SetParent(currentScreenObject.transform, false);
        logoutBtn.onClick.AddListener(async () => {
            await supabaseClient.SignOut();
            ShowAuthScreen();
        });
    }
    
    private void ShowCollection()
    {
        currentScreen = Screen.Collection;
        ClearScreen();
        
        Debug.Log("📚 Creating COLLECTION screen...");
        
        currentScreenObject = CreatePanel("Collection Screen", gameCanvas.transform);
        currentScreenObject.GetComponent<Image>().color = new Color(0.05f, 0.15f, 0.05f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        CreateScreenTitle("CARD COLLECTION");
        CreateScreenInfo("Loading your 236+ cards from database...\nWith beautiful elemental backgrounds!");
        CreateBackButton();
        
        Debug.Log("✅ COLLECTION screen created");
    }
    
    private void ShowDeckBuilder()
    {
        currentScreen = Screen.DeckBuilder;
        ClearScreen();
        
        Debug.Log("🔧 Creating DECK BUILDER screen...");
        
        currentScreenObject = CreatePanel("Deck Builder Screen", gameCanvas.transform);
        currentScreenObject.GetComponent<Image>().color = new Color(0.15f, 0.05f, 0.15f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        CreateScreenTitle("DECK BUILDER");
        CreateScreenInfo("Build your 30-card deck\nwith copy limits by rarity\n(Common: 3, Legendary: 1)");
        CreateBackButton();
        
        Debug.Log("✅ DECK BUILDER screen created");
    }
    
    private void ShowHeroHall()
    {
        currentScreen = Screen.HeroHall;
        ClearScreen();
        
        Debug.Log("🦸 Creating HERO HALL screen...");
        
        currentScreenObject = CreatePanel("Hero Hall Screen", gameCanvas.transform);
        currentScreenObject.GetComponent<Image>().color = new Color(0.15f, 0.1f, 0.05f, 1f);
        SetFullScreen(currentScreenObject.GetComponent<RectTransform>());
        
        CreateScreenTitle("HERO HALL");
        CreateScreenInfo("Select your hero\nwith unique powers and abilities\nfrom your database");
        CreateBackButton();
        
        Debug.Log("✅ HERO HALL screen created");
    }
    
    private void CreateScreenTitle(string title)
    {
        GameObject titleObj = new GameObject("Screen Title");
        titleObj.transform.SetParent(currentScreenObject.transform, false);
        titleObj.layer = 5;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 36;
        titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    private void CreateScreenInfo(string info)
    {
        GameObject infoObj = new GameObject("Screen Info");
        infoObj.transform.SetParent(currentScreenObject.transform, false);
        infoObj.layer = 5;
        
        TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = info;
        infoText.fontSize = 24;
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Center;
        
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.1f, 0.4f);
        infoRect.anchorMax = new Vector2(0.9f, 0.6f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
    }
    
    private void CreateBackButton()
    {
        Button backBtn = CreateButton("Back Button", "BACK TO MENU", new Vector2(0.1f, 0.05f), new Vector2(0.4f, 0.15f));
        backBtn.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.9f, 1f);
        backBtn.transform.SetParent(currentScreenObject.transform, false);
        backBtn.onClick.AddListener(() => ShowMainMenu());
    }
    
    private Button CreateButton(string name, string text, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject btnObj = CreatePanel(name, currentScreenObject.transform);
        Button button = btnObj.AddComponent<Button>();
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = anchorMin;
        btnRect.anchorMax = anchorMax;
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        
        GameObject textObj = new GameObject("Button Text");
        textObj.transform.SetParent(btnObj.transform, false);
        textObj.layer = 5;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    private void OnAuthChanged(bool isAuthenticated)
    {
        Debug.Log($"🔐 Auth changed: {isAuthenticated}");
        
        if (isAuthenticated && currentScreen == Screen.Auth)
        {
            ShowMainMenu();
        }
        else if (!isAuthenticated && currentScreen != Screen.Auth)
        {
            ShowAuthScreen();
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
    
    private void OnDestroy()
    {
        if (supabaseClient != null)
        {
            supabaseClient.OnAuthenticationChanged -= OnAuthChanged;
        }
    }
}