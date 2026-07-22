namespace Meridian.Infrastructure.Security;

/// <summary>
/// Bound from the "Jwt" section of configuration. Kept as a typed object rather
/// than read through string keys so that a missing setting fails at startup
/// instead of at the first login attempt.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 120;

    /// <summary>The key shipped in source control, which must never sign real tokens.</summary>
    public const string DevelopmentPlaceholderKey =
        "meridian-local-development-signing-key-change-before-any-deployment";

    public void Validate(bool isDevelopment)
    {
        if (string.IsNullOrWhiteSpace(SigningKey) || SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey is missing or shorter than 32 characters. HMAC-SHA256 requires a key at least as long as its output.");
        }

        // Outside development the placeholder is a hard failure rather than a
        // warning. It is committed to a public repository, so anything signed
        // with it could be forged by anyone who has read the source.
        if (!isDevelopment && SigningKey == DevelopmentPlaceholderKey)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey is still the development placeholder, which is published in source control. "
                + "Set a unique key for this environment before starting the application.");
        }

        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must both be configured.");
        }
    }
}
