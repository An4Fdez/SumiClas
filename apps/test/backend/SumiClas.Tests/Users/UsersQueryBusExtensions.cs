namespace SumiClas.Tests.Users;

using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public static class UsersQueryBusExtensions
{
    public static void ShouldFindUser(this Mock<IQueryBus> queryBus, Guid userId)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(UserResponseMother.With(userId, StringMother.Random("name"))));

    public static void ShouldNotFindUser(this Mock<IQueryBus> queryBus, Guid userId)
        => queryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(UserErrors.NotFound));
}
