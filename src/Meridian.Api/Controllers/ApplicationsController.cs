using Meridian.Application.Common.Interfaces;
using Meridian.Application.Dtos.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// The application pipeline. Candidates submit and track; recruiting staff rank
/// and move applications through the hiring stages.
/// </summary>
[ApiController]
[Route("api/applications")]
[Produces("application/json")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applications;
    private readonly ICurrentUser _currentUser;

    public ApplicationsController(IApplicationService applications, ICurrentUser currentUser)
    {
        _applications = applications;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Submits an application. The matching engine scores it on the spot, so the
    /// response already carries the full explanation.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CandidateOnly")]
    [ProducesResponseType(typeof(ApplicationDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apply([FromBody] ApplyRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _applications.ApplyAsync(request, userId, ct);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : BadRequest(Problem(result.Error, 400));
    }

    /// <summary>The calling candidate's applications, most recent first.</summary>
    [HttpGet("mine")]
    [Authorize(Policy = "CandidateOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _applications.ListForCandidateAsync(userId, ct));
    }

    /// <summary>One of the calling candidate's applications, with its timeline.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "CandidateOnly")]
    [ProducesResponseType(typeof(ApplicationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMine(int id, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _applications.GetForCandidateAsync(id, userId, ct);
        return result.Succeeded ? Ok(result.Value) : NotFound(Problem(result.Error, 404));
    }

    /// <summary>
    /// The ranked applicant pool for a posting. Scores are recomputed across the
    /// whole pool on each call, because term rarity is measured against the
    /// applicants actually present.
    /// </summary>
    [HttpGet("job/{jobPostingId:int}/ranked")]
    [Authorize(Policy = "RecruitingStaff")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ranked(int jobPostingId, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _applications.RankApplicantsAsync(jobPostingId, userId, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(Problem(result.Error, 400));
    }

    /// <summary>Moves an application to the next stage of the pipeline.</summary>
    [HttpPost("{id:int}/status")]
    [Authorize(Policy = "RecruitingStaff")]
    [ProducesResponseType(typeof(ApplicationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _applications.ChangeStatusAsync(id, request, userId, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(Problem(result.Error, 400));
    }

    /// <summary>Withdraws the calling candidate's own application.</summary>
    [HttpPost("{id:int}/withdraw")]
    [Authorize(Policy = "CandidateOnly")]
    [ProducesResponseType(typeof(ApplicationDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Withdraw(int id, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _applications.WithdrawAsync(id, userId, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(Problem(result.Error, 400));
    }

    /// <summary>Postings recommended to the calling candidate, each with its reasoning.</summary>
    [HttpGet("recommendations")]
    [Authorize(Policy = "CandidateOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<JobRecommendationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Recommendations([FromQuery] int take, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _applications.RecommendJobsAsync(userId, take <= 0 ? 10 : Math.Min(take, 50), ct));
    }

    private static ProblemDetails Problem(string? detail, int status) =>
        new() { Title = status == 404 ? "Not found" : "Request could not be completed", Detail = detail, Status = status };
}
