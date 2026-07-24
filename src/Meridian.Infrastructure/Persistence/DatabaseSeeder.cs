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
        await SeedJobPostingsAsync(ct);
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

    private async Task SeedJobPostingsAsync(CancellationToken ct)
    {
        if (await _context.JobPostings.AnyAsync(ct))
        {
            return;
        }

        var organizations = await _context.Organizations
            .Include(o => o.Departments)
            .ToDictionaryAsync(o => o.Name, ct);

        var skills = await _context.Skills
            .ToDictionaryAsync(s => s.Name, ct);

        var recruiter = await _context.Users
            .FirstAsync(u => u.Email == "recruiter@meridian.example.com", ct);

        var meridian = organizations["Meridian HR Consulting"];
        var northwind = organizations["Northwind Logistics"];
        var halcyon = organizations["Halcyon Financial Group"];

        var techPractice = meridian.Departments.First(d => d.Name == "Technology Practice");
        var engineering = northwind.Departments.First(d => d.Name == "Engineering");
        var digitalBanking = halcyon.Departments.First(d => d.Name == "Digital Banking");

        var job1 = new JobPosting
        {
            Title = "Backend Software Engineer",
            Description = "Join the Technology Practice in Colombo to build and enhance enterprise recruitment workflows used by regional clients. The role focuses on ASP.NET Core services, SQL-backed data processing and integrations that support screening, reporting and candidate lifecycle management.",
            Responsibilities = "Design and implement backend services, build secure REST APIs, optimize database access, write automated tests, support production deployments and collaborate with frontend engineers and recruiters to deliver reliable hiring workflows.",
            OrganizationId = meridian.Id,
            DepartmentId = techPractice.Id,
            City = "Colombo",
            Country = "Sri Lanka",
            WorkMode = WorkMode.Hybrid,
            EmploymentType = EmploymentType.FullTime,
            Seniority = SeniorityLevel.Mid,
            MinYearsExperience = 3,
            RequiredEducation = EducationLevel.Bachelors,
            SalaryMin = 180000,
            SalaryMax = 260000,
            Currency = "LKR",
            Status = JobStatus.Published,
            PublishedAt = DateTime.UtcNow.AddDays(-12),
            ClosingDate = DateTime.UtcNow.AddDays(18),
            RankingStrategy = RankingStrategyType.Hybrid,
            PostedByUserId = recruiter.Id
        };
        job1.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["C#"].Id, IsMandatory = true, Weight = 5, MinYearsExperience = 2 });
        job1.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["ASP.NET Core"].Id, IsMandatory = true, Weight = 5, MinYearsExperience = 2 });
        job1.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["SQL"].Id, IsMandatory = true, Weight = 4, MinYearsExperience = 2 });
        job1.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Entity Framework Core"].Id, IsMandatory = false, Weight = 3, MinYearsExperience = 1 });
        job1.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["REST API Design"].Id, IsMandatory = false, Weight = 3, MinYearsExperience = 1 });
        job1.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Unit Testing"].Id, IsMandatory = false, Weight = 2, MinYearsExperience = 1 });

        var job2 = new JobPosting
        {
            Title = "Senior Full-stack Engineer",
            Description = "Northwind Logistics is hiring a senior engineer in Singapore to modernize internal shipment tracking and client visibility platforms. The role spans backend API development, frontend delivery and technical leadership across distributed product teams.",
            Responsibilities = "Lead design decisions, build resilient APIs and React user interfaces, review pull requests, mentor engineers, improve CI/CD quality and work with operations stakeholders to deliver measurable platform improvements.",
            OrganizationId = northwind.Id,
            DepartmentId = engineering.Id,
            City = "Singapore",
            Country = "Singapore",
            WorkMode = WorkMode.OnSite,
            EmploymentType = EmploymentType.FullTime,
            Seniority = SeniorityLevel.Senior,
            MinYearsExperience = 5,
            RequiredEducation = EducationLevel.Bachelors,
            SalaryMin = 7000,
            SalaryMax = 10000,
            Currency = "SGD",
            Status = JobStatus.Published,
            PublishedAt = DateTime.UtcNow.AddDays(-9),
            ClosingDate = DateTime.UtcNow.AddDays(21),
            RankingStrategy = RankingStrategyType.Hybrid,
            PostedByUserId = recruiter.Id
        };
        job2.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["C#"].Id, IsMandatory = true, Weight = 5, MinYearsExperience = 3 });
        job2.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["React"].Id, IsMandatory = true, Weight = 5, MinYearsExperience = 2 });
        job2.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["ASP.NET Core"].Id, IsMandatory = true, Weight = 4, MinYearsExperience = 3 });
        job2.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Docker"].Id, IsMandatory = false, Weight = 3, MinYearsExperience = 1 });
        job2.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["CI/CD"].Id, IsMandatory = false, Weight = 3, MinYearsExperience = 1 });
        job2.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Team Leadership"].Id, IsMandatory = false, Weight = 2, MinYearsExperience = 1 });

        var job3 = new JobPosting
        {
            Title = "Platform Engineer",
            Description = "Halcyon Financial Group is expanding its digital banking platform in London and needs a platform engineer to improve reliability, automation and service delivery. This role suits engineers who enjoy cloud infrastructure, deployment pipelines and secure backend services.",
            Responsibilities = "Maintain deployment pipelines, improve observability, containerize services, support cloud infrastructure, strengthen service reliability and partner with engineering teams to improve release confidence and runtime performance.",
            OrganizationId = halcyon.Id,
            DepartmentId = digitalBanking.Id,
            City = "London",
            Country = "United Kingdom",
            WorkMode = WorkMode.Hybrid,
            EmploymentType = EmploymentType.FullTime,
            Seniority = SeniorityLevel.Senior,
            MinYearsExperience = 4,
            RequiredEducation = EducationLevel.Bachelors,
            SalaryMin = 65000,
            SalaryMax = 85000,
            Currency = "GBP",
            Status = JobStatus.Published,
            PublishedAt = DateTime.UtcNow.AddDays(-7),
            ClosingDate = DateTime.UtcNow.AddDays(25),
            RankingStrategy = RankingStrategyType.ExperienceFirst,
            PostedByUserId = recruiter.Id
        };
        job3.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Azure"].Id, IsMandatory = true, Weight = 5, MinYearsExperience = 2 });
        job3.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Docker"].Id, IsMandatory = true, Weight = 4, MinYearsExperience = 2 });
        job3.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["CI/CD"].Id, IsMandatory = true, Weight = 4, MinYearsExperience = 2 });
        job3.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Kubernetes"].Id, IsMandatory = false, Weight = 3, MinYearsExperience = 1 });
        job3.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Microservices"].Id, IsMandatory = false, Weight = 3, MinYearsExperience = 1 });
        job3.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Stakeholder Management"].Id, IsMandatory = false, Weight = 2, MinYearsExperience = 1 });

        var job4 = new JobPosting
        {
            Title = "Junior QA Automation Engineer",
            Description = "Meridian HR Consulting is looking for a junior engineer in Colombo to strengthen regression coverage and release confidence across recruiter and candidate workflows. The role is ideal for someone with a solid technical foundation who wants to grow into quality engineering and automation.",
            Responsibilities = "Create and maintain automated test cases, execute regression suites, report defects clearly, collaborate with developers on bug reproduction, improve test data quality and support release validation across web application features.",
            OrganizationId = meridian.Id,
            DepartmentId = techPractice.Id,
            City = "Colombo",
            Country = "Sri Lanka",
            WorkMode = WorkMode.Remote,
            EmploymentType = EmploymentType.FullTime,
            Seniority = SeniorityLevel.Junior,
            MinYearsExperience = 1,
            RequiredEducation = EducationLevel.Diploma,
            SalaryMin = 90000,
            SalaryMax = 140000,
            Currency = "LKR",
            Status = JobStatus.Published,
            PublishedAt = DateTime.UtcNow.AddDays(-5),
            ClosingDate = DateTime.UtcNow.AddDays(20),
            RankingStrategy = RankingStrategyType.SkillWeighted,
            PostedByUserId = recruiter.Id
        };
        job4.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["JavaScript"].Id, IsMandatory = true, Weight = 3, MinYearsExperience = 1 });
        job4.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Unit Testing"].Id, IsMandatory = true, Weight = 4, MinYearsExperience = 1 });
        job4.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["Agile"].Id, IsMandatory = true, Weight = 3, MinYearsExperience = 1 });
        job4.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["CI/CD"].Id, IsMandatory = false, Weight = 2, MinYearsExperience = 0 });
        job4.RequiredSkills.Add(new JobRequiredSkill { SkillId = skills["SQL"].Id, IsMandatory = false, Weight = 2, MinYearsExperience = 0 });

        _context.JobPostings.AddRange(job1, job2, job3, job4);
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