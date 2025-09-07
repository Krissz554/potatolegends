using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PotatoLegends.Data;
using PotatoLegends.UI;
using PotatoLegends.Core;
using PotatoLegends.Battle;
using PotatoLegends.Network;

namespace PotatoLegends.Cards
{
    public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("UI Elements")]
        public Image cardArtImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI manaCostText;
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI flavorText;
        public Image rarityBorder;

        [Header("Card Data")]
        public CardData cardData;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector3 originalPosition;
        private Transform originalParent;
        private bool isDragging = false;

        public delegate void CardPlayedHandler(CardData card, int targetSlot = -1);
        public event CardPlayedHandler OnCardPlayed;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void SetupCard(CardData data)
        {
            cardData = data;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (cardData == null) return;

            nameText.text = cardData.cardName;
            manaCostText.text = cardData.manaCost.ToString();
            descriptionText.text = cardData.description;
            flavorText.text = cardData.flavorText;

            if (cardData.cardType == CardData.CardType.Unit || cardData.cardType == CardData.CardType.Structure)
            {
                attackText.gameObject.SetActive(true);
                healthText.gameObject.SetActive(true);
                attackText.text = cardData.attack.ToString();
                healthText.text = cardData.health.ToString();
            }
            else
            {
                attackText.gameObject.SetActive(false);
                healthText.gameObject.SetActive(false);
            }

            switch (cardData.rarity)
            {
                case CardData.Rarity.Common: rarityBorder.color = Color.gray; break;
                case CardData.Rarity.Uncommon: rarityBorder.color = Color.green; break;
                case CardData.Rarity.Rare: rarityBorder.color = Color.blue; break;
                case CardData.Rarity.Legendary: rarityBorder.color = Color.magenta; break;
                case CardData.Rarity.Exotic: rarityBorder.color = Color.yellow; break;
            }

            if (cardData.illustration != null)
            {
                cardArtImage.sprite = cardData.illustration;
            }
            else
            {
                cardArtImage.color = Color.clear;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Hover effect
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Reset hover effect
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (cardData == null || !CanBePlayed()) return;

            isDragging = true;
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            transform.SetParent(transform.root);
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                rectTransform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
            if (droppedOn != null && droppedOn.CompareTag("BattlefieldSlot"))
            {
                BattlefieldSlot slot = droppedOn.GetComponent<BattlefieldSlot>();
                if (slot != null && slot.IsEmpty)
                {
                    OnCardPlayed?.Invoke(cardData, slot.slotIndex);
                }
                else
                {
                    ReturnToHand();
                }
            }
            else
            {
                ReturnToHand();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (cardData == null || isDragging) return;

            if (cardData.cardType == CardData.CardType.Spell && CanBePlayed())
            {
                OnCardPlayed?.Invoke(cardData);
            }
        }

        private void ReturnToHand()
        {
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }

        private bool CanBePlayed()
        {
            if (GameManager.Instance == null || BattleManager.Instance == null) return false;
            if (BattleManager.Instance.CurrentPlayerId != SupabaseClient.Instance.GetAccessToken()) return false;
            if (BattleManager.Instance.CurrentPlayerMana < cardData.manaCost) return false;

            return true;
        }
    }
}