using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PotatoCardGame.Core
{
    /// <summary>
    /// Main game manager that handles game state, scene transitions, and core game flow
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool debugMode = false;
        
        // Singleton pattern
        public static GameManager Instance { get; private set; }
        
        // Game state
        public enum GameState
        {
            MainMenu,
            Collection,
            DeckBuilder,
            Battle,
            HeroHall,
            Loading
        }
        
        [SerializeField] private GameState currentState = GameState.Loading;
        public GameState CurrentState => currentState;
        
        // Events
        public System.Action<GameState> OnGameStateChanged;
        
        private void Awake()
        {
            // Singleton setup
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
            Debug.Log("🎮 Game Manager Initialized");
            
            // Initialize core systems
            if (debugMode)
            {
                Debug.Log("🐛 Debug Mode Enabled");
            }
            
            // Start the game flow
            StartCoroutine(InitializeGame());
        }
        
        private IEnumerator InitializeGame()
        {
            // Wait for other systems to initialize
            yield return new WaitForSeconds(0.1f);
            
            // Check authentication and show appropriate UI
            if (SupabaseClient.Instance != null && SupabaseClient.Instance.IsAuthenticated)
            {
                // User is logged in, go to main menu
                ChangeGameState(GameState.MainMenu);
            }
            else
            {
                // User needs to log in, this will be handled by LoginScreen
                Debug.Log("🔐 User not authenticated, login screen will handle this");
            }
        }
        
        public void ChangeGameState(GameState newState)
        {
            if (currentState == newState) return;
            
            GameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"🔄 Game State Changed: {previousState} → {newState}");
            OnGameStateChanged?.Invoke(newState);
        }
        
        public void LoadScene(string sceneName)
        {
            ChangeGameState(GameState.Loading);
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                // You can add loading progress UI here
                yield return null;
            }
            
            Debug.Log($"📱 Scene Loaded: {sceneName}");
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"📱 Application Paused: {pauseStatus}");
            // Handle mobile app pause/resume
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            Debug.Log($"📱 Application Focus: {hasFocus}");
            // Handle mobile app focus changes
        }
    }
}