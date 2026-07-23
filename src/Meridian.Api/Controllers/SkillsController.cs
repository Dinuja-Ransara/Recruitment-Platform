using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// The skill taxonomy. Read-only: skills are reference data that the matching
/// engine resolves against, not something a recruiter invents per posting, which
/// is what keeps two spellings of the same skill from scoring differently.
/// </summary>
[ApiController]
[Route("api/skills")]
[Produces("application/json")]
public class SkillsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SkillsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>Lists the taxonomy, ordered by category then name.</summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var skills = await _unitOfWork.Repository<Skill>().ListAsync(ct);

        return Ok(skills
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Category }));
    }
}
