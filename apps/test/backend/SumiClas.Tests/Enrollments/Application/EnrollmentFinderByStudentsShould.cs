namespace SumiClas.Tests.Enrollments.Application;

using AwesomeAssertions;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class EnrollmentFinderByStudentsShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentFinderByStudents.Finder _useCase;

    public EnrollmentFinderByStudentsShould()
    {
        this._useCase = new EnrollmentFinderByStudents.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_the_enrollments_of_the_requested_students_in_any_class_and_status()
    {
        Guid firstStudentId = GuidMother.Random();
        Guid secondStudentId = GuidMother.Random();
        Enrollment suggested = EnrollmentMother.Suggested(firstStudentId, GuidMother.Random());
        Enrollment confirmed = EnrollmentMother.Confirmed(firstStudentId, GuidMother.Random());
        Enrollment ofSecondStudent = EnrollmentMother.Confirmed(secondStudentId, GuidMother.Random());
        this.ShouldReturn(
            suggested,
            confirmed,
            ofSecondStudent,
            EnrollmentMother.Confirmed(GuidMother.Random(), GuidMother.Random()));

        IReadOnlyCollection<EnrollmentResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentFinderByStudentsQueryMother.WithStudentIds(firstStudentId, secondStudentId));

        result.Should().BeEquivalentTo(
        [
            EnrollmentResponseMother.Suggested(suggested),
            EnrollmentResponseMother.Confirmed(confirmed),
            EnrollmentResponseMother.Confirmed(ofSecondStudent),
        ]);
    }

    [Test]
    public async Task return_empty_when_there_are_no_student_ids_even_if_enrollments_exist()
    {
        this.ShouldReturn(EnrollmentMother.Confirmed(GuidMother.Random(), GuidMother.Random()));

        IReadOnlyCollection<EnrollmentResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentFinderByStudentsQueryMother.WithoutStudents());

        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_empty_when_the_student_ids_are_unknown()
    {
        this.ShouldReturn(EnrollmentMother.Confirmed(GuidMother.Random(), GuidMother.Random()));

        IReadOnlyCollection<EnrollmentResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentFinderByStudentsQueryMother.WithStudentIds(GuidMother.Random()));

        result.Should().BeEmpty();
    }
}
