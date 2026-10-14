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

    public ClassFinderBySchoolYearShould()
    {
        this._useCase = new ClassFinderBySchoolYear.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_only_the_classes_of_the_school_year_with_their_name()
    {
        Guid schoolYearId = GuidMother.Random();
        string name = StringMother.Random("class");
        Class first = ClassMother.With(GuidMother.Random(), schoolYearId, name);
        Class second = ClassMother.InSchoolYear(schoolYearId);
        this.ShouldSearchClasses(first, ClassMother.InSchoolYear(GuidMother.Random()), second);

        IReadOnlyCollection<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderBySchoolYearQueryMother.WithSchoolYearId(schoolYearId));

        result.Select(response => response.Id).Should().BeEquivalentTo([first.Id, second.Id]);
        result.Should().OnlyContain(response => response.SchoolYearId == schoolYearId);
        result.Single(response => response.Id == first.Id).Name.Should().Be(name);
    }
}
