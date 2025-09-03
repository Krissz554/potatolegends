import React from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import { 
  Heart, 
  Zap, 
  Clock, 
  Crown, 
  Skull,
  Target,
  Shield,
  Swords,
  Package,
  Users,
  Plus,
  Minus,
  X
} from 'lucide-react'
import ElementalPixelPotato from '@/components/ElementalPixelPotato'
import TradingCard from '@/components/ui/TradingCard'

// Mana Bar Component
export const ManaBar: React.FC<{
  current: number
  max: number
  className?: string
}> = ({ current, max, className = '' }) => {
  return (
    <div className={`flex items-center gap-1 ${className}`}>
      {Array.from({ length: Math.max(max, 1) }, (_, i) => (
        <motion.div
          key={i}
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          transition={{ delay: i * 0.1 }}
          className={`w-6 h-6 rounded-full border-2 ${
            i < current 
              ? 'bg-blue-500 border-blue-600 shadow-blue-400/50 shadow-lg' 
              : 'bg-gray-200 border-gray-300'
          }`}
        >
          <Zap className={`w-3 h-3 mx-auto mt-0.5 ${i < current ? 'text-white' : 'text-gray-400'}`} />
        </motion.div>
      ))}
      <span className="ml-2 font-bold text-sm text-gray-700">
        {current}/{max}
      </span>
    </div>
  )
}

// Hero Portrait Component
export const HeroPortrait: React.FC<{
  health: number
  maxHealth: number
  playerId: string
  isActivePlayer: boolean
  isEnemy?: boolean
  className?: string
}> = ({ health, maxHealth, playerId, isActivePlayer, isEnemy = false, className = '' }) => {
  const healthPercent = (health / maxHealth) * 100
  const isCritical = healthPercent < 25
  const isDead = health <= 0

  return (
    <motion.div 
      className={`relative ${className}`}
      animate={isActivePlayer ? { 
        boxShadow: '0 0 20px rgba(59, 130, 246, 0.6)', 
        scale: 1.05 
      } : {}}
      transition={{ duration: 0.3 }}
    >
      <div className={`relative w-16 h-16 sm:w-20 sm:h-20 rounded-full border-4 ${
        isActivePlayer 
          ? 'border-blue-500 shadow-blue-400/50 shadow-lg' 
          : 'border-gray-300'
      } overflow-hidden bg-gradient-to-br from-amber-100 to-orange-200`}>
        
        {/* Health Circle Background */}
        <div className="absolute inset-0 rounded-full">
          <div 
            className="absolute inset-0 rounded-full bg-gradient-to-br from-red-400 to-red-600 transition-all duration-500"
            style={{ 
              clipPath: `circle(${Math.max(10, healthPercent)}% at center)`,
              opacity: healthPercent < 50 ? 0.8 : 0
            }}
          />
        </div>

        {/* Potato Avatar */}
        <div className="absolute inset-2 rounded-full overflow-hidden">
          <ElementalPixelPotato 
            variant="hero"
            seed={playerId}
            rarity="common"
            size={isEnemy ? 48 : 56}
            className={`w-full h-full ${isCritical ? 'animate-pulse' : ''} ${isDead ? 'grayscale' : ''}`}
          />
        </div>

        {/* Health Text Overlay */}
        <div className="absolute inset-0 flex items-center justify-center">
          <span className={`font-bold text-xs sm:text-sm drop-shadow-lg ${
            isCritical ? 'text-red-100' : 'text-white'
          }`}>
            {health}
          </span>
        </div>

        {/* Status Icons */}
        {isActivePlayer && (
          <motion.div 
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            className="absolute -top-1 -right-1 w-6 h-6 bg-blue-500 rounded-full flex items-center justify-center"
          >
            <Crown className="w-3 h-3 text-white" />
          </motion.div>
        )}

        {isDead && (
          <motion.div 
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            className="absolute inset-0 bg-black/50 rounded-full flex items-center justify-center"
          >
            <Skull className="w-6 h-6 text-white" />
          </motion.div>
        )}
      </div>

      {/* Health Bar */}
      <div className="mt-2 w-full">
        <Progress 
          value={healthPercent} 
          className={`h-2 ${isCritical ? 'animate-pulse' : ''}`}
        />
        <div className="flex justify-between text-xs text-gray-600 mt-1">
          <span>{health} HP</span>
          <Heart className="w-3 h-3" />
        </div>
      </div>
    </motion.div>
  )
}

