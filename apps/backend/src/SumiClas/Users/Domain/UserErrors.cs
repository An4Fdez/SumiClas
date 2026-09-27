using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public static class UserErrors
{
    public static readonly Error FullNameRequired = new("User.FullNameRequired", "El nombre completo del usuario no puede estar vacío.");
    public static readonly Error InvalidEmailFormat = new("User.InvalidEmailFormat", "El formato del correo electrónico no es válido.");
    public static readonly Error ClassIdRequired = new("User.ClassIdRequired", "El identificador de la clase no puede estar vacío.");
    public static readonly Error NoRoleAssignedInClass = new("User.NoRoleAssignedInClass", "El usuario no tiene ningún rol asignado en esta clase.");
    public static readonly Error NotClassRepresentative = new("User.NotClassRepresentative", "El usuario no es el delegado de esta clase.");
    public static readonly Error MaxClassRepresentativesReached = new("User.MaxClassRepresentativesReached", "Esta clase ya tiene el número máximo de delegados.");
}
