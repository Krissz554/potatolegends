using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using PotatoCardGame.Data;
using PotatoCardGame.Battle;
using PotatoCardGame.Network;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Mobile-optimized battle interface
    /// Handles turn-based combat UI with touch controls and visual feedback
    /// </summary>
    public class MobileBattleUI : MonoBehaviour
    {
        [Header("Battle UI Panels")]
        [SerializeField] private GameObject battlePanel;
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private GameObject mulliganPanel;
        [SerializeField] private GameObject gameOverPanel;
        
        [Header("Player Areas")]
        [SerializeField] private Transform playerHandArea;
        [SerializeField] private Transform playerBattlefieldArea;
        [SerializeField] private Transform opponentBattlefieldArea;
        [SerializeField] private Transform opponentHandArea;
        
        [Header("Hero Displays")]
        [SerializeField] private Image playerHeroPortrait;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private TextMeshProUGUI playerManaText;
        [SerializeField] private Image opponentHeroPortrait;
        [SerializeField] private TextMeshProUGUI opponentHealthText;
        [SerializeField] private TextMeshProUGUI opponentManaText;
        
        [Header("Turn Management")]
        [SerializeField] private TextMeshProUGUI turnIndicatorText;
        [SerializeField] private Slider turnTimerSlider;
        [SerializeField] private TextMeshProUGUI turnTimerText;
        [SerializeField] private Button endTurnButton;
        
        [Header("Game Actions")]
        [SerializeField] private Button surrenderButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button heroPowerButton;
        
        [Header("Matchmaking UI")]
        [SerializeField] private TextMeshProUGUI matchmakingStatusText;
        [SerializeField] private Button cancelMatchmakingButton;
        [SerializeField] private Slider matchmakingProgress;
        
        [Header("Mulligan UI")]
        [SerializeField] private Transform mulliganHandArea;
        [SerializeField] private Button confirmMulliganButton;
        [SerializeField] private Button keepHandButton;
        [SerializeField] private TextMeshProUGUI mulliganInstructionText;
        
        [Header("Game Over UI")]
        [SerializeField] private TextMeshProUGUI gameResultText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button returnToMenuButton;
        
        [Header("Mobile Optimization")]
        [SerializeField] private float cardAnimationDuration = 0.3f;
        [SerializeField] private float handCardSpacing = 10f;
        [SerializeField] private float battlefieldCardSpacing = 5f;
        
        // Card UI management
        private List<MobileCardUI> playerHandCards = new List<MobileCardUI>();
        private List<MobileCardUI> playerBattlefieldCards = new List<MobileCardUI>();
        private List<MobileCardUI> opponentBattlefieldCards = new List<MobileCardUI>();
        private List<MobileCardUI> opponentHandCards = new List<MobileCardUI>();
        private List<MobileCardUI> mulliganCards = new List<MobileCardUI>();
        
        // State
        private bool isLocalPlayerTurn = false;
        private List<GameCard> selectedMulliganCards = new List<GameCard>();
        private MobileCardUI selectedCard = null;
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }
        
        private void InitializeUI()
        {
            // Hide all panels initially
            HideAllPanels();
            
            // Setup hand layout for mobile
            SetupHandLayout();
            SetupBattlefieldLayout();
        }
        
        private void HideAllPanels()
        {
            if (battlePanel) battlePanel.SetActive(false);
            if (matchmakingPanel) matchmakingPanel.SetActive(false);
            if (mulliganPanel) mulliganPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
        }
        
        private void SetupHandLayout()
        {
            if (playerHandArea)
            {
                var layoutGroup = playerHandArea.GetComponent<HorizontalLayoutGroup>();
                if (!layoutGroup) layoutGroup = playerHandArea.gameObject.AddComponent<HorizontalLayoutGroup>();
                
                layoutGroup.spacing = handCardSpacing;
                layoutGroup.childControlWidth = false;
                layoutGroup.childControlHeight = false;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
            }
        }
        
        private void SetupBattlefieldLayout()
        {
            // Setup player battlefield
            if (playerBattlefieldArea)
            {
                var layoutGroup = playerBattlefieldArea.GetComponent<HorizontalLayoutGroup>();
                if (!layoutGroup) layoutGroup = playerBattlefieldArea.gameObject.AddComponent<HorizontalLayoutGroup>();
                
                layoutGroup.spacing = battlefieldCardSpacing;
                layoutGroup.childControlWidth = false;
                layoutGroup.childControlHeight = false;
            }
            
            // Setup opponent battlefield
            if (opponentBattlefieldArea)
            {
                var layoutGroup = opponentBattlefieldArea.GetComponent<HorizontalLayoutGroup>();
                if (!layoutGroup) layoutGroup = opponentBattlefieldArea.gameObject.AddComponent<HorizontalLayoutGroup>();
                
                layoutGroup.spacing = battlefieldCardSpacing;
                layoutGroup.childControlWidth = false;
                layoutGroup.childControlHeight = false;
            }
        }
        
        private void SetupEventListeners()
        {
            // Battle actions
            if (endTurnButton) endTurnButton.onClick.AddListener(OnEndTurnClicked);
            if (surrenderButton) surrenderButton.onClick.AddListener(OnSurrenderClicked);
            if (heroPowerButton) heroPowerButton.onClick.AddListener(OnHeroPowerClicked);
            if (menuButton) menuButton.onClick.AddListener(OnMenuClicked);
            
            // Matchmaking
            if (cancelMatchmakingButton) cancelMatchmakingButton.onClick.AddListener(OnCancelMatchmaking);
            
            // Mulligan
            if (confirmMulliganButton) confirmMulliganButton.onClick.AddListener(OnConfirmMulligan);
            if (keepHandButton) keepHandButton.onClick.AddListener(OnKeepHand);
            
            // Game over
            if (playAgainButton) playAgainButton.onClick.AddListener(OnPlayAgain);
            if (returnToMenuButton) returnToMenuButton.onClick.AddListener(OnReturnToMenu);
            
            // Battle manager events
            if (BattleManager.Instance)
            {
                BattleManager.Instance.OnBattlePhaseChanged += OnBattlePhaseChanged;
                BattleManager.Instance.OnTurnChanged += OnTurnChanged;
                BattleManager.Instance.OnTurnTimeUpdated += OnTurnTimeUpdated;
                BattleManager.Instance.OnBattleMessage += OnBattleMessage;
            }
            
            // Realtime service events
            if (RealtimeBattleService.Instance)
            {
                RealtimeBattleService.Instance.OnMatchFound += OnMatchFound;
                RealtimeBattleService.Instance.OnBattleStateUpdated += OnBattleStateUpdated;
                RealtimeBattleService.Instance.OnBattleEnded += OnBattleEnded;
            }
        }
        
        #region Matchmaking UI
        
        public async void StartMatchmaking()
        {
            ShowMatchmakingPanel();
            
            bool success = await RealtimeBattleService.Instance.JoinMatchmaking();
            
            if (!success)
            {
                HideMatchmakingPanel();
            }
        }
        
        private void ShowMatchmakingPanel()
        {
            HideAllPanels();
            if (matchmakingPanel) matchmakingPanel.SetActive(true);
            
            if (matchmakingStatusText) matchmakingStatusText.text = "Searching for opponent...";
            if (matchmakingProgress) matchmakingProgress.value = 0f;
            
            // Animate progress bar
            if (matchmakingProgress)
            {
                matchmakingProgress.DOValue(1f, 30f).SetLoops(-1, LoopType.Restart);
            }
        }
        
        private void HideMatchmakingPanel()
        {
            if (matchmakingPanel) matchmakingPanel.SetActive(false);
            if (matchmakingProgress) matchmakingProgress.DOKill();
        }
        
        #endregion
        
        #region Battle UI
        
        private void ShowBattlePanel()
        {
            HideAllPanels();
            if (battlePanel) battlePanel.SetActive(true);
        }
        
        private void UpdatePlayerDisplay(BattlePlayer player, bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                if (playerHealthText) playerHealthText.text = player.GetHeroHealth().ToString();
                if (playerManaText) playerManaText.text = $"{player.GetCurrentMana()}/{player.GetMaxMana()}";
            }
            else
            {
                if (opponentHealthText) opponentHealthText.text = player.GetHeroHealth().ToString();
                if (opponentManaText) opponentManaText.text = $"{player.GetCurrentMana()}/{player.GetMaxMana()}";
            }
        }
        
        private void UpdateHandDisplay(List<GameCard> hand, bool isLocalPlayer)
        {
            var handArea = isLocalPlayer ? playerHandArea : opponentHandArea;
            var handCards = isLocalPlayer ? playerHandCards : opponentHandCards;
            
            if (!handArea) return;
            
            // Clear existing hand
            foreach (var cardUI in handCards)
            {
                if (cardUI && cardUI.gameObject) DestroyImmediate(cardUI.gameObject);
            }
            handCards.Clear();
            
            // Create new hand cards
            foreach (var card in hand)
            {
                CreateHandCard(card, handArea, handCards, isLocalPlayer);
            }
        }
        
        private void CreateHandCard(GameCard card, Transform parent, List<MobileCardUI> cardList, bool isLocalPlayer)
        {
            // TODO: Use card prefab to create card UI
            GameObject cardObj = new GameObject($"Card_{card.name}");
            cardObj.transform.SetParent(parent);
            
            var cardUI = cardObj.AddComponent<MobileCardUI>();
            cardUI.SetCardData(card);
            
            if (isLocalPlayer)
            {
                cardUI.OnCardTapped += OnPlayerCardTapped;
                cardUI.SetInteractable(true);
            }
            else
            {
                // Opponent cards show as card backs
                cardUI.SetInteractable(false);
            }
            
            cardList.Add(cardUI);
        }
        
        #endregion
        
        #region Mulligan UI
        
        private void ShowMulliganPanel(List<GameCard> startingHand)
        {
            HideAllPanels();
            if (mulliganPanel) mulliganPanel.SetActive(true);
            
            if (mulliganInstructionText) 
                mulliganInstructionText.text = "Tap cards to mulligan them";
            
            // Display starting hand for mulligan
            DisplayMulliganHand(startingHand);
        }
        
        private void DisplayMulliganHand(List<GameCard> hand)
        {
            // Clear existing mulligan cards
            foreach (var cardUI in mulliganCards)
            {
                if (cardUI && cardUI.gameObject) DestroyImmediate(cardUI.gameObject);
            }
            mulliganCards.Clear();
            
            // Create mulligan cards
            foreach (var card in hand)
            {
                CreateMulliganCard(card);
            }
        }
        
        private void CreateMulliganCard(GameCard card)
        {
            // TODO: Create mulligan card UI
            GameObject cardObj = new GameObject($"MulliganCard_{card.name}");
            cardObj.transform.SetParent(mulliganHandArea);
            
            var cardUI = cardObj.AddComponent<MobileCardUI>();
            cardUI.SetCardData(card);
            cardUI.OnCardTapped += OnMulliganCardTapped;
            
            mulliganCards.Add(cardUI);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnMatchFound(string battleSessionId)
        {
            HideMatchmakingPanel();
            
            // TODO: Initialize battle with session ID
            Debug.Log($"🎉 Match found! Starting battle: {battleSessionId}");
        }
        
        private void OnBattlePhaseChanged(BattleManager.BattlePhase phase)
        {
            switch (phase)
            {
                case BattleManager.BattlePhase.Mulligan:
                    // Show mulligan UI
                    var startingHand = BattleManager.Instance.GetLocalPlayer().GetHand();
                    ShowMulliganPanel(startingHand);
                    break;
                    
                case BattleManager.BattlePhase.Battle:
                    ShowBattlePanel();
                    UpdateBattleDisplay();
                    break;
                    
                case BattleManager.BattlePhase.GameOver:
                    ShowGameOverPanel();
                    break;
            }
        }
        
        private void OnTurnChanged(bool isLocalTurn)
        {
            isLocalPlayerTurn = isLocalTurn;
            
            if (turnIndicatorText)
                turnIndicatorText.text = isLocalTurn ? "Your Turn" : "Opponent's Turn";
            
            if (endTurnButton) endTurnButton.interactable = isLocalTurn;
            
            // Update card interactability
            foreach (var cardUI in playerHandCards)
            {
                cardUI.SetInteractable(isLocalTurn);
            }
            
            Debug.Log($"🔄 Turn changed. Local player: {isLocalTurn}");
        }
        
        private void OnTurnTimeUpdated(float timeRemaining)
        {
            if (turnTimerSlider)
            {
                turnTimerSlider.value = timeRemaining / 60f; // Assuming 60 second turns
            }
            
            if (turnTimerText)
            {
                turnTimerText.text = Mathf.Ceil(timeRemaining).ToString();
            }
            
            // Visual warning when time is low
            if (timeRemaining <= 10f && turnTimerText)
            {
                turnTimerText.color = Color.red;
                turnTimerText.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
            }
            else if (turnTimerText)
            {
                turnTimerText.color = Color.white;
            }
        }
        
        private void OnBattleMessage(string message)
        {
            // TODO: Show message toast or notification
            Debug.Log($"💬 Battle message: {message}");
        }
        
        private void OnPlayerCardTapped(MobileCardUI cardUI)
        {
            if (!isLocalPlayerTurn) return;
            
            var card = cardUI.GetCardData();
            
            // Check if player can afford the card
            var localPlayer = BattleManager.Instance.GetLocalPlayer();
            if (localPlayer.GetCurrentMana() < card.mana_cost)
            {
                Debug.LogWarning($"⚠️ Not enough mana to play {card.name}");
                return;
            }
            
            // Select card for playing
            SelectCard(cardUI);
        }
        
        private void SelectCard(MobileCardUI cardUI)
        {
            // Deselect previous card
            if (selectedCard) selectedCard.SetSelected(false);
            
            // Select new card
            selectedCard = cardUI;
            cardUI.SetSelected(true);
            
            Debug.Log($"🎯 Selected card: {cardUI.GetCardData().name}");
            
            // TODO: Show valid play areas or target indicators
        }
        
        private void OnMulliganCardTapped(MobileCardUI cardUI)
        {
            var card = cardUI.GetCardData();
            
            if (selectedMulliganCards.Contains(card))
            {
                selectedMulliganCards.Remove(card);
                cardUI.SetSelected(false);
            }
            else
            {
                selectedMulliganCards.Add(card);
                cardUI.SetSelected(true);
            }
            
            Debug.Log($"🔄 Mulligan card selected: {card.name}. Total selected: {selectedMulliganCards.Count}");
        }
        
        private void OnBattleStateUpdated(BattleSession battleSession)
        {
            // Update UI based on server battle state
            UpdateBattleDisplay();
        }
        
        private void OnBattleEnded(string winnerId)
        {
            string localUserId = GetCurrentUserId();
            bool isWinner = winnerId == localUserId;
            
            ShowGameOverPanel(isWinner ? "Victory!" : "Defeat!");
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnEndTurnClicked()
        {
            if (!isLocalPlayerTurn) return;
            
            BattleManager.Instance.EndTurn();
            Debug.Log("⏭️ End turn clicked");
        }
        
        private void OnSurrenderClicked()
        {
            // TODO: Show confirmation dialog
            Debug.Log("🏳️ Surrender clicked");
        }
        
        private void OnHeroPowerClicked()
        {
            // TODO: Implement hero power usage
            Debug.Log("⚡ Hero power clicked");
        }
        
        private void OnMenuClicked()
        {
            // TODO: Show battle menu (settings, surrender, etc.)
            Debug.Log("📋 Menu clicked");
        }
        
        private void OnCancelMatchmaking()
        {
            RealtimeBattleService.Instance.LeaveMatchmaking();
            HideMatchmakingPanel();
        }
        
        private void OnConfirmMulligan()
        {
            // TODO: Send mulligan choices to server
            Debug.Log($"🔄 Confirmed mulligan: {selectedMulliganCards.Count} cards");
            
            // Hide mulligan panel and start battle
            if (mulliganPanel) mulliganPanel.SetActive(false);
            BattleManager.Instance.ChangeBattlePhase(BattleManager.BattlePhase.Battle);
        }
        
        private void OnKeepHand()
        {
            selectedMulliganCards.Clear();
            OnConfirmMulligan();
        }
        
        private void OnPlayAgain()
        {
            StartMatchmaking();
        }
        
        private void OnReturnToMenu()
        {
            // Return to main menu
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Display Updates
        
        private void UpdateBattleDisplay()
        {
            var localPlayer = BattleManager.Instance.GetLocalPlayer();
            var opponentPlayer = BattleManager.Instance.GetOpponentPlayer();
            
            if (localPlayer != null)
            {
                UpdatePlayerDisplay(localPlayer, true);
                UpdateHandDisplay(localPlayer.GetHand(), true);
                UpdateBattlefieldDisplay(localPlayer.GetBattlefield(), true);
            }
            
            if (opponentPlayer != null)
            {
                UpdatePlayerDisplay(opponentPlayer, false);
                UpdateHandDisplay(opponentPlayer.GetHand(), false);
                UpdateBattlefieldDisplay(opponentPlayer.GetBattlefield(), false);
            }
        }
        
        private void UpdateBattlefieldDisplay(List<GameCard> battlefield, bool isLocalPlayer)
        {
            var battlefieldArea = isLocalPlayer ? playerBattlefieldArea : opponentBattlefieldArea;
            var battlefieldCards = isLocalPlayer ? playerBattlefieldCards : opponentBattlefieldCards;
            
            if (!battlefieldArea) return;
            
            // Clear existing battlefield
            foreach (var cardUI in battlefieldCards)
            {
                if (cardUI && cardUI.gameObject) DestroyImmediate(cardUI.gameObject);
            }
            battlefieldCards.Clear();
            
            // Create battlefield cards
            foreach (var card in battlefield)
            {
                CreateBattlefieldCard(card, battlefieldArea, battlefieldCards, isLocalPlayer);
            }
        }
        
        private void CreateBattlefieldCard(GameCard card, Transform parent, List<MobileCardUI> cardList, bool isLocalPlayer)
        {
            GameObject cardObj = new GameObject($"BattlefieldCard_{card.name}");
            cardObj.transform.SetParent(parent);
            
            var cardUI = cardObj.AddComponent<MobileCardUI>();
            cardUI.SetCardData(card);
            
            if (isLocalPlayer)
            {
                cardUI.OnCardTapped += OnBattlefieldCardTapped;
                cardUI.SetInteractable(card.CanAttack() && isLocalPlayerTurn);
            }
            else
            {
                cardUI.SetInteractable(false);
            }
            
            cardList.Add(cardUI);
        }
        
        private void OnBattlefieldCardTapped(MobileCardUI cardUI)
        {
            // TODO: Handle battlefield card selection (for attacking)
            Debug.Log($"⚔️ Battlefield card tapped: {cardUI.GetCardData().name}");
        }
        
        #endregion
        
        #region Game Over
        
        private void ShowGameOverPanel(string result = "")
        {
            HideAllPanels();
            if (gameOverPanel) gameOverPanel.SetActive(true);
            
            if (gameResultText) gameResultText.text = result;
        }
        
        #endregion
        
        #region Public Methods
        
        public void ShowBattleUI()
        {
            gameObject.SetActive(true);
            StartMatchmaking();
        }
        
        public void HideBattleUI()
        {
            gameObject.SetActive(false);
            
            // Cleanup any active matchmaking or battles
            if (RealtimeBattleService.Instance.IsSearchingForMatch())
            {
                RealtimeBattleService.Instance.LeaveMatchmaking();
            }
        }
        
        #endregion
        
        #region Utility
        
        private string GetCurrentUserId()
        {
            // TODO: Get from authentication
            return "placeholder-user-id";
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Cleanup event listeners
            if (BattleManager.Instance)
            {
                BattleManager.Instance.OnBattlePhaseChanged -= OnBattlePhaseChanged;
                BattleManager.Instance.OnTurnChanged -= OnTurnChanged;
                BattleManager.Instance.OnTurnTimeUpdated -= OnTurnTimeUpdated;
                BattleManager.Instance.OnBattleMessage -= OnBattleMessage;
            }
            
            if (RealtimeBattleService.Instance)
            {
                RealtimeBattleService.Instance.OnMatchFound -= OnMatchFound;
                RealtimeBattleService.Instance.OnBattleStateUpdated -= OnBattleStateUpdated;
                RealtimeBattleService.Instance.OnBattleEnded -= OnBattleEnded;
            }
        }
    }
}