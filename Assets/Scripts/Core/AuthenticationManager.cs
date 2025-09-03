using System;
using System.Threading.Tasks;
using UnityEngine;
using PotatoCardGame.Network;

namespace PotatoCardGame.Core
{
    /// <summary>
    /// Manages user authentication state and provides a unified interface
    /// Integrates with Supabase authentication and manages user sessions
    /// </summary>
    public class AuthenticationManager : MonoBehaviour
    {
        [Header("Authentication Settings")]
        [SerializeField] private bool autoLogin = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        public static AuthenticationManager Instance { get; private set; }
        
        // User data
        private UserProfile currentUser;
        private bool isAuthenticated = false;
        
        // Events
        public System.Action<bool> OnAuthenticationChanged;
        public System.Action<UserProfile> OnUserProfileLoaded;
        public System.Action<string> OnAuthenticationError;
        
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
            if (enableDebugLogs) Debug.Log("🔐 Authentication Manager Initialized");
            
            // Setup Supabase event listeners
            if (SupabaseClient.Instance)
            {
                SupabaseClient.Instance.OnAuthenticationChanged += OnSupabaseAuthChanged;
                SupabaseClient.Instance.OnError += OnSupabaseError;
            }
            
            // Try auto-login if enabled
            if (autoLogin)
            {
                CheckStoredAuthentication();
            }
        }
        
        #region Authentication Methods
        
        public async Task<bool> SignIn(string email, string password)
        {
            try
            {
                if (enableDebugLogs) Debug.Log($"🔑 Attempting sign in: {email}");
                
                bool success = await SupabaseClient.Instance.SignIn(email, password);
                
                if (success)
                {
                    await LoadUserProfile();
                    if (enableDebugLogs) Debug.Log("✅ Sign in successful");
                }
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Sign in error: {e.Message}");
                OnAuthenticationError?.Invoke($"Sign in failed: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SignUp(string email, string password, string displayName = "")
        {
            try
            {
                if (enableDebugLogs) Debug.Log($"📝 Attempting sign up: {email}");
                
                bool success = await SupabaseClient.Instance.SignUp(email, password);
                
                if (success)
                {
                    // Create user profile
                    await CreateUserProfile(email, displayName);
                    if (enableDebugLogs) Debug.Log("✅ Sign up successful");
                }
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Sign up error: {e.Message}");
                OnAuthenticationError?.Invoke($"Sign up failed: {e.Message}");
                return false;
            }
        }
        
        public void SignOut()
        {
            if (enableDebugLogs) Debug.Log("👋 Signing out...");
            
            SupabaseClient.Instance.SignOut();
            
            currentUser = null;
            isAuthenticated = false;
            
            OnAuthenticationChanged?.Invoke(false);
        }
        
        private void CheckStoredAuthentication()
        {
            if (SupabaseClient.Instance && SupabaseClient.Instance.IsAuthenticated)
            {
                isAuthenticated = true;
                LoadUserProfile();
                OnAuthenticationChanged?.Invoke(true);
                
                if (enableDebugLogs) Debug.Log("🔐 Restored authentication from storage");
            }
        }
        
        #endregion
        
        #region User Profile Management
        
        private async Task LoadUserProfile()
        {
            try
            {
                if (!SupabaseClient.Instance.IsAuthenticated) return;
                
                // TODO: Get user ID from Supabase auth token
                string userId = "placeholder-user-id";
                
                if (enableDebugLogs) Debug.Log($"👤 Loading user profile: {userId}");
                
                // Load user profile from database
                var profiles = await SupabaseClient.Instance.GetData<List<UserProfile>>($"user_profiles?id=eq.{userId}");
                
                if (profiles != null && profiles.Count > 0)
                {
                    currentUser = profiles[0];
                    OnUserProfileLoaded?.Invoke(currentUser);
                    
                    if (enableDebugLogs) Debug.Log($"✅ User profile loaded: {currentUser.display_name}");
                }
                else
                {
                    Debug.LogWarning("⚠️ User profile not found");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading user profile: {e.Message}");
            }
        }
        
        private async Task CreateUserProfile(string email, string displayName)
        {
            try
            {
                var profile = new UserProfile
                {
                    id = Guid.NewGuid().ToString(), // TODO: Use actual user ID from auth
                    email = email,
                    display_name = string.IsNullOrEmpty(displayName) ? email.Split('@')[0] : displayName,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };
                
                bool success = await SupabaseClient.Instance.PostData("user_profiles", profile);
                
                if (success)
                {
                    currentUser = profile;
                    OnUserProfileLoaded?.Invoke(currentUser);
                    
                    if (enableDebugLogs) Debug.Log($"✅ User profile created: {profile.display_name}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error creating user profile: {e.Message}");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSupabaseAuthChanged(bool authenticated)
        {
            isAuthenticated = authenticated;
            
            if (authenticated)
            {
                LoadUserProfile();
            }
            else
            {
                currentUser = null;
            }
            
            OnAuthenticationChanged?.Invoke(authenticated);
            
            if (enableDebugLogs) Debug.Log($"🔐 Authentication changed: {authenticated}");
        }
        
        private void OnSupabaseError(string errorMessage)
        {
            OnAuthenticationError?.Invoke(errorMessage);
        }
        
        #endregion
        
        #region Public Getters
        
        public bool IsAuthenticated() => isAuthenticated;
        public UserProfile GetCurrentUser() => currentUser;
        public string GetCurrentUserId() => currentUser?.id ?? "";
        public string GetCurrentUserName() => currentUser?.display_name ?? "Player";
        public string GetCurrentUserEmail() => currentUser?.email ?? "";
        
        #endregion
    }
    
    /// <summary>
    /// User profile data structure matching the database
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public string id;
        public string email;
        public string display_name;
        public string username;
        public DateTime created_at;
        public DateTime updated_at;
        
        // Additional game-specific stats
        public int total_games_played;
        public int total_wins;
        public int total_losses;
        public int cards_collected;
        public int decks_created;
        public string favorite_hero;
        public DateTime last_login;
    }
}