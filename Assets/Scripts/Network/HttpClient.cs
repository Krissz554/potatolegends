using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                // Set headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }

                // Set body for POST/PUT requests
                if (!string.IsNullOrEmpty(body) && (method == "POST" || method == "PUT"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }

                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(request.downloadHandler.text, null);
                }
                else
                {
                    callback?.Invoke(null, request.error);
                }
            }
        }

        public void MakeRequestAsync(string url, string method, string body, Dictionary<string, string> headers, System.Action<string, string> callback)
        {
            StartCoroutine(MakeRequest(url, method, body, headers, callback));
        }
    }
}