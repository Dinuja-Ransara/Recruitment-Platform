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
import { Badge, Button, EmptyState, ErrorNote, Loading } from '../../components/ui'
import { RankBadge, ScoreDial } from '../../components/ScoreDial'
import { ExplanationPanel } from '../../components/ExplanationPanel'

type StageFilter = 'all' | 'new' | 'progressing' | 'closed'

function statusTone(status: number) {
  if (status === 6) return 'positive' as const
  if (status === 7 || status === 8) return 'danger' as const
  if (status >= 2) return 'accent' as const
  return 'neutral' as const
}

/**
 * The recruiter's screening surface, and the screen this product is judged on.
 *
 * Ranking is recomputed server-side on every load, because inverse document
 * frequency is measured against the applicants actually present. Each row can be
 * opened into the full breakdown, so a shortlist is defensible rather than a
 * number taken on trust.
 */
export function RankedApplicants() {
  const { id } = useParams()
  const [job, setJob] = useState<JobDetail | null>(null)
  const [applicants, setApplicants] = useState<Applicant[] | null>(null)
  const [expanded, setExpanded] = useState<number | null>(null)
  const [stageFilter, setStageFilter] = useState<StageFilter>('all')
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

  const all = applicants ?? []
  const matches = (a: Applicant, filter: StageFilter) =>
    filter === 'all' ||
    (filter === 'new' && a.status <= 1) ||
    (filter === 'progressing' && a.status >= 2 && a.status <= 5) ||
    (filter === 'closed' && a.status >= 6)

  const visible = all.filter((a) => matches(a, stageFilter))

  const filters: [StageFilter, string][] = [
    ['all', 'All'],
    ['new', 'Awaiting screening'],
    ['progressing', 'In progress'],
    ['closed', 'Closed'],
  ]

  const scored = all.filter((a) => a.matchScore != null)
  const best = scored.length ? Math.max(...scored.map((a) => a.matchScore!)) : 0
  const gaps = all.filter((a) => a.explanation?.hasMandatoryGap).length

  return (
    <>
      <PageHeading
        title={job ? job.title : 'Ranked applicants'}
        subtitle={
          job
            ? `${job.city}, ${job.country} · ranked by the ${job.skillRequirements.length ? 'matching engine' : 'matching engine'}, open any row to see why`
            : undefined
        }
      />

      {all.length > 0 && (
        <div className="mb-6 grid gap-3 sm:grid-cols-3">
          <Stat label="Applicants" value={String(all.length)} />
          <Stat label="Best match" value={best.toFixed(1)} accent />
          <Stat
            label="Missing a mandatory skill"
            value={String(gaps)}
            note={gaps > 0 ? 'capped, cannot be offset' : undefined}
          />
        </div>
      )}

      {error && (
        <div className="mb-4">
          <ErrorNote>{error}</ErrorNote>
        </div>
      )}

      {all.length > 0 && (
        <div className="mb-5 flex flex-wrap gap-1.5">
          {filters.map(([key, label]) => {
            const count = all.filter((a) => matches(a, key)).length
            return (
              <button
                key={key}
                onClick={() => setStageFilter(key)}
                className={`rounded-full border px-3.5 py-1.5 text-sm transition-colors ${
                  stageFilter === key
                    ? 'border-ink-900 bg-ink-900 font-medium text-white'
                    : 'border-line-strong bg-surface text-ink-500 hover:border-ink-400 hover:text-ink-900'
                }`}
              >
                {label}
                <span className="tabular ml-2 text-xs opacity-60">{count}</span>
              </button>
            )
          })}
        </div>
      )}

      {applicants === null ? (
        <Loading rows={4} />
      ) : all.length === 0 ? (
        <EmptyState title="No applicants yet" detail="Ranking appears once candidates apply." />
      ) : visible.length === 0 ? (
        <EmptyState title="Nothing at this stage" detail="Choose another filter above." />
      ) : (
        <ul className="stagger space-y-2">
          {visible.map((applicant) => {
            const isOpen = expanded === applicant.applicationId
            const next = allowedTransitions[applicant.status] ?? []
            const rank = all.findIndex((a) => a.applicationId === applicant.applicationId) + 1
            const capped = applicant.explanation?.hasMandatoryGap ?? false

            return (
              <li key={applicant.applicationId}>
                <article
                  className={`rounded-[6px] border bg-surface transition-all duration-200 ${
                    isOpen
                      ? 'border-accent-line shadow-[var(--shadow-lift)]'
                      : 'border-line shadow-[var(--shadow-card)] hover:border-line-strong'
                  }`}
                >
                  <div className="flex flex-wrap items-center gap-4 p-4 sm:flex-nowrap">
                    <RankBadge position={rank} />

                    <div className="min-w-0 flex-1">
                      <div className="flex flex-wrap items-center gap-2">
                        <h3 className="font-semibold text-ink-900">{applicant.fullName}</h3>
                        <Badge tone={statusTone(applicant.status)}>
                          {appStatusLabel[applicant.status]}
                        </Badge>
                        {capped && <Badge tone="warning">Mandatory gap</Badge>}
                      </div>
                      <p className="mt-0.5 truncate text-sm text-ink-500">{applicant.headline}</p>
                      <p className="mt-1 text-xs text-ink-400">
                        {applicant.city}, {applicant.country}
                        <span className="tabular mx-1.5">·</span>
                        <span className="tabular">{applicant.yearsOfExperience}</span> years
                        <span className="mx-1.5">·</span>
                        {educationLabel[applicant.highestEducation]}
                        <span className="mx-1.5">·</span>
                        applied {formatDate(applicant.submittedAt)}
                      </p>
                    </div>

                    <div className="flex items-center gap-3">
                      {applicant.matchScore != null && (
                        <ScoreDial score={applicant.matchScore} capped={capped} size="md" />
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

                  {applicant.explanation && !isOpen && (
                    <p className="border-t border-line px-4 py-2.5 text-sm text-ink-500">
                      {applicant.explanation.summary}
                    </p>
                  )}

                  {isOpen && applicant.explanation && (
                    <div className="animate-expand border-t border-line bg-surface-alt p-5">
                      <ExplanationPanel explanation={applicant.explanation} />
                    </div>
                  )}

                  {next.length > 0 && (
                    <div className="flex flex-wrap items-center gap-2 border-t border-line px-4 py-2.5">
                      <span className="text-xs text-ink-400">Move to</span>
                      {next.map((status) => (
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
                </article>
              </li>
            )
          })}
        </ul>
      )}
    </>
  )
}

function Stat({
  label,
  value,
  note,
  accent = false,
}: {
  label: string
  value: string
  note?: string
  accent?: boolean
}) {
  return (
    <div className="rounded-[6px] border border-line bg-surface px-4 py-3 shadow-[var(--shadow-card)]">
      <p className={`tabular text-2xl font-semibold ${accent ? 'text-accent' : 'text-ink-900'}`}>
        {value}
      </p>
      <p className="mt-0.5 text-xs font-medium text-ink-700">{label}</p>
      {note && <p className="text-xs text-ink-400">{note}</p>}
    </div>
  )
}
