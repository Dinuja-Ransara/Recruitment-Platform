using Meridian.Application.Dtos.Jobs;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Infrastructure.Persistence.Repositories;
using Meridian.Infrastructure.Services;

namespace Meridian.Infrastructure.Tests;

/// <summary>
/// Exercises JobService against a real (SQLite-backed) database. The
/// ownership tests here cover the exact scenario found during manual API
/// testing: creating a posting under an organisation the recruiter does not
/// belong to succeeds, but every later action on it correctly fails, because
/// only the later actions check ownership. These tests pin that behaviour
/// down so a future change either fixes it deliberately or breaks a test
/// that says so, rather than being rediscovered by hand again.
/// </summary>
public class JobServiceTests : IDisposable
{
    private readonly TestDb _db;
    private readonly JobService _sut;

    public JobServiceTests()
    {
        _db = new TestDb();
        var unitOfWork = new UnitOfWork(_db.Context);
        _sut = new JobService(_db.Context, unitOfWork);
    }

    private User SeedRecruiter(int organizationId)
    {
        var recruiter = new User
        {
            Email = $"recruiter{organizationId}@meridian.example.com",
            FullName = "Test Recruiter",
            PasswordHash = "not-checked-in-these-tests",
            IsActive = true,
            OrganizationId = organizationId
        };
        _db.Context.Users.Add(recruiter);
        _db.Context.SaveChanges();
        return recruiter;
    }

    private static CreateJobRequest ValidRequest(int organizationId) => new()
    {
        Title = "Backend Developer",
        Description = "Build and maintain backend services.",
        Responsibilities = "API design, database work",
        OrganizationId = organizationId,
        City = "Colombo",
        Country = "Sri Lanka",
        WorkMode = WorkMode.OnSite,
        EmploymentType = EmploymentType.FullTime,
        Seniority = SeniorityLevel.Mid,
        MinYearsExperience = 1,
        RequiredEducation = EducationLevel.Bachelors,
        SalaryMin = 100000,
        SalaryMax = 200000,
        Currency = "LKR"
    };

    [Fact]
    public async Task CreateAsync_WithMinSalaryAboveMaxSalary_FailsValidation()
    {
        var (_, _, org) = _db.SeedBaseline();
        var recruiter = SeedRecruiter(org.Id);

        var request = ValidRequest(org.Id) with { SalaryMin = 300000, SalaryMax = 100000 };
        var result = await _sut.CreateAsync(request, recruiter.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("minimum salary", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesPostingInDraftStatus()
    {
        var (_, _, org) = _db.SeedBaseline();
        var recruiter = SeedRecruiter(org.Id);

        var result = await _sut.CreateAsync(ValidRequest(org.Id), recruiter.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(JobStatus.Draft, result.Value!.Status);
    }

    /// <summary>
    /// This is the bug an actual Postman run surfaced: creating a job under
    /// an organisation the recruiter does not belong to is allowed, because
    /// CreateAsync only checks the organisation exists, not that the caller
    /// belongs to it.
    /// </summary>
    [Fact]
    public async Task CreateAsync_UnderAnotherOrganization_IsNotRejectedAtCreateTime()
    {
        var (_, _, homeOrg) = _db.SeedBaseline();
        var otherOrg = new Organization { Name = "Other Company", Industry = "Other", Country = "UK", City = "London" };
        _db.Context.Organizations.Add(otherOrg);
        _db.Context.SaveChanges();

        var recruiter = SeedRecruiter(homeOrg.Id);

        var result = await _sut.CreateAsync(ValidRequest(otherOrg.Id), recruiter.Id);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task PublishAsync_OnPostingBelongingToAnotherOrganization_Fails()
    {
        var (_, _, homeOrg) = _db.SeedBaseline();
        var otherOrg = new Organization { Name = "Other Company", Industry = "Other", Country = "UK", City = "London" };
        _db.Context.Organizations.Add(otherOrg);
        _db.Context.SaveChanges();

        var recruiter = SeedRecruiter(homeOrg.Id);
        var created = await _sut.CreateAsync(ValidRequest(otherOrg.Id), recruiter.Id);

        var result = await _sut.PublishAsync(created.Value!.Id, recruiter.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("another organisation", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublishAsync_OnPostingOwnedByCaller_Succeeds()
    {
        var (_, _, org) = _db.SeedBaseline();
        var recruiter = SeedRecruiter(org.Id);
        var created = await _sut.CreateAsync(ValidRequest(org.Id), recruiter.Id);

        var result = await _sut.PublishAsync(created.Value!.Id, recruiter.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(JobStatus.Published, result.Value!.Status);
    }

    [Fact]
    public async Task PublishAsync_OnAClosedPosting_CannotBeRepublished()
    {
        var (_, _, org) = _db.SeedBaseline();
        var recruiter = SeedRecruiter(org.Id);
        var created = await _sut.CreateAsync(ValidRequest(org.Id), recruiter.Id);
        await _sut.PublishAsync(created.Value!.Id, recruiter.Id);
        await _sut.CloseAsync(created.Value!.Id, recruiter.Id);

        var result = await _sut.PublishAsync(created.Value!.Id, recruiter.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("cannot be republished", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose() => _db.Dispose();
}
