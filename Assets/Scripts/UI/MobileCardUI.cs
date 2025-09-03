using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using PotatoCardGame.Data;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Mobile-optimized card UI component
    /// Displays card information with touch-friendly interface
    /// </summary>
    public class MobileCardUI : MonoBehaviour
    {
        [Header("Card UI Elements")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image cardFrame;
        [SerializeField] private Image cardArt;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI manaCostText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI abilityText;
        [SerializeField] private Image rarityGem;
        [SerializeField] private Image elementIcon;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject selectionGlow;
        [SerializeField] private GameObject hoverEffect;
        [SerializeField] private CanvasGroup cardCanvasGroup;
        
        [Header("Mobile Optimization")]
        [SerializeField] private float touchAreaPadding = 20f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private float selectedScale = 1.1f;
        
        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color legendaryColor = Color.magenta;
        [SerializeField] private Color exoticColor = Color.red;
        
        // Card data and state
        private GameCard cardData;
        private bool isSelected = false;
        private bool isInteractable = true;
        private Vector3 originalScale;
        
        // Events for mobile interaction
        public System.Action<MobileCardUI> OnCardTapped;
        public System.Action<MobileCardUI> OnCardLongPressed;
        public System.Action<MobileCardUI> OnCardDoubleTapped;
        
        // Touch handling
        private float lastTapTime = 0f;
        private float doubleTapThreshold = 0.3f;
        private float longPressThreshold = 0.8f;
        private bool isLongPressing = false;
        private float touchStartTime = 0f;
        
        private void Awake()
        {
            originalScale = transform.localScale;
            
            if (cardCanvasGroup == null)
                cardCanvasGroup = GetComponent<CanvasGroup>();
                
            // Ensure we have a button component for mobile touch detection
            if (GetComponent<Button>() == null)
            {
                var button = gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None; // We handle our own transitions
            }
        }
        
        private void Start()
        {
            SetupTouchHandling();
        }
        
        private void SetupTouchHandling()
        {
            var button = GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(OnCardTouched);
            }
        }
        
        #region Card Data Management
        
        public void SetCardData(GameCard card)
        {
            cardData = card;
            UpdateCardDisplay();
        }
        
        private void UpdateCardDisplay()
        {
            if (cardData == null) return;
            
            // Basic card information
            if (cardNameText) cardNameText.text = cardData.name;
            if (manaCostText) manaCostText.text = cardData.mana_cost.ToString();
            if (attackText) attackText.text = cardData.attack.ToString();
            if (healthText) healthText.text = cardData.hp.ToString();
            if (abilityText) abilityText.text = cardData.ability_text;
            
            // Rarity styling
            UpdateRarityDisplay();
            
            // Card art (placeholder for now - will load from Supabase)
            UpdateCardArt();
            
            Debug.Log($"🃏 Updated card display: {cardData.name}");
        }
        
        private void UpdateRarityDisplay()
        {
            Color rarityColor = GetRarityColor(cardData.rarity);
            
            if (rarityGem) rarityGem.color = rarityColor;
            if (cardFrame) cardFrame.color = Color.Lerp(Color.white, rarityColor, 0.3f);
            
            // Update text colors for better mobile readability
            if (cardNameText) cardNameText.color = rarityColor;
        }
        
        private Color GetRarityColor(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => commonColor,
                "uncommon" => uncommonColor,
                "rare" => rareColor,
                "legendary" => legendaryColor,
                "exotic" => exoticColor,
                _ => commonColor
            };
        }
        
        private void UpdateCardArt()
        {
            // TODO: Load card art from URL or generated pixel art
            // For now, use a placeholder or generate based on card data
            if (cardArt)
            {
                // Placeholder: Set background color based on potato type/element
                cardArt.color = GetElementColor(cardData.potato_type);
            }
        }
        
        private Color GetElementColor(string potatoType)
        {
            return potatoType?.ToLower() switch
            {
                "fire" => new Color(1f, 0.3f, 0.3f),
                "water" => new Color(0.3f, 0.3f, 1f),
                "earth" => new Color(0.6f, 0.4f, 0.2f),
                "air" => new Color(0.8f, 0.8f, 0.9f),
                "light" => new Color(1f, 1f, 0.7f),
                "dark" => new Color(0.3f, 0.2f, 0.4f),
                "lightning" => new Color(1f, 1f, 0.3f),
                "void" => new Color(0.2f, 0.1f, 0.3f),
                _ => new Color(0.8f, 0.7f, 0.5f) // Default potato color
            };
        }
        
        #endregion
        
        #region Mobile Touch Handling
        
        private void OnCardTouched()
        {
            if (!isInteractable) return;
            
            float currentTime = Time.time;
            
            // Check for double tap
            if (currentTime - lastTapTime < doubleTapThreshold)
            {
                OnCardDoubleTapped?.Invoke(this);
                Debug.Log($"👆👆 Double tapped: {cardData?.name}");
                return;
            }
            
            lastTapTime = currentTime;
            touchStartTime = currentTime;
            isLongPressing = true;
            
            // Start checking for long press
            DOVirtual.DelayedCall(longPressThreshold, () => {
                if (isLongPressing && currentTime == touchStartTime)
                {
                    OnCardLongPressed?.Invoke(this);
                    isLongPressing = false;
                    Debug.Log($"👆⏰ Long pressed: {cardData?.name}");
                }
            });
            
            // Regular tap after delay (if no double tap occurs)
            DOVirtual.DelayedCall(doubleTapThreshold, () => {
                if (lastTapTime == currentTime && isLongPressing)
                {
                    isLongPressing = false;
                    OnCardTapped?.Invoke(this);
                    Debug.Log($"👆 Tapped: {cardData?.name}");
                }
            });
        }
        
        #endregion
        
        #region Visual States
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (selectionGlow) selectionGlow.SetActive(selected);
            
            // Scale animation for selection
            Vector3 targetScale = selected ? originalScale * selectedScale : originalScale;
            transform.DOScale(targetScale, animationDuration).SetEase(Ease.OutBack);
            
            Debug.Log($"🎯 Card {cardData?.name} selected: {selected}");
        }
        
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (cardCanvasGroup)
            {
                cardCanvasGroup.alpha = interactable ? 1f : 0.5f;
                cardCanvasGroup.interactable = interactable;
            }
            
            var button = GetComponent<Button>();
            if (button) button.interactable = interactable;
        }
        
        public void ShowHoverEffect(bool show)
        {
            if (hoverEffect) hoverEffect.SetActive(show);
        }
        
        public void PlayCardAnimation()
        {
            // Animation for playing the card
            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            cardCanvasGroup.DOFade(0f, 0.5f);
            
            Debug.Log($"✨ Playing card: {cardData?.name}");
        }
        
        public void DestroyCardAnimation()
        {
            // Animation for card destruction
            transform.DOShakeScale(0.3f, 0.5f);
            cardCanvasGroup.DOFade(0f, 0.5f)
                .OnComplete(() => gameObject.SetActive(false));
            
            Debug.Log($"💀 Destroying card: {cardData?.name}");
        }
        
        #endregion
        
        #region Public Getters
        
        public GameCard GetCardData() => cardData;
        public bool IsSelected() => isSelected;
        public bool IsInteractable() => isInteractable;
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up tweens
            transform.DOKill();
            if (cardCanvasGroup) cardCanvasGroup.DOKill();
        }
    }
}