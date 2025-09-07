using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace PotatoLegends.Editor
{
    public class SceneCreator : EditorWindow
    {
        [MenuItem("Potato Legends/Create All Scenes")]
        public static void CreateAllScenes()
        {
            CreateScene("Auth");
            CreateScene("MainMenu");
            CreateScene("Collection");
            CreateScene("DeckBuilder");
            CreateScene("HeroHall");
            CreateScene("Battle");
            
            Debug.Log("All scenes created successfully!");
        }

        private static void CreateScene(string sceneName)
        {
            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create Main Camera
            GameObject camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 0, -10);
            
            // Set camera to orthographic for 2D
            Camera cam = camera.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            
            // Save scene
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"Created scene: {sceneName}");
        }
    }
}