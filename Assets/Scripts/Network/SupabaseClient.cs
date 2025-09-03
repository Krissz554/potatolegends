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
    /// Handles all communication with Supabase backend
    /// Manages authentication, database operations, and real-time subscriptions
    /// </summary>
    public class SupabaseClient : MonoBehaviour
    {
        [Header("Supabase Configuration")]
        [SerializeField] private string supabaseUrl = "https://xsknbbvyagngljxkftkd.supabase.co";
        [SerializeField] private string anonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTYwNTg0MzIsImV4cCI6MjA3MTYzNDQzMn0.J8G45OdXxTbWuGO8N05_50qOVPq7BMSDo1xeegXQMW0";
        
        // Singleton pattern
        public static SupabaseClient Instance { get; private set; }
        
        // Authentication
        private string accessToken = "";
        private string refreshToken = "";
        private bool isAuthenticated = false;
        
        public bool IsAuthenticated => isAuthenticated;
        public string AccessToken => accessToken;
        
        // Events
        public System.Action<bool> OnAuthenticationChanged;
        public System.Action<string> OnError;
        
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
            Debug.Log("🔌 Supabase Client Initialized");
            
            // Check for stored authentication
            LoadStoredAuth();
        }
        
        #region Authentication
        
        public async Task<bool> SignIn(string email, string password)
        {
            try
            {
                Debug.Log($"🔐 Attempting to sign in: {email}");
                
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
                    isAuthenticated = true;
                    
                    SaveAuth();
                    OnAuthenticationChanged?.Invoke(true);
                    
                    Debug.Log("✅ Sign in successful");
                    return true;
                }
                else
                {
                    Debug.LogError("❌ Sign in failed: Invalid response");
                    OnError?.Invoke("Sign in failed");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Sign in error: {e.Message}");
                OnError?.Invoke($"Sign in error: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SignUp(string email, string password)
        {
            try
            {
                Debug.Log($"📝 Attempting to sign up: {email}");
                
                var signUpData = new
                {
                    email = email,
                    password = password
                };
                
                string jsonData = JsonConvert.SerializeObject(signUpData);
                string response = await PostRequest("/auth/v1/signup", jsonData);
                
                Debug.Log("✅ Sign up successful - Check email for confirmation");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Sign up error: {e.Message}");
                OnError?.Invoke($"Sign up error: {e.Message}");
                return false;
            }
        }
        
        public void SignOut()
        {
            accessToken = "";
            refreshToken = "";
            isAuthenticated = false;
            
            ClearStoredAuth();
            OnAuthenticationChanged?.Invoke(false);
            
            Debug.Log("👋 Signed out successfully");
        }
        
        #endregion
        
        #region Database Operations
        
        public async Task<T> GetData<T>(string table, string filter = "")
        {
            try
            {
                string endpoint = $"/rest/v1/{table}";
                if (!string.IsNullOrEmpty(filter))
                {
                    endpoint += $"?{filter}";
                }
                
                string response = await GetRequest(endpoint);
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Get data error: {e.Message}");
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
                Debug.LogError($"❌ Post data error: {e.Message}");
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
                request.SetRequestHeader("apikey", anonKey);
                request.SetRequestHeader("Content-Type", "application/json");
                
                if (isAuthenticated)
                {
                    request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                }
                
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
                    throw new Exception($"Request failed: {request.error}");
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
                
                request.SetRequestHeader("apikey", anonKey);
                request.SetRequestHeader("Content-Type", "application/json");
                
                if (isAuthenticated)
                {
                    request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                }
                
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
                    throw new Exception($"Request failed: {request.error}");
                }
            }
        }
        
        #endregion
        
        #region Auth Storage
        
        private void SaveAuth()
        {
            PlayerPrefs.SetString("supabase_access_token", accessToken);
            PlayerPrefs.SetString("supabase_refresh_token", refreshToken);
            PlayerPrefs.SetInt("supabase_authenticated", 1);
            PlayerPrefs.Save();
        }
        
        private void LoadStoredAuth()
        {
            if (PlayerPrefs.HasKey("supabase_authenticated") && PlayerPrefs.GetInt("supabase_authenticated") == 1)
            {
                accessToken = PlayerPrefs.GetString("supabase_access_token", "");
                refreshToken = PlayerPrefs.GetString("supabase_refresh_token", "");
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    isAuthenticated = true;
                    OnAuthenticationChanged?.Invoke(true);
                    Debug.Log("🔐 Restored authentication from storage");
                }
            }
        }
        
        private void ClearStoredAuth()
        {
            PlayerPrefs.DeleteKey("supabase_access_token");
            PlayerPrefs.DeleteKey("supabase_refresh_token");
            PlayerPrefs.DeleteKey("supabase_authenticated");
            PlayerPrefs.Save();
        }
        
        #endregion
    }
    
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
    }
}