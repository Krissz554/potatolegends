using UnityEngine;
using UnityEngine.SceneManagement;
using PotatoCardGame.Network;
using PotatoCardGame.Cards;
using PotatoCardGame.UI;

namespace PotatoCardGame.Core
{
    /// <summary>
    /// REAL Game Manager that coordinates the complete game
    /// Manages all real systems and screens exactly like your web version
    /// </summary>
    public class RealGameManager : MonoBehaviour
    {
        [Header("Game Systems")]
        [SerializeField] private RealSupabaseClient supabaseClient;
        [SerializeField] private RealCardManager cardManager;
        [SerializeField] private GameFlowManager gameFlowManager;
        
        [Header("Game Screens")]
        [SerializeField] private RealAuthScreen authScreen;
        [SerializeField] private RealMainMenu mainMenu;
        [SerializeField] private RealCollectionScreen collectionScreen;
        [SerializeField] private RealDeckBuilder deckBuilder;
        [SerializeField] private RealHeroHall heroHall;
        
        // Singleton
        public static RealGameManager Instance { get; private set; }
        
        // Game state
        private bool isInitialized = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeGame()
        {
            Debug.Log("🚀 Initializing REAL Potato Legends Mobile Game...");
            
            // Create all systems if they don't exist
            CreateGameSystems();
            CreateGameScreens();
            
            // Setup event connections
            SetupEventConnections();
            
            // Start the game flow
            StartGameFlow();
            
            isInitialized = true;
            
            Debug.Log("✅ REAL Game initialization complete!");
        }
        
        private void CreateGameSystems()
        {
            // Create Supabase client
            if (supabaseClient == null)
            {
                GameObject supabaseObj = new GameObject("RealSupabaseClient");
                supabaseObj.transform.SetParent(transform);
                supabaseClient = supabaseObj.AddComponent<RealSupabaseClient>();
                Debug.Log("✅ Real Supabase Client created");
            }
            
            // Create card manager
            if (cardManager == null)
            {
                GameObject cardManagerObj = new GameObject("RealCardManager");
                cardManagerObj.transform.SetParent(transform);
                cardManager = cardManagerObj.AddComponent<RealCardManager>();
                Debug.Log("✅ Real Card Manager created");
            }
            
            // Create game flow manager
            if (gameFlowManager == null)
            {
                GameObject gameFlowObj = new GameObject("GameFlowManager");
                gameFlowObj.transform.SetParent(transform);
                gameFlowManager = gameFlowObj.AddComponent<GameFlowManager>();
                Debug.Log("✅ Game Flow Manager created");
            }
        }
        
        private void CreateGameScreens()
        {
            // Create auth screen
            if (authScreen == null)
            {
                GameObject authObj = new GameObject("RealAuthScreen");
                authObj.transform.SetParent(transform);
                authScreen = authObj.AddComponent<RealAuthScreen>();
                Debug.Log("✅ Real Auth Screen created");
            }
            
            // Create main menu
            if (mainMenu == null)
            {
                GameObject mainMenuObj = new GameObject("RealMainMenu");
                mainMenuObj.transform.SetParent(transform);
                mainMenu = mainMenuObj.AddComponent<RealMainMenu>();
                Debug.Log("✅ Real Main Menu created");
            }
            
            // Create collection screen
            if (collectionScreen == null)
            {
                GameObject collectionObj = new GameObject("RealCollectionScreen");
                collectionObj.transform.SetParent(transform);
                collectionScreen = collectionObj.AddComponent<RealCollectionScreen>();
                Debug.Log("✅ Real Collection Screen created");
            }
            
            // Create deck builder
            if (deckBuilder == null)
            {
                GameObject deckBuilderObj = new GameObject("RealDeckBuilder");
                deckBuilderObj.transform.SetParent(transform);
                deckBuilder = deckBuilderObj.AddComponent<RealDeckBuilder>();
                Debug.Log("✅ Real Deck Builder created");
            }
            
            // Create hero hall
            if (heroHall == null)
            {
                GameObject heroHallObj = new GameObject("RealHeroHall");
                heroHallObj.transform.SetParent(transform);
                heroHall = heroHallObj.AddComponent<RealHeroHall>();
                Debug.Log("✅ Real Hero Hall created");
            }
        }
        
        private void SetupEventConnections()
        {
            Debug.Log("🔗 Setting up event connections...");
            
            // Authentication flow
            if (supabaseClient != null)
            {
                supabaseClient.OnAuthenticationChanged += OnAuthenticationChanged;
                supabaseClient.OnError += OnSupabaseError;
                supabaseClient.OnUserProfileLoaded += OnUserProfileLoaded;
            }
            
            // Game flow
            if (gameFlowManager != null)
            {
                gameFlowManager.OnGameFlowChanged += OnGameFlowChanged;
            }
            
            Debug.Log("✅ Event connections established");
        }
        
        private void StartGameFlow()
        {
            Debug.Log("🎮 Starting game flow...");
            
            // Check if user is already authenticated
            if (RealSupabaseClient.Instance?.IsAuthenticated == true)
            {
                Debug.Log("🔐 User already authenticated - going to main menu");
                NavigateToMainMenu();
            }
            else
            {
                Debug.Log("🔐 No authentication - showing login screen");
                NavigateToAuth();
            }
        }
        
        #region Navigation Methods (Public API)
        
