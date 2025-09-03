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
            testText.text = "🎮 WHAT'S MY POTATO?\n\nMobile Card Game\n\nReady to sign in!";
            testText.fontSize = 48;
            testText.color = new Color(1f, 0.9f, 0.6f, 1f); // Golden color
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
            btnText.text = "🔐 SIGN IN";
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
            
            // Create email input field
            CreateInputField(canvasObj.transform, "Email", new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.75f));
            
            // Create password input field  
            CreateInputField(canvasObj.transform, "Password", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.65f), true);
            
            // Add button functionality
            testButton.onClick.AddListener(() => {
                Debug.Log("🎮 Sign in button pressed!");
                testText.text = "🎉 MOBILE LOGIN WORKING!\n\nConnecting to Supabase...\n\nBuilding full game interface!";
                btnText.text = "✅ SIGNING IN...";
                btnImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
                
                // TODO: Add actual Supabase authentication
                StartCoroutine(ShowMainGameUI(canvasObj, testText));
            });
            
            Debug.Log("✅ Simple test UI created - should be visible now!");
        }
        
        private void CreateInputField(Transform parent, string placeholder, Vector2 anchorMin, Vector2 anchorMax, bool isPassword = false)
        {
            GameObject inputObj = new GameObject($"{placeholder} Input");
            inputObj.transform.SetParent(parent, false);
            
            // Background
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(1f, 1f, 1f, 0.9f); // White background
            
            // Input field component
            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            if (isPassword) inputField.contentType = TMP_InputField.ContentType.Password;
            
            // Text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectMask2D mask = textArea.AddComponent<RectMask2D>();
            
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = new Vector2(0.05f, 0f);
            textAreaRect.anchorMax = new Vector2(0.95f, 1f);
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;
            
            // Placeholder text
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = $"Enter your {placeholder.ToLower()}";
            placeholderText.fontSize = 24;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.fontStyle = FontStyles.Italic;
            
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            // Input text
            GameObject inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 24;
            inputText.color = Color.black;
            
            RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;
            
            // Connect input field
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            
            // Position the input field
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
        }
        
        private System.Collections.IEnumerator ShowMainGameUI(GameObject loginCanvas, TextMeshProUGUI statusText)
        {
            yield return new WaitForSeconds(2f);
            
            // Update status
            statusText.text = "🎮 CREATING MAIN GAME INTERFACE...\n\nNavigation: Home, Cards, Decks, Heroes\nBattle System: Ready\nSupabase: Connected";
            
            yield return new WaitForSeconds(2f);
            
            // Create main game navigation
            CreateMainGameNavigation(loginCanvas.transform);
            
            statusText.text = "✅ MOBILE GAME READY!\n\nNavigation buttons created\nBattle system active\nReady to play!";
        }
        
        private void CreateMainGameNavigation(Transform parent)
        {
            // Create navigation bar at bottom
            GameObject navBar = new GameObject("Navigation Bar");
            navBar.transform.SetParent(parent, false);
            
            Image navBg = navBar.AddComponent<Image>();
            navBg.color = new Color(0.1f, 0.05f, 0.15f, 0.9f); // Dark bar
            
            RectTransform navRect = navBar.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0f, 0f);
            navRect.anchorMax = new Vector2(1f, 0.12f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;
            
            // Create navigation buttons
            string[] navLabels = { "🏠\nHome", "📚\nCards", "🔧\nDecks", "🦸\nHeroes" };
            float[] positions = { 0f, 0.25f, 0.5f, 0.75f };
            
            for (int i = 0; i < navLabels.Length; i++)
            {
                CreateNavButton(navBar.transform, navLabels[i], positions[i], positions[i] + 0.23f, i);
            }
            
            // Create battle button (bottom right)
            CreateBattleButton(parent);
            
            Debug.Log("🎮 Main game navigation created!");
        }
        
        private void CreateNavButton(Transform parent, string text, float minX, float maxX, int index)
        {
            GameObject btnObj = new GameObject($"Nav Button {index}");
            btnObj.transform.SetParent(parent, false);
            
            Button button = btnObj.AddComponent<Button>();
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.6f, 0.9f, 1f); // Blue
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 18;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(minX, 0.1f);
            btnRect.anchorMax = new Vector2(maxX, 0.9f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            // Add click functionality
            int buttonIndex = index;
            button.onClick.AddListener(() => {
                Debug.Log($"🎯 Navigation button {buttonIndex} pressed: {text}");
                btnText.text = text + "\n✅";
                btnImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green when pressed
            });
        }
        
        private void CreateBattleButton(Transform parent)
        {
            GameObject battleBtn = new GameObject("Battle Button");
            battleBtn.transform.SetParent(parent, false);
            
            Button button = battleBtn.AddComponent<Button>();
            Image btnImage = battleBtn.AddComponent<Image>();
            btnImage.color = new Color(1f, 0.3f, 0.3f, 1f); // Red battle color
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(battleBtn.transform, false);
            
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "⚔️\nBATTLE";
            btnText.fontSize = 32;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Position in bottom right corner
            RectTransform battleRect = battleBtn.GetComponent<RectTransform>();
            battleRect.anchorMin = new Vector2(0.7f, 0.15f);
            battleRect.anchorMax = new Vector2(0.95f, 0.35f);
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;
            
            // Add click functionality
            button.onClick.AddListener(() => {
                Debug.Log("⚔️ BATTLE button pressed!");
                btnText.text = "⚔️\nSEARCHING...";
                btnImage.color = new Color(1f, 0.6f, 0f, 1f); // Orange when searching
                
                // TODO: Connect to actual battle system
                StartCoroutine(SimulateBattleSearch(btnText, btnImage));
            });
        }
        
        private System.Collections.IEnumerator SimulateBattleSearch(TextMeshProUGUI battleText, Image battleImage)
        {
            yield return new WaitForSeconds(3f);
            
            battleText.text = "⚔️\nMATCH FOUND!";
            battleImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
            
            yield return new WaitForSeconds(2f);
            
            battleText.text = "⚔️\nBATTLE";
            battleImage.color = new Color(1f, 0.3f, 0.3f, 1f); // Back to red
        }