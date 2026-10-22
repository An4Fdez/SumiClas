using Microsoft.AspNetCore.Identity;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Infrastructure;

/// <summary>PBKDF2 with a random salt in the versioned format of ASP.NET Core Identity. Thread-safe: register it as a singleton.</summary>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();
    private readonly string _throwawayHash;

    public IdentityPasswordHasher()
    {
        _throwawayHash = Generate(Password.Create(Guid.NewGuid().ToString("N")));
    }

    // The Identity hasher never reads the user it is given, so none is passed.
    public string Generate(Password password) => _hasher.HashPassword(null!, password.Value);

    public PasswordVerification Verify(string? hash, string password)
    {
        if (hash is null)
        {
            _hasher.VerifyHashedPassword(null!, _throwawayHash, password);
            return PasswordVerification.Failed;
        }

        try
        {
            return _hasher.VerifyHashedPassword(null!, hash, password) switch
            {
                PasswordVerificationResult.Success => PasswordVerification.Success,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordVerification.SuccessRehashNeeded,
                _ => PasswordVerification.Failed,
            };
        }
        catch (FormatException)
        {
            // A stored hash that is not valid Base64 is a corrupted credential: it can never match.
            return PasswordVerification.Failed;
        }
    }
}
