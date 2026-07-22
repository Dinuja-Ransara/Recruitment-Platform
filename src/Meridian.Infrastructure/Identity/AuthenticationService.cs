using Meridian.Application.Common.Interfaces;
using Meridian.Application.Common.Models;
using Meridian.Application.Dtos.Auth;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Identity;

/// <summary>
/// Registration and login.
///
/// Lives in the infrastructure layer because it depends on Entity Framework's
/// eager loading to fetch a user together with their roles in one round trip.
/// The API layer only ever sees IAuthenticationService.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly MeridianDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthenticationService(
        MeridianDbContext context,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        // The same message is returned whether the account is missing or the
        // password is wrong, so the endpoint cannot be used to enumerate which
        // email addresses are registered.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await WriteAuditAsync(user?.Id, "LoginFailed", "User", user?.Id.ToString(), ipAddress, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<LoginResponse>.Failure("Invalid email address or password.");
        }

        if (!user.IsActive)
        {
            await WriteAuditAsync(user.Id, "LoginBlockedInactive", "User", user.Id.ToString(), ipAddress, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<LoginResponse>.Failure("This account has been deactivated. Contact an administrator.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await WriteAuditAsync(user.Id, "LoginSucceeded", "User", user.Id.ToString(), ipAddress, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<LoginResponse>.Success(BuildResponse(user));
    }

    public async Task<Result<LoginResponse>> RegisterCandidateAsync(RegisterCandidateRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email == email, ct))
        {
            return Result<LoginResponse>.Failure("An account already exists for that email address.");
        }

        var candidateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Role.Candidate, ct);
        if (candidateRole is null)
        {
            return Result<LoginResponse>.Failure("The Candidate role is not configured. Seed the database first.");
        }

        // Creating the user, their role assignment, their candidate profile and the
        // audit entry is one logical operation, so all four commit together or not
        // at all. The hash is computed before entering the transaction because
        // BCrypt at work factor 12 is deliberately slow and there is no reason to
        // hold a database transaction open while it runs.
        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            IsActive = true,
            CandidateProfile = new CandidateProfile
            {
                Headline = request.Headline.Trim(),
                City = request.City.Trim(),
                Country = request.Country.Trim(),
                YearsOfExperience = request.YearsOfExperience
            }
        };

        user.UserRoles.Add(new UserRole { RoleId = candidateRole.Id });

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(token);

            await WriteAuditAsync(user.Id, "CandidateRegistered", "User", user.Id.ToString(), ipAddress, token);
        }, ct);

        var created = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Organization)
            .FirstAsync(u => u.Id == user.Id, ct);

        return Result<LoginResponse>.Success(BuildResponse(created));
    }

    public async Task<Result<UserSummary>> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return user is null
            ? Result<UserSummary>.Failure("User not found.")
            : Result<UserSummary>.Success(BuildSummary(user));
    }

    private LoginResponse BuildResponse(User user)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _tokenService.CreateToken(user, roles);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = BuildSummary(user)
        };
    }

    private static UserSummary BuildSummary(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
        OrganizationName = user.Organization?.Name
    };

    private async Task WriteAuditAsync(int? userId, string action, string entity, string? entityId, string? ip, CancellationToken ct)
    {
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entity,
            EntityId = entityId,
            IpAddress = ip
        }, ct);
    }
}
