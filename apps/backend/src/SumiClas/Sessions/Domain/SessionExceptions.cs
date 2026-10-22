using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public static class SessionExceptions
{
    public sealed class UserRequired() : DomainException("Session.UserRequired", "La sesión necesita un usuario.");

    public sealed class TokenHashRequired()
        : DomainException("Session.TokenHashRequired", "El token de sesión necesita su huella.");

    /// <summary>
    /// A refresh token that was already replaced came back. The rotation is aborted, so the revocation of the session has
    /// to be sent as a separate command by whoever catches this (see the refresh endpoint).
    /// </summary>
    public sealed class RefreshTokenReused()
        : DomainException("Session.RefreshTokenReused", "Se ha reutilizado un token de sesión que ya había sido reemplazado.");
}
