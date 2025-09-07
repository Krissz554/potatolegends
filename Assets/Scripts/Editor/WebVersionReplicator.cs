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
    public class WebVersionReplicator : EditorWindow
    {
        [MenuItem("Potato Legends/Replicate Web Version")]
        public static void ReplicateWebVersion()
        {
            Debug.Log("🎮 Replicating Web Version for Mobile...");
            
            CreateAuthScene();
            CreateMainMenuScene();
            CreateCollectionScene();
            CreateDeckBuilderScene();
            CreateHeroHallScene();
            CreateBattleScene();
            
            Debug.Log("✅ Web version replicated successfully for mobile!");
        }

        [MenuItem("Potato Legends/Create Auth Scene (Mobile)")]
        public static void CreateAuthScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Auth";
            
            // Create Main Camera
            CreateMainCamera();
            
            // Create Canvas with mobile optimization
            var canvas = CreateMobileCanvas("AuthCanvas");
            
            // Create Auth UI that replicates web version
            CreateWebStyleAuthUI(canvas);
            
            // Create GameInitializer
            CreateGameInitializer();
            
            // Save scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Auth.unity");
            Debug.Log("✅ Auth scene created with web-style UI!");
        }

        [MenuItem("Potato Legends/Create Main Menu Scene (Web Style)")]
        public static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("MainMenuCanvas");
            CreateWebStyleMainMenuUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MainMenu.unity");
            Debug.Log("✅ Main Menu scene created with web-style UI!");
        }

        [MenuItem("Potato Legends/Create Collection Scene (Web Style)")]
        public static void CreateCollectionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Collection";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("CollectionCanvas");
            CreateWebStyleCollectionUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Collection.unity");
            Debug.Log("✅ Collection scene created with web-style UI!");
        }

        [MenuItem("Potato Legends/Create Deck Builder Scene (Web Style)")]
        public static void CreateDeckBuilderScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "DeckBuilder";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("DeckBuilderCanvas");
            CreateWebStyleDeckBuilderUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/DeckBuilder.unity");
            Debug.Log("✅ Deck Builder scene created with web-style UI!");
        }

        [MenuItem("Potato Legends/Create Hero Hall Scene (Web Style)")]
        public static void CreateHeroHallScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "HeroHall";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("HeroHallCanvas");
            CreateWebStyleHeroHallUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/HeroHall.unity");
            Debug.Log("✅ Hero Hall scene created with web-style UI!");
        }

        [MenuItem("Potato Legends/Create Battle Scene (Web Style)")]
        public static void CreateBattleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Battle";
            
            CreateMainCamera();
            var canvas = CreateMobileCanvas("BattleCanvas");
            CreateWebStyleBattleUI(canvas);
            CreateGameInitializer();
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Battle.unity");
            Debug.Log("✅ Battle scene created with web-style UI!");
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

        private static void CreateGameInitializer()
        {
            var initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
        }

        private static void CreateWebStyleAuthUI(GameObject canvas)
        {
            // Create background with main pixel art
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(canvas.transform, false);
            
            var backgroundImage = backgroundObj.AddComponent<Image>();
            // Try to load the main pixel background
            var backgroundSprite = Resources.Load<Sprite>("Images/art/main-pixel");
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

            // Create Auth Panel (replicating web modal style)
            var authPanel = new GameObject("AuthPanel");
            authPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = authPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.2f);
            panelRect.anchorMax = new Vector2(0.9f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = authPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
            
            // Add rounded corners effect
            var panelMask = authPanel.AddComponent<Mask>();
            var panelMaskImage = authPanel.AddComponent<Image>();
            panelMaskImage.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

            // Create Title (replicating web style)
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(authPanel.transform, false);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Join the Potato Club! 🥔";
            titleText.fontSize = 36;
            titleText.color = new Color(1f, 0.8f, 0f, 1f); // Gold color
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Create Subtitle
            var subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(authPanel.transform, false);
            
            var subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Create an account to save your favorite potato personalities";
            subtitleText.fontSize = 18;
            subtitleText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            
            var subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.1f, 0.75f);
            subtitleRect.anchorMax = new Vector2(0.9f, 0.8f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;

            // Create Tab System (Sign In / Sign Up)
            CreateTabSystem(authPanel);

            // Create Email Input
            var emailInput = CreateWebStyleInputField(authPanel, "EmailInput", "Email", "your@email.com", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.6f));

            // Create Password Input
            var passwordInput = CreateWebStyleInputField(authPanel, "PasswordInput", "Password", "••••••••", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.45f));
            passwordInput.contentType = TMP_InputField.ContentType.Password;

            // Create Sign In Button
            var signInButton = CreateWebStyleButton(authPanel, "SignInButton", "Sign In", new Vector2(0.1f, 0.2f), new Vector2(0.45f, 0.3f), new Color(0.2f, 0.6f, 0.2f, 1f));

            // Create Sign Up Button
            var signUpButton = CreateWebStyleButton(authPanel, "SignUpButton", "Create Account", new Vector2(0.55f, 0.2f), new Vector2(0.9f, 0.3f), new Color(0.2f, 0.4f, 0.8f, 1f));

            // Create Google Sign In Button
            var googleButton = CreateWebStyleButton(authPanel, "GoogleSignInButton", "Sign in with Google", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.18f), new Color(0.9f, 0.9f, 0.9f, 1f));

            // Create Error Message
            var errorMessage = CreateText(authPanel, "ErrorMessage", "", 16, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.1f));
            errorMessage.color = new Color(1f, 0.3f, 0.3f, 1f);

            // Add AuthScreenUI script and assign references
            var authScreenUI = authPanel.AddComponent<AuthScreenUI>();
            AssignAuthReferences(authScreenUI, emailInput, passwordInput, signInButton, signUpButton, googleButton, errorMessage, null);
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
            var signInTab = CreateWebStyleButton(tabContainer, "SignInTab", "Sign In", new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Color(0.3f, 0.3f, 0.3f, 1f));

            // Create Sign Up Tab
            var signUpTab = CreateWebStyleButton(tabContainer, "SignUpTab", "Sign Up", new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Color(0.3f, 0.3f, 0.3f, 1f));
        }

        private static void CreateWebStyleMainMenuUI(GameObject canvas)
        {
            // Create background with main pixel art
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(canvas.transform, false);
            
            var backgroundImage = backgroundObj.AddComponent<Image>();
            var backgroundSprite = Resources.Load<Sprite>("Images/art/main-pixel");
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
            var logoObj = new GameObject("Logo");
            logoObj.transform.SetParent(navBar.transform, false);
            
            var logoText = logoObj.AddComponent<TextMeshProUGUI>();
            logoText.text = "What's My Potato?";
            logoText.fontSize = 32;
            logoText.color = Color.white;
            logoText.alignment = TextAlignmentOptions.Center;
            logoText.fontStyle = FontStyles.Bold;
            
            var logoRect = logoObj.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.1f, 0.2f);
            logoRect.anchorMax = new Vector2(0.9f, 0.8f);
            logoRect.offsetMin = Vector2.zero;
            logoRect.offsetMax = Vector2.zero;

            // Create Main Title
            var mainTitleObj = new GameObject("MainTitle");
            mainTitleObj.transform.SetParent(canvas.transform, false);
            
            var mainTitleText = mainTitleObj.AddComponent<TextMeshProUGUI>();
            mainTitleText.text = "POTATO LEGENDS";
            mainTitleText.fontSize = 72;
            mainTitleText.color = new Color(1f, 0.8f, 0f, 1f); // Gold
            mainTitleText.alignment = TextAlignmentOptions.Center;
            mainTitleText.fontStyle = FontStyles.Bold;
            
            var mainTitleRect = mainTitleObj.GetComponent<RectTransform>();
            mainTitleRect.anchorMin = new Vector2(0.1f, 0.6f);
            mainTitleRect.anchorMax = new Vector2(0.9f, 0.75f);
            mainTitleRect.offsetMin = Vector2.zero;
            mainTitleRect.offsetMax = Vector2.zero;

            // Create Subtitle
            var subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(canvas.transform, false);
            
            var subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Enter the mystical realm of trading card warfare";
            subtitleText.fontSize = 24;
            subtitleText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            
            var subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.1f, 0.55f);
            subtitleRect.anchorMax = new Vector2(0.9f, 0.6f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;

            // Create Menu Buttons (replicating web style)
            CreateWebStyleButton(canvas, "CollectionButton", "Collection", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.48f), new Color(0.2f, 0.6f, 0.4f, 0.8f));
            CreateWebStyleButton(canvas, "DeckBuilderButton", "Deck Builder", new Vector2(0.1f, 0.32f), new Vector2(0.9f, 0.4f), new Color(0.2f, 0.4f, 0.8f, 0.8f));
            CreateWebStyleButton(canvas, "HeroHallButton", "Hero Hall", new Vector2(0.1f, 0.24f), new Vector2(0.9f, 0.32f), new Color(0.8f, 0.6f, 0.2f, 0.8f));
            CreateWebStyleButton(canvas, "SettingsButton", "Settings", new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.24f), new Color(0.6f, 0.6f, 0.6f, 0.8f));

            // Create Battle Button (replicating web style)
            var battleButton = CreateWebStyleButton(canvas, "BattleButton", "BATTLE", new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.15f), new Color(0.8f, 0.2f, 0.2f, 1f));
            
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

        private static void CreateWebStyleCollectionUI(GameObject canvas)
        {
            // Create background
            CreateBackground(canvas);
            
            // Create Collection Panel
            var collectionPanel = new GameObject("CollectionPanel");
            collectionPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = collectionPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Create Back Button
            CreateWebStyleButton(collectionPanel, "BackButton", "← Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            
            // Create Title
            CreateText(collectionPanel, "Title", "COLLECTION", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            // Create Search Bar
            CreateWebStyleInputField(collectionPanel, "SearchInput", "Search cards...", "Search cards...", new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.85f));
            
            // Create Filter Buttons
            CreateWebStyleButton(collectionPanel, "AllFilterButton", "All", new Vector2(0.1f, 0.75f), new Vector2(0.2f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            CreateWebStyleButton(collectionPanel, "CommonFilterButton", "Common", new Vector2(0.25f, 0.75f), new Vector2(0.35f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            CreateWebStyleButton(collectionPanel, "RareFilterButton", "Rare", new Vector2(0.4f, 0.75f), new Vector2(0.5f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            CreateWebStyleButton(collectionPanel, "LegendaryFilterButton", "Legendary", new Vector2(0.55f, 0.75f), new Vector2(0.7f, 0.8f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
            
            // Add CollectionScreenUI script
            collectionPanel.AddComponent<CollectionScreenUI>();
        }

        private static void CreateWebStyleDeckBuilderUI(GameObject canvas)
        {
            CreateBackground(canvas);
            
            var deckBuilderPanel = new GameObject("DeckBuilderPanel");
            deckBuilderPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = deckBuilderPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            CreateWebStyleButton(deckBuilderPanel, "BackButton", "← Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            CreateText(deckBuilderPanel, "Title", "DECK BUILDER", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            deckBuilderPanel.AddComponent<DeckBuilderScreen>();
        }

        private static void CreateWebStyleHeroHallUI(GameObject canvas)
        {
            CreateBackground(canvas);
            
            var heroHallPanel = new GameObject("HeroHallPanel");
            heroHallPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = heroHallPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            CreateWebStyleButton(heroHallPanel, "BackButton", "← Back", new Vector2(0.05f, 0.9f), new Vector2(0.2f, 0.95f), new Color(0.4f, 0.2f, 0.2f, 0.8f));
            CreateText(heroHallPanel, "Title", "HERO HALL", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            heroHallPanel.AddComponent<HeroHallScreen>();
        }

        private static void CreateWebStyleBattleUI(GameObject canvas)
        {
            CreateBackground(canvas);
            
            var battlePanel = new GameObject("BattlePanel");
            battlePanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = battlePanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            CreateText(battlePanel, "Title", "BATTLE ARENA", 48, new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.95f));
            
            battlePanel.AddComponent<BattleScreen>();
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

        private static Button CreateWebStyleButton(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
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

        private static TMP_InputField CreateWebStyleInputField(GameObject parent, string name, string placeholder, string text, Vector2 anchorMin, Vector2 anchorMax)
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
    }
}