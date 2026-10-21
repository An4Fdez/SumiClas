using SumiClas.Shared.Domain;

namespace SumiClas.Subjects.Domain;

public static class SubjectExceptions
{
    public sealed class NotFound() : DomainException("Subject.NotFound", "No se ha encontrado la asignatura.");

    public sealed class IdRequired()
        : DomainException("Subject.IdRequired", "El identificador de la asignatura no puede estar vacío.");

    public sealed class ClassRequired()
        : DomainException("Subject.ClassRequired", "La asignatura debe pertenecer a una clase.");

    public sealed class LabelRequired()
        : DomainException("Subject.LabelRequired", "El nombre de la asignatura no puede estar vacío.");
}
