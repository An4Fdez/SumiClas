namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;

[TestFixture]
public class StudentGuardianRemoveShould : StudentsUnitTestCase
{
    private readonly StudentGuardianRemove.Remove _useCase;

    public StudentGuardianRemoveShould()
    {
        this._useCase = new StudentGuardianRemove.Remove(this.Repository.Object, this.UserProvider.Object);
    }

    [Test]
    public async Task remove_the_other_guardian_when_the_current_user_is_a_guardian_of_the_student()
    {
        Guid currentId = GuidMother.Random();
        Guid otherId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId, otherId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        await this._useCase.ExecuteAsync(StudentGuardianRemoveCommandMother.With(student.Id, otherId));

        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task let_a_guardian_remove_themselves_when_the_student_has_other_guardians()
    {
        Guid currentId = GuidMother.Random();
        Guid otherId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId, otherId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        await this._useCase.ExecuteAsync(
            StudentGuardianRemoveCommandMother.With(student.Id, currentId));

        student.Guardians.Should().Equal(GuardianLinkMother.Father(otherId));
    }

    [Test]
    public async Task throw_at_least_one_guardian_required_and_keep_the_guardian_when_it_is_the_last_one()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianRemoveCommandMother.With(student.Id, currentId));

        await act.Should().ThrowAsync<StudentExceptions.AtLeastOneGuardianRequired>();
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task throw_guardian_not_linked_before_the_last_guardian_rule_when_the_guardian_is_not_linked()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianRemoveCommandMother.With(student.Id, GuidMother.Random()));

        await act.Should().ThrowAsync<StudentExceptions.GuardianNotLinked>();
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task throw_guardian_change_forbidden_and_keep_the_guardians_when_the_current_user_is_not_a_guardian()
    {
        Guid motherId = GuidMother.Random();
        Guid fatherId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(motherId, fatherId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianRemoveCommandMother.With(student.Id, fatherId));

        await act.Should().ThrowAsync<StudentExceptions.GuardianChangeForbidden>();
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(motherId), GuardianLinkMother.Father(fatherId));
    }

    [Test]
    public async Task throw_not_found_when_the_student_does_not_exist()
    {
        this.ShouldFetchStudents(StudentMother.GuardedBy(GuidMother.Random()));
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianRemoveCommandMother.With(GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<StudentExceptions.NotFound>();
    }

    [Test]
    public async Task throw_unauthenticated_without_fetching_the_student_when_there_is_no_current_user()
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random(), GuidMother.Random());
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianRemoveCommandMother.With(student.Id, GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveFetched();
        student.Guardians.Should().HaveCount(2);
    }
}
