using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// REAL Authentication Screen that actually connects to Supabase
    /// Handles real login/signup with your database
    /// </summary>
    public class RealAuthScreen : MonoBehaviour
    {
        [Header("Authentication UI")]
        [SerializeField] private TMP_InputField emailField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private TMP_InputField confirmPasswordField;
        [SerializeField] private TMP_InputField displayNameField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button signupButton;
        [SerializeField] private Button switchModeButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private GameObject signupFields;
        
        // State
        private bool isSignupMode = false;
        private bool isProcessing = false;
        
        private void Start()
        {
            CreateAuthUI();
            SetupEventListeners();
            
            // Show by default when game starts
            gameObject.SetActive(true);
        }
        
        private void CreateAuthUI()
        {
            Debug.Log("🔐 Creating real authentication UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            CreateAuthInterface(canvas.transform);
            
            Debug.Log("✅ Real authentication UI created");
        }
        
        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Auth Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if it doesn't exist (USE NEW INPUT SYSTEM)
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
                // Use NEW Input System (recommended for Unity 6)
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("✅ Using NEW Input System UI module (recommended)");
            }
        }
        
        private void CreateAuthInterface(Transform parent)
        {
            // Background
            GameObject background = CreatePanel("Auth Background", parent);
            Image bgImage = background.GetComponent<Image>();
            
            // Beautiful gradient background
            bgImage.color = new Color(0.05f, 0.1f, 0.2f, 1f); // Dark blue
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Auth panel
            GameObject authPanel = CreatePanel("Auth Panel", background.transform);
            Image authBg = authPanel.GetComponent<Image>();
            authBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform authRect = authPanel.GetComponent<RectTransform>();
            authRect.anchorMin = new Vector2(0.1f, 0.25f);
            authRect.anchorMax = new Vector2(0.9f, 0.75f);
            authRect.offsetMin = Vector2.zero;
            authRect.offsetMax = Vector2.zero;
            
            // Game title
            GameObject titleObj = new GameObject("Game Title");
            titleObj.transform.SetParent(authPanel.transform, false);
            titleObj.layer = 5;
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🥔 POTATO LEGENDS\nMOBILE CARD GAME";
            titleText.fontSize = 36;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Email field
            emailField = CreateInputField(authPanel.transform, "Email", "Enter your email...", 
                new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.75f));
            
            // Password field
            passwordField = CreateInputField(authPanel.transform, "Password", "Enter your password...", 
                new Vector2(0.1f, 0.52f), new Vector2(0.9f, 0.62f));
            passwordField.inputType = TMP_InputField.InputType.Password;
            
            // Signup fields (hidden by default)
            signupFields = new GameObject("Signup Fields");
            signupFields.transform.SetParent(authPanel.transform, false);
            signupFields.layer = 5;
            signupFields.SetActive(false);
            
            RectTransform signupRect = signupFields.GetComponent<RectTransform>();
            signupRect.anchorMin = new Vector2(0.1f, 0.26f);
            signupRect.anchorMax = new Vector2(0.9f, 0.5f);
            signupRect.offsetMin = Vector2.zero;
            signupRect.offsetMax = Vector2.zero;
            
            // Confirm password field (signup only)
            confirmPasswordField = CreateInputField(signupFields.transform, "Confirm Password", "Confirm your password...", 
                new Vector2(0f, 0.5f), new Vector2(1f, 0.8f));
            confirmPasswordField.inputType = TMP_InputField.InputType.Password;
            
            // Display name field (signup only)
            displayNameField = CreateInputField(signupFields.transform, "Display Name", "Your display name (optional)...", 
                new Vector2(0f, 0.1f), new Vector2(1f, 0.4f));
            
            // Login button
            GameObject loginBtnObj = CreatePanel("Login Button", authPanel.transform);
            loginButton = loginBtnObj.AddComponent<Button>();
            Image loginBtnImg = loginBtnObj.GetComponent<Image>();
            loginBtnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green
            
            GameObject loginTextObj = new GameObject("Login Text");
            loginTextObj.transform.SetParent(loginBtnObj.transform, false);
            loginTextObj.layer = 5;
            
            TextMeshProUGUI loginText = loginTextObj.AddComponent<TextMeshProUGUI>();
            loginText.text = "🔐 LOGIN";
            loginText.fontSize = 28;
            loginText.color = Color.white;
            loginText.alignment = TextAlignmentOptions.Center;
            loginText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(loginTextObj.GetComponent<RectTransform>());
            
            RectTransform loginBtnRect = loginBtnObj.GetComponent<RectTransform>();
            loginBtnRect.anchorMin = new Vector2(0.1f, 0.35f);
            loginBtnRect.anchorMax = new Vector2(0.9f, 0.48f);
            loginBtnRect.offsetMin = Vector2.zero;
            loginBtnRect.offsetMax = Vector2.zero;
            
            // Switch mode button
            GameObject switchBtnObj = CreatePanel("Switch Mode Button", authPanel.transform);
            switchModeButton = switchBtnObj.AddComponent<Button>();
            Image switchBtnImg = switchBtnObj.GetComponent<Image>();
            switchBtnImg.color = new Color(0.3f, 0.6f, 0.9f, 1f); // Blue
            
            GameObject switchTextObj = new GameObject("Switch Text");
            switchTextObj.transform.SetParent(switchBtnObj.transform, false);
            switchTextObj.layer = 5;
            
            TextMeshProUGUI switchText = switchTextObj.AddComponent<TextMeshProUGUI>();
            switchText.text = "📝 CREATE ACCOUNT";
            switchText.fontSize = 20;
            switchText.color = Color.white;
            switchText.alignment = TextAlignmentOptions.Center;
            
            SetFullScreen(switchTextObj.GetComponent<RectTransform>());
            
            RectTransform switchBtnRect = switchBtnObj.GetComponent<RectTransform>();
            switchBtnRect.anchorMin = new Vector2(0.1f, 0.22f);
            switchBtnRect.anchorMax = new Vector2(0.9f, 0.32f);
            switchBtnRect.offsetMin = Vector2.zero;
            switchBtnRect.offsetMax = Vector2.zero;
            
            // Status text
            GameObject statusObj = new GameObject("Status Text");
            statusObj.transform.SetParent(authPanel.transform, false);
            statusObj.layer = 5;
            
            statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Enter your credentials to play!";
            statusText.fontSize = 18;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.1f, 0.05f);
            statusRect.anchorMax = new Vector2(0.9f, 0.18f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
        }
        
        private TMP_InputField CreateInputField(Transform parent, string fieldName, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject fieldObj = CreatePanel($"{fieldName} Field", parent);
            fieldObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            
            RectTransform fieldRect = fieldObj.GetComponent<RectTransform>();
            fieldRect.anchorMin = anchorMin;
            fieldRect.anchorMax = anchorMax;
            fieldRect.offsetMin = Vector2.zero;
            fieldRect.offsetMax = Vector2.zero;
            
            TMP_InputField inputField = fieldObj.AddComponent<TMP_InputField>();
            
            // Text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(fieldObj.transform, false);
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
            placeholderText.text = placeholder;
            placeholderText.fontSize = 18;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.raycastTarget = false;
            
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());
            
            // Input text
            GameObject inputTextObj = new GameObject("Input Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
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
        
        private void SetupEventListeners()
        {
            // UI events
            if (loginButton) loginButton.onClick.AddListener(OnLoginPressed);
            if (signupButton) signupButton.onClick.AddListener(OnSignupPressed);
            if (switchModeButton) switchModeButton.onClick.AddListener(OnSwitchModePressed);
            
            // Supabase events
            if (RealSupabaseClient.Instance != null)
            {
                RealSupabaseClient.Instance.OnAuthenticationChanged += OnAuthChanged;
                RealSupabaseClient.Instance.OnError += OnAuthError;
            }
            
            // Game flow events
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged += OnGameFlowChanged;
            }
        }
        
        private async void OnLoginPressed()
        {
            if (isProcessing) return;
            
            string email = emailField?.text?.Trim() ?? "";
            string password = passwordField?.text ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                UpdateStatus("❌ Please enter email and password", Color.red);
                return;
            }
            
            isProcessing = true;
            UpdateStatus("🔄 Signing in...", Color.yellow);
            
            try
            {
                bool success = await RealSupabaseClient.Instance.SignIn(email, password);
                
                if (success)
                {
                    UpdateStatus("✅ Login successful!", Color.green);
                    
                    // Navigate to main menu
                    if (GameFlowManager.Instance != null)
                    {
                        GameFlowManager.Instance.NavigateToMainMenu();
                    }
                }
                else
                {
                    UpdateStatus("❌ Invalid email or password", Color.red);
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Login error: {e.Message}");
                UpdateStatus("❌ Login failed. Please try again.", Color.red);
            }
            
            isProcessing = false;
        }
        
        private async void OnSignupPressed()
        {
            if (isProcessing) return;
            
            string email = emailField?.text?.Trim() ?? "";
            string password = passwordField?.text ?? "";
            string confirmPassword = confirmPasswordField?.text ?? "";
            string displayName = displayNameField?.text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                UpdateStatus("❌ Please enter email and password", Color.red);
                return;
            }
            
            if (password != confirmPassword)
            {
                UpdateStatus("❌ Passwords don't match", Color.red);
                return;
            }
            
            if (password.Length < 6)
            {
                UpdateStatus("❌ Password must be at least 6 characters", Color.red);
                return;
            }
            
            isProcessing = true;
            UpdateStatus("📝 Creating account...", Color.yellow);
            
            try
            {
                bool success = await RealSupabaseClient.Instance.SignUp(email, password, displayName);
                
                if (success)
                {
                    UpdateStatus("✅ Account created! Please check your email to verify.", Color.green);
                    
                    // Switch back to login mode
                    SwitchToLoginMode();
                }
                else
                {
                    UpdateStatus("❌ Failed to create account", Color.red);
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Signup error: {e.Message}");
                UpdateStatus("❌ Signup failed. Please try again.", Color.red);
            }
            
            isProcessing = false;
        }
        
        private void OnSwitchModePressed()
        {
            if (isSignupMode)
            {
                SwitchToLoginMode();
            }
            else
            {
                SwitchToSignupMode();
            }
        }
        
        private void SwitchToLoginMode()
        {
            isSignupMode = false;
            
            if (titleText) titleText.text = "🥔 POTATO LEGENDS\nMOBILE CARD GAME";
            if (signupFields) signupFields.SetActive(false);
            if (loginButton) loginButton.gameObject.SetActive(true);
            if (signupButton) signupButton.gameObject.SetActive(false);
            
            // Update switch button
            if (switchModeButton)
            {
                var switchText = switchModeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (switchText) switchText.text = "📝 CREATE ACCOUNT";
            }
            
            UpdateStatus("🔐 Enter your credentials to play!", Color.white);
            
            Debug.Log("🔄 Switched to login mode");
        }
        
        private void SwitchToSignupMode()
        {
            isSignupMode = true;
            
            if (titleText) titleText.text = "📝 CREATE ACCOUNT";
            if (signupFields) signupFields.SetActive(true);
            if (loginButton) loginButton.gameObject.SetActive(false);
            if (signupButton) signupButton.gameObject.SetActive(true);
            
            // Update switch button
            if (switchModeButton)
            {
                var switchText = switchModeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (switchText) switchText.text = "🔐 BACK TO LOGIN";
            }
            
            UpdateStatus("📝 Create your account to start playing!", Color.white);
            
            Debug.Log("🔄 Switched to signup mode");
        }
        
        private void UpdateStatus(string message, Color color)
        {
            if (statusText)
            {
                statusText.text = message;
                statusText.color = color;
            }
            
            Debug.Log($"📱 Auth Status: {message}");
        }
        
        private void OnAuthChanged(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                Debug.Log("✅ Authentication successful - hiding auth screen");
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("🔐 User logged out - showing auth screen");
                gameObject.SetActive(true);
                ClearFields();
            }
        }
        
        private void OnAuthError(string error)
        {
            UpdateStatus($"❌ {error}", Color.red);
            isProcessing = false;
        }
        
        private void ClearFields()
        {
            if (emailField) emailField.text = "";
            if (passwordField) passwordField.text = "";
            if (confirmPasswordField) confirmPasswordField.text = "";
            if (displayNameField) displayNameField.text = "";
            
            SwitchToLoginMode();
        }
        
        private void OnGameFlowChanged(GameFlowManager.GameFlow newFlow)
        {
            bool shouldShow = newFlow == GameFlowManager.GameFlow.Authentication;
            gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                ClearFields();
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
            if (RealSupabaseClient.Instance != null)
            {
                RealSupabaseClient.Instance.OnAuthenticationChanged -= OnAuthChanged;
                RealSupabaseClient.Instance.OnError -= OnAuthError;
            }
            
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged -= OnGameFlowChanged;
            }
        }
    }
}