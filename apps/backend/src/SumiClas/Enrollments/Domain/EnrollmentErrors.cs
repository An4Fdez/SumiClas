using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public static class EnrollmentErrors
{
    public static readonly Error IdRequired = new("Enrollment.IdRequired", "El identificador de la matrícula no puede estar vacío.");
    public static readonly Error AlreadyExists = new("Enrollment.AlreadyExists", "El alumno ya tiene una matrícula confirmada en este curso escolar.");
    public static readonly Error StudentRequired = new("Enrollment.StudentRequired", "La matrícula requiere un alumno.");
    public static readonly Error ClassRequired = new("Enrollment.ClassRequired", "La matrícula requiere una clase.");
    public static readonly Error AlreadyConfirmed = new("Enrollment.AlreadyConfirmed", "La matrícula ya estaba confirmada.");
}
