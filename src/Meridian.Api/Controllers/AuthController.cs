using Meridian.Application.Common.Interfaces;
using Meridian.Application.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// Registration, login and identity of the calling user.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authentication;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthenticationService authentication, ICurrentUser currentUser)
    {
        _authentication = authentication;
        _currentUser = currentUser;
    }

    /// <summary>Authenticates a user and issues a JWT access token.</summary>
    /// <response code="200">Authentication succeeded, token returned.</response>
    /// <response code="401">Email address or password was incorrect.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authentication.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

        return result.Succeeded
            ? Ok(result.Value)
            : Unauthorized(new ProblemDetails { Title = "Authentication failed", Detail = result.Error, Status = 401 });
    }

    /// <summary>Registers a new candidate account and signs them straight in.</summary>
    /// <response code="201">Account created, token returned.</response>
    /// <response code="409">That email address is already registered.</response>
    [HttpPost("register/candidate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterCandidate([FromBody] RegisterCandidateRequest request, CancellationToken ct)
    {
        var result = await _authentication.RegisterCandidateAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : Conflict(new ProblemDetails { Title = "Registration failed", Detail = result.Error, Status = 409 });
    }

    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await _authentication.GetCurrentUserAsync(userId, ct);
        return result.Succeeded ? Ok(result.Value) : NotFound(result.Error);
    }
}
