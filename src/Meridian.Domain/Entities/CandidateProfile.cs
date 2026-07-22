using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

/// <summary>
/// The job-seeker facing half of a user account. Kept separate from User so that
/// recruiters and administrators do not carry a table's worth of null columns.
/// </summary>
public class CandidateProfile : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal YearsOfExperience { get; set; }
    public EducationLevel HighestEducation { get; set; } = EducationLevel.Unspecified;
    public bool IsOpenToRemote { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
