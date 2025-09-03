using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Loads images from URLs (like your card illustration_url from database)
    /// Caches images for better performance
    /// </summary>
    public class ImageLoader : MonoBehaviour
    {
        // Singleton
        public static ImageLoader Instance { get; private set; }
        
        // Image cache
        private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();
        private Dictionary<string, Coroutine> loadingCoroutines = new Dictionary<string, Coroutine>();
        
        [Header("Loading Settings")]
        [SerializeField] private Sprite defaultCardSprite;
        [SerializeField] private Sprite loadingSprite;
        [SerializeField] private int maxCacheSize = 100;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Load image from URL and set it to the target Image component
        /// </summary>
        public void LoadImageFromURL(string imageUrl, Image targetImage, bool useCache = true)
        {
            if (string.IsNullOrEmpty(imageUrl) || targetImage == null)
            {
                SetDefaultImage(targetImage);
                return;
            }
            
            // Check cache first
            if (useCache && imageCache.ContainsKey(imageUrl))
            {
                targetImage.sprite = imageCache[imageUrl];
                return;
            }
            
            // Set loading image
            if (loadingSprite != null)
            {
                targetImage.sprite = loadingSprite;
            }
            
            // Cancel any existing loading for this URL
            if (loadingCoroutines.ContainsKey(imageUrl))
            {
                StopCoroutine(loadingCoroutines[imageUrl]);
            }
            
            // Start loading
            Coroutine loadCoroutine = StartCoroutine(LoadImageCoroutine(imageUrl, targetImage, useCache));
            loadingCoroutines[imageUrl] = loadCoroutine;
        }
        
        private IEnumerator LoadImageCoroutine(string imageUrl, Image targetImage, bool useCache)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        
                        if (texture != null)
                        {
                            // Create sprite from texture
                            Sprite sprite = Sprite.Create(
                                texture, 
                                new Rect(0, 0, texture.width, texture.height), 
                                new Vector2(0.5f, 0.5f)
                            );
                            
                            // Cache the sprite
                            if (useCache)
                            {
                                CacheSprite(imageUrl, sprite);
                            }
                            
                            // Set to target image (if it still exists)
                            if (targetImage != null)
                            {
                                targetImage.sprite = sprite;
                            }
                            
                            Debug.Log($"✅ Loaded image: {imageUrl}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"❌ Error creating sprite from {imageUrl}: {e.Message}");
                        SetDefaultImage(targetImage);
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Failed to load image: {imageUrl} - {request.error}");
                    SetDefaultImage(targetImage);
                }
                
                // Remove from loading coroutines
                if (loadingCoroutines.ContainsKey(imageUrl))
                {
                    loadingCoroutines.Remove(imageUrl);
                }
            }
        }
        
        private void CacheSprite(string url, Sprite sprite)
        {
            // Manage cache size
            if (imageCache.Count >= maxCacheSize)
            {
                // Remove oldest entries (simple FIFO)
                var oldestKey = "";
                foreach (var key in imageCache.Keys)
                {
                    oldestKey = key;
                    break;
                }
                
                if (!string.IsNullOrEmpty(oldestKey))
                {
                    if (imageCache[oldestKey] != null)
                    {
                        Destroy(imageCache[oldestKey].texture);
                    }
                    imageCache.Remove(oldestKey);
                }
            }
            
            imageCache[url] = sprite;
        }
        
        private void SetDefaultImage(Image targetImage)
        {
            if (targetImage != null && defaultCardSprite != null)
            {
                targetImage.sprite = defaultCardSprite;
            }
        }
        
        /// <summary>
        /// Preload images for better performance
        /// </summary>
        public void PreloadImages(List<string> imageUrls)
        {
            StartCoroutine(PreloadImagesCoroutine(imageUrls));
        }
        
        private IEnumerator PreloadImagesCoroutine(List<string> imageUrls)
        {
            foreach (string url in imageUrls)
            {
                if (!imageCache.ContainsKey(url))
                {
                    yield return StartCoroutine(LoadImageCoroutine(url, null, true));
                    yield return new WaitForSeconds(0.1f); // Small delay between loads
                }
            }
            
            Debug.Log($"✅ Preloaded {imageUrls.Count} images");
        }
        
        /// <summary>
        /// Clear image cache to free memory
        /// </summary>
        public void ClearCache()
        {
            foreach (var sprite in imageCache.Values)
            {
                if (sprite != null && sprite.texture != null)
                {
                    Destroy(sprite.texture);
                }
            }
            
            imageCache.Clear();
            Debug.Log("🗑️ Image cache cleared");
        }
        
        /// <summary>
        /// Get cache statistics
        /// </summary>
        public void LogCacheStats()
        {
            Debug.Log($"📊 Image Cache: {imageCache.Count}/{maxCacheSize} images cached");
        }
    }
}