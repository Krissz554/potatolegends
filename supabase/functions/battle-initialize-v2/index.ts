import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2'

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': 'authorization, x-client-info, apikey, content-type',
}

async function getUserActiveHero(supabaseClient: any, userId: string) {
  try {
    // Use the RPC function to get user's active hero
    const { data, error } = await supabaseClient
      .rpc('get_user_active_hero', { user_uuid: userId })
    
    if (error) {
      console.error('Error fetching user hero:', error)
      // Don't throw yet, try to give them the default hero
    }
    
    if (!data || data.length === 0) {
      console.log(`⚠️ No active hero found for user ${userId}, giving them Potato King...`)
      
      // Give the user the default Potato King hero
      try {
        const { error: giveHeroError } = await supabaseClient
          .rpc('give_potato_king_to_user', { user_uuid: userId })
        
        if (giveHeroError) {
          console.error('Error giving Potato King to user:', giveHeroError)
          throw new Error(`Failed to assign default hero to user ${userId}: ${giveHeroError.message}`)
        }
        
        console.log(`✅ Gave Potato King to user ${userId}`)
        
        // Now try to get the hero again
        const { data: heroData, error: heroError } = await supabaseClient
          .rpc('get_user_active_hero', { user_uuid: userId })
        
        if (heroError || !heroData || heroData.length === 0) {
          console.error('Still no hero after giving Potato King:', heroError)
          throw new Error(`Failed to get hero for user ${userId} even after giving default hero`)
        }
        
        const hero = heroData[0]
        console.log(`🦸 User ${userId} now has default hero:`, {
          name: hero.name,
          hp: hero.base_hp,
          mana: hero.base_mana,
          power: hero.hero_power_name
        })
        
        return hero
      } catch (giveHeroError) {
        console.error('Error giving default hero:', giveHeroError)
        throw new Error(`User ${userId} has no active hero and failed to assign default hero: ${giveHeroError.message}`)
      }
    }
    
    const hero = data[0]
    console.log(`🦸 User ${userId} active hero:`, {
      name: hero.name,
      hp: hero.base_hp,
      mana: hero.base_mana,
      power: hero.hero_power_name
    })
    
    return hero
  } catch (error) {
    console.error('getUserActiveHero error:', error)
    throw error
  }
}

