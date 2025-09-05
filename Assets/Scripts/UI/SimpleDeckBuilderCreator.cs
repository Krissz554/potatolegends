using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// SIMPLE Deck Builder Creator - Just check the box to create!
    /// </summary>
    public class SimpleDeckBuilderCreator : MonoBehaviour
    {
        [Header("✨ SUPER SIMPLE SETUP")]
        [Space(10)]
        
        [Header("👆 CHECK THIS BOX TO CREATE DECK BUILDER:")]
        [Tooltip("Check this box and the deck builder will be created instantly!")]
        public bool CREATE_DECK_BUILDER_NOW = false;
        
        [Space(20)]
        [Header("📋 What This Does:")]
        [TextArea(5, 8)]
        public string explanation = "✅ Creates a fully editable deck builder\n✅ Shows all 226 cards from your collection\n✅ Displays your real deck with 30 cards\n✅ Loads your custom backgrounds (no overlays!)\n✅ Everything editable in Inspector\n\nAfter creation, you can:\n🎨 Drag panels to resize\n🎨 Change colors with color pickers\n🎨 Adjust card sizes and spacing\n🎨 Move elements anywhere you want";
        
        [Space(10)]
        [Header("🎯 Status:")]
        public bool deckBuilderCreated = false;
        
        void Update()
        {
            if (CREATE_DECK_BUILDER_NOW && !deckBuilderCreated)
            {
                CreateDeckBuilder();
                CREATE_DECK_BUILDER_NOW = false;
                deckBuilderCreated = true;
            }
        }
        
        void CreateDeckBuilder()
        {
            Debug.Log("🚀 Creating Simple Editable Deck Builder...");
            
            // Find or create canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("UI Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                Debug.Log("✅ Created UI Canvas");
            }
            
            // Create main deck builder container
            GameObject deckBuilder = new GameObject("🎮 EDITABLE_DECK_BUILDER");
            deckBuilder.transform.SetParent(canvas.transform, false);
            
            RectTransform mainRect = deckBuilder.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
            
            // Main background (will use your custom background)
            Image mainBg = deckBuilder.AddComponent<Image>();
            mainBg.color = new Color(0.2f, 0.1f, 0.3f, 1f); // Purple fallback
            
            // Load custom background if available
            Sprite customBg = Resources.Load<Sprite>("UI/Backgrounds/deck-builder-bg");
            if (customBg != null)
            {
                mainBg.sprite = customBg;
                mainBg.color = Color.white;
                Debug.Log("✅ Applied your custom deck builder background!");
            }
            else
            {
                Debug.Log("📝 Using fallback background - upload deck-builder-bg.png for custom background");
            }
            
            // Create top management bar
            GameObject managementBar = CreatePanel("🔧 Management Bar", deckBuilder.transform, 
                new Vector2(0.02f, 0.88f), new Vector2(0.98f, 0.98f), new Color(0.1f, 0.1f, 0.1f, 0.7f));
            
            // Load custom management bar background
            Image mgmtBg = managementBar.GetComponent<Image>();
            Sprite customMgmtBg = Resources.Load<Sprite>("UI/Backgrounds/deck-management-bar-bg");
            if (customMgmtBg != null)
            {
                mgmtBg.sprite = customMgmtBg;
                mgmtBg.color = Color.white;
                Debug.Log("✅ Applied your custom management bar background!");
            }
            
            // Create collection panel (left)
            GameObject collectionPanel = CreatePanel("📚 Collection Panel", deckBuilder.transform,
                new Vector2(0.02f, 0.15f), new Vector2(0.48f, 0.85f), new Color(0.1f, 0.2f, 0.1f, 0.3f));
            
            // Load custom collection background
            Image collectionBg = collectionPanel.GetComponent<Image>();
            Sprite customCollectionBg = Resources.Load<Sprite>("UI/Backgrounds/collection-panel-bg");
            if (customCollectionBg != null)
            {
                collectionBg.sprite = customCollectionBg;
                collectionBg.color = Color.white; // No overlay!
                Debug.Log("✅ Applied your custom collection panel background!");
            }
            
            // Create deck panel (right)
            GameObject deckPanel = CreatePanel("🃏 Deck Panel", deckBuilder.transform,
                new Vector2(0.52f, 0.15f), new Vector2(0.98f, 0.85f), new Color(0.2f, 0.1f, 0.2f, 0.3f));
            
            // Load custom deck background  
            Image deckBg = deckPanel.GetComponent<Image>();
            Sprite customDeckBg = Resources.Load<Sprite>("UI/Backgrounds/deck-panel-bg");
            if (customDeckBg != null)
            {
                deckBg.sprite = customDeckBg;
                deckBg.color = Color.white; // No overlay!
                Debug.Log("✅ Applied your custom deck panel background!");
            }
            
            // Create scroll view for collection
            GameObject scrollView = CreateScrollView("Collection Scroll", collectionPanel.transform);
            
            // Create grid for deck
            GameObject deckGrid = CreateDeckGrid("Deck Grid", deckPanel.transform);
            
            // Add titles
            CreateTitle("Management Title", managementBar.transform, "🔧 DECK BUILDER", 20, new Vector2(0.05f, 0.2f), new Vector2(0.4f, 0.8f));
            CreateTitle("Active Deck Title", managementBar.transform, "👑 Active: Loading...", 14, new Vector2(0.42f, 0.2f), new Vector2(0.68f, 0.8f));
            CreateTitle("Collection Title", collectionPanel.transform, "YOUR COLLECTION", 16, new Vector2(0.05f, 0.9f), new Vector2(0.95f, 1f));
            CreateTitle("Deck Title", deckPanel.transform, "CURRENT DECK", 16, new Vector2(0.05f, 0.9f), new Vector2(0.95f, 1f));
            
            // Create buttons
            CreateButton("Create Deck Button", managementBar.transform, "➕ New", new Vector2(0.7f, 0.2f), new Vector2(0.83f, 0.8f));
            CreateButton("Deck Selector Button", managementBar.transform, "📋 Decks", new Vector2(0.85f, 0.2f), new Vector2(0.98f, 0.8f));
            CreateButton("Back Button", deckBuilder.transform, "← Back", new Vector2(0.02f, 0.02f), new Vector2(0.15f, 0.12f));
            
            // Add the editable component
            EditableDeckBuilder editableComponent = deckBuilder.AddComponent<EditableDeckBuilder>();
            
            // Auto-assign all references
            editableComponent.managementBarArea = managementBar.GetComponent<RectTransform>();
            editableComponent.collectionPanelArea = collectionPanel.GetComponent<RectTransform>();
            editableComponent.deckPanelArea = deckPanel.GetComponent<RectTransform>();
            
            // Background sprites will be loaded from Resources - no need to assign here
            // editableComponent.managementBarBackground = managementBar.GetComponent<Image>().sprite;
            // editableComponent.collectionPanelBackground = collectionPanel.GetComponent<Image>().sprite;
            // editableComponent.deckPanelBackground = deckPanel.GetComponent<Image>().sprite;
            
            editableComponent.collectionScrollView = scrollView.GetComponent<ScrollRect>();
            editableComponent.collectionGrid = scrollView.transform.Find("Viewport/Content").GetComponent<GridLayoutGroup>();
            editableComponent.deckGrid = deckGrid.GetComponent<GridLayoutGroup>();
            
            editableComponent.titleText = managementBar.transform.Find("Management Title").GetComponent<TextMeshProUGUI>();
            editableComponent.activeDeckText = managementBar.transform.Find("Active Deck Title").GetComponent<TextMeshProUGUI>();
            editableComponent.collectionTitleText = collectionPanel.transform.Find("Collection Title").GetComponent<TextMeshProUGUI>();
            editableComponent.deckTitleText = deckPanel.transform.Find("Deck Title").GetComponent<TextMeshProUGUI>();
            
            editableComponent.createDeckButton = managementBar.transform.Find("Create Deck Button").GetComponent<Button>();
            editableComponent.deckSelectorButton = managementBar.transform.Find("Deck Selector Button").GetComponent<Button>();
            editableComponent.backButton = deckBuilder.transform.Find("Back Button").GetComponent<Button>();
            
            Debug.Log("✅ EDITABLE Deck Builder created successfully!");
            Debug.Log("🎮 Select 'EDITABLE_DECK_BUILDER' in Hierarchy to edit in Inspector!");
        }
        
        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
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
            image.color = color;
            
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
        
        private GameObject CreateDeckGrid(string name, Transform parent)
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
        
        private GameObject CreateTitle(string name, Transform parent, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
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
            titleRect.anchorMin = anchorMin;
            titleRect.anchorMax = anchorMax;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            return titleObj;
        }
        
        private GameObject CreateButton(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
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