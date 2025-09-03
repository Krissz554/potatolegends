import React, { useState, useEffect } from 'react'
import { supabase } from '@/lib/supabase'
import { useAuth } from '@/contexts/AuthContext'

import { toast } from 'sonner'
import TradingCard from '@/components/ui/TradingCard'
import CardBack from '@/components/ui/CardBack'
import HeroDisplay from '@/components/ui/HeroDisplay'
import TurnIndicator from '@/components/ui/TurnIndicator'
import ElementalPixelPotato from '@/components/ElementalPixelPotato'
import { useTurnTimer } from '@/hooks/useTurnTimer'
import AblyService from '@/lib/ablyService'
import { HealthBar } from '@/components/ui/HealthBar'


// Arena background images
import arenaDay from '@/assets/pixel/arena/day/arena-day.png'
import arenaNight from '@/assets/pixel/arena/night/arena-night.png'

// Custom potato assets
import potato0001 from '@/assets/pixel-potatoes/potato_0001/potato_0001.png'

// Import stat icons for battlefield display
import atkIcon from '@/assets/pixel/icons/ATK-icon.png'

interface PixelArenaProps {
  battleSessionId: string
}

interface Hero {
  id: string
  hero_id: string
  name: string
  description: string
  base_hp: number
  base_mana: number
  hero_power_name: string
  hero_power_description: string
  hero_power_cost: number
  rarity: string
  element_type: string
}

interface GameState {
  hands?: {
    player1_hand?: any[]
    player2_hand?: any[]
  }
  player1_id?: string
  player2_id?: string
  current_turn_player_id?: string
  status?: string
  turn_number?: number
  turn_phase?: 'start' | 'main' | 'end'
  turn_time_remaining?: number
  turn_started_at?: string
  phase?: string
  player1_hero?: {
    hp: number
    max_hp: number
    mana: number
    max_mana: number
    hero_power_available: boolean
    hero_power_cooldown: number
    hero_power_used_this_turn?: boolean
    fatigue_damage?: number
  }
  player2_hero?: {
    hp: number
    max_hp: number
    mana: number
    max_mana: number
    hero_power_available: boolean
    hero_power_cooldown: number
    hero_power_used_this_turn?: boolean
    fatigue_damage?: number
  }
}

