import { supabase } from '@/lib/supabase'
import { toast } from 'sonner'
import { DBPotato } from '@/lib/potatoRegistryDB'
import { generatePotatoStats } from '@/lib/potatoData'

/**
 * Check if user has a valid 30-card deck that can be used for battles
 */
export async function checkUserHasValidDeck(userId: string): Promise<boolean> {
  try {
    // Get user's active deck
    const { data: activeDeck, error: deckError } = await supabase
      .from('decks')
      .select('id')
      .eq('user_id', userId)
      .eq('is_active', true)
      .single()

    if (deckError || !activeDeck) {
      console.log('❌ No active deck found for user:', userId)
      return false
    }

    // Check if deck has exactly 30 cards and valid copy limits
    const { data: deckCards, error: cardsError } = await supabase
      .from('deck_cards')
      .select(`
        quantity,
        card_complete!inner(
          name,
          rarity
        )
      `)
      .eq('deck_id', activeDeck.id)

    if (cardsError) {
      console.error('❌ Error checking deck cards:', cardsError)
      return false
    }

    // Calculate total cards in deck
    const totalCards = deckCards?.reduce((sum, card) => sum + card.quantity, 0) || 0
    
    if (totalCards !== 30) {
      console.log(`❌ Invalid deck size: ${totalCards}/30 cards`)
      return false
    }

    // Check copy limits for each card
    const getMaxCopiesByRarity = (rarity: string): number => {
      switch (rarity) {
        case 'common': return 3;
        case 'uncommon': return 2;
        case 'rare': return 2;
        case 'legendary': return 1;
        case 'exotic': return 1;
        default: return 2;
      }
    };

    for (const deckCard of deckCards || []) {
      const maxAllowed = getMaxCopiesByRarity(deckCard.card_complete.rarity);
      if (deckCard.quantity > maxAllowed) {
        console.log(`❌ Invalid copy count: ${deckCard.card_complete.name} (${deckCard.card_complete.rarity}) has ${deckCard.quantity} copies, max allowed: ${maxAllowed}`);
        return false;
      }
    }
    
    console.log(`✅ User deck is valid: ${totalCards} cards with proper copy limits`)
    return true

  } catch (error) {
    console.error('❌ Error checking user deck:', error)
    return false
  }
}

export interface BattleDeck {
  id: string
  user_id: string
  potato_id: string
  deck_position: number
  potato?: DBPotato
}

export interface MatchmakingEntry {
  id: string
  user_id: string
  joined_at: string
  status: 'searching' | 'matched' | 'cancelled'
}

export interface BattleSession {
  id: string
  player1_id: string
  player2_id: string
  current_turn_player_id: string | null
  game_state: {
    player1_deck: BattleDeck[]
    player2_deck: BattleDeck[]
    player1_hand: BattleDeck[] // Remaining cards in hand
    player2_hand: BattleDeck[]
    battlefield: {
      player1_active?: BattleDeck & { current_hp: number; max_hp: number; attack: number }
      player2_active?: BattleDeck & { current_hp: number; max_hp: number; attack: number }
    }
    phase: 'waiting_first_deploy' | 'waiting_second_deploy' | 'combat' | 'waiting_redeploy' | 'finished'
    turn_count: number
    round_count: number
    actions: BattleAction[]
    first_player_id: string // Who starts the game
    current_attacker_id?: string // Who attacks in current combat turn
    last_round_winner_id?: string // Who won the last round
    waiting_for_deploy_player_id?: string // Who needs to deploy next card
  }
  status: 'deploying' | 'battling' | 'finished'
  winner_id?: string
  started_at: string
  finished_at?: string
}

export interface BattleAction {
  id: string
  battle_session_id: string
  player_id: string
  action_type: 'deploy' | 'attack' | 'surrender' | 'start_game'
  action_data: {
    card_id?: string
    damage?: number
    target_card_id?: string
    remaining_hp?: number
    defeated_card_id?: string
    round_winner?: string
    [key: string]: any
  }
  created_at: string
}



// Save battle deck to database for specific slot
export const saveBattleDeck = async (battleDeck: (DBPotato | null)[], deckSlot: number = 1): Promise<{ success: boolean; error?: string }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) {
      return { success: false, error: 'User not authenticated' }
    }

    if (deckSlot < 1 || deckSlot > 3) {
      return { success: false, error: 'Invalid deck slot. Must be 1, 2, or 3.' }
    }

    // Remove existing deck for this slot
    await supabase
      .from('battle_decks')
      .delete()
      .eq('user_id', user.id)
      .eq('deck_slot', deckSlot)

    // Insert new deck
    const deckEntries = battleDeck
      .map((potato, index) => potato ? {
        user_id: user.id,
        potato_id: potato.id,
        deck_position: index + 1,
        deck_slot: deckSlot
      } : null)
      .filter(Boolean)

    if (deckEntries.length > 0) {
      const { error: insertError } = await supabase
        .from('battle_decks')
        .insert(deckEntries)

      if (insertError) {
        console.error('Error saving battle deck:', insertError)
        return { success: false, error: insertError.message }
      }
    }

    return { success: true }
  } catch (error) {
    console.error('Unexpected error saving battle deck:', error)
    return { success: false, error: 'An unexpected error occurred' }
  }
}

