using Meridian.Ai.Models;
using Meridian.Ai.Strategies;
using Meridian.Domain.Enums;

namespace Meridian.Ai.Matching;

/// <summary>
/// The public entry point to the scoring engine.
///
/// Everything here runs locally and deterministically. There is no model
/// download, no API key and no network call, which means the ranking behaves
/// identically on a developer machine, in CI and in front of an evaluator, and
/// that the same inputs always produce the same score. That reproducibility is
/// what allows the engine to be unit tested at all.
/// </summary>
public interface IMatchingEngine
{
    /// <summary>Scores one candidate against one posting.</summary>
    MatchExplanation Score(JobProfile job, CandidateProfileSnapshot candidate, RankingStrategyType? strategyOverride = null);

    /// <summary>
    /// Ranks an applicant pool for one posting, best first. The corpus is fitted
    /// across the whole pool, so term rarity is judged against the actual
    /// applicants rather than an arbitrary external baseline.
    /// </summary>
    IReadOnlyList<RankedCandidate> RankApplicants(
        JobProfile job, IReadOnlyList<CandidateProfileSnapshot> candidates, RankingStrategyType? strategyOverride = null);

    /// <summary>Recommends postings for one candidate, best first.</summary>
    IReadOnlyList<RecommendedJob> RecommendJobs(
        CandidateProfileSnapshot candidate, IReadOnlyList<JobProfile> jobs, int take = 10);
}

public record RankedCandidate(int CandidateId, string FullName, MatchExplanation Explanation);

public record RecommendedJob(int JobId, string Title, MatchExplanation Explanation);

public class MatchingEngine : IMatchingEngine
{
    private readonly IRankingStrategyFactory _strategies;

    public MatchingEngine(IRankingStrategyFactory strategies)
    {
        _strategies = strategies;
    }

    public MatchExplanation Score(
        JobProfile job, CandidateProfileSnapshot candidate, RankingStrategyType? strategyOverride = null)
    {
        var vectoriser = new TfIdfVectoriser();

        // With a single pair there is no corpus to learn rarity from, so both
        // documents are fitted against each other. Similarity is still meaningful;
        // it is simply less discriminating than when ranking a real pool.
        vectoriser.Fit(new[]
        {
            $"{job.Title} {job.Description}",
            $"{candidate.Headline} {candidate.ResumeText}"
        });

        var strategy = _strategies.Create(strategyOverride ?? DefaultFor(job));
        return strategy.Score(job, candidate, vectoriser);
    }

    public IReadOnlyList<RankedCandidate> RankApplicants(
        JobProfile job, IReadOnlyList<CandidateProfileSnapshot> candidates, RankingStrategyType? strategyOverride = null)
    {
        if (candidates.Count == 0)
        {
            return Array.Empty<RankedCandidate>();
        }

        var vectoriser = new TfIdfVectoriser();
        var corpus = new List<string> { $"{job.Title} {job.Description}" };
        corpus.AddRange(candidates.Select(c => $"{c.Headline} {c.ResumeText}"));
        vectoriser.Fit(corpus);

        var strategy = _strategies.Create(strategyOverride ?? DefaultFor(job));

        return candidates
            .Select(c => new RankedCandidate(c.CandidateId, c.FullName, strategy.Score(job, c, vectoriser)))
            // Ties are broken by candidate id so the order is stable across runs.
            // An unstable ranking would make the same shortlist look different on
            // refresh, which destroys a recruiter's trust in the tool.
            .OrderByDescending(r => r.Explanation.Score)
            .ThenBy(r => r.CandidateId)
            .ToList();
    }

    public IReadOnlyList<RecommendedJob> RecommendJobs(
        CandidateProfileSnapshot candidate, IReadOnlyList<JobProfile> jobs, int take = 10)
    {
        if (jobs.Count == 0)
        {
            return Array.Empty<RecommendedJob>();
        }

        var vectoriser = new TfIdfVectoriser();
        var corpus = new List<string> { $"{candidate.Headline} {candidate.ResumeText}" };
        corpus.AddRange(jobs.Select(j => $"{j.Title} {j.Description}"));
        vectoriser.Fit(corpus);

        return jobs
            .Select(job =>
            {
                var strategy = _strategies.Create(DefaultFor(job));
                return new RecommendedJob(job.JobId, job.Title, strategy.Score(job, candidate, vectoriser));
            })
            .OrderByDescending(r => r.Explanation.Score)
            .ThenBy(r => r.JobId)
            .Take(take)
            .ToList();
    }

    /// <summary>
    /// Seniority picks the default strategy when the posting has not chosen one.
    /// Leadership hires are decided on depth of experience; everything else
    /// starts from the balanced blend.
    /// </summary>
    private static RankingStrategyType DefaultFor(JobProfile job) =>
        job.Seniority >= SeniorityLevel.Lead
            ? RankingStrategyType.ExperienceFirst
            : RankingStrategyType.Hybrid;
}
