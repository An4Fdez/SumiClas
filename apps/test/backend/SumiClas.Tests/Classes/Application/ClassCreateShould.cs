namespace SumiClas.Tests.Classes.Application;

using AwesomeAssertions;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassCreateShould : ClassesUnitTestCase
{
    private readonly ClassCreate.Create _useCase;

    public ClassCreateShould()
    {
        this._useCase = new ClassCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_class_with_its_data_and_the_trimmed_name_when_the_school_year_is_active()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        string name = StringMother.Random("class");
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedClasses();

        Result result = await this._useCase.ExecuteAsync(
            ClassCreateCommandMother.With(classId, schoolYearId, "  " + name + " "));

        result.IsSuccess.Should().BeTrue();
        Class added = this.Added.Single();
        added.Id.Should().Be(classId);
        added.SchoolYearId.Should().Be(schoolYearId);
        added.Name.Should().Be(name);
    }

    [Test]
    public async Task raise_the_created_event_with_the_current_user_as_creator_and_the_clock_time()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid creatorId = GuidMother.Random();
        string name = StringMother.Random("class");
        this.ShouldResolveCurrentUser(creatorId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedClasses();

        await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(classId, schoolYearId, name));

        ClassCreatedDomainEvent created = this.Added.Single().DomainEvents
            .Should().ContainSingle().Which.Should().BeOfType<ClassCreatedDomainEvent>().Subject;
        created.ClassId.Should().Be(classId);
        created.SchoolYearId.Should().Be(schoolYearId);
        created.Name.Should().Be(name);
        created.CreatorId.Should().Be(creatorId);
        created.OccurredOn.Should().Be(Now);
    }

    [Test]
    public async Task return_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            StringMother.Random("class")));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_class_id_is_empty()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            Guid.Empty,
            GuidMother.Random(),
            StringMother.Random("class")));

        result.Error.Should().Be(ClassErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_school_year_required_without_asking_or_adding_when_the_school_year_id_is_empty()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            Guid.Empty,
            StringMother.Random("class")));

        result.Error.Should().Be(ClassErrors.SchoolYearRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_name_required_without_asking_or_adding_when_the_name_is_blank()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            "   "));

        result.Error.Should().Be(ClassErrors.NameRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_creator_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            StringMother.Random("class")));

        result.Error.Should().Be(ClassErrors.CreatorRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_school_year_archived_without_adding_when_the_school_year_is_archived()
    {
        this.ShouldCaptureAddedClasses();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            schoolYearId,
            StringMother.Random("class")));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        this.ShouldCaptureAddedClasses();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            schoolYearId,
            StringMother.Random("class")));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.Added.Should().BeEmpty();
    }
}
