using SumiClas.Shared.Domain;

namespace SumiClas.Subjects.Domain;

public static class SubjectErrors
{
    public static readonly Error ClassRequired = new("Subject.ClassRequired", "La asignatura debe pertenecer a una clase.");
    public static readonly Error LabelRequired = new("Subject.LabelRequired", "El nombre de la asignatura no puede estar vacío.");
}
