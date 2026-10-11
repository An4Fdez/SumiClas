using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;

namespace SumiClas.Tests.Students.Application;

[TestFixture]
public class StudentGuardianFinderByStudentsTests
{
    private Mock<IStudentRepository> _students = null!;
    private List<Student> _store = null!;
    private StudentGuardianFinderByStudents.Finder _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        _students = new Mock<IStudentRepository>();
        _students
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<Student>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<Student>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<Student>>(_store.Where(specification.Criteria.Compile()).ToList()));
        _useCase = new StudentGuardianFinderByStudents.Finder(_students.Object);
    }

    private Student AddStudent(string firstName, params Guid[] guardianIds)
    {
        var student = Student.Register(
            Guid.NewGuid(),
            StudentName.Create(firstName, "Pérez").Value,
            guardianIds[0],
            GuardianRole.Mother).Value;
        foreach (var guardianId in guardianIds.Skip(1))
        {
            student.AddGuardian(guardianId, GuardianRole.Father);
        }

        _store.Add(student);
        return student;
    }

    [Test]
    public async Task UseCase_ReturnsTheGuardianIdsOfTheRequestedStudentsOnly()
    {
        var motherA = Guid.NewGuid();
        var motherB = Guid.NewGuid();
        var father = Guid.NewGuid();
        var requested = AddStudent("Lucía", motherA, father);
        var other = AddStudent("Mateo", motherB);

        var result = await _useCase.ExecuteAsync(new StudentGuardianFinderByStudents.Query([requested.Id]), CancellationToken.None);

        Assert.That(result, Is.EquivalentTo(new[] { motherA, father }));
        Assert.That(result, Does.Not.Contain(motherB));
        Assert.That(_store, Does.Contain(other));
    }

    [Test]
    public async Task UseCase_WithSiblingsOfTheSameGuardian_ReturnsEachGuardianOnce()
    {
        var mother = Guid.NewGuid();
        var first = AddStudent("Lucía", mother);
        var second = AddStudent("Mateo", mother);

        var result = await _useCase.ExecuteAsync(new StudentGuardianFinderByStudents.Query([first.Id, second.Id]), CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { mother }));
    }

    [Test]
    public async Task UseCase_WithNoStudentIds_ReturnsEmptyWithoutQueryingTheRepository()
    {
        var result = await _useCase.ExecuteAsync(new StudentGuardianFinderByStudents.Query([]), CancellationToken.None);

        Assert.That(result, Is.Empty);
        _students.Verify(
            repository => repository.SearchAsync(It.IsAny<ISpecification<Student>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UseCase_WithUnknownStudentIds_ReturnsEmpty()
    {
        AddStudent("Lucía", Guid.NewGuid());

        var result = await _useCase.ExecuteAsync(new StudentGuardianFinderByStudents.Query([Guid.NewGuid()]), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Query_ReturnsOnlyGuardianIds_NoStudentData()
    {
        Assert.That(
            typeof(StudentGuardianFinderByStudents.Query).GetInterfaces(),
            Does.Contain(typeof(IQuery<IReadOnlyCollection<Guid>>)));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new StudentGuardianFinderByStudents.Handler(_useCase);

        var result = await handler.Handle(new StudentGuardianFinderByStudents.Query([]), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }
}
