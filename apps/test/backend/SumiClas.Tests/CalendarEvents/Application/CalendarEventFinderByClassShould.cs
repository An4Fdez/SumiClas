namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.CalendarEvents.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using EnrollmentResponseMother = SumiClas.Tests.Enrollments.Application.EnrollmentResponseMother;

[TestFixture]
public class CalendarEventFinderByClassShould : CalendarEventsUnitTestCase
{
    private static readonly DateOnly TODAY = DateOnly.FromDateTime(Now.UtcDateTime);

    private readonly CalendarEventFinderByClass.Finder _useCase;

    public CalendarEventFinderByClassShould()
    {
        this._useCase = new CalendarEventFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_school_events_class_events_and_birthdays_of_the_confirmed_students_only()
    {
        Guid classId = GuidMother.Random();
        Guid enrolledStudentId = GuidMother.Random();
        string schoolTitle = StringMother.Random("school");
        string classTitle = StringMother.Random("class");
        string birthdayTitle = StringMother.Random("birthday");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId, EnrollmentResponseMother.ConfirmedFor(enrolledStudentId, classId));
        this.ShouldFindMembers();
        this.ShouldSearchCalendarEvents(
            CalendarEventMother.SchoolWide(schoolId, schoolTitle, TODAY, GuidMother.Random()),
            CalendarEventMother.ForClass(classId, classTitle, TODAY, GuidMother.Random()),
            CalendarEventMother.ForClass(
                GuidMother.Random(),
                StringMother.Random("other"),
                TODAY,
                GuidMother.Random()),
            CalendarEventMother.Birthday(enrolledStudentId, birthdayTitle, 5, 17, GuidMother.Random()),
            CalendarEventMother.Birthday(
                GuidMother.Random(),
                StringMother.Random("stranger"),
                5,
                17,
                GuidMother.Random()));

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(item => item.Title).Should().BeEquivalentTo(schoolTitle, classTitle, birthdayTitle);
        this.ShouldHaveSearched(3);
    }

