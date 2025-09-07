using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private void LoadCollection()
        {
            Debug.Log($"📚 Loading collection - Filter: {currentFilter}, Search: {currentSearchTerm}");
            
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
            // TODO: Load actual cards from your data source
            // For now, create mock cards
            CreateMockCards();
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
            if (statsText != null)
            {
                statsText.text = $"Cards: {totalCards}/500";
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