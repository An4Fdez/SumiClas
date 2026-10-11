using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserProfileFinderCurrentTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<IUserProvider> _userProvider = null!;
    private UserProfileFinderCurrent.Finder _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _users = new Mock<IUserRepository>();
        _userProvider = new Mock<IUserProvider>();
        _useCase = new UserProfileFinderCurrent.Finder(new UserFinder(_users.Object), _userProvider.Object);
    }

    private User GivenCurrentUser()
    {
        var user = User.Register(Guid.NewGuid(), "Ana Perez", Email.Create("ana@example.com").Value, "hash", new Mock<IClock>().Object).Value;
        _userProvider.Setup(provider => provider.GetCurrentUserId()).Returns(Result<Guid>.Success(user.Id));
        _users.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return user;
    }

    [Test]
    public async Task UseCase_WhenThereIsNoCurrentUser_PropagatesTheFailure()
    {
        _userProvider.Setup(provider => provider.GetCurrentUserId())
            .Returns(Result<Guid>.Failure(AuthenticationErrors.Unauthenticated));

        var result = await _useCase.ExecuteAsync(new UserProfileFinderCurrent.Query());

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.Unauthenticated));
        _users.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WhenTheUserDoesNotExist_ReturnsNotFound()
    {
        _userProvider.Setup(provider => provider.GetCurrentUserId()).Returns(Result<Guid>.Success(Guid.NewGuid()));

        var result = await _useCase.ExecuteAsync(new UserProfileFinderCurrent.Query());

        Assert.That(result.Error, Is.EqualTo(UserErrors.NotFound));
    }

    [Test]
    public async Task UseCase_ReturnsTheProfileWithIdNameEmailAndClassRoles()
    {
        var user = GivenCurrentUser();
        var classId = Guid.NewGuid();
        user.AssignAsClassRepresentative(classId, 0);

        var result = await _useCase.ExecuteAsync(new UserProfileFinderCurrent.Query());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(user.Id));
        Assert.That(result.Value.FullName, Is.EqualTo("Ana Perez"));
        Assert.That(result.Value.Email, Is.EqualTo("ana@example.com"));
        Assert.That(result.Value.ClassRoles.Select(role => (role.ClassId, role.Role)),
            Is.EqualTo(new[] { (classId, "ClassRepresentative") }));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var user = GivenCurrentUser();
        var handler = new UserProfileFinderCurrent.Handler(_useCase);

        var result = await handler.Handle(new UserProfileFinderCurrent.Query(), CancellationToken.None);

        Assert.That(result.Value.Id, Is.EqualTo(user.Id));
    }
}
