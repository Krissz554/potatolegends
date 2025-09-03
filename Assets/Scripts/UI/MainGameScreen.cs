using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PotatoCardGame.Core;
using PotatoCardGame.Network;
using PotatoCardGame.Battle;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Main game screen that matches the web version layout
    /// Top icons: Collection, Deck Builder, Hero Hall
    /// Bottom right: BATTLE button
    /// Main content area with game information
    /// </summary>
    public class MainGameScreen : MonoBehaviour
    {
        [Header("Top Navigation")]
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button heroHallButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button logoutButton;
        
        [Header("Battle System")]
        [SerializeField] private Button battleButton;
        [SerializeField] private TextMeshProUGUI battleButtonText;
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private TextMeshProUGUI matchmakingStatusText;
        [SerializeField] private Button cancelMatchmakingButton;
        
        [Header("Main Content")]
        [SerializeField] private TextMeshProUGUI welcomeText;
        [SerializeField] private TextMeshProUGUI userStatsText;
        [SerializeField] private TextMeshProUGUI collectionStatsText;
        
        [Header("User Info")]
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private TextMeshProUGUI userLevelText;
        
        private bool isInMatchmaking = false;
        private bool hasValidDeck = true;
        
        private void Start()
        {
            CreateMainGameUI();
            SetupEventListeners();
            LoadUserData();
        }
        
        private void CreateMainGameUI()
        {
            Debug.Log("🎮 Creating main game UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateMainCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            // Create main game interface
            CreateMainInterface(canvas.transform);
            
            Debug.Log("✅ Main game UI created");
        }
        
        private void CreateMainCanvas()
        {
            GameObject canvasObj = new GameObject("Main Game Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if needed
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }
        }
        
        private void CreateMainInterface(Transform parent)
        {
            // Background
            GameObject background = CreatePanel("Main Background", parent);
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.08f, 0.04f, 0.15f, 1f); // Dark purple background
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Top navigation bar
            CreateTopNavigation(background.transform);
            
            // Main content area
            CreateMainContent(background.transform);
            
            // Battle button (bottom right)
            CreateBattleButton(background.transform);
            
            // Matchmaking panel (hidden by default)
            CreateMatchmakingPanel(background.transform);
        }
        
        private void CreateTopNavigation(Transform parent)
        {
            GameObject topNav = CreatePanel("Top Navigation", parent);
            Image navBg = topNav.GetComponent<Image>();
            navBg.color = new Color(0f, 0f, 0f, 0.6f); // Semi-transparent
            
            RectTransform navRect = topNav.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0f, 0.9f);
            navRect.anchorMax = new Vector2(1f, 1f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;
            
            // User info (left side)
            CreateUserInfo(topNav.transform);
            
            // Navigation icons (center)
            CreateNavigationIcons(topNav.transform);
            
            // Settings and logout (right side)
            CreateTopRightButtons(topNav.transform);
        }
        
        private void CreateUserInfo(Transform parent)
        {
            GameObject userInfo = new GameObject("User Info");
            userInfo.transform.SetParent(parent, false);
            userInfo.layer = 5;
            
            // User name
            userNameText = userInfo.AddComponent<TextMeshProUGUI>();
            userNameText.text = "Loading...";
            userNameText.fontSize = 24;
            userNameText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            userNameText.alignment = TextAlignmentOptions.Left;
            userNameText.fontStyle = FontStyles.Bold;
            
            RectTransform userRect = userInfo.GetComponent<RectTransform>();
            userRect.anchorMin = new Vector2(0.05f, 0f);
            userRect.anchorMax = new Vector2(0.4f, 1f);
            userRect.offsetMin = Vector2.zero;
            userRect.offsetMax = Vector2.zero;
        }
        
        private void CreateNavigationIcons(Transform parent)
        {
            // Collection button
            collectionButton = CreateTopNavButton(parent, "Collection", "📚", 
                new Vector2(0.4f, 0.1f), new Vector2(0.5f, 0.9f));
            
            // Deck Builder button  
            deckBuilderButton = CreateTopNavButton(parent, "Deck Builder", "🔧",
                new Vector2(0.5f, 0.1f), new Vector2(0.6f, 0.9f));
            
            // Hero Hall button
            heroHallButton = CreateTopNavButton(parent, "Hero Hall", "🦸",
                new Vector2(0.6f, 0.1f), new Vector2(0.7f, 0.9f));
        }
        
        private Button CreateTopNavButton(Transform parent, string name, string icon, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = CreatePanel(name, parent);
            Button button = btnObj.AddComponent<Button>();
            Image btnImage = btnObj.GetComponent<Image>();
            btnImage.color = new Color(0.2f, 0.1f, 0.3f, 0.8f); // Purple
            
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            iconObj.layer = 5;
            
            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = icon;
            iconText.fontSize = 32;
            iconText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            iconText.alignment = TextAlignmentOptions.Center;
            
            SetFullScreen(iconObj.GetComponent<RectTransform>());
            
            // Position
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        private void CreateTopRightButtons(Transform parent)
        {
            // Settings button
            settingsButton = CreateTopNavButton(parent, "Settings", "⚙️",
                new Vector2(0.8f, 0.1f), new Vector2(0.9f, 0.9f));
            
            // Logout button
            logoutButton = CreateTopNavButton(parent, "Logout", "🚪",
                new Vector2(0.9f, 0.1f), new Vector2(1f, 0.9f));
        }
        
        private void CreateMainContent(Transform parent)
        {
            GameObject content = CreatePanel("Main Content", parent);
            content.GetComponent<Image>().color = Color.clear; // Transparent
            
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.05f, 0.1f);
            contentRect.anchorMax = new Vector2(0.95f, 0.9f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Welcome message
            GameObject welcomeObj = new GameObject("Welcome Message");
            welcomeObj.transform.SetParent(content.transform, false);
            welcomeObj.layer = 5;
            
            welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
            welcomeText.text = "🎉 WELCOME TO THE POTATO CARD ARENA!\n\nReady for epic battles?";
            welcomeText.fontSize = 40;
            welcomeText.color = Color.white;
            welcomeText.alignment = TextAlignmentOptions.Center;
            welcomeText.fontStyle = FontStyles.Bold;
            
            RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
            welcomeRect.anchorMin = new Vector2(0f, 0.7f);
            welcomeRect.anchorMax = new Vector2(1f, 0.9f);
            welcomeRect.offsetMin = Vector2.zero;
            welcomeRect.offsetMax = Vector2.zero;
            
            // User stats
            GameObject statsObj = new GameObject("User Stats");
            statsObj.transform.SetParent(content.transform, false);
            statsObj.layer = 5;
            
            userStatsText = statsObj.AddComponent<TextMeshProUGUI>();
            userStatsText.text = "Loading your stats...";
            userStatsText.fontSize = 28;
            userStatsText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            userStatsText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0f, 0.5f);
            statsRect.anchorMax = new Vector2(1f, 0.65f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // Collection stats
            GameObject collectionObj = new GameObject("Collection Stats");
            collectionObj.transform.SetParent(content.transform, false);
            collectionObj.layer = 5;
            
            collectionStatsText = collectionObj.AddComponent<TextMeshProUGUI>();
            collectionStatsText.text = "Loading collection...";
            collectionStatsText.fontSize = 24;
            collectionStatsText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            collectionStatsText.alignment = TextAlignmentOptions.Center;
            
            RectTransform collectionRect = collectionObj.GetComponent<RectTransform>();
            collectionRect.anchorMin = new Vector2(0f, 0.35f);
            collectionRect.anchorMax = new Vector2(1f, 0.45f);
            collectionRect.offsetMin = Vector2.zero;
            collectionRect.offsetMax = Vector2.zero;
        }
        
        private void CreateBattleButton(Transform parent)
        {
            // Battle button (bottom right corner)
            GameObject battleBtnObj = CreatePanel("Battle Button", parent);
            battleButton = battleBtnObj.AddComponent<Button>();
            Image battleImage = battleBtnObj.GetComponent<Image>();
            battleImage.color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
            
            // Battle text
            GameObject battleTextObj = new GameObject("Battle Text");
            battleTextObj.transform.SetParent(battleBtnObj.transform, false);
            battleTextObj.layer = 5;
            
            battleButtonText = battleTextObj.AddComponent<TextMeshProUGUI>();
            battleButtonText.text = "⚔️\nBATTLE";
            battleButtonText.fontSize = 36;
            battleButtonText.color = Color.white;
            battleButtonText.alignment = TextAlignmentOptions.Center;
            battleButtonText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(battleTextObj.GetComponent<RectTransform>());
            
            // Position in bottom right
            RectTransform battleRect = battleBtnObj.GetComponent<RectTransform>();
            battleRect.anchorMin = new Vector2(0.7f, 0.02f);
            battleRect.anchorMax = new Vector2(0.98f, 0.22f);
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;
        }
        
        private void CreateMatchmakingPanel(Transform parent)
        {
            matchmakingPanel = CreatePanel("Matchmaking Panel", parent);
            Image panelBg = matchmakingPanel.GetComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.8f); // Dark overlay
            
            SetFullScreen(matchmakingPanel.GetComponent<RectTransform>());
            matchmakingPanel.SetActive(false); // Hidden by default
            
            // Matchmaking status
            GameObject statusObj = new GameObject("Matchmaking Status");
            statusObj.transform.SetParent(matchmakingPanel.transform, false);
            statusObj.layer = 5;
            
            matchmakingStatusText = statusObj.AddComponent<TextMeshProUGUI>();
            matchmakingStatusText.text = "🔍 Searching for opponent...";
            matchmakingStatusText.fontSize = 48;
            matchmakingStatusText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            matchmakingStatusText.alignment = TextAlignmentOptions.Center;
            matchmakingStatusText.fontStyle = FontStyles.Bold;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.1f, 0.4f);
            statusRect.anchorMax = new Vector2(0.9f, 0.6f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            
            // Cancel button
            GameObject cancelBtnObj = CreatePanel("Cancel Button", matchmakingPanel.transform);
            cancelMatchmakingButton = cancelBtnObj.AddComponent<Button>();
            Image cancelImage = cancelBtnObj.GetComponent<Image>();
            cancelImage.color = new Color(0.7f, 0.3f, 0.3f, 1f); // Red
            
            GameObject cancelTextObj = new GameObject("Cancel Text");
            cancelTextObj.transform.SetParent(cancelBtnObj.transform, false);
            cancelTextObj.layer = 5;
            
            TextMeshProUGUI cancelText = cancelTextObj.AddComponent<TextMeshProUGUI>();
            cancelText.text = "❌ CANCEL";
            cancelText.fontSize = 32;
            cancelText.color = Color.white;
            cancelText.alignment = TextAlignmentOptions.Center;
            cancelText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(cancelTextObj.GetComponent<RectTransform>());
            
            RectTransform cancelRect = cancelBtnObj.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.3f, 0.2f);
            cancelRect.anchorMax = new Vector2(0.7f, 0.35f);
            cancelRect.offsetMin = Vector2.zero;
            cancelRect.offsetMax = Vector2.zero;
        }
        
        private void SetupEventListeners()
        {
            // Top navigation
            if (collectionButton) collectionButton.onClick.AddListener(() => NavigateToScreen("collection"));
            if (deckBuilderButton) deckBuilderButton.onClick.AddListener(() => NavigateToScreen("deck-builder"));
            if (heroHallButton) heroHallButton.onClick.AddListener(() => NavigateToScreen("hero-hall"));
            if (settingsButton) settingsButton.onClick.AddListener(OnSettingsPressed);
            if (logoutButton) logoutButton.onClick.AddListener(OnLogoutPressed);
            
            // Battle system
            if (battleButton) battleButton.onClick.AddListener(OnBattlePressed);
            if (cancelMatchmakingButton) cancelMatchmakingButton.onClick.AddListener(OnCancelMatchmaking);
            
            // Subscribe to game flow events
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged += OnGameFlowChanged;
            }
            
            // Subscribe to battle events
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
            }
            
            // Subscribe to authentication events
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnUserProfileLoaded += OnUserProfileLoaded;
            }
        }
        
        private void NavigateToScreen(string screenName)
        {
            Debug.Log($"🎯 Navigating to: {screenName}");
            
            if (GameFlowManager.Instance != null)
            {
                switch (screenName)
                {
                    case "collection":
                        GameFlowManager.Instance.NavigateToCollection();
                        break;
                    case "deck-builder":
                        GameFlowManager.Instance.NavigateToDeckBuilder();
                        break;
                    case "hero-hall":
                        GameFlowManager.Instance.NavigateToHeroHall();
                        break;
                }
            }
        }
        
        private async void OnBattlePressed()
        {
            Debug.Log("⚔️ Battle button pressed!");
            
            if (isInMatchmaking)
            {
                Debug.Log("⚠️ Already in matchmaking");
                return;
            }
            
            // Check if user has valid deck (like web version)
            if (!hasValidDeck)
            {
                ShowBattleError("You need a valid 30-card deck to battle!");
                return;
            }
            
            // Start matchmaking
            SetMatchmakingState(true, "🔍 Searching for worthy opponent...");
            
            bool success = await BattleManager.Instance.StartMatchmaking();
            if (!success)
            {
                SetMatchmakingState(false, "");
                ShowBattleError("Failed to start matchmaking. Please try again.");
            }
        }
        
        private void OnCancelMatchmaking()
        {
            Debug.Log("❌ Cancelling matchmaking");
            
            BattleManager.Instance.CancelMatchmaking();
            SetMatchmakingState(false, "");
        }
        
        private async void OnSettingsPressed()
        {
            Debug.Log("⚙️ Settings pressed");
            // TODO: Implement settings screen
        }
        
        private async void OnLogoutPressed()
        {
            Debug.Log("🚪 Logout pressed");
            
            await SupabaseClient.Instance.SignOut();
            // Authentication event will handle the flow change
        }
        
        private void OnGameFlowChanged(GameFlowManager.GameFlow newFlow)
        {
            // Handle visibility based on game flow
            bool shouldShowMainScreen = newFlow == GameFlowManager.GameFlow.MainMenu;
            gameObject.SetActive(shouldShowMainScreen);
        }
        
        private void OnBattleStateChanged(BattleManager.BattleState newState)
        {
            switch (newState)
            {
                case BattleManager.BattleState.Searching:
                    SetMatchmakingState(true, "🔍 Searching for opponent...");
                    break;
                    
                case BattleManager.BattleState.Initializing:
                    SetMatchmakingState(true, "⚡ Match found! Initializing battle...");
                    break;
                    
                case BattleManager.BattleState.InProgress:
                    SetMatchmakingState(false, "");
                    // Battle UI will take over
                    break;
                    
                case BattleManager.BattleState.NotInBattle:
                    SetMatchmakingState(false, "");
                    break;
            }
        }
        
        private void OnUserProfileLoaded(SupabaseClient.UserProfile profile)
        {
            if (userNameText)
            {
                string displayName = !string.IsNullOrEmpty(profile.display_name) ? 
                    profile.display_name : profile.email.Split('@')[0];
                userNameText.text = $"Welcome, {displayName}!";
            }
            
            LoadUserStats();
        }
        
        private async void LoadUserData()
        {
            if (!SupabaseClient.Instance.IsAuthenticated) return;
            
            // Load user stats and collection info
            await LoadUserStats();
            await LoadCollectionStats();
            await CheckDeckValidity();
        }
        
        private async System.Threading.Tasks.Task LoadUserStats()
        {
            try
            {
                // TODO: Load actual user stats from database
                if (userStatsText)
                {
                    userStatsText.text = "🏆 Battles: Loading... | 🎯 Wins: Loading... | 📊 Level: Loading...";
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading user stats: {e.Message}");
            }
        }
        
        private async System.Threading.Tasks.Task LoadCollectionStats()
        {
            try
            {
                // TODO: Load collection stats from database like web version
                if (collectionStatsText)
                {
                    collectionStatsText.text = "📚 Cards: Loading... | ✨ Unique: Loading... | 📈 Collection: Loading...";
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading collection stats: {e.Message}");
            }
        }
        
        private async System.Threading.Tasks.Task CheckDeckValidity()
        {
            try
            {
                string userId = SupabaseClient.Instance.GetUserId();
                hasValidDeck = await BattleManager.Instance.CheckUserHasValidDeck(userId);
                
                UpdateBattleButtonState();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error checking deck validity: {e.Message}");
                hasValidDeck = false;
                UpdateBattleButtonState();
            }
        }
        
        private void UpdateBattleButtonState()
        {
            if (battleButton == null || battleButtonText == null) return;
            
            if (hasValidDeck)
            {
                battleButton.interactable = true;
                battleButtonText.text = "⚔️\nBATTLE";
                battleButton.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
            }
            else
            {
                battleButton.interactable = false;
                battleButtonText.text = "❌\nNO DECK";
                battleButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
            }
        }
        
        private void SetMatchmakingState(bool searching, string statusMessage)
        {
            isInMatchmaking = searching;
            
            if (matchmakingPanel) matchmakingPanel.SetActive(searching);
            if (matchmakingStatusText && searching) matchmakingStatusText.text = statusMessage;
            
            if (battleButton) battleButton.interactable = !searching;
            
            if (searching)
            {
                StartCoroutine(AnimateMatchmakingStatus());
            }
        }
        
        private void ShowBattleError(string message)
        {
            Debug.LogWarning($"⚠️ Battle Error: {message}");
            
            // TODO: Show error popup or toast
            if (userStatsText)
            {
                userStatsText.text = $"❌ {message}";
                userStatsText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red
                
                StartCoroutine(ResetStatsTextAfterDelay());
            }
        }
        
        private IEnumerator ResetStatsTextAfterDelay()
        {
            yield return new WaitForSeconds(3f);
            LoadUserStats();
            if (userStatsText) userStatsText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }
        
        private IEnumerator AnimateMatchmakingStatus()
        {
            while (isInMatchmaking && matchmakingStatusText != null)
            {
                // Pulse animation
                float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.1f;
                matchmakingStatusText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            
            if (matchmakingStatusText)
            {
                matchmakingStatusText.transform.localScale = Vector3.one;
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
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged -= OnGameFlowChanged;
            }
            
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
            }
            
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnUserProfileLoaded -= OnUserProfileLoaded;
            }
        }
    }
}