using SumiClas.Shared.Domain;

namespace SumiClas.Subjects.Domain;

public static class SubjectErrors
{
    public static readonly Error NotFound = new("Subject.NotFound", "No se ha encontrado la asignatura.");
    public static readonly Error IdRequired = new("Subject.IdRequired", "El identificador de la asignatura no puede estar vacío.");
    public static readonly Error ClassRequired = new("Subject.ClassRequired", "La asignatura debe pertenecer a una clase.");
    public static readonly Error LabelRequired = new("Subject.LabelRequired", "El nombre de la asignatura no puede estar vacío.");
}
