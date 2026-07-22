import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { api, tokenStore, type LoginResponse, type UserSummary } from '../lib/api'

export const Roles = {
  Candidate: 'Candidate',
  Recruiter: 'Recruiter',
  HiringManager: 'HiringManager',
  Administrator: 'Administrator',
} as const

interface AuthState {
  user: UserSummary | null
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  hasRole: (...roles: string[]) => boolean
}

const AuthContext = createContext<AuthState | undefined>(undefined)

/**
 * Holds the authenticated session.
 *
 * The token lives in localStorage, but the user object is always re-fetched from
 * /api/auth/me on mount rather than being cached alongside it. That means a token
 * revoked or expired server-side fails immediately on reload, instead of the UI
 * trusting a stale copy of the user's roles.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserSummary | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const token = tokenStore.get()
    if (!token) {
      setIsLoading(false)
      return
    }

    api
      .get<UserSummary>('/api/auth/me')
      .then(setUser)
      .catch(() => {
        tokenStore.clear()
        setUser(null)
      })
      .finally(() => setIsLoading(false))
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const result = await api.post<LoginResponse>('/api/auth/login', { email, password })
    tokenStore.set(result.accessToken)
    setUser(result.user)
  }, [])

  const logout = useCallback(() => {
    tokenStore.clear()
    setUser(null)
  }, [])

  const hasRole = useCallback(
    (...roles: string[]) => !!user && roles.some((role) => user.roles.includes(role)),
    [user],
  )

  const value = useMemo(
    () => ({ user, isLoading, login, logout, hasRole }),
    [user, isLoading, login, logout, hasRole],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside an AuthProvider.')
  }
  return context
}
