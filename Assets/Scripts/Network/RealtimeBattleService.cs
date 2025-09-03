using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using PotatoCardGame.Data;
using PotatoCardGame.Battle;

namespace PotatoCardGame.Network
{
    /// <summary>
    /// Handles real-time multiplayer battle communication with Supabase
    /// Manages matchmaking, battle sessions, and real-time game state sync
    /// </summary>
    public class RealtimeBattleService : MonoBehaviour
    {
        [Header("Matchmaking Settings")]
        [SerializeField] private float matchmakingTimeout = 60f;
        [SerializeField] private float battleUpdateInterval = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        public static RealtimeBattleService Instance { get; private set; }
        
        // Matchmaking state
        private bool isSearchingForMatch = false;
        private float matchmakingTimer = 0f;
        private string currentMatchmakingId;
        
        // Battle state
        private string currentBattleSessionId;
        private bool isBattleActive = false;
        private float lastBattleUpdate = 0f;
        
        // Events
        public System.Action<string> OnMatchFound; // Battle session ID
        public System.Action OnMatchmakingCancelled;
        public System.Action<int> OnMatchmakingUpdate; // Players in queue
        public System.Action<BattleSession> OnBattleStateUpdated;
        public System.Action<string> OnBattleEnded; // Winner ID
        public System.Action<string> OnRealtimeError;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            // Update matchmaking timer
            if (isSearchingForMatch)
            {
                UpdateMatchmaking();
            }
            
            // Update battle state
            if (isBattleActive)
            {
                UpdateBattleState();
            }
        }
        
        #region Matchmaking
        
