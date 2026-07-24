using Meridian.Application.Common.Interfaces;
using Meridian.Application.Dtos.Resumes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// Resume upload and storage. Files are held in Cloudflare R2, never on the
/// API's own disk, so the API can be scaled or redeployed without carrying
/// uploaded files with it.
/// </summary>
[ApiController]
[Route("api/resumes")]
[Produces("application/json")]
[Authorize(Policy = "CandidateOnly")]
public class ResumesController : ControllerBase
{
    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private readonly IResumeService _resumes;
    private readonly ICurrentUser _currentUser;

    public ResumesController(IResumeService resumes, ICurrentUser currentUser)
    {
        _resumes = resumes;
        _currentUser = currentUser;
    }

    /// <summary>Uploads a CV. The first one uploaded becomes the candidate's primary resume.</summary>
    /// <response code="201">Uploaded and stored.</response>
    /// <response code="400">Rejected: wrong file type, too large, or no candidate profile.</response>
    [HttpPost]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ResumeSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(Problem400("No file was provided."));
        }

        await using var stream = file.OpenReadStream();
        var result = await _resumes.UploadAsync(userId, file.FileName, file.ContentType, file.Length, stream, ct);

        return result.Succeeded
            ? CreatedAtAction(nameof(List), null, result.Value)
            : BadRequest(Problem400(result.Error));
    }

    /// <summary>The calling candidate's own resumes, most recent first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResumeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _resumes.ListForCandidateAsync(userId, ct));
    }

    /// <summary>Deletes one of the calling candidate's own resumes.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _resumes.DeleteAsync(id, userId, ct);
        return result.Succeeded ? NoContent() : BadRequest(Problem400(result.Error));
    }

    private static ProblemDetails Problem400(string? detail) =>
        new() { Title = "Request could not be completed", Detail = detail, Status = 400 };
}
