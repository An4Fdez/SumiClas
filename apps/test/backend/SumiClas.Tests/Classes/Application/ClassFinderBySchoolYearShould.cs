namespace SumiClas.Tests.Classes.Application;

using AwesomeAssertions;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Tests.Classes.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassFinderBySchoolYearShould : ClassesUnitTestCase
{
    private readonly ClassFinderBySchoolYear.Finder _useCase;
    private readonly ClassFinderBySchoolYear.Handler _handler;

    public ClassFinderBySchoolYearShould()
    {
        this._useCase = new ClassFinderBySchoolYear.Finder(this.Repository.Object);
        this._handler = new ClassFinderBySchoolYear.Handler(this._useCase);
    }

    [Test]
    public async Task return_only_the_classes_of_the_school_year_with_their_name()
    {
        Guid schoolYearId = GuidMother.Random();
        string name = StringMother.Random("class");
        Class first = ClassMother.With(GuidMother.Random(), schoolYearId, name);
        Class second = ClassMother.InSchoolYear(schoolYearId);
        this.ShouldReturn(first, ClassMother.InSchoolYear(GuidMother.Random()), second);

        IReadOnlyCollection<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderBySchoolYearQueryMother.WithSchoolYearId(schoolYearId));

        result.Select(response => response.Id).Should().BeEquivalentTo([first.Id, second.Id]);
        result.Should().OnlyContain(response => response.SchoolYearId == schoolYearId);
        result.Single(response => response.Id == first.Id).Name.Should().Be(name);
    }

    [Test]
    public async Task return_empty_when_the_school_year_has_no_classes()
    {
        this.ShouldReturn(ClassMother.InSchoolYear(GuidMother.Random()));

        IReadOnlyCollection<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderBySchoolYearQueryMother.WithSchoolYearId(GuidMother.Random()));

        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        Guid schoolYearId = GuidMother.Random();
        Class existing = ClassMother.InSchoolYear(schoolYearId);
        this.ShouldReturn(existing);

        IReadOnlyCollection<ClassResponse> result = await this._handler.Handle(
            ClassFinderBySchoolYearQueryMother.WithSchoolYearId(schoolYearId),
            CancellationToken.None);

        result.Select(response => response.Id).Should().Equal(existing.Id);
    }
}
