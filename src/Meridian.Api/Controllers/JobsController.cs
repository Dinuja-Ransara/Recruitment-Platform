using Meridian.Application.Common.Interfaces;
using Meridian.Application.Dtos.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// Job postings. The search and detail endpoints are open to any authenticated
/// user because candidates browse the board; everything that writes is
/// restricted to recruiting staff and additionally scoped to the caller's own
/// organisation inside the service.
/// </summary>
[ApiController]
[Route("api/jobs")]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobs;
    private readonly ICurrentUser _currentUser;

    public JobsController(IJobService jobs, ICurrentUser currentUser)
    {
        _jobs = jobs;
        _currentUser = currentUser;
    }

    /// <summary>Searches published postings. Open to anyone, including guests.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<JobSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] JobSearchQuery query, CancellationToken ct)
        => Ok(await _jobs.SearchAsync(query, ct));

    /// <summary>Lists the calling recruiter's own postings, including drafts.</summary>
    [HttpGet("mine")]
    [Authorize(Policy = "RecruitingStaff")]
    [ProducesResponseType(typeof(PagedResult<JobSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mine([FromQuery] JobSearchQuery query, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _jobs.ListForRecruiterAsync(userId, query, ct));
    }

    /// <summary>Returns one posting. Recruiting staff additionally see drafts.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var privileged = _currentUser.IsInRole(Domain.Entities.Role.Recruiter)
                         || _currentUser.IsInRole(Domain.Entities.Role.HiringManager)
                         || _currentUser.IsInRole(Domain.Entities.Role.Administrator);

        var result = await _jobs.GetByIdAsync(id, privileged, ct);
        return result.Succeeded ? Ok(result.Value) : NotFound(Problem404(result.Error));
    }

    /// <summary>Creates a posting in Draft status.</summary>
    [HttpPost]
    [Authorize(Policy = "RecruiterOnly")]
    [ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _jobs.CreateAsync(request, userId, ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(Problem400(result.Error));
    }

    /// <summary>Replaces a posting, including its full skill requirement set.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "RecruiterOnly")]
    [ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJobRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _jobs.UpdateAsync(id, request, userId, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(Problem400(result.Error));
    }

    /// <summary>Moves a posting to Published so it appears on the public board.</summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(Policy = "RecruiterOnly")]
    [ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
        => await Transition(id, ct, (service, userId) => service.PublishAsync(id, userId, ct));

    /// <summary>Closes a posting to new applications.</summary>
    [HttpPost("{id:int}/close")]
    [Authorize(Policy = "RecruiterOnly")]
    [ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
        => await Transition(id, ct, (service, userId) => service.CloseAsync(id, userId, ct));

    /// <summary>
    /// Deep-copies a posting into a new draft. Used when the same role is opened
    /// in another office, which is routine for a multinational client.
    /// </summary>
    [HttpPost("{id:int}/duplicate")]
    [Authorize(Policy = "RecruiterOnly")]
    [ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Duplicate(int id, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _jobs.DuplicateAsync(id, userId, ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(Problem400(result.Error));
    }

    private async Task<IActionResult> Transition(
        int id, CancellationToken ct, Func<IJobService, int, Task<Application.Common.Models.Result<JobDetailDto>>> operation)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await operation(_jobs, userId);
        return result.Succeeded ? Ok(result.Value) : BadRequest(Problem400(result.Error));
    }

    private static ProblemDetails Problem400(string? detail) =>
        new() { Title = "Request could not be completed", Detail = detail, Status = 400 };

    private static ProblemDetails Problem404(string? detail) =>
        new() { Title = "Not found", Detail = detail, Status = 404 };
}