    [Test]
    public async Task not_return_the_birthday_of_a_student_whose_enrollment_is_only_suggested()
    {
        Guid classId = GuidMother.Random();
        Guid suggestedStudentId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId, EnrollmentResponseMother.SuggestedFor(suggestedStudentId, classId));
        this.ShouldFindMembers();
        this.ShouldSearchCalendarEvents(CalendarEventMother.Birthday(
            suggestedStudentId,
            StringMother.Random("birthday"),
            5,
            17,
            GuidMother.Random()));

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task not_return_the_school_events_of_other_schools()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        CalendarEvent mine = CalendarEventMother.SchoolWide(
            schoolId,
            StringMother.Random("mine"),
            TODAY,
            GuidMother.Random());
        CalendarEvent otherSchool = CalendarEventMother.SchoolWide(
            GuidMother.Random(),
            StringMother.Random("other"),
            TODAY,
            GuidMother.Random());
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindMembers();
        this.ShouldSearchCalendarEvents(mine, otherSchool);

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.Value.Select(item => item.Id).Should().Equal(mine.Id);
    }

    [Test]
    public async Task return_the_school_error_without_searching_when_the_school_of_the_class_cannot_be_found()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailFindingSchoolOfSchoolYear(schoolYearId, SchoolYearErrors.NotFound);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        this.ShouldHaveSearched(0);
    }

    [Test]
    public async Task return_only_the_school_and_class_events_without_searching_birthdays_when_nobody_is_enrolled()
    {
        Guid classId = GuidMother.Random();
        string schoolTitle = StringMother.Random("school");
        string classTitle = StringMother.Random("class");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindMembers();
        this.ShouldSearchCalendarEvents(
            CalendarEventMother.SchoolWide(schoolId, schoolTitle, TODAY, GuidMother.Random()),
            CalendarEventMother.ForClass(classId, classTitle, TODAY, GuidMother.Random()),
            CalendarEventMother.Birthday(
                GuidMother.Random(),
                StringMother.Random("stranger"),
                5,
                17,
                GuidMother.Random()));

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.Value.Select(item => item.Title).Should().BeEquivalentTo(schoolTitle, classTitle);
        this.ShouldHaveSearched(2);
    }

    [Test]
    public async Task return_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.Error.Should().Be(UserErrors.ClassAccessForbidden);
        this.ShouldHaveSearched(0);
    }

    [Test]
    public async Task return_unauthenticated_without_reading_when_there_is_no_current_user()
    {
        Guid classId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldHaveNoCurrentUser();

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.ShouldHaveSearched(0);
    }

    [Test]
    public async Task throw_class_not_found_without_searching_events_or_enrollments_when_the_class_does_not_exist()
    {
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldHaveSearched(0);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task list_the_birthdays_first_and_then_the_dated_events_by_year_month_and_day()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId, EnrollmentResponseMother.ConfirmedFor(studentId, classId));
        this.ShouldFindMembers();
        this.ShouldSearchCalendarEvents(
            CalendarEventMother.SchoolWide(schoolId, "march-10-2027", new DateOnly(2027, 3, 10), GuidMother.Random()),
            CalendarEventMother.ForClass(classId, "december-2026", new DateOnly(2026, 12, 1), GuidMother.Random()),
            CalendarEventMother.Birthday(studentId, "birthday-december", 12, 25, GuidMother.Random()),
            CalendarEventMother.ForClass(classId, "january-2027", new DateOnly(2027, 1, 5), GuidMother.Random()),
            CalendarEventMother.Birthday(studentId, "birthday-march-20", 3, 20, GuidMother.Random()),
            CalendarEventMother.SchoolWide(schoolId, "march-2-2027", new DateOnly(2027, 3, 2), GuidMother.Random()),
            CalendarEventMother.Birthday(studentId, "birthday-march-1", 3, 1, GuidMother.Random()));

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        result.Value.Select(item => item.Title).Should().Equal(
            "birthday-march-1",
            "birthday-march-20",
            "birthday-december",
            "december-2026",
            "january-2027",
            "march-2-2027",
            "march-10-2027");
    }

    [Test]
    public async Task map_a_dated_event_with_its_date_time_location_cost_author_and_creation_time()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string title = StringMother.Random("trip");
        string location = StringMother.Random("place");
        string cost = StringMother.Random("cost");
        string authorName = StringMother.Random("author");
        DateTimeOffset createdAt = Now.AddHours(-5);
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindMembers(UserClassMemberResponseMother.Representative(authorId, authorName));
        this.ShouldSearchCalendarEvents(CalendarEventMother.Trip(
            classId,
            title,
            new DateOnly(2027, 2, 28),
            new TimeOnly(9, 30),
            location,
            cost,
            authorId,
            createdAt));

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        CalendarEventResponse item = result.Value.Should().ContainSingle().Subject;
        item.Scope.Should().Be("Class");
        item.ClassId.Should().Be(classId);
        item.StudentId.Should().BeNull();
        item.Type.Should().Be("Trip");
        item.Title.Should().Be(title);
        item.Year.Should().Be(2027);
        item.Month.Should().Be(2);
        item.Day.Should().Be(28);
        item.Time.Should().Be(new TimeOnly(9, 30));
        item.Location.Should().Be(location);
        item.Cost.Should().Be(cost);
        item.AuthorId.Should().Be(authorId);
        item.AuthorName.Should().Be(authorName);
        item.AuthorRole.Should().Be("ClassRepresentative");
        item.CreatedAt.Should().Be(createdAt);
    }

    [Test]
    public async Task map_a_birthday_with_its_student_and_month_and_day_but_without_year_or_class()
    {
        Guid classId = GuidMother.Random();
        Guid studentId = GuidMother.Random();
        string title = StringMother.Random("birthday");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId, EnrollmentResponseMother.ConfirmedFor(studentId, classId));
        this.ShouldFindMembers();
        this.ShouldSearchCalendarEvents(CalendarEventMother.Birthday(studentId, title, 2, 29, GuidMother.Random()));

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        CalendarEventResponse item = result.Value.Should().ContainSingle().Subject;
        item.Scope.Should().Be("Class");
        item.ClassId.Should().BeNull();
        item.StudentId.Should().Be(studentId);
        item.Type.Should().Be("Birthday");
        item.Title.Should().Be(title);
        item.Year.Should().BeNull();
        item.Month.Should().Be(2);
        item.Day.Should().Be(29);
        item.Time.Should().BeNull();
        item.Location.Should().BeNull();
        item.Cost.Should().BeNull();
    }

    [Test]
    public async Task return_an_empty_author_name_and_no_role_when_the_author_is_not_a_known_user()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindMembers(
            UserClassMemberResponseMother.Representative(GuidMother.Random(), StringMother.Random("other")));
        this.ShouldSearchCalendarEvents(
            CalendarEventMother.SchoolWide(schoolId, StringMother.Random("school"), TODAY, GuidMother.Random()));

        Result<IReadOnlyCollection<CalendarEventResponse>> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByClassQueryMother.WithClassId(classId));

        CalendarEventResponse item = result.Value.Should().ContainSingle().Subject;
        item.AuthorName.Should().BeEmpty();
        item.AuthorRole.Should().BeNull();
    }

    [Test]
    public async Task ask_for_every_author_once_in_the_context_of_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid firstAuthorId = GuidMother.Random();
        Guid secondAuthorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldFindEnrollments(classId);
        this.ShouldFindMembers();
        this.ShouldSearchCalendarEvents(
            CalendarEventMother.SchoolWide(schoolId, StringMother.Random("school"), TODAY, firstAuthorId),
            CalendarEventMother.ForClass(classId, StringMother.Random("class"), TODAY, firstAuthorId),
            CalendarEventMother.ForClass(classId, StringMother.Random("class"), TODAY, secondAuthorId));

        await this._useCase.ExecuteAsync(CalendarEventFinderByClassQueryMother.WithClassId(classId));

        this.ShouldHaveAskedForAuthors(classId, firstAuthorId, secondAuthorId);
    }

    private void ShouldFindEnrollments(Guid classId, params EnrollmentResponse[] enrollments)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<EnrollmentFinderByClass.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

    private void ShouldFindMembers(params UserClassMemberResponse[] members)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserClassMemberFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);

    private void ShouldHaveAskedForAuthors(Guid classId, params Guid[] authorIds)
        => this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserClassMemberFinderByIds.Query>(query => query.ClassId == classId
                    && query.UserIds.Count == authorIds.Length
                    && authorIds.All(authorId => query.UserIds.Contains(authorId))),
                It.IsAny<CancellationToken>()),
            Times.Once);
}