        public void NavigateToAuth()
        {
            ChangeGameFlow(GameFlowManager.GameFlow.Authentication);
        }
        
        public void NavigateToMainMenu()
        {
            ChangeGameFlow(GameFlowManager.GameFlow.MainMenu);
        }
        
        public void NavigateToCollection()
        {
            ChangeGameFlow(GameFlowManager.GameFlow.Collection);
        }
        
        public void NavigateToDeckBuilder()
        {
            ChangeGameFlow(GameFlowManager.GameFlow.DeckBuilder);
        }
        
        public void NavigateToHeroHall()
        {
            ChangeGameFlow(GameFlowManager.GameFlow.HeroHall);
        }
        
        public void NavigateToBattle()
        {
            ChangeGameFlow(GameFlowManager.GameFlow.Battle);
        }
        
        private void ChangeGameFlow(GameFlowManager.GameFlow newFlow)
        {
            if (gameFlowManager != null)
            {
                Debug.Log($"🔄 Changing game flow to: {newFlow}");
                
                // Update game flow manager
                var currentFlowField = typeof(GameFlowManager).GetField("currentFlow", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                currentFlowField?.SetValue(gameFlowManager, newFlow);
                
                // Notify all listeners
                gameFlowManager.OnGameFlowChanged?.Invoke(newFlow);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            Debug.Log($"🔐 Authentication changed: {isAuthenticated}");
            
            if (isAuthenticated)
            {
                // User logged in - go to main menu
                NavigateToMainMenu();
            }
            else
            {
                // User logged out - go to auth
                NavigateToAuth();
            }
        }
        
        private void OnSupabaseError(string error)
        {
            Debug.LogError($"❌ Supabase Error: {error}");
            // Handle errors appropriately
        }
        
        private void OnUserProfileLoaded(RealSupabaseClient.UserProfile userProfile)
        {
            Debug.Log($"👤 User profile loaded: {userProfile.display_name ?? userProfile.email}");
        }
        
        private void OnGameFlowChanged(GameFlowManager.GameFlow newFlow)
        {
            Debug.Log($"🎮 Game flow changed to: {newFlow}");
            
            // Update screen visibility based on flow
            UpdateScreenVisibility(newFlow);
        }
        
        private void UpdateScreenVisibility(GameFlowManager.GameFlow flow)
        {
            // Hide all screens first
            if (authScreen) authScreen.gameObject.SetActive(false);
            if (mainMenu) mainMenu.gameObject.SetActive(false);
            if (collectionScreen) collectionScreen.gameObject.SetActive(false);
            if (deckBuilder) deckBuilder.gameObject.SetActive(false);
            if (heroHall) heroHall.gameObject.SetActive(false);
            
            // Show the appropriate screen
            switch (flow)
            {
                case GameFlowManager.GameFlow.Authentication:
                    if (authScreen) authScreen.gameObject.SetActive(true);
                    break;
                    
                case GameFlowManager.GameFlow.MainMenu:
                    if (mainMenu) mainMenu.gameObject.SetActive(true);
                    break;
                    
                case GameFlowManager.GameFlow.Collection:
                    if (collectionScreen) collectionScreen.gameObject.SetActive(true);
                    break;
                    
                case GameFlowManager.GameFlow.DeckBuilder:
                    if (deckBuilder) deckBuilder.gameObject.SetActive(true);
                    break;
                    
                case GameFlowManager.GameFlow.HeroHall:
                    if (heroHall) heroHall.gameObject.SetActive(true);
                    break;
                    
                case GameFlowManager.GameFlow.Battle:
                    // TODO: Show battle arena
                    Debug.Log("⚔️ Entering battle arena...");
                    break;
                    
                default:
                    Debug.Log($"🔄 Unknown game flow: {flow}");
                    break;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        public bool IsAuthenticated()
        {
            return RealSupabaseClient.Instance?.IsAuthenticated ?? false;
        }
        
        public string GetUserId()
        {
            return RealSupabaseClient.Instance?.UserId ?? "";
        }
        
        public RealSupabaseClient.UserProfile GetCurrentUser()
        {
            return RealSupabaseClient.Instance?.CurrentUser;
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (supabaseClient != null)
            {
                supabaseClient.OnAuthenticationChanged -= OnAuthenticationChanged;
                supabaseClient.OnError -= OnSupabaseError;
                supabaseClient.OnUserProfileLoaded -= OnUserProfileLoaded;
            }
            
            if (gameFlowManager != null)
            {
                gameFlowManager.OnGameFlowChanged -= OnGameFlowChanged;
            }
        }
        
        #region Debug Methods
        
        [ContextMenu("Force Navigate to Auth")]
        public void DebugNavigateToAuth()
        {
            NavigateToAuth();
        }
        
        [ContextMenu("Force Navigate to Main Menu")]
        public void DebugNavigateToMainMenu()
        {
            NavigateToMainMenu();
        }
        
        [ContextMenu("Force Navigate to Collection")]
        public void DebugNavigateToCollection()
        {
            NavigateToCollection();
        }
        
        [ContextMenu("Force Navigate to Deck Builder")]
        public void DebugNavigateToDeckBuilder()
        {
            NavigateToDeckBuilder();
        }
        
        [ContextMenu("Force Navigate to Hero Hall")]
        public void DebugNavigateToHeroHall()
        {
            NavigateToHeroHall();
        }
        
        #endregion
    }
}