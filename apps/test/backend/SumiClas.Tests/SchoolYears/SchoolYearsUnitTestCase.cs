namespace SumiClas.Tests.SchoolYears;

using Moq;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class SchoolYearsUnitTestCase : UnitTestCase
{
    private readonly List<SchoolYear> _stored = [];
    private readonly List<SchoolYear> _added = [];

    protected Mock<ISchoolYearRepository> Repository { get; } = new();

    protected IReadOnlyList<SchoolYear> Added => this._added;

    [SetUp]
    public void SchoolYearsSetUp()
    {
        this._stored.Clear();
        this._added.Clear();
        this.Repository.Reset();
        this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => this._stored.FirstOrDefault(schoolYear => schoolYear.Id == id));
        this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<SchoolYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<SchoolYear> specification, CancellationToken _)
                => InMemorySearch.Run(this._stored, specification));
        this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<SchoolYear>(), It.IsAny<CancellationToken>()))
            .Callback<SchoolYear, CancellationToken>((schoolYear, _) => this._added.Add(schoolYear))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldReturn(params SchoolYear[] schoolYears) => this._stored.AddRange(schoolYears);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<SchoolYear>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
