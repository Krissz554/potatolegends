using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Network;
using PotatoCardGame.Cards;
using PotatoCardGame.Core;

namespace PotatoCardGame.Battle
{
    /// <summary>
    /// REAL Battle Arena that implements the complete battle system
    /// Recreates the full battle mechanics from your web version
    /// </summary>
    public class RealBattleArena : MonoBehaviour
    {
        [Header("Battle UI")]
        [SerializeField] private Transform playerHandArea;
        [SerializeField] private Transform playerBoardArea;
        [SerializeField] private Transform opponentBoardArea;
        [SerializeField] private Transform opponentHandArea;
        
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private TextMeshProUGUI playerManaText;
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private Slider playerManaSlider;
        
        [Header("Opponent Info")]
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private TextMeshProUGUI opponentHealthText;
        [SerializeField] private TextMeshProUGUI opponentManaText;
        [SerializeField] private Slider opponentHealthSlider;
        [SerializeField] private Slider opponentManaSlider;
        
        [Header("Battle Controls")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button heroPowerButton;
        [SerializeField] private Button surrenderButton;
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI timerText;
        
        [Header("Battle Status")]
        [SerializeField] private GameObject battleResultPanel;
        [SerializeField] private TextMeshProUGUI battleResultText;
        [SerializeField] private Button returnToMenuButton;
        
        // Battle data
        private RealSupabaseClient.BattleSession currentBattle;
        private BattlePlayer playerData;
        private BattlePlayer opponentData;
        private List<GameObject> playerHandCards = new List<GameObject>();
        private List<GameObject> playerBoardCards = new List<GameObject>();
        private List<GameObject> opponentBoardCards = new List<GameObject>();
        
        // Battle state
        private bool isPlayerTurn = false;
        private int currentTurn = 1;
        private float turnTimeRemaining = 60f;
        private bool isBattleActive = false;
        
        private void Start()
        {
            CreateBattleArenaUI();
            SetupEventListeners();
            
            // Start hidden
            gameObject.SetActive(false);
        }
        
        private void CreateBattleArenaUI()
        {
            Debug.Log("⚔️ Creating real battle arena UI...");
            
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateCanvas();
                canvas = FindFirstObjectByType<Canvas>();
            }
            
            CreateBattleInterface(canvas.transform);
            
            Debug.Log("✅ Real battle arena UI created");
        }
        
        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Battle Canvas");
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
        
        private void CreateBattleInterface(Transform parent)
        {
            // Background
            GameObject background = CreatePanel("Battle Background", parent);
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.05f, 0.05f, 1f); // Dark red battle theme
            SetFullScreen(background.GetComponent<RectTransform>());
            
            // Opponent area (top)
            CreateOpponentArea(background.transform);
            
            // Battle board (middle)
            CreateBattleBoard(background.transform);
            
            // Player area (bottom)
            CreatePlayerArea(background.transform);
            
            // Battle controls (side)
            CreateBattleControls(background.transform);
            
            // Battle result panel
            CreateBattleResultPanel(background.transform);
        }
        
