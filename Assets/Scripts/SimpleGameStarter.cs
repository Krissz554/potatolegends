using UnityEngine;

/// <summary>
/// SIMPLE Game Starter - Add this to start your Potato Legends game
/// This is outside any namespace so Unity can easily find it
/// </summary>
public class SimpleGameStarter : MonoBehaviour
{
    [Header("🥔 Potato Legends Game Starter")]
    [Tooltip("Click this button in the Inspector to start the game")]
    [SerializeField] private bool startGameOnPlay = true;
    
    private void Start()
    {
        if (startGameOnPlay)
        {
            StartGame();
        }
    }
    
    /// <summary>
    /// Call this method to start the complete game
    /// You can also use the context menu: Right-click component → "Start Potato Legends"
    /// </summary>
    [ContextMenu("Start Potato Legends")]
    public void StartGame()
    {
        Debug.Log("🥔 Starting Potato Legends Mobile Game...");
        
        // Create the complete game system
        CreateCompleteGameSystem();
        
        Debug.Log("🚀 Potato Legends game started successfully!");
    }
    
    private void CreateCompleteGameSystem()
    {
        // Check if game is already running
        if (GameObject.Find("PotatoLegendsGameSystem") != null)
        {
            Debug.Log("✅ Game system already exists!");
            return;
        }
        
        // Create main game system
        GameObject gameSystemObj = new GameObject("PotatoLegendsGameSystem");
        DontDestroyOnLoad(gameSystemObj);
        
        // Add all the real game components
        CreateSupabaseSystem(gameSystemObj);
        CreateCardSystem(gameSystemObj);
        CreateAuthSystem(gameSystemObj);
        CreateMainMenuSystem(gameSystemObj);
        CreateCollectionSystem(gameSystemObj);
        CreateDeckBuilderSystem(gameSystemObj);
        CreateHeroHallSystem(gameSystemObj);
        CreateBattleSystem(gameSystemObj);
        
        Debug.Log("✅ Complete game system created!");
    }
    
    private void CreateSupabaseSystem(GameObject parent)
    {
        GameObject supabaseObj = new GameObject("RealSupabaseClient");
        supabaseObj.transform.SetParent(parent.transform);
        
        // Add the RealSupabaseClient component
        var supabaseType = System.Type.GetType("PotatoCardGame.Network.RealSupabaseClient");
        if (supabaseType != null)
        {
            supabaseObj.AddComponent(supabaseType);
            Debug.Log("✅ Supabase system created");
        }
        else
        {
            Debug.LogError("❌ Could not find RealSupabaseClient - make sure all scripts are copied!");
        }
    }
    
    private void CreateCardSystem(GameObject parent)
    {
        GameObject cardObj = new GameObject("RealCardManager");
        cardObj.transform.SetParent(parent.transform);
        
        var cardType = System.Type.GetType("PotatoCardGame.Cards.RealCardManager");
        if (cardType != null)
        {
            cardObj.AddComponent(cardType);
            Debug.Log("✅ Card system created");
        }
    }
    
    private void CreateAuthSystem(GameObject parent)
    {
        GameObject authObj = new GameObject("RealAuthScreen");
        authObj.transform.SetParent(parent.transform);
        
        var authType = System.Type.GetType("PotatoCardGame.UI.RealAuthScreen");
        if (authType != null)
        {
            authObj.AddComponent(authType);
            Debug.Log("✅ Auth system created");
        }
    }
    
    private void CreateMainMenuSystem(GameObject parent)
    {
        GameObject menuObj = new GameObject("RealMainMenu");
        menuObj.transform.SetParent(parent.transform);
        
        var menuType = System.Type.GetType("PotatoCardGame.UI.RealMainMenu");
        if (menuType != null)
        {
            menuObj.AddComponent(menuType);
            Debug.Log("✅ Main menu system created");
        }
    }
    
    private void CreateCollectionSystem(GameObject parent)
    {
        GameObject collectionObj = new GameObject("RealCollectionScreen");
        collectionObj.transform.SetParent(parent.transform);
        
        var collectionType = System.Type.GetType("PotatoCardGame.UI.RealCollectionScreen");
        if (collectionType != null)
        {
            collectionObj.AddComponent(collectionType);
            Debug.Log("✅ Collection system created");
        }
    }
    
    private void CreateDeckBuilderSystem(GameObject parent)
    {
        GameObject deckObj = new GameObject("RealDeckBuilder");
        deckObj.transform.SetParent(parent.transform);
        
        var deckType = System.Type.GetType("PotatoCardGame.UI.RealDeckBuilder");
        if (deckType != null)
        {
            deckObj.AddComponent(deckType);
            Debug.Log("✅ Deck builder system created");
        }
    }
    
    private void CreateHeroHallSystem(GameObject parent)
    {
        GameObject heroObj = new GameObject("RealHeroHall");
        heroObj.transform.SetParent(parent.transform);
        
        var heroType = System.Type.GetType("PotatoCardGame.UI.RealHeroHall");
        if (heroType != null)
        {
            heroObj.AddComponent(heroType);
            Debug.Log("✅ Hero hall system created");
        }
    }
    
    private void CreateBattleSystem(GameObject parent)
    {
        GameObject battleObj = new GameObject("RealBattleArena");
        battleObj.transform.SetParent(parent.transform);
        
        var battleType = System.Type.GetType("PotatoCardGame.Battle.RealBattleArena");
        if (battleType != null)
        {
            battleObj.AddComponent(battleType);
            Debug.Log("✅ Battle system created");
        }
    }
    
    /// <summary>
    /// Test method to check if all scripts are properly loaded
    /// </summary>
    [ContextMenu("Test Script Loading")]
    public void TestScriptLoading()
    {
        Debug.Log("🔍 Testing script loading...");
        
        string[] requiredTypes = {
            "PotatoCardGame.Network.RealSupabaseClient",
            "PotatoCardGame.Cards.RealCardManager", 
            "PotatoCardGame.UI.RealAuthScreen",
            "PotatoCardGame.UI.RealMainMenu",
            "PotatoCardGame.UI.RealCollectionScreen",
            "PotatoCardGame.UI.RealDeckBuilder",
            "PotatoCardGame.UI.RealHeroHall",
            "PotatoCardGame.Battle.RealBattleArena",
            "PotatoCardGame.Core.RealGameManager"
        };
        
        foreach (string typeName in requiredTypes)
        {
            var type = System.Type.GetType(typeName);
            if (type != null)
            {
                Debug.Log($"✅ Found: {typeName}");
            }
            else
            {
                Debug.LogError($"❌ Missing: {typeName}");
            }
        }
    }
}