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
using SumiClas.Users.Application;

[TestFixture]
public class EnrollmentSuggestedCreateShould : EnrollmentsUnitTestCase
{
    private readonly EnrollmentSuggestedCreate.Create _useCase;
    private readonly EnrollmentSuggestedCreate.Handler _handler;

    public EnrollmentSuggestedCreateShould()
    {
        this._useCase = new EnrollmentSuggestedCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object,
            this.Clock.Object);
        this._handler = new EnrollmentSuggestedCreate.Handler(this._useCase);
    }

    [Test]
    public async Task create_one_suggested_enrollment_for_each_confirmed_student()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId);
        this.AddConfirmed(oldClassId);
        this.AddConfirmed(oldClassId);
        this.AddConfirmed(oldClassId);

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(3, 0));
        this.Added.Should().HaveCount(3);
        this.Added.Should().OnlyContain(enrollment =>
            enrollment.Status == EnrollmentStatus.Suggested && enrollment.ClassId == newClassId);
        this.Added.Select(enrollment => enrollment.Id).Distinct().Should().HaveCount(3);
    }

    [Test]
    public async Task only_copy_the_confirmed_enrollments_of_the_previous_class()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId);
        this.AddConfirmed(oldClassId);
        this.ShouldReturn(EnrollmentMother.Suggested(GuidMother.Random(), oldClassId));

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(1, 0));
    }

    [Test]
    public async Task return_suggest_forbidden_when_the_current_user_is_not_a_representative()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId, isRepresentative: false);
        this.AddConfirmed(oldClassId);

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
        this.ShouldResolveClasses(GuidMother.Random(), classId);

        Result<EnrollmentSuggestedCreateResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(classId, classId));

        result.Error.Should().Be(EnrollmentErrors.SameClass);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_when_the_class_id_is_empty()
    {
        Guid oldClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, GuidMother.Random());

        Result<EnrollmentSuggestedCreateResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(Guid.Empty, oldClassId));

        result.Error.Should().Be(EnrollmentErrors.ClassRequired);
    }

    [Test]
    public async Task return_that_failure_when_the_new_school_year_is_archived()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        Error archived = ErrorMother.With(code: "SchoolYear.Archived");
        this.ShouldResolveClasses(oldClassId, newClassId, newYearActive: Result.Failure(archived));

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Error.Should().Be(archived);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_same_school_year_when_the_classes_belong_to_the_same_school_year()
    {
        Guid newClassId = GuidMother.Random();
        Guid otherOldClassId = GuidMother.Random();
        this.ShouldResolveClasses(GuidMother.Random(), newClassId, otherOldClassId);

        Result<EnrollmentSuggestedCreateResponse> result = await this._useCase.ExecuteAsync(
            EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, otherOldClassId));

        result.Error.Should().Be(EnrollmentErrors.SameSchoolYear);
    }

    [Test]
    public async Task return_different_school_when_the_classes_belong_to_different_schools()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId, inSameSchool: false);
        this.AddConfirmed(oldClassId);

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Error.Should().Be(EnrollmentErrors.DifferentSchool);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task skip_the_students_already_in_the_new_class()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId);
        Guid alreadySuggested = this.AddConfirmed(oldClassId);
        this.AddConfirmed(oldClassId);
        this.ShouldReturn(EnrollmentMother.Suggested(alreadySuggested, newClassId));

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(1, 1));
    }

    [Test]
    public async Task skip_the_students_with_a_confirmed_enrollment_in_the_new_school_year()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId);
        Guid movedByTheGuardian = this.AddConfirmed(oldClassId);
        this.AddConfirmed(oldClassId);
        this.ShouldReturn(EnrollmentMother.Confirmed(movedByTheGuardian, newClassId));

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(1, 1));
    }

    [Test]
    public async Task create_nothing_the_second_time_when_it_runs_twice()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId);
        this.AddConfirmed(oldClassId);
        this.AddConfirmed(oldClassId);

        Result<EnrollmentSuggestedCreateResponse> first = await this.Suggest(newClassId, oldClassId);
        Result<EnrollmentSuggestedCreateResponse> second = await this.Suggest(newClassId, oldClassId);

        first.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(2, 0));
        second.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Create(0, 2));
        this.Added.Should().HaveCount(2);
    }

    [Test]
    public async Task return_zeroes_when_there_are_no_confirmed_students()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId);

        Result<EnrollmentSuggestedCreateResponse> result = await this.Suggest(newClassId, oldClassId);

        result.Value.Should().Be(EnrollmentSuggestedCreateResponseMother.Nothing());
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_command()
    {
        Guid oldClassId = GuidMother.Random();
        Guid newClassId = GuidMother.Random();
        this.ShouldResolveClasses(oldClassId, newClassId);
        this.AddConfirmed(oldClassId);

        Result<EnrollmentSuggestedCreateResponse> result = await this._handler.Handle(
            EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, oldClassId),
            CancellationToken.None);

        result.Value.Created.Should().Be(1);
    }

    private Guid AddConfirmed(Guid classId)
    {
        Guid studentId = GuidMother.Random();
        this.ShouldReturn(EnrollmentMother.Confirmed(studentId, classId));
        return studentId;
    }

    private Task<Result<EnrollmentSuggestedCreateResponse>> Suggest(Guid newClassId, Guid oldClassId)
        => this._useCase.ExecuteAsync(EnrollmentSuggestedCreateCommandMother.WithClasses(newClassId, oldClassId));

    private void ShouldResolveClasses(
        Guid oldClassId,
        Guid newClassId,
        Guid? otherOldClassId = null,
        bool isRepresentative = true,
        bool inSameSchool = true,
        Result? newYearActive = null)
    {
        Guid schoolId = GuidMother.Random();
        Guid previousSchoolId = inSameSchool ? schoolId : GuidMother.Random();
        Guid oldYearId = GuidMother.Random();
        Guid newYearId = GuidMother.Random();
        Guid otherClassId = otherOldClassId ?? GuidMother.Random();
        Result yearActive = newYearActive ?? Result.Success();

        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<Result<ClassResponse>>>(), It.IsAny<CancellationToken>()))
            .Returns<IQuery<Result<ClassResponse>>, CancellationToken>((query, _) =>
                Task.FromResult(Result<ClassResponse>.Success(ClassOf(
                    ((ClassFinderById.Query)query).ClassId,
                    oldClassId,
                    newClassId,
                    otherClassId,
                    oldYearId,
                    newYearId))));
        this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<IQuery<Result>>(query => query is SchoolYearEnsureIsActive.Query),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(yearActive);
        this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(isRepresentative);
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<SchoolYearResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new List<SchoolYearResponse>
            {
                SchoolYearResponseMother.Archived(oldYearId, previousSchoolId),
                SchoolYearResponseMother.Active(newYearId, schoolId),
            });
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<ClassResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassResponse> { ClassResponseMother.With(newClassId, newYearId) });
    }

    private static ClassResponse ClassOf(
        Guid classId,
        Guid oldClassId,
        Guid newClassId,
        Guid otherOldClassId,
        Guid oldYearId,
        Guid newYearId)
    {
        if (classId == newClassId)
        {
            return ClassResponseMother.With(newClassId, newYearId);
        }

        if (classId == otherOldClassId)
        {
            return ClassResponseMother.With(otherOldClassId, newYearId);
        }

        return ClassResponseMother.With(oldClassId, oldYearId);
    }
}
