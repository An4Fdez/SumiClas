namespace SumiClas.Tests.Users;

using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Application;
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
    }

    protected void ShouldFetchUsers(params User[] users)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => users.FirstOrDefault(user => user.Id == id));

    protected void ShouldSearchUsers(params User[] users)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<User> specification, CancellationToken _)
                => (IReadOnlyCollection<User>)users.Where(specification.Criteria.Compile()).ToList());

    protected void ShouldFetchNoUsers()
        => this.ShouldFetchUsers();

    protected void ShouldSearchNoUsers()
        => this.ShouldSearchUsers();

    protected void ShouldCaptureSavedUsers()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => this._saved.Add(user))
            .Returns(Task.CompletedTask);

    protected void ShouldFind(User user)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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

    protected void ShouldFindClass(Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.WithId(classId)));

    protected void ShouldNotFindClass()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Failure(ClassErrors.NotFound));

    protected void ShouldFindClassInSchoolYear(Guid classId, Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.With(classId, schoolYearId)));

    protected void ShouldEnsureSchoolYearIsActive(Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldFailEnsuringSchoolYearIsActive(Guid schoolYearId, Error error)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));
}
