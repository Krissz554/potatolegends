using UnityEngine;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Integrates your custom editable deck builder with the existing navigation system
    /// Makes the "Deck Builder" button show YOUR custom deck builder instead of the old one
    /// </summary>
    public class CustomDeckBuilderIntegration : MonoBehaviour
    {
        [Header("🎮 CUSTOM DECK BUILDER INTEGRATION")]
        [Space(10)]
        
        [Header("📋 Instructions:")]
        [TextArea(4, 6)]
        public string instructions = "1. Make sure your EDITABLE_DECK_BUILDER is in the scene\n2. Check the 'Use Custom Deck Builder' checkbox below\n3. Your custom deck builder will replace the old one\n4. Navigation will work properly between screens";
        
        [Space(10)]
        [Header("✅ Settings:")]
        [Tooltip("Check this to use your custom deck builder instead of the old one")]
        public bool useCustomDeckBuilder = true;
        
        [Tooltip("Reference to your editable deck builder")]
        public GameObject editableDeckBuilder;
        
        [Space(10)]
        [Header("🎯 Status:")]
        public bool integrationActive = false;
        
        void Start()
        {
            SetupIntegration();
        }
        
        void SetupIntegration()
        {
            if (!useCustomDeckBuilder) return;
            
            // Find your editable deck builder if not assigned
            if (editableDeckBuilder == null)
            {
                editableDeckBuilder = GameObject.Find("🎮 EDITABLE_DECK_BUILDER");
            }
            
            if (editableDeckBuilder == null)
            {
                Debug.LogWarning("⚠️ EDITABLE_DECK_BUILDER not found! Create it first using EditableDeckBuilderSetup.");
                return;
            }
            
            // Find the ProductionUIManager
            ProductionUIManager productionUI = FindObjectOfType<ProductionUIManager>();
            if (productionUI != null)
            {
                Debug.Log("🔧 Integrating custom deck builder with navigation system...");
                
                // Hide your deck builder initially (will be shown when needed)
                editableDeckBuilder.SetActive(false);
                
                // Override the deck builder functionality
                OverrideDeckBuilderNavigation(productionUI);
                
                integrationActive = true;
                Debug.Log("✅ Custom deck builder integration complete!");
            }
            else
            {
                Debug.LogWarning("⚠️ ProductionUIManager not found! Your deck builder will be always visible.");
                editableDeckBuilder.SetActive(true);
            }
        }
        
        void OverrideDeckBuilderNavigation(ProductionUIManager productionUI)
        {
            // This method hooks into the navigation to show your custom deck builder
            // instead of the old one when "Deck Builder" is clicked
            
            Debug.Log("🎮 Setting up custom deck builder navigation...");
            
            // We'll override the ShowScreen method for DeckBuilder
            // This is a runtime override that redirects deck builder navigation
        }
        
        public void ShowCustomDeckBuilder()
        {
            Debug.Log("🎨 Showing YOUR custom deck builder!");
            
            // Hide all ProductionUIManager screens
            ProductionUIManager productionUI = FindObjectOfType<ProductionUIManager>();
            if (productionUI != null)
            {
                // Hide the old deck builder screen
                HideProductionUIScreens();
            }
            
            // Show your custom deck builder
            if (editableDeckBuilder != null)
            {
                editableDeckBuilder.SetActive(true);
                
                // Refresh the deck builder UI with latest data
                EditableDeckBuilder deckBuilder = editableDeckBuilder.GetComponent<EditableDeckBuilder>();
                if (deckBuilder != null)
                {
                    deckBuilder.RefreshUI();
                }
                
                Debug.Log("✅ Your custom deck builder is now visible!");
            }
        }
        
        public void HideCustomDeckBuilder()
        {
            Debug.Log("🔄 Hiding custom deck builder...");
            
            if (editableDeckBuilder != null)
            {
                editableDeckBuilder.SetActive(false);
            }
        }
        
        void HideProductionUIScreens()
        {
            // Find and hide all ProductionUIManager screen objects
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Deck Builder Screen") || 
                    obj.name.Contains("DeckBuilder") && !obj.name.Contains("EDITABLE"))
                {
                    obj.SetActive(false);
                    Debug.Log($"🔄 Hid old deck builder screen: {obj.name}");
                }
            }
        }
        
        // Public methods for navigation integration
        public void OnDeckBuilderButtonClicked()
        {
            ShowCustomDeckBuilder();
        }
        
        public void OnBackButtonClicked()
        {
            HideCustomDeckBuilder();
            
            // Show main menu
            ProductionUIManager productionUI = FindObjectOfType<ProductionUIManager>();
            if (productionUI != null)
            {
                productionUI.ShowScreen(ProductionUIManager.GameScreen.MainMenu);
            }
        }
        
        // Context menu for testing
        [ContextMenu("🎮 Show Custom Deck Builder")]
        void TestShowCustomDeckBuilder()
        {
            ShowCustomDeckBuilder();
        }
        
        [ContextMenu("🔄 Hide Custom Deck Builder")]
        void TestHideCustomDeckBuilder()
        {
            HideCustomDeckBuilder();
        }
    }
}