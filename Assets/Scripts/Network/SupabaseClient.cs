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

            using (UnityWebRequest request = new UnityWebRequest(fullUrl, method))
            {
                // Set headers
                request.SetRequestHeader("apikey", anonKey);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Prefer", "return=minimal");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                }

                // Set body for POST/PUT requests
                if (!string.IsNullOrEmpty(body))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }

                request.downloadHandler = new DownloadHandlerBuffer();

                // Send request
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return (request.downloadHandler.text, null);
                }
                else
                {
                    string errorMessage = $"Request failed: {request.error}";
                    if (!string.IsNullOrEmpty(request.downloadHandler.text))
                    {
                        errorMessage += $" - {request.downloadHandler.text}";
                    }
                    return (null, errorMessage);
                }
            }
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
}