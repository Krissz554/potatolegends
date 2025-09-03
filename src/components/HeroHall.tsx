import React, { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { useAuth } from '@/contexts/AuthContext'
import { supabase } from '@/lib/supabase'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Progress } from '@/components/ui/progress'
import { 
  Crown, 
  Zap, 
  Heart, 
  Shield, 
  Swords,
  Trophy,
  Star,
  Lock,
  CheckCircle,
  Flame,
  Snowflake,
  Sun,
  Sparkles
} from 'lucide-react'
import { toast } from 'sonner'
import ElementalPixelPotato from '@/components/ElementalPixelPotato'
import { RARITY_COLORS } from '@/lib/potatoData'
import { unlockStarterHeroes } from '@/lib/heroService'

interface Hero {
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

  total_wins?: number
  total_losses?: number
  total_games?: number
  unlocked?: boolean
  is_active?: boolean
}

const elementIcons = {
  fire: Flame,
  ice: Snowflake,
  lightning: Zap,
  light: Sun,
  void: Sparkles
}

const elementColors = {
  fire: 'text-red-500 bg-red-100',
  ice: 'text-blue-500 bg-blue-100',
  lightning: 'text-purple-500 bg-purple-100',
  light: 'text-yellow-500 bg-yellow-100',
  void: 'text-gray-500 bg-gray-100'
}

