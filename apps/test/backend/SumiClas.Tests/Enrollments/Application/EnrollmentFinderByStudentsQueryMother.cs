namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.Enrollments.Application;

public static class EnrollmentFinderByStudentsQueryMother
{
    public static EnrollmentFinderByStudents.Query WithStudentIds(params Guid[] studentIds) => new(studentIds);

    public static EnrollmentFinderByStudents.Query WithoutStudents() => new([]);
}
