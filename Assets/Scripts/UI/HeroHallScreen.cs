using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PotatoLegends.Data;

namespace PotatoLegends.UI
{
    public class HeroHallScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public Transform heroGridParent;
        public GameObject heroCardPrefab;
        public TextMeshProUGUI selectedHeroNameText;
        public TextMeshProUGUI selectedHeroDescriptionText;
        public Button selectHeroButton;

        private List<HeroCard> displayedHeroes = new List<HeroCard>();
        private HeroData selectedHero;

        void OnEnable()
        {
            LoadHeroes();
            selectHeroButton.onClick.AddListener(OnSelectHeroButtonClicked);
            UpdateSelectedHeroUI();
        }

        void OnDisable()
        {
            ClearDisplayedHeroes();
            selectHeroButton.onClick.RemoveAllListeners();
        }

        private void LoadHeroes()
        {
            ClearDisplayedHeroes();

            HeroData hero1 = ScriptableObject.CreateInstance<HeroData>();
            hero1.heroId = "hero_001";
            hero1.heroName = "Potato King";
            hero1.description = "A majestic ruler with a powerful decree.";
            hero1.baseHp = 30;
            hero1.baseMana = 1;
            hero1.heroPowerName = "Royal Decree";
            hero1.heroPowerDescription = "Deal 2 damage to an enemy or restore 2 health to your hero.";
            hero1.heroPowerCost = 2;
            hero1.rarity = HeroData.Rarity.Legendary;
            hero1.elementType = HeroData.ElementType.Light;

            HeroData hero2 = ScriptableObject.CreateInstance<HeroData>();
            hero2.heroId = "hero_002";
            hero2.heroName = "Spud Mage";
            hero2.description = "Wields elemental potato magic.";
            hero2.baseHp = 25;
            hero2.baseMana = 1;
            hero2.heroPowerName = "Arcane Bolt";
            hero2.heroPowerDescription = "Deal 1 damage to an enemy unit.";
            hero2.heroPowerCost = 1;
            hero2.rarity = HeroData.Rarity.Rare;
            hero2.elementType = HeroData.ElementType.Fire;

            DisplayHero(hero1);
            DisplayHero(hero2);

            if (displayedHeroes.Count > 0)
            {
                OnHeroSelected(displayedHeroes[0].heroData);
            }
        }

        private void DisplayHero(HeroData heroData)
        {
            GameObject heroObj = Instantiate(heroCardPrefab, heroGridParent);
            HeroCard heroCard = heroObj.GetComponent<HeroCard>();
            if (heroCard != null)
            {
                heroCard.SetupHero(heroData);
                heroCard.GetComponent<Button>().onClick.AddListener(() => OnHeroSelected(heroData));
                displayedHeroes.Add(heroCard);
            }
        }

        private void OnHeroSelected(HeroData hero)
        {
            selectedHero = hero;
            UpdateSelectedHeroUI();
            Debug.Log($"Selected Hero: {hero.heroName}");
        }

        private void UpdateSelectedHeroUI()
        {
            if (selectedHero != null)
            {
                selectedHeroNameText.text = selectedHero.heroName;
                selectedHeroDescriptionText.text = selectedHero.description;
                selectHeroButton.interactable = true;
            }
            else
            {
                selectedHeroNameText.text = "Select a Hero";
                selectedHeroDescriptionText.text = "";
                selectHeroButton.interactable = false;
            }
        }

        private void OnSelectHeroButtonClicked()
        {
            if (selectedHero != null)
            {
                Debug.Log($"Hero '{selectedHero.heroName}' selected for play (logic not implemented).");
                GameManager.Instance.ChangeGameState(GameManager.GameState.MainMenu);
            }
        }

        private void ClearDisplayedHeroes()
        {
            foreach (HeroCard heroCard in displayedHeroes)
            {
                Destroy(heroCard.gameObject);
            }
            displayedHeroes.Clear();
        }
    }
}