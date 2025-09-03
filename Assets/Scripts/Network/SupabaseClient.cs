using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace PotatoCardGame.Network
{
    /// <summary>
    /// Enhanced Supabase client with complete authentication and database operations
    /// Handles all communication with the Supabase backend for the mobile game
    /// </summary>
    public class SupabaseClient : MonoBehaviour
    {
        [Header("Supabase Configuration")]
        [SerializeField] private string supabaseUrl = "https://xsknbbvyagngljxkftkd.supabase.co";
        [SerializeField] private string anonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTYwNTg0MzIsImV4cCI6MjA3MTYzNDQzMn0.J8G45OdXxTbWuGO8N05_50qOVPq7BMSDo1xeegXQMW0";
        
        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;
        
        // Singleton pattern
        public static SupabaseClient Instance { get; private set; }
        
        // Authentication
        private string accessToken = "";
        private string refreshToken = "";
        private string userId = "";
        private bool isAuthenticated = false;
        
        public bool IsAuthenticated => isAuthenticated;
        public string AccessToken => accessToken;
        public string UserId => userId;
        
        // Events
        public System.Action<bool> OnAuthenticationChanged;
        public System.Action<string> OnError;
        public System.Action<UserProfile> OnUserProfileLoaded;
        
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
            Log("🔌 Supabase Client Initialized");
            
            // Check for stored authentication
            LoadStoredAuth();
        }
        
        #region Authentication
        
        public async Task<bool> SignIn(string email, string password)
        {
            try
            {
                Log($"🔐 Attempting to sign in: {email}");
                
                var signInData = new
                {
                    email = email,
                    password = password
                };
                
                string jsonData = JsonConvert.SerializeObject(signInData);
                string response = await PostRequest("/auth/v1/token?grant_type=password", jsonData);
                
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);
                
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.access_token))
                {
                    accessToken = authResponse.access_token;
                    refreshToken = authResponse.refresh_token;
                    userId = authResponse.user.id;
                    isAuthenticated = true;
                    
                    SaveAuth();
                    OnAuthenticationChanged?.Invoke(true);
                    
                    Log("✅ Sign in successful");
                    
                    // Load user profile
                    await LoadUserProfile();
                    
                    return true;
                }
                else
                {
                    LogError("❌ Sign in failed: Invalid response");
                    OnError?.Invoke("Sign in failed");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogError($"❌ Sign in error: {e.Message}");
                OnError?.Invoke($"Sign in error: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SignUp(string email, string password, string displayName = "")
        {
            try
            {
                Log($"📝 Attempting to sign up: {email}");
                
                var signUpData = new
                {
                    email = email,
                    password = password,
                    data = new
                    {
                        display_name = displayName
                    }
                };
                
                string jsonData = JsonConvert.SerializeObject(signUpData);
                string response = await PostRequest("/auth/v1/signup", jsonData);
                
                Log("✅ Sign up successful - Check email for confirmation");
                return true;
            }
            catch (Exception e)
            {
                LogError($"❌ Sign up error: {e.Message}");
                OnError?.Invoke($"Sign up error: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SignOut()
        {
            try
            {
                if (isAuthenticated)
                {
                    await PostRequest("/auth/v1/logout", "{}");
                }
                
                accessToken = "";
                refreshToken = "";
                userId = "";
                isAuthenticated = false;
                
                ClearStoredAuth();
                OnAuthenticationChanged?.Invoke(false);
                
                Log("👋 Signed out successfully");
                return true;
            }
            catch (Exception e)
            {
                LogError($"❌ Sign out error: {e.Message}");
                return false;
            }
        }
        
        private async Task LoadUserProfile()
        {
            try
            {
                var profile = await GetData<List<UserProfile>>($"user_profiles?id=eq.{userId}&select=*");
                
                if (profile != null && profile.Count > 0)
                {
                    OnUserProfileLoaded?.Invoke(profile[0]);
                    Log($"👤 User profile loaded: {profile[0].display_name}");
                }
            }
            catch (Exception e)
            {
                LogError($"❌ Error loading user profile: {e.Message}");
            }
        }
        
        #endregion
        
        #region Database Operations
        
        public async Task<T> GetData<T>(string endpoint)
        {
            try
            {
                if (!endpoint.StartsWith("/rest/v1/"))
                {
                    endpoint = "/rest/v1/" + endpoint;
                }
                
                string response = await GetRequest(endpoint);
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception e)
            {
                LogError($"❌ Get data error: {e.Message}");
                OnError?.Invoke($"Database error: {e.Message}");
                return default(T);
            }
        }
        
        public async Task<bool> PostData<T>(string table, T data)
        {
            try
            {
                string endpoint = $"/rest/v1/{table}";
                string jsonData = JsonConvert.SerializeObject(data);
                
                await PostRequest(endpoint, jsonData);
                return true;
            }
            catch (Exception e)
            {
                LogError($"❌ Post data error: {e.Message}");
                OnError?.Invoke($"Database error: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> UpdateData<T>(string table, string id, T data)
        {
            try
            {
                string endpoint = $"/rest/v1/{table}?id=eq.{id}";
                string jsonData = JsonConvert.SerializeObject(data);
                
                await PatchRequest(endpoint, jsonData);
                return true;
            }
            catch (Exception e)
            {
                LogError($"❌ Update data error: {e.Message}");
                OnError?.Invoke($"Database error: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> DeleteData(string table, string id)
        {
            try
            {
                string endpoint = $"/rest/v1/{table}?id=eq.{id}";
                await DeleteRequest(endpoint);
                return true;
            }
            catch (Exception e)
            {
                LogError($"❌ Delete data error: {e.Message}");
                OnError?.Invoke($"Database error: {e.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region HTTP Requests
        
        private async Task<string> GetRequest(string endpoint)
        {
            string url = supabaseUrl + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                SetRequestHeaders(request);
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"GET Request failed: {request.error} - {request.downloadHandler.text}");
                }
            }
        }
        
        private async Task<string> PostRequest(string endpoint, string jsonData)
        {
            string url = supabaseUrl + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                SetRequestHeaders(request);
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"POST Request failed: {request.error} - {request.downloadHandler.text}");
                }
            }
        }
        
        private async Task<string> PatchRequest(string endpoint, string jsonData)
        {
            string url = supabaseUrl + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
            {
                request.method = "PATCH";
                SetRequestHeaders(request);
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"PATCH Request failed: {request.error}");
                }
            }
        }
        
        private async Task<string> DeleteRequest(string endpoint)
        {
            string url = supabaseUrl + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                SetRequestHeaders(request);
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"DELETE Request failed: {request.error}");
                }
            }
        }
        
        private void SetRequestHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("apikey", anonKey);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=representation");
            
            if (isAuthenticated)
            {
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            }
        }
        
        #endregion
        
        #region Auth Storage
        
        private void SaveAuth()
        {
            PlayerPrefs.SetString("supabase_access_token", accessToken);
            PlayerPrefs.SetString("supabase_refresh_token", refreshToken);
            PlayerPrefs.SetString("supabase_user_id", userId);
            PlayerPrefs.SetInt("supabase_authenticated", 1);
            PlayerPrefs.Save();
            
            Log("💾 Authentication saved to device");
        }
        
        private void LoadStoredAuth()
        {
            if (PlayerPrefs.HasKey("supabase_authenticated") && PlayerPrefs.GetInt("supabase_authenticated") == 1)
            {
                accessToken = PlayerPrefs.GetString("supabase_access_token", "");
                refreshToken = PlayerPrefs.GetString("supabase_refresh_token", "");
                userId = PlayerPrefs.GetString("supabase_user_id", "");
                
                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(userId))
                {
                    isAuthenticated = true;
                    OnAuthenticationChanged?.Invoke(true);
                    Log("🔐 Restored authentication from device storage");
                    
                    // Load user profile
                    _ = LoadUserProfile();
                }
            }
        }
        
        private void ClearStoredAuth()
        {
            PlayerPrefs.DeleteKey("supabase_access_token");
            PlayerPrefs.DeleteKey("supabase_refresh_token");
            PlayerPrefs.DeleteKey("supabase_user_id");
            PlayerPrefs.DeleteKey("supabase_authenticated");
            PlayerPrefs.Save();
            
            Log("🗑️ Authentication cleared from device");
        }
        
        public string GetUserId()
        {
            return userId;
        }
        
        #endregion
        
        #region Edge Functions
        
        public async Task<T> CallEdgeFunction<T>(string functionName, object parameters = null)
        {
            try
            {
                string endpoint = $"/functions/v1/{functionName}";
                string jsonData = parameters != null ? JsonConvert.SerializeObject(parameters) : "{}";
                
                string response = await PostRequest(endpoint, jsonData);
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception e)
            {
                LogError($"❌ Edge function error: {e.Message}");
                throw;
            }
        }
        
        public async Task<bool> JoinMatchmaking()
        {
            try
            {
                var result = await CallEdgeFunction<MatchmakingResult>("matchmaker", new { user_id = userId });
                return result.success;
            }
            catch (Exception e)
            {
                LogError($"❌ Matchmaking error: {e.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Logging
        
        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[SupabaseClient] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[SupabaseClient] {message}");
        }
        
        #endregion
        
        #region Data Models
        
        [Serializable]
        public class AuthResponse
        {
            public string access_token;
            public string refresh_token;
            public string token_type;
            public int expires_in;
            public User user;
        }
        
        [Serializable]
        public class User
        {
            public string id;
            public string email;
            public string created_at;
            public UserMetadata user_metadata;
        }
        
        [Serializable]
        public class UserMetadata
        {
            public string display_name;
        }
        
        [Serializable]
        public class UserProfile
        {
            public string id;
            public string email;
            public string display_name;
            public string username;
            public string created_at;
            public string updated_at;
        }
        
        [Serializable]
        public class MatchmakingResult
        {
            public bool success;
            public int matches_created;
            public int queue_size;
            public string message;
        }
        
        #endregion
    }
}