// Load battle deck from database for specific slot
export const loadBattleDeck = async (deckSlot: number = 1): Promise<{ deck: (DBPotato | null)[]; error?: string }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) {
      return { deck: [null, null, null, null, null], error: 'User not authenticated' }
    }

    if (deckSlot < 1 || deckSlot > 3) {
      return { deck: [null, null, null, null, null], error: 'Invalid deck slot. Must be 1, 2, or 3.' }
    }

    const { data: deckData, error } = await supabase
      .from('battle_decks')
      .select(`
        deck_position,
        potato_registry (
          id,
          registry_id,
          name,
          adjective,
          potato_type,
          trait,
          description,
          rarity,
          generation_seed,
          hp,
          attack,
          created_at
        )
      `)
      .eq('user_id', user.id)
      .eq('deck_slot', deckSlot)
      .order('deck_position')

    if (error) {
      console.error('Error loading battle deck:', error)
      return { deck: [null, null, null, null, null], error: error.message }
    }

    // Reconstruct deck array
    const deck: (DBPotato | null)[] = [null, null, null, null, null]
    deckData?.forEach(entry => {
      if (entry.potato_registry && entry.deck_position >= 1 && entry.deck_position <= 5) {
        deck[entry.deck_position - 1] = {
          ...entry.potato_registry,
          is_unlocked: true, // Cards in deck are always unlocked
          is_favorite: false
        } as DBPotato
      }
    })

    return { deck }
  } catch (error) {
    console.error('Unexpected error loading battle deck:', error)
    return { deck: [null, null, null, null, null], error: 'An unexpected error occurred' }
  }
}

// Get user's active deck slot
export const getActiveDeckSlot = async (): Promise<{ deckSlot: number; error?: string }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) {
      return { deckSlot: 1, error: 'User not authenticated' }
    }

    // Simply return slot 1 since we now use is_active flag on decks table
    // No need for separate user_deck_settings table
    return { deckSlot: 1 }
  } catch (error) {
    console.error('Unexpected error getting active deck slot:', error)
    return { deckSlot: 1, error: 'An unexpected error occurred' }
  }
}

// Set user's active deck slot (simplified - use is_active flag on decks table)
export const setActiveDeckSlot = async (deckSlot: number): Promise<{ success: boolean; error?: string }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) {
      return { success: false, error: 'User not authenticated' }
    }

    // Since we removed user_deck_settings, just return success
    // The active deck is now managed by the is_active flag on the decks table
    console.log('Active deck slot setting simplified - using decks.is_active flag')
    return { success: true }
  } catch (error) {
    console.error('Unexpected error setting active deck slot:', error)
    return { success: false, error: 'An unexpected error occurred' }
  }
}

// Join matchmaking queue
export const joinMatchmaking = async (): Promise<{ success: boolean; error?: string; activeBattle?: BattleSession }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) {
      return { success: false, error: 'User not authenticated' }
    }

    console.log('Joining matchmaking queue for user:', user.id)

    // First check if user already has an active battle
    const { activeBattle } = await checkForActiveBattle()
    if (activeBattle) {
      toast.success('🎮 Rejoining your existing battle!')
      
      // Dispatch the battleFound event to open the arena
      window.dispatchEvent(new CustomEvent('battleFound', { detail: activeBattle }))
      
      return { success: true, activeBattle }
    }

        // Check if user has a valid 30-card deck
    const hasValidDeck = await checkUserHasValidDeck(user.id)
    
    if (!hasValidDeck) {
      console.log('⚠️ User does not have a valid 30-card deck')
      return { 
        success: false, 
        error: 'You need to build a complete 30-card deck before you can battle! Go to the Deck Builder to create your deck.'
      }
    }

    // Clear any existing entries for this user first (simpler approach)
    await supabase
      .from('matchmaking_queue')
      .delete()
      .eq('user_id', user.id)

    // Add to queue
    const { error: insertError } = await supabase
      .from('matchmaking_queue')
      .insert({
        user_id: user.id,
        status: 'searching'
      })

    if (insertError) {
      console.error('Error joining matchmaking queue:', insertError)
      return { success: false, error: insertError.message }
    }

    console.log('Successfully joined matchmaking queue')

    // Start listening for matches and wait for subscription to be ready
    startMatchmakingListener()
    
    // Start bulletproof background polling for missed matches
    startBackgroundMatchmakingPolling()
    
    // Wait a moment for real-time subscription to be fully established
    console.log('⏳ Waiting for real-time subscription to be ready...')
    await new Promise(resolve => setTimeout(resolve, 1500))

    startPeriodicMatchmaking()

    // Server will handle matchmaking automatically via edge function

    return { success: true }
  } catch (error) {
    console.error('Unexpected error joining matchmaking:', error)
    return { success: false, error: 'An unexpected error occurred' }
  }
}

// Leave matchmaking queue
export const leaveMatchmaking = async (): Promise<{ success: boolean }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) return { success: false }

    await supabase
      .from('matchmaking_queue')
      .delete()
      .eq('user_id', user.id)

    stopMatchmakingListener()
    stopBackgroundMatchmakingPolling()
    stopPeriodicMatchmaking()
    return { success: true }
  } catch (error) {
    console.error('Error leaving matchmaking:', error)
    return { success: false }
  }
}

// Matchmaking listener
let matchmakingSubscription: any = null
let isMatchmakingActive = false

export const startMatchmakingListener = () => {
  if (matchmakingSubscription) {
    console.log('📡 Matchmaking subscription already exists, removing old one...')
    stopMatchmakingListener()
    // Wait a bit for cleanup to complete
    setTimeout(() => startMatchmakingListenerInternal(), 1000)
    return
  }
  
  startMatchmakingListenerInternal()
}

