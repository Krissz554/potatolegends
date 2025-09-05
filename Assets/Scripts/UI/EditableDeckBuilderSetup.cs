using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Sets up the Editable Deck Builder structure
    /// Creates all UI elements that can then be edited in Inspector
    /// </summary>
    public class EditableDeckBuilderSetup : MonoBehaviour
    {
        [Header("🎮 DECK BUILDER SETUP")]
        [Space(10)]
        
        [Header("👆 CHECK THIS BOX TO CREATE DECK BUILDER:")]
        [Tooltip("Check this checkbox to create the editable deck builder instantly!")]
        public bool createDeckBuilderNow = false;
        
        [Space(10)]
        [Header("📋 Instructions:")]
        [TextArea(4, 6)]
        public string instructions = "1. Check the 'Create Deck Builder Now' checkbox above ☝️\n2. The editable deck builder will be created automatically\n3. Select 'EDITABLE_DECK_BUILDER' in Hierarchy\n4. Edit everything in Inspector!\n\nYou can also right-click this component and select 'Create Editable Deck Builder'";
        
        [Space(10)]
        [Header("✅ Status:")]
        [Tooltip("Shows if setup is complete")]
        public bool setupComplete = false;
        
        void Update()
        {
            // Check if user wants to create the deck builder
            if (createDeckBuilderNow && !setupComplete)
            {
                CreateEditableDeckBuilder();
                createDeckBuilderNow = false;
            }
        }
        
        [ContextMenu("🎨 Create Editable Deck Builder")]
        public void CreateEditableDeckBuilder()
        {
            Debug.Log("🎨 Creating EDITABLE Deck Builder structure...");
            
            // Create main canvas if needed
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Deck Builder Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create main deck builder container
            GameObject deckBuilderObj = new GameObject("EDITABLE_DECK_BUILDER");
            deckBuilderObj.transform.SetParent(canvas.transform, false);
            
            RectTransform mainRect = deckBuilderObj.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
            
            // Add main background
            Image mainBg = deckBuilderObj.AddComponent<Image>();
            mainBg.color = new Color(0.2f, 0.1f, 0.3f, 1f); // Purple fallback
            
            // Create management bar area
            GameObject managementBar = CreateEditablePanel("Management Bar", deckBuilderObj.transform, 
                new Vector2(0.02f, 0.88f), new Vector2(0.98f, 0.98f));
            
            // Create collection panel area  
            GameObject collectionPanel = CreateEditablePanel("Collection Panel", deckBuilderObj.transform,
                new Vector2(0.02f, 0.15f), new Vector2(0.48f, 0.85f));
            
            // Create deck panel area
            GameObject deckPanel = CreateEditablePanel("Deck Panel", deckBuilderObj.transform,
                new Vector2(0.52f, 0.15f), new Vector2(0.98f, 0.85f));
            
            // Create collection scroll view
            GameObject collectionScroll = CreateScrollView("Collection Scroll View", collectionPanel.transform);
            
            // Create deck grid area
            GameObject deckGrid = CreateGridArea("Deck Grid", deckPanel.transform);
            
            // Add titles
            CreateTitle("Deck Builder Title", managementBar.transform, "🔧 DECK BUILDER", 18);
            CreateTitle("Active Deck Text", managementBar.transform, "👑 Active: Loading...", 14);
            CreateTitle("Collection Title", collectionPanel.transform, "YOUR COLLECTION", 16);
            CreateTitle("Deck Title", deckPanel.transform, "CURRENT DECK (0/30)", 16);
            
            // Create buttons
            CreateEditableButton("Create Deck Button", managementBar.transform, "➕ New", new Vector2(0.7f, 0.1f), new Vector2(0.85f, 0.9f));
            CreateEditableButton("Deck Selector Button", managementBar.transform, "📋 Decks", new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f));
            CreateEditableButton("Back Button", deckBuilderObj.transform, "← Back", new Vector2(0.02f, 0.02f), new Vector2(0.15f, 0.12f));
            
            // Add the EditableDeckBuilder component
            EditableDeckBuilder deckBuilder = deckBuilderObj.AddComponent<EditableDeckBuilder>();
            
            // Auto-assign references
            deckBuilder.managementBarArea = managementBar.GetComponent<RectTransform>();
            deckBuilder.collectionPanelArea = collectionPanel.GetComponent<RectTransform>();
            deckBuilder.deckPanelArea = deckPanel.GetComponent<RectTransform>();
            
            deckBuilder.collectionPanelBackground = collectionPanel.GetComponent<Image>();
            deckBuilder.deckPanelBackground = deckPanel.GetComponent<Image>();
            deckBuilder.managementBarBackground = managementBar.GetComponent<Image>();
            
            deckBuilder.collectionScrollView = collectionScroll.GetComponent<ScrollRect>();
            deckBuilder.collectionGrid = collectionScroll.transform.Find("Viewport/Content").GetComponent<GridLayoutGroup>();
            deckBuilder.deckGrid = deckGrid.GetComponent<GridLayoutGroup>();
            
            // Assign text components
            deckBuilder.titleText = managementBar.transform.Find("Deck Builder Title").GetComponent<TextMeshProUGUI>();
            deckBuilder.activeDeckText = managementBar.transform.Find("Active Deck Text").GetComponent<TextMeshProUGUI>();
            deckBuilder.collectionTitleText = collectionPanel.transform.Find("Collection Title").GetComponent<TextMeshProUGUI>();
            deckBuilder.deckTitleText = deckPanel.transform.Find("Deck Title").GetComponent<TextMeshProUGUI>();
            
            // Assign buttons
            deckBuilder.createDeckButton = managementBar.transform.Find("Create Deck Button").GetComponent<Button>();
            deckBuilder.deckSelectorButton = managementBar.transform.Find("Deck Selector Button").GetComponent<Button>();
            deckBuilder.backButton = deckBuilderObj.transform.Find("Back Button").GetComponent<Button>();
            
            setupComplete = true;
            
            Debug.Log("✅ EDITABLE Deck Builder created! Check Inspector to customize everything!");
            Debug.Log("🎮 You can now edit positions, sizes, colors, and layout in Inspector!");
        }
        
        private GameObject CreateEditablePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.layer = 5;
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.3f); // Semi-transparent fallback
            
            return panel;
        }
        
        private GameObject CreateScrollView(string name, Transform parent)
        {
            GameObject scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);
            scrollObj.layer = 5;
            
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.95f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = Color.clear;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            viewport.layer = 5;
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewportRect;
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            scroll.content = contentRect;
            
            // Grid layout
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 160);
            grid.spacing = new Vector2(5, 5);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            return scrollObj;
        }
        
        private GameObject CreateGridArea(string name, Transform parent)
        {
            GameObject gridObj = new GameObject(name);
            gridObj.transform.SetParent(parent, false);
            gridObj.layer = 5;
            
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.05f, 0.15f);
            gridRect.anchorMax = new Vector2(0.95f, 0.95f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;
            
            GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(100, 140);
            grid.spacing = new Vector2(5, 5);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            
            return gridObj;
        }
        
        private GameObject CreateTitle(string name, Transform parent, string text, int fontSize)
        {
            GameObject titleObj = new GameObject(name);
            titleObj.transform.SetParent(parent, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = text;
            titleText.fontSize = fontSize;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.1f);
            titleRect.anchorMax = new Vector2(0.6f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            return titleObj;
        }
        
        private GameObject CreateEditableButton(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            buttonObj.layer = 5;
            
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.8f, 0.8f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 12;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return buttonObj;
        }
    }
}