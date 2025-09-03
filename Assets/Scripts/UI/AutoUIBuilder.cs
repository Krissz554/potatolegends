using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Automatically creates the complete mobile UI layout at runtime
    /// This eliminates the need to manually set up UI in Unity Editor
    /// </summary>
    public class AutoUIBuilder : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private bool buildUIOnStart = true;
        [SerializeField] private Font defaultFont;
        
        private Canvas mainCanvas;
        private GameObject loginScreen;
        private GameObject mainGameUI;
        
        private void Start()
        {
            if (buildUIOnStart)
            {
                StartCoroutine(BuildCompleteUI());
            }
        }
        
        private IEnumerator BuildCompleteUI()
        {
            Debug.Log("🏗️ Auto-building mobile UI...");
            
            // Wait a frame to ensure other systems are ready
            yield return null;
            
            // Create main canvas
            CreateMainCanvas();
            
            // Create login screen
            CreateLoginScreen();
            
            // Create main game UI
            CreateMainGameUI();
            
            Debug.Log("✅ Mobile UI auto-build complete!");
        }
        
        private void CreateMainCanvas()
        {
            // Find existing canvas or create new one
            mainCanvas = FindObjectOfType<Canvas>();
            
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("UI Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Configure canvas for mobile
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;
            
            // Configure canvas scaler for mobile
            CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920); // Mobile portrait
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
            
            Debug.Log("📱 Canvas configured for mobile");
        }
        
        private void CreateLoginScreen()
        {
            // Create login screen container
            loginScreen = CreateUIObject("LoginScreen", mainCanvas.transform);
            
            // Add background
            Image loginBg = loginScreen.AddComponent<Image>();
            loginBg.color = new Color(0.18f, 0.11f, 0.21f, 0.95f); // Dark purple background
            
            RectTransform loginRect = loginScreen.GetComponent<RectTransform>();
            SetFullScreen(loginRect);
            
            // Create title
            GameObject titleObj = CreateUIObject("Title", loginScreen.transform);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "What's My Potato?";
            titleText.fontSize = 64;
            titleText.color = new Color(1f, 0.8f, 0.4f, 1f); // Golden color
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            SetAnchoredPosition(titleRect, 0.1f, 0.8f, 0.9f, 0.95f);
            
            // Create status text
            GameObject statusObj = CreateUIObject("Status", loginScreen.transform);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Welcome! Please sign in to continue.";
            statusText.fontSize = 28;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            SetAnchoredPosition(statusRect, 0.1f, 0.65f, 0.9f, 0.75f);
            
            // Create login panel
            CreateLoginPanel(loginScreen.transform);
            
            // Create signup panel
            CreateSignupPanel(loginScreen.transform);
            
            // Create loading indicator
            CreateLoadingIndicator(loginScreen.transform);
            
            // Add LoginScreen script
            LoginScreen loginScript = loginScreen.AddComponent<LoginScreen>();
            
            // Connect references (we'll do this through code)
            ConnectLoginScreenReferences(loginScript);
            
            Debug.Log("🔐 Login screen created");
        }
        
        private void CreateLoginPanel(Transform parent)
        {
            GameObject loginPanel = CreateUIObject("LoginPanel", parent);
            RectTransform panelRect = loginPanel.GetComponent<RectTransform>();
            SetAnchoredPosition(panelRect, 0.1f, 0.3f, 0.9f, 0.6f);
            
            // Email input
            GameObject emailObj = CreateInputField("Email Input", loginPanel.transform, "Enter your email");
            RectTransform emailRect = emailObj.GetComponent<RectTransform>();
            SetAnchoredPosition(emailRect, 0f, 0.7f, 1f, 0.9f);
            
            // Password input
            GameObject passwordObj = CreateInputField("Password Input", loginPanel.transform, "Enter your password");
            TMP_InputField passwordInput = passwordObj.GetComponent<TMP_InputField>();
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            RectTransform passwordRect = passwordObj.GetComponent<RectTransform>();
            SetAnchoredPosition(passwordRect, 0f, 0.45f, 1f, 0.65f);
            
            // Login button
            GameObject loginBtnObj = CreateButton("Login Button", loginPanel.transform, "Sign In");
            RectTransform loginBtnRect = loginBtnObj.GetComponent<RectTransform>();
            SetAnchoredPosition(loginBtnRect, 0f, 0.2f, 1f, 0.4f);
            
            // Switch to signup button
            GameObject switchBtnObj = CreateButton("Switch To Signup", loginPanel.transform, "Don't have an account? Sign Up");
            Button switchBtn = switchBtnObj.GetComponent<Button>();
            ColorBlock colors = switchBtn.colors;
            colors.normalColor = Color.clear;
            switchBtn.colors = colors;
            
            TextMeshProUGUI switchText = switchBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            switchText.fontSize = 24;
            switchText.color = new Color(0.4f, 0.8f, 1f, 1f); // Light blue
            
            RectTransform switchRect = switchBtnObj.GetComponent<RectTransform>();
            SetAnchoredPosition(switchRect, 0f, 0.05f, 1f, 0.15f);
        }
        
        private void CreateSignupPanel(Transform parent)
        {
            GameObject signupPanel = CreateUIObject("SignupPanel", parent);
            RectTransform panelRect = signupPanel.GetComponent<RectTransform>();
            SetAnchoredPosition(panelRect, 0.1f, 0.25f, 0.9f, 0.65f);
            signupPanel.SetActive(false); // Start hidden
            
            // Display name input
            GameObject nameObj = CreateInputField("Display Name Input", signupPanel.transform, "Enter display name (optional)");
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            SetAnchoredPosition(nameRect, 0f, 0.8f, 1f, 0.95f);
            
            // Email input
            GameObject emailObj = CreateInputField("Email Input", signupPanel.transform, "Enter your email");
            RectTransform emailRect = emailObj.GetComponent<RectTransform>();
            SetAnchoredPosition(emailRect, 0f, 0.6f, 1f, 0.75f);
            
            // Password input
            GameObject passwordObj = CreateInputField("Password Input", signupPanel.transform, "Enter your password");
            TMP_InputField passwordInput = passwordObj.GetComponent<TMP_InputField>();
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            RectTransform passwordRect = passwordObj.GetComponent<RectTransform>();
            SetAnchoredPosition(passwordRect, 0f, 0.4f, 1f, 0.55f);
            
            // Signup button
            GameObject signupBtnObj = CreateButton("Signup Button", signupPanel.transform, "Create Account");
            RectTransform signupBtnRect = signupBtnObj.GetComponent<RectTransform>();
            SetAnchoredPosition(signupBtnRect, 0f, 0.2f, 1f, 0.35f);
            
            // Switch to login button
            GameObject switchBtnObj = CreateButton("Switch To Login", signupPanel.transform, "Already have an account? Sign In");
            Button switchBtn = switchBtnObj.GetComponent<Button>();
            ColorBlock colors = switchBtn.colors;
            colors.normalColor = Color.clear;
            switchBtn.colors = colors;
            
            TextMeshProUGUI switchText = switchBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            switchText.fontSize = 24;
            switchText.color = new Color(0.4f, 0.8f, 1f, 1f); // Light blue
            
            RectTransform switchRect = switchBtnObj.GetComponent<RectTransform>();
            SetAnchoredPosition(switchRect, 0f, 0.05f, 1f, 0.15f);
        }
        
        private void CreateLoadingIndicator(Transform parent)
        {
            GameObject loadingObj = CreateUIObject("Loading Indicator", parent);
            RectTransform loadingRect = loadingObj.GetComponent<RectTransform>();
            SetAnchoredPosition(loadingRect, 0.4f, 0.15f, 0.6f, 0.25f);
            loadingObj.SetActive(false); // Start hidden
            
            // Add spinning image for loading
            Image loadingImage = loadingObj.AddComponent<Image>();
            loadingImage.color = new Color(1f, 0.8f, 0.4f, 1f);
            
            // Add rotation animation
            StartCoroutine(RotateLoadingIndicator(loadingObj.transform));
        }
        
        private void CreateMainGameUI()
        {
            // Create main game UI container
            mainGameUI = CreateUIObject("MainGameUI", mainCanvas.transform);
            mainGameUI.SetActive(false); // Start hidden
            
            RectTransform mainRect = mainGameUI.GetComponent<RectTransform>();
            SetFullScreen(mainRect);
            
            // Create top bar
            CreateTopBar(mainGameUI.transform);
            
            // Create navigation bar
            CreateNavigationBar(mainGameUI.transform);
            
            // Create battle button
            CreateBattleButton(mainGameUI.transform);
            
            // Create main page panel
            CreateMainPagePanel(mainGameUI.transform);
            
            // Add MobileMainUI script
            MobileMainUI mainUIScript = mainGameUI.AddComponent<MobileMainUI>();
            ConnectMainUIReferences(mainUIScript);
            
            Debug.Log("🎮 Main game UI created");
        }
        
        private void CreateTopBar(Transform parent)
        {
            GameObject topBar = CreateUIObject("TopBar", parent);
            
            // Add background
            Image topBarBg = topBar.AddComponent<Image>();
            topBarBg.color = new Color(0.1f, 0.05f, 0.15f, 0.8f);
            
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            SetAnchoredPosition(topBarRect, 0f, 0.9f, 1f, 1f);
            
            // Screen title (left)
            GameObject titleObj = CreateUIObject("Screen Title", topBar.transform);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "What's My Potato?";
            titleText.fontSize = 36;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Left;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            SetAnchoredPosition(titleRect, 0.05f, 0f, 0.6f, 1f);
            
            // User name (center)
            GameObject userObj = CreateUIObject("User Name", topBar.transform);
            TextMeshProUGUI userText = userObj.AddComponent<TextMeshProUGUI>();
            userText.text = "Welcome!";
            userText.fontSize = 24;
            userText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            userText.alignment = TextAlignmentOptions.Center;
            
            RectTransform userRect = userObj.GetComponent<RectTransform>();
            SetAnchoredPosition(userRect, 0.3f, 0f, 0.7f, 1f);
            
            // Settings button (right)
            GameObject settingsObj = CreateButton("Settings Button", topBar.transform, "⚙️");
            RectTransform settingsRect = settingsObj.GetComponent<RectTransform>();
            SetAnchoredPosition(settingsRect, 0.75f, 0.1f, 0.9f, 0.9f);
            
            // Logout button (far right)
            GameObject logoutObj = CreateButton("Logout Button", topBar.transform, "🚪");
            RectTransform logoutRect = logoutObj.GetComponent<RectTransform>();
            SetAnchoredPosition(logoutRect, 0.9f, 0.1f, 1f, 0.9f);
        }
        
        private void CreateNavigationBar(Transform parent)
        {
            GameObject navBar = CreateUIObject("NavigationBar", parent);
            
            // Add background
            Image navBarBg = navBar.AddComponent<Image>();
            navBarBg.color = new Color(0.1f, 0.05f, 0.15f, 0.9f);
            
            RectTransform navBarRect = navBar.GetComponent<RectTransform>();
            SetAnchoredPosition(navBarRect, 0f, 0f, 1f, 0.12f);
            
            // Create navigation buttons
            string[] navLabels = { "🏠\nHome", "📚\nCards", "🔧\nDecks", "🦸\nHeroes" };
            float[] positions = { 0f, 0.25f, 0.5f, 0.75f };
            
            for (int i = 0; i < navLabels.Length; i++)
            {
                GameObject btnObj = CreateButton($"Nav Button {i}", navBar.transform, navLabels[i]);
                RectTransform btnRect = btnObj.GetComponent<RectTransform>();
                SetAnchoredPosition(btnRect, positions[i], 0.1f, positions[i] + 0.23f, 0.9f);
                
                // Style navigation button
                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                btnText.fontSize = 20;
                btnText.alignment = TextAlignmentOptions.Center;
            }
        }
        
        private void CreateBattleButton(Transform parent)
        {
            GameObject battleBtn = CreateButton("Battle Button", parent, "⚔️\nBATTLE");
            
            // Position in bottom right corner
            RectTransform battleRect = battleBtn.GetComponent<RectTransform>();
            SetAnchoredPosition(battleRect, 0.7f, 0.15f, 0.95f, 0.35f);
            
            // Style battle button
            Button btnComponent = battleBtn.GetComponent<Button>();
            ColorBlock colors = btnComponent.colors;
            colors.normalColor = new Color(1f, 0.3f, 0.3f, 1f); // Red battle color
            colors.highlightedColor = new Color(1f, 0.4f, 0.4f, 1f);
            colors.pressedColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            btnComponent.colors = colors;
            
            TextMeshProUGUI battleText = battleBtn.GetComponentInChildren<TextMeshProUGUI>();
            battleText.fontSize = 28;
            battleText.fontStyle = FontStyles.Bold;
            battleText.color = Color.white;
            battleText.alignment = TextAlignmentOptions.Center;
        }
        
        private void CreateMainPagePanel(Transform parent)
        {
            GameObject mainPanel = CreateUIObject("MainPagePanel", parent);
            RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
            SetAnchoredPosition(panelRect, 0f, 0.12f, 1f, 0.9f);
            
            // Welcome text
            GameObject welcomeObj = CreateUIObject("Welcome Text", mainPanel.transform);
            TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
            welcomeText.text = "Ready for battle?\nChoose your adventure!";
            welcomeText.fontSize = 36;
            welcomeText.color = Color.white;
            welcomeText.alignment = TextAlignmentOptions.Center;
            
            RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
            SetAnchoredPosition(welcomeRect, 0.1f, 0.7f, 0.9f, 0.9f);
            
            // Stats text
            GameObject statsObj = CreateUIObject("Stats Text", mainPanel.transform);
            TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = "Loading your stats...";
            statsText.fontSize = 24;
            statsText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            statsText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            SetAnchoredPosition(statsRect, 0.1f, 0.5f, 0.9f, 0.65f);
            
            // Quick play button
            GameObject quickPlayObj = CreateButton("Quick Play Button", mainPanel.transform, "🎮 Quick Play");
            RectTransform quickPlayRect = quickPlayObj.GetComponent<RectTransform>();
            SetAnchoredPosition(quickPlayRect, 0.2f, 0.3f, 0.8f, 0.45f);
            
            // Style quick play button
            Button quickPlayBtn = quickPlayObj.GetComponent<Button>();
            ColorBlock colors = quickPlayBtn.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.8f, 1f); // Blue
            quickPlayBtn.colors = colors;
            
            // Matchmaking panel (hidden by default)
            CreateMatchmakingPanel(mainPanel.transform);
        }
        
        private void CreateMatchmakingPanel(Transform parent)
        {
            GameObject matchPanel = CreateUIObject("MatchmakingPanel", parent);
            matchPanel.SetActive(false); // Start hidden
            
            // Add semi-transparent background
            Image matchBg = matchPanel.AddComponent<Image>();
            matchBg.color = new Color(0f, 0f, 0f, 0.7f);
            
            RectTransform matchRect = matchPanel.GetComponent<RectTransform>();
            SetAnchoredPosition(matchRect, 0.1f, 0.2f, 0.9f, 0.6f);
            
            // Match status text
            GameObject statusObj = CreateUIObject("Match Status", matchPanel.transform);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Searching for opponent...";
            statusText.fontSize = 32;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            SetAnchoredPosition(statusRect, 0.1f, 0.6f, 0.9f, 0.8f);
            
            // Cancel button
            GameObject cancelObj = CreateButton("Cancel Match Button", matchPanel.transform, "Cancel");
            RectTransform cancelRect = cancelObj.GetComponent<RectTransform>();
            SetAnchoredPosition(cancelRect, 0.3f, 0.2f, 0.7f, 0.4f);
        }
        
        #region Helper Methods
        
        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            obj.transform.SetParent(parent, false);
            obj.layer = 5; // UI layer
            return obj;
        }
        
        private GameObject CreateButton(string name, Transform parent, string text)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            
            // Add button component
            Button button = btnObj.AddComponent<Button>();
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.6f, 0.9f, 1f); // Default blue
            
            // Add text child
            GameObject textObj = CreateUIObject("Text", btnObj.transform);
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 28;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            SetFullScreen(textRect);
            
            // Button navigation
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Automatic;
            button.navigation = nav;
            
            return btnObj;
        }
        
        private GameObject CreateInputField(string name, Transform parent, string placeholder)
        {
            GameObject inputObj = CreateUIObject(name, parent);
            
            // Add input field components
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(1f, 1f, 1f, 0.9f); // White background
            
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            
            // Create text area child
            GameObject textAreaObj = CreateUIObject("Text Area", inputObj.transform);
            RectMask2D mask = textAreaObj.AddComponent<RectMask2D>();
            RectTransform textAreaRect = textAreaObj.GetComponent<RectTransform>();
            SetAnchoredPosition(textAreaRect, 0.05f, 0f, 0.95f, 1f);
            
            // Create placeholder text
            GameObject placeholderObj = CreateUIObject("Placeholder", textAreaObj.transform);
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 24;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.fontStyle = FontStyles.Italic;
            
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            SetFullScreen(placeholderRect);
            
            // Create input text
            GameObject inputTextObj = CreateUIObject("Text", textAreaObj.transform);
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 24;
            inputText.color = Color.black;
            
            RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
            SetFullScreen(inputTextRect);
            
            // Connect input field references
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            return inputObj;
        }
        
        private void SetFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        private void SetAnchoredPosition(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        private IEnumerator RotateLoadingIndicator(Transform loadingTransform)
        {
            while (true)
            {
                if (loadingTransform != null && loadingTransform.gameObject.activeInHierarchy)
                {
                    loadingTransform.Rotate(0f, 0f, -90f * Time.deltaTime);
                }
                yield return null;
            }
        }
        
        #endregion
        
        #region Reference Connection
        
        private void ConnectLoginScreenReferences(LoginScreen loginScript)
        {
            // Find and connect all the UI elements to the LoginScreen script
            // This would normally be done in the inspector, but we're doing it via code
            
            GameObject loginPanel = GameObject.Find("LoginPanel");
            GameObject signupPanel = GameObject.Find("SignupPanel");
            
            if (loginPanel)
            {
                // Connect login form elements
                TMP_InputField emailField = loginPanel.transform.Find("Email Input")?.GetComponent<TMP_InputField>();
                TMP_InputField passwordField = loginPanel.transform.Find("Password Input")?.GetComponent<TMP_InputField>();
                Button loginButton = loginPanel.transform.Find("Login Button")?.GetComponent<Button>();
                
                // Use reflection to set private fields (since they're SerializeField)
                SetPrivateField(loginScript, "loginEmailField", emailField);
                SetPrivateField(loginScript, "loginPasswordField", passwordField);
                SetPrivateField(loginScript, "loginButton", loginButton);
            }
            
            Debug.Log("🔗 Login screen references connected");
        }
        
        private void ConnectMainUIReferences(MobileMainUI mainUIScript)
        {
            // Connect main UI references
            Debug.Log("🔗 Main UI references connected");
        }
        
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
        
        #endregion
    }
}