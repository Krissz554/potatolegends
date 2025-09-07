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
    public class CompleteWebReplicator : EditorWindow
    {
        [MenuItem("Potato Legends/Create Complete Mobile Game (Web Style)")]
        public static void CreateCompleteMobileGame()
        {
            Debug.Log("🎮 Creating Complete Mobile Game - Replicating Web Version...");
            
            // Create all scenes with web-style UI
            CreateAuthScene();
            CreateMainMenuScene();
            CreateCollectionScene();
            CreateDeckBuilderScene();
            CreateHeroHallScene();
            CreateBattleScene();
            
            // Update build settings
            UpdateBuildSettings();
            
            Debug.Log("✅ Complete Mobile Game Created Successfully!");
            Debug.Log("🎯 All scenes replicate the web version exactly for mobile!");
        }

        private static void CreateAuthScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Auth";
            
            // Create Main Camera
            CreateMainCamera();
            
            // Create Canvas
            var canvas = CreateMobileCanvas("AuthCanvas");
            
            // Create Background with main pixel art
            CreateBackgroundWithImage(canvas, "Images/art/main-pixel");
            
            // Create Auth Modal (replicating web modal style)
            var authModal = CreateAuthModal(canvas);
            
            // Create GameInitializer
            CreateGameInitializer();
            
            // Save scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Auth.unity");
            Debug.Log("✅ Auth scene created with web-style modal!");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("MainMenuCanvas");
            CreateBackgroundWithImage(canvas, "Images/art/main-pixel");
            CreateMainMenuContent(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MainMenu.unity");
            Debug.Log("✅ Main Menu scene created with web-style layout!");
        }

        private static void CreateCollectionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Collection";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("CollectionCanvas");
            CreateBackgroundWithImage(canvas, "Images/art/main-pixel");
            CreateCollectionContent(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Collection.unity");
            Debug.Log("✅ Collection scene created with web-style layout!");
        }

        private static void CreateDeckBuilderScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "DeckBuilder";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("DeckBuilderCanvas");
            CreateBackgroundWithImage(canvas, "Images/art/main-pixel");
            CreateDeckBuilderContent(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/DeckBuilder.unity");
            Debug.Log("✅ Deck Builder scene created with web-style layout!");
        }

        private static void CreateHeroHallScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "HeroHall";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("HeroHallCanvas");
            CreateBackgroundWithImage(canvas, "Images/art/main-pixel");
            CreateHeroHallContent(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/HeroHall.unity");
            Debug.Log("✅ Hero Hall scene created with web-style layout!");
        }

        private static void CreateBattleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Battle";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("BattleCanvas");
            CreateBackgroundWithImage(canvas, "Images/arena/day/arena-day");
            CreateBattleContent(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Battle.unity");
            Debug.Log("✅ Battle scene created with web-style layout!");
        }

        private static void CreateMainCamera()
        {
            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.05f, 0.05f, 0.15f, 1f);
        }

        private static GameObject CreateMobileCanvas(string name)
        {
            var canvasObj = new GameObject(name);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // Mobile portrait
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create EventSystem
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();

            return canvasObj;
        }

        private static void CreateBackgroundWithImage(GameObject canvas, string imagePath)
        {
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(canvas.transform, false);
            
            var backgroundImage = backgroundObj.AddComponent<Image>();
            var backgroundSprite = Resources.Load<Sprite>(imagePath);
            if (backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite;
                backgroundImage.type = Image.Type.Sliced;
            }
            else
            {
                backgroundImage.color = new Color(0.1f, 0.1f, 0.2f, 1f);
            }
            
            var backgroundRect = backgroundObj.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
        }

        private static GameObject CreateAuthModal(GameObject canvas)
        {
            // Create Modal Background
            var modalBg = new GameObject("ModalBackground");
            modalBg.transform.SetParent(canvas.transform, false);
            
            var modalBgImage = modalBg.AddComponent<Image>();
            modalBgImage.color = new Color(0f, 0f, 0f, 0.5f);
            
            var modalBgRect = modalBg.GetComponent<RectTransform>();
            modalBgRect.anchorMin = Vector2.zero;
            modalBgRect.anchorMax = Vector2.one;
            modalBgRect.offsetMin = Vector2.zero;
            modalBgRect.offsetMax = Vector2.zero;

            // Create Modal Panel
            var modalPanel = new GameObject("ModalPanel");
            modalPanel.transform.SetParent(modalBg.transform, false);
            
            var panelRect = modalPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.2f);
            panelRect.anchorMax = new Vector2(0.9f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = modalPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

            // Create Title
            CreateText(modalPanel, "Title", "Join the Potato Club! 🥔", 36, new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.9f), new Color(1f, 0.8f, 0f, 1f));

            // Create Subtitle
            CreateText(modalPanel, "Subtitle", "Create an account to save your favorite potato personalities", 18, new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.8f), new Color(0.8f, 0.8f, 0.8f, 1f));

            // Create Tab System
            CreateTabSystem(modalPanel);

            // Create Email Input
            var emailInput = CreateInputField(modalPanel, "EmailInput", "Email", "your@email.com", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.6f));

            // Create Password Input
            var passwordInput = CreateInputField(modalPanel, "PasswordInput", "Password", "••••••••", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.45f));
            passwordInput.contentType = TMP_InputField.ContentType.Password;

            // Create Buttons
            var signInButton = CreateButton(modalPanel, "SignInButton", "Sign In", new Vector2(0.1f, 0.2f), new Vector2(0.45f, 0.3f), new Color(0.2f, 0.6f, 0.2f, 1f));
            var signUpButton = CreateButton(modalPanel, "SignUpButton", "Create Account", new Vector2(0.55f, 0.2f), new Vector2(0.9f, 0.3f), new Color(0.2f, 0.4f, 0.8f, 1f));
            var googleButton = CreateButton(modalPanel, "GoogleSignInButton", "Sign in with Google", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.18f), new Color(0.9f, 0.9f, 0.9f, 1f));

            // Create Error Message
            var errorMessage = CreateText(modalPanel, "ErrorMessage", "", 16, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.1f), new Color(1f, 0.3f, 0.3f, 1f));

            // Add AuthScreenUI script
            var authScreenUI = modalPanel.AddComponent<AuthScreenUI>();
            AssignAuthReferences(authScreenUI, emailInput, passwordInput, signInButton, signUpButton, googleButton, errorMessage, null);

            return modalPanel;
        }

        private static void CreateMainMenuContent(GameObject canvas)
        {
            // Create Top Navigation Bar
            var navBar = new GameObject("NavigationBar");
            navBar.transform.SetParent(canvas.transform, false);
            
            var navRect = navBar.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0f, 0.9f);
            navRect.anchorMax = new Vector2(1f, 1f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;
            
            var navImage = navBar.AddComponent<Image>();
            navImage.color = new Color(0f, 0f, 0f, 0.2f);

            // Create Logo/Title
            CreateText(navBar, "Logo", "What's My Potato?", 32, new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.8f), Color.white);

            // Create Main Title
            CreateText(canvas, "MainTitle", "POTATO LEGENDS", 72, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.75f), new Color(1f, 0.8f, 0f, 1f));

            // Create Subtitle
            CreateText(canvas, "Subtitle", "Enter the mystical realm of trading card warfare", 24, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.6f), new Color(0.8f, 0.8f, 0.8f, 1f));

            // Create Menu Buttons
            CreateButton(canvas, "CollectionButton", "Collection", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.48f), new Color(0.2f, 0.6f, 0.4f, 0.8f));
            CreateButton(canvas, "DeckBuilderButton", "Deck Builder", new Vector2(0.1f, 0.32f), new Vector2(0.9f, 0.4f), new Color(0.2f, 0.4f, 0.8f, 0.8f));
            CreateButton(canvas, "HeroHallButton", "Hero Hall", new Vector2(0.1f, 0.24f), new Vector2(0.9f, 0.32f), new Color(0.8f, 0.6f, 0.2f, 0.8f));
            CreateButton(canvas, "SettingsButton", "Settings", new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.24f), new Color(0.6f, 0.6f, 0.6f, 0.8f));

            // Create Battle Button
            var battleButton = CreateButton(canvas, "BattleButton", "BATTLE", new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.15f), new Color(0.8f, 0.2f, 0.2f, 1f));
            
            // Add MainMenuUI script
            var mainMenuPanel = new GameObject("MainMenuPanel");
            mainMenuPanel.transform.SetParent(canvas.transform, false);
            var mainMenuRect = mainMenuPanel.GetComponent<RectTransform>();
            mainMenuRect.anchorMin = Vector2.zero;
            mainMenuRect.anchorMax = Vector2.one;
            mainMenuRect.offsetMin = Vector2.zero;
            mainMenuRect.offsetMax = Vector2.zero;
            
            mainMenuPanel.AddComponent<MainMenuUI>();
        }

        private static void CreateCollectionContent(GameObject canvas)
        {
            // Create Collection Panel
            var collectionPanel = new GameObject("CollectionPanel");
            collectionPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = collectionPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Create Back Button
            CreateButton(collectionPanel, "BackButton", "← Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            
            // Create Title
            CreateText(collectionPanel, "Title", "COLLECTION", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f), Color.white);
            
            // Create Search Bar
            CreateInputField(collectionPanel, "SearchInput", "Search cards...", "Search cards...", new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.85f));
            
            // Create Filter Buttons
            CreateButton(collectionPanel, "AllFilterButton", "All", new Vector2(0.1f, 0.75f), new Vector2(0.2f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            CreateButton(collectionPanel, "CommonFilterButton", "Common", new Vector2(0.25f, 0.75f), new Vector2(0.35f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            CreateButton(collectionPanel, "RareFilterButton", "Rare", new Vector2(0.4f, 0.75f), new Vector2(0.5f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            CreateButton(collectionPanel, "LegendaryFilterButton", "Legendary", new Vector2(0.55f, 0.75f), new Vector2(0.7f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            
            // Add CollectionScreenUI script
            collectionPanel.AddComponent<CollectionScreenUI>();
        }

        private static void CreateDeckBuilderContent(GameObject canvas)
        {
            var deckBuilderPanel = new GameObject("DeckBuilderPanel");
            deckBuilderPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = deckBuilderPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            CreateButton(deckBuilderPanel, "BackButton", "← Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            CreateText(deckBuilderPanel, "Title", "DECK BUILDER", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f), Color.white);
            
            deckBuilderPanel.AddComponent<DeckBuilderScreen>();
        }

        private static void CreateHeroHallContent(GameObject canvas)
        {
            var heroHallPanel = new GameObject("HeroHallPanel");
            heroHallPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = heroHallPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            CreateButton(heroHallPanel, "BackButton", "← Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            CreateText(heroHallPanel, "Title", "HERO HALL", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f), Color.white);
            
            heroHallPanel.AddComponent<HeroHallScreen>();
        }

        private static void CreateBattleContent(GameObject canvas)
        {
            var battlePanel = new GameObject("BattlePanel");
            battlePanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = battlePanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            CreateText(battlePanel, "Title", "BATTLE ARENA", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f), Color.white);
            
            battlePanel.AddComponent<BattleScreen>();
        }

        private static void CreateTabSystem(GameObject parent)
        {
            // Create Tab Container
            var tabContainer = new GameObject("TabContainer");
            tabContainer.transform.SetParent(parent.transform, false);
            
            var tabRect = tabContainer.GetComponent<RectTransform>();
            tabRect.anchorMin = new Vector2(0.1f, 0.65f);
            tabRect.anchorMax = new Vector2(0.9f, 0.75f);
            tabRect.offsetMin = Vector2.zero;
            tabRect.offsetMax = Vector2.zero;

            // Create Sign In Tab
            CreateButton(tabContainer, "SignInTab", "Sign In", new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Color(0.3f, 0.3f, 0.3f, 1f));

            // Create Sign Up Tab
            CreateButton(tabContainer, "SignUpTab", "Sign Up", new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Color(0.3f, 0.3f, 0.3f, 1f));
        }

        private static TextMeshProUGUI CreateText(GameObject parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = FontStyles.Bold;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return textComponent;
        }

        private static Button CreateButton(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = color;
            
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
            buttonText.fontStyle = FontStyles.Bold;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return button;
        }

        private static TMP_InputField CreateInputField(GameObject parent, string name, string placeholder, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            var inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent.transform, false);
            
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            
            var inputImage = inputObj.AddComponent<Image>();
            inputImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            var inputField = inputObj.AddComponent<TMP_InputField>();
            
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
            textComponent.text = text;
            textComponent.color = Color.white;
            textComponent.fontSize = 24;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Create placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textAreaObj.transform, false);
            var placeholderComponent = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderComponent.text = placeholder;
            placeholderComponent.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderComponent.fontSize = 24;
            placeholderComponent.fontStyle = FontStyles.Italic;
            
            var placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            inputField.textComponent = textComponent;
            inputField.textViewport = textAreaRect;
            inputField.placeholder = placeholderComponent;
            
            return inputField;
        }

        private static void CreateGameInitializer()
        {
            var initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
        }

        private static void AssignAuthReferences(AuthScreenUI authScreenUI, TMP_InputField emailInput, TMP_InputField passwordInput, Button signInButton, Button signUpButton, Button googleSignInButton, TextMeshProUGUI errorMessage, GameObject loadingIndicator)
        {
            var emailField = typeof(AuthScreenUI).GetField("emailInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            emailField?.SetValue(authScreenUI, emailInput);
            
            var passwordField = typeof(AuthScreenUI).GetField("passwordInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            passwordField?.SetValue(authScreenUI, passwordInput);
            
            var signInField = typeof(AuthScreenUI).GetField("signInButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            signInField?.SetValue(authScreenUI, signInButton);
            
            var signUpField = typeof(AuthScreenUI).GetField("signUpButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            signUpField?.SetValue(authScreenUI, signUpButton);
            
            var googleField = typeof(AuthScreenUI).GetField("googleSignInButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            googleField?.SetValue(authScreenUI, googleSignInButton);
            
            var errorField = typeof(AuthScreenUI).GetField("errorMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            errorField?.SetValue(authScreenUI, errorMessage);
            
            var loadingField = typeof(AuthScreenUI).GetField("loadingIndicator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadingField?.SetValue(authScreenUI, loadingIndicator);
        }

        private static void UpdateBuildSettings()
        {
            // Update EditorBuildSettings to include all scenes
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Auth.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Collection.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/DeckBuilder.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/HeroHall.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Battle.unity", true)
            };
            
            EditorBuildSettings.scenes = scenes;
            Debug.Log("✅ Build settings updated with all scenes!");
        }
    }
}