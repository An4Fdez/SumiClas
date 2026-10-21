namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using MediatR;
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

        Func<Task> act = () => this.Ask(student.Id, guardianId);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task allow_any_of_the_other_guardians_of_the_student()
    {
        Guid otherGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(GuidMother.Random(), otherGuardianId);
        this.ShouldFetchStudents(student);

        Func<Task> act = () => this.Ask(student.Id, otherGuardianId);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task deny_a_user_who_is_not_a_guardian_of_the_student()
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random(), GuidMother.Random());
        this.ShouldFetchStudents(student);

        Func<Task> act = () => this.Ask(student.Id, GuidMother.Random());

        await act.Should().ThrowAsync<StudentExceptions.GuardianAccessForbidden>();
    }

    [Test]
    public async Task deny_a_guardian_of_another_student()
    {
        Guid guardianId = GuidMother.Random();
        Student requested = StudentMother.GuardedBy(GuidMother.Random());
        Student other = StudentMother.GuardedBy(guardianId);
        this.ShouldFetchStudents(requested, other);

        Func<Task> act = () => this.Ask(requested.Id, guardianId);

        await act.Should().ThrowAsync<StudentExceptions.GuardianAccessForbidden>();
    }

    [Test]
    public async Task throw_not_found_when_the_student_does_not_exist()
    {
        this.ShouldFetchStudents(StudentMother.GuardedBy(GuidMother.Random()));

        Func<Task> act = () => this.Ask(GuidMother.Random(), GuidMother.Random());

        await act.Should().ThrowAsync<StudentExceptions.NotFound>();
    }

    private Task<Unit> Ask(Guid studentId, Guid userId)
        => this._useCase.ExecuteAsync(StudentGuardianEnsureQueryMother.With(studentId, userId));
}
