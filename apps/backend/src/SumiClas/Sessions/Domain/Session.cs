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

    private Session(Guid id, Guid userId, DateTimeOffset createdAt) : base(id)
    {
        UserId = userId;
        CreatedAt = createdAt;
    }

    public static Result<Session> Create(Guid userId, string tokenHash, IClock clock)
    {
        if (userId == Guid.Empty)
        {
            return Result<Session>.Failure(SessionErrors.UserRequired);
        }

        var validation = Validate(tokenHash);
        if (validation.IsFailure)
        {
            return Result<Session>.Failure(validation.Error!);
        }

        var session = new Session(Guid.NewGuid(), userId, clock.UtcNow);
        session._refreshTokens.Add(RefreshToken.Create(tokenHash, clock));
        return Result<Session>.Success(session);
    }

    private static Result Validate(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result.Failure(SessionErrors.TokenHashRequired);
        }
        return Result.Success();
    }

    public bool IsActive(IClock clock) => RevokedAt is null;

    public bool HasToken(string tokenHash) => _refreshTokens.Any(token => token.TokenHash == tokenHash);

    /// <summary>Idempotent: the first revocation date is kept.</summary>
    public void Revoke(IClock clock)
    {
        RevokedAt ??= clock.UtcNow;
    }

    public Result<SessionRotation> Rotate(string tokenHash, string newTokenHash, IClock clock)
    {
        var validation = Validate(newTokenHash);
        if (validation.IsFailure)
        {
            return Result<SessionRotation>.Failure(validation.Error!);
        }
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result<SessionRotation>.Failure(SessionErrors.TokenHashRequired);
        }

        if (RevokedAt is not null)
        {
            return Done(new SessionRotation.Invalid());
        }

        var current = _refreshTokens.FirstOrDefault(token => token.TokenHash == tokenHash);
        if (current is null)
        {
            return Done(new SessionRotation.Invalid());
        }

        if (current.ReplacedAt is not null)
        {
            if (current.WasReplacedWithin(clock, RefreshGrace))
            {
                return Done(new SessionRotation.Invalid());
            }

            Revoke(clock);
            return Done(new SessionRotation.Reused());
        }

        if (!current.IsActive(clock))
        {
            return Done(new SessionRotation.Invalid());
        }

        var successor = RefreshToken.Create(newTokenHash, clock);
        current.ReplaceWith(successor.Id, clock);
        _refreshTokens.Add(successor);
        LastRotatedAt = clock.UtcNow;
        Prune(clock, current.Id, successor.Id);
        return Done(new SessionRotation.Rotated(successor));
    }

    private void Prune(IClock clock, Guid keepPrevious, Guid keepCurrent)
    {
        var now = clock.UtcNow;
        _refreshTokens.RemoveAll(token =>
            token.Id != keepPrevious
            && token.Id != keepCurrent
            && (token.ExpiresAt <= now || (token.ReplacedAt is { } replacedAt && now - replacedAt > ReplacedTokenRetention)));
    }

    private static Result<SessionRotation> Done(SessionRotation rotation) => Result<SessionRotation>.Success(rotation);
}
