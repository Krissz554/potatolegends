using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Mobile login screen that appears when game starts
    /// Handles user authentication with Supabase backend
    /// </summary>
    public class LoginScreen : MonoBehaviour
    {
        [Header("Login Panel")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private TMP_InputField loginEmailField;
        [SerializeField] private TMP_InputField loginPasswordField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button switchToSignupButton;
        
        [Header("Signup Panel")]
        [SerializeField] private GameObject signupPanel;
        [SerializeField] private TMP_InputField signupEmailField;
        [SerializeField] private TMP_InputField signupPasswordField;
        [SerializeField] private TMP_InputField signupDisplayNameField;
        [SerializeField] private Button signupButton;
        [SerializeField] private Button switchToLoginButton;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private Button guestButton; // Optional guest mode
        
        [Header("Main Game UI")]
        [SerializeField] private GameObject mainGameUI;
        
        private bool isLoginMode = true;
        private bool isLoading = false;
        
        private void Start()
        {
            SetupUI();
            CheckExistingAuth();
        }
        
        private void SetupUI()
        {
            // Setup button listeners
            if (loginButton) loginButton.onClick.AddListener(OnLoginButtonPressed);
            if (signupButton) signupButton.onClick.AddListener(OnSignupButtonPressed);
            if (switchToSignupButton) switchToSignupButton.onClick.AddListener(() => SwitchMode(false));
            if (switchToLoginButton) switchToLoginButton.onClick.AddListener(() => SwitchMode(true));
            if (guestButton) guestButton.onClick.AddListener(OnGuestButtonPressed);
            
            // Setup initial UI state
            SwitchMode(true); // Start with login mode
            SetLoading(false);
            
            // Subscribe to authentication events
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged += OnAuthenticationChanged;
                SupabaseClient.Instance.OnError += OnAuthenticationError;
            }
            
            // Set title
            if (titleText) titleText.text = "What's My Potato?";
            if (statusText) statusText.text = "Welcome! Please sign in to continue.";
        }
        
        private void CheckExistingAuth()
        {
            // Check if user is already logged in
            if (SupabaseClient.Instance != null && SupabaseClient.Instance.IsAuthenticated)
            {
                Debug.Log("🔐 User already authenticated, proceeding to main game");
                ProceedToMainGame();
            }
        }
        
        private void SwitchMode(bool loginMode)
        {
            isLoginMode = loginMode;
            
            if (loginPanel) loginPanel.SetActive(loginMode);
            if (signupPanel) signupPanel.SetActive(!loginMode);
            
            if (statusText)
            {
                statusText.text = loginMode ? 
                    "Welcome back! Sign in to continue." : 
                    "Join the potato adventure! Create your account.";
            }
        }
        
        private async void OnLoginButtonPressed()
        {
            if (isLoading) return;
            
            string email = loginEmailField?.text?.Trim() ?? "";
            string password = loginPasswordField?.text ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowStatus("Please enter both email and password.", Color.red);
                return;
            }
            
            SetLoading(true);
            ShowStatus("Signing in...", Color.yellow);
            
            bool success = await SupabaseClient.Instance.SignIn(email, password);
            
            if (success)
            {
                ShowStatus("Sign in successful!", Color.green);
                // Authentication changed event will handle the transition
            }
            else
            {
                SetLoading(false);
                ShowStatus("Sign in failed. Please check your credentials.", Color.red);
            }
        }
        
        private async void OnSignupButtonPressed()
        {
            if (isLoading) return;
            
            string email = signupEmailField?.text?.Trim() ?? "";
            string password = signupPasswordField?.text ?? "";
            string displayName = signupDisplayNameField?.text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowStatus("Please enter both email and password.", Color.red);
                return;
            }
            
            if (password.Length < 6)
            {
                ShowStatus("Password must be at least 6 characters.", Color.red);
                return;
            }
            
            SetLoading(true);
            ShowStatus("Creating account...", Color.yellow);
            
            bool success = await SupabaseClient.Instance.SignUp(email, password, displayName);
            
            if (success)
            {
                ShowStatus("Account created! Please check your email for confirmation.", Color.green);
                SwitchMode(true); // Switch back to login
                
                // Pre-fill login email
                if (loginEmailField) loginEmailField.text = email;
            }
            else
            {
                ShowStatus("Sign up failed. Please try again.", Color.red);
            }
            
            SetLoading(false);
        }
        
        private void OnGuestButtonPressed()
        {
            // TODO: Implement guest mode if needed
            ShowStatus("Guest mode not implemented yet.", Color.yellow);
        }
        
        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                Debug.Log("✅ Authentication successful, transitioning to main game");
                StartCoroutine(TransitionToMainGame());
            }
            else
            {
                Debug.Log("❌ Authentication lost, returning to login");
                gameObject.SetActive(true);
                if (mainGameUI) mainGameUI.SetActive(false);
            }
        }
        
        private void OnAuthenticationError(string error)
        {
            SetLoading(false);
            ShowStatus($"Error: {error}", Color.red);
        }
        
        private IEnumerator TransitionToMainGame()
        {
            yield return new WaitForSeconds(1f); // Brief pause to show success message
            ProceedToMainGame();
        }
        
        private void ProceedToMainGame()
        {
            // Hide login screen
            gameObject.SetActive(false);
            
            // Show main game UI
            if (mainGameUI) mainGameUI.SetActive(true);
            
            // Change game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameManager.GameState.MainMenu);
            }
            
            Debug.Log("🎮 Transitioned to main game UI");
        }
        
        private void SetLoading(bool loading)
        {
            isLoading = loading;
            
            if (loadingIndicator) loadingIndicator.SetActive(loading);
            if (loginButton) loginButton.interactable = !loading;
            if (signupButton) signupButton.interactable = !loading;
            if (switchToSignupButton) switchToSignupButton.interactable = !loading;
            if (switchToLoginButton) switchToLoginButton.interactable = !loading;
        }
        
        private void ShowStatus(string message, Color color)
        {
            if (statusText)
            {
                statusText.text = message;
                statusText.color = color;
            }
            
            Debug.Log($"📱 Login Status: {message}");
        }
        
        private void ClearForms()
        {
            if (loginEmailField) loginEmailField.text = "";
            if (loginPasswordField) loginPasswordField.text = "";
            if (signupEmailField) signupEmailField.text = "";
            if (signupPasswordField) signupPasswordField.text = "";
            if (signupDisplayNameField) signupDisplayNameField.text = "";
        }
        
        private void OnDestroy()
        {
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged -= OnAuthenticationChanged;
                SupabaseClient.Instance.OnError -= OnAuthenticationError;
            }
        }
        
        // Public method for logout
        public void ShowLoginScreen()
        {
            gameObject.SetActive(true);
            if (mainGameUI) mainGameUI.SetActive(false);
            ClearForms();
            SetLoading(false);
            ShowStatus("Please sign in to continue.", Color.white);
        }
    }
}