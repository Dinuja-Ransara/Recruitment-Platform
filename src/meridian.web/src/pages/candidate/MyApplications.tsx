import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ApiError, api } from '../../lib/api'
import {
  appStatusLabel,
  formatDate,
  type ApplicationDetail,
  type ApplicationSummary,
} from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Badge, Button, Card, EmptyState, ErrorNote, Loading } from '../../components/ui'
import { ScoreDial } from '../../components/ScoreDial'
import { ExplanationPanel } from '../../components/ExplanationPanel'

function statusTone(status: number) {
  if (status === 6) return 'positive' as const
  if (status === 7 || status === 8) return 'danger' as const
  if (status >= 2) return 'accent' as const
  return 'neutral' as const
}

export function MyApplications() {
  const [applications, setApplications] = useState<ApplicationSummary[] | null>(null)

  useEffect(() => {
    api.get<ApplicationSummary[]>('/api/applications/mine').then(setApplications)
  }, [])

  return (
    <>
      <PageHeading title="My applications" subtitle="Every application you have submitted." />

      {applications === null ? (
        <Loading />
      ) : applications.length === 0 ? (
        <EmptyState title="Nothing submitted yet" detail="Applications appear here once you apply." />
      ) : (
        <ul className="stagger space-y-3">
          {applications.map((app) => (
            <li key={app.id}>
              <Link
                to={`/candidate/applications/${app.id}`}
                className="block rounded-[6px] border border-line bg-surface p-5 shadow-[var(--shadow-card)] transition-all hover:border-line-strong hover:shadow-[var(--shadow-lift)]"
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h3 className="font-medium text-ink-900">{app.jobTitle}</h3>
                    <p className="mt-0.5 text-sm text-ink-500">
                      {app.organizationName} &middot; {app.city}, {app.country}
                    </p>
                    {app.matchSummary && (
                      <p className="mt-1.5 text-xs text-ink-500">{app.matchSummary}</p>
                    )}
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="flex flex-col items-end gap-1.5">
                      <Badge tone={statusTone(app.status)}>{appStatusLabel[app.status]}</Badge>
                      <span className="text-xs text-ink-400">{formatDate(app.submittedAt)}</span>
                    </div>
                    {app.matchScore != null && <ScoreDial score={app.matchScore} />}
                  </div>
                </div>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </>
  )
}

export function ApplicationDetailPage() {
  const { id } = useParams()
  const [application, setApplication] = useState<ApplicationDetail | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  function load() {
    api
      .get<ApplicationDetail>(`/api/applications/${id}`)
      .then(setApplication)
      .catch((e: ApiError) => setError(e.message))
  }

  useEffect(load, [id])

  async function withdraw() {
    setBusy(true)
    setError(null)
    try {
      const updated = await api.post<ApplicationDetail>(`/api/applications/${id}/withdraw`, {})
      setApplication(updated)
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Could not withdraw.')
    } finally {
      setBusy(false)
    }
  }

  if (error && !application) return <ErrorNote>{error}</ErrorNote>
  if (!application) return <Loading />

  const canWithdraw = application.status < 6

  return (
    <>
      <PageHeading
        title={application.jobTitle}
        subtitle={`${application.organizationName} · submitted ${formatDate(application.submittedAt)}`}
      />

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          {application.explanation && (
            <Card title="How your CV scored against this posting">
              <ExplanationPanel explanation={application.explanation} />
            </Card>
          )}

          {application.coverLetter && (
            <Card title="Your cover letter">
              <p className="text-sm leading-relaxed whitespace-pre-line text-ink-700">
                {application.coverLetter}
              </p>
            </Card>
          )}
        </div>

        <div className="space-y-6">
          <Card title="Status">
            <Badge tone={statusTone(application.status)}>{appStatusLabel[application.status]}</Badge>

            {error && (
              <div className="mt-3">
                <ErrorNote>{error}</ErrorNote>
              </div>
            )}

            {canWithdraw && (
              <div className="mt-4">
                <Button variant="danger" size="sm" onClick={withdraw} disabled={busy}>
                  Withdraw application
                </Button>
              </div>
            )}
          </Card>

          <Card title="Timeline">
            {application.timeline.length === 0 ? (
              <p className="text-sm text-ink-500">No activity recorded.</p>
            ) : (
              <ol className="space-y-4">
                {application.timeline.map((event, index) => (
                  <li key={index} className="relative border-l border-line pl-4">
                    <span className="absolute top-1.5 -left-[3px] h-1.5 w-1.5 rounded-full bg-accent" />
                    <p className="text-sm font-medium text-ink-900">
                      {appStatusLabel[event.toStatus]}
                    </p>
                    {event.note && <p className="mt-0.5 text-xs text-ink-700">{event.note}</p>}
                    <p className="mt-0.5 text-xs text-ink-300">
                      {formatDate(event.occurredAt)}
                      {event.actorName && ` · ${event.actorName}`}
                    </p>
                  </li>
                ))}
              </ol>
            )}
          </Card>
        </div>
      </div>
    </>
  )
}
