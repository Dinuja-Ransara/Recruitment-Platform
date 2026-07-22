using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? CostCentre { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}
