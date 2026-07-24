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

        var candidate2 = BuildUser("nuwan.perera@meridian.example.com", "Nuwan Perera", roles[Role.Candidate]);
        candidate2.CandidateProfile = new CandidateProfile
        {
            Headline = "Mid-level .NET backend engineer",
            Summary = "Nuwan is a backend-focused software engineer with four years of experience building APIs and business applications using C#, ASP.NET Core and SQL Server. He has implemented REST endpoints, background jobs and third-party integrations in systems used by internal operations teams. He is comfortable with Entity Framework Core, unit testing and CI/CD practices, and works well with frontend developers to refine API contracts and performance.",
            City = "Colombo",
            Country = "Sri Lanka",
            YearsOfExperience = 4,
            HighestEducation = EducationLevel.Bachelors,
            IsOpenToRemote = true
        };

        var candidate3 = BuildUser("sanduni.jayasinghe@meridian.example.com", "Sanduni Jayasinghe", roles[Role.Candidate]);
        candidate3.CandidateProfile = new CandidateProfile
        {
            Headline = "Senior full-stack engineer, .NET and React",
            Summary = "Sanduni has seven years of experience building enterprise web applications with .NET on the backend and React on the frontend. She has designed domain models, optimized reporting queries and introduced automated testing practices for customer-facing systems in logistics and finance. She regularly leads code reviews, mentors junior developers and collaborates with product stakeholders to deliver maintainable features on schedule.",
            City = "Singapore",
            Country = "Singapore",
            YearsOfExperience = 7,
            HighestEducation = EducationLevel.Bachelors,
            IsOpenToRemote = true
        };

        var candidate4 = BuildUser("isuru.fernando@meridian.example.com", "Isuru Fernando", roles[Role.Candidate]);
        candidate4.CandidateProfile = new CandidateProfile
        {
            Headline = "Backend engineer, Java and Spring Boot",
            Summary = "Isuru is a backend engineer with three years of experience building REST APIs using Java, Spring Boot and PostgreSQL. He has worked on authentication flows, asynchronous processing and microservice-based systems, and follows clean coding and testing practices. Although his recent work is mainly in Java rather than C#, he understands core backend engineering concepts and can adapt quickly to new technologies.",
            City = "London",
            Country = "United Kingdom",
            YearsOfExperience = 3,
            HighestEducation = EducationLevel.Bachelors,
            IsOpenToRemote = true
        };

        var candidate5 = BuildUser("tharushi.madushani@meridian.example.com", "Tharushi Madushani", roles[Role.Candidate]);
        candidate5.CandidateProfile = new CandidateProfile
        {
            Headline = "Junior .NET developer",
            Summary = "Tharushi is a junior developer with one year of experience contributing to internal tools built with ASP.NET Core MVC and SQL Server. She has worked on CRUD features, validation rules and simple reporting pages, and is growing her confidence with Entity Framework, LINQ and REST APIs. She performs well under guidance from senior engineers and is eager to grow into a more advanced backend development role.",
            City = "Colombo",
            Country = "Sri Lanka",
            YearsOfExperience = 1,
            HighestEducation = EducationLevel.Bachelors,
            IsOpenToRemote = false
        };

        var candidate6 = BuildUser("arjun.mehta@meridian.example.com", "Arjun Mehta", roles[Role.Candidate]);
        candidate6.CandidateProfile = new CandidateProfile
        {
            Headline = "Principal software architect, Java microservices",
            Summary = "Arjun is a principal software architect with twelve years of experience designing and leading distributed systems in finance and e-commerce environments. His core expertise is in Java, Spring Cloud, Kubernetes and event-driven microservice platforms, and he has led engineering teams across multiple regions. He is strong in architecture, stakeholder communication and technical leadership, but his hands-on work is primarily in the Java ecosystem rather than C# and ASP.NET Core.",
            City = "Singapore",
            Country = "Singapore",
            YearsOfExperience = 12,
            HighestEducation = EducationLevel.Masters,
            IsOpenToRemote = true
        };

        var candidate7 = BuildUser("dilani.karunaratne@meridian.example.com", "Dilani Karunaratne", roles[Role.Candidate]);
        candidate7.CandidateProfile = new CandidateProfile
        {
            Headline = "Assistant manager, retail operations",
            Summary = "Dilani has six years of experience managing day-to-day retail operations, including supervising staff, coordinating inventory checks, preparing sales reports and resolving customer escalations. She is organized, dependable and experienced in operational administration, but she does not have a software engineering background or hands-on experience with programming, databases or application development. Her profile is intentionally included as a poor fit for technical roles in the demonstration dataset.",
            City = "Colombo",
            Country = "Sri Lanka",
            YearsOfExperience = 6,
            HighestEducation = EducationLevel.Bachelors,
            IsOpenToRemote = false
        };

        _context.Users.AddRange(
    administrator,
    recruiter,
    hiringManager,
    candidate,
    candidate2,
    candidate3,
    candidate4,
    candidate5,
    candidate6,
    candidate7);
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
