using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace PotatoLegends.Editor
{
    public class CompleteGameSceneGenerator : EditorWindow
    {
        [MenuItem("Potato Legends/Generate Complete Game Scenes")]
        public static void ShowWindow()
        {
            GetWindow<CompleteGameSceneGenerator>("Complete Game Scene Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Complete Game Scene Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This will generate all scenes with complete UI structure:", EditorStyles.helpBox);
            GUILayout.Space(5);

            if (GUILayout.Button("🚀 Generate All Scenes", GUILayout.Height(40)))
            {
                GenerateAllScenes();
            }

            GUILayout.Space(10);
            GUILayout.Label("Individual Scene Generation:", EditorStyles.boldLabel);

            if (GUILayout.Button("🔐 Generate Auth Scene"))
            {
                GenerateAuthScene();
            }

            if (GUILayout.Button("🏠 Generate Main Menu Scene"))
            {
                GenerateMainMenuScene();
            }

            if (GUILayout.Button("📚 Generate Collection Scene"))
            {
                GenerateCollectionScene();
            }

            if (GUILayout.Button("🃏 Generate Deck Builder Scene"))
            {
                GenerateDeckBuilderScene();
            }

            if (GUILayout.Button("👑 Generate Hero Hall Scene"))
            {
                GenerateHeroHallScene();
            }

            if (GUILayout.Button("⚔️ Generate Battle Scene"))
            {
                GenerateBattleScene();
            }

            GUILayout.Space(10);
            GUILayout.Label("Utility Functions:", EditorStyles.boldLabel);

            if (GUILayout.Button("🧹 Clear Current Scene"))
            {
                ClearCurrentScene();
            }

            if (GUILayout.Button("💾 Save All Scenes"))
            {
                SaveAllScenes();
            }
        }

        private void GenerateAllScenes()
        {
            GenerateAuthScene();
            GenerateMainMenuScene();
            GenerateCollectionScene();
            GenerateDeckBuilderScene();
            GenerateHeroHallScene();
            GenerateBattleScene();
            
            Debug.Log("🎮 All game scenes generated successfully!");
            EditorUtility.DisplayDialog("Success", "All game scenes have been generated with complete UI structure!", "OK");
        }

        private void GenerateAuthScene()
        {
            ClearCurrentScene();
            
            // Create Camera
            CreateCamera();
            
            // Create UI Canvas
            GameObject canvas = CreateUICanvas();
            
            // Create Auth Screen
            CreateAuthScreen(canvas);
            
            // Save Scene
            SaveScene("Auth");
            Debug.Log("🔐 Auth scene generated!");
        }

        private void GenerateMainMenuScene()
        {
            ClearCurrentScene();
            
            CreateCamera();
            GameObject canvas = CreateUICanvas();
            CreateMainMenu(canvas);
            
            SaveScene("MainMenu");
            Debug.Log("🏠 Main Menu scene generated!");
        }

        private void GenerateCollectionScene()
        {
            ClearCurrentScene();
            
            CreateCamera();
            GameObject canvas = CreateUICanvas();
            CreateCollectionScreen(canvas);
            
            SaveScene("Collection");
            Debug.Log("📚 Collection scene generated!");
        }

        private void GenerateDeckBuilderScene()
        {
            ClearCurrentScene();
            
            CreateCamera();
            GameObject canvas = CreateUICanvas();
            CreateDeckBuilderScreen(canvas);
            
            SaveScene("DeckBuilder");
            Debug.Log("🃏 Deck Builder scene generated!");
        }

        private void GenerateHeroHallScene()
        {
            ClearCurrentScene();
            
            CreateCamera();
            GameObject canvas = CreateUICanvas();
            CreateHeroHallScreen(canvas);
            
            SaveScene("HeroHall");
            Debug.Log("👑 Hero Hall scene generated!");
        }

        private void GenerateBattleScene()
        {
            ClearCurrentScene();
            
            CreateCamera();
            GameObject canvas = CreateUICanvas();
            CreateBattleScreen(canvas);
            
            SaveScene("Battle");
            Debug.Log("⚔️ Battle scene generated!");
        }

        private void ClearCurrentScene()
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name != "Directional Light")
                {
                    DestroyImmediate(obj);
                }
            }
        }

        private void CreateCamera()
        {
            GameObject camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            
            Camera cam = camera.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.depth = -1;
            
            camera.transform.position = new Vector3(0, 0, -10);
        }

        private GameObject CreateUICanvas()
        {
            // Create Canvas
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            GraphicRaycaster raycaster = canvas.AddComponent<GraphicRaycaster>();

            // Configure Canvas
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            return canvas;
        }

        private void CreateAuthScreen(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", Vector2.zero, new Vector2(1920, 1080));
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Auth Container
            GameObject authContainer = CreateUIElement(parent, "AuthContainer", Vector2.zero, new Vector2(1920, 1080));

            // Auth Panel
            GameObject authPanel = CreateUIElement(authContainer, "AuthPanel", Vector2.zero, new Vector2(400, 600));
            Image panelImage = authPanel.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // Logo Area
            GameObject logoArea = CreateUIElement(authPanel, "LogoArea", new Vector2(0, 200), new Vector2(80, 80));
            Image logoImage = logoArea.AddComponent<Image>();
            logoImage.color = Color.yellow; // Placeholder for logo

            // Title
            GameObject title = CreateUIElement(authPanel, "Title", new Vector2(0, 100), new Vector2(300, 50));
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "What's My Potato?";
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;

            // Email Input
            GameObject emailInput = CreateUIElement(authPanel, "EmailInput", new Vector2(0, 0), new Vector2(300, 40));
            Image emailBg = emailInput.AddComponent<Image>();
            emailBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            TMP_InputField emailField = emailInput.AddComponent<TMP_InputField>();
            emailField.textComponent = CreateTextComponent(emailInput, "EmailText", "Email", new Color(0.8f, 0.8f, 0.8f, 1f));

            // Password Input
            GameObject passwordInput = CreateUIElement(authPanel, "PasswordInput", new Vector2(0, -60), new Vector2(300, 40));
            Image passwordBg = passwordInput.AddComponent<Image>();
            passwordBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            TMP_InputField passwordField = passwordInput.AddComponent<TMP_InputField>();
            passwordField.textComponent = CreateTextComponent(passwordInput, "PasswordText", "Password", new Color(0.8f, 0.8f, 0.8f, 1f));
            passwordField.contentType = TMP_InputField.ContentType.Password;

            // Sign In Button
            GameObject signInBtn = CreateUIElement(authPanel, "SignInButton", new Vector2(0, -120), new Vector2(300, 50));
            Image signInImage = signInBtn.AddComponent<Image>();
            signInImage.color = new Color(0.2f, 0.5f, 1f, 1f);
            Button signInButton = signInBtn.AddComponent<Button>();
            CreateTextComponent(signInBtn, "SignInText", "Sign In", Color.white);

            // Sign Up Button
            GameObject signUpBtn = CreateUIElement(authPanel, "SignUpButton", new Vector2(0, -180), new Vector2(300, 50));
            Image signUpImage = signUpBtn.AddComponent<Image>();
            signUpImage.color = new Color(0.5f, 0.2f, 1f, 1f);
            Button signUpButton = signUpBtn.AddComponent<Button>();
            CreateTextComponent(signUpBtn, "SignUpText", "Sign Up", Color.white);

            // Error Message
            GameObject errorMsg = CreateUIElement(authPanel, "ErrorMessage", new Vector2(0, -240), new Vector2(300, 30));
            TextMeshProUGUI errorText = errorMsg.AddComponent<TextMeshProUGUI>();
            errorText.text = "";
            errorText.fontSize = 12;
            errorText.color = Color.red;
            errorText.alignment = TextAlignmentOptions.Center;
            errorMsg.SetActive(false);
        }

        private void CreateMainMenu(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", Vector2.zero, new Vector2(1920, 1080));
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Main Menu Container
            GameObject mainMenu = CreateUIElement(parent, "MainMenu", Vector2.zero, new Vector2(1920, 1080));

            // Top Navigation Bar
            GameObject topBar = CreateUIElement(mainMenu, "TopBar", new Vector2(0, 400), new Vector2(1920, 80));
            Image topBarImage = topBar.AddComponent<Image>();
            topBarImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Logo Section
            GameObject logoSection = CreateUIElement(topBar, "LogoSection", new Vector2(-800, 0), new Vector2(200, 60));
            CreateTextComponent(logoSection, "GameTitle", "🥔 What's My Potato?", Color.white);

            // Navigation Buttons
            string[] navButtons = { "Collection", "Deck Builder", "Hero Hall", "Leaderboards", "Social" };
            for (int i = 0; i < navButtons.Length; i++)
            {
                GameObject navBtn = CreateUIElement(topBar, navButtons[i] + "Button", new Vector2(-400 + i * 150, 0), new Vector2(120, 40));
                Image navImage = navBtn.AddComponent<Image>();
                navImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                navBtn.AddComponent<Button>();
                CreateTextComponent(navBtn, navButtons[i] + "Text", navButtons[i], Color.white);
            }

            // User Menu (Top Right)
            GameObject userMenu = CreateUIElement(topBar, "UserMenu", new Vector2(800, 0), new Vector2(200, 60));
            CreateTextComponent(userMenu, "UserText", "Player Name", Color.white);

            // Main Title
            GameObject mainTitle = CreateUIElement(mainMenu, "MainTitle", new Vector2(0, 100), new Vector2(800, 100));
            TextMeshProUGUI titleText = mainTitle.AddComponent<TextMeshProUGUI>();
            titleText.text = "POTATO LEGENDS";
            titleText.fontSize = 48;
            titleText.color = Color.yellow;
            titleText.alignment = TextAlignmentOptions.Center;

            // Status Message
            GameObject statusMsg = CreateUIElement(mainMenu, "StatusMessage", new Vector2(0, 0), new Vector2(600, 40));
            TextMeshProUGUI statusText = statusMsg.AddComponent<TextMeshProUGUI>();
            statusText.text = "Ready to battle!";
            statusText.fontSize = 18;
            statusText.color = Color.green;
            statusText.alignment = TextAlignmentOptions.Center;

            // Battle Button (Bottom Right)
            GameObject battleBtn = CreateUIElement(mainMenu, "BattleButton", new Vector2(600, -300), new Vector2(200, 80));
            Image battleImage = battleBtn.AddComponent<Image>();
            battleImage.color = new Color(1f, 0.2f, 0.2f, 1f);
            Button battleButton = battleBtn.AddComponent<Button>();
            CreateTextComponent(battleBtn, "BattleText", "BATTLE", Color.white);

            // Settings Button (Bottom Left)
            GameObject settingsBtn = CreateUIElement(mainMenu, "SettingsButton", new Vector2(-600, -300), new Vector2(120, 50));
            Image settingsImage = settingsBtn.AddComponent<Image>();
            settingsImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            Button settingsButton = settingsBtn.AddComponent<Button>();
            CreateTextComponent(settingsBtn, "SettingsText", "Settings", Color.white);
        }

        private void CreateCollectionScreen(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", Vector2.zero, new Vector2(1920, 1080));
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Collection Container
            GameObject collection = CreateUIElement(parent, "Collection", Vector2.zero, new Vector2(1920, 1080));

            // Header
            GameObject header = CreateUIElement(collection, "Header", new Vector2(0, 400), new Vector2(1920, 80));
            Image headerImage = header.AddComponent<Image>();
            headerImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            CreateTextComponent(header, "Title", "Collection", Color.white);

            // Back Button
            GameObject backBtn = CreateUIElement(header, "BackButton", new Vector2(-800, 0), new Vector2(100, 40));
            Image backImage = backBtn.AddComponent<Image>();
            backImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            Button backButton = backBtn.AddComponent<Button>();
            CreateTextComponent(backBtn, "BackText", "Back", Color.white);

            // Search and Filters
            GameObject filters = CreateUIElement(collection, "Filters", new Vector2(0, 300), new Vector2(1920, 60));
            Image filtersImage = filters.AddComponent<Image>();
            filtersImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Search Field
            GameObject searchField = CreateUIElement(filters, "SearchField", new Vector2(-600, 0), new Vector2(300, 40));
            Image searchImage = searchField.AddComponent<Image>();
            searchImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            TMP_InputField searchInput = searchField.AddComponent<TMP_InputField>();
            searchInput.textComponent = CreateTextComponent(searchField, "SearchText", "Search cards...", new Color(0.8f, 0.8f, 0.8f, 1f));

            // Filter Buttons
            string[] filters_array = { "All", "Common", "Rare", "Epic", "Legendary" };
            for (int i = 0; i < filters_array.Length; i++)
            {
                GameObject filterBtn = CreateUIElement(filters, filters_array[i] + "Filter", new Vector2(-200 + i * 100, 0), new Vector2(80, 40));
                Image filterImage = filterBtn.AddComponent<Image>();
                filterImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                Button filterButton = filterBtn.AddComponent<Button>();
                CreateTextComponent(filterBtn, filters_array[i] + "Text", filters_array[i], Color.white);
            }

            // Collection Stats
            GameObject stats = CreateUIElement(collection, "Stats", new Vector2(600, 300), new Vector2(300, 60));
            CreateTextComponent(stats, "StatsText", "Cards: 150/500", Color.white);

            // Card Grid Area
            GameObject cardGridArea = CreateUIElement(collection, "CardGridArea", new Vector2(0, 0), new Vector2(1800, 500));
            Image gridImage = cardGridArea.AddComponent<Image>();
            gridImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Scroll View
            GameObject scrollView = CreateUIElement(cardGridArea, "ScrollView", Vector2.zero, new Vector2(1800, 500));
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Viewport
            GameObject viewport = CreateUIElement(scrollView, "Viewport", Vector2.zero, new Vector2(1800, 500));
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewport.AddComponent<Mask>();

            // Content
            GameObject content = CreateUIElement(viewport, "Content", Vector2.zero, new Vector2(1800, 1000));
            GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(150, 200);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 6;

            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Pagination
            GameObject pagination = CreateUIElement(collection, "Pagination", new Vector2(0, -300), new Vector2(400, 40));
            CreateTextComponent(pagination, "PageText", "Page 1 of 10", Color.white);
        }

        private void CreateDeckBuilderScreen(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", Vector2.zero, new Vector2(1920, 1080));
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Deck Builder Container
            GameObject deckBuilder = CreateUIElement(parent, "DeckBuilder", Vector2.zero, new Vector2(1920, 1080));

            // Header
            GameObject header = CreateUIElement(deckBuilder, "Header", new Vector2(0, 400), new Vector2(1920, 80));
            Image headerImage = header.AddComponent<Image>();
            headerImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            CreateTextComponent(header, "Title", "Deck Builder", Color.white);

            // Back Button
            GameObject backBtn = CreateUIElement(header, "BackButton", new Vector2(-800, 0), new Vector2(100, 40));
            Image backImage = backBtn.AddComponent<Image>();
            backImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            Button backButton = backBtn.AddComponent<Button>();
            CreateTextComponent(backBtn, "BackText", "Back", Color.white);

            // Left Panel (Card Library)
            GameObject leftPanel = CreateUIElement(deckBuilder, "LeftPanel", new Vector2(-600, 0), new Vector2(600, 800));
            Image leftImage = leftPanel.AddComponent<Image>();
            leftImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Library Header
            GameObject libraryHeader = CreateUIElement(leftPanel, "LibraryHeader", new Vector2(0, 350), new Vector2(580, 40));
            CreateTextComponent(libraryHeader, "LibraryTitle", "Card Library", Color.white);

            // Library Scroll View
            GameObject libraryScroll = CreateUIElement(leftPanel, "LibraryScroll", new Vector2(0, 0), new Vector2(580, 600));
            ScrollRect libraryScrollRect = libraryScroll.AddComponent<ScrollRect>();
            libraryScrollRect.horizontal = false;
            libraryScrollRect.vertical = true;

            // Library Viewport
            GameObject libraryViewport = CreateUIElement(libraryScroll, "LibraryViewport", Vector2.zero, new Vector2(580, 600));
            libraryViewport.AddComponent<Mask>();

            // Library Content
            GameObject libraryContent = CreateUIElement(libraryViewport, "LibraryContent", Vector2.zero, new Vector2(580, 1200));
            GridLayoutGroup libraryGrid = libraryContent.AddComponent<GridLayoutGroup>();
            libraryGrid.cellSize = new Vector2(120, 150);
            libraryGrid.spacing = new Vector2(5, 5);
            libraryGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            libraryGrid.constraintCount = 4;

            libraryScrollRect.content = libraryContent.GetComponent<RectTransform>();
            libraryScrollRect.viewport = libraryViewport.GetComponent<RectTransform>();

            // Right Panel (Deck Area)
            GameObject rightPanel = CreateUIElement(deckBuilder, "RightPanel", new Vector2(600, 0), new Vector2(600, 800));
            Image rightImage = rightPanel.AddComponent<Image>();
            rightImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Deck Header
            GameObject deckHeader = CreateUIElement(rightPanel, "DeckHeader", new Vector2(0, 350), new Vector2(580, 40));
            CreateTextComponent(deckHeader, "DeckTitle", "My Deck (0/30)", Color.white);

            // Deck Slots
            GameObject deckSlots = CreateUIElement(rightPanel, "DeckSlots", new Vector2(0, 200), new Vector2(500, 400));
            GridLayoutGroup deckGrid = deckSlots.AddComponent<GridLayoutGroup>();
            deckGrid.cellSize = new Vector2(80, 120);
            deckGrid.spacing = new Vector2(5, 5);
            deckGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            deckGrid.constraintCount = 5;

            // Deck Actions
            GameObject deckActions = CreateUIElement(rightPanel, "DeckActions", new Vector2(0, -300), new Vector2(500, 60));
            string[] actions = { "Save Deck", "Clear Deck", "Validate" };
            for (int i = 0; i < actions.Length; i++)
            {
                GameObject actionBtn = CreateUIElement(deckActions, actions[i] + "Button", new Vector2(-100 + i * 100, 0), new Vector2(80, 40));
                Image actionImage = actionBtn.AddComponent<Image>();
                actionImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                Button actionButton = actionBtn.AddComponent<Button>();
                CreateTextComponent(actionBtn, actions[i] + "Text", actions[i], Color.white);
            }
        }

        private void CreateHeroHallScreen(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", Vector2.zero, new Vector2(1920, 1080));
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Hero Hall Container
            GameObject heroHall = CreateUIElement(parent, "HeroHall", Vector2.zero, new Vector2(1920, 1080));

            // Header
            GameObject header = CreateUIElement(heroHall, "Header", new Vector2(0, 400), new Vector2(1920, 80));
            Image headerImage = header.AddComponent<Image>();
            headerImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            CreateTextComponent(header, "Title", "Hero Hall", Color.white);

            // Back Button
            GameObject backBtn = CreateUIElement(header, "BackButton", new Vector2(-800, 0), new Vector2(100, 40));
            Image backImage = backBtn.AddComponent<Image>();
            backImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            Button backButton = backBtn.AddComponent<Button>();
            CreateTextComponent(backBtn, "BackText", "Back", Color.white);

            // Selected Hero Area
            GameObject selectedHero = CreateUIElement(heroHall, "SelectedHero", new Vector2(0, 200), new Vector2(400, 300));
            Image selectedImage = selectedHero.AddComponent<Image>();
            selectedImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Hero Image
            GameObject heroImage = CreateUIElement(selectedHero, "HeroImage", new Vector2(0, 50), new Vector2(120, 120));
            Image heroImg = heroImage.AddComponent<Image>();
            heroImg.color = Color.gray; // Placeholder

            // Hero Name
            GameObject heroName = CreateUIElement(selectedHero, "HeroName", new Vector2(0, -50), new Vector2(300, 30));
            CreateTextComponent(heroName, "HeroNameText", "Select a Hero", Color.white);

            // Hero Stats
            GameObject heroStats = CreateUIElement(selectedHero, "HeroStats", new Vector2(0, -100), new Vector2(300, 100));
            CreateTextComponent(heroStats, "HeroStatsText", "Health: 100\nAttack: 50\nDefense: 30", Color.white);

            // Hero Grid
            GameObject heroGrid = CreateUIElement(heroHall, "HeroGrid", new Vector2(0, -200), new Vector2(1600, 400));
            Image gridImage = heroGrid.AddComponent<Image>();
            gridImage.color = new Color(0.15f, 0.15f, 0.15f, 0.5f);

            // Hero Scroll View
            GameObject heroScroll = CreateUIElement(heroGrid, "HeroScroll", Vector2.zero, new Vector2(1600, 400));
            ScrollRect heroScrollRect = heroScroll.AddComponent<ScrollRect>();
            heroScrollRect.horizontal = true;
            heroScrollRect.vertical = false;

            // Hero Viewport
            GameObject heroViewport = CreateUIElement(heroScroll, "HeroViewport", Vector2.zero, new Vector2(1600, 400));
            heroViewport.AddComponent<Mask>();

            // Hero Content
            GameObject heroContent = CreateUIElement(heroViewport, "HeroContent", Vector2.zero, new Vector2(2000, 400));
            GridLayoutGroup heroGridLayout = heroContent.AddComponent<GridLayoutGroup>();
            heroGridLayout.cellSize = new Vector2(150, 200);
            heroGridLayout.spacing = new Vector2(10, 10);
            heroGridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            heroGridLayout.constraintCount = 1;

            heroScrollRect.content = heroContent.GetComponent<RectTransform>();
            heroScrollRect.viewport = heroViewport.GetComponent<RectTransform>();
        }

        private void CreateBattleScreen(GameObject parent)
        {
            // Background
            GameObject background = CreateUIElement(parent, "Background", Vector2.zero, new Vector2(1920, 1080));
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Battle UI Container
            GameObject battleUI = CreateUIElement(parent, "BattleUI", Vector2.zero, new Vector2(1920, 1080));

            // Top HUD
            GameObject topHUD = CreateUIElement(battleUI, "TopHUD", new Vector2(0, 400), new Vector2(1920, 100));
            Image topHUDImage = topHUD.AddComponent<Image>();
            topHUDImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Player HUD
            GameObject playerHUD = CreateUIElement(topHUD, "PlayerHUD", new Vector2(-600, 0), new Vector2(400, 80));
            CreateTextComponent(playerHUD, "PlayerName", "Player", Color.white);
            
            GameObject playerHealth = CreateUIElement(playerHUD, "PlayerHealth", new Vector2(0, -20), new Vector2(200, 20));
            Image playerHealthImage = playerHealth.AddComponent<Image>();
            playerHealthImage.color = Color.green;

            // Enemy HUD
            GameObject enemyHUD = CreateUIElement(topHUD, "EnemyHUD", new Vector2(600, 0), new Vector2(400, 80));
            CreateTextComponent(enemyHUD, "EnemyName", "Enemy", Color.white);
            
            GameObject enemyHealth = CreateUIElement(enemyHUD, "EnemyHealth", new Vector2(0, -20), new Vector2(200, 20));
            Image enemyHealthImage = enemyHealth.AddComponent<Image>();
            enemyHealthImage.color = Color.red;

            // Battlefield
            GameObject battlefield = CreateUIElement(battleUI, "Battlefield", new Vector2(0, 0), new Vector2(1600, 400));
            Image battlefieldImage = battlefield.AddComponent<Image>();
            battlefieldImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Player Area
            GameObject playerArea = CreateUIElement(battlefield, "PlayerArea", new Vector2(0, -100), new Vector2(1600, 100));
            Image playerAreaImage = playerArea.AddComponent<Image>();
            playerAreaImage.color = new Color(0.2f, 0.5f, 0.2f, 0.3f);

            // Enemy Area
            GameObject enemyArea = CreateUIElement(battlefield, "EnemyArea", new Vector2(0, 100), new Vector2(1600, 100));
            Image enemyAreaImage = enemyArea.AddComponent<Image>();
            enemyAreaImage.color = new Color(0.5f, 0.2f, 0.2f, 0.3f);

            // Hand Area
            GameObject handArea = CreateUIElement(battleUI, "HandArea", new Vector2(0, -300), new Vector2(1600, 150));
            Image handImage = handArea.AddComponent<Image>();
            handImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Hand Cards Scroll
            GameObject handScroll = CreateUIElement(handArea, "HandScroll", Vector2.zero, new Vector2(1600, 150));
            ScrollRect handScrollRect = handScroll.AddComponent<ScrollRect>();
            handScrollRect.horizontal = true;
            handScrollRect.vertical = false;

            // Hand Viewport
            GameObject handViewport = CreateUIElement(handScroll, "HandViewport", Vector2.zero, new Vector2(1600, 150));
            handViewport.AddComponent<Mask>();

            // Hand Content
            GameObject handContent = CreateUIElement(handViewport, "HandContent", Vector2.zero, new Vector2(2000, 150));
            GridLayoutGroup handGrid = handContent.AddComponent<GridLayoutGroup>();
            handGrid.cellSize = new Vector2(100, 120);
            handGrid.spacing = new Vector2(5, 5);
            handGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            handGrid.constraintCount = 1;

            handScrollRect.content = handContent.GetComponent<RectTransform>();
            handScrollRect.viewport = handViewport.GetComponent<RectTransform>();

            // Action Buttons
            GameObject actionButtons = CreateUIElement(battleUI, "ActionButtons", new Vector2(600, -400), new Vector2(300, 80));
            string[] actions = { "Attack", "Defend", "Ability", "End Turn" };
            for (int i = 0; i < actions.Length; i++)
            {
                GameObject actionBtn = CreateUIElement(actionButtons, actions[i] + "Button", new Vector2(-100 + i * 50, 0), new Vector2(60, 40));
                Image actionImage = actionBtn.AddComponent<Image>();
                actionImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                Button actionButton = actionBtn.AddComponent<Button>();
                CreateTextComponent(actionBtn, actions[i] + "Text", actions[i], Color.white);
            }

            // Battle Log
            GameObject battleLog = CreateUIElement(battleUI, "BattleLog", new Vector2(-600, -400), new Vector2(400, 200));
            Image logImage = battleLog.AddComponent<Image>();
            logImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            CreateTextComponent(battleLog, "LogText", "Battle started!\nWaiting for your move...", Color.white);
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

        private TextMeshProUGUI CreateTextComponent(GameObject parent, string name, string text, Color color)
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
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            return textComponent;
        }

        private void SaveScene(string sceneName)
        {
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        }

        private void SaveAllScenes()
        {
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("💾 All scenes saved!");
        }
    }
}