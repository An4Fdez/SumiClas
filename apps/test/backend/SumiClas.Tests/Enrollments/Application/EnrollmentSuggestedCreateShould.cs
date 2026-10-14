namespace SumiClas.Tests.Enrollments.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Shared.Domain;

[TestFixture]
public class EnrollmentSuggestedCreateShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentSuggestedCreate.Create _useCase;

    public EnrollmentSuggestedCreateShould()
    {
        this._useCase = new EnrollmentSuggestedCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task create_one_suggested_enrollment_for_each_confirmed_student_of_the_previous_class_only()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative();
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldFindClassesOfNewSchoolYear(newClassId, newYearId);
        this.ShouldSearchEnrollments(
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Suggested(GuidMother.Random(), oldClassId));
        this.ShouldCaptureAddedEnrollments();

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(3, 0));
        this.Added.Should().HaveCount(3);
        this.Added.Should().OnlyContain(enrollment =>
            enrollment.Status == EnrollmentStatus.Suggested && enrollment.ClassId == newClassId);
        this.Added.Select(enrollment => enrollment.Id).Distinct().Should().HaveCount(3);
    }

    [Test]
    public async Task return_suggest_forbidden_when_the_current_user_is_not_a_representative()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldNotHaveClassRepresentative();

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EnrollmentErrors.SuggestForbidden);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_unauthenticated_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(
            GuidMother.Random(),
            GuidMother.Random());

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
    }

    [Test]
    public async Task return_same_class_without_any_lookup_when_both_classes_are_the_same()
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result<EnrollmentSuggestedCreateResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(classId, classId));

        result.Error.Should().Be(EnrollmentErrors.SameClass);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_when_the_class_id_is_empty()
    {
        Guid oldClassId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result<EnrollmentSuggestedCreateResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(Guid.Empty, oldClassId));

        result.Error.Should().Be(EnrollmentErrors.ClassRequired);
    }

    [Test]
    public async Task return_that_failure_when_the_new_school_year_is_archived()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Error archived = ErrorMother.With(code: "SchoolYear.Archived");
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(newYearId, archived);

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Error.Should().Be(archived);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_same_school_year_when_the_classes_belong_to_the_same_school_year()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid otherOldClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId, otherOldClassId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative();

        Result<EnrollmentSuggestedCreateResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, otherOldClassId));

        result.Error.Should().Be(EnrollmentErrors.SameSchoolYear);
    }

    [Test]
    public async Task return_different_school_when_the_classes_belong_to_different_schools()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative();
        this.ShouldFindSchoolYears(oldYearId, newYearId, inSameSchool: false);

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Error.Should().Be(EnrollmentErrors.DifferentSchool);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task skip_the_students_already_in_the_new_class()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid alreadySuggested = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative();
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldFindClassesOfNewSchoolYear(newClassId, newYearId);
        this.ShouldSearchEnrollments(
            EnrollmentMother.Confirmed(alreadySuggested, oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Suggested(alreadySuggested, newClassId));

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(1, 1));
    }

    [Test]
    public async Task skip_the_students_with_a_confirmed_enrollment_in_the_new_school_year()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid movedByTheGuardian = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative();
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldFindClassesOfNewSchoolYear(newClassId, newYearId);
        this.ShouldSearchEnrollments(
            EnrollmentMother.Confirmed(movedByTheGuardian, oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Confirmed(movedByTheGuardian, newClassId));

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(1, 1));
    }

    [Test]
    public async Task return_zeroes_when_there_are_no_confirmed_students()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative();
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldSearchNoEnrollments();

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Nothing());
    }

    private Task<Result<EnrollmentSuggestedCreateResponse>> Suggest(Guid newClassId, Guid oldClassId)
        => this._useCase.ExecuteAsync(EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, oldClassId));

    private void ShouldFindClasses(
        Guid oldClassId,
        Guid oldYearId,
        Guid newClassId,
        Guid newYearId,
        Guid? sameYearAsNewClassId = null)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .Returns<IQuery<Result<ClassResponse>>, CancellationToken>((query, _) =>
                Task.FromResult(Result<ClassResponse>.Success(ClassOf(
                    ((ClassFinderById.Query)query).ClassId,
                    oldClassId,
                    newClassId,
                    sameYearAsNewClassId,
                    oldYearId,
                    newYearId))));

    private void ShouldHaveClassRepresentative()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

    private void ShouldNotHaveClassRepresentative()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

    private void ShouldFindSchoolYears(Guid oldYearId, Guid newYearId, bool inSameSchool = true)
    {
        Guid schoolId = GuidMother.Random();
        Guid previousSchoolId = inSameSchool ? schoolId : GuidMother.Random();
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<SchoolYearResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new List<SchoolYearResponse>
            {
                SchoolYearResponseMother.Archived(oldYearId, previousSchoolId),
                SchoolYearResponseMother.Active(newYearId, schoolId),
            });
    }

    private void ShouldFindClassesOfNewSchoolYear(Guid newClassId, Guid newYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<ClassResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassResponse> { ClassResponseMother.With(newClassId, newYearId) });

    private static ClassResponse ClassOf(
        Guid classId,
        Guid oldClassId,
        Guid newClassId,
        Guid? sameYearAsNewClassId,
        Guid oldYearId,
        Guid newYearId)
    {
        if (classId == newClassId)
        {
            return ClassResponseMother.With(newClassId, newYearId);
        }

        if (classId == sameYearAsNewClassId)
        {
            return ClassResponseMother.With(classId, newYearId);
        }

        return ClassResponseMother.With(oldClassId, oldYearId);
    }
}
