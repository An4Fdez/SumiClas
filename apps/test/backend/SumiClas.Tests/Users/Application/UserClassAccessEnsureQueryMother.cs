namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassAccessEnsureQueryMother
{
    public static UserClassAccessEnsure.Query With(Guid userId, Guid classId) => new(userId, classId);
}
