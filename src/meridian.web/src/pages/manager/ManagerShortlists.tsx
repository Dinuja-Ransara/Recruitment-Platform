import { useEffect, useState } from 'react'
import { ApiError, api } from '../../lib/api'
import {
  allowedTransitions,
  appStatusLabel,
  educationLabel,
  type Applicant,
  type JobSummary,
  type Paged,
} from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Badge, Button, Card, EmptyState, ErrorNote, Loading } from '../../components/ui'
import { RankBadge, ScoreDial } from '../../components/ScoreDial'
import { ExplanationPanel } from '../../components/ExplanationPanel'

/**
 * The hiring manager's surface.
 *
 * A manager works from the shortlist rather than the raw pool: recruiters screen,
 * managers decide. Everything before Shortlisted is filtered out so the decision
 * is made on the candidates the recruiter has already stood behind.
 */
export function ManagerShortlists() {
  const [jobs, setJobs] = useState<JobSummary[] | null>(null)
  const [selectedJob, setSelectedJob] = useState<number | null>(null)
  const [applicants, setApplicants] = useState<Applicant[] | null>(null)
  const [expanded, setExpanded] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  useEffect(() => {
    api
      .get<Paged<JobSummary>>('/api/jobs/mine?pageSize=50')
      .then((r) => {
        setJobs(r.items)
        if (r.items.length > 0) setSelectedJob(r.items[0].id)
      })
      .catch((e: ApiError) => setError(e.message))
  }, [])

  function loadApplicants(jobId: number) {
    setApplicants(null)
    api
      .get<Applicant[]>(`/api/applications/job/${jobId}/ranked`)
      .then(setApplicants)
      .catch((e: ApiError) => setError(e.message))
  }

  useEffect(() => {
    if (selectedJob != null) loadApplicants(selectedJob)
  }, [selectedJob])

  async function decide(applicationId: number, status: number) {
    setBusyId(applicationId)
    setError(null)
    try {
      await api.post(`/api/applications/${applicationId}/status`, { status, note: 'Hiring manager decision.' })
      if (selectedJob != null) loadApplicants(selectedJob)
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Could not record the decision.')
    } finally {
      setBusyId(null)
    }
  }

  // Shortlisted and beyond. Managers do not screen raw applications.
  const shortlisted = applicants?.filter((a) => a.status >= 2 && a.status <= 5) ?? []

  return (
    <>
      <PageHeading
        title="Shortlists"
        subtitle="Candidates a recruiter has advanced, with the reasoning behind each score."
      />

      {error && (
        <div className="mb-4">
          <ErrorNote>{error}</ErrorNote>
        </div>
      )}

      {jobs === null ? (
        <Loading />
      ) : jobs.length === 0 ? (
        <EmptyState title="No postings for your organisation" />
      ) : (
        <>
          <div className="mb-6 flex flex-wrap gap-2">
            {jobs.map((job) => (
              <button
                key={job.id}
                onClick={() => setSelectedJob(job.id)}
                className={`rounded-[6px] border px-3 py-1.5 text-sm transition-colors ${
                  selectedJob === job.id
                    ? 'border-accent bg-accent-soft font-medium text-accent'
                    : 'border-line-strong text-ink-700 hover:border-accent'
                }`}
              >
                {job.title}
                <span className="tabular ml-2 text-xs text-ink-300">{job.applicationCount}</span>
              </button>
            ))}
          </div>

          {applicants === null ? (
            <Loading />
          ) : shortlisted.length === 0 ? (
            <EmptyState
              title="Nothing shortlisted yet"
              detail="Candidates appear here once a recruiter advances them past screening."
            />
          ) : (
            <ul className="stagger space-y-3">
              {shortlisted.map((applicant, position) => {
                const isOpen = expanded === applicant.applicationId
                const next = allowedTransitions[applicant.status] ?? []

                return (
                  <li key={applicant.applicationId}>
                    <Card>
                      <div className="flex flex-wrap items-start justify-between gap-4">
                        <div className="flex min-w-0 gap-4">
                          <RankBadge position={position + 1} />
                          <div className="min-w-0">
                          <div className="flex flex-wrap items-center gap-2">
                            <h3 className="font-semibold text-ink-900">{applicant.fullName}</h3>
                            <Badge tone="accent">{appStatusLabel[applicant.status]}</Badge>
                            {applicant.explanation?.hasMandatoryGap && (
                              <Badge tone="warning">Mandatory gap</Badge>
                            )}
                          </div>
                          <p className="mt-0.5 text-sm text-ink-500">{applicant.headline}</p>
                          <p className="mt-1 text-xs text-ink-400">
                            {applicant.city}, {applicant.country} &middot;{' '}
                            <span className="tabular">{applicant.yearsOfExperience}</span> years
                            &middot; {educationLabel[applicant.highestEducation]}
                          </p>
                          </div>
                        </div>

                        <div className="flex items-center gap-3">
                          {applicant.matchScore != null && (
                            <ScoreDial
                              score={applicant.matchScore}
                              capped={applicant.explanation?.hasMandatoryGap}
                            />
                          )}
                          <Button
                            size="sm"
                            variant={isOpen ? 'secondary' : 'ghost'}
                            onClick={() => setExpanded(isOpen ? null : applicant.applicationId)}
                          >
                            {isOpen ? 'Hide' : 'Why this score'}
                          </Button>
                        </div>
                      </div>

                      {next.length > 0 && (
                        <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-line pt-4">
                          <span className="text-xs text-ink-500">Decision:</span>
                          {next.map((status) => (
                            <Button
                              key={status}
                              size="sm"
                              variant={status === 7 ? 'danger' : status === 6 ? 'primary' : 'secondary'}
                              disabled={busyId === applicant.applicationId}
                              onClick={() => decide(applicant.applicationId, status)}
                            >
                              {appStatusLabel[status]}
                            </Button>
                          ))}
                        </div>
                      )}

                      {isOpen && applicant.explanation && (
                        <div className="mt-5 border-t border-line pt-5">
                          <ExplanationPanel explanation={applicant.explanation} />
                        </div>
                      )}
                    </Card>
                  </li>
                )
              })}
            </ul>
          )}
        </>
      )}
    </>
  )
}
