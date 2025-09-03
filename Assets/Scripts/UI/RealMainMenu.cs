using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Cards;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// REAL Main Menu that actually works like your web version
    /// Shows navigation icons and BATTLE button exactly like the browser game
    /// </summary>
    public class RealMainMenu : MonoBehaviour
    {
        [Header("Main Menu UI")]
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button heroHallButton;
        [SerializeField] private Button battleButton;
        [SerializeField] private Button logoutButton;
        
        [Header("User Info")]
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private TextMeshProUGUI userStatsText;
        [SerializeField] private TextMeshProUGUI deckValidationText;
        
        [Header("Status")]
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private TextMeshProUGUI matchmakingText;
        [SerializeField] private Button cancelMatchmakingButton;
        
        // State
        private bool isInMatchmaking = false;
        private bool hasDeckValidation = false;
        
        private void Start()
        {
            CreateMainMenuUI();
            SetupEventListeners();
            LoadUserInfo();
        }
        
        private void CreateMainMenuUI()
        {
            Debug.Log("🏠 Creating real main menu UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            CreateMainMenuInterface(canvas.transform);
            
            // Start hidden
            gameObject.SetActive(false);
            
            Debug.Log("✅ Real main menu UI created");
        }
        
        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Main Menu Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        private void CreateMainMenuInterface(Transform parent)
        {
            // Background
            GameObject background = CreatePanel("Main Menu Background", parent);
            Image bgImage = background.GetComponent<Image>();
            
            // Create gradient background like web version
            bgImage.color = new Color(0.1f, 0.15f, 0.3f, 1f); // Dark blue-purple
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Top bar with user info
            CreateTopBar(background.transform);
            
            // Navigation icons (top area)
            CreateNavigationIcons(background.transform);
            
            // Main content area
            CreateMainContent(background.transform);
            
            // BATTLE button (bottom right)
            CreateBattleButton(background.transform);
            
            // Matchmaking overlay
            CreateMatchmakingPanel(background.transform);
        }
        
        private void CreateTopBar(Transform parent)
        {
            GameObject topBar = CreatePanel("Top Bar", parent);
            Image topBarBg = topBar.GetComponent<Image>();
            topBarBg.color = new Color(0f, 0f, 0f, 0.7f);
            
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 0.9f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.offsetMin = Vector2.zero;
            topBarRect.offsetMax = Vector2.zero;
            
            // User name
            GameObject userNameObj = new GameObject("User Name");
            userNameObj.transform.SetParent(topBar.transform, false);
            userNameObj.layer = 5;
            
            userNameText = userNameObj.AddComponent<TextMeshProUGUI>();
            userNameText.text = "Welcome, Player!";
            userNameText.fontSize = 24;
            userNameText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            userNameText.alignment = TextAlignmentOptions.Left;
            userNameText.fontStyle = FontStyles.Bold;
            
            RectTransform userNameRect = userNameObj.GetComponent<RectTransform>();
            userNameRect.anchorMin = new Vector2(0.05f, 0f);
            userNameRect.anchorMax = new Vector2(0.6f, 1f);
            userNameRect.offsetMin = Vector2.zero;
            userNameRect.offsetMax = Vector2.zero;
            
            // Logout button
            GameObject logoutBtnObj = CreatePanel("Logout Button", topBar.transform);
            logoutButton = logoutBtnObj.AddComponent<Button>();
            Image logoutBtnImg = logoutBtnObj.GetComponent<Image>();
            logoutBtnImg.color = new Color(0.7f, 0.3f, 0.3f, 1f);
            
            GameObject logoutTextObj = new GameObject("Logout Text");
            logoutTextObj.transform.SetParent(logoutBtnObj.transform, false);
            logoutTextObj.layer = 5;
            
            TextMeshProUGUI logoutText = logoutTextObj.AddComponent<TextMeshProUGUI>();
            logoutText.text = "🚪 LOGOUT";
            logoutText.fontSize = 18;
            logoutText.color = Color.white;
            logoutText.alignment = TextAlignmentOptions.Center;
            logoutText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(logoutTextObj.GetComponent<RectTransform>());
            
            RectTransform logoutBtnRect = logoutBtnObj.GetComponent<RectTransform>();
            logoutBtnRect.anchorMin = new Vector2(0.8f, 0.1f);
            logoutBtnRect.anchorMax = new Vector2(0.95f, 0.9f);
            logoutBtnRect.offsetMin = Vector2.zero;
            logoutBtnRect.offsetMax = Vector2.zero;
        }
        
        private void CreateNavigationIcons(Transform parent)
        {
            GameObject navPanel = CreatePanel("Navigation Panel", parent);
            Image navBg = navPanel.GetComponent<Image>();
            navBg.color = new Color(0f, 0f, 0f, 0.5f);
            
            RectTransform navRect = navPanel.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0.05f, 0.75f);
            navRect.anchorMax = new Vector2(0.95f, 0.88f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;
            
            // Collection button
            collectionButton = CreateNavButton(navPanel.transform, "📚 COLLECTION", 
                new Vector2(0.05f, 0.1f), new Vector2(0.3f, 0.9f), new Color(0.2f, 0.7f, 0.3f, 1f));
            
            // Deck builder button
            deckBuilderButton = CreateNavButton(navPanel.transform, "🔧 DECK BUILDER", 
                new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.9f), new Color(0.7f, 0.3f, 0.7f, 1f));
            
            // Hero hall button
            heroHallButton = CreateNavButton(navPanel.transform, "🦸 HERO HALL", 
                new Vector2(0.7f, 0.1f), new Vector2(0.95f, 0.9f), new Color(0.9f, 0.6f, 0.2f, 1f));
        }
        
        private Button CreateNavButton(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject btnObj = CreatePanel($"Nav Button {text}", parent);
            Button button = btnObj.AddComponent<Button>();
            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = color;
            
            GameObject textObj = new GameObject("Button Text");
            textObj.transform.SetParent(btnObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        private void CreateMainContent(Transform parent)
        {
            GameObject contentPanel = CreatePanel("Main Content Panel", parent);
            Image contentBg = contentPanel.GetComponent<Image>();
            contentBg.color = new Color(0f, 0f, 0f, 0.3f);
            
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.05f, 0.25f);
            contentRect.anchorMax = new Vector2(0.95f, 0.73f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Welcome message
            GameObject welcomeObj = new GameObject("Welcome Message");
            welcomeObj.transform.SetParent(contentPanel.transform, false);
            welcomeObj.layer = 5;
            
            TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
            welcomeText.text = "🥔 POTATO LEGENDS\nMOBILE CARD GAME\n\nWelcome to the arena!\nBuild your deck and battle!";
            welcomeText.fontSize = 28;
            welcomeText.color = new Color(1f, 0.9f, 0.7f, 1f);
            welcomeText.alignment = TextAlignmentOptions.Center;
            welcomeText.fontStyle = FontStyles.Bold;
            
            RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
            welcomeRect.anchorMin = new Vector2(0.1f, 0.4f);
            welcomeRect.anchorMax = new Vector2(0.9f, 0.8f);
            welcomeRect.offsetMin = Vector2.zero;
            welcomeRect.offsetMax = Vector2.zero;
            
            // User stats
            GameObject statsObj = new GameObject("User Stats");
            statsObj.transform.SetParent(contentPanel.transform, false);
            statsObj.layer = 5;
            
            userStatsText = statsObj.AddComponent<TextMeshProUGUI>();
            userStatsText.text = "Loading stats...";
            userStatsText.fontSize = 18;
            userStatsText.color = Color.white;
            userStatsText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.1f, 0.2f);
            statsRect.anchorMax = new Vector2(0.9f, 0.35f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // Deck validation status
            GameObject deckValidObj = new GameObject("Deck Validation");
            deckValidObj.transform.SetParent(contentPanel.transform, false);
            deckValidObj.layer = 5;
            
            deckValidationText = deckValidObj.AddComponent<TextMeshProUGUI>();
            deckValidationText.text = "Checking deck...";
            deckValidationText.fontSize = 20;
            deckValidationText.color = new Color(1f, 0.8f, 0.2f, 1f);
            deckValidationText.alignment = TextAlignmentOptions.Center;
            deckValidationText.fontStyle = FontStyles.Bold;
            
            RectTransform deckValidRect = deckValidObj.GetComponent<RectTransform>();
            deckValidRect.anchorMin = new Vector2(0.1f, 0.05f);
            deckValidRect.anchorMax = new Vector2(0.9f, 0.15f);
            deckValidRect.offsetMin = Vector2.zero;
            deckValidRect.offsetMax = Vector2.zero;
        }
        
        private void CreateBattleButton(Transform parent)
        {
            // BATTLE button (bottom right corner like web version)
            GameObject battleBtnObj = CreatePanel("BATTLE Button", parent);
            battleButton = battleBtnObj.AddComponent<Button>();
            Image battleBtnImg = battleBtnObj.GetComponent<Image>();
            battleBtnImg.color = new Color(1f, 0.2f, 0.2f, 1f); // Red like web version
            
            GameObject battleTextObj = new GameObject("Battle Text");
            battleTextObj.transform.SetParent(battleBtnObj.transform, false);
            battleTextObj.layer = 5;
            
            TextMeshProUGUI battleText = battleTextObj.AddComponent<TextMeshProUGUI>();
            battleText.text = "⚔️\nBATTLE";
            battleText.fontSize = 32;
            battleText.color = Color.white;
            battleText.alignment = TextAlignmentOptions.Center;
            battleText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(battleTextObj.GetComponent<RectTransform>());
            
            // Position in bottom right like web version
            RectTransform battleBtnRect = battleBtnObj.GetComponent<RectTransform>();
            battleBtnRect.anchorMin = new Vector2(0.7f, 0.05f);
            battleBtnRect.anchorMax = new Vector2(0.95f, 0.22f);
            battleBtnRect.offsetMin = Vector2.zero;
            battleBtnRect.offsetMax = Vector2.zero;
        }
        
        private void CreateMatchmakingPanel(Transform parent)
        {
            matchmakingPanel = CreatePanel("Matchmaking Panel", parent);
            Image matchmakingBg = matchmakingPanel.GetComponent<Image>();
            matchmakingBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            SetFullScreen(matchmakingPanel.GetComponent<RectTransform>());
            matchmakingPanel.SetActive(false);
            
            // Matchmaking content
            GameObject matchmakingContent = CreatePanel("Matchmaking Content", matchmakingPanel.transform);
            matchmakingContent.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
            
            RectTransform matchmakingRect = matchmakingContent.GetComponent<RectTransform>();
            matchmakingRect.anchorMin = new Vector2(0.1f, 0.3f);
            matchmakingRect.anchorMax = new Vector2(0.9f, 0.7f);
            matchmakingRect.offsetMin = Vector2.zero;
            matchmakingRect.offsetMax = Vector2.zero;
            
            // Matchmaking text
            GameObject matchmakingTextObj = new GameObject("Matchmaking Text");
            matchmakingTextObj.transform.SetParent(matchmakingContent.transform, false);
            matchmakingTextObj.layer = 5;
            
            matchmakingText = matchmakingTextObj.AddComponent<TextMeshProUGUI>();
            matchmakingText.text = "🔍 SEARCHING FOR OPPONENT...\n\nPlease wait while we find\na worthy challenger!";
            matchmakingText.fontSize = 28;
            matchmakingText.color = new Color(1f, 0.8f, 0.2f, 1f);
            matchmakingText.alignment = TextAlignmentOptions.Center;
            matchmakingText.fontStyle = FontStyles.Bold;
            
            RectTransform matchmakingTextRect = matchmakingTextObj.GetComponent<RectTransform>();
            matchmakingTextRect.anchorMin = new Vector2(0.1f, 0.4f);
            matchmakingTextRect.anchorMax = new Vector2(0.9f, 0.8f);
            matchmakingTextRect.offsetMin = Vector2.zero;
            matchmakingTextRect.offsetMax = Vector2.zero;
            
            // Cancel matchmaking button
            GameObject cancelBtnObj = CreatePanel("Cancel Matchmaking Button", matchmakingContent.transform);
            cancelMatchmakingButton = cancelBtnObj.AddComponent<Button>();
            Image cancelBtnImg = cancelBtnObj.GetComponent<Image>();
            cancelBtnImg.color = new Color(0.7f, 0.3f, 0.3f, 1f);
            
            GameObject cancelTextObj = new GameObject("Cancel Text");
            cancelTextObj.transform.SetParent(cancelBtnObj.transform, false);
            cancelTextObj.layer = 5;
            
            TextMeshProUGUI cancelText = cancelTextObj.AddComponent<TextMeshProUGUI>();
            cancelText.text = "❌ CANCEL";
            cancelText.fontSize = 24;
            cancelText.color = Color.white;
            cancelText.alignment = TextAlignmentOptions.Center;
            cancelText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(cancelTextObj.GetComponent<RectTransform>());
            
            RectTransform cancelBtnRect = cancelBtnObj.GetComponent<RectTransform>();
            cancelBtnRect.anchorMin = new Vector2(0.25f, 0.1f);
            cancelBtnRect.anchorMax = new Vector2(0.75f, 0.25f);
            cancelBtnRect.offsetMin = Vector2.zero;
            cancelBtnRect.offsetMax = Vector2.zero;
        }
        
        private void SetupEventListeners()
        {
            // Navigation buttons
            if (collectionButton) collectionButton.onClick.AddListener(OnCollectionPressed);
            if (deckBuilderButton) deckBuilderButton.onClick.AddListener(OnDeckBuilderPressed);
            if (heroHallButton) heroHallButton.onClick.AddListener(OnHeroHallPressed);
            if (battleButton) battleButton.onClick.AddListener(OnBattlePressed);
            if (logoutButton) logoutButton.onClick.AddListener(OnLogoutPressed);
            if (cancelMatchmakingButton) cancelMatchmakingButton.onClick.AddListener(OnCancelMatchmakingPressed);
            
            // Supabase events
            if (RealSupabaseClient.Instance != null)
            {
                RealSupabaseClient.Instance.OnAuthenticationChanged += OnAuthChanged;
                RealSupabaseClient.Instance.OnUserProfileLoaded += OnUserProfileLoaded;
            }
            
            // Game flow events
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged += OnGameFlowChanged;
            }
        }
        
        private async void LoadUserInfo()
        {
            try
            {
                if (RealSupabaseClient.Instance?.IsAuthenticated == true)
                {
                    Debug.Log("📊 Loading user information...");
                    
                    var userProfile = RealSupabaseClient.Instance.CurrentUser;
                    if (userProfile != null)
                    {
                        UpdateUserDisplay(userProfile);
                    }
                    
                    // Check deck validation
                    await CheckDeckValidation();
                    
                    // Load user stats
                    await LoadUserStats();
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading user info: {e.Message}");
            }
        }
        
        private async Task CheckDeckValidation()
        {
            try
            {
                if (RealSupabaseClient.Instance?.IsAuthenticated == true)
                {
                    // Use the same validation as web version
                    bool hasValidDeck = await RealSupabaseClient.Instance.CallRPC<bool>("check_user_has_valid_deck", 
                        new { user_id = RealSupabaseClient.Instance.UserId });
                    
                    hasDeckValidation = hasValidDeck;
                    
                    if (deckValidationText)
                    {
                        if (hasValidDeck)
                        {
                            deckValidationText.text = "✅ Deck Ready for Battle!";
                            deckValidationText.color = new Color(0.2f, 1f, 0.2f, 1f);
                        }
                        else
                        {
                            deckValidationText.text = "❌ Build a 30-card deck to battle";
                            deckValidationText.color = new Color(1f, 0.3f, 0.3f, 1f);
                        }
                    }
                    
                    // Enable/disable battle button
                    if (battleButton)
                    {
                        battleButton.interactable = hasValidDeck;
                        Image battleImg = battleButton.GetComponent<Image>();
                        if (battleImg)
                        {
                            battleImg.color = hasValidDeck ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                        }
                    }
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error checking deck validation: {e.Message}");
            }
        }
        
        private async Task LoadUserStats()
        {
            try
            {
                // Load user statistics from database
                var userStats = await RealSupabaseClient.Instance.GetData<List<Dictionary<string, object>>>(
                    $"user_profiles?id=eq.{RealSupabaseClient.Instance.UserId}&select=*"
                );
                
                if (userStats != null && userStats.Count > 0)
                {
                    var stats = userStats[0];
                    
                    if (userStatsText)
                    {
                        string statsDisplay = "📊 Your Stats:\n";
                        
                        if (stats.ContainsKey("total_wins"))
                            statsDisplay += $"🏆 Wins: {stats["total_wins"]}\n";
                        if (stats.ContainsKey("total_losses"))
                            statsDisplay += $"💀 Losses: {stats["total_losses"]}\n";
                        if (stats.ContainsKey("rank"))
                            statsDisplay += $"⭐ Rank: {stats["rank"]}\n";
                        
                        userStatsText.text = statsDisplay;
                    }
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading user stats: {e.Message}");
                
                if (userStatsText)
                {
                    userStatsText.text = "📊 Stats: Loading...";
                }
            }
        }
        
        private void UpdateUserDisplay(RealSupabaseClient.UserProfile userProfile)
        {
            if (userNameText)
            {
                string displayName = userProfile.display_name ?? userProfile.username ?? userProfile.email;
                userNameText.text = $"Welcome, {displayName}!";
            }
        }
        
        private void OnCollectionPressed()
        {
            Debug.Log("📚 Opening collection...");
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.NavigateToCollection();
            }
        }
        
        private void OnDeckBuilderPressed()
        {
            Debug.Log("🔧 Opening deck builder...");
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.NavigateToDeckBuilder();
            }
        }
        
        private void OnHeroHallPressed()
        {
            Debug.Log("🦸 Opening hero hall...");
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.NavigateToHeroHall();
            }
        }
        
        private async void OnBattlePressed()
        {
            if (!hasDeckValidation)
            {
                Debug.Log("❌ Cannot battle - invalid deck");
                return;
            }
            
            Debug.Log("⚔️ Starting matchmaking...");
            
            try
            {
                // Join matchmaking queue
                bool success = await RealSupabaseClient.Instance.JoinMatchmaking();
                
                if (success)
                {
                    isInMatchmaking = true;
                    if (matchmakingPanel) matchmakingPanel.SetActive(true);
                    
                    // Start polling for match
                    StartMatchmakingPolling();
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error starting matchmaking: {e.Message}");
            }
        }
        
        private async void StartMatchmakingPolling()
        {
            while (isInMatchmaking)
            {
                try
                {
                    var battleSession = await RealSupabaseClient.Instance.CheckForBattle();
                    
                    if (battleSession != null)
                    {
                        Debug.Log($"⚔️ Match found! Battle ID: {battleSession.id}");
                        
                        isInMatchmaking = false;
                        if (matchmakingPanel) matchmakingPanel.SetActive(false);
                        
                        // Navigate to battle
                        if (GameFlowManager.Instance != null)
                        {
                            GameFlowManager.Instance.NavigateToBattle();
                        }
                        
                        break;
                    }
                    
                    // Wait 2 seconds before next check
                    await Task.Delay(2000);
                    
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Error checking for match: {e.Message}");
                    await Task.Delay(5000); // Wait longer on error
                }
            }
        }
        
        private async void OnCancelMatchmakingPressed()
        {
            Debug.Log("❌ Cancelling matchmaking...");
            
            try
            {
                await RealSupabaseClient.Instance.LeaveMatchmaking();
                isInMatchmaking = false;
                
                if (matchmakingPanel) matchmakingPanel.SetActive(false);
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error cancelling matchmaking: {e.Message}");
            }
        }
        
        private async void OnLogoutPressed()
        {
            Debug.Log("🚪 Logging out...");
            
            try
            {
                // Cancel any ongoing matchmaking
                if (isInMatchmaking)
                {
                    await RealSupabaseClient.Instance.LeaveMatchmaking();
                    isInMatchmaking = false;
                }
                
                // Sign out
                await RealSupabaseClient.Instance.SignOut();
                
                // Navigate back to auth
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.NavigateToAuth();
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error logging out: {e.Message}");
            }
        }
        
        private void OnAuthChanged(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                LoadUserInfo();
            }
            else
            {
                // Clear user info
                if (userNameText) userNameText.text = "Not logged in";
                if (userStatsText) userStatsText.text = "";
                if (deckValidationText) deckValidationText.text = "";
            }
        }
        
        private void OnUserProfileLoaded(RealSupabaseClient.UserProfile userProfile)
        {
            UpdateUserDisplay(userProfile);
        }
        
        private void OnGameFlowChanged(GameFlowManager.GameFlow newFlow)
        {
            bool shouldShow = newFlow == GameFlowManager.GameFlow.MainMenu;
            gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                LoadUserInfo(); // Refresh when shown
                
                // Refresh deck validation
                _ = CheckDeckValidation();
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
            // Cancel matchmaking if active
            if (isInMatchmaking)
            {
                _ = RealSupabaseClient.Instance?.LeaveMatchmaking();
            }
            
            if (RealSupabaseClient.Instance != null)
            {
                RealSupabaseClient.Instance.OnAuthenticationChanged -= OnAuthChanged;
                RealSupabaseClient.Instance.OnUserProfileLoaded -= OnUserProfileLoaded;
            }
            
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged -= OnGameFlowChanged;
            }
        }
    }
}