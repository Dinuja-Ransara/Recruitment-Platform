using Meridian.Domain.Entities;

namespace Meridian.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Issues a signed JWT carrying the user's identity and every role they hold.
    /// Roles are emitted as individual role claims so that ASP.NET Core's
    /// authorisation policies can evaluate them without a database round trip.
    /// </summary>
    TokenResult CreateToken(User user, IEnumerable<string> roles);
}

public record TokenResult(string AccessToken, DateTime ExpiresAtUtc);
