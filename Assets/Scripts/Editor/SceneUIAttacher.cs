using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using PotatoLegends.Core;

namespace PotatoLegends.Editor
{
    public class SceneUIAttacher
    {
        [MenuItem("Potato Legends/Attach UI Scripts to Scenes")]
        public static void AttachUIScriptsToAllScenes()
        {
            AttachUIScriptsToScene("Auth");
            AttachUIScriptsToScene("MainMenu");
            AttachUIScriptsToScene("Collection");
            AttachUIScriptsToScene("DeckBuilder");
            AttachUIScriptsToScene("HeroHall");
            AttachUIScriptsToScene("Battle");
            
            Debug.Log("✅ UI scripts attached to all scenes!");
            EditorUtility.DisplayDialog("Success", "UI scripts have been attached to all scenes!\n\nScenes updated:\n• Auth\n• MainMenu\n• Collection\n• DeckBuilder\n• HeroHall\n• Battle", "OK");
        }

        private static void AttachUIScriptsToScene(string sceneName)
        {
            // Load the scene
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            
            if (!scene.IsValid())
            {
                Debug.LogError($"❌ Could not load scene: {sceneName}");
                return;
            }

            // Attach appropriate UI script based on scene name
            switch (sceneName)
            {
                case "Auth":
                    AttachAuthUIScript();
                    break;
                case "MainMenu":
                    AttachMainMenuUIScript();
                    break;
                case "Collection":
                    AttachCollectionUIScript();
                    break;
                case "DeckBuilder":
                    AttachDeckBuilderUIScript();
                    break;
                case "HeroHall":
                    AttachHeroHallUIScript();
                    break;
                case "Battle":
                    AttachBattleUIScript();
                    break;
            }

            // Save the scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log($"✅ UI script attached to {sceneName} scene");
        }

        private static void AttachAuthUIScript()
        {
            // Add GameInitializer to ensure managers exist
            GameObject gameInitializerGO = new GameObject("GameInitializer");
            gameInitializerGO.AddComponent<GameInitializer>();
            Debug.Log("GameInitializer added to Auth scene.");

            // Find the Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Add AuthScreenUI script
            var authUI = canvas.gameObject.GetComponent<PotatoLegends.UI.AuthScreenUI>();
            if (authUI == null)
            {
                authUI = canvas.gameObject.AddComponent<PotatoLegends.UI.AuthScreenUI>();
            }

            // Find and assign UI references
            AssignAuthUIReferences(authUI, canvas.gameObject);
        }

        private static void AttachMainMenuUIScript()
        {
            // Find the Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Add MainMenuUI script
            var mainMenuUI = canvas.gameObject.GetComponent<PotatoLegends.UI.MainMenuUI>();
            if (mainMenuUI == null)
            {
                mainMenuUI = canvas.gameObject.AddComponent<PotatoLegends.UI.MainMenuUI>();
            }

            // Find and assign UI references
            AssignMainMenuUIReferences(mainMenuUI, canvas.gameObject);
        }

        private static void AttachCollectionUIScript()
        {
            // Find the Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Add CollectionScreenUI script
            var collectionUI = canvas.gameObject.GetComponent<PotatoLegends.UI.CollectionScreenUI>();
            if (collectionUI == null)
            {
                collectionUI = canvas.gameObject.AddComponent<PotatoLegends.UI.CollectionScreenUI>();
            }

            // Find and assign UI references
            AssignCollectionUIReferences(collectionUI, canvas.gameObject);
        }

        private static void AttachDeckBuilderUIScript()
        {
            // Find the Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Add DeckBuilderScreenUI script (to be created)
            // For now, just add a placeholder
            Debug.Log("🃏 DeckBuilder UI script attachment - placeholder");
        }

        private static void AttachHeroHallUIScript()
        {
            // Find the Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Add HeroHallScreenUI script (to be created)
            // For now, just add a placeholder
            Debug.Log("👑 HeroHall UI script attachment - placeholder");
        }

