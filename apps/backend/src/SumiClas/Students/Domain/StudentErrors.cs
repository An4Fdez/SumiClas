using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public static class StudentErrors
{
    public static readonly Error NameRequired = new("Student.NameRequired", "El nombre del alumno no puede estar vacío.");
    public static readonly Error FirstSurnameRequired = new("Student.FirstSurnameRequired", "El primer apellido del alumno no puede estar vacío.");
    public static readonly Error FirstGuardianRequired = new("Student.FirstGuardianRequired", "Un alumno debe registrarse con al menos un tutor.");
    public static readonly Error GuardianIdRequired = new("Student.GuardianIdRequired", "El identificador del tutor no puede estar vacío.");
    public static readonly Error GuardianAlreadyLinked = new("Student.GuardianAlreadyLinked", "Este tutor ya está vinculado al alumno.");
    public static readonly Error GuardianNotLinked = new("Student.GuardianNotLinked", "El tutor no está vinculado a este alumno.");
    public static readonly Error AtLeastOneGuardianRequired = new("Student.AtLeastOneGuardianRequired", "Un alumno siempre debe tener al menos un tutor.");
}
