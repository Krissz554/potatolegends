using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PotatoCardGame.Data;
using PotatoCardGame.Cards;
using PotatoCardGame.Network;
using PotatoCardGame.Core;
using PotatoCardGame.UI;

namespace PotatoCardGame.Battle
{
    /// <summary>
    /// Manages turn-based battle mechanics, game state, and real-time synchronization
    /// Handles all battle logic including card playing, combat, and win conditions
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("Battle UI")]
        [SerializeField] private Transform playerHandArea;
        [SerializeField] private Transform playerBattlefield;
        [SerializeField] private Transform opponentBattlefield;
        [SerializeField] private Transform opponentHandArea;
        
        [Header("Hero Areas")]
        [SerializeField] private Transform playerHeroArea;
        [SerializeField] private Transform opponentHeroArea;
        
        [Header("Battle Settings")]
        [SerializeField] private int maxHandSize = 7;
        [SerializeField] private int maxBattlefieldSlots = 5;
        [SerializeField] private float turnTimer = 60f;
        [SerializeField] private int startingMana = 1;
        [SerializeField] private int maxMana = 10;
        
        // Singleton
        public static BattleManager Instance { get; private set; }
        
        // Battle state
        private BattleState currentBattleState = BattleState.NotInBattle;
        private string battleSessionId;
        private string playerId;
        private string opponentId;
        private bool isPlayerTurn = false;
        private int currentTurn = 1;
        private int playerMana = 1;
        private int opponentMana = 1;
        private float currentTurnTime = 60f;
        
        // Card collections
        private List<CardData> playerHand = new List<CardData>();
        private List<CardData> playerDeck = new List<CardData>();
        private List<CardData> playerBattlefieldCards = new List<CardData>();
        private List<CardData> opponentBattlefieldCards = new List<CardData>();
        
        // Hero data
        private HeroData playerHero;
        private HeroData opponentHero;
        
        // Events
        public System.Action<BattleState> OnBattleStateChanged;
        public System.Action<bool> OnTurnChanged;
        public System.Action<int> OnManaChanged;
        public System.Action<float> OnTurnTimerChanged;
        public System.Action<string> OnBattleEnded;
        
        public enum BattleState
        {
            NotInBattle,
            Searching,
            Initializing,
            Mulligan,
            InProgress,
            Ended
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            Debug.Log("⚔️ Battle Manager Initialized");
            
            // Get player ID from authentication
            if (SupabaseClient.Instance != null && SupabaseClient.Instance.IsAuthenticated)
            {
                playerId = SupabaseClient.Instance.GetUserId();
            }
        }
        
        private void Update()
        {
            // Update turn timer
            if (currentBattleState == BattleState.InProgress && isPlayerTurn)
            {
                currentTurnTime -= Time.deltaTime;
                OnTurnTimerChanged?.Invoke(currentTurnTime);
                
                if (currentTurnTime <= 0)
                {
                    EndTurn();
                }
            }
        }
        
        #region Battle Flow
        
        public async Task<bool> StartMatchmaking()
        {
            try
            {
                Debug.Log("🔍 Starting matchmaking...");
                
                SetBattleState(BattleState.Searching);
                
                // Join matchmaking queue via Supabase
                var joinData = new { user_id = playerId };
                bool success = await SupabaseClient.Instance.PostData("matchmaking_queue", joinData);
                
                if (success)
                {
                    Debug.Log("✅ Joined matchmaking queue");
                    StartCoroutine(PollForMatch());
                    return true;
                }
                else
                {
                    Debug.LogError("❌ Failed to join matchmaking");
                    SetBattleState(BattleState.NotInBattle);
                    return false;
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Matchmaking error: {e.Message}");
                SetBattleState(BattleState.NotInBattle);
                return false;
            }
        }
        
        private IEnumerator PollForMatch()
        {
            while (currentBattleState == BattleState.Searching)
            {
                yield return new WaitForSeconds(2f);
                
                // Start async check without await in coroutine
                StartCoroutine(CheckForMatchAsync());
            }
        }
        
        private IEnumerator CheckForMatchAsync()
        {
            // Simple polling without complex async/await in coroutines
            Debug.Log("🔍 Checking for match...");
            
            // For now, just continue searching
            // TODO: Implement proper match checking
            yield return null;
        }
        
        public async Task InitializeBattle(BattleSession battleSession)
        {
            try
            {
                Debug.Log($"🎯 Initializing battle: {battleSession.id}");
                
                SetBattleState(BattleState.Initializing);
                
                battleSessionId = battleSession.id;
                opponentId = battleSession.player1_id == playerId ? battleSession.player2_id : battleSession.player1_id;
                
                // Load player deck
                await LoadPlayerDeck();
                
                // Initialize battle UI
                InitializeBattleUI();
                
                // Draw starting hand
                DrawStartingHand();
                
                // Set initial turn
                isPlayerTurn = battleSession.current_turn_player_id == playerId;
                currentTurn = battleSession.turn_number;
                playerMana = startingMana;
                
                SetBattleState(BattleState.Mulligan);
                
                Debug.Log("✅ Battle initialized successfully");
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Battle initialization error: {e.Message}");
                SetBattleState(BattleState.NotInBattle);
            }
        }
        
        #endregion
        
        #region Card Operations
        
        public bool CanPlayCard(CardData card)
        {
            if (!isPlayerTurn) return false;
            if (card.manaCost > playerMana) return false;
            if (currentBattleState != BattleState.InProgress) return false;
            
            // Check battlefield space for units
            if (card.cardType == CardType.Unit && playerBattlefieldCards.Count >= maxBattlefieldSlots)
                return false;
                
            return true;
        }
        
        public async Task<bool> PlayCard(CardData card, int targetIndex = -1)
        {
            if (!CanPlayCard(card))
            {
                Debug.LogWarning($"⚠️ Cannot play card: {card.cardName}");
                return false;
            }
            
            try
            {
                Debug.Log($"🎴 Playing card: {card.cardName}");
                
                // Spend mana
                playerMana -= card.manaCost;
                OnManaChanged?.Invoke(playerMana);
                
                // Remove from hand
                playerHand.Remove(card);
                
                // Handle card based on type
                switch (card.cardType)
                {
                    case CardType.Unit:
                        PlayUnitCard(card);
                        break;
                    case CardType.Spell:
                        PlaySpellCard(card, targetIndex);
                        break;
                    case CardType.Structure:
                        PlayStructureCard(card);
                        break;
                }
                
                // Send to server
                await SendCardPlayToServer(card, targetIndex);
                
                // Update UI
                UpdateBattleUI();
                
                return true;
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error playing card: {e.Message}");
                return false;
            }
        }
        
        private void PlayUnitCard(CardData card)
        {
            // Add to battlefield
            playerBattlefieldCards.Add(card);
            
            // Create visual card on battlefield
            GameObject cardObject = CardManager.Instance.CreateCardObject(card, playerBattlefield);
            
            Debug.Log($"🛡️ Unit played: {card.cardName} ({card.attack}/{card.health})");
        }
        
        private void PlaySpellCard(CardData card, int targetIndex)
        {
            // Execute spell effects
            Debug.Log($"✨ Spell cast: {card.cardName}");
            
            // TODO: Implement spell targeting and effects
        }
        
        private void PlayStructureCard(CardData card)
        {
            // Add structure to battlefield
            playerBattlefieldCards.Add(card);
            
            Debug.Log($"🏗️ Structure built: {card.cardName}");
        }
        
        #endregion
        
        #region Turn Management
        
        public void EndTurn()
        {
            if (!isPlayerTurn) return;
            
            Debug.Log("🔄 Ending turn...");
            
            isPlayerTurn = false;
            currentTurn++;
            currentTurnTime = turnTimer;
            
            // Increase mana
            playerMana = Mathf.Min(maxMana, currentTurn);
            OnManaChanged?.Invoke(playerMana);
            
            // Draw card
            DrawCard();
            
            // Send turn end to server
            SendTurnEndToServer();
            
            OnTurnChanged?.Invoke(isPlayerTurn);
        }
        
        private void StartTurn()
        {
            Debug.Log("▶️ Starting turn...");
            
            isPlayerTurn = true;
            currentTurnTime = turnTimer;
            
            // Reset card attack flags
            foreach (CardData card in playerBattlefieldCards)
            {
                card.hasAttackedThisTurn = false;
            }
            
            OnTurnChanged?.Invoke(isPlayerTurn);
        }
        
        #endregion
        
        #region Deck and Hand Management
        
        private async Task LoadPlayerDeck()
        {
            try
            {
                // Get player's active deck from database
                var deckResponse = await SupabaseClient.Instance.GetData<List<PlayerDeck>>(
                    $"decks?user_id=eq.{playerId}&is_active=eq.true&select=*"
                );
                
                if (deckResponse != null && deckResponse.Count > 0)
                {
                    var deck = deckResponse[0];
                    
                    // Load deck cards
                    var deckCardsResponse = await SupabaseClient.Instance.GetData<List<DeckCard>>(
                        $"deck_cards?deck_id=eq.{deck.id}&select=*,card_complete(*)"
                    );
                    
                    if (deckCardsResponse != null)
                    {
                        playerDeck.Clear();
                        foreach (var deckCard in deckCardsResponse)
                        {
                            CardData cardData = CardManager.Instance.GetCardData(deckCard.card_id);
                            if (cardData != null)
                            {
                                // Add multiple copies based on quantity
                                for (int i = 0; i < deckCard.quantity; i++)
                                {
                                    playerDeck.Add(cardData.CreateRuntimeCopy());
                                }
                            }
                        }
                        
                        // Shuffle deck
                        ShuffleDeck();
                        
                        Debug.Log($"🎴 Deck loaded: {playerDeck.Count} cards");
                    }
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading deck: {e.Message}");
            }
        }
        
        private void ShuffleDeck()
        {
            for (int i = 0; i < playerDeck.Count; i++)
            {
                CardData temp = playerDeck[i];
                int randomIndex = Random.Range(i, playerDeck.Count);
                playerDeck[i] = playerDeck[randomIndex];
                playerDeck[randomIndex] = temp;
            }
            
            Debug.Log("🔀 Deck shuffled");
        }
        
        private void DrawStartingHand()
        {
            int startingHandSize = 4; // Standard starting hand
            
            for (int i = 0; i < startingHandSize && playerDeck.Count > 0; i++)
            {
                DrawCard();
            }
            
            Debug.Log($"✋ Starting hand drawn: {playerHand.Count} cards");
        }
        
        private void DrawCard()
        {
            if (playerDeck.Count == 0)
            {
                // Fatigue damage
                Debug.Log("💀 Deck empty - fatigue damage!");
                return;
            }
            
            if (playerHand.Count >= maxHandSize)
            {
                // Hand full - card burned
                Debug.Log("🔥 Hand full - card burned!");
                playerDeck.RemoveAt(0);
                return;
            }
            
            // Draw card
            CardData drawnCard = playerDeck[0];
            playerDeck.RemoveAt(0);
            playerHand.Add(drawnCard);
            
            // Create visual card in hand
            GameObject cardObject = CardManager.Instance.CreateCardObject(drawnCard, playerHandArea);
            
            Debug.Log($"📤 Card drawn: {drawnCard.cardName}");
        }
        
        #endregion
        
        #region Network Communication
        
        private async Task SendCardPlayToServer(CardData card, int targetIndex)
        {
            try
            {
                var playData = new
                {
                    battle_session_id = battleSessionId,
                    player_id = playerId,
                    card_id = card.cardId,
                    target_index = targetIndex,
                    action_type = "play_card"
                };
                
                await SupabaseClient.Instance.PostData("battle_actions", playData);
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error sending card play: {e.Message}");
            }
        }
        
        private async Task SendTurnEndToServer()
        {
            try
            {
                var turnData = new
                {
                    battle_session_id = battleSessionId,
                    player_id = playerId,
                    action_type = "end_turn"
                };
                
                await SupabaseClient.Instance.PostData("battle_actions", turnData);
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error sending turn end: {e.Message}");
            }
        }
        
        #endregion
        
        #region UI Updates
        
        private void InitializeBattleUI()
        {
            // Switch to battle screen
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen(GameManager.GameState.Battle);
            }
            
            UpdateBattleUI();
        }
        
        private void UpdateBattleUI()
        {
            // Update mana display
            OnManaChanged?.Invoke(playerMana);
            
            // Update turn indicator
            OnTurnChanged?.Invoke(isPlayerTurn);
            
            // Update timer
            OnTurnTimerChanged?.Invoke(currentTurnTime);
        }
        
        #endregion
        
        #region Public Interface
        
        public void SetBattleState(BattleState newState)
        {
            if (currentBattleState == newState) return;
            
            BattleState previousState = currentBattleState;
            currentBattleState = newState;
            
            Debug.Log($"⚔️ Battle State: {previousState} → {newState}");
            OnBattleStateChanged?.Invoke(newState);
        }
        
        public bool IsInBattle()
        {
            return currentBattleState == BattleState.InProgress;
        }
        
        public bool IsPlayerTurn()
        {
            return isPlayerTurn && currentBattleState == BattleState.InProgress;
        }
        
        public int GetPlayerMana()
        {
            return playerMana;
        }
        
        public List<CardData> GetPlayerHand()
        {
            return new List<CardData>(playerHand);
        }
        
        public void CancelMatchmaking()
        {
            SetBattleState(BattleState.NotInBattle);
            StopAllCoroutines();
            Debug.Log("❌ Matchmaking cancelled");
        }
        
        #endregion
        
        #region Data Models
        
        [System.Serializable]
        public class BattleSession
        {
            public string id;
            public string player1_id;
            public string player2_id;
            public string current_turn_player_id;
            public string status;
            public int turn_number;
            public string game_state;
        }
        
        [System.Serializable]
        public class PlayerDeck
        {
            public string id;
            public string user_id;
            public string name;
            public bool is_active;
        }
        
        [System.Serializable]
        public class DeckCard
        {
            public string deck_id;
            public string card_id;
            public int quantity;
        }
        
        [System.Serializable]
        public class HeroData
        {
            public string hero_id;
            public string name;
            public int current_hp;
            public int max_hp;
            public int current_mana;
            public int max_mana;
            public bool hero_power_available;
            public string hero_power_name;
            public string hero_power_description;
        }
        
        #endregion
    }
}