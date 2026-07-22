interface BrandProps {
  /** Renders the wordmark for a dark surface when true. */
  onDark?: boolean
  size?: 'sm' | 'md'
}

/**
 * The Meridian lockup: mark plus wordmark.
 *
 * Kept as one component so the header, the sign-in panel and any future surface
 * cannot drift apart in spacing or casing.
 */
export function Brand({ onDark = false, size = 'md' }: BrandProps) {
  const markSize = size === 'sm' ? 'h-6' : 'h-8'

  return (
    <div className="flex items-center gap-2.5">
      <img
        src="/meridian-mark.png"
        alt=""
        aria-hidden="true"
        className={`${markSize} w-auto`}
      />
      <div className="leading-tight">
        <p
          className={`text-sm font-semibold tracking-[0.18em] uppercase ${
            onDark ? 'text-white' : 'text-ink-900'
          }`}
        >
          Meridian
        </p>
        <p className={`text-xs ${onDark ? 'text-ink-300' : 'text-ink-500'}`}>
          Talent Platform
        </p>
      </div>
    </div>
  )
}
