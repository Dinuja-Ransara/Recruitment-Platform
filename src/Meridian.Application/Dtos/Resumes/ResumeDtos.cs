namespace Meridian.Application.Dtos.Resumes;

public record ResumeSummaryDto
{
    public int Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SourceFormat { get; init; } = string.Empty;
    public long SizeInBytes { get; init; }
    public bool IsPrimary { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>Pre-signed, time-limited. Never a permanent public link.</summary>
    public string DownloadUrl { get; init; } = string.Empty;
}
