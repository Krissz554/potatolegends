using UnityEngine;
using System.Collections.Generic;

namespace PotatoLegends.Data
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Potato Card Game/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Core Identifiers")]
        public string id;
        public string registry_id;
        public new string name;
        [TextArea] public string description;
        public string set_id;
        public string[] format_legalities;

        [Header("Gameplay Stats")]
        public int mana_cost;
        public int attack;
        public int hp;
        public int structure_hp;
        public int spell_damage;
        public int heal_amount;

        [Header("Card Classification")]
        public Rarity rarity;
        public CardType card_type;
        public UnitClass unit_class;

        [Header("Potato/Element System")]
        public PotatoType potato_type;
        public string trait;
        public string adjective;

        [Header("Abilities & Keywords")]
        public string[] keywords;
        [TextArea] public string ability_text;
        public Dictionary<string, object> passive_effects;
        public Dictionary<string, object> triggered_effects;
        public string passive_effect;
        public TargetType target_type;

        [Header("Metadata / Cosmetic")]
        public string illustration_url;
        public string frame_style;
        [TextArea] public string flavor_text;

        [Header("Balance / Dev Data")]
        public bool exotic;
        public bool is_exotic;
        public bool is_legendary;
        public string release_date;
        public string[] tags;

        [Header("Advanced")]
        public object[] alternate_skins;
        public string voice_line_url;
        public int craft_cost;
        public int dust_value;
        public Dictionary<string, object> level_up_conditions;
        public string[] token_spawns;

        [Header("Unity Specific")]
        public Sprite illustration;
        public Sprite rarityFrame;
        public Sprite elementFrame;

        [System.NonSerialized] public int currentHealth;
        [System.NonSerialized] public int currentAttack;
        [System.NonSerialized] public bool hasAttackedThisTurn;
        [System.NonSerialized] public int deployedTurn;

        // Legacy property aliases for backward compatibility
        public string cardId => id;
        public string cardName => name;
        public int manaCost => mana_cost;
        public int health => hp;
        public CardType cardType => card_type;
        public Rarity rarityType => rarity;
        public PotatoType elementType => potato_type;

        public void InitializeRuntimeStats()
        {
            currentHealth = hp;
            currentAttack = attack;
            hasAttackedThisTurn = false;
            deployedTurn = 0;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;
            Debug.Log($"{name} took {damage} damage. Remaining HP: {currentHealth}");
        }

        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > hp) currentHealth = hp;
            Debug.Log($"{name} healed for {amount}. Current HP: {currentHealth}");
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }
    }

    public enum CardType { unit, spell, structure, relic }
    public enum Rarity { common, uncommon, rare, legendary, exotic }
    public enum PotatoType { ice, fire, lightning, light, @void }
    public enum UnitClass { warrior, tank, mage, healer, mixed, none }
    public enum TargetType { none, enemy_unit, ally_unit, any_unit, enemy_hero, ally_hero, all_enemies, all_allies, all_units, random_enemy, random_ally }
}