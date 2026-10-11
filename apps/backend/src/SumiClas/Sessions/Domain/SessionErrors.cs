using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public static class SessionErrors
{
    public static readonly Error UserRequired = new("Session.UserRequired", "La sesión necesita un usuario.");
    public static readonly Error TokenHashRequired = new("Session.TokenHashRequired", "El token de sesión necesita su huella.");
}
