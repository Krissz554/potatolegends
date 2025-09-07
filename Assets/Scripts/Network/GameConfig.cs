using UnityEngine;

namespace PotatoLegends.Network
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Potato Legends/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Supabase Configuration")]
        [Tooltip("Your Supabase project URL")]
        public string supabaseUrl = "https://your-project.supabase.co";
        
        [Tooltip("Your Supabase anonymous key")]
        public string supabaseAnonKey = "your-anon-key";
        
        [Header("Ably Configuration")]
        [Tooltip("Your Ably API key for real-time features")]
        public string ablyApiKey = "your-ably-key";
        
        [Header("Game Settings")]
        [Tooltip("Enable debug logging")]
        public bool enableDebugLogging = true;
        
        [Tooltip("Enable haptic feedback on mobile")]
        public bool enableHapticFeedback = true;
        
        [Tooltip("Default card images path in Resources folder")]
        public string cardImagesPath = "Cards/";
        
        [Tooltip("Default hero images path in Resources folder")]
        public string heroImagesPath = "Heroes/";
    }
}