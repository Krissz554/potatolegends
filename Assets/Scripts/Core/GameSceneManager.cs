using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace PotatoLegends.Core
{
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string authSceneName = "Auth";
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string collectionSceneName = "Collection";
        [SerializeField] private string deckBuilderSceneName = "DeckBuilder";
        [SerializeField] private string heroHallSceneName = "HeroHall";
        [SerializeField] private string battleSceneName = "Battle";

        [Header("Loading Settings")]
        [SerializeField] private float minimumLoadingTime = 1f;
        [SerializeField] private GameObject loadingScreenPrefab;

        private bool isLoading = false;
        private GameObject currentLoadingScreen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Start the app flow
            StartAppFlow();
        }

        private void StartAppFlow()
        {
            // Check if user is already authenticated
            if (IsUserAuthenticated())
            {
                LoadMainMenu();
            }
            else
            {
                LoadAuth();
            }
        }

        private bool IsUserAuthenticated()
        {
            // Check if user has valid authentication token
            string token = PlayerPrefs.GetString("user_token");
            string userId = PlayerPrefs.GetString("user_id");
            
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                return false;
            }

            // Set the token in SupabaseClient if it exists
            if (PotatoLegends.Network.SupabaseClient.Instance != null)
            {
                PotatoLegends.Network.SupabaseClient.Instance.SetAccessToken(token);
            }

            return true;
        }

        #region Public Scene Loading Methods

        public void LoadAuth()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneCoroutine(authSceneName));
        }

        public void LoadMainMenu()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneCoroutine(mainMenuSceneName));
        }

        public void LoadCollection()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneCoroutine(collectionSceneName));
        }

        public void LoadDeckBuilder()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneCoroutine(deckBuilderSceneName));
        }

        public void LoadHeroHall()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneCoroutine(heroHallSceneName));
        }

        public void LoadBattle()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneCoroutine(battleSceneName));
        }

        public void StartMatchmaking()
        {
            if (isLoading) return;
            Debug.Log("🔍 Starting matchmaking...");
            // TODO: Implement actual matchmaking logic
            // For now, just load battle scene after a delay
            StartCoroutine(StartMatchmakingCoroutine());
        }

        #endregion

        #region Private Methods

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            isLoading = true;
            
            // Show loading screen
            ShowLoadingScreen();
            
            // Wait minimum loading time for smooth transition
            yield return new WaitForSeconds(minimumLoadingTime);
            
            // Load the scene
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            
            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Hide loading screen
            HideLoadingScreen();
            
            isLoading = false;
            
            Debug.Log($"✅ Loaded scene: {sceneName}");
        }

        private IEnumerator StartMatchmakingCoroutine()
        {
            isLoading = true;
            
            // Show matchmaking UI
            ShowMatchmakingUI();
            
            // Simulate matchmaking time
            yield return new WaitForSeconds(2f);
            
            // TODO: Replace with actual matchmaking result
            // For now, just load battle scene
            yield return StartCoroutine(LoadSceneCoroutine(battleSceneName));
        }

        private void ShowLoadingScreen()
        {
            if (loadingScreenPrefab != null)
            {
                currentLoadingScreen = Instantiate(loadingScreenPrefab);
            }
            else
            {
                // Create simple loading screen if no prefab
                CreateSimpleLoadingScreen();
            }
        }

        private void HideLoadingScreen()
        {
            if (currentLoadingScreen != null)
            {
                Destroy(currentLoadingScreen);
                currentLoadingScreen = null;
            }
        }

        private void ShowMatchmakingUI()
        {
            // TODO: Show matchmaking UI
            Debug.Log("🔍 Searching for opponents...");
        }

        private void CreateSimpleLoadingScreen()
        {
            // Create a simple loading screen programmatically
            GameObject loadingCanvas = new GameObject("LoadingCanvas");
            Canvas canvas = loadingCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            GameObject loadingText = new GameObject("LoadingText");
            loadingText.transform.SetParent(loadingCanvas.transform);
            
            // Add TextMeshPro component
            var textComponent = loadingText.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.text = "Loading...";
            textComponent.fontSize = 24;
            textComponent.color = Color.white;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;

            // Position in center of screen
            RectTransform rectTransform = loadingText.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            currentLoadingScreen = loadingCanvas;
        }

        #endregion

        #region Authentication Methods

        public void OnAuthenticationSuccess(string userToken)
        {
            // Save authentication token
            PlayerPrefs.SetString("user_token", userToken);
            PlayerPrefs.Save();
            
            Debug.Log("✅ User authenticated successfully");
            
            // Load main menu after successful authentication
            LoadMainMenu();
        }

        public void OnAuthenticationFailed(string errorMessage)
        {
            Debug.LogError($"❌ Authentication failed: {errorMessage}");
            // TODO: Show error message to user
        }

        public async void Logout()
        {
            Debug.Log("👋 User logging out");
            
            // Sign out from Supabase
            if (PotatoLegends.Network.SupabaseClient.Instance != null)
            {
                await PotatoLegends.Network.SupabaseClient.Instance.SignOut();
            }
            
            // Clear local authentication data
            PlayerPrefs.DeleteKey("user_token");
            PlayerPrefs.DeleteKey("user_id");
            PlayerPrefs.DeleteKey("user_email");
            PlayerPrefs.Save();
            
            Debug.Log("👋 User logged out");
            
            // Load auth scene
            LoadAuth();
        }

        #endregion
    }
}