        public async Task<bool> JoinMatchmaking()
        {
            if (isSearchingForMatch) return false;
            
            try
            {
                if (enableDebugLogs) Debug.Log("🔍 Joining matchmaking queue...");
                
                // Check if user has a valid deck
                // TODO: Implement deck validation
                
                // Join matchmaking queue
                var queueEntry = new
                {
                    user_id = GetCurrentUserId(),
                    joined_at = DateTime.UtcNow,
                    status = "searching"
                };
                
                bool success = await SupabaseClient.Instance.PostData("matchmaking_queue", queueEntry);
                
                if (success)
                {
                    isSearchingForMatch = true;
                    matchmakingTimer = 0f;
                    
                    // Start polling for matches
                    InvokeRepeating(nameof(CheckForMatch), 2f, 2f);
                    
                    if (enableDebugLogs) Debug.Log("✅ Joined matchmaking queue");
                    return true;
                }
                else
                {
                    OnRealtimeError?.Invoke("Failed to join matchmaking");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Matchmaking error: {e.Message}");
                OnRealtimeError?.Invoke($"Matchmaking error: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> LeaveMatchmaking()
        {
            if (!isSearchingForMatch) return true;
            
            try
            {
                if (enableDebugLogs) Debug.Log("🚪 Leaving matchmaking queue...");
                
                // Remove from queue
                // TODO: Implement queue removal API call
                
                isSearchingForMatch = false;
                CancelInvoke(nameof(CheckForMatch));
                
                OnMatchmakingCancelled?.Invoke();
                
                if (enableDebugLogs) Debug.Log("✅ Left matchmaking queue");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error leaving matchmaking: {e.Message}");
                return false;
            }
        }
        
        private void UpdateMatchmaking()
        {
            matchmakingTimer += Time.deltaTime;
            
            if (matchmakingTimer >= matchmakingTimeout)
            {
                // Timeout - cancel matchmaking
                LeaveMatchmaking();
                OnRealtimeError?.Invoke("Matchmaking timeout");
            }
        }
        
        private async void CheckForMatch()
        {
            if (!isSearchingForMatch) return;
            
            try
            {
                // Check if a battle session has been created for this user
                string userId = GetCurrentUserId();
                string query = $"battle_sessions?select=*&or=(player1_id.eq.{userId},player2_id.eq.{userId})&status=eq.waiting";
                
                var battleSessions = await SupabaseClient.Instance.GetData<List<BattleSession>>(query);
                
                if (battleSessions != null && battleSessions.Count > 0)
                {
                    var battleSession = battleSessions[0];
                    
                    // Match found!
                    isSearchingForMatch = false;
                    CancelInvoke(nameof(CheckForMatch));
                    
                    currentBattleSessionId = battleSession.id;
                    OnMatchFound?.Invoke(battleSession.id);
                    
                    if (enableDebugLogs) Debug.Log($"🎉 Match found! Battle session: {battleSession.id}");
                }
                else
                {
                    // Still waiting - could update queue count here
                    if (enableDebugLogs) Debug.Log("⏳ Still searching for match...");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error checking for match: {e.Message}");
            }
        }
        
        #endregion
        
        #region Battle Session Management
        
        public async Task<BattleSession> StartBattle(string battleSessionId)
        {
            try
            {
                currentBattleSessionId = battleSessionId;
                isBattleActive = true;
                
                if (enableDebugLogs) Debug.Log($"⚔️ Starting battle session: {battleSessionId}");
                
                // Get battle session details
                string query = $"battle_sessions?select=*&id=eq.{battleSessionId}";
                var battleSessions = await SupabaseClient.Instance.GetData<List<BattleSession>>(query);
                
                if (battleSessions != null && battleSessions.Count > 0)
                {
                    var battleSession = battleSessions[0];
                    
                    // Start real-time updates
                    InvokeRepeating(nameof(UpdateBattleFromServer), 1f, battleUpdateInterval);
                    
                    return battleSession;
                }
                else
                {
                    throw new Exception("Battle session not found");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error starting battle: {e.Message}");
                OnRealtimeError?.Invoke($"Failed to start battle: {e.Message}");
                return null;
            }
        }
        
        public async Task<bool> SendBattleAction(BattleAction action)
        {
            if (!isBattleActive || string.IsNullOrEmpty(currentBattleSessionId))
                return false;
            
            try
            {
                if (enableDebugLogs) Debug.Log($"📤 Sending battle action: {action.actionType}");
                
                // Send action to server
                var actionData = new
                {
                    battle_session_id = currentBattleSessionId,
                    player_id = GetCurrentUserId(),
                    action_type = action.actionType,
                    action_data = JsonConvert.SerializeObject(action),
                    timestamp = DateTime.UtcNow
                };
                
                bool success = await SupabaseClient.Instance.PostData("battle_actions", actionData);
                
                if (success && enableDebugLogs)
                {
                    Debug.Log("✅ Battle action sent successfully");
                }
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error sending battle action: {e.Message}");
                OnRealtimeError?.Invoke($"Failed to send action: {e.Message}");
                return false;
            }
        }
        
        private async void UpdateBattleFromServer()
        {
            if (!isBattleActive || string.IsNullOrEmpty(currentBattleSessionId))
                return;
            
            try
            {
                // Get latest battle state
                string query = $"battle_sessions?select=*&id=eq.{currentBattleSessionId}";
                var battleSessions = await SupabaseClient.Instance.GetData<List<BattleSession>>(query);
                
                if (battleSessions != null && battleSessions.Count > 0)
                {
                    var battleSession = battleSessions[0];
                    
                    // Check if battle ended
                    if (battleSession.status == "finished" || battleSession.status == "cancelled")
                    {
                        EndBattle(battleSession.winner_id);
                        return;
                    }
                    
                    // Update battle state
                    OnBattleStateUpdated?.Invoke(battleSession);
                    
                    lastBattleUpdate = Time.time;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error updating battle state: {e.Message}");
            }
        }
        
        private void EndBattle(string winnerId)
        {
            isBattleActive = false;
            CancelInvoke(nameof(UpdateBattleFromServer));
            
            currentBattleSessionId = "";
            
            OnBattleEnded?.Invoke(winnerId);
            
            if (enableDebugLogs) Debug.Log($"🏁 Battle ended. Winner: {winnerId}");
        }
        
        #endregion
        
        #region Utility Methods
        
        private string GetCurrentUserId()
        {
            // TODO: Get from authentication system
            return "placeholder-user-id";
        }
        
        public bool IsSearchingForMatch() => isSearchingForMatch;
        public bool IsBattleActive() => isBattleActive;
        public string GetCurrentBattleSessionId() => currentBattleSessionId;
        
        #endregion
        
        private void OnDestroy()
        {
            // Cleanup
            if (isSearchingForMatch)
            {
                LeaveMatchmaking();
            }
            
            CancelInvoke();
        }
    }
    
    /// <summary>
    /// Represents a battle session from the database
    /// </summary>
    [Serializable]
    public class BattleSession
    {
        public string id;
        public string player1_id;
        public string player2_id;
        public string current_turn_player_id;
        public string status; // waiting, active, finished, cancelled
        public string winner_id;
        public DateTime started_at;
        public DateTime finished_at;
        public int round_number;
        public int turn_number;
        public int player1_cards_remaining;
        public int player2_cards_remaining;
        public string game_state; // JSON string of game state
        public DateTime updated_at;
    }
    
    /// <summary>
    /// Represents a battle action that can be sent to the server
    /// </summary>
    [Serializable]
    public class BattleAction
    {
        public string actionType; // play_card, attack, end_turn, surrender, etc.
        public string cardId;
        public Vector2 targetPosition;
        public string targetId;
        public Dictionary<string, object> additionalData;
        
        public BattleAction(string type)
        {
            actionType = type;
            additionalData = new Dictionary<string, object>();
        }
    }
}