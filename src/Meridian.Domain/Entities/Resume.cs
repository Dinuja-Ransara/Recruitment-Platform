using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// An uploaded CV. RawText is the extracted plain text produced by the parser
/// selected through ResumeParserFactory; it is what the matching engine indexes.
/// </summary>
public class Resume : BaseEntity
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }

    /// <summary>pdf, docx, txt, json or xml. Determines which parser is built.</summary>
    public string SourceFormat { get; set; } = string.Empty;

    public string RawText { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime? ParsedAt { get; set; }

    public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
}
