namespace Meridian.Infrastructure.Storage;

/// <summary>
/// Bound from the "R2" section of configuration. R2 is Cloudflare's object
/// storage, chosen over Azure Blob or S3 because the platform is already on
/// Cloudflare for the Pages deployment and the Workers AI assistant, one
/// account rather than three.
/// </summary>
public class R2Settings
{
    public const string SectionName = "R2";

    /// <summary>The Cloudflare account id. The R2 S3-compatible endpoint is
    /// always https://{AccountId}.r2.cloudflarestorage.com.</summary>
    public string AccountId { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "meridian-resumes";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AccountId)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey);
}
