namespace SumiClas.Tests.Enrollments.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class EnrollmentConfirmShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentConfirm.Confirm _useCase;

    public EnrollmentConfirmShould()
    {
        this._useCase = new EnrollmentConfirm.Confirm(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task confirm_the_suggested_enrollment_with_the_clock_time_ignoring_the_unrelated_enrollments()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid schoolYearId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        Enrollment otherSuggested = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        Enrollment confirmedElsewhere = EnrollmentMother.Confirmed(studentId, GuidMother.Random());
        Enrollment confirmedOfAnother = EnrollmentMother.Confirmed(GuidMother.Random(), enrollment.ClassId);
        this.ShouldFetchEnrollments(enrollment, otherSuggested, confirmedElsewhere, confirmedOfAnother);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(enrollment.StudentId, currentUserId);
        this.ShouldSearchEnrollments(enrollment, otherSuggested, confirmedElsewhere, confirmedOfAnother);
        this.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindClassesOfSchoolYear(schoolYearId, enrollment.ClassId, otherSuggested.ClassId);
        this.AdvanceClock(TimeSpan.FromDays(1));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Confirmed);
        otherSuggested.Status.Should().Be(EnrollmentStatus.Suggested);
        enrollment.ConfirmedAt.Should().Be(Now + TimeSpan.FromDays(1));
        enrollment.CreatedAt.Should().Be(Now);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_already_exists_when_the_student_is_confirmed_in_another_class_of_the_school_year()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        Enrollment confirmedElsewhere = EnrollmentMother.Confirmed(studentId, GuidMother.Random());
        this.ShouldFetchEnrollments(enrollment, confirmedElsewhere);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(enrollment.StudentId, currentUserId);
        this.ShouldSearchEnrollments(enrollment, confirmedElsewhere);
        this.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindClassesOfSchoolYear(schoolYearId, enrollment.ClassId, confirmedElsewhere.ClassId);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(EnrollmentErrors.AlreadyExists);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
        enrollment.ConfirmedAt.Should().BeNull();
    }

    [Test]
    public async Task return_already_confirmed_and_keep_the_confirmation_time_when_the_enrollment_is_already_confirmed()
    {
        Guid schoolYearId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Confirmed(GuidMother.Random(), GuidMother.Random());
        this.ShouldFetchEnrollments(enrollment);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(enrollment.StudentId, currentUserId);
        this.ShouldSearchEnrollments(enrollment);
        this.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindClassesOfSchoolYear(schoolYearId, enrollment.ClassId);
        this.AdvanceClock(TimeSpan.FromDays(1));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(EnrollmentErrors.AlreadyConfirmed);
        enrollment.ConfirmedAt.Should().Be(Now);
    }

    [Test]
    public async Task return_the_school_year_failure_before_checking_the_existing_enrollments()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        Enrollment confirmedElsewhere = EnrollmentMother.Confirmed(studentId, GuidMother.Random());
        this.ShouldFetchEnrollments(enrollment, confirmedElsewhere);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(enrollment.StudentId, currentUserId);
        this.ShouldSearchEnrollments(enrollment, confirmedElsewhere);
        this.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);
        this.ShouldFindClassesOfSchoolYear(schoolYearId, enrollment.ClassId, confirmedElsewhere.ClassId);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
    }

    [Test]
    public async Task return_school_year_not_found_without_confirming_when_the_school_year_does_not_exist()
    {
        Guid schoolYearId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        this.ShouldFetchEnrollments(enrollment);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(enrollment.StudentId, currentUserId);
        this.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
    }

    [Test]
    public async Task throw_class_not_found_without_confirming_when_the_class_does_not_exist()
    {
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        this.ShouldFetchEnrollments(enrollment);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(enrollment.StudentId, currentUserId);
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
    }

    [Test]
    public async Task return_not_found_without_asking_other_modules_when_the_enrollment_does_not_exist()
    {
        this.ShouldFetchEnrollments(EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random()));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(GuidMother.Random()));

        result.Error.Should().Be(EnrollmentErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_guardian_access_forbidden_without_confirming_when_the_user_is_not_the_guardian()
    {
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        Guid currentUserId = GuidMother.Random();
        this.ShouldFetchEnrollments(enrollment);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFailEnsuringStudentGuardian(
            enrollment.StudentId,
            currentUserId,
            StudentErrors.GuardianAccessForbidden);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(StudentErrors.GuardianAccessForbidden);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
    }

    [Test]
    public async Task return_unauthenticated_without_confirming_when_there_is_no_current_user()
    {
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        this.ShouldFetchEnrollments(enrollment);
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
    }
}
