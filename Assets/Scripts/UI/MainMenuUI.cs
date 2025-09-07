using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoLegends.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button heroHallButton;
        [SerializeField] private Button leaderboardsButton;
        [SerializeField] private Button socialButton;
        [SerializeField] private Button battleButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button logoutButton;

        [Header("User Info")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI statusMessageText;

        [Header("Animation")]
        [SerializeField] private float buttonAnimationDuration = 0.2f;

        private void Start()
        {
            SetupUI();
            UpdateUserInfo();
        }

        private void SetupUI()
        {
            // Setup navigation buttons
            if (collectionButton != null)
                collectionButton.onClick.AddListener(() => NavigateToScene("Collection"));
            
            if (deckBuilderButton != null)
                deckBuilderButton.onClick.AddListener(() => NavigateToScene("DeckBuilder"));
            
            if (heroHallButton != null)
                heroHallButton.onClick.AddListener(() => NavigateToScene("HeroHall"));
            
            if (leaderboardsButton != null)
                leaderboardsButton.onClick.AddListener(OnLeaderboardsClicked);
            
            if (socialButton != null)
                socialButton.onClick.AddListener(OnSocialClicked);

            // Setup main action buttons
            if (battleButton != null)
                battleButton.onClick.AddListener(OnBattleClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);

            // Add button animations
            AddButtonAnimations();
        }

        private void AddButtonAnimations()
        {
            Button[] buttons = { collectionButton, deckBuilderButton, heroHallButton, 
                               leaderboardsButton, socialButton, battleButton, settingsButton, logoutButton };

            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    AddButtonAnimation(button);
                }
            }
        }

        private void AddButtonAnimation(Button button)
        {
            // Add scale animation on click
            button.onClick.AddListener(() => {
                StartCoroutine(AnimateButton(button));
            });
        }

        private System.Collections.IEnumerator AnimateButton(Button button)
        {
            Vector3 originalScale = button.transform.localScale;
            Vector3 pressedScale = originalScale * 0.95f;

            // Scale down
            float elapsed = 0f;
            while (elapsed < buttonAnimationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (buttonAnimationDuration / 2);
                button.transform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }

            // Scale back up
            elapsed = 0f;
            while (elapsed < buttonAnimationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (buttonAnimationDuration / 2);
                button.transform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }

            button.transform.localScale = originalScale;
        }

        private void NavigateToScene(string sceneName)
        {
            Debug.Log($"🧭 Navigating to {sceneName}");
            
            if (PotatoLegends.Core.GameSceneManager.Instance != null)
            {
                switch (sceneName)
                {
                    case "Collection":
                        PotatoLegends.Core.GameSceneManager.Instance.LoadCollection();
                        break;
                    case "DeckBuilder":
                        PotatoLegends.Core.GameSceneManager.Instance.LoadDeckBuilder();
                        break;
                    case "HeroHall":
                        PotatoLegends.Core.GameSceneManager.Instance.LoadHeroHall();
                        break;
                }
            }
        }

        private void OnBattleClicked()
        {
            Debug.Log("⚔️ Battle button clicked - Starting matchmaking");
            
            if (PotatoLegends.Core.GameSceneManager.Instance != null)
            {
                PotatoLegends.Core.GameSceneManager.Instance.StartMatchmaking();
            }
        }

        private void OnLeaderboardsClicked()
        {
            Debug.Log("🏆 Leaderboards button clicked");
            // TODO: Implement leaderboards functionality
            ShowComingSoonMessage("Leaderboards");
        }

        private void OnSocialClicked()
        {
            Debug.Log("👥 Social button clicked");
            // TODO: Implement social functionality
            ShowComingSoonMessage("Social Features");
        }

        private void OnSettingsClicked()
        {
            Debug.Log("⚙️ Settings button clicked");
            // TODO: Implement settings functionality
            ShowComingSoonMessage("Settings");
        }

        private void OnLogoutClicked()
        {
            Debug.Log("👋 Logout button clicked");
            
            // Show confirmation dialog
            ShowLogoutConfirmation();
        }

        private void ShowLogoutConfirmation()
        {
            // TODO: Implement proper confirmation dialog
            // For now, just logout directly
            if (PotatoLegends.Core.GameSceneManager.Instance != null)
            {
                PotatoLegends.Core.GameSceneManager.Instance.Logout();
            }
        }

        private void ShowComingSoonMessage(string feature)
        {
            if (statusMessageText != null)
            {
                string originalText = statusMessageText.text;
                statusMessageText.text = $"{feature} coming soon!";
                statusMessageText.color = Color.yellow;
                
                // Reset after 3 seconds
                StartCoroutine(ResetStatusMessage(originalText, 3f));
            }
        }

        private System.Collections.IEnumerator ResetStatusMessage(string originalText, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (statusMessageText != null)
            {
                statusMessageText.text = originalText;
                statusMessageText.color = Color.green;
            }
        }

        private void UpdateUserInfo()
        {
            // Get player email from saved data or use default
            string playerEmail = PlayerPrefs.GetString("user_email", "Player");
            
            if (playerNameText != null)
            {
                playerNameText.text = playerEmail;
            }

            if (statusMessageText != null)
            {
                statusMessageText.text = "Ready to battle!";
                statusMessageText.color = Color.green;
            }
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (collectionButton != null)
                collectionButton.onClick.RemoveAllListeners();
            
            if (deckBuilderButton != null)
                deckBuilderButton.onClick.RemoveAllListeners();
            
            if (heroHallButton != null)
                heroHallButton.onClick.RemoveAllListeners();
            
            if (leaderboardsButton != null)
                leaderboardsButton.onClick.RemoveAllListeners();
            
            if (socialButton != null)
                socialButton.onClick.RemoveAllListeners();
            
            if (battleButton != null)
                battleButton.onClick.RemoveAllListeners();
            
            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();
            
            if (logoutButton != null)
                logoutButton.onClick.RemoveAllListeners();
        }
    }
}