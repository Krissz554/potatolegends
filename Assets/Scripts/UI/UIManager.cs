using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Main UI Manager that handles all screen transitions and UI state
    /// Manages the mobile-optimized interface for the card game
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Screen Management")]
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject collectionScreen;
        [SerializeField] private GameObject deckBuilderScreen;
        [SerializeField] private GameObject battleScreen;
        [SerializeField] private GameObject heroHallScreen;
        [SerializeField] private GameObject loadingScreen;
        
        [Header("Common UI Elements")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI screenTitle;
        [SerializeField] private GameObject topBar;
        [SerializeField] private GameObject bottomNavigation;
        
        [Header("Mobile Navigation")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button heroHallButton;
        [SerializeField] private Button battleButton;
        
        [Header("User Info")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI userLevelText;
        [SerializeField] private Image userAvatar;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button profileButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float screenTransitionDuration = 0.3f;
        [SerializeField] private Ease screenTransitionEase = Ease.OutQuad;
        
        // Singleton pattern
        public static UIManager Instance { get; private set; }
        
        // Current screen tracking
        private GameObject currentScreen;
        private GameManager.GameState currentUIState;
        
        // Events
        public System.Action<GameManager.GameState> OnScreenChanged;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            SetupMobileNavigation();
            ShowScreen(GameManager.GameState.MainMenu);
        }
        
        private void Initialize()
        {
            Debug.Log("📱 UI Manager Initialized for Mobile");
            
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
            
            // Setup back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonPressed);
            }
            
            // Setup settings
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OpenSettings);
            }
        }
        
        private void SetupMobileNavigation()
        {
            // Setup bottom navigation buttons
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => ShowScreen(GameManager.GameState.MainMenu));
                
            if (collectionButton != null)
                collectionButton.onClick.AddListener(() => ShowScreen(GameManager.GameState.Collection));
                
            if (deckBuilderButton != null)
                deckBuilderButton.onClick.AddListener(() => ShowScreen(GameManager.GameState.DeckBuilder));
                
            if (heroHallButton != null)
                heroHallButton.onClick.AddListener(() => ShowScreen(GameManager.GameState.HeroHall));
                
            if (battleButton != null)
                battleButton.onClick.AddListener(() => ShowScreen(GameManager.GameState.Battle));
        }
        
        public void ShowScreen(GameManager.GameState newState)
        {
            if (currentUIState == newState) return;
            
            StartCoroutine(TransitionToScreen(newState));
        }
        
        private IEnumerator TransitionToScreen(GameManager.GameState newState)
        {
            // Show loading if needed
            if (newState == GameManager.GameState.Loading)
            {
                if (loadingScreen != null)
                {
                    loadingScreen.SetActive(true);
                }
                yield break;
            }
            
            // Hide current screen
            if (currentScreen != null)
            {
                yield return StartCoroutine(HideScreen(currentScreen));
            }
            
            // Get new screen
            GameObject newScreen = GetScreenForState(newState);
            
            if (newScreen != null)
            {
                // Show new screen
                yield return StartCoroutine(ShowScreen(newScreen));
                currentScreen = newScreen;
            }
            
            // Update state
            currentUIState = newState;
            
            // Update UI elements
            UpdateTopBar(newState);
            UpdateBottomNavigation(newState);
            
            // Notify listeners
            OnScreenChanged?.Invoke(newState);
            
            // Update game manager state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(newState);
            }
            
            Debug.Log($"📱 UI Screen Changed to: {newState}");
        }
        
        private GameObject GetScreenForState(GameManager.GameState state)
        {
            return state switch
            {
                GameManager.GameState.MainMenu => mainMenuScreen,
                GameManager.GameState.Collection => collectionScreen,
                GameManager.GameState.DeckBuilder => deckBuilderScreen,
                GameManager.GameState.Battle => battleScreen,
                GameManager.GameState.HeroHall => heroHallScreen,
                _ => mainMenuScreen
            };
        }
        
        private IEnumerator ShowScreen(GameObject screen)
        {
            if (screen == null) yield break;
            
            screen.SetActive(true);
            
            // Animate in
            CanvasGroup canvasGroup = screen.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = screen.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, screenTransitionDuration).SetEase(screenTransitionEase);
            
            // Scale animation for mobile feel
            RectTransform rectTransform = screen.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * 0.9f;
                rectTransform.DOScale(Vector3.one, screenTransitionDuration).SetEase(Ease.OutBack);
            }
            
            yield return new WaitForSeconds(screenTransitionDuration);
        }
        
        private IEnumerator HideScreen(GameObject screen)
        {
            if (screen == null) yield break;
            
            CanvasGroup canvasGroup = screen.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, screenTransitionDuration * 0.5f).SetEase(Ease.InQuad);
            }
            
            yield return new WaitForSeconds(screenTransitionDuration * 0.5f);
            
            screen.SetActive(false);
        }
        
        private void UpdateTopBar(GameManager.GameState state)
        {
            if (screenTitle != null)
            {
                string title = state switch
                {
                    GameManager.GameState.MainMenu => "What's My Potato?",
                    GameManager.GameState.Collection => "Card Collection",
                    GameManager.GameState.DeckBuilder => "Deck Builder",
                    GameManager.GameState.Battle => "Battle Arena",
                    GameManager.GameState.HeroHall => "Hero Hall",
                    _ => "Potato Card Game"
                };
                
                screenTitle.text = title;
            }
            
            // Show/hide back button based on screen
            if (backButton != null)
            {
                backButton.gameObject.SetActive(state != GameManager.GameState.MainMenu);
            }
        }
        
        private void UpdateBottomNavigation(GameManager.GameState state)
        {
            // Update navigation button states
            UpdateNavButton(mainMenuButton, state == GameManager.GameState.MainMenu);
            UpdateNavButton(collectionButton, state == GameManager.GameState.Collection);
            UpdateNavButton(deckBuilderButton, state == GameManager.GameState.DeckBuilder);
            UpdateNavButton(heroHallButton, state == GameManager.GameState.HeroHall);
            UpdateNavButton(battleButton, state == GameManager.GameState.Battle);
        }
        
        private void UpdateNavButton(Button button, bool isActive)
        {
            if (button == null) return;
            
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isActive ? Color.white : Color.gray;
            }
            
            // Scale effect for active button
            if (isActive)
            {
                button.transform.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutBack);
            }
            else
            {
                button.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
            }
        }
        
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            ShowScreen(newState);
        }
        
        private void OnBackButtonPressed()
        {
            // Handle back navigation
            switch (currentUIState)
            {
                case GameManager.GameState.Collection:
                case GameManager.GameState.DeckBuilder:
                case GameManager.GameState.HeroHall:
                    ShowScreen(GameManager.GameState.MainMenu);
                    break;
                case GameManager.GameState.Battle:
                    // Show battle menu or return to main menu
                    ShowBattleMenu();
                    break;
                default:
                    ShowScreen(GameManager.GameState.MainMenu);
                    break;
            }
        }
        
        private void ShowBattleMenu()
        {
            // TODO: Implement battle pause menu
            Debug.Log("🛑 Battle menu not implemented yet");
        }
        
        private void OpenSettings()
        {
            // TODO: Implement settings modal
            Debug.Log("⚙️ Settings not implemented yet");
        }
        
        public void UpdateUserInfo(string username, int level, Sprite avatar = null)
        {
            if (usernameText != null)
                usernameText.text = username;
                
            if (userLevelText != null)
                userLevelText.text = $"Level {level}";
                
            if (userAvatar != null && avatar != null)
                userAvatar.sprite = avatar;
        }
        
        public void ShowLoadingScreen(bool show)
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(show);
            }
        }
        
        public void ShowToast(string message, float duration = 3f)
        {
            // TODO: Implement mobile toast notifications
            Debug.Log($"🍞 Toast: {message}");
        }
        
        // Mobile-specific methods
        public void HandleDeviceRotation()
        {
            // Handle screen orientation changes
            Debug.Log($"📱 Screen orientation: {Screen.orientation}");
        }
        
        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Game paused - save state, pause timers
                Debug.Log("⏸️ Game paused");
            }
            else
            {
                // Game resumed - restore state, resume timers
                Debug.Log("▶️ Game resumed");
            }
        }
        
        private void OnDestroy()
        {
            // Cleanup
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
            
            // Stop all tweens
            DOTween.KillAll();
        }
    }
}