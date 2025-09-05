using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// EDITABLE UI SAVE SYSTEM
    /// Save your UI changes made during Play mode permanently!
    /// </summary>
    public class EditableUISaveSystem : MonoBehaviour
    {
        [Header("💾 UI SAVE SYSTEM - SAVE YOUR CHANGES!")]
        [Space(10)]
        
        [Header("💾 Save Your Changes")]
        [Space(5)]
        [Header("👆 CHECK TO SAVE ALL UI CHANGES:")]
        [Tooltip("Save all current UI settings to persistent storage")]
        public bool SAVE_UI_CHANGES_NOW = false;
        
        [Space(10)]
        [Header("🔄 Load Saved Changes")]
        [Tooltip("Load previously saved UI settings")]
        public bool LOAD_SAVED_UI_CHANGES = false;
        
        [Space(10)]
        [Header("🗑️ Reset to Defaults")]
        [Tooltip("Clear all saved changes and reset to defaults")]
        public bool RESET_ALL_UI_TO_DEFAULTS = false;
        
        [Space(10)]
        [Header("📋 Save File Info")]
        [Tooltip("Shows the save file location")]
        [TextArea(3, 5)]
        public string saveFileInfo = "UI settings are saved to:\\nApplication.persistentDataPath/ui_settings.json\\n\\nThis file persists between game sessions\\nand survives Unity restarts!";
        
        [Space(10)]
        [Header("📊 Current Status")]
        [Tooltip("Shows if saved settings exist")]
        public bool hasSavedSettings = false;
        
        private string saveFilePath;
        
        void Start()
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "ui_settings.json");
            CheckForSavedSettings();
        }
        
        void Update()
        {
            if (SAVE_UI_CHANGES_NOW)
            {
                SAVE_UI_CHANGES_NOW = false;
                SaveAllUISettings();
            }
            
            if (LOAD_SAVED_UI_CHANGES)
            {
                LOAD_SAVED_UI_CHANGES = false;
                LoadAllUISettings();
            }
            
            if (RESET_ALL_UI_TO_DEFAULTS)
            {
                RESET_ALL_UI_TO_DEFAULTS = false;
                ResetAllUIToDefaults();
            }
        }
        
        private void CheckForSavedSettings()
        {
            hasSavedSettings = File.Exists(saveFilePath);
            Debug.Log($"💾 UI Save System ready. Saved settings exist: {hasSavedSettings}");
        }
        
        public void SaveAllUISettings()
        {
            Debug.Log("💾 Saving all UI settings...");
            
            UISettings settings = new UISettings();
            
            // Save Auth Screen settings
            EditableAuthScreen authScreen = FindFirstObjectByType<EditableAuthScreen>();
            if (authScreen != null)
            {
                settings.authSettings = new AuthScreenSettings
                {
                    titleText = authScreen.titleText,
                    titleFontSize = authScreen.titleFontSize,
                    titleColor = ColorToString(authScreen.titleColor),
                    formPanelColor = ColorToString(authScreen.formPanelColor),
                    buttonColor = ColorToString(authScreen.buttonColor),
                    showRememberMe = authScreen.showRememberMe,
                    showForgotPassword = authScreen.showForgotPassword
                };
            }
            
            // Save Main Menu settings
            EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
            if (mainMenu != null)
            {
                settings.mainMenuSettings = new MainMenuSettings
                {
                    welcomeMessage = mainMenu.welcomeMessage,
                    welcomeFontSize = mainMenu.welcomeFontSize,
                    welcomeColor = ColorToString(mainMenu.welcomeColor),
                    navigationCardColor = ColorToString(mainMenu.navigationCardColor),
                    battleButtonColor = ColorToString(mainMenu.battleButtonColor),
                    navigationCardsPerRow = mainMenu.navigationCardsPerRow,
                    navigationCardSize = mainMenu.navigationCardSize,
                    battleButtonSize = mainMenu.battleButtonSize,
                    settingsButtonRect = Vector4ToString(mainMenu.settingsButtonRect),
                    shopButtonRect = Vector4ToString(mainMenu.shopButtonRect),
                    logoutButtonRect = Vector4ToString(mainMenu.logoutButtonRect),
                    settingsButtonColor = ColorToString(mainMenu.settingsButtonColor),
                    shopButtonColor = ColorToString(mainMenu.shopButtonColor),
                    logoutButtonColor = ColorToString(mainMenu.logoutButtonColor)
                };
            }
            
            // Save Collection settings
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection != null)
            {
                settings.collectionSettings = new CollectionSettings
                {
                    screenTitle = collection.screenTitle,
                    titleFontSize = collection.titleFontSize,
                    titleColor = ColorToString(collection.titleColor),
                    cardWidth = collection.cardWidth,
                    cardHeight = collection.cardHeight,
                    cardsPerRow = collection.cardsPerRow,
                    cardSpacing = collection.cardSpacing,
                    maxCardsToShow = collection.maxCardsToShow
                };
            }
            
            // Save Deck Builder settings
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder != null)
            {
                settings.deckBuilderSettings = new DeckBuilderSettings
                {
                    collectionCardSize = Vector2ToString(deckBuilder.collectionCardSize),
                    deckCardSize = Vector2ToString(deckBuilder.deckCardSize),
                    collectionColumnsPerRow = deckBuilder.collectionColumnsPerRow,
                    deckColumnsPerRow = deckBuilder.deckColumnsPerRow,
                    cardTint = ColorToString(deckBuilder.cardTint)
                };
            }
            
            // Save Hero Hall settings
            EditableHeroHall heroHall = FindFirstObjectByType<EditableHeroHall>();
            if (heroHall != null)
            {
                settings.heroHallSettings = new HeroHallSettings
                {
                    screenTitle = heroHall.screenTitle,
                    titleFontSize = heroHall.titleFontSize,
                    titleColor = ColorToString(heroHall.titleColor),
                    heroCardWidth = heroHall.heroCardWidth,
                    heroCardHeight = heroHall.heroCardHeight,
                    heroesPerRow = heroHall.heroesPerRow,
                    heroSpacing = heroHall.heroSpacing
                };
            }
            
            // Save to file
            try
            {
                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(saveFilePath, json);
                hasSavedSettings = true;
                Debug.Log($"✅ UI settings saved to: {saveFilePath}");
                Debug.Log("🎮 Your changes will now persist between game sessions!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error saving UI settings: {e.Message}");
            }
        }
        
        public void LoadAllUISettings()
        {
            if (!File.Exists(saveFilePath))
            {
                Debug.LogWarning("⚠️ No saved UI settings found");
                return;
            }
            
            try
            {
                Debug.Log("📂 Loading saved UI settings...");
                string json = File.ReadAllText(saveFilePath);
                UISettings settings = JsonUtility.FromJson<UISettings>(json);
                
                // Apply Auth Screen settings
                EditableAuthScreen authScreen = FindFirstObjectByType<EditableAuthScreen>();
                if (authScreen != null && settings.authSettings != null)
                {
                    authScreen.titleText = settings.authSettings.titleText;
                    authScreen.titleFontSize = settings.authSettings.titleFontSize;
                    authScreen.titleColor = StringToColor(settings.authSettings.titleColor);
                    authScreen.formPanelColor = StringToColor(settings.authSettings.formPanelColor);
                    authScreen.buttonColor = StringToColor(settings.authSettings.buttonColor);
                    authScreen.showRememberMe = settings.authSettings.showRememberMe;
                    authScreen.showForgotPassword = settings.authSettings.showForgotPassword;
                    authScreen.RefreshScreen();
                }
                
                // Apply Main Menu settings
                EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
                if (mainMenu != null && settings.mainMenuSettings != null)
                {
                    mainMenu.welcomeMessage = settings.mainMenuSettings.welcomeMessage;
                    mainMenu.welcomeFontSize = settings.mainMenuSettings.welcomeFontSize;
                    mainMenu.welcomeColor = StringToColor(settings.mainMenuSettings.welcomeColor);
                    mainMenu.navigationCardColor = StringToColor(settings.mainMenuSettings.navigationCardColor);
                    mainMenu.battleButtonColor = StringToColor(settings.mainMenuSettings.battleButtonColor);
                    mainMenu.navigationCardsPerRow = settings.mainMenuSettings.navigationCardsPerRow;
                    mainMenu.navigationCardSize = settings.mainMenuSettings.navigationCardSize;
                    mainMenu.battleButtonSize = settings.mainMenuSettings.battleButtonSize;
                    mainMenu.settingsButtonRect = StringToVector4(settings.mainMenuSettings.settingsButtonRect);
                    mainMenu.shopButtonRect = StringToVector4(settings.mainMenuSettings.shopButtonRect);
                    mainMenu.logoutButtonRect = StringToVector4(settings.mainMenuSettings.logoutButtonRect);
                    mainMenu.settingsButtonColor = StringToColor(settings.mainMenuSettings.settingsButtonColor);
                    mainMenu.shopButtonColor = StringToColor(settings.mainMenuSettings.shopButtonColor);
                    mainMenu.logoutButtonColor = StringToColor(settings.mainMenuSettings.logoutButtonColor);
                    mainMenu.RefreshScreen();
                }
                
                // Apply Collection settings
                EditableCollection collection = FindFirstObjectByType<EditableCollection>();
                if (collection != null && settings.collectionSettings != null)
                {
                    collection.screenTitle = settings.collectionSettings.screenTitle;
                    collection.titleFontSize = settings.collectionSettings.titleFontSize;
                    collection.titleColor = StringToColor(settings.collectionSettings.titleColor);
                    collection.cardWidth = settings.collectionSettings.cardWidth;
                    collection.cardHeight = settings.collectionSettings.cardHeight;
                    collection.cardsPerRow = settings.collectionSettings.cardsPerRow;
                    collection.cardSpacing = settings.collectionSettings.cardSpacing;
                    collection.maxCardsToShow = settings.collectionSettings.maxCardsToShow;
                    collection.RefreshScreen();
                }
                
                // Apply Deck Builder settings
                EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
                if (deckBuilder != null && settings.deckBuilderSettings != null)
                {
                    deckBuilder.collectionCardSize = StringToVector2(settings.deckBuilderSettings.collectionCardSize);
                    deckBuilder.deckCardSize = StringToVector2(settings.deckBuilderSettings.deckCardSize);
                    deckBuilder.collectionColumnsPerRow = settings.deckBuilderSettings.collectionColumnsPerRow;
                    deckBuilder.deckColumnsPerRow = settings.deckBuilderSettings.deckColumnsPerRow;
                    deckBuilder.cardTint = StringToColor(settings.deckBuilderSettings.cardTint);
                    deckBuilder.RefreshScreen();
                }
                
                // Apply Hero Hall settings
                EditableHeroHall heroHall = FindFirstObjectByType<EditableHeroHall>();
                if (heroHall != null && settings.heroHallSettings != null)
                {
                    heroHall.screenTitle = settings.heroHallSettings.screenTitle;
                    heroHall.titleFontSize = settings.heroHallSettings.titleFontSize;
                    heroHall.titleColor = StringToColor(settings.heroHallSettings.titleColor);
                    heroHall.heroCardWidth = settings.heroHallSettings.heroCardWidth;
                    heroHall.heroCardHeight = settings.heroHallSettings.heroCardHeight;
                    heroHall.heroesPerRow = settings.heroHallSettings.heroesPerRow;
                    heroHall.heroSpacing = settings.heroHallSettings.heroSpacing;
                    heroHall.RefreshScreen();
                }
                
                Debug.Log("✅ All UI settings loaded from saved file!");
                Debug.Log("🎮 Your custom UI has been restored!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading UI settings: {e.Message}");
            }
        }
        
        public void ResetAllUIToDefaults()
        {
            Debug.Log("🔄 Resetting all UI to defaults...");
            
            // Delete save file
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("🗑️ Deleted saved UI settings");
            }
            
            // Reset all screens
            EditableAuthScreen authScreen = FindFirstObjectByType<EditableAuthScreen>();
            if (authScreen != null) authScreen.ResetToDefaults();
            
            EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
            if (mainMenu != null) mainMenu.ResetToDefaults();
            
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection != null) collection.ResetToDefaults();
            
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder != null) deckBuilder.ResetToDefaults();
            
            EditableHeroHall heroHall = FindFirstObjectByType<EditableHeroHall>();
            if (heroHall != null) heroHall.ResetToDefaults();
            
            EditableBattleArena battleArena = FindFirstObjectByType<EditableBattleArena>();
            if (battleArena != null) battleArena.ResetToDefaults();
            
            hasSavedSettings = false;
            Debug.Log("✅ All UI reset to defaults!");
        }
        
        // Helper methods for serialization
        private string ColorToString(Color color)
        {
            return $"{color.r},{color.g},{color.b},{color.a}";
        }
        
        private Color StringToColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString)) return Color.white;
            
            string[] parts = colorString.Split(',');
            if (parts.Length == 4)
            {
                return new Color(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]), 
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                );
            }
            return Color.white;
        }
        
        private string Vector2ToString(Vector2 vector)
        {
            return $"{vector.x},{vector.y}";
        }
        
        private Vector2 StringToVector2(string vectorString)
        {
            if (string.IsNullOrEmpty(vectorString)) return Vector2.zero;
            
            string[] parts = vectorString.Split(',');
            if (parts.Length == 2)
            {
                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            return Vector2.zero;
        }
        
        private string Vector4ToString(Vector4 vector)
        {
            return $"{vector.x},{vector.y},{vector.z},{vector.w}";
        }
        
        private Vector4 StringToVector4(string vectorString)
        {
            if (string.IsNullOrEmpty(vectorString)) return Vector4.zero;
            
            string[] parts = vectorString.Split(',');
            if (parts.Length == 4)
            {
                return new Vector4(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                );
            }
            return Vector4.zero;
        }
    }
    
    // Data classes for serialization
    [System.Serializable]
    public class UISettings
    {
        public AuthScreenSettings authSettings;
        public MainMenuSettings mainMenuSettings;
        public CollectionSettings collectionSettings;
        public DeckBuilderSettings deckBuilderSettings;
        public HeroHallSettings heroHallSettings;
    }
    
    [System.Serializable]
    public class AuthScreenSettings
    {
        public string titleText;
        public int titleFontSize;
        public string titleColor;
        public string formPanelColor;
        public string buttonColor;
        public bool showRememberMe;
        public bool showForgotPassword;
    }
    
    [System.Serializable]
    public class MainMenuSettings
    {
        public string welcomeMessage;
        public int welcomeFontSize;
        public string welcomeColor;
        public string navigationCardColor;
        public string battleButtonColor;
        public int navigationCardsPerRow;
        public float navigationCardSize;
        public float battleButtonSize;
        public string settingsButtonRect;
        public string shopButtonRect;
        public string logoutButtonRect;
        public string settingsButtonColor;
        public string shopButtonColor;
        public string logoutButtonColor;
    }
    
    [System.Serializable]
    public class CollectionSettings
    {
        public string screenTitle;
        public int titleFontSize;
        public string titleColor;
        public float cardWidth;
        public float cardHeight;
        public int cardsPerRow;
        public float cardSpacing;
        public int maxCardsToShow;
    }
    
    [System.Serializable]
    public class DeckBuilderSettings
    {
        public string collectionCardSize;
        public string deckCardSize;
        public int collectionColumnsPerRow;
        public int deckColumnsPerRow;
        public string cardTint;
    }
    
    [System.Serializable]
    public class HeroHallSettings
    {
        public string screenTitle;
        public int titleFontSize;
        public string titleColor;
        public float heroCardWidth;
        public float heroCardHeight;
        public int heroesPerRow;
        public float heroSpacing;
    }
}