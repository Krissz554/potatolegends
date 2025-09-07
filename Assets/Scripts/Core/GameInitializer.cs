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
            // Ensure HttpClient exists first
            if (PotatoLegends.Network.HttpClient.Instance == null)
            {
                GameObject httpClient = new GameObject("HttpClient");
                httpClient.AddComponent<PotatoLegends.Network.HttpClient>();
            }

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

            // Ensure CollectionManager exists
            if (PotatoLegends.Collection.CollectionManager.Instance == null)
            {
                GameObject collectionManager = new GameObject("CollectionManager");
                collectionManager.AddComponent<PotatoLegends.Collection.CollectionManager>();
            }

            // Ensure AblyClient exists
            if (PotatoLegends.Network.AblyClient.Instance == null)
            {
                GameObject ablyClient = new GameObject("AblyClient");
                ablyClient.AddComponent<PotatoLegends.Network.AblyClient>();
            }
        }
    }
}