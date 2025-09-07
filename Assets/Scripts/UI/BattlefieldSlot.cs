using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace PotatoLegends.UI
{
    public class BattlefieldSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
    {
        [Header("UI Components")]
        public Image slotImage;
        public Image cardImage;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI healthText;
        public GameObject emptySlotIndicator;
        public GameObject hoverIndicator;
        
        [Header("Visual States")]
        public Color emptyColor = Color.gray;
        public Color filledColor = Color.white;
        public Color hoverColor = Color.yellow;
        public Color attackableColor = Color.red;
        public Color targetableColor = Color.blue;
        
        [Header("Events")]
        public System.Action<BattlefieldSlot> OnSlotClicked;
        public System.Action<BattlefieldSlot> OnSlotHovered;
        public System.Action<BattlefieldSlot> OnSlotUnhovered;
        public System.Action<BattlefieldSlot, CardData> OnCardDropped;
        
        public int slotIndex;
        public bool isPlayerSlot = true;
        private CardData currentCard;
        private bool isEmpty = true;
        private bool isHovered = false;
        private bool isAttackable = false;
        private bool isTargetable = false;
        private bool isDragging = false;
        
        private void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            SetEmpty();
        }
        
        public void SetCard(CardData card)
        {
            if (card == null)
            {
                SetEmpty();
                return;
            }
            
            currentCard = card;
            isEmpty = false;
            
            UpdateCardDisplay();
            
            if (emptySlotIndicator != null)
                emptySlotIndicator.SetActive(false);
        }
        
        public void SetEmpty()
        {
            currentCard = null;
            isEmpty = true;
            
            if (cardImage != null)
                cardImage.sprite = null;
            
            if (cardNameText != null)
                cardNameText.text = "";
            
            if (attackText != null)
                attackText.text = "";
            
            if (healthText != null)
                healthText.text = "";
            
            if (emptySlotIndicator != null)
                emptySlotIndicator.SetActive(true);
        }
        
        private void UpdateCardDisplay()
        {
            if (currentCard == null) return;
            
            if (cardImage != null && currentCard.illustration != null)
            {
                cardImage.sprite = currentCard.illustration;
            }
            
            if (cardNameText != null)
            {
                cardNameText.text = currentCard.cardName;
            }
            
            if (currentCard.cardType == CardData.CardType.Unit || currentCard.cardType == CardData.CardType.Structure)
            {
                if (attackText != null)
                {
                    attackText.text = currentCard.attack.ToString();
                    attackText.gameObject.SetActive(true);
                }
                
                if (healthText != null)
                {
                    healthText.text = currentCard.health.ToString();
                    healthText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (attackText != null)
                    attackText.gameObject.SetActive(false);
                
                if (healthText != null)
                    healthText.gameObject.SetActive(false);
            }
        }
        
        public void SetAttackable(bool attackable)
        {
            isAttackable = attackable;
            UpdateVisualState();
        }
        
        public void SetTargetable(bool targetable)
        {
            isTargetable = targetable;
            UpdateVisualState();
        }
        
        public void SetDragging(bool dragging)
        {
            isDragging = dragging;
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (isEmpty)
            {
                if (slotImage != null)
                {
                    if (isDragging)
                    {
                        slotImage.color = hoverColor;
                    }
                    else
                    {
                        slotImage.color = emptyColor;
                    }
                }
            }
            else
            {
                if (slotImage != null)
                {
                    if (isAttackable)
                    {
                        slotImage.color = attackableColor;
                    }
                    else if (isTargetable)
                    {
                        slotImage.color = targetableColor;
                    }
                    else if (isHovered)
                    {
                        slotImage.color = hoverColor;
                    }
                    else
                    {
                        slotImage.color = filledColor;
                    }
                }
            }
            
            if (hoverIndicator != null)
                hoverIndicator.SetActive(isHovered);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"BattlefieldSlot: Slot {slotIndex} clicked");
            OnSlotClicked?.Invoke(this);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            OnSlotHovered?.Invoke(this);
            UpdateVisualState();
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            OnSlotUnhovered?.Invoke(this);
            UpdateVisualState();
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"BattlefieldSlot: Card dropped on slot {slotIndex}");
            
            CardDisplay cardDisplay = eventData.pointerDrag?.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                CardData card = cardDisplay.cardData;
                if (card != null)
                {
                    OnCardDropped?.Invoke(this, card);
                }
            }
        }
        
        public int GetSlotIndex() => slotIndex;
        public CardData GetCard() => currentCard;
        public bool IsEmpty() => isEmpty;
        public bool IsHovered() => isHovered;
        public bool IsAttackable() => isAttackable;
        public bool IsTargetable() => isTargetable;
    }
}