using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// REAL Hero Hall that loads and manages actual heroes from database
    /// </summary>
    public class RealHeroHall : MonoBehaviour
    {
        [Header("Hero Hall UI")]
        [SerializeField] private Transform heroGrid;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button backButton;
        [SerializeField] private Button selectHeroButton;
        
        [Header("Hero Detail")]
        [SerializeField] private GameObject heroDetailPanel;
        [SerializeField] private TextMeshProUGUI detailHeroName;
        [SerializeField] private TextMeshProUGUI detailHeroDescription;
        [SerializeField] private TextMeshProUGUI detailHeroStats;
        [SerializeField] private TextMeshProUGUI detailHeroPower;
        [SerializeField] private Button closeDetailButton;
        
        // Hero data
        private List<RealSupabaseClient.Hero> availableHeroes = new List<RealSupabaseClient.Hero>();
        private List<RealSupabaseClient.UserHero> userHeroes = new List<RealSupabaseClient.UserHero>();
        private List<GameObject> heroDisplays = new List<GameObject>();
        private RealSupabaseClient.Hero selectedHero;
        
        private void Start()
        {
            CreateHeroHallUI();
            SetupEventListeners();
            LoadHeroes();
        }
        
        private void CreateHeroHallUI()
        {
            Debug.Log("🦸 Creating real hero hall UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            CreateHeroHallInterface(canvas.transform);
            
            // Start hidden
            gameObject.SetActive(false);
            
            Debug.Log("✅ Real hero hall UI created");
        }
        
        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Hero Hall Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        private void CreateHeroHallInterface(Transform parent)
        {
            // Background
            GameObject background = CreatePanel("Hero Hall Background", parent);
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.1f, 0.05f, 1f); // Dark bronze
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Top bar
            CreateTopBar(background.transform);
            
            // Hero grid
            CreateHeroGrid(background.transform);
            
            // Hero detail panel
            CreateHeroDetailPanel(background.transform);
        }
        
        private void CreateTopBar(Transform parent)
        {
            GameObject topBar = CreatePanel("Top Bar", parent);
            Image topBarBg = topBar.GetComponent<Image>();
            topBarBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 0.9f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.offsetMin = Vector2.zero;
            topBarRect.offsetMax = Vector2.zero;
            
            // Back button
            GameObject backBtnObj = CreatePanel("Back Button", topBar.transform);
            backButton = backBtnObj.AddComponent<Button>();
            Image backBtnImg = backBtnObj.GetComponent<Image>();
            backBtnImg.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            GameObject backTextObj = new GameObject("Back Text");
            backTextObj.transform.SetParent(backBtnObj.transform, false);
            backTextObj.layer = 5;
            
            TextMeshProUGUI backText = backTextObj.AddComponent<TextMeshProUGUI>();
            backText.text = "← BACK";
            backText.fontSize = 24;
            backText.color = Color.white;
            backText.alignment = TextAlignmentOptions.Center;
            backText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(backTextObj.GetComponent<RectTransform>());
            
            RectTransform backBtnRect = backBtnObj.GetComponent<RectTransform>();
            backBtnRect.anchorMin = new Vector2(0.05f, 0.1f);
            backBtnRect.anchorMax = new Vector2(0.25f, 0.9f);
            backBtnRect.offsetMin = Vector2.zero;
            backBtnRect.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Hero Hall Title");
            titleObj.transform.SetParent(topBar.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🦸 HERO HALL";
            titleText.fontSize = 32;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0f);
            titleRect.anchorMax = new Vector2(0.7f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
        }
        
        private void CreateHeroGrid(Transform parent)
        {
            // Scroll view
            GameObject scrollViewObj = new GameObject("Hero Scroll View");
            scrollViewObj.transform.SetParent(parent, false);
            scrollViewObj.layer = 5;
            
            scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollViewObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            
            RectTransform scrollViewRect = scrollViewObj.GetComponent<RectTransform>();
            scrollViewRect.anchorMin = new Vector2(0.05f, 0.1f);
            scrollViewRect.anchorMax = new Vector2(0.95f, 0.88f);
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            viewport.layer = 5;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<RectMask2D>();
            
            SetFullScreen(viewport.GetComponent<RectTransform>());
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            content.layer = 5;
            
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            scrollRect.content = contentRect;
            
            // Grid layout
            GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(200, 280); // Hero card size
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4; // 4 heroes per row
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            heroGrid = content.transform;
        }
        
        private void CreateHeroDetailPanel(Transform parent)
        {
            heroDetailPanel = CreatePanel("Hero Detail Panel", parent);
            Image detailBg = heroDetailPanel.GetComponent<Image>();
            detailBg.color = new Color(0f, 0f, 0f, 0.95f);
            
            SetFullScreen(heroDetailPanel.GetComponent<RectTransform>());
            heroDetailPanel.SetActive(false);
            
            // Detail content
            GameObject detailContent = CreatePanel("Detail Content", heroDetailPanel.transform);
            detailContent.GetComponent<Image>().color = new Color(0.2f, 0.1f, 0.05f, 0.95f);
            
            RectTransform detailRect = detailContent.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0.1f, 0.2f);
            detailRect.anchorMax = new Vector2(0.9f, 0.8f);
            detailRect.offsetMin = Vector2.zero;
            detailRect.offsetMax = Vector2.zero;
            
            // Hero name
            GameObject nameObj = new GameObject("Hero Name");
            nameObj.transform.SetParent(detailContent.transform, false);
            nameObj.layer = 5;
            
            detailHeroName = nameObj.AddComponent<TextMeshProUGUI>();
            detailHeroName.text = "Hero Name";
            detailHeroName.fontSize = 36;
            detailHeroName.color = new Color(1f, 0.8f, 0.2f, 1f);
            detailHeroName.alignment = TextAlignmentOptions.Center;
            detailHeroName.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.1f, 0.8f);
            nameRect.anchorMax = new Vector2(0.9f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Hero stats
            GameObject statsObj = new GameObject("Hero Stats");
            statsObj.transform.SetParent(detailContent.transform, false);
            statsObj.layer = 5;
            
            detailHeroStats = statsObj.AddComponent<TextMeshProUGUI>();
            detailHeroStats.text = "Health: 30 | Mana: 10";
            detailHeroStats.fontSize = 24;
            detailHeroStats.color = Color.white;
            detailHeroStats.alignment = TextAlignmentOptions.Center;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.1f, 0.65f);
            statsRect.anchorMax = new Vector2(0.9f, 0.75f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // Hero description
            GameObject descObj = new GameObject("Hero Description");
            descObj.transform.SetParent(detailContent.transform, false);
            descObj.layer = 5;
            
            detailHeroDescription = descObj.AddComponent<TextMeshProUGUI>();
            detailHeroDescription.text = "Hero description";
            detailHeroDescription.fontSize = 20;
            detailHeroDescription.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            detailHeroDescription.alignment = TextAlignmentOptions.Center;
            
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.45f);
            descRect.anchorMax = new Vector2(0.9f, 0.6f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            
            // Hero power
            GameObject powerObj = new GameObject("Hero Power");
            powerObj.transform.SetParent(detailContent.transform, false);
            powerObj.layer = 5;
            
            detailHeroPower = powerObj.AddComponent<TextMeshProUGUI>();
            detailHeroPower.text = "Hero Power: Description";
            detailHeroPower.fontSize = 18;
            detailHeroPower.color = new Color(0.8f, 0.8f, 1f, 1f);
            detailHeroPower.alignment = TextAlignmentOptions.Center;
            
            RectTransform powerRect = powerObj.GetComponent<RectTransform>();
            powerRect.anchorMin = new Vector2(0.1f, 0.25f);
            powerRect.anchorMax = new Vector2(0.9f, 0.4f);
            powerRect.offsetMin = Vector2.zero;
            powerRect.offsetMax = Vector2.zero;
            
            // Select hero button
            GameObject selectBtnObj = CreatePanel("Select Hero Button", detailContent.transform);
            selectHeroButton = selectBtnObj.AddComponent<Button>();
            Image selectBtnImg = selectBtnObj.GetComponent<Image>();
            selectBtnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);
            
            GameObject selectTextObj = new GameObject("Select Text");
            selectTextObj.transform.SetParent(selectBtnObj.transform, false);
            selectTextObj.layer = 5;
            
            TextMeshProUGUI selectText = selectTextObj.AddComponent<TextMeshProUGUI>();
            selectText.text = "⭐ SELECT HERO";
            selectText.fontSize = 24;
            selectText.color = Color.white;
            selectText.alignment = TextAlignmentOptions.Center;
            selectText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(selectTextObj.GetComponent<RectTransform>());
            
            RectTransform selectBtnRect = selectBtnObj.GetComponent<RectTransform>();
            selectBtnRect.anchorMin = new Vector2(0.2f, 0.1f);
            selectBtnRect.anchorMax = new Vector2(0.8f, 0.2f);
            selectBtnRect.offsetMin = Vector2.zero;
            selectBtnRect.offsetMax = Vector2.zero;
            
            // Close button
            GameObject closeBtnObj = CreatePanel("Close Button", detailContent.transform);
            closeDetailButton = closeBtnObj.AddComponent<Button>();
            Image closeBtnImg = closeBtnObj.GetComponent<Image>();
            closeBtnImg.color = new Color(0.7f, 0.3f, 0.3f, 1f);
            
            GameObject closeTextObj = new GameObject("Close Text");
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            closeTextObj.layer = 5;
            
            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "✖";
            closeText.fontSize = 24;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(closeTextObj.GetComponent<RectTransform>());
            
            RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.9f, 0.85f);
            closeBtnRect.anchorMax = new Vector2(1f, 0.95f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;
        }
        
        private void SetupEventListeners()
        {
            // UI events
            if (backButton) backButton.onClick.AddListener(OnBackPressed);
            if (closeDetailButton) closeDetailButton.onClick.AddListener(OnCloseDetailPressed);
            if (selectHeroButton) selectHeroButton.onClick.AddListener(OnSelectHeroPressed);
            
            // Game flow events
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged += OnGameFlowChanged;
            }
        }
        
        private async void LoadHeroes()
        {
            Debug.Log("🔄 Loading heroes from database...");
            
            try
            {
                // Load available heroes
                availableHeroes = await RealSupabaseClient.Instance.LoadAvailableHeroes();
                
                // Load user heroes
                userHeroes = await RealSupabaseClient.Instance.LoadUserHeroes();
                
                DisplayHeroes();
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading heroes: {e.Message}");
            }
        }
        
        private void DisplayHeroes()
        {
            // Clear existing displays
            foreach (GameObject heroDisplay in heroDisplays)
            {
                if (heroDisplay != null) Destroy(heroDisplay);
            }
            heroDisplays.Clear();
            
            // Create hero displays
            foreach (var hero in availableHeroes.Take(20)) // Limit for performance
            {
                GameObject heroObj = CreateHeroDisplay(hero);
                if (heroObj != null)
                {
                    heroDisplays.Add(heroObj);
                }
            }
            
            Debug.Log($"🦸 Displayed {heroDisplays.Count} heroes");
        }
        
        private GameObject CreateHeroDisplay(RealSupabaseClient.Hero hero)
        {
            GameObject heroCard = CreatePanel($"Hero_{hero.id}", heroGrid);
            
            // Hero background based on element
            Image heroBg = heroCard.GetComponent<Image>();
            heroBg.color = GetHeroElementColor(hero.element_type);
            
            // Hero name
            GameObject nameObj = new GameObject("Hero Name");
            nameObj.transform.SetParent(heroCard.transform, false);
            nameObj.layer = 5;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = hero.name;
            nameText.fontSize = 18;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.8f);
            nameRect.anchorMax = new Vector2(0.95f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Hero stats
            GameObject statsObj = new GameObject("Hero Stats");
            statsObj.transform.SetParent(heroCard.transform, false);
            statsObj.layer = 5;
            
            TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = $"❤️ {hero.base_hp} | 💎 {hero.base_mana}";
            statsText.fontSize = 16;
            statsText.color = Color.white;
            statsText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.05f, 0.65f);
            statsRect.anchorMax = new Vector2(0.95f, 0.75f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // Hero power name
            GameObject powerObj = new GameObject("Hero Power");
            powerObj.transform.SetParent(heroCard.transform, false);
            powerObj.layer = 5;
            
            TextMeshProUGUI powerText = powerObj.AddComponent<TextMeshProUGUI>();
            powerText.text = hero.hero_power_name ?? "No Power";
            powerText.fontSize = 14;
            powerText.color = new Color(0.8f, 0.8f, 1f, 1f);
            powerText.alignment = TextAlignmentOptions.Center;
            
            RectTransform powerRect = powerObj.GetComponent<RectTransform>();
            powerRect.anchorMin = new Vector2(0.05f, 0.45f);
            powerRect.anchorMax = new Vector2(0.95f, 0.6f);
            powerRect.offsetMin = Vector2.zero;
            powerRect.offsetMax = Vector2.zero;
            
            // Rarity indicator
            GameObject rarityObj = new GameObject("Rarity");
            rarityObj.transform.SetParent(heroCard.transform, false);
            rarityObj.layer = 5;
            
            TextMeshProUGUI rarityText = rarityObj.AddComponent<TextMeshProUGUI>();
            rarityText.text = GetRarityIcon(hero.rarity);
            rarityText.fontSize = 20;
            rarityText.color = GetRarityColor(hero.rarity);
            rarityText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rarityRect = rarityObj.GetComponent<RectTransform>();
            rarityRect.anchorMin = new Vector2(0.8f, 0.05f);
            rarityRect.anchorMax = new Vector2(1f, 0.25f);
            rarityRect.offsetMin = Vector2.zero;
            rarityRect.offsetMax = Vector2.zero;
            
            // Check if user owns this hero
            bool isOwned = userHeroes.Any(uh => uh.hero_id == hero.id);
            bool isActive = userHeroes.Any(uh => uh.hero_id == hero.id && uh.is_active);
            
            // Ownership indicator
            GameObject ownershipObj = new GameObject("Ownership");
            ownershipObj.transform.SetParent(heroCard.transform, false);
            ownershipObj.layer = 5;
            
            TextMeshProUGUI ownershipText = ownershipObj.AddComponent<TextMeshProUGUI>();
            ownershipText.text = isActive ? "⭐ ACTIVE" : isOwned ? "✅ OWNED" : "🔒 LOCKED";
            ownershipText.fontSize = 12;
            ownershipText.color = isActive ? new Color(1f, 0.8f, 0.2f, 1f) : 
                                  isOwned ? new Color(0.2f, 1f, 0.2f, 1f) : 
                                  new Color(0.5f, 0.5f, 0.5f, 1f);
            ownershipText.alignment = TextAlignmentOptions.Center;
            ownershipText.fontStyle = FontStyles.Bold;
            
            RectTransform ownershipRect = ownershipObj.GetComponent<RectTransform>();
            ownershipRect.anchorMin = new Vector2(0.05f, 0.05f);
            ownershipRect.anchorMax = new Vector2(0.75f, 0.2f);
            ownershipRect.offsetMin = Vector2.zero;
            ownershipRect.offsetMax = Vector2.zero;
            
            // Add click handler
            Button heroButton = heroCard.AddComponent<Button>();
            heroButton.onClick.AddListener(() => OnHeroClicked(hero));
            
            // Disable if not owned
            if (!isOwned)
            {
                heroBg.color = new Color(heroBg.color.r * 0.5f, heroBg.color.g * 0.5f, heroBg.color.b * 0.5f, 0.7f);
                heroButton.interactable = false;
            }
            
            return heroCard;
        }
        
        private Color GetHeroElementColor(string elementType)
        {
            return elementType?.ToLower() switch
            {
                "fire" => new Color(1f, 0.3f, 0.2f, 0.9f),      // Red
                "ice" => new Color(0.2f, 0.6f, 1f, 0.9f),       // Blue
                "lightning" => new Color(1f, 1f, 0.2f, 0.9f),   // Yellow
                "light" => new Color(1f, 0.9f, 0.7f, 0.9f),     // Light yellow
                "void" => new Color(0.4f, 0.2f, 0.6f, 0.9f),    // Purple
                _ => new Color(0.5f, 0.5f, 0.5f, 0.9f)          // Gray
            };
        }
        
        private string GetRarityIcon(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => "⚪",
                "uncommon" => "🟢",
                "rare" => "🔵",
                "legendary" => "🟡",
                "exotic" => "🟣",
                _ => "⚪"
            };
        }
        
        private Color GetRarityColor(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => Color.white,
                "uncommon" => Color.green,
                "rare" => Color.blue,
                "legendary" => Color.yellow,
                "exotic" => new Color(0.8f, 0.2f, 1f, 1f), // Purple
                _ => Color.white
            };
        }
        
        private void OnHeroClicked(RealSupabaseClient.Hero hero)
        {
            selectedHero = hero;
            ShowHeroDetail(hero);
        }
        
        private void ShowHeroDetail(RealSupabaseClient.Hero hero)
        {
            if (heroDetailPanel == null) return;
            
            // Update detail UI
            if (detailHeroName) detailHeroName.text = hero.name;
            if (detailHeroDescription) detailHeroDescription.text = hero.description ?? "A mighty hero ready for battle!";
            
            if (detailHeroStats)
            {
                detailHeroStats.text = $"❤️ Health: {hero.base_hp} | 💎 Mana: {hero.base_mana} | Element: {hero.element_type}";
            }
            
            if (detailHeroPower)
            {
                string powerText = $"🔥 {hero.hero_power_name}";
                if (hero.hero_power_cost > 0) powerText += $" (Cost: {hero.hero_power_cost})";
                if (!string.IsNullOrEmpty(hero.hero_power_description))
                    powerText += $"\n{hero.hero_power_description}";
                
                detailHeroPower.text = powerText;
            }
            
            // Check if user owns this hero
            bool isOwned = userHeroes.Any(uh => uh.hero_id == hero.id);
            if (selectHeroButton)
            {
                selectHeroButton.interactable = isOwned;
                Image selectImg = selectHeroButton.GetComponent<Image>();
                if (selectImg)
                {
                    selectImg.color = isOwned ? new Color(0.2f, 0.7f, 0.3f, 1f) : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
            
            heroDetailPanel.SetActive(true);
        }
        
        private async void OnSelectHeroPressed()
        {
            if (selectedHero == null) return;
            
            try
            {
                Debug.Log($"⭐ Selecting hero: {selectedHero.name}");
                
                // Set all user heroes to inactive
                foreach (var userHero in userHeroes)
                {
                    userHero.is_active = false;
                }
                
                // Set selected hero to active
                var targetUserHero = userHeroes.FirstOrDefault(uh => uh.hero_id == selectedHero.id);
                if (targetUserHero != null)
                {
                    targetUserHero.is_active = true;
                    
                    // Update in database
                    var updateData = new { is_active = true };
                    await RealSupabaseClient.Instance.PostData<object>($"user_heroes?id=eq.{targetUserHero.id}", updateData);
                    
                    // Deactivate others
                    var deactivateData = new { is_active = false };
                    await RealSupabaseClient.Instance.PostData<object>($"user_heroes?user_id=eq.{RealSupabaseClient.Instance.UserId}&id=neq.{targetUserHero.id}", deactivateData);
                    
                    Debug.Log($"✅ {selectedHero.name} is now your active hero!");
                    
                    // Refresh display
                    DisplayHeroes();
                    
                    // Close detail panel
                    OnCloseDetailPressed();
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error selecting hero: {e.Message}");
            }
        }
        
        private void OnCloseDetailPressed()
        {
            if (heroDetailPanel) heroDetailPanel.SetActive(false);
        }
        
        private void OnBackPressed()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.NavigateToMainMenu();
            }
        }
        
        private void OnGameFlowChanged(GameFlowManager.GameFlow newFlow)
        {
            bool shouldShow = newFlow == GameFlowManager.GameFlow.HeroHall;
            gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                LoadHeroes(); // Refresh when shown
            }
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
        
        private void OnDestroy()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged -= OnGameFlowChanged;
            }
        }
    }
}