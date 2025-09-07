using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PotatoLegends.Cards;
using PotatoLegends.Collection;
using PotatoLegends.Data;
using PotatoLegends.Network;

namespace PotatoLegends.UI
{
    public class CollectionScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public Transform cardGridParent;
        public GameObject cardDisplayPrefab;
        public TextMeshProUGUI collectionStatsText;

        private List<CardDisplay> displayedCards = new List<CardDisplay>();

        void OnEnable()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCollectionUpdated += DisplayCollection;
                if (SupabaseClient.Instance != null && !string.IsNullOrEmpty(SupabaseClient.Instance.GetAccessToken()))
                {
                    _ = CollectionManager.Instance.LoadUserCollection();
                }
                else
                {
                    Debug.LogWarning("CollectionScreen: No user logged in to load collection.");
                }
            }
            DisplayCollection();
        }

        void OnDisable()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCollectionUpdated -= DisplayCollection;
            }
            ClearDisplayedCards();
        }

        private void DisplayCollection()
        {
            ClearDisplayedCards();

            if (CollectionManager.Instance == null || CollectionManager.Instance.UserCollection.Count == 0)
            {
                collectionStatsText.text = "No cards in collection.";
                return;
            }

            foreach (CollectionItem item in CollectionManager.Instance.UserCollection)
            {
                GameObject cardObj = Instantiate(cardDisplayPrefab, cardGridParent);
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                if (display != null)
                {
                    display.SetupCard(item.card);
                    displayedCards.Add(display);
                }
            }
            UpdateCollectionStats();
        }

        private void UpdateCollectionStats()
        {
            if (CollectionManager.Instance != null)
            {
                collectionStatsText.text = $"Total Cards: {CollectionManager.Instance.UserCollection.Count}";
            }
        }

        private void ClearDisplayedCards()
        {
            foreach (CardDisplay display in displayedCards)
            {
                Destroy(display.gameObject);
            }
            displayedCards.Clear();
        }
    }
}