using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.Property(a => a.CoverLetter).HasMaxLength(4000);

        // The explainability payload produced by the matching engine, stored as
        // JSON so the score breakdown can evolve without a schema migration.
        builder.Property(a => a.ScoreBreakdownJson).HasColumnType("nvarchar(max)");

        // Optimistic concurrency: a recruiter shortlisting and a hiring manager
        // rejecting the same application must not silently overwrite each other.
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.HasOne(a => a.JobPosting)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CandidateProfile)
            .WithMany(c => c.Applications)
            .HasForeignKey(a => a.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Resume)
            .WithMany()
            .HasForeignKey(a => a.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);

        // A candidate may apply to a given posting only once.
        builder.HasIndex(a => new { a.JobPostingId, a.CandidateProfileId }).IsUnique();
        builder.HasIndex(a => a.Status);
    }
}

public class ApplicationEventConfiguration : IEntityTypeConfiguration<ApplicationEvent>
{
    public void Configure(EntityTypeBuilder<ApplicationEvent> builder)
    {
        builder.ToTable("ApplicationEvents");

        builder.Property(e => e.Note).HasMaxLength(1000);

        builder.HasOne(e => e.JobApplication)
            .WithMany(a => a.Events)
            .HasForeignKey(e => e.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ActorUser)
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.JobApplicationId, e.OccurredAt });
    }
}
