using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoLegends.Core;
using PotatoLegends.Collection;
using PotatoLegends.Data;
using PotatoLegends.Network;
using System.Collections.Generic;
using System.Linq;

namespace PotatoLegends.UI
{
    public class CollectionScreenUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Button[] filterButtons;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Transform cardGridParent;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Filter Settings")]
        [SerializeField] private string[] filterNames = { "All", "Common", "Rare", "Epic", "Legendary" };
        [SerializeField] private Color selectedFilterColor = Color.yellow;
        [SerializeField] private Color normalFilterColor = Color.white;

        [Header("Card Display")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private int cardsPerPage = 18;
        [SerializeField] private int currentPage = 1;

        private string currentSearchTerm = "";
        private string currentFilter = "All";
        private int totalCards = 150; // Mock data

        private void Start()
        {
            SetupUI();
            LoadCollection();
        }

        private void SetupUI()
        {
            // Setup back button
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            // Setup search field
            if (searchField != null)
            {
                searchField.onValueChanged.AddListener(OnSearchChanged);
                searchField.placeholder.GetComponent<TextMeshProUGUI>().text = "Search cards...";
            }

            // Setup filter buttons
            SetupFilterButtons();

            // Setup scroll rect
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.AddListener(OnScrollChanged);
            }
        }

        private void SetupFilterButtons()
        {
            for (int i = 0; i < filterButtons.Length && i < filterNames.Length; i++)
            {
                if (filterButtons[i] != null)
                {
                    int filterIndex = i; // Capture for closure
                    filterButtons[i].onClick.AddListener(() => OnFilterClicked(filterIndex));
                    
                    // Update button text
                    TextMeshProUGUI buttonText = filterButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = filterNames[i];
                    }
                }
            }

