namespace SumiClas.Tests.Students.Application;

using AwesomeAssertions;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Domain;
using SumiClas.Users.Domain;

[TestFixture]
public class StudentRegisterShould : StudentsUnitTestCase
{
    private readonly StudentRegister.Register _useCase;

    public StudentRegisterShould()
    {
        this._useCase = new StudentRegister.Register(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_student_with_the_trimmed_name_and_the_current_user_as_its_only_guardian()
    {
        Guid studentId = GuidMother.Random();
        Guid guardianId = GuidMother.Random();
        string firstName = StringMother.Random("First");
        string firstSurname = StringMother.Random("Surname");
        string secondSurname = StringMother.Random("Second");
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindUser(guardianId);
        this.ShouldCaptureAddedStudents();

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.With(
            studentId,
            "  " + firstName + " ",
            " " + firstSurname + "  ",
            "  " + secondSurname + " ",
            nameof(GuardianRole.Father)));

        result.IsSuccess.Should().BeTrue();
        Student added = this.Added.Single();
        added.Id.Should().Be(studentId);
        added.Name.Should().Be(StudentNameMother.With(firstName, firstSurname, secondSurname));
        added.Guardians.Should().Equal(GuardianLinkMother.Father(guardianId));
        added.DomainEvents.Should().BeEmpty();
    }

    [TestCase(null)]
    [TestCase("   ")]
    public async Task store_no_second_surname_when_it_is_missing_or_blank(string? secondSurname)
    {
        Guid guardianId = GuidMother.Random();
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindUser(guardianId);
        this.ShouldCaptureAddedStudents();

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.With(
            GuidMother.Random(),
            StringMother.Random("First"),
            StringMother.Random("Surname"),
            secondSurname,
            nameof(GuardianRole.Mother)));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().Name.SecondSurname.Should().BeNull();
    }

    [TestCase("mother", GuardianRole.Mother)]
    [TestCase("GUARDIAN", GuardianRole.Guardian)]
    public async Task register_the_first_guardian_with_the_role_ignoring_the_case(string role, GuardianRole expected)
    {
        Guid guardianId = GuidMother.Random();
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindUser(guardianId);
        this.ShouldCaptureAddedStudents();

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithRole(role));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().Guardians.Should().Equal(GuardianLinkMother.Create(guardianId, expected));
    }

    [Test]
    public async Task accept_a_numeric_role_that_matches_a_defined_role()
    {
        Guid guardianId = GuidMother.Random();
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldFindUser(guardianId);
        this.ShouldCaptureAddedStudents();

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithRole("1"));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().Guardians.Should().Equal(GuardianLinkMother.Mother(guardianId));
    }

    [TestCase("   ")]
    [TestCase("Uncle")]
    [TestCase("99")]
    [TestCase("-1")]
    public async Task return_invalid_guardian_role_without_asking_or_adding_when_the_role_is_not_valid(string role)
    {
        this.ShouldCaptureAddedStudents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithRole(role));

        result.Error.Should().Be(StudentErrors.InvalidGuardianRole);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedStudents();
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithRole("Uncle"));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_name_required_without_asking_or_adding_when_the_first_name_is_blank()
    {
        this.ShouldCaptureAddedStudents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            StudentRegisterCommandMother.WithName("   ", StringMother.Random("Surname")));

        result.Error.Should().Be(StudentErrors.NameRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_first_surname_required_without_asking_or_adding_when_it_is_blank()
    {
        this.ShouldCaptureAddedStudents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            StudentRegisterCommandMother.WithName(StringMother.Random("First"), "   "));

        result.Error.Should().Be(StudentErrors.FirstSurnameRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_name_required_when_the_first_name_and_the_first_surname_are_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithName(" ", " "));

        result.Error.Should().Be(StudentErrors.NameRequired);
    }

    [Test]
    public async Task return_invalid_guardian_role_when_the_role_and_the_name_are_both_invalid()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.With(
            GuidMother.Random(),
            string.Empty,
            string.Empty,
            null,
            "Uncle"));

        result.Error.Should().Be(StudentErrors.InvalidGuardianRole);
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_student_id_is_empty()
    {
        this.ShouldCaptureAddedStudents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithId(Guid.Empty));

        result.Error.Should().Be(StudentErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_name_required_when_the_student_id_is_empty_and_the_name_is_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.With(
            Guid.Empty,
            string.Empty,
            StringMother.Random("Surname"),
            null,
            nameof(GuardianRole.Mother)));

        result.Error.Should().Be(StudentErrors.NameRequired);
    }

    [Test]
    public async Task return_first_guardian_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldCaptureAddedStudents();
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithId(GuidMother.Random()));

        result.Error.Should().Be(StudentErrors.FirstGuardianRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_user_not_found_without_adding_when_the_current_user_does_not_exist()
    {
        this.ShouldCaptureAddedStudents();
        Guid guardianId = GuidMother.Random();
        this.ShouldResolveCurrentUser(guardianId);
        this.ShouldNotFindUser(guardianId);

        Result result = await this._useCase.ExecuteAsync(StudentRegisterCommandMother.WithId(GuidMother.Random()));

        result.Error.Should().Be(UserErrors.NotFound);
        this.Added.Should().BeEmpty();
    }
}
