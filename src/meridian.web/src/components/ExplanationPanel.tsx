import type { MatchExplanation } from '../lib/types'
import { Badge } from './ui'

/**
 * Renders why a score is what it is.
 *
 * This is the component the whole engine exists to make possible. It shows the
 * arithmetic: each factor's measured value, the weight applied to it, and what
 * that contributed to the total. A recruiter can defend a shortlist from this,
 * and a rejected candidate could be told something more useful than "no".
 */
export function ExplanationPanel({ explanation }: { explanation: MatchExplanation }) {
  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center gap-2">
        <Badge tone="accent">{explanation.strategy}</Badge>
        {explanation.hasMandatoryGap && <Badge tone="warning">Mandatory requirement not met</Badge>}
        <span className="text-sm text-ink-700">{explanation.summary}</span>
      </div>

      <div>
        <h4 className="text-xs font-semibold tracking-wide text-ink-500 uppercase">
          How the score was reached
        </h4>
        <div className="mt-3 overflow-x-auto">
          <table className="w-full min-w-[560px] text-left text-sm">
            <thead>
              <tr className="border-b border-line text-xs tracking-wide text-ink-500 uppercase">
                <th className="py-2 pr-4 font-medium">Factor</th>
                <th className="py-2 pr-4 text-right font-medium">Measured</th>
                <th className="py-2 pr-4 text-right font-medium">Weight</th>
                <th className="py-2 pr-4 text-right font-medium">Contribution</th>
                <th className="py-2 font-medium">Basis</th>
              </tr>
            </thead>
            <tbody>
              {explanation.factors.map((factor) => (
                <tr key={factor.name} className="border-b border-line align-top last:border-0">
                  <td className="py-2.5 pr-4 font-medium text-ink-900">{factor.name}</td>
                  <td className="py-2.5 pr-4">
                    {/* The bar makes a weak factor visible at a glance. Reading
                        five decimals to find the one dragging a score down is
                        exactly the work this panel exists to remove. */}
                    <div className="flex items-center justify-end gap-2">
                      <div className="h-1.5 w-16 overflow-hidden rounded-full bg-surface-alt">
                        <div
                          className={`h-full ${
                            factor.value >= 0.75
                              ? 'bg-positive'
                              : factor.value >= 0.4
                                ? 'bg-accent'
                                : 'bg-warning'
                          }`}
                          style={{ width: `${Math.min(100, factor.value * 100)}%` }}
                        />
                      </div>
                      <span className="tabular w-11 text-right text-ink-700">
                        {factor.value.toFixed(2)}
                      </span>
                    </div>
                  </td>
                  <td className="tabular py-2.5 pr-4 text-right text-ink-500">
                    {factor.weight.toFixed(2)}
                  </td>
                  <td className="tabular py-2.5 pr-4 text-right font-medium text-ink-900">
                    {(factor.contribution * 100).toFixed(1)}
                  </td>
                  <td className="py-2.5 text-xs text-ink-500">{factor.detail}</td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr className="border-t border-line-strong">
                <td className="py-2.5 pr-4 text-xs font-semibold tracking-wide text-ink-500 uppercase">
                  Total
                </td>
                <td />
                <td />
                <td className="tabular py-2.5 pr-4 text-right font-semibold text-ink-900">
                  {explanation.score.toFixed(1)}
                </td>
                <td />
              </tr>
            </tfoot>
          </table>
        </div>
      </div>

      <div className="grid gap-5 sm:grid-cols-2">
        <div>
          <h4 className="text-xs font-semibold tracking-wide text-ink-500 uppercase">
            Evidenced skills
          </h4>
          {explanation.matchedSkills.length === 0 ? (
            <p className="mt-2 text-sm text-ink-500">None of the required skills were found.</p>
          ) : (
            <ul className="mt-2 space-y-1.5">
              {explanation.matchedSkills.map((skill) => (
                <li key={skill.skill} className="flex items-center justify-between text-sm">
                  <span className="text-ink-900">
                    {skill.skill}
                    {skill.isMandatory && <span className="ml-1 text-xs text-ink-500">required</span>}
                  </span>
                  <span
                    className={`tabular text-xs ${
                      skill.meetsExperienceBar ? 'text-positive' : 'text-warning'
                    }`}
                  >
                    {skill.candidateYears}y against {skill.requiredYears}y
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div>
          <h4 className="text-xs font-semibold tracking-wide text-ink-500 uppercase">Gaps</h4>
          {explanation.missingSkills.length === 0 ? (
            <p className="mt-2 text-sm text-positive">Every required skill is evidenced.</p>
          ) : (
            <ul className="mt-2 space-y-1.5">
              {explanation.missingSkills.map((skill) => (
                <li key={skill.skill} className="flex items-center justify-between text-sm">
                  <span className="text-ink-900">{skill.skill}</span>
                  {skill.isMandatory ? (
                    <Badge tone="danger">Mandatory</Badge>
                  ) : (
                    <Badge>Preferred</Badge>
                  )}
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}
