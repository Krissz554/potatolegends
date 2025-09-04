using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// PRODUCTION-QUALITY UI Manager for mobile card game
    /// Clash Royale / Legends of Runeterra style interface
    /// Fantasy + Elemental Potato World theme with pixel-art backgrounds
    /// </summary>
    public class ProductionUIManager : MonoBehaviour
    {
        [Header("🎮 Production Mobile Card Game UI")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool debugMode = true;
        
        [Header("🎨 UI Asset References")]
        [SerializeField] private UIAssetLibrary assetLibrary;
        
        // Game state management
        public enum GameScreen { Auth, MainMenu, Collection, DeckBuilder, HeroHall, BattleMatchmaking, Battle }
        private GameScreen currentScreen = GameScreen.Auth;
        private GameScreen previousScreen = GameScreen.Auth;
        
        // Core systems
        private RealSupabaseClient supabaseClient;
        private Canvas gameCanvas;
        private CanvasGroup currentScreenGroup;
        private Animator screenAnimator;
        
        // UI References
        private GameObject currentScreenObject;
        private Dictionary<GameScreen, GameObject> screenCache = new Dictionary<GameScreen, GameObject>();
        
        // Auth screen references
        private TMP_InputField emailField;
        private TMP_InputField passwordField;
        private TextMeshProUGUI statusText;
        private Button loginButton;
        private Button registerButton;
        
        // Main menu references
        private TextMeshProUGUI playerNameText;
        private TextMeshProUGUI playerStatsText;
        private Button battleButton;
        private Button collectionButton;
        private Button deckBuilderButton;
        private Button heroHallButton;
        
        // Elemental color scheme (Fantasy + Elemental Potato World)
        private readonly ElementalColorPalette colors = new ElementalColorPalette();
        
        private void Start()
        {
            if (autoStart)
            {
                InitializeProductionUI();
            }
        }
        
        private async void InitializeProductionUI()
        {
            Debug.Log("🎮 Initializing PRODUCTION-QUALITY Potato Legends Mobile UI...");
            
            // Create asset library if not assigned
            if (assetLibrary == null)
            {
                assetLibrary = CreateAssetLibrary();
            }
            
            // Setup core systems
            await SetupCoreUISystem();
            
            // Create Supabase integration
            SetupSupabaseIntegration();
            
            // Show initial screen
            await ShowScreen(GameScreen.Auth, true);
            
            Debug.Log("✅ PRODUCTION UI initialized - showing polished auth screen!");
        }
        
        private async Task SetupCoreUISystem()
        {
            // Create production-quality canvas
            CreateProductionCanvas();
            
            // Setup mobile scaling and safe areas
            SetupMobileScaling();
            
            // Create screen animator for transitions
            SetupScreenAnimator();
            
            Debug.Log("🎨 Production UI system setup complete");
            
            await Task.Yield();
        }
        
        private void CreateProductionCanvas()
        {
            // Main game canvas with professional setup
            GameObject canvasObj = new GameObject("Production Game Canvas");
            gameCanvas = canvasObj.AddComponent<Canvas>();
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameCanvas.sortingOrder = 100;
            
            // Mobile-first scaling (supports all screen sizes and notches)
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Modern mobile resolution
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balanced scaling
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Professional EventSystem with New Input System
            CreateEventSystem();
            
            Debug.Log("🎨 Production canvas created with mobile scaling");
        }
        
        private void CreateEventSystem()
        {
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("Production EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
                // Use NEW Input System for Unity 6 (better mobile support)
                var inputModule = eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                
                Debug.Log("✅ Created EventSystem with NEW Input System for mobile");
            }
        }
        
        private void SetupMobileScaling()
        {
            // Add safe area support for mobile devices (notches, etc.)
            GameObject safeAreaObj = new GameObject("Safe Area Handler");
            safeAreaObj.transform.SetParent(gameCanvas.transform, false);
            safeAreaObj.AddComponent<SafeAreaHandler>();
            
            Debug.Log("📱 Mobile scaling and safe area support enabled");
        }
        
        private void SetupScreenAnimator()
        {
            // Create animator for smooth screen transitions
            GameObject animatorObj = new GameObject("Screen Animator");
            animatorObj.transform.SetParent(gameCanvas.transform, false);
            screenAnimator = animatorObj.AddComponent<Animator>();
            
            // TODO: Create AnimatorController for screen transitions
            Debug.Log("✨ Screen animator setup for smooth transitions");
        }
        
        private void SetupSupabaseIntegration()
        {
            // Create Supabase client if not exists
            var existingClient = FindFirstObjectByType<RealSupabaseClient>();
            if (existingClient == null)
            {
                GameObject supabaseObj = new GameObject("Production Supabase Client");
                supabaseClient = supabaseObj.AddComponent<RealSupabaseClient>();
            }
            else
            {
                supabaseClient = existingClient;
            }
            
            // Subscribe to authentication events
            supabaseClient.OnAuthenticationChanged += OnAuthenticationChanged;
            supabaseClient.OnError += OnSupabaseError;
            
            Debug.Log("🔌 Supabase integration setup for production UI");
        }
        
        #region Screen Management
        
        public async Task ShowScreen(GameScreen targetScreen, bool immediate = false)
        {
            Debug.Log($"🔄 Transitioning to {targetScreen} screen");
            
            previousScreen = currentScreen;
            currentScreen = targetScreen;
            
            // Animate out current screen
            if (!immediate && currentScreenObject != null)
            {
                StartCoroutine(AnimateScreenOut());
                await Task.Delay(200); // Wait for animation
            }
            
            // Clear current screen
            ClearCurrentScreen();
            
            // Create new screen
            await CreateScreen(targetScreen);
            
            // Animate in new screen
            if (!immediate)
            {
                StartCoroutine(AnimateScreenIn());
            }
            
            Debug.Log($"✅ Successfully transitioned to {targetScreen}");
        }
        
        private async Task CreateScreen(GameScreen screen)
        {
            switch (screen)
            {
                case GameScreen.Auth:
                    currentScreenObject = await CreateAuthScreen();
                    break;
                case GameScreen.MainMenu:
                    currentScreenObject = await CreateMainMenuScreen();
                    break;
                case GameScreen.Collection:
                    currentScreenObject = await CreateCollectionScreen();
                    break;
                case GameScreen.DeckBuilder:
                    currentScreenObject = await CreateDeckBuilderScreen();
                    break;
                case GameScreen.HeroHall:
                    currentScreenObject = await CreateHeroHallScreen();
                    break;
                case GameScreen.BattleMatchmaking:
                    currentScreenObject = await CreateBattleMatchmakingScreen();
                    break;
                default:
                    Debug.LogError($"❌ Unknown screen: {screen}");
                    break;
            }
            
            // Add CanvasGroup for animations
            if (currentScreenObject != null)
            {
                currentScreenGroup = currentScreenObject.AddComponent<CanvasGroup>();
            }
        }
        
        #endregion
        
        #region Auth Screen (Production Quality)
        
        private async Task<GameObject> CreateAuthScreen()
        {
            Debug.Log("🔐 Creating PRODUCTION auth screen...");
            
            // Main background with fantasy theme
            GameObject authScreen = CreateFullscreenBackground("Auth Screen", assetLibrary.authBackground, colors.darkBackground);
            
            // Add atmospheric particles/effects placeholder
            CreateAtmosphericEffects(authScreen.transform);
            
            // Central auth panel with fantasy frame
            GameObject authPanel = CreateFantasyPanel(
                "Auth Panel", 
                authScreen.transform,
                new Vector2(0.1f, 0.2f), 
                new Vector2(0.9f, 0.8f),
                assetLibrary.authPanelFrame
            );
            
            // Game logo with fantasy styling
            CreateGameLogo(authPanel.transform);
            
            // Professional input fields
            emailField = CreateFantasyInputField(
                "Email Field", 
                authPanel.transform, 
                "Email Address", 
                new Vector2(0.1f, 0.55f), 
                new Vector2(0.9f, 0.65f)
            );
            
            passwordField = CreateFantasyInputField(
                "Password Field", 
                authPanel.transform, 
                "Password", 
                new Vector2(0.1f, 0.42f), 
                new Vector2(0.9f, 0.52f)
            );
            passwordField.inputType = TMP_InputField.InputType.Password;
            
            // Fantasy-styled buttons
            loginButton = CreateFantasyButton(
                "Login Button", 
                authPanel.transform, 
                "ENTER REALM", 
                colors.primaryGreen,
                assetLibrary.primaryButtonSprite,
                new Vector2(0.1f, 0.25f), 
                new Vector2(0.9f, 0.35f)
            );
            
            loginButton.onClick.AddListener(async () => await HandleLogin());
            
            // Register toggle button
            registerButton = CreateFantasyButton(
                "Register Button", 
                authPanel.transform, 
                "CREATE ACCOUNT", 
                colors.primaryBlue,
                assetLibrary.secondaryButtonSprite,
                new Vector2(0.1f, 0.12f), 
                new Vector2(0.9f, 0.22f)
            );
            
            registerButton.onClick.AddListener(() => ToggleAuthMode());
            
            // Status text with fantasy styling
            statusText = CreateFantasyStatusText(
                authPanel.transform, 
                "Welcome to the Elemental Potato Realm!\nEnter your credentials to begin your journey.",
                new Vector2(0.1f, 0.02f), 
                new Vector2(0.9f, 0.1f)
            );
            
            await Task.Yield();
            return authScreen;
        }
        
        private void CreateGameLogo(Transform parent)
        {
            GameObject logoContainer = new GameObject("Game Logo");
            logoContainer.transform.SetParent(parent, false);
            logoContainer.layer = 5;
            
            RectTransform logoRect = logoContainer.AddComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.1f, 0.7f);
            logoRect.anchorMax = new Vector2(0.9f, 0.9f);
            logoRect.offsetMin = Vector2.zero;
            logoRect.offsetMax = Vector2.zero;
            
            // Main title with fantasy font
            GameObject titleObj = new GameObject("Game Title");
            titleObj.transform.SetParent(logoContainer.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "POTATO LEGENDS";
            titleText.fontSize = 42;
            titleText.color = colors.legendaryGold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            // Add text outline for readability
            titleText.outlineColor = Color.black;
            titleText.outlineWidth = 0.3f;
            
            SetFullScreen(titleObj.GetComponent<RectTransform>());
            
            // Subtitle
            GameObject subtitleObj = new GameObject("Game Subtitle");
            subtitleObj.transform.SetParent(logoContainer.transform, false);
            subtitleObj.layer = 5;
            
            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Elemental Card Arena";
            subtitleText.fontSize = 18;
            subtitleText.color = colors.textSecondary;
            subtitleText.alignment = TextAlignmentOptions.Center;
            
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0.3f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
            
            // Add logo glow effect
            StartCoroutine(AnimateLogoGlow(titleText));
        }
        
        #endregion
        
        #region Main Menu Screen (Card-Based Navigation)
        
        private async Task<GameObject> CreateMainMenuScreen()
        {
            Debug.Log("🏠 Creating PRODUCTION main menu...");
            
            // Main background with fantasy atmosphere
            GameObject mainScreen = CreateFullscreenBackground("Main Menu", assetLibrary.mainMenuBackground, colors.darkBackground);
            
            // Atmospheric effects
            CreateAtmosphericEffects(mainScreen.transform);
            
            // Top header with player info
            CreatePlayerHeader(mainScreen.transform);
            
            // Central navigation cards (Clash Royale style)
            CreateNavigationCardGrid(mainScreen.transform);
            
            // Prominent battle button (bottom right)
            CreateProductionBattleButton(mainScreen.transform);
            
            // Bottom utility bar
            CreateBottomUtilityBar(mainScreen.transform);
            
            await Task.Yield();
            return mainScreen;
        }
        
        private void CreatePlayerHeader(Transform parent)
        {
            GameObject headerPanel = CreateFantasyPanel(
                "Player Header",
                parent,
                new Vector2(0.05f, 0.88f),
                new Vector2(0.95f, 0.98f),
                assetLibrary.headerPanelSprite
            );
            
            // Player avatar placeholder
            GameObject avatarObj = CreateFantasyPanel(
                "Player Avatar",
                headerPanel.transform,
                new Vector2(0.02f, 0.1f),
                new Vector2(0.15f, 0.9f),
                assetLibrary.avatarFrameSprite
            );
            
            Image avatarImage = avatarObj.GetComponent<Image>();
            avatarImage.color = colors.primaryBlue; // Placeholder color
            
            // Player name
            GameObject nameObj = new GameObject("Player Name");
            nameObj.transform.SetParent(headerPanel.transform, false);
            nameObj.layer = 5;
            
            playerNameText = nameObj.AddComponent<TextMeshProUGUI>();
            playerNameText.text = "Champion";
            playerNameText.fontSize = 20;
            playerNameText.color = colors.textPrimary;
            playerNameText.alignment = TextAlignmentOptions.Left;
            playerNameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.18f, 0.5f);
            nameRect.anchorMax = new Vector2(0.6f, 0.9f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Player stats
            GameObject statsObj = new GameObject("Player Stats");
            statsObj.transform.SetParent(headerPanel.transform, false);
            statsObj.layer = 5;
            
            playerStatsText = statsObj.AddComponent<TextMeshProUGUI>();
            playerStatsText.text = "Level 1 • Ready for Battle";
            playerStatsText.fontSize = 14;
            playerStatsText.color = colors.textSecondary;
            playerStatsText.alignment = TextAlignmentOptions.Left;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.18f, 0.1f);
            statsRect.anchorMax = new Vector2(0.6f, 0.5f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // Currency display (gold/gems)
            CreateCurrencyDisplay(headerPanel.transform);
        }
        
        private void CreateCurrencyDisplay(Transform parent)
        {
            // Gold display
            GameObject goldContainer = new GameObject("Gold Container");
            goldContainer.transform.SetParent(parent, false);
            goldContainer.layer = 5;
            
            RectTransform goldRect = goldContainer.AddComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0.65f, 0.1f);
            goldRect.anchorMax = new Vector2(0.85f, 0.9f);
            goldRect.offsetMin = Vector2.zero;
            goldRect.offsetMax = Vector2.zero;
            
            // Gold icon (CUSTOM or fallback) - PROPER SIZE
            GameObject goldIcon = CreatePanel("Gold Icon", goldContainer.transform);
            Image goldIconImg = goldIcon.GetComponent<Image>();
            
            if (assetLibrary.goldIcon != null)
            {
                goldIconImg.sprite = assetLibrary.goldIcon;
                goldIconImg.color = Color.white;
                goldIconImg.type = Image.Type.Simple; // Preserve aspect ratio
                goldIconImg.preserveAspect = true; // Keep original proportions
                Debug.Log("✅ Applied custom gold icon with proper proportions!");
            }
            else
            {
                goldIconImg.color = colors.legendaryGold;
                Debug.Log("📝 Using fallback color for gold icon");
            }
            
            RectTransform goldIconRect = goldIcon.GetComponent<RectTransform>();
            goldIconRect.anchorMin = new Vector2(0f, 0.1f);
            goldIconRect.anchorMax = new Vector2(0.35f, 0.9f); // Smaller, square-like
            goldIconRect.offsetMin = Vector2.zero;
            goldIconRect.offsetMax = Vector2.zero;
            
            // Gold text
            GameObject goldTextObj = new GameObject("Gold Text");
            goldTextObj.transform.SetParent(goldContainer.transform, false);
            goldTextObj.layer = 5;
            
            TextMeshProUGUI goldText = goldTextObj.AddComponent<TextMeshProUGUI>();
            goldText.text = "1250";
            goldText.fontSize = 16;
            goldText.color = colors.textPrimary;
            goldText.alignment = TextAlignmentOptions.Left;
            goldText.fontStyle = FontStyles.Bold;
            
            RectTransform goldTextRect = goldTextObj.GetComponent<RectTransform>();
            goldTextRect.anchorMin = new Vector2(0.4f, 0f);
            goldTextRect.anchorMax = new Vector2(1f, 1f);
            goldTextRect.offsetMin = Vector2.zero;
            goldTextRect.offsetMax = Vector2.zero;
            
            // Gems display (similar structure)
            CreateGemsDisplay(parent);
        }
        
        private void CreateGemsDisplay(Transform parent)
        {
            GameObject gemContainer = new GameObject("Gem Container");
            gemContainer.transform.SetParent(parent, false);
            gemContainer.layer = 5;
            
            RectTransform gemRect = gemContainer.AddComponent<RectTransform>();
            gemRect.anchorMin = new Vector2(0.87f, 0.1f);
            gemRect.anchorMax = new Vector2(0.98f, 0.9f);
            gemRect.offsetMin = Vector2.zero;
            gemRect.offsetMax = Vector2.zero;
            
            // Gem icon (CUSTOM or fallback) - PROPER SIZE
            GameObject gemIcon = CreatePanel("Gem Icon", gemContainer.transform);
            Image gemIconImg = gemIcon.GetComponent<Image>();
            
            if (assetLibrary.gemIcon != null)
            {
                gemIconImg.sprite = assetLibrary.gemIcon;
                gemIconImg.color = Color.white;
                gemIconImg.type = Image.Type.Simple; // Preserve aspect ratio
                gemIconImg.preserveAspect = true; // Keep original proportions
                Debug.Log("✅ Applied custom gem icon with proper proportions!");
            }
            else
            {
                gemIconImg.color = colors.rarePurple;
                Debug.Log("📝 Using fallback color for gem icon");
            }
            
            RectTransform gemIconRect = gemIcon.GetComponent<RectTransform>();
            gemIconRect.anchorMin = new Vector2(0f, 0.1f);
            gemIconRect.anchorMax = new Vector2(0.4f, 0.9f); // Smaller, square-like
            gemIconRect.offsetMin = Vector2.zero;
            gemIconRect.offsetMax = Vector2.zero;
            
            // Gem text
            GameObject gemTextObj = new GameObject("Gem Text");
            gemTextObj.transform.SetParent(gemContainer.transform, false);
            gemTextObj.layer = 5;
            
            TextMeshProUGUI gemText = gemTextObj.AddComponent<TextMeshProUGUI>();
            gemText.text = "50";
            gemText.fontSize = 14;
            gemText.color = colors.textPrimary;
            gemText.alignment = TextAlignmentOptions.Left;
            gemText.fontStyle = FontStyles.Bold;
            
            RectTransform gemTextRect = gemTextObj.GetComponent<RectTransform>();
            gemTextRect.anchorMin = new Vector2(0.45f, 0f);
            gemTextRect.anchorMax = new Vector2(1f, 1f);
            gemTextRect.offsetMin = Vector2.zero;
            gemTextRect.offsetMax = Vector2.zero;
        }
        
        private void CreateNavigationCardGrid(Transform parent)
        {
            // Navigation container
            GameObject navContainer = new GameObject("Navigation Card Grid");
            navContainer.transform.SetParent(parent, false);
            navContainer.layer = 5;
            
            RectTransform navRect = navContainer.AddComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0.08f, 0.45f);
            navRect.anchorMax = new Vector2(0.92f, 0.82f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;
            
            // Grid layout for professional card arrangement
            GridLayoutGroup gridLayout = navContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(250, 140); // Professional card size
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3; // 3 cards per row
            
            // Collection card
            collectionButton = CreateNavigationCard(
                navContainer.transform, 
                "COLLECTION", 
                "Browse Cards", 
                colors.primaryGreen,
                assetLibrary.collectionIcon,
                () => ShowFunctionalCollection()
            );
            
            // Deck Builder card
            deckBuilderButton = CreateNavigationCard(
                navContainer.transform, 
                "DECK BUILDER", 
                "Craft Strategy", 
                colors.rarePurple,
                assetLibrary.deckBuilderIcon,
                () => ShowFunctionalDeckBuilder()
            );
            
            // Hero Hall card
            heroHallButton = CreateNavigationCard(
                navContainer.transform, 
                "HERO HALL", 
                "Choose Champion", 
                colors.legendaryGold,
                assetLibrary.heroHallIcon,
                () => ShowFunctionalHeroHall()
            );
        }
        
        private Button CreateNavigationCard(Transform parent, string title, string subtitle, Color accentColor, Sprite iconSprite, System.Action onClick)
        {
            GameObject card = CreateFantasyPanel(
                $"Nav Card {title}", 
                parent, 
                Vector2.zero, 
                Vector2.one,
                assetLibrary.navigationCardSprite
            );
            
            // Add gradient overlay for accent color
            CreateGradientOverlay(card.transform, accentColor, 0.2f);
            
            // Icon (CUSTOM or fallback)
            GameObject iconObj = CreatePanel("Card Icon", card.transform);
            Image iconImage = iconObj.GetComponent<Image>();
            
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.color = Color.white;
                Debug.Log($"✅ Applied custom icon for {title}!");
            }
            else
            {
                iconImage.sprite = CreatePlaceholderIcon(accentColor);
                iconImage.color = Color.white;
                Debug.Log($"📝 Using fallback icon for {title}");
            }
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.3f, 0.6f);
            iconRect.anchorMax = new Vector2(0.7f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Card Title");
            titleObj.transform.SetParent(card.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 16;
            titleText.color = colors.textPrimary;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.4f);
            titleRect.anchorMax = new Vector2(0.95f, 0.6f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Subtitle
            GameObject subtitleObj = new GameObject("Card Subtitle");
            subtitleObj.transform.SetParent(card.transform, false);
            subtitleObj.layer = 5;
            
            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = subtitle;
            subtitleText.fontSize = 12;
            subtitleText.color = colors.textSecondary;
            subtitleText.alignment = TextAlignmentOptions.Center;
            
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.05f, 0.15f);
            subtitleRect.anchorMax = new Vector2(0.95f, 0.35f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
            
            // Button functionality with professional animations
            Button cardButton = card.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                StartCoroutine(AnimateCardPress(card, onClick));
            });
            
            SetupProfessionalButtonColors(cardButton);
            
            return cardButton;
        }
        
        private void CreateProductionBattleButton(Transform parent)
        {
            // Battle button container with special effects
            GameObject battleContainer = new GameObject("Battle Button Container");
            battleContainer.transform.SetParent(parent, false);
            battleContainer.layer = 5;
            
            RectTransform battleRect = battleContainer.AddComponent<RectTransform>();
            battleRect.anchorMin = new Vector2(0.6f, 0.05f);
            battleRect.anchorMax = new Vector2(0.95f, 0.28f);
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;
            
            // Glow effect background
            GameObject glowObj = CreatePanel("Battle Glow", battleContainer.transform);
            Image glowImage = glowObj.GetComponent<Image>();
            glowImage.color = new Color(1f, 0.4f, 0.1f, 0.4f); // Orange battle glow
            SetFullScreen(glowObj.GetComponent<RectTransform>());
            
            // Main battle button with CUSTOM ICON
            battleButton = CreateCustomBattleButton(
                battleContainer.transform,
                new Vector2(0.1f, 0.1f),
                new Vector2(0.9f, 0.9f)
            );
            
            battleButton.onClick.AddListener(() => {
                StartCoroutine(AnimateBattleButtonPress());
            });
            
            // Continuous glow animation
            StartCoroutine(AnimateBattleGlow(glowImage));
        }
        
        private Button CreateCustomBattleButton(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            // If we have a custom battle icon, use ONLY the icon as the button
            if (assetLibrary.battleIcon != null)
            {
                Debug.Log("🎨 Creating PURE ICON battle button with your custom icon!");
                
                GameObject btnObj = new GameObject("Pure Icon Battle Button");
                btnObj.transform.SetParent(parent, false);
                btnObj.layer = 5;
                
                // The button IS your custom icon (no background rectangle!)
                Button button = btnObj.AddComponent<Button>();
                Image buttonImage = btnObj.AddComponent<Image>();
                buttonImage.sprite = assetLibrary.battleIcon;
                buttonImage.color = Color.white;
                buttonImage.type = Image.Type.Simple; // Keep original proportions
                
                RectTransform btnRect = btnObj.GetComponent<RectTransform>();
                btnRect.anchorMin = anchorMin;
                btnRect.anchorMax = anchorMax;
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;
                
                // Professional button interaction (no background color changes)
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); // Slightly brighter on hover
                colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Slightly darker when pressed
                colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                colors.fadeDuration = 0.1f;
                button.colors = colors;
                
                Debug.Log("✅ Created pure icon battle button - no ugly rectangle!");
                return button;
            }
            else
            {
                // Fallback: Create old-style button if no custom icon
                Debug.Log("📝 No custom battle icon found - creating fallback button");
                
                GameObject btnObj = CreatePanel("Fallback Battle Button", parent);
                Button button = btnObj.AddComponent<Button>();
                Image buttonImage = btnObj.GetComponent<Image>();
                buttonImage.color = colors.battleRed;
                
                RectTransform btnRect = btnObj.GetComponent<RectTransform>();
                btnRect.anchorMin = anchorMin;
                btnRect.anchorMax = anchorMax;
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;
                
                // Fallback text
                GameObject textObj = new GameObject("Battle Text");
                textObj.transform.SetParent(btnObj.transform, false);
                textObj.layer = 5;
                
                TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = "BATTLE";
                buttonText.fontSize = 20;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.raycastTarget = false;
                
                SetFullScreen(textObj.GetComponent<RectTransform>());
                SetupProfessionalButtonColors(button);
                
                return button;
            }
        }
        
        private void CreateBottomUtilityBar(Transform parent)
        {
            // NO background panel - just place icons directly on screen
            Debug.Log("🔧 Creating compact utility buttons without background...");
            
            // Calculate button size (square, compact)
            float buttonSize = 0.06f; // 6% of screen width/height
            float spacing = 0.01f; // 1% spacing between buttons
            float startX = 0.05f; // Start 5% from left edge
            float bottomY = 0.02f; // 2% from bottom
            
            // Settings button (leftmost)
            CreateCompactUtilityButton(
                parent, 
                "Settings", 
                assetLibrary.settingsIcon, 
                colors.primaryBlue,
                new Vector2(startX, bottomY),
                new Vector2(startX + buttonSize, bottomY + buttonSize)
            );
            
            // Shop button (middle)
            CreateCompactUtilityButton(
                parent, 
                "Shop", 
                assetLibrary.shopIcon, 
                colors.legendaryGold,
                new Vector2(startX + buttonSize + spacing, bottomY),
                new Vector2(startX + (buttonSize * 2) + spacing, bottomY + buttonSize)
            );
            
            // Logout button (rightmost) - USES CUSTOM LOGOUT ICON!
            Button logoutBtn = CreateCompactUtilityButton(
                parent, 
                "Logout", 
                Resources.Load<Sprite>("UI/Icons/logout-icon"), // Direct load for logout icon
                colors.battleRed,
                new Vector2(startX + (buttonSize * 2) + (spacing * 2), bottomY),
                new Vector2(startX + (buttonSize * 3) + (spacing * 2), bottomY + buttonSize)
            );
            
            logoutBtn.onClick.AddListener(async () => {
                await HandleLogout();
            });
        }
        
        private Button CreateCompactUtilityButton(Transform parent, string name, Sprite customIcon, Color fallbackColor, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = new GameObject($"Compact {name} Button");
            btnObj.transform.SetParent(parent, false);
            btnObj.layer = 5;
            
            Button button = btnObj.AddComponent<Button>();
            Image buttonImage = btnObj.AddComponent<Image>();
            
            // Use custom icon if available (PURE ICON, NO BACKGROUND)
            if (customIcon != null)
            {
                buttonImage.sprite = customIcon;
                buttonImage.color = Color.white;
                buttonImage.type = Image.Type.Simple; // Preserve aspect ratio
                buttonImage.preserveAspect = true; // Keep original proportions
                Debug.Log($"✅ Applied compact custom {name.ToLower()} icon - pure icon, no background!");
            }
            else
            {
                // Fallback: minimal colored icon
                buttonImage.color = fallbackColor;
                Debug.Log($"📝 Using compact fallback for {name.ToLower()} button");
                
                // Minimal single letter for fallback
                GameObject textObj = new GameObject($"{name} Text");
                textObj.transform.SetParent(btnObj.transform, false);
                textObj.layer = 5;
                
                TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = name.Substring(0, 1).ToUpper(); // Just first letter (S, $, L)
                buttonText.fontSize = 20;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.raycastTarget = false;
                buttonText.outlineColor = Color.black;
                buttonText.outlineWidth = 0.2f;
                
                SetFullScreen(textObj.GetComponent<RectTransform>());
            }
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Professional compact button interactions
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); // Slight brightness on hover
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Slight dim on press
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            return button;
        }
        
        private Button CreateSmallUtilityButton(Transform parent, string name, Sprite customIcon, Color fallbackColor, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = new GameObject($"Small {name} Button");
            btnObj.transform.SetParent(parent, false);
            btnObj.layer = 5;
            
            Button button = btnObj.AddComponent<Button>();
            Image buttonImage = btnObj.AddComponent<Image>();
            
            // Use custom icon if available
            if (customIcon != null)
            {
                buttonImage.sprite = customIcon;
                buttonImage.color = Color.white;
                buttonImage.type = Image.Type.Simple; // Preserve aspect ratio
                buttonImage.preserveAspect = true; // Keep original proportions
                Debug.Log($"✅ Applied small custom {name.ToLower()} icon with proper proportions!");
            }
            else
            {
                // Fallback: small colored square with minimal text
                buttonImage.color = fallbackColor;
                Debug.Log($"📝 Using small fallback for {name.ToLower()} button");
                
                // Minimal text for fallback
                GameObject textObj = new GameObject($"{name} Text");
                textObj.transform.SetParent(btnObj.transform, false);
                textObj.layer = 5;
                
                TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = name.Substring(0, 1).ToUpper(); // Just first letter
                buttonText.fontSize = 16;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.raycastTarget = false;
                
                SetFullScreen(textObj.GetComponent<RectTransform>());
            }
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Professional small button interactions
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            return button;
        }
        
        private Button CreateCustomUtilityButton(Transform parent, string name, Sprite customIcon, Color fallbackColor, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = new GameObject($"Custom {name} Button");
            btnObj.transform.SetParent(parent, false);
            btnObj.layer = 5;
            
            Button button = btnObj.AddComponent<Button>();
            Image buttonImage = btnObj.AddComponent<Image>();
            
            // Use custom icon if available, otherwise fallback to colored button
            if (customIcon != null)
            {
                buttonImage.sprite = customIcon;
                buttonImage.color = Color.white;
                buttonImage.type = Image.Type.Simple;
                Debug.Log($"✅ Applied custom {name.ToLower()} icon!");
            }
            else
            {
                buttonImage.color = fallbackColor;
                Debug.Log($"📝 Using fallback color for {name.ToLower()} button");
                
                // Add text for fallback
                GameObject textObj = new GameObject($"{name} Text");
                textObj.transform.SetParent(btnObj.transform, false);
                textObj.layer = 5;
                
                TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = name.ToUpper();
                buttonText.fontSize = 12;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.raycastTarget = false;
                
                SetFullScreen(textObj.GetComponent<RectTransform>());
            }
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Professional button interactions
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            return button;
        }
        
        private Button CreateUtilityButton(Transform parent, string text, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            return CreateFantasyButton(
                $"{text} Button",
                parent,
                text.ToUpper(),
                color,
                assetLibrary.utilityButtonSprite,
                anchorMin,
                anchorMax
            );
        }
        
        #endregion
        
        #region Collection Screen (Professional Card Grid)
        
        private async Task<GameObject> CreateCollectionScreen()
        {
            Debug.Log("📚 Creating PRODUCTION collection screen with REAL CARDS...");
            
            GameObject collectionScreen = CreateFullscreenBackground("Collection Screen", assetLibrary.collectionBackground, colors.primaryGreen);
            
            // Professional header
            CreateScreenHeader(collectionScreen.transform, "CARD COLLECTION", "Browse your elemental cards", colors.primaryGreen);
            
            // Filter bar
            CreateCollectionFilters(collectionScreen.transform);
            
            // REAL Card grid container
            await CreateRealCollectionGrid(collectionScreen.transform);
            
            // Back button
            CreateProfessionalBackButton(collectionScreen.transform);
            
            await Task.Yield();
            return collectionScreen;
        }
        
        private async Task CreateRealCollectionGrid(Transform parent)
        {
            try
            {
                Debug.Log("🔄 Loading REAL cards for collection...");
                
                if (RealSupabaseClient.Instance == null)
                {
                    Debug.LogError("❌ RealSupabaseClient not found!");
                    return;
                }
                
                // Load real data
                var allCards = await RealSupabaseClient.Instance.LoadAllCards();
                var userCollection = await RealSupabaseClient.Instance.LoadUserCollection();
                
                Debug.Log($"✅ Loaded {allCards.Count} total cards, {userCollection.Count} in user collection");
                
                // Create scroll view for real cards
                GameObject scrollView = new GameObject("Real Collection Scroll View");
                scrollView.transform.SetParent(parent, false);
                scrollView.layer = 5;
                
                RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
                scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
                scrollRect.anchorMax = new Vector2(0.95f, 0.76f);
                scrollRect.offsetMin = Vector2.zero;
                scrollRect.offsetMax = Vector2.zero;
                
                ScrollRect scrollComponent = scrollView.AddComponent<ScrollRect>();
                Image scrollBg = scrollView.AddComponent<Image>();
                scrollBg.color = new Color(0f, 0f, 0f, 0.1f);
                
                // Viewport
                GameObject viewport = new GameObject("Viewport");
                viewport.transform.SetParent(scrollView.transform, false);
                viewport.layer = 5;
                viewport.AddComponent<RectMask2D>();
                
                RectTransform viewportRect = viewport.AddComponent<RectTransform>();
                SetFullScreen(viewportRect);
                scrollComponent.viewport = viewportRect;
                
                // Content container
                GameObject content = new GameObject("Content");
                content.transform.SetParent(viewport.transform, false);
                content.layer = 5;
                
                RectTransform contentRect = content.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                scrollComponent.content = contentRect;
                
                // Grid layout for real cards
                GridLayoutGroup cardGrid = content.AddComponent<GridLayoutGroup>();
                cardGrid.cellSize = new Vector2(160, 220);
                cardGrid.spacing = new Vector2(10, 10);
                cardGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                cardGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
                cardGrid.childAlignment = TextAnchor.UpperCenter;
                cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                cardGrid.constraintCount = 6;
                
                ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                // Create real card displays
                int cardsDisplayed = 0;
                foreach (var collectionItem in userCollection.Take(50)) // Performance limit
                {
                    if (collectionItem.quantity > 0)
                    {
                        GameObject cardObj = CreateRealCollectionCard(collectionItem, content.transform);
                        if (cardObj != null) cardsDisplayed++;
                    }
                }
                
                Debug.Log($"✅ Created {cardsDisplayed} real card displays in collection");
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error creating real collection grid: {e.Message}");
            }
        }
        
        private GameObject CreateRealCollectionCard(RealSupabaseClient.CollectionItem collectionItem, Transform parent)
        {
            var card = collectionItem.card;
            
            GameObject cardObj = new GameObject($"RealCard_{card.name}");
            cardObj.transform.SetParent(parent, false);
            cardObj.layer = 5;
            
            // Card background with REAL elemental background
            Image cardImage = cardObj.AddComponent<Image>();
            Sprite elementalBg = GetElementalBackground(card.potato_type, card.exotic || card.is_exotic);
            
            if (elementalBg != null)
            {
                cardImage.sprite = elementalBg;
                cardImage.type = Image.Type.Simple;
                Debug.Log($"✅ Applied elemental background for {card.name} ({card.potato_type})");
            }
            else
            {
                cardImage.color = GetElementalColor(card.potato_type);
            }
            
            // Card name
            CreateCardName(cardObj.transform, card.name);
            
            // Mana cost
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            
            // Attack/Health (for units)
            if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
            if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), new Color(0.3f, 1f, 0.3f, 1f), new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
            
            // Quantity badge
            if (collectionItem.quantity > 1)
            {
                CreateQuantityBadge(cardObj.transform, collectionItem.quantity);
            }
            
            // Rarity indicator
            CreateRarityIndicator(cardObj.transform, card.rarity);
            
            // Click handler
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                Debug.Log($"🃏 Clicked card: {card.name}");
                // TODO: Show card details
            });
            
            // Professional button colors
            ColorBlock colors = cardButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            cardButton.colors = colors;
            
            return cardObj;
        }
        
        private void CreateCollectionFilters(Transform parent)
        {
            GameObject filterPanel = CreateFantasyPanel(
                "Filter Panel",
                parent,
                new Vector2(0.05f, 0.78f),
                new Vector2(0.95f, 0.86f),
                assetLibrary.filterPanelSprite
            );
            
            // Search field
            CreateFantasyInputField(
                "Search Field",
                filterPanel.transform,
                "Search cards...",
                new Vector2(0.05f, 0.1f),
                new Vector2(0.4f, 0.9f)
            );
            
            // Filter buttons
            CreateFilterButton(filterPanel.transform, "ALL", new Vector2(0.45f, 0.1f), new Vector2(0.6f, 0.9f));
            CreateFilterButton(filterPanel.transform, "FIRE", new Vector2(0.62f, 0.1f), new Vector2(0.75f, 0.9f));
            CreateFilterButton(filterPanel.transform, "ICE", new Vector2(0.77f, 0.1f), new Vector2(0.9f, 0.9f));
        }
        
        private void CreateFilterButton(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            CreateFantasyButton(
                $"Filter {text}",
                parent,
                text,
                colors.buttonNormal,
                assetLibrary.filterButtonSprite,
                anchorMin,
                anchorMax
            );
        }
        
        private void CreateCollectionGrid(Transform parent)
        {
            // Scroll view for card collection
            GameObject scrollView = new GameObject("Collection Scroll View");
            scrollView.transform.SetParent(parent, false);
            scrollView.layer = 5;
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.76f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            ScrollRect scrollComponent = scrollView.AddComponent<ScrollRect>();
            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0f, 0f, 0f, 0.1f); // Subtle background
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<RectMask2D>();
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            SetFullScreen(viewportRect);
            scrollComponent.viewport = viewportRect;
            
            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            scrollComponent.content = contentRect;
            
            // Grid layout for cards
            GridLayoutGroup cardGrid = content.AddComponent<GridLayoutGroup>();
            cardGrid.cellSize = new Vector2(160, 220); // Professional card size
            cardGrid.spacing = new Vector2(10, 10);
            cardGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            cardGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            cardGrid.childAlignment = TextAnchor.UpperCenter;
            cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cardGrid.constraintCount = 6; // 6 cards per row for mobile
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // TODO: Populate with actual cards from database
            CreatePlaceholderCards(content.transform);
        }
        
        private void CreatePlaceholderCards(Transform parent)
        {
            // Create some placeholder cards to show the grid
            for (int i = 0; i < 12; i++)
            {
                CreatePlaceholderCard(parent, $"Card {i + 1}");
            }
        }
        
        private void CreatePlaceholderCard(Transform parent, string cardName)
        {
            GameObject card = CreateFantasyPanel(
                cardName,
                parent,
                Vector2.zero,
                Vector2.one,
                assetLibrary.cardFrameSprite
            );
            
            // Card background (elemental)
            Image cardBg = card.GetComponent<Image>();
            cardBg.color = colors.GetRandomElementalColor();
            
            // Card name
            GameObject nameObj = new GameObject("Card Name");
            nameObj.transform.SetParent(card.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = cardName;
            nameText.fontSize = 12;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            nameText.outlineColor = Color.black;
            nameText.outlineWidth = 0.2f;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.8f);
            nameRect.anchorMax = new Vector2(0.95f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Mana cost
            CreateCardStat(card.transform, "3", colors.primaryBlue, new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.25f));
            
            // Attack
            CreateCardStat(card.transform, "2", colors.battleRed, new Vector2(0.05f, 0.75f), new Vector2(0.25f, 0.95f));
            
            // Health
            CreateCardStat(card.transform, "4", colors.primaryGreen, new Vector2(0.75f, 0.75f), new Vector2(0.95f, 0.95f));
            
            // Add click animation
            Button cardButton = card.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                StartCoroutine(AnimateCardPress(card, null));
            });
            
            SetupProfessionalButtonColors(cardButton);
        }
        
        private void CreateCardStat(Transform parent, string value, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject statObj = CreatePanel("Card Stat", parent);
            Image statBg = statObj.GetComponent<Image>();
            statBg.color = color;
            
            RectTransform statRect = statObj.GetComponent<RectTransform>();
            statRect.anchorMin = anchorMin;
            statRect.anchorMax = anchorMax;
            statRect.offsetMin = Vector2.zero;
            statRect.offsetMax = Vector2.zero;
            
            // Stat text
            GameObject textObj = new GameObject("Stat Text");
            textObj.transform.SetParent(statObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI statText = textObj.AddComponent<TextMeshProUGUI>();
            statText.text = value;
            statText.fontSize = 14;
            statText.color = Color.white;
            statText.alignment = TextAlignmentOptions.Center;
            statText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
        }
        
        #endregion
        
        #region Deck Builder Screen
        
        private async Task<GameObject> CreateDeckBuilderScreen()
        {
            Debug.Log("🔧 Creating PRODUCTION deck builder with REAL FUNCTIONALITY...");
            
            GameObject deckScreen = CreateFullscreenBackground("Deck Builder Screen", assetLibrary.deckBuilderBackground, colors.rarePurple);
            
            CreateScreenHeader(deckScreen.transform, "DECK BUILDER", "Craft your perfect strategy", colors.rarePurple);
            
            // REAL Split view with functional deck building
            await CreateRealDeckBuilderSplitView(deckScreen.transform);
            
            CreateProfessionalBackButton(deckScreen.transform);
            
            await Task.Yield();
            return deckScreen;
        }
        
        private async Task CreateRealDeckBuilderSplitView(Transform parent)
        {
            try
            {
                Debug.Log("🔄 Loading REAL deck builder data...");
                
                if (RealSupabaseClient.Instance == null)
                {
                    Debug.LogError("❌ RealSupabaseClient not found!");
                    return;
                }
                
                // Load user collection and decks for deck building
                var userCollection = await RealSupabaseClient.Instance.LoadUserCollection();
                var userDecks = await RealSupabaseClient.Instance.LoadUserDecks();
                Debug.Log($"✅ Loaded {userCollection.Count} cards and {userDecks.Count} decks for deck building");
                
                // Deck management panel (top)
                GameObject deckManagementPanel = CreateFantasyPanel(
                    "Deck Management Panel",
                    parent,
                    new Vector2(0.02f, 0.02f),
                    new Vector2(0.98f, 0.12f),
                    assetLibrary.headerPanelSprite
                );
                
                CreateDeckManagementArea(deckManagementPanel.transform, userDecks);
                
                // Available cards panel (left)
                GameObject availablePanel = CreateFantasyPanel(
                    "Available Cards Panel",
                    parent,
                    new Vector2(0.02f, 0.15f),
                    new Vector2(0.48f, 0.98f),
                    assetLibrary.deckPanelSprite
                );
                
                CreateAvailableCardsArea(availablePanel.transform, userCollection);
                
                // Current deck panel (right)
                GameObject deckPanel = CreateFantasyPanel(
                    "Current Deck Panel",
                    parent,
                    new Vector2(0.52f, 0.15f),
                    new Vector2(0.98f, 0.98f),
                    assetLibrary.cardsPanelSprite
                );
                
                // Load active deck if available
                var activeDeck = userDecks.FirstOrDefault(d => d.is_active);
                if (activeDeck != null)
                {
                    var deckWithCards = await RealSupabaseClient.Instance.LoadDeckCards(activeDeck.id);
                    CreateCurrentDeckArea(deckPanel.transform, deckWithCards);
                }
                else
                {
                    CreateCurrentDeckArea(deckPanel.transform, null);
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error creating deck builder: {e.Message}");
            }
        }
        
        private void CreateAvailableCardsArea(Transform parent, List<RealSupabaseClient.CollectionItem> userCollection)
        {
            // Title
            GameObject titleObj = new GameObject("Available Cards Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "YOUR COLLECTION";
            titleText.fontSize = 18;
            titleText.color = new Color(0.8f, 1f, 0.8f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Create scroll view for available cards
            CreateAvailableCardsScrollView(parent, userCollection);
        }
        
        private void CreateAvailableCardsScrollView(Transform parent, List<RealSupabaseClient.CollectionItem> userCollection)
        {
            GameObject scrollView = CreatePanel("Available Cards Scroll", parent);
            scrollView.GetComponent<Image>().color = Color.clear;
            
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.05f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.88f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            ScrollRect scrollComponent = scrollView.AddComponent<ScrollRect>();
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            viewport.layer = 5;
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewport.AddComponent<RectMask2D>();
            SetFullScreen(viewportRect);
            scrollComponent.viewport = viewportRect;
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            scrollComponent.content = contentRect;
            
            // Grid layout
            GridLayoutGroup cardGrid = content.AddComponent<GridLayoutGroup>();
            cardGrid.cellSize = new Vector2(120, 160);
            cardGrid.spacing = new Vector2(5, 5);
            cardGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            cardGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            cardGrid.childAlignment = TextAnchor.UpperCenter;
            cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cardGrid.constraintCount = 3;
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Create available card displays - SHOW ALL CARDS!
            int cardsDisplayed = 0;
            try 
            {
                var ownedCards = userCollection.Where(item => item.quantity > 0); // Show ALL cards, not just 30
                Debug.Log($"🔍 About to create cards from {ownedCards.Count()} owned cards");
                
                foreach (var collectionItem in ownedCards)
                {
                    try 
                    {
                        GameObject cardObj = CreateDeckBuilderCard(collectionItem, content.transform);
                        if (cardObj != null) 
                        {
                            cardsDisplayed++;
                            Debug.Log($"✅ Created deck builder card: {collectionItem.card.name}");
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ CreateDeckBuilderCard returned null for {collectionItem.card.name}");
                        }
                    }
                    catch (System.Exception cardEx)
                    {
                        Debug.LogError($"❌ Error creating card {collectionItem.card.name}: {cardEx.Message}");
                        Debug.LogError($"❌ Stack trace: {cardEx.StackTrace}");
                    }
                }
                
                Debug.Log($"✅ Created {cardsDisplayed} available cards for deck building");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error in CreateAvailableCardsScrollView: {ex.Message}");
                Debug.LogError($"❌ Stack trace: {ex.StackTrace}");
            }
        }
        
        private GameObject CreateDeckBuilderCard(RealSupabaseClient.CollectionItem collectionItem, Transform parent)
        {
            var card = collectionItem.card;
            
            GameObject cardObj = new GameObject($"DeckCard_{card.name}");
            cardObj.transform.SetParent(parent, false);
            cardObj.layer = 5;
            
            // Card background with REAL elemental background
            Image cardImage = cardObj.AddComponent<Image>();
            Sprite elementalBg = GetElementalBackground(card.potato_type, card.exotic || card.is_exotic);
            
            if (elementalBg != null)
            {
                cardImage.sprite = elementalBg;
                cardImage.type = Image.Type.Simple;
            }
            else
            {
                cardImage.color = GetElementalColor(card.potato_type);
            }
            
            // Card name
            CreateCardName(cardObj.transform, card.name);
            
            // Mana cost
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            
            // Attack/Health (for units)
            if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
            if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), new Color(0.3f, 1f, 0.3f, 1f), new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
            
            // Quantity badge
            if (collectionItem.quantity > 1)
            {
                CreateQuantityBadge(cardObj.transform, collectionItem.quantity);
            }
            
            // Rarity indicator
            CreateRarityIndicator(cardObj.transform, card.rarity);
            
            // Click handler to add to deck
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                Debug.Log($"🃏 Adding {card.name} to deck...");
                // TODO: Add deck building logic
            });
            
            // Professional button colors
            ColorBlock colors = cardButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            cardButton.colors = colors;
            
            return cardObj;
        }
        
        private void CreateDeckManagementArea(Transform parent, List<RealSupabaseClient.Deck> userDecks)
        {
            // Title
            GameObject titleObj = new GameObject("Deck Management Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🔧 DECK MANAGEMENT";
            titleText.fontSize = 18;
            titleText.color = new Color(1f, 0.9f, 0.3f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.02f, 0.5f);
            titleRect.anchorMax = new Vector2(0.3f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Active deck indicator
            var activeDeck = userDecks.FirstOrDefault(d => d.is_active);
            if (activeDeck != null)
            {
                GameObject activeIndicator = new GameObject("Active Deck Indicator");
                activeIndicator.transform.SetParent(parent, false);
                activeIndicator.layer = 5;
                
                TextMeshProUGUI activeText = activeIndicator.AddComponent<TextMeshProUGUI>();
                activeText.text = $"👑 Active: {activeDeck.name}";
                activeText.fontSize = 14;
                activeText.color = new Color(1f, 0.8f, 0.2f, 1f);
                activeText.alignment = TextAlignmentOptions.Left;
                
                RectTransform activeRect = activeIndicator.GetComponent<RectTransform>();
                activeRect.anchorMin = new Vector2(0.32f, 0.5f);
                activeRect.anchorMax = new Vector2(0.65f, 0.95f);
                activeRect.offsetMin = Vector2.zero;
                activeRect.offsetMax = Vector2.zero;
            }
            
            // Create new deck button
            GameObject newDeckBtn = CreateFantasyButton(
                "Create New Deck",
                parent,
                new Vector2(0.67f, 0.1f),
                new Vector2(0.85f, 0.9f),
                "➕ New Deck",
                new Color(0.2f, 0.8f, 0.2f, 1f),
                () => {
                    Debug.Log("🃏 Create new deck clicked - TODO: Implement deck creation dialog");
                    // TODO: Implement deck creation dialog
                }
            );
            
            // Deck selector dropdown (placeholder for now)
            GameObject deckSelectorBtn = CreateFantasyButton(
                "Deck Selector",
                parent,
                new Vector2(0.87f, 0.1f),
                new Vector2(0.98f, 0.9f),
                "📋 Decks",
                new Color(0.3f, 0.6f, 1f, 1f),
                () => {
                    Debug.Log("🃏 Deck selector clicked - TODO: Implement deck selector");
                    // TODO: Implement deck selector dropdown
                }
            );
        }
        
        private void CreateCurrentDeckArea(Transform parent, RealSupabaseClient.Deck currentDeck)
        {
            // Title
            GameObject titleObj = new GameObject("Current Deck Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            string deckName = currentDeck?.name ?? "No Deck Selected";
            int cardCount = currentDeck?.total_cards ?? 0;
            titleText.text = $"{deckName} ({cardCount}/30)";
            titleText.fontSize = 18;
            titleText.color = new Color(1f, 0.8f, 1f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Deck area
            GameObject deckArea = CreatePanel("Deck Area", parent);
            deckArea.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform deckRect = deckArea.GetComponent<RectTransform>();
            deckRect.anchorMin = new Vector2(0.05f, 0.05f);
            deckRect.anchorMax = new Vector2(0.95f, 0.88f);
            deckRect.offsetMin = Vector2.zero;
            deckRect.offsetMax = Vector2.zero;
            
            // Grid layout for deck cards
            GridLayoutGroup deckGrid = deckArea.AddComponent<GridLayoutGroup>();
            deckGrid.cellSize = new Vector2(100, 140);
            deckGrid.spacing = new Vector2(5, 5);
            deckGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            deckGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            deckGrid.childAlignment = TextAnchor.UpperCenter;
            deckGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            deckGrid.constraintCount = 4;
            
            // Display current deck cards or placeholders
            if (currentDeck != null && currentDeck.cards != null && currentDeck.cards.Count > 0)
            {
                CreateDeckCards(deckArea.transform, currentDeck.cards);
            }
            else
            {
                CreateDeckPlaceholders(deckArea.transform);
            }
        }
        
        private void CreateDeckCards(Transform parent, List<RealSupabaseClient.DeckCard> deckCards)
        {
            Debug.Log($"🃏 Creating deck display with {deckCards.Count} unique cards");
            
            int totalCards = 0;
            foreach (var deckCard in deckCards)
            {
                for (int i = 0; i < deckCard.quantity; i++)
                {
                    GameObject cardObj = CreateDeckDisplayCard(deckCard.card, parent);
                    if (cardObj != null)
                    {
                        totalCards++;
                        
                        // Add quantity indicator for cards with multiple copies
                        if (deckCard.quantity > 1)
                        {
                            CreateQuantityBadge(cardObj.transform, deckCard.quantity);
                        }
                    }
                }
            }
            
            // Fill remaining slots with placeholders
            for (int i = totalCards; i < 30; i++)
            {
                CreateDeckSlotPlaceholder(parent, i + 1);
            }
            
            Debug.Log($"✅ Created deck display with {totalCards} cards and {30 - totalCards} placeholders");
        }
        
        private GameObject CreateDeckDisplayCard(RealSupabaseClient.EnhancedCard card, Transform parent)
        {
            GameObject cardObj = new GameObject($"DeckDisplayCard_{card.name}");
            cardObj.transform.SetParent(parent, false);
            cardObj.layer = 5;
            
            // Card background with elemental background
            Image cardImage = cardObj.AddComponent<Image>();
            Sprite elementalBg = GetElementalBackground(card.potato_type, card.exotic || card.is_exotic);
            
            if (elementalBg != null)
            {
                cardImage.sprite = elementalBg;
                cardImage.type = Image.Type.Simple;
            }
            else
            {
                cardImage.color = GetElementalColor(card.potato_type);
            }
            
            // Card name (smaller for deck view)
            CreateCardName(cardObj.transform, card.name);
            
            // Mana cost
            CreateCardStat(cardObj.transform, card.mana_cost.ToString(), new Color(0.2f, 0.6f, 1f, 1f), new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f));
            
            // Attack/Health (for units) - smaller
            if (card.attack.HasValue) CreateCardStat(cardObj.transform, card.attack.Value.ToString(), new Color(1f, 0.3f, 0.3f, 1f), new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.2f));
            if (card.hp.HasValue) CreateCardStat(cardObj.transform, card.hp.Value.ToString(), new Color(0.3f, 1f, 0.3f, 1f), new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.2f));
            
            // Click handler to remove from deck
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                Debug.Log($"🃏 Removing {card.name} from deck...");
                // TODO: Add deck removal logic
            });
            
            // Professional button colors
            ColorBlock colors = cardButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 0.9f, 0.9f, 1f); // Slightly red tint for removal
            colors.pressedColor = new Color(0.95f, 0.8f, 0.8f, 1f);
            cardButton.colors = colors;
            
            return cardObj;
        }
        
        private void CreateDeckSlotPlaceholder(Transform parent, int slotNumber)
        {
            GameObject slotObj = CreatePanel($"Deck Slot {slotNumber}", parent);
            Image slotImage = slotObj.GetComponent<Image>();
            slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            
            // Slot number
            GameObject numberObj = new GameObject("Slot Number");
            numberObj.transform.SetParent(slotObj.transform, false);
            numberObj.layer = 5;
            
            TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
            numberText.text = slotNumber.ToString();
            numberText.fontSize = 10;
            numberText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            numberText.alignment = TextAlignmentOptions.Center;
            
            RectTransform numberRect = numberObj.GetComponent<RectTransform>();
            SetFullScreen(numberRect);
        }
        
        private void CreateDeckPlaceholders(Transform parent)
        {
            // Create 30 placeholder deck slots
            for (int i = 0; i < 30; i++)
            {
                GameObject slotObj = CreatePanel($"Deck Slot {i + 1}", parent);
                Image slotImage = slotObj.GetComponent<Image>();
                slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                
                // Slot number
                GameObject numberObj = new GameObject("Slot Number");
                numberObj.transform.SetParent(slotObj.transform, false);
                numberObj.layer = 5;
                
                TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
                numberText.text = (i + 1).ToString();
                numberText.fontSize = 14;
                numberText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                numberText.alignment = TextAlignmentOptions.Center;
                
                SetFullScreen(numberObj.GetComponent<RectTransform>());
            }
        }
        
        private void CreateDeckBuilderSplitView(Transform parent)
        {
            // Left panel - Current deck
            GameObject deckPanel = CreateFantasyPanel(
                "Current Deck Panel",
                parent,
                new Vector2(0.02f, 0.15f),
                new Vector2(0.48f, 0.85f),
                assetLibrary.deckPanelSprite
            );
            
            CreateDeckPanelContent(deckPanel.transform);
            
            // Right panel - Available cards
            GameObject cardsPanel = CreateFantasyPanel(
                "Available Cards Panel",
                parent,
                new Vector2(0.52f, 0.15f),
                new Vector2(0.98f, 0.85f),
                assetLibrary.cardsPanelSprite
            );
            
            CreateCardsPanelContent(cardsPanel.transform);
        }
        
        private void CreateDeckPanelContent(Transform parent)
        {
            // Deck title
            GameObject titleObj = new GameObject("Deck Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "CURRENT DECK";
            titleText.fontSize = 18;
            titleText.color = colors.textPrimary;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Deck count
            GameObject countObj = new GameObject("Deck Count");
            countObj.transform.SetParent(parent, false);
            countObj.layer = 5;
            
            TextMeshProUGUI countText = countObj.AddComponent<TextMeshProUGUI>();
            countText.text = "0 / 30 Cards";
            countText.fontSize = 16;
            countText.color = colors.textSecondary;
            countText.alignment = TextAlignmentOptions.Center;
            
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.05f, 0.82f);
            countRect.anchorMax = new Vector2(0.95f, 0.9f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
            
            // Deck grid (TODO: populate with actual deck cards)
            CreateDeckGrid(parent);
        }
        
        private void CreateCardsPanelContent(Transform parent)
        {
            // Available cards title
            GameObject titleObj = new GameObject("Cards Title");
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "YOUR COLLECTION";
            titleText.fontSize = 18;
            titleText.color = colors.textPrimary;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Cards grid (TODO: populate with actual collection)
            CreateAvailableCardsGrid(parent);
        }
        
        #endregion
        
        #region Hero Hall Screen
        
        private async Task<GameObject> CreateHeroHallScreen()
        {
            Debug.Log("🦸 Creating PRODUCTION hero hall...");
            
            GameObject heroScreen = CreateFullscreenBackground("Hero Hall Screen", assetLibrary.heroHallBackground, colors.legendaryGold);
            
            CreateScreenHeader(heroScreen.transform, "HERO HALL", "Choose your champion", colors.legendaryGold);
            
            // Hero grid
            CreateHeroGrid(heroScreen.transform);
            
            CreateProfessionalBackButton(heroScreen.transform);
            
            await Task.Yield();
            return heroScreen;
        }
        
        private void CreateHeroGrid(Transform parent)
        {
            GameObject heroContainer = new GameObject("Hero Grid Container");
            heroContainer.transform.SetParent(parent, false);
            heroContainer.layer = 5;
            
            RectTransform heroRect = heroContainer.AddComponent<RectTransform>();
            heroRect.anchorMin = new Vector2(0.1f, 0.2f);
            heroRect.anchorMax = new Vector2(0.9f, 0.8f);
            heroRect.offsetMin = Vector2.zero;
            heroRect.offsetMax = Vector2.zero;
            
            // Grid layout for heroes
            GridLayoutGroup heroGrid = heroContainer.AddComponent<GridLayoutGroup>();
            heroGrid.cellSize = new Vector2(200, 280);
            heroGrid.spacing = new Vector2(20, 20);
            heroGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            heroGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            heroGrid.childAlignment = TextAnchor.MiddleCenter;
            heroGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            heroGrid.constraintCount = 4; // 4 heroes per row
            
            // TODO: Create actual hero cards from database
            CreatePlaceholderHeroCard(heroContainer.transform, "Fire Champion");
        }
        
        private void CreatePlaceholderHeroCard(Transform parent, string heroName)
        {
            GameObject heroCard = CreateFantasyPanel(
                heroName,
                parent,
                Vector2.zero,
                Vector2.one,
                assetLibrary.heroCardSprite
            );
            
            // Hero background
            Image heroBg = heroCard.GetComponent<Image>();
            heroBg.color = colors.fireRed;
            
            // Hero name
            GameObject nameObj = new GameObject("Hero Name");
            nameObj.transform.SetParent(heroCard.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = heroName;
            nameText.fontSize = 16;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            nameText.outlineColor = Color.black;
            nameText.outlineWidth = 0.3f;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.95f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
        }
        
        #endregion
        
        #region Professional UI Components
        
        private GameObject CreateFullscreenBackground(string name, Sprite backgroundSprite, Color fallbackColor)
        {
            GameObject background = CreatePanel(name, gameCanvas.transform);
            Image bgImage = background.GetComponent<Image>();
            
            if (backgroundSprite != null)
            {
                bgImage.sprite = backgroundSprite;
                bgImage.type = Image.Type.Simple; // Keep original proportions for backgrounds
                bgImage.color = Color.white; // Ensure full visibility
                Debug.Log($"✅ Applied custom background: {backgroundSprite.name}");
            }
            else
            {
                bgImage.color = fallbackColor;
                Debug.Log($"📝 Using fallback color for {name}");
            }
            
            SetFullScreen(background.GetComponent<RectTransform>());
            return background;
        }
        
        private GameObject CreateFantasyPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Sprite panelSprite)
        {
            GameObject panel = CreatePanel(name, parent);
            Image panelImage = panel.GetComponent<Image>();
            
            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Sliced; // 9-slice scaling
            }
            else
            {
                // Professional fallback with gradient effect
                panelImage.color = colors.cardBackground;
            }
            
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = anchorMin;
            panelRect.anchorMax = anchorMax;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Add shadow effect
            CreatePanelShadow(panel.transform);
            
            return panel;
        }
        
        private void CreatePanelShadow(Transform panelTransform)
        {
            GameObject shadow = CreatePanel("Panel Shadow", panelTransform.parent);
            Image shadowImage = shadow.GetComponent<Image>();
            shadowImage.color = new Color(0f, 0f, 0f, 0.4f);
            
            RectTransform shadowRect = shadow.GetComponent<RectTransform>();
            RectTransform panelRect = panelTransform.GetComponent<RectTransform>();
            
            shadowRect.anchorMin = panelRect.anchorMin;
            shadowRect.anchorMax = panelRect.anchorMax;
            shadowRect.offsetMin = new Vector2(6, -6); // Shadow offset
            shadowRect.offsetMax = new Vector2(6, -6);
            
            shadow.transform.SetSiblingIndex(panelTransform.GetSiblingIndex()); // Behind panel
        }
        
        private Button CreateFantasyButton(string name, Transform parent, string text, Color baseColor, Sprite buttonSprite, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = CreatePanel(name, parent);
            Button button = btnObj.AddComponent<Button>();
            Image buttonImage = btnObj.GetComponent<Image>();
            
            if (buttonSprite != null)
            {
                buttonImage.sprite = buttonSprite;
                buttonImage.type = Image.Type.Sliced;
            }
            else
            {
                buttonImage.color = baseColor;
            }
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Professional button colors with hover effects
            SetupProfessionalButtonColors(button);
            
            // Button text with fantasy styling
            GameObject textObj = new GameObject("Button Text");
            textObj.transform.SetParent(btnObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.raycastTarget = false;
            
            // Add text outline for readability
            buttonText.outlineColor = Color.black;
            buttonText.outlineWidth = 0.25f;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
            
            return button;
        }
        
        private TMP_InputField CreateFantasyInputField(string name, Transform parent, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject fieldObj = CreatePanel(name, parent);
            Image fieldImage = fieldObj.GetComponent<Image>();
            fieldImage.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);
            
            RectTransform fieldRect = fieldObj.GetComponent<RectTransform>();
            fieldRect.anchorMin = anchorMin;
            fieldRect.anchorMax = anchorMax;
            fieldRect.offsetMin = Vector2.zero;
            fieldRect.offsetMax = Vector2.zero;
            
            TMP_InputField inputField = fieldObj.AddComponent<TMP_InputField>();
            
            // Input text
            GameObject inputTextObj = new GameObject("Input Text");
            inputTextObj.transform.SetParent(fieldObj.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 18;
            inputText.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            inputText.alignment = TextAlignmentOptions.Left;
            
            RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
            inputTextRect.anchorMin = new Vector2(0.08f, 0f);
            inputTextRect.anchorMax = new Vector2(0.92f, 1f);
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;
            
            // Placeholder with professional styling
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(fieldObj.transform, false);
            placeholderObj.layer = 5;
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 18;
            placeholderText.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            placeholderText.alignment = TextAlignmentOptions.Left;
            placeholderText.raycastTarget = false;
            
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0.08f, 0f);
            placeholderRect.anchorMax = new Vector2(0.92f, 1f);
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            return inputField;
        }
        
        private TextMeshProUGUI CreateFantasyStatusText(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject statusObj = new GameObject("Status Text");
            statusObj.transform.SetParent(parent, false);
            statusObj.layer = 5;
            
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = text;
            statusText.fontSize = 16;
            statusText.color = colors.textSecondary;
            statusText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = anchorMin;
            statusRect.anchorMax = anchorMax;
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            
            return statusText;
        }
        
        private void CreateScreenHeader(Transform parent, string title, string subtitle, Color accentColor)
        {
            GameObject headerPanel = CreateFantasyPanel(
                "Screen Header",
                parent,
                new Vector2(0.05f, 0.88f),
                new Vector2(0.95f, 0.98f),
                assetLibrary.headerPanelSprite
            );
            
            CreateGradientOverlay(headerPanel.transform, accentColor, 0.3f);
            
            // Title
            GameObject titleObj = new GameObject("Screen Title");
            titleObj.transform.SetParent(headerPanel.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            titleText.outlineColor = Color.black;
            titleText.outlineWidth = 0.3f;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.4f);
            titleRect.anchorMax = new Vector2(0.9f, 0.8f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Subtitle
            GameObject subtitleObj = new GameObject("Screen Subtitle");
            subtitleObj.transform.SetParent(headerPanel.transform, false);
            subtitleObj.layer = 5;
            
            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = subtitle;
            subtitleText.fontSize = 14;
            subtitleText.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.1f, 0.1f);
            subtitleRect.anchorMax = new Vector2(0.9f, 0.4f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
        }
        
        private void CreateProfessionalBackButton(Transform parent)
        {
            Button backBtn = CreateFantasyButton(
                "Professional Back Button",
                parent,
                "← BACK",
                colors.primaryBlue,
                assetLibrary.backButtonSprite,
                new Vector2(0.05f, 0.05f),
                new Vector2(0.25f, 0.12f)
            );
            
            backBtn.onClick.AddListener(() => {
                StartCoroutine(AnimateCardPress(backBtn.gameObject, () => ShowScreen(GameScreen.MainMenu)));
            });
        }
        
        #endregion
        
        #region Professional Animations & Effects
        
        private IEnumerator AnimateScreenIn()
        {
            if (currentScreenGroup == null) yield break;
            
            // Fade and scale in animation
            currentScreenGroup.alpha = 0f;
            currentScreenObject.transform.localScale = Vector3.one * 0.9f;
            
            float time = 0f;
            float duration = 0.3f;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = time / duration;
                
                // Smooth ease-out
                float easedProgress = 1f - (1f - progress) * (1f - progress);
                
                currentScreenGroup.alpha = easedProgress;
                currentScreenObject.transform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, easedProgress);
                
                yield return null;
            }
            
            currentScreenGroup.alpha = 1f;
            currentScreenObject.transform.localScale = Vector3.one;
        }
        
        private IEnumerator AnimateScreenOut()
        {
            if (currentScreenGroup == null) yield break;
            
            float time = 0f;
            float duration = 0.2f;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = time / duration;
                
                currentScreenGroup.alpha = 1f - progress;
                currentScreenObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, progress);
                
                yield return null;
            }
        }
        
        private IEnumerator AnimateCardPress(GameObject card, System.Action onComplete = null)
        {
            Transform cardTransform = card.transform;
            Vector3 originalScale = cardTransform.localScale;
            Vector3 pressedScale = originalScale * 0.95f;
            
            // Press animation
            float time = 0f;
            float duration = 0.1f;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = time / duration;
                cardTransform.localScale = Vector3.Lerp(originalScale, pressedScale, progress);
                yield return null;
            }
            
            // Release animation
            time = 0f;
            while (time < duration * 2f)
            {
                time += Time.deltaTime;
                float progress = time / (duration * 2f);
                cardTransform.localScale = Vector3.Lerp(pressedScale, originalScale, progress);
                yield return null;
            }
            
            cardTransform.localScale = originalScale;
            onComplete?.Invoke();
        }
        
        private IEnumerator AnimateBattleGlow(Image glowImage)
        {
            Color originalColor = glowImage.color;
            
            while (glowImage != null)
            {
                float pulse = (Mathf.Sin(Time.time * 1.5f) + 1f) / 2f; // Slow pulse
                Color glowColor = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * pulse);
                glowImage.color = glowColor;
                
                yield return null;
            }
        }
        
        private IEnumerator AnimateLogoGlow(TextMeshProUGUI logoText)
        {
            Color originalColor = logoText.color;
            
            while (logoText != null)
            {
                float glow = (Mathf.Sin(Time.time * 0.8f) + 1f) / 2f; // Subtle glow
                Color glowColor = Color.Lerp(originalColor, colors.lightGold, glow * 0.3f);
                logoText.color = glowColor;
                
                yield return null;
            }
        }
        
        private IEnumerator AnimateBattleButtonPress()
        {
            Debug.Log("⚔️ BATTLE button pressed with professional animation!");
            
            // Enhanced battle button animation
            yield return StartCoroutine(AnimateCardPress(battleButton.gameObject));
            
            // Transition to matchmaking
            _ = ShowScreen(GameScreen.BattleMatchmaking);
        }
        
        #endregion
        
        #region Functional Screen Navigation
        
        private void ShowFunctionalCollection()
        {
            Debug.Log("📚 Opening REAL collection with beautiful UI...");
            _ = ShowScreen(GameScreen.Collection);
        }
        
        private void ShowFunctionalDeckBuilder()
        {
            Debug.Log("🔧 Opening REAL deck builder with beautiful UI...");
            _ = ShowScreen(GameScreen.DeckBuilder);
        }
        
        private void ShowFunctionalHeroHall()
        {
            Debug.Log("🦸 Opening REAL hero hall with beautiful UI...");
            _ = ShowScreen(GameScreen.HeroHall);
        }
        
        #endregion
        
        #region Event Handlers
        
        private async Task HandleLogin()
        {
            string email = emailField.text.Trim();
            string password = passwordField.text;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                UpdateStatus("Please enter your credentials", colors.battleRed);
                StartCoroutine(ShakeElement(loginButton.transform));
                return;
            }
            
            UpdateStatus("Entering the realm...", colors.primaryBlue);
            StartCoroutine(AnimateLoadingButton(loginButton));
            
            try
            {
                bool success = await supabaseClient.SignIn(email, password);
                
                if (success)
                {
                    UpdateStatus("Welcome to the arena!", colors.primaryGreen);
                    await Task.Delay(1000);
                    await ShowScreen(GameScreen.MainMenu);
                }
                else
                {
                    UpdateStatus("Invalid credentials", colors.battleRed);
                    StartCoroutine(ShakeElement(loginButton.transform));
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus("Connection failed", colors.battleRed);
                StartCoroutine(ShakeElement(loginButton.transform));
                Debug.LogError($"❌ Login error: {e.Message}");
            }
        }
        
        private async Task HandleLogout()
        {
            await supabaseClient.SignOut();
            await ShowScreen(GameScreen.Auth);
        }
        
        private void ToggleAuthMode()
        {
            // TODO: Switch between login and register modes
            Debug.Log("🔄 Toggling auth mode");
        }
        
        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            Debug.Log($"🔐 Authentication changed: {isAuthenticated}");
            
            if (isAuthenticated && currentScreen == GameScreen.Auth)
            {
                _ = ShowScreen(GameScreen.MainMenu);
            }
            else if (!isAuthenticated && currentScreen != GameScreen.Auth)
            {
                _ = ShowScreen(GameScreen.Auth);
            }
        }
        
        private void OnSupabaseError(string error)
        {
            Debug.LogError($"❌ Supabase error: {error}");
            UpdateStatus($"Error: {error}", colors.battleRed);
        }
        
        #endregion
        
        #region Helper Methods & Utilities
        
        private void UpdateStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
                StartCoroutine(AnimateStatusText());
            }
            
            Debug.Log($"📱 Status: {message}");
        }
        
        private IEnumerator AnimateStatusText()
        {
            if (statusText == null) yield break;
            
            Transform textTransform = statusText.transform;
            Vector3 originalScale = textTransform.localScale;
            
            textTransform.localScale = Vector3.zero;
            
            float time = 0f;
            float duration = 0.3f;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = time / duration;
                textTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
                yield return null;
            }
            
            textTransform.localScale = originalScale;
        }
        
        private IEnumerator ShakeElement(Transform element)
        {
            Vector3 originalPosition = element.localPosition;
            float shakeDuration = 0.4f;
            float shakeIntensity = 8f;
            
            float time = 0f;
            
            while (time < shakeDuration)
            {
                time += Time.deltaTime;
                
                float x = Random.Range(-shakeIntensity, shakeIntensity);
                element.localPosition = originalPosition + new Vector3(x, 0, 0);
                
                yield return null;
            }
            
            element.localPosition = originalPosition;
        }
        
        private IEnumerator AnimateLoadingButton(Button button)
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            string originalText = buttonText.text;
            
            string[] loadingStates = { "CONNECTING", "CONNECTING.", "CONNECTING..", "CONNECTING..." };
            int currentState = 0;
            
            for (int i = 0; i < 8; i++)
            {
                buttonText.text = loadingStates[currentState];
                currentState = (currentState + 1) % loadingStates.Length;
                yield return new WaitForSeconds(0.3f);
            }
            
            buttonText.text = originalText;
        }
        
        private void SetupProfessionalButtonColors(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.05f, 1.05f, 1.05f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }
        
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.layer = 5;
            
            panel.AddComponent<RectTransform>();
            panel.AddComponent<CanvasRenderer>();
            panel.AddComponent<Image>();
            
            return panel;
        }
        
        private void SetFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        private void ClearCurrentScreen()
        {
            if (currentScreenObject != null)
            {
                Destroy(currentScreenObject);
                currentScreenObject = null;
                currentScreenGroup = null;
            }
        }
        
        #endregion
        
        #region Real Card Helper Methods
        
        private void CreateCardName(Transform parent, string name)
        {
            GameObject nameObj = new GameObject("Card Name");
            nameObj.transform.SetParent(parent, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = name;
            nameText.fontSize = 11;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            nameText.outlineColor = Color.black;
            nameText.outlineWidth = 0.2f;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.95f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
        }
        
        
        private void CreateQuantityBadge(Transform parent, int quantity)
        {
            GameObject quantityObj = CreatePanel("Quantity Badge", parent);
            Image quantityBg = quantityObj.GetComponent<Image>();
            quantityBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform quantityRect = quantityObj.GetComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(0.75f, 0.75f);
            quantityRect.anchorMax = new Vector2(0.95f, 0.9f);
            quantityRect.offsetMin = Vector2.zero;
            quantityRect.offsetMax = Vector2.zero;
            
            GameObject textObj = new GameObject("Quantity Text");
            textObj.transform.SetParent(quantityObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI quantityText = textObj.AddComponent<TextMeshProUGUI>();
            quantityText.text = quantity.ToString();
            quantityText.fontSize = 10;
            quantityText.color = Color.white;
            quantityText.alignment = TextAlignmentOptions.Center;
            quantityText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
        }
        
        private void CreateRarityIndicator(Transform parent, string rarity)
        {
            GameObject rarityObj = CreatePanel("Rarity Indicator", parent);
            Image rarityBg = rarityObj.GetComponent<Image>();
            
            Color rarityColor = rarity?.ToLower() switch
            {
                "common" => Color.white,
                "uncommon" => Color.green,
                "rare" => Color.blue,
                "legendary" => Color.yellow,
                "exotic" => new Color(1f, 0.2f, 1f, 1f),
                _ => Color.white
            };
            
            rarityBg.color = rarityColor;
            
            RectTransform rarityRect = rarityObj.GetComponent<RectTransform>();
            rarityRect.anchorMin = new Vector2(0.75f, 0.8f);
            rarityRect.anchorMax = new Vector2(0.95f, 0.95f);
            rarityRect.offsetMin = Vector2.zero;
            rarityRect.offsetMax = Vector2.zero;
        }
        
        private Sprite GetElementalBackground(string potatoType, bool isExotic)
        {
            if (isExotic)
            {
                return Resources.Load<Sprite>("ElementalBackgrounds/exotic-class-card");
            }
            
            string backgroundName = potatoType?.ToLower() switch
            {
                "fire" => "fire-card",
                "ice" => "ice-card",
                "light" => "light-card",
                "lightning" => "lightning-card",
                "void" => "void-card",
                _ => "void-card"
            };
            
            return Resources.Load<Sprite>($"ElementalBackgrounds/{backgroundName}");
        }
        
        private Color GetElementalColor(string potatoType)
        {
            return potatoType?.ToLower() switch
            {
                "fire" => new Color(0.9f, 0.3f, 0.2f, 1f),
                "ice" => new Color(0.2f, 0.6f, 0.9f, 1f),
                "lightning" => new Color(1f, 0.9f, 0.2f, 1f),
                "light" => new Color(1f, 0.95f, 0.7f, 1f),
                "void" => new Color(0.5f, 0.2f, 0.7f, 1f),
                _ => new Color(0.5f, 0.5f, 0.5f, 1f)
            };
        }
        
        private void CreateAtmosphericEffects(Transform parent)
        {
            // TODO: Add subtle particle effects for atmosphere
            Debug.Log("✨ Atmospheric effects placeholder");
        }
        
        private UIAssetLibrary CreateAssetLibrary()
        {
            // Create ScriptableObject asset library with automatic asset loading
            UIAssetLibrary library = ScriptableObject.CreateInstance<UIAssetLibrary>();
            
            // Auto-load custom assets from UI folders
            LoadCustomAssets(library);
            
            return library;
        }
        
        private void LoadCustomAssets(UIAssetLibrary library)
        {
            Debug.Log("🎨 Auto-loading custom assets...");
            
            // Debug: Check what's actually in Resources
            Debug.Log("🔍 Checking Resources folders...");
            
            // Auto-load icons (correct Resources path)
            library.goldIcon = Resources.Load<Sprite>("UI/Icons/gold-icon");
            library.gemIcon = Resources.Load<Sprite>("UI/Icons/gems-icon");
            library.battleIcon = Resources.Load<Sprite>("UI/Icons/battle-icon");
            library.settingsIcon = Resources.Load<Sprite>("UI/Icons/settings-icon");
            library.shopIcon = Resources.Load<Sprite>("UI/Icons/shop-icon");
            
            // Debug each icon load
            Debug.Log($"🔍 Gold icon: {(library.goldIcon != null ? "FOUND" : "NOT FOUND")}");
            Debug.Log($"🔍 Battle icon: {(library.battleIcon != null ? "FOUND" : "NOT FOUND")}");
            
            // Auto-load backgrounds
            library.mainMenuBackground = Resources.Load<Sprite>("UI/Backgrounds/main-menu-bg");
            library.authBackground = Resources.Load<Sprite>("UI/Backgrounds/auth-bg");
            library.collectionBackground = Resources.Load<Sprite>("UI/Backgrounds/collection-bg");
            library.deckBuilderBackground = Resources.Load<Sprite>("UI/Backgrounds/deck-builder-bg");
            library.heroHallBackground = Resources.Load<Sprite>("UI/Backgrounds/hero-hall-bg");
            
            // Debug background loads
            Debug.Log($"🔍 Main menu background: {(library.mainMenuBackground != null ? "FOUND" : "NOT FOUND")}");
            
            // Auto-load buttons
            library.primaryButtonSprite = Resources.Load<Sprite>("UI/Buttons/primary-button");
            library.battleButtonSprite = Resources.Load<Sprite>("UI/Buttons/battle-button");
            library.secondaryButtonSprite = Resources.Load<Sprite>("UI/Buttons/secondary-button");
            
            // Log what was found
            int assetsFound = 0;
            if (library.goldIcon != null) { assetsFound++; Debug.Log("✅ Found custom gold icon"); }
            if (library.gemIcon != null) { assetsFound++; Debug.Log("✅ Found custom gems icon"); }
            if (library.battleIcon != null) { assetsFound++; Debug.Log("✅ Found custom battle icon"); }
            if (library.mainMenuBackground != null) { assetsFound++; Debug.Log("✅ Found custom main menu background"); }
            if (library.settingsIcon != null) { assetsFound++; Debug.Log("✅ Found custom settings icon"); }
            if (library.shopIcon != null) { assetsFound++; Debug.Log("✅ Found custom shop icon"); }
            
            Debug.Log($"🎨 Auto-loaded {assetsFound} custom assets");
            
            if (assetsFound == 0)
            {
                Debug.Log("📝 No custom assets found - using beautiful defaults");
                Debug.Log("💡 Make sure assets are in Assets/Resources/UI/Icons/ and Assets/Resources/UI/Backgrounds/");
                Debug.Log("💡 Make sure Texture Type is set to 'Sprite (2D and UI)'");
                
                // Debug: List what's actually in Resources
                var allResources = Resources.LoadAll<Sprite>("UI");
                Debug.Log($"🔍 Found {allResources.Length} total sprites in Resources/UI/");
                foreach (var resource in allResources)
                {
                    Debug.Log($"🔍 Available resource: {resource.name}");
                }
            }
        }
        
        private Sprite CreatePlaceholderIcon(Color color)
        {
            // Create simple colored square as placeholder
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
        }
        
        private void CreateGradientOverlay(Transform parent, Color color, float alpha)
        {
            GameObject overlay = CreatePanel("Gradient Overlay", parent);
            Image overlayImage = overlay.GetComponent<Image>();
            overlayImage.color = new Color(color.r, color.g, color.b, alpha);
            SetFullScreen(overlay.GetComponent<RectTransform>());
        }
        
        // TODO: Implement these methods
        private void CreateDeckGrid(Transform parent) { }
        private void CreateAvailableCardsGrid(Transform parent) { }
        private async Task<GameObject> CreateBattleMatchmakingScreen() { return null; }
        
        #endregion
        
        private void OnDestroy()
        {
            if (supabaseClient != null)
            {
                supabaseClient.OnAuthenticationChanged -= OnAuthenticationChanged;
                supabaseClient.OnError -= OnSupabaseError;
            }
        }
    }
    
    #region Supporting Classes
    
    /// <summary>
    /// Elemental Color Palette for Fantasy + Elemental Potato World
    /// </summary>
    [System.Serializable]
    public class ElementalColorPalette
    {
        [Header("🔥 Elemental Colors")]
        public Color fireRed = new Color(0.9f, 0.2f, 0.1f, 1f);
        public Color iceBlue = new Color(0.1f, 0.6f, 0.9f, 1f);
        public Color lightningYellow = new Color(1f, 0.9f, 0.1f, 1f);
        public Color lightGold = new Color(1f, 0.95f, 0.7f, 1f);
        public Color voidPurple = new Color(0.4f, 0.1f, 0.6f, 1f);
        
        [Header("🎨 UI Colors")]
        public Color primaryBlue = new Color(0.2f, 0.4f, 0.8f, 1f);
        public Color primaryGreen = new Color(0.2f, 0.7f, 0.3f, 1f);
        public Color battleRed = new Color(0.9f, 0.2f, 0.2f, 1f);
        public Color legendaryGold = new Color(1f, 0.8f, 0.2f, 1f);
        public Color rarePurple = new Color(0.6f, 0.3f, 0.9f, 1f);
        
        [Header("🖼️ Background Colors")]
        public Color darkBackground = new Color(0.08f, 0.12f, 0.18f, 1f);
        public Color cardBackground = new Color(0.15f, 0.2f, 0.28f, 0.95f);
        public Color buttonNormal = new Color(0.25f, 0.35f, 0.5f, 1f);
        
        [Header("📝 Text Colors")]
        public Color textPrimary = new Color(0.95f, 0.95f, 0.95f, 1f);
        public Color textSecondary = new Color(0.7f, 0.75f, 0.8f, 1f);
        
        public Color GetRandomElementalColor()
        {
            Color[] elementalColors = { fireRed, iceBlue, lightningYellow, lightGold, voidPurple };
            return elementalColors[Random.Range(0, elementalColors.Length)];
        }
    }
    
    /// <summary>
    /// Safe Area Handler for mobile devices (notches, etc.)
    /// </summary>
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            ApplySafeArea();
        }
        
        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            
            Debug.Log($"📱 Safe area applied: {safeArea}");
        }
    }
    
    #endregion
}