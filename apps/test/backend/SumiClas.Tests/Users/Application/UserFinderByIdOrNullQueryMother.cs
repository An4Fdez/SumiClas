namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserFinderByIdOrNullQueryMother
{
    public static UserFinderByIdOrNull.Query WithUserId(Guid userId) => new(userId);
}
