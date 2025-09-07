using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PotatoLegends.Core;
using PotatoLegends.Network;
using PotatoLegends.UI;
using PotatoLegends.Collection;

namespace PotatoLegends.Editor
{
    public class CompilationTest : EditorWindow
    {
        [MenuItem("Potato Legends/Test Compilation")]
        public static void TestCompilation()
        {
            Debug.Log("🧪 Testing Compilation...");
            
            // Test basic Unity types
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.Log($"✅ SceneManager works: {scene.name}");
            
            // Test UI types
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"✅ Canvas found: {canvas.name}");
            }
            
            // Test EventSystem types
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                Debug.Log($"✅ EventSystem found: {eventSystem.name}");
            }
            
            // Test our custom types
            var gameInitializer = Object.FindFirstObjectByType<GameInitializer>();
            if (gameInitializer != null)
            {
                Debug.Log($"✅ GameInitializer found: {gameInitializer.name}");
            }
            
            // Test UI scripts
            var authScreenUI = Object.FindFirstObjectByType<AuthScreenUI>();
            if (authScreenUI != null)
            {
                Debug.Log($"✅ AuthScreenUI found: {authScreenUI.name}");
            }
            
            var mainMenuUI = Object.FindFirstObjectByType<MainMenuUI>();
            if (mainMenuUI != null)
            {
                Debug.Log($"✅ MainMenuUI found: {mainMenuUI.name}");
            }
            
            Debug.Log("🎉 Compilation Test Complete - All types are working!");
        }
    }
}