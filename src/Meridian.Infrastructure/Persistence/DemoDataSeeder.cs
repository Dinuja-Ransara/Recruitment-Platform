using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Persistence;

/// <summary>
/// Seeds the demonstration applicant pool.
///
/// The candidates are deliberately varied rather than uniform: a near-perfect
/// fit, several partial fits, one strong engineer missing a mandatory skill, and
/// one clearly unsuitable applicant. A ranking screen only proves anything when
/// the pool it ranks has a real distribution in it.
///
/// Resume text is written as prose because the matching engine reads it with
/// TF-IDF. A pool of "Lorem ipsum" resumes would produce meaningless similarity
/// scores and an unconvincing demonstration.
/// </summary>
public class DemoDataSeeder
{
    private readonly MeridianDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DemoDataSeeder(MeridianDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    private sealed record CandidateSpec(
        string Email,
        string FullName,
        string Headline,
        string City,
        string Country,
        decimal Years,
        EducationLevel Education,
        bool OpenToRemote,
        string ResumeText,
        Dictionary<string, decimal> Skills);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Idempotency is keyed on a specific seeded account rather than on a row
        // count, because a count is also moved by real registrations and would
        // silently skip seeding on a database that merely has users in it.
        if (await _context.Users.AnyAsync(u => u.Email == "dilani.rathnayake@example.com", ct))
        {
            return;
        }

        var candidateRole = await _context.Roles.FirstAsync(r => r.Name == Role.Candidate, ct);
        var skillLookup = await _context.Skills.ToDictionaryAsync(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var spec in BuildCandidates())
        {
            if (await _context.Users.AnyAsync(u => u.Email == spec.Email, ct))
            {
                continue;
            }

            var user = new User
            {
                Email = spec.Email,
                FullName = spec.FullName,
                PasswordHash = _passwordHasher.Hash(DatabaseSeeder.DemoPassword),
                IsActive = true,
                UserRoles = { new UserRole { RoleId = candidateRole.Id } },
                CandidateProfile = new CandidateProfile
                {
                    Headline = spec.Headline,
                    Summary = spec.ResumeText[..Math.Min(400, spec.ResumeText.Length)],
                    City = spec.City,
                    Country = spec.Country,
                    YearsOfExperience = spec.Years,
                    HighestEducation = spec.Education,
                    IsOpenToRemote = spec.OpenToRemote
                }
            };

            var resume = new Resume
            {
                FileName = $"{spec.FullName.Replace(' ', '_')}_CV.pdf",
                StoredPath = $"seed/{spec.Email}.pdf",
                ContentType = "application/pdf",
                SourceFormat = "pdf",
                SizeInBytes = spec.ResumeText.Length,
                RawText = spec.ResumeText,
                IsPrimary = true,
                ParsedAt = DateTime.UtcNow
            };

            foreach (var (skillName, years) in spec.Skills)
            {
                if (skillLookup.TryGetValue(skillName, out var skillId))
                {
                    resume.ResumeSkills.Add(new ResumeSkill
                    {
                        SkillId = skillId,
                        YearsOfExperience = years,
                        // Seeded extractions are treated as high confidence because
                        // the text was authored to contain them.
                        ConfidenceScore = 0.95,
                        EvidenceSnippet = FindEvidence(spec.ResumeText, skillName)
                    });
                }
            }

            user.CandidateProfile.Resumes.Add(resume);
            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>Pulls the sentence mentioning a skill, so the UI can cite evidence.</summary>
    private static string? FindEvidence(string resumeText, string skill)
    {
        var sentences = resumeText.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var hit = sentences.FirstOrDefault(s => s.Contains(skill, StringComparison.OrdinalIgnoreCase));
        return hit is null ? null : (hit.Length > 240 ? hit[..240] : hit);
    }

    private static List<CandidateSpec> BuildCandidates() =>
    [
        new("dilani.rathnayake@example.com", "Dilani Rathnayake",
            "Senior .NET engineer, payments and settlement", "Colombo", "Sri Lanka", 7,
            EducationLevel.Masters, true,
            "Seven years building transaction processing systems in C# on ASP.NET Core. Led the settlement "
            + "and clearing service for a regional card scheme, handling reconciliation across three markets. "
            + "Designed the relational model on SQL Server with careful indexing of the settlement tables. "
            + "Introduced Docker based deployment and a CI/CD pipeline, cutting release time from days to hours. "
            + "Mentored four engineers and ran the architecture review forum.",
            new Dictionary<string, decimal>
            {
                ["C#"] = 7, ["ASP.NET Core"] = 6, ["SQL Server"] = 6, ["Docker"] = 4,
                ["Entity Framework Core"] = 5, ["CI/CD"] = 4, ["REST API Design"] = 6, ["Team Leadership"] = 3
            }),

        new("kasun.silva@example.com", "Kasun Silva",
            "Backend engineer, C# and distributed services", "Kandy", "Sri Lanka", 5,
            EducationLevel.Bachelors, true,
            "Five years of backend development in C# and ASP.NET Core for logistics and freight tracking. "
            + "Built REST APIs consumed by mobile clients across two regions, with SQL Server as the primary store. "
            + "Familiar with containerised workloads although most deployment was handled by a platform team. "
            + "Comfortable with unit testing and code review.",
            new Dictionary<string, decimal>
            {
                ["C#"] = 5, ["ASP.NET Core"] = 4, ["SQL Server"] = 4, ["REST API Design"] = 4, ["Unit Testing"] = 3
            }),

        new("nadeesha.perera@example.com", "Nadeesha Perera",
            "Full stack engineer, React and Node", "Colombo", "Sri Lanka", 4,
            EducationLevel.Bachelors, true,
            "Four years building customer facing web applications with React and TypeScript on a Node.js backend. "
            + "Delivered a booking platform serving thirty thousand monthly users, with PostgreSQL behind it. "
            + "Some exposure to C# through an internal reporting tool. Strong on interface work, accessibility "
            + "and design systems.",
            new Dictionary<string, decimal>
            {
                ["React"] = 4, ["TypeScript"] = 4, ["JavaScript"] = 5, ["Node.js"] = 4,
                ["PostgreSQL"] = 3, ["C#"] = 1
            }),

        new("arjun.mehta@example.com", "Arjun Mehta",
            "Principal engineer, platform and reliability", "Singapore", "Singapore", 12,
            EducationLevel.Masters, false,
            "Twelve years across platform engineering and site reliability. Ran the payments platform for a "
            + "regional bank, owning availability targets and incident response. Deep experience with "
            + "microservices, Kubernetes and observability. Wrote services primarily in Java, with C# on two "
            + "earlier products. Led an engineering group of eighteen across three offices.",
            new Dictionary<string, decimal>
            {
                ["Java"] = 12, ["Kubernetes"] = 6, ["Microservices"] = 8, ["Docker"] = 7,
                ["C#"] = 3, ["Team Leadership"] = 7, ["AWS"] = 5
            }),

        // Strong on paper, but missing a mandatory requirement. Exists so the
        // demonstration can show the mandatory-gap cap doing its job.
        new("fatima.hassan@example.com", "Fatima Hassan",
            "Backend engineer, C# and cloud", "London", "United Kingdom", 6,
            EducationLevel.Bachelors, true,
            "Six years of C# and ASP.NET Core work across insurance and healthcare. Built claims processing "
            + "APIs handling high volumes, deployed on Azure with Docker. Worked exclusively with PostgreSQL "
            + "and MongoDB for persistence, never with SQL Server. Strong on testing discipline and CI/CD.",
            new Dictionary<string, decimal>
            {
                ["C#"] = 6, ["ASP.NET Core"] = 5, ["Docker"] = 4, ["Azure"] = 4,
                ["PostgreSQL"] = 5, ["MongoDB"] = 3, ["Unit Testing"] = 5, ["CI/CD"] = 4
            }),

        new("thilina.bandara@example.com", "Thilina Bandara",
            "Graduate software engineer", "Galle", "Sri Lanka", 1,
            EducationLevel.Bachelors, true,
            "Recent computer science graduate with one year in a junior role. Built internal tools in C# and "
            + "learned ASP.NET Core on the job. University final year project was a library management system "
            + "with a SQL Server backend. Keen to work on larger systems.",
            new Dictionary<string, decimal>
            {
                ["C#"] = 1, ["ASP.NET Core"] = 1, ["SQL Server"] = 1
            }),

        new("marie.dubois@example.com", "Marie Dubois",
            "Hospitality operations supervisor", "Paris", "France", 8,
            EducationLevel.Diploma, false,
            "Eight years in hotel operations, supervising front of house teams of up to twenty. Responsible for "
            + "rota planning, guest relations, supplier negotiation and stock control. Introduced a new booking "
            + "process that reduced check in time significantly.",
            new Dictionary<string, decimal>
            {
                ["Stakeholder Management"] = 6, ["Team Leadership"] = 8
            })
    ];
}
