using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System;
using System.Text;
using UnityEngine.Networking;
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
            string fullUrl = supabaseUrl + endpoint;
            Debug.Log($"Supabase Request: {method} {fullUrl}");

            // For now, return dummy data to avoid UnityWebRequest issues
            // TODO: Implement proper HTTP client when Unity Web Request package is available
            await Task.Delay(500); // Simulate network delay

            if (method == "GET")
            {
                // Return dummy collection data for GET requests
                return ("[]", null);
            }
            else if (method == "POST")
            {
                // Return dummy auth response for POST requests
                if (endpoint.Contains("token") || endpoint.Contains("signup"))
                {
                    return ("{\"access_token\":\"dummy_token\",\"user\":{\"id\":\"dummy_user_id\",\"email\":\"test@example.com\"}}", null);
                }
                return ("{\"success\": true}", null);
            }

            return ("{\"success\": true}", null);
        }

        public async Task<(string userId, string error)> SignIn(string email, string password)
        {
            Debug.Log($"SupabaseClient: Signing in {email}");
            
            var authData = new
            {
                email = email,
                password = password
            };

            string body = JsonHelper.ToJson(authData);
            var (data, error) = await MakeRequest("/auth/v1/token?grant_type=password", "POST", body);

            if (error != null)
            {
                Debug.LogError($"SupabaseClient: Sign-in failed: {error}");
                return (null, error);
            }

            try
            {
                var authResponse = JsonUtility.FromJson<AuthResponse>(data);
                SetAccessToken(authResponse.access_token);
                
                Debug.Log($"SupabaseClient: Sign-in successful for {email}");
                return (authResponse.user.id, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"SupabaseClient: Sign-in JSON parsing error: {e.Message}");
                return (null, $"Sign-in failed: {e.Message}");
            }
        }

        public async Task<(string userId, string error)> SignUp(string email, string password)
        {
            Debug.Log($"SupabaseClient: Signing up {email}");
            
            var authData = new
            {
                email = email,
                password = password
            };

            string body = JsonHelper.ToJson(authData);
            var (data, error) = await MakeRequest("/auth/v1/signup", "POST", body);

            if (error != null)
            {
                Debug.LogError($"SupabaseClient: Sign-up failed: {error}");
                return (null, error);
            }

            try
            {
                var authResponse = JsonUtility.FromJson<AuthResponse>(data);
                SetAccessToken(authResponse.access_token);
                
                Debug.Log($"SupabaseClient: Sign-up successful for {email}");
                return (authResponse.user.id, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"SupabaseClient: Sign-up JSON parsing error: {e.Message}");
                return (null, $"Sign-up failed: {e.Message}");
            }
        }

        public async Task SignOut()
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Call Supabase signout endpoint
                var (data, error) = await MakeRequest("/auth/v1/logout", "POST");
                if (error != null)
                {
                    Debug.LogWarning($"SupabaseClient: Sign-out request failed: {error}");
                }
            }
            
            accessToken = null;
            PlayerPrefs.DeleteKey("user_token");
            PlayerPrefs.DeleteKey("user_id");
            PlayerPrefs.Save();
            Debug.Log("SupabaseClient: Signed out.");
        }

        public async Task<(T[] data, string error)> GetData<T>(string tableName, string query = null)
        {
            string url = $"/rest/v1/{tableName}";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"?{query}";
            }

            var (data, error) = await MakeRequest(url, "GET");

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

            var (data, error) = await MakeRequest(url, "POST", body);

            return (data, error);
        }

        public async Task<(CollectionItem[] collection, string error)> GetUserCollection(string userId)
        {
            Debug.Log($"SupabaseClient: Fetching collection for {userId}");
            
            var (data, error) = await MakeRequest($"/rest/v1/rpc/get_user_collection?user_uuid={userId}", "GET");
            
            if (error != null)
            {
                Debug.LogError($"SupabaseClient: Failed to fetch collection: {error}");
                return (null, error);
            }

            try
            {
                var collectionData = JsonHelper.FromJsonArray<CollectionItem>(data);
                Debug.Log($"SupabaseClient: Loaded {collectionData.Length} collection items");
                return (collectionData, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"SupabaseClient: Collection JSON parsing error: {e.Message}");
                return (null, $"Collection parsing error: {e.Message}");
            }
        }

        public async Task<(CardData[] cards, string error)> GetAllCards()
        {
            Debug.Log($"SupabaseClient: Fetching all cards");
            
            var (data, error) = await MakeRequest("/rest/v1/card_complete?order=rarity.desc,name.asc", "GET");
            
            if (error != null)
            {
                Debug.LogError($"SupabaseClient: Failed to fetch cards: {error}");
                return (null, error);
            }

            try
            {
                var cardsData = JsonHelper.FromJsonArray<CardData>(data);
                Debug.Log($"SupabaseClient: Loaded {cardsData.Length} cards");
                return (cardsData, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"SupabaseClient: Cards JSON parsing error: {e.Message}");
                return (null, $"Cards parsing error: {e.Message}");
            }
        }
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public User user;
    }

    [System.Serializable]
    public class User
    {
        public string id;
        public string email;
        public string created_at;
        public string updated_at;
    }

    [System.Serializable]
    public class CollectionItem
    {
        public CardData card;
        public int quantity;
        public string acquired_at;
        public string source;
    }
}