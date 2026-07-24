import { useEffect, useRef, useState } from 'react'
import type { ReactNode } from 'react'

/** Shared primitives. Kept in one file so spacing and tone cannot drift apart. */

export function Card({
  title,
  action,
  children,
  className = '',
}: {
  title?: string
  action?: ReactNode
  children: ReactNode
  className?: string
}) {
  return (
    <section
      className={`rounded-[6px] border border-line bg-surface shadow-[var(--shadow-card)] ${className}`}
    >
      {(title || action) && (
        <header className="flex items-center justify-between gap-3 border-b border-line px-5 py-3">
          {title && (
            <h2 className="text-[11px] font-semibold tracking-[0.09em] text-ink-400 uppercase">
              {title}
            </h2>
          )}
          {action}
        </header>
      )}
      <div className="p-5">{children}</div>
    </section>
  )
}

type Tone = 'neutral' | 'accent' | 'positive' | 'warning' | 'danger'

const toneClasses: Record<Tone, string> = {
  neutral: 'bg-surface-sunk text-ink-700 border-transparent',
  accent: 'bg-accent-soft text-accent border-accent-line',
  positive: 'bg-positive-soft text-positive border-transparent',
  warning: 'bg-warning-soft text-warning border-transparent',
  danger: 'bg-danger-soft text-danger border-transparent',
}

export function Badge({ children, tone = 'neutral' }: { children: ReactNode; tone?: Tone }) {
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-[11px] font-medium whitespace-nowrap ${toneClasses[tone]}`}
    >
      {children}
    </span>
  )
}

export function Button({
  children,
  onClick,
  type = 'button',
  variant = 'primary',
  disabled,
  size = 'md',
}: {
  children: ReactNode
  onClick?: () => void
  type?: 'button' | 'submit'
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
  disabled?: boolean
  size?: 'sm' | 'md'
}) {
  const base =
    'inline-flex items-center justify-center rounded-[6px] font-medium transition-all duration-150 active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-50 disabled:active:scale-100'
  const sizing = size === 'sm' ? 'px-3 py-1.5 text-xs' : 'px-4 py-2 text-sm'
  const variants = {
    primary: 'bg-ink-900 text-white hover:bg-ink-800',
    secondary: 'border border-line-strong bg-surface text-ink-700 hover:border-ink-400 hover:text-ink-900',
    ghost: 'text-ink-500 hover:bg-surface-sunk hover:text-ink-900',
    danger: 'border border-danger/30 bg-surface text-danger hover:bg-danger-soft',
  }

  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={`${base} ${sizing} ${variants[variant]}`}
    >
      {children}
    </button>
  )
}

export function EmptyState({ title, detail }: { title: string; detail?: string }) {
  return (
    <div className="rounded-[6px] border border-dashed border-line-strong bg-surface-alt px-6 py-12 text-center">
      <p className="text-sm font-medium text-ink-700">{title}</p>
      {detail && <p className="mt-1 text-sm text-ink-400">{detail}</p>}
    </div>
  )
}

/**
 * Skeleton rows rather than a spinner.
 *
 * A spinner says only that something is happening. A skeleton says what shape is
 * arriving, so the layout does not jump when it does and the wait feels shorter
 * than the same wait behind a spinner.
 */
export function Loading({ rows = 3 }: { rows?: number }) {
  return (
    <div className="space-y-3" aria-busy="true" aria-live="polite">
      <span className="sr-only">Loading</span>
      {Array.from({ length: rows }).map((_, index) => (
        <div
          key={index}
          className="flex items-center gap-4 rounded-[6px] border border-line bg-surface p-5"
        >
          <div className="h-9 w-9 shrink-0 animate-pulse rounded-full bg-surface-sunk" />
          <div className="flex-1 space-y-2">
            <div className="h-3 w-1/3 animate-pulse rounded bg-surface-sunk" />
            <div className="h-3 w-1/2 animate-pulse rounded bg-surface-sunk" />
          </div>
          <div className="h-9 w-14 shrink-0 animate-pulse rounded bg-surface-sunk" />
        </div>
      ))}
    </div>
  )
}

export function ErrorNote({ children }: { children: ReactNode }) {
  return (
    <p
      role="alert"
      className="rounded-[6px] border border-danger/25 bg-danger-soft px-3 py-2 text-sm text-danger"
    >
      {children}
    </p>
  )
}

/**
 * A figure that counts up to its value on first appearance.
 *
 * The movement is what makes a dashboard read as live rather than printed. It is
 * short, and it settles on the true value rather than approximating it.
 */
export function CountUp({ value, decimals = 0 }: { value: number; decimals?: number }) {
  const [shown, setShown] = useState(0)
  const frame = useRef<number>(0)

  useEffect(() => {
    const duration = 550
    const start = performance.now()

    const step = (now: number) => {
      const progress = Math.min(1, (now - start) / duration)
      // Ease out, so the number decelerates into place instead of stopping dead.
      const eased = 1 - Math.pow(1 - progress, 3)
      setShown(value * eased)

      if (progress < 1) {
        frame.current = requestAnimationFrame(step)
      }
    }

    frame.current = requestAnimationFrame(step)
    return () => cancelAnimationFrame(frame.current)
  }, [value])

  return <>{shown.toFixed(decimals)}</>
}

export function Field({
  label,
  children,
  hint,
}: {
  label: string
  children: ReactNode
  hint?: string
}) {
  return (
    <label className="block">
      <span className="block text-sm font-medium text-ink-700">{label}</span>
      {children}
      {hint && <span className="mt-1 block text-xs text-ink-400">{hint}</span>}
    </label>
  )
}

export const inputClass =
  'mt-1.5 w-full rounded-[6px] border border-line-strong bg-surface px-3 py-2 text-sm transition-colors outline-none placeholder:text-ink-300 focus:border-accent focus:ring-4 focus:ring-accent-soft'

/**
 * Retained so existing call sites keep working. New surfaces should use the
 * score dial, which is the platform's signature reading of a score.
 */
export function ScoreBar({ score, capped = false }: { score: number; capped?: boolean }) {
  const tone = capped
    ? 'bg-warning'
    : score >= 75
      ? 'bg-positive'
      : score >= 55
        ? 'bg-accent'
        : 'bg-ink-300'

  return (
    <div className="flex items-center gap-2">
      <div className="h-1.5 w-20 overflow-hidden rounded-full bg-surface-sunk">
        <div
          className={`h-full ${tone}`}
          style={{ width: `${Math.min(100, score)}%`, transition: 'width 700ms cubic-bezier(0.16,1,0.3,1)' }}
        />
      </div>
      <span className="tabular text-sm font-semibold text-ink-900">{score.toFixed(1)}</span>
    </div>
  )
}
