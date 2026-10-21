using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public static class StudentExceptions
{
    public sealed class NotFound() : DomainException("Student.NotFound", "No se ha encontrado el alumno.");

    public sealed class IdRequired()
        : DomainException("Student.IdRequired", "El identificador del alumno no puede estar vacío.");

    public sealed class GuardianChangeForbidden()
        : DomainException("Student.GuardianChangeForbidden", "Solo un tutor del alumno puede cambiar sus tutores.");

    public sealed class GuardianAccessForbidden()
        : DomainException("Student.GuardianAccessForbidden", "Solo un tutor del alumno puede hacer esto.");

    public sealed class InvalidGuardianRole()
        : DomainException("Student.InvalidGuardianRole", "El rol del tutor no es válido.");

    public sealed class NameRequired()
        : DomainException("Student.NameRequired", "El nombre del alumno no puede estar vacío.");

    public sealed class FirstSurnameRequired()
        : DomainException("Student.FirstSurnameRequired", "El primer apellido del alumno no puede estar vacío.");

    public sealed class FirstGuardianRequired()
        : DomainException("Student.FirstGuardianRequired", "Un alumno debe registrarse con al menos un tutor.");

    public sealed class GuardianIdRequired()
        : DomainException("Student.GuardianIdRequired", "El identificador del tutor no puede estar vacío.");

    public sealed class GuardianAlreadyLinked()
        : DomainException("Student.GuardianAlreadyLinked", "Este tutor ya está vinculado al alumno.");

    public sealed class GuardianNotLinked()
        : DomainException("Student.GuardianNotLinked", "El tutor no está vinculado a este alumno.");

    public sealed class AtLeastOneGuardianRequired()
        : DomainException(
            "Student.AtLeastOneGuardianRequired",
            "Un alumno siempre debe tener al menos un tutor.");
}
