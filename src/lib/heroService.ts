// Hero service for managing hero operations
import { supabase } from './supabase'
import { toast } from 'sonner'

/**
 * Manually unlock starter heroes for the current user
 * Useful if the automatic unlock didn't work
 */
export async function unlockStarterHeroes() {
  try {
    const { data: { user }, error: userError } = await supabase.auth.getUser()
    if (userError || !user) {
      throw new Error('User not authenticated')
    }

    console.log('🦸 Unlocking Potato King for user:', user.id)

    const { data, error } = await supabase.rpc('give_potato_king_to_user', {
      user_uuid: user.id
    })

    if (error) {
      throw error
    }

    console.log('✅ Starter heroes unlocked successfully')
    toast.success('Starter heroes unlocked!')
    return { success: true }

  } catch (error) {
    console.error('❌ Failed to unlock starter heroes:', error)
    toast.error('Failed to unlock starter heroes')
    return { success: false, error }
  }
}

/**
 * Get all heroes available to the user
 */
export async function getUserHeroes() {
  try {
    const { data: { user }, error: userError } = await supabase.auth.getUser()
    if (userError || !user) {
      throw new Error('User not authenticated')
    }

    // Get user's heroes with hero details
    const { data: userHeroes, error } = await supabase
      .from('user_heroes')
      .select(`
        *,
        heroes (*)
      `)
      .eq('user_id', user.id)

    if (error) {
      throw error
    }

    return { success: true, heroes: userHeroes }

  } catch (error) {
    console.error('❌ Failed to get user heroes:', error)
    return { success: false, error }
  }
}

/**
 * Set active hero for the user
 */
export async function setActiveHero(heroId: string) {
  try {
    const { data: { user }, error: userError } = await supabase.auth.getUser()
    if (userError || !user) {
      throw new Error('User not authenticated')
    }

    const { data, error } = await supabase.rpc('set_active_hero', {
      user_uuid: user.id,
      target_hero_id: heroId
    })

    if (error) {
      throw error
    }

    toast.success('Hero selected!')
    return { success: true }

  } catch (error) {
    console.error('❌ Failed to set active hero:', error)
    toast.error('Failed to select hero')
    return { success: false, error }
  }
}