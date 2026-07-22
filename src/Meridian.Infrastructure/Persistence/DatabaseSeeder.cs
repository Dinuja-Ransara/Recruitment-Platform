using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Persistence;

/// <summary>
/// Seeds the reference data and the demonstration dataset.
///
/// The demonstration data is deliberately story-shaped rather than "test user 1":
/// three offices in different countries, real-looking postings and a hiring
/// pipeline already in motion, so that analytics screens plot a believable
/// distribution during the demonstration.
///
/// Every step is idempotent, so the seeder can run on each startup without
/// duplicating rows.
/// </summary>
public class DatabaseSeeder
{
    private readonly MeridianDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>Shared password for every seeded demonstration account.</summary>
    public const string DemoPassword = "Meridian#2026";

    public DatabaseSeeder(MeridianDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync(ct);
        await SeedOrganizationsAsync(ct);
        await SeedSkillsAsync(ct);
        await SeedUsersAsync(ct);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        if (await _context.Roles.AnyAsync(ct))
        {
            return;
        }

        _context.Roles.AddRange(
            new Role { Name = Role.Candidate, Description = "Job seeker. Manages a profile, uploads CVs and applies to postings." },
            new Role { Name = Role.Recruiter, Description = "Creates postings, screens applicants and schedules interviews." },
            new Role { Name = Role.HiringManager, Description = "Reviews shortlists, records evaluations and makes hiring decisions." },
            new Role { Name = Role.Administrator, Description = "Manages users, roles, organisations and system configuration." });

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedOrganizationsAsync(CancellationToken ct)
    {
        if (await _context.Organizations.AnyAsync(ct))
        {
            return;
        }

        var meridian = new Organization
        {
            Name = "Meridian HR Consulting",
            Industry = "Human Resource Consulting",
            Country = "Sri Lanka",
            City = "Colombo",
            Website = "https://meridian.example.com",
            Departments =
            {
                new Department { Name = "Technology Practice", CostCentre = "TECH-01" },
                new Department { Name = "Financial Services Practice", CostCentre = "FIN-01" },
                new Department { Name = "Internal Operations", CostCentre = "OPS-01" }
            }
        };

        var northwind = new Organization
        {
            Name = "Northwind Logistics",
            Industry = "Supply Chain and Logistics",
            Country = "Singapore",
            City = "Singapore",
            Departments =
            {
                new Department { Name = "Engineering", CostCentre = "ENG-SG" },
                new Department { Name = "Operations", CostCentre = "OPS-SG" }
            }
        };

        var halcyon = new Organization
        {
            Name = "Halcyon Financial Group",
            Industry = "Banking and Insurance",
            Country = "United Kingdom",
            City = "London",
            Departments =
            {
                new Department { Name = "Digital Banking", CostCentre = "DB-UK" },
                new Department { Name = "Risk and Compliance", CostCentre = "RC-UK" }
            }
        };

        _context.Organizations.AddRange(meridian, northwind, halcyon);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedSkillsAsync(CancellationToken ct)
    {
        if (await _context.Skills.AnyAsync(ct))
        {
            return;
        }

        // Canonical name, category, then the aliases that resolve back to it.
        var taxonomy = new (string Name, string Category, string[] Aliases)[]
        {
            ("C#", "Language", new[] { "csharp", "c sharp", "c-sharp" }),
            ("JavaScript", "Language", new[] { "js", "ecmascript" }),
            ("TypeScript", "Language", new[] { "ts" }),
            ("Python", "Language", Array.Empty<string>()),
            ("Java", "Language", Array.Empty<string>()),
            ("SQL", "Language", new[] { "t-sql", "tsql" }),
            ("ASP.NET Core", "Framework", new[] { "aspnet core", "asp net core", "dotnet core" }),
            ("Entity Framework Core", "Framework", new[] { "ef core", "efcore" }),
            ("React", "Framework", new[] { "react.js", "reactjs" }),
            ("Angular", "Framework", Array.Empty<string>()),
            ("Node.js", "Framework", new[] { "node", "nodejs" }),
            ("SQL Server", "Database", new[] { "mssql", "microsoft sql server" }),
            ("PostgreSQL", "Database", new[] { "postgres" }),
            ("MongoDB", "Database", new[] { "mongo" }),
            ("Docker", "Platform", Array.Empty<string>()),
            ("Kubernetes", "Platform", new[] { "k8s" }),
            ("Azure", "Cloud", new[] { "microsoft azure" }),
            ("AWS", "Cloud", new[] { "amazon web services" }),
            ("REST API Design", "Practice", new[] { "rest", "restful api" }),
            ("Microservices", "Practice", Array.Empty<string>()),
            ("Unit Testing", "Practice", new[] { "xunit", "nunit" }),
            ("CI/CD", "Practice", new[] { "continuous integration", "continuous delivery" }),
            ("Agile", "Practice", new[] { "scrum" }),
            ("Stakeholder Management", "Leadership", Array.Empty<string>()),
            ("Team Leadership", "Leadership", new[] { "people management" })
        };

        foreach (var (name, category, aliases) in taxonomy)
        {
            var skill = new Skill { Name = name, Category = category };
            foreach (var alias in aliases)
            {
                skill.Aliases.Add(new SkillAlias { Alias = alias });
            }

            _context.Skills.Add(skill);
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await _context.Users.AnyAsync(ct))
        {
            return;
        }

        var roles = await _context.Roles.ToDictionaryAsync(r => r.Name, ct);
        var meridian = await _context.Organizations
            .Include(o => o.Departments)
            .FirstAsync(o => o.Name == "Meridian HR Consulting", ct);

        var techPractice = meridian.Departments.First(d => d.Name == "Technology Practice");

        var administrator = BuildUser("admin@meridian.example.com", "Priya Wickramasinghe", roles[Role.Administrator]);
        administrator.OrganizationId = meridian.Id;

        var recruiter = BuildUser("recruiter@meridian.example.com", "Nuwan Perera", roles[Role.Recruiter]);
        recruiter.OrganizationId = meridian.Id;
        recruiter.DepartmentId = techPractice.Id;

        var hiringManager = BuildUser("manager@meridian.example.com", "Ayesha Fernando", roles[Role.HiringManager]);
        hiringManager.OrganizationId = meridian.Id;
        hiringManager.DepartmentId = techPractice.Id;

        var candidate = BuildUser("candidate@meridian.example.com", "Ruwan Jayasuriya", roles[Role.Candidate]);
        candidate.CandidateProfile = new CandidateProfile
        {
            Headline = "Full stack engineer, .NET and React",
            Summary = "Six years building line-of-business web applications on ASP.NET Core and React, "
                      + "most recently on a payments platform handling regional settlement.",
            City = "Colombo",
            Country = "Sri Lanka",
            YearsOfExperience = 6,
            HighestEducation = EducationLevel.Bachelors,
            IsOpenToRemote = true
        };

        _context.Users.AddRange(administrator, recruiter, hiringManager, candidate);
        await _context.SaveChangesAsync(ct);
    }

    private User BuildUser(string email, string fullName, Role role) => new()
    {
        Email = email,
        FullName = fullName,
        PasswordHash = _passwordHasher.Hash(DemoPassword),
        IsActive = true,
        UserRoles = { new UserRole { RoleId = role.Id } }
    };
}
