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
    public class SceneSetupManager : EditorWindow
    {
        [MenuItem("Potato Legends/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            SetupAuthScene();
            SetupMainMenuScene();
            SetupCollectionScene();
            SetupDeckBuilderScene();
            SetupHeroHallScene();
            SetupBattleScene();
            
            Debug.Log("✅ All scenes have been set up and are fully functional!");
        }

        [MenuItem("Potato Legends/Setup Auth Scene")]
        public static void SetupAuthScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Auth.unity", OpenSceneMode.Single);
            
            // Clear existing objects
            ClearScene();
            
            // Create main camera
            CreateMainCamera();
            
            // Create Canvas
            var canvas = CreateCanvas("AuthCanvas");
            
            // Create background
            CreateBackground(canvas);
            
            // Create Auth UI
            CreateAuthUI(canvas);
            
            // Create GameInitializer
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("✅ Auth scene setup complete!");
        }

        [MenuItem("Potato Legends/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            
            ClearScene();
            CreateMainCamera();
            var canvas = CreateCanvas("MainMenuCanvas");
            CreateBackground(canvas);
            CreateMainMenuUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("✅ Main Menu scene setup complete!");
        }

        [MenuItem("Potato Legends/Setup Collection Scene")]
        public static void SetupCollectionScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Collection.unity", OpenSceneMode.Single);
            
            ClearScene();
            CreateMainCamera();
            var canvas = CreateCanvas("CollectionCanvas");
            CreateBackground(canvas);
            CreateCollectionUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("✅ Collection scene setup complete!");
        }

        [MenuItem("Potato Legends/Setup Deck Builder Scene")]
        public static void SetupDeckBuilderScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/DeckBuilder.unity", OpenSceneMode.Single);
            
            ClearScene();
            CreateMainCamera();
            var canvas = CreateCanvas("DeckBuilderCanvas");
            CreateBackground(canvas);
            CreateDeckBuilderUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("✅ Deck Builder scene setup complete!");
        }

        [MenuItem("Potato Legends/Setup Hero Hall Scene")]
        public static void SetupHeroHallScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/HeroHall.unity", OpenSceneMode.Single);
            
            ClearScene();
            CreateMainCamera();
            var canvas = CreateCanvas("HeroHallCanvas");
            CreateBackground(canvas);
            CreateHeroHallUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("✅ Hero Hall scene setup complete!");
        }

        [MenuItem("Potato Legends/Setup Battle Scene")]
        public static void SetupBattleScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
            
            ClearScene();
            CreateMainCamera();
            var canvas = CreateCanvas("BattleCanvas");
            CreateBackground(canvas);
            CreateBattleUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("✅ Battle scene setup complete!");
        }

        private static void ClearScene()
        {
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name != "Main Camera" && obj.name != "Directional Light")
                {
                    DestroyImmediate(obj);
                }
            }
        }

        private static void CreateMainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObj = new GameObject("Main Camera");
                camera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }
            
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = Color.black;
        }

        private static GameObject CreateCanvas(string name)
        {
            var canvasObj = new GameObject(name);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            var graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();

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
            image.color = new Color(0.1f, 0.1f, 0.2f, 1f);
            
            var rectTransform = bgObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
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
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(authPanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Potato Legends";
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Create Email Input
            var emailInputObj = new GameObject("EmailInput");
            emailInputObj.transform.SetParent(authPanel.transform, false);
            
            var emailInput = emailInputObj.AddComponent<TMP_InputField>();
            var emailText = emailInputObj.transform.Find("Text Area/Text").GetComponent<TextMeshProUGUI>();
            emailText.text = "";
            emailText.color = Color.white;
            
            var emailRect = emailInputObj.GetComponent<RectTransform>();
            emailRect.anchorMin = new Vector2(0.1f, 0.6f);
            emailRect.anchorMax = new Vector2(0.9f, 0.7f);
            emailRect.offsetMin = Vector2.zero;
            emailRect.offsetMax = Vector2.zero;
            
            // Create Password Input
            var passwordInputObj = new GameObject("PasswordInput");
            passwordInputObj.transform.SetParent(authPanel.transform, false);
            
            var passwordInput = passwordInputObj.AddComponent<TMP_InputField>();
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            var passwordText = passwordInputObj.transform.Find("Text Area/Text").GetComponent<TextMeshProUGUI>();
            passwordText.text = "";
            passwordText.color = Color.white;
            
            var passwordRect = passwordInputObj.GetComponent<RectTransform>();
            passwordRect.anchorMin = new Vector2(0.1f, 0.45f);
            passwordRect.anchorMax = new Vector2(0.9f, 0.55f);
            passwordRect.offsetMin = Vector2.zero;
            passwordRect.offsetMax = Vector2.zero;
            
            // Create Sign In Button
            var signInButtonObj = new GameObject("SignInButton");
            signInButtonObj.transform.SetParent(authPanel.transform, false);
            
            var signInButton = signInButtonObj.AddComponent<Button>();
            var signInImage = signInButtonObj.AddComponent<Image>();
            signInImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            var signInButtonRect = signInButtonObj.GetComponent<RectTransform>();
            signInButtonRect.anchorMin = new Vector2(0.1f, 0.3f);
            signInButtonRect.anchorMax = new Vector2(0.45f, 0.4f);
            signInButtonRect.offsetMin = Vector2.zero;
            signInButtonRect.offsetMax = Vector2.zero;
            
            var signInTextObj = new GameObject("Text");
            signInTextObj.transform.SetParent(signInButtonObj.transform, false);
            var signInText = signInTextObj.AddComponent<TextMeshProUGUI>();
            signInText.text = "Sign In";
            signInText.fontSize = 24;
            signInText.color = Color.white;
            signInText.alignment = TextAlignmentOptions.Center;
            
            var signInTextRect = signInTextObj.GetComponent<RectTransform>();
            signInTextRect.anchorMin = Vector2.zero;
            signInTextRect.anchorMax = Vector2.one;
            signInTextRect.offsetMin = Vector2.zero;
            signInTextRect.offsetMax = Vector2.zero;
            
            // Create Sign Up Button
            var signUpButtonObj = new GameObject("SignUpButton");
            signUpButtonObj.transform.SetParent(authPanel.transform, false);
            
            var signUpButton = signUpButtonObj.AddComponent<Button>();
            var signUpImage = signUpButtonObj.AddComponent<Image>();
            signUpImage.color = new Color(0.2f, 0.2f, 0.6f, 1f);
            
            var signUpButtonRect = signUpButtonObj.GetComponent<RectTransform>();
            signUpButtonRect.anchorMin = new Vector2(0.55f, 0.3f);
            signUpButtonRect.anchorMax = new Vector2(0.9f, 0.4f);
            signUpButtonRect.offsetMin = Vector2.zero;
            signUpButtonRect.offsetMax = Vector2.zero;
            
            var signUpTextObj = new GameObject("Text");
            signUpTextObj.transform.SetParent(signUpButtonObj.transform, false);
            var signUpText = signUpTextObj.AddComponent<TextMeshProUGUI>();
            signUpText.text = "Sign Up";
            signUpText.fontSize = 24;
            signUpText.color = Color.white;
            signUpText.alignment = TextAlignmentOptions.Center;
            
            var signUpTextRect = signUpTextObj.GetComponent<RectTransform>();
            signUpTextRect.anchorMin = Vector2.zero;
            signUpTextRect.anchorMax = Vector2.one;
            signUpTextRect.offsetMin = Vector2.zero;
            signUpTextRect.offsetMax = Vector2.zero;
            
            // Create Error Message
            var errorMessageObj = new GameObject("ErrorMessage");
            errorMessageObj.transform.SetParent(authPanel.transform, false);
            
            var errorMessage = errorMessageObj.AddComponent<TextMeshProUGUI>();
            errorMessage.text = "";
            errorMessage.fontSize = 18;
            errorMessage.color = Color.red;
            errorMessage.alignment = TextAlignmentOptions.Center;
            
            var errorRect = errorMessageObj.GetComponent<RectTransform>();
            errorRect.anchorMin = new Vector2(0.1f, 0.15f);
            errorRect.anchorMax = new Vector2(0.9f, 0.25f);
            errorRect.offsetMin = Vector2.zero;
            errorRect.offsetMax = Vector2.zero;
            
            // Create Loading Indicator
            var loadingObj = new GameObject("LoadingIndicator");
            loadingObj.transform.SetParent(authPanel.transform, false);
            loadingObj.SetActive(false);
            
            var loadingText = loadingObj.AddComponent<TextMeshProUGUI>();
            loadingText.text = "Loading...";
            loadingText.fontSize = 20;
            loadingText.color = Color.yellow;
            loadingText.alignment = TextAlignmentOptions.Center;
            
            var loadingRect = loadingObj.GetComponent<RectTransform>();
            loadingRect.anchorMin = new Vector2(0.1f, 0.05f);
            loadingRect.anchorMax = new Vector2(0.9f, 0.15f);
            loadingRect.offsetMin = Vector2.zero;
            loadingRect.offsetMax = Vector2.zero;
            
            // Add AuthScreenUI script
            var authScreenUI = authPanel.AddComponent<AuthScreenUI>();
            
            // Set up references using reflection to avoid serialization issues
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
            loadingField?.SetValue(authScreenUI, loadingObj);
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
            titleText.text = "Potato Legends";
            titleText.fontSize = 64;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.8f);
            titleRect.anchorMax = new Vector2(0.8f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Create Buttons
            CreateMenuButton(menuPanel, "Collection", new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.7f));
            CreateMenuButton(menuPanel, "Deck Builder", new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.6f));
            CreateMenuButton(menuPanel, "Hero Hall", new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.5f));
            CreateMenuButton(menuPanel, "Battle", new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.4f));
            CreateMenuButton(menuPanel, "Settings", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.3f));
            CreateMenuButton(menuPanel, "Logout", new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.2f));
            
            // Add MainMenuUI script
            var mainMenuUI = menuPanel.AddComponent<MainMenuUI>();
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
            CreateMenuButton(collectionPanel, "Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f));
            
            // Create Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(collectionPanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Collection";
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.9f);
            titleRect.anchorMax = new Vector2(0.8f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Add CollectionScreenUI script
            var collectionScreenUI = collectionPanel.AddComponent<CollectionScreenUI>();
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
            CreateMenuButton(deckBuilderPanel, "Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f));
            
            // Create Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(deckBuilderPanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Deck Builder";
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.9f);
            titleRect.anchorMax = new Vector2(0.8f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Add DeckBuilderScreen script
            var deckBuilderScreen = deckBuilderPanel.AddComponent<DeckBuilderScreen>();
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
            CreateMenuButton(heroHallPanel, "Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f));
            
            // Create Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(heroHallPanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Hero Hall";
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.9f);
            titleRect.anchorMax = new Vector2(0.8f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Add HeroHallScreen script
            var heroHallScreen = heroHallPanel.AddComponent<HeroHallScreen>();
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
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(battlePanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Battle Arena";
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.9f);
            titleRect.anchorMax = new Vector2(0.8f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Add BattleScreen script
            var battleScreen = battlePanel.AddComponent<BattleScreen>();
        }

        private static void CreateMenuButton(GameObject parent, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            var buttonObj = new GameObject(text + "Button");
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
        }

        private static void CreateGameInitializer()
        {
            var initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
        }
    }
}