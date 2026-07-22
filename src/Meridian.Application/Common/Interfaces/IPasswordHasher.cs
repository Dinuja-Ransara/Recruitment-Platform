namespace Meridian.Application.Common.Interfaces;

/// <summary>
/// Abstracts the hashing algorithm so it can be replaced without touching the
/// authentication service. The concrete implementation uses BCrypt with a work
/// factor, never a bare SHA hash.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string passwordHash);
}
