using SumiClas.Shared.Domain;

namespace SumiClas.Authentication.Domain;

public static class UserCredentialErrors
{
    public static readonly Error IdRequired = new("UserCredential.IdRequired", "El identificador del usuario de la credencial no puede estar vacío.");
    public static readonly Error PasswordHashRequired = new("UserCredential.PasswordHashRequired", "La credencial necesita la contraseña cifrada.");
}
