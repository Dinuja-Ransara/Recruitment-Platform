import { NavLink, Outlet } from 'react-router-dom'
import { Roles, useAuth } from '../auth/AuthContext'
import { Brand } from './Brand'

interface NavItem {
  to: string
  label: string
  roles: string[]
}

/**
 * Navigation is filtered by role, so each portal only advertises what its user
 * can actually reach. The API enforces the same boundaries independently; this
 * exists so nobody is offered a door that will not open.
 */
const navigation: NavItem[] = [
  { to: '/candidate', label: 'Overview', roles: [Roles.Candidate] },
  { to: '/candidate/jobs', label: 'Find jobs', roles: [Roles.Candidate] },
  { to: '/candidate/applications', label: 'My applications', roles: [Roles.Candidate] },

  { to: '/recruiter', label: 'Overview', roles: [Roles.Recruiter] },
  { to: '/recruiter/jobs', label: 'Postings', roles: [Roles.Recruiter] },
  { to: '/recruiter/jobs/new', label: 'New posting', roles: [Roles.Recruiter] },

  { to: '/manager', label: 'Shortlists', roles: [Roles.HiringManager] },

  { to: '/admin', label: 'System', roles: [Roles.Administrator] },
  { to: '/admin/organizations', label: 'Organisations', roles: [Roles.Administrator] },
]

export function AppShell() {
  const { user, logout, hasRole } = useAuth()
  const visible = navigation.filter((item) => hasRole(...item.roles))

  return (
    <div className="min-h-full">
      <header className="sticky top-0 z-10 border-b border-line bg-surface/95 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-6 px-6 py-3">
          <Brand size="sm" />

          <nav className="hidden flex-1 items-center gap-1 md:flex">
            {visible.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to.split('/').length === 2}
                className={({ isActive }) =>
                  `rounded-[6px] px-3 py-1.5 text-sm transition-colors ${
                    isActive
                      ? 'bg-accent-soft font-medium text-accent'
                      : 'text-ink-700 hover:text-accent'
                  }`
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="flex items-center gap-3">
            <div className="text-right">
              <p className="text-sm font-medium text-ink-900">{user?.fullName}</p>
              <p className="text-xs text-ink-500">{user?.roles.join(', ')}</p>
            </div>
            <button
              onClick={logout}
              className="rounded-[6px] border border-line-strong px-3 py-1.5 text-sm text-ink-700 transition-colors hover:border-accent hover:text-accent"
            >
              Sign out
            </button>
          </div>
        </div>

        {/* Small screens get the navigation on its own row rather than losing it. */}
        <nav className="flex gap-1 overflow-x-auto border-t border-line px-6 py-2 md:hidden">
          {visible.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to.split('/').length === 2}
              className={({ isActive }) =>
                `rounded-[6px] px-3 py-1.5 text-sm whitespace-nowrap ${
                  isActive ? 'bg-accent-soft font-medium text-accent' : 'text-ink-700'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-8">
        <Outlet />
      </main>
    </div>
  )
}

export function PageHeading({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="mb-6">
      <h1 className="text-xl font-semibold text-ink-900">{title}</h1>
      {subtitle && <p className="mt-1 text-sm text-ink-500">{subtitle}</p>}
    </div>
  )
}
