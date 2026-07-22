using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("Interviews");

        builder.Property(i => i.MeetingUrl).HasMaxLength(500);
        builder.Property(i => i.LocationNote).HasMaxLength(300);
        builder.Property(i => i.CalendarUid).IsRequired().HasMaxLength(60);

        builder.HasOne(i => i.JobApplication)
            .WithMany(a => a.Interviews)
            .HasForeignKey(i => i.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Interviewer)
            .WithMany()
            .HasForeignKey(i => i.InterviewerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Availability checks query an interviewer diary by time window.
        builder.HasIndex(i => new { i.InterviewerUserId, i.ScheduledStart });
    }
}

public class InterviewFeedbackConfiguration : IEntityTypeConfiguration<InterviewFeedback>
{
    public void Configure(EntityTypeBuilder<InterviewFeedback> builder)
    {
        builder.ToTable("InterviewFeedbacks");

        builder.Property(f => f.Strengths).HasMaxLength(2000);
        builder.Property(f => f.Concerns).HasMaxLength(2000);

        builder.HasOne(f => f.Interview)
            .WithOne(i => i.Feedback)
            .HasForeignKey<InterviewFeedback>(f => f.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.SubmittedBy)
            .WithMany()
            .HasForeignKey(f => f.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("Evaluations");

        builder.Property(e => e.Comments).HasMaxLength(2000);

        // OverallScore is derived from the three component scores in the domain
        // model, so it is computed on read and never stored.
        builder.Ignore(e => e.OverallScore);

        builder.HasOne(e => e.JobApplication)
            .WithMany(a => a.Evaluations)
            .HasForeignKey(e => e.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Evaluator)
            .WithMany()
            .HasForeignKey(e => e.EvaluatorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
