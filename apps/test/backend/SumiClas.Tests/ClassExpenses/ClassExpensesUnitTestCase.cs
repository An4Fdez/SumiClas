namespace SumiClas.Tests.ClassExpenses;

using Moq;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;

public abstract class ClassExpensesUnitTestCase : UnitTestCase
{
    private readonly List<ClassExpense> _added = [];

    protected Mock<IClassExpenseRepository> Repository { get; } = new();

    protected IReadOnlyList<ClassExpense> Added => this._added;

    [SetUp]
    public void ClassExpensesSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldCaptureAddedClassExpenses()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<ClassExpense>(), It.IsAny<CancellationToken>()))
            .Callback<ClassExpense, CancellationToken>((classExpense, _) => this._added.Add(classExpense))
            .Returns(Task.CompletedTask);

    protected void ShouldFetchClassExpenses(params ClassExpense[] classExpenses)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => classExpenses.FirstOrDefault(classExpense => classExpense.Id == id));

    protected void ShouldSearchClassExpenses(params ClassExpense[] classExpenses)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<ClassExpense>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<ClassExpense> specification, CancellationToken _)
                => InMemorySearch.Run(classExpenses, specification));

    protected void ShouldFetchNoClassExpenses()
        => this.ShouldFetchClassExpenses();

    protected void ShouldSearchNoClassExpenses()
        => this.ShouldSearchClassExpenses();

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<ClassExpense>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

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
}
