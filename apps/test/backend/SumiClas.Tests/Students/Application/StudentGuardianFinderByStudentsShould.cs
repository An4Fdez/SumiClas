namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;

[TestFixture]
public class StudentGuardianFinderByStudentsShould : StudentsUnitTestCase
{
    private readonly StudentGuardianFinderByStudents.Finder _useCase;

    public StudentGuardianFinderByStudentsShould()
    {
        this._useCase = new StudentGuardianFinderByStudents.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_the_guardian_ids_of_the_requested_students_only()
    {
        Guid motherA = GuidMother.Random();
        Guid motherB = GuidMother.Random();
        Guid father = GuidMother.Random();
        Student requested = StudentMother.GuardedBy(motherA, father);
        Student other = StudentMother.GuardedBy(motherB);
        Student[] students = [requested, other];
        this.ShouldSearchStudents(students);

        IReadOnlyCollection<Guid> result = await this._useCase.ExecuteAsync(
            StudentGuardianFinderByStudentsQueryMother.WithStudentIds(requested.Id),
            CancellationToken.None);

        result.Should().BeEquivalentTo([motherA, father]);
        result.Should().NotContain(motherB);
    }

    [Test]
    public async Task return_each_guardian_once_when_the_students_are_siblings_of_the_same_guardian()
    {
        Guid mother = GuidMother.Random();
        Student first = StudentMother.GuardedBy(mother);
        Student second = StudentMother.GuardedBy(mother);
        this.ShouldSearchStudents(first, second);

        IReadOnlyCollection<Guid> result = await this._useCase.ExecuteAsync(
            StudentGuardianFinderByStudentsQueryMother.WithStudentIds(first.Id, second.Id),
            CancellationToken.None);

        result.Should().Equal(mother);
    }

    [Test]
    public async Task return_empty_without_querying_the_repository_when_there_are_no_student_ids()
    {
        IReadOnlyCollection<Guid> result = await this._useCase.ExecuteAsync(
            StudentGuardianFinderByStudentsQueryMother.WithoutStudents(),
            CancellationToken.None);

        result.Should().BeEmpty();
        this.ShouldNotHaveSearched();
    }
}
