namespace SumiClas.Shared.Domain;

public static class AuthenticationErrors
{
    public static readonly Error Unauthenticated = new("Authentication.Unauthenticated", "No hemos podido identificar tu sesión. Inicia sesión de nuevo.");

    /// <summary>One message for an unknown email, a missing credential and a wrong password, so the login never says which one failed.</summary>
    public static readonly Error InvalidCredentials = new("Authentication.InvalidCredentials", "El correo electrónico o la contraseña no son correctos.");

    public static readonly Error PasswordTooWeak = new("Authentication.PasswordTooWeak", "La contraseña debe tener entre 8 y 128 caracteres.");

    public static readonly Error RefreshTokenInvalid = new("Authentication.RefreshTokenInvalid", "Tu sesión ha caducado. Inicia sesión de nuevo.");

    public static readonly Error Forbidden = new("Authentication.Forbidden", "No tienes permiso para hacer esto.");
}
