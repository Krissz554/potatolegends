using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoLegends.Core;
using PotatoLegends.Network;
using PotatoLegends.UI;

namespace PotatoLegends.Utils
{
    public class SceneTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public bool enableDebugLogging = true;
        public bool testOnStart = true;

        void Start()
        {
            if (testOnStart)
            {
                TestSceneSetup();
            }
        }

        [ContextMenu("Test Scene Setup")]
        public void TestSceneSetup()
        {
            Debug.Log("🧪 Starting Scene Test...");
            
            // Test Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Debug.Log("✅ Canvas found: " + canvas.name);
            }
            else
            {
                Debug.LogError("❌ No Canvas found!");
                return;
            }

            // Test EventSystem
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                Debug.Log("✅ EventSystem found: " + eventSystem.name);
            }
            else
            {
                Debug.LogError("❌ No EventSystem found!");
            }

            // Test UI Scripts
            TestUIScripts();

            // Test Managers
            TestManagers();

            Debug.Log("🎉 Scene Test Complete!");
        }

        private void TestUIScripts()
        {
            var authScreenUI = FindObjectOfType<AuthScreenUI>();
            if (authScreenUI != null)
            {
                Debug.Log("✅ AuthScreenUI found");
                TestAuthScreenUI(authScreenUI);
            }

            var mainMenuUI = FindObjectOfType<MainMenuUI>();
            if (mainMenuUI != null)
            {
                Debug.Log("✅ MainMenuUI found");
            }

            var collectionScreenUI = FindObjectOfType<CollectionScreenUI>();
            if (collectionScreenUI != null)
            {
                Debug.Log("✅ CollectionScreenUI found");
            }

            var deckBuilderScreen = FindObjectOfType<DeckBuilderScreen>();
            if (deckBuilderScreen != null)
            {
                Debug.Log("✅ DeckBuilderScreen found");
            }

            var heroHallScreen = FindObjectOfType<HeroHallScreen>();
            if (heroHallScreen != null)
            {
                Debug.Log("✅ HeroHallScreen found");
            }

            var battleScreen = FindObjectOfType<BattleScreen>();
            if (battleScreen != null)
            {
                Debug.Log("✅ BattleScreen found");
            }
        }

        private void TestAuthScreenUI(AuthScreenUI authScreenUI)
        {
            // Test if UI elements are properly assigned
            var emailInput = GetPrivateField<TMP_InputField>(authScreenUI, "emailInput");
            var passwordInput = GetPrivateField<TMP_InputField>(authScreenUI, "passwordInput");
            var signInButton = GetPrivateField<Button>(authScreenUI, "signInButton");
            var signUpButton = GetPrivateField<Button>(authScreenUI, "signUpButton");
            var errorMessage = GetPrivateField<TextMeshProUGUI>(authScreenUI, "errorMessage");
            var loadingIndicator = GetPrivateField<GameObject>(authScreenUI, "loadingIndicator");

            if (emailInput != null) Debug.Log("✅ Email Input assigned");
            else Debug.LogError("❌ Email Input not assigned");

            if (passwordInput != null) Debug.Log("✅ Password Input assigned");
            else Debug.LogError("❌ Password Input not assigned");

            if (signInButton != null) Debug.Log("✅ Sign In Button assigned");
            else Debug.LogError("❌ Sign In Button not assigned");

            if (signUpButton != null) Debug.Log("✅ Sign Up Button assigned");
            else Debug.LogError("❌ Sign Up Button not assigned");

            if (errorMessage != null) Debug.Log("✅ Error Message assigned");
            else Debug.LogError("❌ Error Message not assigned");

            if (loadingIndicator != null) Debug.Log("✅ Loading Indicator assigned");
            else Debug.LogError("❌ Loading Indicator not assigned");
        }

        private void TestManagers()
        {
            // Test GameInitializer
            var gameInitializer = FindObjectOfType<GameInitializer>();
            if (gameInitializer != null)
            {
                Debug.Log("✅ GameInitializer found");
            }
            else
            {
                Debug.LogError("❌ GameInitializer not found!");
            }

            // Test SupabaseClient
            if (SupabaseClient.Instance != null)
            {
                Debug.Log("✅ SupabaseClient found");
            }
            else
            {
                Debug.LogWarning("⚠️ SupabaseClient not found (will be created by GameInitializer)");
            }

            // Test GameSceneManager
            if (GameSceneManager.Instance != null)
            {
                Debug.Log("✅ GameSceneManager found");
            }
            else
            {
                Debug.LogWarning("⚠️ GameSceneManager not found (will be created by GameInitializer)");
            }
        }

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (T)field.GetValue(obj);
            }
            return default(T);
        }

        [ContextMenu("Test Button Clicks")]
        public void TestButtonClicks()
        {
            Debug.Log("🧪 Testing Button Clicks...");

            var buttons = FindObjectsOfType<Button>();
            Debug.Log($"Found {buttons.Length} buttons");

            foreach (var button in buttons)
            {
                if (button.interactable)
                {
                    Debug.Log($"Testing button: {button.name}");
                    // Simulate button click
                    button.onClick.Invoke();
                }
            }
        }

        [ContextMenu("Test Input Fields")]
        public void TestInputFields()
        {
            Debug.Log("🧪 Testing Input Fields...");

            var inputFields = FindObjectsOfType<TMP_InputField>();
            Debug.Log($"Found {inputFields.Length} input fields");

            foreach (var inputField in inputFields)
            {
                Debug.Log($"Testing input field: {inputField.name}");
                inputField.text = "test@example.com";
                Debug.Log($"Set text to: {inputField.text}");
            }
        }
    }
}