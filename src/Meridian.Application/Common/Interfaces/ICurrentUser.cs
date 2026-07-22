namespace Meridian.Application.Common.Interfaces;

/// <summary>
/// Gives the application layer access to the calling principal without taking a
/// dependency on HttpContext, which keeps the layer testable.
/// </summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
