namespace SumiClas.Shared.Domain;

public static class AuthenticationErrors
{
    public static readonly Error Unauthenticated = new("Authentication.Unauthenticated", "No se ha identificado al usuario.");
}
