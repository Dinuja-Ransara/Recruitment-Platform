using Meridian.Application.Common.Interfaces;
using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Jobs;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// Job posting lifecycle: draft, publish, pause, close, and duplication.
///
/// Authorisation here is ownership-scoped rather than role-scoped. The API has
/// already established that the caller is a recruiter; this layer additionally
/// checks that the posting belongs to the recruiter's own organisation, so one
/// client's recruiter cannot edit another client's vacancy.
/// </summary>
public class JobService : IJobService
{
    private readonly MeridianDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public JobService(MeridianDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<JobSummaryDto>> SearchAsync(JobSearchQuery query, CancellationToken ct = default)
    {
        var baseQuery = BaseQuery()
            .Where(j => j.Status == JobStatus.Published)
            .Where(j => j.ClosingDate == null || j.ClosingDate >= DateTime.UtcNow);

        return await PageAsync(ApplyFilters(baseQuery, query), query, ct);
    }

    public async Task<PagedResult<JobSummaryDto>> ListForRecruiterAsync(int recruiterUserId, JobSearchQuery query, CancellationToken ct = default)
    {
        var organizationId = await GetOrganizationIdAsync(recruiterUserId, ct);

        var baseQuery = BaseQuery().Where(j => j.OrganizationId == organizationId);
        return await PageAsync(ApplyFilters(baseQuery, query), query, ct);
    }

    public async Task<Result<JobDetailDto>> GetByIdAsync(int id, bool includeUnpublished, CancellationToken ct = default)
    {
        var job = await BaseQuery().FirstOrDefaultAsync(j => j.Id == id, ct);

        if (job is null)
        {
            return Result<JobDetailDto>.Failure("Job posting not found.");
        }

        if (!includeUnpublished && job.Status != JobStatus.Published)
        {
            // Unpublished postings are indistinguishable from missing ones to the
            // public, so a draft title cannot be discovered by walking ids.
            return Result<JobDetailDto>.Failure("Job posting not found.");
        }

        return Result<JobDetailDto>.Success(ToDetail(job));
    }

    public async Task<Result<JobDetailDto>> CreateAsync(CreateJobRequest request, int recruiterUserId, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(request, ct);
        if (validation is not null)
        {
            return Result<JobDetailDto>.Failure(validation);
        }

        var job = new JobPosting
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Responsibilities = request.Responsibilities,
            OrganizationId = request.OrganizationId,
            DepartmentId = request.DepartmentId,
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            WorkMode = request.WorkMode,
            EmploymentType = request.EmploymentType,
            Seniority = request.Seniority,
            MinYearsExperience = request.MinYearsExperience,
            RequiredEducation = request.RequiredEducation,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Currency = request.Currency,
            ClosingDate = request.ClosingDate,
            RankingStrategy = request.RankingStrategy,
            PostedByUserId = recruiterUserId,
            Status = JobStatus.Draft
        };

        foreach (var skill in request.Skills)
        {
            job.RequiredSkills.Add(new JobRequiredSkill
            {
                SkillId = skill.SkillId,
                IsMandatory = skill.IsMandatory,
                Weight = skill.Weight,
                MinYearsExperience = skill.MinYearsExperience
            });
        }

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _context.JobPostings.Add(job);
            await _context.SaveChangesAsync(token);
            await AuditAsync(recruiterUserId, "JobCreated", job.Id, token);
        }, ct);

        return await GetByIdAsync(job.Id, includeUnpublished: true, ct);
    }

    public async Task<Result<JobDetailDto>> UpdateAsync(int id, UpdateJobRequest request, int recruiterUserId, CancellationToken ct = default)
    {
        var job = await _context.JobPostings
            .Include(j => j.RequiredSkills)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        var ownership = await CheckOwnershipAsync(job, recruiterUserId, ct);
        if (ownership is not null)
        {
            return Result<JobDetailDto>.Failure(ownership);
        }

        var validation = await ValidateAsync(request, ct);
        if (validation is not null)
        {
            return Result<JobDetailDto>.Failure(validation);
        }

        job!.Title = request.Title.Trim();
        job.Description = request.Description;
        job.Responsibilities = request.Responsibilities;
        job.DepartmentId = request.DepartmentId;
        job.City = request.City.Trim();
        job.Country = request.Country.Trim();
        job.WorkMode = request.WorkMode;
        job.EmploymentType = request.EmploymentType;
        job.Seniority = request.Seniority;
        job.MinYearsExperience = request.MinYearsExperience;
        job.RequiredEducation = request.RequiredEducation;
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.Currency = request.Currency;
        job.ClosingDate = request.ClosingDate;
        job.RankingStrategy = request.RankingStrategy;
        job.Status = request.Status;

        // Skill requirements are replaced wholesale rather than diffed. The set is
        // small and the write is transactional, so the simpler code wins.
        _context.JobRequiredSkills.RemoveRange(job.RequiredSkills);
        foreach (var skill in request.Skills)
        {
            job.RequiredSkills.Add(new JobRequiredSkill
            {
                JobPostingId = job.Id,
                SkillId = skill.SkillId,
                IsMandatory = skill.IsMandatory,
                Weight = skill.Weight,
                MinYearsExperience = skill.MinYearsExperience
            });
        }

        await _unitOfWork.ExecuteInTransactionAsync(
            token => AuditAsync(recruiterUserId, "JobUpdated", job.Id, token), ct);

        return await GetByIdAsync(job.Id, includeUnpublished: true, ct);
    }

    public Task<Result<JobDetailDto>> PublishAsync(int id, int recruiterUserId, CancellationToken ct = default)
        => TransitionAsync(id, recruiterUserId, JobStatus.Published, "JobPublished", ct);

    public Task<Result<JobDetailDto>> CloseAsync(int id, int recruiterUserId, CancellationToken ct = default)
        => TransitionAsync(id, recruiterUserId, JobStatus.Closed, "JobClosed", ct);

    public async Task<Result<JobDetailDto>> DuplicateAsync(int id, int recruiterUserId, CancellationToken ct = default)
    {
        var original = await _context.JobPostings
            .Include(j => j.RequiredSkills)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        var ownership = await CheckOwnershipAsync(original, recruiterUserId, ct);
        if (ownership is not null)
        {
            return Result<JobDetailDto>.Failure(ownership);
        }

        // Prototype pattern. The entity knows how to copy itself, including its
        // skill requirements, and deliberately returns a Draft with no identity,
        // no publication date and none of the original's applications.
        var copy = original!.Clone();
        copy.Title = $"{original.Title} (copy)";
        copy.PostedByUserId = recruiterUserId;

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _context.JobPostings.Add(copy);
            await _context.SaveChangesAsync(token);
            await AuditAsync(recruiterUserId, "JobDuplicated", copy.Id, token);
        }, ct);

        return await GetByIdAsync(copy.Id, includeUnpublished: true, ct);
    }

    // -----------------------------------------------------------------------
    // Internals
    // -----------------------------------------------------------------------

    private IQueryable<JobPosting> BaseQuery() => _context.JobPostings
        .AsNoTracking()
        .Include(j => j.Organization)
        .Include(j => j.Department)
        .Include(j => j.PostedBy)
        .Include(j => j.RequiredSkills).ThenInclude(s => s.Skill)
        .Include(j => j.Applications);

    private static IQueryable<JobPosting> ApplyFilters(IQueryable<JobPosting> source, JobSearchQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            source = source.Where(j =>
                EF.Functions.Like(j.Title, $"%{keyword}%") ||
                EF.Functions.Like(j.Description, $"%{keyword}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Country))
        {
            source = source.Where(j => j.Country == query.Country);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            source = source.Where(j => j.City == query.City);
        }

        if (query.WorkMode.HasValue)
        {
            source = source.Where(j => j.WorkMode == query.WorkMode.Value);
        }

        if (query.EmploymentType.HasValue)
        {
            source = source.Where(j => j.EmploymentType == query.EmploymentType.Value);
        }

        if (query.Seniority.HasValue)
        {
            source = source.Where(j => j.Seniority == query.Seniority.Value);
        }

        if (query.OrganizationId.HasValue)
        {
            source = source.Where(j => j.OrganizationId == query.OrganizationId.Value);
        }

        if (query.MaxYearsExperience.HasValue)
        {
            // Candidates filter by what they can actually meet, so this compares
            // against the posting's minimum requirement rather than a maximum.
            source = source.Where(j => j.MinYearsExperience <= query.MaxYearsExperience.Value);
        }

        return source;
    }

    private static async Task<PagedResult<JobSummaryDto>> PageAsync(
        IQueryable<JobPosting> source, JobSearchQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var total = await source.CountAsync(ct);
        var items = await source
            .OrderByDescending(j => j.PublishedAt ?? j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<JobSummaryDto>
        {
            Items = items.Select(ToSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    private async Task<Result<JobDetailDto>> TransitionAsync(
        int id, int recruiterUserId, JobStatus target, string auditAction, CancellationToken ct)
    {
        var job = await _context.JobPostings.FirstOrDefaultAsync(j => j.Id == id, ct);

        var ownership = await CheckOwnershipAsync(job, recruiterUserId, ct);
        if (ownership is not null)
        {
            return Result<JobDetailDto>.Failure(ownership);
        }

        if (target == JobStatus.Published)
        {
            if (job!.Status == JobStatus.Closed)
            {
                return Result<JobDetailDto>.Failure("A closed posting cannot be republished. Duplicate it instead.");
            }

            job.PublishedAt ??= DateTime.UtcNow;
        }

        job!.Status = target;

        await _unitOfWork.ExecuteInTransactionAsync(
            token => AuditAsync(recruiterUserId, auditAction, job.Id, token), ct);

        return await GetByIdAsync(job.Id, includeUnpublished: true, ct);
    }

    private async Task<string?> ValidateAsync(CreateJobRequest request, CancellationToken ct)
    {
        if (request.SalaryMin.HasValue && request.SalaryMax.HasValue && request.SalaryMin > request.SalaryMax)
        {
            return "The minimum salary cannot exceed the maximum.";
        }

        if (request.ClosingDate.HasValue && request.ClosingDate < DateTime.UtcNow.Date)
        {
            return "The closing date cannot be in the past.";
        }

        if (!await _context.Organizations.AnyAsync(o => o.Id == request.OrganizationId, ct))
        {
            return "The specified organisation does not exist.";
        }

        if (request.DepartmentId.HasValue &&
            !await _context.Departments.AnyAsync(
                d => d.Id == request.DepartmentId && d.OrganizationId == request.OrganizationId, ct))
        {
            return "The specified department does not belong to that organisation.";
        }

        var skillIds = request.Skills.Select(s => s.SkillId).ToList();
        if (skillIds.Distinct().Count() != skillIds.Count)
        {
            return "The same skill was listed more than once.";
        }

        if (skillIds.Count > 0)
        {
            var known = await _context.Skills.CountAsync(s => skillIds.Contains(s.Id), ct);
            if (known != skillIds.Count)
            {
                return "One or more of the listed skills does not exist in the taxonomy.";
            }
        }

        return null;
    }

    private async Task<string?> CheckOwnershipAsync(JobPosting? job, int recruiterUserId, CancellationToken ct)
    {
        if (job is null)
        {
            return "Job posting not found.";
        }

        var organizationId = await GetOrganizationIdAsync(recruiterUserId, ct);
        return job.OrganizationId == organizationId
            ? null
            : "This posting belongs to another organisation.";
    }

    private async Task<int?> GetOrganizationIdAsync(int userId, CancellationToken ct)
        => await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.OrganizationId)
            .FirstOrDefaultAsync(ct);

    private async Task AuditAsync(int userId, string action, int jobId, CancellationToken ct)
        => await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = nameof(JobPosting),
            EntityId = jobId.ToString()
        }, ct);

    private static JobSummaryDto ToSummary(JobPosting job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        OrganizationName = job.Organization?.Name ?? string.Empty,
        DepartmentName = job.Department?.Name,
        City = job.City,
        Country = job.Country,
        WorkMode = job.WorkMode,
        EmploymentType = job.EmploymentType,
        Seniority = job.Seniority,
        MinYearsExperience = job.MinYearsExperience,
        SalaryMin = job.SalaryMin,
        SalaryMax = job.SalaryMax,
        Currency = job.Currency,
        Status = job.Status,
        PublishedAt = job.PublishedAt,
        ClosingDate = job.ClosingDate,
        ApplicationCount = job.Applications?.Count ?? 0,
        RequiredSkills = job.RequiredSkills?
            .OrderByDescending(s => s.IsMandatory).ThenByDescending(s => s.Weight)
            .Select(s => s.Skill?.Name ?? string.Empty)
            .ToList() ?? new List<string>()
    };

    private static JobDetailDto ToDetail(JobPosting job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        OrganizationName = job.Organization?.Name ?? string.Empty,
        DepartmentName = job.Department?.Name,
        City = job.City,
        Country = job.Country,
        WorkMode = job.WorkMode,
        EmploymentType = job.EmploymentType,
        Seniority = job.Seniority,
        MinYearsExperience = job.MinYearsExperience,
        SalaryMin = job.SalaryMin,
        SalaryMax = job.SalaryMax,
        Currency = job.Currency,
        Status = job.Status,
        PublishedAt = job.PublishedAt,
        ClosingDate = job.ClosingDate,
        ApplicationCount = job.Applications?.Count ?? 0,
        RequiredSkills = job.RequiredSkills?
            .OrderByDescending(s => s.IsMandatory).ThenByDescending(s => s.Weight)
            .Select(s => s.Skill?.Name ?? string.Empty)
            .ToList() ?? new List<string>(),
        Description = job.Description,
        Responsibilities = job.Responsibilities,
        RequiredEducation = job.RequiredEducation,
        RankingStrategy = job.RankingStrategy,
        PostedByName = job.PostedBy?.FullName ?? string.Empty,
        SkillRequirements = job.RequiredSkills?
            .OrderByDescending(s => s.IsMandatory).ThenByDescending(s => s.Weight)
            .Select(s => new JobSkillRequirementDto
            {
                SkillId = s.SkillId,
                SkillName = s.Skill?.Name ?? string.Empty,
                IsMandatory = s.IsMandatory,
                Weight = s.Weight,
                MinYearsExperience = s.MinYearsExperience
            })
            .ToList() ?? new List<JobSkillRequirementDto>()
    };
}
