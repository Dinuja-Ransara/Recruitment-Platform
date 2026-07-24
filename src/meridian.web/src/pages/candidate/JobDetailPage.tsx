import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ApiError, api } from '../../lib/api'
import {
  educationLabel,
  employmentLabel,
  formatDate,
  formatSalary,
  seniorityLabel,
  workModeLabel,
  type ApplicationDetail,
  type JobDetail,
} from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Badge, Button, Card, ErrorNote, Loading, inputClass } from '../../components/ui'

export function JobDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [job, setJob] = useState<JobDetail | null>(null)
  const [coverLetter, setCoverLetter] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    api
      .get<JobDetail>(`/api/jobs/${id}`)
      .then(setJob)
      .catch((e: ApiError) => setError(e.message))
  }, [id])

  async function apply() {
    setSubmitting(true)
    setError(null)
    try {
      const created = await api.post<ApplicationDetail>('/api/applications', {
        jobPostingId: Number(id),
        coverLetter: coverLetter.trim() || null,
      })
      navigate(`/candidate/applications/${created.id}`)
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Could not submit the application.')
    } finally {
      setSubmitting(false)
    }
  }

  if (error && !job) return <ErrorNote>{error}</ErrorNote>
  if (!job) return <Loading />

  return (
    <>
      <PageHeading title={job.title} subtitle={`${job.organizationName} · ${job.city}, ${job.country}`} />

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Card title="About the role">
            <p className="text-sm leading-relaxed whitespace-pre-line text-ink-700">
              {job.description}
            </p>
            {job.responsibilities && (
              <>
                <h3 className="mt-5 text-xs font-semibold tracking-wide text-ink-500 uppercase">
                  Responsibilities
                </h3>
                <p className="mt-2 text-sm leading-relaxed whitespace-pre-line text-ink-700">
                  {job.responsibilities}
                </p>
              </>
            )}
          </Card>

          <Card title="What this posting asks for">
            {job.skillRequirements.length === 0 ? (
              <p className="text-sm text-ink-500">No specific skills listed.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[420px] text-left text-sm">
                  <thead>
                    <tr className="border-b border-line text-xs tracking-wide text-ink-500 uppercase">
                      <th className="py-2 pr-4 font-medium">Skill</th>
                      <th className="py-2 pr-4 font-medium">Requirement</th>
                      <th className="py-2 pr-4 text-right font-medium">Min years</th>
                      <th className="py-2 text-right font-medium">Weight</th>
                    </tr>
                  </thead>
                  <tbody>
                    {job.skillRequirements.map((skill) => (
                      <tr key={skill.skillId} className="border-b border-line last:border-0">
                        <td className="py-2 pr-4 font-medium text-ink-900">{skill.skillName}</td>
                        <td className="py-2 pr-4">
                          {skill.isMandatory ? (
                            <Badge tone="danger">Mandatory</Badge>
                          ) : (
                            <Badge>Preferred</Badge>
                          )}
                        </td>
                        <td className="tabular py-2 pr-4 text-right text-ink-700">
                          {skill.minYearsExperience}
                        </td>
                        <td className="tabular py-2 text-right text-ink-500">{skill.weight}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </div>

        <div className="space-y-6">
          <Card title="Details">
            <dl className="space-y-3 text-sm">
              <Row label="Salary" value={formatSalary(job)} />
              <Row label="Work mode" value={workModeLabel[job.workMode]} />
              <Row label="Employment" value={employmentLabel[job.employmentType]} />
              <Row label="Seniority" value={seniorityLabel[job.seniority]} />
              <Row label="Experience" value={`${job.minYearsExperience}+ years`} />
              <Row label="Education" value={educationLabel[job.requiredEducation]} />
              <Row label="Closes" value={formatDate(job.closingDate)} />
              <Row label="Applicants" value={String(job.applicationCount)} />
            </dl>
          </Card>

          <Card title="Apply">
            <label className="block">
              <span className="block text-sm font-medium text-ink-700">Cover letter</span>
              <textarea
                rows={5}
                value={coverLetter}
                onChange={(e) => setCoverLetter(e.target.value)}
                placeholder="Optional. Why you are a fit for this role."
                className={inputClass}
              />
            </label>

            {error && (
              <div className="mt-3">
                <ErrorNote>{error}</ErrorNote>
              </div>
            )}

            <div className="mt-4">
              <Button onClick={apply} disabled={submitting}>
                {submitting ? 'Submitting...' : 'Submit application'}
              </Button>
            </div>

            <p className="mt-3 text-xs text-ink-500">
              Your CV is scored against this posting on submission, and you will see the full
              breakdown immediately.
            </p>
          </Card>
        </div>
      </div>
    </>
  )
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-4">
      <dt className="text-ink-500">{label}</dt>
      <dd className="text-right font-medium text-ink-900">{value}</dd>
    </div>
  )
}
