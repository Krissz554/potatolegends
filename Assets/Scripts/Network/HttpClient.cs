using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PotatoLegends.Network
{
    public class HttpClient : MonoBehaviour
    {
        public static HttpClient Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator MakeRequest(string url, string method, string body, Dictionary<string, string> headers, System.Action<string, string> callback)
        {
            // For now, simulate HTTP requests since UnityWebRequest has package issues
            Debug.Log($"HTTP {method} {url}");
            
            // Simulate network delay
            yield return new WaitForSeconds(0.5f);
            
            // Return dummy responses based on endpoint
            if (url.Contains("/auth/v1/token") || url.Contains("/auth/v1/signup"))
            {
                // Auth response
                string authResponse = "{\"access_token\":\"dummy_token_123\",\"user\":{\"id\":\"user_123\",\"email\":\"test@example.com\"}}";
                callback?.Invoke(authResponse, null);
            }
            else if (url.Contains("/rest/v1/rpc/get_user_collection"))
            {
                // Collection response
                string collectionResponse = "[]";
                callback?.Invoke(collectionResponse, null);
            }
            else if (url.Contains("/rest/v1/card_complete"))
            {
                // Cards response
                string cardsResponse = "[]";
                callback?.Invoke(cardsResponse, null);
            }
            else
            {
                // Default success response
                callback?.Invoke("{\"success\": true}", null);
            }
        }

        public void MakeRequestAsync(string url, string method, string body, Dictionary<string, string> headers, System.Action<string, string> callback)
        {
            StartCoroutine(MakeRequest(url, method, body, headers, callback));
        }
    }
}