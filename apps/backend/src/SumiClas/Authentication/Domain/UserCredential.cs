using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;

namespace SumiClas.Authentication.Domain;

/// <summary>
/// How a user proves who they are. Its <c>Id</c> is the id of the user it belongs to (same value, no foreign key:
/// modules reference each other by id only). Only the hash of the password is kept, never the password.
/// </summary>
public sealed class UserCredential : AggregateRoot
{
    public string PasswordHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }

    private UserCredential(Guid id, string passwordHash, DateTimeOffset createdAt) : base(id)
    {
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
        ChangedAt = createdAt;
    }

    /// <summary>
    /// <paramref name="fullName"/> and <paramref name="email"/> are not fields of the credential: they only travel in the
    /// <see cref="UserCredentialCreatedDomainEvent"/> so the Users module can create the profile (it validates them).
    /// </summary>
    public static Result<UserCredential> Create(Guid userId, string passwordHash, string fullName, string email, IClock clock)
    {
        if (userId == Guid.Empty)
        {
            return Result<UserCredential>.Failure(UserCredentialErrors.IdRequired);
        }
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result<UserCredential>.Failure(UserCredentialErrors.PasswordHashRequired);
        }

        var credential = new UserCredential(userId, passwordHash, clock.UtcNow);
        credential.AddDomainEvent(new UserCredentialCreatedDomainEvent(userId, fullName, email, clock));
        return Result<UserCredential>.Success(credential);
    }

    /// <summary>Stores a newer hash of the same password (the hasher asks for it when its parameters have been strengthened).</summary>
    public Result ReplacePasswordHash(string passwordHash, IClock clock)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure(UserCredentialErrors.PasswordHashRequired);
        }

        PasswordHash = passwordHash;
        ChangedAt = clock.UtcNow;
        return Result.Success();
    }
}
