namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Students.Domain.Services;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;

[TestFixture]
public class StudentGuardianEnsureShould : StudentsUnitTestCase
{
    private readonly StudentGuardianEnsure.Ensure _useCase;

    public StudentGuardianEnsureShould()
    {
        this._useCase = new StudentGuardianEnsure.Ensure(new StudentFinder(this.Repository.Object));
    }

    [Test]
    public async Task allow_the_main_guardian_of_the_student()
    {
        Guid guardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(guardianId);
        this.ShouldFetchStudents(student);

        Result result = await this.Ask(student.Id, guardianId);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task allow_any_of_the_other_guardians_of_the_student()
    {
        Guid otherGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(GuidMother.Random(), otherGuardianId);
        this.ShouldFetchStudents(student);

        Result result = await this.Ask(student.Id, otherGuardianId);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task deny_a_user_who_is_not_a_guardian_of_the_student()
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random(), GuidMother.Random());
        this.ShouldFetchStudents(student);

        Result result = await this.Ask(student.Id, GuidMother.Random());

        result.Error.Should().Be(StudentErrors.GuardianAccessForbidden);
    }

    [Test]
    public async Task deny_a_guardian_of_another_student()
    {
        Guid guardianId = GuidMother.Random();
        Student requested = StudentMother.GuardedBy(GuidMother.Random());
        Student other = StudentMother.GuardedBy(guardianId);
        this.ShouldFetchStudents(requested, other);

        Result result = await this.Ask(requested.Id, guardianId);

        result.Error.Should().Be(StudentErrors.GuardianAccessForbidden);
    }

    [Test]
    public async Task return_not_found_when_the_student_does_not_exist()
    {
        this.ShouldFetchStudents(StudentMother.GuardedBy(GuidMother.Random()));

        Result result = await this.Ask(GuidMother.Random(), GuidMother.Random());

        result.Error.Should().Be(StudentErrors.NotFound);
    }

    private Task<Result> Ask(Guid studentId, Guid userId)
        => this._useCase.ExecuteAsync(StudentGuardianEnsureQueryMother.With(studentId, userId));
}
