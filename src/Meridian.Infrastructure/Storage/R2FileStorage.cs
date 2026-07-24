using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Meridian.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Meridian.Infrastructure.Storage;

/// <summary>
/// Cloud storage for resumes and supporting documents, backed by Cloudflare
/// R2. R2 exposes an S3-compatible API, so the official AWS SDK talks to it
/// directly against R2's own endpoint, no Cloudflare-specific SDK needed.
///
/// The bucket is private. Every download goes through a pre-signed URL that
/// expires, rather than a permanent public link, because a CV is personal
/// data and should not sit at a guessable, permanent address.
/// </summary>
public class R2FileStorage : IFileStorage
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;

    public R2FileStorage(IOptions<R2Settings> options)
    {
        var settings = options.Value;
        _bucket = settings.BucketName;

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{settings.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
        };

        _client = new AmazonS3Client(
            new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
            config);
    }

    public async Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        }, ct);

        return key;
    }

    public Task<string> GetDownloadUrlAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        var url = _client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.GET,
        });

        return Task.FromResult(url);
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await _client.DeleteObjectAsync(_bucket, key, ct);
    }
}
