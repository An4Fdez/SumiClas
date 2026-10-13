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
    private readonly StudentFinderById.Handler _handler;

    public StudentFinderByIdShould()
    {
        this._useCase = new StudentFinderById.Finder(new StudentFinder(this.Repository.Object));
        this._handler = new StudentFinderById.Handler(this._useCase);
    }

    [Test]
    public async Task return_the_id_of_the_requested_student_only()
    {
        Student requested = StudentMother.GuardedBy(GuidMother.Random());
        Student other = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldReturn(other, requested);

        Result<StudentFinderById.StudentResponse> result = await this._useCase.ExecuteAsync(
            StudentFinderByIdQueryMother.WithStudentId(requested.Id));

        result.Value.Should().Be(StudentResponseMother.WithId(requested.Id));
    }

    [Test]
    public async Task return_not_found_when_the_student_does_not_exist()
    {
        this.ShouldReturn(StudentMother.GuardedBy(GuidMother.Random()));

        Result<StudentFinderById.StudentResponse> result = await this._useCase.ExecuteAsync(
            StudentFinderByIdQueryMother.WithStudentId(GuidMother.Random()));

        result.Error.Should().Be(StudentErrors.NotFound);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldReturn(student);

        Result<StudentFinderById.StudentResponse> result = await this._handler.Handle(
            StudentFinderByIdQueryMother.WithStudentId(student.Id),
            CancellationToken.None);

        result.Value.Should().Be(StudentResponseMother.WithId(student.Id));
    }
}
