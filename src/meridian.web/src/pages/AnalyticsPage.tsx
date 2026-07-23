import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import { PageHeading } from '../components/AppShell'
import { Card, CountUp, EmptyState, Loading } from '../components/ui'

interface FunnelStage {
  status: number
  label: string
  count: number
  percentageOfTotal: number
}

interface PostingPerformance {
  jobPostingId: number
  title: string
  city: string
  applications: number
  shortlisted: number
  averageScore: number
  topScore: number
}

interface Analytics {
  publishedPostings: number
  draftPostings: number
  totalApplications: number
  activeApplications: number
  averageMatchScore: number
  mandatoryGapRejectionRate: number
  funnel: FunnelStage[]
  postingPerformance: PostingPerformance[]
  byLocation: { location: string; postings: number; applications: number }[]
  mostRequestedSkills: { skill: string; postingsRequiring: number; mandatoryIn: number }[]
}

export function AnalyticsPage() {
  const [data, setData] = useState<Analytics | null>(null)

  useEffect(() => {
    api.get<Analytics>('/api/analytics/recruitment').then(setData)
  }, [])

  if (!data) {
    return (
      <>
        <PageHeading title="Recruitment analytics" />
        <Loading />
      </>
    )
  }

  const funnelTop = data.funnel[0]?.count ?? 0

  return (
    <>
      <PageHeading
        title="Recruitment analytics"
        subtitle="Every figure is derived from the application pipeline rather than stored, so it cannot disagree with the lists beneath it."
      />

      <div className="stagger mb-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <Metric label="Published postings" value={data.publishedPostings} note={`${data.draftPostings} draft`} />
        <Metric label="Applications" value={data.totalApplications} note={`${data.activeApplications} still active`} />
        <Metric label="Average match" value={data.averageMatchScore} note="across scored applications" />
        <Metric
          label="Rejections for a mandatory gap"
          value={`${data.mandatoryGapRejectionRate}%`}
          note="of all rejections"
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Card title="Hiring funnel">
            {funnelTop === 0 ? (
              <EmptyState title="No applications yet" />
            ) : (
              <>
                <ul className="space-y-3">
                  {data.funnel.map((stage, index) => {
                    const width = funnelTop === 0 ? 0 : (stage.count / funnelTop) * 100
                    const previous = index === 0 ? null : data.funnel[index - 1]
                    const dropOff =
                      previous && previous.count > 0
                        ? Math.round(((previous.count - stage.count) / previous.count) * 100)
                        : null

                    return (
                      <li key={stage.label}>
                        <div className="flex items-baseline justify-between text-sm">
                          <span className="font-medium text-ink-900">{stage.label}</span>
                          <span className="tabular text-ink-500">
                            {stage.count}
                            <span className="ml-2 text-xs text-ink-300">{stage.percentageOfTotal}%</span>
                          </span>
                        </div>
                        <div className="mt-1 h-6 overflow-hidden rounded-[4px] bg-surface-alt">
                          <div
                            className="h-full bg-accent/80 transition-[width] duration-500"
                            style={{ width: `${width}%` }}
                          />
                        </div>
                        {dropOff !== null && dropOff > 0 && (
                          <p className="mt-1 text-xs text-ink-300">
                            {dropOff}% did not progress from {previous!.label.toLowerCase()}
                          </p>
                        )}
                      </li>
                    )
                  })}
                </ul>
                <p className="mt-4 border-t border-line pt-3 text-xs text-ink-500">
                  An application counts towards every stage it reached, taken from its timeline rather
                  than its current status, so a candidate rejected after interview still counts as
                  having been screened and shortlisted.
                </p>
              </>
            )}
          </Card>
        </div>

        <div className="space-y-6">
          <Card title="Most requested skills">
            {data.mostRequestedSkills.length === 0 ? (
              <EmptyState title="No skill requirements yet" />
            ) : (
              <ul className="space-y-2.5">
                {data.mostRequestedSkills.map((skill) => (
                  <li key={skill.skill} className="flex items-center justify-between gap-3 text-sm">
                    <span className="truncate text-ink-900">{skill.skill}</span>
                    <span className="tabular shrink-0 text-xs text-ink-500">
                      {skill.postingsRequiring} postings
                      {skill.mandatoryIn > 0 && (
                        <span className="ml-1 text-warning">{skill.mandatoryIn} mandatory</span>
                      )}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </Card>

          <Card title="By location">
            <ul className="space-y-2.5">
              {data.byLocation.map((row) => (
                <li key={row.location} className="flex items-center justify-between gap-3 text-sm">
                  <span className="truncate text-ink-900">{row.location}</span>
                  <span className="tabular shrink-0 text-xs text-ink-500">
                    {row.postings} postings, {row.applications} applied
                  </span>
                </li>
              ))}
            </ul>
          </Card>
        </div>
      </div>

      <div className="mt-6">
        <Card title="Posting performance">
          {data.postingPerformance.length === 0 ? (
            <EmptyState title="No published postings" />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[620px] text-left text-sm">
                <thead>
                  <tr className="border-b border-line text-xs tracking-wide text-ink-500 uppercase">
                    <th className="py-2 pr-4 font-medium">Posting</th>
                    <th className="py-2 pr-4 font-medium">Location</th>
                    <th className="py-2 pr-4 text-right font-medium">Applied</th>
                    <th className="py-2 pr-4 text-right font-medium">Shortlisted</th>
                    <th className="py-2 pr-4 text-right font-medium">Average</th>
                    <th className="py-2 text-right font-medium">Best</th>
                  </tr>
                </thead>
                <tbody>
                  {data.postingPerformance.map((posting) => (
                    <tr key={posting.jobPostingId} className="border-b border-line last:border-0">
                      <td className="py-2.5 pr-4">
                        <Link
                          to={`/recruiter/jobs/${posting.jobPostingId}/applicants`}
                          className="font-medium text-ink-900 hover:text-accent"
                        >
                          {posting.title}
                        </Link>
                      </td>
                      <td className="py-2.5 pr-4 text-ink-500">{posting.city}</td>
                      <td className="tabular py-2.5 pr-4 text-right text-ink-700">{posting.applications}</td>
                      <td className="tabular py-2.5 pr-4 text-right text-ink-700">{posting.shortlisted}</td>
                      <td className="tabular py-2.5 pr-4 text-right text-ink-700">{posting.averageScore}</td>
                      <td className="tabular py-2.5 text-right font-medium text-ink-900">{posting.topScore}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      </div>
    </>
  )
}

function Metric({ label, value, note }: { label: string; value: number | string; note?: string }) {
  return (
    <div className="rounded-[6px] border border-line bg-surface px-4 py-3">
      <p className="tabular text-2xl font-semibold text-ink-900">
        {typeof value === 'number' ? <CountUp value={value} decimals={value % 1 === 0 ? 0 : 1} /> : value}
      </p>
      <p className="mt-0.5 text-xs font-medium text-ink-700">{label}</p>
      {note && <p className="text-xs text-ink-300">{note}</p>}
    </div>
  )
}
