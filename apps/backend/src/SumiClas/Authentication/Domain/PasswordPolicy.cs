using SumiClas.Shared.Domain;

namespace SumiClas.Authentication.Domain;

/// <summary>
/// Same minimum as the registration form (8 characters). The "appears in known leaks" check stays client-only
/// (it queries a public service from the browser). The maximum protects the hasher from huge inputs.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static Result Validate(string? password)
        => password is { Length: >= MinLength and <= MaxLength }
            ? Result.Success()
            : Result.Failure(AuthenticationErrors.PasswordTooWeak);
}