const startMatchmakingListenerInternal = () => {
  console.log('📡 Starting bulletproof matchmaking real-time subscription...')
  isMatchmakingActive = true
  
  matchmakingSubscription = supabase
    .channel('matchmaking-global') // Use consistent channel name
    .on('postgres_changes', 
      { 
        event: 'INSERT', 
        schema: 'public', 
        table: 'matchmaking_queue' 
      }, 
      async (payload) => {
        console.log('📥 New player joined queue:', payload.new)
        // Server will handle matching automatically
      }
    )
    .on('postgres_changes', 
      { 
        event: 'INSERT', 
        schema: 'public', 
        table: 'battle_sessions' 
      }, 
      async (payload) => {

        
        const { data: { user } } = await supabase.auth.getUser()
        if (!user) {
          console.error('❌ No user found for battle session event')
          return
        }

        const battleSession = payload.new as BattleSession
        console.log('👤 PRODUCTION - Current user ID:', user.id)
        console.log('🥊 PRODUCTION - Battle players:', { player1: battleSession.player1_id, player2: battleSession.player2_id })
        
        // Check if this is a test battle (has the test UUID)
        const isTestBattle = battleSession.player2_id === '99999999-9999-9999-9999-999999999999'
        console.log('🧪 PRODUCTION - Is test battle?', isTestBattle)
        
        if (battleSession.player1_id === user.id || battleSession.player2_id === user.id) {
          console.log('✅ PRODUCTION - Battle session created for current user:', battleSession.id)
          console.log('🎮 PRODUCTION - Battle details:', {
            battleId: battleSession.id,
            status: battleSession.status,
            player1: battleSession.player1_id,
            player2: battleSession.player2_id,
            currentUser: user.id,
            isPlayer1: battleSession.player1_id === user.id,
            isPlayer2: battleSession.player2_id === user.id
          })
          
          if (!isTestBattle) {
            console.log('🎯 PRODUCTION - Real battle found! Showing arena...')
            toast.success('🎯 Match found! Entering battle arena...')
            
            // Clean up matchmaking first
            console.log('🧹 PRODUCTION - Cleaning up matchmaking...')
            await leaveMatchmaking()
            
            // Small delay to ensure cleanup completed
            setTimeout(() => {
              console.log('🚀 PRODUCTION - Dispatching battleFound event with battle:', battleSession.id)
              window.dispatchEvent(new CustomEvent('battleFound', { detail: battleSession }))
            }, 100)
          } else {
            console.log('🧪 PRODUCTION - Ignoring test battle, not dispatching battleFound')
          }
        } else {
          console.log('ℹ️ PRODUCTION - Battle session not for current user, ignoring')
          console.log('🔍 PRODUCTION - User ID comparison:', {
            currentUser: user.id,
            player1: battleSession.player1_id,
            player2: battleSession.player2_id,
            matchesPlayer1: battleSession.player1_id === user.id,
            matchesPlayer2: battleSession.player2_id === user.id
          })
        }
      }
    )
    .subscribe((status, err) => {
      console.log('📡 Matchmaking subscription status:', status)
      if (err) {
        console.error('📡 Matchmaking subscription error:', err)
        
        // Handle specific binding mismatch error
        if (err.message && err.message.includes('mismatch between server and client bindings')) {
          console.log('🔧 Binding mismatch detected, forcing cleanup and retry...')
          stopMatchmakingListener()
          setTimeout(() => {
            if (isMatchmakingActive) {
              startMatchmakingListener()
            }
          }, 3000) // Longer delay for binding mismatch
          return
        }
      }
      
      if (status === 'SUBSCRIBED') {
        console.log('✅ Matchmaking real-time subscription is now active and ready!')
        
        // Set up heartbeat to keep connection alive (less frequent)
        const heartbeat = setInterval(() => {
          console.log('💓 Matchmaking heartbeat')
        }, 60000) // Heartbeat every 60 seconds (less spam)
        
        // Store heartbeat for cleanup
        if (matchmakingSubscription) {
          (matchmakingSubscription as any).heartbeat = heartbeat
        }
      } else if (status === 'CHANNEL_ERROR') {
        console.error('❌ Matchmaking subscription error!')
        // Less aggressive retry with exponential backoff
        setTimeout(() => {
          if (isMatchmakingActive) {
            startMatchmakingListener()
          }
        }, 5000) // 5 second delay instead of 2
      } else if (status === 'CLOSED' && isMatchmakingActive) {
        console.log('📡 Matchmaking subscription closed')
        // Much less aggressive reconnection - only if truly needed
        setTimeout(() => {
          if (isMatchmakingActive && !matchmakingSubscription) {
            console.log('🔄 Attempting matchmaking reconnect...')
            startMatchmakingListener()
          }
        }, 3000) // 3 second delay instead of 1
      } else if (status === 'TIMED_OUT' && isMatchmakingActive) {
        console.log('⏰ Matchmaking subscription timed out')
        setTimeout(() => {
          if (isMatchmakingActive && !matchmakingSubscription) {
            console.log('🔄 Reconnecting after timeout...')
            startMatchmakingListener()
          }
        }, 2000) // 2 second delay instead of 500ms
      } else if (status === 'CLOSED' || status === 'TIMED_OUT') {
        console.log('📡 Matchmaking subscription closed (inactive)')
      }
    })
}

export const stopMatchmakingListener = () => {
  if (matchmakingSubscription) {
    console.log('📡 Cleaning up matchmaking subscription')
    isMatchmakingActive = false
    // Clean up heartbeat if it exists
    if ((matchmakingSubscription as any).heartbeat) {
      clearInterval((matchmakingSubscription as any).heartbeat)
    }
    supabase.removeChannel(matchmakingSubscription)
    matchmakingSubscription = null
  }
}

// Periodic matchmaking
let matchmakingInterval: NodeJS.Timeout | null = null
let matchmakingPollingInterval: NodeJS.Timeout | null = null
let backgroundMatchmakingInterval: NodeJS.Timeout | null = null

// This function will be redefined below with client-side matching

const stopPeriodicMatchmaking = () => {
  if (matchmakingInterval) {
    clearInterval(matchmakingInterval)
    matchmakingInterval = null
  }
  if (matchmakingPollingInterval) {
    clearInterval(matchmakingPollingInterval)
    matchmakingPollingInterval = null
  }
  if (backgroundMatchmakingInterval) {
    clearInterval(backgroundMatchmakingInterval)
    backgroundMatchmakingInterval = null
  }
}

