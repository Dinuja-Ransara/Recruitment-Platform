using Meridian.Application.Common.Interfaces;
using Meridian.Application.Dtos.Analytics;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Api.Controllers;

/// <summary>
/// Recruitment analytics.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
[Authorize(Policy = "RecruitingStaff")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    private readonly ICurrentUser _currentUser;
    private readonly MeridianDbContext _context;

    public AnalyticsController(IAnalyticsService analytics, ICurrentUser currentUser, MeridianDbContext context)
    {
        _analytics = analytics;
        _currentUser = currentUser;
        _context = context;
    }

    /// <summary>
    /// Analytics for the caller's own organisation. An administrator sees every
    /// organisation, since oversight is the purpose of the role.
    /// </summary>
    [HttpGet("recruitment")]
    [ProducesResponseType(typeof(RecruitmentAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Recruitment(CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        int? organizationId = null;
        if (!_currentUser.IsInRole(Role.Administrator))
        {
            organizationId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.OrganizationId)
                .FirstOrDefaultAsync(ct);
        }

        return Ok(await _analytics.GetForOrganizationAsync(organizationId, ct));
    }
}
