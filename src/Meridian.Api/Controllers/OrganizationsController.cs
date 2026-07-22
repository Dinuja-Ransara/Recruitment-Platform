using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// Client organisations and their departments.
/// </summary>
[ApiController]
[Route("api/organizations")]
[Produces("application/json")]
public class OrganizationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public OrganizationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Lists every active client organisation. Restricted to recruiting staff:
    /// candidates have no reason to enumerate the consultancy's client list.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RecruitingStaff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var organizations = await _unitOfWork.Repository<Organization>()
            .FindAsync(o => o.IsActive, ct);

        var departments = await _unitOfWork.Repository<Department>().ListAsync(ct);

        var payload = organizations
            .OrderBy(o => o.Name)
            .Select(o => new
            {
                o.Id,
                o.Name,
                o.Industry,
                o.City,
                o.Country,
                Departments = departments
                    .Where(d => d.OrganizationId == o.Id)
                    .OrderBy(d => d.Name)
                    .Select(d => new { d.Id, d.Name, d.CostCentre })
            });

        return Ok(payload);
    }
}
