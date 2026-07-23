using Meridian.Application.Dtos.Analytics;

namespace Meridian.Application.Common.Interfaces;

public interface IAnalyticsService
{
    /// <summary>
    /// Recruitment analytics scoped to one organisation, or across every
    /// organisation when no identifier is supplied, which is the administrator's view.
    /// </summary>
    Task<RecruitmentAnalyticsDto> GetForOrganizationAsync(int? organizationId, CancellationToken ct = default);
}
