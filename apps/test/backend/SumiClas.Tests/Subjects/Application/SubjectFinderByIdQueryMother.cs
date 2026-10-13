namespace SumiClas.Tests.Subjects.Application;

using SumiClas.Subjects.Application;

public static class SubjectFinderByIdQueryMother
{
    public static SubjectFinderById.Query WithSubjectId(Guid subjectId) => new(subjectId);
}
