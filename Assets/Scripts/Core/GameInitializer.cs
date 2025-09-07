using UnityEngine;

namespace PotatoLegends.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Required Managers")]
        [SerializeField] private GameObject gameSceneManagerPrefab;
        [SerializeField] private GameObject supabaseClientPrefab;

        private void Awake()
        {
            // Ensure GameSceneManager exists
            if (GameSceneManager.Instance == null)
            {
                if (gameSceneManagerPrefab != null)
                {
                    Instantiate(gameSceneManagerPrefab);
                }
                else
                {
                    GameObject gameSceneManager = new GameObject("GameSceneManager");
                    gameSceneManager.AddComponent<GameSceneManager>();
                }
            }

            // Ensure SupabaseClient exists
            if (PotatoLegends.Network.SupabaseClient.Instance == null)
            {
                if (supabaseClientPrefab != null)
                {
                    Instantiate(supabaseClientPrefab);
                }
                else
                {
                    GameObject supabaseClient = new GameObject("SupabaseClient");
                    supabaseClient.AddComponent<PotatoLegends.Network.SupabaseClient>();
                }
            }
        }
    }
}