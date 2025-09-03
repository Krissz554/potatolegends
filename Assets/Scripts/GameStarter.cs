using UnityEngine;

/// <summary>
/// Simple Game Starter - Add this to a GameObject in your scene
/// This will automatically create the RealGameManager and start the game
/// </summary>
public class GameStarter : MonoBehaviour
{
    [Header("Game Startup")]
    [SerializeField] private bool autoStartGame = true;
    
    private void Start()
    {
        if (autoStartGame)
        {
            StartPotatoLegendsGame();
        }
    }
    
    [ContextMenu("Start Potato Legends Game")]
    public void StartPotatoLegendsGame()
    {
        Debug.Log("🥔 Starting Potato Legends Mobile Game...");
        
        // Check if RealGameManager already exists
        var existingGameManager = FindFirstObjectByType<PotatoCardGame.Core.RealGameManager>();
        if (existingGameManager != null)
        {
            Debug.Log("✅ RealGameManager already exists!");
            return;
        }
        
        // Create RealGameManager
        GameObject gameManagerObj = new GameObject("RealGameManager");
        var realGameManager = gameManagerObj.AddComponent<PotatoCardGame.Core.RealGameManager>();
        
        Debug.Log("🚀 RealGameManager created and game started!");
        
        // Optionally disable this starter after creating the game manager
        gameObject.SetActive(false);
    }
}