export const HeroHall: React.FC<{
  isOpen?: boolean
  onClose?: () => void
  standalone?: boolean
}> = ({ isOpen = true, onClose = () => {}, standalone = false }) => {
  const { user } = useAuth()
  const [heroes, setHeroes] = useState<Hero[]>([])
  const [userHeroes, setUserHeroes] = useState<Hero[]>([])
  const [activeHero, setActiveHero] = useState<Hero | null>(null)
  const [loading, setLoading] = useState(true)
  const [switching, setSwitching] = useState(false)

  // Load heroes data
  useEffect(() => {
    if ((isOpen || standalone) && user) {
      loadHeroData()
    }
  }, [isOpen, standalone, user])

  const loadHeroData = async () => {
    if (!user) return

    setLoading(true)
    try {
      // Load all heroes
      const { data: allHeroes, error: heroesError } = await supabase
        .from('heroes')
        .select('*')
        .order('rarity, name')

      if (heroesError) {
        console.error('Heroes table error:', heroesError)
        toast.error('Failed to load heroes. Please try refreshing.')
        setLoading(false)
        return
      }

      // Load user's heroes
      const { data: userHeroData, error: userHeroError } = await supabase
        .from('user_heroes')
        .select(`
          hero_id,
          is_active,
          total_wins,
          total_losses,
          heroes (*)
        `)
        .eq('user_id', user.id)

      if (userHeroError) throw userHeroError

      // Combine data
      const userHeroIds = new Set(userHeroData?.map(uh => uh.hero_id) || [])
      const combinedHeroes = allHeroes?.map(hero => {
        const userHero = userHeroData?.find(uh => uh.hero_id === hero.hero_id)
        const totalGames = (userHero?.total_wins || 0) + (userHero?.total_losses || 0)
        return {
          ...hero,
          unlocked: userHeroIds.has(hero.hero_id),
          is_active: userHero?.is_active || false,
          total_wins: userHero?.total_wins || 0,
          total_losses: userHero?.total_losses || 0,
          total_games: totalGames
        }
      }) || []

      setHeroes(combinedHeroes)
      setUserHeroes(combinedHeroes.filter(h => h.unlocked))
      setActiveHero(combinedHeroes.find(h => h.is_active) || null)

    } catch (error) {
      console.error('Error loading hero data:', error)
      toast.error('Failed to load heroes')
    } finally {
      setLoading(false)
    }
  }

  const handleSetActiveHero = async (heroId: string) => {
    if (!user || switching) return

    setSwitching(true)
    try {
      const { data, error } = await supabase.rpc('set_active_hero', {
        user_uuid: user.id,
        target_hero_id: heroId
      })

      if (error) throw error

      toast.success('Hero selected for battle!')
      await loadHeroData() // Refresh data

    } catch (error) {
      console.error('Error setting active hero:', error)
      toast.error('Failed to select hero')
    } finally {
      setSwitching(false)
    }
  }

  const unlockHero = async (hero: Hero) => {
    if (!user) return

    try {
      const { error } = await supabase
        .from('user_heroes')
        .insert({
          user_id: user.id,
          hero_id: hero.hero_id,
          is_active: heroes.filter(h => h.unlocked).length === 0, // Make active if first hero
          total_wins: 0,
          total_losses: 0
        })

      if (error) throw error

      // Reload data
      await loadHeroData()
      toast.success(`Unlocked ${hero.name}!`)
    } catch (error) {
      console.error('Error unlocking hero:', error)
      toast.error('Failed to unlock hero')
    }
  }

  const HeroCard: React.FC<{ hero: Hero, showStats?: boolean }> = ({ hero, showStats = false }) => {
    const ElementIcon = elementIcons[hero.element_type as keyof typeof elementIcons] || Sparkles
    const rarityStyle = RARITY_COLORS[hero.rarity as keyof typeof RARITY_COLORS]

    return (
      <motion.div
        initial={{ scale: 0.9, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        whileHover={{ scale: 1.02 }}
        className={`relative overflow-hidden rounded-lg border-2 transition-all cursor-pointer ${
          hero.is_active 
            ? 'border-green-500 shadow-lg ring-2 ring-green-200' 
            : hero.unlocked 
              ? 'border-gray-200 hover:border-gray-300 hover:shadow-md' 
              : 'border-gray-100 opacity-75'
        }`}
        onClick={() => hero.unlocked && !hero.is_active && handleSetActiveHero(hero.hero_id)}
      >
        <Card className="h-full border-0">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-lg flex items-center gap-2">
                <ElementIcon className={`w-5 h-5 ${elementColors[hero.element_type as keyof typeof elementColors]?.split(' ')[0]}`} />
                {hero.name}
              </CardTitle>
              
              <div className="flex items-center gap-1">
                {/* Rarity Badge */}
                <Badge 
                  variant="secondary" 
                  className={`text-xs ${rarityStyle?.bgColor} ${rarityStyle?.textColor}`}
                >
                  {hero.rarity.toUpperCase()}
                </Badge>
                
                {/* Status Badge */}
                {hero.is_active && (
                  <Badge variant="default" className="bg-green-500 text-white text-xs">
                    <CheckCircle className="w-3 h-3 mr-1" />
                    ACTIVE
                  </Badge>
                )}
                
                {!hero.unlocked && (
                  <Badge variant="secondary" className="bg-gray-500 text-white text-xs">
                    <Lock className="w-3 h-3 mr-1" />
                    LOCKED
                  </Badge>
                )}
              </div>
            </div>
          </CardHeader>

          <CardContent className="space-y-3">
            {/* Hero Art */}
            <div className="flex justify-center">
              <div className="w-24 h-24 rounded-full bg-gradient-to-br from-amber-100 to-orange-200 flex items-center justify-center">
                <ElementalPixelPotato
                  cardName={hero.name}
                  potatoType={hero.element_type as any}
                  rarity={hero.rarity as any}
                  seed={hero.hero_id}
                  size={60}
                />
              </div>
            </div>

            {/* Hero Stats */}
            <div className="grid grid-cols-2 gap-2 text-sm">
              <div className="flex items-center gap-1">
                <Heart className="w-4 h-4 text-red-500" />
                <span className="font-semibold">{hero.base_hp}</span>
                <span className="text-gray-500">HP</span>
              </div>
              <div className="flex items-center gap-1">
                <Zap className="w-4 h-4 text-blue-500" />
                <span className="font-semibold">{hero.base_mana}</span>
                <span className="text-gray-500">Mana</span>
              </div>
            </div>

            {/* Hero Power */}
            <div className="bg-gray-50 rounded p-2 text-sm">
              <div className="font-semibold text-purple-700 flex items-center gap-1">
                <Crown className="w-4 h-4" />
                {hero.hero_power_name}
                <span className="ml-auto text-xs bg-purple-100 text-purple-700 px-1 rounded">
                  {hero.hero_power_cost} mana
                </span>
              </div>
              <p className="text-gray-600 text-xs mt-1">{hero.hero_power_description}</p>
            </div>

            {/* Battle Stats (if unlocked and showStats) */}
            {hero.unlocked && showStats && (
              <div className="border-t pt-2">
                <div className="grid grid-cols-3 gap-2 text-xs text-center">
                  <div>
                    <div className="font-semibold text-green-600">{hero.total_wins}</div>
                    <div className="text-gray-500">Wins</div>
                  </div>
                  <div>
                    <div className="font-semibold text-red-600">{hero.total_losses}</div>
                    <div className="text-gray-500">Losses</div>
                  </div>
                  <div>
                    <div className="font-semibold text-blue-600">
                      {hero.total_games > 0 ? Math.round((hero.total_wins / hero.total_games) * 100) : 0}%
                    </div>
                    <div className="text-gray-500">Win Rate</div>
                  </div>
                </div>
              </div>
            )}

            {/* Description */}
            <p className="text-xs text-gray-600">{hero.description}</p>

            {/* Action Button */}
            {hero.unlocked && !hero.is_active && (
              <Button 
                size="sm" 
                variant="outline" 
                className="w-full"
                disabled={switching}
                onClick={(e) => {
                  e.stopPropagation()
                  handleSetActiveHero(hero.hero_id)
                }}
              >
                {switching ? 'Selecting...' : 'Select Hero'}
              </Button>
            )}
          </CardContent>
        </Card>
      </motion.div>
    )
  }

  if (!isOpen && !standalone) return null

  // Standalone mode - render without modal overlay (exact StandaloneHeroHall design)
  if (standalone) {
    if (loading) {
      return (
        <div className="flex items-center justify-center py-20">
          <div className="text-center">
            <Sparkles className="w-12 h-12 text-blue-400 mx-auto mb-4 animate-spin" />
            <p className="text-white text-lg">Loading heroes...</p>
          </div>
        </div>
      )
    }

    return (
      <div className="space-y-8">
        {/* Active Hero Display - Redesigned */}
        {activeHero && (
          <div className="relative bg-gradient-to-br from-yellow-900/40 via-orange-900/30 to-red-900/40 rounded-2xl border border-yellow-500/30 p-8 backdrop-blur-sm overflow-hidden">
            {/* Background Effects */}
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-yellow-400/10 via-transparent to-transparent"></div>
            <div className="absolute top-4 right-4">
              <div className="w-24 h-24 rounded-full bg-gradient-to-br from-yellow-400/20 to-orange-500/20 blur-xl"></div>
            </div>
            
            <div className="relative">
              <div className="flex items-center gap-3 mb-4">
                <Crown className="w-8 h-8 text-yellow-400 drop-shadow-lg" />
                <h2 className="text-2xl font-bold text-white" style={{ fontFamily: "'Cinzel', serif" }}>
                  Your Champion
                </h2>
              </div>
              
              <div className="flex items-center gap-6">
                {/* Hero Avatar - Larger and More Prominent */}
                <div className="relative">
                  <div className="w-24 h-24 rounded-2xl bg-gradient-to-br from-yellow-400/20 to-orange-500/20 border-2 border-yellow-400/50 shadow-2xl overflow-hidden backdrop-blur-sm">
                    <ElementalPixelPotato
                      seed={activeHero.hero_id}
                      potatoType={activeHero.element_type as any}
                      rarity={activeHero.rarity as any}
                      cardName={activeHero.name}
                      size={96}
                    />
                  </div>
                  <div className="absolute -top-2 -right-2 bg-yellow-500 text-yellow-900 rounded-full p-2 shadow-lg">
                    <Crown className="w-4 h-4" />
                  </div>
                </div>
                
                {/* Hero Info - Better Typography */}
                <div className="flex-1">
                  <h3 className="text-3xl font-bold text-yellow-100 mb-2" style={{ fontFamily: "'Cinzel', serif" }}>
                    {activeHero.name}
                  </h3>
                  <p className="text-yellow-200/80 text-lg mb-4 max-w-2xl leading-relaxed">
                    {activeHero.description}
                  </p>
                  
                  {/* Stats - Redesigned with Better Visual Hierarchy */}
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <div className="bg-black/20 rounded-lg p-3 border border-red-500/30">
                      <div className="flex items-center gap-2">
                        <Heart className="w-5 h-5 text-red-400" />
                        <span className="text-red-300 font-semibold">{activeHero.base_hp}</span>
                      </div>
                      <div className="text-xs text-red-200/60 mt-1">Health</div>
                    </div>
                    
                    <div className="bg-black/20 rounded-lg p-3 border border-blue-500/30">
                      <div className="flex items-center gap-2">
                        <Zap className="w-5 h-5 text-blue-400" />
                        <span className="text-blue-300 font-semibold">{activeHero.base_mana}</span>
                      </div>
                      <div className="text-xs text-blue-200/60 mt-1">Mana</div>
                    </div>
                    
                    <div className="bg-black/20 rounded-lg p-3 border border-green-500/30">
                      <div className="flex items-center gap-2">
                        <Trophy className="w-5 h-5 text-green-400" />
                        <span className="text-green-300 font-semibold">{activeHero.total_wins}</span>
                      </div>
                      <div className="text-xs text-green-200/60 mt-1">Victories</div>
                    </div>
                    
                    <div className="bg-black/20 rounded-lg p-3 border border-purple-500/30">
                      <div className="flex items-center gap-2">
                        <Star className="w-5 h-5 text-purple-400" />
                        <span className="text-purple-300 font-semibold">
                          {activeHero.total_games ? Math.round((activeHero.total_wins! / activeHero.total_games) * 100) : 0}%
                        </span>
                      </div>
                      <div className="text-xs text-purple-200/60 mt-1">Win Rate</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Redesigned Tabs Section */}
        <div className="space-y-6">
          <Tabs defaultValue="unlocked" className="w-full">
            {/* Enhanced Tab List */}
            <div className="flex justify-center mb-8">
              <TabsList className="bg-gradient-to-r from-slate-800/80 to-slate-900/80 border border-white/10 rounded-2xl p-2 backdrop-blur-sm shadow-2xl">
                <TabsTrigger 
                  value="unlocked" 
                  className="text-white font-semibold px-8 py-3 rounded-xl data-[state=active]:bg-gradient-to-r data-[state=active]:from-blue-500 data-[state=active]:to-purple-600 data-[state=active]:text-white data-[state=active]:shadow-lg transition-all duration-300"
                  style={{ fontFamily: "'Cinzel', serif" }}
                >
                  <Shield className="w-5 h-5 mr-2" />
                  Your Heroes ({userHeroes.length})
                </TabsTrigger>
                <TabsTrigger 
                  value="all" 
                  className="text-white font-semibold px-8 py-3 rounded-xl data-[state=active]:bg-gradient-to-r data-[state=active]:from-blue-500 data-[state=active]:to-purple-600 data-[state=active]:text-white data-[state=active]:shadow-lg transition-all duration-300"
                  style={{ fontFamily: "'Cinzel', serif" }}
                >
                  <Sparkles className="w-5 h-5 mr-2" />
                  All Heroes ({heroes.length})
                </TabsTrigger>
              </TabsList>
            </div>

            <TabsContent value="unlocked" className="space-y-6">
              {userHeroes.length === 0 ? (
                <div className="relative bg-gradient-to-br from-slate-900/60 via-slate-800/40 to-slate-900/60 rounded-2xl border border-slate-700/50 p-12 backdrop-blur-sm text-center overflow-hidden">
                  <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-500/5 via-transparent to-transparent"></div>
                  <div className="relative">
                    <div className="w-24 h-24 mx-auto mb-6 rounded-full bg-gradient-to-br from-slate-700/50 to-slate-800/50 border border-slate-600/30 flex items-center justify-center">
                      <Lock className="w-12 h-12 text-slate-400" />
                    </div>
                    <h3 className="text-2xl font-bold text-white mb-4" style={{ fontFamily: "'Cinzel', serif" }}>
                      No Heroes in Your Roster
                    </h3>
                    <p className="text-slate-300 text-lg mb-6 max-w-md mx-auto leading-relaxed">
                      Build your legendary team by exploring the "All Heroes" section and unlocking powerful champions.
                    </p>
                    <div className="inline-flex items-center gap-2 text-blue-400 font-semibold">
                      <Sparkles className="w-5 h-5" />
                      Discover new heroes awaits!
                    </div>
                  </div>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-8">
                  {userHeroes.map((hero) => (
                    <StandaloneHeroCard
                      key={hero.hero_id}
                      hero={hero}
                      isActive={hero.is_active}
                      onSelect={() => handleSetActiveHero(hero.hero_id)}
                      switching={switching}
                    />
                  ))}
                </div>
              )}
            </TabsContent>

            <TabsContent value="all" className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-8">
                {heroes.map((hero) => (
                  <StandaloneHeroCard
                    key={hero.hero_id}
                    hero={hero}
                    isActive={hero.is_active}
                    onSelect={() => hero.unlocked ? handleSetActiveHero(hero.hero_id) : unlockHero(hero)}
                    switching={switching}
                    showUnlock={!hero.unlocked}
                  />
                ))}
              </div>
            </TabsContent>
          </Tabs>
        </div>
      </div>
    )
  }

  // Modal mode - render with overlay
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50"
      onClick={onClose}
    >
      <motion.div
        initial={{ scale: 0.9, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        exit={{ scale: 0.9, opacity: 0 }}
        className="bg-white rounded-xl shadow-xl max-w-6xl w-full max-h-[90vh] overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="p-6 border-b">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold flex items-center gap-2">
                <Crown className="w-6 h-6 text-yellow-500" />
                Hero Hall
              </h1>
              <p className="text-gray-600">Choose your champion for battle</p>
            </div>
            
            {activeHero && (
              <div className="text-right">
                <div className="text-sm text-gray-500">Active Hero</div>
                <div className="font-semibold text-green-600">{activeHero.name}</div>
              </div>
            )}
          </div>
        </div>

        <div className="p-6 overflow-y-auto max-h-[calc(90vh-120px)]">
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-yellow-600"></div>
            </div>
          ) : (
            <Tabs defaultValue="unlocked" className="space-y-6">
              <TabsList className="grid w-full grid-cols-2">
                <TabsTrigger value="unlocked">My Heroes ({userHeroes.length})</TabsTrigger>
                <TabsTrigger value="all">All Heroes ({heroes.length})</TabsTrigger>
              </TabsList>

              <TabsContent value="unlocked" className="space-y-4">
                {userHeroes.length === 0 ? (
                  <div className="text-center py-12">
                    <Crown className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                    <h3 className="text-lg font-semibold text-gray-600">No Heroes Unlocked</h3>
                    <p className="text-gray-500 mb-4">It looks like your starter heroes weren't unlocked automatically.</p>
                    <Button 
                      onClick={async () => {
                        const result = await unlockStarterHeroes()
                        if (result.success) {
                          loadHeroData() // Refresh the data
                        }
                      }}
                      variant="default"
                      className="mb-2"
                    >
                      <Crown className="w-4 h-4 mr-2" />
                      Unlock Potato King
                    </Button>
                    <p className="text-xs text-gray-400">This will give you the Potato King hero!</p>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {userHeroes.map((hero) => (
                      <HeroCard key={hero.hero_id} hero={hero} showStats={true} />
                    ))}
                  </div>
                )}
              </TabsContent>

              <TabsContent value="all" className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {heroes.map((hero) => (
                    <HeroCard key={hero.hero_id} hero={hero} />
                  ))}
                </div>
              </TabsContent>
            </Tabs>
          )}
        </div>

        <div className="p-4 border-t bg-gray-50 flex justify-end">
          <Button onClick={onClose}>Close</Button>
        </div>
      </motion.div>
    </motion.div>
  )
}

// StandaloneHeroCard component (from original StandaloneHeroHall)
interface StandaloneHeroCardProps {
  hero: Hero
  isActive?: boolean
  onSelect: () => void
  switching?: boolean
  showUnlock?: boolean
}

const StandaloneHeroCard: React.FC<StandaloneHeroCardProps> = ({ hero, isActive, onSelect, switching, showUnlock }) => {
  const ElementIcon = elementIcons[hero.element_type as keyof typeof elementIcons] || Sparkles
  const winRate = hero.total_games ? Math.round((hero.total_wins! / hero.total_games) * 100) : 0

  // Rarity-based gradients
  const rarityGradients = {
    common: 'from-slate-700/60 via-slate-600/40 to-slate-700/60',
    uncommon: 'from-green-800/60 via-green-700/40 to-green-800/60',
    rare: 'from-blue-800/60 via-blue-700/40 to-blue-800/60',
    legendary: 'from-purple-800/60 via-purple-700/40 to-purple-800/60',
    exotic: 'from-orange-800/60 via-orange-700/40 to-orange-800/60'
  }

  const rarityBorders = {
    common: 'border-slate-500/40',
    uncommon: 'border-green-500/40',
    rare: 'border-blue-500/40',
    legendary: 'border-purple-500/40',
    exotic: 'border-orange-500/40'
  }

  const elementTypeColors = {
    fire: 'from-red-500/30 to-orange-500/30',
    ice: 'from-cyan-500/30 to-blue-500/30',
    lightning: 'from-purple-500/30 to-violet-500/30',
    light: 'from-yellow-500/30 to-amber-500/30',
    void: 'from-gray-500/30 to-slate-500/30'
  }

  return (
    <motion.div
      whileHover={{ scale: 1.02, y: -4 }}
      whileTap={{ scale: 0.98 }}
      className="group"
    >
      <div className={`relative cursor-pointer transition-all duration-300 rounded-2xl border backdrop-blur-sm overflow-hidden h-full flex flex-col ${
        isActive 
          ? 'ring-2 ring-yellow-400/60 bg-gradient-to-br from-yellow-900/40 via-amber-900/30 to-orange-900/40 border-yellow-500/50 shadow-xl shadow-yellow-500/20'
          : `bg-gradient-to-br ${rarityGradients[hero.rarity as keyof typeof rarityGradients] || rarityGradients.common} ${rarityBorders[hero.rarity as keyof typeof rarityBorders] || rarityBorders.common} hover:border-white/40 group-hover:shadow-xl group-hover:shadow-black/30`
      }`}>
        
        {/* Background Element Effect */}
        <div className={`absolute inset-0 bg-gradient-to-br ${elementTypeColors[hero.element_type as keyof typeof elementTypeColors] || elementTypeColors.void} opacity-20`}></div>
        
        {/* Active Crown */}
        {isActive && (
          <div className="absolute -top-3 left-1/2 transform -translate-x-1/2 z-20">
            <div className="bg-gradient-to-r from-yellow-400 to-amber-500 text-yellow-900 rounded-full px-4 py-2 shadow-lg border border-yellow-300">
              <div className="flex items-center gap-2 font-bold text-sm">
                <Crown className="w-4 h-4" />
                CHAMPION
              </div>
            </div>
          </div>
        )}

        {/* Lock Overlay */}
        {showUnlock && (
          <div className="absolute inset-0 bg-black/70 backdrop-blur-sm rounded-2xl flex items-center justify-center z-10">
            <div className="text-center">
              <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-gradient-to-br from-slate-600 to-slate-800 border border-slate-500 flex items-center justify-center">
                <Lock className="w-8 h-8 text-slate-300" />
              </div>
              <div className="text-slate-300 font-semibold">Locked</div>
            </div>
          </div>
        )}

        <div className="relative p-6 flex-1 flex flex-col" onClick={onSelect}>
          {/* Hero Header */}
          <div className="flex items-start gap-4 mb-6">
            {/* Hero Avatar - Enhanced */}
            <div className="relative">
              <div className="w-20 h-20 rounded-2xl bg-gradient-to-br from-white/10 to-white/5 border-2 border-white/20 shadow-2xl overflow-hidden backdrop-blur-sm">
                <ElementalPixelPotato
                  seed={hero.hero_id}
                  potatoType={hero.element_type as any}
                  rarity={hero.rarity as any}
                  cardName={hero.name}
                  size={80}
                />
              </div>
              {/* Element Badge */}
              <div className={`absolute -bottom-2 -right-2 p-2 rounded-lg bg-gradient-to-br ${elementTypeColors[hero.element_type as keyof typeof elementTypeColors] || elementTypeColors.void} border border-white/20 shadow-lg`}>
                <ElementIcon className="w-4 h-4 text-white" />
              </div>
            </div>
            
            {/* Hero Info */}
            <div className="flex-1 min-w-0">
              <h3 className="text-xl font-bold text-white mb-2 truncate" style={{ fontFamily: "'Cinzel', serif" }}>
                {hero.name}
              </h3>
              <div className="flex items-center gap-2 mb-3">
                <div className={`px-3 py-1 rounded-full text-xs font-semibold border ${
                  hero.rarity === 'legendary' ? 'bg-purple-500/20 text-purple-200 border-purple-400/30' :
                  hero.rarity === 'exotic' ? 'bg-orange-500/20 text-orange-200 border-orange-400/30' :
                  hero.rarity === 'rare' ? 'bg-blue-500/20 text-blue-200 border-blue-400/30' :
                  hero.rarity === 'uncommon' ? 'bg-green-500/20 text-green-200 border-green-400/30' :
                  'bg-slate-500/20 text-slate-200 border-slate-400/30'
                }`}>
                  {hero.rarity.toUpperCase()}
                </div>
              </div>
              
              {/* Stats Grid */}
              <div className="grid grid-cols-2 gap-2">
                <div className="bg-black/20 rounded-lg p-2 border border-red-500/20">
                  <div className="flex items-center gap-1">
                    <Heart className="w-4 h-4 text-red-400" />
                    <span className="text-red-300 font-semibold text-sm">{hero.base_hp}</span>
                  </div>
                </div>
                <div className="bg-black/20 rounded-lg p-2 border border-blue-500/20">
                  <div className="flex items-center gap-1">
                    <Zap className="w-4 h-4 text-blue-400" />
                    <span className="text-blue-300 font-semibold text-sm">{hero.base_mana}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Description */}
          <p className="text-white/80 text-sm mb-6 leading-relaxed flex-1">
            {hero.description}
          </p>

          {/* Hero Power */}
          <div className="bg-black/30 rounded-xl p-4 mb-6 border border-white/10">
            <div className="flex items-center gap-2 mb-2">
              <Shield className="w-4 h-4 text-purple-400" />
              <span className="text-white font-semibold text-sm">{hero.hero_power_name}</span>
              <div className="bg-purple-500/20 text-purple-200 border border-purple-400/30 rounded-full px-2 py-1 text-xs">
                {hero.hero_power_cost} ⚡
              </div>
            </div>
            <p className="text-white/70 text-xs leading-relaxed">{hero.hero_power_description}</p>
          </div>

          {/* Battle Stats - Only for unlocked heroes */}
          {hero.unlocked && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-white/60 text-sm">Battle Record</span>
                <span className="text-white font-semibold">{winRate}% Win Rate</span>
              </div>
              <div className="w-full bg-black/30 rounded-full h-2 overflow-hidden">
                <div 
                  className="h-full bg-gradient-to-r from-green-500 to-emerald-400 transition-all duration-500"
                  style={{ width: `${winRate}%` }}
                ></div>
              </div>
              <div className="flex items-center justify-between text-xs text-white/60">
                <span>{hero.total_wins} Victories</span>
                <span>{hero.total_losses} Defeats</span>
              </div>
            </div>
          )}
        </div>

        {/* Action Button */}
        <div className="p-6 pt-0">
          <Button
            onClick={onSelect}
            disabled={switching}
            className={`w-full font-semibold py-3 rounded-xl transition-all duration-300 ${
              showUnlock 
                ? 'bg-gradient-to-r from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 text-white shadow-lg hover:shadow-blue-500/25' :
              isActive 
                ? 'bg-gradient-to-r from-yellow-500 to-amber-500 hover:from-yellow-600 hover:to-amber-600 text-yellow-900 shadow-lg hover:shadow-yellow-500/25' :
                'bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white shadow-lg hover:shadow-green-500/25'
            }`}
          >
            {switching ? (
              <div className="flex items-center gap-2">
                <Sparkles className="w-4 h-4 animate-spin" />
                Switching...
              </div>
            ) : showUnlock ? (
              <div className="flex items-center gap-2">
                <Lock className="w-4 h-4" />
                Unlock Hero
              </div>
            ) : isActive ? (
              <div className="flex items-center gap-2">
                <Crown className="w-4 h-4" />
                Active Champion
              </div>
            ) : (
              <div className="flex items-center gap-2">
                <Swords className="w-4 h-4" />
                Select as Champion
              </div>
            )}
          </Button>
        </div>
      </div>
    </motion.div>
  )
}



export default HeroHall