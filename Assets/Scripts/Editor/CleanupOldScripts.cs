using UnityEngine;
using UnityEditor;
using System.IO;

namespace PotatoLegends.Editor
{
    public class CleanupOldScripts : EditorWindow
    {
        [MenuItem("Potato Legends/Cleanup Old Scripts")]
        public static void CleanupOldScripts()
        {
            Debug.Log("🧹 Cleaning up old complex scripts...");
            
            // List of old scripts to remove
            string[] oldScripts = {
                "Assets/Scripts/Editor/MobileSceneBuilder.cs",
                "Assets/Scripts/Editor/SceneUIAttacher.cs",
                "Assets/Scripts/Editor/CompilationTest.cs",
                "Assets/Scripts/Editor/InputManagerSetup.cs",
                "Assets/Scripts/Editor/WebVersionReplicator.cs"
            };
            
            foreach (string script in oldScripts)
            {
                if (File.Exists(script))
                {
                    File.Delete(script);
                    Debug.Log($"✅ Removed: {script}");
                }
            }
            
            // Refresh asset database
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Cleanup complete! Only essential scripts remain.");
        }
    }
}