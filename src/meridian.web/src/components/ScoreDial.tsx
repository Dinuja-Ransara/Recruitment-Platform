import { useEffect, useState } from 'react'

/**
 * The score dial.
 *
 * This is the platform's signature element and appears wherever a score does, so
 * that a score is recognisable at a glance in any context. An arc rather than a
 * bar because an arc reads as a measurement against a fixed maximum, and a bar in
 * a table row reads as a comparison against its neighbours, which is the wrong
 * question: 84 out of 100 means something on its own.
 *
 * The arc sweeps from zero on mount so the eye is drawn to where the value
 * settles. Colour carries the same information as position, so the reading does
 * not depend on distinguishing hues.
 */
interface Props {
  score: number
  /** A capped score is shown in warning colour with a notch on the track. */
  capped?: boolean
  size?: 'sm' | 'md' | 'lg'
  label?: string
}

const DIMENSIONS = {
  sm: { box: 40, stroke: 3.5, text: 'text-[11px]' },
  md: { box: 56, stroke: 4.5, text: 'text-sm' },
  lg: { box: 92, stroke: 6, text: 'text-2xl' },
}

export function ScoreDial({ score, capped = false, size = 'md', label }: Props) {
  const { box, stroke, text } = DIMENSIONS[size]
  const radius = (box - stroke) / 2
  const circumference = 2 * Math.PI * radius

  // The arc occupies three quarters of the circle, leaving a gap at the bottom
  // so the shape reads as a gauge rather than a pie chart.
  const sweep = 0.75
  const trackLength = circumference * sweep

  const [displayed, setDisplayed] = useState(0)

  useEffect(() => {
    // One frame at zero, so the transition has somewhere to travel from.
    const frame = requestAnimationFrame(() => setDisplayed(score))
    return () => cancelAnimationFrame(frame)
  }, [score])

  const tone = capped
    ? 'var(--color-warning)'
    : score >= 75
      ? 'var(--color-positive)'
      : score >= 55
        ? 'var(--color-accent)'
        : 'var(--color-ink-400)'

  return (
    <div className="relative inline-flex shrink-0 flex-col items-center">
      <svg
        width={box}
        height={box}
        viewBox={`0 0 ${box} ${box}`}
        className="-rotate-[225deg]"
        role="img"
        aria-label={`Match score ${score.toFixed(1)} out of 100${capped ? ', capped by a missing mandatory requirement' : ''}`}
      >
        <circle
          cx={box / 2}
          cy={box / 2}
          r={radius}
          fill="none"
          stroke="var(--color-surface-sunk)"
          strokeWidth={stroke}
          strokeLinecap="round"
          strokeDasharray={`${trackLength} ${circumference}`}
        />
        <circle
          cx={box / 2}
          cy={box / 2}
          r={radius}
          fill="none"
          stroke={tone}
          strokeWidth={stroke}
          strokeLinecap="round"
          strokeDasharray={`${(trackLength * Math.min(100, displayed)) / 100} ${circumference}`}
          style={{ transition: 'stroke-dasharray 700ms cubic-bezier(0.16, 1, 0.3, 1)' }}
        />
      </svg>

      <div className="absolute inset-0 flex flex-col items-center justify-center">
        <span className={`tabular font-semibold text-ink-900 ${text}`}>{score.toFixed(0)}</span>
        {size === 'lg' && label && (
          <span className="mt-0.5 text-[10px] tracking-wide text-ink-400 uppercase">{label}</span>
        )}
      </div>
    </div>
  )
}

/**
 * Rank position. The leading three are given a filled medallion, because a
 * recruiter's attention goes to the top of a list and the interface should agree
 * with that rather than treating position 1 and position 27 identically.
 */
export function RankBadge({ position }: { position: number }) {
  const leading = position <= 3

  return (
    <span
      className={`tabular flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold ${
        leading
          ? 'bg-ink-900 text-white'
          : 'border border-line-strong bg-surface text-ink-400'
      }`}
      aria-label={`Rank ${position}`}
    >
      {position}
    </span>
  )
}
