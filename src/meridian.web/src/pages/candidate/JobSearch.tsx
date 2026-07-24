import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../../lib/api'
import {
  employmentLabel,
  formatDate,
  formatSalary,
  seniorityLabel,
  workModeLabel,
  type JobSummary,
  type Paged,
} from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Badge, Card, EmptyState, Loading, inputClass } from '../../components/ui'

export function JobSearch() {
  const [keyword, setKeyword] = useState('')
  const [country, setCountry] = useState('')
  const [workMode, setWorkMode] = useState('')
  const [results, setResults] = useState<Paged<JobSummary> | null>(null)

  useEffect(() => {
    // Debounced so typing does not fire a request per keystroke.
    const timer = setTimeout(() => {
      const params = new URLSearchParams()
      if (keyword.trim()) params.set('keyword', keyword.trim())
      if (country) params.set('country', country)
      if (workMode) params.set('workMode', workMode)
      params.set('pageSize', '20')

      setResults(null)
      api.get<Paged<JobSummary>>(`/api/jobs?${params}`).then(setResults)
    }, 300)

    return () => clearTimeout(timer)
  }, [keyword, country, workMode])

  return (
    <>
      <PageHeading title="Find jobs" subtitle="Open postings across every client organisation." />

      <Card className="mb-6">
        <div className="grid gap-4 sm:grid-cols-3">
          <label className="block">
            <span className="block text-sm font-medium text-ink-700">Keyword</span>
            <input
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="Title or description"
              className={inputClass}
            />
          </label>
          <label className="block">
            <span className="block text-sm font-medium text-ink-700">Country</span>
            <select value={country} onChange={(e) => setCountry(e.target.value)} className={inputClass}>
              <option value="">Any</option>
              <option>Sri Lanka</option>
              <option>Singapore</option>
              <option>United Kingdom</option>
            </select>
          </label>
          <label className="block">
            <span className="block text-sm font-medium text-ink-700">Work mode</span>
            <select value={workMode} onChange={(e) => setWorkMode(e.target.value)} className={inputClass}>
              <option value="">Any</option>
              <option value="0">On site</option>
              <option value="1">Hybrid</option>
              <option value="2">Remote</option>
            </select>
          </label>
        </div>
      </Card>

      {results === null ? (
        <Loading />
      ) : results.items.length === 0 ? (
        <EmptyState title="No postings match those filters" detail="Try widening the search." />
      ) : (
        <>
          <p className="mb-3 text-sm text-ink-500">
            {results.totalCount} {results.totalCount === 1 ? 'posting' : 'postings'}
          </p>
          <ul className="space-y-3">
            {results.items.map((job) => (
              <li key={job.id}>
                <Link
                  to={`/candidate/jobs/${job.id}`}
                  className="block rounded-[6px] border border-line bg-surface p-5 transition-colors hover:border-accent"
                >
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <h3 className="font-medium text-ink-900">{job.title}</h3>
                      <p className="mt-0.5 text-sm text-ink-500">
                        {job.organizationName}
                        {job.departmentName && ` · ${job.departmentName}`}
                      </p>
                    </div>
                    <span className="text-sm text-ink-700">{formatSalary(job)}</span>
                  </div>

                  <div className="mt-3 flex flex-wrap gap-1.5">
                    <Badge>{`${job.city}, ${job.country}`}</Badge>
                    <Badge>{workModeLabel[job.workMode]}</Badge>
                    <Badge>{employmentLabel[job.employmentType]}</Badge>
                    <Badge>{seniorityLabel[job.seniority]}</Badge>
                    <Badge>{`${job.minYearsExperience}+ years`}</Badge>
                  </div>

                  {job.requiredSkills.length > 0 && (
                    <p className="mt-3 text-xs text-ink-500">
                      Skills: {job.requiredSkills.join(', ')}
                    </p>
                  )}

                  <p className="mt-2 text-xs text-ink-300">
                    {job.applicationCount} applied &middot; closes {formatDate(job.closingDate)}
                  </p>
                </Link>
              </li>
            ))}
          </ul>
        </>
      )}
    </>
  )
}
