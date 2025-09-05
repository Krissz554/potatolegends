using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// FULLY EDITABLE Battle Arena Screen
    /// Customize battle UI completely in Inspector!
    /// </summary>
    public class EditableBattleArena : MonoBehaviour
    {
        [Header("⚔️ BATTLE ARENA - EDIT EVERYTHING!")]
        [Space(10)]
        
        [Header("🎨 Background & Layout")]
        [Tooltip("Custom battle background")]
        public Sprite battleBackground;
        
        [Tooltip("Background color tint")]
        public Color backgroundTint = Color.white;
        
        [Space(10)]
        [Header("📱 Layout Areas - Drag to Reposition")]
        [Tooltip("Opponent area (top)")]
        public RectTransform opponentArea;
        
        [Tooltip("Battle field area (middle)")]
        public RectTransform battleFieldArea;
        
        [Tooltip("Player area (bottom)")]
        public RectTransform playerArea;
        
        [Tooltip("Player hand area")]
        public RectTransform playerHandArea;
        
        [Tooltip("Battle UI area (mana, turn, etc)")]
        public RectTransform battleUIArea;
        
        [Space(10)]
        [Header("🎨 Visual Styling")]
        [Tooltip("Player health bar color")]
        public Color playerHealthColor = Color.green;
        
        [Tooltip("Opponent health bar color")]
        public Color opponentHealthColor = Color.red;
        
        [Tooltip("Mana crystal color")]
        public Color manaColor = Color.blue;
        
        [Tooltip("Turn indicator color")]
        public Color turnIndicatorColor = new Color(1f, 0.9f, 0.3f, 1f);
        
        [Tooltip("Card in hand color")]
        public Color handCardColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        
        [Space(10)]
        [Header("🃏 Battle Card Settings")]
        [Range(60, 120)]
        [Tooltip("Hand card width")]
        public float handCardWidth = 80f;
        
        [Range(80, 160)]
        [Tooltip("Hand card height")]
        public float handCardHeight = 110f;
        
        [Range(100, 200)]
        [Tooltip("Field card width")]
        public float fieldCardWidth = 120f;
        
        [Range(140, 240)]
        [Tooltip("Field card height")]
        public float fieldCardHeight = 160f;
        
        [Space(10)]
        [Header("⚡ Battle UI Settings")]
        [Range(12, 32)]
        [Tooltip("Battle info font size")]
        public int battleInfoFontSize = 16;
        
        [Tooltip("Show turn timer")]
        public bool showTurnTimer = true;
        
        [Tooltip("Show mana crystals")]
        public bool showManaCrystals = true;
        
        [Tooltip("Show battle log")]
        public bool showBattleLog = true;
        
        [Space(10)]
        [Header("🔄 Quick Actions")]
        [Tooltip("Refresh battle arena")]
        public bool refreshScreen = false;
        
        [Tooltip("Reset to defaults")]
        public bool resetToDefaults = false;
        
        [Tooltip("Start mock battle")]
        public bool startMockBattle = false;
        
        // Runtime data
        private bool isInitialized = false;
        private bool isBattleActive = false;
        
        void Start()
        {
            InitializeBattleArena();
        }
        
        void OnValidate()
        {
            if (Application.isPlaying && isInitialized)
            {
                if (refreshScreen)
                {
                    refreshScreen = false;
                    RefreshScreen();
                }
                
                if (resetToDefaults)
                {
                    resetToDefaults = false;
                    ResetToDefaults();
                }
                
                if (startMockBattle)
                {
                    startMockBattle = false;
                    StartMockBattle();
                }
                
                ApplyVisualSettings();
            }
        }
        
        private void InitializeBattleArena()
        {
            Debug.Log("⚔️ Initializing EDITABLE Battle Arena...");
            
            // Create layout
            CreateEditableLayout();
            
            // Create content
            CreateBattleContent();
            
            // Apply settings
            ApplyVisualSettings();
            
            isInitialized = true;
            Debug.Log("✅ Editable Battle Arena ready!");
        }
        
        private void CreateEditableLayout()
        {
            // Background
            CreateEditableBackground();
            
            // Create areas
            if (opponentArea == null) opponentArea = CreateEditableArea("Opponent Area", new Vector2(0f, 0.75f), new Vector2(1f, 0.95f));
            if (battleFieldArea == null) battleFieldArea = CreateEditableArea("Battle Field Area", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.73f));
            if (playerArea == null) playerArea = CreateEditableArea("Player Area", new Vector2(0f, 0.15f), new Vector2(1f, 0.33f));
            if (playerHandArea == null) playerHandArea = CreateEditableArea("Player Hand Area", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.13f));
            if (battleUIArea == null) battleUIArea = CreateEditableArea("Battle UI Area", new Vector2(0f, 0.95f), new Vector2(1f, 1f));
        }
        
        private void CreateEditableBackground()
        {
            GameObject bgObj = new GameObject("🎨 Battle Background");
            bgObj.transform.SetParent(transform, false);
            bgObj.layer = 5;
            
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImg = bgObj.AddComponent<Image>();
            if (battleBackground != null)
            {
                bgImg.sprite = battleBackground;
                bgImg.color = backgroundTint;
            }
            else
            {
                bgImg.color = new Color(0.15f, 0.1f, 0.05f, 1f); // Brown battlefield theme
            }
        }
        
        private RectTransform CreateEditableArea(string areaName, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject areaObj = new GameObject(areaName);
            areaObj.transform.SetParent(transform, false);
            areaObj.layer = 5;
            
            RectTransform areaRect = areaObj.AddComponent<RectTransform>();
            areaRect.anchorMin = anchorMin;
            areaRect.anchorMax = anchorMax;
            areaRect.offsetMin = Vector2.zero;
            areaRect.offsetMax = Vector2.zero;
            
            Image areaImg = areaObj.AddComponent<Image>();
            areaImg.color = new Color(1f, 0f, 0f, 0.05f); // Very transparent red
            
            return areaRect;
        }
        
        private void CreateBattleContent()
        {
            // Battle UI (mana, turn, etc)
            if (battleUIArea != null)
            {
                CreateBattleUI();
            }
            
            // Opponent area
            if (opponentArea != null)
            {
                CreateOpponentDisplay();
            }
            
            // Player area
            if (playerArea != null)
            {
                CreatePlayerDisplay();
            }
            
            // Player hand
            if (playerHandArea != null)
            {
                CreatePlayerHand();
            }
            
            // Battle field
            if (battleFieldArea != null)
            {
                CreateBattleField();
            }
        }
        
        private void CreateBattleUI()
        {
            // Back/Surrender button
            CreateButton("🏃 SURRENDER", battleUIArea, new Vector2(0.02f, 0.1f), new Vector2(0.15f, 0.9f), new Color(0.8f, 0.2f, 0.2f, 1f), () => {
                EditableUIManager uiManager = FindObjectOfType<EditableUIManager>();
                uiManager?.GoToMainMenu();
            });
            
            // Turn indicator
            CreateText("Turn Indicator", "YOUR TURN", battleUIArea, new Vector2(0.35f, 0f), new Vector2(0.65f, 1f), battleInfoFontSize, turnIndicatorColor);
            
            // Mana display
            if (showManaCrystals)
            {
                CreateText("Mana Display", "⚡ 5/10", battleUIArea, new Vector2(0.7f, 0f), new Vector2(0.9f, 1f), battleInfoFontSize, manaColor);
            }
        }
        
        private void CreateOpponentDisplay()
        {
            // Opponent name
            CreateText("Opponent Name", "Opponent Player", opponentArea, new Vector2(0.05f, 0.5f), new Vector2(0.4f, 1f), 14, Color.white);
            
            // Opponent health
            CreateHealthBar("Opponent Health", 25, 30, opponentArea, new Vector2(0.45f, 0.6f), new Vector2(0.85f, 0.9f), opponentHealthColor);
            
            // Opponent cards in hand count
            CreateText("Opponent Hand Count", "🃏 7", opponentArea, new Vector2(0.9f, 0.5f), new Vector2(1f, 1f), 12, Color.white);
        }
        
        private void CreatePlayerDisplay()
        {
            // Player name
            CreateText("Player Name", "You", playerArea, new Vector2(0.05f, 0f), new Vector2(0.4f, 0.5f), 14, Color.white);
            
            // Player health
            CreateHealthBar("Player Health", 28, 30, playerArea, new Vector2(0.45f, 0.1f), new Vector2(0.85f, 0.4f), playerHealthColor);
            
            // Hero power button
            CreateButton("🔮 HERO POWER", playerArea, new Vector2(0.05f, 0.6f), new Vector2(0.3f, 0.95f), new Color(0.6f, 0.3f, 1f, 1f), UseHeroPower);
            
            // End turn button
            CreateButton("END TURN", playerArea, new Vector2(0.7f, 0.6f), new Vector2(0.95f, 0.95f), new Color(0.3f, 0.7f, 0.3f, 1f), EndTurn);
        }
        
        private void CreatePlayerHand()
        {
            // Mock hand cards
            for (int i = 0; i < 5; i++)
            {
                float cardX = 0.05f + (i * 0.18f);
                CreateHandCard($"Hand Card {i+1}", playerHandArea, new Vector2(cardX, 0.1f), new Vector2(cardX + 0.15f, 0.9f));
            }
        }
        
        private void CreateBattleField()
        {
            // Opponent field (top half)
            CreateText("Opponent Field Label", "OPPONENT FIELD", battleFieldArea, new Vector2(0.1f, 0.8f), new Vector2(0.9f, 0.95f), 12, Color.white);
            
            // Player field (bottom half)
            CreateText("Player Field Label", "YOUR FIELD", battleFieldArea, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.2f), 12, Color.white);
            
            // Field slots (placeholder)
            for (int i = 0; i < 3; i++)
            {
                float slotX = 0.2f + (i * 0.2f);
                CreateFieldSlot($"Opponent Slot {i+1}", battleFieldArea, new Vector2(slotX, 0.55f), new Vector2(slotX + 0.15f, 0.78f), true);
                CreateFieldSlot($"Player Slot {i+1}", battleFieldArea, new Vector2(slotX, 0.25f), new Vector2(slotX + 0.15f, 0.48f), false);
            }
        }
        
        private void CreateHandCard(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject cardObj = new GameObject(name);
            cardObj.transform.SetParent(parent, false);
            cardObj.layer = 5;
            
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = handCardColor;
            
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.onClick.AddListener(() => {
                Debug.Log($"🃏 Played card: {name}");
            });
            
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchorMin = anchorMin;
            cardRect.anchorMax = anchorMax;
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            
            // Card cost
            CreateText("Card Cost", "3", cardObj.transform, new Vector2(0.05f, 0.8f), new Vector2(0.3f, 0.95f), 10, manaColor);
            
            // Card name
            CreateText("Card Name", name.Replace("Hand Card ", "Card "), cardObj.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.25f), 8, Color.white);
        }
        
        private void CreateFieldSlot(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, bool isOpponent)
        {
            GameObject slotObj = new GameObject(name);
            slotObj.transform.SetParent(parent, false);
            slotObj.layer = 5;
            
            Image slotImg = slotObj.AddComponent<Image>();
            slotImg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Empty slot
            
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.anchorMin = anchorMin;
            slotRect.anchorMax = anchorMax;
            slotRect.offsetMin = Vector2.zero;
            slotRect.offsetMax = Vector2.zero;
            
            // Slot indicator
            CreateText("Slot Text", "+", slotObj.transform, Vector2.zero, Vector2.one, 20, new Color(0.6f, 0.6f, 0.6f, 0.8f));
        }
        
        private void CreateHealthBar(string name, int currentHealth, int maxHealth, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject healthBarObj = new GameObject(name);
            healthBarObj.transform.SetParent(parent, false);
            healthBarObj.layer = 5;
            
            // Background
            Image healthBg = healthBarObj.AddComponent<Image>();
            healthBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            RectTransform healthRect = healthBarObj.GetComponent<RectTransform>();
            healthRect.anchorMin = anchorMin;
            healthRect.anchorMax = anchorMax;
            healthRect.offsetMin = Vector2.zero;
            healthRect.offsetMax = Vector2.zero;
            
            // Health fill
            GameObject healthFill = new GameObject("Health Fill");
            healthFill.transform.SetParent(healthBarObj.transform, false);
            healthFill.layer = 5;
            
            Image fillImg = healthFill.AddComponent<Image>();
            fillImg.color = color;
            
            RectTransform fillRect = healthFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2((float)currentHealth / maxHealth, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            // Health text
            CreateText("Health Text", $"{currentHealth}/{maxHealth}", healthBarObj.transform, Vector2.zero, Vector2.one, 12, Color.white);
        }
        
        private void CreateButton(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, System.Action onClick)
        {
            GameObject btnObj = new GameObject($"Button: {text}");
            btnObj.transform.SetParent(parent, false);
            btnObj.layer = 5;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;
            
            Button button = btnObj.AddComponent<Button>();
            if (onClick != null) button.onClick.AddListener(() => onClick());
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            CreateText("Button Text", text, btnObj.transform, Vector2.zero, Vector2.one, 12, Color.white);
        }
        
        private void CreateText(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            textObj.layer = 5;
            
            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        private void UseHeroPower()
        {
            Debug.Log("🔮 Hero power used!");
            // TODO: Implement hero power logic
        }
        
        private void EndTurn()
        {
            Debug.Log("🔄 Turn ended!");
            // TODO: Implement turn logic
        }
        
        private void StartMockBattle()
        {
            Debug.Log("⚔️ Starting mock battle...");
            isBattleActive = true;
            // TODO: Implement battle logic
        }
        
        public void RefreshScreen()
        {
            Debug.Log("🔄 Refreshing battle arena...");
            ApplyVisualSettings();
        }
        
        public void ResetToDefaults()
        {
            Debug.Log("🔄 Resetting battle arena to defaults...");
            
            handCardWidth = 80f;
            handCardHeight = 110f;
            fieldCardWidth = 120f;
            fieldCardHeight = 160f;
            battleInfoFontSize = 16;
            
            ApplyVisualSettings();
        }
        
        private void ApplyVisualSettings()
        {
            // Update health bar colors
            Image[] images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name == "Health Fill")
                {
                    if (img.transform.parent.name.Contains("Player"))
                        img.color = playerHealthColor;
                    else if (img.transform.parent.name.Contains("Opponent"))
                        img.color = opponentHealthColor;
                }
                else if (img.name.Contains("Hand Card"))
                {
                    img.color = handCardColor;
                }
            }
            
            // Update text colors
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.name == "Turn Indicator")
                {
                    text.color = turnIndicatorColor;
                    text.fontSize = battleInfoFontSize;
                }
                else if (text.name == "Mana Display")
                {
                    text.color = manaColor;
                    text.fontSize = battleInfoFontSize;
                }
            }
        }
    }
}