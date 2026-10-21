namespace SumiClas.Tests.Classes.Application;

using AwesomeAssertions;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class ClassRenameShould : ClassesUnitTestCase
{
    private readonly ClassRename.Rename _useCase;

    public ClassRenameShould()
    {
        this._useCase = new ClassRename.Rename(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task rename_the_class_with_the_trimmed_name_when_its_school_year_is_active()
    {
        Guid schoolYearId = GuidMother.Random();
        string newName = StringMother.Random("class");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, StringMother.Random("old"));
        this.ShouldFetchClasses(schoolClass);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, schoolClass.Id);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        await this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, "  " + newName + " "));

        schoolClass.Name.Should().Be(newName);
        schoolClass.SchoolYearId.Should().Be(schoolYearId);
    }

    [Test]
    public async Task throw_not_found_without_asking_the_school_year_when_the_class_does_not_exist()
    {
        this.ShouldFetchNoClasses();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(GuidMother.Random(), StringMother.Random("class")));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_name_required_and_keep_the_name_when_the_new_name_is_blank()
    {
        Guid schoolYearId = GuidMother.Random();
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, oldName);
        this.ShouldFetchClasses(schoolClass);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, schoolClass.Id);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassRenameCommandMother.With(schoolClass.Id, "   "));

        await act.Should().ThrowAsync<ClassExceptions.NameRequired>();
        schoolClass.Name.Should().Be(oldName);
    }

    [Test]
    public async Task throw_school_year_archived_and_keep_the_name_when_the_school_year_is_archived()
    {
        Guid schoolYearId = GuidMother.Random();
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, oldName);
        this.ShouldFetchClasses(schoolClass);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, schoolClass.Id);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, StringMother.Random("class")));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        schoolClass.Name.Should().Be(oldName);
    }

    [Test]
    public async Task throw_school_year_archived_before_validating_the_name_when_both_fail()
    {
        Guid schoolYearId = GuidMother.Random();
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, StringMother.Random("old"));
        this.ShouldFetchClasses(schoolClass);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, schoolClass.Id);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(ClassRenameCommandMother.With(schoolClass.Id, " "));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
    }

    [Test]
    public async Task throw_school_year_not_found_and_keep_the_name_when_the_school_year_does_not_exist()
    {
        Guid schoolYearId = GuidMother.Random();
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, oldName);
        this.ShouldFetchClasses(schoolClass);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, schoolClass.Id);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, StringMother.Random("class")));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        schoolClass.Name.Should().Be(oldName);
    }

    [Test]
    public async Task throw_class_representative_forbidden_and_keep_the_name_when_the_user_is_not_a_delegate()
    {
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), GuidMother.Random(), oldName);
        Guid currentUserId = GuidMother.Random();
        this.ShouldFetchClasses(schoolClass);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassRepresentative(currentUserId, schoolClass.Id);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, StringMother.Random("class")));

        (await act.Should().ThrowAsync<ResultFailureException>())
            .Which.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
        schoolClass.Name.Should().Be(oldName);
    }

    [Test]
    public async Task throw_unauthenticated_and_keep_the_name_when_there_is_no_current_user()
    {
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), GuidMother.Random(), oldName);
        this.ShouldFetchClasses(schoolClass);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, StringMother.Random("class")));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        schoolClass.Name.Should().Be(oldName);
    }
}
