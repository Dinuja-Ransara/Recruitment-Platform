/** Mirrors the enums in Meridian.Domain. Numeric, matching the API's JSON. */

export const WorkMode = { OnSite: 0, Hybrid: 1, Remote: 2 } as const
export const EmploymentType = { FullTime: 0, PartTime: 1, Contract: 2, Internship: 3 } as const
export const Seniority = { Intern: 0, Junior: 1, Mid: 2, Senior: 3, Lead: 4, Principal: 5 } as const
export const Education = { Unspecified: 0, Diploma: 1, Bachelors: 2, Masters: 3, Doctorate: 4 } as const
export const JobStatus = { Draft: 0, Published: 1, Paused: 2, Closed: 3 } as const

export const AppStatus = {
  Submitted: 0,
  UnderReview: 1,
  Shortlisted: 2,
  InterviewScheduled: 3,
  Interviewed: 4,
  OfferExtended: 5,
  Hired: 6,
  Rejected: 7,
  Withdrawn: 8,
} as const

export const workModeLabel = ['On site', 'Hybrid', 'Remote']
export const employmentLabel = ['Full time', 'Part time', 'Contract', 'Internship']
export const seniorityLabel = ['Intern', 'Junior', 'Mid', 'Senior', 'Lead', 'Principal']
export const educationLabel = ['Unspecified', 'Diploma', 'Bachelors', 'Masters', 'Doctorate']
export const jobStatusLabel = ['Draft', 'Published', 'Paused', 'Closed']

export const appStatusLabel = [
  'Submitted',
  'Under review',
  'Shortlisted',
  'Interview scheduled',
  'Interviewed',
  'Offer extended',
  'Hired',
  'Rejected',
  'Withdrawn',
]

/** Statuses recruiting staff may move an application into, by current status. */
export const allowedTransitions: Record<number, number[]> = {
  0: [1, 7],
  1: [2, 7],
  2: [3, 7],
  3: [4, 7],
  4: [5, 7],
  5: [6, 7],
  6: [],
  7: [],
  8: [],
}

export interface JobSummary {
  id: number
  title: string
  organizationName: string
  departmentName: string | null
  city: string
  country: string
  workMode: number
  employmentType: number
  seniority: number
  minYearsExperience: number
  salaryMin: number | null
  salaryMax: number | null
  currency: string
  status: number
  publishedAt: string | null
  closingDate: string | null
  applicationCount: number
  requiredSkills: string[]
}

export interface SkillRequirement {
  skillId: number
  skillName: string
  isMandatory: boolean
  weight: number
  minYearsExperience: number
}

export interface JobDetail extends JobSummary {
  description: string
  responsibilities: string
  requiredEducation: number
  rankingStrategy: number
  postedByName: string
  skillRequirements: SkillRequirement[]
}

export interface Paged<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ScoreFactor {
  name: string
  value: number
  weight: number
  contribution: number
  detail: string
}

export interface MatchExplanation {
  score: number
  strategy: string
  summary: string
  hasMandatoryGap: boolean
  factors: ScoreFactor[]
  matchedSkills: {
    skill: string
    isMandatory: boolean
    candidateYears: number
    requiredYears: number
    meetsExperienceBar: boolean
  }[]
  missingSkills: { skill: string; isMandatory: boolean; requiredYears: number }[]
}

export interface Applicant {
  applicationId: number
  candidateProfileId: number
  fullName: string
  headline: string
  city: string
  country: string
  yearsOfExperience: number
  highestEducation: number
  status: number
  submittedAt: string
  matchScore: number | null
  explanation: MatchExplanation | null
}

export interface ApplicationSummary {
  id: number
  jobPostingId: number
  jobTitle: string
  organizationName: string
  city: string
  country: string
  status: number
  submittedAt: string
  matchScore: number | null
  matchSummary: string | null
}

export interface ApplicationEvent {
  fromStatus: number | null
  toStatus: number
  note: string | null
  actorName: string | null
  occurredAt: string
}

export interface ApplicationDetail extends ApplicationSummary {
  coverLetter: string | null
  explanation: MatchExplanation | null
  timeline: ApplicationEvent[]
}

export interface JobRecommendation {
  jobPostingId: number
  title: string
  organizationName: string
  city: string
  country: string
  workMode: number
  alreadyApplied: boolean
  explanation: MatchExplanation
}

export interface Organization {
  id: number
  name: string
  industry: string
  city: string
  country: string
  departments: { id: number; name: string; costCentre: string | null }[]
}

export interface SystemHealth {
  generatedAtUtc: string
  counts: Record<string, number>
  recentSecurityEvents: {
    action: string
    entityName: string
    entityId: string | null
    ipAddress: string | null
    occurredAt: string
  }[]
}

export function formatSalary(job: Pick<JobSummary, 'salaryMin' | 'salaryMax' | 'currency'>) {
  if (job.salaryMin == null && job.salaryMax == null) return 'Not disclosed'
  const fmt = (n: number) => n.toLocaleString(undefined, { maximumFractionDigits: 0 })
  if (job.salaryMin != null && job.salaryMax != null) {
    return `${job.currency} ${fmt(job.salaryMin)} to ${fmt(job.salaryMax)}`
  }
  return `${job.currency} ${fmt((job.salaryMin ?? job.salaryMax)!)}`
}

export function formatDate(value: string | null) {
  if (!value) return '-'
  return new Date(value).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}
