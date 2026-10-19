namespace SumiClas.Tests.LostItems;

using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class LostItemsUnitTestCase : UnitTestCase
{
    private readonly List<LostItem> _added = [];

    protected Mock<ILostItemRepository> Repository { get; } = new();

    protected IReadOnlyList<LostItem> Added => this._added;

    [SetUp]
    public void LostItemsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldCaptureAddedLostItems()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<LostItem>(), It.IsAny<CancellationToken>()))
            .Callback<LostItem, CancellationToken>((lostItem, _) => this._added.Add(lostItem))
            .Returns(Task.CompletedTask);

    protected void ShouldFetchLostItems(params LostItem[] lostItems)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => lostItems.FirstOrDefault(lostItem => lostItem.Id == id));

    protected void ShouldSearchLostItems(params LostItem[] lostItems)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<LostItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<LostItem> specification, CancellationToken _)
                => InMemorySearch.Run(lostItems, specification));

    protected void ShouldFetchNoLostItems()
        => this.ShouldFetchLostItems();

    protected void ShouldSearchNoLostItems()
        => this.ShouldSearchLostItems();

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<LostItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(UserResponseMother.With(userId, StringMother.Random("name"))));

    protected void ShouldNotFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(UserErrors.NotFound));

    protected void ShouldFindClassInSchoolYear(Guid classId, Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassResponse>.Success(ClassResponseMother.With(classId, schoolYearId)));

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

    protected void ShouldAllowClassAccess(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassAccessEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldDenyClassAccess(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassAccessEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(UserErrors.ClassAccessForbidden));
}
