namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserIsClassRepresentativeQueryMother
{
    public static UserIsClassRepresentative.Query With(Guid userId, Guid classId) => new(userId, classId);
}
