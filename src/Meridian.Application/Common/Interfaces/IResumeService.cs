using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Resumes;

namespace Meridian.Application.Common.Interfaces;

public interface IResumeService
{
    /// <summary>
    /// Uploads a CV to cloud storage and records it against the calling
    /// candidate's profile. The first resume a candidate uploads becomes
    /// their primary one automatically.
    /// </summary>
    Task<Result<ResumeSummaryDto>> UploadAsync(
        int candidateUserId, string fileName, string contentType, long sizeInBytes, Stream content, CancellationToken ct = default);

    Task<IReadOnlyList<ResumeSummaryDto>> ListForCandidateAsync(int candidateUserId, CancellationToken ct = default);

    Task<Result<bool>> DeleteAsync(int resumeId, int candidateUserId, CancellationToken ct = default);
}
