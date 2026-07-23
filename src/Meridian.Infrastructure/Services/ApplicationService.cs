using System.Text.Json;
using Meridian.Ai.Matching;
using Meridian.Application.Common.Interfaces;
using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Applications;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// The application pipeline: submit, track, rank, transition, withdraw.
///
/// Scoring happens at two moments. Once on submission, so the candidate and the
/// recruiter both see a result immediately, and again whenever the pool is
/// ranked, because inverse document frequency depends on the corpus and a score
/// computed against one applicant means less than one computed against forty.
/// </summary>
public class ApplicationService : IApplicationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    /// <summary>
    /// Transitions the pipeline permits. Encoded once here rather than checked ad
    /// hoc, so an application cannot jump from Submitted straight to Hired.
    /// </summary>
    private static readonly Dictionary<ApplicationStatus, ApplicationStatus[]> AllowedTransitions = new()
    {
        [ApplicationStatus.Submitted] = new[] { ApplicationStatus.UnderReview, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn },
        [ApplicationStatus.UnderReview] = new[] { ApplicationStatus.Shortlisted, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn },
        [ApplicationStatus.Shortlisted] = new[] { ApplicationStatus.InterviewScheduled, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn },
        [ApplicationStatus.InterviewScheduled] = new[] { ApplicationStatus.Interviewed, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn },
        [ApplicationStatus.Interviewed] = new[] { ApplicationStatus.OfferExtended, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn },
        [ApplicationStatus.OfferExtended] = new[] { ApplicationStatus.Hired, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn },
        [ApplicationStatus.Hired] = Array.Empty<ApplicationStatus>(),
        [ApplicationStatus.Rejected] = Array.Empty<ApplicationStatus>(),
        [ApplicationStatus.Withdrawn] = Array.Empty<ApplicationStatus>()
    };

    private readonly MeridianDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchingEngine _engine;

    public ApplicationService(MeridianDbContext context, IUnitOfWork unitOfWork, IMatchingEngine engine)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _engine = engine;
    }

    public async Task<Result<ApplicationDetailDto>> ApplyAsync(ApplyRequest request, int candidateUserId, CancellationToken ct = default)
    {
        var profile = await LoadCandidateAsync(candidateUserId, ct);
        if (profile is null)
        {
            return Result<ApplicationDetailDto>.Failure("No candidate profile exists for this account.");
        }

        var job = await JobQuery().FirstOrDefaultAsync(j => j.Id == request.JobPostingId, ct);
        if (job is null || job.Status != JobStatus.Published)
        {
            return Result<ApplicationDetailDto>.Failure("That posting is not open for applications.");
        }

        if (job.ClosingDate is not null && job.ClosingDate < DateTime.UtcNow)
        {
            return Result<ApplicationDetailDto>.Failure("The closing date for this posting has passed.");
        }

        if (await _context.JobApplications.AnyAsync(
                a => a.JobPostingId == job.Id && a.CandidateProfileId == profile.Id, ct))
        {
            return Result<ApplicationDetailDto>.Failure("You have already applied to this posting.");
        }

        var resume = request.ResumeId is { } resumeId
            ? profile.Resumes.FirstOrDefault(r => r.Id == resumeId)
            : profile.Resumes.FirstOrDefault(r => r.IsPrimary) ?? profile.Resumes.FirstOrDefault();

        if (request.ResumeId is not null && resume is null)
        {
            return Result<ApplicationDetailDto>.Failure("That resume does not belong to this candidate.");
        }

        var explanation = _engine.Score(
            MatchProfileMapper.ToJobProfile(job),
            MatchProfileMapper.ToCandidateSnapshot(profile, resume),
            job.RankingStrategy);

        var application = new JobApplication
        {
            JobPostingId = job.Id,
            CandidateProfileId = profile.Id,
            ResumeId = resume?.Id,
            CoverLetter = request.CoverLetter,
            Status = ApplicationStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            MatchScore = explanation.Score,
            ScoreBreakdownJson = JsonSerializer.Serialize(MatchProfileMapper.ToDto(explanation), JsonOptions),
            ScoredAt = DateTime.UtcNow
        };

        // The application, its first timeline entry, the recruiter's notification
        // and the audit record are one logical event and commit together.
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync(token);

            _context.ApplicationEvents.Add(new ApplicationEvent
            {
                JobApplicationId = application.Id,
                FromStatus = null,
                ToStatus = ApplicationStatus.Submitted,
                Note = "Application submitted.",
                ActorUserId = candidateUserId
            });

            _context.Notifications.Add(new Notification
            {
                UserId = job.PostedByUserId,
                Channel = NotificationChannel.InApp,
                Subject = $"New applicant for {job.Title}",
                Body = $"{profile.User?.FullName} applied. {explanation.Summary}",
                RelatedEntityName = nameof(JobApplication),
                RelatedEntityId = application.Id
            });

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = candidateUserId,
                Action = "ApplicationSubmitted",
                EntityName = nameof(JobApplication),
                EntityId = application.Id.ToString()
            });
        }, ct);

        return await GetForCandidateAsync(application.Id, candidateUserId, ct);
    }

    public async Task<IReadOnlyList<ApplicationSummaryDto>> ListForCandidateAsync(int candidateUserId, CancellationToken ct = default)
    {
        var profileId = await _context.CandidateProfiles
            .Where(c => c.UserId == candidateUserId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);

        if (profileId is null)
        {
            return Array.Empty<ApplicationSummaryDto>();
        }

        var applications = await _context.JobApplications
            .AsNoTracking()
            .Include(a => a.JobPosting).ThenInclude(j => j.Organization)
            .Where(a => a.CandidateProfileId == profileId)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync(ct);

        return applications.Select(ToSummary).ToList();
    }

    public async Task<Result<ApplicationDetailDto>> GetForCandidateAsync(int applicationId, int candidateUserId, CancellationToken ct = default)
    {
        var application = await DetailQuery().FirstOrDefaultAsync(a => a.Id == applicationId, ct);

        if (application is null || application.CandidateProfile.UserId != candidateUserId)
        {
            // Someone else's application is reported as missing rather than
            // forbidden, so ids cannot be probed for existence.
            return Result<ApplicationDetailDto>.Failure("Application not found.");
        }

        return Result<ApplicationDetailDto>.Success(ToDetail(application));
    }

    public async Task<Result<IReadOnlyList<ApplicantDto>>> RankApplicantsAsync(int jobPostingId, int staffUserId, CancellationToken ct = default)
    {
        var job = await JobQuery().FirstOrDefaultAsync(j => j.Id == jobPostingId, ct);
        if (job is null)
        {
            return Result<IReadOnlyList<ApplicantDto>>.Failure("Job posting not found.");
        }

        var organizationId = await _context.Users
            .Where(u => u.Id == staffUserId)
            .Select(u => u.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (job.OrganizationId != organizationId)
        {
            return Result<IReadOnlyList<ApplicantDto>>.Failure("This posting belongs to another organisation.");
        }

        var applications = await _context.JobApplications
            .Include(a => a.CandidateProfile).ThenInclude(c => c.User)
            .Include(a => a.Resume!).ThenInclude(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
            .Where(a => a.JobPostingId == jobPostingId)
            .ToListAsync(ct);

        if (applications.Count == 0)
        {
            return Result<IReadOnlyList<ApplicantDto>>.Success(Array.Empty<ApplicantDto>());
        }

        var jobProfile = MatchProfileMapper.ToJobProfile(job);
        var snapshots = applications
            .Select(a => MatchProfileMapper.ToCandidateSnapshot(a.CandidateProfile, a.Resume))
            .ToList();

        var ranked = _engine.RankApplicants(jobProfile, snapshots, job.RankingStrategy);

        // Persist the freshly computed scores so the candidate's own view and any
        // later report agree with what the recruiter just saw.
        var byCandidate = applications.ToDictionary(a => a.CandidateProfileId);
        foreach (var entry in ranked)
        {
            if (byCandidate.TryGetValue(entry.CandidateId, out var application))
            {
                application.MatchScore = entry.Explanation.Score;
                application.ScoreBreakdownJson = JsonSerializer.Serialize(MatchProfileMapper.ToDto(entry.Explanation), JsonOptions);
                application.ScoredAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);

        var result = ranked
            .Where(r => byCandidate.ContainsKey(r.CandidateId))
            .Select(r =>
            {
                var application = byCandidate[r.CandidateId];
                var profile = application.CandidateProfile;

                return new ApplicantDto
                {
                    ApplicationId = application.Id,
                    CandidateProfileId = profile.Id,
                    FullName = profile.User?.FullName ?? string.Empty,
                    Headline = profile.Headline,
                    City = profile.City,
                    Country = profile.Country,
                    YearsOfExperience = profile.YearsOfExperience,
                    HighestEducation = profile.HighestEducation,
                    Status = application.Status,
                    SubmittedAt = application.SubmittedAt,
                    MatchScore = r.Explanation.Score,
                    Explanation = MatchProfileMapper.ToDto(r.Explanation)
                };
            })
            .ToList();

        return Result<IReadOnlyList<ApplicantDto>>.Success(result);
    }

    public async Task<Result<ApplicationDetailDto>> ChangeStatusAsync(
        int applicationId, ChangeStatusRequest request, int staffUserId, CancellationToken ct = default)
    {
        var application = await _context.JobApplications
            .Include(a => a.JobPosting)
            .Include(a => a.CandidateProfile).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct);

        if (application is null)
        {
            return Result<ApplicationDetailDto>.Failure("Application not found.");
        }

        var organizationId = await _context.Users
            .Where(u => u.Id == staffUserId)
            .Select(u => u.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (application.JobPosting.OrganizationId != organizationId)
        {
            return Result<ApplicationDetailDto>.Failure("This application belongs to another organisation.");
        }

        var transitionError = ValidateTransition(application.Status, request.Status);
        if (transitionError is not null)
        {
            return Result<ApplicationDetailDto>.Failure(transitionError);
        }

        var previous = application.Status;
        application.Status = request.Status;

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _context.ApplicationEvents.Add(new ApplicationEvent
            {
                JobApplicationId = application.Id,
                FromStatus = previous,
                ToStatus = request.Status,
                Note = request.Note,
                ActorUserId = staffUserId
            });

            if (application.CandidateProfile.UserId != 0)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = application.CandidateProfile.UserId,
                    Channel = NotificationChannel.Email,
                    Subject = $"Update on your application for {application.JobPosting.Title}",
                    Body = $"Your application status changed from {previous} to {request.Status}. {request.Note}".Trim(),
                    RelatedEntityName = nameof(JobApplication),
                    RelatedEntityId = application.Id
                });
            }

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = staffUserId,
                Action = $"ApplicationStatus{request.Status}",
                EntityName = nameof(JobApplication),
                EntityId = application.Id.ToString(),
                Details = $"{previous} to {request.Status}"
            });

            await Task.CompletedTask;
        }, ct);

        var refreshed = await DetailQuery().FirstAsync(a => a.Id == applicationId, ct);
        return Result<ApplicationDetailDto>.Success(ToDetail(refreshed));
    }

    public async Task<Result<ApplicationDetailDto>> WithdrawAsync(int applicationId, int candidateUserId, CancellationToken ct = default)
    {
        var application = await _context.JobApplications
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct);

        if (application is null || application.CandidateProfile.UserId != candidateUserId)
        {
            return Result<ApplicationDetailDto>.Failure("Application not found.");
        }

        var transitionError = ValidateTransition(application.Status, ApplicationStatus.Withdrawn);
        if (transitionError is not null)
        {
            return Result<ApplicationDetailDto>.Failure(transitionError);
        }

        var previous = application.Status;
        application.Status = ApplicationStatus.Withdrawn;

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _context.ApplicationEvents.Add(new ApplicationEvent
            {
                JobApplicationId = application.Id,
                FromStatus = previous,
                ToStatus = ApplicationStatus.Withdrawn,
                Note = "Withdrawn by the candidate.",
                ActorUserId = candidateUserId
            });

            await Task.CompletedTask;
        }, ct);

        return await GetForCandidateAsync(applicationId, candidateUserId, ct);
    }

    public async Task<IReadOnlyList<JobRecommendationDto>> RecommendJobsAsync(
        int candidateUserId, int take = 10, CancellationToken ct = default)
    {
        var profile = await LoadCandidateAsync(candidateUserId, ct);
        if (profile is null)
        {
            return Array.Empty<JobRecommendationDto>();
        }

        var openJobs = await JobQuery()
            .Where(j => j.Status == JobStatus.Published)
            .Where(j => j.ClosingDate == null || j.ClosingDate >= DateTime.UtcNow)
            .ToListAsync(ct);

        if (openJobs.Count == 0)
        {
            return Array.Empty<JobRecommendationDto>();
        }

        var appliedJobIds = await _context.JobApplications
            .Where(a => a.CandidateProfileId == profile.Id)
            .Select(a => a.JobPostingId)
            .ToListAsync(ct);

        var resume = profile.Resumes.FirstOrDefault(r => r.IsPrimary) ?? profile.Resumes.FirstOrDefault();
        var snapshot = MatchProfileMapper.ToCandidateSnapshot(profile, resume);

        var profiles = openJobs.Select(MatchProfileMapper.ToJobProfile).ToList();
        var recommendations = _engine.RecommendJobs(snapshot, profiles, take);

        var jobsById = openJobs.ToDictionary(j => j.Id);

        return recommendations
            .Where(r => jobsById.ContainsKey(r.JobId))
            .Select(r =>
            {
                var job = jobsById[r.JobId];
                return new JobRecommendationDto
                {
                    JobPostingId = job.Id,
                    Title = job.Title,
                    OrganizationName = job.Organization?.Name ?? string.Empty,
                    City = job.City,
                    Country = job.Country,
                    WorkMode = job.WorkMode,
                    AlreadyApplied = appliedJobIds.Contains(job.Id),
                    Explanation = MatchProfileMapper.ToDto(r.Explanation)
                };
            })
            .ToList();
    }

    // -----------------------------------------------------------------------
    // Internals
    // -----------------------------------------------------------------------

    private static string? ValidateTransition(ApplicationStatus from, ApplicationStatus to)
    {
        if (from == to)
        {
            return $"The application is already {from}.";
        }

        if (!AllowedTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
        {
            return $"An application cannot move from {from} to {to}.";
        }

        return null;
    }

    private IQueryable<JobPosting> JobQuery() => _context.JobPostings
        .Include(j => j.Organization)
        .Include(j => j.RequiredSkills).ThenInclude(s => s.Skill);

    private IQueryable<JobApplication> DetailQuery() => _context.JobApplications
        .AsNoTracking()
        .Include(a => a.JobPosting).ThenInclude(j => j.Organization)
        .Include(a => a.CandidateProfile)
        .Include(a => a.Events).ThenInclude(e => e.ActorUser);

    private async Task<CandidateProfile?> LoadCandidateAsync(int userId, CancellationToken ct) =>
        await _context.CandidateProfiles
            .Include(c => c.User)
            .Include(c => c.Resumes).ThenInclude(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

    private static ApplicationSummaryDto ToSummary(JobApplication application) => new()
    {
        Id = application.Id,
        JobPostingId = application.JobPostingId,
        JobTitle = application.JobPosting?.Title ?? string.Empty,
        OrganizationName = application.JobPosting?.Organization?.Name ?? string.Empty,
        City = application.JobPosting?.City ?? string.Empty,
        Country = application.JobPosting?.Country ?? string.Empty,
        Status = application.Status,
        SubmittedAt = application.SubmittedAt,
        MatchScore = application.MatchScore,
        MatchSummary = DeserialiseExplanation(application.ScoreBreakdownJson)?.Summary
    };

    private static ApplicationDetailDto ToDetail(JobApplication application) => new()
    {
        Id = application.Id,
        JobPostingId = application.JobPostingId,
        JobTitle = application.JobPosting?.Title ?? string.Empty,
        OrganizationName = application.JobPosting?.Organization?.Name ?? string.Empty,
        City = application.JobPosting?.City ?? string.Empty,
        Country = application.JobPosting?.Country ?? string.Empty,
        Status = application.Status,
        SubmittedAt = application.SubmittedAt,
        MatchScore = application.MatchScore,
        CoverLetter = application.CoverLetter,
        Explanation = DeserialiseExplanation(application.ScoreBreakdownJson),
        MatchSummary = DeserialiseExplanation(application.ScoreBreakdownJson)?.Summary,
        Timeline = application.Events?
            .OrderBy(e => e.OccurredAt)
            .Select(e => new ApplicationEventDto(e.FromStatus, e.ToStatus, e.Note, e.ActorUser?.FullName, e.OccurredAt))
            .ToList() ?? new List<ApplicationEventDto>()
    };

    private static MatchExplanationDto? DeserialiseExplanation(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<MatchExplanationDto>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // A breakdown written by an older shape of the DTO must not break the
            // whole response. The score itself is stored separately and survives.
            return null;
        }
    }
}
