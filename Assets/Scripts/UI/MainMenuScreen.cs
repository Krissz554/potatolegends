using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoLegends.Core;
using PotatoLegends.Network;

namespace PotatoLegends.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button battleButton;
        public Button collectionButton;
        public Button deckBuilderButton;
        public Button heroHallButton;
        public Button settingsButton;
        public Button logoutButton;
        public TextMeshProUGUI welcomeText;

        void OnEnable()
        {
            if (SupabaseClient.Instance != null && !string.IsNullOrEmpty(SupabaseClient.Instance.GetAccessToken()))
            {
                welcomeText.text = $"Welcome, Player!";
            }
            else
            {
                welcomeText.text = "Welcome!";
            }

            battleButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(GameManager.GameState.Battle));
            collectionButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(GameManager.GameState.Collection));
            deckBuilderButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(GameManager.GameState.DeckBuilder));
            heroHallButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(GameManager.GameState.HeroHall));
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        }

        void OnDisable()
        {
            battleButton.onClick.RemoveAllListeners();
            collectionButton.onClick.RemoveAllListeners();
            deckBuilderButton.onClick.RemoveAllListeners();
            heroHallButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            logoutButton.onClick.RemoveAllListeners();
        }

        private void OnSettingsButtonClicked()
        {
            Debug.Log("Settings button clicked (not implemented yet).");
        }

        private void OnLogoutButtonClicked()
        {
            SupabaseClient.Instance.SignOut();
            GameManager.Instance.ChangeGameState(GameManager.GameState.Auth);
        }
    }
}