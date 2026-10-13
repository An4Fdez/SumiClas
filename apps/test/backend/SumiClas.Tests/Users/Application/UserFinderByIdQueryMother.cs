namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserFinderByIdQueryMother
{
    public static UserFinderById.Query WithUserId(Guid userId) => new(userId);
}
