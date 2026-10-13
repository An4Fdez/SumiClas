namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;

public static class StudentGuardianAddCommandMother
{
    public static StudentGuardianAdd.Command With(Guid studentId, Guid guardianId, string role)
        => new(studentId, guardianId, role);
}