serve(async (req) => {
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    console.log('🎮 BATTLE-INITIALIZE-V2 FUNCTION CALLED - 30-CARD DECK SYSTEM')
    
    // Create Supabase client
    const supabaseClient = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? '',
      {
        auth: {
          autoRefreshToken: false,
          persistSession: false
        }
      }
    )

    // Get user from auth header
    const authHeader = req.headers.get('authorization')
    if (!authHeader) {
      throw new Error('No authorization header')
    }

    const { data: { user }, error: userError } = await supabaseClient.auth.getUser(authHeader.replace('Bearer ', ''))
    if (userError || !user) {
      throw new Error('Invalid user')
    }

    // Parse request
    const requestBody = await req.json()
    const { battleId } = requestBody

    if (!battleId) {
      throw new Error('Battle ID is required')
    }

    console.log(`🎯 Initializing battle: ${battleId}`)

    // Get battle session
    const { data: battle, error: battleError } = await supabaseClient
      .from('battle_sessions')
      .select('*')
      .eq('id', battleId)
      .single()

    if (battleError || !battle) {
      throw new Error('Battle session not found')
    }

    // Verify user is part of this battle
    if (battle.player1_id !== user.id && battle.player2_id !== user.id) {
      throw new Error('User not part of this battle')
    }

    console.log(`🔍 Loading 30-card decks for players...`)

    // Load both players' active decks
    const [player1Deck, player2Deck] = await Promise.all([
      loadPlayerDeck(supabaseClient, battle.player1_id),
      loadPlayerDeck(supabaseClient, battle.player2_id)
    ])

    console.log(`✅ Loaded decks - Player1: ${player1Deck.length} cards, Player2: ${player2Deck.length} cards`)

    // Validate deck sizes
    if (player1Deck.length !== 30 || player2Deck.length !== 30) {
      throw new Error(`Invalid deck sizes. Both players need exactly 30 cards. Player1: ${player1Deck.length}, Player2: ${player2Deck.length}`)
    }

    // Generate RNG seed for deterministic gameplay
    const rngSeed = Math.floor(Math.random() * 1000000)

    // Shuffle both decks deterministically
    const shuffledPlayer1Deck = shuffleDeck([...player1Deck], rngSeed + 1)
    const shuffledPlayer2Deck = shuffleDeck([...player2Deck], rngSeed + 2)

    // Determine first player (player who initiated battle goes first)
    const firstPlayerId = battle.player1_id

    // Mulligan phase: both players get 4 cards to decide what to keep
    const player1HandSize = 4
    const player2HandSize = 4

    console.log(`🎯 First player: ${firstPlayerId}`)
    console.log(`🃏 Hand sizes - Player1: ${player1HandSize}, Player2: ${player2HandSize}`)

    // Fetch hero data for both players
    console.log('🦸 Fetching hero data for both players...')
    const [player1Hero, player2Hero] = await Promise.all([
      getUserActiveHero(supabaseClient, battle.player1_id),
      getUserActiveHero(supabaseClient, battle.player2_id)
    ])

    console.log('✅ Hero data loaded:', {
      player1: { name: player1Hero.name, hp: player1Hero.base_hp },
      player2: { name: player2Hero.name, hp: player2Hero.base_hp }
    })

    // Create initial game state for 30-card format
    const initialGameState = {
      // Deck management
      player1_deck: shuffledPlayer1Deck.slice(player1HandSize), // Remaining deck after drawing hand
      player2_deck: shuffledPlayer2Deck.slice(player2HandSize),
      
      // Hands (grouped for client compatibility)
      hands: {
        player1_hand: shuffledPlayer1Deck.slice(0, player1HandSize), // Starting hand based on turn order
        player2_hand: shuffledPlayer2Deck.slice(0, player2HandSize)
      },
      
      // Hero stats (dynamic from database)
      player1_hero: {
        id: player1Hero.hero_id,
        name: player1Hero.name,
        hp: player1Hero.base_hp,
        max_hp: player1Hero.base_hp,
        mana: 1, // Start with 1 mana for first turn
        max_mana: 1, // Start with 1 max mana
        fatigue_damage: 0, // Tracks fatigue damage for empty deck
        hero_power_name: player1Hero.hero_power_name,
        hero_power_description: player1Hero.hero_power_description,
        hero_power_cost: player1Hero.hero_power_cost,
        hero_power_used_this_turn: false,
        hero_power_available: true,
        hero_power_cooldown: 0
      },
      player2_hero: {
        id: player2Hero.hero_id,
        name: player2Hero.name,
        hp: player2Hero.base_hp,
        max_hp: player2Hero.base_hp,
        mana: 1, // Start with 1 mana for first turn
        max_mana: 1, // Start with 1 max mana
        fatigue_damage: 0,
        hero_power_name: player2Hero.hero_power_name,
        hero_power_description: player2Hero.hero_power_description,
        hero_power_cost: player2Hero.hero_power_cost,
        hero_power_used_this_turn: false,
        hero_power_available: true,
        hero_power_cooldown: 0
      },
      
      // Battlefield
      battlefield: {
        player1_units: [], // Array of units on battlefield
        player2_units: [],
        player1_structures: [], // Structures/enchantments
        player2_structures: [],
        player1_slots: Array(6).fill(null), // 6 deployment slots for player 1
        player2_slots: Array(6).fill(null)  // 6 deployment slots for player 2
      },
      
      // Graveyards
      player1_graveyard: [],
      player2_graveyard: [],
      
      // Player identification (CRITICAL for hand assignment)
      player1_id: battle.player1_id,
      player2_id: battle.player2_id,
      
      // Game flow
      phase: 'mulligan', // mulligan → start_turn → main_phase → end_turn → game_over
      turn_phase: null, // Will be 'start', 'main', or 'end' during active gameplay
      turn_count: 1,
      turn_number: 1,
      first_player_id: firstPlayerId,
      current_turn_player_id: null, // No turns during mulligan phase
      turn_time_remaining: null, // No timer during mulligan
      turn_started_at: null,
      
      // Game state tracking
      game_status: 'active', // active, finished, draw
      winner_id: null,
      actions: [], // Action history
      rng_seed: rngSeed, // For deterministic randomness
      
      // Mulligan tracking
      mulligan_completed: {
        [battle.player1_id]: false,
        [battle.player2_id]: false
      }
    }

    console.log(`🎮 Game initialized with 30-card format`)
    console.log(`🃏 Total cards in play: ${player1Deck.length + player2Deck.length}`)

    // Update battle session with initial game state - immediately start mulligan
    const { error: updateError } = await supabaseClient
      .from('battle_sessions')
      .update({
        game_state: initialGameState,
        status: 'active', // Battle is active (in mulligan phase)
        current_turn_player_id: null, // No current turn during mulligan
        updated_at: new Date().toISOString()
      })
      .eq('id', battleId)

    if (updateError) {
      console.error('❌ Database update failed:', updateError)
      throw new Error(`Failed to initialize battle: ${updateError.message}`)
    }

    // Publish to Ably for real-time updates
    try {
      const ablyResponse = await fetch('https://rest.ably.io/channels/match:' + battleId + '/messages', {
        method: 'POST',
        headers: {
          'Authorization': 'Basic ' + btoa('1Vf05w.bzqjGQ:6YuVnRKLTCKDlHR-tDzDZPGfA5pBP57vWP4nFRrFXm0'),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'battle_initialized',
          data: {
            type: 'initialization',
            battleId,
            status: 'active',
            phase: 'mulligan',
            timestamp: Date.now()
          }
        })
      })

      if (!ablyResponse.ok) {
        console.error('❌ Failed to publish battle initialization to Ably:', ablyResponse.status)
      } else {
        console.log('✅ Published battle initialization to Ably')
      }
    } catch (ablyError) {
      console.error('❌ Ably publish error:', ablyError)
    }

    console.log('✅ Battle initialization successful!')

    return new Response(
      JSON.stringify({
        success: true,
        battleId: battleId,
        gameState: initialGameState,
        message: 'Battle initialized with 30-card decks'
      }),
      {
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 200,
      }
    )

  } catch (error) {
    console.error('Initialize battle error:', error)
    return new Response(
      JSON.stringify({
        success: false,
        error: error.message
      }),
      { 
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
        status: 400 
      }
    )
  }
})

