namespace SumiClas.Tests.Enrollments.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Enrollments.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Shared.Domain;
using SumiClas.Users.Application;

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
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative(currentUserId, newClassId);
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldFindClassesOfNewSchoolYear(newYearId, newClassId);
        this.ShouldSearchEnrollments(
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Suggested(GuidMother.Random(), oldClassId));
        this.ShouldCaptureAddedEnrollments();

        EnrollmentSuggestedCreateResponse result = await this.Suggest(newClassId, oldClassId);

        result.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(3, 0));
        this.Added.Should().HaveCount(3);
        this.Added.Should().OnlyContain(enrollment =>
            enrollment.Status == EnrollmentStatus.Suggested && enrollment.ClassId == newClassId);
        this.Added.Select(enrollment => enrollment.Id).Distinct().Should().HaveCount(3);
    }

    [Test]
    public async Task throw_suggest_forbidden_when_the_current_user_is_not_a_representative()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldNotHaveClassRepresentative(currentUserId, newClassId);

        Func<Task> act = () => this.Suggest(newClassId, oldClassId);

        await act.Should().ThrowAsync<EnrollmentExceptions.SuggestForbidden>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_unauthenticated_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this.Suggest(
            GuidMother.Random(),
            GuidMother.Random());

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
    }

    [Test]
    public async Task throw_same_class_without_any_lookup_when_both_classes_are_the_same()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(classId, classId));


        await act.Should().ThrowAsync<EnrollmentExceptions.SameClass>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_required_when_the_class_id_is_empty()
    {
        Guid oldClassId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(Guid.Empty, oldClassId));


        await act.Should().ThrowAsync<EnrollmentExceptions.ClassRequired>();
    }

    [Test]
    public async Task throw_class_required_when_the_previous_class_id_is_empty()
    {
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, Guid.Empty));

        await act.Should().ThrowAsync<EnrollmentExceptions.ClassRequired>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_that_failure_when_the_new_school_year_is_archived()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(newYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this.Suggest(newClassId, oldClassId);

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_same_school_year_when_the_classes_belong_to_the_same_school_year()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid otherOldClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId, otherOldClassId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative(currentUserId, newClassId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, otherOldClassId));


        await act.Should().ThrowAsync<EnrollmentExceptions.SameSchoolYear>();
    }

    [Test]
    public async Task throw_different_school_when_the_classes_belong_to_different_schools()
    {
        this.ShouldCaptureAddedEnrollments();
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative(currentUserId, newClassId);
        this.ShouldFindSchoolYears(oldYearId, newYearId, inSameSchool: false);

        Func<Task> act = () => this.Suggest(newClassId, oldClassId);


        await act.Should().ThrowAsync<EnrollmentExceptions.DifferentSchool>();
        this.Added.Should().BeEmpty();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task throw_different_school_when_one_of_the_school_years_is_missing(bool newYearIsMissing)
    {
        this.ShouldCaptureAddedEnrollments();
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative(currentUserId, newClassId);
        this.ShouldFindOnlyOneSchoolYear(newYearIsMissing ? oldYearId : newYearId);

        Func<Task> act = () => this.Suggest(newClassId, oldClassId);

        await act.Should().ThrowAsync<EnrollmentExceptions.DifferentSchool>();
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
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative(currentUserId, newClassId);
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldFindClassesOfNewSchoolYear(newYearId, newClassId);
        this.ShouldSearchEnrollments(
            EnrollmentMother.Confirmed(alreadySuggested, oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Suggested(alreadySuggested, newClassId));

        EnrollmentSuggestedCreateResponse result = await this.Suggest(newClassId, oldClassId);

        result.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(1, 1));
    }

    [Test]
    public async Task skip_the_students_with_a_confirmed_enrollment_in_another_class_of_the_new_school_year()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid otherNewYearClassId = GuidMother.Random();
        Guid movedByTheGuardian = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative(currentUserId, newClassId);
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldFindClassesOfNewSchoolYear(newYearId, newClassId, otherNewYearClassId);
        this.ShouldSearchEnrollments(
            EnrollmentMother.Confirmed(movedByTheGuardian, oldClassId),
            EnrollmentMother.Confirmed(GuidMother.Random(), oldClassId),
            EnrollmentMother.Confirmed(movedByTheGuardian, otherNewYearClassId));

        EnrollmentSuggestedCreateResponse result = await this.Suggest(newClassId, oldClassId);

        result.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(1, 1));
    }

    [Test]
    public async Task return_zeroes_when_there_are_no_confirmed_students()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClasses(oldClassId, oldYearId, newClassId, newYearId);
        this.ShouldEnsureSchoolYearIsActive(newYearId);
        this.ShouldHaveClassRepresentative(currentUserId, newClassId);
        this.ShouldFindSchoolYears(oldYearId, newYearId);
        this.ShouldSearchNoEnrollments();

        EnrollmentSuggestedCreateResponse result = await this.Suggest(newClassId, oldClassId);

        result.Should().Be(EnrollmentSuggestedCreateResponseMother.Nothing());
    }

    private Task<EnrollmentSuggestedCreateResponse> Suggest(Guid newClassId, Guid oldClassId)
        => this._useCase.ExecuteAsync(EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, oldClassId));

    private void ShouldFindClasses(
        Guid oldClassId,
        Guid oldYearId,
        Guid newClassId,
        Guid newYearId,
        Guid? sameYearAsNewClassId = null)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<ClassResponse>>(), It.IsAny<CancellationToken>()))
            .Returns<IQuery<ClassResponse>, CancellationToken>((query, _) =>
                Task.FromResult(ClassOf(
                    ((ClassFinderById.Query)query).ClassId,
                    oldClassId,
                    newClassId,
                    sameYearAsNewClassId,
                    oldYearId,
                    newYearId)));

    private void ShouldHaveClassRepresentative(Guid userId, Guid classId)
        => this.ShouldAskIfClassRepresentative(userId, classId, true);

    private void ShouldNotHaveClassRepresentative(Guid userId, Guid classId)
        => this.ShouldAskIfClassRepresentative(userId, classId, false);

    private void ShouldAskIfClassRepresentative(Guid userId, Guid classId, bool isRepresentative)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserIsClassRepresentative.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isRepresentative);

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

    private void ShouldFindOnlyOneSchoolYear(Guid foundYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<SchoolYearResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new List<SchoolYearResponse>
            {
                SchoolYearResponseMother.Active(foundYearId, GuidMother.Random()),
            });

    private void ShouldFindClassesOfNewSchoolYear(Guid newYearId, params Guid[] classIds)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<ClassResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(classIds.Select(classId => ClassResponseMother.With(classId, newYearId)).ToList());

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
