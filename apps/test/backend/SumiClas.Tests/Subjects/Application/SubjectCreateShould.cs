namespace SumiClas.Tests.Subjects.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Classes;
using SumiClas.Tests.SchoolYears;
using SumiClas.Tests.Shared;

[TestFixture]
public class SubjectCreateShould : SubjectsUnitTestCase
{
    private readonly SubjectCreate.Create _useCase;

    public SubjectCreateShould()
    {
        this._useCase = new SubjectCreate.Create(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task add_the_subject_with_its_data_and_the_trimmed_label_when_the_school_year_is_active()
    {
        Guid subjectId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        string label = StringMother.Random("subject");
        this.ShouldResolveActiveClass(classId, GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(subjectId, classId, "  " + label + " "));

        result.IsSuccess.Should().BeTrue();
        Subject added = this.Added.Single();
        added.Id.Should().Be(subjectId);
        added.ClassId.Should().Be(classId);
        added.Label.Should().Be(label);
        added.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task return_class_not_found_without_adding_when_the_class_does_not_exist()
    {
        this.QueryBus.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(SubjectCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            StringMother.Random("subject")));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_year_archived_without_adding_when_the_school_year_is_archived()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), classId, StringMother.Random("subject")));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), classId, StringMother.Random("subject")));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_subject_id_is_empty()
    {
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(Guid.Empty, GuidMother.Random(), StringMother.Random("subject")));

        result.Error.Should().Be(SubjectErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_class_id_is_empty()
    {
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), Guid.Empty, StringMother.Random("subject")));

        result.Error.Should().Be(SubjectErrors.ClassRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task return_label_required_without_asking_or_adding_when_the_label_is_blank(string label)
    {
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), GuidMother.Random(), label));

        result.Error.Should().Be(SubjectErrors.LabelRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_label_required_instead_of_class_not_found_when_both_fail()
    {
        this.QueryBus.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), GuidMother.Random(), " "));

        result.Error.Should().Be(SubjectErrors.LabelRequired);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_when_every_field_is_invalid()
    {
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(Guid.Empty, Guid.Empty, " "));

        result.Error.Should().Be(SubjectErrors.IdRequired);
    }

    [Test]
    public async Task return_class_required_when_the_class_id_and_the_label_are_invalid()
    {
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), Guid.Empty, " "));

        result.Error.Should().Be(SubjectErrors.ClassRequired);
    }

    private void ShouldResolveActiveClass(Guid classId, Guid schoolYearId)
    {
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldEnsureSchoolYearIsActive(schoolYearId);
    }
}
