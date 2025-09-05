using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FULLY EDITABLE Hero Hall Screen
    /// Customize hero selection UI completely in Inspector!
    /// </summary>
    public class EditableHeroHall : MonoBehaviour
    {
        [Header("🦸 HERO HALL - EDIT EVERYTHING!")]
        [Space(10)]
        
        [Header("🎨 Background & Layout")]
        [Tooltip("Custom background image")]
        public Sprite heroHallBackground;
        
        [Tooltip("Background color tint")]
        public Color backgroundTint = Color.white;
        
        [Space(10)]
        [Header("📱 Layout Areas - Drag to Reposition")]
        [Tooltip("Header area (title, back button)")]
        public RectTransform headerArea;
        
        [Tooltip("Active hero display area")]
        public RectTransform activeHeroArea;
        
        [Tooltip("Hero selection grid area")]
        public RectTransform heroGridArea;
        
        [Tooltip("Hero details area")]
        public RectTransform heroDetailsArea;
        
        [Space(10)]
        [Header("🎨 Visual Styling")]
        [Tooltip("Screen title")]
        public string screenTitle = "🦸 HERO HALL";
        
        [Range(18, 48)]
        [Tooltip("Title font size")]
        public int titleFontSize = 32;
        
        [Tooltip("Title color")]
        public Color titleColor = new Color(1f, 0.9f, 0.3f, 1f);
        
        [Tooltip("Hero card background color")]
        public Color heroCardColor = new Color(0.2f, 0.2f, 0.4f, 0.9f);
        
        [Tooltip("Active hero highlight color")]
        public Color activeHeroColor = new Color(1f, 0.8f, 0f, 1f);
        
        [Tooltip("Hero details panel color")]
        public Color detailsPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        [Space(10)]
        [Header("🦸 Hero Display Settings")]
        [Range(100, 250)]
        [Tooltip("Hero card width")]
        public float heroCardWidth = 150f;
        
        [Range(120, 300)]
        [Tooltip("Hero card height")]
        public float heroCardHeight = 200f;
        
        [Range(2, 5)]
        [Tooltip("Heroes per row")]
        public int heroesPerRow = 3;
        
        [Range(5, 20)]
        [Tooltip("Spacing between hero cards")]
        public float heroSpacing = 10f;
        
        [Space(10)]
        [Header("📊 Hero Info Display")]
        [Tooltip("Show hero power description")]
        public bool showHeroPower = true;
        
        [Tooltip("Show hero stats")]
        public bool showHeroStats = true;
        
        [Tooltip("Show hero lore/background")]
        public bool showHeroLore = true;
        
        [Space(10)]
        [Header("🔄 Quick Actions")]
        [Tooltip("Refresh hero hall")]
        public bool refreshScreen = false;
        
        [Tooltip("Reset to defaults")]
        public bool resetToDefaults = false;
        
        [Tooltip("Reload hero data")]
        public bool reloadHeroData = false;
        
        // Runtime data
        private List<RealSupabaseClient.Hero> availableHeroes;
        private RealSupabaseClient.Hero selectedHero;
        private RealSupabaseClient.Hero activeHero;
        private bool isInitialized = false;
        
        void Start()
        {
            InitializeHeroHall();
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
                
                if (reloadHeroData)
                {
                    reloadHeroData = false;
                    _ = LoadHeroData();
                }
                
                ApplyVisualSettings();
            }
        }
        
        private async void InitializeHeroHall()
        {
            Debug.Log("🦸 Initializing EDITABLE Hero Hall...");
            
            // Create layout
            CreateEditableLayout();
            
            // Load hero data
            await LoadHeroData();
            
            // Create content
            CreateHeroHallContent();
            
            // Apply settings
            ApplyVisualSettings();
            
            isInitialized = true;
            Debug.Log("✅ Editable Hero Hall ready!");
        }
        
        private void CreateEditableLayout()
        {
            // Background
            CreateEditableBackground();
            
            // Create areas
            if (headerArea == null) headerArea = CreateEditableArea("Header Area", new Vector2(0f, 0.9f), new Vector2(1f, 1f));
            if (activeHeroArea == null) activeHeroArea = CreateEditableArea("Active Hero Area", new Vector2(0f, 0.7f), new Vector2(1f, 0.88f));
            if (heroGridArea == null) heroGridArea = CreateEditableArea("Hero Grid Area", new Vector2(0.02f, 0.25f), new Vector2(0.98f, 0.68f));
            if (heroDetailsArea == null) heroDetailsArea = CreateEditableArea("Hero Details Area", new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.22f));
        }
        
        private void CreateEditableBackground()
        {
            GameObject bgObj = new GameObject("🎨 Hero Hall Background");
            bgObj.transform.SetParent(transform, false);
            bgObj.layer = 5;
            
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImg = bgObj.AddComponent<Image>();
            if (heroHallBackground != null)
            {
                bgImg.sprite = heroHallBackground;
                bgImg.color = backgroundTint;
            }
            else
            {
                bgImg.color = new Color(0.1f, 0.05f, 0.15f, 1f); // Purple theme
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
            
            Image areaImg = areaObj.AddComponent<Image>();
            areaImg.color = new Color(1f, 0f, 1f, 0.05f); // Very transparent purple
            
            return areaRect;
        }
        
        private async System.Threading.Tasks.Task LoadHeroData()
        {
            // Wait for RealSupabaseClient
            int attempts = 0;
            while (RealSupabaseClient.Instance == null && attempts < 50)
            {
                await System.Threading.Tasks.Task.Delay(100);
                attempts++;
            }
            
            if (RealSupabaseClient.Instance == null)
            {
                Debug.LogError("❌ Could not load hero data - Supabase not ready");
                CreateMockHeroData();
                return;
            }
            
            try
            {
                availableHeroes = await RealSupabaseClient.Instance.LoadAvailableHeroes();
                activeHero = await RealSupabaseClient.Instance.LoadActiveHero();
                
                Debug.Log($"🦸 Loaded {availableHeroes?.Count ?? 0} heroes");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Error loading heroes: {e.Message} - using mock data");
                CreateMockHeroData();
            }
        }
        
        private void CreateMockHeroData()
        {
            // Create mock heroes for testing
            availableHeroes = new List<RealSupabaseClient.Hero>
            {
                new RealSupabaseClient.Hero { id = "1", name = "Fire Mage", element = "fire", health = 30, hero_power = "Deal 2 damage to any target" },
                new RealSupabaseClient.Hero { id = "2", name = "Ice Guardian", element = "ice", health = 35, hero_power = "Freeze an enemy for 1 turn" },
                new RealSupabaseClient.Hero { id = "3", name = "Storm Lord", element = "lightning", health = 28, hero_power = "Deal 1 damage to all enemies" },
                new RealSupabaseClient.Hero { id = "4", name = "Light Paladin", element = "light", health = 32, hero_power = "Heal 3 health" },
                new RealSupabaseClient.Hero { id = "5", name = "Void Assassin", element = "void", health = 25, hero_power = "Destroy a random enemy card" }
            };
            
            activeHero = availableHeroes[0]; // Default to first hero
        }
        
        private void CreateHeroHallContent()
        {
            // Header
            if (headerArea != null)
            {
                CreateHeroHallHeader();
            }
            
            // Active hero display
            if (activeHeroArea != null)
            {
                CreateActiveHeroDisplay();
            }
            
            // Hero grid
            if (heroGridArea != null)
            {
                CreateHeroGrid();
            }
            
            // Hero details
            if (heroDetailsArea != null)
            {
                CreateHeroDetails();
            }
        }
        
        private void CreateHeroHallHeader()
        {
            // Back button
            CreateButton("← BACK", headerArea, new Vector2(0.02f, 0.1f), new Vector2(0.15f, 0.9f), new Color(0.8f, 0.2f, 0.2f, 1f), () => {
                EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                uiManager?.GoToMainMenu();
            });
            
            // Title
            CreateText("Hero Hall Title", screenTitle, headerArea, new Vector2(0.2f, 0f), new Vector2(0.8f, 1f), titleFontSize, titleColor);
        }
        
        private void CreateActiveHeroDisplay()
        {
            if (activeHero == null) return;
            
            // Active hero indicator
            CreateText("Active Hero Label", "👑 ACTIVE HERO:", activeHeroArea, new Vector2(0.05f, 0.5f), new Vector2(0.3f, 1f), 16, activeHeroColor);
            
            // Active hero name
            CreateText("Active Hero Name", activeHero.name, activeHeroArea, new Vector2(0.35f, 0.5f), new Vector2(0.7f, 1f), 18, Color.white);
            
            // Active hero element
            Color elementColor = GetElementalColor(activeHero.element);
            CreateText("Active Hero Element", $"{GetElementIcon(activeHero.element)} {activeHero.element?.ToUpper()}", 
                activeHeroArea, new Vector2(0.75f, 0.5f), new Vector2(0.95f, 1f), 14, elementColor);
        }
        
        private void CreateHeroGrid()
        {
            if (availableHeroes == null || availableHeroes.Count == 0)
            {
                CreateText("No Heroes", "No heroes available", heroGridArea, Vector2.zero, Vector2.one, 20, Color.gray);
                return;
            }
            
            // Create grid layout
            GameObject heroGrid = new GameObject("Hero Grid");
            heroGrid.transform.SetParent(heroGridArea, false);
            heroGrid.layer = 5;
            
            RectTransform gridRect = heroGrid.AddComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;
            
            GridLayoutGroup grid = heroGrid.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(heroCardWidth, heroCardHeight);
            grid.spacing = new Vector2(heroSpacing, heroSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = heroesPerRow;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.childAlignment = TextAnchor.UpperCenter;
            
            // Create hero cards
            foreach (var hero in availableHeroes)
            {
                CreateHeroCard(hero, heroGrid.transform);
            }
        }
        
        private void CreateHeroCard(RealSupabaseClient.Hero hero, Transform parent)
        {
            GameObject heroObj = new GameObject($"Hero: {hero.name}");
            heroObj.transform.SetParent(parent, false);
            heroObj.layer = 5;
            
            Image heroImg = heroObj.AddComponent<Image>();
            heroImg.color = hero.id == activeHero?.id ? activeHeroColor : heroCardColor;
            
            Button heroButton = heroObj.AddComponent<Button>();
            heroButton.onClick.AddListener(() => SelectHero(hero));
            
            // Hero portrait area (placeholder for now)
            CreateHeroPortrait(hero, heroObj.transform);
            
            // Hero name
            CreateText("Hero Name", hero.name, heroObj.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.2f), 12, Color.white);
            
            // Hero element
            Color elementColor = GetElementalColor(hero.element);
            CreateText("Hero Element", GetElementIcon(hero.element), heroObj.transform, new Vector2(0.05f, 0.8f), new Vector2(0.25f, 0.95f), 16, elementColor);
            
            // Hero health
            CreateText("Hero Health", $"❤️{hero.health}", heroObj.transform, new Vector2(0.75f, 0.8f), new Vector2(0.95f, 0.95f), 12, Color.red);
            
            // Active indicator
            if (hero.id == activeHero?.id)
            {
                CreateText("Active Indicator", "👑", heroObj.transform, new Vector2(0.4f, 0.8f), new Vector2(0.6f, 0.95f), 20, activeHeroColor);
            }
        }
        
        private void CreateHeroPortrait(RealSupabaseClient.Hero hero, Transform parent)
        {
            GameObject portraitObj = new GameObject("Hero Portrait");
            portraitObj.transform.SetParent(parent, false);
            portraitObj.layer = 5;
            
            Image portraitImg = portraitObj.AddComponent<Image>();
            portraitImg.color = GetElementalColor(hero.element);
            
            RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.15f, 0.25f);
            portraitRect.anchorMax = new Vector2(0.85f, 0.75f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            
            // Hero initial as placeholder
            CreateText("Hero Initial", hero.name.Substring(0, 1), portraitObj.transform, Vector2.zero, Vector2.one, 24, Color.white);
        }
        
        private void CreateHeroDetails()
        {
            if (selectedHero == null) selectedHero = activeHero;
            if (selectedHero == null) return;
            
            // Details panel background
            GameObject detailsPanel = new GameObject("Hero Details Panel");
            detailsPanel.transform.SetParent(heroDetailsArea, false);
            detailsPanel.layer = 5;
            
            Image panelImg = detailsPanel.AddComponent<Image>();
            panelImg.color = detailsPanelColor;
            
            RectTransform panelRect = detailsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Hero name
            CreateText("Selected Hero Name", selectedHero.name, detailsPanel.transform, new Vector2(0.05f, 0.7f), new Vector2(0.4f, 0.95f), 16, Color.white);
            
            // Hero power
            if (showHeroPower)
            {
                CreateText("Hero Power", $"Power: {selectedHero.hero_power}", detailsPanel.transform, new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.65f), 12, Color.cyan);
            }
            
            // Hero stats
            if (showHeroStats)
            {
                CreateText("Hero Health", $"Health: {selectedHero.health}", detailsPanel.transform, new Vector2(0.45f, 0.7f), new Vector2(0.7f, 0.95f), 14, Color.red);
                CreateText("Hero Element", $"Element: {selectedHero.element}", detailsPanel.transform, new Vector2(0.75f, 0.7f), new Vector2(0.95f, 0.95f), 14, GetElementalColor(selectedHero.element));
            }
            
            // Set as active button
            if (selectedHero.id != activeHero?.id)
            {
                CreateButton("SET AS ACTIVE", detailsPanel.transform, new Vector2(0.05f, 0.05f), new Vector2(0.3f, 0.35f), activeHeroColor, () => SetActiveHero(selectedHero));
            }
            else
            {
                CreateText("Already Active", "✅ ACTIVE HERO", detailsPanel.transform, new Vector2(0.05f, 0.05f), new Vector2(0.3f, 0.35f), 12, activeHeroColor);
            }
        }
        
        private void SelectHero(RealSupabaseClient.Hero hero)
        {
            Debug.Log($"🦸 Selected hero: {hero.name}");
            selectedHero = hero;
            
            // Refresh hero details
            RefreshHeroDetails();
            
            // Update hero card highlights
            RefreshHeroGrid();
        }
        
        private async void SetActiveHero(RealSupabaseClient.Hero hero)
        {
            Debug.Log($"👑 Setting active hero: {hero.name}");
            
            if (RealSupabaseClient.Instance != null)
            {
                bool success = await RealSupabaseClient.Instance.SetActiveHero(hero.id);
                if (success)
                {
                    activeHero = hero;
                    RefreshScreen();
                    Debug.Log($"✅ {hero.name} is now your active hero!");
                }
            }
        }
        
        private void RefreshHeroDetails()
        {
            // Remove existing details
            Transform existingDetails = heroDetailsArea.Find("Hero Details Panel");
            if (existingDetails != null)
            {
                DestroyImmediate(existingDetails.gameObject);
            }
            
            // Recreate details
            CreateHeroDetails();
        }
        
        private void RefreshHeroGrid()
        {
            // Update hero card colors
            Image[] heroImages = heroGridArea.GetComponentsInChildren<Image>();
            foreach (var img in heroImages)
            {
                if (img.name.Contains("Hero:"))
                {
                    string heroName = img.name.Substring(img.name.IndexOf(":") + 1).Trim();
                    bool isActive = activeHero?.name == heroName;
                    bool isSelected = selectedHero?.name == heroName;
                    
                    if (isActive)
                        img.color = activeHeroColor;
                    else if (isSelected)
                        img.color = heroCardColor * 1.2f; // Slightly brighter
                    else
                        img.color = heroCardColor;
                }
            }
        }
        
        private void CreateButton(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, System.Action onClick)
        {
            GameObject btnObj = new GameObject($"Button: {text}");
            btnObj.transform.SetParent(parent, false);
            btnObj.layer = 5;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;
            
            Button button = btnObj.AddComponent<Button>();
            if (onClick != null) button.onClick.AddListener(() => onClick());
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            CreateText("Button Text", text, btnObj.transform, Vector2.zero, Vector2.one, 12, Color.white);
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
        
        private Color GetElementalColor(string element)
        {
            return element?.ToLower() switch
            {
                "fire" => new Color(1f, 0.3f, 0.1f, 1f),
                "ice" => new Color(0.1f, 0.6f, 1f, 1f),
                "lightning" => new Color(1f, 1f, 0.2f, 1f),
                "light" => new Color(1f, 1f, 0.8f, 1f),
                "void" => new Color(0.4f, 0.1f, 0.6f, 1f),
                "exotic" => new Color(1f, 0.5f, 1f, 1f),
                _ => new Color(0.5f, 0.5f, 0.5f, 1f)
            };
        }
        
        private string GetElementIcon(string element)
        {
            return element?.ToLower() switch
            {
                "fire" => "🔥",
                "ice" => "🧊",
                "lightning" => "⚡",
                "light" => "☀️",
                "void" => "🌑",
                "exotic" => "✨",
                _ => "❓"
            };
        }
        
        public void RefreshScreen()
        {
            Debug.Log("🔄 Refreshing hero hall...");
            ApplyVisualSettings();
            RefreshHeroGrid();
            RefreshHeroDetails();
        }
        
        public void ResetToDefaults()
        {
            Debug.Log("🔄 Resetting hero hall to defaults...");
            
            heroCardWidth = 150f;
            heroCardHeight = 200f;
            heroesPerRow = 3;
            heroSpacing = 10f;
            titleFontSize = 32;
            
            ApplyVisualSettings();
        }
        
        private void ApplyVisualSettings()
        {
            // Update grid if it exists
            GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>();
            if (grid != null)
            {
                grid.cellSize = new Vector2(heroCardWidth, heroCardHeight);
                grid.spacing = new Vector2(heroSpacing, heroSpacing);
                grid.constraintCount = heroesPerRow;
            }
            
            // Update title
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.name == "Hero Hall Title")
                {
                    text.text = screenTitle;
                    text.fontSize = titleFontSize;
                    text.color = titleColor;
                }
            }
        }
    }
}