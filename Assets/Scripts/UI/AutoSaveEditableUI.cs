using UnityEngine;
using System.Collections;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// AUTO-SAVE EDITABLE UI SYSTEM
    /// Automatically saves every UI change you make instantly!
    /// </summary>
    public class AutoSaveEditableUI : MonoBehaviour
    {
        [Header("💾 AUTO-SAVE UI SYSTEM")]
        [Space(10)]
        
        [Header("✨ Auto-Save Status")]
        public bool autoSaveEnabled = true;
        
        [Range(0.1f, 2f)]
        public float saveCheckInterval = 0.5f;
        
        public string lastSaveTime = "Not saved yet";
        public int autoSaveCount = 0;
        
        [Space(10)]
        [Header("📊 Auto-Save Info")]
        [TextArea(3, 4)]
        public string autoSaveInfo = "AUTO-SAVE ACTIVE! Every change saves automatically every 0.5 seconds. No buttons needed!";
        
        // Change detection hashes
        private string lastAuthHash = "";
        private string lastMainMenuHash = "";
        private string lastCollectionHash = "";
        private string lastDeckBuilderHash = "";
        
        void Start()
        {
            if (autoSaveEnabled)
            {
                Debug.Log("💾 AUTO-SAVE UI SYSTEM ACTIVATED!");
                StartCoroutine(AutoSaveCoroutine());
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
            string currentAuthHash = GetAuthHash();
            string currentMainMenuHash = GetMainMenuHash();
            string currentCollectionHash = GetCollectionHash();
            string currentDeckBuilderHash = GetDeckBuilderHash();
            
            bool changed = currentAuthHash != lastAuthHash ||
                          currentMainMenuHash != lastMainMenuHash ||
                          currentCollectionHash != lastCollectionHash ||
                          currentDeckBuilderHash != lastDeckBuilderHash;
            
            if (changed)
            {
                lastAuthHash = currentAuthHash;
                lastMainMenuHash = currentMainMenuHash;
                lastCollectionHash = currentCollectionHash;
                lastDeckBuilderHash = currentDeckBuilderHash;
            }
            
            return changed;
        }
        
        private string GetAuthHash()
        {
            EditableAuthScreen auth = FindFirstObjectByType<EditableAuthScreen>();
            if (auth == null) return "";
            return $"{auth.titleFontSize}_{auth.titleColor}_{auth.titleText}";
        }
        
        private string GetMainMenuHash()
        {
            EditableMainMenu mainMenu = FindFirstObjectByType<EditableMainMenu>();
            if (mainMenu == null) return "";
            return $"{mainMenu.welcomeFontSize}_{mainMenu.welcomeColor}_{mainMenu.settingsButtonRect}_{mainMenu.shopButtonRect}_{mainMenu.logoutButtonRect}";
        }
        
        private string GetCollectionHash()
        {
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection == null) return "";
            return $"{collection.cardWidth}_{collection.cardHeight}_{collection.cardsPerRow}";
        }
        
        private string GetDeckBuilderHash()
        {
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder == null) return "";
            return $"{deckBuilder.collectionCardSize}_{deckBuilder.deckCardSize}_{deckBuilder.collectionColumnsPerRow}";
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
            }
            
            PlayerPrefs.Save();
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
                mainMenu.settingsButtonRect = StringToVector4(PlayerPrefs.GetString("UI_MainMenu_SettingsButtonRect", Vector4ToString(mainMenu.settingsButtonRect)));
                mainMenu.shopButtonRect = StringToVector4(PlayerPrefs.GetString("UI_MainMenu_ShopButtonRect", Vector4ToString(mainMenu.shopButtonRect)));
                mainMenu.logoutButtonRect = StringToVector4(PlayerPrefs.GetString("UI_MainMenu_LogoutButtonRect", Vector4ToString(mainMenu.logoutButtonRect)));
                mainMenu.settingsButtonColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_SettingsButtonColor", ColorToString(mainMenu.settingsButtonColor)));
                mainMenu.shopButtonColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_ShopButtonColor", ColorToString(mainMenu.shopButtonColor)));
                mainMenu.logoutButtonColor = StringToColor(PlayerPrefs.GetString("UI_MainMenu_LogoutButtonColor", ColorToString(mainMenu.logoutButtonColor)));
            }
            
            // Load Collection
            EditableCollection collection = FindFirstObjectByType<EditableCollection>();
            if (collection != null && PlayerPrefs.HasKey("UI_Collection_CardWidth"))
            {
                collection.cardWidth = PlayerPrefs.GetFloat("UI_Collection_CardWidth", collection.cardWidth);
                collection.cardHeight = PlayerPrefs.GetFloat("UI_Collection_CardHeight", collection.cardHeight);
                collection.cardsPerRow = PlayerPrefs.GetInt("UI_Collection_CardsPerRow", collection.cardsPerRow);
                collection.cardSpacing = PlayerPrefs.GetFloat("UI_Collection_CardSpacing", collection.cardSpacing);
            }
            
            // Load Deck Builder
            EditableDeckBuilder deckBuilder = FindFirstObjectByType<EditableDeckBuilder>();
            if (deckBuilder != null && PlayerPrefs.HasKey("UI_DeckBuilder_CollectionCardSize"))
            {
                deckBuilder.collectionCardSize = StringToVector2(PlayerPrefs.GetString("UI_DeckBuilder_CollectionCardSize", Vector2ToString(deckBuilder.collectionCardSize)));
                deckBuilder.deckCardSize = StringToVector2(PlayerPrefs.GetString("UI_DeckBuilder_DeckCardSize", Vector2ToString(deckBuilder.deckCardSize)));
                deckBuilder.collectionColumnsPerRow = PlayerPrefs.GetInt("UI_DeckBuilder_CollectionColumnsPerRow", deckBuilder.collectionColumnsPerRow);
                deckBuilder.deckColumnsPerRow = PlayerPrefs.GetInt("UI_DeckBuilder_DeckColumnsPerRow", deckBuilder.deckColumnsPerRow);
            }
            
            Debug.Log("✅ All auto-saved UI changes loaded!");
        }
        
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
                    return new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                }
            }
            catch { }
            
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
            catch { }
            
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
                    return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                }
            }
            catch { }
            
            return Vector4.zero;
        }
    }
}