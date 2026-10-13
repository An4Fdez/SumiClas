namespace SumiClas.Tests.Subjects.Application;

using SumiClas.Subjects.Application;

public static class SubjectResponseMother
{
    public static SubjectResponse With(Guid id, Guid classId, string label) => new(id, classId, label);
}
