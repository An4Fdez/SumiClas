namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;

public static class StudentFinderByIdQueryMother
{
    public static StudentFinderById.Query WithStudentId(Guid studentId) => new(studentId);
}