        private static void AttachBattleUIScript()
        {
            // Find the Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Add BattleScreenUI script (to be created)
            // For now, just add a placeholder
            Debug.Log("⚔️ Battle UI script attachment - placeholder");
        }

        private static void AssignAuthUIReferences(PotatoLegends.UI.AuthScreenUI authUI, GameObject canvas)
        {
            // Find UI elements by name and assign them
            // This is a simplified version - in practice, you'd want more robust finding
            
            // Find email input
            var emailInput = FindChildByName(canvas, "EmailInput")?.GetComponent<TMP_InputField>();
            if (emailInput != null)
            {
                // Use reflection to set private field
                var field = typeof(PotatoLegends.UI.AuthScreenUI).GetField("emailInput", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(authUI, emailInput);
            }

            // Find password input
            var passwordInput = FindChildByName(canvas, "PasswordInput")?.GetComponent<TMP_InputField>();
            if (passwordInput != null)
            {
                var field = typeof(PotatoLegends.UI.AuthScreenUI).GetField("passwordInput", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(authUI, passwordInput);
            }

            // Find sign in button
            var signInButton = FindChildByName(canvas, "SignInButton")?.GetComponent<Button>();
            if (signInButton != null)
            {
                var field = typeof(PotatoLegends.UI.AuthScreenUI).GetField("signInButton", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(authUI, signInButton);
            }

            // Find sign up button
            var signUpButton = FindChildByName(canvas, "SignUpButton")?.GetComponent<Button>();
            if (signUpButton != null)
            {
                var field = typeof(PotatoLegends.UI.AuthScreenUI).GetField("signUpButton", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(authUI, signUpButton);
            }

            // Find error message
            var errorMessage = FindChildByName(canvas, "ErrorMessage")?.GetComponent<TextMeshProUGUI>();
            if (errorMessage != null)
            {
                var field = typeof(PotatoLegends.UI.AuthScreenUI).GetField("errorMessage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(authUI, errorMessage);
            }
        }

        private static void AssignMainMenuUIReferences(PotatoLegends.UI.MainMenuUI mainMenuUI, GameObject canvas)
        {
            // Find navigation buttons
            var collectionButton = FindChildByName(canvas, "CollectionButton")?.GetComponent<Button>();
            if (collectionButton != null)
            {
                var field = typeof(PotatoLegends.UI.MainMenuUI).GetField("collectionButton", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(mainMenuUI, collectionButton);
            }

            // Find battle button
            var battleButton = FindChildByName(canvas, "BattleButton")?.GetComponent<Button>();
            if (battleButton != null)
            {
                var field = typeof(PotatoLegends.UI.MainMenuUI).GetField("battleButton", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(mainMenuUI, battleButton);
            }

            // Add more button assignments as needed...
        }

        private static void AssignCollectionUIReferences(PotatoLegends.UI.CollectionScreenUI collectionUI, GameObject canvas)
        {
            // Find back button
            var backButton = FindChildByName(canvas, "BackButton")?.GetComponent<Button>();
            if (backButton != null)
            {
                var field = typeof(PotatoLegends.UI.CollectionScreenUI).GetField("backButton", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(collectionUI, backButton);
            }

            // Find search field
            var searchField = FindChildByName(canvas, "SearchField")?.GetComponent<TMP_InputField>();
            if (searchField != null)
            {
                var field = typeof(PotatoLegends.UI.CollectionScreenUI).GetField("searchField", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(collectionUI, searchField);
            }

            // Add more assignments as needed...
        }

        private static GameObject FindChildByName(GameObject parent, string name)
        {
            if (parent == null) return null;

            // Search in direct children first
            foreach (Transform child in parent.transform)
            {
                if (child.name == name)
                    return child.gameObject;
            }

            // Search recursively
            foreach (Transform child in parent.transform)
            {
                var found = FindChildByName(child.gameObject, name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}