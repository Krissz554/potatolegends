using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PotatoLegends.Data;
using PotatoLegends.Cards;
using PotatoLegends.Battle;
using PotatoLegends.Network;
using PotatoLegends.Core;

namespace PotatoLegends.UI
{
    public class BattleScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI battleStatusText;
        public TextMeshProUGUI playerManaText;
        public TextMeshProUGUI opponentManaText;
        public TextMeshProUGUI turnText;
        public Button endTurnButton;

        [Header("Player UI")]
        public Transform playerHandParent;
        public Transform playerBattlefieldParent;
        public GameObject cardDisplayPrefab;
        public GameObject battlefieldSlotPrefab;

        [Header("Opponent UI")]
        public Transform opponentHandParent;
        public Transform opponentBattlefieldParent;
        public GameObject opponentCardBackPrefab;

        private List<CardDisplay> playerHandCards = new List<CardDisplay>();
        private List<GameObject> opponentHandCards = new List<GameObject>();
        private List<BattlefieldSlot> playerBattlefieldSlots = new List<BattlefieldSlot>();
        private List<BattlefieldSlot> opponentBattlefieldSlots = new List<BattlefieldSlot>();

        void OnEnable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateUpdated += UpdateBattleUI;
                BattleManager.Instance.OnTurnChanged += OnTurnChanged;
                BattleManager.Instance.OnCardDeployed += OnCardDeployed;
                BattleManager.Instance.OnCardRemoved += OnCardRemoved;
                BattleManager.Instance.OnBattleEnded += OnBattleEnded;
            }

            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

            SetupBattlefieldSlots();
            UpdateBattleUI();
        }

        void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateUpdated -= UpdateBattleUI;
                BattleManager.Instance.OnTurnChanged -= OnTurnChanged;
                BattleManager.Instance.OnCardDeployed -= OnCardDeployed;
                BattleManager.Instance.OnCardRemoved -= OnCardRemoved;
                BattleManager.Instance.OnBattleEnded -= OnBattleEnded;
            }
            endTurnButton.onClick.RemoveAllListeners();
            ClearBattlefieldSlots();
            ClearHandCards();
        }

        private void SetupBattlefieldSlots()
        {
            ClearBattlefieldSlots();

            for (int i = 0; i < 6; i++)
            {
                GameObject slotObj = Instantiate(battlefieldSlotPrefab, playerBattlefieldParent);
                BattlefieldSlot slot = slotObj.GetComponent<BattlefieldSlot>();
                if (slot != null)
                {
                    slot.slotIndex = i;
                    slot.isPlayerSlot = true;
                    playerBattlefieldSlots.Add(slot);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                GameObject slotObj = Instantiate(battlefieldSlotPrefab, opponentBattlefieldParent);
                BattlefieldSlot slot = slotObj.GetComponent<BattlefieldSlot>();
                if (slot != null)
                {
                    slot.slotIndex = i;
                    slot.isPlayerSlot = false;
                    opponentBattlefieldSlots.Add(slot);
                }
            }
        }

        private void UpdateBattleUI()
        {
            if (BattleManager.Instance == null) return;

            battleStatusText.text = $"Phase: {BattleManager.Instance.CurrentBattlePhase}";
            playerManaText.text = $"Mana: {BattleManager.Instance.CurrentPlayerMana}/10";
            opponentManaText.text = $"Opponent Mana: {BattleManager.Instance.OpponentPlayerMana}/10";
            turnText.text = $"Turn: {BattleManager.Instance.CurrentTurnNumber}";

            endTurnButton.interactable = BattleManager.Instance.CurrentTurnPlayerId == SupabaseClient.Instance.GetAccessToken();

            UpdatePlayerHandUI();
            UpdateOpponentHandUI();
            UpdateBattlefieldUI();
        }

        private void UpdatePlayerHandUI()
        {
            ClearHandCards();
            for (int i = 0; i < 5; i++)
            {
                CardData dummyCard = ScriptableObject.CreateInstance<CardData>();
                dummyCard.cardId = $"hand_card_{i}";
                dummyCard.cardName = $"Hand Card {i+1}";
                dummyCard.manaCost = Random.Range(1, 5);
                dummyCard.attack = Random.Range(1, 5);
                dummyCard.health = Random.Range(1, 5);
                dummyCard.cardType = (i % 2 == 0) ? CardData.CardType.Unit : CardData.CardType.Spell;
                dummyCard.rarity = CardData.Rarity.Common;

                GameObject cardObj = Instantiate(cardDisplayPrefab, playerHandParent);
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                if (display != null)
                {
                    display.SetupCard(dummyCard);
                    display.OnCardPlayed += HandleCardPlayed;
                    playerHandCards.Add(display);
                }
            }
        }

        private void UpdateOpponentHandUI()
        {
            foreach (GameObject cardBack in opponentHandCards)
            {
                Destroy(cardBack);
            }
            opponentHandCards.Clear();

            for (int i = 0; i < 5; i++)
            {
                GameObject cardBackObj = Instantiate(opponentCardBackPrefab, opponentHandParent);
                opponentHandCards.Add(cardBackObj);
            }
        }

        private void UpdateBattlefieldUI()
        {
            // Placeholder for battlefield updates
        }

        private void OnTurnChanged(string newPlayerId)
        {
            Debug.Log($"BattleScreen: Turn changed to {newPlayerId}");
            UpdateBattleUI();
        }

        private void OnCardDeployed(CardData card, int slotIndex)
        {
            Debug.Log($"BattleScreen: Card {card.cardName} deployed to slot {slotIndex}");
            if (slotIndex >= 0 && slotIndex < playerBattlefieldSlots.Count)
            {
                playerBattlefieldSlots[slotIndex].SetCard(card);
            }
            UpdatePlayerHandUI();
        }

        private void OnCardRemoved(CardData card, int slotIndex)
        {
            Debug.Log($"BattleScreen: Card {card.cardName} removed from slot {slotIndex}");
            if (slotIndex >= 0 && slotIndex < playerBattlefieldSlots.Count)
            {
                playerBattlefieldSlots[slotIndex].SetEmpty();
            }
            UpdateBattleUI();
        }

        private void OnBattleEnded(string winnerId)
        {
            Debug.Log($"BattleScreen: Battle ended. Winner: {winnerId}");
            battleStatusText.text = $"Battle Ended! Winner: {winnerId}";
            endTurnButton.interactable = false;
        }

        private async void OnEndTurnButtonClicked()
        {
            Debug.Log("End Turn button clicked.");
            await BattleManager.Instance.EndTurn();
        }

        private async void HandleCardPlayed(CardData card, int targetSlot = -1)
        {
            Debug.Log($"Card {card.cardName} played. Target slot: {targetSlot}");
            if (card.cardType == CardData.CardType.Unit || card.cardType == CardData.CardType.Structure)
            {
                int emptySlotIndex = -1;
                for (int i = 0; i < playerBattlefieldSlots.Count; i++)
                {
                    if (playerBattlefieldSlots[i].IsEmpty())
                    {
                        emptySlotIndex = i;
                        break;
                    }
                }

                if (emptySlotIndex != -1)
                {
                    await BattleManager.Instance.DeployCard(card, emptySlotIndex);
                }
                else
                {
                    Debug.LogWarning("No empty battlefield slot available!");
                }
            }
            else if (card.cardType == CardData.CardType.Spell)
            {
                Debug.Log($"Spell {card.cardName} cast (logic not implemented yet).");
                UpdatePlayerHandUI();
            }
        }

        private void ClearBattlefieldSlots()
        {
            foreach (BattlefieldSlot slot in playerBattlefieldSlots)
            {
                Destroy(slot.gameObject);
            }
            playerBattlefieldSlots.Clear();

            foreach (BattlefieldSlot slot in opponentBattlefieldSlots)
            {
                Destroy(slot.gameObject);
            }
            opponentBattlefieldSlots.Clear();
        }

        private void ClearHandCards()
        {
            foreach (CardDisplay card in playerHandCards)
            {
                card.OnCardPlayed -= HandleCardPlayed;
                Destroy(card.gameObject);
            }
            playerHandCards.Clear();

            foreach (GameObject cardBack in opponentHandCards)
            {
                Destroy(cardBack);
            }
            opponentHandCards.Clear();
        }
    }
}