import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import { Roles, useAuth } from '../auth/AuthContext'
import {
  formatDate,
  jobStatusLabel,
  type JobSummary,
  type Organization,
  type Paged,
  type SystemHealth,
} from '../lib/types'
import { PageHeading } from '../components/AppShell'
import { Badge, Card, EmptyState, Loading } from '../components/ui'

/**
 * Overview for recruiting staff and administrators.
 *
 * One component rather than three near-identical ones: the sections it renders
 * are chosen by role, which keeps the layout and tone consistent across the
 * three staff portals without duplicating them.
 */
export function StaffOverview() {
  const { user, hasRole } = useAuth()
  const [jobs, setJobs] = useState<JobSummary[] | null>(null)
  const [health, setHealth] = useState<SystemHealth | null>(null)
  const [organizations, setOrganizations] = useState<Organization[] | null>(null)

  useEffect(() => {
    api
      .get<Paged<JobSummary>>('/api/jobs/mine?pageSize=50')
      .then((r) => setJobs(r.items))
      .catch(() => setJobs([]))

    api.get<Organization[]>('/api/organizations').then(setOrganizations).catch(() => setOrganizations([]))

    if (hasRole(Roles.Administrator)) {
      api.get<SystemHealth>('/api/admin/system-health').then(setHealth).catch(() => undefined)
    }
  }, [hasRole])

  const published = jobs?.filter((j) => j.status === 1) ?? []
  const totalApplicants = jobs?.reduce((sum, j) => sum + j.applicationCount, 0) ?? 0

  return (
    <>
      <PageHeading
        title={`Welcome back, ${user?.fullName.split(' ')[0] ?? ''}`}
        subtitle={user?.organizationName ?? undefined}
      />

      <div className="mb-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <Metric label="Postings" value={jobs?.length ?? 0} />
        <Metric label="Published" value={published.length} />
        <Metric label="Applicants" value={totalApplicants} />
        <Metric label="Client organisations" value={organizations?.length ?? 0} />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Card
            title="Recent postings"
            action={
              hasRole(Roles.Recruiter) ? (
                <Link to="/recruiter/jobs" className="text-sm text-accent hover:underline">
                  Manage
                </Link>
              ) : undefined
            }
          >
            {jobs === null ? (
              <Loading />
            ) : jobs.length === 0 ? (
              <EmptyState title="No postings for your organisation yet" />
            ) : (
              <ul className="divide-y divide-line">
                {jobs.slice(0, 6).map((job) => (
                  <li key={job.id} className="flex items-center justify-between gap-4 py-3 first:pt-0 last:pb-0">
                    <div className="min-w-0">
                      <Link
                        to={`/recruiter/jobs/${job.id}/applicants`}
                        className="font-medium text-ink-900 hover:text-accent"
                      >
                        {job.title}
                      </Link>
                      <p className="mt-0.5 text-xs text-ink-500">
                        {job.city}, {job.country} &middot; closes {formatDate(job.closingDate)}
                      </p>
                    </div>
                    <div className="flex shrink-0 items-center gap-2">
                      <span className="tabular text-sm text-ink-700">{job.applicationCount}</span>
                      <Badge tone={job.status === 1 ? 'positive' : 'neutral'}>
                        {jobStatusLabel[job.status]}
                      </Badge>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </Card>
        </div>

        <div className="space-y-6">
          {health && (
            <Card title="System monitoring">
              <div className="grid grid-cols-2 gap-2">
                {Object.entries(health.counts).map(([label, value]) => (
                  <div key={label} className="rounded-[6px] border border-line bg-surface-alt px-2.5 py-2">
                    <p className="tabular text-base font-semibold text-ink-900">{value}</p>
                    <p className="text-xs text-ink-500 capitalize">
                      {label.replace(/([A-Z])/g, ' $1').trim()}
                    </p>
                  </div>
                ))}
              </div>
            </Card>
          )}

          {health && (
            <Card title="Recent security events">
              <ul className="space-y-2.5">
                {health.recentSecurityEvents.slice(0, 6).map((event, i) => (
                  <li key={i} className="text-sm">
                    <p className="font-medium text-ink-900">{event.action}</p>
                    <p className="tabular text-xs text-ink-500">
                      {event.entityName} #{event.entityId} &middot; {event.ipAddress ?? '-'} &middot;{' '}
                      {new Date(event.occurredAt).toLocaleString()}
                    </p>
                  </li>
                ))}
              </ul>
            </Card>
          )}
        </div>
      </div>
    </>
  )
}

function Metric({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-[6px] border border-line bg-surface px-4 py-3">
      <p className="tabular text-2xl font-semibold text-ink-900">{value}</p>
      <p className="mt-0.5 text-xs text-ink-500">{label}</p>
    </div>
  )
}

export function OrganizationsPage() {
  const [organizations, setOrganizations] = useState<Organization[] | null>(null)

  useEffect(() => {
    api.get<Organization[]>('/api/organizations').then(setOrganizations)
  }, [])

  return (
    <>
      <PageHeading title="Client organisations" subtitle="Companies the consultancy recruits for." />

      {organizations === null ? (
        <Loading />
      ) : (
        <div className="grid gap-4 md:grid-cols-3">
          {organizations.map((org) => (
            <Card key={org.id} title={org.name}>
              <p className="text-sm text-ink-500">{org.industry}</p>
              <p className="mt-1 text-sm text-ink-700">
                {org.city}, {org.country}
              </p>
              <ul className="mt-4 space-y-1.5 border-t border-line pt-3">
                {org.departments.map((d) => (
                  <li key={d.id} className="flex justify-between text-sm">
                    <span className="text-ink-700">{d.name}</span>
                    <span className="tabular text-xs text-ink-300">{d.costCentre}</span>
                  </li>
                ))}
              </ul>
            </Card>
          ))}
        </div>
      )}
    </>
  )
}
