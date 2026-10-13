namespace SumiClas.Tests.Classes;

using Moq;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class ClassesUnitTestCase : UnitTestCase
{
    private readonly List<Class> _stored = [];
    private readonly List<Class> _added = [];

    protected Mock<IClassRepository> Repository { get; } = new();

    protected IReadOnlyList<Class> Added => this._added;

    [SetUp]
    public void ClassesSetUp()
    {
        this._stored.Clear();
        this._added.Clear();
        this.Repository.Reset();
        this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => this._stored.FirstOrDefault(schoolClass => schoolClass.Id == id));
        this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Class>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Class> specification, CancellationToken _)
                => InMemorySearch.Run(this._stored, specification));
        this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Class>(), It.IsAny<CancellationToken>()))
            .Callback<Class, CancellationToken>((schoolClass, _) => this._added.Add(schoolClass))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldReturn(params Class[] classes) => this._stored.AddRange(classes);
}
