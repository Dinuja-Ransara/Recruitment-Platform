using Meridian.Ai.Matching;
using Meridian.Ai.Models;
using Meridian.Ai.Strategies;
using Meridian.Domain.Enums;

namespace Meridian.Ai.Tests;

public class MatchingEngineTests
{
    private static MatchingEngine Engine() => new(new RankingStrategyFactory());

    [Fact]
    public void Scoring_is_deterministic_across_repeated_runs()
    {
        // The central claim of the engine. If this fails, nothing else in the
        // suite means anything, because the results would not be reproducible.
        var job = TestData.BackendJob();
        var candidate = TestData.StrongCandidate();

        var first = Engine().Score(job, candidate);
        var second = Engine().Score(job, candidate);
        var third = Engine().Score(job, candidate);

        Assert.Equal(first.Score, second.Score);
        Assert.Equal(second.Score, third.Score);
    }

    [Fact]
    public void Strong_candidate_outscores_unrelated_candidate()
    {
        var job = TestData.BackendJob();

        var strong = Engine().Score(job, TestData.StrongCandidate());
        var unrelated = Engine().Score(job, TestData.UnrelatedCandidate());

        Assert.True(strong.Score > unrelated.Score,
            $"Expected the matching candidate to outscore the unrelated one, got {strong.Score} against {unrelated.Score}.");
    }

    [Fact]
    public void Strong_candidate_scores_in_the_upper_band()
    {
        var result = Engine().Score(TestData.BackendJob(), TestData.StrongCandidate());

        Assert.True(result.Score >= 70, $"Expected at least 70 for a genuine match, got {result.Score}.");
        Assert.False(result.HasMandatoryGap);
    }

    [Fact]
    public void Missing_mandatory_skill_caps_the_score_regardless_of_other_strengths()
    {
        // Without the cap, a candidate could compensate for a hard requirement
        // failure by scoring well on experience, location and resume text. A
        // screening tool must not permit that.
        var result = Engine().Score(TestData.BackendJob(), TestData.MissingMandatorySkill());

        Assert.True(result.HasMandatoryGap);
        Assert.True(result.Score <= 55, $"Expected the cap to hold at 55, got {result.Score}.");
        Assert.Contains("SQL Server", result.Summary);
    }

    [Fact]
    public void Every_score_carries_its_factor_breakdown()
    {
        var result = Engine().Score(TestData.BackendJob(), TestData.StrongCandidate());

        Assert.Equal(5, result.Factors.Count);
        Assert.All(result.Factors, factor =>
        {
            Assert.False(string.IsNullOrWhiteSpace(factor.Name));
            Assert.False(string.IsNullOrWhiteSpace(factor.Detail));
            Assert.InRange(factor.Value, 0, 1);
        });

        // The reported contributions must actually reconstruct the headline score,
        // otherwise the explanation would be decoration rather than evidence.
        var reconstructed = Math.Round(result.Factors.Sum(f => f.Contribution) * 100, 1);
        Assert.InRange(reconstructed, result.Score - 0.2, result.Score + 0.2);
    }

    [Fact]
    public void Matched_and_missing_skills_together_account_for_every_requirement()
    {
        var job = TestData.BackendJob();
        var result = Engine().Score(job, TestData.MissingMandatorySkill());

        Assert.Equal(job.Skills.Count, result.MatchedSkills.Count + result.MissingSkills.Count);
        Assert.Contains(result.MissingSkills, s => s.Skill == "SQL Server" && s.IsMandatory);
    }

    [Fact]
    public void Ranking_is_ordered_by_score_and_stable_on_ties()
    {
        var job = TestData.BackendJob();
        var pool = new List<CandidateProfileSnapshot>
        {
            TestData.UnrelatedCandidate(),
            TestData.StrongCandidate(),
            TestData.MissingMandatorySkill()
        };

        var ranked = Engine().RankApplicants(job, pool);
        var scores = ranked.Select(r => r.Explanation.Score).ToList();

        Assert.Equal(scores.OrderByDescending(s => s), scores);
        Assert.Equal(TestData.StrongCandidate().CandidateId, ranked[0].CandidateId);

        // Re-ranking the same pool in a different input order must not change the result.
        var reshuffled = Engine().RankApplicants(job, pool.AsEnumerable().Reverse().ToList());
        Assert.Equal(ranked.Select(r => r.CandidateId), reshuffled.Select(r => r.CandidateId));
    }

    [Fact]
    public void Empty_applicant_pool_returns_empty_rather_than_throwing()
    {
        var ranked = Engine().RankApplicants(TestData.BackendJob(), Array.Empty<CandidateProfileSnapshot>());
        Assert.Empty(ranked);
    }

    [Fact]
    public void Job_recommendations_put_the_relevant_posting_first()
    {
        var backend = TestData.BackendJob();
        var unrelated = backend with
        {
            JobId = 2,
            Title = "Front of House Manager",
            Description = "Lead the hotel front desk team, manage guest relations, rotas and supplier orders.",
            Skills = Array.Empty<SkillRequirement>(),
            MinYearsExperience = 1,
            RequiredEducation = EducationLevel.Unspecified
        };

        var recommendations = Engine().RecommendJobs(TestData.StrongCandidate(), new[] { unrelated, backend });

        Assert.Equal(backend.JobId, recommendations[0].JobId);
    }

    [Theory]
    [InlineData(RankingStrategyType.SkillWeighted)]
    [InlineData(RankingStrategyType.TfIdfSimilarity)]
    [InlineData(RankingStrategyType.Hybrid)]
    [InlineData(RankingStrategyType.ExperienceFirst)]
    public void Every_strategy_produces_a_score_inside_the_valid_range(RankingStrategyType type)
    {
        var result = Engine().Score(TestData.BackendJob(), TestData.StrongCandidate(), type);

        Assert.InRange(result.Score, 0, 100);
        Assert.False(string.IsNullOrWhiteSpace(result.Strategy));
    }

    [Fact]
    public void Skill_weighted_strategy_punishes_a_skill_gap_harder_than_experience_first()
    {
        // The strategies must actually differ, otherwise the Strategy pattern is
        // ceremony. A candidate short on skills should fare better under the
        // experience-led strategy than under the skills-led one.
        var job = TestData.BackendJob();
        var thinSkills = TestData.StrongCandidate() with
        {
            Skills = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["C#"] = 6 }
        };

        var skillLed = Engine().Score(job, thinSkills, RankingStrategyType.SkillWeighted);
        var experienceLed = Engine().Score(job, thinSkills, RankingStrategyType.ExperienceFirst);

        Assert.True(experienceLed.Score > skillLed.Score,
            $"Experience-first should be kinder to a skills gap, got {experienceLed.Score} against {skillLed.Score}.");
    }

    [Fact]
    public void Unknown_strategy_value_falls_back_to_hybrid_instead_of_throwing()
    {
        var factory = new RankingStrategyFactory();
        var strategy = factory.Create((RankingStrategyType)99);

        Assert.Equal(RankingStrategyType.Hybrid, strategy.Type);
    }

    [Fact]
    public void All_registered_strategies_declare_weights_that_sum_to_one()
    {
        // A typo in a subclass would otherwise let a score exceed 100 silently.
        foreach (var strategy in new RankingStrategyFactory().All())
        {
            var result = Engine().Score(TestData.BackendJob(), TestData.StrongCandidate(), strategy.Type);
            var weightTotal = result.Factors.Sum(f => f.Weight);

            Assert.InRange(weightTotal, 0.999, 1.001);
        }
    }
}
