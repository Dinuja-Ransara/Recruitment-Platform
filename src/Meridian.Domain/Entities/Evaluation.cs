using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// A hiring manager's structured scoring of a candidate. Separate from
/// InterviewFeedback because a manager evaluates the application as a whole,
/// potentially across several interview rounds.
/// </summary>
public class Evaluation : BaseEntity
{
    public int JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public int EvaluatorUserId { get; set; }
    public User Evaluator { get; set; } = null!;

    /// <summary>Each score is out of 10.</summary>
    public int TechnicalScore { get; set; }
    public int CommunicationScore { get; set; }
    public int CultureFitScore { get; set; }

    public string? Comments { get; set; }

    /// <summary>Unweighted mean of the three component scores.</summary>
    public double OverallScore => (TechnicalScore + CommunicationScore + CultureFitScore) / 3.0;
}
