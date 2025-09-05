using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FULLY EDITABLE Main Menu Screen
    /// Customize main menu completely in Inspector!
    /// </summary>
    public class EditableMainMenu : MonoBehaviour
    {
        [Header("🏠 MAIN MENU - EDIT EVERYTHING IN INSPECTOR!")]
        [Space(10)]
        
        [Header("🎨 Background & Layout")]
        [Tooltip("Custom background image")]
        public Sprite mainMenuBackground;
        
        [Tooltip("Background color tint")]
        public Color backgroundTint = Color.white;
        
        [Space(10)]
        [Header("📱 Layout Areas - Drag to Reposition")]
        [Tooltip("Top header area (player info, currency)")]
        public RectTransform headerArea;
        
        [Tooltip("Navigation cards area (Collection, Deck Builder, Hero Hall)")]
        public RectTransform navigationArea;
        
        [Tooltip("Battle button area")]
        public RectTransform battleButtonArea;
        
        [Tooltip("Utility buttons area (settings, shop, logout)")]
        public RectTransform utilityArea;
        
        [Tooltip("Welcome message area")]
        public RectTransform welcomeArea;
        
        [Space(10)]
        [Header("🎨 Visual Styling")]
        [Tooltip("Welcome message text")]
        public string welcomeMessage = "Welcome to Potato Legends!";
        
        [Range(16, 48)]
        [Tooltip("Welcome text font size")]
        public int welcomeFontSize = 24;
        
        [Tooltip("Welcome text color")]
        public Color welcomeColor = new Color(1f, 0.9f, 0.3f, 1f);
        
        [Tooltip("Navigation card color")]
        public Color navigationCardColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        
        [Tooltip("Battle button color")]
        public Color battleButtonColor = new Color(0.8f, 0.2f, 0.1f, 1f);
        
        [Tooltip("Utility button color")]
        public Color utilityButtonColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        [Space(10)]
        [Header("💰 Currency Display")]
        [Tooltip("Show gold display")]
        public bool showGold = true;
        
        [Tooltip("Show gems display")]
        public bool showGems = true;
        
        [Tooltip("Gold icon sprite")]
        public Sprite goldIcon;
        
        [Tooltip("Gems icon sprite")]
        public Sprite gemsIcon;
        
        [Space(10)]
        [Header("🎮 Navigation Icons")]
        [Tooltip("Collection icon")]
        public Sprite collectionIcon;
        
        [Tooltip("Deck builder icon")]
        public Sprite deckBuilderIcon;
        
        [Tooltip("Hero hall icon")]
        public Sprite heroHallIcon;
        
        [Tooltip("Battle icon")]
        public Sprite battleIcon;
        
        [Space(10)]
        [Header("🔧 Layout Settings")]
        [Range(2, 6)]
        [Tooltip("Navigation cards per row")]
        public int navigationCardsPerRow = 3;
        
        [Range(100, 300)]
        [Tooltip("Navigation card size")]
        public float navigationCardSize = 150f;
        
        [Range(100, 200)]
        [Tooltip("Battle button size")]
        public float battleButtonSize = 120f;
        
        [Space(10)]
        [Header("🔄 Quick Actions")]
        [Tooltip("Refresh this screen")]
        public bool refreshScreen = false;
        
        [Tooltip("Reset to defaults")]
        public bool resetToDefaults = false;
        
        // Runtime data
        private bool isInitialized = false;
        
        void Start()
        {
            InitializeMainMenu();
        }
        
        void OnValidate()
        {
            if (Application.isPlaying && isInitialized)
            {
                if (refreshScreen)
                {
                    refreshScreen = false;
                    RefreshScreen();
                }
                
                if (resetToDefaults)
                {
                    resetToDefaults = false;
                    ResetToDefaults();
                }
                
                ApplyVisualSettings();
            }
        }
        
        private void InitializeMainMenu()
        {
            Debug.Log("🏠 Initializing EDITABLE Main Menu...");
            
            // Create editable layout
            CreateEditableLayout();
            
            // Create content
            CreateMainMenuContent();
            
            // Apply settings
            ApplyVisualSettings();
            
            isInitialized = true;
            Debug.Log("✅ Editable Main Menu ready for customization!");
        }
        
        private void CreateEditableLayout()
        {
            // Background
            CreateEditableBackground();
            
            // Create areas if they don't exist
            if (headerArea == null) headerArea = CreateEditableArea("Header Area", new Vector2(0f, 0.85f), new Vector2(1f, 1f));
            if (welcomeArea == null) welcomeArea = CreateEditableArea("Welcome Area", new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.82f));
            if (navigationArea == null) navigationArea = CreateEditableArea("Navigation Area", new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.65f));
            if (battleButtonArea == null) battleButtonArea = CreateEditableArea("Battle Button Area", new Vector2(0.75f, 0.05f), new Vector2(0.95f, 0.25f));
            if (utilityArea == null) utilityArea = CreateEditableArea("Utility Area", new Vector2(0.05f, 0.05f), new Vector2(0.7f, 0.15f));
        }
        
        private void CreateEditableBackground()
        {
            GameObject bgObj = new GameObject("🎨 Main Menu Background");
            bgObj.transform.SetParent(transform, false);
            bgObj.layer = 5;
            
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImg = bgObj.AddComponent<Image>();
            if (mainMenuBackground != null)
            {
                bgImg.sprite = mainMenuBackground;
                bgImg.color = backgroundTint;
            }
            else
            {
                bgImg.color = new Color(0.05f, 0.1f, 0.2f, 1f); // Dark blue fallback
            }
        }
        
        private RectTransform CreateEditableArea(string areaName, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject areaObj = new GameObject(areaName);
            areaObj.transform.SetParent(transform, false);
            areaObj.layer = 5;
            
            RectTransform areaRect = areaObj.AddComponent<RectTransform>();
            areaRect.anchorMin = anchorMin;
            areaRect.anchorMax = anchorMax;
            areaRect.offsetMin = Vector2.zero;
            areaRect.offsetMax = Vector2.zero;
            
            // Visual indicator for editing
            Image areaImg = areaObj.AddComponent<Image>();
            areaImg.color = new Color(0f, 1f, 0f, 0.05f); // Very transparent green
            
            return areaRect;
        }
        
        private void CreateMainMenuContent()
        {
            // Header with player info and currency
            if (headerArea != null)
            {
                CreatePlayerHeader();
            }
            
            // Welcome message
            if (welcomeArea != null)
            {
                CreateWelcomeMessage();
            }
            
            // Navigation cards
            if (navigationArea != null)
            {
                CreateNavigationCards();
            }
            
            // Battle button
            if (battleButtonArea != null)
            {
                CreateBattleButton();
            }
            
            // Utility buttons
            if (utilityArea != null)
            {
                CreateUtilityButtons();
            }
        }
        
        private void CreatePlayerHeader()
        {
            // Player name
            CreateText("Player Name", "cianti12345", headerArea, new Vector2(0.05f, 0.5f), new Vector2(0.4f, 1f), 18, Color.white);
            
            // Gold display
            if (showGold)
            {
                CreateCurrencyDisplay("Gold", "1,250", goldIcon, headerArea, new Vector2(0.5f, 0.5f), new Vector2(0.7f, 1f));
            }
            
            // Gems display
            if (showGems)
            {
                CreateCurrencyDisplay("Gems", "45", gemsIcon, headerArea, new Vector2(0.75f, 0.5f), new Vector2(0.95f, 1f));
            }
        }
        
        private void CreateWelcomeMessage()
        {
            CreateText("Welcome Message", welcomeMessage, welcomeArea, Vector2.zero, Vector2.one, welcomeFontSize, welcomeColor);
        }
        
        private void CreateNavigationCards()
        {
            // Collection card
            CreateNavigationCard("COLLECTION", "Browse your cards", collectionIcon, navigationCardColor, 
                new Vector2(0.05f, 0.6f), new Vector2(0.32f, 0.95f), () => {
                    EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                    uiManager?.GoToCollection();
                });
            
            // Deck Builder card
            CreateNavigationCard("DECK BUILDER", "Build your deck", deckBuilderIcon, navigationCardColor,
                new Vector2(0.37f, 0.6f), new Vector2(0.63f, 0.95f), () => {
                    EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                    uiManager?.GoToDeckBuilder();
                });
            
            // Hero Hall card
            CreateNavigationCard("HERO HALL", "Choose your hero", heroHallIcon, navigationCardColor,
                new Vector2(0.68f, 0.6f), new Vector2(0.95f, 0.95f), () => {
                    EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                    uiManager?.GoToHeroHall();
                });
        }
        
        private void CreateBattleButton()
        {
            GameObject battleBtn = new GameObject("⚔️ BATTLE Button");
            battleBtn.transform.SetParent(battleButtonArea, false);
            battleBtn.layer = 5;
            
            Image battleImg = battleBtn.AddComponent<Image>();
            if (battleIcon != null)
            {
                battleImg.sprite = battleIcon;
                battleImg.color = Color.white;
            }
            else
            {
                battleImg.color = battleButtonColor;
            }
            
            Button button = battleBtn.AddComponent<Button>();
            button.onClick.AddListener(() => {
                EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                uiManager?.GoToBattleArena();
            });
            
            RectTransform battleRect = battleBtn.GetComponent<RectTransform>();
            battleRect.anchorMin = Vector2.zero;
            battleRect.anchorMax = Vector2.one;
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;
            
            // Battle text
            if (battleIcon == null) // Only show text if no icon
            {
                CreateText("Battle Text", "BATTLE", battleBtn.transform, Vector2.zero, Vector2.one, 16, Color.white);
            }
        }
        
        private void CreateUtilityButtons()
        {
            // Settings button
            CreateUtilityButton("⚙️ Settings", new Vector2(0f, 0.5f), new Vector2(0.2f, 1f), () => {
                Debug.Log("⚙️ Settings clicked");
            });
            
            // Shop button
            CreateUtilityButton("🛒 Shop", new Vector2(0.25f, 0.5f), new Vector2(0.45f, 1f), () => {
                Debug.Log("🛒 Shop clicked");
            });
            
            // Logout button
            CreateUtilityButton("🚪 Logout", new Vector2(0.5f, 0.5f), new Vector2(0.7f, 1f), () => {
                EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                uiManager?.GoToAuth();
            });
        }
        
        private void CreateNavigationCard(string title, string subtitle, Sprite icon, Color bgColor, Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
        {
            GameObject cardObj = new GameObject($"Nav Card: {title}");
            cardObj.transform.SetParent(navigationArea, false);
            cardObj.layer = 5;
            
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = bgColor;
            
            Button cardButton = cardObj.AddComponent<Button>();
            if (onClick != null) cardButton.onClick.AddListener(() => onClick());
            
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchorMin = anchorMin;
            cardRect.anchorMax = anchorMax;
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            
            // Icon
            if (icon != null)
            {
                CreateIcon(icon, cardObj.transform, new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.9f));
            }
            
            // Title
            CreateText("Title", title, cardObj.transform, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.5f), 14, Color.white);
            
            // Subtitle
            CreateText("Subtitle", subtitle, cardObj.transform, new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.3f), 10, new Color(0.8f, 0.8f, 0.8f, 1f));
        }
        
        private void CreateUtilityButton(string text, Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
        {
            GameObject btnObj = new GameObject($"Utility: {text}");
            btnObj.transform.SetParent(utilityArea, false);
            btnObj.layer = 5;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = utilityButtonColor;
            
            Button button = btnObj.AddComponent<Button>();
            if (onClick != null) button.onClick.AddListener(() => onClick());
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            CreateText("Button Text", text, btnObj.transform, Vector2.zero, Vector2.one, 12, Color.white);
        }
        
        private void CreateCurrencyDisplay(string currencyName, string amount, Sprite icon, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject currencyObj = new GameObject($"{currencyName} Display");
            currencyObj.transform.SetParent(parent, false);
            currencyObj.layer = 5;
            
            RectTransform currencyRect = currencyObj.GetComponent<RectTransform>();
            currencyRect.anchorMin = anchorMin;
            currencyRect.anchorMax = anchorMax;
            currencyRect.offsetMin = Vector2.zero;
            currencyRect.offsetMax = Vector2.zero;
            
            // Background
            Image currencyBg = currencyObj.AddComponent<Image>();
            currencyBg.color = new Color(0f, 0f, 0f, 0.3f);
            
            // Icon
            if (icon != null)
            {
                CreateIcon(icon, currencyObj.transform, new Vector2(0.1f, 0.2f), new Vector2(0.4f, 0.8f));
            }
            
            // Amount text
            CreateText("Amount", amount, currencyObj.transform, new Vector2(0.45f, 0f), new Vector2(0.9f, 1f), 14, Color.white);
        }
        
        private void CreateIcon(Sprite icon, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(parent, false);
            iconObj.layer = 5;
            
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = anchorMin;
            iconRect.anchorMax = anchorMax;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
        }
        
        private void CreateText(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            textObj.layer = 5;
            
            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        public void RefreshScreen()
        {
            Debug.Log("🔄 Refreshing main menu...");
            ApplyVisualSettings();
        }
        
        public void ResetToDefaults()
        {
            Debug.Log("🔄 Resetting main menu to defaults...");
            
            // Reset positions
            if (headerArea != null)
            {
                headerArea.anchorMin = new Vector2(0f, 0.85f);
                headerArea.anchorMax = new Vector2(1f, 1f);
            }
            
            if (navigationArea != null)
            {
                navigationArea.anchorMin = new Vector2(0.1f, 0.3f);
                navigationArea.anchorMax = new Vector2(0.9f, 0.65f);
            }
            
            if (battleButtonArea != null)
            {
                battleButtonArea.anchorMin = new Vector2(0.75f, 0.05f);
                battleButtonArea.anchorMax = new Vector2(0.95f, 0.25f);
            }
            
            // Reset visual settings
            welcomeFontSize = 24;
            welcomeColor = new Color(1f, 0.9f, 0.3f, 1f);
            navigationCardColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
            battleButtonColor = new Color(0.8f, 0.2f, 0.1f, 1f);
            
            ApplyVisualSettings();
        }
        
        private void ApplyVisualSettings()
        {
            // Apply welcome message settings
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.name == "Welcome Message")
                {
                    text.text = welcomeMessage;
                    text.fontSize = welcomeFontSize;
                    text.color = welcomeColor;
                }
            }
            
            // Apply navigation card colors
            Image[] images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.Contains("Nav Card"))
                {
                    img.color = navigationCardColor;
                }
                else if (img.name.Contains("BATTLE"))
                {
                    img.color = battleButtonColor;
                }
                else if (img.name.Contains("Utility"))
                {
                    img.color = utilityButtonColor;
                }
            }
        }
    }
}