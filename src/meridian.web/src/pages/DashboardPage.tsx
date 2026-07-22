import { useEffect, useState } from 'react'
import { api, ApiError } from '../lib/api'
import { Roles, useAuth } from '../auth/AuthContext'

interface Department {
  id: number
  name: string
  costCentre: string | null
}

interface Organization {
  id: number
  name: string
  industry: string
  city: string
  country: string
  departments: Department[]
}

interface SystemHealth {
  generatedAtUtc: string
  counts: Record<string, number>
  recentSecurityEvents: {
    action: string
    entityName: string
    entityId: string | null
    ipAddress: string | null
    occurredAt: string
  }[]
}

/**
 * Placeholder landing surface for P0.
 *
 * Its purpose is to prove the vertical slice: a real JWT reaches the API, role
 * policies are enforced server-side, and the data rendered came out of SQL
 * Server. The four role-specific portals replace it in the next phase.
 */
export function DashboardPage() {
  const { user, logout, hasRole } = useAuth()
  const [organizations, setOrganizations] = useState<Organization[] | null>(null)
  const [health, setHealth] = useState<SystemHealth | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (hasRole(Roles.Recruiter, Roles.HiringManager, Roles.Administrator)) {
      api
        .get<Organization[]>('/api/organizations')
        .then(setOrganizations)
        .catch((caught: ApiError) => setError(caught.message))
    }

    if (hasRole(Roles.Administrator)) {
      api
        .get<SystemHealth>('/api/admin/system-health')
        .then(setHealth)
        .catch(() => undefined)
    }
  }, [hasRole])

  return (
    <div className="min-h-full">
      <header className="border-b border-line bg-surface">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <div>
            <p className="text-sm font-semibold tracking-[0.18em] text-accent uppercase">
              Meridian
            </p>
            <p className="text-xs text-ink-500">Talent Platform</p>
          </div>

          <div className="flex items-center gap-4">
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
      </header>

      <main className="mx-auto max-w-6xl space-y-6 px-6 py-8">
        <section className="rounded-[6px] border border-line bg-surface p-6">
          <h1 className="text-lg font-semibold text-ink-900">
            Signed in as {user?.fullName}
          </h1>
          <dl className="mt-4 grid gap-4 text-sm sm:grid-cols-3">
            <div>
              <dt className="text-ink-500">Email</dt>
              <dd className="mt-0.5 font-medium text-ink-900">{user?.email}</dd>
            </div>
            <div>
              <dt className="text-ink-500">Roles</dt>
              <dd className="mt-0.5 font-medium text-ink-900">
                {user?.roles.join(', ')}
              </dd>
            </div>
            <div>
              <dt className="text-ink-500">Organisation</dt>
              <dd className="mt-0.5 font-medium text-ink-900">
                {user?.organizationName ?? 'Not affiliated'}
              </dd>
            </div>
          </dl>
        </section>

        {health && (
          <section className="rounded-[6px] border border-line bg-surface p-6">
            <h2 className="text-sm font-semibold tracking-wide text-ink-500 uppercase">
              System monitoring
            </h2>
            <div className="mt-4 grid gap-3 sm:grid-cols-3 lg:grid-cols-5">
              {Object.entries(health.counts).map(([label, value]) => (
                <div
                  key={label}
                  className="rounded-[6px] border border-line bg-surface-alt px-3 py-2.5"
                >
                  <p className="tabular text-xl font-semibold text-ink-900">{value}</p>
                  <p className="mt-0.5 text-xs text-ink-500 capitalize">
                    {label.replace(/([A-Z])/g, ' $1').trim()}
                  </p>
                </div>
              ))}
            </div>

            <h3 className="mt-6 text-sm font-semibold tracking-wide text-ink-500 uppercase">
              Recent security events
            </h3>
            <div className="mt-3 overflow-x-auto">
              <table className="w-full min-w-[520px] text-left text-sm">
                <thead>
                  <tr className="border-b border-line text-xs tracking-wide text-ink-500 uppercase">
                    <th className="py-2 pr-4 font-medium">Action</th>
                    <th className="py-2 pr-4 font-medium">Entity</th>
                    <th className="py-2 pr-4 font-medium">Source IP</th>
                    <th className="py-2 font-medium">Occurred</th>
                  </tr>
                </thead>
                <tbody>
                  {health.recentSecurityEvents.map((event, index) => (
                    <tr key={index} className="border-b border-line last:border-0">
                      <td className="py-2 pr-4 font-medium text-ink-900">
                        {event.action}
                      </td>
                      <td className="py-2 pr-4 text-ink-700">
                        {event.entityName} #{event.entityId}
                      </td>
                      <td className="tabular py-2 pr-4 text-ink-500">
                        {event.ipAddress ?? '-'}
                      </td>
                      <td className="tabular py-2 text-ink-500">
                        {new Date(event.occurredAt).toLocaleString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}

        {organizations && (
          <section className="rounded-[6px] border border-line bg-surface p-6">
            <h2 className="text-sm font-semibold tracking-wide text-ink-500 uppercase">
              Client organisations
            </h2>
            <div className="mt-4 grid gap-4 md:grid-cols-3">
              {organizations.map((organization) => (
                <article
                  key={organization.id}
                  className="rounded-[6px] border border-line bg-surface-alt p-4"
                >
                  <h3 className="font-medium text-ink-900">{organization.name}</h3>
                  <p className="mt-0.5 text-xs text-ink-500">
                    {organization.industry}
                  </p>
                  <p className="mt-2 text-xs text-ink-700">
                    {organization.city}, {organization.country}
                  </p>
                  <ul className="mt-3 space-y-1 border-t border-line pt-3">
                    {organization.departments.map((department) => (
                      <li
                        key={department.id}
                        className="flex justify-between text-xs text-ink-700"
                      >
                        <span>{department.name}</span>
                        <span className="tabular text-ink-300">
                          {department.costCentre}
                        </span>
                      </li>
                    ))}
                  </ul>
                </article>
              ))}
            </div>
          </section>
        )}

        {hasRole(Roles.Candidate) && (
          <section className="rounded-[6px] border border-line bg-surface p-6">
            <h2 className="text-sm font-semibold tracking-wide text-ink-500 uppercase">
              Candidate access
            </h2>
            <p className="mt-2 text-sm text-ink-700">
              This account holds the Candidate role, so the client organisation
              directory is not requested. A direct call to that endpoint with this
              token is rejected by the API with 403 Forbidden.
            </p>
          </section>
        )}

        {error && (
          <p
            role="alert"
            className="rounded-[6px] border border-danger/30 bg-danger/5 px-4 py-3 text-sm text-danger"
          >
            {error}
          </p>
        )}
      </main>
    </div>
  )
}
