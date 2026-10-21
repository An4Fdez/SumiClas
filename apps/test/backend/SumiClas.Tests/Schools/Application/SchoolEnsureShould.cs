namespace SumiClas.Tests.Schools.Application;

using AwesomeAssertions;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Schools.Domain.Services;
using SumiClas.Tests.Schools.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolEnsureShould : SchoolsUnitTestCase
{
    private readonly SchoolEnsure.Ensure _useCase;

    public SchoolEnsureShould()
    {
        this._useCase = new SchoolEnsure.Ensure(new SchoolFinder(this.Repository.Object));
    }

    [Test]
    public async Task complete_without_asking_other_modules_when_the_school_exists()
    {
        School school = SchoolMother.InLocality(StringMother.Random("school"), GuidMother.Random());
        this.ShouldFetchSchools(school);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolEnsureQueryMother.WithSchoolId(school.Id),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
        this.ShouldNotHaveAskedAnyModule();
    }

    [Test]
    public async Task throw_not_found_when_the_school_is_unknown()
    {
        this.ShouldFetchSchools(SchoolMother.InLocality(StringMother.Random("school"), GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolEnsureQueryMother.Random(),
            CancellationToken.None);

        await act.Should().ThrowAsync<SchoolExceptions.NotFound>();
        this.ShouldNotHaveAskedAnyModule();
    }
}
