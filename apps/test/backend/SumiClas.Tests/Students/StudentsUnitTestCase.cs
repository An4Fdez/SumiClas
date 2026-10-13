namespace SumiClas.Tests.Students;

using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;

public abstract class StudentsUnitTestCase : UnitTestCase
{
    private readonly List<Student> _stored = [];
    private readonly List<Student> _added = [];

    protected Mock<IStudentRepository> Repository { get; } = new();

    protected IReadOnlyList<Student> Added => this._added;

    [SetUp]
    public void StudentsSetUp()
    {
        this._stored.Clear();
        this._added.Clear();
        this.Repository.Reset();
        this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => this._stored.FirstOrDefault(student => student.Id == id));
        this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Student>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Student> specification, CancellationToken _)
                => (IReadOnlyCollection<Student>)this._stored.Where(specification.Criteria.Compile()).ToList());
        this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .Callback<Student, CancellationToken>((student, _) => this._added.Add(student))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldReturn(params Student[] students) => this._stored.AddRange(students);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Student>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveFetched()
        => this.Repository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
