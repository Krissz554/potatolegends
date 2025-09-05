using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FULLY EDITABLE Authentication Screen
    /// Customize login/signup UI completely in Inspector!
    /// </summary>
    public class EditableAuthScreen : MonoBehaviour
    {
        [Header("🔐 AUTHENTICATION SCREEN - EDIT EVERYTHING!")]
        [Space(10)]
        
        [Header("🎨 Background & Layout")]
        [Tooltip("Custom background image for auth screen")]
        public Sprite authBackgroundImage;
        
        [Tooltip("Background color tint")]
        public Color backgroundTint = Color.white;
        
        [Tooltip("Enable background parallax effect")]
        public bool enableParallaxEffect = false;
        
        [Space(10)]
        [Header("📱 Layout Areas - Drag to Reposition")]
        [Tooltip("Title area (game logo/name)")]
        public RectTransform titleArea;
        
        [Tooltip("Login form area")]
        public RectTransform loginFormArea;
        
        [Tooltip("Signup form area")]
        public RectTransform signupFormArea;
        
        [Tooltip("Social login area (Google, etc)")]
        public RectTransform socialLoginArea;
        
        [Tooltip("Footer area (terms, privacy)")]
        public RectTransform footerArea;
        
        [Space(10)]
        [Header("🎨 Visual Styling")]
        [Tooltip("Title text settings")]
        public string titleText = "🥔 POTATO LEGENDS";
        
        [Range(20, 100)]
        [Tooltip("Title font size")]
        public int titleFontSize = 48;
        
        [Tooltip("Title color")]
        public Color titleColor = new Color(1f, 0.9f, 0.3f, 1f);
        
        [Tooltip("Form panel background color")]
        public Color formPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        [Tooltip("Button color")]
        public Color buttonColor = new Color(0.2f, 0.7f, 0.2f, 1f);
        
        [Tooltip("Input field color")]
        public Color inputFieldColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        [Space(10)]
        [Header("📝 Form Settings")]
        [Range(12, 32)]
        [Tooltip("Form text size")]
        public int formTextSize = 16;
        
        [Range(14, 36)]
        [Tooltip("Button text size")]
        public int buttonTextSize = 18;
        
        [Tooltip("Show remember me checkbox")]
        public bool showRememberMe = true;
        
        [Tooltip("Show forgot password link")]
        public bool showForgotPassword = true;
        
        [Space(10)]
        [Header("🔄 Quick Actions")]
        [Tooltip("Refresh this screen with current settings")]
        public bool refreshScreen = false;
        
        [Tooltip("Reset to default layout")]
        public bool resetToDefaults = false;
        
        [Tooltip("Switch to login mode")]
        public bool showLoginMode = true;
        
        // Runtime data
        private bool isInitialized = false;
        private Canvas mainCanvas;
        
        void Start()
        {
            InitializeAuthScreen();
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
                
                ApplyVisualSettings();
            }
        }
        
        private void InitializeAuthScreen()
        {
            Debug.Log("🔐 Initializing EDITABLE Auth Screen...");
            
            // Setup canvas if needed
            SetupCanvas();
            
            // Create editable layout areas
            CreateEditableLayout();
            
            // Apply initial settings
            ApplyVisualSettings();
            
            // Create form content
            CreateAuthContent();
            
            isInitialized = true;
            Debug.Log("✅ Editable Auth Screen ready for customization!");
        }
        
        private void SetupCanvas()
        {
            mainCanvas = GetComponent<Canvas>();
            if (mainCanvas == null) mainCanvas = gameObject.AddComponent<Canvas>();
            
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 10;
            
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }
        
        private void CreateEditableLayout()
        {
            // Background
            CreateEditableBackground();
            
            // Create layout areas if they don't exist
            if (titleArea == null) titleArea = CreateEditableArea("Title Area", new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.9f));
            if (loginFormArea == null) loginFormArea = CreateEditableArea("Login Form Area", new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.65f));
            if (signupFormArea == null) signupFormArea = CreateEditableArea("Signup Form Area", new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.65f));
            if (socialLoginArea == null) socialLoginArea = CreateEditableArea("Social Login Area", new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.25f));
            if (footerArea == null) footerArea = CreateEditableArea("Footer Area", new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.12f));
            
            // Initially hide signup form
            if (signupFormArea != null) signupFormArea.gameObject.SetActive(false);
        }
        
        private void CreateEditableBackground()
        {
            GameObject bgObj = new GameObject("🎨 Editable Background");
            bgObj.transform.SetParent(transform, false);
            bgObj.layer = 5;
            
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImg = bgObj.AddComponent<Image>();
            if (authBackgroundImage != null)
            {
                bgImg.sprite = authBackgroundImage;
                bgImg.color = backgroundTint;
            }
            else
            {
                bgImg.color = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark blue fallback
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
            
            // Add visual indicator for editing
            Image areaImg = areaObj.AddComponent<Image>();
            areaImg.color = new Color(1f, 1f, 1f, 0.05f); // Very transparent white for editing visibility
            
            return areaRect;
        }
        
        private void CreateAuthContent()
        {
            // Create title
            if (titleArea != null)
            {
                CreateTitle();
            }
            
            // Create login form
            if (loginFormArea != null)
            {
                CreateLoginForm();
            }
            
            // Create signup form
            if (signupFormArea != null)
            {
                CreateSignupForm();
            }
            
            // Create social login
            if (socialLoginArea != null)
            {
                CreateSocialLogin();
            }
            
            // Create footer
            if (footerArea != null)
            {
                CreateFooter();
            }
        }
        
        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Game Title");
            titleObj.transform.SetParent(titleArea, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleTextComp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTextComp.text = titleText;
            titleTextComp.fontSize = titleFontSize;
            titleTextComp.color = titleColor;
            titleTextComp.alignment = TextAlignmentOptions.Center;
            titleTextComp.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
        }
        
        private void CreateLoginForm()
        {
            // Login panel background
            GameObject loginPanel = new GameObject("Login Panel");
            loginPanel.transform.SetParent(loginFormArea, false);
            loginPanel.layer = 5;
            
            Image loginPanelImg = loginPanel.AddComponent<Image>();
            loginPanelImg.color = formPanelColor;
            
            RectTransform loginPanelRect = loginPanel.GetComponent<RectTransform>();
            loginPanelRect.anchorMin = Vector2.zero;
            loginPanelRect.anchorMax = Vector2.one;
            loginPanelRect.offsetMin = Vector2.zero;
            loginPanelRect.offsetMax = Vector2.zero;
            
            // Email input
            CreateInputField("Email Input", "Enter your email...", loginPanel.transform, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.85f));
            
            // Password input
            CreateInputField("Password Input", "Enter your password...", loginPanel.transform, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.65f), true);
            
            // Remember me checkbox
            if (showRememberMe)
            {
                CreateCheckbox("Remember Me", loginPanel.transform, new Vector2(0.1f, 0.35f), new Vector2(0.5f, 0.45f));
            }
            
            // Forgot password link
            if (showForgotPassword)
            {
                CreateTextButton("Forgot Password?", loginPanel.transform, new Vector2(0.5f, 0.35f), new Vector2(0.9f, 0.45f), Color.cyan);
            }
            
            // Login button
            CreateButton("LOGIN", loginPanel.transform, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.3f), buttonColor, HandleLogin);
            
            // Switch to signup button
            CreateTextButton("Don't have an account? Sign Up", loginPanel.transform, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.12f), Color.white, () => SwitchToSignup());
        }
        
        private void CreateSignupForm()
        {
            // Signup panel background
            GameObject signupPanel = new GameObject("Signup Panel");
            signupPanel.transform.SetParent(signupFormArea, false);
            signupPanel.layer = 5;
            
            Image signupPanelImg = signupPanel.AddComponent<Image>();
            signupPanelImg.color = formPanelColor;
            
            RectTransform signupPanelRect = signupPanel.GetComponent<RectTransform>();
            signupPanelRect.anchorMin = Vector2.zero;
            signupPanelRect.anchorMax = Vector2.one;
            signupPanelRect.offsetMin = Vector2.zero;
            signupPanelRect.offsetMax = Vector2.zero;
            
            // Username input
            CreateInputField("Username Input", "Choose a username...", signupPanel.transform, new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.9f));
            
            // Email input
            CreateInputField("Email Input", "Enter your email...", signupPanel.transform, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.75f));
            
            // Password input
            CreateInputField("Password Input", "Create a password...", signupPanel.transform, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.6f), true);
            
            // Confirm password input
            CreateInputField("Confirm Password Input", "Confirm your password...", signupPanel.transform, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.45f), true);
            
            // Terms checkbox
            CreateCheckbox("I agree to Terms & Privacy Policy", signupPanel.transform, new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.32f));
            
            // Signup button
            CreateButton("SIGN UP", signupPanel.transform, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.2f), new Color(0.2f, 0.2f, 0.7f, 1f), HandleSignup);
            
            // Switch to login button
            CreateTextButton("Already have an account? Log In", signupPanel.transform, new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.08f), Color.white, () => SwitchToLogin());
        }
        
        private void CreateSocialLogin()
        {
            // Google login button
            CreateButton("🔗 Continue with Google", socialLoginArea, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.9f), new Color(0.8f, 0.2f, 0.2f, 1f), HandleGoogleLogin);
        }
        
        private void CreateFooter()
        {
            CreateTextButton("Terms of Service", footerArea, new Vector2(0.1f, 0.5f), new Vector2(0.45f, 0.9f), Color.gray);
            CreateTextButton("Privacy Policy", footerArea, new Vector2(0.55f, 0.5f), new Vector2(0.9f, 0.9f), Color.gray);
        }
        
        private void CreateInputField(string name, string placeholder, Transform parent, Vector2 anchorMin, Vector2 anchorMax, bool isPassword = false)
        {
            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);
            inputObj.layer = 5;
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = inputFieldColor;
            
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.placeholder = CreatePlaceholderText(placeholder, inputObj.transform);
            inputField.textComponent = CreateInputText(inputObj.transform);
            
            if (isPassword)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;
            }
        }
        
        private TextMeshProUGUI CreatePlaceholderText(string text, Transform parent)
        {
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(parent, false);
            placeholderObj.layer = 5;
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = text;
            placeholderText.fontSize = formTextSize;
            placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
            
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0.1f, 0f);
            placeholderRect.anchorMax = new Vector2(0.9f, 1f);
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            return placeholderText;
        }
        
        private TextMeshProUGUI CreateInputText(Transform parent)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            textObj.layer = 5;
            
            TextMeshProUGUI inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = formTextSize;
            inputText.color = Color.white;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0f);
            textRect.anchorMax = new Vector2(0.9f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return inputText;
        }
        
        private void CreateButton(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, System.Action onClick = null)
        {
            GameObject buttonObj = new GameObject($"Button: {text}");
            buttonObj.transform.SetParent(parent, false);
            buttonObj.layer = 5;
            
            Image buttonImg = buttonObj.AddComponent<Image>();
            buttonImg.color = color;
            
            Button button = buttonObj.AddComponent<Button>();
            if (onClick != null) button.onClick.AddListener(() => onClick());
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            // Button text
            GameObject buttonTextObj = new GameObject("Button Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            buttonTextObj.layer = 5;
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = buttonTextSize;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
        }
        
        private void CreateTextButton(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, System.Action onClick = null)
        {
            GameObject textButtonObj = new GameObject($"Text Button: {text}");
            textButtonObj.transform.SetParent(parent, false);
            textButtonObj.layer = 5;
            
            Button button = textButtonObj.AddComponent<Button>();
            button.targetGraphic = null; // No background
            if (onClick != null) button.onClick.AddListener(() => onClick());
            
            RectTransform textButtonRect = textButtonObj.GetComponent<RectTransform>();
            textButtonRect.anchorMin = anchorMin;
            textButtonRect.anchorMax = anchorMax;
            textButtonRect.offsetMin = Vector2.zero;
            textButtonRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI textButtonText = textButtonObj.AddComponent<TextMeshProUGUI>();
            textButtonText.text = text;
            textButtonText.fontSize = formTextSize - 2;
            textButtonText.color = color;
            textButtonText.alignment = TextAlignmentOptions.Center;
            textButtonText.fontStyle = FontStyles.Underline;
        }
        
        private void CreateCheckbox(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject checkboxObj = new GameObject($"Checkbox: {text}");
            checkboxObj.transform.SetParent(parent, false);
            checkboxObj.layer = 5;
            
            Toggle checkbox = checkboxObj.AddComponent<Toggle>();
            
            RectTransform checkboxRect = checkboxObj.GetComponent<RectTransform>();
            checkboxRect.anchorMin = anchorMin;
            checkboxRect.anchorMax = anchorMax;
            checkboxRect.offsetMin = Vector2.zero;
            checkboxRect.offsetMax = Vector2.zero;
            
            // Checkbox background
            Image checkboxBg = checkboxObj.AddComponent<Image>();
            checkboxBg.color = inputFieldColor;
            
            // Checkbox label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(checkboxObj.transform, false);
            labelObj.layer = 5;
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = text;
            labelText.fontSize = formTextSize - 2;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.15f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            checkbox.targetGraphic = checkboxBg;
        }
        
        public void RefreshScreen()
        {
            Debug.Log("🔄 Refreshing editable auth screen...");
            ApplyVisualSettings();
            // Recreate content if needed
        }
        
        public void ResetToDefaults()
        {
            Debug.Log("🔄 Resetting auth screen to defaults...");
            
            // Reset to default positions
            if (titleArea != null)
            {
                titleArea.anchorMin = new Vector2(0.1f, 0.7f);
                titleArea.anchorMax = new Vector2(0.9f, 0.9f);
            }
            
            if (loginFormArea != null)
            {
                loginFormArea.anchorMin = new Vector2(0.2f, 0.3f);
                loginFormArea.anchorMax = new Vector2(0.8f, 0.65f);
            }
            
            // Reset visual settings
            titleFontSize = 48;
            titleColor = new Color(1f, 0.9f, 0.3f, 1f);
            formPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            buttonColor = new Color(0.2f, 0.7f, 0.2f, 1f);
            
            ApplyVisualSettings();
        }
        
        private void ApplyVisualSettings()
        {
            // Apply title settings
            TextMeshProUGUI[] titles = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var title in titles)
            {
                if (title.name == "Game Title")
                {
                    title.text = titleText;
                    title.fontSize = titleFontSize;
                    title.color = titleColor;
                }
            }
            
            // Apply form panel colors
            Image[] panels = GetComponentsInChildren<Image>();
            foreach (var panel in panels)
            {
                if (panel.name.Contains("Panel"))
                {
                    panel.color = formPanelColor;
                }
            }
            
            // Apply button colors
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                if (button.name.Contains("LOGIN") || button.name.Contains("SIGN UP"))
                {
                    button.GetComponent<Image>().color = buttonColor;
                }
            }
        }
        
        private void SwitchToLogin()
        {
            showLoginMode = true;
            if (loginFormArea != null) loginFormArea.gameObject.SetActive(true);
            if (signupFormArea != null) signupFormArea.gameObject.SetActive(false);
            Debug.Log("🔄 Switched to login mode");
        }
        
        private void SwitchToSignup()
        {
            showLoginMode = false;
            if (loginFormArea != null) loginFormArea.gameObject.SetActive(false);
            if (signupFormArea != null) signupFormArea.gameObject.SetActive(true);
            Debug.Log("🔄 Switched to signup mode");
        }
        
        private async void HandleLogin()
        {
            Debug.Log("🔐 Handling login...");
            
            // Get input values
            TMP_InputField[] inputs = loginFormArea.GetComponentsInChildren<TMP_InputField>();
            string email = "";
            string password = "";
            
            foreach (var input in inputs)
            {
                if (input.name.Contains("Email"))
                    email = input.text;
                else if (input.name.Contains("Password"))
                    password = input.text;
            }
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("⚠️ Please fill in all fields");
                return;
            }
            
            // Authenticate with Supabase
            bool success = await RealSupabaseClient.Instance.SignIn(email, password);
            
            if (success)
            {
                Debug.Log("✅ Login successful!");
                // Navigate to main menu
                EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                if (uiManager != null)
                {
                    uiManager.GoToMainMenu();
                }
            }
            else
            {
                Debug.LogError("❌ Login failed!");
            }
        }
        
        private async void HandleSignup()
        {
            Debug.Log("📝 Handling signup...");
            
            // Get input values from signup form
            TMP_InputField[] inputs = signupFormArea.GetComponentsInChildren<TMP_InputField>();
            string username = "";
            string email = "";
            string password = "";
            string confirmPassword = "";
            
            foreach (var input in inputs)
            {
                if (input.name.Contains("Username"))
                    username = input.text;
                else if (input.name.Contains("Email"))
                    email = input.text;
                else if (input.name.Contains("Password") && !input.name.Contains("Confirm"))
                    password = input.text;
                else if (input.name.Contains("Confirm"))
                    confirmPassword = input.text;
            }
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                Debug.LogWarning("⚠️ Please check all fields and ensure passwords match");
                return;
            }
            
            // Sign up with Supabase
            bool success = await RealSupabaseClient.Instance.SignUp(email, password);
            
            if (success)
            {
                Debug.Log("✅ Signup successful!");
                // Navigate to main menu
                EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                if (uiManager != null)
                {
                    uiManager.GoToMainMenu();
                }
            }
            else
            {
                Debug.LogError("❌ Signup failed!");
            }
        }
        
        private void HandleGoogleLogin()
        {
            Debug.Log("🔗 Google login clicked - TODO: Implement Google OAuth");
        }
    }
}