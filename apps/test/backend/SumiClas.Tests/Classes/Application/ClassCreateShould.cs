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

        await this._useCase.ExecuteAsync(
            ClassCreateCommandMother.With(classId, schoolYearId, "  " + name + " "));

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
    public async Task throw_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            StringMother.Random("class")));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_id_required_without_asking_or_adding_when_the_class_id_is_empty()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            Guid.Empty,
            GuidMother.Random(),
            StringMother.Random("class")));

        await act.Should().ThrowAsync<ClassExceptions.IdRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_school_year_required_without_asking_or_adding_when_the_school_year_id_is_empty()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            Guid.Empty,
            StringMother.Random("class")));

        await act.Should().ThrowAsync<ClassExceptions.SchoolYearRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_name_required_without_asking_or_adding_when_the_name_is_blank()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            "   "));

        await act.Should().ThrowAsync<ClassExceptions.NameRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_creator_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldCaptureAddedClasses();
        this.ShouldResolveCurrentUser(Guid.Empty);

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            StringMother.Random("class")));

        await act.Should().ThrowAsync<ClassExceptions.CreatorRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_school_year_archived_without_adding_when_the_school_year_is_archived()
    {
        this.ShouldCaptureAddedClasses();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            schoolYearId,
            StringMother.Random("class")));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        this.ShouldCaptureAddedClasses();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassCreateCommandMother.With(
            GuidMother.Random(),
            schoolYearId,
            StringMother.Random("class")));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }
}
