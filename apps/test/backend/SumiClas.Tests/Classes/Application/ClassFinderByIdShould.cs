namespace SumiClas.Tests.Classes.Application;

using AwesomeAssertions;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Classes.Domain.Services;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassFinderByIdShould : ClassesUnitTestCase
{
    private readonly ClassFinderById.Finder _useCase;

    public ClassFinderByIdShould()
    {
        this._useCase = new ClassFinderById.Finder(new ClassByIdFinder(this.Repository.Object));
    }

    [Test]
    public async Task return_the_class_with_its_school_year_and_name_when_it_exists()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        string name = StringMother.Random("class");
        this.ShouldFetchClasses(
            ClassMother.InSchoolYear(GuidMother.Random()),
            ClassMother.With(classId, schoolYearId, name));

        ClassResponse result = await this._useCase.ExecuteAsync(
            ClassFinderByIdQueryMother.WithClassId(classId));
        result.Id.Should().Be(classId);
        result.SchoolYearId.Should().Be(schoolYearId);
        result.Name.Should().Be(name);
    }

    [Test]
    public async Task throw_not_found_when_the_class_does_not_exist()
    {
        this.ShouldFetchClasses(ClassMother.InSchoolYear(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassFinderByIdQueryMother.WithClassId(GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
    }
}
