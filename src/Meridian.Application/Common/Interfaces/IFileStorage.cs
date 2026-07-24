namespace Meridian.Application.Common.Interfaces;

/// <summary>
/// Abstracts where an uploaded file physically lands. The application layer
/// asks for a resume to be stored and later fetched; it never knows whether
/// that means Cloudflare R2, Azure Blob Storage, or a local disk, which is
/// what keeps the storage provider swappable without touching business logic.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Uploads a file and returns the key it was stored under. The key is
    /// what gets persisted on the entity (<c>Resume.StoredPath</c>), not a
    /// URL, because a private bucket has no public URL to store.
    /// </summary>
    Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Produces a time-limited URL a browser can download the file from
    /// directly, without the API proxying the bytes through itself.
    /// </summary>
    Task<string> GetDownloadUrlAsync(string key, TimeSpan expiry, CancellationToken ct = default);

    Task DeleteAsync(string key, CancellationToken ct = default);
}
