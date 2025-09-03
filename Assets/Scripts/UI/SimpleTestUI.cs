using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Simple test UI to verify the mobile interface is working
    /// Creates basic UI elements that should be immediately visible
    /// </summary>
    public class SimpleTestUI : MonoBehaviour
    {
        private void Start()
        {
            CreateBasicUI();
        }
        
        private void CreateBasicUI()
        {
            Debug.Log("🏗️ Creating simple test UI...");
            
            // Create Canvas
            GameObject canvasObj = new GameObject("Test Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create test text
            GameObject textObj = new GameObject("Test Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            
            TextMeshProUGUI testText = textObj.AddComponent<TextMeshProUGUI>();
            testText.text = "🎮 POTATO CARD GAME\n\nMobile UI Loading...\n\nTap anywhere to continue";
            testText.fontSize = 48;
            testText.color = Color.white;
            testText.alignment = TextAlignmentOptions.Center;
            testText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.4f);
            textRect.anchorMax = new Vector2(0.9f, 0.6f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Create background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            bgObj.transform.SetAsFirstSibling(); // Behind text
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.18f, 0.11f, 0.21f, 1f); // Purple background
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // Create test button
            GameObject btnObj = new GameObject("Test Button");
            btnObj.transform.SetParent(canvasObj.transform, false);
            
            Button testButton = btnObj.AddComponent<Button>();
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(1f, 0.5f, 0.2f, 1f); // Orange button
            
            GameObject btnTextObj = new GameObject("Button Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "🔐 TEST LOGIN";
            btnText.fontSize = 36;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.2f, 0.2f);
            btnRect.anchorMax = new Vector2(0.8f, 0.35f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Add button functionality
            testButton.onClick.AddListener(() => {
                Debug.Log("🎮 Test button pressed!");
                testText.text = "🎉 MOBILE UI WORKING!\n\nReady to build full interface!\n\nAuthentication system ready.";
                btnText.text = "✅ SUCCESS!";
                btnImage.color = Color.green;
            });
            
            Debug.Log("✅ Simple test UI created - should be visible now!");
        }
    }
}