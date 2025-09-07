using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using PotatoLegends.Utils;
using PotatoLegends.Data;
using PotatoLegends.Network;

namespace PotatoLegends.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Battle State")]
        public string battleSessionId;
        public string CurrentPlayerId { get; private set; }
        public string OpponentPlayerId { get; private set; }
        public int CurrentPlayerMana { get; private set; }
        public int OpponentPlayerMana { get; private set; }
        public int CurrentTurnNumber { get; private set; }
        public string CurrentTurnPlayerId { get; private set; }
        public BattlePhase CurrentBattlePhase { get; private set; }

        public event Action OnBattleStateUpdated;
        public event Action<string> OnTurnChanged;
        public event Action<CardData, int> OnCardDeployed;
        public event Action<CardData, int> OnCardRemoved;
        public event Action<string> OnBattleEnded;

        private BattleSession currentBattleSession;

        public enum BattlePhase { Mulligan, Deploying, Battling, Ended }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            if (string.IsNullOrEmpty(battleSessionId))
            {
                Debug.LogWarning("BattleManager: No battleSessionId set. Starting dummy battle for testing.");
                StartDummyBattle();
            }
            else
            {
                StartBattle(battleSessionId);
            }
        }

        private async void StartDummyBattle()
        {
            CurrentPlayerId = "dummy_player_1";
            OpponentPlayerId = "dummy_player_2";
            CurrentTurnPlayerId = CurrentPlayerId;
            CurrentPlayerMana = 1;
            OpponentPlayerMana = 0;
            CurrentTurnNumber = 1;
            CurrentBattlePhase = BattlePhase.Mulligan;
            Debug.Log("BattleManager: Dummy battle started.");
            OnBattleStateUpdated?.Invoke();

            await Task.Delay(1000);
            CurrentBattlePhase = BattlePhase.Deploying;
            OnBattleStateUpdated?.Invoke();
        }

        public async void StartBattle(string sessionId)
        {
            battleSessionId = sessionId;
            Debug.Log($"BattleManager: Starting battle session: {battleSessionId}");
            await FetchBattleState();
        }

        private async Task FetchBattleState()
        {
            if (string.IsNullOrEmpty(battleSessionId)) return;

            var (data, error) = await SupabaseClient.Instance.CallEdgeFunction("get-active-battle", new { battleId = battleSessionId });

            if (error != null)
            {
                Debug.LogError($"Failed to fetch battle state: {error}");
                return;
            }

            try
            {
                var response = JsonHelper.FromJson<SupabaseBattleResponse>(data);
                if (response.success && response.activeBattle != null)
                {
                    currentBattleSession = response.activeBattle;
                    UpdateLocalBattleState(currentBattleSession);
                    Debug.Log("BattleManager: Initial battle state fetched and updated.");
                }
                else
                {
                    Debug.LogError($"Failed to get active battle: {response.error}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"BattleManager: JSON deserialization error: {e.Message}");
            }
        }

        private void UpdateLocalBattleState(BattleSession session)
        {
            currentBattleSession = session;
            CurrentPlayerId = SupabaseClient.Instance.GetAccessToken();
            OpponentPlayerId = (session.player1_id == CurrentPlayerId) ? session.player2_id : session.player1_id;

            if (session.game_state != null)
            {
                CurrentTurnPlayerId = session.game_state.current_turn_player_id;
                CurrentTurnNumber = session.game_state.turn_count;
                
                if (session.player1_id == CurrentPlayerId)
                {
                    CurrentPlayerMana = session.game_state.player1_hero?.mana ?? 0;
                    OpponentPlayerMana = session.game_state.player2_hero?.mana ?? 0;
                }
                else
                {
                    CurrentPlayerMana = session.game_state.player2_hero?.mana ?? 0;
                    OpponentPlayerMana = session.game_state.player1_hero?.mana ?? 0;
                }

                switch (session.game_state.phase)
                {
                    case "mulligan": CurrentBattlePhase = BattlePhase.Mulligan; break;
                    case "deploying": CurrentBattlePhase = BattlePhase.Deploying; break;
                    case "battling": CurrentBattlePhase = BattlePhase.Battling; break;
                    case "finished": CurrentBattlePhase = BattlePhase.Ended; break;
                    default: CurrentBattlePhase = BattlePhase.Battling; break;
                }
            }
            
            OnBattleStateUpdated?.Invoke();
            OnTurnChanged?.Invoke(CurrentTurnPlayerId);
        }

        public async Task<(bool success, string error)> DeployCard(CardData card, int slotIndex)
        {
            if (string.IsNullOrEmpty(battleSessionId) || card == null)
            {
                return (false, "Invalid battle session or card data.");
            }

            var payload = new { battleId = battleSessionId, cardId = card.cardId, slotIndex = slotIndex };
            var (data, error) = await SupabaseClient.Instance.CallEdgeFunction("battle-deploy-card", payload);

            if (error != null)
            {
                return (false, error);
            }

            var response = JsonHelper.FromJson<SupabaseFunctionResponse>(data);
            if (response.success)
            {
                Debug.Log($"Card {card.cardName} deployed to slot {slotIndex}.");
                OnCardDeployed?.Invoke(card, slotIndex);
                await FetchBattleState();
                return (true, null);
            }
            else
            {
                Debug.LogError($"Failed to deploy card: {response.error}");
                return (false, response.error);
            }
        }

        public async Task<(bool success, string error)> EndTurn()
        {
            if (string.IsNullOrEmpty(battleSessionId))
            {
                return (false, "Invalid battle session.");
            }

            var payload = new { battleId = battleSessionId, playerId = CurrentPlayerId, isAutoEnd = false };
            var (data, error) = await SupabaseClient.Instance.CallEdgeFunction("battle-end-turn", payload);

            if (error != null)
            {
                return (false, error);
            }

            var response = JsonHelper.FromJson<SupabaseFunctionResponse>(data);
            if (response.success)
            {
                Debug.Log("Turn ended.");
                await FetchBattleState();
                return (true, null);
            }
            else
            {
                Debug.LogError($"Failed to end turn: {response.error}");
                return (false, response.error);
            }
        }

        [System.Serializable]
        public class SupabaseBattleResponse
        {
            public bool success;
            public BattleSession activeBattle;
            public string error;
        }

        [System.Serializable]
        public class SupabaseFunctionResponse
        {
            public bool success;
            public string message;
            public string error;
        }

        [System.Serializable]
        public class BattleSession
        {
            public string id;
            public string player1_id;
            public string player2_id;
            public string current_turn_player_id;
            public BattleGameState game_state;
            public string status;
            public string winner_id;
            public string started_at;
            public string finished_at;
        }

        [System.Serializable]
        public class BattleGameState
        {
            public string current_turn_player_id;
            public int turn_count;
            public string phase;
            public PlayerHeroState player1_hero;
            public PlayerHeroState player2_hero;
        }

        [System.Serializable]
        public class PlayerHeroState
        {
            public int hp;
            public int max_hp;
            public int mana;
            public int max_mana;
            public bool hero_power_available;
            public bool hero_power_used_this_turn;
            public int hero_power_cooldown;
        }
    }
}