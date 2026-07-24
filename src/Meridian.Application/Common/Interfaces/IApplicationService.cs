using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Applications;

namespace Meridian.Application.Common.Interfaces;

public interface IApplicationService
{
    /// <summary>Submits an application and scores it against the posting immediately.</summary>
    Task<Result<ApplicationDetailDto>> ApplyAsync(ApplyRequest request, int candidateUserId, CancellationToken ct = default);

    /// <summary>The calling candidate's own applications.</summary>
    Task<IReadOnlyList<ApplicationSummaryDto>> ListForCandidateAsync(int candidateUserId, CancellationToken ct = default);

    Task<Result<ApplicationDetailDto>> GetForCandidateAsync(int applicationId, int candidateUserId, CancellationToken ct = default);

    /// <summary>
    /// The applicant pool for a posting, ranked best first by the matching engine.
    /// Scores are recomputed across the whole pool so term rarity is judged
    /// against the actual applicants.
    /// </summary>
    Task<Result<IReadOnlyList<ApplicantDto>>> RankApplicantsAsync(int jobPostingId, int staffUserId, CancellationToken ct = default);

    /// <summary>Moves an application through the pipeline, recording the transition.</summary>
    Task<Result<ApplicationDetailDto>> ChangeStatusAsync(
        int applicationId, ChangeStatusRequest request, int staffUserId, CancellationToken ct = default);

    /// <summary>Withdraws the candidate's own application.</summary>
    Task<Result<ApplicationDetailDto>> WithdrawAsync(int applicationId, int candidateUserId, CancellationToken ct = default);

    /// <summary>Postings recommended to the calling candidate, best first.</summary>
    Task<IReadOnlyList<JobRecommendationDto>> RecommendJobsAsync(int candidateUserId, int take = 10, CancellationToken ct = default);
}
