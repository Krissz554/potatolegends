import React, { useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import TradingCard from '@/components/ui/TradingCard'

interface HandProps {
  cards: any[]
  isPlayer: boolean
  isActive: boolean
  selectedCard?: any
  onCardSelect: (card: any) => void
  onCardPlay: (card: any) => void
  canAffordCard: (card: any) => boolean
  className?: string
}

interface CardDetailsModalProps {
  card: any
  isOpen: boolean
  onClose: () => void
}

/**
 * Hand: Pixel-styled card hand with fanning and details modal
 * Features: Responsive fanning, hover magnification, pixel parchment details
 */
export const Hand: React.FC<HandProps> = ({
  cards,
  isPlayer,
  isActive,
  selectedCard,
  onCardSelect,
  onCardPlay,
  canAffordCard,
  className = ''
}) => {
  const [hoveredCard, setHoveredCard] = useState<any>(null)
  const [detailsCard, setDetailsCard] = useState<any>(null)

  // Calculate fan positioning
  const getFanPosition = (index: number, total: number) => {
    if (total <= 1) return { x: 0, y: 0, rotation: 0 }
    
    const maxFanAngle = Math.min(60, total * 8) // Max 60 degrees fan
    const angleStep = maxFanAngle / (total - 1)
    const angle = (index * angleStep) - (maxFanAngle / 2)
    const cardWidth = 120
    const radius = cardWidth * 2
    
    const x = Math.sin(angle * Math.PI / 180) * radius * 0.3
    const y = Math.cos(angle * Math.PI / 180) * radius * 0.1
    
    return {
      x: x,
      y: Math.abs(y),
      rotation: angle
    }
  }

  if (!isPlayer) {
    // Enemy hand - show card backs
    return (
      <div className={`flex justify-center items-center ${className}`}>

        <div className="flex items-center gap-1 max-w-4xl overflow-x-auto">
          {cards.map((_, index) => (
            <motion.div
              key={index}
              initial={{ y: -20, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              transition={{ delay: index * 0.05 }}
              className="w-16 h-24 bg-gradient-to-br from-slate-700 to-slate-800 border-2 border-red-400/50 rounded-lg shadow-lg flex-shrink-0"
                              style={{
                  imageRendering: 'pixelated'
                }}
            >
              {/* Card Back Design */}
              <div className="w-full h-full rounded-md bg-gradient-to-br from-red-900/40 to-red-800/60 border border-red-400/30 flex items-center justify-center">
                <div className="text-red-400 text-xs font-bold transform -rotate-12">?</div>
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    )
  }

  return (
    <>
      <div className={`relative ${className}`}>


        {/* Cards Container */}
        <div className="relative flex justify-center items-end h-40 overflow-visible">
          <AnimatePresence>
            {cards.map((card, index) => {
              const fanPos = getFanPosition(index, cards.length)
              const isHovered = hoveredCard?.id === card.id
              const isSelected = selectedCard?.id === card.id
              const canAfford = canAffordCard(card)
              const canPlay = canAfford && isActive

              return (
                <motion.div
                  key={card.id || index}
                  className="absolute cursor-pointer"
                  style={{
                    zIndex: isHovered ? 20 : isSelected ? 15 : 10 - index,
                    imageRendering: 'pixelated'
                  }}
                  initial={{ 
                    y: 100, 
                    opacity: 0, 
                    scale: 0.8,
                    rotate: fanPos.rotation 
                  }}
                  animate={{ 
                    x: fanPos.x,
                    y: isHovered ? fanPos.y - 40 : fanPos.y,
                    opacity: 1,
                    scale: isHovered ? 1.15 : isSelected ? 1.05 : 1,
                    rotate: isHovered ? 0 : fanPos.rotation
                  }}
                  exit={{ 
                    y: -100, 
                    opacity: 0, 
                    scale: 0.8,
                    transition: { duration: 0.3 }
                  }}
                  transition={{ 
                    delay: index * 0.1,
                    type: "spring",
                    stiffness: 200,
                    damping: 20
                  }}
                  onHoverStart={() => setHoveredCard(card)}
                  onHoverEnd={() => setHoveredCard(null)}
                  onClick={() => {
                    if (canPlay) {
                      onCardPlay(card)
                    } else {
                      onCardSelect(card)
                    }
                  }}
                  onDoubleClick={() => setDetailsCard(card)}
                  whileTap={{ scale: 0.95 }}
                >
                  <div 
                    className={`w-32 transition-all duration-200 ${
                      !canPlay ? 'opacity-50 grayscale' : 'hover:drop-shadow-2xl hover:brightness-110'
                    } ${
                      isSelected ? 'ring-4 ring-yellow-400 rounded-lg' : ''
                    }`}
                  >
                    <TradingCard
                      card={card}
                      owned={1}
                      // Removed showDownloadButton prop - download functionality removed
                      className={`
                        !w-full !max-w-none aspect-[2/3] !min-w-0 !min-h-0 border-4
                        ${canPlay 
                          ? isHovered 
                            ? 'border-yellow-400 shadow-lg shadow-yellow-400/50' 
                            : 'border-blue-400 hover:border-yellow-400' 
                          : 'border-gray-600'
                        }
                        backdrop-blur-sm transition-all duration-200
                      `}
                    />
                  </div>

                  {/* Mana Cost Indicator */}
                  {!canAfford && (
                    <div className="absolute -top-2 -right-2 w-6 h-6 bg-red-500 border-2 border-red-600 rounded-full flex items-center justify-center">
                      <span className="text-white text-xs font-bold" style={{ fontFamily: "'Press Start 2P', monospace" }}>
                        {card.mana_cost}
                      </span>
                    </div>
                  )}

                  {/* Play Indicator */}
                  {canPlay && isHovered && (
                    <motion.div
                      className="absolute -top-4 left-1/2 transform -translate-x-1/2 bg-green-500 border-2 border-green-600 rounded px-2 py-1"
                      initial={{ scale: 0, y: 10 }}
                      animate={{ scale: 1, y: 0 }}
                      style={{
                        imageRendering: 'pixelated'
                      }}
                    >
                      <span className="text-white text-xs font-bold" style={{ fontFamily: "'Press Start 2P', monospace" }}>
                        PLAY
                      </span>
                    </motion.div>
                  )}

                  {/* Select Indicator */}
                  {!canPlay && isHovered && (
                    <motion.div
                      className="absolute -top-4 left-1/2 transform -translate-x-1/2 bg-blue-500 border-2 border-blue-600 rounded px-2 py-1"
                      initial={{ scale: 0, y: 10 }}
                      animate={{ scale: 1, y: 0 }}
                      style={{
                        imageRendering: 'pixelated'
                      }}
                    >
                      <span className="text-white text-xs font-bold" style={{ fontFamily: "'Press Start 2P', monospace" }}>
                        VIEW
                      </span>
                    </motion.div>
                  )}
                </motion.div>
              )
            })}
          </AnimatePresence>
        </div>

        {/* Instructions */}
        {cards.length > 0 && (
          <div className="text-center text-white/60 text-xs mt-2" style={{ fontFamily: "'Press Start 2P', monospace" }}>
            {isActive ? 'CLICK TO PLAY • DOUBLE-CLICK FOR DETAILS' : 'WAITING FOR TURN'}
          </div>
        )}
      </div>

      {/* Card Details Modal */}
      <CardDetailsModal
        card={detailsCard}
        isOpen={!!detailsCard}
        onClose={() => setDetailsCard(null)}
      />
    </>
  )
}

/**
 * Pixel parchment card details modal
 */
const CardDetailsModal: React.FC<CardDetailsModalProps> = ({ card, isOpen, onClose }) => {
  if (!card) return null

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          className="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-50"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          onClick={onClose}
        >
          <motion.div
            className="relative max-w-md mx-4"
            initial={{ scale: 0.8, y: 50 }}
            animate={{ scale: 1, y: 0 }}
            exit={{ scale: 0.8, y: 50 }}
            onClick={(e) => e.stopPropagation()}
          >
            {/* Parchment Background */}
            <div 
              className="relative bg-gradient-to-br from-amber-100 to-amber-200 border-8 border-amber-800 rounded-2xl p-6 shadow-2xl"
              style={{
                backgroundImage: `
                  radial-gradient(circle at 20% 20%, rgba(139, 69, 19, 0.1) 0%, transparent 50%),
                  radial-gradient(circle at 80% 80%, rgba(160, 82, 45, 0.1) 0%, transparent 50%),
                  linear-gradient(45deg, transparent 40%, rgba(139, 69, 19, 0.05) 50%, transparent 60%)
                `,
                imageRendering: 'pixelated'
              }}
            >
              {/* Decorative Corners */}
              <div className="absolute top-2 left-2 w-4 h-4 bg-amber-800 transform rotate-45"></div>
              <div className="absolute top-2 right-2 w-4 h-4 bg-amber-800 transform rotate-45"></div>
              <div className="absolute bottom-2 left-2 w-4 h-4 bg-amber-800 transform rotate-45"></div>
              <div className="absolute bottom-2 right-2 w-4 h-4 bg-amber-800 transform rotate-45"></div>

              {/* Card Display */}
              <div className="flex flex-col items-center">
                <div className="w-64 mb-4">
                  <TradingCard
                    card={card}
                    owned={1}
                    // Removed showDownloadButton prop - download functionality removed
                    className="!w-full !max-w-none aspect-[2/3] !min-w-0 !min-h-0 border-4 border-amber-800 shadow-lg"
                  />
                </div>

                {/* Card Details */}
                <div className="text-center space-y-2">
                  <h3 className="text-2xl font-bold text-amber-900" style={{ fontFamily: "'Cinzel', serif" }}>
                    {card.name}
                  </h3>
                  
                  <div className="text-amber-700" style={{ fontFamily: "'Press Start 2P', monospace" }}>
                    {card.potato_type?.toUpperCase()} • {card.rarity?.toUpperCase()}
                  </div>

                  {card.ability_text && (
                    <div className="bg-blue-50 border-2 border-blue-200 rounded-lg p-3 max-w-xs">
                      <div className="text-blue-800 text-sm italic">
                        {card.ability_text}
                      </div>
                    </div>
                  )}

                  {card.description && (
                    <div className="text-amber-800 text-sm italic max-w-xs">
                      "{card.description}"
                    </div>
                  )}
                </div>

                {/* Close Button */}
                <motion.button
                  className="mt-4 bg-amber-800 hover:bg-amber-700 text-white px-4 py-2 rounded border-2 border-amber-900"
                  onClick={onClose}
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  style={{
                    fontFamily: "'Press Start 2P', monospace",
                    fontSize: '10px',
                    imageRendering: 'pixelated'
                  }}
                >
                  CLOSE
                </motion.button>
              </div>
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  )
}

export default Hand