import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { ApiError, api } from '../../lib/api'
import {
  allowedTransitions,
  appStatusLabel,
  educationLabel,
  formatDate,
  type Applicant,
  type JobDetail,
} from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Badge, Button, Card, EmptyState, ErrorNote, Loading, ScoreBar } from '../../components/ui'
import { ExplanationPanel } from '../../components/ExplanationPanel'

/**
 * The recruiter's screening surface.
 *
 * Ranking is recomputed server-side on every load, because inverse document
 * frequency is measured against the applicants actually present. Each row can be
 * expanded into the full breakdown, so a shortlist is defensible rather than
 * being a number the recruiter has to take on trust.
 */
export function RankedApplicants() {
  const { id } = useParams()
  const [job, setJob] = useState<JobDetail | null>(null)
  const [applicants, setApplicants] = useState<Applicant[] | null>(null)
  const [expanded, setExpanded] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  function load() {
    api.get<JobDetail>(`/api/jobs/${id}`).then(setJob).catch(() => undefined)
    api
      .get<Applicant[]>(`/api/applications/job/${id}/ranked`)
      .then(setApplicants)
      .catch((e: ApiError) => setError(e.message))
  }

  useEffect(load, [id])

  async function changeStatus(applicationId: number, status: number) {
    setBusyId(applicationId)
    setError(null)
    try {
      await api.post(`/api/applications/${applicationId}/status`, { status, note: null })
      load()
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Could not update the application.')
    } finally {
      setBusyId(null)
    }
  }

  return (
    <>
      <PageHeading
        title={job ? `Applicants: ${job.title}` : 'Ranked applicants'}
        subtitle="Ranked by the matching engine. Open any row to see how the score was reached."
      />

      {error && (
        <div className="mb-4">
          <ErrorNote>{error}</ErrorNote>
        </div>
      )}

      {applicants === null ? (
        <Loading />
      ) : applicants.length === 0 ? (
        <EmptyState title="No applicants yet" detail="Ranking appears once candidates apply." />
      ) : (
        <ul className="space-y-3">
          {applicants.map((applicant, index) => {
            const isOpen = expanded === applicant.applicationId
            const nextStatuses = allowedTransitions[applicant.status] ?? []

            return (
              <li key={applicant.applicationId}>
                <Card>
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div className="flex min-w-0 gap-4">
                      <span className="tabular mt-0.5 text-lg font-semibold text-ink-300">
                        {index + 1}
                      </span>
                      <div className="min-w-0">
                        <div className="flex flex-wrap items-center gap-2">
                          <h3 className="font-medium text-ink-900">{applicant.fullName}</h3>
                          <Badge>{appStatusLabel[applicant.status]}</Badge>
                          {applicant.explanation?.hasMandatoryGap && (
                            <Badge tone="warning">Mandatory gap</Badge>
                          )}
                        </div>
                        <p className="mt-0.5 text-sm text-ink-500">{applicant.headline}</p>
                        <p className="mt-1 text-xs text-ink-300">
                          {applicant.city}, {applicant.country} &middot;{' '}
                          {applicant.yearsOfExperience} years &middot;{' '}
                          {educationLabel[applicant.highestEducation]} &middot; applied{' '}
                          {formatDate(applicant.submittedAt)}
                        </p>
                      </div>
                    </div>

                    <div className="flex flex-col items-end gap-2">
                      {applicant.matchScore != null && (
                        <ScoreBar
                          score={applicant.matchScore}
                          capped={applicant.explanation?.hasMandatoryGap}
                        />
                      )}
                      <Button
                        size="sm"
                        variant="secondary"
                        onClick={() => setExpanded(isOpen ? null : applicant.applicationId)}
                      >
                        {isOpen ? 'Hide breakdown' : 'Why this score'}
                      </Button>
                    </div>
                  </div>

                  {applicant.explanation && (
                    <p className="mt-3 text-sm text-ink-700">{applicant.explanation.summary}</p>
                  )}

                  {nextStatuses.length > 0 && (
                    <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-line pt-4">
                      <span className="text-xs text-ink-500">Move to:</span>
                      {nextStatuses.map((status) => (
                        <Button
                          key={status}
                          size="sm"
                          variant={status === 7 ? 'danger' : 'secondary'}
                          disabled={busyId === applicant.applicationId}
                          onClick={() => changeStatus(applicant.applicationId, status)}
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
  )
}
