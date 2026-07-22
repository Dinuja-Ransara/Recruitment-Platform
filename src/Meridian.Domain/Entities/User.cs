using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// An authenticated principal. A user may hold several roles, which is why the
/// relationship is many-to-many rather than a single RoleId column: a hiring
/// manager at a client organisation is frequently also a recruiter.
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public CandidateProfile? CandidateProfile { get; set; }
}
