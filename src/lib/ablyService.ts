import Ably from 'ably'

// Ably service for real-time multiplayer functionality
class AblyService {
  private client: Ably.Realtime | null = null
  private channels: Map<string, Ably.RealtimeChannel> = new Map()

  constructor(apiKey: string, clientId?: string) {
    this.client = new Ably.Realtime({
      key: apiKey,
      clientId: clientId || null, // Must be string or null, not undefined
      echoMessages: false, // Don't echo our own messages
      autoConnect: true
    })

    // Connection event handlers
    this.client.connection.on('connected', () => {
      console.log('✅ Connected to Ably real-time network')
    })

    this.client.connection.on('disconnected', () => {
      console.log('🔌 Disconnected from Ably')
    })

    this.client.connection.on('failed', (error) => {
      console.error('❌ Ably connection failed:', error)
    })
  }

  // Get or create a channel for a match
  getMatchChannel(matchId: string): Ably.RealtimeChannel {
    const channelName = `match:${matchId}`
    
    if (!this.channels.has(channelName)) {
      if (!this.client) {
        throw new Error('Ably client not initialized')
      }
      
      const channel = this.client.channels.get(channelName)
      this.channels.set(channelName, channel)
      console.log(`📡 Created Ably channel: ${channelName}`)
    }

    return this.channels.get(channelName)!
  }

  // Subscribe to game state updates
  subscribeToGameUpdates(
    matchId: string, 
    onUpdate: (data: any) => void,
    onCombat: (data: any) => void,
    onTurnChange: (data: any) => void
  ) {
    const channel = this.getMatchChannel(matchId)

    // Subscribe to different event types
    channel.subscribe('game_state_update', (message) => {
      console.log('📊 Game state update from Ably:', message.data)
      onUpdate(message.data)
    })

    channel.subscribe('combat_result', (message) => {
      console.log('⚔️ Combat result from Ably:', message.data)
      onCombat(message.data)
    })

    channel.subscribe('turn_change', (message) => {
      console.log('🔄 Turn change from Ably:', message.data)
      onTurnChange(message.data)
    })

    return channel
  }

  // Presence tracking for online/AFK status
  async enterPresence(matchId: string, userData: any) {
    const channel = this.getMatchChannel(matchId)
    
    try {
      await channel.presence.enter({
        ...userData,
        joined_at: Date.now(),
        status: 'active'
      })
      console.log('👥 Entered presence for match:', matchId)
    } catch (error) {
      console.error('❌ Failed to enter presence:', error)
    }
  }

  // Update presence (for heartbeats)
  async updatePresence(matchId: string, userData: any) {
    const channel = this.getMatchChannel(matchId)
    
    try {
      await channel.presence.update({
        ...userData,
        last_heartbeat: Date.now(),
        status: 'active'
      })
    } catch (error) {
      console.error('❌ Failed to update presence:', error)
    }
  }

  // Leave presence
  async leavePresence(matchId: string) {
    const channel = this.getMatchChannel(matchId)
    
    try {
      await channel.presence.leave()
      console.log('👋 Left presence for match:', matchId)
    } catch (error) {
      console.error('❌ Failed to leave presence:', error)
    }
  }

  // Ping/pong for latency measurement
  async sendPing(matchId: string, callback: (latency: number) => void) {
    const channel = this.getMatchChannel(matchId)
    const pingTime = Date.now()
    
    // Listen for pong
    const pongListener = (message: Ably.Message) => {
      if (message.data.ping_time === pingTime) {
        const latency = Date.now() - pingTime
        callback(latency)
        channel.unsubscribe('pong', pongListener)
      }
    }
    
    channel.subscribe('pong', pongListener)
    
    // Send ping
    await channel.publish('ping', {
      ping_time: pingTime,
      user_id: this.client?.auth.clientId
    })
  }

  // Clean up channels
  cleanup() {
    this.channels.forEach((channel, channelName) => {
      channel.detach()
      console.log(`🧹 Detached from channel: ${channelName}`)
    })
    this.channels.clear()
    
    if (this.client) {
      this.client.close()
    }
  }

  // Get connection state
  getConnectionState() {
    return this.client?.connection.state || 'disconnected'
  }

  // Get connection latency
  getLatency(): number {
    return this.client?.connection.connectionManager?.connectionDetails?.maxIdleInterval || 0
  }
}

export default AblyService