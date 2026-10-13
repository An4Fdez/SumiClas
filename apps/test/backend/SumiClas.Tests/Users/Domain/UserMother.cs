namespace SumiClas.Tests.Users.Domain;

using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

public static class UserMother
{
    public static User Random() => With();

    public static User With(
        Guid? id = null,
        string? fullName = null,
        Email? email = null,
        string? passwordHash = null,
        IEnumerable<ClassRoleAssignment>? classRoles = null)
        => new(
            id ?? GuidMother.Random(),
            fullName ?? StringMother.Random("User"),
            email ?? EmailMother.Random(),
            passwordHash ?? StringMother.Random("hash"),
            UnitTestCase.Now,
            classRoles ?? []);

    public static User WithName(string fullName) => With(fullName: fullName);

    public static User WithEmail(string email) => With(email: EmailMother.Create(email));

    public static User Representative(Guid classId, string? fullName = null)
        => With(fullName: fullName, classRoles: [ClassRoleAssignmentMother.Representative(classId)]);
}
