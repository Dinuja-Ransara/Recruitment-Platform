using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// Administration portal endpoints: system monitoring and the audit trail.
/// </summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
[Authorize(Policy = "AdministratorOnly")]
public class AdminController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// System monitoring snapshot: record counts across the core tables plus the
    /// most recent security events. Backs the administrator dashboard.
    /// </summary>
    [HttpGet("system-health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SystemHealth(CancellationToken ct)
    {
        var recentAudit = _unitOfWork.Repository<AuditLog>().Query()
            .OrderByDescending(a => a.OccurredAt)
            .Take(10)
            .Select(a => new { a.Action, a.EntityName, a.EntityId, a.IpAddress, a.OccurredAt })
            .ToList();

        return Ok(new
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Counts = new
            {
                Users = await _unitOfWork.Repository<User>().CountAsync(ct: ct),
                Roles = await _unitOfWork.Repository<Role>().CountAsync(ct: ct),
                Organizations = await _unitOfWork.Repository<Organization>().CountAsync(ct: ct),
                Departments = await _unitOfWork.Repository<Department>().CountAsync(ct: ct),
                Skills = await _unitOfWork.Repository<Skill>().CountAsync(ct: ct),
                SkillAliases = await _unitOfWork.Repository<SkillAlias>().CountAsync(ct: ct),
                CandidateProfiles = await _unitOfWork.Repository<CandidateProfile>().CountAsync(ct: ct),
                JobPostings = await _unitOfWork.Repository<JobPosting>().CountAsync(ct: ct),
                JobApplications = await _unitOfWork.Repository<JobApplication>().CountAsync(ct: ct),
                AuditLogEntries = await _unitOfWork.Repository<AuditLog>().CountAsync(ct: ct)
            },
            RecentSecurityEvents = recentAudit
        });
    }
}
