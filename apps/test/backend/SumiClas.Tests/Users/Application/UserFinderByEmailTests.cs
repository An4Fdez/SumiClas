using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserFinderByEmailTests
{
    private List<User> _store = null!;
    private UserFinderByEmail.Finder _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        var users = new Mock<IUserRepository>();
        users
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<User>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<User>>(
                    SpecificationConverter<User>.GetQuery(_store.AsQueryable(), specification).ToList()));
        _useCase = new UserFinderByEmail.Finder(users.Object);
    }

    [Test]
    public async Task UseCase_FindsTheUserIgnoringCaseAndSpacesAndReturnsOnlyIdAndName()
    {
        var user = User.Register(Guid.NewGuid(), "Ana Perez", Email.Create("ana@example.com").Value, "hash", new Mock<IClock>().Object).Value;
        _store.Add(user);

        var result = await _useCase.ExecuteAsync(new UserFinderByEmail.Query("  ANA@Example.com "), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new UserResponse(user.Id, "Ana Perez")));
    }

    [Test]
    public async Task UseCase_WithUnknownEmail_ReturnsNotFound()
    {
        var result = await _useCase.ExecuteAsync(new UserFinderByEmail.Query("nobody@example.com"), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(UserErrors.NotFound));
    }

    [Test]
    public async Task UseCase_WithMalformedEmail_ReturnsInvalidEmailFormat()
    {
        var result = await _useCase.ExecuteAsync(new UserFinderByEmail.Query("nonsense"), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidEmailFormat));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var user = User.Register(Guid.NewGuid(), "Ana Perez", Email.Create("ana@example.com").Value, "hash", new Mock<IClock>().Object).Value;
        _store.Add(user);
        var handler = new UserFinderByEmail.Handler(_useCase);

        var result = await handler.Handle(new UserFinderByEmail.Query("ana@example.com"), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(new UserResponse(user.Id, "Ana Perez")));
    }
}
