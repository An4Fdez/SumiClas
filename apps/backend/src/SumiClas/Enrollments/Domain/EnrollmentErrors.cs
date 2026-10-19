using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public static class EnrollmentErrors
{
    public static readonly Error IdRequired = new("Enrollment.IdRequired", "El identificador de la matrícula no puede estar vacío.");
    public static readonly Error NotFound = new("Enrollment.NotFound", "No se ha encontrado la matrícula.");
    public static readonly Error AlreadyExists = new("Enrollment.AlreadyExists", "El alumno ya tiene una matrícula confirmada en este curso escolar.");
    public static readonly Error StudentRequired = new("Enrollment.StudentRequired", "La matrícula requiere un alumno.");
    public static readonly Error ClassRequired = new("Enrollment.ClassRequired", "La matrícula requiere una clase.");
    public static readonly Error AlreadyConfirmed = new("Enrollment.AlreadyConfirmed", "La matrícula ya estaba confirmada.");
    public static readonly Error SameClass = new("Enrollment.SameClass", "La clase de origen y la de destino no pueden ser la misma.");
    public static readonly Error SameSchoolYear = new("Enrollment.SameSchoolYear", "Las dos clases son del mismo curso escolar: la clase nueva debe ser de un curso distinto.");
    public static readonly Error DifferentSchool = new("Enrollment.DifferentSchool", "Las dos clases deben ser del mismo colegio.");
    public static readonly Error SuggestForbidden = new("Enrollment.SuggestForbidden", "Solo un delegado de la clase puede sugerir matrículas en ella.");
}
