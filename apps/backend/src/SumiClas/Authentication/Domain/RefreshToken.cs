using SumiClas.Shared.Domain;

namespace SumiClas.Authentication.Domain;

/// <summary>
/// One link of a session chain. Only the hash of the token value is kept. Each rotation replaces a token with a successor
/// of the same family, so revoking the family closes the whole session.
/// </summary>
public sealed class RefreshToken : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset? ReplacedAt { get; private set; }
    public Guid? ReplacedById { get; private set; }

    private RefreshToken(Guid id, Guid userId, Guid familyId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt) : base(id)
    {
        UserId = userId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public static Result<RefreshToken> Create(Guid id, Guid userId, Guid familyId, string tokenHash, IClock clock, TimeSpan lifetime)
    {
        if (id == Guid.Empty)
        {
            return Result<RefreshToken>.Failure(RefreshTokenErrors.IdRequired);
        }
        if (userId == Guid.Empty)
        {
            return Result<RefreshToken>.Failure(RefreshTokenErrors.UserRequired);
        }
        if (familyId == Guid.Empty)
        {
            return Result<RefreshToken>.Failure(RefreshTokenErrors.FamilyRequired);
        }
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result<RefreshToken>.Failure(RefreshTokenErrors.HashRequired);
        }
        if (lifetime <= TimeSpan.Zero)
        {
            return Result<RefreshToken>.Failure(RefreshTokenErrors.LifetimeInvalid);
        }

        var now = clock.UtcNow;
        return Result<RefreshToken>.Success(new RefreshToken(id, userId, familyId, tokenHash, now, now + lifetime));
    }

    public bool IsActive(IClock clock)
        => RevokedAt is null && ReplacedAt is null && clock.UtcNow < ExpiresAt;

    public Result MarkReplaced(Guid successorId, IClock clock)
    {
        if (!IsActive(clock))
        {
            return Result.Failure(RefreshTokenErrors.NotActive);
        }

        ReplacedAt = clock.UtcNow;
        ReplacedById = successorId;
        return Result.Success();
    }

    public void Revoke(IClock clock)
    {
        RevokedAt ??= clock.UtcNow;
    }

    public bool WasReplacedWithin(IClock clock, TimeSpan grace)
        => ReplacedAt is { } replacedAt && clock.UtcNow - replacedAt <= grace;
}