// Bulletproof background matchmaking detection
export const startBackgroundMatchmakingPolling = () => {
  
  
  // Stop any existing polling
  if (matchmakingPollingInterval) {
    clearInterval(matchmakingPollingInterval)
  }
  if (backgroundMatchmakingInterval) {
    clearInterval(backgroundMatchmakingInterval)
  }

  // Regular polling every 3 seconds
  matchmakingPollingInterval = setInterval(async () => {
    try {
      const result = await checkForActiveBattle()
      if (result.activeBattle) {
        console.log('🎯 Background polling found active battle:', result.activeBattle.id)
        // Trigger battle found event
        window.dispatchEvent(new CustomEvent('battleFound', { detail: { activeBattle: result.activeBattle } }))
        // Stop polling since battle was found
        stopPeriodicMatchmaking()
      }
    } catch (error) {
      console.log('Background matchmaking poll error (non-critical):', error)
    }
  }, 3000)

  // Aggressive background polling every 1 second (for when tab is minimized)
  backgroundMatchmakingInterval = setInterval(async () => {
    try {
      const result = await checkForActiveBattle()
      if (result.activeBattle) {
        console.log('🕐 Aggressive background polling found battle:', result.activeBattle.id)
        window.dispatchEvent(new CustomEvent('battleFound', { detail: { activeBattle: result.activeBattle } }))
        stopPeriodicMatchmaking()
      }
    } catch (error) {
      // Silent error for aggressive polling
    }
  }, 1000)

  // Enhanced visibility change handling for matchmaking
  const handleVisibilityChange = async () => {
    if (!document.hidden) {

      
      // Check for active battles
      try {
        const result = await checkForActiveBattle()
        if (result.activeBattle) {
          console.log('🎯 Visibility check found battle:', result.activeBattle.id)
          window.dispatchEvent(new CustomEvent('battleFound', { detail: { activeBattle: result.activeBattle } }))
          stopPeriodicMatchmaking()
          return
        }
      } catch (error) {
        console.log('Visibility matchmaking check error:', error)
      }
      
      // Check matchmaking subscription health
      if (matchmakingSubscription) {
        try {
          // Try to ping the subscription by checking its state
          const isHealthy = matchmakingSubscription.state === 'joined'
          if (!isHealthy) {
            console.log('🔄 Matchmaking subscription unhealthy, forcing reconnect...')
            startMatchmakingListener()
          }
        } catch (error) {
          console.log('Matchmaking health check error:', error)
          startMatchmakingListener()
        }
      }
    }
  }

  // Also handle window focus for additional reliability
  const handleWindowFocus = () => {
    if (!document.hidden) {
      handleVisibilityChange()
    }
  }

  document.addEventListener('visibilitychange', handleVisibilityChange)
  window.addEventListener('focus', handleWindowFocus)
  
  return () => {
    document.removeEventListener('visibilitychange', handleVisibilityChange)
    window.removeEventListener('focus', handleWindowFocus)
    stopPeriodicMatchmaking()
  }
}

export const stopBackgroundMatchmakingPolling = () => {
  stopPeriodicMatchmaking()
}

// Client-side matching function that creates battles directly
// This function is no longer needed - server-side matchmaking handles everything

// Server-side matchmaking via edge function
const startPeriodicMatchmaking = () => {
  if (matchmakingInterval) return

  // Call server-side matchmaker every 2 seconds
  matchmakingInterval = setInterval(async () => {
    await callServerMatchmaker()
  }, 2000)
}

// Removed testQueueVisibility function - test-queue edge function was deleted

// =============================================
// BULLETPROOF BATTLE REAL-TIME SYSTEM
// =============================================

// Battle real-time subscription management
let battleSubscriptions: Map<string, any> = new Map()
let battlePollingIntervals: Map<string, NodeJS.Timeout> = new Map()
let isBattleSystemActive = false

// Enhanced battle subscription with bulletproof features
export const setupBulletproofBattleSubscription = (battleId: string, onUpdate: (battleData: any) => void) => {
  if (!battleId || typeof onUpdate !== 'function') {
    console.error('❌ Invalid parameters for bulletproof battle subscription')
    return
  }
  
  console.log('🛡️ Setting up bulletproof battle subscription for:', battleId)
  
  // Clean up existing subscription for this battle
  cleanupBattleSubscription(battleId)
  
  isBattleSystemActive = true
  
  // Create enhanced subscription with heartbeat and recovery
  const battleChannel = supabase
    .channel(`bulletproof_battle_${battleId}`)
    .on(
      'postgres_changes',
      { 
        event: 'UPDATE', 
        schema: 'public', 
        table: 'battle_sessions', 
        filter: `id=eq.${battleId}` 
      },
      (payload) => {
        console.log('🔄 Real-time battle update received:', payload.new)
        if (payload.new) {
          onUpdate(payload.new)
        }
      }
    )
    .subscribe((status, err) => {
      console.log('📡 Bulletproof battle subscription status:', status, battleId)
      
      if (err) {
        console.error('❌ Battle subscription error:', err)
        
        // Handle binding mismatch errors
        if (err.message && err.message.includes('mismatch between server and client bindings')) {
          console.log('🔧 Battle binding mismatch, forcing reconnect...')
          setTimeout(() => {
            if (isBattleSystemActive) {
              setupBulletproofBattleSubscription(battleId, onUpdate)
            }
          }, 3000)
          return
        }
      }
      
      if (status === 'SUBSCRIBED') {
        console.log('✅ Bulletproof battle subscription active!')
        
        // Set up battle heartbeat to keep connection alive
        const heartbeat = setInterval(() => {
          console.log('💓 Battle heartbeat for:', battleId)
          
          // Perform lightweight check to ensure subscription is alive
          if (!document.hidden) {
            checkBattleConnectionHealth(battleId)
          }
        }, 30000) // Heartbeat every 30 seconds
        
        // Store heartbeat reference
        if (battleChannel) {
          (battleChannel as any).heartbeat = heartbeat
        }
        
        // Start background polling as backup
        startBattleBackgroundPolling(battleId, onUpdate)
        
      } else if (status === 'CHANNEL_ERROR' || status === 'CLOSED' || status === 'TIMED_OUT') {
        console.log('🔄 Battle subscription issue, attempting recovery...')
        
        // Exponential backoff for reconnection
        const retryDelay = status === 'TIMED_OUT' ? 2000 : 5000
        setTimeout(() => {
          if (isBattleSystemActive) {
            setupBulletproofBattleSubscription(battleId, onUpdate)
          }
        }, retryDelay)
      }
    })
  
  // Store subscription with onUpdate callback for reconnections
  (battleChannel as any).onUpdate = onUpdate
  battleSubscriptions.set(battleId, battleChannel)
  
  // Set up Page Visibility API for immediate reconnect when tab becomes visible
  setupBattleVisibilityHandling(battleId, onUpdate)
}

