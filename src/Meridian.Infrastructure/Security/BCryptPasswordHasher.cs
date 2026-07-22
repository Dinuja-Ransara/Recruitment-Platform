using Meridian.Application.Common.Interfaces;

namespace Meridian.Infrastructure.Security;

/// <summary>
/// BCrypt-based password hashing.
///
/// BCrypt was chosen over a raw SHA-256 hash because it is deliberately slow and
/// salts every hash automatically, which defeats both rainbow tables and the
/// GPU-parallel brute force that makes fast hashes unsuitable for passwords. The
/// work factor is raised as hardware improves without invalidating stored hashes,
/// because the cost is encoded inside each hash string.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// 2^12 rounds. Roughly 250ms per hash on current hardware, which is tolerable
    /// for a login and expensive for an attacker running billions of guesses.
    /// </summary>
    private const int WorkFactor = 12;

    public string Hash(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
        {
            throw new ArgumentException("Password must not be empty.", nameof(plainTextPassword));
        }

        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
    }

    public bool Verify(string plainTextPassword, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // A malformed hash in the database must read as a failed login rather
            // than crash the authentication endpoint.
            return false;
        }
    }
}
