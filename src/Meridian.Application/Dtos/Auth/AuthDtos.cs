using System.ComponentModel.DataAnnotations;

namespace Meridian.Application.Dtos.Auth;

public record LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public UserSummary User { get; init; } = new();
}

public record UserSummary
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public string? OrganizationName { get; init; }
}

public record RegisterCandidateRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; init; }

    [MaxLength(200)]
    public string Headline { get; init; } = string.Empty;

    [MaxLength(100)]
    public string City { get; init; } = string.Empty;

    [MaxLength(100)]
    public string Country { get; init; } = string.Empty;

    [Range(0, 60)]
    public decimal YearsOfExperience { get; init; }
}