// Background polling as backup for real-time updates
const startBattleBackgroundPolling = (battleId: string, onUpdate: (battleData: any) => void) => {
  // Clear existing polling for this battle
  const existingInterval = battlePollingIntervals.get(battleId)
  if (existingInterval) {
    clearInterval(existingInterval)
  }
  
  // Start background polling (less frequent than matchmaking)
  const pollingInterval = setInterval(async () => {
    try {
      // Only poll if tab is hidden or connection seems unhealthy
      if (document.hidden || !isConnectionHealthy(battleId)) {
        const { data: battleData, error } = await supabase
          .from('battle_sessions')
          .select('*')
          .eq('id', battleId)
          .single()
        
        if (!error && battleData) {
          console.log('🔄 Background battle polling update for:', battleId)
          onUpdate(battleData)
        }
      }
    } catch (error) {
      // Silent polling errors to avoid spam
      console.log('Background battle polling error (non-critical):', error)
    }
  }, 5000) // Poll every 5 seconds when needed
  
  battlePollingIntervals.set(battleId, pollingInterval)
}

// Check battle connection health
const checkBattleConnectionHealth = async (battleId: string) => {
  try {
    // Simple health check - try to fetch battle data
    const { data, error } = await supabase
      .from('battle_sessions')
      .select('id')
      .eq('id', battleId)
      .single()
    
    return !error && data
  } catch {
    return false
  }
}

// Check if connection is healthy for a battle
const isConnectionHealthy = (battleId: string): boolean => {
  const subscription = battleSubscriptions.get(battleId)
  if (!subscription) return false
  
  // Check if subscription is in a good state
  const channel = subscription._topic || subscription.topic
  return channel && subscription.state === 'joined'
}

// Page Visibility API handling for battles
const setupBattleVisibilityHandling = (battleId: string, onUpdate: (battleData: any) => void) => {
  const handleBattleVisibilityChange = async () => {
    if (!document.hidden && isBattleSystemActive) {
      console.log('👁️ Tab visible - checking battle connection health for:', battleId)
      
      const isHealthy = await checkBattleConnectionHealth(battleId)
      if (!isHealthy) {
        console.log('🔄 Battle connection unhealthy, forcing reconnect...')
        // Avoid circular dependency - just clean up and let the system reconnect naturally
        cleanupBattleSubscription(battleId)
        setTimeout(() => {
          setupBulletproofBattleSubscription(battleId, onUpdate)
        }, 1000)
      } else {
        // Force refresh battle state when tab becomes visible
        try {
          const { data: battleData, error } = await supabase
            .from('battle_sessions')
            .select('*')
            .eq('id', battleId)
            .single()
          
          if (!error && battleData) {
            console.log('🔄 Visibility refresh for battle:', battleId)
            onUpdate(battleData)
          }
        } catch (error) {
          console.log('Visibility refresh error:', error)
        }
      }
    }
  }
  
  // Remove existing listener if any
  document.removeEventListener('visibilitychange', handleBattleVisibilityChange)
  document.addEventListener('visibilitychange', handleBattleVisibilityChange)
  
  // Also handle window focus/blur for additional reliability
  const handleWindowFocus = () => {
    if (isBattleSystemActive) {
      handleBattleVisibilityChange()
    }
  }
  
  window.removeEventListener('focus', handleWindowFocus)
  window.addEventListener('focus', handleWindowFocus)
}

// Clean up battle subscription
export const cleanupBattleSubscription = (battleId: string) => {
  if (!battleId) return
  
  console.log('🧹 Cleaning up battle subscription for:', battleId)
  
  try {
    // Clean up real-time subscription
    const subscription = battleSubscriptions.get(battleId)
    if (subscription) {
      // Clean up heartbeat
      if ((subscription as any).heartbeat) {
        clearInterval((subscription as any).heartbeat)
      }
      
      supabase.removeChannel(subscription)
      battleSubscriptions.delete(battleId)
    }
    
    // Clean up background polling
    const pollingInterval = battlePollingIntervals.get(battleId)
    if (pollingInterval) {
      clearInterval(pollingInterval)
      battlePollingIntervals.delete(battleId)
    }
  } catch (error) {
    console.error('Error during battle subscription cleanup:', error)
  }
}

// Clean up all battle subscriptions
export const cleanupAllBattleSubscriptions = () => {
  console.log('🧹 Cleaning up all battle subscriptions')
  isBattleSystemActive = false
  
  // Clean up all subscriptions
  for (const [battleId] of battleSubscriptions) {
    cleanupBattleSubscription(battleId)
  }
  
  // Clean up event listeners
  document.removeEventListener('visibilitychange', () => {})
  window.removeEventListener('focus', () => {})
}

