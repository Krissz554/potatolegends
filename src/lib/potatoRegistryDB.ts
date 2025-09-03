import { supabase } from './supabase'
import type { Rarity } from './potatoData'

export interface DBPotato {
  id: string
  registry_id: string
  name: string
  adjective: string
  potato_type: string
  trait: string
  rarity: Rarity
  description: string
  palette_name: string
  variation_index: number
  generation_seed: string
  pixel_art_data?: any
  sort_order: number
  created_at: string
  hp?: number
  attack?: number
  is_unlocked?: boolean
  unlocked_at?: string
  discovered_seed?: string
  is_favorite?: boolean
  view_count?: number
}

export interface RegistryStats {
  total: number
  unlocked: number
  byRarity: Record<Rarity, { total: number; unlocked: number }>
  completionPercentage: number
}



// Get all cards from card_complete table (our single source of truth)
export const getPotatoRegistryFromDB = async (): Promise<{
  potatoes: DBPotato[]
  stats: RegistryStats
  error?: any
}> => {
  try {
    // Use card_complete table as single source of truth
    const { data: cards, error } = await supabase
      .from('card_complete')
      .select('*')
      .order('sort_order')
    
    if (error) {
      console.error('Error fetching card registry:', error)
      return { potatoes: [], stats: { total: 0, unlocked: 0, byRarity: {} as any, completionPercentage: 0 }, error }
    }

    // Convert card_complete format to DBPotato format for compatibility
    const potatoes = (cards || []).map(card => ({
      id: card.id,
      registry_id: card.registry_id,
      name: card.name,
      adjective: card.adjective || '',
      potato_type: card.potato_type,
      trait: card.trait || '',
      rarity: card.rarity,
      description: card.description,
      palette_name: card.palette_name || '',
      variation_index: card.variation_index || 0,
      generation_seed: card.generation_seed || '',
      pixel_art_data: card.pixel_art_data,
      sort_order: card.sort_order || 0,
      created_at: card.created_at,
      hp: card.hp,
      attack: card.attack,
      is_unlocked: true, // All cards are "unlocked" in the new system
      unlocked_at: card.created_at,
      discovered_seed: card.generation_seed,
      is_favorite: false,
      view_count: 0
    }))

    // Calculate stats
    const stats = calculateRegistryStatsFromDB(potatoes)
    
    return { potatoes, stats }
    
  } catch (err) {
    console.error('Unexpected error fetching registry:', err)
    return { potatoes: [], stats: { total: 0, unlocked: 0, byRarity: {} as any, completionPercentage: 0 }, error: err }
  }
}

// Calculate stats from database potatoes
const calculateRegistryStatsFromDB = (potatoes: DBPotato[]): RegistryStats => {
  const stats: RegistryStats = {
    total: potatoes.length,
    unlocked: 0,
    byRarity: {
      common: { total: 0, unlocked: 0 },
      uncommon: { total: 0, unlocked: 0 },
      rare: { total: 0, unlocked: 0 },
      legendary: { total: 0, unlocked: 0 },
      exotic: { total: 0, unlocked: 0 }
    },
    completionPercentage: 0
  }

  potatoes.forEach(potato => {
    stats.byRarity[potato.rarity].total++
    if (potato.is_unlocked) {
      stats.unlocked++
      stats.byRarity[potato.rarity].unlocked++
    }
  })

  stats.completionPercentage = stats.total > 0 ? (stats.unlocked / stats.total) * 100 : 0

  return stats
}

// Add card to user's collection (replaces old unlock system)
export const unlockPotato = async (
  adjective: string,
  potatoType: string,
  trait: string,
  discoveredSeed: string
): Promise<{ success: boolean; potatoId?: string; error?: any }> => {
  try {
    // Get current user
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    
    if (authError || !user) {
      console.warn('User not authenticated for card unlock')
      return { success: false, error: 'User not authenticated' }
    }

    // Find the card in card_complete table
    const { data: card, error: findError } = await supabase
      .from('card_complete')
      .select('id')
      .eq('adjective', adjective)
      .eq('potato_type', potatoType)
      .eq('trait', trait)
      .single()
    
    if (findError || !card) {
      console.warn('Card not found in registry:', { adjective, potatoType, trait })
      return { success: false, error: findError || 'Card not found' }
    }

    // Check if already in user's collection
    const { data: existingCard } = await supabase
      .from('collections')
      .select('id, quantity')
      .eq('user_id', user.id)
      .eq('card_id', card.id)
      .single()
    
    if (existingCard) {
      // Already in collection, increase quantity
      const { error: updateError } = await supabase
        .from('collections')
        .update({ quantity: (existingCard.quantity || 0) + 1 })
        .eq('id', existingCard.id)
      
      if (updateError) {
        console.error('Error updating card quantity:', updateError)
        return { success: false, error: updateError }
      }
    } else {
      // Add to collection
      const { error: insertError } = await supabase
        .from('collections')
        .insert({
          user_id: user.id,
          card_id: card.id,
          quantity: 1,
          source: 'discovered'
        })
      
      if (insertError) {
        console.error('Error adding card to collection:', insertError)
        return { success: false, error: insertError }
      }
    }

    console.log('🔓 Card added to collection!', { adjective, potatoType, trait, userId: user.id })
    return { success: true, potatoId: card.id }
    
  } catch (err) {
    console.error('Unexpected error adding card to collection:', err)
    return { success: false, error: err }
  }
}

