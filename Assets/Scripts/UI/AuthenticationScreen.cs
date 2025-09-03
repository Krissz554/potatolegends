using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Authentication screen that appears first when game loads
    /// Handles login and registration exactly like the web version
    /// </summary>
    public class AuthenticationScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private TMP_InputField loginEmailField;
        [SerializeField] private TMP_InputField loginPasswordField;
        [SerializeField] private TMP_InputField registerEmailField;
        [SerializeField] private TMP_InputField registerPasswordField;
        [SerializeField] private TMP_InputField registerDisplayNameField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button switchToRegisterButton;
        [SerializeField] private Button switchToLoginButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject loadingIndicator;
        
        private bool isLoginMode = true;
        private bool isProcessing = false;
        
        private void Start()
        {
            CreateAuthenticationUI();
            SetupEventListeners();
        }
        
        private void CreateAuthenticationUI()
        {
            Debug.Log("🔐 Creating authentication UI...");
            
            // Create main canvas if needed
            CreateMainCanvas();
            
            // Create authentication screen
            CreateAuthScreen();
            
            Debug.Log("✅ Authentication UI created");
        }
        
        private void CreateMainCanvas()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Auth Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create EventSystem
            CreateEventSystem();
        }
        
        private void CreateEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
                Debug.Log("✅ EventSystem created");
            }
        }
        
        private void CreateAuthScreen()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            
            // Background
            GameObject background = CreatePanel("Auth Background", canvas.transform);
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.05f, 0.2f, 1f); // Dark purple
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Title
            CreateTitle(background.transform);
            
            // Login panel (active by default)
            loginPanel = CreateLoginPanel(background.transform);
            
            // Register panel (inactive by default)
            registerPanel = CreateRegisterPanel(background.transform);
            registerPanel.SetActive(false);
            
            // Status text
            CreateStatusText(background.transform);
            
            // Loading indicator
            CreateLoadingIndicator(background.transform);
        }
        
        private void CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "WHAT'S MY POTATO?";
            titleText.fontSize = 64;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
        }
        
        private GameObject CreateLoginPanel(Transform parent)
        {
            GameObject panel = CreatePanel("Login Panel", parent);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);
            
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.35f);
            panelRect.anchorMax = new Vector2(0.9f, 0.7f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Welcome text
            GameObject welcomeObj = new GameObject("Welcome Text");
            welcomeObj.transform.SetParent(panel.transform, false);
            welcomeObj.layer = 5;
            
            TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
            welcomeText.text = "Welcome Back!";
            welcomeText.fontSize = 36;
            welcomeText.color = Color.white;
            welcomeText.alignment = TextAlignmentOptions.Center;
            welcomeText.fontStyle = FontStyles.Bold;
            
            RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
            welcomeRect.anchorMin = new Vector2(0.1f, 0.8f);
            welcomeRect.anchorMax = new Vector2(0.9f, 0.95f);
            welcomeRect.offsetMin = Vector2.zero;
            welcomeRect.offsetMax = Vector2.zero;
            
            // Email input
            loginEmailField = CreateInputField(panel.transform, "Email", 
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.7f));
            
            // Password input
            loginPasswordField = CreateInputField(panel.transform, "Password", 
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.5f), true);
            
            // Login button
            loginButton = CreateButton(panel.transform, "Login Button", "SIGN IN",
                new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.3f), new Color(0.2f, 0.6f, 0.9f, 1f));
            
            // Switch to register button
            switchToRegisterButton = CreateTextButton(panel.transform, "Switch to Register",
                "Don't have an account? Sign Up",
                new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.12f));
            
            return panel;
        }
        
        private GameObject CreateRegisterPanel(Transform parent)
        {
            GameObject panel = CreatePanel("Register Panel", parent);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);
            
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.3f);
            panelRect.anchorMax = new Vector2(0.9f, 0.75f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Welcome text
            GameObject welcomeObj = new GameObject("Welcome Text");
            welcomeObj.transform.SetParent(panel.transform, false);
            welcomeObj.layer = 5;
            
            TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
            welcomeText.text = "Join the Potato Army!";
            welcomeText.fontSize = 36;
            welcomeText.color = Color.white;
            welcomeText.alignment = TextAlignmentOptions.Center;
            welcomeText.fontStyle = FontStyles.Bold;
            
            RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
            welcomeRect.anchorMin = new Vector2(0.1f, 0.85f);
            welcomeRect.anchorMax = new Vector2(0.9f, 0.98f);
            welcomeRect.offsetMin = Vector2.zero;
            welcomeRect.offsetMax = Vector2.zero;
            
            // Display name input
            registerDisplayNameField = CreateInputField(panel.transform, "Display Name", 
                new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.82f));
            
            // Email input
            registerEmailField = CreateInputField(panel.transform, "Email", 
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.67f));
            
            // Password input
            registerPasswordField = CreateInputField(panel.transform, "Password", 
                new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.52f), true);
            
            // Register button
            registerButton = CreateButton(panel.transform, "Register Button", "CREATE ACCOUNT",
                new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.35f), new Color(0.2f, 0.8f, 0.3f, 1f));
            
            // Switch to login button
            switchToLoginButton = CreateTextButton(panel.transform, "Switch to Login",
                "Already have an account? Sign In",
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.15f));
            
            return panel;
        }
        
        private void CreateStatusText(Transform parent)
        {
            GameObject statusObj = new GameObject("Status Text");
            statusObj.transform.SetParent(parent, false);
            statusObj.layer = 5;
            
            statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Please sign in to enter the Potato Card Arena";
            statusText.fontSize = 28;
            statusText.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            statusText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.1f, 0.25f);
            statusRect.anchorMax = new Vector2(0.9f, 0.32f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
        }
        
        private void CreateLoadingIndicator(Transform parent)
        {
            loadingIndicator = new GameObject("Loading Indicator");
            loadingIndicator.transform.SetParent(parent, false);
            loadingIndicator.layer = 5;
            
            TextMeshProUGUI loadingText = loadingIndicator.AddComponent<TextMeshProUGUI>();
            loadingText.text = "🔄 Processing...";
            loadingText.fontSize = 32;
            loadingText.color = new Color(1f, 0.8f, 0.2f, 1f);
            loadingText.alignment = TextAlignmentOptions.Center;
            loadingText.fontStyle = FontStyles.Bold;
            
            RectTransform loadingRect = loadingIndicator.GetComponent<RectTransform>();
            loadingRect.anchorMin = new Vector2(0.2f, 0.15f);
            loadingRect.anchorMax = new Vector2(0.8f, 0.25f);
            loadingRect.offsetMin = Vector2.zero;
            loadingRect.offsetMax = Vector2.zero;
            
            loadingIndicator.SetActive(false);
            
            // Add spinning animation
            StartCoroutine(AnimateLoadingSpinner(loadingText));
        }
        
        private void SetupEventListeners()
        {
            if (loginButton) loginButton.onClick.AddListener(OnLoginPressed);
            if (registerButton) registerButton.onClick.AddListener(OnRegisterPressed);
            if (switchToRegisterButton) switchToRegisterButton.onClick.AddListener(() => SwitchMode(false));
            if (switchToLoginButton) switchToLoginButton.onClick.AddListener(() => SwitchMode(true));
            
            // Subscribe to authentication events
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged += OnAuthenticationSuccess;
                SupabaseClient.Instance.OnError += OnAuthenticationError;
            }
        }
        
        private void SwitchMode(bool loginMode)
        {
            isLoginMode = loginMode;
            
            if (loginPanel) loginPanel.SetActive(loginMode);
            if (registerPanel) registerPanel.SetActive(!loginMode);
            
            if (statusText)
            {
                statusText.text = loginMode ? 
                    "Welcome back to the Potato Card Arena!" : 
                    "Join thousands of potato warriors!";
            }
            
            Debug.Log($"🔄 Switched to {(loginMode ? "login" : "register")} mode");
        }
        
        private async void OnLoginPressed()
        {
            if (isProcessing) return;
            
            string email = loginEmailField?.text?.Trim() ?? "";
            string password = loginPasswordField?.text ?? "";
            
            if (!ValidateLoginInput(email, password)) return;
            
            SetProcessing(true, "Signing in...");
            
            bool success = await SupabaseClient.Instance.SignIn(email, password);
            
            if (!success)
            {
                SetProcessing(false);
                ShowError("Sign in failed. Please check your credentials.");
            }
            // Success handled by OnAuthenticationSuccess event
        }
        
        private async void OnRegisterPressed()
        {
            if (isProcessing) return;
            
            string email = registerEmailField?.text?.Trim() ?? "";
            string password = registerPasswordField?.text ?? "";
            string displayName = registerDisplayNameField?.text?.Trim() ?? "";
            
            if (!ValidateRegisterInput(email, password, displayName)) return;
            
            SetProcessing(true, "Creating account...");
            
            bool success = await SupabaseClient.Instance.SignUp(email, password, displayName);
            
            if (success)
            {
                SetProcessing(false);
                ShowSuccess("Account created! Please check your email for confirmation, then sign in.");
                SwitchMode(true); // Switch to login mode
                
                // Pre-fill email
                if (loginEmailField) loginEmailField.text = email;
            }
            else
            {
                SetProcessing(false);
                ShowError("Registration failed. Please try again.");
            }
        }
        
        private bool ValidateLoginInput(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both email and password");
                return false;
            }
            
            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                return false;
            }
            
            return true;
        }
        
        private bool ValidateRegisterInput(string email, string password, string displayName)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both email and password");
                return false;
            }
            
            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                return false;
            }
            
            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return false;
            }
            
            return true;
        }
        
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        private void OnAuthenticationSuccess(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                SetProcessing(false);
                ShowSuccess("Welcome to the Potato Card Arena!");
                
                // Transition to main game
                StartCoroutine(TransitionToMainGame());
            }
        }
        
        private void OnAuthenticationError(string error)
        {
            SetProcessing(false);
            ShowError($"Authentication error: {error}");
        }
        
        private IEnumerator TransitionToMainGame()
        {
            yield return new WaitForSeconds(1f);
            
            // Hide this screen
            gameObject.SetActive(false);
            
            // Notify game flow manager
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.ChangeGameFlow(GameFlowManager.GameFlow.MainMenu);
            }
        }
        
        private void SetProcessing(bool processing, string message = "")
        {
            isProcessing = processing;
            
            if (loadingIndicator) loadingIndicator.SetActive(processing);
            
            if (loginButton) loginButton.interactable = !processing;
            if (registerButton) registerButton.interactable = !processing;
            if (switchToRegisterButton) switchToRegisterButton.interactable = !processing;
            if (switchToLoginButton) switchToLoginButton.interactable = !processing;
            
            if (!string.IsNullOrEmpty(message) && statusText)
            {
                statusText.text = message;
                statusText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            }
        }
        
        private void ShowError(string message)
        {
            if (statusText)
            {
                statusText.text = message;
                statusText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red
            }
            
            Debug.LogWarning($"❌ Auth Error: {message}");
        }
        
        private void ShowSuccess(string message)
        {
            if (statusText)
            {
                statusText.text = message;
                statusText.color = new Color(0.3f, 1f, 0.3f, 1f); // Green
            }
            
            Debug.Log($"✅ Auth Success: {message}");
        }
        
        #region UI Creation Helpers
        
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
        
        private TMP_InputField CreateInputField(Transform parent, string placeholder, Vector2 anchorMin, Vector2 anchorMax, bool isPassword = false)
        {
            GameObject inputObj = new GameObject($"{placeholder} Input");
            inputObj.transform.SetParent(parent, false);
            inputObj.layer = 5;
            
            // Background
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(1f, 1f, 1f, 0.9f);
            
            // Input field
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            if (isPassword) inputField.contentType = TMP_InputField.ContentType.Password;
            
            // Text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            textArea.layer = 5;
            textArea.AddComponent<RectMask2D>();
            
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = new Vector2(0.05f, 0f);
            textAreaRect.anchorMax = new Vector2(0.95f, 1f);
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;
            
            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            placeholderObj.layer = 5;
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = $"Enter your {placeholder.ToLower()}";
            placeholderText.fontSize = 24;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.raycastTarget = false;
            
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());
            
            // Input text
            GameObject inputTextObj = new GameObject("Input Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 24;
            inputText.color = Color.black;
            inputText.raycastTarget = false;
            
            SetFullScreen(inputTextObj.GetComponent<RectTransform>());
            
            // Connect components
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            // Position
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            
            return inputField;
        }
        
        private Button CreateButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject buttonObj = CreatePanel(name, parent);
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = color;
            
            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 32;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
            
            // Position
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        private Button CreateTextButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            buttonObj.layer = 5;
            
            Button button = buttonObj.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            
            TextMeshProUGUI buttonText = buttonObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 22;
            buttonText.color = new Color(0.4f, 0.8f, 1f, 1f); // Light blue
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Underline;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        private void SetFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        private IEnumerator AnimateLoadingSpinner(TextMeshProUGUI loadingText)
        {
            string[] spinnerFrames = { "🔄", "🔃", "🔄", "🔃" };
            int frameIndex = 0;
            
            while (loadingText != null)
            {
                if (loadingIndicator.activeInHierarchy)
                {
                    loadingText.text = $"{spinnerFrames[frameIndex]} Processing...";
                    frameIndex = (frameIndex + 1) % spinnerFrames.Length;
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged -= OnAuthenticationSuccess;
                SupabaseClient.Instance.OnError -= OnAuthenticationError;
            }
        }
    }
}