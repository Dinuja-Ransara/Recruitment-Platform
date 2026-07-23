using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Persistence;

/// <summary>
/// Maintenance operations that run against an existing database.
/// </summary>
public static class DatabaseMaintenance
{
    /// <summary>
    /// Empties every table so the seeders can rebuild the demonstration data.
    ///
    /// Rows are deleted rather than the database being dropped. On shared hosting
    /// the application's SQL login can read and write inside its own database but
    /// has no CREATE DATABASE permission, so dropping the database leaves one
    /// that nothing in the application is able to recreate.
    ///
    /// TRUNCATE is not used either, because it is refused on any table with an
    /// incoming foreign key. DELETE in dependency order works everywhere, and the
    /// identity reseed keeps the demonstration ids stable at 1, 2, 3 and so on,
    /// which matters because the printed demonstration steps refer to them.
    /// </summary>
    public static async Task ClearAllDataAsync(MeridianDbContext context, CancellationToken ct = default)
    {
        // Children before parents.
        var tablesInDeleteOrder = new[]
        {
            "ApplicationEvents",
            "InterviewFeedbacks",
            "Interviews",
            "Evaluations",
            "JobApplications",
            "JobRequiredSkills",
            "JobPostings",
            "ResumeSkills",
            "Resumes",
            "CandidateProfiles",
            "Notifications",
            "AuditLogs",
            "UserRoles",
            "Users",
            "Departments",
            "Organizations",
            "SkillAliases",
            "Skills",
            "Roles"
        };

        foreach (var table in tablesInDeleteOrder)
        {
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]", ct);

            // Reseed only where an identity column exists. UserRoles has a
            // composite key and no identity, so it is skipped.
            if (table != "UserRoles")
            {
                await context.Database.ExecuteSqlRawAsync(
                    $"IF OBJECTPROPERTY(OBJECT_ID('{table}'), 'TableHasIdentity') = 1 " +
                    $"DBCC CHECKIDENT ('[{table}]', RESEED, 0)", ct);
            }
        }
    }
}
