import { supabase } from './supabase'
import { RARITY_WEIGHTS, type Rarity } from './potatoData'

export interface RegistryPotato {
  id: string
  registry_id: string
  name: string
  adjective: string
  potato_type: string
  trait: string
  rarity: Rarity
  description: string
  palette_name: string
  generation_seed: string
  hp?: number
  attack?: number
  is_unlocked?: boolean
}

// Cache for registry potatoes to avoid repeated database calls
let registryCache: RegistryPotato[] | null = null
let cacheTimestamp: number = 0
const CACHE_DURATION = 5 * 60 * 1000 // 5 minutes

// Weighted random selection based on rarity
function pickRarity(random: () => number): Rarity {
  const totalWeight = Object.values(RARITY_WEIGHTS).reduce((sum, weight) => sum + weight, 0)
  let randomValue = random() * totalWeight
  
  for (const [rarity, weight] of Object.entries(RARITY_WEIGHTS)) {
    randomValue -= weight
    if (randomValue <= 0) {
      return rarity as Rarity
    }
  }
  return 'common'
}

// Simple hash function for seed consistency
function hashString(str: string): number {
  let h = 2166136261 >>> 0
  for (let i = 0; i < str.length; i++) {
    h ^= str.charCodeAt(i)
    h += (h << 1) + (h << 4) + (h << 7) + (h << 8) + (h << 24)
  }
  return h >>> 0
}

// Deterministic random number generator
function mulberry32(a: number) {
  return function () {
    let t = (a += 0x6d2b79f5)
    t = Math.imul(t ^ (t >>> 15), t | 1)
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61)
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296
  }
}

// Get all registry potatoes (with caching)
async function getRegistryPotatoes(): Promise<RegistryPotato[]> {
  const now = Date.now()
  
  // Return cached data if it's still fresh
  if (registryCache && (now - cacheTimestamp) < CACHE_DURATION) {
    return registryCache
  }

  try {
    const { data, error } = await supabase
      .from('card_complete')
      .select('*')
      .order('sort_order')

    if (error) {
      console.error('Error fetching card registry for generation:', error)
      return registryCache || [] // Return cached data or empty array on error
    }

    // Update cache
    registryCache = data || []
    cacheTimestamp = now
    
    console.log(`🃏 Loaded ${registryCache.length} cards from registry`)
    return registryCache
    
  } catch (err) {
    console.error('Unexpected error fetching registry:', err)
    return registryCache || []
  }
}

// Generate a potato from the registry using a seed
export async function generatePotatoFromRegistry(seed?: string): Promise<{
  potato: RegistryPotato | null
  wasAlreadyUnlocked: boolean
  error?: string
}> {
  try {
    const registry = await getRegistryPotatoes()
    
    if (!registry || registry.length === 0) {
      return { 
        potato: null, 
        wasAlreadyUnlocked: false,
        error: 'Registry is empty. Please seed the database first.' 
      }
    }

    // Use provided seed or generate random one
    const actualSeed = seed || `random-${Date.now()}-${Math.random()}`

    // Create deterministic random function from seed
    const rnd = mulberry32(hashString(actualSeed))
    
    // Pick a rarity based on weights
    const targetRarity = pickRarity(rnd)
    
    // Filter potatoes by the selected rarity with safety checks
    const potatoesOfRarity = (registry || []).filter(p => p && p.rarity === targetRarity)
    
    if (!potatoesOfRarity || potatoesOfRarity.length === 0) {
      console.warn(`No potatoes found for rarity: ${targetRarity}`)
      // Fallback to any potato if none of target rarity
      if (!registry || registry.length === 0) {
        return {
          potato: null,
          wasAlreadyUnlocked: false,
          error: 'No potatoes available in registry'
        }
      }
      const randomIndex = Math.floor(rnd() * registry.length)
      const selectedPotato = registry[randomIndex]
      if (!selectedPotato) {
        return {
          potato: null,
          wasAlreadyUnlocked: false,
          error: 'Failed to select potato from registry'
        }
      }
      return { 
        potato: selectedPotato, 
        wasAlreadyUnlocked: !!selectedPotato.is_unlocked 
      }
    }
    
    // Pick a random potato from the selected rarity
    const randomIndex = Math.floor(rnd() * potatoesOfRarity.length)
    const selectedPotato = potatoesOfRarity[randomIndex]
    
    if (!selectedPotato) {
      return {
        potato: null,
        wasAlreadyUnlocked: false,
        error: 'Failed to select potato from filtered list'
      }
    }
    
    console.log(`🎲 Generated ${selectedPotato.rarity} potato: ${selectedPotato.name} (${selectedPotato.is_unlocked ? 'already unlocked' : 'locked'})`)
    
    return { 
      potato: selectedPotato, 
      wasAlreadyUnlocked: !!selectedPotato.is_unlocked 
    }
    
  } catch (err) {
    console.error('Error generating potato from registry:', err)
    return { 
      potato: null, 
      wasAlreadyUnlocked: false,
      error: 'Failed to generate potato from registry.' 
    }
  }
}

// Generate a specific potato by its registry ID (for sharing)
export async function getPotatoByRegistryId(registryId: string): Promise<{
  potato: RegistryPotato | null
  wasAlreadyUnlocked: boolean
  error?: string
}> {
  try {
    const { data, error } = await supabase
      .from('card_complete')
      .select('*')
      .eq('registry_id', registryId)
      .single()

    if (error) {
      console.error('Error fetching card by registry ID:', error)
      return { 
        potato: null, 
        wasAlreadyUnlocked: false,
        error: 'Card not found in registry.' 
      }
    }

    return { 
      potato: data, 
      wasAlreadyUnlocked: true // All cards are "unlocked" in new system
    }
    
  } catch (err) {
    console.error('Unexpected error fetching potato by ID:', err)
    return { 
      potato: null, 
      wasAlreadyUnlocked: false,
      error: 'Failed to fetch potato.' 
    }
  }
}

// Clear the cache (useful for testing or after seeding)
export function clearRegistryCache(): void {
  registryCache = null
  cacheTimestamp = 0
  console.log('🧹 Registry cache cleared')
}

// Get registry statistics
export async function getRegistryStats(): Promise<{
  total: number
  byRarity: Record<Rarity, number>
  error?: string
}> {
  try {
    const registry = await getRegistryPotatoes()
    
    const stats = {
      total: registry.length,
      byRarity: {
        common: 0,
        uncommon: 0,
        rare: 0,
        legendary: 0,
        exotic: 0
      } as Record<Rarity, number>
    }

    registry.forEach(potato => {
      stats.byRarity[potato.rarity]++
    })

    return stats
    
  } catch (err) {
    console.error('Error getting registry stats:', err)
    return {
      total: 0,
      byRarity: {
        common: 0,
        uncommon: 0,
        rare: 0,
        legendary: 0,
        exotic: 0
      },
      error: 'Failed to get registry stats.'
    }
  }
}