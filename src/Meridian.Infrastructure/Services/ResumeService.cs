using Meridian.Application.Common.Interfaces;
using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Resumes;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// Resume upload and storage. The file itself goes to <see cref="IFileStorage"/>
/// (Cloudflare R2); this service owns only the database record and the rules
/// around it, deliberately unaware of which cloud provider actually holds the
/// bytes.
/// </summary>
public class ResumeService : IResumeService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".txt"
    };

    private const long MaxSizeInBytes = 5 * 1024 * 1024;

    private readonly MeridianDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorage _storage;

    public ResumeService(MeridianDbContext context, IUnitOfWork unitOfWork, IFileStorage storage)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task<Result<ResumeSummaryDto>> UploadAsync(
        int candidateUserId, string fileName, string contentType, long sizeInBytes, Stream content, CancellationToken ct = default)
    {
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == candidateUserId, ct);
        if (profile is null)
        {
            return Result<ResumeSummaryDto>.Failure("No candidate profile exists for this account.");
        }

        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return Result<ResumeSummaryDto>.Failure("Only PDF, DOC, DOCX or TXT files are accepted.");
        }

        if (sizeInBytes <= 0 || sizeInBytes > MaxSizeInBytes)
        {
            return Result<ResumeSummaryDto>.Failure("The file must be under 5 MB.");
        }

        var isFirstResume = !await _context.Resumes.AnyAsync(r => r.CandidateProfileId == profile.Id, ct);

        // Object keys are namespaced by candidate id and made unique with a
        // guid, so two people uploading "cv.pdf" on the same day never collide,
        // and a candidate's own resumes are never intermixed with anyone else's.
        var key = $"resumes/{profile.Id}/{Guid.NewGuid():N}{extension}";
        await _storage.UploadAsync(key, content, contentType, ct);

        var resume = new Resume
        {
            CandidateProfileId = profile.Id,
            FileName = fileName,
            StoredPath = key,
            ContentType = contentType,
            SizeInBytes = sizeInBytes,
            SourceFormat = extension.TrimStart('.').ToLowerInvariant(),
            IsPrimary = isFirstResume
        };

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync(token);
        }, ct);

        return Result<ResumeSummaryDto>.Success(await ToDtoAsync(resume, ct));
    }

    public async Task<IReadOnlyList<ResumeSummaryDto>> ListForCandidateAsync(int candidateUserId, CancellationToken ct = default)
    {
        var resumes = await _context.Resumes
            .Where(r => r.CandidateProfile.UserId == candidateUserId)
            .OrderByDescending(r => r.IsPrimary)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var result = new List<ResumeSummaryDto>(resumes.Count);
        foreach (var resume in resumes)
        {
            result.Add(await ToDtoAsync(resume, ct));
        }
        return result;
    }

    public async Task<Result<bool>> DeleteAsync(int resumeId, int candidateUserId, CancellationToken ct = default)
    {
        var resume = await _context.Resumes
            .Include(r => r.CandidateProfile)
            .FirstOrDefaultAsync(r => r.Id == resumeId, ct);

        if (resume is null || resume.CandidateProfile.UserId != candidateUserId)
        {
            return Result<bool>.Failure("Resume not found.");
        }

        await _storage.DeleteAsync(resume.StoredPath, ct);
        _context.Resumes.Remove(resume);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private async Task<ResumeSummaryDto> ToDtoAsync(Resume resume, CancellationToken ct) => new()
    {
        Id = resume.Id,
        FileName = resume.FileName,
        SourceFormat = resume.SourceFormat,
        SizeInBytes = resume.SizeInBytes,
        IsPrimary = resume.IsPrimary,
        CreatedAt = resume.CreatedAt,
        DownloadUrl = await _storage.GetDownloadUrlAsync(resume.StoredPath, TimeSpan.FromMinutes(15), ct)
    };
}
