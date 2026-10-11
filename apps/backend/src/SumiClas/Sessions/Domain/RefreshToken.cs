using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

/// <summary>
/// One link of a session's token chain, owned by <see cref="Session"/>. Only the hash of the token value is kept.
/// </summary>
public sealed class RefreshToken : Entity
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(14);

    public string TokenHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ReplacedAt { get; private set; }
    public Guid? ReplacedById { get; private set; }

    private RefreshToken(Guid id, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt) : base(id)
    {
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    internal static RefreshToken Create(string tokenHash, IClock clock)
    {
        var now = clock.UtcNow;
        return new RefreshToken(Guid.NewGuid(), tokenHash, now, now + Lifetime);
    }

    public bool IsActive(IClock clock)
        => ReplacedAt is null && clock.UtcNow < ExpiresAt;

    public bool WasReplacedWithin(IClock clock, TimeSpan grace)
        => ReplacedAt is { } replacedAt && clock.UtcNow - replacedAt <= grace;

    internal void ReplaceWith(Guid successorId, IClock clock)
    {
        ReplacedAt = clock.UtcNow;
        ReplacedById = successorId;
    }
}
