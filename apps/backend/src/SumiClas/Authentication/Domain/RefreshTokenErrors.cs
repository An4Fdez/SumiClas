using SumiClas.Shared.Domain;

namespace SumiClas.Authentication.Domain;

public static class RefreshTokenErrors
{
    public static readonly Error IdRequired = new("RefreshToken.IdRequired", "El identificador del token de sesión no puede estar vacío.");
    public static readonly Error UserRequired = new("RefreshToken.UserRequired", "El token de sesión necesita un usuario.");
    public static readonly Error FamilyRequired = new("RefreshToken.FamilyRequired", "El token de sesión necesita una familia.");
    public static readonly Error HashRequired = new("RefreshToken.HashRequired", "El token de sesión necesita su huella.");
}
