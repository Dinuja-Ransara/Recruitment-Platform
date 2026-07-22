import { useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { ApiError } from '../lib/api'
import { Brand } from '../components/Brand'

const demoAccounts = [
  { role: 'Administrator', email: 'admin@meridian.example.com' },
  { role: 'Recruiter', email: 'recruiter@meridian.example.com' },
  { role: 'Hiring Manager', email: 'manager@meridian.example.com' },
  { role: 'Candidate', email: 'candidate@meridian.example.com' },
]

const DEMO_PASSWORD = 'Meridian#2026'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)

    try {
      await login(email, password)
      const from = (location.state as { from?: string } | null)?.from
      navigate(from ?? '/dashboard', { replace: true })
    } catch (caught) {
      setError(
        caught instanceof ApiError
          ? caught.message
          : 'Could not reach the server. Check that the API is running.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  function useDemoAccount(demoEmail: string) {
    setEmail(demoEmail)
    setPassword(DEMO_PASSWORD)
    setError(null)
  }

  return (
    <div className="grid min-h-full lg:grid-cols-2">
      {/* Brand panel. Hidden below lg so the form owns a small screen entirely. */}
      <div className="hidden flex-col justify-between bg-ink-900 p-12 text-white lg:flex">
        <Brand onDark />

        <div className="max-w-md">
          <h1 className="text-3xl leading-tight font-semibold">
            Recruitment decisions that can explain themselves.
          </h1>
          <p className="mt-4 text-sm leading-relaxed text-ink-300">
            Candidate ranking, skill extraction and job matching run on a scoring
            engine that reports which factors produced each result, rather than
            returning a number and asking to be trusted.
          </p>
        </div>

        <p className="text-xs text-ink-500">
          SE205.3 Software Architecture &middot; NSBM Green University
        </p>
      </div>

      {/* Sign-in panel */}
      <div className="flex items-center justify-center p-6 sm:p-12">
        <div className="w-full max-w-sm">
          <h2 className="text-xl font-semibold text-ink-900">Sign in</h2>
          <p className="mt-1 text-sm text-ink-500">
            Use your Meridian account to continue.
          </p>

          <form onSubmit={handleSubmit} className="mt-8 space-y-4" noValidate>
            <div>
              <label
                htmlFor="email"
                className="block text-sm font-medium text-ink-700"
              >
                Email address
              </label>
              <input
                id="email"
                type="email"
                required
                autoComplete="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                className="mt-1.5 w-full rounded-[6px] border border-line-strong bg-surface px-3 py-2 text-sm outline-none focus:border-accent focus:ring-2 focus:ring-accent-soft"
              />
            </div>

            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-ink-700"
              >
                Password
              </label>
              <input
                id="password"
                type="password"
                required
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                className="mt-1.5 w-full rounded-[6px] border border-line-strong bg-surface px-3 py-2 text-sm outline-none focus:border-accent focus:ring-2 focus:ring-accent-soft"
              />
            </div>

            {error && (
              <p
                role="alert"
                className="rounded-[6px] border border-danger/30 bg-danger/5 px-3 py-2 text-sm text-danger"
              >
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-[6px] bg-accent px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? 'Signing in...' : 'Sign in'}
            </button>
          </form>

          <div className="mt-10 border-t border-line pt-6">
            <p className="text-xs font-medium tracking-wide text-ink-500 uppercase">
              Demonstration accounts
            </p>
            <div className="mt-3 space-y-1.5">
              {demoAccounts.map((account) => (
                <button
                  key={account.email}
                  type="button"
                  onClick={() => useDemoAccount(account.email)}
                  className="flex w-full items-center justify-between rounded-[6px] border border-line bg-surface px-3 py-2 text-left text-sm transition-colors hover:border-accent hover:bg-accent-soft"
                >
                  <span className="font-medium text-ink-700">{account.role}</span>
                  <span className="text-xs text-ink-500">{account.email}</span>
                </button>
              ))}
            </div>
            <p className="mt-3 text-xs text-ink-300">
              All demonstration accounts share the password {DEMO_PASSWORD}
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
