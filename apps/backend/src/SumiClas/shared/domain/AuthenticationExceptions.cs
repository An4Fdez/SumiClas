namespace SumiClas.Shared.Domain;

public static class AuthenticationExceptions
{
    public sealed class Unauthenticated() : DomainException(
        "Authentication.Unauthenticated",
        "No hemos podido identificar tu sesión. Inicia sesión de nuevo.");

    /// <summary>One message for an unknown email, a missing credential and a wrong password, so the login never says which one failed.</summary>
    public sealed class InvalidCredentials() : DomainException(
        "Authentication.InvalidCredentials",
        "El correo electrónico o la contraseña no son correctos.");

    public sealed class PasswordTooWeak() : DomainException(
        "Authentication.PasswordTooWeak",
        "La contraseña debe tener entre 8 y 128 caracteres.");

    /// <summary>The refresh token is missing, unknown, expired, revoked or already replaced.</summary>
    public sealed class RefreshTokenInvalid() : DomainException(
        "Authentication.RefreshTokenInvalid",
        "Tu sesión ha caducado. Inicia sesión de nuevo.");

    /// <summary>Only the authorization middleware answers with it; no use case throws it.</summary>
    public sealed class Forbidden()
        : DomainException("Authentication.Forbidden", "No tienes permiso para hacer esto.");
}
