namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;
using SumiClas.Users.Domain;

[TestFixture]
public class StudentGuardianAddShould : StudentsUnitTestCase
{
    private readonly StudentGuardianAdd.Add _useCase;

    public StudentGuardianAddShould()
    {
        this._useCase = new StudentGuardianAdd.Add(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task link_the_new_guardian_with_the_role_when_the_current_user_is_a_guardian_of_the_student()
    {
        Guid currentId = GuidMother.Random();
        Guid newGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);
        this.ShouldFindUser(newGuardianId);

        await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, nameof(GuardianRole.Father)));

        student.Guardians.Should().Equal(
            GuardianLinkMother.Mother(currentId),
            GuardianLinkMother.Father(newGuardianId));
    }

    [Test]
    public async Task let_a_secondary_guardian_link_another_guardian()
    {
        Guid motherId = GuidMother.Random();
        Guid currentId = GuidMother.Random();
        Guid newGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(motherId, currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);
        this.ShouldFindUser(newGuardianId);

        await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, nameof(GuardianRole.Guardian)));

        student.Guardians.Should().HaveCount(3);
        student.Guardians.Should().Contain(GuardianLinkMother.Create(newGuardianId, GuardianRole.Guardian));
    }

    [TestCase("mother", GuardianRole.Mother)]
    [TestCase("GUARDIAN", GuardianRole.Guardian)]
    public async Task link_the_guardian_with_the_role_ignoring_the_case(string role, GuardianRole expected)
    {
        Guid currentId = GuidMother.Random();
        Guid newGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);
        this.ShouldFindUser(newGuardianId);

        await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, role));

        student.Guardians.Should().Contain(GuardianLinkMother.Create(newGuardianId, expected));
    }

    [TestCase("   ")]
    [TestCase("Uncle")]
    [TestCase("99")]
    public async Task throw_invalid_guardian_role_without_fetching_or_asking_when_the_role_is_not_valid(string role)
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, GuidMother.Random(), role));

        await act.Should().ThrowAsync<StudentExceptions.InvalidGuardianRole>();
        this.ShouldNotHaveFetched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_unauthenticated_without_fetching_or_asking_when_there_is_no_current_user()
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, GuidMother.Random(), "Uncle"));

        (await act.Should().ThrowAsync<ResultFailureException>())
            .Which.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.ShouldNotHaveFetched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_not_found_without_asking_for_the_guardian_when_the_student_does_not_exist()
    {
        this.ShouldFetchStudents(StudentMother.GuardedBy(GuidMother.Random()));
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(
                GuidMother.Random(),
                GuidMother.Random(),
                nameof(GuardianRole.Mother)));

        await act.Should().ThrowAsync<StudentExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_guardian_change_forbidden_without_asking_when_the_current_user_is_not_a_guardian()
    {
        Guid motherId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(motherId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, GuidMother.Random(), nameof(GuardianRole.Father)));

        await act.Should().ThrowAsync<StudentExceptions.GuardianChangeForbidden>();
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(motherId));
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_user_not_found_and_keep_the_guardians_when_the_new_guardian_does_not_exist()
    {
        Guid currentId = GuidMother.Random();
        Guid newGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);
        this.ShouldNotFindUser(newGuardianId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, nameof(GuardianRole.Father)));

        (await act.Should().ThrowAsync<ResultFailureException>())
            .Which.Error.Should().Be(UserErrors.NotFound);
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task throw_guardian_already_linked_and_keep_the_original_role_when_the_guardian_is_already_linked()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);
        this.ShouldFindUser(currentId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, currentId, nameof(GuardianRole.Father)));

        await act.Should().ThrowAsync<StudentExceptions.GuardianAlreadyLinked>();
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task throw_guardian_id_required_and_keep_the_guardians_when_the_new_guardian_id_is_empty()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldFetchStudents(student);
        this.ShouldResolveCurrentUser(currentId);
        this.ShouldFindUser(Guid.Empty);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, Guid.Empty, nameof(GuardianRole.Father)));

        await act.Should().ThrowAsync<StudentExceptions.GuardianIdRequired>();
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }
}