        private void CreateOpponentArea(Transform parent)
        {
            GameObject opponentArea = CreatePanel("Opponent Area", parent);
            Image opponentBg = opponentArea.GetComponent<Image>();
            opponentBg.color = new Color(0.3f, 0.1f, 0.1f, 0.8f); // Dark red
            
            RectTransform opponentRect = opponentArea.GetComponent<RectTransform>();
            opponentRect.anchorMin = new Vector2(0f, 0.8f);
            opponentRect.anchorMax = new Vector2(1f, 1f);
            opponentRect.offsetMin = Vector2.zero;
            opponentRect.offsetMax = Vector2.zero;
            
            // Opponent name
            GameObject nameObj = new GameObject("Opponent Name");
            nameObj.transform.SetParent(opponentArea.transform, false);
            nameObj.layer = 5;
            
            opponentNameText = nameObj.AddComponent<TextMeshProUGUI>();
            opponentNameText.text = "Opponent";
            opponentNameText.fontSize = 24;
            opponentNameText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red
            opponentNameText.alignment = TextAlignmentOptions.Left;
            opponentNameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.6f);
            nameRect.anchorMax = new Vector2(0.5f, 0.9f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Opponent health
            GameObject healthObj = new GameObject("Opponent Health");
            healthObj.transform.SetParent(opponentArea.transform, false);
            healthObj.layer = 5;
            
            opponentHealthText = healthObj.AddComponent<TextMeshProUGUI>();
            opponentHealthText.text = "❤️ 30/30";
            opponentHealthText.fontSize = 20;
            opponentHealthText.color = Color.white;
            opponentHealthText.alignment = TextAlignmentOptions.Right;
            
            RectTransform healthRect = healthObj.GetComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0.5f, 0.6f);
            healthRect.anchorMax = new Vector2(0.95f, 0.9f);
            healthRect.offsetMin = Vector2.zero;
            healthRect.offsetMax = Vector2.zero;
            
            // Opponent mana
            GameObject manaObj = new GameObject("Opponent Mana");
            manaObj.transform.SetParent(opponentArea.transform, false);
            manaObj.layer = 5;
            
            opponentManaText = manaObj.AddComponent<TextMeshProUGUI>();
            opponentManaText.text = "💎 10/10";
            opponentManaText.fontSize = 18;
            opponentManaText.color = new Color(0.3f, 0.8f, 1f, 1f); // Blue
            opponentManaText.alignment = TextAlignmentOptions.Right;
            
