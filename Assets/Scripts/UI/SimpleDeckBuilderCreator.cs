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
            
            editableComponent.managementBarBackground = managementBar.GetComponent<Image>();
            editableComponent.collectionPanelBackground = collectionPanel.GetComponent<Image>();
            editableComponent.deckPanelBackground = deckPanel.GetComponent<Image>();
            
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