namespace Meridian.Domain.Entities;

/// <summary>
/// Join entity for the many-to-many between User and Role. Declared explicitly
/// rather than left implicit so the assignment itself can be audited.
/// </summary>
public class UserRole
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
