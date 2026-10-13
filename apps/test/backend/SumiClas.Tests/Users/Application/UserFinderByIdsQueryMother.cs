namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserFinderByIdsQueryMother
{
    public static UserFinderByIds.Query WithUserIds(params Guid[] userIds) => new(userIds);
}
