namespace SumiClas.Tests.Enrollments.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class EnrollmentCreateShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentCreate.Create _useCase;

    public EnrollmentCreateShould()
    {
        this._useCase = new EnrollmentCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_a_confirmed_enrollment_with_the_command_ids_and_the_clock_time_ignoring_other_enrollments()
    {
        Guid enrollmentId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(studentId, currentUserId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindClassesOfSchoolYear(schoolYearId, classId);
        this.ShouldSearchEnrollments(
            EnrollmentMother.Suggested(studentId, classId),
            EnrollmentMother.Confirmed(studentId, GuidMother.Random()),
            EnrollmentMother.Confirmed(GuidMother.Random(), classId));
        this.ShouldCaptureAddedEnrollments();

        await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(enrollmentId, studentId, classId));

        Enrollment added = this.Added.Single();
        added.Id.Should().Be(enrollmentId);
        added.StudentId.Should().Be(studentId);
        added.ClassId.Should().Be(classId);
        added.Status.Should().Be(EnrollmentStatus.Confirmed);
        added.CreatedAt.Should().Be(Now);
        added.ConfirmedAt.Should().Be(Now);
        added.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task throw_already_exists_when_the_student_is_confirmed_in_another_class_of_the_school_year()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(studentId, currentUserId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindClassesOfSchoolYear(schoolYearId, classId, otherClassId);
        this.ShouldSearchEnrollments(EnrollmentMother.Confirmed(studentId, otherClassId));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));


        await act.Should().ThrowAsync<EnrollmentExceptions.AlreadyExists>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task create_nothing_the_second_time_when_it_runs_twice()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(studentId, currentUserId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindClassesOfSchoolYear(schoolYearId, classId);
        this.ShouldSearchNoEnrollments();
        this.ShouldCaptureAddedEnrollments();

        await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        await act.Should().ThrowAsync<EnrollmentExceptions.AlreadyExists>();
        this.Added.Should().ContainSingle();
    }

    [Test]
    public async Task throw_the_school_year_failure_before_checking_the_existing_enrollments()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(studentId, currentUserId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldFindClassesOfSchoolYear(schoolYearId, classId, otherClassId);
        this.ShouldSearchEnrollments(EnrollmentMother.Confirmed(studentId, otherClassId));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(studentId, currentUserId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_not_found_without_adding_when_the_class_does_not_exist()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid studentId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldEnsureStudentGuardian(studentId, currentUserId);
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_student_not_found_without_asking_for_the_class_when_the_student_does_not_exist()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid studentId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFailEnsuringStudentGuardian(studentId, currentUserId, new StudentExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, GuidMother.Random()));

        await act.Should().ThrowAsync<StudentExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_id_required_without_asking_or_adding_when_the_enrollment_id_is_empty()
    {
        this.ShouldCaptureAddedEnrollments();
        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(Guid.Empty, GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<EnrollmentExceptions.IdRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_student_required_without_asking_or_adding_when_the_student_id_is_empty()
    {
        this.ShouldCaptureAddedEnrollments();
        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), Guid.Empty, GuidMother.Random()));

        await act.Should().ThrowAsync<EnrollmentExceptions.StudentRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_required_without_asking_or_adding_when_the_class_id_is_empty()
    {
        this.ShouldCaptureAddedEnrollments();
        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), GuidMother.Random(), Guid.Empty));

        await act.Should().ThrowAsync<EnrollmentExceptions.ClassRequired>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_id_required_when_every_id_is_empty()
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(Guid.Empty, Guid.Empty, Guid.Empty));

        await act.Should().ThrowAsync<EnrollmentExceptions.IdRequired>();
    }

    [Test]
    public async Task throw_student_required_when_the_student_and_the_class_ids_are_empty()
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), Guid.Empty, Guid.Empty));

        await act.Should().ThrowAsync<EnrollmentExceptions.StudentRequired>();
    }

    [Test]
    public async Task throw_guardian_access_forbidden_without_adding_when_the_user_is_not_a_guardian_of_the_student()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid studentId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFailEnsuringStudentGuardian(
            studentId,
            currentUserId,
            new StudentExceptions.GuardianAccessForbidden());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, GuidMother.Random()));

        await act.Should().ThrowAsync<StudentExceptions.GuardianAccessForbidden>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedEnrollments();
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_unauthenticated_before_validating_the_ids_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedEnrollments();
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(Guid.Empty, GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Added.Should().BeEmpty();
    }
}
