namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Students.Domain.Services;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;

[TestFixture]
public class StudentFinderByIdShould : StudentsUnitTestCase
{
    private readonly StudentFinderById.Finder _useCase;

    public StudentFinderByIdShould()
    {
        this._useCase = new StudentFinderById.Finder(new StudentFinder(this.Repository.Object));
    }

    [Test]
    public async Task return_the_id_of_the_requested_student_only()
    {
        Student requested = StudentMother.GuardedBy(GuidMother.Random());
        Student other = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldFetchStudents(other, requested);

        StudentFinderById.StudentResponse result = await this._useCase.ExecuteAsync(
            StudentFinderByIdQueryMother.WithStudentId(requested.Id));

        result.Should().Be(StudentResponseMother.WithId(requested.Id));
    }

    [Test]
    public async Task throw_not_found_when_the_student_does_not_exist()
    {
        this.ShouldFetchStudents(StudentMother.GuardedBy(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentFinderByIdQueryMother.WithStudentId(GuidMother.Random()));

        await act.Should().ThrowAsync<StudentExceptions.NotFound>();
    }
}
