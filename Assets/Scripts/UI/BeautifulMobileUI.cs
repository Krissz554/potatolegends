using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Beautiful, professional mobile UI for the potato card game
    /// Modern design with gradients, animations, and polished visuals
    /// </summary>
    public class BeautifulMobileUI : MonoBehaviour
    {
        [Header("UI Colors")]
        [SerializeField] private Color primaryColor = new Color(0.2f, 0.1f, 0.4f, 1f); // Deep purple
        [SerializeField] private Color secondaryColor = new Color(0.8f, 0.4f, 0.1f, 1f); // Orange
        [SerializeField] private Color accentColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold
        [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.3f, 1f); // Green
        [SerializeField] private Color dangerColor = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
        
        private Canvas mainCanvas;
        private GameObject loginScreen;
        private GameObject mainGameUI;
        private bool isTransitioning = false;
        
        private void Start()
        {
            CreateProfessionalUI();
        }
        
        private void CreateProfessionalUI()
        {
            Debug.Log("🎨 Creating beautiful mobile UI...");
            
            CreateMainCanvas();
            CreateLoginScreen();
            
            Debug.Log("✅ Beautiful mobile UI created!");
        }
        
        private void CreateMainCanvas()
        {
            // Create main canvas
            GameObject canvasObj = new GameObject("Game Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;
            
            // Canvas scaler for perfect mobile scaling
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Mobile portrait
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem with proper input handling
            CreateEventSystem();
        }
        
        private void CreateEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                
                // Auto-detect and use appropriate input module
                try
                {
                    var inputSystemType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                    if (inputSystemType != null)
                    {
                        eventSystemObj.AddComponent(inputSystemType);
                        Debug.Log("✅ Using New Input System");
                    }
                    else
                    {
                        eventSystemObj.AddComponent<StandaloneInputModule>();
                        Debug.Log("✅ Using Legacy Input System");
                    }
                }
                catch
                {
                    eventSystemObj.AddComponent<StandaloneInputModule>();
                    Debug.Log("✅ Using Legacy Input System (fallback)");
                }
            }
        }
        
        private void CreateLoginScreen()
        {
            loginScreen = CreateUIPanel("Login Screen", mainCanvas.transform);
            
            // Beautiful gradient background
            CreateGradientBackground(loginScreen);
            
            // Animated title with glow effect
            CreateAnimatedTitle(loginScreen.transform);
            
            // Stylized login form
            CreateLoginForm(loginScreen.transform);
            
            // Floating particles background effect
            CreateFloatingParticles(loginScreen.transform);
        }
        
        private void CreateGradientBackground(GameObject parent)
        {
            // Create gradient background using multiple images
            GameObject bgTop = CreateUIPanel("Background Top", parent.transform);
            Image topImage = bgTop.GetComponent<Image>();
            topImage.color = new Color(0.15f, 0.05f, 0.25f, 1f); // Dark purple top
            SetFullScreen(bgTop.GetComponent<RectTransform>());
            
            GameObject bgGradient = CreateUIPanel("Background Gradient", parent.transform);
            Image gradientImage = bgGradient.GetComponent<Image>();
            gradientImage.color = new Color(0.25f, 0.15f, 0.35f, 0.8f); // Purple gradient
            
            RectTransform gradientRect = bgGradient.GetComponent<RectTransform>();
            gradientRect.anchorMin = new Vector2(0f, 0f);
            gradientRect.anchorMax = new Vector2(1f, 0.7f);
            gradientRect.offsetMin = Vector2.zero;
            gradientRect.offsetMax = Vector2.zero;
            
            // Add subtle animation to background
            StartCoroutine(AnimateBackgroundGradient(gradientImage));
        }
        
        private void CreateAnimatedTitle(Transform parent)
        {
            GameObject titleContainer = CreateUIPanel("Title Container", parent);
            titleContainer.GetComponent<Image>().color = Color.clear; // Invisible container
            
            RectTransform titleRect = titleContainer.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.75f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Main title
            GameObject titleObj = new GameObject("Game Title");
            titleObj.transform.SetParent(titleContainer.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "WHAT'S MY POTATO?";
            titleText.fontSize = 64;
            titleText.color = accentColor; // Gold
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            // Add outline effect
            titleText.outlineColor = new Color(0f, 0f, 0f, 0.8f);
            titleText.outlineWidth = 0.3f;
            
            SetFullScreen(titleObj.GetComponent<RectTransform>());
            
            // Subtitle
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(titleContainer.transform, false);
            subtitleObj.layer = 5;
            
            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Mobile Card Battle Arena";
            subtitleText.fontSize = 28;
            subtitleText.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.fontStyle = FontStyles.Italic;
            
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0.4f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
            
            // Animate title
            StartCoroutine(AnimateTitle(titleText, subtitleText));
        }
        
        private void CreateLoginForm(Transform parent)
        {
            // Form container with rounded background
            GameObject formContainer = CreateUIPanel("Login Form", parent);
            Image formBg = formContainer.GetComponent<Image>();
            formBg.color = new Color(0f, 0f, 0f, 0.4f); // Semi-transparent dark
            
            RectTransform formRect = formContainer.GetComponent<RectTransform>();
            formRect.anchorMin = new Vector2(0.08f, 0.35f);
            formRect.anchorMax = new Vector2(0.92f, 0.65f);
            formRect.offsetMin = Vector2.zero;
            formRect.offsetMax = Vector2.zero;
            
            // Welcome text
            GameObject welcomeObj = new GameObject("Welcome Text");
            welcomeObj.transform.SetParent(formContainer.transform, false);
            welcomeObj.layer = 5;
            
            TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
            welcomeText.text = "Welcome Back, Potato Warrior!";
            welcomeText.fontSize = 32;
            welcomeText.color = accentColor;
            welcomeText.alignment = TextAlignmentOptions.Center;
            welcomeText.fontStyle = FontStyles.Bold;
            
            RectTransform welcomeRect = welcomeObj.GetComponent<RectTransform>();
            welcomeRect.anchorMin = new Vector2(0.05f, 0.8f);
            welcomeRect.anchorMax = new Vector2(0.95f, 0.95f);
            welcomeRect.offsetMin = Vector2.zero;
            welcomeRect.offsetMax = Vector2.zero;
            
            // Email input field
            CreateStylizedInputField(formContainer.transform, "Email", 
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.7f), "📧", false);
            
            // Password input field
            CreateStylizedInputField(formContainer.transform, "Password", 
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.5f), "🔒", true);
            
            // Sign in button
            CreateStylizedButton(formContainer.transform, "Sign In Button", "🚀 SIGN IN",
                new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.25f), secondaryColor, () => {
                    OnSignInPressed(welcomeText, formContainer);
                });
            
            // Sign up link
            CreateTextButton(formContainer.transform, "Sign Up Link", 
                "Don't have an account? Create one!", 
                new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.08f), () => {
                    Debug.Log("🔗 Sign up pressed");
                    welcomeText.text = "Sign up feature coming soon!";
                    welcomeText.color = successColor;
                });
        }
        
        private void CreateStylizedInputField(Transform parent, string fieldName, Vector2 anchorMin, Vector2 anchorMax, string icon, bool isPassword)
        {
            // Input container with shadow effect
            GameObject inputContainer = CreateUIPanel($"{fieldName} Container", parent);
            Image containerBg = inputContainer.GetComponent<Image>();
            containerBg.color = new Color(1f, 1f, 1f, 0.95f); // Almost white
            
            RectTransform containerRect = inputContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = anchorMin;
            containerRect.anchorMax = anchorMax;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Icon
            GameObject iconObj = new GameObject($"{fieldName} Icon");
            iconObj.transform.SetParent(inputContainer.transform, false);
            iconObj.layer = 5;
            
            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = icon;
            iconText.fontSize = 32;
            iconText.color = primaryColor;
            iconText.alignment = TextAlignmentOptions.Center;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0f);
            iconRect.anchorMax = new Vector2(0.15f, 1f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            
            // Input field
            GameObject inputObj = new GameObject($"{fieldName} Input");
            inputObj.transform.SetParent(inputContainer.transform, false);
            inputObj.layer = 5;
            
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            if (isPassword) inputField.contentType = TMP_InputField.ContentType.Password;
            
            // Text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            textArea.layer = 5;
            textArea.AddComponent<RectMask2D>();
            
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = new Vector2(0.05f, 0.1f);
            textAreaRect.anchorMax = new Vector2(0.95f, 0.9f);
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;
            
            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            placeholderObj.layer = 5;
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = $"Enter your {fieldName.ToLower()}";
            placeholderText.fontSize = 28;
            placeholderText.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            placeholderText.alignment = TextAlignmentOptions.Left;
            placeholderText.raycastTarget = false;
            
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());
            
            // Input text
            GameObject inputTextObj = new GameObject("Input Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            inputTextObj.layer = 5;
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 28;
            inputText.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            inputText.alignment = TextAlignmentOptions.Left;
            inputText.raycastTarget = false;
            
            SetFullScreen(inputTextObj.GetComponent<RectTransform>());
            
            // Connect input field
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            // Position input field
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.15f, 0f);
            inputRect.anchorMax = new Vector2(1f, 1f);
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            
            // Add focus animations
            AddInputFieldAnimations(inputContainer, inputField);
        }
        
        private void CreateStylizedButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color buttonColor, System.Action onClickAction)
        {
            // Button container for shadow effect
            GameObject buttonShadow = CreateUIPanel($"{name} Shadow", parent);
            Image shadowImage = buttonShadow.GetComponent<Image>();
            shadowImage.color = new Color(0f, 0f, 0f, 0.3f); // Shadow
            
            RectTransform shadowRect = buttonShadow.GetComponent<RectTransform>();
            shadowRect.anchorMin = anchorMin;
            shadowRect.anchorMax = anchorMax;
            shadowRect.offsetMin = new Vector2(5f, -5f); // Shadow offset
            shadowRect.offsetMax = new Vector2(5f, -5f);
            
            // Main button
            GameObject buttonObj = CreateUIPanel(name, parent);
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = buttonColor;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            // Button text
            GameObject textObj = new GameObject("Button Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 36;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            
            // Add outline for better readability
            btnText.outlineColor = new Color(0f, 0f, 0f, 0.5f);
            btnText.outlineWidth = 0.2f;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
            
            // Button animations and interactions
            AddButtonAnimations(button, buttonImage, btnText, shadowRect);
            
            // Click action
            button.onClick.AddListener(() => onClickAction?.Invoke());
        }
        
        private void CreateTextButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, System.Action onClickAction)
        {
            GameObject textBtnObj = new GameObject(name);
            textBtnObj.transform.SetParent(parent, false);
            textBtnObj.layer = 5;
            
            Button button = textBtnObj.AddComponent<Button>();
            button.transition = Selectable.Transition.None; // Custom animation
            
            TextMeshProUGUI textComponent = textBtnObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 24;
            textComponent.color = accentColor;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = FontStyles.Underline;
            
            RectTransform textRect = textBtnObj.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Hover effect
            AddTextButtonHover(button, textComponent);
            
            button.onClick.AddListener(() => onClickAction?.Invoke());
        }
        
        private void CreateFloatingParticles(Transform parent)
        {
            // Create floating potato emojis as background decoration
            for (int i = 0; i < 8; i++)
            {
                GameObject particle = new GameObject($"Particle {i}");
                particle.transform.SetParent(parent, false);
                particle.layer = 5;
                
                TextMeshProUGUI particleText = particle.AddComponent<TextMeshProUGUI>();
                particleText.text = "🥔";
                particleText.fontSize = Random.Range(24, 48);
                particleText.color = new Color(1f, 1f, 1f, Random.Range(0.1f, 0.3f));
                particleText.alignment = TextAlignmentOptions.Center;
                particleText.raycastTarget = false;
                
                RectTransform particleRect = particle.GetComponent<RectTransform>();
                particleRect.anchorMin = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
                particleRect.anchorMax = particleRect.anchorMin;
                particleRect.sizeDelta = new Vector2(60, 60);
                
                // Animate floating
                StartCoroutine(AnimateFloatingParticle(particle.transform, i));
            }
        }
        
        private void OnSignInPressed(TextMeshProUGUI statusText, GameObject formContainer)
        {
            if (isTransitioning) return;
            
            Debug.Log("🚀 Beautiful sign in pressed!");
            
            isTransitioning = true;
            StartCoroutine(BeautifulSignInTransition(statusText, formContainer));
        }
        
        private IEnumerator BeautifulSignInTransition(TextMeshProUGUI statusText, GameObject formContainer)
        {
            // Update status with style
            statusText.text = "🔮 Connecting to Potato Realm...";
            statusText.color = accentColor;
            
            // Animate form fade out
            yield return StartCoroutine(AnimateFade(formContainer, 0f, 0.5f));
            
            // Show connecting animation
            statusText.text = "✨ Authenticating Potato Powers...";
            yield return new WaitForSeconds(1f);
            
            statusText.text = "🎮 Loading Game Interface...";
            yield return new WaitForSeconds(1f);
            
            // Create main game UI
            CreateMainGameInterface();
            
            // Transition to main UI
            yield return StartCoroutine(TransitionToMainGame());
        }
        
        private void CreateMainGameInterface()
        {
            mainGameUI = CreateUIPanel("Main Game UI", mainCanvas.transform);
            mainGameUI.SetActive(false); // Start hidden
            
            // Create gradient background for main UI
            CreateMainGameBackground(mainGameUI);
            
            // Create top bar
            CreateTopBar(mainGameUI.transform);
            
            // Create navigation system
            CreateNavigationBar(mainGameUI.transform);
            
            // Create battle button
            CreateBattleButton(mainGameUI.transform);
            
            // Create main content area
            CreateMainContent(mainGameUI.transform);
        }
        
        private void CreateMainGameBackground(GameObject parent)
        {
            // Animated gradient background
            GameObject bg1 = CreateUIPanel("Main BG 1", parent.transform);
            Image bg1Image = bg1.GetComponent<Image>();
            bg1Image.color = new Color(0.1f, 0.05f, 0.2f, 1f);
            SetFullScreen(bg1.GetComponent<RectTransform>());
            
            GameObject bg2 = CreateUIPanel("Main BG 2", parent.transform);
            Image bg2Image = bg2.GetComponent<Image>();
            bg2Image.color = new Color(0.2f, 0.1f, 0.3f, 0.7f);
            
            RectTransform bg2Rect = bg2.GetComponent<RectTransform>();
            bg2Rect.anchorMin = new Vector2(0f, 0.3f);
            bg2Rect.anchorMax = new Vector2(1f, 1f);
            bg2Rect.offsetMin = Vector2.zero;
            bg2Rect.offsetMax = Vector2.zero;
            
            // Animate main background
            StartCoroutine(AnimateMainBackground(bg2Image));
        }
        
        private void CreateTopBar(Transform parent)
        {
            GameObject topBar = CreateUIPanel("Top Bar", parent);
            Image topBarBg = topBar.GetComponent<Image>();
            topBarBg.color = new Color(0f, 0f, 0f, 0.6f);
            
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 0.9f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.offsetMin = Vector2.zero;
            topBarRect.offsetMax = Vector2.zero;
            
            // User welcome text
            GameObject userText = new GameObject("User Text");
            userText.transform.SetParent(topBar.transform, false);
            userText.layer = 5;
            
            TextMeshProUGUI userWelcome = userText.AddComponent<TextMeshProUGUI>();
            userWelcome.text = "🎮 Welcome to the Arena!";
            userWelcome.fontSize = 28;
            userWelcome.color = accentColor;
            userWelcome.alignment = TextAlignmentOptions.Center;
            userWelcome.fontStyle = FontStyles.Bold;
            
            SetFullScreen(userText.GetComponent<RectTransform>());
        }
        
        private void CreateNavigationBar(Transform parent)
        {
            GameObject navBar = CreateUIPanel("Navigation Bar", parent);
            Image navBg = navBar.GetComponent<Image>();
            navBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform navRect = navBar.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0f, 0f);
            navRect.anchorMax = new Vector2(1f, 0.15f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;
            
            // Navigation buttons with beautiful styling
            CreateNavButton(navBar.transform, "🏠", "HOME", 0f, 0.25f, 0);
            CreateNavButton(navBar.transform, "📚", "CARDS", 0.25f, 0.5f, 1);
            CreateNavButton(navBar.transform, "🔧", "DECKS", 0.5f, 0.75f, 2);
            CreateNavButton(navBar.transform, "🦸", "HEROES", 0.75f, 1f, 3);
        }
        
        private void CreateNavButton(Transform parent, string icon, string label, float minX, float maxX, int index)
        {
            GameObject navBtn = CreateUIPanel($"Nav {label}", parent);
            Button button = navBtn.AddComponent<Button>();
            Image btnImage = navBtn.GetComponent<Image>();
            btnImage.color = new Color(0.3f, 0.2f, 0.5f, 0.8f);
            
            RectTransform btnRect = navBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(minX + 0.02f, 0.1f);
            btnRect.anchorMax = new Vector2(maxX - 0.02f, 0.9f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(navBtn.transform, false);
            iconObj.layer = 5;
            
            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = icon;
            iconText.fontSize = 32;
            iconText.color = accentColor;
            iconText.alignment = TextAlignmentOptions.Center;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.4f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            
            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(navBtn.transform, false);
            labelObj.layer = 5;
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
            
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0.4f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Button animations
            AddNavButtonAnimations(button, btnImage, iconText, labelText, index);
        }
        
        private void CreateBattleButton(Transform parent)
        {
            // Battle button shadow
            GameObject battleShadow = CreateUIPanel("Battle Shadow", parent);
            Image shadowImage = battleShadow.GetComponent<Image>();
            shadowImage.color = new Color(0f, 0f, 0f, 0.4f);
            
            RectTransform shadowRect = battleShadow.GetComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(0.65f, 0.18f);
            shadowRect.anchorMax = new Vector2(0.95f, 0.4f);
            shadowRect.offsetMin = new Vector2(8f, -8f);
            shadowRect.offsetMax = new Vector2(8f, -8f);
            
            // Main battle button
            GameObject battleBtn = CreateUIPanel("Battle Button", parent);
            Button button = battleBtn.AddComponent<Button>();
            Image btnImage = battleBtn.GetComponent<Image>();
            btnImage.color = dangerColor; // Red for battle
            
            RectTransform btnRect = battleBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.65f, 0.18f);
            btnRect.anchorMax = new Vector2(0.95f, 0.4f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Battle icon
            GameObject iconObj = new GameObject("Battle Icon");
            iconObj.transform.SetParent(battleBtn.transform, false);
            iconObj.layer = 5;
            
            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = "⚔️";
            iconText.fontSize = 48;
            iconText.color = Color.white;
            iconText.alignment = TextAlignmentOptions.Center;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.4f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            
            // Battle label
            GameObject labelObj = new GameObject("Battle Label");
            labelObj.transform.SetParent(battleBtn.transform, false);
            labelObj.layer = 5;
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "BATTLE";
            labelText.fontSize = 24;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
            
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0.4f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Epic battle button animations
            AddBattleButtonAnimations(button, btnImage, iconText, labelText, shadowRect);
        }
        
        private void CreateMainContent(Transform parent)
        {
            GameObject contentArea = CreateUIPanel("Main Content", parent);
            contentArea.GetComponent<Image>().color = Color.clear;
            
            RectTransform contentRect = contentArea.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.05f, 0.15f);
            contentRect.anchorMax = new Vector2(0.95f, 0.9f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Welcome message
            GameObject welcomeObj = new GameObject("Main Welcome");
            welcomeObj.transform.SetParent(contentArea.transform, false);
            welcomeObj.layer = 5;
            
            TextMeshProUGUI welcomeText = welcomeObj.AddComponent<TextMeshProUGUI>();
            welcomeText.text = "🎉 READY FOR BATTLE!\n\nChoose your adventure:\n• Browse your card collection\n• Build powerful decks\n• Meet legendary heroes\n• Battle other players!";
            welcomeText.fontSize = 32;
            welcomeText.color = Color.white;
            welcomeText.alignment = TextAlignmentOptions.Center;
            welcomeText.lineSpacing = 10f;
            
            SetFullScreen(welcomeObj.GetComponent<RectTransform>());
            
            // Animate welcome text
            StartCoroutine(AnimateWelcomeText(welcomeText));
        }
        
        private IEnumerator TransitionToMainGame()
        {
            // Hide login screen with beautiful animation
            yield return StartCoroutine(AnimateFade(loginScreen, 0f, 0.8f));
            loginScreen.SetActive(false);
            
            // Show main game UI with animation
            mainGameUI.SetActive(true);
            yield return StartCoroutine(AnimateFade(mainGameUI, 1f, 0.8f));
            
            isTransitioning = false;
            
            Debug.Log("🎮 Transitioned to beautiful main game UI!");
        }
        
        #region Animation Methods
        
        private IEnumerator AnimateTitle(TextMeshProUGUI titleText, TextMeshProUGUI subtitleText)
        {
            // Animate title scale in
            titleText.transform.localScale = Vector3.zero;
            subtitleText.transform.localScale = Vector3.zero;
            
            yield return StartCoroutine(AnimateScale(titleText.transform, Vector3.one, 1f));
            yield return StartCoroutine(AnimateScale(subtitleText.transform, Vector3.one, 0.5f));
            
            // Continuous glow animation
            StartCoroutine(AnimateTextGlow(titleText));
        }
        
        private IEnumerator AnimateTextGlow(TextMeshProUGUI text)
        {
            Color originalColor = text.color;
            
            while (text != null)
            {
                // Pulse effect
                float glow = (Mathf.Sin(Time.time * 2f) + 1f) / 2f; // 0 to 1
                text.color = Color.Lerp(originalColor, Color.white, glow * 0.3f);
                yield return null;
            }
        }
        
        private IEnumerator AnimateBackgroundGradient(Image gradientImage)
        {
            Color originalColor = gradientImage.color;
            
            while (gradientImage != null)
            {
                float wave = (Mathf.Sin(Time.time * 0.5f) + 1f) / 2f;
                gradientImage.color = Color.Lerp(originalColor, new Color(0.3f, 0.2f, 0.4f, 0.8f), wave * 0.5f);
                yield return null;
            }
        }
        
        private IEnumerator AnimateMainBackground(Image backgroundImage)
        {
            Color originalColor = backgroundImage.color;
            
            while (backgroundImage != null)
            {
                float wave = (Mathf.Sin(Time.time * 0.3f) + 1f) / 2f;
                backgroundImage.color = Color.Lerp(originalColor, new Color(0.25f, 0.15f, 0.35f, 0.9f), wave * 0.3f);
                yield return null;
            }
        }
        
        private IEnumerator AnimateFloatingParticle(Transform particle, int index)
        {
            Vector3 startPos = particle.localPosition;
            float speed = Random.Range(10f, 30f);
            float amplitude = Random.Range(50f, 150f);
            
            while (particle != null)
            {
                float time = Time.time * speed + index * 2f;
                Vector3 offset = new Vector3(
                    Mathf.Sin(time) * amplitude,
                    Mathf.Cos(time * 0.7f) * amplitude,
                    0f
                );
                
                particle.localPosition = startPos + offset;
                yield return null;
            }
        }
        
        private IEnumerator AnimateWelcomeText(TextMeshProUGUI welcomeText)
        {
            welcomeText.color = new Color(1f, 1f, 1f, 0f);
            
            // Fade in
            yield return StartCoroutine(AnimateTextFade(welcomeText, 1f, 1f));
            
            // Continuous subtle pulse
            StartCoroutine(AnimateTextPulse(welcomeText));
        }
        
        private IEnumerator AnimateTextPulse(TextMeshProUGUI text)
        {
            while (text != null)
            {
                float pulse = (Mathf.Sin(Time.time * 1.5f) + 1f) / 2f;
                float scale = Mathf.Lerp(0.98f, 1.02f, pulse);
                text.transform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
        
        #endregion
        
        #region Button Animations
        
        private void AddButtonAnimations(Button button, Image buttonImage, TextMeshProUGUI buttonText, RectTransform shadowRect)
        {
            Color originalColor = buttonImage.color;
            Color hoverColor = new Color(originalColor.r * 1.2f, originalColor.g * 1.2f, originalColor.b * 1.2f, 1f);
            
            // Add event triggers
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            
            // Hover enter
            EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
            hoverEnter.eventID = EventTriggerType.PointerEnter;
            hoverEnter.callback.AddListener((data) => {
                StartCoroutine(AnimateButtonHover(button.transform, buttonImage, hoverColor, true));
            });
            trigger.triggers.Add(hoverEnter);
            
            // Hover exit
            EventTrigger.Entry hoverExit = new EventTrigger.Entry();
            hoverExit.eventID = EventTriggerType.PointerExit;
            hoverExit.callback.AddListener((data) => {
                StartCoroutine(AnimateButtonHover(button.transform, buttonImage, originalColor, false));
            });
            trigger.triggers.Add(hoverExit);
            
            // Click animation
            EventTrigger.Entry clickDown = new EventTrigger.Entry();
            clickDown.eventID = EventTriggerType.PointerDown;
            clickDown.callback.AddListener((data) => {
                StartCoroutine(AnimateButtonPress(button.transform, shadowRect));
            });
            trigger.triggers.Add(clickDown);
        }
        
        private void AddNavButtonAnimations(Button button, Image buttonImage, TextMeshProUGUI iconText, TextMeshProUGUI labelText, int index)
        {
            Color originalColor = buttonImage.color;
            Color activeColor = new Color(0.5f, 0.3f, 0.7f, 1f);
            
            button.onClick.AddListener(() => {
                Debug.Log($"🎯 Navigation: {labelText.text}");
                StartCoroutine(AnimateNavButtonPress(buttonImage, iconText, labelText, originalColor, activeColor));
            });
            
            // Hover animations
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
            hoverEnter.eventID = EventTriggerType.PointerEnter;
            hoverEnter.callback.AddListener((data) => {
                StartCoroutine(AnimateScale(button.transform, Vector3.one * 1.1f, 0.2f));
            });
            trigger.triggers.Add(hoverEnter);
            
            EventTrigger.Entry hoverExit = new EventTrigger.Entry();
            hoverExit.eventID = EventTriggerType.PointerExit;
            hoverExit.callback.AddListener((data) => {
                StartCoroutine(AnimateScale(button.transform, Vector3.one, 0.2f));
            });
            trigger.triggers.Add(hoverExit);
        }
        
        private void AddBattleButtonAnimations(Button button, Image buttonImage, TextMeshProUGUI iconText, TextMeshProUGUI labelText, RectTransform shadowRect)
        {
            Color originalColor = buttonImage.color;
            Color searchingColor = new Color(1f, 0.6f, 0f, 1f); // Orange
            Color foundColor = successColor; // Green
            
            button.onClick.AddListener(() => {
                Debug.Log("⚔️ EPIC BATTLE BUTTON PRESSED!");
                StartCoroutine(AnimateBattleSearch(buttonImage, iconText, labelText, originalColor, searchingColor, foundColor));
            });
            
            // Epic hover effect
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
            hoverEnter.eventID = EventTriggerType.PointerEnter;
            hoverEnter.callback.AddListener((data) => {
                StartCoroutine(AnimateScale(button.transform, Vector3.one * 1.05f, 0.1f));
                StartCoroutine(AnimateBattleGlow(iconText));
            });
            trigger.triggers.Add(hoverEnter);
            
            EventTrigger.Entry hoverExit = new EventTrigger.Entry();
            hoverExit.eventID = EventTriggerType.PointerExit;
            hoverExit.callback.AddListener((data) => {
                StartCoroutine(AnimateScale(button.transform, Vector3.one, 0.1f));
            });
            trigger.triggers.Add(hoverExit);
        }
        
        private void AddInputFieldAnimations(GameObject inputContainer, TMP_InputField inputField)
        {
            Image containerImage = inputContainer.GetComponent<Image>();
            Color originalColor = containerImage.color;
            Color focusColor = new Color(1f, 1f, 1f, 1f);
            
            inputField.onSelect.AddListener((text) => {
                StartCoroutine(AnimateInputFocus(containerImage, focusColor, true));
            });
            
            inputField.onDeselect.AddListener((text) => {
                StartCoroutine(AnimateInputFocus(containerImage, originalColor, false));
            });
        }
        
        private void AddTextButtonHover(Button button, TextMeshProUGUI textComponent)
        {
            Color originalColor = textComponent.color;
            Color hoverColor = Color.white;
            
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
            hoverEnter.eventID = EventTriggerType.PointerEnter;
            hoverEnter.callback.AddListener((data) => {
                StartCoroutine(AnimateTextColor(textComponent, hoverColor, 0.2f));
            });
            trigger.triggers.Add(hoverEnter);
            
            EventTrigger.Entry hoverExit = new EventTrigger.Entry();
            hoverExit.eventID = EventTriggerType.PointerExit;
            hoverExit.callback.AddListener((data) => {
                StartCoroutine(AnimateTextColor(textComponent, originalColor, 0.2f));
            });
            trigger.triggers.Add(hoverExit);
        }
        
        #endregion
        
        #region Animation Coroutines
        
        private IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration)
        {
            Vector3 startScale = target.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = Mathf.SmoothStep(0f, 1f, t);
                
                target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            target.localScale = targetScale;
        }
        
        private IEnumerator AnimateFade(GameObject target, float targetAlpha, float duration)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = target.AddComponent<CanvasGroup>();
            
            float startAlpha = canvasGroup.alpha;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
        }
        
        private IEnumerator AnimateButtonHover(Transform buttonTransform, Image buttonImage, Color targetColor, bool isHovering)
        {
            Color startColor = buttonImage.color;
            Vector3 targetScale = isHovering ? Vector3.one * 1.05f : Vector3.one;
            
            float duration = 0.2f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                buttonImage.color = Color.Lerp(startColor, targetColor, t);
                buttonTransform.localScale = Vector3.Lerp(buttonTransform.localScale, targetScale, t);
                yield return null;
            }
            
            buttonImage.color = targetColor;
            buttonTransform.localScale = targetScale;
        }
        
        private IEnumerator AnimateButtonPress(Transform buttonTransform, RectTransform shadowRect)
        {
            Vector3 originalScale = buttonTransform.localScale;
            Vector2 originalShadowOffset = shadowRect.offsetMin;
            
            // Press down
            yield return StartCoroutine(AnimateScale(buttonTransform, originalScale * 0.95f, 0.1f));
            shadowRect.offsetMin = Vector2.zero;
            shadowRect.offsetMax = Vector2.zero;
            
            yield return new WaitForSeconds(0.1f);
            
            // Release
            yield return StartCoroutine(AnimateScale(buttonTransform, originalScale, 0.1f));
            shadowRect.offsetMin = originalShadowOffset;
            shadowRect.offsetMax = originalShadowOffset;
        }
        
        private IEnumerator AnimateNavButtonPress(Image buttonImage, TextMeshProUGUI iconText, TextMeshProUGUI labelText, Color originalColor, Color activeColor)
        {
            // Flash active color
            buttonImage.color = activeColor;
            iconText.color = Color.white;
            labelText.color = Color.white;
            
            yield return new WaitForSeconds(0.3f);
            
            // Return to original
            yield return StartCoroutine(AnimateImageColor(buttonImage, originalColor, 0.5f));
            iconText.color = accentColor;
            labelText.color = Color.white;
        }
        
        private IEnumerator AnimateBattleSearch(Image buttonImage, TextMeshProUGUI iconText, TextMeshProUGUI labelText, Color originalColor, Color searchingColor, Color foundColor)
        {
            // Start searching
            labelText.text = "SEARCHING...";
            yield return StartCoroutine(AnimateImageColor(buttonImage, searchingColor, 0.3f));
            
            // Pulse while searching
            for (int i = 0; i < 6; i++)
            {
                yield return StartCoroutine(AnimateScale(buttonImage.transform, Vector3.one * 1.1f, 0.25f));
                yield return StartCoroutine(AnimateScale(buttonImage.transform, Vector3.one, 0.25f));
            }
            
            // Match found!
            labelText.text = "MATCH FOUND!";
            iconText.text = "🎉";
            yield return StartCoroutine(AnimateImageColor(buttonImage, foundColor, 0.3f));
            
            yield return new WaitForSeconds(2f);
            
            // Return to normal
            labelText.text = "BATTLE";
            iconText.text = "⚔️";
            yield return StartCoroutine(AnimateImageColor(buttonImage, originalColor, 0.5f));
        }
        
        private IEnumerator AnimateBattleGlow(TextMeshProUGUI iconText)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return StartCoroutine(AnimateScale(iconText.transform, Vector3.one * 1.2f, 0.2f));
                yield return StartCoroutine(AnimateScale(iconText.transform, Vector3.one, 0.2f));
            }
        }
        
        private IEnumerator AnimateInputFocus(Image inputImage, Color targetColor, bool isFocused)
        {
            Color startColor = inputImage.color;
            float duration = 0.3f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                inputImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            inputImage.color = targetColor;
        }
        
        private IEnumerator AnimateTextColor(TextMeshProUGUI text, Color targetColor, float duration)
        {
            Color startColor = text.color;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                text.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            text.color = targetColor;
        }
        
        private IEnumerator AnimateImageColor(Image image, Color targetColor, float duration)
        {
            Color startColor = image.color;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                image.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            image.color = targetColor;
        }
        
        private IEnumerator AnimateTextFade(TextMeshProUGUI text, float targetAlpha, float duration)
        {
            Color startColor = text.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                text.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            text.color = targetColor;
        }
        
        #endregion
        
        #region Helper Methods
        
        private GameObject CreateUIPanel(string name, Transform parent)
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
        
        #endregion
    }
}