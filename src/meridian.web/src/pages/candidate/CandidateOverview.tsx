import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../../lib/api'
import { useAuth } from '../../auth/AuthContext'
import {
  appStatusLabel,
  formatDate,
  workModeLabel,
  type ApplicationSummary,
  type JobRecommendation,
} from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Badge, Card, CountUp, EmptyState, Loading } from '../../components/ui'
import { ScoreDial } from '../../components/ScoreDial'
import { ResumesCard } from '../../components/ResumesCard'

export function CandidateOverview() {
  const { user } = useAuth()
  const [recommendations, setRecommendations] = useState<JobRecommendation[] | null>(null)
  const [applications, setApplications] = useState<ApplicationSummary[] | null>(null)

  useEffect(() => {
    api.get<JobRecommendation[]>('/api/applications/recommendations?take=5').then(setRecommendations)
    api.get<ApplicationSummary[]>('/api/applications/mine').then(setApplications)
  }, [])

  const active = applications?.filter((a) => a.status < 6) ?? []

  return (
    <>
      <PageHeading
        title={`Welcome back, ${user?.fullName.split(' ')[0] ?? ''}`}
        subtitle="Recommendations are scored against your CV, and every score can be opened up."
      />

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Card
            title="Recommended for you"
            action={
              <Link to="/candidate/jobs" className="text-sm text-accent hover:underline">
                Browse all
              </Link>
            }
          >
            {recommendations === null ? (
              <Loading />
            ) : recommendations.length === 0 ? (
              <EmptyState
                title="No open postings yet"
                detail="Recommendations appear once recruiters publish roles."
              />
            ) : (
              <ul className="stagger divide-y divide-line">
                {recommendations.map((rec) => (
                  <li key={rec.jobPostingId} className="py-3 first:pt-0 last:pb-0">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div className="min-w-0">
                        <Link
                          to={`/candidate/jobs/${rec.jobPostingId}`}
                          className="font-medium text-ink-900 hover:text-accent"
                        >
                          {rec.title}
                        </Link>
                        <p className="mt-0.5 text-sm text-ink-500">
                          {rec.organizationName} &middot; {rec.city}, {rec.country} &middot;{' '}
                          {workModeLabel[rec.workMode]}
                        </p>
                        <p className="mt-1 text-xs text-ink-500">{rec.explanation.summary}</p>
                      </div>
                      <div className="flex items-center gap-3">
                        {rec.alreadyApplied && <Badge tone="accent">Applied</Badge>}
                        <ScoreDial
                          score={rec.explanation.score}
                          capped={rec.explanation.hasMandatoryGap}
                        />
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </Card>
        </div>

        <div className="space-y-6">
          <Card title="Application activity">
            {applications === null ? (
              <Loading />
            ) : applications.length === 0 ? (
              <EmptyState title="No applications yet" />
            ) : (
              <>
                <div className="grid grid-cols-2 gap-3">
                  <Metric label="Total" value={applications.length} />
                  <Metric label="In progress" value={active.length} />
                </div>
                <ul className="mt-4 space-y-3 border-t border-line pt-4">
                  {applications.slice(0, 4).map((app) => (
                    <li key={app.id} className="text-sm">
                      <Link
                        to={`/candidate/applications/${app.id}`}
                        className="font-medium text-ink-900 hover:text-accent"
                      >
                        {app.jobTitle}
                      </Link>
                      <p className="mt-0.5 flex items-center gap-2 text-xs text-ink-500">
                        <Badge>{appStatusLabel[app.status]}</Badge>
                        {formatDate(app.submittedAt)}
                      </p>
                    </li>
                  ))}
                </ul>
              </>
            )}
          </Card>

          <ResumesCard />
        </div>
      </div>
    </>
  )
}

function Metric({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-[6px] border border-line bg-surface-alt px-3 py-2.5">
      <p className="tabular text-xl font-semibold text-ink-900">
        <CountUp value={value} />
      </p>
      <p className="mt-0.5 text-xs text-ink-500">{label}</p>
    </div>
  )
}
