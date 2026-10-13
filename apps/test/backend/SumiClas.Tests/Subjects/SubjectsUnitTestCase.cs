namespace SumiClas.Tests.Subjects;

using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Shared;

public abstract class SubjectsUnitTestCase : UnitTestCase
{
    private readonly List<Subject> _stored = [];
    private readonly List<Subject> _added = [];

    protected Mock<ISubjectRepository> Repository { get; } = new();

    protected IReadOnlyList<Subject> Added => this._added;

    [SetUp]
    public void SubjectsSetUp()
    {
        this._stored.Clear();
        this._added.Clear();
        this.Repository.Reset();
        this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => this._stored.FirstOrDefault(subject => subject.Id == id));
        this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Subject>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Subject> specification, CancellationToken _)
                => InMemorySearch.Run(this._stored, specification));
        this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Subject>(), It.IsAny<CancellationToken>()))
            .Callback<Subject, CancellationToken>((subject, _) => this._added.Add(subject))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldReturn(params Subject[] subjects) => this._stored.AddRange(subjects);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Subject>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
