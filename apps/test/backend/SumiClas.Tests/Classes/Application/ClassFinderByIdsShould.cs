namespace SumiClas.Tests.Classes.Application;

using AwesomeAssertions;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Tests.Classes.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassFinderByIdsShould : ClassesUnitTestCase
{
    private readonly ClassFinderByIds.Finder _useCase;

    public ClassFinderByIdsShould()
    {
        this._useCase = new ClassFinderByIds.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_only_the_requested_classes_with_their_school_year_and_name()
    {
        Guid schoolYearId = GuidMother.Random();
        string name = StringMother.Random("class");
        Class requested = ClassMother.With(GuidMother.Random(), schoolYearId, name);
        Class otherRequested = ClassMother.InSchoolYear(GuidMother.Random());
        this.ShouldReturn(requested, otherRequested, ClassMother.InSchoolYear(GuidMother.Random()));

        IReadOnlyCollection<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderByIdsQueryMother.WithIds(requested.Id, otherRequested.Id));

        result.Select(response => response.Id).Should().BeEquivalentTo([requested.Id, otherRequested.Id]);
        ClassResponse found = result.Single(response => response.Id == requested.Id);
        found.SchoolYearId.Should().Be(schoolYearId);
        found.Name.Should().Be(name);
    }

    [Test]
    public async Task skip_the_ids_that_do_not_exist()
    {
        Class existing = ClassMother.InSchoolYear(GuidMother.Random());
        this.ShouldReturn(existing);

        IReadOnlyCollection<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderByIdsQueryMother.WithIds(GuidMother.Random(), existing.Id));

        result.Select(response => response.Id).Should().Equal(existing.Id);
    }

    [Test]
    public async Task return_empty_when_no_class_matches_the_ids()
    {
        this.ShouldReturn(ClassMother.InSchoolYear(GuidMother.Random()));

        IReadOnlyCollection<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderByIdsQueryMother.WithIds(GuidMother.Random()));

        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_empty_when_no_ids_are_requested()
    {
        this.ShouldReturn(ClassMother.InSchoolYear(GuidMother.Random()));

        IReadOnlyCollection<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderByIdsQueryMother.WithIds());

        result.Should().BeEmpty();
    }
}
