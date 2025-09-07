using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using PotatoLegends.Data;
using PotatoLegends.Cards;

namespace PotatoLegends.UI
{
    public class DeckSlot : MonoBehaviour, IDropHandler
    {
        public int slotIndex;
        public CardDisplay currentCardDisplay;
        public Image slotImage;

        public event Action<CardData> OnCardRemovedFromSlot;

        public bool IsEmpty => currentCardDisplay == null;

        void Awake()
        {
            Button button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
            button.onClick.AddListener(OnSlotClicked);
        }

        public void SetCard(CardData card)
        {
            if (currentCardDisplay != null)
            {
                Destroy(currentCardDisplay.gameObject);
            }

            GameObject cardObj = Instantiate(UIManager.Instance.cardDisplayPrefab, transform);
            currentCardDisplay = cardObj.GetComponent<CardDisplay>();
            if (currentCardDisplay != null)
            {
                currentCardDisplay.SetupCard(card);
                currentCardDisplay.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }

        public void ClearCard()
        {
            if (currentCardDisplay != null)
            {
                Destroy(currentCardDisplay.gameObject);
                currentCardDisplay = null;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                CardDisplay draggedCardDisplay = eventData.pointerDrag.GetComponent<CardDisplay>();
                if (draggedCardDisplay != null && IsEmpty)
                {
                    CollectionManager.Instance.AddCardToDeck(draggedCardDisplay.cardData);
                }
            }
        }

        private void OnSlotClicked()
        {
            if (currentCardDisplay != null)
            {
                OnCardRemovedFromSlot?.Invoke(currentCardDisplay.cardData);
                ClearCard();
            }
        }
    }
}