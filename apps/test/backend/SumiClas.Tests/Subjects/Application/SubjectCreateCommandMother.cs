namespace SumiClas.Tests.Subjects.Application;

using SumiClas.Subjects.Application;

public static class SubjectCreateCommandMother
{
    public static SubjectCreate.Command With(Guid id, Guid classId, string label) => new(id, classId, label);
}
