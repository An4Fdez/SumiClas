namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;
using SumiClas.Tests.Users;
using SumiClas.Users.Domain;

[TestFixture]
public class StudentGuardianAddShould : StudentsUnitTestCase
{
    private readonly StudentGuardianAdd.Add _useCase;
    private readonly StudentGuardianAdd.Handler _handler;

    public StudentGuardianAddShould()
    {
        this._useCase = new StudentGuardianAdd.Add(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
        this._handler = new StudentGuardianAdd.Handler(this._useCase);
    }

    [Test]
    public async Task link_the_new_guardian_with_the_role_when_the_current_user_is_a_guardian_of_the_student()
    {
        Guid currentId = GuidMother.Random();
        Guid newGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(currentId);
        this.QueryBus.ShouldFindUser(newGuardianId);

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, nameof(GuardianRole.Father)));

        result.IsSuccess.Should().BeTrue();
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
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(currentId);
        this.QueryBus.ShouldFindUser(newGuardianId);

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, nameof(GuardianRole.Guardian)));

        result.IsSuccess.Should().BeTrue();
        student.Guardians.Should().HaveCount(3);
        student.Guardians.Should().Contain(GuardianLinkMother.Create(newGuardianId, GuardianRole.Guardian));
    }

    [TestCase("Father", GuardianRole.Father)]
    [TestCase("mother", GuardianRole.Mother)]
    [TestCase("GUARDIAN", GuardianRole.Guardian)]
    public async Task link_the_guardian_with_the_role_ignoring_the_case(string role, GuardianRole expected)
    {
        Guid currentId = GuidMother.Random();
        Guid newGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(currentId);
        this.QueryBus.ShouldFindUser(newGuardianId);

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, role));

        result.IsSuccess.Should().BeTrue();
        student.Guardians.Should().Contain(GuardianLinkMother.Create(newGuardianId, expected));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("Uncle")]
    [TestCase("99")]
    public async Task return_invalid_guardian_role_without_fetching_or_asking_when_the_role_is_not_valid(string role)
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, GuidMother.Random(), role));

        result.Error.Should().Be(StudentErrors.InvalidGuardianRole);
        this.ShouldNotHaveFetched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_unauthenticated_without_fetching_or_asking_when_there_is_no_current_user()
    {
        Student student = StudentMother.GuardedBy(GuidMother.Random());
        this.ShouldReturn(student);
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, GuidMother.Random(), "Uncle"));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.ShouldNotHaveFetched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_not_found_without_asking_for_the_guardian_when_the_student_does_not_exist()
    {
        this.ShouldReturn(StudentMother.GuardedBy(GuidMother.Random()));
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(
                GuidMother.Random(),
                GuidMother.Random(),
                nameof(GuardianRole.Mother)));

        result.Error.Should().Be(StudentErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_guardian_change_forbidden_without_asking_when_the_current_user_is_not_a_guardian()
    {
        Guid motherId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(motherId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, GuidMother.Random(), nameof(GuardianRole.Father)));

        result.Error.Should().Be(StudentErrors.GuardianChangeForbidden);
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(motherId));
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_user_not_found_and_keep_the_guardians_when_the_new_guardian_does_not_exist()
    {
        Guid currentId = GuidMother.Random();
        Guid newGuardianId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(currentId);
        this.QueryBus.ShouldNotFindUser(newGuardianId);

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, newGuardianId, nameof(GuardianRole.Father)));

        result.Error.Should().Be(UserErrors.NotFound);
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task return_guardian_already_linked_and_keep_the_original_role_when_the_guardian_is_already_linked()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(currentId);
        this.QueryBus.ShouldFindUser(currentId);

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, currentId, nameof(GuardianRole.Father)));

        result.Error.Should().Be(StudentErrors.GuardianAlreadyLinked);
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task return_guardian_id_required_and_keep_the_guardians_when_the_new_guardian_id_is_empty()
    {
        Guid currentId = GuidMother.Random();
        Student student = StudentMother.GuardedBy(currentId);
        this.ShouldReturn(student);
        this.ShouldResolveCurrentUser(currentId);
        this.QueryBus.ShouldFindUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(
            StudentGuardianAddCommandMother.With(student.Id, Guid.Empty, nameof(GuardianRole.Father)));

        result.Error.Should().Be(StudentErrors.GuardianIdRequired);
        student.Guardians.Should().Equal(GuardianLinkMother.Mother(currentId));
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_command()
    {
        this.ShouldHaveNoCurrentUser();

        Result result = await this._handler.Handle(
            StudentGuardianAddCommandMother.With(GuidMother.Random(), GuidMother.Random(), nameof(GuardianRole.Mother)),
            CancellationToken.None);

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
    }
}