// Load a player's active deck with 30 cards
async function loadPlayerDeck(supabase: any, playerId: string) {
  console.log(`📚 Loading active deck for player: ${playerId}`)

  // Get player's active deck
  const { data: activeDeck, error: deckError } = await supabase
    .from('decks')
    .select('id, name')
    .eq('user_id', playerId)
    .eq('is_active', true)
    .single()

  if (deckError || !activeDeck) {
    console.log(`❌ No active deck found for player ${playerId}`)
    throw new Error(`Player ${playerId} does not have an active deck. Players must build a valid 30-card deck before battling.`)
  }

  // Load deck cards
  return await loadDeckCards(supabase, activeDeck.id)
}

// Load cards from a specific deck
async function loadDeckCards(supabase: any, deckId: string) {
  console.log(`🃏 Loading cards for deck: ${deckId}`)

  const { data: deckCards, error: cardsError } = await supabase
    .from('deck_cards')
    .select(`
      quantity,
      card_id,
      card_complete(*)
    `)
    .eq('deck_id', deckId)

  if (cardsError) {
    console.error('❌ Deck cards query error:', cardsError)
    throw new Error(`Failed to load deck cards: ${cardsError.message}`)
  }

  console.log('📊 Raw deck cards response:', deckCards)
  console.log('📊 First deck card sample:', deckCards?.[0])

  if (!deckCards || deckCards.length === 0) {
    throw new Error('Deck is empty')
  }

  // Get all unique card IDs from the deck
  const cardIds = deckCards.map(dc => dc.card_id)
  console.log('🎯 Card IDs to fetch:', cardIds)

  // Fetch all card data separately to ensure we get complete information
  const { data: cardData, error: cardDataError } = await supabase
    .from('card_complete')
    .select('*')
    .in('id', cardIds)

  if (cardDataError) {
    console.error('❌ Card data query error:', cardDataError)
    throw new Error(`Failed to load card data: ${cardDataError.message}`)
  }

  console.log('🃏 Fetched card data:', cardData)
  console.log('🃏 First card data sample:', cardData?.[0])

  // Create a map for quick lookup
  const cardMap = new Map()
  cardData?.forEach(card => {
    cardMap.set(card.id, card)
  })

  // Expand cards based on quantity (e.g., if quantity is 3, add 3 copies)
  const expandedDeck = []
  for (const { quantity, card_id } of deckCards) {
    const cardInfo = cardMap.get(card_id)
    if (!cardInfo) {
      console.error(`❌ Card not found: ${card_id}`)
      continue
    }

    for (let i = 0; i < quantity; i++) {
      expandedDeck.push({
        // Flatten card structure for direct access in UI
        id: `${cardInfo.id}_${i}`, // Unique ID for each copy
        ...cardInfo, // Spread all card properties to root level
        original_id: cardInfo.id, // Keep reference to original card ID
        deck_position: expandedDeck.length + 1
      })
    }
  }

  console.log(`✅ Deck loaded: ${expandedDeck.length} total cards`)
  console.log('📋 First card sample:', expandedDeck[0])
  console.log('📋 Card properties check:', {
    hasName: expandedDeck[0]?.name,
    hasManaCost: expandedDeck[0]?.mana_cost,
    hasAttack: expandedDeck[0]?.attack,
    hasHp: expandedDeck[0]?.hp,
    hasPotato: expandedDeck[0]?.potato,
    allKeys: expandedDeck[0] ? Object.keys(expandedDeck[0]) : []
  })
  return expandedDeck
}

// Deterministic deck shuffling using seeded random
function shuffleDeck(deck: any[], seed: number) {
  const rng = seedRandom(seed)
  const shuffled = [...deck]
  
  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(rng() * (i + 1))
    ;[shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]]
  }
  
  return shuffled
}

// Seeded random number generator (Linear Congruential Generator)
function seedRandom(seed: number) {
  let state = seed
  return function() {
    state = (state * 1664525 + 1013904223) % 4294967296
    return state / 4294967296
  }
}