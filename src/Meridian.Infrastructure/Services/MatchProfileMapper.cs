using Meridian.Ai.Models;
using Meridian.Application.Dtos.Applications;
using Meridian.Domain.Entities;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// Translates between persistence entities and the plain records the matching
/// engine works with.
///
/// This mapper is the reason Meridian.Ai has no dependency on Entity Framework.
/// The engine never sees a tracked entity or a lazy-loading proxy, which is what
/// lets it be unit tested with literals and keeps its scoring reproducible.
/// </summary>
public static class MatchProfileMapper
{
    public static JobProfile ToJobProfile(JobPosting job) => new()
    {
        JobId = job.Id,
        Title = job.Title,
        Description = $"{job.Description} {job.Responsibilities}",
        City = job.City,
        Country = job.Country,
        WorkMode = job.WorkMode,
        Seniority = job.Seniority,
        MinYearsExperience = job.MinYearsExperience,
        RequiredEducation = job.RequiredEducation,
        Skills = job.RequiredSkills
            .Where(s => s.Skill is not null)
            .Select(s => new SkillRequirement(s.Skill!.Name, s.IsMandatory, s.Weight, s.MinYearsExperience))
            .ToList()
    };

    /// <summary>
    /// Builds the candidate snapshot from their profile and a chosen resume.
    ///
    /// Skills come from the parsed resume rather than anything the candidate
    /// typed into a form, so the score rests on evidence in the document.
    /// </summary>
    public static CandidateProfileSnapshot ToCandidateSnapshot(CandidateProfile profile, Resume? resume)
    {
        var skills = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        if (resume?.ResumeSkills is not null)
        {
            foreach (var skill in resume.ResumeSkills.Where(s => s.Skill is not null))
            {
                // Keep the highest claim if a skill somehow appears twice.
                var name = skill.Skill!.Name;
                if (!skills.TryGetValue(name, out var existing) || skill.YearsOfExperience > existing)
                {
                    skills[name] = skill.YearsOfExperience;
                }
            }
        }

        return new CandidateProfileSnapshot
        {
            CandidateId = profile.Id,
            FullName = profile.User?.FullName ?? string.Empty,
            Headline = profile.Headline,
            ResumeText = string.IsNullOrWhiteSpace(resume?.RawText) ? profile.Summary : resume!.RawText,
            City = profile.City,
            Country = profile.Country,
            IsOpenToRemote = profile.IsOpenToRemote,
            YearsOfExperience = profile.YearsOfExperience,
            HighestEducation = profile.HighestEducation,
            Skills = skills
        };
    }

    public static MatchExplanationDto ToDto(MatchExplanation explanation) => new()
    {
        Score = explanation.Score,
        Strategy = explanation.Strategy,
        Summary = explanation.Summary,
        HasMandatoryGap = explanation.HasMandatoryGap,
        Factors = explanation.Factors
            .Select(f => new ScoreFactorDto(f.Name, Math.Round(f.Value, 4), f.Weight, Math.Round(f.Contribution, 4), f.Detail))
            .ToList(),
        MatchedSkills = explanation.MatchedSkills
            .Select(s => new SkillMatchDto(s.Skill, s.IsMandatory, s.CandidateYears, s.RequiredYears, s.MeetsExperienceBar))
            .ToList(),
        MissingSkills = explanation.MissingSkills
            .Select(s => new SkillGapDto(s.Skill, s.IsMandatory, s.RequiredYears))
            .ToList()
    };
}
