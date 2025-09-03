using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using PotatoCardGame.Data;

namespace PotatoCardGame.Cards
{
    /// <summary>
    /// Handles the visual display and interaction of cards in the game
    /// Supports both hand cards and battlefield cards with touch/mouse interaction
    /// </summary>
    public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
                              IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image cardArt;
        [SerializeField] private Image cardFrame;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI manaCostText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI abilityText;
        [SerializeField] private Image rarityGem;
        [SerializeField] private Image elementIcon;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject glowEffect;
        [SerializeField] private GameObject selectionHighlight;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float hoverDuration = 0.2f;
        [SerializeField] private float dragAlpha = 0.8f;
        
        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color legendaryColor = Color.magenta;
        [SerializeField] private Color exoticColor = Color.red;
        
        // Card data and state
        private CardData cardData;
        private bool isInteractable = true;
        private bool isDragging = false;
        private bool isInHand = true;
        private Vector3 originalPosition;
        private Transform originalParent;
        private int siblingIndex;
        
        // Events
        public System.Action<CardDisplay> OnCardClicked;
        public System.Action<CardDisplay> OnCardHover;
        public System.Action<CardDisplay> OnCardDragStart;
        public System.Action<CardDisplay, Vector3> OnCardDragEnd;
        public System.Action<CardDisplay> OnCardPlayed;
        
        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
                
            originalPosition = transform.position;
            originalParent = transform.parent;
        }
        
        public void Initialize(CardData data, bool inHand = true)
        {
            cardData = data;
            isInHand = inHand;
            UpdateCardDisplay();
            
            Debug.Log($"🃏 Card initialized: {cardData.cardName}");
        }
        
        private void UpdateCardDisplay()
        {
            if (cardData == null) return;
            
            // Basic info
            if (cardNameText) cardNameText.text = cardData.cardName;
            if (manaCostText) manaCostText.text = cardData.manaCost.ToString();
            if (attackText) attackText.text = cardData.currentAttack.ToString();
            if (healthText) healthText.text = cardData.currentHealth.ToString();
            if (abilityText) abilityText.text = cardData.abilityText;
            
            // Art and frame
            if (cardArt && cardData.cardArt) 
                cardArt.sprite = cardData.cardArt;
            if (cardFrame && cardData.cardFrame) 
                cardFrame.sprite = cardData.cardFrame;
            
            // Rarity coloring
            UpdateRarityDisplay();
            
            // Element icon
            UpdateElementDisplay();
            
            // Update health color if damaged
            UpdateHealthDisplay();
        }
        
        private void UpdateRarityDisplay()
        {
            Color rarityColor = GetRarityColor(cardData.rarity);
            
            if (rarityGem) rarityGem.color = rarityColor;
            if (cardFrame) cardFrame.color = Color.Lerp(Color.white, rarityColor, 0.3f);
        }
        
        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => commonColor,
                Rarity.Uncommon => uncommonColor,
                Rarity.Rare => rareColor,
                Rarity.Legendary => legendaryColor,
                Rarity.Exotic => exoticColor,
                _ => commonColor
            };
        }
        
        private void UpdateElementDisplay()
        {
            // TODO: Set element icon based on cardData.elementType
            // This will need element icons imported from your web game
        }
        
        private void UpdateHealthDisplay()
        {
            if (healthText && cardData.currentHealth < cardData.health)
            {
                healthText.color = Color.red; // Damaged
            }
            else if (healthText)
            {
                healthText.color = Color.white; // Full health
            }
        }
        
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            canvasGroup.interactable = interactable;
            canvasGroup.alpha = interactable ? 1f : 0.5f;
        }
        
        public void SetGlowEffect(bool enabled)
        {
            if (glowEffect) glowEffect.SetActive(enabled);
        }
        
        public void SetSelected(bool selected)
        {
            if (selectionHighlight) selectionHighlight.SetActive(selected);
        }
        
        #region Pointer Events
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable || isDragging) return;
            
            // Scale up animation
            transform.DOScale(Vector3.one * hoverScale, hoverDuration)
                     .SetEase(Ease.OutBack);
            
            // Bring to front
            transform.SetAsLastSibling();
            
            OnCardHover?.Invoke(this);
            
            Debug.Log($"🖱️ Hovering over card: {cardData?.cardName}");
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable || isDragging) return;
            
            // Scale back to normal
            transform.DOScale(Vector3.one, hoverDuration)
                     .SetEase(Ease.OutQuad);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable || isDragging) return;
            
            OnCardClicked?.Invoke(this);
            
            Debug.Log($"👆 Card clicked: {cardData?.cardName}");
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isInteractable || !isInHand) return;
            
            isDragging = true;
            originalPosition = transform.position;
            siblingIndex = transform.GetSiblingIndex();
            
            // Visual feedback
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
            
            // Scale back to normal size
            transform.DOScale(Vector3.one, 0.1f);
            
            OnCardDragStart?.Invoke(this);
            
            Debug.Log($"🖐️ Started dragging card: {cardData?.cardName}");
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            // Follow mouse/touch position
            Vector3 worldPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                transform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out worldPosition
            );
            
            transform.position = worldPosition;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Check if card was dropped on a valid play area
            Vector3 dropPosition = transform.position;
            OnCardDragEnd?.Invoke(this, dropPosition);
            
            Debug.Log($"🎯 Finished dragging card: {cardData?.cardName}");
        }
        
        #endregion
        
        public void ReturnToHand()
        {
            // Animate back to original position
            transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutQuad);
            transform.SetSiblingIndex(siblingIndex);
        }
        
        public void PlayCard()
        {
            // Animation for playing the card
            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                     .OnComplete(() => {
                         OnCardPlayed?.Invoke(this);
                         gameObject.SetActive(false);
                     });
            
            Debug.Log($"✨ Card played: {cardData?.cardName}");
        }
        
        public void DestroyCard()
        {
            // Death animation
            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            canvasGroup.DOFade(0f, 0.5f)
                       .OnComplete(() => Destroy(gameObject));
            
            Debug.Log($"💀 Card destroyed: {cardData?.cardName}");
        }
        
        // Public getters
        public CardData GetCardData() => cardData;
        public bool IsInHand() => isInHand;
        public bool IsDragging() => isDragging;
        
        private void OnDestroy()
        {
            // Clean up any running tweens
            transform.DOKill();
            if (canvasGroup) canvasGroup.DOKill();
        }
    }
}