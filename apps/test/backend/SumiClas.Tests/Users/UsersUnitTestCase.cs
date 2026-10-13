namespace SumiClas.Tests.Users;

using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes;
using SumiClas.Tests.SchoolYears;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

public abstract class UsersUnitTestCase : UnitTestCase
{
    private readonly List<User> _saved = [];

    protected Mock<IUserRepository> Repository { get; } = new();

    protected IReadOnlyList<User> Saved => this._saved;

    [SetUp]
    public void UsersSetUp()
    {
        this._saved.Clear();
        this.Repository.Reset();
        this.ShouldReturn();
        this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => this._saved.Add(user))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldReturn(params User[] users)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<User> specification, CancellationToken _)
                => (IReadOnlyCollection<User>)users.Where(specification.Criteria.Compile()).ToList());

    protected void ShouldFind(User user)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    protected void ShouldResolveActiveClass(Guid classId, Guid schoolYearId)
    {
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldEnsureSchoolYearIsActive(schoolYearId);
    }

    protected void ShouldHaveSaved(
        Guid id,
        string fullName,
        string email,
        string passwordHash,
        DateTimeOffset changedAt)
        => this.Repository.Verify(
            repository => repository.AddAsync(
                It.Is<User>(user =>
                    user.Id == id
                    && user.FullName == fullName
                    && user.Email.Value == email
                    && user.PasswordHash == passwordHash
                    && user.PasswordChangedAt == changedAt),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSaved()
        => this.Repository.Verify(
            repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<User>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveFetched()
        => this.Repository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
