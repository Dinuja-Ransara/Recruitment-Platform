using System.Security.Claims;
using Meridian.Application.Common.Interfaces;

namespace Meridian.Api.Services;

/// <summary>
/// Reads the calling principal out of the JWT claims on the current request.
///
/// This is the only place HttpContext is touched on behalf of the application
/// layer, which is what allows services to depend on ICurrentUser and be unit
/// tested with a stub rather than a mocked HTTP pipeline.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
