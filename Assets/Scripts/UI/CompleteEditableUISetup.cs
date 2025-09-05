using UnityEngine;
using UnityEngine.UI;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// COMPLETE EDITABLE UI SETUP
    /// Creates ALL editable screens with one click!
    /// </summary>
    public class CompleteEditableUISetup : MonoBehaviour
    {
        [Header("🎮 COMPLETE EDITABLE UI SYSTEM SETUP")]
        [Space(10)]
        
        [Header("✨ ONE-CLICK SETUP")]
        [Space(5)]
        [Header("👆 CHECK THIS BOX TO CREATE ALL EDITABLE SCREENS:")]
        [Tooltip("Check this to create the complete editable UI system!")]
        public bool CREATE_ALL_EDITABLE_SCREENS = false;
        
        [Space(10)]
        [Header("📋 What This Creates:")]
        [TextArea(8, 12)]
        public string explanation = "✅ Editable Authentication Screen (login/signup)\\n✅ Editable Main Menu (navigation, battle button)\\n✅ Editable Collection (card browsing, filters)\\n✅ Editable Deck Builder (deck management)\\n✅ Editable Hero Hall (hero selection)\\n✅ Editable Battle Arena (battle interface)\\n\\n🎨 EVERY SCREEN FULLY CUSTOMIZABLE:\\n• Drag/drop to reposition elements\\n• Resize panels and buttons\\n• Change colors, fonts, sizes\\n• Add/remove UI elements\\n• Real-time preview in Inspector\\n\\n🔄 REPLACES OLD SYSTEM:\\n• Disables ProductionUIManager\\n• Uses your editable screens instead\\n• Maintains all functionality\\n• Loads real data from Supabase";
        
        [Space(10)]
        [Header("📋 Status:")]
        [Tooltip("Shows if setup is complete")]
        public bool setupComplete = false;
        
        void Update()
        {
            if (CREATE_ALL_EDITABLE_SCREENS && !setupComplete)
            {
                CREATE_ALL_EDITABLE_SCREENS = false;
                CreateCompleteEditableUISystem();
            }
        }
        
        [ContextMenu("🎨 Create Complete Editable UI System")]
        public void CreateCompleteEditableUISystem()
        {
            Debug.Log("🎮 Creating COMPLETE EDITABLE UI SYSTEM...");
            
            // Disable old ProductionUIManager if it exists
            ProductionUIManager oldUI = FindObjectOfType<ProductionUIManager>();
            if (oldUI != null)
            {
                oldUI.gameObject.SetActive(false);
                Debug.Log("🔇 Disabled old ProductionUIManager");
            }
            
            // Create main UI manager
            GameObject uiManagerObj = new GameObject("🎮 EDITABLE_UI_MANAGER");
            uiManagerObj.transform.SetParent(null); // Top level
            EditableUIManager uiManager = uiManagerObj.AddComponent<EditableUIManager>();
            
            // Add AUTO-SAVE system for instant persistent UI changes
            uiManagerObj.AddComponent<AutoSaveEditableUI>();
            Debug.Log("💾 Added AUTO-SAVE UI System - changes save instantly!");
            
            // Create all editable screens
            CreateEditableAuthScreen(uiManagerObj.transform);
            CreateEditableMainMenu(uiManagerObj.transform);
            CreateEditableCollection(uiManagerObj.transform);
            CreateEditableDeckBuilder(uiManagerObj.transform);
            CreateEditableHeroHall(uiManagerObj.transform);
            CreateEditableBattleArena(uiManagerObj.transform);
            
            // Setup canvas and scaling
            SetupMainCanvas(uiManagerObj);
            
            // Mark as complete
            setupComplete = true;
            
            Debug.Log("✅ COMPLETE EDITABLE UI SYSTEM CREATED!");
            Debug.Log("🎨 Select screens in Hierarchy to edit in Inspector!");
            Debug.Log("🔄 All screens are now fully customizable!");
        }
        
        private void CreateEditableAuthScreen(Transform parent)
        {
            GameObject authObj = new GameObject("🔐 EDITABLE_AUTH_SCREEN");
            authObj.transform.SetParent(parent, false);
            authObj.layer = 5;
            
            RectTransform authRect = authObj.AddComponent<RectTransform>();
            authRect.anchorMin = Vector2.zero;
            authRect.anchorMax = Vector2.one;
            authRect.offsetMin = Vector2.zero;
            authRect.offsetMax = Vector2.zero;
            
            EditableAuthScreen authScreen = authObj.AddComponent<EditableAuthScreen>();
            
            Debug.Log("✅ Created Editable Auth Screen");
        }
        
        private void CreateEditableMainMenu(Transform parent)
        {
            GameObject mainMenuObj = new GameObject("🏠 EDITABLE_MAIN_MENU");
            mainMenuObj.transform.SetParent(parent, false);
            mainMenuObj.layer = 5;
            
            RectTransform mainMenuRect = mainMenuObj.AddComponent<RectTransform>();
            mainMenuRect.anchorMin = Vector2.zero;
            mainMenuRect.anchorMax = Vector2.one;
            mainMenuRect.offsetMin = Vector2.zero;
            mainMenuRect.offsetMax = Vector2.zero;
            
            EditableMainMenu mainMenu = mainMenuObj.AddComponent<EditableMainMenu>();
            
            Debug.Log("✅ Created Editable Main Menu");
        }
        
        private void CreateEditableCollection(Transform parent)
        {
            GameObject collectionObj = new GameObject("📚 EDITABLE_COLLECTION");
            collectionObj.transform.SetParent(parent, false);
            collectionObj.layer = 5;
            
            RectTransform collectionRect = collectionObj.AddComponent<RectTransform>();
            collectionRect.anchorMin = Vector2.zero;
            collectionRect.anchorMax = Vector2.one;
            collectionRect.offsetMin = Vector2.zero;
            collectionRect.offsetMax = Vector2.zero;
            
            EditableCollection collection = collectionObj.AddComponent<EditableCollection>();
            
            Debug.Log("✅ Created Editable Collection");
        }
        
        private void CreateEditableDeckBuilder(Transform parent)
        {
            GameObject deckBuilderObj = new GameObject("🔧 EDITABLE_DECK_BUILDER");
            deckBuilderObj.transform.SetParent(parent, false);
            deckBuilderObj.layer = 5;
            
            RectTransform deckBuilderRect = deckBuilderObj.AddComponent<RectTransform>();
            deckBuilderRect.anchorMin = Vector2.zero;
            deckBuilderRect.anchorMax = Vector2.one;
            deckBuilderRect.offsetMin = Vector2.zero;
            deckBuilderRect.offsetMax = Vector2.zero;
            
            EditableDeckBuilder deckBuilder = deckBuilderObj.AddComponent<EditableDeckBuilder>();
            
            Debug.Log("✅ Created Editable Deck Builder");
        }
        
        private void CreateEditableHeroHall(Transform parent)
        {
            GameObject heroHallObj = new GameObject("🦸 EDITABLE_HERO_HALL");
            heroHallObj.transform.SetParent(parent, false);
            heroHallObj.layer = 5;
            
            RectTransform heroHallRect = heroHallObj.AddComponent<RectTransform>();
            heroHallRect.anchorMin = Vector2.zero;
            heroHallRect.anchorMax = Vector2.one;
            heroHallRect.offsetMin = Vector2.zero;
            heroHallRect.offsetMax = Vector2.zero;
            
            EditableHeroHall heroHall = heroHallObj.AddComponent<EditableHeroHall>();
            
            Debug.Log("✅ Created Editable Hero Hall");
        }
        
        private void CreateEditableBattleArena(Transform parent)
        {
            GameObject battleObj = new GameObject("⚔️ EDITABLE_BATTLE_ARENA");
            battleObj.transform.SetParent(parent, false);
            battleObj.layer = 5;
            
            RectTransform battleRect = battleObj.AddComponent<RectTransform>();
            battleRect.anchorMin = Vector2.zero;
            battleRect.anchorMax = Vector2.one;
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;
            
            EditableBattleArena battleArena = battleObj.AddComponent<EditableBattleArena>();
            
            Debug.Log("✅ Created Editable Battle Arena");
        }
        
        private void SetupMainCanvas(GameObject uiManagerObj)
        {
            Canvas canvas = uiManagerObj.GetComponent<Canvas>();
            if (canvas == null) canvas = uiManagerObj.AddComponent<Canvas>();
            
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = uiManagerObj.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = uiManagerObj.AddComponent<CanvasScaler>();
            
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            GraphicRaycaster raycaster = uiManagerObj.GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = uiManagerObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if needed
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                
                Debug.Log("✅ Created EventSystem for editable UI");
            }
            
            Debug.Log("✅ Canvas and scaling setup for complete editable UI system");
        }
    }
}