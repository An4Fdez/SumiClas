namespace SumiClas.Tests.Enrollments.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.SchoolYears;
using SumiClas.Tests.Shared;

[TestFixture]
public class EnrollmentConfirmShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentConfirm.Confirm _useCase;
    private readonly EnrollmentConfirm.Handler _handler;

    public EnrollmentConfirmShould()
    {
        this._useCase = new EnrollmentConfirm.Confirm(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object);
        this._handler = new EnrollmentConfirm.Handler(this._useCase);
    }

    [Test]
    public async Task confirm_the_suggested_enrollment_with_the_clock_time()
    {
        Guid schoolYearId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        this.ShouldReturn(enrollment);
        this.ShouldResolveSchoolYearOf(enrollment, schoolYearId);
        this.AdvanceClock(TimeSpan.FromDays(1));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Confirmed);
        enrollment.ConfirmedAt.Should().Be(Now + TimeSpan.FromDays(1));
        enrollment.CreatedAt.Should().Be(Now);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task confirm_the_enrollment_when_the_student_only_has_other_suggested_enrollments_in_the_school_year()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        Enrollment otherSuggested = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        this.ShouldReturn(enrollment, otherSuggested);
        this.ShouldResolveSchoolYearOf(enrollment, schoolYearId, otherSuggested.ClassId);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Confirmed);
        otherSuggested.Status.Should().Be(EnrollmentStatus.Suggested);
    }

    [Test]
    public async Task confirm_the_enrollment_when_the_student_is_confirmed_only_in_a_class_of_another_school_year()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        this.ShouldReturn(enrollment, EnrollmentMother.Confirmed(studentId, GuidMother.Random()));
        this.ShouldResolveSchoolYearOf(enrollment, schoolYearId);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Confirmed);
    }

    [Test]
    public async Task confirm_the_enrollment_when_only_another_student_is_confirmed_in_the_school_year()
    {
        Guid schoolYearId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        this.ShouldReturn(enrollment, EnrollmentMother.Confirmed(GuidMother.Random(), enrollment.ClassId));
        this.ShouldResolveSchoolYearOf(enrollment, schoolYearId);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Confirmed);
    }

    [Test]
    public async Task return_already_exists_when_the_student_is_confirmed_in_another_class_of_the_school_year()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Enrollment enrollment = EnrollmentMother.Suggested(studentId, GuidMother.Random());
        Enrollment confirmedElsewhere = EnrollmentMother.Confirmed(studentId, GuidMother.Random());
        this.ShouldReturn(enrollment, confirmedElsewhere);
        this.ShouldResolveSchoolYearOf(enrollment, schoolYearId, confirmedElsewhere.ClassId);

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
        this.ShouldReturn(enrollment);
        this.ShouldResolveSchoolYearOf(enrollment, schoolYearId);
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
        this.ShouldReturn(enrollment, confirmedElsewhere);
        this.QueryBus.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

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
        this.ShouldReturn(enrollment);
        this.QueryBus.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
    }

    [Test]
    public async Task return_class_not_found_without_confirming_when_the_class_does_not_exist()
    {
        Enrollment enrollment = EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random());
        this.ShouldReturn(enrollment);
        this.QueryBus.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(enrollment.Id));

        result.Error.Should().Be(ClassErrors.NotFound);
        enrollment.Status.Should().Be(EnrollmentStatus.Suggested);
    }

    [Test]
    public async Task return_not_found_without_asking_other_modules_when_the_enrollment_does_not_exist()
    {
        this.ShouldReturn(EnrollmentMother.Suggested(GuidMother.Random(), GuidMother.Random()));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentConfirmCommandMother.WithEnrollmentId(GuidMother.Random()));

        result.Error.Should().Be(EnrollmentErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_command()
    {
        Result result = await this._handler.Handle(
            EnrollmentConfirmCommandMother.WithEnrollmentId(GuidMother.Random()),
            CancellationToken.None);

        result.Error.Should().Be(EnrollmentErrors.NotFound);
    }

    private void ShouldResolveSchoolYearOf(Enrollment enrollment, Guid schoolYearId, params Guid[] otherClassIds)
    {
        this.QueryBus.ShouldFindClassInSchoolYear(enrollment.ClassId, schoolYearId);
        this.QueryBus.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.QueryBus.ShouldFindClassesOfSchoolYear(schoolYearId, [enrollment.ClassId, .. otherClassIds]);
    }
}
