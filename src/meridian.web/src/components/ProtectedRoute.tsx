import { Navigate, useLocation } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from '../auth/AuthContext'

interface Props {
  children: ReactNode
  /** When supplied, the user must hold at least one of these roles. */
  roles?: string[]
}

/**
 * Route guard.
 *
 * This is a usability control, not a security control: it stops a candidate
 * navigating into a recruiter screen and seeing an empty page. The actual
 * enforcement is the [Authorize] policy on the API, which is what a request
 * crafted outside the browser would still hit.
 */
export function ProtectedRoute({ children, roles }: Props) {
  const { user, isLoading, hasRole } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center text-ink-500">
        Checking your session...
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />
  }

  if (roles && !hasRole(...roles)) {
    return <Navigate to="/dashboard" replace />
  }

  return <>{children}</>
}
