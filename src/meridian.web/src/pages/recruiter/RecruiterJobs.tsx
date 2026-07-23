import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ApiError, api } from '../../lib/api'
import {
  formatDate,
  jobStatusLabel,
  seniorityLabel,
  workModeLabel,
  type JobDetail,
  type JobSummary,
  type Paged,
} from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Badge, Button, Card, EmptyState, ErrorNote, Loading } from '../../components/ui'

function statusTone(status: number) {
  if (status === 1) return 'positive' as const
  if (status === 3) return 'danger' as const
  if (status === 2) return 'warning' as const
  return 'neutral' as const
}

export function RecruiterJobs() {
  const [jobs, setJobs] = useState<JobSummary[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  function load() {
    api
      .get<Paged<JobSummary>>('/api/jobs/mine?pageSize=50')
      .then((r) => setJobs(r.items))
      .catch((e: ApiError) => setError(e.message))
  }

  useEffect(load, [])

  async function act(id: number, action: 'publish' | 'close' | 'duplicate') {
    setBusyId(id)
    setError(null)
    try {
      await api.post<JobDetail>(`/api/jobs/${id}/${action}`, {})
      load()
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Action failed.')
    } finally {
      setBusyId(null)
    }
  }

  return (
    <>
      <PageHeading
        title="Postings"
        subtitle="Every posting for your organisation, including drafts."
      />

      {error && (
        <div className="mb-4">
          <ErrorNote>{error}</ErrorNote>
        </div>
      )}

      {jobs === null ? (
        <Loading />
      ) : jobs.length === 0 ? (
        <EmptyState title="No postings yet" detail="Create one to start receiving applicants." />
      ) : (
        <ul className="space-y-3">
          {jobs.map((job) => (
            <li key={job.id}>
              <Card>
                <div className="flex flex-wrap items-start justify-between gap-4">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <h3 className="font-medium text-ink-900">{job.title}</h3>
                      <Badge tone={statusTone(job.status)}>{jobStatusLabel[job.status]}</Badge>
                    </div>
                    <p className="mt-0.5 text-sm text-ink-500">
                      {job.city}, {job.country} &middot; {workModeLabel[job.workMode]} &middot;{' '}
                      {seniorityLabel[job.seniority]}
                    </p>
                    <p className="mt-1 text-xs text-ink-300">
                      {job.applicationCount} applicants &middot; closes {formatDate(job.closingDate)}
                    </p>
                  </div>

                  <div className="flex flex-wrap items-center gap-2">
                    <Link
                      to={`/recruiter/jobs/${job.id}/applicants`}
                      className="rounded-[6px] bg-accent px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-accent-hover"
                    >
                      Ranked applicants
                    </Link>

                    {job.status === 0 && (
                      <Button
                        size="sm"
                        variant="secondary"
                        disabled={busyId === job.id}
                        onClick={() => act(job.id, 'publish')}
                      >
                        Publish
                      </Button>
                    )}

                    {job.status === 1 && (
                      <Button
                        size="sm"
                        variant="secondary"
                        disabled={busyId === job.id}
                        onClick={() => act(job.id, 'close')}
                      >
                        Close
                      </Button>
                    )}

                    <Button
                      size="sm"
                      variant="secondary"
                      disabled={busyId === job.id}
                      onClick={() => act(job.id, 'duplicate')}
                    >
                      Duplicate
                    </Button>
                  </div>
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </>
  )
}
