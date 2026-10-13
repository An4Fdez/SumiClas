namespace SumiClas.Tests.Enrollments.Application;

using AwesomeAssertions;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class EnrollmentFinderByClassShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentFinderByClass.Finder _useCase;

    public EnrollmentFinderByClassShould()
    {
        this._useCase = new EnrollmentFinderByClass.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_the_confirmed_and_suggested_enrollments_of_the_class_only()
    {
        Guid classId = GuidMother.Random();
        Enrollment confirmed = EnrollmentMother.Confirmed(GuidMother.Random(), classId);
        Enrollment suggested = EnrollmentMother.Suggested(GuidMother.Random(), classId);
        this.ShouldReturn(
            confirmed,
            suggested,
            EnrollmentMother.Confirmed(GuidMother.Random(), GuidMother.Random()));

        IReadOnlyCollection<EnrollmentResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentFinderByClassQueryMother.WithClassId(classId));

        result.Should().BeEquivalentTo(
        [
            EnrollmentResponseMother.Confirmed(confirmed),
            EnrollmentResponseMother.Suggested(suggested),
        ]);
    }

    [Test]
    public async Task return_empty_when_the_class_has_no_enrollments()
    {
        this.ShouldReturn(EnrollmentMother.Confirmed(GuidMother.Random(), GuidMother.Random()));

        IReadOnlyCollection<EnrollmentResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        result.Should().BeEmpty();
    }
}
