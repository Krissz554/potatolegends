using System;
using System.Collections.Generic;
using UnityEngine;

namespace PotatoCardGame.Data
{
    /// <summary>
    /// Hero data structure matching the heroes table in Supabase
    /// Represents playable heroes with unique powers and abilities
    /// </summary>
    [Serializable]
    public class Hero
    {
        public string id;
        public string hero_id;
        public string name;
        public string description;
        public int base_hp;
        public int base_mana;
        public string hero_power_name;
        public string hero_power_description;
        public int hero_power_cost;
        public string rarity;
        public string element_type;
        public DateTime created_at;
        public DateTime updated_at;
        
        // Runtime battle properties
        [NonSerialized] public int current_hp;
        [NonSerialized] public int current_mana;
        [NonSerialized] public int max_mana;
        [NonSerialized] public bool hero_power_available;
        [NonSerialized] public int hero_power_cooldown;
        [NonSerialized] public bool hero_power_used_this_turn;
        [NonSerialized] public int fatigue_damage;
        
        public void InitializeForBattle()
        {
            current_hp = base_hp;
            current_mana = base_mana;
            max_mana = base_mana;
            hero_power_available = true;
            hero_power_cooldown = 0;
            hero_power_used_this_turn = false;
            fatigue_damage = 0;
        }
        
        public void StartTurn(int turnNumber)
        {
            // Increase mana each turn (up to 10)
            max_mana = Mathf.Min(10, turnNumber);
            current_mana = max_mana;
            
            // Reset hero power if it was used
            hero_power_used_this_turn = false;
            
            // Reduce cooldown
            if (hero_power_cooldown > 0)
            {
                hero_power_cooldown--;
                if (hero_power_cooldown == 0)
                {
                    hero_power_available = true;
                }
            }
        }
        
        public bool CanUseHeroPower()
        {
            return hero_power_available && 
                   !hero_power_used_this_turn && 
                   current_mana >= hero_power_cost &&
                   hero_power_cooldown == 0;
        }
        
        public void UseHeroPower()
        {
            if (!CanUseHeroPower()) return;
            
            current_mana -= hero_power_cost;
            hero_power_used_this_turn = true;
            hero_power_available = false;
            hero_power_cooldown = 1; // Most hero powers have 1 turn cooldown
            
            Debug.Log($"⚡ {name} used hero power: {hero_power_name}");
        }
        
        public void TakeDamage(int damage)
        {
            current_hp = Mathf.Max(0, current_hp - damage);
            Debug.Log($"🎯 {name} took {damage} damage. HP: {current_hp}/{base_hp}");
        }
        
        public void Heal(int healAmount)
        {
            current_hp = Mathf.Min(base_hp, current_hp + healAmount);
            Debug.Log($"💚 {name} healed for {healAmount}. HP: {current_hp}/{base_hp}");
        }
        
        public bool IsDefeated()
        {
            return current_hp <= 0;
        }
        
        public Color GetElementColor()
        {
            return element_type?.ToLower() switch
            {
                "fire" => new Color(1f, 0.3f, 0.3f),
                "water" => new Color(0.3f, 0.3f, 1f),
                "earth" => new Color(0.6f, 0.4f, 0.2f),
                "air" => new Color(0.8f, 0.8f, 0.9f),
                "light" => new Color(1f, 1f, 0.7f),
                "dark" => new Color(0.3f, 0.2f, 0.4f),
                "lightning" => new Color(1f, 1f, 0.3f),
                "void" => new Color(0.2f, 0.1f, 0.3f),
                _ => new Color(0.8f, 0.7f, 0.5f) // Neutral
            };
        }
        
        public Color GetRarityColor()
        {
            return rarity?.ToLower() switch
            {
                "common" => new Color(0.6f, 0.6f, 0.6f),
                "uncommon" => new Color(0.0f, 0.8f, 0.0f),
                "rare" => new Color(0.0f, 0.5f, 1.0f),
                "legendary" => new Color(0.8f, 0.0f, 0.8f),
                "exotic" => new Color(1.0f, 0.5f, 0.0f),
                _ => Color.white
            };
        }
    }
    
    /// <summary>
    /// User's hero collection and progress
    /// </summary>
    [Serializable]
    public class UserHero
    {
        public string id;
        public string user_id;
        public string hero_id;
        public bool is_active;
        public int total_wins;
        public int total_losses;
        public DateTime created_at;
        public DateTime updated_at;
        
        // Associated hero data
        public Hero hero_data;
        
        public float GetWinRate()
        {
            int totalGames = total_wins + total_losses;
            return totalGames > 0 ? (float)total_wins / totalGames : 0f;
        }
        
        public int GetTotalGames()
        {
            return total_wins + total_losses;
        }
    }
}