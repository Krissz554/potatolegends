using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// MASTER EDITABLE UI MANAGER - Controls all editable screens
    /// Every screen is fully customizable in Unity Inspector!
    /// Drag, resize, reposition, style everything without code changes!
    /// </summary>
    public class EditableUIManager : MonoBehaviour
    {
        [Header("🎮 MASTER UI CONTROL - EDIT EVERYTHING IN INSPECTOR!")]
        [Space(10)]
        
        [Header("📱 Screen Management")]
        [Tooltip("Current active screen")]
        public EditableScreen currentScreen = EditableScreen.Auth;
        
        [Space(10)]
        [Header("🔧 Screen References - Drag from Hierarchy")]
        [Tooltip("Editable Authentication Screen")]
        public EditableAuthScreen authScreen;
        
        [Tooltip("Editable Main Menu Screen")]
        public EditableMainMenu mainMenuScreen;
        
        [Tooltip("Editable Collection Screen")]
        public EditableCollection collectionScreen;
        
        [Tooltip("Editable Deck Builder Screen")]
        public EditableDeckBuilder deckBuilderScreen;
        
        [Tooltip("Editable Hero Hall Screen")]
        public EditableHeroHall heroHallScreen;
        
        [Tooltip("Editable Battle Arena Screen")]
        public EditableBattleArena battleArenaScreen;
        
        [Space(10)]
        [Header("🎨 Global UI Settings")]
        [Range(0.5f, 2f)]
        [Tooltip("Global UI scale multiplier")]
        public float globalUIScale = 1f;
        
        [Tooltip("Global UI color tint")]
        public Color globalColorTint = Color.white;
        
        [Space(10)]
        [Header("📱 Mobile Settings")]
        [Tooltip("Enable safe area support")]
        public bool enableSafeArea = true;
        
        [Tooltip("Mobile scaling mode")]
        public CanvasScaler.ScaleMode mobileScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        [Space(10)]
        [Header("🔄 Quick Actions")]
        [Tooltip("Refresh all screens with current settings")]
        public bool refreshAllScreens = false;
        
        [Tooltip("Reset all screens to default positions")]
        public bool resetAllScreens = false;
        
        public enum EditableScreen
        {
            Auth,
            MainMenu,
            Collection,
            DeckBuilder,
            HeroHall,
            BattleArena
        }
        
        void Start()
        {
            InitializeEditableUISystem();
        }
        
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (refreshAllScreens)
                {
                    refreshAllScreens = false;
                    RefreshAllScreens();
                }
                
                if (resetAllScreens)
                {
                    resetAllScreens = false;
                    ResetAllScreens();
                }
                
                ApplyGlobalSettings();
            }
        }
        
        private void InitializeEditableUISystem()
        {
            Debug.Log("🎮 Initializing COMPLETE EDITABLE UI SYSTEM...");
            
            // Setup mobile scaling
            SetupMobileScaling();
            
            // Initialize all screens but hide them
            InitializeAllScreens();
            
            // Show the starting screen
            ShowScreen(currentScreen);
            
            Debug.Log("✅ Complete Editable UI System ready! Edit everything in Inspector!");
        }
        
        private void SetupMobileScaling()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            
            scaler.uiScaleMode = mobileScaleMode;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = gameObject.AddComponent<GraphicRaycaster>();
            
            Debug.Log("📱 Mobile scaling setup for editable UI");
        }
        
        private void InitializeAllScreens()
        {
            // Find or create all editable screens
            if (authScreen == null) authScreen = FindOrCreateEditableScreen<EditableAuthScreen>("🔐 EDITABLE_AUTH_SCREEN");
            if (mainMenuScreen == null) mainMenuScreen = FindOrCreateEditableScreen<EditableMainMenu>("🏠 EDITABLE_MAIN_MENU");
            if (collectionScreen == null) collectionScreen = FindOrCreateEditableScreen<EditableCollection>("📚 EDITABLE_COLLECTION");
            if (deckBuilderScreen == null) deckBuilderScreen = FindOrCreateEditableScreen<EditableDeckBuilder>("🔧 EDITABLE_DECK_BUILDER");
            if (heroHallScreen == null) heroHallScreen = FindOrCreateEditableScreen<EditableHeroHall>("🦸 EDITABLE_HERO_HALL");
            if (battleArenaScreen == null) battleArenaScreen = FindOrCreateEditableScreen<EditableBattleArena>("⚔️ EDITABLE_BATTLE_ARENA");
            
            // Hide all screens initially
            HideAllScreens();
        }
        
        private T FindOrCreateEditableScreen<T>(string screenName) where T : MonoBehaviour
        {
            // Try to find existing screen
            T existingScreen = FindObjectOfType<T>();
            if (existingScreen != null) return existingScreen;
            
            // Create new screen
            GameObject screenObj = new GameObject(screenName);
            screenObj.transform.SetParent(transform, false);
            screenObj.layer = 5;
            
            // Add RectTransform for UI
            RectTransform screenRect = screenObj.AddComponent<RectTransform>();
            screenRect.anchorMin = Vector2.zero;
            screenRect.anchorMax = Vector2.one;
            screenRect.offsetMin = Vector2.zero;
            screenRect.offsetMax = Vector2.zero;
            
            // Add the editable screen component
            T screenComponent = screenObj.AddComponent<T>();
            
            Debug.Log($"✅ Created editable screen: {screenName}");
            return screenComponent;
        }
        
        public void ShowScreen(EditableScreen screen)
        {
            Debug.Log($"🔄 Switching to editable screen: {screen}");
            
            // Hide all screens
            HideAllScreens();
            
            // Show requested screen
            switch (screen)
            {
                case EditableScreen.Auth:
                    if (authScreen != null) authScreen.gameObject.SetActive(true);
                    break;
                case EditableScreen.MainMenu:
                    if (mainMenuScreen != null) mainMenuScreen.gameObject.SetActive(true);
                    break;
                case EditableScreen.Collection:
                    if (collectionScreen != null) collectionScreen.gameObject.SetActive(true);
                    break;
                case EditableScreen.DeckBuilder:
                    if (deckBuilderScreen != null) deckBuilderScreen.gameObject.SetActive(true);
                    break;
                case EditableScreen.HeroHall:
                    if (heroHallScreen != null) heroHallScreen.gameObject.SetActive(true);
                    break;
                case EditableScreen.BattleArena:
                    if (battleArenaScreen != null) battleArenaScreen.gameObject.SetActive(true);
                    break;
            }
            
            currentScreen = screen;
            
            // Disable ProductionUIManager if it exists
            ProductionUIManager oldUI = FindObjectOfType<ProductionUIManager>();
            if (oldUI != null)
            {
                oldUI.gameObject.SetActive(false);
                Debug.Log("🔇 Disabled old ProductionUIManager - using your editable UI!");
            }
        }
        
        private void HideAllScreens()
        {
            if (authScreen != null) authScreen.gameObject.SetActive(false);
            if (mainMenuScreen != null) mainMenuScreen.gameObject.SetActive(false);
            if (collectionScreen != null) collectionScreen.gameObject.SetActive(false);
            if (deckBuilderScreen != null) deckBuilderScreen.gameObject.SetActive(false);
            if (heroHallScreen != null) heroHallScreen.gameObject.SetActive(false);
            if (battleArenaScreen != null) battleArenaScreen.gameObject.SetActive(false);
        }
        
        private void RefreshAllScreens()
        {
            Debug.Log("🔄 Refreshing all editable screens...");
            
            if (authScreen != null) authScreen.RefreshScreen();
            if (mainMenuScreen != null) mainMenuScreen.RefreshScreen();
            if (collectionScreen != null) collectionScreen.RefreshScreen();
            if (deckBuilderScreen != null) deckBuilderScreen.RefreshScreen();
            if (heroHallScreen != null) heroHallScreen.RefreshScreen();
            if (battleArenaScreen != null) battleArenaScreen.RefreshScreen();
            
            Debug.Log("✅ All screens refreshed with current settings!");
        }
        
        private void ResetAllScreens()
        {
            Debug.Log("🔄 Resetting all screens to default positions...");
            
            // Reset each screen to default layout
            if (authScreen != null) authScreen.ResetToDefaults();
            if (mainMenuScreen != null) mainMenuScreen.ResetToDefaults();
            if (collectionScreen != null) collectionScreen.ResetToDefaults();
            if (deckBuilderScreen != null) deckBuilderScreen.ResetToDefaults();
            if (heroHallScreen != null) heroHallScreen.ResetToDefaults();
            if (battleArenaScreen != null) battleArenaScreen.ResetToDefaults();
            
            Debug.Log("✅ All screens reset to defaults!");
        }
        
        private void ApplyGlobalSettings()
        {
            // Apply global scale
            transform.localScale = Vector3.one * globalUIScale;
            
            // Apply global color tint to all screens
            ApplyGlobalColorTint();
        }
        
        private void ApplyGlobalColorTint()
        {
            Image[] allImages = GetComponentsInChildren<Image>();
            foreach (Image img in allImages)
            {
                if (img.color != Color.clear) // Don't tint transparent images
                {
                    img.color = img.color * globalColorTint;
                }
            }
        }
        
        // Public methods for navigation (called by buttons)
        public void GoToAuth() => ShowScreen(EditableScreen.Auth);
        public void GoToMainMenu() => ShowScreen(EditableScreen.MainMenu);
        public void GoToCollection() => ShowScreen(EditableScreen.Collection);
        public void GoToDeckBuilder() => ShowScreen(EditableScreen.DeckBuilder);
        public void GoToHeroHall() => ShowScreen(EditableScreen.HeroHall);
        public void GoToBattleArena() => ShowScreen(EditableScreen.BattleArena);
    }
}