            // Set first filter as selected
            SetFilterSelected(0);
        }

        private void OnBackClicked()
        {
            Debug.Log("🔙 Back button clicked");
            
            if (PotatoLegends.Core.GameSceneManager.Instance != null)
            {
                PotatoLegends.Core.GameSceneManager.Instance.LoadMainMenu();
            }
        }

        private void OnSearchChanged(string searchTerm)
        {
            currentSearchTerm = searchTerm;
            Debug.Log($"🔍 Search term changed: {searchTerm}");
            
            // Debounce search
            StopAllCoroutines();
            StartCoroutine(PerformSearch());
        }

        private System.Collections.IEnumerator PerformSearch()
        {
            yield return new WaitForSeconds(0.5f); // Wait for user to stop typing
            LoadCollection();
        }

        private void OnFilterClicked(int filterIndex)
        {
            if (filterIndex < filterNames.Length)
            {
                currentFilter = filterNames[filterIndex];
                Debug.Log($"🏷️ Filter changed to: {currentFilter}");
                
                SetFilterSelected(filterIndex);
                LoadCollection();
            }
        }

        private void SetFilterSelected(int selectedIndex)
        {
            for (int i = 0; i < filterButtons.Length; i++)
            {
                if (filterButtons[i] != null)
                {
                    Image buttonImage = filterButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = i == selectedIndex ? selectedFilterColor : normalFilterColor;
                    }
                }
            }
        }

        private void OnScrollChanged(Vector2 scrollPosition)
        {
            // Handle infinite scroll or pagination
            // TODO: Implement infinite scroll or load more cards
        }

        private async void LoadCollection()
        {
            Debug.Log($"📚 Loading collection - Filter: {currentFilter}, Search: {currentSearchTerm}");
            
            // Ensure CollectionManager exists
            if (CollectionManager.Instance == null)
            {
                GameObject collectionManagerGO = new GameObject("CollectionManager");
                collectionManagerGO.AddComponent<CollectionManager>();
            }

            // Load user collection
            await CollectionManager.Instance.LoadUserCollection();
            
            // Subscribe to collection loaded event
            CollectionManager.Instance.OnCollectionLoaded += OnCollectionLoaded;
            CollectionManager.Instance.OnCollectionError += OnCollectionError;
            
            // Clear existing cards
            ClearCardGrid();
            
            // Load cards based on filter and search
            LoadCardsForCurrentFilter();
            
            // Update stats
            UpdateStats();
        }

        private void ClearCardGrid()
        {
            if (cardGridParent != null)
            {
                foreach (Transform child in cardGridParent)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void LoadCardsForCurrentFilter()
        {
            if (CollectionManager.Instance == null || !CollectionManager.Instance.isCollectionLoaded)
            {
                Debug.Log("Collection not loaded yet, waiting...");
                return;
            }

            // Get filtered collection
            filteredCollection = GetFilteredCollection();
            
            // Create card UI objects
            CreateCardObjects();
        }

        private List<CollectionItem> GetFilteredCollection()
        {
            var collection = CollectionManager.Instance.userCollection;
            
            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchTerm))
            {
                collection = collection.Where(item => 
                    item.card.name.ToLower().Contains(currentSearchTerm.ToLower()) ||
                    item.card.description.ToLower().Contains(currentSearchTerm.ToLower())
                ).ToList();
            }

            // Apply rarity filter
            if (currentFilter != "All")
            {
                CardData.Rarity targetRarity = GetRarityFromFilter(currentFilter);
                collection = collection.Where(item => item.card.rarity == targetRarity).ToList();
            }

            return collection.ToList();
        }

        private CardData.Rarity GetRarityFromFilter(string filter)
        {
            switch (filter)
            {
                case "Common": return CardData.Rarity.common;
                case "Uncommon": return CardData.Rarity.uncommon;
                case "Rare": return CardData.Rarity.rare;
                case "Legendary": return CardData.Rarity.legendary;
                case "Exotic": return CardData.Rarity.exotic;
                default: return CardData.Rarity.common;
            }
        }

        private void CreateCardObjects()
        {
            if (cardPrefab == null || cardGridParent == null)
            {
                Debug.LogError("Card prefab or grid parent not assigned!");
                return;
            }

            foreach (var item in filteredCollection)
            {
                GameObject cardObj = Instantiate(cardPrefab, cardGridParent);
                CardUI cardUI = cardObj.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.SetCollectionItem(item);
                }
                cardObjects.Add(cardObj);
            }
        }

        private void OnCollectionLoaded(CollectionItem[] collection)
        {
            Debug.Log($"Collection loaded: {collection.Length} items");
            LoadCardsForCurrentFilter();
            UpdateStats();
        }

        private void OnCollectionError(string error)
        {
            Debug.LogError($"Collection error: {error}");
            // TODO: Show error message to user
        }

        private void CreateMockCards()
        {
            if (cardPrefab == null || cardGridParent == null)
            {
                Debug.LogWarning("Card prefab or grid parent not assigned!");
                return;
            }

            int cardsToShow = Mathf.Min(cardsPerPage, totalCards);
            
            for (int i = 0; i < cardsToShow; i++)
            {
                GameObject cardObj = Instantiate(cardPrefab, cardGridParent);
                
                // Setup card data
                SetupMockCard(cardObj, i);
            }
        }

        private void SetupMockCard(GameObject cardObj, int cardIndex)
        {
            // TODO: Setup actual card data
            // For now, just add some basic components
            
            // Add card click handler
            Button cardButton = cardObj.GetComponent<Button>();
            if (cardButton == null)
                cardButton = cardObj.AddComponent<Button>();
            
            cardButton.onClick.AddListener(() => OnCardClicked(cardIndex));
            
            // Add card image placeholder
            Image cardImage = cardObj.GetComponent<Image>();
            if (cardImage == null)
                cardImage = cardObj.AddComponent<Image>();
            
            // Set random color for demo
            cardImage.color = new Color(
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                1f
            );
        }

        private void OnCardClicked(int cardIndex)
        {
            Debug.Log($"🃏 Card clicked: {cardIndex}");
            // TODO: Show card details or add to deck
        }

        private void UpdateStats()
        {
            if (statsText != null && CollectionManager.Instance != null)
            {
                var stats = CollectionManager.Instance.GetCollectionStats();
                statsText.text = $"Cards: {stats.uniqueCards} / {CollectionManager.Instance.allCards.Count} | " +
                               $"Total: {stats.totalCards} | " +
                               $"Completion: {stats.completionPercentage:F1}%";
            }
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (backButton != null)
                backButton.onClick.RemoveAllListeners();

            if (searchField != null)
                searchField.onValueChanged.RemoveAllListeners();

            foreach (Button filterButton in filterButtons)
            {
                if (filterButton != null)
                    filterButton.onClick.RemoveAllListeners();
            }

            if (scrollRect != null)
                scrollRect.onValueChanged.RemoveAllListeners();
        }
    }
}