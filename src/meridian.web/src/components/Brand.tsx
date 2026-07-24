interface BrandProps {
  /** Renders the lockup for a dark surface when true. */
  onDark?: boolean
  size?: 'sm' | 'md' | 'lg'
}

/**
 * The Meridian mark.
 *
 * Drawn as geometry rather than shipped as a bitmap, so it stays crisp at 24px in
 * a top bar, inherits whatever colour it is placed in, and costs about a kilobyte
 * instead of a third of a megabyte. One continuous ribbon crossing itself: two
 * sides of a hiring market meeting at a single point, which is the whole product.
 */
export function MeridianMark({ className = 'h-7 w-auto' }: { className?: string }) {
  return (
    <svg viewBox="0 0 96 64" fill="none" className={className} role="img" aria-label="Meridian">
      <path
        d="M48 32C40 16 26 12 16 18C6 24 6 40 16 46C26 52 40 48 48 32C56 16 70 12 80 18C90 24 90 40 80 46C70 52 56 48 48 32Z"
        stroke="currentColor"
        strokeWidth="12"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

const markSizes = { sm: 'h-6 w-auto', md: 'h-8 w-auto', lg: 'h-10 w-auto' }
const wordSizes = { sm: 'text-lg', md: 'text-2xl', lg: 'text-3xl' }
const eyebrowSizes = { sm: '9px', md: '11px', lg: '11px' }
const eyebrowMargins = { sm: 'mt-0.5', md: 'mt-1.5', lg: 'mt-1.5' }

/**
 * The full lockup: mark plus wordmark.
 *
 * Kept as one component so the header, the sign-in panel and any future surface
 * cannot drift apart in spacing or casing. The wordmark is set in the display
 * serif rather than tracked-out uppercase sans, because a name stated plainly at
 * size reads more confident than a name whispered in small caps.
 */
export function Brand({ onDark = false, size = 'md' }: BrandProps) {
  return (
    <div className="flex items-center gap-2.5">
      <MeridianMark className={`${markSizes[size]} ${onDark ? 'text-white' : 'text-accent'}`} />
      <div className="leading-none">
        <p className={`display ${wordSizes[size]} ${onDark ? 'text-white' : 'text-ink-900'}`}>
          Meridian
        </p>
        <p
          className={`eyebrow ${eyebrowMargins[size]} ${onDark ? 'text-white/50' : 'text-ink-500'}`}
          style={{ fontSize: eyebrowSizes[size] }}
        >
          Talent Platform
        </p>
      </div>
    </div>
  )
}
