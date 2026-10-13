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
    private readonly ClassFinderById.Handler _handler;

    public ClassFinderByIdShould()
    {
        this._useCase = new ClassFinderById.Finder(new ClassByIdFinder(this.Repository.Object));
        this._handler = new ClassFinderById.Handler(this._useCase);
    }

    [Test]
    public async Task return_the_class_with_its_school_year_and_name_when_it_exists()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        string name = StringMother.Random("class");
        this.ShouldReturn(
            ClassMother.InSchoolYear(GuidMother.Random()),
            ClassMother.With(classId, schoolYearId, name));

        Result<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderByIdQueryMother.WithClassId(classId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(classId);
        result.Value.SchoolYearId.Should().Be(schoolYearId);
        result.Value.Name.Should().Be(name);
    }

    [Test]
    public async Task return_not_found_when_the_class_does_not_exist()
    {
        this.ShouldReturn(ClassMother.InSchoolYear(GuidMother.Random()));

        Result<ClassResponse> result = await this._useCase.ExecuteAsync(
            ClassFinderByIdQueryMother.WithClassId(GuidMother.Random()));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClassErrors.NotFound);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        Result<ClassResponse> result = await this._handler.Handle(
            ClassFinderByIdQueryMother.WithClassId(GuidMother.Random()),
            CancellationToken.None);

        result.Error.Should().Be(ClassErrors.NotFound);
    }
}
