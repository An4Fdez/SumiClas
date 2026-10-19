using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "No se ha encontrado el usuario.");
    public static readonly Error IdRequired = new("User.IdRequired", "El identificador del usuario no puede estar vacío.");
    public static readonly Error EmailAlreadyRegistered = new("User.EmailAlreadyRegistered", "Ya existe un usuario con este correo electrónico.");
    public static readonly Error PasswordHashRequired = new("User.PasswordHashRequired", "El usuario necesita la contraseña cifrada.");
    public static readonly Error FullNameRequired = new("User.FullNameRequired", "El nombre completo del usuario no puede estar vacío.");
    public static readonly Error InvalidEmailFormat = new("User.InvalidEmailFormat", "El formato del correo electrónico no es válido.");
    public static readonly Error ClassIdRequired = new("User.ClassIdRequired", "El identificador de la clase no puede estar vacío.");
    public static readonly Error NoRoleAssignedInClass = new("User.NoRoleAssignedInClass", "El usuario no tiene ningún rol asignado en esta clase.");
    public static readonly Error NotClassRepresentative = new("User.NotClassRepresentative", "El usuario no es el delegado de esta clase.");
    public static readonly Error AssignForbidden = new("User.AssignForbidden", "Solo un delegado de la clase puede asignar el rol de delegado.");
    public static readonly Error ClassRepresentativeAlreadyExists = new("User.ClassRepresentativeAlreadyExists", "Esta clase ya tiene delegado.");
    public static readonly Error LastClassRepresentativeCannotResign = new("User.LastClassRepresentativeCannotResign", "El último delegado no puede renunciar: antes hay que asignar a otro.");
    public static readonly Error ClassMembersForbidden = new("User.ClassMembersForbidden", "Solo los miembros de la clase pueden ver quién forma parte de ella.");
    public static readonly Error ClassAccessForbidden = new("User.ClassAccessForbidden", "No perteneces a esta clase, así que no puedes ver ni cambiar sus datos.");
    public static readonly Error MaxClassRepresentativesReached = new("User.MaxClassRepresentativesReached", "Esta clase ya tiene el número máximo de delegados.");
}
