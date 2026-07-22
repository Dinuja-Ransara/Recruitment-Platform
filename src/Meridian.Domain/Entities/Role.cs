using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// A security role. Roles are data rather than a hard-coded enum so that the
/// administration portal can manage them without a redeployment.
/// </summary>
public class Role : BaseEntity
{
    public const string Candidate = "Candidate";
    public const string Recruiter = "Recruiter";
    public const string HiringManager = "HiringManager";
    public const string Administrator = "Administrator";

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