// =============================================
// REAL-TIME MONITORING & HEALTH CHECKS
// =============================================

// Get real-time system status for debugging
export const getRealTimeSystemStatus = () => {
  const matchmakingStatus = {
    isActive: isMatchmakingActive,
    hasSubscription: !!matchmakingSubscription,
    subscriptionState: matchmakingSubscription?.state || 'unknown'
  }
  
  const battleStatus = {
    activeBattles: battleSubscriptions.size,
    isSystemActive: isBattleSystemActive,
    battleIds: Array.from(battleSubscriptions.keys())
  }
  
  return {
    matchmaking: matchmakingStatus,
    battles: battleStatus,
    timestamp: new Date().toISOString()
  }
}

// Force refresh all real-time connections
export const forceRefreshAllRealTimeConnections = () => {
  console.log('🔄 Force refreshing all real-time connections')
  
  // Refresh matchmaking
  if (isMatchmakingActive) {
    stopMatchmakingListener()
    setTimeout(() => {
      startMatchmakingListener()
    }, 1000)
  }
  
  // Refresh battle subscriptions (avoid direct calls during iteration)
  const battleIds = Array.from(battleSubscriptions.keys())
  for (const battleId of battleIds) {
    const subscription = battleSubscriptions.get(battleId)
    if (subscription && (subscription as any).onUpdate) {
      const onUpdate = (subscription as any).onUpdate
      cleanupBattleSubscription(battleId)
      setTimeout(() => {
        setupBulletproofBattleSubscription(battleId, onUpdate)
      }, 1000)
    }
  }
}

// Test real-time connectivity
export const testRealTimeConnectivity = async () => {
  console.log('🧪 Testing real-time connectivity...')
  
  try {
    // Test basic Supabase connection
    const { data: { session } } = await supabase.auth.getSession()
    if (!session) {
      return { success: false, error: 'No authentication session' }
    }
    
    // Test database connectivity
    const { data, error } = await supabase
      .from('battle_sessions')
      .select('id')
      .limit(1)
    
    if (error) {
      return { success: false, error: 'Database connectivity failed: ' + error.message }
    }
    
    // Test real-time subscription creation
    const testChannel = supabase
      .channel('connectivity-test-' + Date.now())
      .subscribe((status) => {
        console.log('🧪 Test subscription status:', status)
      })
    
    // Clean up test subscription after 5 seconds
    setTimeout(() => {
      supabase.removeChannel(testChannel)
    }, 5000)
    
    return { 
      success: true, 
      message: 'Real-time connectivity test completed',
      systemStatus: getRealTimeSystemStatus()
    }
  } catch (error) {
    return { success: false, error: 'Connectivity test failed: ' + error.message }
  }
}

// Force reset user's matchmaking state (emergency function)
export const forceResetMatchmakingState = async (): Promise<{ success: boolean; error?: string }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) {
      return { success: false, error: 'User not authenticated' }
    }

    console.log('🔄 Force resetting matchmaking state for user:', user.id)

    // Call the database function to reset state
    const { error } = await supabase.rpc('reset_user_matchmaking_state', {
      target_user_id: user.id
    })

    if (error) {
      console.error('Error resetting matchmaking state:', error)
      return { success: false, error: error.message }
    }

    // Also stop any local matchmaking
    stopMatchmakingListener()
    stopPeriodicMatchmaking()

    console.log('✅ Matchmaking state reset successfully')
    toast.success('🔄 Matchmaking state reset')
    return { success: true }
  } catch (error) {
    console.error('Unexpected error resetting matchmaking state:', error)
    return { success: false, error: 'An unexpected error occurred' }
  }
}

// Test real-time notifications
export const testRealtimeNotifications = async () => {
  try {
    console.log('🧪 Testing real-time notifications...')
    
    const { data: { session } } = await supabase.auth.getSession()
    if (!session) {
      console.error('❌ No auth session for test')
      return
    }

    // First, manually subscribe to test the subscription
    console.log('📡 Setting up test real-time subscription...')
    const testChannel = supabase
      .channel('test-realtime-' + Date.now())
      .on('postgres_changes', 
        { 
          event: 'INSERT', 
          schema: 'public', 
          table: 'battle_sessions' 
        }, 
        (payload) => {
          console.log('🎮 TEST - Real-time battle session INSERT detected:', payload)
          toast.success('🎮 Real-time notification received!')
        }
      )
      .subscribe((status) => {
        console.log('📡 Test subscription status:', status)
        if (status === 'SUBSCRIBED') {
          console.log('✅ Test real-time subscription is ready!')
          toast.success('📡 Test subscription ready')
        }
      })

    // Wait a moment for subscription to be ready
    console.log('⏳ Waiting for test subscription to be ready...')
    await new Promise(resolve => setTimeout(resolve, 2000))

    // Call the safe database function to create a test battle
    console.log('🔥 Creating safe test battle directly in database...')
    const { data: testResult, error: testError } = await supabase
      .rpc('test_realtime_battle_creation_safe')

    if (testError) {
      console.error('❌ Error creating safe test battle:', testError)
      toast.error('❌ Test battle creation failed: ' + testError.message)
    } else {
      console.log('✅ Safe test battle created:', testResult)
      toast.success('🔥 Safe test battle created - check for real-time notification!')
    }

    // Clean up test subscription after 15 seconds
    setTimeout(async () => {
      console.log('🧹 Cleaning up test subscription and battles')
      supabase.removeChannel(testChannel)
      
      // Clean up test battles
      try {
        const { data: cleanupResult } = await supabase.rpc('cleanup_test_battles_safe')
        console.log('🧹 Cleaned up safe test battles:', cleanupResult)
      } catch (cleanupError) {
        console.error('⚠️ Error cleaning up test battles:', cleanupError)
      }
    }, 15000)

    return testResult
  } catch (error) {
    console.error('❌ Error testing real-time notifications:', error)
    return null
  }
}