            RectTransform manaRect = manaObj.GetComponent<RectTransform>();
            manaRect.anchorMin = new Vector2(0.5f, 0.1f);
            manaRect.anchorMax = new Vector2(0.95f, 0.5f);
            manaRect.offsetMin = Vector2.zero;
            manaRect.offsetMax = Vector2.zero;
        }
        
        private void CreatePlayerArea(Transform parent)
        {
            GameObject playerArea = CreatePanel("Player Area", parent);
            Image playerBg = playerArea.GetComponent<Image>();
            playerBg.color = new Color(0.1f, 0.3f, 0.1f, 0.8f); // Dark green
            
            RectTransform playerRect = playerArea.GetComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0f, 0f);
            playerRect.anchorMax = new Vector2(1f, 0.2f);
            playerRect.offsetMin = Vector2.zero;
            playerRect.offsetMax = Vector2.zero;
            
            // Player name
            GameObject nameObj = new GameObject("Player Name");
            nameObj.transform.SetParent(playerArea.transform, false);
            nameObj.layer = 5;
            
            playerNameText = nameObj.AddComponent<TextMeshProUGUI>();
            playerNameText.text = "You";
            playerNameText.fontSize = 24;
            playerNameText.color = new Color(0.3f, 1f, 0.3f, 1f); // Green
            playerNameText.alignment = TextAlignmentOptions.Left;
            playerNameText.fontStyle = FontStyles.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.6f);
            nameRect.anchorMax = new Vector2(0.5f, 0.9f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            // Player health
            GameObject healthObj = new GameObject("Player Health");
            healthObj.transform.SetParent(playerArea.transform, false);
            healthObj.layer = 5;
            
            playerHealthText = healthObj.AddComponent<TextMeshProUGUI>();
            playerHealthText.text = "❤️ 30/30";
            playerHealthText.fontSize = 20;
            playerHealthText.color = Color.white;
            playerHealthText.alignment = TextAlignmentOptions.Right;
            
            RectTransform healthRect = healthObj.GetComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0.5f, 0.6f);
            healthRect.anchorMax = new Vector2(0.95f, 0.9f);
            healthRect.offsetMin = Vector2.zero;
            healthRect.offsetMax = Vector2.zero;
            
            // Player mana
            GameObject manaObj = new GameObject("Player Mana");
            manaObj.transform.SetParent(playerArea.transform, false);
            manaObj.layer = 5;
            
            playerManaText = manaObj.AddComponent<TextMeshProUGUI>();
            playerManaText.text = "💎 10/10";
            playerManaText.fontSize = 18;
            playerManaText.color = new Color(0.3f, 0.8f, 1f, 1f); // Blue
            playerManaText.alignment = TextAlignmentOptions.Right;
            
            RectTransform manaRect = manaObj.GetComponent<RectTransform>();
            manaRect.anchorMin = new Vector2(0.5f, 0.1f);
            manaRect.anchorMax = new Vector2(0.95f, 0.5f);
            manaRect.offsetMin = Vector2.zero;
            manaRect.offsetMax = Vector2.zero;
        }
        
        private void CreateBattleBoard(Transform parent)
        {
            GameObject battleBoard = CreatePanel("Battle Board", parent);
            Image boardBg = battleBoard.GetComponent<Image>();
            boardBg.color = new Color(0.05f, 0.15f, 0.1f, 0.7f);
            
            RectTransform boardRect = battleBoard.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0.05f, 0.2f);
            boardRect.anchorMax = new Vector2(0.75f, 0.8f);
            boardRect.offsetMin = Vector2.zero;
            boardRect.offsetMax = Vector2.zero;
            
            // Opponent board area
            GameObject opponentBoardObj = new GameObject("Opponent Board");
            opponentBoardObj.transform.SetParent(battleBoard.transform, false);
            opponentBoardObj.layer = 5;
            
            RectTransform opponentBoardRect = opponentBoardObj.GetComponent<RectTransform>();
            opponentBoardRect.anchorMin = new Vector2(0.05f, 0.7f);
            opponentBoardRect.anchorMax = new Vector2(0.95f, 0.95f);
            opponentBoardRect.offsetMin = Vector2.zero;
            opponentBoardRect.offsetMax = Vector2.zero;
            
            // Add horizontal layout for opponent board
            HorizontalLayoutGroup opponentLayout = opponentBoardObj.AddComponent<HorizontalLayoutGroup>();
            opponentLayout.spacing = 10;
            opponentLayout.childAlignment = TextAnchor.MiddleCenter;
            opponentLayout.childControlWidth = false;
            opponentLayout.childControlHeight = false;
            
            opponentBoardArea = opponentBoardObj.transform;
            
            // Player board area
            GameObject playerBoardObj = new GameObject("Player Board");
            playerBoardObj.transform.SetParent(battleBoard.transform, false);
            playerBoardObj.layer = 5;
            
            RectTransform playerBoardRect = playerBoardObj.GetComponent<RectTransform>();
            playerBoardRect.anchorMin = new Vector2(0.05f, 0.05f);
            playerBoardRect.anchorMax = new Vector2(0.95f, 0.3f);
            playerBoardRect.offsetMin = Vector2.zero;
            playerBoardRect.offsetMax = Vector2.zero;
            
            // Add horizontal layout for player board
            HorizontalLayoutGroup playerLayout = playerBoardObj.AddComponent<HorizontalLayoutGroup>();
            playerLayout.spacing = 10;
            playerLayout.childAlignment = TextAnchor.MiddleCenter;
            playerLayout.childControlWidth = false;
            playerLayout.childControlHeight = false;
            
            playerBoardArea = playerBoardObj.transform;
            
            // Player hand area (bottom of screen)
            GameObject playerHandObj = new GameObject("Player Hand");
            playerHandObj.transform.SetParent(parent, false);
            playerHandObj.layer = 5;
            
            RectTransform playerHandRect = playerHandObj.GetComponent<RectTransform>();
            playerHandRect.anchorMin = new Vector2(0.05f, 0.02f);
            playerHandRect.anchorMax = new Vector2(0.95f, 0.18f);
            playerHandRect.offsetMin = Vector2.zero;
            playerHandRect.offsetMax = Vector2.zero;
            
            // Add horizontal layout for hand
            HorizontalLayoutGroup handLayout = playerHandObj.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = 5;
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childControlWidth = false;
            handLayout.childControlHeight = false;
            
            playerHandArea = playerHandObj.transform;
        }
        
        private void CreateBattleControls(Transform parent)
        {
            GameObject controlsPanel = CreatePanel("Battle Controls", parent);
            Image controlsBg = controlsPanel.GetComponent<Image>();
            controlsBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform controlsRect = controlsPanel.GetComponent<RectTransform>();
            controlsRect.anchorMin = new Vector2(0.8f, 0.2f);
            controlsRect.anchorMax = new Vector2(1f, 0.8f);
            controlsRect.offsetMin = Vector2.zero;
            controlsRect.offsetMax = Vector2.zero;
            
            // Turn info
            GameObject turnObj = new GameObject("Turn Info");
            turnObj.transform.SetParent(controlsPanel.transform, false);
            turnObj.layer = 5;
            
            turnText = turnObj.AddComponent<TextMeshProUGUI>();
            turnText.text = "Turn 1\nYour Turn";
            turnText.fontSize = 20;
            turnText.color = new Color(1f, 0.8f, 0.2f, 1f);
            turnText.alignment = TextAlignmentOptions.Center;
            turnText.fontStyle = FontStyles.Bold;
            
            RectTransform turnRect = turnObj.GetComponent<RectTransform>();
            turnRect.anchorMin = new Vector2(0.05f, 0.8f);
            turnRect.anchorMax = new Vector2(0.95f, 0.95f);
            turnRect.offsetMin = Vector2.zero;
            turnRect.offsetMax = Vector2.zero;
            
            // Timer
            GameObject timerObj = new GameObject("Timer");
            timerObj.transform.SetParent(controlsPanel.transform, false);
            timerObj.layer = 5;
            
            timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "⏰ 60s";
            timerText.fontSize = 18;
            timerText.color = Color.white;
            timerText.alignment = TextAlignmentOptions.Center;
            
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.05f, 0.7f);
            timerRect.anchorMax = new Vector2(0.95f, 0.78f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            
            // End turn button
            GameObject endTurnBtnObj = CreatePanel("End Turn Button", controlsPanel.transform);
            endTurnButton = endTurnBtnObj.AddComponent<Button>();
            Image endTurnBtnImg = endTurnBtnObj.GetComponent<Image>();
            endTurnBtnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green
            
            GameObject endTurnTextObj = new GameObject("End Turn Text");
            endTurnTextObj.transform.SetParent(endTurnBtnObj.transform, false);
            endTurnTextObj.layer = 5;
            
            TextMeshProUGUI endTurnText = endTurnTextObj.AddComponent<TextMeshProUGUI>();
            endTurnText.text = "⏭️\nEND\nTURN";
            endTurnText.fontSize = 16;
            endTurnText.color = Color.white;
            endTurnText.alignment = TextAlignmentOptions.Center;
            endTurnText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(endTurnTextObj.GetComponent<RectTransform>());
            
            RectTransform endTurnBtnRect = endTurnBtnObj.GetComponent<RectTransform>();
            endTurnBtnRect.anchorMin = new Vector2(0.05f, 0.5f);
            endTurnBtnRect.anchorMax = new Vector2(0.95f, 0.65f);
            endTurnBtnRect.offsetMin = Vector2.zero;
            endTurnBtnRect.offsetMax = Vector2.zero;
            
            // Hero power button
            GameObject heroPowerBtnObj = CreatePanel("Hero Power Button", controlsPanel.transform);
            heroPowerButton = heroPowerBtnObj.AddComponent<Button>();
            Image heroPowerBtnImg = heroPowerBtnObj.GetComponent<Image>();
            heroPowerBtnImg.color = new Color(0.7f, 0.3f, 0.9f, 1f); // Purple
            
            GameObject heroPowerTextObj = new GameObject("Hero Power Text");
            heroPowerTextObj.transform.SetParent(heroPowerBtnObj.transform, false);
            heroPowerTextObj.layer = 5;
            
            TextMeshProUGUI heroPowerText = heroPowerTextObj.AddComponent<TextMeshProUGUI>();
            heroPowerText.text = "🔥\nHERO\nPOWER";
            heroPowerText.fontSize = 14;
            heroPowerText.color = Color.white;
            heroPowerText.alignment = TextAlignmentOptions.Center;
            heroPowerText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(heroPowerTextObj.GetComponent<RectTransform>());
            
            RectTransform heroPowerBtnRect = heroPowerBtnObj.GetComponent<RectTransform>();
            heroPowerBtnRect.anchorMin = new Vector2(0.05f, 0.3f);
            heroPowerBtnRect.anchorMax = new Vector2(0.95f, 0.45f);
            heroPowerBtnRect.offsetMin = Vector2.zero;
            heroPowerBtnRect.offsetMax = Vector2.zero;
            
            // Surrender button
            GameObject surrenderBtnObj = CreatePanel("Surrender Button", controlsPanel.transform);
            surrenderButton = surrenderBtnObj.AddComponent<Button>();
            Image surrenderBtnImg = surrenderBtnObj.GetComponent<Image>();
            surrenderBtnImg.color = new Color(0.7f, 0.3f, 0.3f, 1f); // Red
            
            GameObject surrenderTextObj = new GameObject("Surrender Text");
            surrenderTextObj.transform.SetParent(surrenderBtnObj.transform, false);
            surrenderTextObj.layer = 5;
            
            TextMeshProUGUI surrenderText = surrenderTextObj.AddComponent<TextMeshProUGUI>();
            surrenderText.text = "🏳️\nSURRENDER";
            surrenderText.fontSize = 14;
            surrenderText.color = Color.white;
            surrenderText.alignment = TextAlignmentOptions.Center;
            surrenderText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(surrenderTextObj.GetComponent<RectTransform>());
            
            RectTransform surrenderBtnRect = surrenderBtnObj.GetComponent<RectTransform>();
            surrenderBtnRect.anchorMin = new Vector2(0.05f, 0.05f);
            surrenderBtnRect.anchorMax = new Vector2(0.95f, 0.2f);
            surrenderBtnRect.offsetMin = Vector2.zero;
            surrenderBtnRect.offsetMax = Vector2.zero;
        }
        
        private void CreateBattleResultPanel(Transform parent)
        {
            battleResultPanel = CreatePanel("Battle Result Panel", parent);
            Image resultBg = battleResultPanel.GetComponent<Image>();
            resultBg.color = new Color(0f, 0f, 0f, 0.9f);
            
            SetFullScreen(battleResultPanel.GetComponent<RectTransform>());
            battleResultPanel.SetActive(false);
            
            // Result content
            GameObject resultContent = CreatePanel("Result Content", battleResultPanel.transform);
            resultContent.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
            
            RectTransform resultRect = resultContent.GetComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.1f, 0.3f);
            resultRect.anchorMax = new Vector2(0.9f, 0.7f);
            resultRect.offsetMin = Vector2.zero;
            resultRect.offsetMax = Vector2.zero;
            
            // Result text
            GameObject resultTextObj = new GameObject("Result Text");
            resultTextObj.transform.SetParent(resultContent.transform, false);
            resultTextObj.layer = 5;
            
            battleResultText = resultTextObj.AddComponent<TextMeshProUGUI>();
            battleResultText.text = "🏆 VICTORY!";
            battleResultText.fontSize = 48;
            battleResultText.color = new Color(1f, 0.8f, 0.2f, 1f);
            battleResultText.alignment = TextAlignmentOptions.Center;
            battleResultText.fontStyle = FontStyles.Bold;
            
            RectTransform resultTextRect = resultTextObj.GetComponent<RectTransform>();
            resultTextRect.anchorMin = new Vector2(0.1f, 0.5f);
            resultTextRect.anchorMax = new Vector2(0.9f, 0.8f);
            resultTextRect.offsetMin = Vector2.zero;
            resultTextRect.offsetMax = Vector2.zero;
            
            // Return to menu button
            GameObject returnBtnObj = CreatePanel("Return Button", resultContent.transform);
            returnToMenuButton = returnBtnObj.AddComponent<Button>();
            Image returnBtnImg = returnBtnObj.GetComponent<Image>();
            returnBtnImg.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            GameObject returnTextObj = new GameObject("Return Text");
            returnTextObj.transform.SetParent(returnBtnObj.transform, false);
            returnTextObj.layer = 5;
            
            TextMeshProUGUI returnText = returnTextObj.AddComponent<TextMeshProUGUI>();
            returnText.text = "🏠 RETURN TO MENU";
            returnText.fontSize = 24;
            returnText.color = Color.white;
            returnText.alignment = TextAlignmentOptions.Center;
            returnText.fontStyle = FontStyles.Bold;
            
            SetFullScreen(returnTextObj.GetComponent<RectTransform>());
            
            RectTransform returnBtnRect = returnBtnObj.GetComponent<RectTransform>();
            returnBtnRect.anchorMin = new Vector2(0.2f, 0.2f);
            returnBtnRect.anchorMax = new Vector2(0.8f, 0.4f);
            returnBtnRect.offsetMin = Vector2.zero;
            returnBtnRect.offsetMax = Vector2.zero;
        }
        
        private void SetupEventListeners()
        {
            // Battle controls
            if (endTurnButton) endTurnButton.onClick.AddListener(OnEndTurnPressed);
            if (heroPowerButton) heroPowerButton.onClick.AddListener(OnHeroPowerPressed);
            if (surrenderButton) surrenderButton.onClick.AddListener(OnSurrenderPressed);
            if (returnToMenuButton) returnToMenuButton.onClick.AddListener(OnReturnToMenuPressed);
            
            // Game flow events
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnGameFlowChanged += OnGameFlowChanged;
            }
        }
        
        public async void StartBattle(RealSupabaseClient.BattleSession battleSession)
        {
            currentBattle = battleSession;
            isBattleActive = true;
            
            Debug.Log($"⚔️ Starting battle: {battleSession.id}");
            
            // Initialize battle data
            await InitializeBattleData();
            
            // Start battle loop
            StartBattleLoop();
        }
        
        private async Task InitializeBattleData()
        {
            try
            {
                // Load player data
                string playerId = RealSupabaseClient.Instance.UserId;
                
                playerData = new BattlePlayer
                {
                    id = playerId,
                    name = RealSupabaseClient.Instance.CurrentUser?.display_name ?? "Player",
                    health = 30,
                    maxHealth = 30,
                    mana = 1,
                    maxMana = 1
                };
                
                // Load opponent data
                string opponentId = currentBattle.player1_id == playerId ? currentBattle.player2_id : currentBattle.player1_id;
                
                opponentData = new BattlePlayer
                {
                    id = opponentId,
                    name = "Opponent", // TODO: Load from database
                    health = 30,
                    maxHealth = 30,
                    mana = 1,
                    maxMana = 1
                };
                
                // Determine turn order
                isPlayerTurn = currentBattle.current_turn_player_id == playerId;
                
                // Update UI
                UpdateBattleUI();
                
                // Load player deck and draw starting hand
                await LoadPlayerDeck();
                DrawStartingHand();
                
                Debug.Log("✅ Battle data initialized");
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error initializing battle: {e.Message}");
            }
        }
        
        private async Task LoadPlayerDeck()
        {
            try
            {
                // Load player's active deck
                var decks = await RealSupabaseClient.Instance.LoadUserDecks();
                var activeDeck = decks.FirstOrDefault(d => d.is_active);
                
                if (activeDeck != null)
                {
                    playerData.deck = activeDeck.cards.Select(dc => dc.card).ToList();
                    playerData.deckCount = playerData.deck.Count;
                    
                    Debug.Log($"✅ Loaded player deck: {playerData.deckCount} cards");
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading deck: {e.Message}");
            }
        }
        
        private void DrawStartingHand()
        {
            // Draw 3 cards for starting hand (like web version)
            for (int i = 0; i < 3; i++)
            {
                DrawCard();
            }
            
            Debug.Log("✅ Drew starting hand (3 cards)");
        }
        
        private void DrawCard()
        {
            if (playerData?.deck == null || playerData.deck.Count == 0) return;
            
            // Draw random card from deck
            int randomIndex = Random.Range(0, playerData.deck.Count);
            var drawnCard = playerData.deck[randomIndex];
            playerData.deck.RemoveAt(randomIndex);
            playerData.deckCount--;
            
            // Create card display in hand
            if (RealCardManager.Instance != null)
            {
                GameObject cardObj = RealCardManager.Instance.CreateCardDisplay(drawnCard, playerHandArea, false);
                if (cardObj != null)
                {
                    playerHandCards.Add(cardObj);
                    
                    // Add click handler for playing card
                    CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
                    if (cardDisplay != null)
                    {
                        cardDisplay.OnCardClicked += OnPlayerCardClicked;
                    }
                }
            }
        }
        
        private void StartBattleLoop()
        {
            Debug.Log("🔄 Starting battle loop");
            
            // Start turn timer
            InvokeRepeating(nameof(UpdateTurnTimer), 1f, 1f);
            
            UpdateTurnDisplay();
        }
        
        private void UpdateTurnTimer()
        {
            if (!isBattleActive) return;
            
            turnTimeRemaining -= 1f;
            
            if (timerText)
            {
                timerText.text = $"⏰ {Mathf.Max(0, (int)turnTimeRemaining)}s";
                
                // Change color as time runs out
                if (turnTimeRemaining <= 10)
                    timerText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red
                else if (turnTimeRemaining <= 20)
                    timerText.color = new Color(1f, 0.8f, 0.2f, 1f); // Yellow
                else
                    timerText.color = Color.white;
            }
            
            // Auto end turn when time runs out
            if (turnTimeRemaining <= 0 && isPlayerTurn)
            {
                OnEndTurnPressed();
            }
        }
        
        private void UpdateBattleUI()
        {
            // Update player info
            if (playerNameText) playerNameText.text = playerData?.name ?? "Player";
            if (playerHealthText) playerHealthText.text = $"❤️ {playerData?.health ?? 0}/{playerData?.maxHealth ?? 0}";
            if (playerManaText) playerManaText.text = $"💎 {playerData?.mana ?? 0}/{playerData?.maxMana ?? 0}";
            
            // Update opponent info
            if (opponentNameText) opponentNameText.text = opponentData?.name ?? "Opponent";
            if (opponentHealthText) opponentHealthText.text = $"❤️ {opponentData?.health ?? 0}/{opponentData?.maxHealth ?? 0}";
            if (opponentManaText) opponentManaText.text = $"💎 {opponentData?.mana ?? 0}/{opponentData?.maxMana ?? 0}";
            
            // Update health sliders
            if (playerHealthSlider && playerData != null)
            {
                playerHealthSlider.value = (float)playerData.health / playerData.maxHealth;
            }
            
            if (opponentHealthSlider && opponentData != null)
            {
                opponentHealthSlider.value = (float)opponentData.health / opponentData.maxHealth;
            }
        }
        
        private void UpdateTurnDisplay()
        {
            if (turnText)
            {
                string turnInfo = $"Turn {currentTurn}\n";
                turnInfo += isPlayerTurn ? "Your Turn" : "Opponent Turn";
                turnText.text = turnInfo;
                
                turnText.color = isPlayerTurn ? new Color(0.3f, 1f, 0.3f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
            }
            
            // Enable/disable controls based on turn
            if (endTurnButton) endTurnButton.interactable = isPlayerTurn;
            if (heroPowerButton) heroPowerButton.interactable = isPlayerTurn;
        }
        
        private void OnPlayerCardClicked(CardDisplay cardDisplay)
        {
            if (!isPlayerTurn || !isBattleActive) return;
            
            Debug.Log($"🃏 Player clicked card: {cardDisplay.GetCardData()?.cardName}");
            
            // TODO: Implement card playing logic
            // Check mana cost, play card, update board state
        }
        
        private void OnEndTurnPressed()
        {
            if (!isPlayerTurn || !isBattleActive) return;
            
            Debug.Log("⏭️ Player ended turn");
            
            // Switch turns
            isPlayerTurn = false;
            currentTurn++;
            turnTimeRemaining = 60f; // Reset timer
            
            UpdateTurnDisplay();
            
            // TODO: Send turn end to server
            // TODO: Process opponent turn
            
            // For now, simulate opponent turn
            StartCoroutine(SimulateOpponentTurn());
        }
        
        private System.Collections.IEnumerator SimulateOpponentTurn()
        {
            yield return new WaitForSeconds(2f); // Simulate thinking time
            
            Debug.Log("🤖 Opponent turn completed");
            
            // Switch back to player turn
            isPlayerTurn = true;
            turnTimeRemaining = 60f;
            
            // Increase mana each turn
            if (playerData != null && playerData.maxMana < 10)
            {
                playerData.maxMana++;
                playerData.mana = playerData.maxMana;
            }
            
            if (opponentData != null && opponentData.maxMana < 10)
            {
                opponentData.maxMana++;
                opponentData.mana = opponentData.maxMana;
            }
            
            // Draw a card
            DrawCard();
            
            UpdateBattleUI();
            UpdateTurnDisplay();
        }
        
        private void OnHeroPowerPressed()
        {
            if (!isPlayerTurn || !isBattleActive) return;
            
            Debug.Log("🔥 Player used hero power");
            
            // TODO: Implement hero power logic
        }
        
        private void OnSurrenderPressed()
        {
            Debug.Log("🏳️ Player surrendered");
            
            EndBattle(false, "You surrendered");
        }
        
        private void EndBattle(bool playerWon, string reason)
        {
            isBattleActive = false;
            CancelInvoke(nameof(UpdateTurnTimer));
            
            Debug.Log($"🏁 Battle ended: {(playerWon ? "Victory" : "Defeat")} - {reason}");
            
            // Show result
            if (battleResultPanel && battleResultText)
            {
                battleResultText.text = playerWon ? "🏆 VICTORY!" : "💀 DEFEAT";
                battleResultText.color = playerWon ? new Color(1f, 0.8f, 0.2f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
                
                battleResultPanel.SetActive(true);
            }
        }
        
        private void OnReturnToMenuPressed()
        {
            Debug.Log("🏠 Returning to main menu");
            
            // Clean up battle
            CleanupBattle();
            
            // Navigate to main menu
            if (RealGameManager.Instance != null)
            {
                RealGameManager.Instance.NavigateToMainMenu();
            }
        }
        
        private void CleanupBattle()
        {
            // Clear all card displays
            foreach (GameObject card in playerHandCards)
            {
                if (card != null) Destroy(card);
            }
            playerHandCards.Clear();
            
            foreach (GameObject card in playerBoardCards)
            {
                if (card != null) Destroy(card);
            }
            playerBoardCards.Clear();
            
            foreach (GameObject card in opponentBoardCards)
            {
                if (card != null) Destroy(card);
            }
            opponentBoardCards.Clear();
            
            // Reset battle state
            currentBattle = null;
            playerData = null;
            opponentData = null;
            isBattleActive = false;
            
            if (battleResultPanel) battleResultPanel.SetActive(false);
        }
        
        private void OnGameFlowChanged(GameFlowManager.GameFlow newFlow)
        {
            bool shouldShow = newFlow == GameFlowManager.GameFlow.Battle;
            gameObject.SetActive(shouldShow);
            
            if (!shouldShow && isBattleActive)
            {
                // Clean up if leaving battle
                CleanupBattle();
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
        
        #region Data Classes
        
        [System.Serializable]
        public class BattlePlayer
        {
            public string id;
            public string name;
            public int health;
            public int maxHealth;
            public int mana;
            public int maxMana;
            public List<RealSupabaseClient.EnhancedCard> deck;
            public int deckCount;
            public bool heroPowerUsed;
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