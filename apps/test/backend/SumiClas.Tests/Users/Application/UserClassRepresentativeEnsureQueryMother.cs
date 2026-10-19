namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassRepresentativeEnsureQueryMother
{
    public static UserClassRepresentativeEnsure.Query With(Guid userId, Guid classId) => new(userId, classId);
}
