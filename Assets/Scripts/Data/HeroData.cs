using UnityEngine;

namespace PotatoLegends.Data
{
    [CreateAssetMenu(fileName = "New Hero", menuName = "Potato Card Game/Hero Data")]
    public class HeroData : ScriptableObject
    {
        public string heroId;
        public string heroName;
        [TextArea] public string description;

        public int baseHp;
        public int baseMana;
        public Sprite heroPortrait;

        [Header("Hero Power")]
        public string heroPowerName;
        [TextArea] public string heroPowerDescription;
        public int heroPowerCost;
        public HeroPowerTargetType heroPowerTargetType;
        public int heroPowerValue;

        public Rarity rarity;
        public ElementType elementType;

        public enum HeroPowerTargetType { None, EnemyUnit, AllyUnit, AnyUnit, EnemyHero, AllyHero }
        public enum Rarity { Common, Uncommon, Rare, Legendary, Exotic }
        public enum ElementType { Light, Void, Fire, Ice, Lightning }

        [System.NonSerialized] public int currentHp;
        [System.NonSerialized] public int currentMana;
        [System.NonSerialized] public bool heroPowerUsedThisTurn;
        [System.NonSerialized] public int heroPowerCooldown;

        public void InitializeRuntimeStats()
        {
            currentHp = baseHp;
            currentMana = 0;
            heroPowerUsedThisTurn = false;
            heroPowerCooldown = 0;
        }

        public void GainMana(int amount)
        {
            currentMana += amount;
            if (currentMana > 10) currentMana = 10;
        }

        public bool CanUseHeroPower()
        {
            return !heroPowerUsedThisTurn && currentMana >= heroPowerCost && heroPowerCooldown <= 0;
        }

        public void UseHeroPower()
        {
            if (CanUseHeroPower())
            {
                currentMana -= heroPowerCost;
                heroPowerUsedThisTurn = true;
                heroPowerCooldown = 1;
                Debug.Log($"{heroName} used {heroPowerName}!");
            }
            else
            {
                Debug.LogWarning($"{heroName} cannot use {heroPowerName} right now.");
            }
        }
    }
}