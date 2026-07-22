using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// A client company of the consultancy. The platform is multi-region: postings,
/// users and departments all hang off an organisation.
/// </summary>
public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}
