using SumiClas.Shared.Domain;

namespace SumiClas.RefreshTokens.Domain;

public static class RefreshTokenErrors
{
    public static readonly Error IdRequired = new("RefreshToken.IdRequired", "El identificador del token de sesión no puede estar vacío.");
    public static readonly Error UserRequired = new("RefreshToken.UserRequired", "El token de sesión necesita un usuario.");
    public static readonly Error FamilyRequired = new("RefreshToken.FamilyRequired", "El token de sesión necesita una familia.");
    public static readonly Error HashRequired = new("RefreshToken.HashRequired", "El token de sesión necesita su huella.");
    public static readonly Error LifetimeInvalid = new("RefreshToken.LifetimeInvalid", "La duración del token de sesión debe ser mayor que cero.");
    public static readonly Error NotActive = new("RefreshToken.NotActive", "El token de sesión ya no está activo.");
}
