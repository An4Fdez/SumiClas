namespace SumiClas.Tests.Classes.Application;

using AwesomeAssertions;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassRenameShould : ClassesUnitTestCase
{
    private readonly ClassRename.Rename _useCase;

    public ClassRenameShould()
    {
        this._useCase = new ClassRename.Rename(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task rename_the_class_with_the_trimmed_name_when_its_school_year_is_active()
    {
        Guid schoolYearId = GuidMother.Random();
        string newName = StringMother.Random("class");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, StringMother.Random("old"));
        this.ShouldFetchClasses(schoolClass);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        Result result = await this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, "  " + newName + " "));

        result.IsSuccess.Should().BeTrue();
        schoolClass.Name.Should().Be(newName);
        schoolClass.SchoolYearId.Should().Be(schoolYearId);
    }

    [Test]
    public async Task return_not_found_without_asking_the_school_year_when_the_class_does_not_exist()
    {
        this.ShouldFetchNoClasses();

        Result result = await this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(GuidMother.Random(), StringMother.Random("class")));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_name_required_and_keep_the_name_when_the_new_name_is_blank()
    {
        Guid schoolYearId = GuidMother.Random();
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, oldName);
        this.ShouldFetchClasses(schoolClass);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        Result result = await this._useCase.ExecuteAsync(ClassRenameCommandMother.With(schoolClass.Id, "   "));

        result.Error.Should().Be(ClassErrors.NameRequired);
        schoolClass.Name.Should().Be(oldName);
    }

    [Test]
    public async Task return_school_year_archived_and_keep_the_name_when_the_school_year_is_archived()
    {
        Guid schoolYearId = GuidMother.Random();
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, oldName);
        this.ShouldFetchClasses(schoolClass);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, StringMother.Random("class")));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        schoolClass.Name.Should().Be(oldName);
    }

    [Test]
    public async Task return_school_year_archived_before_validating_the_name_when_both_fail()
    {
        Guid schoolYearId = GuidMother.Random();
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, StringMother.Random("old"));
        this.ShouldFetchClasses(schoolClass);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(ClassRenameCommandMother.With(schoolClass.Id, " "));

        result.Error.Should().Be(SchoolYearErrors.Archived);
    }

    [Test]
    public async Task return_school_year_not_found_and_keep_the_name_when_the_school_year_does_not_exist()
    {
        Guid schoolYearId = GuidMother.Random();
        string oldName = StringMother.Random("old");
        Class schoolClass = ClassMother.With(GuidMother.Random(), schoolYearId, oldName);
        this.ShouldFetchClasses(schoolClass);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            ClassRenameCommandMother.With(schoolClass.Id, StringMother.Random("class")));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        schoolClass.Name.Should().Be(oldName);
    }
}
