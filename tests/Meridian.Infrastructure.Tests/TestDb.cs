using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Tests;

/// <summary>
/// A handful of text columns are configured with the SQL-Server-specific raw
/// type <c>nvarchar(max)</c> (see JobConfiguration, ApplicationConfiguration,
/// CandidateConfiguration, SystemConfiguration). SQLite's DDL parser does not
/// understand that syntax and fails table creation outright. Production
/// configuration is correct for SQL Server and stays untouched; this subclass
/// exists only so the test database can be created at all.
/// </summary>
internal sealed class TestMeridianDbContext : MeridianDbContext
{
    public TestMeridianDbContext(DbContextOptions<MeridianDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.GetColumnType() == "nvarchar(max)")
                {
                    property.SetColumnType(null);
                }
            }
        }
    }
}

/// <summary>
/// Builds a real <see cref="MeridianDbContext"/> against a SQLite in-memory
/// database rather than EF Core's InMemory provider.
///
/// The InMemory provider cannot open a transaction, and both
/// <c>JobService</c> and <c>AuthenticationService</c> commit through
/// <c>IUnitOfWork.ExecuteInTransactionAsync</c>, which calls
/// <c>Database.BeginTransactionAsync</c>. SQLite is a real relational engine,
/// so transactions, and the schema itself, behave the same way they would
/// against SQL Server. The connection is kept open for the lifetime of the
/// context, which is what keeps an in-memory SQLite database alive between
/// calls, closing it would drop the database.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public MeridianDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MeridianDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new TestMeridianDbContext(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// The minimum reference data most service tests need: the four roles and
    /// one organisation with a recruiter and a hiring manager already in it.
    /// Mirrors the shape <c>DatabaseSeeder</c> produces, kept intentionally
    /// smaller since a unit test should not depend on the full demo dataset.
    /// </summary>
    public (Role Candidate, Role Recruiter, Organization Org) SeedBaseline()
    {
        var candidateRole = new Role { Name = Role.Candidate, Description = "Job seeker" };
        var recruiterRole = new Role { Name = Role.Recruiter, Description = "Recruiter" };
        var managerRole = new Role { Name = Role.HiringManager, Description = "Hiring manager" };
        var adminRole = new Role { Name = Role.Administrator, Description = "Administrator" };
        Context.Roles.AddRange(candidateRole, recruiterRole, managerRole, adminRole);

        var org = new Organization
        {
            Name = "Meridian HR Consulting",
            Industry = "Human Resource Consulting",
            Country = "Sri Lanka",
            City = "Colombo"
        };
        Context.Organizations.Add(org);

        Context.SaveChanges();
        return (candidateRole, recruiterRole, org);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
