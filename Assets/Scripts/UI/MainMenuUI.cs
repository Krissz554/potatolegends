using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Core;
using PotatoCardGame.Network;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Main menu UI controller for the mobile game
    /// Handles navigation between different game modes and authentication
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject authPanel;
        [SerializeField] private GameObject loadingPanel;
        
        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button heroHallButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button logoutButton;
        
        [Header("Authentication UI")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button signInButton;
        [SerializeField] private Button signUpButton;
        [SerializeField] private Button backToMenuButton;
        
        [Header("User Info")]
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private GameObject userInfoPanel;
        
        [Header("Loading")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Slider loadingProgress;
        
        private bool isAuthenticated = false;
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            CheckAuthenticationStatus();
        }
        
        private void InitializeUI()
        {
            // Show main menu by default
            ShowMainMenu();
            
            // Initialize loading text
            if (loadingText) loadingText.text = "Loading...";
            if (loadingProgress) loadingProgress.value = 0f;
        }
        
        private void SetupEventListeners()
        {
            // Main menu buttons
            if (playButton) playButton.onClick.AddListener(OnPlayClicked);
            if (collectionButton) collectionButton.onClick.AddListener(OnCollectionClicked);
            if (deckBuilderButton) deckBuilderButton.onClick.AddListener(OnDeckBuilderClicked);
            if (heroHallButton) heroHallButton.onClick.AddListener(OnHeroHallClicked);
            if (settingsButton) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (loginButton) loginButton.onClick.AddListener(OnLoginClicked);
            if (logoutButton) logoutButton.onClick.AddListener(OnLogoutClicked);
            
            // Auth buttons
            if (signInButton) signInButton.onClick.AddListener(OnSignInClicked);
            if (signUpButton) signUpButton.onClick.AddListener(OnSignUpClicked);
            if (backToMenuButton) backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            
            // Supabase events
            if (SupabaseClient.Instance)
            {
                SupabaseClient.Instance.OnAuthenticationChanged += OnAuthenticationChanged;
                SupabaseClient.Instance.OnError += OnSupabaseError;
            }
        }
        
        private void CheckAuthenticationStatus()
        {
            if (SupabaseClient.Instance)
            {
                isAuthenticated = SupabaseClient.Instance.IsAuthenticated;
                UpdateUIForAuthStatus();
            }
        }
        
        private void UpdateUIForAuthStatus()
        {
            // Update UI based on authentication status
            if (loginButton) loginButton.gameObject.SetActive(!isAuthenticated);
            if (logoutButton) logoutButton.gameObject.SetActive(isAuthenticated);
            if (userInfoPanel) userInfoPanel.SetActive(isAuthenticated);
            
            // Enable/disable features that require authentication
            if (playButton) playButton.interactable = isAuthenticated;
            if (collectionButton) collectionButton.interactable = isAuthenticated;
            if (deckBuilderButton) deckBuilderButton.interactable = isAuthenticated;
            if (heroHallButton) heroHallButton.interactable = isAuthenticated;
            
            if (userNameText && isAuthenticated)
            {
                userNameText.text = "Player"; // Will be updated with actual username
            }
            
            Debug.Log($"🔐 UI updated for auth status: {isAuthenticated}");
        }
        
        #region UI Navigation
        
        private void ShowMainMenu()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(true);
            if (authPanel) authPanel.SetActive(false);
            if (loadingPanel) loadingPanel.SetActive(false);
        }
        
        private void ShowAuthPanel()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (authPanel) authPanel.SetActive(true);
            if (loadingPanel) loadingPanel.SetActive(false);
            
            // Clear input fields
            if (emailInput) emailInput.text = "";
            if (passwordInput) passwordInput.text = "";
        }
        
        private void ShowLoadingPanel(string message = "Loading...")
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (authPanel) authPanel.SetActive(false);
            if (loadingPanel) loadingPanel.SetActive(true);
            
            if (loadingText) loadingText.text = message;
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnPlayClicked()
        {
            if (!isAuthenticated)
            {
                ShowAuthPanel();
                return;
            }
            
            Debug.Log("🎮 Play button clicked - Starting matchmaking");
            GameManager.Instance.ChangeGameState(GameManager.GameState.Battle);
            
            // TODO: Implement matchmaking UI
            ShowLoadingPanel("Finding opponent...");
        }
        
        private void OnCollectionClicked()
        {
            if (!isAuthenticated)
            {
                ShowAuthPanel();
                return;
            }
            
            Debug.Log("📚 Collection button clicked");
            GameManager.Instance.ChangeGameState(GameManager.GameState.Collection);
            
            // TODO: Open collection UI
        }
        
        private void OnDeckBuilderClicked()
        {
            if (!isAuthenticated)
            {
                ShowAuthPanel();
                return;
            }
            
            Debug.Log("🔧 Deck Builder button clicked");
            GameManager.Instance.ChangeGameState(GameManager.GameState.DeckBuilder);
            
            // TODO: Open deck builder UI
        }
        
        private void OnHeroHallClicked()
        {
            if (!isAuthenticated)
            {
                ShowAuthPanel();
                return;
            }
            
            Debug.Log("🦸 Hero Hall button clicked");
            
            // TODO: Open hero selection UI
        }
        
        private void OnSettingsClicked()
        {
            Debug.Log("⚙️ Settings button clicked");
            
            // TODO: Open settings UI
        }
        
        private void OnLoginClicked()
        {
            ShowAuthPanel();
        }
        
        private async void OnLogoutClicked()
        {
            Debug.Log("👋 Logout button clicked");
            
            if (SupabaseClient.Instance)
            {
                SupabaseClient.Instance.SignOut();
            }
        }
        
        private async void OnSignInClicked()
        {
            string email = emailInput?.text ?? "";
            string password = passwordInput?.text ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("⚠️ Email or password is empty");
                return;
            }
            
            ShowLoadingPanel("Signing in...");
            
            bool success = await SupabaseClient.Instance.SignIn(email, password);
            
            if (success)
            {
                ShowMainMenu();
            }
            else
            {
                ShowAuthPanel();
            }
        }
        
        private async void OnSignUpClicked()
        {
            string email = emailInput?.text ?? "";
            string password = passwordInput?.text ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("⚠️ Email or password is empty");
                return;
            }
            
            ShowLoadingPanel("Creating account...");
            
            bool success = await SupabaseClient.Instance.SignUp(email, password);
            
            if (success)
            {
                ShowMainMenu();
            }
            else
            {
                ShowAuthPanel();
            }
        }
        
        private void OnBackToMenuClicked()
        {
            ShowMainMenu();
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnAuthenticationChanged(bool authenticated)
        {
            isAuthenticated = authenticated;
            UpdateUIForAuthStatus();
            
            if (authenticated)
            {
                ShowMainMenu();
                LoadUserData();
            }
        }
        
        private async void LoadUserData()
        {
            ShowLoadingPanel("Loading your cards...");
            
            // Load user's cards and collection
            if (CardService.Instance)
            {
                await CardService.Instance.LoadAllCards();
                // await CardService.Instance.LoadUserCollection(user_id); // Need user ID
            }
            
            ShowMainMenu();
        }
        
        private void OnSupabaseError(string errorMessage)
        {
            Debug.LogError($"❌ Supabase Error: {errorMessage}");
            
            // Show error to user (could use a toast notification)
            ShowMainMenu();
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Cleanup event listeners
            if (SupabaseClient.Instance)
            {
                SupabaseClient.Instance.OnAuthenticationChanged -= OnAuthenticationChanged;
                SupabaseClient.Instance.OnError -= OnSupabaseError;
            }
        }
    }
}