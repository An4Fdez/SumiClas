using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserFinderByIdsTests
{
    private List<User> _store = null!;
    private Mock<IUserRepository> _users = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        _users = new Mock<IUserRepository>();
        _users
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<User>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<User>>(
                    SpecificationConverter<User>.GetQuery(_store.AsQueryable(), specification).ToList()));
    }

    private User AddUser(string fullName)
    {
        var id = Guid.NewGuid();
        var user = User.Register(id, fullName, Email.Create($"{id:N}@example.com").Value, "hash", new Mock<IClock>().Object).Value;
        _store.Add(user);
        return user;
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var user = AddUser("Ana Perez");
        var handler = new UserFinderByIds.Handler(new UserFinderByIds.Finder(_users.Object));

        var result = await handler.Handle(new UserFinderByIds.Query([user.Id, Guid.NewGuid()]), CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { new UserResponse(user.Id, "Ana Perez") }));
    }
}
