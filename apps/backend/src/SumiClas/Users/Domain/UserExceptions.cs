using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public static class UserExceptions
{
    public sealed class NotFound() : DomainException("User.NotFound", "No se ha encontrado el usuario.");

    public sealed class IdRequired()
        : DomainException("User.IdRequired", "El identificador del usuario no puede estar vacío.");

    public sealed class EmailAlreadyRegistered()
        : DomainException("User.EmailAlreadyRegistered", "Ya existe un usuario con este correo electrónico.");

    public sealed class PasswordHashRequired()
        : DomainException("User.PasswordHashRequired", "El usuario necesita la contraseña cifrada.");

    public sealed class FullNameRequired()
        : DomainException("User.FullNameRequired", "El nombre completo del usuario no puede estar vacío.");

    public sealed class InvalidEmailFormat()
        : DomainException("User.InvalidEmailFormat", "El formato del correo electrónico no es válido.");

    public sealed class ClassIdRequired()
        : DomainException("User.ClassIdRequired", "El identificador de la clase no puede estar vacío.");

    public sealed class NoRoleAssignedInClass()
        : DomainException("User.NoRoleAssignedInClass", "El usuario no tiene ningún rol asignado en esta clase.");

    public sealed class NotClassRepresentative()
        : DomainException("User.NotClassRepresentative", "El usuario no es el delegado de esta clase.");

    public sealed class AssignForbidden()
        : DomainException("User.AssignForbidden", "Solo un delegado de la clase puede asignar el rol de delegado.");

    public sealed class ClassRepresentativeAlreadyExists()
        : DomainException("User.ClassRepresentativeAlreadyExists", "Esta clase ya tiene delegado.");

    public sealed class LastClassRepresentativeCannotResign()
        : DomainException(
            "User.LastClassRepresentativeCannotResign",
            "El último delegado no puede renunciar: antes hay que asignar a otro.");

    public sealed class ClassMembersForbidden()
        : DomainException(
            "User.ClassMembersForbidden",
            "Solo los miembros de la clase pueden ver quién forma parte de ella.");

    public sealed class ClassAccessForbidden()
        : DomainException(
            "User.ClassAccessForbidden",
            "No perteneces a esta clase, así que no puedes ver ni cambiar sus datos.");

    public sealed class ClassRepresentativeForbidden()
        : DomainException("User.ClassRepresentativeForbidden", "Solo un delegado de la clase puede hacer esto.");

    public sealed class MaxClassRepresentativesReached()
        : DomainException("User.MaxClassRepresentativesReached", "Esta clase ya tiene el número máximo de delegados.");
}
