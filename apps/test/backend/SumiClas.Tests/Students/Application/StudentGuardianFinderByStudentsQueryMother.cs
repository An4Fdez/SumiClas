namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;

public static class StudentGuardianFinderByStudentsQueryMother
{
    public static StudentGuardianFinderByStudents.Query WithStudentIds(params Guid[] studentIds) => new(studentIds);

    public static StudentGuardianFinderByStudents.Query WithoutStudents() => new([]);
}
