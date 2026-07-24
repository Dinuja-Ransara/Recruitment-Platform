import { Link } from 'react-router-dom'
import { Brand } from '../components/Brand'

/**
 * The public landing page.
 *
 * Deliberately not a feature grid of icon cards. The one thing that separates
 * this platform from every other recruitment tool is that its scoring can
 * explain itself, so the page leads with an actual score breakdown rather than
 * describing one.
 */
export function HomePage() {
  return (
    <div className="min-h-full bg-surface">
      <header className="border-b border-line">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <Brand size="sm" />
          <div className="flex items-center gap-3">
            <a href="#how" className="hidden text-sm text-ink-700 hover:text-accent sm:block">
              How it works
            </a>
            <a href="#roles" className="hidden text-sm text-ink-700 hover:text-accent sm:block">
              For teams
            </a>
            <Link
              to="/login"
              className="rounded-[6px] bg-accent px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-accent-hover"
            >
              Sign in
            </Link>
          </div>
        </div>
      </header>

      {/* Hero */}
      <section className="border-b border-line bg-ink-900">
        <div className="mx-auto grid max-w-6xl gap-10 px-6 py-16 lg:grid-cols-2 lg:py-24">
          <div className="flex flex-col justify-center">
            <p className="text-xs font-semibold tracking-[0.2em] text-accent-soft uppercase">
              Recruitment and talent management
            </p>
            <h1 className="mt-4 text-4xl leading-[1.1] font-semibold text-white sm:text-5xl">
              Screening that shows its working.
            </h1>
            <p className="mt-5 max-w-lg text-base leading-relaxed text-ink-300">
              Meridian ranks applicants against a role and then tells you exactly why:
              which skills were evidenced, which were missing, and what each factor
              contributed to the score. No black box, no API key, no guesswork.
            </p>

            <div className="mt-8 flex flex-wrap gap-3">
              <Link
                to="/login"
                className="rounded-[6px] bg-accent px-5 py-2.5 text-sm font-medium text-white transition-colors hover:bg-accent-hover"
              >
                Try the live demo
              </Link>
              <a
                href="#how"
                className="rounded-[6px] border border-white/25 px-5 py-2.5 text-sm font-medium text-white transition-colors hover:border-white/60"
              >
                See how scoring works
              </a>
            </div>

            <p className="mt-6 text-xs text-ink-500">
              Demonstration accounts for all four roles, no signup required.
            </p>
          </div>

          {/* A real ranking, not a mockup of one. These are the actual seeded
              applicants and the actual scores the engine produces. */}
          <div className="rounded-[6px] border border-white/10 bg-white/[0.03] p-5">
            <div className="flex items-center justify-between border-b border-white/10 pb-3">
              <p className="text-xs font-medium tracking-wide text-ink-300 uppercase">
                Senior Backend Engineer, Payments
              </p>
              <span className="text-xs text-ink-500">7 applicants</span>
            </div>

            <ul className="mt-1 divide-y divide-white/5">
              {[
                { name: 'Dilani Rathnayake', years: 7, score: 84.7, gap: false },
                { name: 'Kasun Silva', years: 5, score: 75.2, gap: false },
                { name: 'Fatima Hassan', years: 6, score: 55.0, gap: true },
                { name: 'Thilina Bandara', years: 1, score: 47.8, gap: false },
                { name: 'Arjun Mehta', years: 12, score: 46.0, gap: true },
              ].map((row, index) => (
                <li key={row.name} className="flex items-center gap-3 py-2.5">
                  <span className="tabular w-4 text-sm text-ink-500">{index + 1}</span>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm text-white">{row.name}</p>
                    <p className="text-xs text-ink-500">
                      {row.years} years
                      {row.gap && <span className="ml-2 text-warning">mandatory gap</span>}
                    </p>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="h-1.5 w-16 overflow-hidden rounded-full bg-white/10">
                      <div
                        className={row.gap ? 'h-full bg-warning' : 'h-full bg-accent'}
                        style={{ width: `${row.score}%` }}
                      />
                    </div>
                    <span className="tabular w-10 text-right text-sm font-semibold text-white">
                      {row.score}
                    </span>
                  </div>
                </li>
              ))}
            </ul>

            <p className="mt-3 border-t border-white/10 pt-3 text-xs leading-relaxed text-ink-500">
              Arjun has twelve years and is a principal engineer, and still ranks
              fifth. He is missing a mandatory requirement, and the platform will
              not let strength elsewhere disguise that.
            </p>
          </div>
        </div>
      </section>

      {/* How scoring works */}
      <section id="how" className="border-b border-line">
        <div className="mx-auto max-w-6xl px-6 py-16">
          <h2 className="text-2xl font-semibold text-ink-900">How a score is reached</h2>
          <p className="mt-2 max-w-2xl text-sm leading-relaxed text-ink-500">
            Five signals are measured independently, then weighted. The weighting is
            chosen per posting, because a backend vacancy and a practice lead should
            not be decided the same way.
          </p>

          <div className="mt-8 overflow-x-auto rounded-[6px] border border-line">
            <table className="w-full min-w-[640px] text-left text-sm">
              <thead className="bg-surface-alt">
                <tr className="text-xs tracking-wide text-ink-500 uppercase">
                  <th className="px-4 py-3 font-medium">Signal</th>
                  <th className="px-4 py-3 font-medium">What it measures</th>
                  <th className="px-4 py-3 text-right font-medium">Typical weight</th>
                </tr>
              </thead>
              <tbody>
                {[
                  ['Skill coverage', 'Evidenced skills against the posting, weighted by importance, with partial credit for shallow experience', '0.40'],
                  ['Resume relevance', 'TF-IDF cosine similarity between the CV and the job description, catching experience no skill list would capture', '0.25'],
                  ['Experience fit', 'Years against the requirement, with a taper so a principal engineer is not the ideal junior hire', '0.20'],
                  ['Location fit', 'Same city, same country, or remote-compatible', '0.08'],
                  ['Education fit', 'Against the stated requirement, with no reward for exceeding it', '0.07'],
                ].map(([signal, detail, weight]) => (
                  <tr key={signal} className="border-t border-line">
                    <td className="px-4 py-3 font-medium whitespace-nowrap text-ink-900">{signal}</td>
                    <td className="px-4 py-3 text-ink-700">{detail}</td>
                    <td className="tabular px-4 py-3 text-right text-ink-500">{weight}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="mt-6 rounded-[6px] border border-accent/30 bg-accent-soft p-5">
            <p className="text-sm leading-relaxed text-ink-900">
              <strong>A missing mandatory skill caps the score rather than reducing it.</strong>{' '}
              Without that rule a candidate could offset a hard requirement failure by
              scoring well everywhere else. A screening tool must not allow that, and
              the cap is visible in the breakdown rather than applied quietly.
            </p>
          </div>
        </div>
      </section>

      {/* Roles */}
      <section id="roles" className="border-b border-line bg-surface-alt">
        <div className="mx-auto max-w-6xl px-6 py-16">
          <h2 className="text-2xl font-semibold text-ink-900">Four roles, one pipeline</h2>

          <div className="mt-8 grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {[
              {
                role: 'Candidates',
                detail:
                  'Build a profile, apply, and see the same score breakdown the recruiter sees. Rejection stops being a black box.',
              },
              {
                role: 'Recruiters',
                detail:
                  'Post roles with weighted skill requirements, choose the ranking strategy, and screen a ranked pool you can defend.',
              },
              {
                role: 'Hiring managers',
                detail:
                  'Work from the shortlist rather than the raw pool. Recruiters screen, managers decide.',
              },
              {
                role: 'Administrators',
                detail:
                  'Manage users, roles and client organisations, and monitor the system and its audit trail.',
              },
            ].map((item) => (
              <div key={item.role} className="rounded-[6px] border border-line bg-surface p-5">
                <h3 className="font-medium text-ink-900">{item.role}</h3>
                <p className="mt-2 text-sm leading-relaxed text-ink-500">{item.detail}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <footer className="border-t border-line">
        <div className="mx-auto flex max-w-6xl flex-col gap-3 px-6 py-8 sm:flex-row sm:items-center sm:justify-between">
          <Brand size="sm" />
          <p className="text-xs text-ink-500">
            SE205.3 Software Architecture &middot; Group 4 &middot; NSBM Green University
          </p>
        </div>
      </footer>
    </div>
  )
}
