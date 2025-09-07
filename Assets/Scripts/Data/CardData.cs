using UnityEngine;

namespace PotatoLegends.Data
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Potato Card Game/Card Data")]
    public class CardData : ScriptableObject
    {
        public string cardId;
        public string cardName;
        [TextArea] public string description;
        [TextArea] public string flavorText;

        public int manaCost;
        public int attack;
        public int health;
        public int spellDamage;
        public int healAmount;

        public CardType cardType;
        public Rarity rarity;
        public ElementType elementType;
        public UnitClass unitClass;

        public Sprite illustration;

        [Header("Keywords & Abilities")]
        public bool hasCharge;
        public bool hasTaunt;
        public bool hasLifesteal;
        public bool hasPoison;
        public bool hasDivineShield;
        public bool hasDoubleStrike;
        public bool hasWindfury;
        public bool hasRush;
        public bool hasStealth;
        public bool hasImmune;
        public bool isFrozen;
        public bool isSilenced;
        [TextArea] public string abilityText;

        public enum CardType { Unit, Spell, Structure, Relic }
        public enum Rarity { Common, Uncommon, Rare, Legendary, Exotic }
        public enum ElementType { Light, Void, Fire, Ice, Lightning }
        public enum UnitClass { Warrior, Tank, Mage, Healer, Mixed, None }

        [System.NonSerialized] public int currentHealth;
        [System.NonSerialized] public int currentAttack;
        [System.NonSerialized] public bool hasAttackedThisTurn;
        [System.NonSerialized] public int deployedTurn;

        public void InitializeRuntimeStats()
        {
            currentHealth = health;
            currentAttack = attack;
            hasAttackedThisTurn = false;
            deployedTurn = 0;
            isFrozen = false;
            isSilenced = false;
        }

        public void TakeDamage(int damage)
        {
            if (hasImmune) return;

            if (hasDivineShield)
            {
                hasDivineShield = false;
                Debug.Log($"{cardName} divine shield absorbed {damage} damage!");
                return;
            }

            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;
            Debug.Log($"{cardName} took {damage} damage. Remaining HP: {currentHealth}");
        }

        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > health) currentHealth = health;
            Debug.Log($"{cardName} healed for {amount}. Current HP: {currentHealth}");
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }
    }
}