using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Jobs;

namespace Meridian.Application.Common.Interfaces;

public interface IJobService
{
    /// <summary>Public job board search. Returns published postings only.</summary>
    Task<PagedResult<JobSummaryDto>> SearchAsync(JobSearchQuery query, CancellationToken ct = default);

    /// <summary>Postings belonging to the calling recruiter's organisation, any status.</summary>
    Task<PagedResult<JobSummaryDto>> ListForRecruiterAsync(int recruiterUserId, JobSearchQuery query, CancellationToken ct = default);

    Task<Result<JobDetailDto>> GetByIdAsync(int id, bool includeUnpublished, CancellationToken ct = default);

    Task<Result<JobDetailDto>> CreateAsync(CreateJobRequest request, int recruiterUserId, CancellationToken ct = default);

    Task<Result<JobDetailDto>> UpdateAsync(int id, UpdateJobRequest request, int recruiterUserId, CancellationToken ct = default);

    Task<Result<JobDetailDto>> PublishAsync(int id, int recruiterUserId, CancellationToken ct = default);

    Task<Result<JobDetailDto>> CloseAsync(int id, int recruiterUserId, CancellationToken ct = default);

    /// <summary>
    /// Prototype pattern in use. Deep-copies a posting into a new draft so a
    /// recruiter can repost the same role in another office and edit only the
    /// delta rather than retyping the description and every skill requirement.
    /// </summary>
    Task<Result<JobDetailDto>> DuplicateAsync(int id, int recruiterUserId, CancellationToken ct = default);
}
