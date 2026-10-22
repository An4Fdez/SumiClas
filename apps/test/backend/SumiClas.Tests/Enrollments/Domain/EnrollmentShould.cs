namespace SumiClas.Tests.Enrollments.Domain;

using AwesomeAssertions;
using SumiClas.Enrollments.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class EnrollmentShould : UnitTestCase
{
    [Test]
    public void create_a_confirmed_enrollment_with_the_clock_time_when_enrolled_directly()
    {
        Guid id = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();

        Enrollment enrollment = Enrollment.Create(id, studentId, classId, this.Clock.Object);

        enrollment.Id.Should().Be(id);
        enrollment.StudentId.Should().Be(studentId);
        enrollment.ClassId.Should().Be(classId);
        enrollment.Status.Should().Be(EnrollmentStatus.Confirmed);
        enrollment.CreatedAt.Should().Be(Now);
        enrollment.ConfirmedAt.Should().Be(Now);
    }

    [Test]
    public void create_a_suggested_enrollment_without_confirmation_time_when_suggested()
    {
        Guid id = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();

        Enrollment enrollment = Enrollment.CreateSuggested(id, studentId, classId, this.Clock.Object);

        enrollment.Id.Should().Be(id);
        enrollment.StudentId.Should().Be(studentId);
        enrollment.ClassId.Should().Be(classId);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
        enrollment.CreatedAt.Should().Be(Now);
        enrollment.ConfirmedAt.Should().BeNull();
    }

    [Test]
    public void throw_id_required_when_a_suggested_enrollment_has_an_empty_id()
    {
        Func<Enrollment> act = () => Enrollment.CreateSuggested(
            Guid.Empty,
            GuidMother.Random(),
            GuidMother.Random(),
            this.Clock.Object);

        act.Should().Throw<EnrollmentExceptions.IdRequired>();
    }

    [Test]
    public void throw_student_required_when_a_suggested_enrollment_has_an_empty_student()
    {
        Func<Enrollment> act = () => Enrollment.CreateSuggested(
            GuidMother.Random(),
            Guid.Empty,
            GuidMother.Random(),
            this.Clock.Object);

        act.Should().Throw<EnrollmentExceptions.StudentRequired>();
    }

    [Test]
    public void throw_class_required_when_a_suggested_enrollment_has_an_empty_class()
    {
        Func<Enrollment> act = () => Enrollment.CreateSuggested(
            GuidMother.Random(),
            GuidMother.Random(),
            Guid.Empty,
            this.Clock.Object);

        act.Should().Throw<EnrollmentExceptions.ClassRequired>();
    }

    [Test]
    public void confirm_a_suggested_enrollment_with_the_clock_time()
    {
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        this.AdvanceClock(TimeSpan.FromDays(1));

        enrollment.Confirm(this.Clock.Object);

        enrollment.Status.Should().Be(EnrollmentStatus.Confirmed);
        enrollment.ConfirmedAt.Should().Be(Now + TimeSpan.FromDays(1));
        enrollment.CreatedAt.Should().Be(Now);
    }

    [Test]
    public void throw_already_confirmed_and_keep_the_confirmation_time_when_confirmed_twice()
    {
        Enrollment enrollment = EnrollmentMother.Confirmed(GuidMother.Random(), GuidMother.Random());
        this.AdvanceClock(TimeSpan.FromDays(1));

        Action act = () => enrollment.Confirm(this.Clock.Object);

        act.Should().Throw<EnrollmentExceptions.AlreadyConfirmed>();
        enrollment.ConfirmedAt.Should().Be(Now);
    }
}
