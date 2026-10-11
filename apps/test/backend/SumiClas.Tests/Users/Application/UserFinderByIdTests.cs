using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserFinderByIdTests
{
    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var user = User.Register(Guid.NewGuid(), "Ana Perez", Email.Create("ana@example.com").Value, "hash", new Mock<IClock>().Object).Value;
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = new UserFinderById.Handler(new UserFinderById.Finder(new UserFinder(users.Object)));

        var result = await handler.Handle(new UserFinderById.Query(user.Id), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(new UserResponse(user.Id, "Ana Perez")));
    }
}
