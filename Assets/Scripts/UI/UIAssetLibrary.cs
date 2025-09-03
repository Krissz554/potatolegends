using UnityEngine;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// UI Asset Library - Centralized asset management for easy replacement
    /// Reference system for all UI sprites, fonts, and materials
    /// </summary>
    [CreateAssetMenu(fileName = "UI Asset Library", menuName = "Potato Legends/UI Asset Library")]
    public class UIAssetLibrary : ScriptableObject
    {
        [Header("🖼️ Background Assets")]
        [Tooltip("Fullscreen background for auth screen")]
        public Sprite authBackground;
        
        [Tooltip("Fullscreen background for main menu")]
        public Sprite mainMenuBackground;
        
        [Tooltip("Fullscreen background for collection")]
        public Sprite collectionBackground;
        
        [Tooltip("Fullscreen background for deck builder")]
        public Sprite deckBuilderBackground;
        
        [Tooltip("Fullscreen background for hero hall")]
        public Sprite heroHallBackground;
        
        [Tooltip("Fullscreen background for battle arena")]
        public Sprite battleBackground;
        
        [Header("🎨 Panel Assets")]
        [Tooltip("Fantasy frame for auth panel")]
        public Sprite authPanelFrame;
        
        [Tooltip("Header panel sprite")]
        public Sprite headerPanelSprite;
        
        [Tooltip("Navigation card frame")]
        public Sprite navigationCardSprite;
        
        [Tooltip("Collection filter panel")]
        public Sprite filterPanelSprite;
        
        [Tooltip("Deck builder left panel")]
        public Sprite deckPanelSprite;
        
        [Tooltip("Deck builder right panel")]
        public Sprite cardsPanelSprite;
        
        [Tooltip("Utility bar panel")]
        public Sprite utilityBarSprite;
        
        [Header("🔘 Button Assets")]
        [Tooltip("Primary button sprite (login, confirm, etc.)")]
        public Sprite primaryButtonSprite;
        
        [Tooltip("Secondary button sprite (cancel, back, etc.)")]
        public Sprite secondaryButtonSprite;
        
        [Tooltip("Battle button sprite (special styling)")]
        public Sprite battleButtonSprite;
        
        [Tooltip("Navigation back button")]
        public Sprite backButtonSprite;
        
        [Tooltip("Filter button sprite")]
        public Sprite filterButtonSprite;
        
        [Tooltip("Utility button sprite")]
        public Sprite utilityButtonSprite;
        
        [Header("🃏 Card Assets")]
        [Tooltip("Card frame sprite")]
        public Sprite cardFrameSprite;
        
        [Tooltip("Hero card frame")]
        public Sprite heroCardSprite;
        
        [Tooltip("Deck slot frame")]
        public Sprite deckSlotSprite;
        
        [Header("🎭 Icon Assets")]
        [Tooltip("Collection navigation icon")]
        public Sprite collectionIcon;
        
        [Tooltip("Deck builder navigation icon")]
        public Sprite deckBuilderIcon;
        
        [Tooltip("Hero hall navigation icon")]
        public Sprite heroHallIcon;
        
        [Tooltip("Battle icon")]
        public Sprite battleIcon;
        
        [Tooltip("Settings icon")]
        public Sprite settingsIcon;
        
        [Tooltip("Shop icon")]
        public Sprite shopIcon;
        
        [Tooltip("Gold currency icon")]
        public Sprite goldIcon;
        
        [Tooltip("Gem currency icon")]
        public Sprite gemIcon;
        
        [Tooltip("Player avatar frame")]
        public Sprite avatarFrameSprite;
        
        [Header("⚡ Elemental Icons")]
        [Tooltip("Fire element icon")]
        public Sprite fireIcon;
        
        [Tooltip("Ice element icon")]
        public Sprite iceIcon;
        
        [Tooltip("Lightning element icon")]
        public Sprite lightningIcon;
        
        [Tooltip("Light element icon")]
        public Sprite lightIcon;
        
        [Tooltip("Void element icon")]
        public Sprite voidIcon;
        
        [Header("🎯 Stat Icons")]
        [Tooltip("Mana cost icon")]
        public Sprite manaIcon;
        
        [Tooltip("Attack stat icon")]
        public Sprite attackIcon;
        
        [Tooltip("Health stat icon")]
        public Sprite healthIcon;
        
        [Tooltip("Hero power icon")]
        public Sprite heroPowerIcon;
        
        [Header("🔤 Font Assets")]
        [Tooltip("Fantasy font for titles")]
        public TMP_FontAsset fantasyTitleFont;
        
        [Tooltip("Clean font for body text")]
        public TMP_FontAsset cleanBodyFont;
        
        [Header("🎨 Material Assets")]
        [Tooltip("Gradient material for backgrounds")]
        public Material gradientMaterial;
        
        [Tooltip("Glow material for special effects")]
        public Material glowMaterial;
        
        [Tooltip("Card material with shader effects")]
        public Material cardMaterial;
        
        /// <summary>
        /// Get elemental background sprite based on element type
        /// </summary>
        public Sprite GetElementalBackground(string elementType)
        {
            return elementType?.ToLower() switch
            {
                "fire" => GetFireBackground(),
                "ice" => GetIceBackground(),
                "lightning" => GetLightningBackground(),
                "light" => GetLightBackground(),
                "void" => GetVoidBackground(),
                _ => GetVoidBackground() // Default
            };
        }
        
        /// <summary>
        /// Get elemental icon based on element type
        /// </summary>
        public Sprite GetElementalIcon(string elementType)
        {
            return elementType?.ToLower() switch
            {
                "fire" => fireIcon,
                "ice" => iceIcon,
                "lightning" => lightningIcon,
                "light" => lightIcon,
                "void" => voidIcon,
                _ => voidIcon // Default
            };
        }
        
        // Placeholder methods for elemental backgrounds
        // These will reference the actual elemental card backgrounds you have
        private Sprite GetFireBackground()
        {
            // Reference to fire-card.png from Resources/ElementalBackgrounds/
            return Resources.Load<Sprite>("ElementalBackgrounds/fire-card");
        }
        
        private Sprite GetIceBackground()
        {
            return Resources.Load<Sprite>("ElementalBackgrounds/ice-card");
        }
        
        private Sprite GetLightningBackground()
        {
            return Resources.Load<Sprite>("ElementalBackgrounds/lightning-card");
        }
        
        private Sprite GetLightBackground()
        {
            return Resources.Load<Sprite>("ElementalBackgrounds/light-card");
        }
        
        private Sprite GetVoidBackground()
        {
            return Resources.Load<Sprite>("ElementalBackgrounds/void-card");
        }
        
        /// <summary>
        /// Validate asset references and log missing assets
        /// </summary>
        [ContextMenu("Validate Asset References")]
        public void ValidateAssets()
        {
            Debug.Log("🔍 Validating UI Asset Library...");
            
            int totalAssets = 0;
            int missingAssets = 0;
            
            // Check backgrounds
            if (authBackground == null) { missingAssets++; Debug.LogWarning("❌ Missing: authBackground"); }
            if (mainMenuBackground == null) { missingAssets++; Debug.LogWarning("❌ Missing: mainMenuBackground"); }
            totalAssets += 6; // Total background assets
            
            // Check buttons
            if (primaryButtonSprite == null) { missingAssets++; Debug.LogWarning("❌ Missing: primaryButtonSprite"); }
            if (battleButtonSprite == null) { missingAssets++; Debug.LogWarning("❌ Missing: battleButtonSprite"); }
            totalAssets += 6; // Total button assets
            
            // Check icons
            if (collectionIcon == null) { missingAssets++; Debug.LogWarning("❌ Missing: collectionIcon"); }
            if (battleIcon == null) { missingAssets++; Debug.LogWarning("❌ Missing: battleIcon"); }
            totalAssets += 15; // Total icon assets
            
            Debug.Log($"📊 Asset validation: {totalAssets - missingAssets}/{totalAssets} assets assigned");
            
            if (missingAssets == 0)
            {
                Debug.Log("✅ All assets properly assigned!");
            }
            else
            {
                Debug.Log($"⚠️ {missingAssets} assets missing - using fallback colors/sprites");
            }
        }
    }
}