namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserIsClassMemberQueryMother
{
    public static UserIsClassMember.Query With(Guid userId, Guid classId) => new(userId, classId);
}
