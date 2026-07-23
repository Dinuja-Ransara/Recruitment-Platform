import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { ApiError, api } from '../../lib/api'
import type { JobDetail, Organization } from '../../lib/types'
import { PageHeading } from '../../components/AppShell'
import { Button, Card, ErrorNote, Field, inputClass } from '../../components/ui'

interface SkillRow {
  skillId: number
  isMandatory: boolean
  weight: number
  minYearsExperience: number
}

interface SkillOption {
  id: number
  name: string
  category: string
}

export function JobForm() {
  const navigate = useNavigate()

  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [skills, setSkills] = useState<SkillOption[]>([])
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const [form, setForm] = useState({
    title: '',
    description: '',
    responsibilities: '',
    organizationId: 0,
    departmentId: '' as string | number,
    city: '',
    country: 'Sri Lanka',
    workMode: 1,
    employmentType: 0,
    seniority: 3,
    minYearsExperience: 3,
    requiredEducation: 2,
    salaryMin: '' as string | number,
    salaryMax: '' as string | number,
    currency: 'LKR',
    closingDate: '',
    rankingStrategy: 2,
  })

  const [skillRows, setSkillRows] = useState<SkillRow[]>([])

  useEffect(() => {
    api.get<Organization[]>('/api/organizations').then((orgs) => {
      setOrganizations(orgs)
      if (orgs.length > 0) setForm((f) => ({ ...f, organizationId: orgs[0].id }))
    })
    api.get<SkillOption[]>('/api/skills').then(setSkills).catch(() => setSkills([]))
  }, [])

  const departments = organizations.find((o) => o.id === Number(form.organizationId))?.departments ?? []

  function addSkill() {
    const used = new Set(skillRows.map((r) => r.skillId))
    const next = skills.find((s) => !used.has(s.id))
    if (next) {
      setSkillRows([...skillRows, { skillId: next.id, isMandatory: false, weight: 3, minYearsExperience: 0 }])
    }
  }

  async function submit(event: FormEvent) {
    event.preventDefault()
    setSaving(true)
    setError(null)

    try {
      const created = await api.post<JobDetail>('/api/jobs', {
        ...form,
        organizationId: Number(form.organizationId),
        departmentId: form.departmentId === '' ? null : Number(form.departmentId),
        salaryMin: form.salaryMin === '' ? null : Number(form.salaryMin),
        salaryMax: form.salaryMax === '' ? null : Number(form.salaryMax),
        closingDate: form.closingDate === '' ? null : new Date(form.closingDate).toISOString(),
        skills: skillRows,
      })
      navigate(`/recruiter/jobs/${created.id}/applicants`)
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'Could not create the posting.')
    } finally {
      setSaving(false)
    }
  }

  function set<K extends keyof typeof form>(key: K, value: (typeof form)[K]) {
    setForm((f) => ({ ...f, [key]: value }))
  }

  return (
    <>
      <PageHeading
        title="New posting"
        subtitle="Created as a draft. Publish it when you are ready for applicants."
      />

      <form onSubmit={submit} className="space-y-6">
        <Card title="Role">
          <div className="space-y-4">
            <Field label="Title">
              <input
                required
                value={form.title}
                onChange={(e) => set('title', e.target.value)}
                className={inputClass}
              />
            </Field>

            <Field label="Description">
              <textarea
                required
                rows={5}
                value={form.description}
                onChange={(e) => set('description', e.target.value)}
                className={inputClass}
              />
            </Field>

            <Field
              label="Responsibilities"
              hint="Both fields feed the matching engine's text comparison, so specific language helps."
            >
              <textarea
                rows={3}
                value={form.responsibilities}
                onChange={(e) => set('responsibilities', e.target.value)}
                className={inputClass}
              />
            </Field>
          </div>
        </Card>

        <Card title="Placement">
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Organisation">
              <select
                value={form.organizationId}
                onChange={(e) => set('organizationId', Number(e.target.value))}
                className={inputClass}
              >
                {organizations.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </Field>

            <Field label="Department">
              <select
                value={form.departmentId}
                onChange={(e) => set('departmentId', e.target.value)}
                className={inputClass}
              >
                <option value="">None</option>
                {departments.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            </Field>

            <Field label="City">
              <input value={form.city} onChange={(e) => set('city', e.target.value)} className={inputClass} />
            </Field>

            <Field label="Country">
              <input
                value={form.country}
                onChange={(e) => set('country', e.target.value)}
                className={inputClass}
              />
            </Field>

            <Field label="Work mode">
              <select
                value={form.workMode}
                onChange={(e) => set('workMode', Number(e.target.value))}
                className={inputClass}
              >
                <option value={0}>On site</option>
                <option value={1}>Hybrid</option>
                <option value={2}>Remote</option>
              </select>
            </Field>

            <Field label="Employment type">
              <select
                value={form.employmentType}
                onChange={(e) => set('employmentType', Number(e.target.value))}
                className={inputClass}
              >
                <option value={0}>Full time</option>
                <option value={1}>Part time</option>
                <option value={2}>Contract</option>
                <option value={3}>Internship</option>
              </select>
            </Field>
          </div>
        </Card>

        <Card title="Requirements and reward">
          <div className="grid gap-4 sm:grid-cols-3">
            <Field label="Seniority">
              <select
                value={form.seniority}
                onChange={(e) => set('seniority', Number(e.target.value))}
                className={inputClass}
              >
                {['Intern', 'Junior', 'Mid', 'Senior', 'Lead', 'Principal'].map((label, i) => (
                  <option key={label} value={i}>
                    {label}
                  </option>
                ))}
              </select>
            </Field>

            <Field label="Minimum years">
              <input
                type="number"
                min={0}
                max={50}
                value={form.minYearsExperience}
                onChange={(e) => set('minYearsExperience', Number(e.target.value))}
                className={inputClass}
              />
            </Field>

            <Field label="Education">
              <select
                value={form.requiredEducation}
                onChange={(e) => set('requiredEducation', Number(e.target.value))}
                className={inputClass}
              >
                {['Unspecified', 'Diploma', 'Bachelors', 'Masters', 'Doctorate'].map((label, i) => (
                  <option key={label} value={i}>
                    {label}
                  </option>
                ))}
              </select>
            </Field>

            <Field label="Salary minimum">
              <input
                type="number"
                value={form.salaryMin}
                onChange={(e) => set('salaryMin', e.target.value)}
                className={inputClass}
              />
            </Field>

            <Field label="Salary maximum">
              <input
                type="number"
                value={form.salaryMax}
                onChange={(e) => set('salaryMax', e.target.value)}
                className={inputClass}
              />
            </Field>

            <Field label="Currency">
              <input
                maxLength={3}
                value={form.currency}
                onChange={(e) => set('currency', e.target.value.toUpperCase())}
                className={inputClass}
              />
            </Field>

            <Field label="Closing date">
              <input
                type="date"
                value={form.closingDate}
                onChange={(e) => set('closingDate', e.target.value)}
                className={inputClass}
              />
            </Field>

            <Field
              label="Ranking strategy"
              hint="Decides how applicants to this posting are scored."
            >
              <select
                value={form.rankingStrategy}
                onChange={(e) => set('rankingStrategy', Number(e.target.value))}
                className={inputClass}
              >
                <option value={0}>Skill weighted</option>
                <option value={1}>Resume similarity</option>
                <option value={2}>Hybrid</option>
                <option value={3}>Experience first</option>
              </select>
            </Field>
          </div>
        </Card>

        <Card
          title="Required skills"
          action={
            <Button size="sm" variant="secondary" onClick={addSkill}>
              Add skill
            </Button>
          }
        >
          {skillRows.length === 0 ? (
            <p className="text-sm text-ink-500">
              No skills added. A posting with no skill requirements is scored on text, experience and
              location alone.
            </p>
          ) : (
            <div className="space-y-3">
              {skillRows.map((row, index) => (
                <div key={index} className="grid gap-3 sm:grid-cols-[2fr_1fr_1fr_auto] sm:items-end">
                  <Field label="Skill">
                    <select
                      value={row.skillId}
                      onChange={(e) => {
                        const next = [...skillRows]
                        next[index] = { ...row, skillId: Number(e.target.value) }
                        setSkillRows(next)
                      }}
                      className={inputClass}
                    >
                      {skills.map((s) => (
                        <option key={s.id} value={s.id}>
                          {s.name}
                        </option>
                      ))}
                    </select>
                  </Field>

                  <Field label="Weight 1-5">
                    <input
                      type="number"
                      min={1}
                      max={5}
                      value={row.weight}
                      onChange={(e) => {
                        const next = [...skillRows]
                        next[index] = { ...row, weight: Number(e.target.value) }
                        setSkillRows(next)
                      }}
                      className={inputClass}
                    />
                  </Field>

                  <Field label="Min years">
                    <input
                      type="number"
                      min={0}
                      value={row.minYearsExperience}
                      onChange={(e) => {
                        const next = [...skillRows]
                        next[index] = { ...row, minYearsExperience: Number(e.target.value) }
                        setSkillRows(next)
                      }}
                      className={inputClass}
                    />
                  </Field>

                  <div className="flex items-center gap-3 pb-2">
                    <label className="flex items-center gap-1.5 text-sm text-ink-700">
                      <input
                        type="checkbox"
                        checked={row.isMandatory}
                        onChange={(e) => {
                          const next = [...skillRows]
                          next[index] = { ...row, isMandatory: e.target.checked }
                          setSkillRows(next)
                        }}
                      />
                      Mandatory
                    </label>
                    <button
                      type="button"
                      onClick={() => setSkillRows(skillRows.filter((_, i) => i !== index))}
                      className="text-sm text-danger hover:underline"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        {error && <ErrorNote>{error}</ErrorNote>}

        <div className="flex gap-3">
          <Button type="submit" disabled={saving}>
            {saving ? 'Creating...' : 'Create draft'}
          </Button>
          <Button variant="secondary" onClick={() => navigate('/recruiter/jobs')}>
            Cancel
          </Button>
        </div>
      </form>
    </>
  )
}