// Test if current user would receive real-time for their actual user ID
export const testRealtimeWithCurrentUser = async () => {
  try {
    console.log('🧪 Testing real-time with current user ID...')
    
    const { data: { user } } = await supabase.auth.getUser()
    if (!user) {
      console.error('❌ No authenticated user')
      return
    }

    console.log('👤 Current user ID:', user.id)

    // Subscribe to battle_sessions for current user
    const userTestChannel = supabase
      .channel('user-test-' + Date.now())
      .on('postgres_changes', 
        { 
          event: 'INSERT', 
          schema: 'public', 
          table: 'battle_sessions' 
        }, 
        (payload) => {
          console.log('🎮 USER TEST - Real-time battle session INSERT:', payload)
          const battle = payload.new as any
          if (battle.player1_id === user.id || battle.player2_id === user.id) {
            console.log('✅ Real-time notification for current user!')
            toast.success('🎮 Real-time works for your user!')
          } else {
            console.log('ℹ️ Real-time for other users')
          }
        }
      )
      .subscribe((status) => {
        console.log('📡 User test subscription status:', status)
      })

    await new Promise(resolve => setTimeout(resolve, 2000))

    // Create a test battle with current user
    const { data: userTestResult, error: userTestError } = await supabase
      .rpc('test_realtime_battle_creation_safe')

    if (userTestError) {
      console.error('❌ Error creating user test battle:', userTestError)
    } else {
      console.log('✅ User test battle created:', userTestResult)
      toast.success('🔥 User test battle created!')
    }

    // Clean up after 15 seconds
    setTimeout(() => {
      console.log('🧹 Cleaning up user test')
      supabase.removeChannel(userTestChannel)
    }, 15000)

    return userTestResult
  } catch (error) {
    console.error('❌ Error testing real-time with current user:', error)
    return null
  }
}

// Call the server-side matchmaker edge function
const callServerMatchmaker = async () => {
  try {

    
    const { data: { session } } = await supabase.auth.getSession()
    if (!session) {
      console.error('❌ No auth session for matchmaker call')
      return
    }

    const response = await supabase.functions.invoke('matchmaker', {
      headers: {
        Authorization: `Bearer ${session.access_token}`,
      },
    })

    if (response.error) {
      console.error('❌ Matchmaker error:', response.error)
      return
    }


    
    // If a battle was created, the real-time listener will handle it
    if (response.data?.success && response.data?.battle) {
      console.log('🎯 Battle created by server matchmaker:', response.data.battle.id)
    }
  } catch (error) {
    console.error('❌ Error calling server matchmaker:', error)
  }
}

// Get current battle session for user
export const getCurrentBattle = async (): Promise<{ battle: BattleSession | null; error?: string }> => {
  try {
    const { data: { user }, error: authError } = await supabase.auth.getUser()
    if (authError || !user) {
      return { battle: null, error: 'User not authenticated' }
    }

    const { data: battles, error } = await supabase
      .from('battle_sessions')
      .select('*')
      .or(`player1_id.eq.${user.id},player2_id.eq.${user.id}`)
      .in('status', ['deploying', 'battling'])
      .order('started_at', { ascending: false })
      .limit(1)

    if (error) {
      console.error('Error getting current battle:', error)
      return { battle: null, error: error.message }
    }

    return { battle: battles?.[0] || null }
  } catch (error) {
    console.error('Unexpected error getting current battle:', error)
    return { battle: null, error: 'An unexpected error occurred' }
  }
}

// Initialize battle with decks and set first player (SECURE VERSION)
export const initializeBattle = async (battleId: string): Promise<{ success: boolean; error?: string }> => {
  try {
    const { data: { session }, error: authError } = await supabase.auth.getSession()
    if (authError || !session) {
      return { success: false, error: 'User not authenticated' }
    }

    // Call secure edge function
    const { data, error } = await supabase.functions.invoke('battle-initialize-v2', {
      body: { battleId }
    })

    if (error) {
      console.error('Initialize battle error:', error)
      return { success: false, error: error.message }
    }

    if (!data.success) {
      return { success: false, error: data.error }
    }

    return { success: true }
  } catch (error) {
    console.error('Error initializing battle:', error)
    return { success: false, error: 'Failed to initialize battle' }
  }
}

// Deploy a card to the battlefield (SECURE VERSION)
export const deployCard = async (
  battleId: string,
  cardId: string,
  targetId?: string
): Promise<{ success: boolean; error?: string }> => {
  try {
    console.log('🎴 deployCard called with:', { battleId, cardId })
    
    const { data: { session }, error: authError } = await supabase.auth.getSession()
    if (authError || !session) {
      console.error('❌ Auth error in deployCard:', authError)
      return { success: false, error: 'User not authenticated' }
    }

    console.log('✅ User authenticated, calling battle-deploy-card edge function')

    // Call secure edge function
    const { data, error } = await supabase.functions.invoke('battle-deploy-card', {
      body: { battleId, cardId, targetId }
    })

    console.log('📡 Edge function response:', { data, error })

    if (error) {
      console.error('❌ Deploy card edge function error:', error)
      console.error('❌ Error details:', {
        message: error.message,
        details: error.details,
        hint: error.hint,
        code: error.code
      })
      return { success: false, error: error.message }
    }

    if (!data.success) {
      console.error('❌ Deploy card failed:', data.error)
      return { success: false, error: data.error }
    }

    console.log('✅ Deploy card successful')
    return { success: true }
  } catch (error) {
    console.error('❌ Error deploying card:', error)
    return { success: false, error: 'Failed to deploy card' }
  }
}

