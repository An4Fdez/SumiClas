using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public sealed class AccessToken : ValueObject
{
    public string Value { get; }
    public DateTimeOffset ExpiresAt { get; }

    private AccessToken(string value, DateTimeOffset expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public static Result<AccessToken> Create(string value, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<AccessToken>.Failure(AccessTokenErrors.ValueRequired);
        }

        if (expiresAt == default)
        {
            return Result<AccessToken>.Failure(AccessTokenErrors.ExpiryRequired);
        }

        return Result<AccessToken>.Success(new AccessToken(value, expiresAt));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return ExpiresAt;
    }
}