// Toggle favorite status (simplified - could be implemented later with user preferences)
export const togglePotatoFavorite = async (
  potatoId: string,
  isFavorite: boolean
): Promise<{ success: boolean; error?: any }> => {
  try {
    // In the new system, we could implement favorites in user_profiles or a separate table
    // For now, just return success (feature could be re-implemented later)
    console.log('Favorite toggle requested (feature simplified in new system)')
    return { success: true }
    
  } catch (err) {
    console.error('Unexpected error toggling favorite:', err)
    return { success: false, error: err }
  }
}

// Filter potatoes
export const filterPotatoRegistryDB = (
  potatoes: DBPotato[],
  filters: {
    search?: string
    rarity?: Rarity[]
    unlockedOnly?: boolean
    lockedOnly?: boolean
  }
): DBPotato[] => {
  let filtered = [...potatoes]

  // Search filter
  if (filters.search) {
    const lowerQuery = filters.search.toLowerCase()
    filtered = filtered.filter(potato => 
      (potato.name || '').toLowerCase().includes(lowerQuery) ||
      (potato.adjective || '').toLowerCase().includes(lowerQuery) ||
      (potato.potato_type || '').toLowerCase().includes(lowerQuery) ||
      (potato.trait || '').toLowerCase().includes(lowerQuery) ||
      (potato.description || '').toLowerCase().includes(lowerQuery)
    )
  }

  // Rarity filter
  if (filters.rarity && filters.rarity.length > 0) {
    filtered = filtered.filter(potato => filters.rarity!.includes(potato.rarity))
  }

  // Unlock status filter
  if (filters.unlockedOnly) {
    filtered = filtered.filter(potato => potato.is_unlocked)
  } else if (filters.lockedOnly) {
    filtered = filtered.filter(potato => !potato.is_unlocked)
  }

  return filtered
}

// Add all cards to user's collection (replaces old unlock all system)
export const unlockAllPotatoes = async (): Promise<{ success: boolean; unlockedCount?: number; error?: any }> => {
  try {
    // Get current user
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    
    if (authError || !user) {
      console.warn('User not authenticated for unlock all')
      return { success: false, error: 'User not authenticated' }
    }

    // Get all cards from card_complete table
    const { data: allCards, error: cardsError } = await supabase
      .from('card_complete')
      .select('id, registry_id, name')
    
    if (cardsError || !allCards) {
      console.error('Error fetching card registry:', cardsError)
      return { success: false, error: cardsError || 'Failed to fetch cards' }
    }

    // Get already owned cards for this user
    const { data: ownedCards, error: ownedError } = await supabase
      .from('collections')
      .select('card_id')
      .eq('user_id', user.id)
    
    if (ownedError) {
      console.error('Error fetching owned cards:', ownedError)
      return { success: false, error: ownedError }
    }

    const ownedIds = new Set(ownedCards?.map(c => c.card_id) || [])
    const cardsToAdd = allCards.filter(c => !ownedIds.has(c.id))

    if (cardsToAdd.length === 0) {
      return { success: true, unlockedCount: 0 }
    }

    // Prepare collection entries
    const collectionEntries = cardsToAdd.map(card => ({
      user_id: user.id,
      card_id: card.id,
      quantity: 1,
      source: 'unlock_all'
    }))

    // Batch insert all cards to collection
    const { error: insertError } = await supabase
      .from('collections')
      .insert(collectionEntries)
    
    if (insertError) {
      console.error('Error batch adding cards to collection:', insertError)
      return { success: false, error: insertError }
    }

    console.log(`🔓 All cards added to collection! ${cardsToAdd.length} new cards for user ${user.id}`)
    return { success: true, unlockedCount: cardsToAdd.length }
    
  } catch (err) {
    console.error('Unexpected error adding all cards to collection:', err)
    return { success: false, error: err }
  }
}

