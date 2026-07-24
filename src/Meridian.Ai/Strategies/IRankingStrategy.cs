using Meridian.Ai.Matching;
using Meridian.Ai.Models;
using Meridian.Domain.Enums;

namespace Meridian.Ai.Strategies;

/// <summary>
/// Strategy pattern. Each implementation weights the same measured factors
/// differently, and the recruiter chooses one per job posting.
///
/// This exists because a single scoring formula is wrong for every role. A
/// backend engineering vacancy should be decided mostly on demonstrable skills;
/// a practice lead should be decided mostly on depth of experience and the
/// language of the CV. Hard-coding one formula would force the recruiter to
/// argue with the tool instead of using it.
/// </summary>
public interface IRankingStrategy
{
    RankingStrategyType Type { get; }

    /// <summary>Shown in the recruiter's strategy picker.</summary>
    string DisplayName { get; }

    /// <summary>Why a recruiter would pick this one. Rendered as help text.</summary>
    string WhenToUse { get; }

    /// <summary>
    /// Scores one candidate against one posting. The vectoriser must already
    /// have been fitted over the corpus being ranked.
    /// </summary>
    MatchExplanation Score(JobProfile job, CandidateProfileSnapshot candidate, TfIdfVectoriser vectoriser);
}
