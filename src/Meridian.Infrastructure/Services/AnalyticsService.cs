using System.Text.Json;
using Meridian.Application.Common.Interfaces;
using Meridian.Application.Dtos.Analytics;
using Meridian.Application.Dtos.Applications;
using Meridian.Domain.Enums;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// Derives recruitment analytics from the application pipeline.
///
/// Nothing here is stored or incrementally maintained. Counters that are updated
/// alongside the events they count drift, and a figure that disagrees with the
/// list beneath it destroys confidence in the whole dashboard. Computing on
/// request is affordable at this scale and cannot disagree with itself.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    /// <summary>
    /// Stages in pipeline order. The funnel counts applications that reached each
    /// stage or passed beyond it, which is why order matters here.
    /// </summary>
    private static readonly (ApplicationStatus Status, string Label)[] FunnelStages =
    [
        (ApplicationStatus.Submitted, "Applied"),
        (ApplicationStatus.UnderReview, "Screened"),
        (ApplicationStatus.Shortlisted, "Shortlisted"),
        (ApplicationStatus.InterviewScheduled, "Interviewing"),
        (ApplicationStatus.OfferExtended, "Offered"),
        (ApplicationStatus.Hired, "Hired")
    ];

    private readonly MeridianDbContext _context;

    public AnalyticsService(MeridianDbContext context)
    {
        _context = context;
    }

    public async Task<RecruitmentAnalyticsDto> GetForOrganizationAsync(int? organizationId, CancellationToken ct = default)
    {
        var postings = await _context.JobPostings
            .AsNoTracking()
            .Include(j => j.RequiredSkills).ThenInclude(s => s.Skill)
            .Where(j => organizationId == null || j.OrganizationId == organizationId)
            .ToListAsync(ct);

        var postingIds = postings.Select(p => p.Id).ToList();

        var applications = await _context.JobApplications
            .AsNoTracking()
            .Where(a => postingIds.Contains(a.JobPostingId))
            .Select(a => new
            {
                a.Id,
                a.JobPostingId,
                a.Status,
                a.MatchScore,
                a.ScoreBreakdownJson
            })
            .ToListAsync(ct);

        var total = applications.Count;

        // Rejected and withdrawn applications left the pipeline; everything else
        // is still in play.
        var active = applications.Count(a =>
            a.Status != ApplicationStatus.Rejected &&
            a.Status != ApplicationStatus.Withdrawn &&
            a.Status != ApplicationStatus.Hired);

        var scored = applications.Where(a => a.MatchScore.HasValue).ToList();

        var mandatoryGapRejections = applications.Count(a =>
            a.Status == ApplicationStatus.Rejected && HasMandatoryGap(a.ScoreBreakdownJson));

        var rejected = applications.Count(a => a.Status == ApplicationStatus.Rejected);

        // The furthest stage each application reached, taken from its timeline
        // rather than its current status. A rejected candidate who was
        // interviewed first did pass through screening and shortlisting, and a
        // funnel that ignored that would understate every stage but the first.
        // Rejection and withdrawal are exits rather than stages, so they are
        // excluded from the maximum.
        var applicationIds = applications.Select(a => a.Id).ToList();

        var eventsByApplication = await _context.ApplicationEvents
            .AsNoTracking()
            .Where(e => applicationIds.Contains(e.JobApplicationId))
            .Where(e => e.ToStatus != ApplicationStatus.Rejected && e.ToStatus != ApplicationStatus.Withdrawn)
            .GroupBy(e => e.JobApplicationId)
            .Select(g => g.Max(e => e.ToStatus))
            .ToListAsync(ct);

        var furthestStageReached = eventsByApplication;

        return new RecruitmentAnalyticsDto
        {
            PublishedPostings = postings.Count(p => p.Status == JobStatus.Published),
            DraftPostings = postings.Count(p => p.Status == JobStatus.Draft),
            TotalApplications = total,
            ActiveApplications = active,
            AverageMatchScore = scored.Count == 0 ? 0 : Math.Round(scored.Average(a => a.MatchScore!.Value), 1),
            MandatoryGapRejectionRate = rejected == 0 ? 0 : Math.Round(mandatoryGapRejections * 100.0 / rejected, 1),

            Funnel = FunnelStages
                .Select(stage =>
                {
                    var count = furthestStageReached.Count(reached => reached >= stage.Status);

                    return new FunnelStageDto(
                        stage.Status,
                        stage.Label,
                        count,
                        total == 0 ? 0 : Math.Round(count * 100.0 / total, 1));
                })
                .ToList(),

            PostingPerformance = postings
                .Where(p => p.Status == JobStatus.Published)
                .Select(p =>
                {
                    var forPosting = applications.Where(a => a.JobPostingId == p.Id).ToList();
                    var scoredForPosting = forPosting.Where(a => a.MatchScore.HasValue).ToList();

                    return new PostingPerformanceDto
                    {
                        JobPostingId = p.Id,
                        Title = p.Title,
                        City = p.City,
                        Applications = forPosting.Count,
                        Shortlisted = forPosting.Count(a => (int)a.Status >= (int)ApplicationStatus.Shortlisted
                                                            && a.Status != ApplicationStatus.Rejected
                                                            && a.Status != ApplicationStatus.Withdrawn),
                        AverageScore = scoredForPosting.Count == 0
                            ? 0
                            : Math.Round(scoredForPosting.Average(a => a.MatchScore!.Value), 1),
                        TopScore = scoredForPosting.Count == 0
                            ? 0
                            : Math.Round(scoredForPosting.Max(a => a.MatchScore!.Value), 1)
                    };
                })
                .OrderByDescending(p => p.Applications)
                .ToList(),

            ByLocation = postings
                .GroupBy(p => string.IsNullOrWhiteSpace(p.City) ? p.Country : $"{p.City}, {p.Country}")
                .Select(g => new LocationBreakdownDto(
                    g.Key,
                    g.Count(),
                    applications.Count(a => g.Select(p => p.Id).Contains(a.JobPostingId))))
                .OrderByDescending(l => l.Applications)
                .ToList(),

            MostRequestedSkills = postings
                .SelectMany(p => p.RequiredSkills)
                .Where(s => s.Skill is not null)
                .GroupBy(s => s.Skill!.Name)
                .Select(g => new SkillDemandDto(g.Key, g.Count(), g.Count(s => s.IsMandatory)))
                .OrderByDescending(s => s.PostingsRequiring)
                .ThenBy(s => s.Skill)
                .Take(8)
                .ToList()
        };
    }

    private static bool HasMandatoryGap(string? breakdownJson)
    {
        if (string.IsNullOrWhiteSpace(breakdownJson))
        {
            return false;
        }

        try
        {
            return JsonSerializer.Deserialize<MatchExplanationDto>(breakdownJson)?.HasMandatoryGap ?? false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
