using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoLegends.UI
{
    public class HeroCard : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image heroPortraitImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI manaText;
        public TextMeshProUGUI heroPowerNameText;
        public TextMeshProUGUI heroPowerDescriptionText;
        public Image rarityBorder;

        [Header("Hero Data")]
        public HeroData heroData;

        public void SetupHero(HeroData data)
        {
            heroData = data;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (heroData == null) return;

            nameText.text = heroData.heroName;
            descriptionText.text = heroData.description;
            hpText.text = heroData.baseHp.ToString();
            manaText.text = heroData.baseMana.ToString();
            heroPowerNameText.text = heroData.heroPowerName;
            heroPowerDescriptionText.text = heroData.heroPowerDescription;

            switch (heroData.rarity)
            {
                case HeroData.Rarity.Common: rarityBorder.color = Color.gray; break;
                case HeroData.Rarity.Uncommon: rarityBorder.color = Color.green; break;
                case HeroData.Rarity.Rare: rarityBorder.color = Color.blue; break;
                case HeroData.Rarity.Legendary: rarityBorder.color = Color.magenta; break;
                case HeroData.Rarity.Exotic: rarityBorder.color = Color.yellow; break;
            }

            if (heroData.heroPortrait != null)
            {
                heroPortraitImage.sprite = heroData.heroPortrait;
            }
            else
            {
                heroPortraitImage.color = Color.clear;
            }
        }
    }
}