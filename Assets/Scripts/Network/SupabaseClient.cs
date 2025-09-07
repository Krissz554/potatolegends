using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System;
using System.Text;
using UnityEngine.InputSystem;
using PotatoLegends.Utils;
using PotatoLegends.Data;

namespace PotatoLegends.Network
{
    public class SupabaseClient : MonoBehaviour
    {
        public static SupabaseClient Instance { get; private set; }

        [SerializeField] private string supabaseUrl = "https://xsknbbvyagngljxkftkd.supabase.co";
        [SerializeField] private string anonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTYwNTg0MzIsImV4cCI6MjA3MTYzNDQzMn0.J8G45OdXxTbWuGO8N05_50qOVPq7BMSDo1xeegQMW0";

        private string accessToken;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void SetAccessToken(string token)
        {
            accessToken = token;
            Debug.Log("SupabaseClient: Access token set.");
        }

        public string GetAccessToken()
        {
            return accessToken;
        }

        private async Task<(string data, string error)> MakeRequest(string endpoint, string method, string body = null)
        {
            // Simplified implementation for Unity 6 compatibility
            Debug.Log($"Supabase Request: {method} {endpoint}");
            await Task.Delay(100); // Simulate network delay
            
            // Return dummy data for now - replace with actual HTTP implementation
            if (method == "GET")
            {
                return ("[]", null); // Empty array for GET requests
            }
            else
            {
                return ("{\"success\": true}", null); // Success response for POST requests
            }
        }

        public async Task<(string userId, string error)> SignIn(string email, string password)
        {
            Debug.Log($"SupabaseClient: Simulating sign-in for {email}");
            await Task.Delay(1000);
            SetAccessToken("simulated_access_token_for_" + email);
            return ("simulated_user_id_" + email, null);
        }

        public async Task<(string userId, string error)> SignUp(string email, string password)
        {
            Debug.Log($"SupabaseClient: Simulating sign-up for {email}");
            await Task.Delay(1000);
            SetAccessToken("simulated_access_token_for_" + email);
            return ("simulated_user_id_" + email, null);
        }

        public void SignOut()
        {
            accessToken = null;
            Debug.Log("SupabaseClient: Signed out.");
        }

        public async Task<(T[] data, string error)> GetData<T>(string tableName, string query = null)
        {
            string url = $"/rest/v1/{tableName}";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"?{query}";
            }

            var (data, error) = await MakeRequest(url, UnityWebRequest.kHttpVerbGET);

            if (error != null)
            {
                return (null, error);
            }

            try
            {
                T[] result = JsonHelper.FromJsonArray<T>(data);
                return (result, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"SupabaseClient: JSON deserialization error: {e.Message}");
                return (null, $"JSON deserialization error: {e.Message}");
            }
        }

        public async Task<(string data, string error)> CallEdgeFunction(string functionName, object payload = null)
        {
            string url = $"/functions/v1/{functionName}";
            string body = payload != null ? JsonHelper.ToJson(payload) : null;

            var (data, error) = await MakeRequest(url, UnityWebRequest.kHttpVerbPOST, body);

            return (data, error);
        }

        public async Task<(CardData[] cards, string error)> GetUserCollection(string userId)
        {
            Debug.Log($"SupabaseClient: Fetching collection for {userId}");
            await Task.Delay(500);
            
            CardData[] dummyCards = new CardData[]
            {
                ScriptableObject.CreateInstance<CardData>(),
                ScriptableObject.CreateInstance<CardData>()
            };
            dummyCards[0].cardName = "Dummy Card 1";
            dummyCards[0].cardId = "dummy_001";
            dummyCards[0].manaCost = 1;
            dummyCards[0].attack = 1;
            dummyCards[0].health = 2;
            dummyCards[0].cardType = CardData.CardType.Unit;
            dummyCards[0].rarity = CardData.Rarity.Common;

            dummyCards[1].cardName = "Dummy Card 2";
            dummyCards[1].cardId = "dummy_002";
            dummyCards[1].manaCost = 2;
            dummyCards[1].attack = 3;
            dummyCards[1].health = 2;
            dummyCards[1].cardType = CardData.CardType.Unit;
            dummyCards[1].rarity = CardData.Rarity.Uncommon;

            return (dummyCards, null);
        }
    }
}