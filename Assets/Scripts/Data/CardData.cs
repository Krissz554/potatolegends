using System.Collections.Generic;
using UnityEngine;

namespace PotatoCardGame.Data
{
    /// <summary>
    /// ScriptableObject that defines a card's properties and abilities
    /// This matches the card structure from your web game's database
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "Potato Card Game/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Basic Information")]
        public string cardId;
        public string cardName;
        public string description;
        
        [Header("Visual")]
        public Sprite cardArt;
        public Sprite cardFrame;
        public string illustrationUrl;
        
        [Header("Game Stats")]
        public int manaCost;
        public int attack;
        public int health;
        public CardType cardType;
        public Rarity rarity;
        
        [Header("Potato Properties")]
        public string potatoType;
        public string trait;
        public string adjective;
        public ElementType elementType;
        
        [Header("Abilities")]
        [TextArea(3, 5)]
        public string abilityText;
        public List<CardAbility> abilities = new List<CardAbility>();
        public List<PassiveEffect> passiveEffects = new List<PassiveEffect>();
        
        [Header("Collection Info")]
        public bool isLegendary;
        public bool isExotic;
        public string setId;
        public List<string> formatLegalities = new List<string>();
        
        [Header("Crafting")]
        public int craftCost;
        public int dustValue;
        
        [Header("Audio")]
        public AudioClip playSound;
        public AudioClip attackSound;
        public AudioClip deathSound;
        
        [Header("Advanced")]
        public List<string> keywords = new List<string>();
        public TargetType targetType;
        public int spellDamage;
        public int healAmount;
        public int structureHp;
        
        // Runtime data
        [System.NonSerialized]
        public int currentHealth;
        [System.NonSerialized]
        public int currentAttack;
        [System.NonSerialized]
        public bool hasAttackedThisTurn;
        [System.NonSerialized]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        
        private void OnEnable()
        {
            // Initialize runtime values
            currentHealth = health;
            currentAttack = attack;
            hasAttackedThisTurn = false;
        }
        
        public CardData CreateRuntimeCopy()
        {
            CardData copy = Instantiate(this);
            copy.currentHealth = health;
            copy.currentAttack = attack;
            copy.hasAttackedThisTurn = false;
            copy.statusEffects = new List<StatusEffect>();
            return copy;
        }
        
        public bool CanAttack()
        {
            return currentAttack > 0 && currentHealth > 0 && !hasAttackedThisTurn;
        }
        
        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            Debug.Log($"🎯 {cardName} took {damage} damage. Health: {currentHealth}/{health}");
        }
        
        public void Heal(int healAmount)
        {
            currentHealth = Mathf.Min(health, currentHealth + healAmount);
            Debug.Log($"💚 {cardName} healed for {healAmount}. Health: {currentHealth}/{health}");
        }
        
        public bool IsDead()
        {
            return currentHealth <= 0;
        }
    }
    
    [System.Serializable]
    public enum CardType
    {
        Unit,
        Spell,
        Structure,
        Hero,
        Token
    }
    
    [System.Serializable]
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary,
        Exotic
    }
    
    [System.Serializable]
    public enum ElementType
    {
        Earth,
        Fire,
        Water,
        Air,
        Light,
        Dark,
        Lightning,
        Void,
        Neutral
    }
    
    [System.Serializable]
    public enum TargetType
    {
        None,
        Enemy,
        Friendly,
        Any,
        Self,
        All,
        Random
    }
    
    [System.Serializable]
    public class CardAbility
    {
        public string abilityName;
        public string description;
        public AbilityType type;
        public AbilityTrigger trigger;
        public int value;
        public TargetType targetType;
        public List<AbilityEffect> effects = new List<AbilityEffect>();
    }
    
    [System.Serializable]
    public enum AbilityType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Draw,
        Summon,
        Transform,
        Destroy,
        Silence
    }
    
    [System.Serializable]
    public enum AbilityTrigger
    {
        OnPlay,
        OnDeath,
        OnAttack,
        OnTakeDamage,
        StartOfTurn,
        EndOfTurn,
        OnTargeted
    }
    
    [System.Serializable]
    public class AbilityEffect
    {
        public AbilityType effectType;
        public int value;
        public int duration;
        public TargetType targetType;
    }
    
    [System.Serializable]
    public class PassiveEffect
    {
        public string effectName;
        public string description;
        public PassiveType type;
        public int value;
    }
    
    [System.Serializable]
    public enum PassiveType
    {
        AttackBonus,
        HealthBonus,
        ManaReduction,
        DamageReduction,
        Taunt,
        Stealth,
        Charge,
        LifeSteal
    }
    
    [System.Serializable]
    public class StatusEffect
    {
        public string effectName;
        public StatusType type;
        public int value;
        public int duration;
        public bool isPermanent;
    }
    
    [System.Serializable]
    public enum StatusType
    {
        AttackBuff,
        AttackDebuff,
        HealthBuff,
        HealthDebuff,
        Frozen,
        Stunned,
        Poisoned,
        Burning,
        Protected
    }
}