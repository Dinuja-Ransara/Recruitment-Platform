using System.Text.Json;
using Meridian.Ai.Matching;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Persistence;

/// <summary>
/// Seeds the demonstration dataset: candidates, postings, and a hiring pipeline
/// already in motion.
///
/// The pipeline matters as much as the volume. A recruitment platform showing
/// twelve postings and no applicants proves nothing; the screens only make sense
/// when applications are spread across the stages, some shortlisted, some
/// rejected, one or two hired.
///
/// Applications are scored at seed time by the real matching engine, so every
/// dashboard is populated on first load rather than waiting for someone to open
/// the ranking screen.
///
/// Everything is deterministic. Which candidate applies where, and how far each
/// application advances, is derived from the data rather than randomised, so the
/// demonstration looks identical every time it is shown.
/// </summary>
public class DemoDataSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    /// <summary>Presence of this account marks the demonstration data as already seeded.</summary>
    private const string MarkerEmail = "dilani.rathnayake@example.com";

    private readonly MeridianDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMatchingEngine _engine;

    public DemoDataSeeder(MeridianDbContext context, IPasswordHasher passwordHasher, IMatchingEngine engine)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _engine = engine;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Keyed on a specific seeded account rather than a row count, because a
        // count also moves when real users register and would silently skip.
        if (await _context.Users.AnyAsync(u => u.Email == MarkerEmail, ct))
        {
            return;
        }

        await SeedStaffAsync(ct);
        await GiveBaseCandidateAResumeAsync(ct);
        await SeedCandidatesAsync(ct);
        await SeedJobsAsync(ct);
        await SeedPipelineAsync(ct);
    }

    // -----------------------------------------------------------------------
    // Recruiting staff for the two client organisations
    // -----------------------------------------------------------------------

    /// <summary>
    /// Each organisation needs its own recruiter, because posting ownership is
    /// scoped by organisation. Without this the client postings would have no
    /// one able to manage them, and the scoping rule could not be demonstrated.
    /// </summary>
    private async Task SeedStaffAsync(CancellationToken ct)
    {
        var recruiterRole = await _context.Roles.FirstAsync(r => r.Name == Role.Recruiter, ct);
        var managerRole = await _context.Roles.FirstAsync(r => r.Name == Role.HiringManager, ct);
        var organizations = await _context.Organizations.Include(o => o.Departments).ToListAsync(ct);

        var staff = new (string Email, string Name, string Organization, int RoleId)[]
        {
            ("recruiter.sg@northwind.example.com", "Mei Ling Chua", "Northwind Logistics", recruiterRole.Id),
            ("manager.sg@northwind.example.com", "Rajesh Kumar", "Northwind Logistics", managerRole.Id),
            ("recruiter.uk@halcyon.example.com", "Eleanor Whitby", "Halcyon Financial Group", recruiterRole.Id),
            ("manager.uk@halcyon.example.com", "Tom Ashworth", "Halcyon Financial Group", managerRole.Id),
        };

        foreach (var (email, name, organizationName, roleId) in staff)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email, ct))
            {
                continue;
            }

            var organization = organizations.First(o => o.Name == organizationName);

            _context.Users.Add(new User
            {
                Email = email,
                FullName = name,
                PasswordHash = _passwordHasher.Hash(DatabaseSeeder.DemoPassword),
                IsActive = true,
                OrganizationId = organization.Id,
                DepartmentId = organization.Departments.FirstOrDefault()?.Id,
                UserRoles = { new UserRole { RoleId = roleId } }
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    // -----------------------------------------------------------------------
    // Candidates
    // -----------------------------------------------------------------------

    /// <summary>
    /// The base seeder creates candidate@meridian.example.com without a CV,
    /// because it only establishes the four sign-in accounts. That account is the
    /// one anyone demonstrating the system logs in as, so it needs a resume and
    /// extracted skills like everybody else. Without one it scores near zero and
    /// the candidate portal looks broken rather than empty.
    /// </summary>
    private async Task GiveBaseCandidateAResumeAsync(CancellationToken ct)
    {
        var profile = await _context.CandidateProfiles
            .Include(c => c.User)
            .Include(c => c.Resumes)
            .FirstOrDefaultAsync(c => c.User!.Email == "candidate@meridian.example.com", ct);

        if (profile is null || profile.Resumes.Count > 0)
        {
            return;
        }

        var skillIds = await _context.Skills.ToDictionaryAsync(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase, ct);

        const string resumeText =
            "Six years building line of business web applications on ASP.NET Core and React, most recently on "
            + "a payments platform handling regional settlement. Comfortable across the stack, from the SQL "
            + "Server schema through the API to the interface. Introduced automated testing to a team that had "
            + "none, and values readable code and honest estimates.";

        var resume = new Resume
        {
            FileName = "Ruwan_Jayasuriya_CV.pdf",
            StoredPath = "seed/candidate@meridian.example.com.pdf",
            ContentType = "application/pdf",
            SourceFormat = "pdf",
            SizeInBytes = resumeText.Length,
            RawText = resumeText,
            IsPrimary = true,
            ParsedAt = DateTime.UtcNow
        };

        var skills = new Dictionary<string, decimal>
        {
            ["C#"] = 6, ["ASP.NET Core"] = 5, ["React"] = 4, ["TypeScript"] = 3,
            ["SQL Server"] = 5, ["REST API Design"] = 5, ["Unit Testing"] = 4
        };

        foreach (var (skillName, years) in skills)
        {
            if (skillIds.TryGetValue(skillName, out var skillId))
            {
                resume.ResumeSkills.Add(new ResumeSkill
                {
                    SkillId = skillId,
                    YearsOfExperience = years,
                    ConfidenceScore = 0.95,
                    EvidenceSnippet = FindEvidence(resumeText, skillName)
                });
            }
        }

        profile.Resumes.Add(resume);
        profile.Summary = resumeText;
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedCandidatesAsync(CancellationToken ct)
    {
        var candidateRole = await _context.Roles.FirstAsync(r => r.Name == Role.Candidate, ct);
        var skillIds = await _context.Skills.ToDictionaryAsync(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var spec in DemoData.Candidates())
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
                FileName = $"{spec.FullName.Replace(' ', '_').Replace("'", string.Empty)}_CV.pdf",
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
                if (skillIds.TryGetValue(skillName, out var skillId))
                {
                    resume.ResumeSkills.Add(new ResumeSkill
                    {
                        SkillId = skillId,
                        YearsOfExperience = years,
                        ConfidenceScore = 0.95,
                        EvidenceSnippet = FindEvidence(spec.ResumeText, skillName)
                    });
                }
            }

            user.CandidateProfile!.Resumes.Add(resume);
            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync(ct);
    }

    // -----------------------------------------------------------------------
    // Job postings
    // -----------------------------------------------------------------------

    private async Task SeedJobsAsync(CancellationToken ct)
    {
        var organizations = await _context.Organizations.Include(o => o.Departments).ToListAsync(ct);
        var skillIds = await _context.Skills.ToDictionaryAsync(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase, ct);

        var recruiters = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == Role.Recruiter))
            .ToListAsync(ct);

        // Publication dates are staggered backwards so the board does not look
        // like every role appeared at the same instant.
        var publishedOffset = 0;

        foreach (var spec in DemoData.Jobs())
        {
            var organization = organizations.FirstOrDefault(o => o.Name == spec.OrganizationName);
            if (organization is null)
            {
                continue;
            }

            if (await _context.JobPostings.AnyAsync(j => j.Title == spec.Title && j.OrganizationId == organization.Id, ct))
            {
                continue;
            }

            var recruiter = recruiters.FirstOrDefault(r => r.OrganizationId == organization.Id)
                            ?? recruiters.First();

            var job = new JobPosting
            {
                Title = spec.Title,
                Description = spec.Description,
                Responsibilities = spec.Responsibilities,
                OrganizationId = organization.Id,
                DepartmentId = organization.Departments.FirstOrDefault(d => d.Name == spec.DepartmentName)?.Id,
                City = spec.City,
                Country = spec.Country,
                WorkMode = spec.WorkMode,
                EmploymentType = spec.EmploymentType,
                Seniority = spec.Seniority,
                MinYearsExperience = spec.MinYears,
                RequiredEducation = spec.Education,
                SalaryMin = spec.SalaryMin,
                SalaryMax = spec.SalaryMax,
                Currency = spec.Currency,
                RankingStrategy = spec.Strategy,
                PostedByUserId = recruiter.Id,
                Status = JobStatus.Published,
                PublishedAt = DateTime.UtcNow.AddDays(-(publishedOffset * 3 + 2)),
                ClosingDate = DateTime.UtcNow.AddDays(45 - publishedOffset)
            };

            foreach (var (skillName, mandatory, weight, minYears) in spec.Skills)
            {
                if (skillIds.TryGetValue(skillName, out var skillId))
                {
                    job.RequiredSkills.Add(new JobRequiredSkill
                    {
                        SkillId = skillId,
                        IsMandatory = mandatory,
                        Weight = weight,
                        MinYearsExperience = minYears
                    });
                }
            }

            _context.JobPostings.Add(job);
            publishedOffset++;
        }

        await _context.SaveChangesAsync(ct);

        // One posting is deliberately left as a draft, so the recruiter's list
        // shows both states and the publish action has something to act on.
        var draft = await _context.JobPostings.OrderBy(j => j.Id).LastOrDefaultAsync(ct);
        if (draft is not null)
        {
            draft.Status = JobStatus.Draft;
            draft.PublishedAt = null;
            await _context.SaveChangesAsync(ct);
        }
    }

    // -----------------------------------------------------------------------
    // The hiring pipeline
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates applications and advances them through the pipeline.
    ///
    /// Who applies where is decided by the matching engine rather than at random:
    /// each candidate applies to the postings they score best against, which is
    /// what a real job board produces. How far each application advances is then
    /// derived from its rank within that posting, so strong candidates sit at the
    /// later stages and weak ones are rejected, exactly as a recruiter would have
    /// left them.
    /// </summary>
    private async Task SeedPipelineAsync(CancellationToken ct)
    {
        var jobs = await _context.JobPostings
            .Include(j => j.RequiredSkills).ThenInclude(s => s.Skill)
            .Where(j => j.Status == JobStatus.Published)
            .OrderBy(j => j.Id)
            .ToListAsync(ct);

        var profiles = await _context.CandidateProfiles
            .Include(c => c.User)
            .Include(c => c.Resumes).ThenInclude(r => r.ResumeSkills).ThenInclude(rs => rs.Skill)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        if (jobs.Count == 0 || profiles.Count == 0)
        {
            return;
        }

        var snapshots = profiles.ToDictionary(
            p => p.Id,
            p => MatchProfileMapper.ToCandidateSnapshot(
                p, p.Resumes.FirstOrDefault(r => r.IsPrimary) ?? p.Resumes.FirstOrDefault()));

        // Decide where each candidate applies: their three best-scoring postings.
        var jobProfiles = jobs.ToDictionary(j => j.Id, MatchProfileMapper.ToJobProfile);
        var applicationsByJob = jobs.ToDictionary(j => j.Id, _ => new List<int>());

        foreach (var profile in profiles)
        {
            var best = jobs
                .Select(job => new
                {
                    JobId = job.Id,
                    Score = _engine.Score(jobProfiles[job.Id], snapshots[profile.Id], job.RankingStrategy).Score
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.JobId)
                .Take(3)
                .ToList();

            foreach (var entry in best)
            {
                applicationsByJob[entry.JobId].Add(profile.Id);
            }
        }

        var daysAgo = 30;

        foreach (var job in jobs)
        {
            var applicantIds = applicationsByJob[job.Id];
            if (applicantIds.Count == 0)
            {
                continue;
            }

            var ranked = _engine.RankApplicants(
                jobProfiles[job.Id],
                applicantIds.Select(id => snapshots[id]).ToList(),
                job.RankingStrategy);

            for (var rank = 0; rank < ranked.Count; rank++)
            {
                var entry = ranked[rank];
                var explanation = entry.Explanation;
                var submittedAt = DateTime.UtcNow.AddDays(-daysAgo).AddHours(rank * 5);

                var status = DecideStatus(rank, ranked.Count, explanation.HasMandatoryGap);

                var application = new JobApplication
                {
                    JobPostingId = job.Id,
                    CandidateProfileId = entry.CandidateId,
                    ResumeId = profiles.First(p => p.Id == entry.CandidateId).Resumes.FirstOrDefault()?.Id,
                    Status = status,
                    SubmittedAt = submittedAt,
                    MatchScore = explanation.Score,
                    ScoreBreakdownJson = JsonSerializer.Serialize(MatchProfileMapper.ToDto(explanation), JsonOptions),
                    ScoredAt = submittedAt
                };

                _context.JobApplications.Add(application);
                await _context.SaveChangesAsync(ct);

                await WriteTimelineAsync(application, status, submittedAt, job.PostedByUserId, ct);
            }

            daysAgo = Math.Max(4, daysAgo - 2);
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Maps a candidate's rank within a posting to how far their application got.
    /// A mandatory gap always ends in rejection, which is what the cap exists to
    /// express, and the strongest applicant on the oldest posting reaches an offer.
    /// </summary>
    private static ApplicationStatus DecideStatus(int rank, int total, bool hasMandatoryGap)
    {
        if (hasMandatoryGap)
        {
            return ApplicationStatus.Rejected;
        }

        return rank switch
        {
            0 when total >= 4 => ApplicationStatus.InterviewScheduled,
            0 => ApplicationStatus.Shortlisted,
            1 => ApplicationStatus.Shortlisted,
            2 => ApplicationStatus.UnderReview,
            3 => ApplicationStatus.UnderReview,
            _ => ApplicationStatus.Submitted
        };
    }

    /// <summary>
    /// Writes the events an application would have accumulated on its way to the
    /// status it holds, so the candidate's timeline reads as a history rather
    /// than a single jump.
    /// </summary>
    private async Task WriteTimelineAsync(
        JobApplication application, ApplicationStatus finalStatus, DateTime submittedAt, int actorUserId, CancellationToken ct)
    {
        var path = finalStatus switch
        {
            ApplicationStatus.Submitted => Array.Empty<ApplicationStatus>(),
            ApplicationStatus.UnderReview => [ApplicationStatus.UnderReview],
            ApplicationStatus.Shortlisted => [ApplicationStatus.UnderReview, ApplicationStatus.Shortlisted],
            ApplicationStatus.InterviewScheduled =>
                [ApplicationStatus.UnderReview, ApplicationStatus.Shortlisted, ApplicationStatus.InterviewScheduled],
            ApplicationStatus.Rejected => [ApplicationStatus.UnderReview, ApplicationStatus.Rejected],
            _ => Array.Empty<ApplicationStatus>()
        };

        _context.ApplicationEvents.Add(new ApplicationEvent
        {
            JobApplicationId = application.Id,
            FromStatus = null,
            ToStatus = ApplicationStatus.Submitted,
            Note = "Application submitted.",
            OccurredAt = submittedAt
        });

        var previous = ApplicationStatus.Submitted;
        var occurredAt = submittedAt;

        foreach (var step in path)
        {
            occurredAt = occurredAt.AddDays(2);

            _context.ApplicationEvents.Add(new ApplicationEvent
            {
                JobApplicationId = application.Id,
                FromStatus = previous,
                ToStatus = step,
                Note = NoteFor(step),
                ActorUserId = actorUserId,
                OccurredAt = occurredAt
            });

            previous = step;
        }

        await _context.SaveChangesAsync(ct);
    }

    private static string NoteFor(ApplicationStatus status) => status switch
    {
        ApplicationStatus.UnderReview => "Screening started.",
        ApplicationStatus.Shortlisted => "Advanced to the hiring manager.",
        ApplicationStatus.InterviewScheduled => "First round interview arranged.",
        ApplicationStatus.Rejected => "Did not meet a mandatory requirement for this posting.",
        _ => string.Empty
    };

    /// <summary>Pulls the sentence mentioning a skill, so the UI can cite evidence.</summary>
    private static string? FindEvidence(string resumeText, string skill)
    {
        var sentences = resumeText.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var hit = sentences.FirstOrDefault(s => s.Contains(skill, StringComparison.OrdinalIgnoreCase));
        return hit is null ? null : (hit.Length > 240 ? hit[..240] : hit);
    }
}
