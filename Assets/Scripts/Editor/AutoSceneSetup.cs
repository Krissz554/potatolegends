using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PotatoLegends.Core;
using PotatoLegends.Network;
using PotatoLegends.UI;
using PotatoLegends.Collection;

namespace PotatoLegends.Editor
{
    [InitializeOnLoad]
    public class AutoSceneSetup
    {
        static AutoSceneSetup()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Ensure all scenes are properly set up when entering play mode
                SetupCurrentScene();
            }
        }

        private static void SetupCurrentScene()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var sceneName = currentScene.name;

            switch (sceneName)
            {
                case "Auth":
                    SetupAuthScene();
                    break;
                case "MainMenu":
                    SetupMainMenuScene();
                    break;
                case "Collection":
                    SetupCollectionScene();
                    break;
                case "DeckBuilder":
                    SetupDeckBuilderScene();
                    break;
                case "HeroHall":
                    SetupHeroHallScene();
                    break;
                case "Battle":
                    SetupBattleScene();
                    break;
            }
        }

        private static void SetupAuthScene()
        {
            var canvas = FindOrCreateCanvas("AuthCanvas");
            if (canvas.GetComponentInChildren<AuthScreenUI>() == null)
            {
                CreateAuthUI(canvas);
            }
            EnsureGameInitializer();
        }

        private static void SetupMainMenuScene()
        {
            var canvas = FindOrCreateCanvas("MainMenuCanvas");
            if (canvas.GetComponentInChildren<MainMenuUI>() == null)
            {
                CreateMainMenuUI(canvas);
            }
            EnsureGameInitializer();
        }

        private static void SetupCollectionScene()
        {
            var canvas = FindOrCreateCanvas("CollectionCanvas");
            if (canvas.GetComponentInChildren<CollectionScreenUI>() == null)
            {
                CreateCollectionUI(canvas);
            }
            EnsureGameInitializer();
        }

        private static void SetupDeckBuilderScene()
        {
            var canvas = FindOrCreateCanvas("DeckBuilderCanvas");
            if (canvas.GetComponentInChildren<DeckBuilderScreen>() == null)
            {
                CreateDeckBuilderUI(canvas);
            }
            EnsureGameInitializer();
        }

        private static void SetupHeroHallScene()
        {
            var canvas = FindOrCreateCanvas("HeroHallCanvas");
            if (canvas.GetComponentInChildren<HeroHallScreen>() == null)
            {
                CreateHeroHallUI(canvas);
            }
            EnsureGameInitializer();
        }

        private static void SetupBattleScene()
        {
            var canvas = FindOrCreateCanvas("BattleCanvas");
            if (canvas.GetComponentInChildren<BattleScreen>() == null)
            {
                CreateBattleUI(canvas);
            }
            EnsureGameInitializer();
        }

        private static GameObject FindOrCreateCanvas(string name)
        {
            var canvas = GameObject.Find(name);
            if (canvas == null)
            {
                canvas = new GameObject(name);
                var canvasComponent = canvas.AddComponent<Canvas>();
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasComponent.sortingOrder = 0;

                var canvasScaler = canvas.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;

                canvas.AddComponent<GraphicRaycaster>();

                // Create EventSystem if it doesn't exist
                if (Object.FindObjectOfType<EventSystem>() == null)
                {
                    var eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<EventSystem>();
                    eventSystemObj.AddComponent<StandaloneInputModule>();
                }
            }
            return canvas;
        }

        private static void EnsureGameInitializer()
        {
            if (Object.FindObjectOfType<GameInitializer>() == null)
            {
                var initializerObj = new GameObject("GameInitializer");
                initializerObj.AddComponent<GameInitializer>();
            }
        }

        private static void CreateAuthUI(GameObject canvas)
        {
            // Create Auth Panel
            var authPanel = new GameObject("AuthPanel");
            authPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = authPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.2f);
            panelRect.anchorMax = new Vector2(0.8f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = authPanel.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
            
            // Create Title
            CreateText(authPanel, "Title", "Potato Legends", 48, new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.95f));
            
            // Create Email Input
            var emailInput = CreateInputField(authPanel, "EmailInput", new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.7f));
            
            // Create Password Input
            var passwordInput = CreateInputField(authPanel, "PasswordInput", new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.55f));
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            
            // Create Sign In Button
            var signInButton = CreateButton(authPanel, "SignInButton", "Sign In", new Vector2(0.1f, 0.3f), new Vector2(0.45f, 0.4f));
            
            // Create Sign Up Button
            var signUpButton = CreateButton(authPanel, "SignUpButton", "Sign Up", new Vector2(0.55f, 0.3f), new Vector2(0.9f, 0.4f));
            
            // Create Error Message
            var errorMessage = CreateText(authPanel, "ErrorMessage", "", 18, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.25f));
            errorMessage.color = Color.red;
            
            // Create Loading Indicator
            var loadingIndicator = CreateText(authPanel, "LoadingIndicator", "Loading...", 20, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.15f));
            loadingIndicator.color = Color.yellow;
            loadingIndicator.gameObject.SetActive(false);
            
            // Add AuthScreenUI script
            var authScreenUI = authPanel.AddComponent<AuthScreenUI>();
            
            // Set up references
            SetPrivateField(authScreenUI, "emailInput", emailInput);
            SetPrivateField(authScreenUI, "passwordInput", passwordInput);
            SetPrivateField(authScreenUI, "signInButton", signInButton);
            SetPrivateField(authScreenUI, "signUpButton", signUpButton);
            SetPrivateField(authScreenUI, "errorMessage", errorMessage);
            SetPrivateField(authScreenUI, "loadingIndicator", loadingIndicator.gameObject);
        }

        private static void CreateMainMenuUI(GameObject canvas)
        {
            // Create Main Menu Panel
            var menuPanel = new GameObject("MainMenuPanel");
            menuPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = menuPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Create Title
            CreateText(menuPanel, "Title", "Potato Legends", 64, new Vector2(0.2f, 0.8f), new Vector2(0.8f, 0.95f));
            
            // Create Buttons
            CreateButton(menuPanel, "CollectionButton", "Collection", new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.7f));
            CreateButton(menuPanel, "DeckBuilderButton", "Deck Builder", new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.6f));
            CreateButton(menuPanel, "HeroHallButton", "Hero Hall", new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.5f));
            CreateButton(menuPanel, "BattleButton", "Battle", new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.4f));
            CreateButton(menuPanel, "SettingsButton", "Settings", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.3f));
            CreateButton(menuPanel, "LogoutButton", "Logout", new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.2f));
            
            // Add MainMenuUI script
            menuPanel.AddComponent<MainMenuUI>();
        }

        private static void CreateCollectionUI(GameObject canvas)
        {
            // Create Collection Panel
            var collectionPanel = new GameObject("CollectionPanel");
            collectionPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = collectionPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Create Back Button
            CreateButton(collectionPanel, "BackButton", "Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f));
            
            // Create Title
            CreateText(collectionPanel, "Title", "Collection", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            // Add CollectionScreenUI script
            collectionPanel.AddComponent<CollectionScreenUI>();
        }

        private static void CreateDeckBuilderUI(GameObject canvas)
        {
            // Create Deck Builder Panel
            var deckBuilderPanel = new GameObject("DeckBuilderPanel");
            deckBuilderPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = deckBuilderPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Create Back Button
            CreateButton(deckBuilderPanel, "BackButton", "Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f));
            
            // Create Title
            CreateText(deckBuilderPanel, "Title", "Deck Builder", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            // Add DeckBuilderScreen script
            deckBuilderPanel.AddComponent<DeckBuilderScreen>();
        }

        private static void CreateHeroHallUI(GameObject canvas)
        {
            // Create Hero Hall Panel
            var heroHallPanel = new GameObject("HeroHallPanel");
            heroHallPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = heroHallPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Create Back Button
            CreateButton(heroHallPanel, "BackButton", "Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f));
            
            // Create Title
            CreateText(heroHallPanel, "Title", "Hero Hall", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            // Add HeroHallScreen script
            heroHallPanel.AddComponent<HeroHallScreen>();
        }

        private static void CreateBattleUI(GameObject canvas)
        {
            // Create Battle Panel
            var battlePanel = new GameObject("BattlePanel");
            battlePanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = battlePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Create Title
            CreateText(battlePanel, "Title", "Battle Arena", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            // Add BattleScreen script
            battlePanel.AddComponent<BattleScreen>();
        }

        private static TextMeshProUGUI CreateText(GameObject parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return textComponent;
        }

        private static Button CreateButton(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.6f, 0.8f);
            
            var buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return button;
        }

        private static TMP_InputField CreateInputField(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent.transform, false);
            
            var inputField = inputObj.AddComponent<TMP_InputField>();
            
            var inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            
            // Create text area
            var textAreaObj = new GameObject("Text Area");
            textAreaObj.transform.SetParent(inputObj.transform, false);
            var textAreaRect = textAreaObj.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;
            
            // Create text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(textAreaObj.transform, false);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = "";
            textComponent.color = Color.white;
            textComponent.fontSize = 24;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            inputField.textComponent = textComponent;
            
            return inputField;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}