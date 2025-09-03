// =============================================
// useMatchConnection Hook - Realtime Match Connection
// Implements rock-solid match sync with Supabase Realtime
// =============================================

import { useState, useEffect, useCallback, useRef } from 'react'
import { supabase } from '@/lib/supabase'
import { toast } from 'sonner'

// Types for match connection
export interface MatchState {
  id: string
  player1_id: string
  player2_id: string
  current_turn_player_id: string | null
  game_state: any
  status: string
  updated_at: string
  [key: string]: any
}

export interface MatchEvent {
  id: string
  match_id: string
  player_id: string
  event_type: string
  event_data: any
  created_at: string
}

export interface ConnectionStatus {
  status: 'connecting' | 'connected' | 'reconnecting' | 'disconnected' | 'error'
  isOnline: boolean
  lastSync: Date | null
  reconnectAttempts: number
}

export interface PlayerPresence {
  userId: string
  isOnline: boolean
  lastSeen: string
}

interface UseMatchConnectionProps {
  matchId: string
  userId: string
  onMatchStateUpdate: (matchState: MatchState) => void
  onMatchEvent?: (event: MatchEvent) => void
  onPlayerPresenceUpdate?: (presence: PlayerPresence[]) => void
}

export const useMatchConnection = ({
  matchId,
  userId,
  onMatchStateUpdate,
  onMatchEvent,
  onPlayerPresenceUpdate
}: UseMatchConnectionProps) => {
  // Connection state
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>({
    status: 'connecting',
    isOnline: false,
    lastSync: null,
    reconnectAttempts: 0
  })

  // Refs for cleanup and reconnection
  const channelRef = useRef<any>(null)
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null)
  const lastKnownUpdatedAt = useRef<string>('')
  const maxReconnectAttempts = 5
  const baseReconnectDelay = 1000 // Start with 1 second

  // Fetch latest match state snapshot
  const fetchMatchSnapshot = useCallback(async (): Promise<MatchState | null> => {
    try {
      console.log('📸 Fetching match state snapshot for:', matchId)
      
      const { data: matchState, error } = await supabase
        .from('battle_sessions') // Using existing table name
        .select('*')
        .eq('id', matchId)
        .single()

      if (error) {
        console.error('❌ Error fetching match snapshot:', error)
        return null
      }

      console.log('✅ Match snapshot fetched, updated_at:', matchState.updated_at)
      lastKnownUpdatedAt.current = matchState.updated_at
      
      return matchState as MatchState
    } catch (error) {
      console.error('❌ Exception fetching match snapshot:', error)
      return null
    }
  }, [matchId])

  // Check if state is stale and needs resync
  const isStateStale = useCallback((serverUpdatedAt: string): boolean => {
    if (!lastKnownUpdatedAt.current) return true
    return new Date(serverUpdatedAt) > new Date(lastKnownUpdatedAt.current)
  }, [])

  // Handle reconnection with exponential backoff
  const scheduleReconnect = useCallback(() => {
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current)
    }

    const attempts = connectionStatus.reconnectAttempts
    if (attempts >= maxReconnectAttempts) {
      console.error('❌ Max reconnection attempts reached')
      setConnectionStatus(prev => ({
        ...prev,
        status: 'error'
      }))
      toast.error('Connection lost. Please refresh the page.')
      return
    }

    const delay = baseReconnectDelay * Math.pow(2, attempts) // Exponential backoff
    console.log(`🔄 Scheduling reconnect attempt ${attempts + 1} in ${delay}ms`)
    
    setConnectionStatus(prev => ({
      ...prev,
      status: 'reconnecting',
      reconnectAttempts: attempts + 1
    }))

    reconnectTimeoutRef.current = setTimeout(() => {
      setupConnection()
    }, delay)
  }, [connectionStatus.reconnectAttempts])

  // Setup realtime connection
  const setupConnection = useCallback(async () => {
    try {
      console.log('🚀 Setting up match connection for:', matchId)
      
      // Clean up existing connection
      if (channelRef.current) {
        console.log('🧹 Cleaning up existing connection')
        supabase.removeChannel(channelRef.current)
        channelRef.current = null
      }

      // Fetch initial state snapshot
      const initialState = await fetchMatchSnapshot()
      if (initialState) {
        onMatchStateUpdate(initialState)
        setConnectionStatus(prev => ({
          ...prev,
          lastSync: new Date()
        }))
      }

      // Create realtime channel with proper naming
      const channel = supabase
        .channel(`match:${matchId}`)
        .on(
          'postgres_changes',
          {
            event: 'UPDATE',
            schema: 'public',
            table: 'battle_sessions',
            filter: `id=eq.${matchId}`
          },
          async (payload) => {
            console.log('📡 Match state update received:', payload.new)
            
            const newState = payload.new as MatchState
            
            // Check if we missed any updates (stale state detection)
            if (isStateStale(newState.updated_at)) {
              console.log('⚠️ Potentially stale state detected, fetching fresh snapshot')
              const freshState = await fetchMatchSnapshot()
              if (freshState) {
                onMatchStateUpdate(freshState)
              }
            } else {
              // State is fresh, use the update directly
              lastKnownUpdatedAt.current = newState.updated_at
              onMatchStateUpdate(newState)
            }
            
            setConnectionStatus(prev => ({
              ...prev,
              lastSync: new Date(),
              reconnectAttempts: 0 // Reset on successful update
            }))
          }
        )
        .on(
          'postgres_changes',
          {
            event: 'INSERT',
            schema: 'public', 
            table: 'battle_actions', // Match events
            filter: `battle_session_id=eq.${matchId}`
          },
          (payload) => {
            console.log('📡 Match event received:', payload.new)
            if (onMatchEvent) {
              onMatchEvent(payload.new as MatchEvent)
            }
          }
        )
        .on('presence', { event: 'sync' }, () => {
          console.log('👥 Presence sync')
          const newState = channel.presenceState()
          
          // Convert presence state to PlayerPresence format
          const presence: PlayerPresence[] = Object.keys(newState).map(key => ({
            userId: key,
            isOnline: true,
            lastSeen: new Date().toISOString()
          }))
          
          if (onPlayerPresenceUpdate) {
            onPlayerPresenceUpdate(presence)
          }
        })
        .on('presence', { event: 'join' }, ({ key, newPresences }) => {
          console.log('👋 Player joined:', key)
        })
        .on('presence', { event: 'leave' }, ({ key, leftPresences }) => {
          console.log('👋 Player left:', key)
        })
        .subscribe(async (status, err) => {
          console.log('📡 Match connection status:', status)
          
          if (err) {
            console.error('❌ Match connection error:', err)
            setConnectionStatus(prev => ({
              ...prev,
              status: 'error',
              isOnline: false
            }))
            scheduleReconnect()
            return
          }

          switch (status) {
            case 'SUBSCRIBED':
              console.log('✅ Match connection established')
              
              // Track presence
              await channel.track({
                userId,
                status: 'online',
                joinedAt: new Date().toISOString()
              })
              
              setConnectionStatus(prev => ({
                ...prev,
                status: 'connected',
                isOnline: true,
                reconnectAttempts: 0
              }))
              
              // Clear any pending reconnection
              if (reconnectTimeoutRef.current) {
                clearTimeout(reconnectTimeoutRef.current)
                reconnectTimeoutRef.current = null
              }
              break
              
            case 'CHANNEL_ERROR':
            case 'TIMED_OUT':
            case 'CLOSED':
              console.log('🔄 Match connection lost, attempting reconnect...')
              setConnectionStatus(prev => ({
                ...prev,
                status: 'disconnected',
                isOnline: false
              }))
              scheduleReconnect()
              break
          }
        })

      channelRef.current = channel
      
    } catch (error) {
      console.error('❌ Error setting up match connection:', error)
      setConnectionStatus(prev => ({
        ...prev,
        status: 'error',
        isOnline: false
      }))
      scheduleReconnect()
    }
  }, [matchId, userId, onMatchStateUpdate, onMatchEvent, onPlayerPresenceUpdate, fetchMatchSnapshot, isStateStale, scheduleReconnect])

  // Handle visibility change for resync
  const handleVisibilityChange = useCallback(async () => {
    if (!document.hidden && connectionStatus.status === 'connected') {
      console.log('👁️ Tab became visible, checking for state sync')
      
      // Force a fresh snapshot check when tab becomes visible
      const freshState = await fetchMatchSnapshot()
      if (freshState) {
        onMatchStateUpdate(freshState)
        setConnectionStatus(prev => ({
          ...prev,
          lastSync: new Date()
        }))
      }
    }
  }, [connectionStatus.status, fetchMatchSnapshot, onMatchStateUpdate])

  // Manual resync function
  const forceResync = useCallback(async () => {
    console.log('🔄 Manual resync requested')
    const freshState = await fetchMatchSnapshot()
    if (freshState) {
      onMatchStateUpdate(freshState)
      setConnectionStatus(prev => ({
        ...prev,
        lastSync: new Date()
      }))
    }
  }, [fetchMatchSnapshot, onMatchStateUpdate])

  // Disconnect function
  const disconnect = useCallback(() => {
    console.log('🔌 Disconnecting from match')
    
    if (channelRef.current) {
      supabase.removeChannel(channelRef.current)
      channelRef.current = null
    }
    
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current)
      reconnectTimeoutRef.current = null
    }
    
    setConnectionStatus({
      status: 'disconnected',
      isOnline: false,
      lastSync: null,
      reconnectAttempts: 0
    })
  }, [])

  // Setup connection on mount and dependencies change
  useEffect(() => {
    if (matchId && userId) {
      setupConnection()
    }
    
    return () => {
      disconnect()
    }
  }, [matchId, userId]) // Only reconnect when these core params change

  // Handle page visibility changes
  useEffect(() => {
    document.addEventListener('visibilitychange', handleVisibilityChange)
    window.addEventListener('focus', handleVisibilityChange)
    
    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange)
      window.removeEventListener('focus', handleVisibilityChange)
    }
  }, [handleVisibilityChange])

  return {
    connectionStatus,
    forceResync,
    disconnect
  }
}