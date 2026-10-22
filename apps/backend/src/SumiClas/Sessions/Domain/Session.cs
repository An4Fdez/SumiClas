using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

/// <summary>
/// A login. The aggregate owns its chain of refresh tokens, so reuse detection and revocation are decided here and a
/// concurrency token on the root keeps two simultaneous rotations from both succeeding.
/// </summary>
public sealed class Session : AggregateRoot
{
    /// <summary>
    /// Tokens replaced longer ago than this (or expired) are dropped on the next rotation to bound the rows loaded per
    /// session. A replay of a token older than this is <see cref="SessionRotation.Invalid"/>, not <see cref="SessionRotation.Reused"/>.
    /// </summary>
    public static readonly TimeSpan ReplacedTokenRetention = TimeSpan.FromHours(24);

    public static readonly TimeSpan RefreshGrace = TimeSpan.FromSeconds(10);

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>Set on every successful rotation so the root row changes and the optimistic concurrency token is checked; otherwise only child rows would change.</summary>
    public DateTimeOffset? LastRotatedAt { get; private set; }

    private Session()
    {
    }

    public Session(
        Guid id,
        Guid userId,
        DateTimeOffset createdAt,
        DateTimeOffset? revokedAt,
        DateTimeOffset? lastRotatedAt,
        IEnumerable<RefreshToken> refreshTokens) : base(id)
    {
        UserId = userId;
        CreatedAt = createdAt;
        RevokedAt = revokedAt;
        LastRotatedAt = lastRotatedAt;
        _refreshTokens.AddRange(refreshTokens);
    }

    public static Session Create(Guid userId, string tokenHash, IClock clock)
    {
        if (userId == Guid.Empty)
        {
            throw new SessionExceptions.UserRequired();
        }

        Validate(tokenHash);

        return new Session(
            Guid.NewGuid(),
            userId,
            clock.UtcNow,
            null,
            null,
            [RefreshToken.Create(tokenHash, clock)]);
    }

    private static void Validate(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new SessionExceptions.TokenHashRequired();
        }
    }

    public bool IsActive(IClock clock) => RevokedAt is null;

    public bool HasToken(string tokenHash) => _refreshTokens.Any(token => token.TokenHash == tokenHash);

    /// <summary>Idempotent: the first revocation date is kept.</summary>
    public void Revoke(IClock clock)
    {
        RevokedAt ??= clock.UtcNow;
    }

    public SessionRotation Rotate(string tokenHash, string newTokenHash, IClock clock)
    {
        Validate(newTokenHash);
        Validate(tokenHash);

        if (RevokedAt is not null)
        {
            return new SessionRotation.Invalid();
        }

        var current = _refreshTokens.FirstOrDefault(token => token.TokenHash == tokenHash);
        if (current is null)
        {
            return new SessionRotation.Invalid();
        }

        if (current.ReplacedAt is not null)
        {
            if (current.WasReplacedWithin(clock, RefreshGrace))
            {
                return new SessionRotation.Invalid();
            }

            Revoke(clock);
            return new SessionRotation.Reused();
        }

        if (!current.IsActive(clock))
        {
            return new SessionRotation.Invalid();
        }

        var successor = RefreshToken.Create(newTokenHash, clock);
        current.ReplaceWith(successor.Id, clock);
        _refreshTokens.Add(successor);
        LastRotatedAt = clock.UtcNow;
        Prune(clock, current.Id, successor.Id);
        return new SessionRotation.Rotated(successor);
    }

    private void Prune(IClock clock, Guid keepPrevious, Guid keepCurrent)
    {
        var now = clock.UtcNow;
        _refreshTokens.RemoveAll(token =>
            token.Id != keepPrevious
            && token.Id != keepCurrent
            && (token.ExpiresAt <= now || (token.ReplacedAt is { } replacedAt && now - replacedAt > ReplacedTokenRetention)));
    }

}
