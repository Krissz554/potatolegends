using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace PotatoLegends.Editor
{
    public class SceneHierarchyBuilder : EditorWindow
    {
        [MenuItem("Potato Legends/Build Scene Hierarchy")]
        public static void ShowWindow()
        {
            GetWindow<SceneHierarchyBuilder>("Scene Hierarchy Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Scene Hierarchy Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Build Auth Scene Hierarchy"))
            {
                BuildAuthScene();
            }

            if (GUILayout.Button("Build Main Menu Hierarchy"))
            {
                BuildMainMenuScene();
            }

            if (GUILayout.Button("Build Collection Scene Hierarchy"))
            {
                BuildCollectionScene();
            }

            if (GUILayout.Button("Build Deck Builder Scene Hierarchy"))
            {
                BuildDeckBuilderScene();
            }

            if (GUILayout.Button("Build Hero Hall Scene Hierarchy"))
            {
                BuildHeroHallScene();
            }

            if (GUILayout.Button("Build Battle Scene Hierarchy"))
            {
                BuildBattleScene();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Build All Scenes"))
            {
                BuildAllScenes();
            }
        }

        private void BuildAuthScene()
        {
            // Clear existing hierarchy
            ClearScene();

            // Create Game Managers
            GameObject gameManagers = CreateGameManagers();

            // Create Camera
            GameObject camera = CreateCamera();

            // Create UI Canvas
            GameObject uiCanvas = CreateUICanvas();

            // Create Auth Screen UI
            CreateAuthScreenUI(uiCanvas);

            Debug.Log("Auth Scene hierarchy built successfully!");
        }

        private void BuildMainMenuScene()
        {
            ClearScene();
            GameObject gameManagers = CreateGameManagers();
            GameObject camera = CreateCamera();
            GameObject uiCanvas = CreateUICanvas();
            CreateMainMenuUI(uiCanvas);
            Debug.Log("Main Menu Scene hierarchy built successfully!");
        }

        private void BuildCollectionScene()
        {
            ClearScene();
            GameObject gameManagers = CreateGameManagers();
            GameObject camera = CreateCamera();
            GameObject uiCanvas = CreateUICanvas();
            CreateCollectionUI(uiCanvas);
            Debug.Log("Collection Scene hierarchy built successfully!");
        }

        private void BuildDeckBuilderScene()
        {
            ClearScene();
            GameObject gameManagers = CreateGameManagers();
            GameObject camera = CreateCamera();
            GameObject uiCanvas = CreateUICanvas();
            CreateDeckBuilderUI(uiCanvas);
            Debug.Log("Deck Builder Scene hierarchy built successfully!");
        }

        private void BuildHeroHallScene()
        {
            ClearScene();
            GameObject gameManagers = CreateGameManagers();
            GameObject camera = CreateCamera();
            GameObject uiCanvas = CreateUICanvas();
            CreateHeroHallUI(uiCanvas);
            Debug.Log("Hero Hall Scene hierarchy built successfully!");
        }

        private void BuildBattleScene()
        {
            ClearScene();
            GameObject gameManagers = CreateGameManagers();
            GameObject camera = CreateCamera();
            GameObject uiCanvas = CreateUICanvas();
            CreateBattleUI(uiCanvas);
            Debug.Log("Battle Scene hierarchy built successfully!");
        }

        private void BuildAllScenes()
        {
            BuildAuthScene();
            BuildMainMenuScene();
            BuildCollectionScene();
            BuildDeckBuilderScene();
            BuildHeroHallScene();
            BuildBattleScene();
            Debug.Log("All scenes built successfully!");
        }

        private void ClearScene()
        {
            // Find and destroy all GameObjects except the ones we want to keep
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name != "Directional Light" && obj.name != "Main Camera")
                {
                    DestroyImmediate(obj);
                }
            }
        }

        private GameObject CreateGameManagers()
        {
            GameObject gameManagers = new GameObject("Game Managers");
            
            // Create individual managers
            CreateManager(gameManagers, "GameManager");
            CreateManager(gameManagers, "AudioManager");
            CreateManager(gameManagers, "InputManager");
            CreateManager(gameManagers, "UIManager");

            return gameManagers;
        }

        private void CreateManager(GameObject parent, string name)
        {
            GameObject manager = new GameObject(name);
            manager.transform.SetParent(parent.transform);
            // Add appropriate scripts here when they're ready
        }

        private GameObject CreateCamera()
        {
            GameObject camera = new GameObject("Camera");
            Camera cam = camera.AddComponent<Camera>();
            AudioListener listener = camera.AddComponent<AudioListener>();
            
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.depth = -1;
            
            camera.transform.position = new Vector3(0, 0, -10);

            return camera;
        }

        private GameObject CreateUICanvas()
        {
            GameObject canvas = new GameObject("UI");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            GraphicRaycaster raycaster = canvas.AddComponent<GraphicRaycaster>();

            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            return canvas;
        }

        private void CreateAuthScreenUI(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", new Vector2(0, 0), new Vector2(1920, 1080));
            background.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Auth Screen Container
            GameObject authContainer = CreateUIElement(parent, "AuthScreen", new Vector2(0, 0), new Vector2(1920, 1080));

            // Auth Panel
            GameObject authPanel = CreateUIElement(authContainer, "AuthPanel", new Vector2(0, 0), new Vector2(400, 600));
            authPanel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // Logo
            GameObject logo = CreateUIElement(authPanel, "Logo", new Vector2(0, 200), new Vector2(80, 80));
            logo.AddComponent<Image>().color = Color.yellow;

            // Title
            GameObject title = CreateUIElement(authPanel, "Title", new Vector2(0, 100), new Vector2(300, 50));
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "What's My Potato?";
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;

            // Email Input
            GameObject emailInput = CreateUIElement(authPanel, "EmailInput", new Vector2(0, 0), new Vector2(300, 40));
            emailInput.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
            TMP_InputField emailField = emailInput.AddComponent<TMP_InputField>();
            emailField.textComponent = CreateTextComponent(emailInput, "EmailText", "Email");

            // Password Input
            GameObject passwordInput = CreateUIElement(authPanel, "PasswordInput", new Vector2(0, -60), new Vector2(300, 40));
            passwordInput.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
            TMP_InputField passwordField = passwordInput.AddComponent<TMP_InputField>();
            passwordField.textComponent = CreateTextComponent(passwordInput, "PasswordText", "Password");
            passwordField.contentType = TMP_InputField.ContentType.Password;

            // Sign In Button
            GameObject signInBtn = CreateUIElement(authPanel, "SignInButton", new Vector2(0, -120), new Vector2(300, 50));
            signInBtn.AddComponent<Image>().color = new Color(0.2f, 0.5f, 1f, 1f);
            Button signInButton = signInBtn.AddComponent<Button>();
            CreateTextComponent(signInBtn, "SignInText", "Sign In");

            // Sign Up Button
            GameObject signUpBtn = CreateUIElement(authPanel, "SignUpButton", new Vector2(0, -180), new Vector2(300, 50));
            signUpBtn.AddComponent<Image>().color = new Color(0.5f, 0.2f, 1f, 1f);
            Button signUpButton = signUpBtn.AddComponent<Button>();
            CreateTextComponent(signUpBtn, "SignUpText", "Sign Up");
        }

        private void CreateMainMenuUI(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", new Vector2(0, 0), new Vector2(1920, 1080));
            background.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Main Menu Container
            GameObject mainMenu = CreateUIElement(parent, "MainMenu", new Vector2(0, 0), new Vector2(1920, 1080));

            // Top Bar
            GameObject topBar = CreateUIElement(mainMenu, "TopBar", new Vector2(0, 400), new Vector2(1920, 80));
            topBar.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Logo Section
            GameObject logoSection = CreateUIElement(topBar, "LogoSection", new Vector2(-800, 0), new Vector2(200, 60));
            CreateTextComponent(logoSection, "GameTitle", "🥔 What's My Potato?");

            // Navigation Buttons
            string[] navButtons = { "Collection", "Deck Builder", "Hero Hall", "Leaderboards", "Social" };
            for (int i = 0; i < navButtons.Length; i++)
            {
                GameObject navBtn = CreateUIElement(topBar, navButtons[i] + "Button", new Vector2(-400 + i * 150, 0), new Vector2(120, 40));
                navBtn.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
                navBtn.AddComponent<Button>();
                CreateTextComponent(navBtn, navButtons[i] + "Text", navButtons[i]);
            }

            // Main Title
            GameObject mainTitle = CreateUIElement(mainMenu, "MainTitle", new Vector2(0, 100), new Vector2(800, 100));
            TextMeshProUGUI titleText = mainTitle.AddComponent<TextMeshProUGUI>();
            titleText.text = "POTATO LEGENDS";
            titleText.fontSize = 48;
            titleText.color = Color.yellow;
            titleText.alignment = TextAlignmentOptions.Center;

            // Battle Button
            GameObject battleBtn = CreateUIElement(mainMenu, "BattleButton", new Vector2(600, -300), new Vector2(200, 80));
            battleBtn.AddComponent<Image>().color = new Color(1f, 0.2f, 0.2f, 1f);
            Button battleButton = battleBtn.AddComponent<Button>();
            CreateTextComponent(battleBtn, "BattleText", "BATTLE");
        }

        private void CreateCollectionUI(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", new Vector2(0, 0), new Vector2(1920, 1080));
            background.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Collection Container
            GameObject collection = CreateUIElement(parent, "Collection", new Vector2(0, 0), new Vector2(1920, 1080));

            // Header
            GameObject header = CreateUIElement(collection, "Header", new Vector2(0, 400), new Vector2(1920, 80));
            header.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            CreateTextComponent(header, "Title", "Collection");

            // Filters
            GameObject filters = CreateUIElement(collection, "Filters", new Vector2(0, 300), new Vector2(1920, 60));
            filters.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Search Field
            GameObject searchField = CreateUIElement(filters, "SearchField", new Vector2(-600, 0), new Vector2(300, 40));
            searchField.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
            TMP_InputField searchInput = searchField.AddComponent<TMP_InputField>();
            searchInput.textComponent = CreateTextComponent(searchField, "SearchText", "Search cards...");

            // Card Grid
            GameObject cardGrid = CreateUIElement(collection, "CardGrid", new Vector2(0, 0), new Vector2(1800, 500));
            cardGrid.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Scroll View
            GameObject scrollView = CreateUIElement(cardGrid, "ScrollView", new Vector2(0, 0), new Vector2(1800, 500));
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Viewport
            GameObject viewport = CreateUIElement(scrollView, "Viewport", new Vector2(0, 0), new Vector2(1800, 500));
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>();

            // Content
            GameObject content = CreateUIElement(viewport, "Content", new Vector2(0, 0), new Vector2(1800, 1000));
            GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(150, 200);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 6;

            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
        }

        private void CreateDeckBuilderUI(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", new Vector2(0, 0), new Vector2(1920, 1080));
            background.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Deck Builder Container
            GameObject deckBuilder = CreateUIElement(parent, "DeckBuilder", new Vector2(0, 0), new Vector2(1920, 1080));

            // Header
            GameObject header = CreateUIElement(deckBuilder, "Header", new Vector2(0, 400), new Vector2(1920, 80));
            header.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            CreateTextComponent(header, "Title", "Deck Builder");

            // Left Panel (Card Library)
            GameObject leftPanel = CreateUIElement(deckBuilder, "LeftPanel", new Vector2(-600, 0), new Vector2(600, 800));
            leftPanel.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Right Panel (Deck Area)
            GameObject rightPanel = CreateUIElement(deckBuilder, "RightPanel", new Vector2(600, 0), new Vector2(600, 800));
            rightPanel.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Deck Slots
            GameObject deckSlots = CreateUIElement(rightPanel, "DeckSlots", new Vector2(0, 200), new Vector2(500, 400));
            GridLayoutGroup deckGrid = deckSlots.AddComponent<GridLayoutGroup>();
            deckGrid.cellSize = new Vector2(80, 120);
            deckGrid.spacing = new Vector2(5, 5);
            deckGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            deckGrid.constraintCount = 5;
        }

        private void CreateHeroHallUI(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", new Vector2(0, 0), new Vector2(1920, 1080));
            background.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Hero Hall Container
            GameObject heroHall = CreateUIElement(parent, "HeroHall", new Vector2(0, 0), new Vector2(1920, 1080));

            // Header
            GameObject header = CreateUIElement(heroHall, "Header", new Vector2(0, 400), new Vector2(1920, 80));
            header.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            CreateTextComponent(header, "Title", "Hero Hall");

            // Selected Hero
            GameObject selectedHero = CreateUIElement(heroHall, "SelectedHero", new Vector2(0, 200), new Vector2(400, 300));
            selectedHero.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Hero Grid
            GameObject heroGrid = CreateUIElement(heroHall, "HeroGrid", new Vector2(0, -200), new Vector2(1600, 400));
            heroGrid.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            GridLayoutGroup heroGridLayout = heroGrid.AddComponent<GridLayoutGroup>();
            heroGridLayout.cellSize = new Vector2(150, 200);
            heroGridLayout.spacing = new Vector2(10, 10);
            heroGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            heroGridLayout.constraintCount = 8;
        }

        private void CreateBattleUI(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", new Vector2(0, 0), new Vector2(1920, 1080));
            background.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Battle UI Container
            GameObject battleUI = CreateUIElement(parent, "BattleUI", new Vector2(0, 0), new Vector2(1920, 1080));

            // Top HUD
            GameObject topHUD = CreateUIElement(battleUI, "TopHUD", new Vector2(0, 400), new Vector2(1920, 100));
            topHUD.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Player HUD
            GameObject playerHUD = CreateUIElement(topHUD, "PlayerHUD", new Vector2(-600, 0), new Vector2(400, 80));
            CreateTextComponent(playerHUD, "PlayerName", "Player");
            GameObject playerHealth = CreateUIElement(playerHUD, "PlayerHealth", new Vector2(0, -20), new Vector2(200, 20));
            playerHealth.AddComponent<Image>().color = Color.green;

            // Enemy HUD
            GameObject enemyHUD = CreateUIElement(topHUD, "EnemyHUD", new Vector2(600, 0), new Vector2(400, 80));
            CreateTextComponent(enemyHUD, "EnemyName", "Enemy");
            GameObject enemyHealth = CreateUIElement(enemyHUD, "EnemyHealth", new Vector2(0, -20), new Vector2(200, 20));
            enemyHealth.AddComponent<Image>().color = Color.red;

            // Battlefield
            GameObject battlefield = CreateUIElement(battleUI, "Battlefield", new Vector2(0, 0), new Vector2(1600, 400));
            battlefield.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Hand Area
            GameObject handArea = CreateUIElement(battleUI, "HandArea", new Vector2(0, -300), new Vector2(1600, 150));
            handArea.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Action Buttons
            GameObject actionButtons = CreateUIElement(battleUI, "ActionButtons", new Vector2(600, -400), new Vector2(300, 80));
            string[] actions = { "Attack", "Defend", "Ability", "End Turn" };
            for (int i = 0; i < actions.Length; i++)
            {
                GameObject actionBtn = CreateUIElement(actionButtons, actions[i] + "Button", new Vector2(-100 + i * 50, 0), new Vector2(60, 40));
                actionBtn.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
                actionBtn.AddComponent<Button>();
                CreateTextComponent(actionBtn, actions[i] + "Text", actions[i]);
            }
        }

        private GameObject CreateUIElement(GameObject parent, string name, Vector2 position, Vector2 size)
        {
            GameObject element = new GameObject(name);
            element.transform.SetParent(parent.transform);
            
            RectTransform rectTransform = element.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            return element;
        }

        private TextMeshProUGUI CreateTextComponent(GameObject parent, string name, string text)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform);
            
            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            return textComponent;
        }
    }
}