using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Core;
using PotatoCardGame.Network;
using PotatoCardGame.Battle;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Controls the main menu interface with mobile-optimized layout
    /// Handles navigation to different game modes and user authentication
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button heroHallButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button profileButton;
        
        [Header("Authentication UI")]
        [SerializeField] private GameObject authPanel;
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject signupPanel;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button signupButton;
        [SerializeField] private Button switchToSignupButton;
        [SerializeField] private Button switchToLoginButton;
        
        [Header("Login Form")]
        [SerializeField] private TMP_InputField loginEmailField;
        [SerializeField] private TMP_InputField loginPasswordField;
        [SerializeField] private Button submitLoginButton;
        
        [Header("Signup Form")]
        [SerializeField] private TMP_InputField signupEmailField;
        [SerializeField] private TMP_InputField signupPasswordField;
        [SerializeField] private TMP_InputField signupDisplayNameField;
        [SerializeField] private Button submitSignupButton;
        
        [Header("User Info")]
        [SerializeField] private GameObject userInfoPanel;
        [SerializeField] private TextMeshProUGUI welcomeText;
        [SerializeField] private TextMeshProUGUI userStatsText;
        [SerializeField] private Button logoutButton;
        
        [Header("Quick Play")]
        [SerializeField] private GameObject quickPlayPanel;
        [SerializeField] private Button findMatchButton;
        [SerializeField] private Button cancelMatchButton;
        [SerializeField] private TextMeshProUGUI matchStatusText;
        [SerializeField] private GameObject matchLoadingIndicator;
        
        [Header("Animation")]
        [SerializeField] private float buttonAnimationDuration = 0.2f;
        [SerializeField] private float panelAnimationDuration = 0.3f;
        
        private bool isSearchingForMatch = false;
        
        private void Start()
        {
            SetupButtonListeners();
            CheckAuthenticationState();
        }
        
        private void SetupButtonListeners()
        {
            // Main navigation buttons
            if (playButton) playButton.onClick.AddListener(OnPlayButtonPressed);
            if (collectionButton) collectionButton.onClick.AddListener(() => NavigateToScreen(GameManager.GameState.Collection));
            if (deckBuilderButton) deckBuilderButton.onClick.AddListener(() => NavigateToScreen(GameManager.GameState.DeckBuilder));
            if (heroHallButton) heroHallButton.onClick.AddListener(() => NavigateToScreen(GameManager.GameState.HeroHall));
            if (settingsButton) settingsButton.onClick.AddListener(OnSettingsPressed);
            if (profileButton) profileButton.onClick.AddListener(OnProfilePressed);
            
            // Authentication buttons
            if (loginButton) loginButton.onClick.AddListener(() => ShowAuthPanel(true));
            if (signupButton) signupButton.onClick.AddListener(() => ShowAuthPanel(false));
            if (switchToSignupButton) switchToSignupButton.onClick.AddListener(() => SwitchAuthMode(false));
            if (switchToLoginButton) switchToLoginButton.onClick.AddListener(() => SwitchAuthMode(true));
            if (logoutButton) logoutButton.onClick.AddListener(OnLogoutPressed);
            
            // Form submission
            if (submitLoginButton) submitLoginButton.onClick.AddListener(OnSubmitLogin);
            if (submitSignupButton) submitSignupButton.onClick.AddListener(OnSubmitSignup);
            
            // Quick play
            if (findMatchButton) findMatchButton.onClick.AddListener(OnFindMatchPressed);
            if (cancelMatchButton) cancelMatchButton.onClick.AddListener(OnCancelMatchPressed);
            
            // Add button animations
            AddButtonAnimations();
        }
        
        private void AddButtonAnimations()
        {
            Button[] buttons = { playButton, collectionButton, deckBuilderButton, heroHallButton, settingsButton };
            
            foreach (Button button in buttons)
            {
                if (button == null) continue;
                
                button.transform.localScale = Vector3.one;
                
                // Add hover/press animations
                var buttonComponent = button.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    // You can add custom button press animations here
                    // For now, we'll handle this in the UI event system
                }
            }
        }
        
        #region Navigation
        
        private void NavigateToScreen(GameManager.GameState state)
        {
            // Check if user is authenticated for certain screens
            if (RequiresAuthentication(state) && !SupabaseClient.Instance.IsAuthenticated)
            {
                ShowAuthPanel(true);
                return;
            }
            
            // Animate button press
            AnimateButtonPress();
            
            // Navigate to screen
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen(state);
            }
        }
        
        private bool RequiresAuthentication(GameManager.GameState state)
        {
            return state == GameManager.GameState.Collection ||
                   state == GameManager.GameState.DeckBuilder ||
                   state == GameManager.GameState.Battle ||
                   state == GameManager.GameState.HeroHall;
        }
        
        private void AnimateButtonPress()
        {
            // Generic button press animation
            // The specific button will be identified by the UI system
        }
        
        #endregion
        
        #region Authentication UI
        
        private void CheckAuthenticationState()
        {
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged += OnAuthenticationChanged;
                SupabaseClient.Instance.OnUserProfileLoaded += OnUserProfileLoaded;
                
                // Update UI based on current auth state
                OnAuthenticationChanged(SupabaseClient.Instance.IsAuthenticated);
            }
        }
        
        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            Debug.Log($"🔐 Authentication changed: {isAuthenticated}");
            
            // Show/hide appropriate panels
            if (authPanel) authPanel.SetActive(!isAuthenticated);
            if (userInfoPanel) userInfoPanel.SetActive(isAuthenticated);
            if (quickPlayPanel) quickPlayPanel.SetActive(isAuthenticated);
            
            // Update button interactability
            UpdateButtonStates(isAuthenticated);
        }
        
        private void OnUserProfileLoaded(SupabaseClient.UserProfile profile)
        {
            if (welcomeText)
            {
                welcomeText.text = $"Welcome back, {profile.display_name ?? profile.email}!";
            }
            
            // TODO: Load and display user stats
            if (userStatsText)
            {
                userStatsText.text = "Loading stats...";
                LoadUserStats();
            }
        }
        
        private async void LoadUserStats()
        {
            try
            {
                // TODO: Implement user stats loading from database
                if (userStatsText)
                {
                    userStatsText.text = "Cards: Loading... | Wins: Loading... | Level: Loading...";
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading user stats: {e.Message}");
            }
        }
        
        private void UpdateButtonStates(bool isAuthenticated)
        {
            // Enable/disable buttons based on auth state
            if (playButton) playButton.interactable = isAuthenticated;
            if (collectionButton) collectionButton.interactable = isAuthenticated;
            if (deckBuilderButton) deckBuilderButton.interactable = isAuthenticated;
            if (heroHallButton) heroHallButton.interactable = isAuthenticated;
            
            // Update button colors
            Color enabledColor = Color.white;
            Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            
            UpdateButtonColor(playButton, isAuthenticated ? enabledColor : disabledColor);
            UpdateButtonColor(collectionButton, isAuthenticated ? enabledColor : disabledColor);
            UpdateButtonColor(deckBuilderButton, isAuthenticated ? enabledColor : disabledColor);
            UpdateButtonColor(heroHallButton, isAuthenticated ? enabledColor : disabledColor);
        }
        
        private void UpdateButtonColor(Button button, Color color)
        {
            if (button == null) return;
            
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.DOColor(color, 0.3f);
            }
        }
        
        private void ShowAuthPanel(bool isLogin)
        {
            if (authPanel) authPanel.SetActive(true);
            SwitchAuthMode(isLogin);
            
            // Animate panel in
            if (authPanel)
            {
                CanvasGroup canvasGroup = authPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = authPanel.AddComponent<CanvasGroup>();
                
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, panelAnimationDuration);
                
                authPanel.transform.localScale = Vector3.one * 0.8f;
                authPanel.transform.DOScale(Vector3.one, panelAnimationDuration).SetEase(Ease.OutBack);
            }
        }
        
        private void SwitchAuthMode(bool isLogin)
        {
            if (loginPanel) loginPanel.SetActive(isLogin);
            if (signupPanel) signupPanel.SetActive(!isLogin);
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnPlayButtonPressed()
        {
            if (!SupabaseClient.Instance.IsAuthenticated)
            {
                ShowAuthPanel(true);
                return;
            }
            
            // Show quick play options
            Debug.Log("🎮 Play button pressed");
            
            // For now, directly start matchmaking
            OnFindMatchPressed();
        }
        
        private async void OnFindMatchPressed()
        {
            if (isSearchingForMatch) return;
            
            Debug.Log("🔍 Find match pressed");
            
            isSearchingForMatch = true;
            UpdateMatchUI(true);
            
            bool success = await BattleManager.Instance.StartMatchmaking();
            
            if (!success)
            {
                isSearchingForMatch = false;
                UpdateMatchUI(false);
            }
        }
        
        private void OnCancelMatchPressed()
        {
            Debug.Log("❌ Cancel match pressed");
            
            isSearchingForMatch = false;
            BattleManager.Instance.CancelMatchmaking();
            UpdateMatchUI(false);
        }
        
        private void UpdateMatchUI(bool isSearching)
        {
            if (findMatchButton) findMatchButton.gameObject.SetActive(!isSearching);
            if (cancelMatchButton) cancelMatchButton.gameObject.SetActive(isSearching);
            if (matchLoadingIndicator) matchLoadingIndicator.SetActive(isSearching);
            
            if (matchStatusText)
            {
                matchStatusText.text = isSearching ? "Searching for opponent..." : "Ready to play!";
            }
        }
        
        private async void OnSubmitLogin()
        {
            string email = loginEmailField?.text ?? "";
            string password = loginPasswordField?.text ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("⚠️ Email and password required");
                return;
            }
            
            Debug.Log($"🔐 Attempting login: {email}");
            
            // Disable button during login
            if (submitLoginButton) submitLoginButton.interactable = false;
            
            bool success = await SupabaseClient.Instance.SignIn(email, password);
            
            if (success)
            {
                // Hide auth panel
                if (authPanel) authPanel.SetActive(false);
                
                // Clear form
                if (loginEmailField) loginEmailField.text = "";
                if (loginPasswordField) loginPasswordField.text = "";
            }
            
            // Re-enable button
            if (submitLoginButton) submitLoginButton.interactable = true;
        }
        
        private async void OnSubmitSignup()
        {
            string email = signupEmailField?.text ?? "";
            string password = signupPasswordField?.text ?? "";
            string displayName = signupDisplayNameField?.text ?? "";
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("⚠️ Email and password required");
                return;
            }
            
            Debug.Log($"📝 Attempting signup: {email}");
            
            // Disable button during signup
            if (submitSignupButton) submitSignupButton.interactable = false;
            
            bool success = await SupabaseClient.Instance.SignUp(email, password, displayName);
            
            if (success)
            {
                // Switch to login mode
                SwitchAuthMode(true);
                
                // Clear form
                if (signupEmailField) signupEmailField.text = "";
                if (signupPasswordField) signupPasswordField.text = "";
                if (signupDisplayNameField) signupDisplayNameField.text = "";
                
                // Show success message
                Debug.Log("✅ Signup successful! Please check your email.");
            }
            
            // Re-enable button
            if (submitSignupButton) submitSignupButton.interactable = true;
        }
        
        private async void OnLogoutPressed()
        {
            Debug.Log("👋 Logout pressed");
            
            await SupabaseClient.Instance.SignOut();
        }
        
        private void OnSettingsPressed()
        {
            Debug.Log("⚙️ Settings pressed");
            // TODO: Implement settings screen
        }
        
        private void OnProfilePressed()
        {
            Debug.Log("👤 Profile pressed");
            // TODO: Implement profile screen
        }
        
        #endregion
        
        private void OnEnable()
        {
            // Subscribe to battle state changes
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
            }
            
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged -= OnAuthenticationChanged;
                SupabaseClient.Instance.OnUserProfileLoaded -= OnUserProfileLoaded;
            }
        }
        
        private void OnBattleStateChanged(BattleManager.BattleState newState)
        {
            switch (newState)
            {
                case BattleManager.BattleState.Searching:
                    UpdateMatchUI(true);
                    break;
                    
                case BattleManager.BattleState.Initializing:
                    if (matchStatusText) matchStatusText.text = "Initializing battle...";
                    break;
                    
                case BattleManager.BattleState.InProgress:
                    // Battle started - hide main menu
                    gameObject.SetActive(false);
                    break;
                    
                case BattleManager.BattleState.NotInBattle:
                    isSearchingForMatch = false;
                    UpdateMatchUI(false);
                    gameObject.SetActive(true);
                    break;
            }
        }
        
        private void OnDestroy()
        {
            // Cleanup DOTween animations
            DOTween.Kill(transform);
        }
    }
}