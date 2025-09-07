using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using PotatoLegends.Data;

namespace PotatoLegends.Network
{
    public class SupabaseClient : MonoBehaviour
    {
        public static SupabaseClient Instance { get; private set; }

        [Header("Supabase Configuration")]
        public GameConfig gameConfig;

        private string accessToken;
        private string userId;
        private string userEmail;

        // Events
        public System.Action<string> OnAuthenticationSuccess;
        public System.Action<string> OnAuthenticationError;
        public System.Action<List<CollectionItem>> OnCollectionLoaded;
        public System.Action<string> OnCollectionError;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved authentication
            LoadSavedAuth();
        }

        private void LoadSavedAuth()
        {
            accessToken = PlayerPrefs.GetString("user_token", "");
            userId = PlayerPrefs.GetString("user_id", "");
            userEmail = PlayerPrefs.GetString("user_email", "");

            if (!string.IsNullOrEmpty(accessToken))
            {
                Debug.Log("Loaded saved authentication");
            }
        }

        public string GetAccessToken()
        {
            return accessToken;
        }

        public string GetUserId()
        {
            return userId;
        }

        public string GetUserEmail()
        {
            return userEmail;
        }

        public void SetAccessToken(string token)
        {
            accessToken = token;
        }

        public async Task<bool> SignIn(string email, string password)
        {
            try
            {
                var response = await MakeRequest("/auth/v1/token?grant_type=password", "POST", 
                    $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}");

                if (string.IsNullOrEmpty(response.error))
                {
                    var authResponse = JsonUtility.FromJson<AuthResponse>(response.data);
                    if (authResponse != null && !string.IsNullOrEmpty(authResponse.access_token))
                    {
                        accessToken = authResponse.access_token;
                        userId = authResponse.user.id;
                        userEmail = authResponse.user.email;

                        // Save to PlayerPrefs
                        PlayerPrefs.SetString("user_token", accessToken);
                        PlayerPrefs.SetString("user_id", userId);
                        PlayerPrefs.SetString("user_email", userEmail);
                        PlayerPrefs.Save();

                        OnAuthenticationSuccess?.Invoke(userEmail);
                        return true;
                    }
                }

                OnAuthenticationError?.Invoke(response.error ?? "Authentication failed");
                return false;
            }
            catch (Exception ex)
            {
                OnAuthenticationError?.Invoke(ex.Message);
                return false;
            }
        }

        public async Task<bool> SignUp(string email, string password)
        {
            try
            {
                var response = await MakeRequest("/auth/v1/signup", "POST", 
                    $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}");

                if (string.IsNullOrEmpty(response.error))
                {
                    var authResponse = JsonUtility.FromJson<AuthResponse>(response.data);
                    if (authResponse != null && !string.IsNullOrEmpty(authResponse.access_token))
                    {
                        accessToken = authResponse.access_token;
                        userId = authResponse.user.id;
                        userEmail = authResponse.user.email;

                        // Save to PlayerPrefs
                        PlayerPrefs.SetString("user_token", accessToken);
                        PlayerPrefs.SetString("user_id", userId);
                        PlayerPrefs.SetString("user_email", userEmail);
                        PlayerPrefs.Save();

                        OnAuthenticationSuccess?.Invoke(userEmail);
                        return true;
                    }
                }

                OnAuthenticationError?.Invoke(response.error ?? "Registration failed");
                return false;
            }
            catch (Exception ex)
            {
                OnAuthenticationError?.Invoke(ex.Message);
                return false;
            }
        }

        public async Task<bool> SignOut()
        {
            try
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await MakeRequest("/auth/v1/logout", "POST", null);
                }

                // Clear local data
                accessToken = "";
                userId = "";
                userEmail = "";

                PlayerPrefs.DeleteKey("user_token");
                PlayerPrefs.DeleteKey("user_id");
                PlayerPrefs.DeleteKey("user_email");
                PlayerPrefs.Save();

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Sign out error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CollectionItem>> GetUserCollection()
        {
            try
            {
                var response = await MakeRequest("/rest/v1/rpc/get_user_collection", "POST", 
                    $"{{\"user_id\":\"{userId}\"}}");

                if (string.IsNullOrEmpty(response.error))
                {
                    var collection = ParseCollectionResponse(response.data);
                    OnCollectionLoaded?.Invoke(collection);
                    return collection;
                }
                else
                {
                    OnCollectionError?.Invoke(response.error);
                    return new List<CollectionItem>();
                }
            }
            catch (Exception ex)
            {
                OnCollectionError?.Invoke(ex.Message);
                return new List<CollectionItem>();
            }
        }

        public async Task<List<CardData>> GetAllCards()
        {
            try
            {
                var response = await MakeRequest("/rest/v1/card_complete", "GET", null);

                if (string.IsNullOrEmpty(response.error))
                {
                    return ParseCardsResponse(response.data);
                }
                else
                {
                    Debug.LogError($"Get all cards error: {response.error}");
                    return new List<CardData>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Get all cards exception: {ex.Message}");
                return new List<CardData>();
            }
        }

        public async Task<(string data, string error)> CallEdgeFunction(string functionName, string payload)
        {
            try
            {
                var response = await MakeRequest($"/functions/v1/{functionName}", "POST", payload);
                return (response.data, response.error);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        private List<CollectionItem> ParseCollectionResponse(string json)
        {
            // Simple JSON parsing for collection items
            if (string.IsNullOrEmpty(json) || json == "[]")
            {
                return new List<CollectionItem>();
            }

            // For now, return empty list - in real implementation, parse the JSON
            return new List<CollectionItem>();
        }

        private List<CardData> ParseCardsResponse(string json)
        {
            // Simple JSON parsing for cards
            if (string.IsNullOrEmpty(json) || json == "[]")
            {
                return new List<CardData>();
            }

            // For now, return empty list - in real implementation, parse the JSON
            return new List<CardData>();
        }

        private async Task<(string data, string error)> MakeRequest(string endpoint, string method, string body = null)
        {
            if (gameConfig == null)
            {
                Debug.LogError("GameConfig is not assigned to SupabaseClient!");
                return (null, "Configuration error");
            }

            string fullUrl = gameConfig.supabaseUrl + endpoint;
            Debug.Log($"Supabase Request: {method} {fullUrl}");

            var headers = new Dictionary<string, string>
            {
                {"apikey", gameConfig.supabaseAnonKey},
                {"Content-Type", "application/json"},
                {"Prefer", "return=minimal"}
            };

            if (!string.IsNullOrEmpty(accessToken))
            {
                headers["Authorization"] = $"Bearer {accessToken}";
            }

            var tcs = new TaskCompletionSource<(string, string)>();
            
            HttpClient.Instance.MakeRequestAsync(fullUrl, method, body, headers, (data, error) =>
            {
                tcs.SetResult((data, error));
            });

            return await tcs.Task;
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(userId);
        }
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string access_token;
        public User user;
    }

    [System.Serializable]
    public class User
    {
        public string id;
        public string email;
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