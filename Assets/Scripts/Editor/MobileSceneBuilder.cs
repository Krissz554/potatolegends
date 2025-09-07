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
    public class MobileSceneBuilder : EditorWindow
    {
        [MenuItem("Potato Legends/Build Mobile Scenes")]
        public static void BuildMobileScenes()
        {
            Debug.Log("🎮 Building Mobile Game Scenes...");
            
            BuildAuthScene();
            BuildMainMenuScene();
            BuildCollectionScene();
            BuildDeckBuilderScene();
            BuildHeroHallScene();
            BuildBattleScene();
            
            Debug.Log("✅ All mobile scenes built successfully!");
        }

        [MenuItem("Potato Legends/Build Auth Scene")]
        public static void BuildAuthScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneMode.Single);
            scene.name = "Auth";
            
            // Create Main Camera
            CreateMainCamera();
            
            // Create Canvas
            var canvas = CreateMobileCanvas("AuthCanvas");
            
            // Create Background
            CreateBackground(canvas);
            
            // Create Auth UI
            CreateAuthUI(canvas);
            
            // Create GameInitializer
            CreateGameInitializer();
            
            // Save scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Auth.unity");
            Debug.Log("✅ Auth scene built!");
        }

        [MenuItem("Potato Legends/Build Main Menu Scene")]
        public static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneMode.Single);
            scene.name = "MainMenu";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("MainMenuCanvas");
            CreateBackground(canvas);
            CreateMainMenuUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MainMenu.unity");
            Debug.Log("✅ Main Menu scene built!");
        }

        [MenuItem("Potato Legends/Build Collection Scene")]
        public static void BuildCollectionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneMode.Single);
            scene.name = "Collection";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("CollectionCanvas");
            CreateBackground(canvas);
            CreateCollectionUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Collection.unity");
            Debug.Log("✅ Collection scene built!");
        }

        [MenuItem("Potato Legends/Build Deck Builder Scene")]
        public static void BuildDeckBuilderScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneMode.Single);
            scene.name = "DeckBuilder";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("DeckBuilderCanvas");
            CreateBackground(canvas);
            CreateDeckBuilderUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/DeckBuilder.unity");
            Debug.Log("✅ Deck Builder scene built!");
        }

        [MenuItem("Potato Legends/Build Hero Hall Scene")]
        public static void BuildHeroHallScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneMode.Single);
            scene.name = "HeroHall";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("HeroHallCanvas");
            CreateBackground(canvas);
            CreateHeroHallUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/HeroHall.unity");
            Debug.Log("✅ Hero Hall scene built!");
        }

        [MenuItem("Potato Legends/Build Battle Scene")]
        public static void BuildBattleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneMode.Single);
            scene.name = "Battle";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("BattleCanvas");
            CreateBackground(canvas);
            CreateBattleUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Battle.unity");
            Debug.Log("✅ Battle scene built!");
        }

        private static void CreateMainCamera()
        {
            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 1f);
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

        private static void CreateBackground(GameObject canvas)
        {
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            
            var image = bgObj.AddComponent<Image>();
            image.color = new Color(0.05f, 0.05f, 0.15f, 1f);
            
            var rectTransform = bgObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void CreateGameInitializer()
        {
            var initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
        }

        private static void CreateAuthUI(GameObject canvas)
        {
            // Create Auth Panel
            var authPanel = new GameObject("AuthPanel");
            authPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = authPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.2f);
            panelRect.anchorMax = new Vector2(0.9f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = authPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
            
            // Create Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(authPanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "POTATO LEGENDS";
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Create Email Input
            var emailInput = CreateInputField(authPanel, "EmailInput", "Email", new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.7f));
            
            // Create Password Input
            var passwordInput = CreateInputField(authPanel, "PasswordInput", "Password", new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.55f));
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            
            // Create Sign In Button
            var signInButton = CreateButton(authPanel, "SignInButton", "SIGN IN", new Vector2(0.1f, 0.3f), new Vector2(0.45f, 0.4f), new Color(0.2f, 0.6f, 0.2f, 1f));
            
            // Create Sign Up Button
            var signUpButton = CreateButton(authPanel, "SignUpButton", "SIGN UP", new Vector2(0.55f, 0.3f), new Vector2(0.9f, 0.4f), new Color(0.2f, 0.2f, 0.6f, 1f));
            
            // Create Error Message
            var errorMessage = CreateText(authPanel, "ErrorMessage", "", 18, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.25f));
            errorMessage.color = Color.red;
            
            // Create Loading Indicator
            var loadingIndicator = CreateText(authPanel, "LoadingIndicator", "Loading...", 20, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.15f));
            loadingIndicator.color = Color.yellow;
            loadingIndicator.gameObject.SetActive(false);
            
            // Add AuthScreenUI script and assign references
            var authScreenUI = authPanel.AddComponent<AuthScreenUI>();
            AssignAuthReferences(authScreenUI, emailInput, passwordInput, signInButton, signUpButton, errorMessage, loadingIndicator.gameObject);
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
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(menuPanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "POTATO LEGENDS";
            titleText.fontSize = 64;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Create Buttons
            CreateButton(menuPanel, "CollectionButton", "COLLECTION", new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.7f), new Color(0.2f, 0.4f, 0.6f, 0.8f));
            CreateButton(menuPanel, "DeckBuilderButton", "DECK BUILDER", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.6f), new Color(0.2f, 0.4f, 0.6f, 0.8f));
            CreateButton(menuPanel, "HeroHallButton", "HERO HALL", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.5f), new Color(0.2f, 0.4f, 0.6f, 0.8f));
            CreateButton(menuPanel, "BattleButton", "BATTLE", new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.4f), new Color(0.6f, 0.2f, 0.2f, 0.8f));
            CreateButton(menuPanel, "SettingsButton", "SETTINGS", new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.3f), new Color(0.2f, 0.4f, 0.6f, 0.8f));
            CreateButton(menuPanel, "LogoutButton", "LOGOUT", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.2f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            
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
            CreateButton(collectionPanel, "BackButton", "BACK", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            
            // Create Title
            CreateText(collectionPanel, "Title", "COLLECTION", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
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
            CreateButton(deckBuilderPanel, "BackButton", "BACK", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            
            // Create Title
            CreateText(deckBuilderPanel, "Title", "DECK BUILDER", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
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
            CreateButton(heroHallPanel, "BackButton", "BACK", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            
            // Create Title
            CreateText(heroHallPanel, "Title", "HERO HALL", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
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
            CreateText(battlePanel, "Title", "BATTLE ARENA", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
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

        private static TMP_InputField CreateInputField(GameObject parent, string name, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
        {
            var inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent.transform, false);
            
            // Add RectTransform first
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            
            // Add Image component for background
            var inputImage = inputObj.AddComponent<Image>();
            inputImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Add TMP_InputField
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
            textComponent.text = "";
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
            
            // Set up input field
            inputField.textComponent = textComponent;
            inputField.textViewport = textAreaRect;
            inputField.placeholder = placeholderComponent;
            
            return inputField;
        }

        private static void AssignAuthReferences(AuthScreenUI authScreenUI, TMP_InputField emailInput, TMP_InputField passwordInput, Button signInButton, Button signUpButton, TextMeshProUGUI errorMessage, GameObject loadingIndicator)
        {
            // Use reflection to assign private fields
            var emailField = typeof(AuthScreenUI).GetField("emailInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            emailField?.SetValue(authScreenUI, emailInput);
            
            var passwordField = typeof(AuthScreenUI).GetField("passwordInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            passwordField?.SetValue(authScreenUI, passwordInput);
            
            var signInField = typeof(AuthScreenUI).GetField("signInButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            signInField?.SetValue(authScreenUI, signInButton);
            
            var signUpField = typeof(AuthScreenUI).GetField("signUpButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            signUpField?.SetValue(authScreenUI, signUpButton);
            
            var errorField = typeof(AuthScreenUI).GetField("errorMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            errorField?.SetValue(authScreenUI, errorMessage);
            
            var loadingField = typeof(AuthScreenUI).GetField("loadingIndicator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadingField?.SetValue(authScreenUI, loadingIndicator);
        }
    }
}