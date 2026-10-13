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
using SumiClas.Tests.Classes;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.SchoolYears;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students;

[TestFixture]
public class EnrollmentCreateShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentCreate.Create _useCase;

    public EnrollmentCreateShould()
    {
        this._useCase = new EnrollmentCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task add_a_confirmed_enrollment_with_the_command_ids_and_the_clock_time()
    {
        Guid enrollmentId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveEnrollableStudentAndClass(studentId, classId, schoolYearId);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(enrollmentId, studentId, classId));

        result.IsSuccess.Should().BeTrue();
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
    public async Task add_the_enrollment_when_the_student_only_has_a_suggested_enrollment_in_the_school_year()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveEnrollableStudentAndClass(studentId, classId, schoolYearId);
        this.ShouldReturn(EnrollmentMother.Suggested(studentId, classId));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        result.IsSuccess.Should().BeTrue();
        this.Added.Should().ContainSingle();
    }

    [Test]
    public async Task add_the_enrollment_when_the_student_is_confirmed_only_in_a_class_of_another_school_year()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveEnrollableStudentAndClass(studentId, classId, schoolYearId);
        this.ShouldReturn(EnrollmentMother.Confirmed(studentId, GuidMother.Random()));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        result.IsSuccess.Should().BeTrue();
        this.Added.Should().ContainSingle();
    }

    [Test]
    public async Task add_the_enrollment_when_only_another_student_is_confirmed_in_the_school_year()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveEnrollableStudentAndClass(studentId, classId, schoolYearId);
        this.ShouldReturn(EnrollmentMother.Confirmed(GuidMother.Random(), classId));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        result.IsSuccess.Should().BeTrue();
        this.Added.Should().ContainSingle();
    }

    [Test]
    public async Task return_already_exists_when_the_student_is_confirmed_in_another_class_of_the_school_year()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid otherClassId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveEnrollableStudentAndClass(studentId, classId, schoolYearId, otherClassId);
        this.ShouldReturn(EnrollmentMother.Confirmed(studentId, otherClassId));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        result.Error.Should().Be(EnrollmentErrors.AlreadyExists);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task create_nothing_the_second_time_when_it_runs_twice()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveEnrollableStudentAndClass(studentId, classId, schoolYearId);

        Result first = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));
        Result second = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        first.IsSuccess.Should().BeTrue();
        second.Error.Should().Be(EnrollmentErrors.AlreadyExists);
        this.Added.Should().ContainSingle();
    }

    [Test]
    public async Task return_the_school_year_failure_before_checking_the_existing_enrollments()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.QueryBus.ShouldFindStudent(studentId);
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldFindClassesOfSchoolYear(schoolYearId, classId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);
        this.ShouldReturn(EnrollmentMother.Confirmed(studentId, classId));

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        Guid studentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.QueryBus.ShouldFindStudent(studentId);
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, classId));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_class_not_found_without_adding_when_the_class_does_not_exist()
    {
        Guid studentId = GuidMother.Random();
        this.QueryBus.ShouldFindStudent(studentId);
        this.QueryBus.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, GuidMother.Random()));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_student_not_found_without_asking_for_the_class_when_the_student_does_not_exist()
    {
        Guid studentId = GuidMother.Random();
        this.QueryBus.ShouldNotFindStudent(studentId);

        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), studentId, GuidMother.Random()));

        result.Error.Should().Be(StudentErrors.NotFound);
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_enrollment_id_is_empty()
    {
        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(Guid.Empty, GuidMother.Random(), GuidMother.Random()));

        result.Error.Should().Be(EnrollmentErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_student_required_without_asking_or_adding_when_the_student_id_is_empty()
    {
        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), Guid.Empty, GuidMother.Random()));

        result.Error.Should().Be(EnrollmentErrors.StudentRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_class_id_is_empty()
    {
        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), GuidMother.Random(), Guid.Empty));

        result.Error.Should().Be(EnrollmentErrors.ClassRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_when_every_id_is_empty()
    {
        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(Guid.Empty, Guid.Empty, Guid.Empty));

        result.Error.Should().Be(EnrollmentErrors.IdRequired);
    }

    [Test]
    public async Task return_student_required_when_the_student_and_the_class_ids_are_empty()
    {
        Result result = await this._useCase.ExecuteAsync(
            EnrollmentCreateCommandMother.With(GuidMother.Random(), Guid.Empty, Guid.Empty));

        result.Error.Should().Be(EnrollmentErrors.StudentRequired);
    }

    private void ShouldResolveEnrollableStudentAndClass(
        Guid studentId,
        Guid classId,
        Guid schoolYearId,
        params Guid[] otherClassIds)
    {
        this.QueryBus.ShouldFindStudent(studentId);
        this.QueryBus.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.QueryBus.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.QueryBus.ShouldFindClassesOfSchoolYear(schoolYearId, [classId, .. otherClassIds]);
    }
}
