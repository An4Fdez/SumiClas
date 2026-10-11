using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public static class AccessTokenErrors
{
    public static readonly Error ValueRequired = new("AccessToken.ValueRequired", "El token de acceso no puede estar vacío.");
    public static readonly Error ExpiryRequired = new("AccessToken.ExpiryRequired", "El token de acceso necesita una fecha de caducidad.");
}
