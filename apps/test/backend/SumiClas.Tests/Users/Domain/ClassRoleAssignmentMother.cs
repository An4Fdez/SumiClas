namespace SumiClas.Tests.Users.Domain;

using SumiClas.Users.Domain;

public static class ClassRoleAssignmentMother
{
    public static ClassRoleAssignment Representative(Guid classId) => new(classId, ClassRoleType.ClassRepresentative);
}
