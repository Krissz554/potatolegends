using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoLegends.Data;

namespace PotatoLegends.UI
{
    public class CardUI : MonoBehaviour
    {
        [Header("Card Visual Elements")]
        public Image cardBackground;
        public Image rarityFrame;
        public Image elementFrame;
        public Image cardIllustration;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI manaCostText;
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI flavorTextText;

        [Header("Icons")]
        public Image manaIcon;
        public Image attackIcon;
        public Image healthIcon;

        [Header("Rarity Colors")]
        public Color commonColor = Color.gray;
        public Color uncommonColor = Color.green;
        public Color rareColor = Color.blue;
        public Color legendaryColor = Color.magenta;
        public Color exoticColor = Color.yellow;

        [Header("Element Colors")]
        public Color fireColor = Color.red;
        public Color iceColor = Color.cyan;
        public Color lightningColor = Color.yellow;
        public Color lightColor = Color.white;
        public Color voidColor = Color.black;

        private CardData cardData;
        private CollectionItem collectionItem;

        public void SetCardData(CardData card)
        {
            cardData = card;
            UpdateCardDisplay();
        }

        public void SetCollectionItem(CollectionItem item)
        {
            collectionItem = item;
            cardData = item.card;
            UpdateCardDisplay();
        }

        private void UpdateCardDisplay()
        {
            if (cardData == null) return;

            // Set basic card info
            if (cardNameText != null)
                cardNameText.text = cardData.name;

            if (manaCostText != null)
                manaCostText.text = cardData.mana_cost.ToString();

            if (descriptionText != null)
                descriptionText.text = cardData.description;

            if (flavorTextText != null)
                flavorTextText.text = cardData.flavor_text;

            // Set stats based on card type
            if (cardData.card_type == CardType.unit)
            {
                if (attackText != null)
                {
                    attackText.text = cardData.attack.ToString();
                    attackText.gameObject.SetActive(true);
                }
                if (healthText != null)
                {
                    healthText.text = cardData.hp.ToString();
                    healthText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (attackText != null) attackText.gameObject.SetActive(false);
                if (healthText != null) healthText.gameObject.SetActive(false);
            }

            // Set rarity frame color
            if (rarityFrame != null)
            {
                rarityFrame.color = GetRarityColor(cardData.rarity);
            }

            // Set element frame color
            if (elementFrame != null)
            {
                elementFrame.color = GetElementColor(cardData.potato_type);
            }

            // Set card background
            if (cardBackground != null)
            {
                cardBackground.color = GetElementColor(cardData.potato_type) * 0.3f;
            }

            // Set illustration
            if (cardIllustration != null && cardData.illustration != null)
            {
                cardIllustration.sprite = cardData.illustration;
            }
        }

        private Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.common:
                    return commonColor;
                case Rarity.uncommon:
                    return uncommonColor;
                case Rarity.rare:
                    return rareColor;
                case Rarity.legendary:
                    return legendaryColor;
                case Rarity.exotic:
                    return exoticColor;
                default:
                    return commonColor;
            }
        }

        private Color GetElementColor(PotatoType elementType)
        {
            switch (elementType)
            {
                case PotatoType.fire:
                    return fireColor;
                case PotatoType.ice:
                    return iceColor;
                case PotatoType.lightning:
                    return lightningColor;
                case PotatoType.light:
                    return lightColor;
                case PotatoType.void:
                    return voidColor;
                default:
                    return Color.white;
            }
        }

        public void OnCardClicked()
        {
            Debug.Log($"Card clicked: {cardData?.name}");
            // TODO: Implement card selection logic
        }
    }
}