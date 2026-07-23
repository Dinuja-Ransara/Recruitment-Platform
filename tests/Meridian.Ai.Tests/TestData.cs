using Meridian.Ai.Models;
using Meridian.Domain.Enums;

namespace Meridian.Ai.Tests;

/// <summary>
/// Fixtures shared across the suite. Written as literals rather than loaded from
/// a database, which is only possible because the engine takes plain records.
/// </summary>
internal static class TestData
{
    public static JobProfile BackendJob(params SkillRequirement[] skills) => new()
    {
        JobId = 1,
        Title = "Senior Backend Engineer, Payments",
        Description = "Own the settlement service clearing card payments across three markets. "
                      + "Design resilient distributed services, lead code review, mentor engineers. "
                      + "Deep C# and ASP.NET Core, relational modelling on SQL Server, containerised deployment.",
        City = "Colombo",
        Country = "Sri Lanka",
        WorkMode = WorkMode.Hybrid,
        Seniority = SeniorityLevel.Senior,
        MinYearsExperience = 5,
        RequiredEducation = EducationLevel.Bachelors,
        Skills = skills.Length > 0 ? skills : new[]
        {
            new SkillRequirement("C#", true, 5, 4),
            new SkillRequirement("ASP.NET Core", true, 5, 3),
            new SkillRequirement("SQL Server", true, 4, 3),
            new SkillRequirement("Docker", false, 2, 1)
        }
    };

    /// <summary>A candidate who genuinely fits the posting above.</summary>
    public static CandidateProfileSnapshot StrongCandidate() => new()
    {
        CandidateId = 10,
        FullName = "Ruwan Jayasuriya",
        Headline = "Senior backend engineer, C# and distributed payments",
        ResumeText = "Six years building payment settlement services in C# on ASP.NET Core. "
                     + "Designed the clearing pipeline for a regional card scheme on SQL Server. "
                     + "Containerised deployment with Docker, led code review, mentored two engineers.",
        City = "Colombo",
        Country = "Sri Lanka",
        IsOpenToRemote = true,
        YearsOfExperience = 6,
        HighestEducation = EducationLevel.Bachelors,
        Skills = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["C#"] = 6, ["ASP.NET Core"] = 5, ["SQL Server"] = 5, ["Docker"] = 3
        }
    };

    /// <summary>Same person, but missing a skill the posting marks mandatory.</summary>
    public static CandidateProfileSnapshot MissingMandatorySkill()
    {
        var candidate = StrongCandidate();
        var skills = new Dictionary<string, decimal>(candidate.Skills, StringComparer.OrdinalIgnoreCase);
        skills.Remove("SQL Server");
        return candidate with { CandidateId = 11, Skills = skills };
    }

    /// <summary>Unrelated background, so both skills and text should score low.</summary>
    public static CandidateProfileSnapshot UnrelatedCandidate() => new()
    {
        CandidateId = 12,
        FullName = "Anon Applicant",
        Headline = "Hospitality supervisor",
        ResumeText = "Managed front of house rotas for a hotel chain, handled guest complaints, "
                     + "stock ordering and supplier relationships.",
        City = "London",
        Country = "United Kingdom",
        IsOpenToRemote = false,
        YearsOfExperience = 1,
        HighestEducation = EducationLevel.Diploma,
        Skills = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    };
}
