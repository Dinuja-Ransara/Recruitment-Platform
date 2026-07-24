using Meridian.Application.Dtos.Auth;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Identity;
using Meridian.Infrastructure.Persistence.Repositories;
using Meridian.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Meridian.Infrastructure.Tests;

/// <summary>
/// Exercises AuthenticationService against a real password hasher, a real JWT
/// signer, and a real (SQLite-backed) database, rather than mocks standing in
/// for all three. Login and registration are the one place in the system
/// where getting the wrong answer is a security incident, so the tests here
/// prefer the real cryptography over a faked "always succeeds" stub.
/// </summary>
public class AuthenticationServiceTests : IDisposable
{
    private readonly TestDb _db;
    private readonly AuthenticationService _sut;
    private readonly BCryptPasswordHasher _hasher = new();

    public AuthenticationServiceTests()
    {
        _db = new TestDb();
        var unitOfWork = new UnitOfWork(_db.Context);
        var tokenService = new JwtTokenService(Options.Create(new JwtSettings
        {
            Issuer = "meridian-tests",
            Audience = "meridian-tests",
            SigningKey = JwtSettings.DevelopmentPlaceholderKey,
            AccessTokenMinutes = 60
        }));

        _sut = new AuthenticationService(_db.Context, unitOfWork, _hasher, tokenService);
    }

    private User SeedUser(Role role, string email, string password, bool isActive = true)
    {
        var user = new User
        {
            Email = email,
            FullName = "Test User",
            PasswordHash = _hasher.Hash(password),
            IsActive = isActive
        };
        user.UserRoles.Add(new UserRole { Role = role });
        _db.Context.Users.Add(user);
        _db.Context.SaveChanges();
        return user;
    }

    [Fact]
    public async Task LoginAsync_WithCorrectPassword_ReturnsTokenAndRoles()
    {
        var (candidateRole, _, _) = _db.SeedBaseline();
        SeedUser(candidateRole, "real@meridian.example.com", "CorrectHorse1!");

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = "real@meridian.example.com", Password = "CorrectHorse1!" }, "127.0.0.1");

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.AccessToken));
        Assert.Contains(Role.Candidate, result.Value.User.Roles);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_FailsWithGenericMessage()
    {
        var (candidateRole, _, _) = _db.SeedBaseline();
        SeedUser(candidateRole, "real@meridian.example.com", "CorrectHorse1!");

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = "real@meridian.example.com", Password = "WrongPassword1!" }, "127.0.0.1");

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid email address or password.", result.Error);
    }

    /// <summary>
    /// The wrong-password message and the unknown-email message must be
    /// identical, character for character. If they differ, an attacker can
    /// tell which email addresses are registered just by watching which
    /// error comes back, without ever guessing a password.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithUnknownEmail_FailsWithSameMessageAsWrongPassword()
    {
        _db.SeedBaseline();

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = "nobody@meridian.example.com", Password = "WhateverPassword1!" }, "127.0.0.1");

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid email address or password.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedAccount_IsBlockedEvenWithCorrectPassword()
    {
        var (candidateRole, _, _) = _db.SeedBaseline();
        SeedUser(candidateRole, "disabled@meridian.example.com", "CorrectHorse1!", isActive: false);

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = "disabled@meridian.example.com", Password = "CorrectHorse1!" }, "127.0.0.1");

        Assert.False(result.Succeeded);
        Assert.Contains("deactivated", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterCandidateAsync_WithNewEmail_CreatesUserHoldingCandidateRole()
    {
        _db.SeedBaseline();

        var result = await _sut.RegisterCandidateAsync(new RegisterCandidateRequest
        {
            Email = "new.candidate@meridian.example.com",
            Password = "CorrectHorse1!",
            FullName = "New Candidate",
            Headline = "Backend Engineer",
            City = "Colombo",
            Country = "Sri Lanka",
            YearsOfExperience = 2
        }, "127.0.0.1");

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.User.Roles);
        Assert.Equal(Role.Candidate, result.Value.User.Roles[0]);
    }

    [Fact]
    public async Task RegisterCandidateAsync_WithEmailAlreadyRegistered_Fails()
    {
        var (candidateRole, _, _) = _db.SeedBaseline();
        SeedUser(candidateRole, "taken@meridian.example.com", "CorrectHorse1!");

        var result = await _sut.RegisterCandidateAsync(new RegisterCandidateRequest
        {
            Email = "taken@meridian.example.com",
            Password = "AnotherPassword1!",
            FullName = "Someone Else",
            Headline = "Analyst",
            City = "Colombo",
            Country = "Sri Lanka",
            YearsOfExperience = 1
        }, "127.0.0.1");

        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose() => _db.Dispose();
}
