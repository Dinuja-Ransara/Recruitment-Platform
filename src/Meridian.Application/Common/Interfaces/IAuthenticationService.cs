using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Auth;

namespace Meridian.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);
    Task<Result<LoginResponse>> RegisterCandidateAsync(RegisterCandidateRequest request, string? ipAddress, CancellationToken ct = default);
    Task<Result<UserSummary>> GetCurrentUserAsync(int userId, CancellationToken ct = default);
}
