using System.Collections.Generic;
using UnityEngine;
using PotatoCardGame.Data;
using PotatoCardGame.Network;
using PotatoCardGame.Core;

namespace PotatoCardGame.Battle
{
    /// <summary>
    /// Core battle system manager for turn-based card combat
    /// Handles game state, turn management, and battle flow
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("Battle Configuration")]
        [SerializeField] private float turnTimeLimit = 60f;
        [SerializeField] private int maxHandSize = 10;
        [SerializeField] private int startingHandSize = 4;
        [SerializeField] private int maxMana = 10;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        public static BattleManager Instance { get; private set; }
        
        // Battle state
        public enum BattlePhase
        {
            WaitingForPlayers,
            Mulligan,
            Battle,
            GameOver
        }
        
        public enum TurnPhase
        {
            Start,
            Main,
            End
        }
        
        [SerializeField] private BattlePhase currentBattlePhase = BattlePhase.WaitingForPlayers;
        [SerializeField] private TurnPhase currentTurnPhase = TurnPhase.Start;
        
        // Players
        private BattlePlayer localPlayer;
        private BattlePlayer opponentPlayer;
        private bool isLocalPlayerTurn = false;
        
        // Game state
        private int currentTurn = 1;
        private float turnTimeRemaining;
        private string battleSessionId;
        
        // Events
        public System.Action<BattlePhase> OnBattlePhaseChanged;
        public System.Action<TurnPhase> OnTurnPhaseChanged;
        public System.Action<bool> OnTurnChanged; // true if local player's turn
        public System.Action<float> OnTurnTimeUpdated;
        public System.Action<BattlePlayer> OnPlayerUpdated;
        public System.Action<string> OnBattleMessage;
        public System.Action<string> OnBattleError;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            turnTimeRemaining = turnTimeLimit;
            
            if (enableDebugLogs) Debug.Log("⚔️ Battle Manager Initialized");
        }
        
        private void Update()
        {
            // Update turn timer
            if (currentBattlePhase == BattlePhase.Battle && isLocalPlayerTurn)
            {
                UpdateTurnTimer();
            }
        }
        
        #region Battle Flow
        
        public void StartBattle(string sessionId, BattlePlayer player1, BattlePlayer player2, bool isPlayer1)
        {
            battleSessionId = sessionId;
            
            if (isPlayer1)
            {
                localPlayer = player1;
                opponentPlayer = player2;
            }
            else
            {
                localPlayer = player2;
                opponentPlayer = player1;
            }
            
            ChangeBattlePhase(BattlePhase.Mulligan);
            
            if (enableDebugLogs) Debug.Log($"⚔️ Battle started! Session: {sessionId}");
        }
        
        public void ChangeBattlePhase(BattlePhase newPhase)
        {
            if (currentBattlePhase == newPhase) return;
            
            var previousPhase = currentBattlePhase;
            currentBattlePhase = newPhase;
            
            OnBattlePhaseChanged?.Invoke(newPhase);
            
            // Handle phase-specific logic
            switch (newPhase)
            {
                case BattlePhase.Mulligan:
                    StartMulliganPhase();
                    break;
                case BattlePhase.Battle:
                    StartBattlePhase();
                    break;
                case BattlePhase.GameOver:
                    EndBattle();
                    break;
            }
            
            if (enableDebugLogs) Debug.Log($"🔄 Battle Phase: {previousPhase} → {newPhase}");
        }
        
        private void StartMulliganPhase()
        {
            // Draw starting hands
            localPlayer.DrawStartingHand(startingHandSize);
            opponentPlayer.DrawStartingHand(startingHandSize);
            
            OnBattleMessage?.Invoke("Choose cards to mulligan");
        }
        
        private void StartBattlePhase()
        {
            // Start first turn
            currentTurn = 1;
            isLocalPlayerTurn = true; // TODO: Determine who goes first
            
            StartTurn();
        }
        
        private void StartTurn()
        {
            ChangeTurnPhase(TurnPhase.Start);
            
            var currentPlayer = isLocalPlayerTurn ? localPlayer : opponentPlayer;
            
            // Start of turn effects
            currentPlayer.StartTurn(currentTurn);
            
            // Draw card
            if (currentTurn > 1) // No draw on first turn
            {
                currentPlayer.DrawCard();
            }
            
            // Reset turn timer
            turnTimeRemaining = turnTimeLimit;
            
            // Move to main phase
            ChangeTurnPhase(TurnPhase.Main);
            
            OnTurnChanged?.Invoke(isLocalPlayerTurn);
            OnBattleMessage?.Invoke(isLocalPlayerTurn ? "Your turn!" : "Opponent's turn");
            
            if (enableDebugLogs) Debug.Log($"🎯 Turn {currentTurn} started. Player: {(isLocalPlayerTurn ? "Local" : "Opponent")}");
        }
        
        public void EndTurn()
        {
            if (currentBattlePhase != BattlePhase.Battle || !isLocalPlayerTurn) return;
            
            ChangeTurnPhase(TurnPhase.End);
            
            // End of turn effects
            localPlayer.EndTurn();
            
            // Switch turns
            isLocalPlayerTurn = !isLocalPlayerTurn;
            currentTurn++;
            
            // Start next turn
            StartTurn();
        }
        
        private void ChangeTurnPhase(TurnPhase newPhase)
        {
            currentTurnPhase = newPhase;
            OnTurnPhaseChanged?.Invoke(newPhase);
        }
        
        private void UpdateTurnTimer()
        {
            turnTimeRemaining -= Time.deltaTime;
            OnTurnTimeUpdated?.Invoke(turnTimeRemaining);
            
            if (turnTimeRemaining <= 0f)
            {
                // Auto-end turn when time runs out
                EndTurn();
            }
        }
        
        private void EndBattle()
        {
            // Determine winner
            BattlePlayer winner = null;
            
            if (localPlayer.GetHeroHealth() <= 0)
                winner = opponentPlayer;
            else if (opponentPlayer.GetHeroHealth() <= 0)
                winner = localPlayer;
            
            string message = winner == localPlayer ? "Victory!" : "Defeat!";
            OnBattleMessage?.Invoke(message);
            
            if (enableDebugLogs) Debug.Log($"🏆 Battle ended. Winner: {winner?.GetPlayerName() ?? "Draw"}");
            
            // Return to main menu after delay
            Invoke(nameof(ReturnToMainMenu), 3f);
        }
        
        private void ReturnToMainMenu()
        {
            GameManager.Instance.ChangeGameState(GameManager.GameState.MainMenu);
        }
        
        #endregion
        
        #region Card Actions
        
        public bool PlayCard(GameCard card, Vector2 targetPosition)
        {
            if (currentBattlePhase != BattlePhase.Battle || !isLocalPlayerTurn) return false;
            
            // Check if player can afford the card
            if (localPlayer.GetCurrentMana() < card.mana_cost)
            {
                OnBattleError?.Invoke("Not enough mana!");
                return false;
            }
            
            // Remove card from hand and pay mana cost
            bool cardRemoved = localPlayer.RemoveCardFromHand(card);
            if (!cardRemoved)
            {
                OnBattleError?.Invoke("Card not in hand!");
                return false;
            }
            
            localPlayer.SpendMana(card.mana_cost);
            
            // Play the card based on type
            if (card.IsUnit())
            {
                PlayUnitCard(card, targetPosition);
            }
            else if (card.IsSpell())
            {
                PlaySpellCard(card, targetPosition);
            }
            else if (card.IsStructure())
            {
                PlayStructureCard(card, targetPosition);
            }
            
            if (enableDebugLogs) Debug.Log($"✨ Played card: {card.name}");
            return true;
        }
        
        private void PlayUnitCard(GameCard card, Vector2 position)
        {
            // Add unit to battlefield
            localPlayer.AddUnitToBattlefield(card, position);
            
            // Trigger card abilities
            TriggerCardAbilities(card, "OnPlay");
        }
        
        private void PlaySpellCard(GameCard card, Vector2 position)
        {
            // Execute spell effect
            ExecuteSpellEffect(card, position);
            
            // Add to graveyard
            localPlayer.AddCardToGraveyard(card);
        }
        
        private void PlayStructureCard(GameCard card, Vector2 position)
        {
            // Add structure to battlefield
            localPlayer.AddStructureToBattlefield(card, position);
            
            // Trigger card abilities
            TriggerCardAbilities(card, "OnPlay");
        }
        
        private void TriggerCardAbilities(GameCard card, string trigger)
        {
            // TODO: Implement ability system
            if (enableDebugLogs) Debug.Log($"🎭 Triggering abilities for {card.name}: {trigger}");
        }
        
        private void ExecuteSpellEffect(GameCard card, Vector2 position)
        {
            // TODO: Implement spell effects based on card.ability_text
            if (enableDebugLogs) Debug.Log($"✨ Executing spell: {card.name}");
        }
        
        #endregion
        
        #region Public Getters
        
        public BattlePhase GetCurrentBattlePhase() => currentBattlePhase;
        public TurnPhase GetCurrentTurnPhase() => currentTurnPhase;
        public bool IsLocalPlayerTurn() => isLocalPlayerTurn;
        public float GetTurnTimeRemaining() => turnTimeRemaining;
        public BattlePlayer GetLocalPlayer() => localPlayer;
        public BattlePlayer GetOpponentPlayer() => opponentPlayer;
        public int GetCurrentTurn() => currentTurn;
        
        #endregion
    }
    
    /// <summary>
    /// Represents a player in battle with their deck, hand, battlefield, etc.
    /// </summary>
    [System.Serializable]
    public class BattlePlayer
    {
        [Header("Player Info")]
        public string playerId;
        public string playerName;
        
        [Header("Hero")]
        public int heroHealth = 20;
        public int maxHeroHealth = 20;
        public int currentMana = 1;
        public int maxMana = 1;
        
        [Header("Cards")]
        public List<GameCard> deck = new List<GameCard>();
        public List<GameCard> hand = new List<GameCard>();
        public List<GameCard> battlefield = new List<GameCard>();
        public List<GameCard> graveyard = new List<GameCard>();
        
        public void StartTurn(int turnNumber)
        {
            // Increase mana
            maxMana = Mathf.Min(10, turnNumber);
            currentMana = maxMana;
            
            // Reset card states
            foreach (var card in battlefield)
            {
                card.has_attacked_this_turn = false;
                card.can_attack = true;
            }
        }
        
        public void EndTurn()
        {
            // End of turn effects
        }
        
        public void DrawStartingHand(int cardCount)
        {
            for (int i = 0; i < cardCount && deck.Count > 0; i++)
            {
                DrawCard();
            }
        }
        
        public GameCard DrawCard()
        {
            if (deck.Count == 0) return null;
            if (hand.Count >= 10) return null; // Hand size limit
            
            GameCard drawnCard = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnCard);
            
            return drawnCard;
        }
        
        public bool RemoveCardFromHand(GameCard card)
        {
            return hand.Remove(card);
        }
        
        public void SpendMana(int amount)
        {
            currentMana = Mathf.Max(0, currentMana - amount);
        }
        
        public void AddUnitToBattlefield(GameCard card, Vector2 position)
        {
            card.battlefield_position = position;
            card.InitializeForBattle();
            battlefield.Add(card);
        }
        
        public void AddStructureToBattlefield(GameCard card, Vector2 position)
        {
            card.battlefield_position = position;
            card.InitializeForBattle();
            battlefield.Add(card);
        }
        
        public void AddCardToGraveyard(GameCard card)
        {
            graveyard.Add(card);
        }
        
        public int GetHeroHealth() => heroHealth;
        public int GetCurrentMana() => currentMana;
        public int GetMaxMana() => maxMana;
        public string GetPlayerName() => playerName;
        public List<GameCard> GetHand() => hand;
        public List<GameCard> GetBattlefield() => battlefield;
    }
}