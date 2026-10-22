namespace SumiClas.Tests.Students;

using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class StudentsUnitTestCase : UnitTestCase
{
    private readonly List<Student> _added = [];

    protected Mock<IStudentRepository> Repository { get; } = new();

    protected IReadOnlyList<Student> Added => this._added;

    [SetUp]
    public void StudentsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldFetchStudents(params Student[] students)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => students.FirstOrDefault(student => student.Id == id));

    protected void ShouldSearchStudents(params Student[] students)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Student>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Student> specification, CancellationToken _)
                => (IReadOnlyCollection<Student>)students.Where(specification.Criteria.Compile()).ToList());

    protected void ShouldCaptureAddedStudents()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .Callback<Student, CancellationToken>((student, _) => this._added.Add(student))
            .Returns(Task.CompletedTask);

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

    protected void ShouldFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserResponseMother.With(userId, StringMother.Random("name")));

    protected void ShouldNotFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.NotFound());
}
