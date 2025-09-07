using UnityEngine;
using System.Collections.Generic;
using PotatoLegends.Core;

namespace PotatoLegends.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Screen Management")]
        [SerializeField] private AuthScreen authScreen;
        [SerializeField] private MainMenuScreen mainMenuScreen;
        [SerializeField] private CollectionScreen collectionScreen;
        [SerializeField] private DeckBuilderScreen deckBuilderScreen;
        [SerializeField] private HeroHallScreen heroHallScreen;
        [SerializeField] private BattleScreen battleScreen;
        [SerializeField] private GameObject loadingScreen;

        private Dictionary<GameManager.GameState, GameObject> screenMap;
        private GameObject currentActiveScreen;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            screenMap = new Dictionary<GameManager.GameState, GameObject>
            {
                { GameManager.GameState.Auth, authScreen?.gameObject },
                { GameManager.GameState.MainMenu, mainMenuScreen?.gameObject },
                { GameManager.GameState.Collection, collectionScreen?.gameObject },
                { GameManager.GameState.DeckBuilder, deckBuilderScreen?.gameObject },
                { GameManager.GameState.HeroHall, heroHallScreen?.gameObject },
                { GameManager.GameState.Battle, battleScreen?.gameObject }
            };

            foreach (var screenObj in screenMap.Values)
            {
                if (screenObj != null) screenObj.SetActive(false);
            }
            if (loadingScreen != null) loadingScreen.SetActive(false);
        }

        void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            ShowScreen(newState);
        }

        public void ShowScreen(GameManager.GameState state)
        {
            if (currentActiveScreen != null)
            {
                currentActiveScreen.SetActive(false);
            }

            if (screenMap.TryGetValue(state, out GameObject targetScreen) && targetScreen != null)
            {
                targetScreen.SetActive(true);
                currentActiveScreen = targetScreen;
                Debug.Log($"UIManager: Displaying screen for state: {state}");
            }
            else
            {
                Debug.LogWarning($"UIManager: No screen found for state: {state}. Defaulting to MainMenu.");
                if (mainMenuScreen != null)
                {
                    mainMenuScreen.gameObject.SetActive(true);
                    currentActiveScreen = mainMenuScreen.gameObject;
                }
                else
                {
                    Debug.LogError("UIManager: MainMenuScreen is not assigned!");
                }
            }

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(state == GameManager.GameState.Loading);
            }
        }

        public void ShowLoadingScreen(bool show)
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(show);
            }
        }
    }
}