// Test function to check if edge functions are updated
export const testEdgeFunctionVersion = async (): Promise<{ version?: string; error?: string }> => {
  try {
    console.log('🧪 Testing edge function version...')
    
    const { data, error } = await supabase.functions.invoke('battle-deploy-card', {
      body: { test: true }
    })

    console.log('🧪 Test response:', { data, error })

    if (error) {
      return { error: error.message }
    }

    return { version: data?.version || 'unknown' }
  } catch (error) {
    console.error('🧪 Test error:', error)
    return { error: 'Test failed' }
  }
}

// Execute an attack between cards (SECURE VERSION)
export const executeAttack = async (battleId: string): Promise<{ success: boolean; error?: string }> => {
  try {
    const { data: { session }, error: authError } = await supabase.auth.getSession()
    if (authError || !session) {
      return { success: false, error: 'User not authenticated' }
    }

    // Call secure edge function
    const { data, error } = await supabase.functions.invoke('battle-execute-attack', {
      body: { battleId }
    })

    if (error) {
      console.error('Execute attack error:', error)
      return { success: false, error: error.message }
    }

    if (!data.success) {
      return { success: false, error: data.error }
    }

    return { success: true }
  } catch (error) {
    console.error('Error executing attack:', error)
    return { success: false, error: 'Failed to execute attack' }
  }
}

// Clean up broken battles (SECURE VERSION)
export const cleanupBrokenBattles = async (): Promise<{ success: boolean; cleanedBattles?: number; error?: string }> => {
  try {
    const { data: { session }, error: authError } = await supabase.auth.getSession()
    if (authError || !session) {
      return { success: false, error: 'User not authenticated' }
    }

    // Call secure edge function
    const { data, error } = await supabase.functions.invoke('battle-cleanup-broken', {
      body: {}
    })

    if (error) {
      console.error('Cleanup battle error:', error)
      return { success: false, error: error.message }
    }

    if (!data.success) {
      return { success: false, error: data.error }
    }

    return { success: true, cleanedBattles: data.cleanedBattles }
  } catch (error) {
    console.error('Error cleaning up battles:', error)
    return { success: false, error: 'Failed to cleanup battles' }
  }
}

// Surrender battle (SECURE VERSION)
export const surrenderBattle = async (battleId: string): Promise<{ success: boolean; error?: string }> => {
  try {
    const { data: { session }, error: authError } = await supabase.auth.getSession()
    if (authError || !session) {
      return { success: false, error: 'User not authenticated' }
    }

    // Call secure edge function
    const { data, error } = await supabase.functions.invoke('battle-surrender', {
      body: { battleId }
    })

    if (error) {
      console.error('Surrender battle error:', error)
      return { success: false, error: error.message }
    }

    if (!data.success) {
      return { success: false, error: data.error }
    }

    return { success: true }
  } catch (error) {
    console.error('Error surrendering battle:', error)
    return { success: false, error: 'Failed to surrender battle' }
  }
}

// Helper function to get rarity numeric value for comparison
const getRarityValue = (rarity: string): number => {
  const rarityValues = {
    'common': 1,
    'uncommon': 2,
    'rare': 3,
    'legendary': 4,
    'exotic': 5
  }
  return rarityValues[rarity] || 1
}

// Battle actions are handled server-side only via edge functions

// Auto-matchmaking function (runs on server/edge function)
export const tryAutoMatch = async (): Promise<{ success: boolean; matchCreated?: boolean; error?: string }> => {
  try {
    // Get players waiting in queue (2 or more)
    const { data: waitingPlayers, error: queueError } = await supabase
      .from('matchmaking_queue')
      .select('user_id, joined_at')
      .eq('status', 'searching')
      .order('joined_at')
      .limit(2)

    if (queueError) {
      return { success: false, error: queueError.message }
    }

    if (!waitingPlayers || waitingPlayers.length < 2) {
      return { success: true, matchCreated: false }
    }

    const [player1, player2] = waitingPlayers

    // Create battle session
    const { data: battleSession, error: battleError } = await supabase
      .from('battle_sessions')
      .insert({
        player1_id: player1.user_id,
        player2_id: player2.user_id,
        game_state: {
          player1_deck: [],
          player2_deck: [],
          player1_hand: [],
          player2_hand: [],
          battlefield: {},
          phase: 'waiting_first_deploy',
          turn_count: 0,
          round_count: 0,
          actions: [],
          first_player_id: player1.user_id,
          current_attacker_id: player1.user_id,
          last_round_winner_id: null,
          waiting_for_deploy_player_id: null
        },
        status: 'deploying'
      })
      .select()
      .single()

    if (battleError) {
      return { success: false, error: battleError.message }
    }

    // Update queue status to matched
    await supabase
      .from('matchmaking_queue')
      .update({ status: 'matched' })
      .in('user_id', [player1.user_id, player2.user_id])

    return { success: true, matchCreated: true }
  } catch (error) {
    console.error('Error in auto-match:', error)
    return { success: false, error: 'Auto-match failed' }
  }
}

// Check for active battle sessions when user loads the app
export const checkForActiveBattle = async (): Promise<{ activeBattle: BattleSession | null; error?: string }> => {
  try {
    const { data: { session } } = await supabase.auth.getSession()
    if (!session) {
      return { activeBattle: null, error: 'User not authenticated' }
    }



    const response = await supabase.functions.invoke('get-active-battle', {
      headers: {
        Authorization: `Bearer ${session.access_token}`,
      },
    })

    if (response.error) {
      console.error('❌ Edge function error:', response.error)
      return { activeBattle: null, error: response.error.message }
    }

    const result = response.data
    if (result.success && result.activeBattle) {

      return { activeBattle: result.activeBattle as BattleSession }
    }


    return { activeBattle: null }
  } catch (error) {
    console.error('❌ Unexpected error checking for active battles:', error)
    return { activeBattle: null, error: 'An unexpected error occurred' }
  }
}

// All battle logic moved to server-side edge functions only
// No client-side fallbacks or battle logic allowed