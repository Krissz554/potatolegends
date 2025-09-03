using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Core;
using PotatoCardGame.Battle;
using PotatoCardGame.Cards;
using PotatoCardGame.Data;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Battle Arena UI that recreates the web version's PixelArena
    /// Complete battle interface with cards, heroes, battlefield, and controls
    /// </summary>
    public class BattleArenaUI : MonoBehaviour
    {
        [Header("Hero Areas")]
        [SerializeField] private Transform playerHeroArea;
        [SerializeField] private Transform opponentHeroArea;
        [SerializeField] private TextMeshProUGUI playerHeroHP;
        [SerializeField] private TextMeshProUGUI playerHeroMana;
        [SerializeField] private TextMeshProUGUI opponentHeroHP;
        [SerializeField] private TextMeshProUGUI opponentHeroMana;
        
        [Header("Card Areas")]
        [SerializeField] private Transform playerHandArea;
        [SerializeField] private Transform playerBattlefieldArea;
        [SerializeField] private Transform opponentBattlefieldArea;
        [SerializeField] private Transform opponentHandArea;
        
        [Header("Battle Controls")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button heroPowerButton;
        [SerializeField] private Button battleMenuButton;
        [SerializeField] private Button surrenderButton;
        
        [Header("Battle Info")]
        [SerializeField] private TextMeshProUGUI turnIndicatorText;
        [SerializeField] private TextMeshProUGUI turnTimerText;
        [SerializeField] private TextMeshProUGUI gamePhaseText;
        
        [Header("Battle Menu")]
        [SerializeField] private GameObject battleMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button forfeitButton;
        
        private string battleSessionId;
        private bool isPlayerTurn = false;
        private bool isInitialized = false;
        
        private void Start()
        {
            CreateBattleArenaUI();
            SetupEventListeners();
        }
        
        private void CreateBattleArenaUI()
        {
            Debug.Log("⚔️ Creating battle arena UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateBattleCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            CreateBattleInterface(canvas.transform);
            
            // Start hidden until battle begins
            gameObject.SetActive(false);
            
            Debug.Log("✅ Battle arena UI created");
        }
        
        private void CreateBattleCanvas()
        {
            GameObject canvasObj = new GameObject("Battle Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // Above main UI
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        private void CreateBattleInterface(Transform parent)
        {
            // Battle background
            GameObject background = CreatePanel("Battle Background", parent);
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.05f, 0.1f, 0.05f, 1f); // Dark green battlefield
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Opponent area (top)
            CreateOpponentArea(background.transform);
            
            // Battlefield (center)
            CreateBattlefield(background.transform);
            
            // Player area (bottom)
            CreatePlayerArea(background.transform);
            
            // Battle controls (right side)
            CreateBattleControls(background.transform);
            
            // Battle info (top right)
            CreateBattleInfo(background.transform);
            
            // Battle menu (hidden by default)
            CreateBattleMenu(background.transform);
        }
        
        private void CreateOpponentArea(Transform parent)
        {
            GameObject opponentArea = CreatePanel("Opponent Area", parent);
            opponentArea.GetComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 0.5f); // Red tint
            
            RectTransform opponentRect = opponentArea.GetComponent<RectTransform>();
            opponentRect.anchorMin = new Vector2(0f, 0.75f);
            opponentRect.anchorMax = new Vector2(1f, 0.95f);
            opponentRect.offsetMin = Vector2.zero;
            opponentRect.offsetMax = Vector2.zero;
            
            // Opponent hero display
            GameObject heroDisplay = CreatePanel("Opponent Hero", opponentArea.transform);
            heroDisplay.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.2f, 0.8f);
            
            RectTransform heroRect = heroDisplay.GetComponent<RectTransform>();
            heroRect.anchorMin = new Vector2(0.4f, 0.2f);
            heroRect.anchorMax = new Vector2(0.6f, 0.8f);
            heroRect.offsetMin = Vector2.zero;
            heroRect.offsetMax = Vector2.zero;
            
            opponentHeroArea = heroDisplay.transform;
            
            // Opponent HP/Mana
            CreateHeroStats(opponentArea.transform, false);
            
            // Opponent hand (face down cards)
            CreateOpponentHand(opponentArea.transform);
        }
        
        private void CreatePlayerArea(Transform parent)
        {
            GameObject playerArea = CreatePanel("Player Area", parent);
            playerArea.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.3f, 0.5f); // Blue tint
            
            RectTransform playerRect = playerArea.GetComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0f, 0.05f);
            playerRect.anchorMax = new Vector2(1f, 0.35f);
            playerRect.offsetMin = Vector2.zero;
            playerRect.offsetMax = Vector2.zero;
            
            // Player hero display
            GameObject heroDisplay = CreatePanel("Player Hero", playerArea.transform);
            heroDisplay.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.5f, 0.8f);
            
            RectTransform heroRect = heroDisplay.GetComponent<RectTransform>();
            heroRect.anchorMin = new Vector2(0.4f, 0.6f);
            heroRect.anchorMax = new Vector2(0.6f, 1f);
            heroRect.offsetMin = Vector2.zero;
            heroRect.offsetMax = Vector2.zero;
            
            playerHeroArea = heroDisplay.transform;
            
            // Player HP/Mana
            CreateHeroStats(playerArea.transform, true);
            
            // Player hand
            CreatePlayerHand(playerArea.transform);
        }
        
        private void CreateBattlefield(Transform parent)
        {
            GameObject battlefield = CreatePanel("Battlefield", parent);
            battlefield.GetComponent<Image>().color = new Color(0.1f, 0.2f, 0.1f, 0.3f); // Green battlefield
            
            RectTransform battlefieldRect = battlefield.GetComponent<RectTransform>();
            battlefieldRect.anchorMin = new Vector2(0.05f, 0.35f);
            battlefieldRect.anchorMax = new Vector2(0.75f, 0.75f);
            battlefieldRect.offsetMin = Vector2.zero;
            battlefieldRect.offsetMax = Vector2.zero;
            
            // Opponent battlefield slots
            GameObject opponentBattlefield = CreatePanel("Opponent Battlefield", battlefield.transform);
            opponentBattlefield.GetComponent<Image>().color = Color.clear;
            
            RectTransform opponentBFRect = opponentBattlefield.GetComponent<RectTransform>();
            opponentBFRect.anchorMin = new Vector2(0f, 0.6f);
            opponentBFRect.anchorMax = new Vector2(1f, 1f);
            opponentBFRect.offsetMin = Vector2.zero;
            opponentBFRect.offsetMax = Vector2.zero;
            
            opponentBattlefieldArea = opponentBattlefield.transform;
            
            // Player battlefield slots
            GameObject playerBattlefield = CreatePanel("Player Battlefield", battlefield.transform);
            playerBattlefield.GetComponent<Image>().color = Color.clear;
            
            RectTransform playerBFRect = playerBattlefield.GetComponent<RectTransform>();
            playerBFRect.anchorMin = new Vector2(0f, 0f);
            playerBFRect.anchorMax = new Vector2(1f, 0.4f);
            playerBFRect.offsetMin = Vector2.zero;
            playerBFRect.offsetMax = Vector2.zero;
            
            playerBattlefieldArea = playerBattlefield.transform;
            
            // Create battlefield slots
            CreateBattlefieldSlots(opponentBattlefieldArea, false);
            CreateBattlefieldSlots(playerBattlefieldArea, true);
        }
        
        private void CreateBattlefieldSlots(Transform parent, bool isPlayer)
        {
            // Create 5 battlefield slots (like web version)
            for (int i = 0; i < 5; i++)
            {
                GameObject slot = CreatePanel($"Slot {i}", parent);
                Image slotImage = slot.GetComponent<Image>();
                slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f); // Subtle slot indicator
                
                RectTransform slotRect = slot.GetComponent<RectTransform>();
                float slotWidth = 0.18f; // 18% width per slot
                float startX = i * 0.2f + 0.01f; // Small spacing
                
                slotRect.anchorMin = new Vector2(startX, 0.1f);
                slotRect.anchorMax = new Vector2(startX + slotWidth, 0.9f);
                slotRect.offsetMin = Vector2.zero;
                slotRect.offsetMax = Vector2.zero;
            }
        }
        
        private void CreateHeroStats(Transform parent, bool isPlayer)
        {
            GameObject statsPanel = CreatePanel($"{(isPlayer ? "Player" : "Opponent")} Stats", parent);
            statsPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
            
            RectTransform statsRect = statsPanel.GetComponent<RectTransform>();
            if (isPlayer)
            {
                statsRect.anchorMin = new Vector2(0.05f, 0.6f);
                statsRect.anchorMax = new Vector2(0.35f, 1f);
            }
            else
            {
                statsRect.anchorMin = new Vector2(0.65f, 0.2f);
                statsRect.anchorMax = new Vector2(0.95f, 0.8f);
            }
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // HP text
            GameObject hpObj = new GameObject("HP Text");
            hpObj.transform.SetParent(statsPanel.transform, false);
            hpObj.layer = 5;
            
            TextMeshProUGUI hpText = hpObj.AddComponent<TextMeshProUGUI>();
            hpText.text = "❤️ 30/30";
            hpText.fontSize = 24;
            hpText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red for HP
            hpText.alignment = TextAlignmentOptions.Center;
            hpText.fontStyle = FontStyles.Bold;
            
            RectTransform hpRect = hpObj.GetComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0f, 0.6f);
            hpRect.anchorMax = new Vector2(1f, 0.8f);
            hpRect.offsetMin = Vector2.zero;
            hpRect.offsetMax = Vector2.zero;
            
            // Mana text
            GameObject manaObj = new GameObject("Mana Text");
            manaObj.transform.SetParent(statsPanel.transform, false);
            manaObj.layer = 5;
            
            TextMeshProUGUI manaText = manaObj.AddComponent<TextMeshProUGUI>();
            manaText.text = "💎 3/3";
            manaText.fontSize = 24;
            manaText.color = new Color(0.3f, 0.3f, 1f, 1f); // Blue for mana
            manaText.alignment = TextAlignmentOptions.Center;
            manaText.fontStyle = FontStyles.Bold;
            
            RectTransform manaRect = manaObj.GetComponent<RectTransform>();
            manaRect.anchorMin = new Vector2(0f, 0.4f);
            manaRect.anchorMax = new Vector2(1f, 0.6f);
            manaRect.offsetMin = Vector2.zero;
            manaRect.offsetMax = Vector2.zero;
            
            // Store references
            if (isPlayer)
            {
                playerHeroHP = hpText;
                playerHeroMana = manaText;
            }
            else
            {
                opponentHeroHP = hpText;
                opponentHeroMana = manaText;
            }
        }
        
        private void CreatePlayerHand(Transform parent)
        {
            GameObject handArea = CreatePanel("Player Hand", parent);
            handArea.GetComponent<Image>().color = Color.clear;
            
            RectTransform handRect = handArea.GetComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.05f, 0.05f);
            handRect.anchorMax = new Vector2(0.95f, 0.5f);
            handRect.offsetMin = Vector2.zero;
            handRect.offsetMax = Vector2.zero;
            
            playerHandArea = handArea.transform;
            
            // Add horizontal layout for cards
            HorizontalLayoutGroup layout = handArea.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateOpponentHand(Transform parent)
        {
            GameObject handArea = CreatePanel("Opponent Hand", parent);
            handArea.GetComponent<Image>().color = Color.clear;
            
            RectTransform handRect = handArea.GetComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.2f, 0.05f);
            handRect.anchorMax = new Vector2(0.8f, 0.4f);
            handRect.offsetMin = Vector2.zero;
            handRect.offsetMax = Vector2.zero;
            
            opponentHandArea = handArea.transform;
            
            // Show card backs for opponent
            for (int i = 0; i < 7; i++) // Max hand size
            {
                CreateCardBack(handArea.transform, i);
            }
        }
        
        private void CreateCardBack(Transform parent, int index)
        {
            GameObject cardBack = CreatePanel($"Card Back {index}", parent);
            Image cardImage = cardBack.GetComponent<Image>();
            cardImage.color = new Color(0.3f, 0.2f, 0.5f, 0.8f); // Purple card back
            
            RectTransform cardRect = cardBack.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(80, 120); // Card size
            
            // Card back text
            GameObject textObj = new GameObject("Card Back Text");
            textObj.transform.SetParent(cardBack.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI cardText = textObj.AddComponent<TextMeshProUGUI>();
            cardText.text = "🥔";
            cardText.fontSize = 32;
            cardText.color = new Color(1f, 0.8f, 0.2f, 1f);
            cardText.alignment = TextAlignmentOptions.Center;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
        }
        
        private void CreateBattleControls(Transform parent)
        {
            GameObject controlsPanel = CreatePanel("Battle Controls", parent);
            controlsPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            
            RectTransform controlsRect = controlsPanel.GetComponent<RectTransform>();
            controlsRect.anchorMin = new Vector2(0.75f, 0.35f);
            controlsRect.anchorMax = new Vector2(1f, 0.75f);
            controlsRect.offsetMin = Vector2.zero;
            controlsRect.offsetMax = Vector2.zero;
            
            // End Turn button
            endTurnButton = CreateControlButton(controlsPanel.transform, "End Turn", "⏭️\nEND TURN",
                new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.9f), new Color(0.2f, 0.6f, 0.9f, 1f));
            
            // Hero Power button
            heroPowerButton = CreateControlButton(controlsPanel.transform, "Hero Power", "✨\nHERO POWER",
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.65f), new Color(0.6f, 0.2f, 0.9f, 1f));
            
            // Battle Menu button
            battleMenuButton = CreateControlButton(controlsPanel.transform, "Menu", "📋\nMENU",
                new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.4f), new Color(0.5f, 0.5f, 0.5f, 1f));
            
            // Surrender button
            surrenderButton = CreateControlButton(controlsPanel.transform, "Surrender", "🏳️\nSURRENDER",
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.15f), new Color(0.8f, 0.3f, 0.3f, 1f));
        }
        
        private Button CreateControlButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject btnObj = CreatePanel(name, parent);
            Button button = btnObj.AddComponent<Button>();
            Image btnImage = btnObj.GetComponent<Image>();
            btnImage.color = color;
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            textObj.layer = 5;
            
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 18;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(textObj.GetComponent<RectTransform>());
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        private void CreateBattleInfo(Transform parent)
        {
            GameObject infoPanel = CreatePanel("Battle Info", parent);
            infoPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            
            RectTransform infoRect = infoPanel.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.75f, 0.75f);
            infoRect.anchorMax = new Vector2(1f, 0.95f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            
            // Turn indicator
            GameObject turnObj = new GameObject("Turn Indicator");
            turnObj.transform.SetParent(infoPanel.transform, false);
            turnObj.layer = 5;
            
            turnIndicatorText = turnObj.AddComponent<TextMeshProUGUI>();
            turnIndicatorText.text = "🎯 Your Turn";
            turnIndicatorText.fontSize = 20;
            turnIndicatorText.color = new Color(0.2f, 1f, 0.2f, 1f);
            turnIndicatorText.alignment = TextAlignmentOptions.Center;
            turnIndicatorText.fontStyle = FontStyles.Bold;
            
            RectTransform turnRect = turnObj.GetComponent<RectTransform>();
            turnRect.anchorMin = new Vector2(0f, 0.6f);
            turnRect.anchorMax = new Vector2(1f, 0.8f);
            turnRect.offsetMin = Vector2.zero;
            turnRect.offsetMax = Vector2.zero;
            
            // Turn timer
            GameObject timerObj = new GameObject("Turn Timer");
            timerObj.transform.SetParent(infoPanel.transform, false);
            timerObj.layer = 5;
            
            turnTimerText = timerObj.AddComponent<TextMeshProUGUI>();
            turnTimerText.text = "⏰ 60s";
            turnTimerText.fontSize = 24;
            turnTimerText.color = Color.white;
            turnTimerText.alignment = TextAlignmentOptions.Center;
            turnTimerText.fontStyle = FontStyles.Bold;
            
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0f, 0.3f);
            timerRect.anchorMax = new Vector2(1f, 0.6f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            
            // Game phase
            GameObject phaseObj = new GameObject("Game Phase");
            phaseObj.transform.SetParent(infoPanel.transform, false);
            phaseObj.layer = 5;
            
            gamePhaseText = phaseObj.AddComponent<TextMeshProUGUI>();
            gamePhaseText.text = "Main Phase";
            gamePhaseText.fontSize = 16;
            gamePhaseText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            gamePhaseText.alignment = TextAlignmentOptions.Center;
            
            RectTransform phaseRect = phaseObj.GetComponent<RectTransform>();
            phaseRect.anchorMin = new Vector2(0f, 0f);
            phaseRect.anchorMax = new Vector2(1f, 0.3f);
            phaseRect.offsetMin = Vector2.zero;
            phaseRect.offsetMax = Vector2.zero;
        }
        
        private void CreateBattleMenu(Transform parent)
        {
            battleMenuPanel = CreatePanel("Battle Menu", parent);
            Image menuBg = battleMenuPanel.GetComponent<Image>();
            menuBg.color = new Color(0f, 0f, 0f, 0.9f);
            
            SetFullScreen(battleMenuPanel.GetComponent<RectTransform>());
            battleMenuPanel.SetActive(false);
            
            // Menu title
            GameObject titleObj = new GameObject("Menu Title");
            titleObj.transform.SetParent(battleMenuPanel.transform, false);
            titleObj.layer = 5;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "⚔️ BATTLE MENU";
            titleText.fontSize = 48;
            titleText.color = new Color(1f, 0.8f, 0.2f, 1f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.7f);
            titleRect.anchorMax = new Vector2(0.9f, 0.85f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Menu buttons
            resumeButton = CreateControlButton(battleMenuPanel.transform, "Resume", "▶️ RESUME",
                new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.65f), new Color(0.2f, 0.8f, 0.2f, 1f));
            
            settingsButton = CreateControlButton(battleMenuPanel.transform, "Settings", "⚙️ SETTINGS",
                new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.45f), new Color(0.5f, 0.5f, 0.5f, 1f));
            
            forfeitButton = CreateControlButton(battleMenuPanel.transform, "Forfeit", "🏳️ FORFEIT",
                new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.25f), new Color(0.8f, 0.3f, 0.3f, 1f));
        }
        
        private void SetupEventListeners()
        {
            // Battle controls
            if (endTurnButton) endTurnButton.onClick.AddListener(OnEndTurnPressed);
            if (heroPowerButton) heroPowerButton.onClick.AddListener(OnHeroPowerPressed);
            if (battleMenuButton) battleMenuButton.onClick.AddListener(OnBattleMenuPressed);
            if (surrenderButton) surrenderButton.onClick.AddListener(OnSurrenderPressed);
            
            // Battle menu
            if (resumeButton) resumeButton.onClick.AddListener(OnResumePressed);
            if (settingsButton) settingsButton.onClick.AddListener(OnSettingsPressed);
            if (forfeitButton) forfeitButton.onClick.AddListener(OnForfeitPressed);
            
            // Subscribe to battle events
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
                BattleManager.Instance.OnTurnChanged += OnTurnChanged;
                BattleManager.Instance.OnManaChanged += OnManaChanged;
                BattleManager.Instance.OnTurnTimerChanged += OnTurnTimerChanged;
            }
        }
        
        public void InitializeBattle(string sessionId)
        {
            battleSessionId = sessionId;
            isInitialized = true;
            
            Debug.Log($"⚔️ Battle arena initialized: {sessionId}");
            
            // Show battle UI
            gameObject.SetActive(true);
            
            // Load battle data
            LoadBattleState();
        }
        
        private async void LoadBattleState()
        {
            // TODO: Load actual battle state from Supabase
            // For now, show placeholder data
            
            if (playerHeroHP) playerHeroHP.text = "❤️ 30/30";
            if (playerHeroMana) playerHeroMana.text = "💎 1/1";
            if (opponentHeroHP) opponentHeroHP.text = "❤️ 30/30";
            if (opponentHeroMana) opponentHeroMana.text = "💎 1/1";
            
            if (turnIndicatorText) turnIndicatorText.text = "🎯 Your Turn";
            if (turnTimerText) turnTimerText.text = "⏰ 60s";
            if (gamePhaseText) gamePhaseText.text = "Main Phase";
            
            Debug.Log("✅ Battle state loaded");
        }
        
        #region Event Handlers
        
        private void OnEndTurnPressed()
        {
            if (!isPlayerTurn) return;
            
            Debug.Log("⏭️ End turn pressed");
            BattleManager.Instance.EndTurn();
        }
        
        private void OnHeroPowerPressed()
        {
            Debug.Log("✨ Hero power pressed");
            // TODO: Implement hero power usage
        }
        
        private void OnBattleMenuPressed()
        {
            Debug.Log("📋 Battle menu pressed");
            if (battleMenuPanel) battleMenuPanel.SetActive(true);
        }
        
        private void OnSurrenderPressed()
        {
            Debug.Log("🏳️ Surrender pressed");
            OnForfeitPressed(); // Same as forfeit
        }
        
        private void OnResumePressed()
        {
            Debug.Log("▶️ Resume pressed");
            if (battleMenuPanel) battleMenuPanel.SetActive(false);
        }
        
        private void OnSettingsPressed()
        {
            Debug.Log("⚙️ Settings pressed");
            // TODO: Implement settings
        }
        
        private void OnForfeitPressed()
        {
            Debug.Log("🏳️ Forfeit pressed");
            
            // TODO: Implement forfeit functionality
            // For now, just exit battle
            ExitBattle();
        }
        
        private void ExitBattle()
        {
            gameObject.SetActive(false);
            
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.ChangeGameFlow(GameFlowManager.GameFlow.MainMenu);
            }
        }
        
        #endregion
        
        #region Battle Event Handlers
        
        private void OnBattleStateChanged(BattleManager.BattleState newState)
        {
            switch (newState)
            {
                case BattleManager.BattleState.InProgress:
                    gameObject.SetActive(true);
                    break;
                    
                case BattleManager.BattleState.Ended:
                case BattleManager.BattleState.NotInBattle:
                    ExitBattle();
                    break;
            }
        }
        
        private void OnTurnChanged(bool playerTurn)
        {
            isPlayerTurn = playerTurn;
            
            if (turnIndicatorText)
            {
                turnIndicatorText.text = playerTurn ? "🎯 Your Turn" : "⏳ Opponent's Turn";
                turnIndicatorText.color = playerTurn ? 
                    new Color(0.2f, 1f, 0.2f, 1f) : // Green
                    new Color(1f, 0.6f, 0.2f, 1f);  // Orange
            }
            
            if (endTurnButton)
            {
                endTurnButton.interactable = playerTurn;
            }
        }
        
        private void OnManaChanged(int currentMana)
        {
            if (playerHeroMana)
            {
                playerHeroMana.text = $"💎 {currentMana}/10";
            }
        }
        
        private void OnTurnTimerChanged(float timeRemaining)
        {
            if (turnTimerText)
            {
                int seconds = Mathf.CeilToInt(timeRemaining);
                turnTimerText.text = $"⏰ {seconds}s";
                
                // Change color based on time remaining
                if (seconds <= 10)
                    turnTimerText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red
                else if (seconds <= 30)
                    turnTimerText.color = new Color(1f, 0.8f, 0.2f, 1f); // Yellow
                else
                    turnTimerText.color = Color.white;
            }
        }
        
        #endregion
        
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
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
                BattleManager.Instance.OnTurnChanged -= OnTurnChanged;
                BattleManager.Instance.OnManaChanged -= OnManaChanged;
                BattleManager.Instance.OnTurnTimerChanged -= OnTurnTimerChanged;
            }
        }
    }
}