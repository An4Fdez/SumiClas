using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public sealed class AccessToken : ValueObject
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    public string Value { get; }
    public DateTimeOffset ExpiresAt { get; }

    private AccessToken(string value, DateTimeOffset expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public static AccessToken Create(string value, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AccessTokenExceptions.ValueRequired();
        }

        if (expiresAt == default)
        {
            throw new AccessTokenExceptions.ExpiryRequired();
        }

        return new AccessToken(value, expiresAt);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return ExpiresAt;
    }
}