// Enemy Zone Component
export const EnemyZone: React.FC<{
  hero: any
  hand: any[]
  deck: any[]
  units: any[]
  graveyard: any[]
  isActivePlayer: boolean
  selectedAttacker?: any
  onTargetClick?: (target: any) => void
  enemyPlayerKey?: string
}> = ({ hero, hand, deck, units, graveyard, isActivePlayer, selectedAttacker, onTargetClick, enemyPlayerKey }) => {
  if (!hero) return null

  return (
    <motion.div 
      initial={{ y: -50, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      className="p-3 sm:p-4 bg-gradient-to-b from-red-100/50 to-transparent"
    >
      <div className="flex items-center justify-between">
        
        {/* Enemy Info */}
        <div className="flex items-center gap-4">
          <div 
            className={`cursor-pointer transition-all ${
              selectedAttacker ? 'hover:ring-2 hover:ring-red-500 hover:shadow-lg' : ''
            }`}
            onClick={() => selectedAttacker && onTargetClick && onTargetClick({ 
              id: `${enemyPlayerKey}_hero`, 
              type: 'hero', 
              hp: hero.hp, 
              max_hp: hero.max_hp 
            })}
          >
            <HeroPortrait
              health={hero.hp}
              maxHealth={hero.max_hp}
              playerId="enemy"
              isActivePlayer={isActivePlayer}
              isEnemy={true}
            />
            {selectedAttacker && (
              <div className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold animate-pulse">
                <Target className="w-4 h-4" />
              </div>
            )}
          </div>
          
          <div className="flex flex-col gap-2">
            <ManaBar current={hero.mana} max={hero.max_mana} />
            <div className="flex items-center gap-3 text-sm text-gray-600">
              <div className="flex items-center gap-1">
                <Package className="w-4 h-4" />
                <span>{deck.length}</span>
              </div>
              <div className="flex items-center gap-1">
                <Users className="w-4 h-4" />
                <span>{hand.length}</span>
              </div>
              <div className="flex items-center gap-1">
                <Skull className="w-4 h-4" />
                <span>{graveyard.length}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Enemy Hand (Face Down Cards) */}
        <div className="flex gap-1">
          {hand.slice(0, Math.min(7, hand.length)).map((_, index) => (
            <motion.div
              key={index}
              initial={{ rotateY: 0 }}
              animate={{ rotateY: 0 }}
              className="w-8 h-12 bg-gradient-to-br from-blue-800 to-blue-900 rounded border border-blue-700 shadow-md"
            />
          ))}
          {hand.length > 7 && (
            <div className="w-8 h-12 bg-blue-900 rounded border border-blue-700 flex items-center justify-center">
              <Plus className="w-3 h-3 text-white" />
            </div>
          )}
        </div>
      </div>

      {/* Enemy Units Row */}
      <div className="mt-4 flex gap-2 min-h-[120px] bg-red-50/30 rounded-lg p-2 border border-red-200/50">
        <AnimatePresence>
          {units.map((unit, index) => (
            <motion.div
              key={unit.id || index}
              initial={{ scale: 0, y: -20 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0, y: -20 }}
              whileHover={{ scale: 1.05, z: 10 }}
              className={`relative cursor-pointer transition-all ${
                selectedAttacker ? 'hover:ring-2 hover:ring-red-500 hover:shadow-lg' : ''
              }`}
              onClick={() => selectedAttacker && onTargetClick && onTargetClick({
                id: unit.id,
                type: 'unit',
                attack: unit.attack || unit.current_attack,
                hp: unit.hp || unit.current_health || unit.health,
                ...unit
              })}
            >
              <TradingCard
                card={unit}
                owned={0} // Remove owned count in battle
                inDeck={0}
                canAdd={false}
                showInGameStats={true} // Show HP and attack in battle
                className="cursor-pointer hover:shadow-lg transition-shadow"
              />
              
              {/* Target Indicator */}
              {selectedAttacker && (
                <div className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold animate-pulse">
                  <Target className="w-3 h-3" />
                </div>
              )}
              
              {/* Unit Status Indicators */}
              {unit.summoning_sickness && (
                <div className="absolute -top-1 -left-1 w-4 h-4 bg-yellow-500 rounded-full flex items-center justify-center">
                  <Clock className="w-2 h-2 text-white" />
                </div>
              )}
            </motion.div>
          ))}
        </AnimatePresence>
        
        {units.length === 0 && (
          <div className="flex-1 flex items-center justify-center text-gray-400 text-sm">
            No units deployed
          </div>
        )}
      </div>
    </motion.div>
  )
}

// Battlefield Component
export const Battlefield: React.FC<{
  myUnits: any[]
  enemyUnits: any[]
  selectedAttacker: any
  onSelectAttacker: (unit: any) => void
  onAttack: (attacker: any, target: any) => void
  onPlayCard: (card: any) => void
  isMyTurn: boolean
}> = ({ myUnits, enemyUnits, selectedAttacker, onSelectAttacker, onAttack, onPlayCard, isMyTurn }) => {
  const [isDragOver, setIsDragOver] = React.useState(false)
  return (
    <motion.div 
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className={`flex-1 relative bg-gradient-to-br from-green-100/30 to-yellow-100/30 border-y border-amber-200 ${
        isDragOver ? 'bg-green-200/50 border-green-400' : ''
      }`}
      onDragOver={(e) => {
        console.log('Drag over battlefield, isMyTurn:', isMyTurn)
        if (isMyTurn) {
          e.preventDefault()
          setIsDragOver(true)
        }
      }}
      onDragLeave={() => {
        console.log('Drag leave battlefield')
        setIsDragOver(false)
      }}
      onDrop={(e) => {
        console.log('Drop event on battlefield, isMyTurn:', isMyTurn)
        e.preventDefault()
        setIsDragOver(false)
        
        if (!isMyTurn) {
          console.log('Drop rejected: not player turn')
          return
        }
        
        try {
          const cardData = e.dataTransfer.getData('cardData')
          console.log('Card data from drop:', cardData)
          if (cardData) {
            const { card, index } = JSON.parse(cardData)
            console.log('Card dropped:', card.name, 'at hand index:', index)
            onPlayCard(card)  // Call with just the card, not the index
          }
        } catch (error) {
          console.error('Error parsing dropped card data:', error)
        }
      }}
    >
      {/* Combat Arrows */}
      <AnimatePresence>
        {selectedAttacker && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="absolute inset-0 pointer-events-none z-10"
          >
            {/* Attack targeting lines would go here */}
            <div className="absolute inset-0 bg-gradient-to-t from-red-500/10 to-transparent" />
          </motion.div>
        )}
      </AnimatePresence>

      {/* Battlefield Center */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
        {isDragOver ? (
          <motion.div
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="bg-green-500 text-white px-6 py-3 rounded-lg shadow-lg"
          >
            <div className="flex items-center gap-2">
              <Plus className="w-5 h-5" />
              <span className="font-semibold">Drop card here to play</span>
            </div>
          </motion.div>
        ) : (
          <motion.div 
            animate={{ rotate: 360 }}
            transition={{ duration: 20, repeat: Infinity, ease: "linear" }}
            className="w-32 h-32 opacity-5"
          >
            <Swords className="w-full h-full text-amber-600" />
          </motion.div>
        )}
      </div>

      {/* Combat Text */}
      {selectedAttacker && (
        <motion.div
          initial={{ scale: 0, y: 20 }}
          animate={{ scale: 1, y: 0 }}
          className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 z-20"
        >
          <div className="bg-red-600 text-white px-4 py-2 rounded-full shadow-lg">
            <Target className="w-4 h-4 inline mr-2" />
            Select Target
          </div>
        </motion.div>
      )}
    </motion.div>
  )
}

// Player Zone Component
export const PlayerZone: React.FC<{
  hero: any
  hand: any[]
  deck: any[]
  units: any[]
  graveyard: any[]
  isActivePlayer: boolean
  selectedCard: any
  onSelectCard: (card: any) => void
  onSelectUnit: (unit: any) => void
  onPlayCard: (card: any) => void
  onEndTurn: () => void
  onHeroPower: () => void
  isProcessing: boolean
  heroPowerActive: boolean
}> = ({ 
  hero, 
  hand, 
  deck, 
  units, 
  graveyard, 
  isActivePlayer, 
  selectedCard, 
  onSelectCard, 
  onSelectUnit,
  onPlayCard, 
  onEndTurn, 
  onHeroPower,
  isProcessing,
  heroPowerActive
}) => {
  
  console.log('👤 PlayerZone render:', {
    heroMana: hero?.mana,
    heroMaxMana: hero?.max_mana,
    isActivePlayer,
    handSize: hand?.length,
    isProcessing,
    firstCardSample: hand?.[0] ? {
      id: hand[0].id,
      name: hand[0].name,
      mana_cost: hand[0].mana_cost,
      attack: hand[0].attack,
      hp: hand[0].hp,
      hasNestedPotato: !!hand[0].potato
    } : null
  })
  if (!hero) return null

  return (
    <motion.div 
      initial={{ y: 50, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      className="p-3 sm:p-4 bg-gradient-to-t from-blue-100/50 to-transparent"
    >
      {/* My Units Row */}
      <div className="mb-4 flex gap-2 min-h-[120px] bg-blue-50/30 rounded-lg p-2 border border-blue-200/50">
        <AnimatePresence>
          {units.map((unit, index) => (
            <motion.div
              key={unit.id || index}
              initial={{ scale: 0, y: 20 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0, y: 20 }}
              whileHover={{ scale: 1.05, z: 10 }}
              className={`relative cursor-pointer transition-all duration-200 ${
                isActivePlayer && !unit.has_attacked && !unit.summoning_sickness 
                  ? 'hover:ring-2 hover:ring-orange-500 hover:scale-105 ring-2 ring-orange-300 ring-opacity-50' 
                  : '' // Remove graying from deployed cards - they're always visible
              }`}
              onClick={() => {
                if (isActivePlayer && !unit.has_attacked && !unit.summoning_sickness) {
                  console.log('🗡️ Unit selected for attack:', unit.name, unit.id)
                  onSelectUnit(unit)
                }
              }}
            >
              <TradingCard
                card={unit}
                owned={0} // Remove owned count in battle
                inDeck={0}
                canAdd={false}
                showInGameStats={true} // Show HP and attack in battle
                className={`transition-all ${
                  selectedCard?.id === unit.id ? 'ring-2 ring-blue-500 shadow-lg' : ''
                } ${
                  isActivePlayer && !unit.has_attacked && !unit.summoning_sickness ? 'hover:shadow-lg' : ''
                }`}
              />
              
              {/* Unit Status */}
              <div className="absolute bottom-0 left-0 right-0 flex justify-between p-1">
                <Badge variant="secondary" className="text-xs bg-red-600 text-white">
                  {unit.current_attack || unit.attack}
                </Badge>
                <Badge variant="secondary" className="text-xs bg-green-600 text-white">
                  {unit.current_health || unit.health || unit.hp}
                </Badge>
              </div>
              
              {/* Attack Ready Indicator */}
              {isActivePlayer && !unit.has_attacked && !unit.summoning_sickness && (
                <div className="absolute -top-2 -right-2 bg-green-500 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold animate-pulse">
                  <Swords className="w-3 h-3" />
                </div>
              )}
              
              {/* Summoning Sickness Indicator */}
              {unit.summoning_sickness && (
                <div className="absolute -top-2 -left-2 bg-yellow-500 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold">
                  <Clock className="w-3 h-3" />
                </div>
              )}
              
              {/* Already Attacked Indicator */}
              {unit.has_attacked && (
                <div className="absolute -top-2 -left-2 bg-gray-500 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold">
                  <Shield className="w-3 h-3" />
                </div>
              )}
            </motion.div>
          ))}
        </AnimatePresence>
        
        {units.length === 0 && (
          <div className="flex-1 flex items-center justify-center text-gray-400 text-sm">
            Deploy units to defend yourself
          </div>
        )}
      </div>

      {/* Player Info & Controls */}
      <div className="flex items-center justify-between">
        
        {/* Player Info */}
        <div className="flex items-center gap-4">
          <HeroPortrait
            health={hero.hp}
            maxHealth={hero.max_hp}
            playerId="player"
            isActivePlayer={isActivePlayer}
          />
          
          <div className="flex flex-col gap-2">
            <ManaBar current={hero.mana} max={hero.max_mana} />
            <div className="flex items-center gap-3 text-sm text-gray-600">
              <div className="flex items-center gap-1">
                <Package className="w-4 h-4" />
                <span>{deck.length}</span>
              </div>
              <div className="flex items-center gap-1">
                <Users className="w-4 h-4" />
                <span>{hand.length}</span>
              </div>
              <div className="flex items-center gap-1">
                <Skull className="w-4 h-4" />
                <span>{graveyard.length}</span>
              </div>
            </div>
          </div>
          
          {/* Hero Power Button */}
          <motion.div
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
          >
            <Button
              onClick={onHeroPower}
              disabled={!isActivePlayer || isProcessing || hero.mana < 2 || hero.hero_power_used_this_turn}
              variant={heroPowerActive ? "default" : "outline"}
              size="sm"
              className={`flex flex-col items-center p-2 h-16 w-20 ${
                heroPowerActive 
                  ? 'bg-purple-600 hover:bg-purple-700 text-white ring-2 ring-purple-400' 
                  : 'border-purple-500 text-purple-600 hover:bg-purple-50'
              } ${
                hero.hero_power_used_this_turn 
                  ? 'opacity-50 cursor-not-allowed' 
                  : ''
              }`}
            >
              <Crown className="w-4 h-4 mb-1" />
              {heroPowerActive ? (
                <>
                  <span className="text-xs font-bold">Cancel</span>
                  <span className="text-xs">Power</span>
                </>
              ) : (
                <>
                  <span className="text-xs font-bold">2</span>
                  <span className="text-xs">Mana</span>
                </>
              )}
            </Button>
          </motion.div>
        </div>

        {/* End Turn Button */}
        <motion.div
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
        >
          <Button
            onClick={onEndTurn}
            disabled={!isActivePlayer || isProcessing}
            size="lg"
            className={`${
              isActivePlayer 
                ? 'bg-green-600 hover:bg-green-700 text-white shadow-lg' 
                : 'bg-gray-300 text-gray-500 cursor-not-allowed'
            } transition-all duration-200`}
          >
            {isProcessing ? (
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
            ) : (
              <Swords className="w-4 h-4 mr-2" />
            )}
            End Turn
          </Button>
        </motion.div>
      </div>

      {/* Hand */}
      <div className="mt-4 overflow-x-auto">
        <div className="flex gap-2 pb-2 min-w-max">
          <AnimatePresence>
            {hand.map((card, index) => (
              <motion.div
                key={card.id || index}
                initial={{ y: 50, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                exit={{ y: 50, opacity: 0 }}
                whileHover={card.mana_cost <= hero.mana && isActivePlayer ? { y: -10, scale: 1.05 } : {}}
                transition={{ delay: index * 0.1 }}
                className="relative"
              >
                <div
                  className={`relative transition-all duration-200 ${
                    card.mana_cost <= hero.mana && isActivePlayer 
                      ? 'cursor-grab active:cursor-grabbing ring-2 ring-green-400 ring-opacity-50 hover:ring-green-500' 
                      : 'cursor-pointer opacity-60 grayscale'
                  }`}
                  draggable={card.mana_cost <= hero.mana && isActivePlayer}
                  onDragStart={(e) => {
                    console.log('Drag start attempted for card:', card.name, 'Mana cost:', card.mana_cost, 'Hero mana:', hero.mana, 'Is active player:', isActivePlayer)
                    if (card.mana_cost <= hero.mana && isActivePlayer) {
                      console.log('Drag start allowed for card:', card.name)
                      e.dataTransfer.setData('cardData', JSON.stringify({ card, index }))
                      e.dataTransfer.effectAllowed = 'move'
                      // Add visual feedback
                      e.currentTarget.style.opacity = '0.5'
                    } else {
                      console.log('Drag start prevented for card:', card.name, 'Reason: mana or not active player')
                      e.preventDefault()
                    }
                  }}
                  onDragEnd={(e) => {
                    // Reset visual effects
                    e.currentTarget.style.opacity = '1'
                  }}
                  onClick={() => {
                    console.log('🖱️ Hand card clicked:', {
                      cardName: card.name,
                      cardManaCost: card.mana_cost,
                      heroMana: hero.mana,
                      isActivePlayer,
                      canPlay: card.mana_cost <= hero.mana && isActivePlayer
                    })
                    
                    // If card is playable and it's player's turn, play it directly
                    if (card.mana_cost <= hero.mana && isActivePlayer) {
                      console.log('🃏 Playing card directly:', card.name)
                      onPlayCard(card)
                    } else {
                      console.log('🖱️ Hand card selected (not playable):', card.name)
                      onSelectCard(card)
                    }
                  }}
                >
                  <TradingCard
                    card={{
                      ...card,
                      // Debug: Log what we're passing to TradingCard
                      debug: console.log('🎴 TradingCard receives:', {
                        id: card.id,
                        name: card.name,
                        mana_cost: card.mana_cost,
                        attack: card.attack,
                        hp: card.hp,
                        potato_type: card.potato_type,
                        rarity: card.rarity,
                        allKeys: Object.keys(card)
                      })
                    }}
                    owned={0} // Remove owned count in battle
                    inDeck={0}
                    canAdd={card.mana_cost <= hero.mana && isActivePlayer}
                    className={`w-24 h-36 transition-all ${
                      selectedCard?.id === card.id ? 'ring-2 ring-blue-500' : ''
                    } ${
                      card.mana_cost <= hero.mana && isActivePlayer 
                        ? 'hover:shadow-xl' 
                        : 'opacity-75'
                    }`}
                  />
                  
                  {/* Playable Indicator */}
                  {card.mana_cost <= hero.mana && isActivePlayer && (
                    <motion.div 
                      animate={{ boxShadow: ['0 0 0 0 rgba(34, 197, 94, 0.7)', '0 0 0 4px rgba(34, 197, 94, 0)'] }}
                      transition={{ duration: 1.5, repeat: Infinity }}
                      className="absolute inset-0 rounded-lg pointer-events-none"
                    />
                  )}
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        </div>
      </div>
    </motion.div>
  )
}

// Game Over Overlay
export const GameOverOverlay: React.FC<{
  isWinner: boolean
  onClose: () => void
}> = ({ isWinner, onClose }) => {
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
    >
      <motion.div
        initial={{ scale: 0.5, y: 50 }}
        animate={{ scale: 1, y: 0 }}
        exit={{ scale: 0.5, y: 50 }}
        className="bg-white rounded-xl p-8 text-center shadow-2xl max-w-md mx-4"
      >
        <motion.div
          animate={{ 
            rotate: [0, 360],
            scale: [1, 1.2, 1]
          }}
          transition={{ 
            duration: 2,
            repeat: Infinity,
            repeatType: "loop"
          }}
          className="text-8xl mb-4"
        >
          {isWinner ? '🎉' : '💔'}
        </motion.div>
        
        <h2 className="text-3xl font-bold mb-4 text-gray-800">
          {isWinner ? 'Victory!' : 'Defeat'}
        </h2>
        
        <p className="text-gray-600 mb-6">
          {isWinner 
            ? 'Congratulations! Your potatoes have triumphed!' 
            : 'Better luck next time, potato warrior!'
          }
        </p>
        
        <div className="space-y-3">
          <Button onClick={onClose} className="w-full" size="lg">
            Return to Menu
          </Button>
        </div>
      </motion.div>
    </motion.div>
  )
}

// Mulligan Overlay Component
export const MulliganOverlay: React.FC<{
  hand: any[]
  hasCompleted: boolean
  onMulligan: (cardIndices: number[]) => void
  isProcessing: boolean
}> = ({ hand, hasCompleted, onMulligan, isProcessing }) => {
  const [selectedCardIndices, setSelectedCardIndices] = React.useState<number[]>([])

  const handleCardClick = (cardIndex: number) => {
    if (hasCompleted || isProcessing) return
    
    setSelectedCardIndices(prev => 
      prev.includes(cardIndex) 
        ? prev.filter(idx => idx !== cardIndex)
        : [...prev, cardIndex]
    )
  }

  const handleConfirmMulligan = () => {
    onMulligan(selectedCardIndices)
    setSelectedCardIndices([])
  }

  if (hasCompleted) {
    return (
      <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
        <motion.div
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          className="bg-white rounded-xl p-8 max-w-md mx-4 text-center"
        >
          <div className="text-6xl mb-4">✅</div>
          <h2 className="text-2xl font-bold text-green-800 mb-2">Mulligan Complete!</h2>
          <p className="text-green-600">Waiting for opponent to finish mulligan...</p>
          <div className="mt-4">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600 mx-auto"></div>
          </div>
        </motion.div>
      </div>
    )
  }

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <motion.div
        initial={{ scale: 0.8, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        className="bg-white rounded-xl p-6 max-w-6xl w-full max-h-[90vh] overflow-auto"
      >
        <div className="text-center mb-6">
          <h2 className="text-3xl font-bold text-gray-800 mb-2">🎯 Mulligan Phase</h2>
          <p className="text-gray-600">
            Select cards you want to redraw. Click cards to select/deselect them.
          </p>
          <p className="text-sm text-gray-500 mt-2">
            Selected cards will be shuffled back into your deck and you'll draw replacements.
          </p>
        </div>

        {/* Hand Display */}
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4 mb-6">
          {hand.map((card, index) => (
            <motion.div
              key={card.id || index}
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={() => handleCardClick(index)}
              className={`cursor-pointer transition-all relative ${
                selectedCardIndices.includes(index)
                  ? 'ring-4 ring-red-500 ring-offset-2 transform scale-105'
                  : 'hover:ring-2 hover:ring-blue-300'
              }`}
            >
              <TradingCard
                card={card}
                size="small"
                className="h-40"
              />
              {selectedCardIndices.includes(index) && (
                <div className="absolute inset-0 bg-red-500/20 rounded-lg flex items-center justify-center">
                  <div className="bg-red-500 text-white rounded-full p-2">
                    <X className="w-4 h-4" />
                  </div>
                </div>
              )}
            </motion.div>
          ))}
        </div>

        {/* Actions */}
        <div className="flex items-center justify-between">
          <div className="text-sm text-gray-600">
            {selectedCardIndices.length > 0 
              ? `${selectedCardIndices.length} card${selectedCardIndices.length === 1 ? '' : 's'} selected for mulligan`
              : 'No cards selected - you will keep your current hand'
            }
          </div>
          
          <div className="flex gap-3">
            <Button
              variant="outline"
              onClick={() => setSelectedCardIndices([])}
              disabled={isProcessing || selectedCardIndices.length === 0}
            >
              Clear Selection
            </Button>
            <Button
              onClick={handleConfirmMulligan}
              disabled={isProcessing}
              className="bg-green-600 hover:bg-green-700 text-white"
            >
              {isProcessing ? (
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
              ) : null}
              {selectedCardIndices.length > 0 ? 'Confirm Mulligan' : 'Keep Hand'}
            </Button>
          </div>
        </div>
      </motion.div>
    </div>
  )
}