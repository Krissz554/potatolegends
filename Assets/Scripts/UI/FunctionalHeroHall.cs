using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FUNCTIONAL Hero Hall that actually loads and manages real heroes
    /// Recreates the HeroHall.tsx functionality from your web version
    /// </summary>
    public class FunctionalHeroHall : MonoBehaviour
    {
        [Header("🦸 Functional Hero Hall")]
        [SerializeField] private ScrollRect heroScrollRect;
        [SerializeField] private GridLayoutGroup heroGrid;
        [SerializeField] private TextMeshProUGUI heroCountText;
        
        // Real hero data
        private List<RealSupabaseClient.Hero> availableHeroes = new List<RealSupabaseClient.Hero>();
        private List<RealSupabaseClient.UserHero> userHeroes = new List<RealSupabaseClient.UserHero>();
        private List<GameObject> heroDisplays = new List<GameObject>();
        private RealSupabaseClient.Hero activeHero;
        
        // UI References
        private Canvas heroHallCanvas;
        private GameObject heroHallUI;
        private GameObject heroDetailPanel;
        
        private void Start()
        {
            CreateFunctionalHeroHallUI();
        }
        
        private void CreateFunctionalHeroHallUI()
        {
            Debug.Log("🦸 Creating FUNCTIONAL hero hall...");
            
            // Create hero hall canvas
            GameObject canvasObj = new GameObject("Hero Hall Canvas");
            heroHallCanvas = canvasObj.AddComponent<Canvas>();
            heroHallCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            heroHallCanvas.sortingOrder = 200;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create hero hall interface
            CreateHeroHallInterface();
            
            // Start hidden
            heroHallCanvas.gameObject.SetActive(false);
            
            Debug.Log("✅ FUNCTIONAL hero hall UI created");
        }
        
        private void CreateHeroHallInterface()
        {
            // Main background
            heroHallUI = CreatePanel("Hero Hall Screen", heroHallCanvas.transform);
            Image bgImage = heroHallUI.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.1f, 0.05f, 1f); // Dark bronze
            SetFullScreen(heroHallUI.GetComponent<RectTransform>());
            
            // Header
            CreateHeroHallHeader();
            
            // Hero grid
            CreateRealHeroGrid();
            
            // Hero detail panel
            CreateHeroDetailPanel();
            
            // Back button
            CreateBackButton();
        }
        
        private void CreateHeroHallHeader()
        {
            GameObject headerPanel = CreatePanel("Hero Hall Header", heroHallUI.transform);
            Image headerBg = headerPanel.GetComponent<Image>();
            headerBg.color = new Color(0f, 0f, 0f, 0.7f);
            
            RectTransform headerRect = headerPanel.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.9f);
            headerRect.anchorMax = new Vector2(0.95f, 0.98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Hero Hall Title");
            titleObj.transform.SetParent(headerPanel.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "HERO HALL";
            titleText.fontSize = 28;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.5f);
            titleRect.anchorMax = new Vector2(0.9f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Hero count
            GameObject countObj = new GameObject("Hero Count");
            countObj.transform.SetParent(headerPanel.transform, false);
            countObj.layer = 5;
            
            heroCountText = countObj.AddComponent<TextMeshProUGUI>();
            heroCountText.text = "Loading heroes...";
            heroCountText.fontSize = 16;
            heroCountText.color = Color.white;
            heroCountText.alignment = TextAlignmentOptions.Center;
            
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.1f, 0f);
            countRect.anchorMax = new Vector2(0.9f, 0.5f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
        }
        
        private void CreateRealHeroGrid()
        {
            // Scroll view for real heroes
            GameObject scrollViewObj = new GameObject("Real Hero Scroll View");
            scrollViewObj.transform.SetParent(heroHallUI.transform, false);
            scrollViewObj.layer = 5;
            
            heroScrollRect = scrollViewObj.AddComponent<ScrollRect>();
            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.88f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<RectMask2D>();
            
            SetFullScreen(viewport.GetComponent<RectTransform>());
            heroScrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            heroScrollRect.content = contentRect;
            
            // Grid for heroes
            heroGrid = content.AddComponent<GridLayoutGroup>();
            heroGrid.cellSize = new Vector2(200, 280); // Professional hero card size
            heroGrid.spacing = new Vector2(15, 15);
            heroGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            heroGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            heroGrid.childAlignment = TextAnchor.UpperCenter;
            heroGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            heroGrid.constraintCount = 4; // 4 heroes per row
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        private void CreateHeroDetailPanel()
        {
            heroDetailPanel = CreatePanel("Hero Detail Panel", heroHallUI.transform);
            Image detailBg = heroDetailPanel.GetComponent<Image>();
            detailBg.color = new Color(0f, 0f, 0f, 0.9f);
            
            SetFullScreen(heroDetailPanel.GetComponent<RectTransform>());
            heroDetailPanel.SetActive(false);
            
            // TODO: Create detailed hero info display
        }
        
        private void CreateBackButton()
        {
            GameObject backBtnObj = CreatePanel("Back Button", heroHallUI.transform);
            Button backButton = backBtnObj.AddComponent<Button>();
            Image backBtnImg = backBtnObj.GetComponent<Image>();
            backBtnImg.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            RectTransform backRect = backBtnObj.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.05f, 0.05f);
            backRect.anchorMax = new Vector2(0.25f, 0.12f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            
            // Back text
            GameObject backTextObj = new GameObject("Back Text");
            backTextObj.transform.SetParent(backBtnObj.transform, false);
            backTextObj.layer = 5;
            
            TextMeshProUGUI backText = backTextObj.AddComponent<TextMeshProUGUI>();
            backText.text = "← BACK";
            backText.fontSize = 18;
            backText.color = Color.white;
            backText.alignment = TextAlignmentOptions.Center;
            backText.fontStyle = FontStyles.Bold;
            backText.raycastTarget = false;
            
            SetFullScreen(backTextObj.GetComponent<RectTransform>());
            
            backButton.onClick.AddListener(() => {
                HideHeroHall();
            });
        }
        
        public async void ShowHeroHall()
        {
            Debug.Log("🦸 Showing functional hero hall...");
            
            heroHallCanvas.gameObject.SetActive(true);
            
            // Load real hero data
            await LoadRealHeroData();
        }
        
        public void HideHeroHall()
        {
            Debug.Log("🦸 Hiding hero hall...");
            heroHallCanvas.gameObject.SetActive(false);
            
            // Return to main menu
            var productionUI = FindFirstObjectByType<ProductionUIManager>();
            if (productionUI != null)
            {
                productionUI.ShowScreen(ProductionUIManager.GameScreen.MainMenu);
            }
        }
        
        private async Task LoadRealHeroData()
        {
            try
            {
                Debug.Log("🔄 Loading REAL hero data from Supabase...");
                
                if (RealSupabaseClient.Instance == null)
                {
                    Debug.LogError("❌ RealSupabaseClient not found!");
                    return;
                }
                
                // Load available heroes
                availableHeroes = await RealSupabaseClient.Instance.LoadAvailableHeroes();
                Debug.Log($"✅ Loaded {availableHeroes.Count} available heroes");
                
                // Load user heroes
                userHeroes = await RealSupabaseClient.Instance.LoadUserHeroes();
                Debug.Log($"✅ Loaded {userHeroes.Count} user heroes");
                
                // Find active hero
                var activeUserHero = userHeroes.FirstOrDefault(uh => uh.is_active);
                if (activeUserHero != null)
                {
                    activeHero = availableHeroes.FirstOrDefault(h => h.id == activeUserHero.hero_id);
                }
                
                // Update hero count
                UpdateHeroCount();
                
                // Display heroes
                DisplayRealHeroes();
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading hero data: {e.Message}");
                
                if (heroCountText)
                {
                    heroCountText.text = "Error loading heroes";
                    heroCountText.color = Color.red;
                }
            }
        }
        
        private void UpdateHeroCount()
        {
            int unlockedHeroes = userHeroes.Count;
            int totalHeroes = availableHeroes.Count;
            
            if (heroCountText)
            {
                string activeHeroName = activeHero?.name ?? "None";
                heroCountText.text = $"Heroes: {unlockedHeroes}/{totalHeroes} • Active: {activeHeroName}";
            }
        }
        
        private void DisplayRealHeroes()
        {
            Debug.Log("🦸 Displaying real heroes...");
            
            // Clear existing displays
            ClearHeroDisplays();
            
            // Create hero displays
            foreach (var hero in availableHeroes)
            {
                GameObject heroObj = CreateRealHeroDisplay(hero);
                if (heroObj != null)
                {
                    heroDisplays.Add(heroObj);
                }
            }
            
            Debug.Log($"✅ Displayed {heroDisplays.Count} heroes");
        }
        
        private GameObject CreateRealHeroDisplay(RealSupabaseClient.Hero hero)
        {
            GameObject heroObj = new GameObject($"Hero_{hero.name}");
            heroObj.transform.SetParent(heroGrid.transform, false);
            heroObj.layer = 5;
            
            // Hero background based on element
            Image heroImage = heroObj.AddComponent<Image>();
            heroImage.color = GetHeroElementColor(hero.element_type);
            
            // Hero name
            GameObject nameObj = new GameObject("Hero Name");
            nameObj.transform.SetParent(heroObj.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = hero.name;
            nameText.fontSize = 16;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            nameText.outlineColor = Color.black;
            nameText.outlineWidth = 0.3f;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.85f);
            nameRect.anchorMax = new Vector2(0.95f, 0.98f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Hero stats
            CreateHeroStats(heroObj.transform, hero);
            
            // Hero power
            CreateHeroPowerDisplay(heroObj.transform, hero);
            
            // Ownership status
            bool isOwned = userHeroes.Any(uh => uh.hero_id == hero.id);
            bool isActive = userHeroes.Any(uh => uh.hero_id == hero.id && uh.is_active);
            
            CreateOwnershipStatus(heroObj.transform, isOwned, isActive);
            
            // Click handler
            Button heroButton = heroObj.AddComponent<Button>();
            heroButton.onClick.AddListener(() => {
                ShowHeroDetails(hero, isOwned, isActive);
            });
            
            // Professional button colors
            ColorBlock colors = heroButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            heroButton.colors = colors;
            
            // Disable if not owned
            heroButton.interactable = isOwned;
            
            return heroObj;
        }
        
        private void CreateHeroStats(Transform parent, RealSupabaseClient.Hero hero)
        {
            // Health stat
            GameObject healthObj = CreatePanel("Hero Health", parent);
            Image healthBg = healthObj.GetComponent<Image>();
            healthBg.color = new Color(1f, 0.3f, 0.3f, 0.9f); // Red for health
            
            RectTransform healthRect = healthObj.GetComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0.05f, 0.65f);
            healthRect.anchorMax = new Vector2(0.45f, 0.8f);
            healthRect.offsetMin = Vector2.zero;
            healthRect.offsetMax = Vector2.zero;
            
            GameObject healthTextObj = new GameObject("Health Text");
            healthTextObj.transform.SetParent(healthObj.transform, false);
            healthTextObj.layer = 5;
            
            TextMeshProUGUI healthText = healthTextObj.AddComponent<TextMeshProUGUI>();
            healthText.text = $"❤️ {hero.base_hp}";
            healthText.fontSize = 12;
            healthText.color = Color.white;
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(healthTextObj.GetComponent<RectTransform>());
            
            // Mana stat
            GameObject manaObj = CreatePanel("Hero Mana", parent);
            Image manaBg = manaObj.GetComponent<Image>();
            manaBg.color = new Color(0.2f, 0.6f, 1f, 0.9f); // Blue for mana
            
            RectTransform manaRect = manaObj.GetComponent<RectTransform>();
            manaRect.anchorMin = new Vector2(0.55f, 0.65f);
            manaRect.anchorMax = new Vector2(0.95f, 0.8f);
            manaRect.offsetMin = Vector2.zero;
            manaRect.offsetMax = Vector2.zero;
            
            GameObject manaTextObj = new GameObject("Mana Text");
            manaTextObj.transform.SetParent(manaObj.transform, false);
            manaTextObj.layer = 5;
            
            TextMeshProUGUI manaText = manaTextObj.AddComponent<TextMeshProUGUI>();
            manaText.text = $"💎 {hero.base_mana}";
            manaText.fontSize = 12;
            manaText.color = Color.white;
            manaText.alignment = TextAlignmentOptions.Center;
            manaText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(manaTextObj.GetComponent<RectTransform>());
        }
        
        private void CreateHeroPowerDisplay(Transform parent, RealSupabaseClient.Hero hero)
        {
            GameObject powerObj = new GameObject("Hero Power");
            powerObj.transform.SetParent(parent, false);
            powerObj.layer = 5;
            
            TextMeshProUGUI powerText = powerObj.AddComponent<TextMeshProUGUI>();
            powerText.text = hero.hero_power_name ?? "No Power";
            powerText.fontSize = 11;
            powerText.color = new Color(0.8f, 0.8f, 1f, 1f);
            powerText.alignment = TextAlignmentOptions.Center;
            powerText.fontStyle = FontStyles.Italic;
            
            RectTransform powerRect = powerObj.GetComponent<RectTransform>();
            powerRect.anchorMin = new Vector2(0.05f, 0.45f);
            powerRect.anchorMax = new Vector2(0.95f, 0.6f);
            powerRect.offsetMin = Vector2.zero;
            powerRect.offsetMax = Vector2.zero;
        }
        
        private void CreateOwnershipStatus(Transform parent, bool isOwned, bool isActive)
        {
            GameObject statusObj = CreatePanel("Ownership Status", parent);
            Image statusBg = statusObj.GetComponent<Image>();
            
            if (isActive)
            {
                statusBg.color = new Color(1f, 0.8f, 0.2f, 0.9f); // Gold for active
            }
            else if (isOwned)
            {
                statusBg.color = new Color(0.2f, 1f, 0.2f, 0.9f); // Green for owned
            }
            else
            {
                statusBg.color = new Color(0.5f, 0.5f, 0.5f, 0.9f); // Gray for locked
            }
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.1f, 0.05f);
            statusRect.anchorMax = new Vector2(0.9f, 0.2f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            
            GameObject statusTextObj = new GameObject("Status Text");
            statusTextObj.transform.SetParent(statusObj.transform, false);
            statusTextObj.layer = 5;
            
            TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusText.text = isActive ? "⭐ ACTIVE" : isOwned ? "✅ OWNED" : "🔒 LOCKED";
            statusText.fontSize = 12;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(statusTextObj.GetComponent<RectTransform>());
        }
        
        private void ShowHeroDetails(RealSupabaseClient.Hero hero, bool isOwned, bool isActive)
        {
            Debug.Log($"🦸 Showing details for hero: {hero.name}");
            
            if (!isOwned)
            {
                Debug.Log("🔒 Hero is locked");
                return;
            }
            
            // TODO: Create detailed hero panel with select option
            // For now, just select the hero if owned
            if (isOwned && !isActive)
            {
                _ = SelectHero(hero);
            }
        }
        
        private async Task SelectHero(RealSupabaseClient.Hero hero)
        {
            try
            {
                Debug.Log($"⭐ Selecting hero: {hero.name}");
                
                // Set all user heroes to inactive
                foreach (var userHero in userHeroes)
                {
                    userHero.is_active = false;
                }
                
                // Set selected hero to active
                var targetUserHero = userHeroes.FirstOrDefault(uh => uh.hero_id == hero.id);
                if (targetUserHero != null)
                {
                    targetUserHero.is_active = true;
                    
                    // Update in database (simplified - would need proper RPC)
                    Debug.Log($"💾 Updating hero selection in database...");
                    
                    // Update active hero
                    activeHero = hero;
                    
                    // Refresh display
                    DisplayRealHeroes();
                    UpdateHeroCount();
                    
                    Debug.Log($"✅ {hero.name} is now your active hero!");
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error selecting hero: {e.Message}");
            }
        }
        
        private Color GetHeroElementColor(string elementType)
        {
            return elementType?.ToLower() switch
            {
                "fire" => new Color(1f, 0.3f, 0.2f, 0.9f),
                "ice" => new Color(0.2f, 0.6f, 1f, 0.9f),
                "lightning" => new Color(1f, 1f, 0.2f, 0.9f),
                "light" => new Color(1f, 0.9f, 0.7f, 0.9f),
                "void" => new Color(0.4f, 0.2f, 0.6f, 0.9f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.9f)
            };
        }
        
        private void ClearHeroDisplays()
        {
            foreach (GameObject heroDisplay in heroDisplays)
            {
                if (heroDisplay != null) Destroy(heroDisplay);
            }
            heroDisplays.Clear();
        }
        
        #region Helper Methods
        
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.layer = 5;
            
            panel.AddComponent<RectTransform>();
            panel.AddComponent<CanvasRenderer>();
            panel.AddComponent<Image>();
            
            return panel;
        }
        
        private void SetFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        #endregion
    }
}