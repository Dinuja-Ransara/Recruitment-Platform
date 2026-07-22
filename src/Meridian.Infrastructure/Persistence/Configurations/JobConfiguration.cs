using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("JobPostings");

        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Description).HasColumnType("nvarchar(max)");
        builder.Property(j => j.Responsibilities).HasColumnType("nvarchar(max)");
        builder.Property(j => j.City).HasMaxLength(100);
        builder.Property(j => j.Country).HasMaxLength(100);
        builder.Property(j => j.Currency).HasMaxLength(3);
        builder.Property(j => j.MinYearsExperience).HasPrecision(4, 1);
        builder.Property(j => j.SalaryMin).HasPrecision(18, 2);
        builder.Property(j => j.SalaryMax).HasPrecision(18, 2);

        builder.HasOne(j => j.Organization)
            .WithMany(o => o.JobPostings)
            .HasForeignKey(j => j.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Department)
            .WithMany(d => d.JobPostings)
            .HasForeignKey(j => j.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.PostedBy)
            .WithMany()
            .HasForeignKey(j => j.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // The public job board filters on status and closing date on every request.
        builder.HasIndex(j => new { j.Status, j.ClosingDate });
        builder.HasIndex(j => j.OrganizationId);
    }
}

public class JobRequiredSkillConfiguration : IEntityTypeConfiguration<JobRequiredSkill>
{
    public void Configure(EntityTypeBuilder<JobRequiredSkill> builder)
    {
        builder.ToTable("JobRequiredSkills");

        builder.Property(s => s.MinYearsExperience).HasPrecision(4, 1);

        builder.HasOne(s => s.JobPosting)
            .WithMany(j => j.RequiredSkills)
            .HasForeignKey(s => s.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Skill)
            .WithMany(sk => sk.JobRequiredSkills)
            .HasForeignKey(s => s.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.JobPostingId, s.SkillId }).IsUnique();
    }
}
