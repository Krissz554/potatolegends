// New Collection Service - Uses only the new card and collection system
import { supabase } from './supabase'

// Enhanced card interface for the new 226-card strategic system
export interface EnhancedCard {
  // Core Identifiers
  id: string
  registry_id: string
  name: string
  description: string
  set_id: string
  format_legalities: string[]
  
  // Gameplay Stats
  mana_cost: number // 0-10 mana
  attack: number | null // For units only
  hp: number | null // For units only
  structure_hp: number | null // For structures only
  spell_damage: number // For spells
  heal_amount: number // For healing effects
  
  // Card Classification
  rarity: 'common' | 'uncommon' | 'rare' | 'legendary' | 'exotic'
  card_type: 'unit' | 'spell' | 'structure' | 'relic'
  unit_class: 'warrior' | 'tank' | 'mage' | 'healer' | 'mixed' | null // For units only
  
  // Potato/Element System
  potato_type: 'ice' | 'fire' | 'lightning' | 'light' | 'void'
  trait: string
  adjective: string
  
  // Abilities & Keywords
  keywords: string[] // charge, taunt, lifesteal, freeze, overload, deathrattle, battlecry, etc.
  ability_text: string
  passive_effects: Record<string, any>
  triggered_effects: Record<string, any>
  passive_effect: string | null // For structures/relics
  target_type: 'none' | 'enemy_unit' | 'ally_unit' | 'any_unit' | 'enemy_hero' | 'ally_hero' | 'all_enemies' | 'all_allies' | 'all_units' | 'random_enemy' | 'random_ally' | null
  
  // Metadata / Cosmetic
  illustration_url: string
  frame_style: string
  flavor_text: string
  
  // Balance / Dev Data
  exotic: boolean
  is_exotic: boolean
  is_legendary: boolean
  release_date: string
  tags: string[]
  
  // Advanced
  alternate_skins: any[]
  voice_line_url: string
  craft_cost: number
  dust_value: number
  level_up_conditions: Record<string, any>
  token_spawns: string[]
  
  // Compatibility fields
  potato_type?: string
  trait?: string
  adjective?: string
  palette_name?: string
  pixel_art_data?: any
}

export interface CollectionItem {
  card: EnhancedCard
  quantity: number
  acquired_at: string
  source: string
}

// Get user's complete collection using new system
export async function getUserCollection(userId: string): Promise<{ success: boolean; data?: CollectionItem[]; error?: string }> {
  try {

    
    const { data, error } = await supabase.rpc('get_user_collection', {
      user_uuid: userId
    })

    if (error) {
      console.error('❌ Error loading collection:', error)
      return { success: false, error: error.message }
    }

    if (!data) {
      console.log('📦 No collection data found for user')
      return { success: true, data: [] }
    }

    // The RPC function returns JSON, so we need to parse it properly
    let collectionData: any[];
    
    if (typeof data === 'string') {
      collectionData = JSON.parse(data);
    } else if (Array.isArray(data)) {
      collectionData = data;
    } else {
      // data is already a JSON object/array from Supabase
      collectionData = data;
    }



    // Transform the data to match our CollectionItem interface
    const collectionItems: CollectionItem[] = (collectionData || []).map((item: any) => ({
      card: item.card || { id: item.card_id }, // Ensure card object exists
      quantity: item.quantity || 0,
      acquired_at: item.acquired_at || new Date().toISOString(),
      source: item.source || 'unknown'
    }));


    
    return { success: true, data: collectionItems }

  } catch (error) {
    console.error('Error in getUserCollection:', error)
    return { success: false, error: 'Failed to load collection' }
  }
}

// Unlock all cards for a user (testing function)
export async function unlockAllCardsForUser(userId: string): Promise<{ success: boolean; cardsUnlocked?: number; error?: string }> {
  try {
    console.log('🎁 Unlocking all cards for user:', userId)
    
    const { data, error } = await supabase.rpc('unlock_all_cards_for_user', {
      user_uuid: userId
    })

    if (error) {
      console.error('❌ Error unlocking cards:', error)
      return { success: false, error: error.message }
    }

    console.log('✅ Cards unlocked:', data)
    return { 
      success: true, 
      cardsUnlocked: data?.cards_unlocked || 0 
    }

  } catch (error) {
    console.error('Error in unlockAllCardsForUser:', error)
    return { success: false, error: 'Failed to unlock cards' }
  }
}

// Get all available cards (for browsing)
export async function getAllCards(): Promise<{ success: boolean; data?: EnhancedCard[]; error?: string }> {
  try {

    
    // Query card_complete table directly (not a view)
    const { data, error } = await supabase
      .from('card_complete')
      .select('*')
      .order('rarity', { ascending: false })
      .order('name', { ascending: true })

    if (error) {
      console.error('❌ Error loading cards:', error)
      return { success: false, error: error.message }
    }


    

    
    return { success: true, data: data || [] }

  } catch (error) {
    console.error('Error in getAllCards:', error)
    return { success: false, error: 'Failed to load cards' }
  }
}

// Add specific card to user's collection
export async function addCardToCollection(
  userId: string, 
  cardId: string, 
  quantity: number = 1,
  source: string = 'manual'
): Promise<{ success: boolean; error?: string }> {
  try {
    console.log(`➕ Adding ${quantity}x card ${cardId} to collection`)
    
    const { error } = await supabase
      .from('collections')
      .upsert({
        user_id: userId,
        card_id: cardId,
        quantity: quantity,
        source: source
      }, {
        onConflict: 'user_id,card_id'
      })

    if (error) {
      console.error('❌ Error adding card to collection:', error)
      return { success: false, error: error.message }
    }

    console.log('✅ Card added to collection')
    return { success: true }

  } catch (error) {
    console.error('Error in addCardToCollection:', error)
    return { success: false, error: 'Failed to add card to collection' }
  }
}

// Get collection statistics
export async function getCollectionStats(userId: string): Promise<{
  success: boolean
  stats?: {
    totalCards: number
    uniqueCards: number
    commonCards: number
    uncommonCards: number
    rareCards: number
    legendaryCards: number
    exoticCards: number
    completionPercentage: number
  }
  error?: string
}> {
  try {
    const [collectionResult, allCardsResult] = await Promise.all([
      getUserCollection(userId),
      getAllCards()
    ])

    if (!collectionResult.success || !allCardsResult.success) {
      return { success: false, error: 'Failed to load collection data' }
    }

    const collection = collectionResult.data || []
    const allCards = allCardsResult.data || []

    const stats = {
      totalCards: collection.reduce((sum, item) => sum + item.quantity, 0),
      uniqueCards: collection.length,
      commonCards: collection.filter(item => item.card.rarity === 'common').length,
      uncommonCards: collection.filter(item => item.card.rarity === 'uncommon').length,
      rareCards: collection.filter(item => item.card.rarity === 'rare').length,
      legendaryCards: collection.filter(item => item.card.rarity === 'legendary').length,
      exoticCards: collection.filter(item => item.card.exotic === true).length,
      completionPercentage: allCards.length > 0 ? Math.round((collection.length / allCards.length) * 100) : 0
    }

    return { success: true, stats }

  } catch (error) {
    console.error('Error in getCollectionStats:', error)
    return { success: false, error: 'Failed to calculate collection stats' }
  }
}