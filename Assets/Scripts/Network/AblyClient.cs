using UnityEngine;
using System;
using System.Collections.Generic;

namespace PotatoLegends.Network
{
    public class AblyClient : MonoBehaviour
    {
        public static AblyClient Instance { get; private set; }

        [Header("Ably Configuration")]
        public GameConfig gameConfig;

        // Events
        public System.Action<string, string> OnMessageReceived;
        public System.Action<string> OnMatchmakingFound;
        public System.Action<string> OnBattleUpdate;

        private bool isConnected = false;
        private string currentChannel = "";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (gameConfig != null && !string.IsNullOrEmpty(gameConfig.ablyApiKey))
            {
                Connect();
            }
            else
            {
                Debug.LogWarning("Ably API key not configured. Real-time features disabled.");
            }
        }

        private void Connect()
        {
            // TODO: Implement actual Ably connection
            // For now, simulate connection
            Debug.Log("🔌 Connecting to Ably...");
            isConnected = true;
            Debug.Log("✅ Connected to Ably");
        }

        public void JoinMatchmaking()
        {
            if (!isConnected)
            {
                Debug.LogError("Not connected to Ably");
                return;
            }

            Debug.Log("🎮 Joining matchmaking queue...");
            // TODO: Implement actual matchmaking
            // Simulate finding a match after 3 seconds
            Invoke(nameof(SimulateMatchFound), 3f);
        }

        private void SimulateMatchFound()
        {
            Debug.Log("🎯 Match found! Starting battle...");
            OnMatchmakingFound?.Invoke("opponent_123");
        }

        public void LeaveMatchmaking()
        {
            Debug.Log("🚪 Leaving matchmaking queue");
            CancelInvoke(nameof(SimulateMatchFound));
        }

        public void JoinBattleChannel(string battleId)
        {
            if (!isConnected)
            {
                Debug.LogError("Not connected to Ably");
                return;
            }

            currentChannel = $"battle_{battleId}";
            Debug.Log($"⚔️ Joining battle channel: {currentChannel}");
            // TODO: Implement actual channel subscription
        }

        public void LeaveBattleChannel()
        {
            if (!string.IsNullOrEmpty(currentChannel))
            {
                Debug.Log($"🚪 Leaving battle channel: {currentChannel}");
                currentChannel = "";
                // TODO: Implement actual channel unsubscription
            }
        }

        public void SendBattleAction(string action, Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(currentChannel))
            {
                Debug.LogError("Not in a battle channel");
                return;
            }

            var message = new Dictionary<string, object>
            {
                {"action", action},
                {"data", data},
                {"timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}
            };

            Debug.Log($"📤 Sending battle action: {action}");
            // TODO: Implement actual message sending
        }

        void OnDestroy()
        {
            LeaveBattleChannel();
            LeaveMatchmaking();
        }
    }
}