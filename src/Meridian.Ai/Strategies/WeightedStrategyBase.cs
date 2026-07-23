using Meridian.Ai.Matching;
using Meridian.Ai.Models;
using Meridian.Domain.Enums;

namespace Meridian.Ai.Strategies;

/// <summary>
/// Shared machinery for every strategy that scores by weighting the standard
/// factors. A concrete strategy supplies only its weights and its identity.
///
/// Template Method in shape: the algorithm is fixed here, the varying part is
/// deferred to the subclass.
/// </summary>
public abstract class WeightedStrategyBase : IRankingStrategy
{
    public abstract RankingStrategyType Type { get; }
    public abstract string DisplayName { get; }
    public abstract string WhenToUse { get; }

    /// <summary>Weights for skill coverage, text similarity, experience, education and location.</summary>
    protected abstract FactorWeights Weights { get; }

    public MatchExplanation Score(JobProfile job, CandidateProfileSnapshot candidate, TfIdfVectoriser vectoriser)
    {
        var (skillValue, matched, missing) = FactorCalculator.SkillCoverage(job, candidate);
        var similarity = FactorCalculator.TextSimilarity(job, candidate, vectoriser);
        var experience = FactorCalculator.ExperienceFit(job, candidate);
        var education = FactorCalculator.EducationFit(job, candidate);
        var (location, locationDetail) = FactorCalculator.LocationFit(job, candidate);

        var weights = Weights;

        var factors = new List<ScoreFactor>
        {
            new("Skill coverage", skillValue, weights.Skills,
                $"{matched.Count} of {job.Skills.Count} required skills evidenced"),
            new("Resume relevance", similarity, weights.Similarity,
                "TF-IDF cosine similarity between the resume and the job description"),
            new("Experience fit", experience, weights.Experience,
                $"{candidate.YearsOfExperience:0.#} years against a {job.MinYearsExperience:0.#} year requirement"),
            new("Education fit", education, weights.Education,
                job.RequiredEducation == EducationLevel.Unspecified
                    ? "No education requirement stated"
                    : $"Holds {candidate.HighestEducation}, posting asks for {job.RequiredEducation}"),
            new("Location fit", location, weights.Location, locationDetail)
        };

        var raw = factors.Sum(f => f.Contribution);

        // A missing mandatory skill caps the result rather than merely reducing it.
        // Without this a candidate can compensate for a hard requirement failure
        // by scoring well everywhere else, which is exactly what a screening tool
        // must not allow.
        var mandatoryGaps = missing.Count(m => m.IsMandatory);
        if (mandatoryGaps > 0)
        {
            raw = Math.Min(raw, 0.55);
        }

        var score = Math.Round(Math.Clamp(raw, 0, 1) * 100, 1);

        return new MatchExplanation
        {
            Score = score,
            Strategy = DisplayName,
            Factors = factors,
            MatchedSkills = matched,
            MissingSkills = missing,
            Summary = BuildSummary(score, matched.Count, job.Skills.Count, mandatoryGaps, missing)
        };
    }

    private static string BuildSummary(
        double score, int matchedCount, int requiredCount, int mandatoryGaps, IReadOnlyList<SkillGap> missing)
    {
        if (mandatoryGaps > 0)
        {
            var names = string.Join(", ", missing.Where(m => m.IsMandatory).Select(m => m.Skill));
            return $"Scored {score:0.#} but capped: missing mandatory {names}.";
        }

        var band = score switch
        {
            >= 85 => "Strong match",
            >= 70 => "Good match",
            >= 55 => "Partial match",
            >= 35 => "Weak match",
            _ => "Poor match"
        };

        return $"{band}, {score:0.#}. Evidenced {matchedCount} of {requiredCount} required skills.";
    }
}

/// <summary>
/// The five weights a strategy applies. They are validated to sum to 1 so that a
/// score can never exceed 100 through a typo in a subclass.
/// </summary>
public record FactorWeights(double Skills, double Similarity, double Experience, double Education, double Location)
{
    public void Validate()
    {
        var total = Skills + Similarity + Experience + Education + Location;
        if (Math.Abs(total - 1.0) > 0.0001)
        {
            throw new InvalidOperationException($"Factor weights must sum to 1.0 but summed to {total}.");
        }
    }
}
