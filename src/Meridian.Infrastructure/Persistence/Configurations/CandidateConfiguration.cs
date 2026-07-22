using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.ToTable("CandidateProfiles");

        builder.Property(c => c.Headline).HasMaxLength(200);
        builder.Property(c => c.Summary).HasMaxLength(4000);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Country).HasMaxLength(100);
        builder.Property(c => c.LinkedInUrl).HasMaxLength(250);
        builder.Property(c => c.PortfolioUrl).HasMaxLength(250);
        builder.Property(c => c.YearsOfExperience).HasPrecision(4, 1);

        // One profile per user, enforced at the database level.
        builder.HasOne(c => c.User)
            .WithOne(u => u.CandidateProfile)
            .HasForeignKey<CandidateProfile>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId).IsUnique();
    }
}

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("Resumes");

        builder.Property(r => r.FileName).IsRequired().HasMaxLength(260);
        builder.Property(r => r.StoredPath).IsRequired().HasMaxLength(500);
        builder.Property(r => r.ContentType).HasMaxLength(120);
        builder.Property(r => r.SourceFormat).HasMaxLength(10);

        // Extracted CV text feeds the TF-IDF index, so it must not be truncated.
        builder.Property(r => r.RawText).HasColumnType("nvarchar(max)");

        builder.HasOne(r => r.CandidateProfile)
            .WithMany(c => c.Resumes)
            .HasForeignKey(r => r.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.CandidateProfileId, r.IsPrimary });
    }
}

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.Property(s => s.Name).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Category).HasMaxLength(80);
        builder.HasIndex(s => s.Name).IsUnique();
    }
}

public class SkillAliasConfiguration : IEntityTypeConfiguration<SkillAlias>
{
    public void Configure(EntityTypeBuilder<SkillAlias> builder)
    {
        builder.ToTable("SkillAliases");
        builder.Property(a => a.Alias).IsRequired().HasMaxLength(120);

        builder.HasOne(a => a.Skill)
            .WithMany(s => s.Aliases)
            .HasForeignKey(a => a.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.Alias).IsUnique();
    }
}

public class ResumeSkillConfiguration : IEntityTypeConfiguration<ResumeSkill>
{
    public void Configure(EntityTypeBuilder<ResumeSkill> builder)
    {
        builder.ToTable("ResumeSkills");

        builder.Property(rs => rs.YearsOfExperience).HasPrecision(4, 1);
        builder.Property(rs => rs.EvidenceSnippet).HasMaxLength(500);

        builder.HasOne(rs => rs.Resume)
            .WithMany(r => r.ResumeSkills)
            .HasForeignKey(rs => rs.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Skill)
            .WithMany(s => s.ResumeSkills)
            .HasForeignKey(rs => rs.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rs => new { rs.ResumeId, rs.SkillId }).IsUnique();
    }
}
