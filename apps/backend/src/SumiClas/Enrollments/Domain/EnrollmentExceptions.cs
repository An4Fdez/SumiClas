using SumiClas.Shared.Domain;

namespace SumiClas.Enrollments.Domain;

public static class EnrollmentExceptions
{
    public sealed class NotFound() : DomainException("Enrollment.NotFound", "No se ha encontrado la matrícula.");

    public sealed class IdRequired()
        : DomainException("Enrollment.IdRequired", "El identificador de la matrícula no puede estar vacío.");

    public sealed class AlreadyExists()
        : DomainException(
            "Enrollment.AlreadyExists",
            "El alumno ya tiene una matrícula confirmada en este curso escolar.");

    public sealed class StudentRequired()
        : DomainException("Enrollment.StudentRequired", "La matrícula requiere un alumno.");

    public sealed class ClassRequired()
        : DomainException("Enrollment.ClassRequired", "La matrícula requiere una clase.");

    public sealed class AlreadyConfirmed()
        : DomainException("Enrollment.AlreadyConfirmed", "La matrícula ya estaba confirmada.");

    public sealed class SameClass()
        : DomainException(
            "Enrollment.SameClass",
            "La clase de origen y la de destino no pueden ser la misma.");

    public sealed class SameSchoolYear()
        : DomainException(
            "Enrollment.SameSchoolYear",
            "Las dos clases son del mismo curso escolar: la clase nueva debe ser de un curso distinto.");

    public sealed class DifferentSchool()
        : DomainException("Enrollment.DifferentSchool", "Las dos clases deben ser del mismo colegio.");

    public sealed class SuggestForbidden()
        : DomainException(
            "Enrollment.SuggestForbidden",
            "Solo un delegado de la clase puede sugerir matrículas en ella.");
}
