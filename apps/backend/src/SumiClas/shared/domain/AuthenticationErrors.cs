namespace SumiClas.Shared.Domain;

public static class AuthenticationErrors
{
    public static readonly Error Unauthenticated = new("Authentication.Unauthenticated", "No se ha identificado al usuario.");

    /// <summary>One message for an unknown email, a missing credential and a wrong password, so the login never says which one failed.</summary>
    public static readonly Error InvalidCredentials = new("Authentication.InvalidCredentials", "El correo electrónico o la contraseña no son correctos.");

    public static readonly Error PasswordTooWeak = new("Authentication.PasswordTooWeak", "La contraseña debe tener entre 8 y 128 caracteres.");
}
