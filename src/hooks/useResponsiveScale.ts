import { useState, useEffect } from 'react'

interface ResponsiveScaleConfig {
  baseWidth: number
  baseHeight: number
  minScale: number
  maxScale: number
  breakpoints: {
    mobile: number
    tablet: number
    desktop: number
    ultrawide: number
  }
}

const defaultConfig: ResponsiveScaleConfig = {
  baseWidth: 1280,
  baseHeight: 720,
  minScale: 0.5,
  maxScale: 1.5,
  breakpoints: {
    mobile: 768,
    tablet: 1024,
    desktop: 1280,
    ultrawide: 1920
  }
}

/**
 * useResponsiveScale: Manages --ui-scale CSS variable for pixel-perfect scaling
 * Features: 16:9 letterboxing, integer scaling, breakpoint-based adjustments
 */
export const useResponsiveScale = (config: Partial<ResponsiveScaleConfig> = {}) => {
  const finalConfig = { ...defaultConfig, ...config }
  const [scale, setScale] = useState(1)
  const [breakpoint, setBreakpoint] = useState<'mobile' | 'tablet' | 'desktop' | 'ultrawide'>('desktop')
  const [containerSize, setContainerSize] = useState({ width: 0, height: 0 })

  useEffect(() => {
    const updateScale = () => {
      const windowWidth = window.innerWidth
      const windowHeight = window.innerHeight

      // Determine breakpoint
      let currentBreakpoint: 'mobile' | 'tablet' | 'desktop' | 'ultrawide'
      if (windowWidth <= finalConfig.breakpoints.mobile) {
        currentBreakpoint = 'mobile'
      } else if (windowWidth <= finalConfig.breakpoints.tablet) {
        currentBreakpoint = 'tablet'
      } else if (windowWidth <= finalConfig.breakpoints.desktop) {
        currentBreakpoint = 'desktop'
      } else {
        currentBreakpoint = 'ultrawide'
      }
      setBreakpoint(currentBreakpoint)

      // Calculate 16:9 letterboxed dimensions
      const targetAspectRatio = finalConfig.baseWidth / finalConfig.baseHeight
      const windowAspectRatio = windowWidth / windowHeight

      let containerWidth = windowWidth
      let containerHeight = windowHeight

      if (windowAspectRatio > targetAspectRatio) {
        // Window is wider than target - letterbox horizontally
        containerWidth = windowHeight * targetAspectRatio
      } else {
        // Window is taller than target - letterbox vertically
        containerHeight = windowWidth / targetAspectRatio
      }

      // Calculate scale based on container width
      let calculatedScale = containerWidth / finalConfig.baseWidth

      // Apply breakpoint-specific scale adjustments
      switch (currentBreakpoint) {
        case 'mobile':
          calculatedScale *= 0.8 // Smaller scale for mobile
          break
        case 'tablet':
          calculatedScale *= 0.9 // Slightly smaller for tablet
          break
        case 'ultrawide':
          calculatedScale *= 1.1 // Slightly larger for ultrawide
          break
        default:
          // Desktop uses calculated scale as-is
          break
      }

      // Clamp to min/max and round to nearest 0.1 for crisp pixels
      const clampedScale = Math.max(
        finalConfig.minScale,
        Math.min(finalConfig.maxScale, calculatedScale)
      )
      const roundedScale = Math.round(clampedScale * 10) / 10

      setScale(roundedScale)
      setContainerSize({ width: containerWidth, height: containerHeight })

      // Update CSS custom property
      document.documentElement.style.setProperty('--ui-scale', roundedScale.toString())
      document.documentElement.style.setProperty('--container-width', `${containerWidth}px`)
      document.documentElement.style.setProperty('--container-height', `${containerHeight}px`)
    }

    // Initial calculation
    updateScale()

    // Listen for resize events
    window.addEventListener('resize', updateScale)
    return () => window.removeEventListener('resize', updateScale)
  }, [finalConfig])

  // Utility functions for responsive values
  const scaleValue = (value: number) => Math.round(value * scale)
  const scaleFont = (baseFontSize: number) => `${Math.max(8, scaleValue(baseFontSize))}px`
  
  const getBreakpointValue = <T>(values: {
    mobile?: T
    tablet?: T
    desktop: T
    ultrawide?: T
  }): T => {
    switch (breakpoint) {
      case 'mobile':
        return values.mobile ?? values.desktop
      case 'tablet':
        return values.tablet ?? values.desktop
      case 'ultrawide':
        return values.ultrawide ?? values.desktop
      default:
        return values.desktop
    }
  }

  return {
    scale,
    breakpoint,
    containerSize,
    scaleValue,
    scaleFont,
    getBreakpointValue,
    isDesktop: breakpoint === 'desktop' || breakpoint === 'ultrawide',
    isTablet: breakpoint === 'tablet',
    isMobile: breakpoint === 'mobile'
  }
}

/**
 * Utility to round transform values to integers for pixel-perfect rendering
 */
export const roundTransform = (transform: {
  x?: number
  y?: number
  scale?: number
  rotate?: number
}) => {
  return {
    x: transform.x ? Math.round(transform.x) : undefined,
    y: transform.y ? Math.round(transform.y) : undefined,
    scale: transform.scale || undefined,
    rotate: transform.rotate || undefined
  }
}

/**
 * CSS utility classes for pixel-perfect rendering
 */
export const pixelPerfectStyles = {
  crisp: {
    imageRendering: 'pixelated' as const,
    MozImageRendering: '-moz-crisp-edges' as const,
    WebkitImageRendering: 'crisp-edges' as const
  },
  smooth: {
    imageRendering: 'auto' as const
  },
  pixelFont: {
    fontFamily: "'Press Start 2P', monospace",
    imageRendering: 'pixelated' as const
  }
}

export default useResponsiveScale