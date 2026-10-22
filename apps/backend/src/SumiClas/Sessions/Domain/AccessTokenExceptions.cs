using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public static class AccessTokenExceptions
{
    public sealed class ValueRequired()
        : DomainException("AccessToken.ValueRequired", "El token de acceso no puede estar vacío.");

    public sealed class ExpiryRequired()
        : DomainException("AccessToken.ExpiryRequired", "El token de acceso necesita una fecha de caducidad.");
}
