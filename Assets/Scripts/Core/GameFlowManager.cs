using System.Collections;
using UnityEngine;
using PotatoCardGame.Network;
using PotatoCardGame.UI;
using PotatoCardGame.Battle;

namespace PotatoCardGame.Core
{
    /// <summary>
    /// Manages the complete game flow matching the web version:
    /// 1. Authentication screen (required login/register)
    /// 2. Main screen with navigation (collection, deck builder, hero hall)
    /// 3. Battle arena with full game mechanics
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Game Flow Settings")]
        [SerializeField] private bool requireAuthentication = true;
        [SerializeField] private bool autoStartBattle = false;
        
        // Singleton
        public static GameFlowManager Instance { get; private set; }
        
        // Game flow state
        public enum GameFlow
        {
            Authentication,  // Login/Register screen
            MainMenu,       // Home screen with navigation
            Collection,     // Card collection browser
            DeckBuilder,    // Deck building interface
            HeroHall,       // Hero selection and management
            Battle,         // Battle arena
            Loading         // Transition states
        }
        
        private GameFlow currentFlow = GameFlow.Authentication;
        
        // Events
        public System.Action<GameFlow> OnGameFlowChanged;
        public System.Action OnAuthenticationComplete;
        public System.Action<string> OnBattleStarted;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            Debug.Log("🎮 Game Flow Manager Initialized");
            
            // Subscribe to authentication events
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged += OnAuthenticationChanged;
            }
            
            // Subscribe to battle events
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
            }
        }
        
        private void Start()
        {
            StartCoroutine(InitializeGameFlow());
        }
        
        private IEnumerator InitializeGameFlow()
        {
            // Wait for all systems to initialize
            yield return new WaitForSeconds(0.5f);
            
            // Check if user is already authenticated
            if (SupabaseClient.Instance != null && SupabaseClient.Instance.IsAuthenticated)
            {
                Debug.Log("🔐 User already authenticated, proceeding to main menu");
                ChangeGameFlow(GameFlow.MainMenu);
            }
            else
            {
                Debug.Log("🔐 User not authenticated, showing authentication screen");
                ChangeGameFlow(GameFlow.Authentication);
            }
        }
        
        public void ChangeGameFlow(GameFlow newFlow)
        {
            if (currentFlow == newFlow) return;
            
            GameFlow previousFlow = currentFlow;
            currentFlow = newFlow;
            
            Debug.Log($"🔄 Game Flow Changed: {previousFlow} → {newFlow}");
            
            // Update GameManager state to match
            if (GameManager.Instance != null)
            {
                GameManager.GameState gameState = ConvertFlowToGameState(newFlow);
                GameManager.Instance.ChangeGameState(gameState);
            }
            
            OnGameFlowChanged?.Invoke(newFlow);
        }
        
        private GameManager.GameState ConvertFlowToGameState(GameFlow flow)
        {
            return flow switch
            {
                GameFlow.Authentication => GameManager.GameState.Loading,
                GameFlow.MainMenu => GameManager.GameState.MainMenu,
                GameFlow.Collection => GameManager.GameState.Collection,
                GameFlow.DeckBuilder => GameManager.GameState.DeckBuilder,
                GameFlow.HeroHall => GameManager.GameState.HeroHall,
                GameFlow.Battle => GameManager.GameState.Battle,
                GameFlow.Loading => GameManager.GameState.Loading,
                _ => GameManager.GameState.MainMenu
            };
        }
        
        #region Authentication Flow
        
        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                Debug.Log("✅ Authentication successful, proceeding to main menu");
                OnAuthenticationComplete?.Invoke();
                ChangeGameFlow(GameFlow.MainMenu);
            }
            else
            {
                Debug.Log("❌ Authentication lost, returning to auth screen");
                ChangeGameFlow(GameFlow.Authentication);
            }
        }
        
        public void RequestAuthentication()
        {
            ChangeGameFlow(GameFlow.Authentication);
        }
        
        #endregion
        
        #region Navigation
        
        public void NavigateToCollection()
        {
            if (!IsAuthenticated()) return;
            ChangeGameFlow(GameFlow.Collection);
        }
        
        public void NavigateToDeckBuilder()
        {
            if (!IsAuthenticated()) return;
            ChangeGameFlow(GameFlow.DeckBuilder);
        }
        
        public void NavigateToHeroHall()
        {
            if (!IsAuthenticated()) return;
            ChangeGameFlow(GameFlow.HeroHall);
        }
        
        public void NavigateToMainMenu()
        {
            if (!IsAuthenticated()) return;
            ChangeGameFlow(GameFlow.MainMenu);
        }
        
        #endregion
        
        #region Battle Flow
        
        public async void StartBattle()
        {
            if (!IsAuthenticated())
            {
                Debug.LogWarning("⚠️ Cannot start battle - user not authenticated");
                return;
            }
            
            Debug.Log("⚔️ Starting battle flow...");
            
            // Check if user has valid deck
            bool hasValidDeck = await CheckUserDeck();
            if (!hasValidDeck)
            {
                Debug.LogWarning("⚠️ Cannot start battle - invalid deck");
                // TODO: Show deck validation error UI
                return;
            }
            
            // Start matchmaking
            bool matchmakingStarted = await BattleManager.Instance.StartMatchmaking();
            if (matchmakingStarted)
            {
                ChangeGameFlow(GameFlow.Loading); // Show searching state
            }
        }
        
        private async System.Threading.Tasks.Task<bool> CheckUserDeck()
        {
            if (BattleManager.Instance == null) return false;
            
            string userId = SupabaseClient.Instance.GetUserId();
            return await BattleManager.Instance.checkUserHasValidDeck(userId);
        }
        
        private void OnBattleStateChanged(BattleManager.BattleState newState)
        {
            switch (newState)
            {
                case BattleManager.BattleState.Searching:
                    ChangeGameFlow(GameFlow.Loading);
                    break;
                    
                case BattleManager.BattleState.Initializing:
                    ChangeGameFlow(GameFlow.Loading);
                    break;
                    
                case BattleManager.BattleState.InProgress:
                    ChangeGameFlow(GameFlow.Battle);
                    break;
                    
                case BattleManager.BattleState.Ended:
                case BattleManager.BattleState.NotInBattle:
                    ChangeGameFlow(GameFlow.MainMenu);
                    break;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private bool IsAuthenticated()
        {
            return SupabaseClient.Instance != null && SupabaseClient.Instance.IsAuthenticated;
        }
        
        public GameFlow GetCurrentFlow()
        {
            return currentFlow;
        }
        
        public bool IsInBattle()
        {
            return currentFlow == GameFlow.Battle;
        }
        
        public bool RequiresAuthentication()
        {
            return requireAuthentication;
        }
        
        public void NavigateToAuth()
        {
            ChangeGameFlow(GameFlow.Authentication);
        }
        
        public void NavigateToBattle()
        {
            ChangeGameFlow(GameFlow.Battle);
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (SupabaseClient.Instance != null)
            {
                SupabaseClient.Instance.OnAuthenticationChanged -= OnAuthenticationChanged;
            }
            
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
            }
        }
    }
}