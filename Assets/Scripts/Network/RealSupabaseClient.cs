using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// REAL Supabase client that actually connects to your database
/// Implements all the functionality from your web version
/// </summary>
public class RealSupabaseClient : MonoBehaviour
    {
        [Header("Supabase Configuration")]
        private const string SUPABASE_URL = "https://xsknbbvyagngljxkftkd.supabase.co";
        private const string SUPABASE_ANON_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhza25iYnZ5YWduZ2xqeGtmdGtkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTYwNTg0MzIsImV4cCI6MjA3MTYzNDQzMn0.J8G45OdXxTbWuGO8N05_50qOVPq7BMSDo1xeegXQMW0";
        
        // Singleton
        public static RealSupabaseClient Instance { get; private set; }
        
        // Authentication state
        private string accessToken = "";
        private string refreshToken = "";
        private string userId = "";
        private bool isAuthenticated = false;
        private UserProfile currentUserProfile;
        
        // Events
        public System.Action<bool> OnAuthenticationChanged;
        public System.Action<string> OnError;
        public System.Action<UserProfile> OnUserProfileLoaded;
        public System.Action<List<EnhancedCard>> OnCardsLoaded;
        public System.Action<List<CollectionItem>> OnCollectionLoaded;
        
        // Properties
        public bool IsAuthenticated => isAuthenticated;
        public string UserId => userId;
        public UserProfile CurrentUser => currentUserProfile;
        
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
            Debug.Log("🔌 Real Supabase Client Initialized");
            LoadStoredAuth();
        }
        
        #region Authentication (Real Implementation)
        
        public async Task<bool> SignIn(string email, string password)
        {
            try
            {
                Debug.Log($"🔐 Signing in: {email}");
                
                var signInData = new
                {
                    email = email.Trim().ToLower(),
                    password = password
                };
                
                string response = await PostRequest("/auth/v1/token?grant_type=password", JsonConvert.SerializeObject(signInData));
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);
                
                if (authResponse?.access_token != null)
                {
                    accessToken = authResponse.access_token;
                    refreshToken = authResponse.refresh_token;
                    userId = authResponse.user.id;
                    isAuthenticated = true;
                    
                    SaveAuth();
                    OnAuthenticationChanged?.Invoke(true);
                    
                    Debug.Log("✅ Sign in successful");
                    
                    // Load user profile
                    await LoadUserProfile();
                    
                    return true;
                }
                
                Debug.LogError("❌ Sign in failed");
                OnError?.Invoke("Invalid email or password");
                return false;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Sign in error: {e.Message}");
                OnError?.Invoke("Sign in failed. Please try again.");
                return false;
            }
        }
        
        public async Task<bool> SignUp(string email, string password, string displayName = "")
        {
            try
            {
                Debug.Log($"📝 Signing up: {email}");
                
                var signUpData = new
                {
                    email = email.Trim().ToLower(),
                    password = password,
                    data = new
                    {
                        display_name = displayName
                    }
                };
                
                await PostRequest("/auth/v1/signup", JsonConvert.SerializeObject(signUpData));
                
                Debug.Log("✅ Sign up successful");
                return true;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Sign up error: {e.Message}");
                OnError?.Invoke("Sign up failed. Please try again.");
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
                currentUserProfile = null;
                
                ClearStoredAuth();
                OnAuthenticationChanged?.Invoke(false);
                
                Debug.Log("👋 Signed out successfully");
                return true;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Sign out error: {e.Message}");
                return false;
            }
        }
        
        private async Task LoadUserProfile()
        {
            try
            {
                var profiles = await GetData<List<UserProfile>>($"user_profiles?id=eq.{userId}&select=*");
                
                if (profiles != null && profiles.Count > 0)
                {
                    currentUserProfile = profiles[0];
                    OnUserProfileLoaded?.Invoke(currentUserProfile);
                    Debug.Log($"👤 User profile loaded: {currentUserProfile.display_name ?? currentUserProfile.email}");
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading user profile: {e.Message}");
            }
        }
        
        #endregion
        
        #region Card System (Real Implementation)
        
        public async Task<List<EnhancedCard>> LoadAllCards()
        {
            try
            {
                Debug.Log("🃏 Loading all cards from database...");
                
                var cards = await GetData<List<EnhancedCard>>("card_complete?select=*&order=name.asc");
                
                if (cards != null)
                {
                    Debug.Log($"✅ Loaded {cards.Count} cards from database");
                    OnCardsLoaded?.Invoke(cards);
                    return cards;
                }
                
                return new List<EnhancedCard>();
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading cards: {e.Message}");
                return new List<EnhancedCard>();
            }
        }
        
        public async Task<List<CollectionItem>> LoadUserCollection()
        {
            try
            {
                if (!isAuthenticated)
                {
                    Debug.LogWarning("⚠️ Cannot load collection - user not authenticated");
                    return new List<CollectionItem>();
                }
                
                Debug.Log($"📚 Loading collection for user: {userId}");
                
                // Use the same RPC function as web version
                var collectionData = await CallRPC<object>("get_user_collection", new { user_uuid = userId });
                
                if (collectionData != null)
                {
                    // Parse the collection data (same as web version)
                    List<CollectionItem> collection = ParseCollectionData(collectionData);
                    
                    Debug.Log($"✅ Loaded {collection.Count} cards in user collection");
                    OnCollectionLoaded?.Invoke(collection);
                    return collection;
                }
                
                return new List<CollectionItem>();
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading collection: {e.Message}");
                return new List<CollectionItem>();
            }
        }
        
        // DECK MANAGEMENT METHODS
        public async Task<List<Deck>> LoadUserDecks()
        {
            Debug.Log($"🃏 Loading decks for user: {userId}");
            
            try
            {
                var decks = await GetData<List<Deck>>($"/rest/v1/decks?user_id=eq.{userId}&order=created_at.desc");
                
                Debug.Log($"✅ Loaded {decks?.Count ?? 0} user decks");
                return decks ?? new List<Deck>();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading user decks: {e.Message}");
                return new List<Deck>();
            }
        }
        
        public async Task<Deck> LoadDeckCards(string deckId)
        {
            Debug.Log($"🃏 Loading cards for deck: {deckId}");
            
            try
            {
                // First get the deck info
                var deckInfo = await GetData<List<Deck>>($"/rest/v1/decks?id=eq.{deckId}");
                if (deckInfo == null || deckInfo.Count == 0)
                {
                    Debug.LogError($"❌ Deck not found: {deckId}");
                    return null;
                }
                
                var deck = deckInfo[0];
                
                // Then get the deck cards with card details
                var deckCardsQuery = $"/rest/v1/deck_cards?deck_id=eq.{deckId}&select=quantity,card_id,cards:card_complete(*)";
                var deckCardsResult = await GetRequest(deckCardsQuery);
                
                // Parse the deck cards
                var deckCards = JsonConvert.DeserializeObject<List<DeckCard>>(deckCardsResult);
                deck.cards = deckCards ?? new List<DeckCard>();
                deck.total_cards = deck.cards.Sum(dc => dc.quantity);
                
                Debug.Log($"✅ Loaded deck '{deck.name}' with {deck.total_cards} cards");
                return deck;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error loading deck cards: {e.Message}");
                return null;
            }
        }
        
        public async Task<bool> CreateDeck(string deckName)
        {
            Debug.Log($"🃏 Creating deck: {deckName}");
            
            try
            {
                // Check if this will be the user's first deck
                var existingDecks = await LoadUserDecks();
                bool isFirstDeck = existingDecks.Count == 0;
                
                var deckData = new 
                {
                    user_id = userId,
                    name = deckName,
                    is_active = isFirstDeck
                };
                
                await PostData<object>("/rest/v1/decks", deckData);
                
                Debug.Log($"✅ Created deck '{deckName}'{(isFirstDeck ? " and set as active" : "")}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error creating deck: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SaveDeckCards(string deckId, List<DeckCard> deckCards)
        {
            Debug.Log($"🃏 Saving {deckCards.Count} cards to deck: {deckId}");
            
            try
            {
                // Delete existing cards
                await DeleteData("deck_cards", $"deck_id=eq.{deckId}");
                
                // Insert new cards
                if (deckCards.Count > 0)
                {
                    var deckCardData = deckCards.Select(dc => new {
                        deck_id = deckId,
                        card_id = dc.card.id,
                        quantity = dc.quantity
                    });
                    
                    await PostData<object>("/rest/v1/deck_cards", deckCardData);
                }
                
                Debug.Log($"✅ Saved deck cards successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error saving deck cards: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SetActiveDeck(string deckId)
        {
            Debug.Log($"🃏 Setting active deck: {deckId}");
            
            try
            {
                // Set all decks to inactive
                await PatchData($"/rest/v1/decks?user_id=eq.{userId}", new { is_active = false });
                
                // Set the selected deck as active
                await PatchData($"/rest/v1/decks?id=eq.{deckId}", new { is_active = true });
                
                Debug.Log($"✅ Set deck as active successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error setting active deck: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> DeleteDeck(string deckId)
        {
            Debug.Log($"🃏 Deleting deck: {deckId}");
            
            try
            {
                // Delete deck cards first (foreign key constraint)
                await DeleteData("deck_cards", $"deck_id=eq.{deckId}");
                
                // Delete the deck itself
                await DeleteData("decks", $"id=eq.{deckId}");
                
                Debug.Log($"✅ Deleted deck successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error deleting deck: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SaveDeck(Deck deck)
        {
            Debug.Log($"💾 Saving deck: {deck.name}");
            
            try
            {
                // Save deck metadata
                var deckData = new
                {
                    id = deck.id,
                    user_id = userId,
                    name = deck.name,
                    is_active = deck.is_active,
                    updated_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };
                
                await PostData<object>("decks", deckData);
                
                // Save deck cards using the dedicated method
                await SaveDeckCards(deck.id, deck.cards);
                
                Debug.Log("✅ Deck saved successfully");
                return true;
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error saving deck: {e.Message}");
                return false;
            }
        }
        
        private List<CollectionItem> ParseCollectionData(object data)
        {
            // Parse collection data exactly like web version
            List<CollectionItem> collection = new List<CollectionItem>();
            
            try
            {
                string jsonString = JsonConvert.SerializeObject(data);
                var collectionArray = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);
                
                foreach (var item in collectionArray)
                {
                    // Parse each collection item
                    CollectionItem collectionItem = new CollectionItem
                    {
                        quantity = Convert.ToInt32(item["quantity"]),
                        acquired_at = item["acquired_at"].ToString(),
                        source = item.ContainsKey("source") ? item["source"].ToString() : "unknown"
                    };
                    
                    // Parse card data
                    if (item.ContainsKey("card"))
                    {
                        string cardJson = JsonConvert.SerializeObject(item["card"]);
                        collectionItem.card = JsonConvert.DeserializeObject<EnhancedCard>(cardJson);
                    }
                    
                    collection.Add(collectionItem);
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error parsing collection data: {e.Message}");
            }
            
            return collection;
        }
        
        #endregion
        
        #region Battle System (Real Implementation)
        
        public async Task<bool> JoinMatchmaking()
        {
            try
            {
                Debug.Log("🔍 Joining matchmaking queue...");
                
                var matchmakingData = new
                {
                    user_id = userId,
                    joined_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    status = "searching"
                };
                
                await PostData<object>("matchmaking_queue", matchmakingData);
                
                Debug.Log("✅ Joined matchmaking queue");
                return true;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error joining matchmaking: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> LeaveMatchmaking()
        {
            try
            {
                await DeleteData("matchmaking_queue", $"user_id=eq.{userId}");
                Debug.Log("❌ Left matchmaking queue");
                return true;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error leaving matchmaking: {e.Message}");
                return false;
            }
        }
        
        public async Task<BattleSession> CheckForBattle()
        {
            try
            {
                var battles = await GetData<List<BattleSession>>(
                    $"battle_sessions?or=(player1_id.eq.{userId},player2_id.eq.{userId})&status=eq.active&select=*"
                );
                
                if (battles != null && battles.Count > 0)
                {
                    Debug.Log($"⚔️ Found active battle: {battles[0].id}");
                    return battles[0];
                }
                
                return null;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error checking for battle: {e.Message}");
                return null;
            }
        }
        
        #endregion
        
        #region Hero System (Real Implementation)
        
        public async Task<List<Hero>> LoadAvailableHeroes()
        {
            try
            {
                var heroes = await GetData<List<Hero>>("heroes?select=*&order=name.asc");
                
                if (heroes != null)
                {
                    Debug.Log($"🦸 Loaded {heroes.Count} available heroes");
                    return heroes;
                }
                
                return new List<Hero>();
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading heroes: {e.Message}");
                return new List<Hero>();
            }
        }
        
        public async Task<List<UserHero>> LoadUserHeroes()
        {
            try
            {
                var userHeroes = await GetData<List<UserHero>>($"user_heroes?user_id=eq.{userId}&select=*,heroes(*)");
                
                if (userHeroes != null)
                {
                    Debug.Log($"🦸 Loaded {userHeroes.Count} user heroes");
                    return userHeroes;
                }
                
                return new List<UserHero>();
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Error loading user heroes: {e.Message}");
                return new List<UserHero>();
            }
        }
        
        #endregion
        
        #region HTTP Operations
        
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
                Debug.LogError($"❌ GET error: {e.Message}");
                throw;
            }
        }
        
        public async Task<T> PostData<T>(string table, object data)
        {
            try
            {
                string endpoint = $"/rest/v1/{table}";
                string jsonData = JsonConvert.SerializeObject(data);
                
                string response = await PostRequest(endpoint, jsonData);
                return JsonConvert.DeserializeObject<T>(response);
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ POST error: {e.Message}");
                throw;
            }
        }
        
        public async Task<T> CallRPC<T>(string functionName, object parameters)
        {
            try
            {
                string endpoint = $"/rest/v1/rpc/{functionName}";
                string jsonData = JsonConvert.SerializeObject(parameters);
                
                string response = await PostRequest(endpoint, jsonData);
                return JsonConvert.DeserializeObject<T>(response);
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ RPC error: {e.Message}");
                throw;
            }
        }
        
        public async Task<bool> DeleteData(string table, string filter)
        {
            try
            {
                string endpoint = $"/rest/v1/{table}?{filter}";
                await DeleteRequest(endpoint);
                return true;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ DELETE error: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> PatchData(string endpoint, object data)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(data);
                await PatchRequest(endpoint, jsonData);
                return true;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ PATCH error: {e.Message}");
                return false;
            }
        }
        
        private async Task<string> GetRequest(string endpoint)
        {
            string url = SUPABASE_URL + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                SetHeaders(request);
                
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Yield();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"GET failed: {request.error} - {request.downloadHandler.text}");
                }
            }
        }
        
        private async Task<string> PostRequest(string endpoint, string jsonData)
        {
            string url = SUPABASE_URL + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                SetHeaders(request);
                
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Yield();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"POST failed: {request.error} - {request.downloadHandler.text}");
                }
            }
        }
        
        private async Task<string> DeleteRequest(string endpoint)
        {
            string url = SUPABASE_URL + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                SetHeaders(request);
                
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Yield();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"DELETE failed: {request.error}");
                }
            }
        }
        
        private async Task<string> PatchRequest(string endpoint, string jsonData)
        {
            string url = SUPABASE_URL + endpoint;
            
            using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
            {
                request.method = "PATCH";
                SetHeaders(request);
                
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Yield();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception($"PATCH failed: {request.error}");
                }
            }
        }
        
        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("apikey", SUPABASE_ANON_KEY);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=representation");
            
            if (isAuthenticated)
            {
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            }
        }
        
        #endregion
        
        #region Data Models (Matching Web Version)
        
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
        public class EnhancedCard
        {
            public string id;
            public string registry_id;
            public string name;
            public string description;
            public string set_id;
            public List<string> format_legalities;
            
            public int mana_cost;
            public int? attack;
            public int? hp;
            public int? structure_hp;
            public int spell_damage;
            public int heal_amount;
            
            public string rarity;
            public string card_type;
            public string unit_class;
            
            public string potato_type;
            public string trait;
            public string adjective;
            
            public List<string> keywords;
            public string ability_text;
            public string passive_effect;
            public string target_type;
            
            public string illustration_url;
            public string frame_style;
            public string flavor_text;
            
            public bool exotic;
            public bool is_exotic;
            public bool is_legendary;
            public string release_date;
            public List<string> tags;
            
            public int craft_cost;
            public int dust_value;
            
            public string voice_line_url;
            public List<string> token_spawns;
        }
        
        [Serializable]
        public class CollectionItem
        {
            public EnhancedCard card;
            public int quantity;
            public string acquired_at;
            public string source;
        }
        
        [Serializable]
        public class Deck
        {
            public string id;
            public string user_id;
            public string name;
            public bool is_active;
            public List<DeckCard> cards;
            public int total_cards;
            public string created_at;
            public string updated_at;
        }
        
        [Serializable]
        public class DeckCard
        {
            public string deck_id;
            public string card_id;
            public EnhancedCard card;
            public int quantity;
        }
        
        [Serializable]
        public class Hero
        {
            public string id;
            public string hero_id;
            public string name;
            public string description;
            public int base_hp;
            public int base_mana;
            public string hero_power_name;
            public string hero_power_description;
            public int hero_power_cost;
            public string rarity;
            public string element_type;
            public string created_at;
            public string updated_at;
        }
        
        [Serializable]
        public class UserHero
        {
            public string id;
            public string user_id;
            public string hero_id;
            public bool is_active;
            public int total_wins;
            public int total_losses;
            public string created_at;
            public string updated_at;
            public Hero hero;
        }
        
        [Serializable]
        public class BattleSession
        {
            public string id;
            public string player1_id;
            public string player2_id;
            public string current_turn_player_id;
            public string status;
            public int turn_number;
            public object game_state;
            public string winner_id;
            public string started_at;
            public string finished_at;
        }
        
        #endregion
        
        #region Storage
        
        private void SaveAuth()
        {
            PlayerPrefs.SetString("supabase_access_token", accessToken);
            PlayerPrefs.SetString("supabase_refresh_token", refreshToken);
            PlayerPrefs.SetString("supabase_user_id", userId);
            PlayerPrefs.SetInt("supabase_authenticated", 1);
            PlayerPrefs.Save();
        }
        
        private void LoadStoredAuth()
        {
            if (PlayerPrefs.GetInt("supabase_authenticated", 0) == 1)
            {
                accessToken = PlayerPrefs.GetString("supabase_access_token", "");
                refreshToken = PlayerPrefs.GetString("supabase_refresh_token", "");
                userId = PlayerPrefs.GetString("supabase_user_id", "");
                
                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(userId))
                {
                    isAuthenticated = true;
                    OnAuthenticationChanged?.Invoke(true);
                    Debug.Log("🔐 Restored authentication from storage");
                    
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
        }
        
        #endregion
    }