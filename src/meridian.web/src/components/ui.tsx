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
    <section className={`rounded-[6px] border border-line bg-surface ${className}`}>
      {(title || action) && (
        <header className="flex items-center justify-between border-b border-line px-5 py-3">
          {title && (
            <h2 className="text-xs font-semibold tracking-wide text-ink-500 uppercase">{title}</h2>
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
  neutral: 'bg-surface-alt text-ink-700 border-line-strong',
  accent: 'bg-accent-soft text-accent border-accent/30',
  positive: 'bg-positive/10 text-positive border-positive/30',
  warning: 'bg-warning/10 text-warning border-warning/30',
  danger: 'bg-danger/10 text-danger border-danger/30',
}

export function Badge({ children, tone = 'neutral' }: { children: ReactNode; tone?: Tone }) {
  return (
    <span
      className={`inline-flex items-center rounded-[4px] border px-2 py-0.5 text-xs font-medium ${toneClasses[tone]}`}
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
  variant?: 'primary' | 'secondary' | 'danger'
  disabled?: boolean
  size?: 'sm' | 'md'
}) {
  const base =
    'inline-flex items-center justify-center rounded-[6px] font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50'
  const sizing = size === 'sm' ? 'px-2.5 py-1.5 text-xs' : 'px-4 py-2 text-sm'
  const variants = {
    primary: 'bg-accent text-white hover:bg-accent-hover',
    secondary: 'border border-line-strong text-ink-700 hover:border-accent hover:text-accent',
    danger: 'border border-danger/40 text-danger hover:bg-danger/5',
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
    <div className="rounded-[6px] border border-dashed border-line-strong px-6 py-10 text-center">
      <p className="text-sm font-medium text-ink-700">{title}</p>
      {detail && <p className="mt-1 text-sm text-ink-500">{detail}</p>}
    </div>
  )
}

export function Loading({ label = 'Loading' }: { label?: string }) {
  return <p className="py-8 text-center text-sm text-ink-500">{label}...</p>
}

export function ErrorNote({ children }: { children: ReactNode }) {
  return (
    <p
      role="alert"
      className="rounded-[6px] border border-danger/30 bg-danger/5 px-3 py-2 text-sm text-danger"
    >
      {children}
    </p>
  )
}

/**
 * A score with a filled bar. The bar is not decoration: at a glance it is the
 * difference between an 84 and a 46 in a list of forty applicants.
 */
export function ScoreBar({ score, capped = false }: { score: number; capped?: boolean }) {
  const tone = capped
    ? 'bg-warning'
    : score >= 70
      ? 'bg-positive'
      : score >= 50
        ? 'bg-accent'
        : 'bg-ink-300'

  return (
    <div className="flex items-center gap-2">
      <div className="h-1.5 w-20 overflow-hidden rounded-full bg-surface-alt">
        <div className={`h-full ${tone}`} style={{ width: `${Math.min(100, score)}%` }} />
      </div>
      <span className="tabular text-sm font-semibold text-ink-900">{score.toFixed(1)}</span>
    </div>
  )
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
      {hint && <span className="mt-1 block text-xs text-ink-500">{hint}</span>}
    </label>
  )
}

export const inputClass =
  'mt-1.5 w-full rounded-[6px] border border-line-strong bg-surface px-3 py-2 text-sm outline-none focus:border-accent focus:ring-2 focus:ring-accent-soft'
