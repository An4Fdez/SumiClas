namespace SumiClas.Shared.Domain;

public static class AuthenticationExceptions
{
    public sealed class Unauthenticated()
        : DomainException(AuthenticationErrors.Unauthenticated.Code, AuthenticationErrors.Unauthenticated.Message);

    /// <summary>One message for an unknown email, a missing credential and a wrong password, so the login never says which one failed.</summary>
    public sealed class InvalidCredentials()
        : DomainException(AuthenticationErrors.InvalidCredentials.Code, AuthenticationErrors.InvalidCredentials.Message);

    public sealed class PasswordTooWeak()
        : DomainException(AuthenticationErrors.PasswordTooWeak.Code, AuthenticationErrors.PasswordTooWeak.Message);

    /// <summary>The refresh token is missing, unknown, expired, revoked or already replaced.</summary>
    public sealed class RefreshTokenInvalid()
        : DomainException(AuthenticationErrors.RefreshTokenInvalid.Code, AuthenticationErrors.RefreshTokenInvalid.Message);
}
