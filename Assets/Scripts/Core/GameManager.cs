using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace PotatoLegends.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState { Loading, Auth, MainMenu, Collection, DeckBuilder, HeroHall, Battle }
        [SerializeField] private GameState currentState = GameState.Loading;
        public event Action<GameState> OnGameStateChanged;

        [Header("Scene Management")]
        [SerializeField] private string authSceneName = "AuthScene";
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";
        [SerializeField] private string battleSceneName = "BattleScene";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("GameManager: Initializing game...");
            ChangeGameState(GameState.Auth);
        }

        public void ChangeGameState(GameState newState)
        {
            if (currentState == newState) return;

            Debug.Log($"GameManager: Changing state from {currentState} to {newState}");
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Auth:
                    LoadScene(authSceneName);
                    break;
                case GameState.MainMenu:
                    LoadScene(mainMenuSceneName);
                    break;
                case GameState.Battle:
                    LoadScene(battleSceneName);
                    break;
                case GameState.Collection:
                case GameState.DeckBuilder:
                case GameState.HeroHall:
                    if (SceneManager.GetActiveScene().name != mainMenuSceneName)
                    {
                        LoadScene(mainMenuSceneName);
                    }
                    break;
            }
        }

        private void LoadScene(string sceneName)
        {
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                Debug.Log($"Scene {sceneName} is already active.");
                return;
            }

            Debug.Log($"GameManager: Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public GameState GetCurrentGameState()
        {
            return currentState;
        }
    }
}