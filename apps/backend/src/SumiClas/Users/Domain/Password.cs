using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

/// <summary>
/// Same minimum as the registration form (8 characters). The "appears in known leaks" check stays client-only
/// (it queries a public service from the browser). The maximum protects the hasher from huge inputs.
/// Login does not build it: a legacy password outside these rules must still verify.
/// </summary>
public sealed class Password : ValueObject
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Result<Password> Create(string? value)
        => value is { Length: >= MinLength and <= MaxLength }
            ? Result<Password>.Success(new Password(value))
            : Result<Password>.Failure(AuthenticationErrors.PasswordTooWeak);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // Redacted so the secret never reaches logs or exception messages through string interpolation.
    public override string ToString() => "********";
}
