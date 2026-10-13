namespace SumiClas.Tests.Subjects.Application;

using SumiClas.Subjects.Application;

public static class SubjectFinderByClassQueryMother
{
    public static SubjectFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
