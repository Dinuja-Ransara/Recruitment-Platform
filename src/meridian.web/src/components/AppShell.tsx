import { useState } from 'react'
import { Link, NavLink, Outlet } from 'react-router-dom'
import { Roles, useAuth } from '../auth/AuthContext'
import { Brand } from './Brand'

interface NavItem {
  to: string
  label: string
  hint: string
  roles: string[]
  end?: boolean
}

interface NavGroup {
  heading: string
  items: NavItem[]
}

/**
 * Navigation is grouped and filtered by role, so each portal shows only what its
 * user can reach. The API enforces the same boundaries independently; this is
 * usability, not security.
 */
const groups: NavGroup[] = [
  {
    heading: 'Candidate',
    items: [
      { to: '/candidate', label: 'Overview', hint: 'Recommendations and activity', roles: [Roles.Candidate], end: true },
      { to: '/candidate/jobs', label: 'Find jobs', hint: 'Search open postings', roles: [Roles.Candidate] },
      { to: '/candidate/applications', label: 'Applications', hint: 'Track your pipeline', roles: [Roles.Candidate] },
    ],
  },
  {
    heading: 'Recruiting',
    items: [
      { to: '/recruiter', label: 'Overview', hint: 'Pipeline at a glance', roles: [Roles.Recruiter], end: true },
      { to: '/recruiter/jobs', label: 'Postings', hint: 'Draft, publish and close', roles: [Roles.Recruiter] },
      { to: '/recruiter/jobs/new', label: 'New posting', hint: 'Create a vacancy', roles: [Roles.Recruiter] },
      { to: '/recruiter/analytics', label: 'Analytics', hint: 'Funnel and posting performance', roles: [Roles.Recruiter] },
    ],
  },
  {
    heading: 'Hiring',
    items: [{ to: '/manager', label: 'Shortlists', hint: 'Review and decide', roles: [Roles.HiringManager], end: true }],
  },
  {
    heading: 'Administration',
    items: [
      { to: '/admin', label: 'System', hint: 'Monitoring and audit', roles: [Roles.Administrator], end: true },
      { to: '/admin/analytics', label: 'Analytics', hint: 'Funnel across all clients', roles: [Roles.Administrator] },
      { to: '/admin/organizations', label: 'Organisations', hint: 'Clients and departments', roles: [Roles.Administrator] },
    ],
  },
]

function initials(name: string) {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
}

export function AppShell() {
  const { user, logout, hasRole } = useAuth()
  const [mobileOpen, setMobileOpen] = useState(false)

  const visibleGroups = groups
    .map((group) => ({ ...group, items: group.items.filter((item) => hasRole(...item.roles)) }))
    .filter((group) => group.items.length > 0)

  const sidebar = (
    <nav className="flex h-full flex-col gap-6 p-4">
      {visibleGroups.map((group) => (
        <div key={group.heading}>
          <p className="px-3 text-[11px] font-semibold tracking-[0.12em] text-ink-300 uppercase">
            {group.heading}
          </p>
          <ul className="mt-2 space-y-0.5">
            {group.items.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  end={item.end}
                  onClick={() => setMobileOpen(false)}
                  className={({ isActive }) =>
                    `block rounded-[6px] px-3 py-2 transition-colors ${
                      isActive
                        ? 'bg-accent-soft text-accent'
                        : 'text-ink-700 hover:bg-surface-alt hover:text-ink-900'
                    }`
                  }
                >
                  {({ isActive }) => (
                    <>
                      <span className={`block text-sm ${isActive ? 'font-medium' : ''}`}>
                        {item.label}
                      </span>
                      <span className="mt-0.5 block text-xs text-ink-300">{item.hint}</span>
                    </>
                  )}
                </NavLink>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </nav>
  )

  return (
    <div className="min-h-full">
      {/* Top bar spans the full width so the brand and account are always present. */}
      <header className="sticky top-0 z-30 border-b border-line bg-surface">
        <div className="flex items-center justify-between gap-4 px-4 py-3 sm:px-6">
          <div className="flex items-center gap-3">
            <button
              onClick={() => setMobileOpen((open) => !open)}
              aria-label="Toggle navigation"
              aria-expanded={mobileOpen}
              className="rounded-[6px] border border-line-strong p-1.5 text-ink-700 lg:hidden"
            >
              <svg width="18" height="18" viewBox="0 0 18 18" aria-hidden="true">
                <path d="M2 4.5h14M2 9h14M2 13.5h14" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
              </svg>
            </button>
            <Link to="/">
              <Brand size="sm" />
            </Link>
          </div>

          <div className="flex items-center gap-3">
            <div className="hidden text-right sm:block">
              <p className="text-sm font-medium text-ink-900">{user?.fullName}</p>
              <p className="text-xs text-ink-500">
                {user?.roles.join(', ')}
                {user?.organizationName && ` · ${user.organizationName}`}
              </p>
            </div>

            <span
              aria-hidden="true"
              className="flex h-9 w-9 items-center justify-center rounded-full bg-accent-soft text-sm font-semibold text-accent"
            >
              {initials(user?.fullName ?? '')}
            </span>

            <button
              onClick={logout}
              className="rounded-[6px] border border-line-strong px-3 py-1.5 text-sm text-ink-700 transition-colors hover:border-accent hover:text-accent"
            >
              Sign out
            </button>
          </div>
        </div>
      </header>

      <div className="mx-auto flex max-w-[1400px]">
        {/* Sidebar. Fixed on large screens, collapsible below. */}
        <aside className="sticky top-[57px] hidden h-[calc(100vh-57px)] w-60 shrink-0 overflow-y-auto border-r border-line bg-surface lg:block">
          {sidebar}
        </aside>

        {mobileOpen && (
          <div className="fixed inset-0 top-[57px] z-20 lg:hidden">
            <button
              aria-label="Close navigation"
              onClick={() => setMobileOpen(false)}
              className="absolute inset-0 bg-ink-900/20"
            />
            <div className="relative h-full w-64 overflow-y-auto border-r border-line bg-surface">
              {sidebar}
            </div>
          </div>
        )}

        <main className="min-w-0 flex-1 px-4 py-6 sm:px-6 lg:px-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

/**
 * Page heading with optional actions. Keeps the top of every screen consistent
 * so a user always knows where the primary action lives.
 */
export function PageHeading({
  title,
  subtitle,
  actions,
}: {
  title: string
  subtitle?: string
  actions?: React.ReactNode
}) {
  return (
    <div className="mb-6 flex flex-wrap items-start justify-between gap-3 border-b border-line pb-4">
      <div>
        <h1 className="text-xl font-semibold text-ink-900">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-ink-500">{subtitle}</p>}
      </div>
      {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
    </div>
  )
}
