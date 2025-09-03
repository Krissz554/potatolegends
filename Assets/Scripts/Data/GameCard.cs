using System;
using System.Collections.Generic;
using UnityEngine;

namespace PotatoCardGame.Data
{
    /// <summary>
    /// Represents a card from the database - matches the card_complete table structure
    /// This is the runtime representation of cards loaded from Supabase
    /// </summary>
    [Serializable]
    public class GameCard
    {
        public string id;
        public string registry_id;
        public string name;
        public string description;
        public string potato_type;
        public string trait;
        public string adjective;
        public string rarity;
        public string palette_name;
        public string pixel_art_data;
        public int generation_seed;
        public int variation_index;
        public int sort_order;
        public int mana_cost;
        public int attack;
        public int hp;
        public string card_type;
        public bool is_legendary;
        public bool exotic;
        public string set_id;
        public List<string> format_legalities;
        public string ability_text;
        public List<string> passive_effects;
        public List<string> triggered_effects;
        public string illustration_url;
        public string frame_style;
        public string flavor_text;
        public DateTime release_date;
        public List<string> tags;
        public int craft_cost;
        public int dust_value;
        public List<string> alternate_skins;
        public string voice_line_url;
        public List<string> level_up_conditions;
        public List<string> token_spawns;
        public DateTime created_at;
        public string pixel_art_config;
        public List<string> visual_effects;
        public string unit_class;
        public List<string> keywords;
        public string target_type;
        public int spell_damage;
        public int heal_amount;
        public int structure_hp;
        public string passive_effect;
        
        // Runtime properties for battle
        [NonSerialized] public int current_hp;
        [NonSerialized] public int current_attack;
        [NonSerialized] public bool has_attacked_this_turn;
        [NonSerialized] public bool can_attack;
        [NonSerialized] public List<StatusEffect> status_effects;
        [NonSerialized] public Vector2 battlefield_position;
        [NonSerialized] public bool is_summoning_sick;
        
        public GameCard()
        {
            format_legalities = new List<string>();
            passive_effects = new List<string>();
            triggered_effects = new List<string>();
            tags = new List<string>();
            alternate_skins = new List<string>();
            level_up_conditions = new List<string>();
            token_spawns = new List<string>();
            visual_effects = new List<string>();
            keywords = new List<string>();
            status_effects = new List<StatusEffect>();
        }
        
        public void InitializeForBattle()
        {
            current_hp = hp;
            current_attack = attack;
            has_attacked_this_turn = false;
            can_attack = !is_summoning_sick;
            status_effects = new List<StatusEffect>();
        }
        
        public bool CanAttack()
        {
            return current_attack > 0 && current_hp > 0 && !has_attacked_this_turn && can_attack;
        }
        
        public void TakeDamage(int damage)
        {
            current_hp = Mathf.Max(0, current_hp - damage);
            Debug.Log($"🎯 {name} took {damage} damage. HP: {current_hp}/{hp}");
        }
        
        public void Heal(int healAmount)
        {
            current_hp = Mathf.Min(hp, current_hp + healAmount);
            Debug.Log($"💚 {name} healed for {healAmount}. HP: {current_hp}/{hp}");
        }
        
        public bool IsDead()
        {
            return current_hp <= 0;
        }
        
        public Color GetRarityColor()
        {
            return rarity?.ToLower() switch
            {
                "common" => new Color(0.6f, 0.6f, 0.6f), // Gray
                "uncommon" => new Color(0.0f, 0.8f, 0.0f), // Green
                "rare" => new Color(0.0f, 0.5f, 1.0f), // Blue
                "legendary" => new Color(0.8f, 0.0f, 0.8f), // Purple
                "exotic" => new Color(1.0f, 0.5f, 0.0f), // Orange
                _ => Color.white
            };
        }
        
        public int GetMaxCopiesAllowed()
        {
            return rarity?.ToLower() switch
            {
                "common" => 3,
                "uncommon" => 2,
                "rare" => 2,
                "legendary" => 1,
                "exotic" => 1,
                _ => 2
            };
        }
        
        public bool IsSpell()
        {
            return card_type?.ToLower() == "spell";
        }
        
        public bool IsUnit()
        {
            return card_type?.ToLower() == "unit";
        }
        
        public bool IsStructure()
        {
            return card_type?.ToLower() == "structure";
        }
    }
    
    /// <summary>
    /// Represents a card in a user's collection with quantity
    /// </summary>
    [Serializable]
    public class CollectionCard
    {
        public GameCard card;
        public int quantity;
        public bool is_unlocked;
        public DateTime unlocked_at;
        
        public CollectionCard(GameCard gameCard, int qty = 1)
        {
            card = gameCard;
            quantity = qty;
            is_unlocked = true;
            unlocked_at = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Represents a card in a deck with quantity
    /// </summary>
    [Serializable]
    public class DeckCard
    {
        public GameCard card;
        public int quantity;
        
        public DeckCard(GameCard gameCard, int qty = 1)
        {
            card = gameCard;
            quantity = qty;
        }
    }
}