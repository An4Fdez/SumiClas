namespace SumiClas.Tests.Students;

using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;

public abstract class StudentsUnitTestCase : UnitTestCase
{
    protected Mock<IStudentRepository> Repository { get; } = new();

    [SetUp]
    public void StudentsSetUp()
    {
        this.Repository.Reset();
        this.ShouldReturn();
    }

    protected void ShouldReturn(params Student[] students)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Student>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Student> specification, CancellationToken _)
                => (IReadOnlyCollection<Student>)students.Where(specification.Criteria.Compile()).ToList());

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Student>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
