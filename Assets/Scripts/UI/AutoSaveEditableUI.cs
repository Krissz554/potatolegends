using UnityEngine;
using System.Collections;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// AUTO-SAVE EDITABLE UI SYSTEM
    /// Automatically saves every UI change you make instantly!
    /// No buttons to click - just edit and changes are saved forever!
    /// </summary>
    public class AutoSaveEditableUI : MonoBehaviour
    {
        [Header("💾 AUTO-SAVE UI SYSTEM - INSTANT SAVING!")]
        [Space(10)]
        
        [Header("✨ Auto-Save Status")]
        [Tooltip("Shows if auto-save is working")]
        public bool autoSaveEnabled = true;
        
        [Tooltip("How often to check for changes (seconds)")]
        [Range(0.1f, 2f)]
        public float saveCheckInterval = 0.5f;
        
        [Tooltip("Shows last save time")]
        public string lastSaveTime = "Not saved yet";
        
        [Tooltip("Number of times settings have been auto-saved")]
        public int autoSaveCount = 0;
        
        [Space(10)]
        [Header("📊 Auto-Save Info")]
        [TextArea(4, 6)]
        public string autoSaveInfo = "AUTO-SAVE ACTIVE! Every change you make is saved instantly. No buttons to click - just edit! Changes persist between game sessions. Never lose your UI customizations. Saves every 0.5 seconds automatically.";
        
        // Previous state tracking for change detection
        private string lastAuthHash = "";
        private string lastMainMenuHash = "";
        private string lastCollectionHash = "";
        private string lastDeckBuilderHash = "";
        private string lastHeroHallHash = "";
        
        void Start()
        {
            if (autoSaveEnabled)
            {
                Debug.Log("💾 AUTO-SAVE UI SYSTEM ACTIVATED!");
                Debug.Log("🎮 Every UI change will be saved automatically!");
                
                // Start auto-save coroutine
                StartCoroutine(AutoSaveCoroutine());
                
                // Load saved settings
                LoadUIChangesOnStart();
            }
        }
        
        private IEnumerator AutoSaveCoroutine()
        {
            while (autoSaveEnabled)
            {
                yield return new WaitForSeconds(saveCheckInterval);
                
                if (HasUIChanged())
                {
                    SaveUIChangesInstantly();
                }
            }
        }
        
        private bool HasUIChanged()
        {
            // Simple hash-based change detection
            string currentAuthHash = GetAuthScreenHash();
            string currentMainMenuHash = GetMainMenuHash();
            string currentCollectionHash = GetCollectionHash();
            string currentDeckBuilderHash = GetDeckBuilderHash();
            string currentHeroHallHash = GetHeroHallHash();
            
            bool changed = currentAuthHash != lastAuthHash ||
                          currentMainMenuHash != lastMainMenuHash ||
                          currentCollectionHash != lastCollectionHash ||
                          currentDeckBuilderHash != lastDeckBuilderHash ||
                          currentHeroHallHash != lastHeroHallHash;
            
            if (changed)
            {
                lastAuthHash = currentAuthHash;
                lastMainMenuHash = currentMainMenuHash;
                lastCollectionHash = currentCollectionHash;
                lastDeckBuilderHash = currentDeckBuilderHash;
                lastHeroHallHash = currentHeroHallHash;
            }
            
            return changed;
        }
        
        private string GetAuthScreenHash()
        {
            EditableAuthScreen auth = FindFirstObjectByType<EditableAuthScreen>();
            if (auth == null) return "";
            
            return $"{auth.titleFontSize}_{auth.titleColor}_{auth.formPanelColor}_{auth.buttonColor}_{auth.titleText}";
        }
        
        private string GetMainMenuHash()
        {
            EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
            if (mainMenu == null) return "";
            
            return $"{mainMenu.welcomeFontSize}_{mainMenu.welcomeColor}_{mainMenu.navigationCardColor}_{mainMenu.battleButtonColor}_{mainMenu.settingsButtonRect}_{mainMenu.shopButtonRect}_{mainMenu.logoutButtonRect}";
        }
        
        private string GetCollectionHash()
        {
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection == null) return "";
            
            return $"{collection.cardWidth}_{collection.cardHeight}_{collection.cardsPerRow}_{collection.titleFontSize}";
        }
        
        private string GetDeckBuilderHash()
        {
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder == null) return "";
            
            return $"{deckBuilder.collectionCardSize}_{deckBuilder.deckCardSize}_{deckBuilder.collectionColumnsPerRow}_{deckBuilder.deckColumnsPerRow}";
        }
        
        private string GetHeroHallHash()
        {
            EditableHeroHall heroHall = FindFirstObjectByType<EditableHeroHall>();
            if (heroHall == null) return "";
            
            return $"{heroHall.heroCardWidth}_{heroHall.heroCardHeight}_{heroHall.heroesPerRow}_{heroHall.titleFontSize}";
        }
        
        private void SaveUIChangesInstantly()
        {
            // Save to PlayerPrefs for instant persistence
            SaveToPlayerPrefs();
            
            // Update last save time
            lastSaveTime = System.DateTime.Now.ToString("HH:mm:ss");
            autoSaveCount++;
            
            Debug.Log($"💾 AUTO-SAVED UI changes #{autoSaveCount} at {lastSaveTime}");
        }
        
        private void SaveToPlayerPrefs()
        {
            // Save Auth Screen
            EditableAuthScreen authScreen = FindFirstObjectByType<EditableAuthScreen>();
            if (authScreen != null)
            {
                PlayerPrefs.SetString("UI_Auth_TitleText", authScreen.titleText);
                PlayerPrefs.SetInt("UI_Auth_TitleFontSize", authScreen.titleFontSize);
                PlayerPrefs.SetString("UI_Auth_TitleColor", ColorToString(authScreen.titleColor));
                PlayerPrefs.SetString("UI_Auth_FormPanelColor", ColorToString(authScreen.formPanelColor));
                PlayerPrefs.SetString("UI_Auth_ButtonColor", ColorToString(authScreen.buttonColor));
            }
            
            // Save Main Menu
            EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
            if (mainMenu != null)
            {
                PlayerPrefs.SetString("UI_MainMenu_WelcomeMessage", mainMenu.welcomeMessage);
                PlayerPrefs.SetInt("UI_MainMenu_WelcomeFontSize", mainMenu.welcomeFontSize);
                PlayerPrefs.SetString("UI_MainMenu_WelcomeColor", ColorToString(mainMenu.welcomeColor));
                PlayerPrefs.SetString("UI_MainMenu_NavigationCardColor", ColorToString(mainMenu.navigationCardColor));
                PlayerPrefs.SetString("UI_MainMenu_BattleButtonColor", ColorToString(mainMenu.battleButtonColor));
                PlayerPrefs.SetInt("UI_MainMenu_NavigationCardsPerRow", mainMenu.navigationCardsPerRow);
                PlayerPrefs.SetFloat("UI_MainMenu_NavigationCardSize", mainMenu.navigationCardSize);
                PlayerPrefs.SetString("UI_MainMenu_SettingsButtonRect", Vector4ToString(mainMenu.settingsButtonRect));
                PlayerPrefs.SetString("UI_MainMenu_ShopButtonRect", Vector4ToString(mainMenu.shopButtonRect));
                PlayerPrefs.SetString("UI_MainMenu_LogoutButtonRect", Vector4ToString(mainMenu.logoutButtonRect));
                PlayerPrefs.SetString("UI_MainMenu_SettingsButtonColor", ColorToString(mainMenu.settingsButtonColor));
                PlayerPrefs.SetString("UI_MainMenu_ShopButtonColor", ColorToString(mainMenu.shopButtonColor));
                PlayerPrefs.SetString("UI_MainMenu_LogoutButtonColor", ColorToString(mainMenu.logoutButtonColor));
            }
            
            // Save Collection
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection != null)
            {
                PlayerPrefs.SetString("UI_Collection_ScreenTitle", collection.screenTitle);
                PlayerPrefs.SetInt("UI_Collection_TitleFontSize", collection.titleFontSize);
                PlayerPrefs.SetString("UI_Collection_TitleColor", ColorToString(collection.titleColor));
                PlayerPrefs.SetFloat("UI_Collection_CardWidth", collection.cardWidth);
                PlayerPrefs.SetFloat("UI_Collection_CardHeight", collection.cardHeight);
                PlayerPrefs.SetInt("UI_Collection_CardsPerRow", collection.cardsPerRow);
                PlayerPrefs.SetFloat("UI_Collection_CardSpacing", collection.cardSpacing);
            }
            
            // Save Deck Builder
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder != null)
            {
                PlayerPrefs.SetString("UI_DeckBuilder_CollectionCardSize", Vector2ToString(deckBuilder.collectionCardSize));
                PlayerPrefs.SetString("UI_DeckBuilder_DeckCardSize", Vector2ToString(deckBuilder.deckCardSize));
                PlayerPrefs.SetInt("UI_DeckBuilder_CollectionColumnsPerRow", deckBuilder.collectionColumnsPerRow);
                PlayerPrefs.SetInt("UI_DeckBuilder_DeckColumnsPerRow", deckBuilder.deckColumnsPerRow);
                PlayerPrefs.SetString("UI_DeckBuilder_CardTint", ColorToString(deckBuilder.cardTint));
            }
            
            // Save Hero Hall
            EditableHeroHall heroHall = FindFirstObjectByType<EditableHeroHall>();
            if (heroHall != null)
            {
                PlayerPrefs.SetString("UI_HeroHall_ScreenTitle", heroHall.screenTitle);
                PlayerPrefs.SetInt("UI_HeroHall_TitleFontSize", heroHall.titleFontSize);
                PlayerPrefs.SetString("UI_HeroHall_TitleColor", ColorToString(heroHall.titleColor));
                PlayerPrefs.SetFloat("UI_HeroHall_HeroCardWidth", heroHall.heroCardWidth);
                PlayerPrefs.SetFloat("UI_HeroHall_HeroCardHeight", heroHall.heroCardHeight);
                PlayerPrefs.SetInt("UI_HeroHall_HeroesPerRow", heroHall.heroesPerRow);
                PlayerPrefs.SetFloat("UI_HeroHall_HeroSpacing", heroHall.heroSpacing);
            }
            
            PlayerPrefs.Save(); // Force immediate save to disk
        }
        
        public void LoadUIChangesOnStart()
        {
            Debug.Log("📂 Loading auto-saved UI changes...");
            
            // Load Auth Screen
            EditableAuthScreen authScreen = FindFirstObjectByType<EditableAuthScreen>();
            if (authScreen != null && PlayerPrefs.HasKey("UI_Auth_TitleText"))
            {
                authScreen.titleText = PlayerPrefs.GetString("UI_Auth_TitleText", authScreen.titleText);
                authScreen.titleFontSize = PlayerPrefs.GetInt("UI_Auth_TitleFontSize", authScreen.titleFontSize);
                authScreen.titleColor = StringToColor(PlayerPrefs.GetString("UI_Auth_TitleColor", ColorToString(authScreen.titleColor)));
                authScreen.formPanelColor = StringToColor(PlayerPrefs.GetString("UI_Auth_FormPanelColor", ColorToString(authScreen.formPanelColor)));
                authScreen.buttonColor = StringToColor(PlayerPrefs.GetString("UI_Auth_ButtonColor", ColorToString(authScreen.buttonColor)));
                Debug.Log("📂 Loaded auth screen settings");
            }
            
            // Load Main Menu
            EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
            if (mainMenu != null && PlayerPrefs.HasKey("UI_MainMenu_WelcomeMessage"))
            {
                mainMenu.welcomeMessage = PlayerPrefs.GetString("UI_MainMenu_WelcomeMessage", mainMenu.welcomeMessage);
                mainMenu.welcomeFontSize = PlayerPrefs.GetInt("UI_MainMenu_WelcomeFontSize", mainMenu.welcomeFontSize);
                mainMenu.welcomeColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_WelcomeColor", ColorToString(mainMenu.welcomeColor)));
                mainMenu.navigationCardColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_NavigationCardColor", ColorToString(mainMenu.navigationCardColor)));
                mainMenu.battleButtonColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_BattleButtonColor", ColorToString(mainMenu.battleButtonColor)));
                mainMenu.navigationCardsPerRow = PlayerPrefs.GetInt("UI_MainMenu_NavigationCardsPerRow", mainMenu.navigationCardsPerRow);
                mainMenu.navigationCardSize = PlayerPrefs.GetFloat("UI_MainMenu_NavigationCardSize", mainMenu.navigationCardSize);
                mainMenu.settingsButtonRect = StringToVector4(PlayerPrefs.GetString("UI_MainMenu_SettingsButtonRect", Vector4ToString(mainMenu.settingsButtonRect)));
                mainMenu.shopButtonRect = StringToVector4(PlayerPrefs.GetString("UI_MainMenu_ShopButtonRect", Vector4ToString(mainMenu.shopButtonRect)));
                mainMenu.logoutButtonRect = StringToVector4(PlayerPrefs.GetString("UI_MainMenu_LogoutButtonRect", Vector4ToString(mainMenu.logoutButtonRect)));
                mainMenu.settingsButtonColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_SettingsButtonColor", ColorToString(mainMenu.settingsButtonColor)));
                mainMenu.shopButtonColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_ShopButtonColor", ColorToString(mainMenu.shopButtonColor)));
                mainMenu.logoutButtonColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_LogoutButtonColor", ColorToString(mainMenu.logoutButtonColor)));
                Debug.Log("📂 Loaded main menu settings");
            }
            
            // Load Collection
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection != null && PlayerPrefs.HasKey("UI_Collection_ScreenTitle"))
            {
                collection.screenTitle = PlayerPrefs.GetString("UI_Collection_ScreenTitle", collection.screenTitle);
                collection.titleFontSize = PlayerPrefs.GetInt("UI_Collection_TitleFontSize", collection.titleFontSize);
                collection.titleColor = StringToColor(PlayerPrefs.GetString("UI_Collection_TitleColor", ColorToString(collection.titleColor)));
                collection.cardWidth = PlayerPrefs.GetFloat("UI_Collection_CardWidth", collection.cardWidth);
                collection.cardHeight = PlayerPrefs.GetFloat("UI_Collection_CardHeight", collection.cardHeight);
                collection.cardsPerRow = PlayerPrefs.GetInt("UI_Collection_CardsPerRow", collection.cardsPerRow);
                collection.cardSpacing = PlayerPrefs.GetFloat("UI_Collection_CardSpacing", collection.cardSpacing);
                Debug.Log("📂 Loaded collection settings");
            }
            
            // Load Deck Builder
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder != null && PlayerPrefs.HasKey("UI_DeckBuilder_CollectionCardSize"))
            {
                deckBuilder.collectionCardSize = StringToVector2(PlayerPrefs.GetString("UI_DeckBuilder_CollectionCardSize", Vector2ToString(deckBuilder.collectionCardSize)));
                deckBuilder.deckCardSize = StringToVector2(PlayerPrefs.GetString("UI_DeckBuilder_DeckCardSize", Vector2ToString(deckBuilder.deckCardSize)));
                deckBuilder.collectionColumnsPerRow = PlayerPrefs.GetInt("UI_DeckBuilder_CollectionColumnsPerRow", deckBuilder.collectionColumnsPerRow);
                deckBuilder.deckColumnsPerRow = PlayerPrefs.GetInt("UI_DeckBuilder_DeckColumnsPerRow", deckBuilder.deckColumnsPerRow);
                deckBuilder.cardTint = StringToColor(PlayerPrefs.GetString("UI_DeckBuilder_CardTint", ColorToString(deckBuilder.cardTint)));
                Debug.Log("📂 Loaded deck builder settings");
            }
            
            // Load Hero Hall
            EditableHeroHall heroHall = FindFirstObjectByType<EditableHeroHall>();
            if (heroHall != null && PlayerPrefs.HasKey("UI_HeroHall_ScreenTitle"))
            {
                heroHall.screenTitle = PlayerPrefs.GetString("UI_HeroHall_ScreenTitle", heroHall.screenTitle);
                heroHall.titleFontSize = PlayerPrefs.GetInt("UI_HeroHall_TitleFontSize", heroHall.titleFontSize);
                heroHall.titleColor = StringToColor(PlayerPrefs.GetString("UI_HeroHall_TitleColor", ColorToString(heroHall.titleColor)));
                heroHall.heroCardWidth = PlayerPrefs.GetFloat("UI_HeroHall_HeroCardWidth", heroHall.heroCardWidth);
                heroHall.heroCardHeight = PlayerPrefs.GetFloat("UI_HeroHall_HeroCardHeight", heroHall.heroCardHeight);
                heroHall.heroesPerRow = PlayerPrefs.GetInt("UI_HeroHall_HeroesPerRow", heroHall.heroesPerRow);
                heroHall.heroSpacing = PlayerPrefs.GetFloat("UI_HeroHall_HeroSpacing", heroHall.heroSpacing);
                Debug.Log("📂 Loaded hero hall settings");
            }
            
            Debug.Log("✅ All auto-saved UI changes loaded!");
        }
        
        private void SaveUIChangesInstantly()
        {
            SaveToPlayerPrefs();
            lastSaveTime = System.DateTime.Now.ToString("HH:mm:ss");
            autoSaveCount++;
            Debug.Log($"💾 AUTO-SAVED UI changes #{autoSaveCount} at {lastSaveTime}");
        }
        
        private void SaveToPlayerPrefs()
        {
            // Save Auth Screen
            EditableAuthScreen authScreen = FindFirstObjectByType<EditableAuthScreen>();
            if (authScreen != null)
            {
                PlayerPrefs.SetString("UI_Auth_TitleText", authScreen.titleText);
                PlayerPrefs.SetInt("UI_Auth_TitleFontSize", authScreen.titleFontSize);
                PlayerPrefs.SetString("UI_Auth_TitleColor", ColorToString(authScreen.titleColor));
                PlayerPrefs.SetString("UI_Auth_FormPanelColor", ColorToString(authScreen.formPanelColor));
                PlayerPrefs.SetString("UI_Auth_ButtonColor", ColorToString(authScreen.buttonColor));
            }
            
            // Save Main Menu
            EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
            if (mainMenu != null)
            {
                PlayerPrefs.SetString("UI_MainMenu_WelcomeMessage", mainMenu.welcomeMessage);
                PlayerPrefs.SetInt("UI_MainMenu_WelcomeFontSize", mainMenu.welcomeFontSize);
                PlayerPrefs.SetString("UI_MainMenu_WelcomeColor", ColorToString(mainMenu.welcomeColor));
                PlayerPrefs.SetString("UI_MainMenu_NavigationCardColor", ColorToString(mainMenu.navigationCardColor));
                PlayerPrefs.SetString("UI_MainMenu_BattleButtonColor", ColorToString(mainMenu.battleButtonColor));
                PlayerPrefs.SetInt("UI_MainMenu_NavigationCardsPerRow", mainMenu.navigationCardsPerRow);
                PlayerPrefs.SetFloat("UI_MainMenu_NavigationCardSize", mainMenu.navigationCardSize);
                PlayerPrefs.SetString("UI_MainMenu_SettingsButtonRect", Vector4ToString(mainMenu.settingsButtonRect));
                PlayerPrefs.SetString("UI_MainMenu_ShopButtonRect", Vector4ToString(mainMenu.shopButtonRect));
                PlayerPrefs.SetString("UI_MainMenu_LogoutButtonRect", Vector4ToString(mainMenu.logoutButtonRect));
                PlayerPrefs.SetString("UI_MainMenu_SettingsButtonColor", ColorToString(mainMenu.settingsButtonColor));
                PlayerPrefs.SetString("UI_MainMenu_ShopButtonColor", ColorToString(mainMenu.shopButtonColor));
                PlayerPrefs.SetString("UI_MainMenu_LogoutButtonColor", ColorToString(mainMenu.logoutButtonColor));
            }
            
            // Save Collection
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection != null)
            {
                PlayerPrefs.SetString("UI_Collection_ScreenTitle", collection.screenTitle);
                PlayerPrefs.SetInt("UI_Collection_TitleFontSize", collection.titleFontSize);
                PlayerPrefs.SetString("UI_Collection_TitleColor", ColorToString(collection.titleColor));
                PlayerPrefs.SetFloat("UI_Collection_CardWidth", collection.cardWidth);
                PlayerPrefs.SetFloat("UI_Collection_CardHeight", collection.cardHeight);
                PlayerPrefs.SetInt("UI_Collection_CardsPerRow", collection.cardsPerRow);
                PlayerPrefs.SetFloat("UI_Collection_CardSpacing", collection.cardSpacing);
            }
            
            // Save Deck Builder
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder != null)
            {
                PlayerPrefs.SetString("UI_DeckBuilder_CollectionCardSize", Vector2ToString(deckBuilder.collectionCardSize));
                PlayerPrefs.SetString("UI_DeckBuilder_DeckCardSize", Vector2ToString(deckBuilder.deckCardSize));
                PlayerPrefs.SetInt("UI_DeckBuilder_CollectionColumnsPerRow", deckBuilder.collectionColumnsPerRow);
                PlayerPrefs.SetInt("UI_DeckBuilder_DeckColumnsPerRow", deckBuilder.deckColumnsPerRow);
                PlayerPrefs.SetString("UI_DeckBuilder_CardTint", ColorToString(deckBuilder.cardTint));
            }
            
            // Save Hero Hall
            EditableHeroHall heroHall = FindFirstObjectByType<EditableHeroHall>();
            if (heroHall != null)
            {
                PlayerPrefs.SetString("UI_HeroHall_ScreenTitle", heroHall.screenTitle);
                PlayerPrefs.SetInt("UI_HeroHall_TitleFontSize", heroHall.titleFontSize);
                PlayerPrefs.SetString("UI_HeroHall_TitleColor", ColorToString(heroHall.titleColor));
                PlayerPrefs.SetFloat("UI_HeroHall_HeroCardWidth", heroHall.heroCardWidth);
                PlayerPrefs.SetFloat("UI_HeroHall_HeroCardHeight", heroHall.heroCardHeight);
                PlayerPrefs.SetInt("UI_HeroHall_HeroesPerRow", heroHall.heroesPerRow);
                PlayerPrefs.SetFloat("UI_HeroHall_HeroSpacing", heroHall.heroSpacing);
            }
            
            PlayerPrefs.Save();
        }
        
        // Helper methods
        private string ColorToString(Color color)
        {
            return $"{color.r:F3},{color.g:F3},{color.b:F3},{color.a:F3}";
        }
        
        private Color StringToColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString)) return Color.white;
            
            try
            {
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
            }
            catch
            {
                Debug.LogWarning($"Could not parse color: {colorString}");
            }
            
            return Color.white;
        }
        
        private string Vector2ToString(Vector2 vector)
        {
            return $"{vector.x:F1},{vector.y:F1}";
        }
        
        private Vector2 StringToVector2(string vectorString)
        {
            if (string.IsNullOrEmpty(vectorString)) return Vector2.zero;
            
            try
            {
                string[] parts = vectorString.Split(',');
                if (parts.Length == 2)
                {
                    return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
                }
            }
            catch
            {
                Debug.LogWarning($"Could not parse Vector2: {vectorString}");
            }
            
            return Vector2.zero;
        }
        
        private string Vector4ToString(Vector4 vector)
        {
            return $"{vector.x:F2},{vector.y:F2},{vector.z:F2},{vector.w:F2}";
        }
        
        private Vector4 StringToVector4(string vectorString)
        {
            if (string.IsNullOrEmpty(vectorString)) return Vector4.zero;
            
            try
            {
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
            }
            catch
            {
                Debug.LogWarning($"Could not parse Vector4: {vectorString}");
            }
            
            return Vector4.zero;
        }
    }
}