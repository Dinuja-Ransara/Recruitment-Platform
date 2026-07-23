using Meridian.Ai.Models;
using Meridian.Domain.Enums;

namespace Meridian.Ai.Matching;

/// <summary>
/// Computes the individual signals every ranking strategy draws on.
///
/// The strategies differ only in how they weight these factors, never in how a
/// factor is measured. Keeping the measurements here means two strategies can
/// disagree about priorities while still agreeing about facts, and it means each
/// factor can be unit tested once rather than once per strategy.
///
/// Every factor returns a value between 0 and 1.
/// </summary>
public static class FactorCalculator
{
    /// <summary>
    /// Weighted proportion of the posting's skills the candidate can evidence.
    ///
    /// Mandatory requirements are counted at double their stated weight, so
    /// missing one costs materially more than missing a preferred skill.
    /// </summary>
    public static (double Value, IReadOnlyList<SkillMatch> Matched, IReadOnlyList<SkillGap> Missing) SkillCoverage(
        JobProfile job, CandidateProfileSnapshot candidate)
    {
        var matched = new List<SkillMatch>();
        var missing = new List<SkillGap>();

        if (job.Skills.Count == 0)
        {
            return (1.0, matched, missing);
        }

        double achievedWeight = 0;
        double totalWeight = 0;

        foreach (var requirement in job.Skills)
        {
            var weight = requirement.Weight * (requirement.IsMandatory ? 2.0 : 1.0);
            totalWeight += weight;

            if (candidate.Skills.TryGetValue(requirement.Skill, out var candidateYears))
            {
                matched.Add(new SkillMatch(requirement.Skill, requirement.IsMandatory, candidateYears, requirement.MinYears));

                // Partial credit when the skill is present but shallow: having two
                // years against a three year requirement is worth more than nothing
                // and less than full marks.
                var depth = requirement.MinYears <= 0
                    ? 1.0
                    : Math.Clamp((double)(candidateYears / requirement.MinYears), 0, 1);

                // Presence alone earns most of the credit; depth tops it up.
                achievedWeight += weight * (0.7 + (0.3 * depth));
            }
            else
            {
                missing.Add(new SkillGap(requirement.Skill, requirement.IsMandatory, requirement.MinYears));
            }
        }

        var value = totalWeight == 0 ? 1.0 : Math.Clamp(achievedWeight / totalWeight, 0, 1);
        return (value, matched, missing);
    }

    /// <summary>
    /// Textual similarity between the resume and the posting, via TF-IDF cosine.
    ///
    /// This is what catches relevant experience the skill taxonomy has no entry
    /// for: domain language, industry terms, tools nobody thought to list.
    /// </summary>
    public static double TextSimilarity(JobProfile job, CandidateProfileSnapshot candidate, TfIdfVectoriser vectoriser)
    {
        var jobVector = vectoriser.Transform($"{job.Title} {job.Description}");
        var candidateVector = vectoriser.Transform($"{candidate.Headline} {candidate.ResumeText}");
        return TfIdfVectoriser.CosineSimilarity(jobVector, candidateVector);
    }

    /// <summary>
    /// How the candidate's total experience sits against the posting's minimum.
    ///
    /// Meeting the bar scores 1. Being under it scores proportionally. Being far
    /// over it is very slightly penalised, because a principal engineer applying
    /// to a junior role is usually a poor mutual fit rather than an ideal hire.
    /// </summary>
    public static double ExperienceFit(JobProfile job, CandidateProfileSnapshot candidate)
    {
        if (job.MinYearsExperience <= 0)
        {
            return 1.0;
        }

        var ratio = (double)(candidate.YearsOfExperience / job.MinYearsExperience);

        if (ratio < 1.0)
        {
            return Math.Clamp(ratio, 0, 1);
        }

        // Overqualification taper: no penalty up to double the requirement, then
        // a gentle decline that never falls below 0.75.
        if (ratio <= 2.0)
        {
            return 1.0;
        }

        return Math.Max(0.75, 1.0 - ((ratio - 2.0) * 0.05));
    }

    /// <summary>
    /// Education against the posting's requirement. Exceeding it is not rewarded
    /// beyond meeting it, because a further degree does not make someone more
    /// suitable for a role that asked for a bachelor's.
    /// </summary>
    public static double EducationFit(JobProfile job, CandidateProfileSnapshot candidate)
    {
        if (job.RequiredEducation == EducationLevel.Unspecified)
        {
            return 1.0;
        }

        if (candidate.HighestEducation == EducationLevel.Unspecified)
        {
            // Unknown is not the same as absent. Neutral rather than zero, so a
            // profile that simply omitted education is not silently eliminated.
            return 0.5;
        }

        var required = (int)job.RequiredEducation;
        var held = (int)candidate.HighestEducation;

        if (held >= required)
        {
            return 1.0;
        }

        return Math.Clamp((double)held / required, 0, 1);
    }

    /// <summary>
    /// Practical availability: same city, same country, or remote-compatible.
    /// A remote posting makes location irrelevant, which is the point of it.
    /// </summary>
    public static (double Value, string Detail) LocationFit(JobProfile job, CandidateProfileSnapshot candidate)
    {
        if (job.WorkMode == WorkMode.Remote)
        {
            return (1.0, "Remote posting, location not a constraint");
        }

        var sameCity = !string.IsNullOrWhiteSpace(job.City)
                       && string.Equals(job.City, candidate.City, StringComparison.OrdinalIgnoreCase);
        if (sameCity)
        {
            return (1.0, $"Candidate is in {candidate.City}");
        }

        var sameCountry = !string.IsNullOrWhiteSpace(job.Country)
                          && string.Equals(job.Country, candidate.Country, StringComparison.OrdinalIgnoreCase);
        if (sameCountry)
        {
            return (0.7, $"Same country ({candidate.Country}), different city");
        }

        if (job.WorkMode == WorkMode.Hybrid && candidate.IsOpenToRemote)
        {
            return (0.4, "Hybrid role, candidate open to remote but based abroad");
        }

        return (0.15, $"Candidate based in {candidate.Country}, role is in {job.Country}");
    }
}
