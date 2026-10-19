namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class CalendarEventCreateShould : CalendarEventsUnitTestCase
{
    private readonly CalendarEventCreate.Create _useCase;

    public CalendarEventCreateShould()
    {
        this._useCase = new CalendarEventCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_school_event_with_its_data_trimmed_and_the_author_and_clock_time()
    {
        Guid eventId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string title = StringMother.Random("event");
        string location = StringMother.Random("place");
        string cost = StringMother.Random("cost");
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldCaptureAddedCalendarEvents();

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.With(
            eventId,
            "School",
            classId,
            "Trip",
            "  " + title + " ",
            new DateOnly(2027, 2, 28),
            new TimeOnly(9, 30),
            "  " + location + " ",
            " " + cost + "  "));

        result.IsSuccess.Should().BeTrue();
        CalendarEvent added = this.Added.Single();
        added.Id.Should().Be(eventId);
        added.Scope.Should().Be(EventScope.School);
        added.ClassId.Should().BeNull();
        added.SchoolId.Should().Be(schoolId);
        added.StudentId.Should().BeNull();
        added.Type.Should().Be(EventType.Trip);
        added.Title.Should().Be(title);
        added.EventDate.Year.Should().Be(2027);
        added.EventDate.Month.Should().Be(2);
        added.EventDate.Day.Should().Be(28);
        added.Time.Should().Be(new TimeOnly(9, 30));
        added.Location.Should().Be(location);
        added.Cost.Should().Be(cost);
        added.AuthorId.Should().Be(authorId);
        added.CreatedAt.Should().Be(Now);
    }

    [Test]
    public async Task add_the_class_event_with_its_class_when_the_class_exists_and_the_school_year_is_active()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedCalendarEvents();

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "Class", classId: classId));

        result.IsSuccess.Should().BeTrue();
        CalendarEvent added = this.Added.Single();
        added.Scope.Should().Be(EventScope.Class);
        added.ClassId.Should().Be(classId);
    }

    [Test]
    public async Task add_the_event_with_the_time_left_empty_when_no_time_is_given()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedCalendarEvents();

        await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(classId: classId));

        this.Added.Single().Time.Should().BeNull();
    }

    [TestCase(null)]
    [TestCase("   ")]
    public async Task store_no_location_and_no_cost_when_they_are_null_or_blank(string? text)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedCalendarEvents();

        await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.With(
            GuidMother.Random(),
            "Class",
            classId,
            "Trip",
            StringMother.Random("event"),
            new DateOnly(2027, 2, 28),
            null,
            text,
            text));

        CalendarEvent added = this.Added.Single();
        added.Location.Should().BeNull();
        added.Cost.Should().BeNull();
    }

    [TestCase("SCHOOL", EventScope.School)]
    [TestCase("class", EventScope.Class)]
    public async Task accept_the_scope_whatever_its_casing(string scope, EventScope expected)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, GuidMother.Random());
        this.ShouldCaptureAddedCalendarEvents();

        await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(scope: scope, classId: classId));

        this.Added.Single().Scope.Should().Be(expected);
    }

    [TestCase("trip", EventType.Trip)]
    [TestCase("MEETING", EventType.Meeting)]
    public async Task accept_the_type_whatever_its_casing(string type, EventType expected)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedCalendarEvents();

        await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(classId: classId, type: type));

        this.Added.Single().Type.Should().Be(expected);
    }

    [Test]
    public async Task return_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: StringMother.Random("scope")));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("")]
    [TestCase("7")]
    public async Task return_invalid_scope_without_asking_or_adding_when_the_scope_is_not_a_known_one(string scope)
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(scope: scope));

        result.Error.Should().Be(CalendarEventErrors.InvalidScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [TestCase("")]
    [TestCase("9")]
    public async Task return_invalid_type_without_asking_or_adding_when_the_type_is_not_a_known_one(string type)
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(type: type));

        result.Error.Should().Be(CalendarEventErrors.InvalidType);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_invalid_scope_when_both_the_scope_and_the_type_are_invalid()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(
            scope: StringMother.Random("scope"),
            type: StringMother.Random("type")));

        result.Error.Should().Be(CalendarEventErrors.InvalidScope);
    }

    [Test]
    public async Task return_invalid_scope_when_the_scope_is_invalid_and_the_id_is_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(id: Guid.Empty, scope: StringMother.Random("scope")));

        result.Error.Should().Be(CalendarEventErrors.InvalidScope);
    }

    [Test]
    public async Task return_invalid_type_when_the_type_is_invalid_and_the_id_is_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(id: Guid.Empty, type: StringMother.Random("type")));

        result.Error.Should().Be(CalendarEventErrors.InvalidType);
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_id_is_empty()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(id: Guid.Empty));

        result.Error.Should().Be(CalendarEventErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_use_create_birthday_without_asking_or_adding_when_the_type_is_birthday()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(type: "Birthday"));

        result.Error.Should().Be(CalendarEventErrors.UseCreateBirthdayForBirthdays);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_class_scope_has_no_class()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.ClassScopeWithClassId(null));

        result.Error.Should().Be(CalendarEventErrors.ClassRequiredForClassScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_class_scope_has_an_empty_class()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.ClassScopeWithClassId(Guid.Empty));

        result.Error.Should().Be(CalendarEventErrors.ClassRequiredForClassScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_title_required_without_asking_or_adding_when_the_title_is_blank()
    {
        string title = "   ";
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(title: title));

        result.Error.Should().Be(CalendarEventErrors.TitleRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_author_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid());

        result.Error.Should().Be(CalendarEventErrors.AuthorRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_when_the_id_is_empty_and_the_type_is_birthday()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(id: Guid.Empty, type: "Birthday"));

        result.Error.Should().Be(CalendarEventErrors.IdRequired);
    }

    [Test]
    public async Task return_use_create_birthday_when_the_type_is_birthday_and_the_class_is_missing()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.ClassScopeWithClassId(null, type: "Birthday"));

        result.Error.Should().Be(CalendarEventErrors.UseCreateBirthdayForBirthdays);
    }

    [Test]
    public async Task return_class_required_when_the_class_is_missing_and_the_title_is_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.ClassScopeWithClassId(null, title: "  "));

        result.Error.Should().Be(CalendarEventErrors.ClassRequiredForClassScope);
    }

    [Test]
    public async Task return_title_required_when_the_title_is_blank_and_the_current_user_id_is_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(CalendarEventCreateCommandMother.Valid(title: "  "));

        result.Error.Should().Be(CalendarEventErrors.TitleRequired);
    }

    [Test]
    public async Task return_user_not_found_without_adding_or_asking_for_the_class_when_the_author_does_not_exist()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindUser(authorId);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "Class", classId: GuidMother.Random()));

        result.Error.Should().Be(UserErrors.NotFound);
        this.Added.Should().BeEmpty();
        this.ShouldHaveAskedOnlyForTheAuthor();
    }

    [Test]
    public async Task return_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindClass();

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "Class", classId: GuidMother.Random()));

        result.Error.Should().Be(ClassErrors.NotFound);
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task return_school_year_archived_without_adding_when_the_school_year_of_the_class_is_archived()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "Class", classId: classId));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_year_not_found_without_adding_when_the_school_year_of_the_class_does_not_exist()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "Class", classId: classId));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.Added.Should().BeEmpty();
    }

    private void ShouldHaveAskedOnlyForTheAuthor()
    {
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_representative_forbidden_without_adding_when_the_author_is_not_a_delegate()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassRepresentative(authorId, classId);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.ClassScopeWithClassId(classId));

        result.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_school_scope_has_no_class()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.With(
                GuidMother.Random(),
                "School",
                null,
                "Trip",
                StringMother.Random("event"),
                new DateOnly(2027, 2, 28),
                null,
                null,
                null));

        result.Error.Should().Be(CalendarEventErrors.ClassRequiredForSchoolScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_school_scope_has_an_empty_class()
    {
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "School", classId: Guid.Empty));

        result.Error.Should().Be(CalendarEventErrors.ClassRequiredForSchoolScope);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_school_year_not_found_without_adding_when_the_school_of_the_class_cannot_be_found()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedCalendarEvents();
        this.ShouldFailFindingSchoolOfSchoolYear(schoolYearId, SchoolYearErrors.NotFound);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "School", classId: classId));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_representative_forbidden_without_adding_the_school_event_when_the_author_is_not_a_delegate()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassRepresentative(authorId, classId);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "School", classId: classId));

        result.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_year_archived_without_adding_the_school_event_when_the_class_is_archived()
    {
        this.ShouldCaptureAddedCalendarEvents();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassRepresentative(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);

        Result result = await this._useCase.ExecuteAsync(
            CalendarEventCreateCommandMother.Valid(scope: "School", classId: classId));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        this.Added.Should().BeEmpty();
    }
}
