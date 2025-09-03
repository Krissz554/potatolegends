using UnityEngine;

/// <summary>
/// Asset Debugger - Helps you check if your custom assets are in the right place
/// </summary>
public class AssetDebugger : MonoBehaviour
{
    [ContextMenu("Debug Custom Assets")]
    public void DebugCustomAssets()
    {
        Debug.Log("🔍 DEBUGGING CUSTOM ASSETS...");
        
        // Check all Resources/UI folders
        CheckResourcesFolder("UI", "Main UI folder");
        CheckResourcesFolder("UI/Icons", "Icons folder");
        CheckResourcesFolder("UI/Backgrounds", "Backgrounds folder");
        CheckResourcesFolder("UI/Buttons", "Buttons folder");
        
        // Try to load specific assets
        Debug.Log("\n🎯 TESTING SPECIFIC ASSET LOADING:");
        
        TestAssetLoad("UI/Icons/battle-icon", "Battle Icon");
        TestAssetLoad("UI/Backgrounds/main-menu-bg", "Main Menu Background");
        TestAssetLoad("UI/Icons/gold-icon", "Gold Icon");
        TestAssetLoad("UI/Icons/gems-icon", "Gems Icon");
        
        // List ALL sprites in Resources
        Debug.Log("\n📋 ALL SPRITES IN RESOURCES:");
        var allSprites = Resources.LoadAll<Sprite>("");
        Debug.Log($"Total sprites in Resources: {allSprites.Length}");
        
        foreach (var sprite in allSprites)
        {
            Debug.Log($"📄 Available: {sprite.name} (path: Resources/{GetResourcePath(sprite)})");
        }
    }
    
    private void CheckResourcesFolder(string path, string description)
    {
        var resources = Resources.LoadAll<Object>(path);
        Debug.Log($"📁 {description} ({path}): {resources.Length} files");
        
        foreach (var resource in resources)
        {
            Debug.Log($"  📄 {resource.name} ({resource.GetType().Name})");
        }
    }
    
    private void TestAssetLoad(string resourcePath, string description)
    {
        var asset = Resources.Load<Sprite>(resourcePath);
        if (asset != null)
        {
            Debug.Log($"✅ {description}: FOUND at {resourcePath}");
        }
        else
        {
            Debug.Log($"❌ {description}: NOT FOUND at {resourcePath}");
        }
    }
    
    private string GetResourcePath(Object obj)
    {
        #if UNITY_EDITOR
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
        if (assetPath.StartsWith("Assets/Resources/"))
        {
            return assetPath.Substring("Assets/Resources/".Length);
        }
        #endif
        return "Unknown";
    }
    
    [ContextMenu("List Project Assets")]
    public void ListProjectAssets()
    {
        Debug.Log("📋 LISTING ALL PROJECT ASSETS IN UI FOLDERS:");
        
        #if UNITY_EDITOR
        // Find all assets in UI folders
        string[] assetPaths = {
            "Assets/UI",
            "Assets/Resources/UI"
        };
        
        foreach (string folderPath in assetPaths)
        {
            Debug.Log($"\n📁 Checking {folderPath}:");
            
            if (System.IO.Directory.Exists(folderPath))
            {
                string[] files = System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.AllDirectories);
                
                foreach (string file in files)
                {
                    if (!file.EndsWith(".meta"))
                    {
                        Debug.Log($"📄 Found: {file}");
                    }
                }
            }
            else
            {
                Debug.Log($"❌ Folder doesn't exist: {folderPath}");
            }
        }
        #endif
    }
}