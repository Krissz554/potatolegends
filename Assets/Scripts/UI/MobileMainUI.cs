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
    /// Main mobile UI that matches the web game functionality
    /// Provides navigation and main game interface for mobile devices
    /// </summary>
    public class MobileMainUI : MonoBehaviour
    {
        [Header("Navigation Buttons")]
        [SerializeField] private Button mainPageButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button heroHallButton;
        
        [Header("Battle Button")]
        [SerializeField] private Button battleButton; // Bottom right corner
        [SerializeField] private TextMeshProUGUI battleButtonText;
        
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI screenTitleText;
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button logoutButton;
        
        [Header("Screen Panels")]
        [SerializeField] private GameObject mainPagePanel;
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private GameObject deckBuilderPanel;
        [SerializeField] private GameObject heroHallPanel;
        
        [Header("Main Page Content")]
        [SerializeField] private TextMeshProUGUI welcomeText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button quickPlayButton;
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private TextMeshProUGUI matchStatusText;
        [SerializeField] private Button cancelMatchButton;
        
        [Header("Login Screen")]
        [SerializeField] private GameObject loginScreen;
        
        // Current state
        private GameManager.GameState currentScreen = GameManager.GameState.MainMenu;
        private bool isSearchingForMatch = false;
        
        private void Start()
        {
            SetupUI();
            CheckAuthentication();
        }
        
        private void SetupUI()
        {
            // Setup navigation buttons
            if (mainPageButton) mainPageButton.onClick.AddListener(() => NavigateToScreen(GameManager.GameState.MainMenu));
            if (collectionButton) collectionButton.onClick.AddListener(() => NavigateToScreen(GameManager.GameState.Collection));
            if (deckBuilderButton) deckBuilderButton.onClick.AddListener(() => NavigateToScreen(GameManager.GameState.DeckBuilder));
            if (heroHallButton) heroHallButton.onClick.AddListener(() => NavigateToScreen(GameManager.GameState.HeroHall));
            
            // Setup battle button
            if (battleButton) battleButton.onClick.AddListener(OnBattleButtonPressed);
            if (quickPlayButton) quickPlayButton.onClick.AddListener(OnQuickPlayPressed);
            if (cancelMatchButton) cancelMatchButton.onClick.AddListener(OnCancelMatchPressed);
            
            // Setup top bar buttons
            if (settingsButton) settingsButton.onClick.AddListener(OnSettingsPressed);
            if (logoutButton) logoutButton.onClick.AddListener(OnLogoutPressed);
            
            // Start with main page
            NavigateToScreen(GameManager.GameState.MainMenu);
            
            // Hide matchmaking panel initially
            if (matchmakingPanel) matchmakingPanel.SetActive(false);
            
            // Subscribe to authentication events
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged += OnAuthenticationChanged;
                SupabaseClient.Instance.OnUserProfileLoaded += OnUserProfileLoaded;
            }
            
            // Subscribe to battle events
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
            }
        }
        
        private void CheckAuthentication()
        {
            if (SupabaseClient.Instance == null || !SupabaseClient.Instance.IsAuthenticated)
            {
                // Show login screen
                if (loginScreen) loginScreen.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                // User is authenticated, show main UI
                if (loginScreen) loginScreen.SetActive(false);
                gameObject.SetActive(true);
                LoadUserInfo();
            }
        }
        
        private void NavigateToScreen(GameManager.GameState newScreen)
        {
            if (currentScreen == newScreen) return;
            
            currentScreen = newScreen;
            
            // Hide all panels
            if (mainPagePanel) mainPagePanel.SetActive(false);
            if (collectionPanel) collectionPanel.SetActive(false);
            if (deckBuilderPanel) deckBuilderPanel.SetActive(false);
            if (heroHallPanel) heroHallPanel.SetActive(false);
            
            // Show selected panel
            GameObject targetPanel = GetPanelForScreen(newScreen);
            if (targetPanel) targetPanel.SetActive(true);
            
            // Update screen title
            UpdateScreenTitle(newScreen);
            
            // Update navigation button states
            UpdateNavigationButtons(newScreen);
            
            // Update game manager state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(newScreen);
            }
            
            Debug.Log($"📱 Navigated to: {newScreen}");
        }
        
        private GameObject GetPanelForScreen(GameManager.GameState screen)
        {
            return screen switch
            {
                GameManager.GameState.MainMenu => mainPagePanel,
                GameManager.GameState.Collection => collectionPanel,
                GameManager.GameState.DeckBuilder => deckBuilderPanel,
                GameManager.GameState.HeroHall => heroHallPanel,
                _ => mainPagePanel
            };
        }
        
        private void UpdateScreenTitle(GameManager.GameState screen)
        {
            if (screenTitleText == null) return;
            
            string title = screen switch
            {
                GameManager.GameState.MainMenu => "What's My Potato?",
                GameManager.GameState.Collection => "Card Collection",
                GameManager.GameState.DeckBuilder => "Deck Builder",
                GameManager.GameState.HeroHall => "Hero Hall",
                _ => "Potato Card Game"
            };
            
            screenTitleText.text = title;
        }
        
        private void UpdateNavigationButtons(GameManager.GameState activeScreen)
        {
            // Update button colors to show active state
            UpdateNavButton(mainPageButton, activeScreen == GameManager.GameState.MainMenu);
            UpdateNavButton(collectionButton, activeScreen == GameManager.GameState.Collection);
            UpdateNavButton(deckBuilderButton, activeScreen == GameManager.GameState.DeckBuilder);
            UpdateNavButton(heroHallButton, activeScreen == GameManager.GameState.HeroHall);
        }
        
        private void UpdateNavButton(Button button, bool isActive)
        {
            if (button == null) return;
            
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.8f);
            }
            
            // Scale effect for active button
            float scale = isActive ? 1.1f : 1f;
            button.transform.localScale = Vector3.one * scale;
        }
        
        #region Authentication Events
        
        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                // User logged in, show main UI
                if (loginScreen) loginScreen.SetActive(false);
                gameObject.SetActive(true);
                LoadUserInfo();
            }
            else
            {
                // User logged out, show login screen
                if (loginScreen) loginScreen.SetActive(true);
                gameObject.SetActive(false);
            }
        }
        
        private void OnUserProfileLoaded(SupabaseClient.UserProfile profile)
        {
            if (userNameText)
            {
                string displayName = !string.IsNullOrEmpty(profile.display_name) ? 
                    profile.display_name : 
                    profile.email.Split('@')[0]; // Use part before @ as fallback
                
                userNameText.text = $"Welcome, {displayName}!";
            }
            
            LoadUserStats();
        }
        
        private void LoadUserInfo()
        {
            // This will trigger OnUserProfileLoaded when complete
            Debug.Log("📊 Loading user information...");
        }
        
        private void LoadUserStats()
        {
            if (statsText)
            {
                // TODO: Load actual stats from database
                statsText.text = "Loading your stats...";
            }
            
            if (welcomeText)
            {
                welcomeText.text = "Ready for battle? Choose your adventure!";
            }
        }
        
        #endregion
        
        #region Battle System
        
        private void OnBattleButtonPressed()
        {
            Debug.Log("⚔️ Battle button pressed");
            OnQuickPlayPressed();
        }
        
        private async void OnQuickPlayPressed()
        {
            if (isSearchingForMatch) return;
            
            Debug.Log("🔍 Starting matchmaking...");
            
            isSearchingForMatch = true;
            UpdateBattleUI(true);
            
            bool success = await BattleManager.Instance.StartMatchmaking();
            
            if (!success)
            {
                isSearchingForMatch = false;
                UpdateBattleUI(false);
            }
        }
        
        private void OnCancelMatchPressed()
        {
            Debug.Log("❌ Cancelling matchmaking");
            
            isSearchingForMatch = false;
            BattleManager.Instance.CancelMatchmaking();
            UpdateBattleUI(false);
        }
        
        private void UpdateBattleUI(bool isSearching)
        {
            if (matchmakingPanel) matchmakingPanel.SetActive(isSearching);
            
            if (matchStatusText)
            {
                matchStatusText.text = isSearching ? "Searching for opponent..." : "";
            }
            
            if (battleButtonText)
            {
                battleButtonText.text = isSearching ? "Searching..." : "BATTLE";
            }
            
            if (battleButton) battleButton.interactable = !isSearching;
            if (quickPlayButton) quickPlayButton.interactable = !isSearching;
        }
        
        private void OnBattleStateChanged(BattleManager.BattleState newState)
        {
            switch (newState)
            {
                case BattleManager.BattleState.Searching:
                    UpdateBattleUI(true);
                    break;
                    
                case BattleManager.BattleState.Initializing:
                    if (matchStatusText) matchStatusText.text = "Match found! Initializing...";
                    break;
                    
                case BattleManager.BattleState.InProgress:
                    // Battle started - hide main UI, show battle UI
                    gameObject.SetActive(false);
                    break;
                    
                case BattleManager.BattleState.NotInBattle:
                    isSearchingForMatch = false;
                    UpdateBattleUI(false);
                    gameObject.SetActive(true);
                    break;
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnSettingsPressed()
        {
            Debug.Log("⚙️ Settings button pressed");
            // TODO: Implement settings screen
        }
        
        private async void OnLogoutPressed()
        {
            Debug.Log("👋 Logout button pressed");
            
            await SupabaseClient.Instance.SignOut();
            
            // Authentication changed event will handle the UI transition
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged -= OnAuthenticationChanged;
                SupabaseClient.Instance.OnUserProfileLoaded -= OnUserProfileLoaded;
            }
            
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
            }
        }
    }
}