const PixelArena: React.FC<PixelArenaProps> = ({ battleSessionId }) => {
  const { user } = useAuth()
  const [isDaytime, setIsDaytime] = useState(true)
  const [isLoading, setIsLoading] = useState(true)
  const [isInitializing, setIsInitializing] = useState(false)
  const [gameState, setGameState] = useState<GameState | null>(null)
  const [battleSession, setBattleSession] = useState<any>(null)
  const [hoveredCardIndex, setHoveredCardIndex] = useState<number | null>(null)
  const [playerHero, setPlayerHero] = useState<Hero | null>(null)
  
  // Mulligan state
  const [selectedMulliganCards, setSelectedMulliganCards] = useState<string[]>([])
  const [opponentHero, setOpponentHero] = useState<Hero | null>(null)
  
  // Game menu states
  const [showGameMenu, setShowGameMenu] = useState(false)
  const [showSurrenderConfirm, setShowSurrenderConfirm] = useState(false)
  
  // Turn state (calculated in real-time from server timestamp)
  const [turnTimeRemaining, setTurnTimeRemaining] = useState<number>(60)
  
  // Drag and drop state (no game state stored client-side)
  const [draggedCard, setDraggedCard] = useState<any>(null)
  const [dragPosition, setDragPosition] = useState<{ x: number; y: number } | null>(null)
  const [isDragging, setIsDragging] = useState(false)
  
  // Mobile touch state
  const [touchStartPos, setTouchStartPos] = useState<{ x: number, y: number } | null>(null)
  const [isTouchDragging, setIsTouchDragging] = useState(false)
  const [selectedAttacker, setSelectedAttacker] = useState<{card: any, slotIndex: number} | null>(null)
  const [isAttackMode, setIsAttackMode] = useState(false)
  const [isHeroPowerMode, setIsHeroPowerMode] = useState(false)
  const [isTargetingMode, setIsTargetingMode] = useState(false)
  const [pendingTargetCard, setPendingTargetCard] = useState<any>(null)
  const [pendingTargetSlot, setPendingTargetSlot] = useState<number | null>(null)
  
  // Real-time connection status
  const [connectionStatus, setConnectionStatus] = useState<'connecting' | 'connected' | 'disconnected' | 'error'>('connecting')
  const [latency, setLatency] = useState<number>(0)
  const [lastSeenEventId, setLastSeenEventId] = useState<number>(0)
  const [isReconnecting, setIsReconnecting] = useState(false)
  const [lastUpdateTime, setLastUpdateTime] = useState<number>(Date.now())
  const [stateVersion, setStateVersion] = useState<number>(0)
  const [battlefieldVersion, setBattlefieldVersion] = useState<number>(0)
  const [ablyService, setAblyService] = useState<AblyService | null>(null)
  
  // Get deployed cards from server game state - 6 slots per player
  const getPlayerBattlefield = () => {
    if (!gameState || !user?.id) return Array(6).fill(null)
    const isPlayer1 = gameState.player1_id === user.id
    
    // Get battlefield slots from server state
    const battlefield = gameState.battlefield || {}
    const playerSlots = isPlayer1 ? battlefield.player1_slots : battlefield.player2_slots
    
    // Ensure we always return exactly 6 slots
    const slots = Array(6).fill(null)
    if (playerSlots) {
      for (let i = 0; i < 6; i++) {
        slots[i] = playerSlots[i] || null
      }
    }
    
    // Apply aura effects from structures and relics
    const structures = slots.filter(card => card && card.card_type === 'structure')
    const relics = slots.filter(card => card && card.card_type === 'relic')
    const auraProviders = [...structures, ...relics]
    
    const enhancedSlots = calculateAuraEffects(slots, auraProviders, isPlayer1 ? 'player1' : 'player2')
    
    // Only log occasionally to avoid performance issues

    
    return enhancedSlots
  }
  
  // Calculate aura bonuses for units based on active structures
  const calculateAuraEffects = (units: any[], structures: any[], playerKey: string) => {
    if (!units || !structures) return units
    
    const enhancedUnits = units.map(unit => {
      if (!unit) return unit
      
      // Start with base stats
      let atkBonus = 0
      let damageReduction = 0
      let hasLifestealBonus = false
      
      // Check each structure for aura effects
      structures.forEach(structure => {
        if (!structure || !structure.keywords) return
        
        const keywords = Array.isArray(structure.keywords) ? structure.keywords : 
          (typeof structure.keywords === 'string' ? JSON.parse(structure.keywords) : [])
        
        keywords.forEach((keyword: string) => {
          switch (keyword) {
            case 'Structure:BuffTribeFireAtk+1':
              if (unit.potato_type === 'fire') atkBonus += 1
              break
            case 'Structure:BuffTribeLightningAtk+1':
              if (unit.potato_type === 'lightning') atkBonus += 1
              break
            case 'Structure:BuffHealersAtk+1':
              if (unit.unit_class === 'healer') atkBonus += 1
              break
            case 'Structure:DamageReductionAllies-1':
              damageReduction += 1
              break
            case 'Structure:BuffLightningUnitsAtk+1':
              if (unit.potato_type === 'lightning') atkBonus += 1
              break
            case 'Relic:GrantLifestealAllies':
              hasLifestealBonus = true
              break
          }
        })
      })
      
      // Apply aura bonuses
      return {
        ...unit,
        aura_attack_bonus: atkBonus,
        aura_damage_reduction: damageReduction,
        aura_lifesteal: hasLifestealBonus || unit.has_lifesteal,
        effective_attack: (unit.attack || 0) + atkBonus
      }
    })
    
    return enhancedUnits
  }
  
  const getOpponentBattlefield = () => {
    if (!gameState || !user?.id) return Array(6).fill(null)
    const isPlayer1 = gameState.player1_id === user.id
    
    // Get opponent's battlefield slots from server state
    const battlefield = gameState.battlefield || {}
    const opponentSlots = isPlayer1 ? battlefield.player2_slots : battlefield.player1_slots
    
    // Ensure we always return exactly 6 slots
    const slots = Array(6).fill(null)
    if (opponentSlots) {
      for (let i = 0; i < 6; i++) {
        slots[i] = opponentSlots[i] || null
      }
    }
    
    // Apply aura effects from opponent's structures and relics
    const structures = slots.filter(card => card && card.card_type === 'structure')
    const relics = slots.filter(card => card && card.card_type === 'relic')
    const auraProviders = [...structures, ...relics]
    
    const enhancedSlots = calculateAuraEffects(slots, auraProviders, isPlayer1 ? 'player2' : 'player1')
    
    return enhancedSlots
  }

  // Timer system info (no client-side logic)
  useTurnTimer()

  // Get current player's mana from existing hero state
  const getPlayerMana = () => {
    if (!gameState || !user?.id) return 0
    const isPlayer1 = gameState.player1_id === user.id
    const playerHero = isPlayer1 ? gameState.player1_hero : gameState.player2_hero
    return playerHero?.mana || 0
  }

  // Get current player's max mana
  const getPlayerMaxMana = () => {
    if (!gameState || !user?.id) return 0
    const isPlayer1 = gameState.player1_id === user.id
    const playerHero = isPlayer1 ? gameState.player1_hero : gameState.player2_hero
    return playerHero?.max_mana || 0
  }

  // Drag and drop handlers with enhanced visuals
  // Handle card click (for spells and other non-draggable cards)
  const handleCardClick = async (card: any, event?: React.MouseEvent) => {
    // Prevent event bubbling to avoid interference with other battle events
    if (event) {
      event.preventDefault()
      event.stopPropagation()
    }
    
    if (isSpellCard(card)) {
      // Only cast spells if it's the player's turn and they can play cards
      if (!canPlayCard(card)) {
        toast.error('Cannot cast spell right now')
        return
      }
      
      // Spells are cast immediately when clicked
      await handleSpellCast(card)
    }
    // Other card types will be handled here in the future (structures, relics)
  }

  const handleDragStart = (card: any, event: React.DragEvent) => {
    // Spells cannot be dragged - they are cast on click
    if (isSpellCard(card)) {
      event.preventDefault()
      toast.info(`${card.name} is a spell - click to cast!`)
      return
    }

    // Only allow dragging units and structures if it's player's turn and they have enough mana
    const isPlayerTurn = gameState?.current_turn_player_id === user?.id
    const hasEnoughMana = getPlayerMana() >= (card.mana_cost || 0)
    
    if (isPlayerTurn && hasEnoughMana) {
      setDraggedCard(card)
      setIsDragging(true)
      
      // Create a custom drag image (invisible, we'll use our own)
      const dragImage = new Image()
      dragImage.src = 'data:image/gif;base64,R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs=' // 1x1 transparent gif
      event.dataTransfer.setDragImage(dragImage, 0, 0)
      
      // Track initial position
      setDragPosition({ x: event.clientX, y: event.clientY })
    } else {
      event.preventDefault()
    }
  }

  const handleDragEnd = () => {
    setDraggedCard(null)
    setDragPosition(null)
    setIsDragging(false)
  }

  // Mobile touch handlers
  const handleTouchStart = (card: any, event: React.TouchEvent) => {
    // Only allow touch dragging if it's player's turn and they have enough mana
    const isPlayerTurn = gameState?.current_turn_player_id === user?.id
    const hasEnoughMana = getPlayerMana() >= (card.mana_cost || 0)
    
    if (!canPlayCard(card) || !isPlayerTurn || !hasEnoughMana) {
      return
    }

    const touch = event.touches[0]
    setTouchStartPos({ x: touch.clientX, y: touch.clientY })
    setDraggedCard(card)
    
    // Prevent default touch behavior
    event.preventDefault()
  }

  const handleTouchMove = (event: React.TouchEvent) => {
    if (!draggedCard || !touchStartPos) return

    const touch = event.touches[0]
    const deltaX = Math.abs(touch.clientX - touchStartPos.x)
    const deltaY = Math.abs(touch.clientY - touchStartPos.y)
    
    // Start dragging if moved more than 10px
    if (!isTouchDragging && (deltaX > 10 || deltaY > 10)) {
      setIsTouchDragging(true)
      setIsDragging(true)
    }

    if (isTouchDragging) {
      setDragPosition({ x: touch.clientX, y: touch.clientY })
    }

    event.preventDefault()
  }

  const handleTouchEnd = (event: React.TouchEvent) => {
    if (!draggedCard || !isTouchDragging) {
      // Reset state
      setDraggedCard(null)
      setTouchStartPos(null)
      setIsTouchDragging(false)
      setIsDragging(false)
      return
    }

    // Find the element under the touch point
    const touch = event.changedTouches[0]
    const elementBelow = document.elementFromPoint(touch.clientX, touch.clientY)
    
    // Find the closest battlefield slot
    const slotElement = elementBelow?.closest('[data-slot-index]')
    if (slotElement) {
      const slotIndex = parseInt(slotElement.getAttribute('data-slot-index') || '-1')
      const isPlayerSlot = slotElement.hasAttribute('data-player-slot')
      
      // Only allow dropping on player's own slots
      if (isPlayerSlot && slotIndex >= 0 && slotIndex <= 5) {
        handleSlotDrop(draggedCard, slotIndex)
      }
    }

    // Reset state
    setDraggedCard(null)
    setTouchStartPos(null)
    setIsTouchDragging(false)
    setIsDragging(false)
    setDragPosition(null)
    
    event.preventDefault()
  }

  // Track mouse position during drag
  const handleDragOver = (event: React.DragEvent) => {
    if (isDragging) {
      setDragPosition({ x: event.clientX, y: event.clientY })
    }
  }

  // Global touch move handler for smooth dragging
  const handleGlobalTouchMove = (event: TouchEvent) => {
    if (isTouchDragging && draggedCard) {
      const touch = event.touches[0]
      setDragPosition({ x: touch.clientX, y: touch.clientY })
      event.preventDefault()
    }
  }

  const handleSlotDrop = async (card: any, slotIndex: number, targetId?: string) => {
    if (!card || !gameState || !battleSessionId) return
    
    // Check if card needs targeting and we haven't provided one yet
    if (needsTargeting(card) && !targetId) {
      // Enter targeting mode
      setPendingTargetCard(card)
      setPendingTargetSlot(slotIndex)
      setIsTargetingMode(true)
      toast.info(`Select a target for ${card.name}'s ability`)
      return
    }
    
    try {

      
      // Force refresh battle state from server before deployment
      const { data: freshBattle, error: refreshError } = await supabase
        .from('battle_sessions')
        .select('*')
        .eq('id', battleSessionId)
        .single()
      
      if (refreshError) {

        toast.error('Failed to sync with server')
        return
      }
      

      
      // Check if server state matches client expectations - prioritize game_state field
      const serverCurrentTurn = freshBattle.game_state?.current_turn_player_id || freshBattle.current_turn_player_id
      if (serverCurrentTurn !== user?.id) {
        toast.error('Not your turn')
        return
      }
      
      // Call secure server-side edge function for card deployment with version control
      const idempotencyKey = `deploy_${battleSessionId}_${card.id}_${slotIndex}_${Date.now()}`
      

      const { data, error } = await supabase.functions.invoke('battle-deploy-card', {
        body: { 
          battleId: battleSessionId, 
          cardId: card.id,
          slotIndex: slotIndex,
          expectedVersion: gameState.version,
          idempotencyKey: idempotencyKey,
          targetId: targetId
        }
      })
      if (error) {
        console.error('Card deployment failed:', error.message)
        
        // Handle version conflict (409)
        if (error.message?.includes('Version mismatch') || error.message?.includes('409')) {
          toast.info('Game state changed - retrying action...')
          
          // Refetch battle state and retry if action is still valid
          try {
            const { data: freshBattle, error: fetchError } = await supabase
              .from('battle_sessions')
              .select('*')
              .eq('id', battleSessionId)
              .single()
            
            if (!fetchError && freshBattle) {
              setGameState(freshBattle.game_state)
              setBattleSession(freshBattle)
              
              // Retry the action if it's still valid
              if (freshBattle.game_state?.current_turn_player_id === user?.id) {
                // Recursive call with updated version
                setTimeout(() => handleSlotDrop(card, slotIndex), 100)
                return
              }
            }
          } catch (retryError) {
            // Retry failed silently
          }
        }
        
        toast.error(`Failed to deploy ${card.name}: ${error.message}`)
        return
      }
      
      toast.success(`Deployed ${card.name}!`)
      
      // The server will update the game state, which will trigger UI updates
      // No client-side state manipulation needed - everything handled securely
      
    } catch (error) {
      console.error('Card deployment error:', error)
      toast.error(`Error deploying ${card.name}`)
    }
  }

  // Check if card is a spell
  const isSpellCard = (card: any) => {
    return card.card_type === 'spell'
  }

  // Check if card can be deployed to battlefield (units, structures, and relics)
  const canDeployToBattlefield = (card: any) => {
    return card.card_type === 'unit' || card.card_type === 'structure' || card.card_type === 'relic' || !card.card_type
  }

  // Check if card has targeting battlecries that need target selection
  const needsTargeting = (card: any) => {
    if (!card.keywords || !Array.isArray(card.keywords)) return false
    
    const targetingKeywords = [
      'Battlecry:FreezeEnemy',
      'Battlecry:Damage3', // Can target specific unit
      'Battlecry:HealAlly1',
      'Battlecry:HealAlly2', 
      'Battlecry:HealAlly3',
      'Spell:FreezeTarget',
      'Spell:Damage3'
    ]
    
    return card.keywords.some((keyword: string) => targetingKeywords.includes(keyword))
  }



  // Handle targeting selection
  const handleTargetSelection = async (targetSlotIndex: number) => {
    if (!isTargetingMode || !pendingTargetCard || pendingTargetSlot === null) return
    
    const targetCard = getOpponentBattlefield()[targetSlotIndex]
    if (!targetCard) {
      toast.error('No unit in that slot to target')
      return
    }
    
    // Deploy with target using existing handleSlotDrop
    await handleSlotDrop(pendingTargetCard, pendingTargetSlot, targetSlotIndex.toString())
    
    // Reset targeting mode
    setIsTargetingMode(false)
    setPendingTargetCard(null)
    setPendingTargetSlot(null)
  }

  // Handle spell card activation
  const handleSpellCast = async (card: any) => {
    if (!card || !gameState || !battleSessionId || !user?.id) return
    
    try {
      console.log('🪄 Casting spell:', card.name)
      
      // Find the card index in player's hand
      const isPlayer1 = gameState.player1_id === user.id
      const playerHand = gameState.hands ? 
        (isPlayer1 ? gameState.hands.player1_hand : gameState.hands.player2_hand) :
        (isPlayer1 ? gameState.player1_hand : gameState.player2_hand)
      
      console.log('🔍 Debug spell casting:')
      console.log('  - Card ID:', card.id)
      console.log('  - Player hand size:', playerHand?.length)
      console.log('  - Hand card IDs:', playerHand?.map((c: any) => c.id))
      
      // Find the card by ID
      const cardIndex = playerHand?.findIndex((handCard: any) => handCard.id === card.id) ?? -1
      
      if (cardIndex === -1) {
        console.log('❌ Card not found in hand')
        toast.error('Card not found in hand')
        return
      }
      
      console.log('✅ Found card at index:', cardIndex)
      
      // Call the battle-play-card edge function with spell parameters
      const { data, error } = await supabase.functions.invoke('battle-play-card', {
        body: { 
          battleId: battleSessionId, 
          cardId: card.id,
          cardIndex: cardIndex,
          // targetId: undefined, // TODO: Add targeting for spells that need it
          position: -1 // Not used for spells
        }
      })
      
      if (error) {
        console.error('Spell casting failed:', error.message)
        toast.error(`Failed to cast ${card.name}: ${error.message}`)
        return
      }
      
      if (data?.success) {
        toast.success(`✨ Cast ${card.name}!`)
        
        // Refresh battle state
        setStateVersion(prev => prev + 1)
      }
      
    } catch (error: any) {
      console.error('Spell casting error:', error)
      toast.error(`Error casting ${card.name}`)
    }
  }

  // Check if card can be played (client-side preview only, server does final validation)
  const canPlayCard = (card: any) => {
    if (!gameState || !user?.id) return false
    
    // Check if battle is in a valid phase for deployment
    const validPhases = ['main', 'waiting_first_deploy', 'waiting_second_deploy', 'waiting_redeploy', 'gameplay']
    const isValidPhase = validPhases.includes(gameState.phase || '')
    
    // Check if it's player's turn
    const isPlayerTurn = gameState.current_turn_player_id === user.id
    
    // Check mana
    const hasEnoughMana = getPlayerMana() >= (card.mana_cost || 0)
    
    // Check turn phase - allow actions during 'start' phase (will auto-transition to 'main')
    const turnPhase = gameState.turn_phase || 'main'
    const canTakeActions = turnPhase === 'main' || turnPhase === 'start' || !gameState.turn_phase
    
    const canDeploy = isValidPhase && hasEnoughMana && isPlayerTurn && canTakeActions
    

    
    return canDeploy
  }

  // Set up global touch event listeners for mobile drag and drop
  useEffect(() => {
    if (isTouchDragging) {
      document.addEventListener('touchmove', handleGlobalTouchMove, { passive: false })
      
      return () => {
        document.removeEventListener('touchmove', handleGlobalTouchMove)
      }
    }
  }, [isTouchDragging, draggedCard])

  // Close game menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (showGameMenu) {
        const target = event.target as Element
        if (!target.closest('.game-menu-container')) {
          setShowGameMenu(false)
        }
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [showGameMenu])

  // Simple real-time timer calculation based on turn_started_at
  useEffect(() => {
    // No active turn or missing timestamp
    if (!gameState?.current_turn_player_id || !gameState?.turn_started_at || !user?.id) {
      setTurnTimeRemaining(60)
      return
    }

    // Calculate timer from timestamp
    const calculateAndUpdateTimer = () => {
      try {
        const turnStarted = new Date(gameState.turn_started_at)
        const now = new Date()
        const elapsedSeconds = Math.floor((now.getTime() - turnStarted.getTime()) / 1000)
        const remaining = Math.max(0, 60 - elapsedSeconds)
        
        setTurnTimeRemaining(remaining)
        
        // Just update the timer - auto-end logic will be handled separately
        
        // Debug log occasionally
        if (remaining % 10 === 0 && Math.random() < 0.2) {
          console.log(`⏰ Timer: ${remaining}s remaining (started: ${turnStarted.toLocaleTimeString()})`)
        }
      } catch (err) {
        console.warn('Timer calculation failed:', err)
        setTurnTimeRemaining(60)
      }
    }

    // Initial calculation
    calculateAndUpdateTimer()

    // Update every second
    const interval = setInterval(calculateAndUpdateTimer, 1000)

    return () => clearInterval(interval)
  }, [gameState?.turn_started_at, gameState?.current_turn_player_id, user?.id])

  // Determine day/night based on time
  useEffect(() => {
    const checkTimeOfDay = async () => {
      try {
        setIsLoading(true)
        
        // Use client time for arena display (simpler and no CORS issues)
        const now = new Date()
        const hour = now.getHours()
        const isDaytimeClient = hour >= 6 && hour < 18 // Daytime: 6 AM to 6 PM
        setIsDaytime(isDaytimeClient)

        
      } catch (error) {
        console.warn('Error checking time, defaulting to day:', error)
        setIsDaytime(true)
      } finally {
        setIsLoading(false)
      }
    }

    checkTimeOfDay()
  }, [])

  // Load battle session and game state
  useEffect(() => {
    const loadBattleSession = async () => {
      if (!battleSessionId || !user) return

      try {
        const { data: battle, error } = await supabase
          .from('battle_sessions')
          .select('*')
          .eq('id', battleSessionId)
          .single()

        if (error) {
          console.error('Error loading battle session:', error)
          return
        }

        setBattleSession(battle)
        setGameState(battle.game_state)


        // Timer will be calculated automatically from turn_started_at timestamp

        // Auto-initialize if battle is waiting for initialization
        if (battle.status === 'waiting_initialization') {
          console.log('🚀 Auto-initializing battle...')
          await handleInitializeBattle()
        }
      } catch (error) {
        console.error('Error in loadBattleSession:', error)
      }
    }

    loadBattleSession()
  }, [battleSessionId, user])

  // Load hero data for both players from their active heroes
  useEffect(() => {
    const loadHeroData = async () => {
      if (!user || !battleSession) return

      try {
        // Get current user's active hero
        const { data: playerHeroData, error: playerError } = await supabase
          .rpc('get_user_active_hero', { user_uuid: user.id })

        if (playerError) {
          console.error('Error loading player hero data:', playerError)
          return
        }

        if (playerHeroData && playerHeroData.length > 0) {

          setPlayerHero(playerHeroData[0])
        }

        // Get opponent's active hero
        const opponentId = battleSession.player1_id === user.id 
          ? battleSession.player2_id 
          : battleSession.player1_id

        if (opponentId) {
          const { data: opponentHeroData, error: opponentError } = await supabase
            .rpc('get_user_active_hero', { user_uuid: opponentId })

          if (opponentError) {
            console.error('Error loading opponent hero data:', opponentError)
            return
          }

          if (opponentHeroData && opponentHeroData.length > 0) {

            setOpponentHero(opponentHeroData[0])
          }
        }
      } catch (error) {
        console.error('Error in loadHeroData:', error)
      }
    }

    loadHeroData()
  }, [user, battleSession])

  // Initialize Ably service
  useEffect(() => {
    const ablyApiKey = import.meta.env.VITE_ABLY_API_KEY

    
    if (!ablyApiKey) {
      console.error('❌ Ably API key not found in environment')
      return
    }

    const service = new AblyService(ablyApiKey, user?.id || null)
    setAblyService(service)

    return () => {
      service.cleanup()
      setAblyService(null)
    }
  }, [user?.id])

  // Ably real-time system for live game updates
  useEffect(() => {
    if (!battleSessionId || !user?.id || !ablyService) {
      return
    }
    setConnectionStatus('connecting')
    
    // Test Ably connection and setup

    // Fetch fresh battle state from Supabase
    const fetchFreshBattleState = async () => {
      try {
        const { data: freshBattle, error: fetchError } = await supabase
          .from('battle_sessions')
          .select('*')
          .eq('id', battleSessionId)
          .single()
        
        if (!fetchError && freshBattle) {
          updateBattleState(freshBattle, 'fresh-fetch')
        }
      } catch (error) {
        console.error('❌ Failed to fetch fresh battle state:', error)
      }
    }

    const updateBattleState = (updatedBattle: any, source: string) => {
      
      // Immutable state updates - always return new references
      setBattleSession(prev => ({ ...JSON.parse(JSON.stringify(updatedBattle)) }))
      setGameState(prev => ({ ...JSON.parse(JSON.stringify(updatedBattle.game_state)) }))
      setLastUpdateTime(prev => Date.now()) // Always new reference
      setStateVersion(prev => prev + 1) // Always new reference
      
      // Log battlefield changes for debugging
      if (updatedBattle.game_state?.battlefield) {
        const playerSlots = updatedBattle.game_state.battlefield.player1_slots || []
        const opponentSlots = updatedBattle.game_state.battlefield.player2_slots || []

      }
      
      // Check for game end conditions
    }

    // Note: Ably setup is handled below in the second setup block

    // Clean Ably subscriptions with proper React patterns
    let ablyChannel: any = null
    let pingInterval: any = null

    try {
      ablyChannel = ablyService.getMatchChannel(battleSessionId)
      
      // Subscribe to all game events with immutable state updates
      ablyChannel.subscribe('combat_result', (message) => {
        
        // Multiple aggressive re-render triggers
        setStateVersion(prev => prev + 1)
        
        // Fetch fresh state
        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
        
        // Additional delayed re-render
        setTimeout(() => {
          setStateVersion(prev => prev + 1)
        }, 100)
      })

      ablyChannel.subscribe('card_deployed', (message) => {
        
        // Multiple immediate re-render triggers for battlefield
        setBattlefieldVersion(prev => prev + 1)
        setStateVersion(prev => prev + 1)
        
        // Fetch fresh state with additional re-renders
        fetchFreshBattleState().then(() => {
          setBattlefieldVersion(prev => prev + 1)
          setStateVersion(prev => prev + 1)
        })
        
        // Additional delayed re-renders to force battlefield update
        setTimeout(() => {
          setBattlefieldVersion(prev => prev + 1)
          setStateVersion(prev => prev + 1)
        }, 100)
        
        setTimeout(() => {
          setBattlefieldVersion(prev => prev + 1)
        }, 200)
      })

      ablyChannel.subscribe('spell_resolved', (message) => {
        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('turn_change', (message) => {
        
        // Multiple aggressive re-render triggers for turn changes
        setStateVersion(prev => prev + 1)
        
        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
        
        // Additional delayed re-render
        setTimeout(() => {
          setStateVersion(prev => prev + 1)
        }, 100)
        
        // Force DOM update
        setTimeout(() => {
          document.body.style.transform = 'translateZ(0)'
          setTimeout(() => {
            document.body.style.transform = ''
          }, 1)
        }, 50)
      })

      ablyChannel.subscribe('turn_action', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('mulligan_completed', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('game_started', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('game_ended', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('battle_initialized', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('attack_executed', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('card_played', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('phase_change', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('attacker_switch', (message) => {

        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      ablyChannel.subscribe('hero_power_used', (message) => {
        console.log('🔥 Hero power used event received:', message.data)
        fetchFreshBattleState().then(() => {
          setStateVersion(prev => prev + 1)
        })
      })

      // Enter presence
      ablyService.enterPresence(battleSessionId, {
        user_id: user.id,
        status: 'active',
        client_version: '1.0.0'
      })

      // Set up ping for latency measurement
      pingInterval = setInterval(() => {
        ablyService.sendPing(battleSessionId, (latency) => {
          setLatency(latency) // Immutable state update
        })
      }, 10000)

      setConnectionStatus('connected')

      // Initial state fetch
      fetchFreshBattleState()

    } catch (error) {
      console.error('Ably setup error:', error)
      setConnectionStatus('error')
    }

    return () => {
      
      if (pingInterval) {
        clearInterval(pingInterval)
      }
      
      if (ablyChannel) {
        ablyService.leavePresence(battleSessionId)
        ablyChannel.detach()
      }
      
      setConnectionStatus('disconnected')
    }
  }, [battleSessionId, user?.id, ablyService])

  // Force battlefield re-render when battlefield version changes
  useEffect(() => {
    if (battlefieldVersion > 0) {

      
      // Force additional DOM updates
      const battlefieldElements = document.querySelectorAll('[class*="battlefield"], [class*="grid-cols-2"]')
      battlefieldElements.forEach(el => {
        (el as HTMLElement).style.transform = 'translateZ(0)'
        setTimeout(() => {
          (el as HTMLElement).style.transform = ''
        }, 1)
      })
    }
  }, [battlefieldVersion])

  // Initialize battle if it's waiting for initialization
  const handleInitializeBattle = async () => {
    if (!battleSessionId || isInitializing) return

    try {
      setIsInitializing(true)
      console.log('🚀 Auto-initializing battle:', battleSessionId)
      
      // Call battle-initialize-v2 directly
      const { data, error } = await supabase.functions.invoke('battle-initialize-v2', {
        body: { battleId: battleSessionId }
      })
      
      if (error) {
        throw error
      }

      if (data.success) {

        // The real-time subscription will automatically update the UI
      } else {
        throw new Error(data.error || 'Failed to initialize battle')
      }
    } catch (error: any) {
      console.error('Error initializing battle:', error)
      toast.error(`Failed to initialize battle: ${error.message}`)
    } finally {
      setIsInitializing(false)
    }
  }

  // Handle hero power activation
  const handleHeroPower = async () => {
    if (!playerHero || !battleSessionId || !user) return

    // Determine player positions
    const isPlayer1 = user?.id === gameState?.player1_id
    const playerHeroState = isPlayer1 ? gameState?.player1_hero : gameState?.player2_hero
    
    // Check if hero power can be used
    if (!playerHeroState?.hero_power_available || 
        playerHeroState?.hero_power_used_this_turn === true ||
        (playerHeroState?.hero_power_cooldown || 0) > 0 ||
        (playerHeroState?.mana || 0) < playerHero.hero_power_cost) {
      toast.error('Hero power is not available')
      return
    }

    // Enter hero power targeting mode
    setIsHeroPowerMode(true)
    setIsAttackMode(false) // Cancel any attack mode
    setSelectedAttacker(null) // Cancel any selected attacker
    
    console.log('🔥 Hero power targeting mode activated:', playerHero.hero_power_name)
    toast.info(`${playerHero.hero_power_name} - Select a target`)
  }

  // Handle hero power target selection
  const handleHeroPowerTarget = async (targetType: 'enemy_unit' | 'enemy_hero' | 'own_hero', targetSlotIndex?: number) => {
    if (!isHeroPowerMode || !playerHero || !battleSessionId || !user) return

    setIsHeroPowerMode(false) // Exit targeting mode

    try {
      const idempotencyKey = `hero-power-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
      
      const { data, error } = await supabase.functions.invoke('battle-hero-power', {
        body: { 
          battleSessionId,
          playerId: user.id,
          targetType,
          targetSlotIndex
        }
      })

      if (error) {
        console.error('❌ Hero power failed:', error)
        toast.error(`Hero power failed: ${error.message}`)
        return
      }

      console.log('✅ Hero power used successfully:', data)
      toast.success(data.message || 'Hero power used successfully!')

    } catch (error) {
      console.error('Error using hero power:', error)
      toast.error('Failed to use hero power')
    }
  }

  // Cancel hero power targeting
  const cancelHeroPowerTargeting = () => {
    setIsHeroPowerMode(false)
    toast.info('Hero power targeting cancelled')
  }

  // Handle mulligan card selection
  const toggleMulliganCard = (cardId: string) => {
    setSelectedMulliganCards(prev => 
      prev.includes(cardId) 
        ? prev.filter(id => id !== cardId)
        : [...prev, cardId]
    )
  }

  // Handle mulligan
  const handleMulligan = async (cardsToRedraw: string[]) => {
    if (!user || !battleSessionId) return

    try {

      
      const { data, error } = await supabase.functions.invoke('battle-mulligan', {
        body: { 
          battleSessionId, 
          playerId: user.id,
          cardsToRedraw
        }
      })
      
      if (error) {
        throw error
      }
      

      
      // Clear selected cards after mulligan
      setSelectedMulliganCards([])
      
      if (data.gameStarted) {
        toast.success(`🚀 Game started! ${data.firstPlayer === user.id ? 'Your turn!' : 'Opponent goes first'}`)
      } else {
        toast.success('🃏 Mulligan completed - waiting for opponent')
      }
      
    } catch (error: any) {
      console.error('Error in mulligan:', error)
      toast.error(`Mulligan failed: ${error.message}`)
    }
  }

  // Handle surrender
  const handleSurrender = async () => {
    if (!battleSession?.id || !user?.id) return

    try {
      console.log('🏳️ Surrendering match...', battleSession.id)
      
      const { data, error } = await supabase.functions.invoke('concede', {
        body: {
          matchId: battleSession.id
        }
      })

      if (error) {
        console.error('❌ Surrender error:', error)
        toast.error('Failed to surrender match')
        return
      }


      toast.success('Match surrendered')
      
      // Close menus
      setShowSurrenderConfirm(false)
      setShowGameMenu(false)
      
    } catch (error) {
      console.error('❌ Surrender error:', error)
      toast.error('Failed to surrender match')
    }
  }

  // Manual end turn (server-driven)
  const handleEndTurn = async () => {
    if (!user || !battleSessionId) return

    try {
      console.log('🏁 Player requesting to end turn...')
      
      // Call edge function for end turn
      const { data, error } = await supabase.functions.invoke('battle-end-turn', {
        body: { 
          battleSessionId, 
          playerId: user.id,
          isAutoEnd: false
        }
      })
      
      if (error) {
        throw error
      }
      

      // UI will update automatically via real-time subscription
      
    } catch (error: any) {
      console.error('Error ending turn:', error)
      toast.error(`Failed to end turn: ${error.message}`)
    }
  }

  // Handle unit attack
  const handleUnitAttack = async (attackerSlot: number, targetSlot: number) => {
    if (!user || !battleSessionId) return

    try {
      console.log('⚔️ Unit attacking:', { attackerSlot, targetSlot })
      
      const { data, error } = await supabase.functions.invoke('battle-unit-attack', {
        body: { 
          battleSessionId,
          playerId: user.id,
          attackerSlot,
          targetSlot,
          targetType: 'unit'
        }
      })

      if (error) {
        console.error('❌ Attack failed:', error)
        toast.error(`Attack failed: ${error.message}`)
        return
      }


      toast.success('Attack executed!')
      
      // Reset attack mode
      setSelectedAttacker(null)
      setIsAttackMode(false)
    } catch (error) {
      console.error('❌ Attack error:', error)
      toast.error(`Attack failed: ${error.message}`)
    }
  }

  // Handle selecting an attacker
  const handleSelectAttacker = (card: any, slotIndex: number) => {

    
    if (!isPlayerTurn) {
      toast.error("Not your turn")
      return
    }
    
    if (!card || card.attack === null || card.attack === 0) {
      toast.error(`${card.name} cannot attack (no attack value)`)
      return
    }
    
    // Allow attacking in both 'main' and 'start' phases (start transitions to main)
    const validPhases = ['main', 'start']
    if (!validPhases.includes(gameState?.turn_phase || 'main')) {
      toast.error(`Cannot attack during ${gameState?.turn_phase} phase`)
      return
    }
    
    // Server will validate summoning sickness and attack availability
    setSelectedAttacker({ card, slotIndex })
    setIsAttackMode(true)
    toast.info(`Selected ${card.name} to attack. Click an enemy unit or hero to target.`)
  }

  // Handle selecting a target (unit)
  const handleSelectTarget = (targetSlot: number) => {
    if (!selectedAttacker || !isAttackMode) return
    
    // Check Taunt restrictions - can only attack non-Taunt units if no Taunt units exist
    const tauntUnits = getEnemyTauntUnits()
    const targetCard = getOpponentBattlefield()[targetSlot]
    
    if (tauntUnits.length > 0 && targetCard && !targetCard.has_taunt) {
      toast.error(`Cannot attack ${targetCard.name}! Must attack Taunt units first: ${tauntUnits.map(u => u.name).join(', ')}`)
      return
    }
    
    handleUnitAttack(selectedAttacker.slotIndex, targetSlot)
  }

  // Check if enemy has Taunt units
  const getEnemyTauntUnits = () => {
    const enemyBattlefield = getOpponentBattlefield()
    return enemyBattlefield.filter(card => card && card.has_taunt)
  }

  // Handle attacking enemy hero
  const handleHeroAttack = async () => {
    if (!selectedAttacker || !isAttackMode || !user || !battleSessionId) return

    // Check for Taunt units - cannot attack hero if enemy has Taunt units
    const tauntUnits = getEnemyTauntUnits()
    if (tauntUnits.length > 0) {
      toast.error(`Cannot attack hero while enemy has Taunt units! Must attack: ${tauntUnits.map(u => u.name).join(', ')}`)
      return
    }

    try {
      console.log('🎯 Unit attacking enemy hero:', selectedAttacker.card.name)
      
      const { data, error } = await supabase.functions.invoke('battle-unit-attack', {
        body: { 
          battleSessionId,
          playerId: user.id,
          attackerSlot: selectedAttacker.slotIndex,
          targetType: 'hero'
        }
      })

      if (error) {
        console.error('❌ Hero attack failed:', error)
        toast.error(`Hero attack failed: ${error.message}`)
        return
      }


      toast.success(`${selectedAttacker.card.name} attacked the enemy hero!`)
      
      // Reset attack mode
      setSelectedAttacker(null)
      setIsAttackMode(false)
    } catch (error) {
      console.error('❌ Hero attack error:', error)
      toast.error(`Hero attack failed: ${error.message}`)
    }
  }



  // Auto turn-end when timer hits 0 (separate effect to avoid dependency issues)
  useEffect(() => {
    const isMyTurn = gameState?.current_turn_player_id === user?.id
    
    if (turnTimeRemaining === 0 && isMyTurn && gameState?.turn_started_at) {
      // Check if enough time has actually elapsed (prevent false triggers)
      const turnStarted = new Date(gameState.turn_started_at)
      const elapsedSeconds = Math.floor((Date.now() - turnStarted.getTime()) / 1000)
      
      if (elapsedSeconds >= 60) {
        console.log('⏰ Timer expired - auto-ending turn')
        handleEndTurn()
      }
    }
  }, [turnTimeRemaining, gameState?.current_turn_player_id, user?.id, gameState?.turn_started_at])

  if (isLoading) {
    return (
      <div className="w-full h-screen flex items-center justify-center bg-gray-900">
        <div className="text-white text-lg">Loading Arena...</div>
      </div>
    )
  }

  const backgroundImage = isDaytime ? arenaDay : arenaNight
  const timeOfDay = isDaytime ? 'Day' : 'Night'

  // Determine player positions
  const isPlayer1 = user?.id === gameState?.player1_id
  
  // Debug player assignment  

  
  // Get hero states with defaults for demo purposes
  const playerHeroState = isPlayer1 ? gameState?.player1_hero : gameState?.player2_hero
  const opponentHeroState = isPlayer1 ? gameState?.player2_hero : gameState?.player1_hero

  // Debug logging for turn and hero states
  // Reduced debug logging - only show critical timer info
  if (Math.random() < 0.02) { // Only log 2% of the time
    const isMyTurn = gameState?.current_turn_player_id === user?.id

  }
  
  // Default hero state for demonstration (fallback when game state is loading)
  const defaultHeroState = {
    hp: playerHero?.base_hp || 30,
    max_hp: playerHero?.base_hp || 30,
    mana: 1,
    max_mana: 1,
    hero_power_available: true,
    hero_power_cooldown: 0
  }
  
  // Try both hand structure formats
  let playerHand: any[] = []
  let opponentHand: any[] = []
  
  if (gameState?.hands) {
    // New structure: gameState.hands.player1_hand
    playerHand = isPlayer1 ? (gameState.hands.player1_hand || []) : (gameState.hands.player2_hand || [])
    opponentHand = isPlayer1 ? (gameState.hands.player2_hand || []) : (gameState.hands.player1_hand || [])
    
    // Debug logging for hand assignment (reduced back to normal)

  } else {
    // Old structure: gameState.player1_hand  
    playerHand = isPlayer1 ? (gameState?.player1_hand || []) : (gameState?.player2_hand || [])
    opponentHand = isPlayer1 ? (gameState?.player2_hand || []) : (gameState?.player1_hand || [])
  }

  // Calculate card position for hand fanning effect
  const getCardPosition = (index: number, totalCards: number) => {
    if (totalCards <= 1) {
      return { rotation: 0, translateX: 0, zIndex: 10 }
    }

    const maxRotation = 25 // Maximum rotation in degrees
    const maxSpread = 300 // Maximum spread in pixels
    const centerIndex = (totalCards - 1) / 2

    // Calculate rotation: cards fan out from center
    const rotationStep = totalCards > 1 ? (maxRotation * 2) / (totalCards - 1) : 0
    const rotation = (index - centerIndex) * rotationStep

    // Calculate horizontal position: cards spread horizontally
    const spreadStep = totalCards > 1 ? maxSpread / (totalCards - 1) : 0
    const translateX = (index - centerIndex) * spreadStep

    // Z-index: left-to-right layering (rightmost cards on top)
    // Each card to the right should be on top of cards to the left
    const zIndex = 10 + index

    return { rotation, translateX, zIndex }
  }

  // Determine if it's player's turn
  const isPlayerTurn = gameState?.current_turn_player_id === user?.id

  return (
    <div 
      key={`arena-${stateVersion}-${battlefieldVersion}-${lastUpdateTime}`} // Force complete re-render on state changes
      className="w-full h-screen relative bg-cover bg-center bg-no-repeat overflow-hidden select-none"
      style={{ backgroundImage: `url(${backgroundImage})` }}
      onDragOver={handleDragOver}
      onTouchMove={isTouchDragging ? handleTouchMove : undefined}
    >


      {/* Turn Indicator - Only show after mulligan phase */}
      {gameState && battleSession?.status === 'active' && gameState?.phase !== 'mulligan' && (
        <TurnIndicator
          isPlayerTurn={isPlayerTurn}
          timeRemaining={turnTimeRemaining}
          onEndTurn={isPlayerTurn ? handleEndTurn : undefined}
        />
      )}

      {/* Game Menu (Hamburger) - Top Left */}
      {gameState && battleSession?.status === 'active' && (
        <div className="absolute top-4 left-4 z-40">
          <div className="relative game-menu-container">
            {/* Hamburger Button */}
            <button
              onClick={() => setShowGameMenu(!showGameMenu)}
              className="bg-black bg-opacity-50 hover:bg-opacity-70 text-white p-3 rounded-lg border border-gray-600 hover:border-gray-400 transition-all duration-200"
            >
              <div className="flex flex-col w-6 h-6 justify-center items-center">
                <div className="w-5 h-0.5 bg-white mb-1 rounded"></div>
                <div className="w-5 h-0.5 bg-white mb-1 rounded"></div>
                <div className="w-5 h-0.5 bg-white rounded"></div>
              </div>
            </button>

            {/* Dropdown Menu */}
            {showGameMenu && (
              <div className="absolute top-full left-0 mt-2 bg-black bg-opacity-90 border border-gray-600 rounded-lg min-w-48 z-50">
                <button
                  onClick={() => {
                    setShowSurrenderConfirm(true)
                    setShowGameMenu(false)
                  }}
                  className="w-full text-left px-4 py-3 text-white hover:bg-red-600 hover:bg-opacity-50 transition-colors duration-200 rounded-lg flex items-center gap-2"
                >
                  <span className="text-red-400">🏳️</span>
                  <span>Surrender</span>
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Opponent Hero (Top Center) */}
      {opponentHero && (
        <div 
          className={`absolute top-4 left-1/2 transform -translate-x-1/2 z-30 transition-all duration-200 ${
            isAttackMode 
              ? 'cursor-crosshair hover:ring-4 hover:ring-red-400 hover:ring-opacity-75' 
              : isHeroPowerMode
              ? 'cursor-target hover:ring-4 hover:ring-yellow-400 hover:ring-opacity-75'
              : ''
          }`}
          onClick={() => {
            if (isAttackMode) {
              handleHeroAttack()
            } else if (isHeroPowerMode) {
              handleHeroPowerTarget('enemy_hero')
            }
          }}
          onTouchEnd={(e) => {
            e.preventDefault()
            if (isAttackMode) {
              handleHeroAttack()
            } else if (isHeroPowerMode) {
              handleHeroPowerTarget('enemy_hero')
            }
          }}
          title={
            isAttackMode 
              ? `Attack ${opponentHero.name} (${opponentHeroState?.hp || defaultHeroState.hp} HP)` 
              : isHeroPowerMode
              ? `Target ${opponentHero.name} with Royal Decree (${opponentHeroState?.hp || defaultHeroState.hp} HP)`
              : ''
          }
        >
          <HeroDisplay
            hero={opponentHero}
            currentHp={opponentHeroState?.hp || defaultHeroState.hp}
            maxHp={opponentHeroState?.max_hp || defaultHeroState.max_hp}
            currentMana={opponentHeroState?.mana || defaultHeroState.mana}
            maxMana={opponentHeroState?.max_mana || defaultHeroState.max_mana}
            isOpponent={true}
            heroPowerAvailable={opponentHeroState?.hero_power_available !== false}
            heroPowerOnCooldown={opponentHeroState?.hero_power_used_this_turn === true || (opponentHeroState?.hero_power_cooldown || 0) > 0}
          />
        </div>
      )}

      {/* Opponent Hand (Top Center) */}
      <div className="absolute top-32 left-1/2 transform -translate-x-1/2">
        <div className="text-center text-white/80 text-xs mb-4" style={{ fontFamily: "'Press Start 2P', monospace" }}>
          OPPONENT: {opponentHand?.length || 0} CARDS
        </div>
        
        {/* Enemy Hand Container */}
        <div className="relative flex justify-center items-start h-80 w-[700px]">
          {Array.from({ length: opponentHand?.length || 0 }).map((_, index) => {
            const position = getCardPosition(index, opponentHand?.length || 0)
            
            return (
              <div
                key={index}
                className="absolute transition-all duration-300 ease-out"
                style={{
                  transform: `translateX(${position.translateX}px) rotate(${-position.rotation}deg) translateY(-30px) scale(0.6)`,
                  zIndex: position.zIndex,
                  transformOrigin: 'top center'
                }}
              >
                <CardBack
                  className="cursor-default"
                />
              </div>
            )
          })}
        </div>
      </div>

      {/* Battlefield Areas (Middle) - 6 Slots Each */}
      <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full flex justify-center gap-80">
        {/* Player Battlefield (Left) - 6 Slots */}
        <div className="flex flex-col items-center">
          <div className="grid grid-cols-2 gap-6">
            {getPlayerBattlefield().map((card, slotIndex) => (
              <div
                key={`player-slot-${slotIndex}-${battlefieldVersion}-${stateVersion}`}
                data-slot-index={slotIndex}
                data-player-slot
                className={`w-36 h-36 border-2 rounded-lg flex items-center justify-center relative transition-all duration-200 ${
                  card 
                    ? 'border-blue-400' 
                    : isDragging 
                      ? 'border-blue-300 border-dashed animate-pulse glow-blue' 
                      : 'border-blue-400/30 border-dashed hover:border-blue-400/50'
                }`}
                onDragOver={(e) => e.preventDefault()}
                onDrop={(e) => {
                  e.preventDefault()
                  if (draggedCard && !card) { // Only drop on empty slots
                    handleSlotDrop(draggedCard, slotIndex)
                    setDraggedCard(null)
                  }
                }}
              >
                {card ? (

                    {/* Show potato image when card is deployed */}
                    <div 
                      className={`w-full h-full relative transition-all duration-200 ${
                        card.deployed_turn === gameState?.turn_number && !card.has_charge
                          ? 'opacity-60 cursor-not-allowed saturate-50' // Summoning sickness visual
                          : card.has_attacked_this_turn 
                            ? 'opacity-50 cursor-not-allowed grayscale' // Already attacked visual
                            : selectedAttacker?.slotIndex === slotIndex 
                              ? 'ring-4 ring-yellow-400 ring-opacity-75 cursor-pointer' 
                              : card.card_type === 'structure'
                                ? 'cursor-default' // Structures can't attack
                                : card.card_type === 'relic'
                                  ? 'cursor-default' // Relics can't attack
                                  : 'hover:ring-2 hover:ring-white/50 cursor-pointer'
                      }`}
                      onClick={() => {
                      // Structures cannot attack
                      if (card.card_type === 'structure') {
                        toast.info(`${card.name} is a structure and cannot attack`)
                        return
                      }
                      
                      // Relics cannot attack
                      if (card.card_type === 'relic') {
                        toast.info(`${card.name} is a relic and provides passive effects`)
                        return
                      }
                      
                      // Only allow clicking if not summoning sick, hasn't attacked, and not frozen
                      if (card.deployed_turn === gameState?.turn_number && !card.has_charge) {
                        toast.error(`${card.name} can't attack this turn (summoning sickness)`)
                        return
                      }
                      if (card.has_attacked_this_turn) {
                        toast.error(`${card.name} has already attacked this turn`)
                        return
                      }
                      if (card.is_frozen) {
                        toast.error(`${card.name} is frozen and cannot attack`)
                        return
                      }
                      handleSelectAttacker(card, slotIndex)
                    }}
                    onTouchEnd={(e) => {
                      e.preventDefault()
                      // Only allow touching if not summoning sick, hasn't attacked, and not frozen
                      if (card.deployed_turn === gameState?.turn_number && !card.has_charge) {
                        toast.error(`${card.name} can't attack this turn (summoning sickness)`)
                        return
                      }
                      if (card.has_attacked_this_turn) {
                        toast.error(`${card.name} has already attacked this turn`)
                        return
                      }
                      if (card.is_frozen) {
                        toast.error(`${card.name} is frozen and cannot attack`)
                        return
                      }
                      handleSelectAttacker(card, slotIndex)
                    }}
                  >
                    {card.registry_id === 'potato_0001' ? (
                      <img 
                        src={potato0001}
                        alt={card.name}
                        className="w-full h-full object-contain"
                        style={{ imageRendering: 'pixelated' }}
                      />
                    ) : (
                      <ElementalPixelPotato
                        seed={card.id || card.potato_id}
                        potatoType={card.potato_type || 'light'}
                        rarity={card.rarity || 'common'}
                        cardName={card.name}
                        size={120} // Even larger size for bigger battlefield slots
                        className="w-full h-full"
                      />
                    )}
                    {/* Custom Health Bar - Only for units and structures (not relics) */}
                    {card.card_type !== 'relic' && (
                      <div className="absolute -bottom-2 left-1/2 transform -translate-x-1/2 w-28">
                        <HealthBar 
                          currentHp={card.current_hp || card.hp || 0}
                          maxHp={card.max_hp || card.hp || 0}
                          className="shadow-lg"
                        />
                      </div>
                    )}
                    {/* Attack indicator - Only for units (not structures or relics) */}
                    {card.card_type !== 'structure' && card.card_type !== 'relic' && (
                      <div className="absolute -top-1 -right-1">
                        <div className="relative flex items-center justify-center w-12 h-8">
                          <img 
                            src={atkIcon} 
                            alt="Attack Points" 
                            className="absolute inset-0 w-full h-full drop-shadow-lg object-contain"
                            style={{ imageRendering: 'pixelated' }}
                          />
                          <span 
                            className={`relative z-10 font-bold text-sm drop-shadow-md ${
                              card.aura_attack_bonus > 0 ? 'text-green-300' : 'text-white'
                            }`}
                            style={{
                              fontFamily: 'PixelFont, monospace',
                              textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
                              transform: 'translateY(1px)'
                            }}
                          >
                            {card.effective_attack || card.attack}
                            {card.aura_attack_bonus > 0 && (
                              <span className="text-green-300 text-xs ml-1">+{card.aura_attack_bonus}</span>
                            )}
                          </span>
                        </div>
                      </div>
                    )}
                    
                    {/* Ability Indicators */}
                    <div className="absolute -top-2 -left-2 flex flex-col gap-1">
                      {/* Taunt Indicator */}
                      {card.has_taunt && (
                        <div className="w-6 h-6 bg-yellow-500 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Taunt - Must be attacked first">
                          🛡️
                        </div>
                      )}
                      
                      {/* Charge Indicator (only show if can attack immediately) */}
                      {!card.summoning_sickness && card.deployed_turn === gameState?.turn_number && (
                        <div className="w-6 h-6 bg-red-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Charge - Can attack immediately">
                          ⚡
                        </div>
                      )}
                      
                      {/* Lifesteal Indicator */}
                      {card.has_lifesteal && (
                        <div className="w-6 h-6 bg-pink-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Lifesteal - Heals when dealing damage">
                          🩸
                        </div>
                      )}
                      
                      {/* Poison Indicator */}
                      {(card.has_poison || card.has_poison_touch) && (
                        <div className="w-6 h-6 bg-green-600 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Poison - Destroys any unit damaged">
                          ☠️
                        </div>
                      )}
                      
                      {/* Frozen Indicator */}
                      {card.is_frozen && (
                        <div className="w-6 h-6 bg-blue-400 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Frozen - Cannot attack">
                          🧊
                        </div>
                      )}
                      
                      {/* Silenced Indicator */}
                      {card.is_silenced && (
                        <div className="w-6 h-6 bg-gray-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Silenced - No abilities">
                          🔇
                        </div>
                      )}
                      
                      {/* Aura Bonus Indicator */}
                      {card.aura_attack_bonus > 0 && (
                        <div className="w-6 h-6 bg-purple-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title={`Aura Bonus: +${card.aura_attack_bonus} ATK from structures`}>
                          💜
                        </div>
                      )}
                      
                      {/* Damage Reduction Indicator */}
                      {card.aura_damage_reduction > 0 && (
                        <div className="w-6 h-6 bg-blue-600 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title={`Damage Reduction: -${card.aura_damage_reduction} from structures`}>
                          🛡️
                        </div>
                      )}

                      {card.has_divine_shield && (
                        <div className="w-6 h-6 bg-yellow-300 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Divine Shield - Blocks first damage">
                          🛡️
                        </div>
                      )}

                      {card.has_double_strike && (
                        <div className="w-6 h-6 bg-orange-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Double Strike - Attacks twice">
                          ⚔️
                        </div>
                      )}

                      {card.has_windfury && (
                        <div className="w-6 h-6 bg-cyan-400 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Windfury - Can attack twice per turn">
                          💨
                        </div>
                      )}

                      {card.has_rush && (
                        <div className="w-6 h-6 bg-orange-400 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Rush - Can attack units immediately">
                          🏃
                        </div>
                      )}

                      {card.has_stealth && (
                        <div className="w-6 h-6 bg-purple-600 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Stealth - Cannot be targeted">
                          👤
                        </div>
                      )}

                      {card.has_immune && (
                        <div className="w-6 h-6 bg-gold-400 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Immune - Takes no damage">
                          ✨
                        </div>
                      )}
                    </div>
                  </div>

                ) : (
                  // Empty slot - just border
                  null
                )}
              </div>
            ))}
          </div>
        </div>
        
        {/* Opponent Battlefield (Right) - 6 Slots */}
        <div className="flex flex-col items-center">
          <div className="grid grid-cols-2 gap-6">
            {getOpponentBattlefield().map((card, slotIndex) => (
              <div
                key={`opponent-slot-${slotIndex}-${battlefieldVersion}-${stateVersion}`}
                data-slot-index={slotIndex}
                data-opponent-slot
                className={`w-36 h-36 border-2 rounded-lg flex items-center justify-center relative transition-all duration-200 ${
                  card 
                    ? isTargetingMode
                      ? 'border-blue-300 cursor-target hover:ring-2 hover:ring-blue-400'
                      : isAttackMode 
                      ? (() => {
                          const tauntUnits = getEnemyTauntUnits()
                          const canTarget = tauntUnits.length === 0 || card.has_taunt
                          return canTarget 
                            ? 'border-red-300 cursor-crosshair hover:ring-2 hover:ring-red-400' 
                            : 'border-gray-400 cursor-not-allowed opacity-50'
                        })()
                      : isHeroPowerMode
                      ? 'border-yellow-300 cursor-target hover:ring-2 hover:ring-yellow-400'
                      : 'border-red-400'
                    : 'border-red-400/30 border-dashed'
                }`}
                onClick={() => {
                  if (card && isAttackMode) {
                    // Check Taunt restrictions before allowing attack
                    const tauntUnits = getEnemyTauntUnits()
                    if (tauntUnits.length > 0 && !card.has_taunt) {
                      toast.error(`Cannot attack ${card.name}! Must attack Taunt units first: ${tauntUnits.map(u => u.name).join(', ')}`)
                      return
                    }
                    handleSelectTarget(slotIndex)
                  } else if (card && isTargetingMode) {
                    handleTargetSelection(slotIndex)
                  } else if (card && isHeroPowerMode) {
                    handleHeroPowerTarget('enemy_unit', slotIndex)
                  }
                }}
                onTouchEnd={(e) => {
                  e.preventDefault()
                  if (card && isAttackMode) {
                    // Check Taunt restrictions before allowing attack
                    const tauntUnits = getEnemyTauntUnits()
                    if (tauntUnits.length > 0 && !card.has_taunt) {
                      toast.error(`Cannot attack ${card.name}! Must attack Taunt units first: ${tauntUnits.map(u => u.name).join(', ')}`)
                      return
                    }
                    handleSelectTarget(slotIndex)
                  } else if (card && isTargetingMode) {
                    handleTargetSelection(slotIndex)
                  } else if (card && isHeroPowerMode) {
                    handleHeroPowerTarget('enemy_unit', slotIndex)
                  }
                }}
              >
                {card ? (

                    {/* Show potato image when card is deployed */}
                    <div 
                      className="w-full h-full relative"
                    >
                    {card.registry_id === 'potato_0001' ? (
                      <img 
                        src={potato0001}
                        alt={card.name}
                        className="w-full h-full object-contain"
                        style={{ imageRendering: 'pixelated' }}
                      />
                    ) : (
                      <ElementalPixelPotato
                        seed={card.id || card.potato_id}
                        potatoType={card.potato_type || 'light'}
                        rarity={card.rarity || 'common'}
                        cardName={card.name}
                        size={120} // Even larger size for bigger battlefield slots
                        className="w-full h-full"
                      />
                    )}
                    {/* Custom Health Bar - Only for units and structures (not relics) */}
                    {card.card_type !== 'relic' && (
                      <div className="absolute -bottom-2 left-1/2 transform -translate-x-1/2 w-28">
                        <HealthBar 
                          currentHp={card.current_hp || card.hp || 0}
                          maxHp={card.max_hp || card.hp || 0}
                          className="shadow-lg"
                        />
                      </div>
                    )}
                    {/* Attack indicator - Only for units (not structures or relics) */}
                    {card.card_type !== 'structure' && card.card_type !== 'relic' && (
                      <div className="absolute -top-1 -right-1">
                        <div className="relative flex items-center justify-center w-12 h-8">
                          <img 
                            src={atkIcon} 
                            alt="Attack Points" 
                            className="absolute inset-0 w-full h-full drop-shadow-lg object-contain"
                            style={{ imageRendering: 'pixelated' }}
                          />
                          <span 
                            className="relative z-10 text-white font-bold text-sm drop-shadow-md"
                            style={{
                              fontFamily: 'PixelFont, monospace',
                              textShadow: '1px 1px 0 #000, -1px -1px 0 #000, 1px -1px 0 #000, -1px 1px 0 #000',
                              transform: 'translateY(1px)'
                            }}
                          >
                            {card.attack}
                          </span>
                        </div>
                      </div>
                    )}
                    
                    {/* Ability Indicators - Opponent */}
                    <div className="absolute -top-2 -left-2 flex flex-col gap-1">
                      {/* Taunt Indicator */}
                      {card.has_taunt && (
                        <div className="w-6 h-6 bg-yellow-500 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Taunt - Must be attacked first">
                          🛡️
                        </div>
                      )}
                      
                      {/* Charge Indicator */}
                      {!card.summoning_sickness && card.deployed_turn === gameState?.turn_number && (
                        <div className="w-6 h-6 bg-red-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Charge - Can attack immediately">
                          ⚡
                        </div>
                      )}
                      
                      {/* Lifesteal Indicator */}
                      {card.has_lifesteal && (
                        <div className="w-6 h-6 bg-pink-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Lifesteal - Heals when dealing damage">
                          🩸
                        </div>
                      )}
                      
                      {/* Poison Indicator */}
                      {(card.has_poison || card.has_poison_touch) && (
                        <div className="w-6 h-6 bg-green-600 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Poison - Destroys any unit damaged">
                          ☠️
                        </div>
                      )}
                      
                      {/* Frozen Indicator */}
                      {card.is_frozen && (
                        <div className="w-6 h-6 bg-blue-400 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Frozen - Cannot attack">
                          🧊
                        </div>
                      )}
                      
                      {/* Silenced Indicator */}
                      {card.is_silenced && (
                        <div className="w-6 h-6 bg-gray-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Silenced - No abilities">
                          🔇
                        </div>
                      )}

                      {/* Advanced Keyword Indicators - Opponent */}
                      {card.has_divine_shield && (
                        <div className="w-6 h-6 bg-yellow-300 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Divine Shield - Blocks first damage">
                          🛡️
                        </div>
                      )}

                      {card.has_double_strike && (
                        <div className="w-6 h-6 bg-orange-500 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Double Strike - Attacks twice">
                          ⚔️
                        </div>
                      )}

                      {card.has_windfury && (
                        <div className="w-6 h-6 bg-cyan-400 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Windfury - Can attack twice per turn">
                          💨
                        </div>
                      )}

                      {card.has_rush && (
                        <div className="w-6 h-6 bg-orange-400 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Rush - Can attack units immediately">
                          🏃
                        </div>
                      )}

                      {card.has_stealth && (
                        <div className="w-6 h-6 bg-purple-600 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg" title="Stealth - Cannot be targeted">
                          👤
                        </div>
                      )}

                      {card.has_immune && (
                        <div className="w-6 h-6 bg-gold-400 rounded-full flex items-center justify-center text-xs font-bold text-black shadow-lg" title="Immune - Takes no damage">
                          ✨
                        </div>
                      )}
                    </div>
                  </div>

                ) : (
                  // Empty slot - just border
                  null
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Attack Mode Indicator */}
      {isAttackMode && selectedAttacker && (
        <div className="absolute top-20 left-1/2 transform -translate-x-1/2 z-50">
          <div className="bg-yellow-600/90 text-white p-4 rounded-lg border-2 border-yellow-400 shadow-lg">
            <div className="text-center">
              <div className="font-bold">⚔️ ATTACK MODE</div>
              <div className="text-sm mt-1">{selectedAttacker.card.name} ready to attack</div>
              <div className="text-xs mt-2">Click an enemy unit or the enemy hero</div>
              <div className="flex gap-2 mt-3">
                <button 
                  onClick={handleHeroAttack}
                  className="px-3 py-1 bg-red-600 hover:bg-red-700 rounded text-xs"
                >
                  🎯 Attack Hero
                </button>
                <button 
                  onClick={() => {
                    setSelectedAttacker(null)
                    setIsAttackMode(false)
                  }}
                  className="px-3 py-1 bg-gray-600 hover:bg-gray-700 rounded text-xs"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Targeting Mode Indicator */}
      {isTargetingMode && pendingTargetCard && (
        <div className="absolute top-20 left-1/2 transform -translate-x-1/2 z-50">
          <div className="bg-blue-600/90 text-white p-4 rounded-lg border-2 border-blue-400 shadow-lg">
            <div className="text-center">
              <div className="font-bold">🎯 TARGETING MODE</div>
              <div className="text-sm mt-1">{pendingTargetCard.name} needs a target</div>
              <div className="text-xs mt-2">Click an enemy unit to target</div>
              <div className="flex gap-2 mt-3">
                <button 
                  onClick={() => {
                    setIsTargetingMode(false)
                    setPendingTargetCard(null)
                    setPendingTargetSlot(null)
                  }}
                  className="px-3 py-1 bg-gray-600 hover:bg-gray-700 rounded text-xs"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Hero Power Mode Overlay */}
      {isHeroPowerMode && (
        <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 z-50 bg-black/80 text-white p-6 rounded-lg border-2 border-yellow-400">
          <div className="text-center">
            <div className="text-xl mb-2" style={{ fontFamily: "'Press Start 2P', monospace" }}>
              🔥 Royal Decree
            </div>
            <div className="text-sm mb-4">
              Select a target:
            </div>
            <div className="text-xs mb-4 text-yellow-300">
              • Enemy units or hero: Deal 2 damage<br/>
              • Your hero: Restore 2 health
            </div>
            <button
              onClick={cancelHeroPowerTargeting}
              className="px-4 py-2 bg-red-600 hover:bg-red-700 rounded text-sm transition-colors"
              style={{ fontFamily: "'Press Start 2P', monospace" }}
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {/* Player Hand (Bottom Center) */}
      <div className="absolute bottom-32 left-1/2 transform -translate-x-1/2">

        
        {/* Hand Container */}
        <div className="relative flex justify-center items-end h-96 w-[800px]">
          {playerHand?.map((card, index) => {
            const position = getCardPosition(index, playerHand.length)
            const isHovered = hoveredCardIndex === index
            
            return (
              <div
                key={`hand-card-${index}-${card.id}`}
                className={`absolute transition-all duration-300 ease-out ${
                  canPlayCard(card) ? (isSpellCard(card) ? 'cursor-pointer' : 'cursor-grab') : 'cursor-not-allowed opacity-60'
                } ${isDragging && draggedCard?.id === card.id ? 'opacity-30 scale-90' : ''} ${
                  isSpellCard(card) && canPlayCard(card) ? 'ring-2 ring-purple-400/50 ring-offset-2 ring-offset-transparent' : ''
                } ${
                  card.card_type === 'structure' && canPlayCard(card) ? 'ring-2 ring-yellow-400/50 ring-offset-2 ring-offset-transparent' : ''
                } ${
                  card.card_type === 'relic' && canPlayCard(card) ? 'ring-2 ring-orange-400/50 ring-offset-2 ring-offset-transparent' : ''
                }`}
                style={{
                  transform: `translateX(${position.translateX}px) rotate(${position.rotation}deg) ${isHovered ? 'translateY(-60px) scale(0.75)' : 'translateY(30px) scale(0.6)'}`,
                  zIndex: isHovered ? 50 : position.zIndex,
                  transformOrigin: 'bottom center'
                }}
                draggable={canPlayCard(card) && canDeployToBattlefield(card)}
                onClick={(e) => handleCardClick(card, e)}
                onDragStart={(e) => handleDragStart(card, e)}
                onDragEnd={handleDragEnd}
                onTouchStart={(e) => handleTouchStart(card, e)}
                onTouchMove={handleTouchMove}
                onTouchEnd={handleTouchEnd}
                onMouseEnter={() => setHoveredCardIndex(index)}
                onMouseLeave={() => setHoveredCardIndex(null)}
              >
                <TradingCard
                  card={{
                    id: card.id,
                    name: card.name,
                    description: card.description,
                    flavor_text: card.flavor_text,
                    potato_type: card.potato_type || 'light',
                    rarity: card.rarity || 'common',
                    card_type: card.card_type || 'unit',
                    mana_cost: card.mana_cost || 0,
                    attack: card.attack || 0,
                    hp: card.hp || card.health || 0,
                    structure_hp: card.structure_hp,
                    ability_text: card.ability_text,
                    is_legendary: card.is_legendary,
                    exotic: card.exotic,
                    adjective: card.adjective,
                    trait: card.trait,
                    registry_id: card.registry_id
                  }}
                  // Removed showDownloadButton prop - download functionality removed
                  className="cursor-pointer pointer-events-auto"
                  onClick={() => {
                    // TODO: Implement card play logic

                  }}
                />
              </div>
            )
          }) || (
            <div className="text-white/60 text-sm">No cards in hand</div>
          )}
        </div>
      </div>

      {/* Player Hero (Bottom Center) */}
      {playerHero && (
        <div 
          className={`absolute bottom-4 left-1/2 transform -translate-x-1/2 z-30 transition-all duration-200 ${
            isHeroPowerMode 
              ? 'cursor-target hover:ring-4 hover:ring-green-400 hover:ring-opacity-75' 
              : ''
          }`}
          onClick={() => {
            if (isHeroPowerMode) {
              handleHeroPowerTarget('own_hero')
            }
          }}
          onTouchEnd={(e) => {
            e.preventDefault()
            if (isHeroPowerMode) {
              handleHeroPowerTarget('own_hero')
            }
          }}
          title={
            isHeroPowerMode
              ? `Heal ${playerHero.name} with Royal Decree (${playerHeroState?.hp || defaultHeroState.hp} HP)`
              : ''
          }
        >
          <HeroDisplay
            hero={playerHero}
            currentHp={playerHeroState?.hp || defaultHeroState.hp}
            maxHp={playerHeroState?.max_hp || defaultHeroState.max_hp}
            currentMana={playerHeroState?.mana || defaultHeroState.mana}
            maxMana={playerHeroState?.max_mana || defaultHeroState.max_mana}
            isOpponent={false}
            heroPowerAvailable={playerHeroState?.hero_power_available !== false}
            heroPowerOnCooldown={playerHeroState?.hero_power_used_this_turn === true || (playerHeroState?.hero_power_cooldown || 0) > 0}
            onHeroPowerClick={handleHeroPower}
          />
        </div>
      )}

      {/* Hovered Card Preview */}
      {hoveredCardIndex !== null && playerHand?.[hoveredCardIndex] && (
        <div className="absolute bottom-[480px] left-1/2 transform -translate-x-1/2 pointer-events-none z-50">
          <div className="transition-all duration-300 ease-out scale-100">
            <TradingCard
              card={{
                id: playerHand[hoveredCardIndex].id,
                name: playerHand[hoveredCardIndex].name,
                description: playerHand[hoveredCardIndex].description,
                flavor_text: playerHand[hoveredCardIndex].flavor_text,
                potato_type: playerHand[hoveredCardIndex].potato_type || 'light',
                rarity: playerHand[hoveredCardIndex].rarity || 'common',
                card_type: playerHand[hoveredCardIndex].card_type || 'unit',
                mana_cost: playerHand[hoveredCardIndex].mana_cost || 0,
                attack: playerHand[hoveredCardIndex].attack || 0,
                hp: playerHand[hoveredCardIndex].hp || playerHand[hoveredCardIndex].health || 0,
                structure_hp: playerHand[hoveredCardIndex].structure_hp,
                ability_text: playerHand[hoveredCardIndex].ability_text,
                is_legendary: playerHand[hoveredCardIndex].is_legendary,
                exotic: playerHand[hoveredCardIndex].exotic,
                adjective: playerHand[hoveredCardIndex].adjective,
                trait: playerHand[hoveredCardIndex].trait,
                registry_id: playerHand[hoveredCardIndex].registry_id
              }}
              // Removed showDownloadButton prop - download functionality removed
              className="shadow-2xl"
            />
          </div>
        </div>
      )}

      {/* Loading indicator during initialization */}
      {battleSession?.status === 'waiting_initialization' && (
        <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-75">
          <div className="text-white text-center">
            <div className="text-2xl mb-4">🎴</div>
            <h2 className="text-xl font-bold mb-3">Initializing Battle...</h2>
            <p className="text-gray-300 text-sm">Dealing cards and setting up game...</p>
          </div>
        </div>
      )}

      {/* Mulligan Phase */}
      {gameState?.phase === 'mulligan' && battleSession && (
        <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-60 p-4 z-50">
          <div className="bg-gradient-to-br from-purple-900 to-blue-900 border-2 border-purple-400 rounded-xl p-6 w-full max-w-[95vw] max-h-[95vh] overflow-y-auto">
            <div className="text-center text-white">
              <h2 className="text-3xl font-bold mb-6" style={{ fontFamily: "'Press Start 2P', monospace" }}>
                🃏 MULLIGAN PHASE
              </h2>
              <p className="text-purple-200 mb-8 text-lg">
                Choose cards to redraw from your starting hand.<br/>
                Click cards to select them, then confirm your choices.
              </p>
              
              {/* Mulligan Status */}
              <div className="mb-8">
                <div className="flex justify-center gap-8 text-base">
                  <div className={`
                    px-3 py-1 rounded-full
                    ${gameState.mulligan_completed?.[user?.id || ''] 
                      ? 'bg-green-600 text-white' 
                      : 'bg-yellow-600 text-white'
                    }
                  `}>
                    You: {gameState.mulligan_completed?.[user?.id || ''] ? 'Ready ✓' : 'Choosing...'}
                  </div>
                  {(() => {
                    const opponentId = battleSession.player1_id === user?.id ? battleSession.player2_id : battleSession.player1_id;
                    const opponentReady = gameState.mulligan_completed?.[opponentId] || false;
                    
                    return (
                      <div className={`
                        px-3 py-1 rounded-full
                        ${opponentReady ? 'bg-green-600 text-white' : 'bg-yellow-600 text-white'}
                      `}>
                        Opponent: {opponentReady ? 'Ready ✓' : 'Choosing...'}
                      </div>
                    );
                  })()}
                </div>
              </div>

              {!gameState.mulligan_completed?.[user?.id || ''] && (
                <div>
                  <p className="text-lg text-purple-300 mb-6">
                    You have {playerHand?.length || 0} cards. Click cards to select them for redrawing.
                  </p>
                  
                  {/* Show player hand for mulligan selection */}
                  <div className="mb-8">
                    <div className="flex justify-center gap-6 flex-wrap max-w-7xl mx-auto">
                      {playerHand?.map((card, index) => {
                        const isSelected = selectedMulliganCards.includes(card.id)
                        return (
                          <div
                            key={card.id || index}
                            onClick={() => toggleMulliganCard(card.id)}
                            className={`
                              relative cursor-pointer transition-all duration-200 transform hover:scale-105
                              ${isSelected ? 'ring-4 ring-red-400 ring-opacity-75 scale-95' : 'hover:ring-2 hover:ring-blue-400'}
                            `}
                            style={{ width: '250px', height: '350px' }}
                          >
                            <TradingCard
                              card={{
                                id: card.id,
                                name: card.name,
                                description: card.description,
                                flavor_text: card.flavor_text,
                                potato_type: card.potato_type || 'light',
                                rarity: card.rarity || 'common',
                                card_type: card.card_type || 'unit',
                                mana_cost: card.mana_cost || 0,
                                attack: card.attack || 0,
                                hp: card.hp || card.health || 0,
                                ability_text: card.ability_text,
                                is_legendary: card.is_legendary,
                                exotic: card.exotic,
                                adjective: card.adjective,
                                trait: card.trait
                              }}
                              // Removed showDownloadButton prop - download functionality removed
                              className={`
                                w-full h-full pointer-events-none
                                ${isSelected ? 'opacity-60' : 'opacity-100'}
                              `}
                            />
                            {isSelected && (
                              <div className="absolute inset-0 bg-red-500 bg-opacity-40 flex items-center justify-center rounded-lg">
                                <div className="bg-red-600 text-white px-4 py-2 rounded-lg text-lg font-bold shadow-lg border-2 border-red-400">
                                  🔄 REDRAW
                                </div>
                              </div>
                            )}
                          </div>
                        )
                      })}
                    </div>
                  </div>
                  
                  <div className="flex justify-center gap-6 mt-8">
                    <button
                      onClick={() => handleMulligan([])}
                      className="bg-green-600 hover:bg-green-700 text-white px-8 py-4 rounded-lg font-bold transition-colors text-lg"
                    >
                      Keep All Cards
                    </button>
                    {selectedMulliganCards.length > 0 && (
                      <button
                        onClick={() => handleMulligan(selectedMulliganCards)}
                        className="bg-blue-600 hover:bg-blue-700 text-white px-8 py-4 rounded-lg font-bold transition-colors text-lg"
                      >
                        Redraw {selectedMulliganCards.length} Card{selectedMulliganCards.length !== 1 ? 's' : ''}
                      </button>
                    )}
                  </div>
                </div>
              )}

              {gameState.mulligan_completed?.[user?.id || ''] && (
                <p className="text-green-300">
                  ✅ Waiting for opponent to complete mulligan...
                </p>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Surrender Confirmation Modal */}
      {showSurrenderConfirm && (
        <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-70 z-60">
          <div className="bg-gradient-to-br from-red-900 to-red-800 border-2 border-red-500 rounded-xl p-8 max-w-md mx-4">
            <div className="text-center text-white">
              <h2 className="text-2xl font-bold mb-4" style={{ fontFamily: "'Press Start 2P', monospace" }}>
                🏳️ SURRENDER MATCH?
              </h2>
              <p className="text-red-200 mb-6 text-lg">
                Are you sure you want to forfeit this battle?<br/>
                This will count as a loss and cannot be undone.
              </p>
              
              <div className="flex justify-center gap-4">
                <button
                  onClick={() => setShowSurrenderConfirm(false)}
                  className="bg-gray-600 hover:bg-gray-700 text-white px-6 py-3 rounded-lg font-bold transition-colors"
                >
                  Cancel
                </button>
                <button
                  onClick={handleSurrender}
                  className="bg-red-600 hover:bg-red-700 text-white px-6 py-3 rounded-lg font-bold transition-colors"
                >
                  Surrender
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Victory/Defeat Screen */}
      {((gameState?.phase === 'ended' && gameState?.winner_id) || (battleSession?.status === 'finished' && battleSession?.winner_id)) && (
        <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-80 z-60">
          <div className={`border-2 rounded-xl p-12 max-w-lg mx-4 ${
            (gameState?.winner_id || battleSession?.winner_id) === user?.id 
              ? 'bg-gradient-to-br from-green-900 to-green-800 border-green-400' 
              : 'bg-gradient-to-br from-red-900 to-red-800 border-red-400'
          }`}>
            <div className="text-center text-white">
              <h1 className={`text-4xl font-bold mb-6 ${
                (gameState?.winner_id || battleSession?.winner_id) === user?.id ? 'text-green-200' : 'text-red-200'
              }`} style={{ fontFamily: "'Press Start 2P', monospace" }}>
                {(gameState?.winner_id || battleSession?.winner_id) === user?.id ? '🏆 VICTORY!' : '💀 DEFEAT'}
              </h1>
              
              <p className={`text-xl mb-8 ${
                (gameState?.winner_id || battleSession?.winner_id) === user?.id ? 'text-green-300' : 'text-red-300'
              }`}>
                {(gameState?.winner_id || battleSession?.winner_id) === user?.id 
                  ? 'Congratulations! You won the battle!' 
                  : 'Better luck next time, warrior!'
                }
              </p>
              
              <button
                onClick={() => window.location.href = '/'}
                className={`px-8 py-4 rounded-lg font-bold text-lg transition-colors ${
                  (gameState?.winner_id || battleSession?.winner_id) === user?.id
                    ? 'bg-green-600 hover:bg-green-700 text-white'
                    : 'bg-red-600 hover:bg-red-700 text-white'
                }`}
              >
                EXIT
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Drag Preview - Floating Card */}
      {isDragging && draggedCard && dragPosition && (
        <div
          className="fixed pointer-events-none z-[9999]"
          style={{
            left: dragPosition.x - 100, // Center the card on cursor
            top: dragPosition.y - 140,
            transform: 'scale(0.7) rotate(-5deg)',
            filter: 'drop-shadow(0 10px 20px rgba(0,0,0,0.5))'
          }}
        >
          <TradingCard
            card={{
              id: draggedCard.id,
              name: draggedCard.name,
              description: draggedCard.description,
              flavor_text: draggedCard.flavor_text,
              potato_type: draggedCard.potato_type || 'light',
              rarity: draggedCard.rarity || 'common',
              card_type: draggedCard.card_type || 'unit',
              mana_cost: draggedCard.mana_cost || 0,
              attack: draggedCard.attack || 0,
              hp: draggedCard.hp || draggedCard.health || 0,
              ability_text: draggedCard.ability_text,
              registry_id: draggedCard.registry_id
            }}
            className="opacity-90"
          />
        </div>
      )}
    </div>
  )
}

export default PixelArena