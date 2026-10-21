namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Domain;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.CalendarEvents.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Shared.Application;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using System.Text;

[TestFixture]
public class AnnouncementFinderByClassShould : AnnouncementsUnitTestCase
{
    private const int VALID_LIMIT = 10;

    private readonly AnnouncementFinderByClass.Finder _useCase;

    public AnnouncementFinderByClassShould()
    {
        this._useCase = new AnnouncementFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_announcement_with_its_author_name_role_and_related_event()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        string authorName = StringMother.Random("author");
        string eventTitle = StringMother.Random("event");
        Announcement announcement = AnnouncementMother.With(
            Now,
            scope: AnnouncementScope.Class,
            classId: classId,
            authorId: authorId,
            relatedEventId: eventId);
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldHaveClassMembers(UserClassMemberResponseMother.Representative(authorId, authorName));
        this.ShouldFindCalendarEvents(CalendarEventSummaryResponseMother.With(eventId, eventTitle));
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldSearchAnnouncements(announcement);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        AnnouncementResponse item = result.Value.Items.Should().ContainSingle().Subject;
        item.Id.Should().Be(announcement.Id);
        item.Scope.Should().Be("Class");
        item.ClassId.Should().Be(classId);
        item.Title.Should().Be(announcement.Title);
        item.Body.Should().Be(announcement.Body);
        item.Priority.Should().Be("Normal");
        item.AuthorId.Should().Be(authorId);
        item.AuthorName.Should().Be(authorName);
        item.AuthorRole.Should().Be("ClassRepresentative");
        item.CreatedAt.Should().Be(announcement.CreatedAt);
        item.RelatedEvent.Should().Be(CalendarEventSummaryResponseMother.With(eventId, eventTitle));
        item.Read.Should().BeFalse();
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_an_empty_name_no_role_and_no_event_when_the_author_and_the_event_are_not_found()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldSearchAnnouncements(
            AnnouncementMother.With(Now, schoolId: schoolId, relatedEventId: GuidMother.Random()));

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        AnnouncementResponse item = result.Value.Items.Should().ContainSingle().Subject;
        item.AuthorName.Should().BeEmpty();
        item.AuthorRole.Should().BeNull();
        item.RelatedEvent.Should().BeNull();
    }

    [Test]
    public async Task mark_as_read_only_the_announcements_the_current_user_has_read()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldSearchAnnouncements(
            AnnouncementMother.ReadBy(schoolId, currentUserId, Now.AddMinutes(3)),
            AnnouncementMother.ReadBy(schoolId, GuidMother.Random(), Now.AddMinutes(2)),
            AnnouncementMother.With(Now.AddMinutes(1), schoolId: schoolId));

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Value.Items.Select(item => item.Read).Should().Equal(true, false, false);
    }

    [Test]
    public async Task return_the_announcements_of_the_school_and_of_the_class_but_not_those_of_other_classes_or_schools()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Announcement school = AnnouncementMother.With(Now, schoolId: schoolId);
        Announcement mine = AnnouncementMother.ForClass(classId, StringMother.Random("notice"), Now);
        Announcement other = AnnouncementMother.ForClass(GuidMother.Random(), StringMother.Random("notice"), Now);
        Announcement otherSchool = AnnouncementMother.With(Now, schoolId: GuidMother.Random());
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldSearchAnnouncements(school, mine, other, otherSchool);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Value.Items.Select(item => item.Id).Should().BeEquivalentTo([school.Id, mine.Id]);
    }

    [Test]
    public async Task ask_for_the_members_and_the_events_only_of_the_returned_page_and_once_each()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Announcement first = AnnouncementMother.With(
            Now.AddMinutes(4),
            schoolId: schoolId,
            authorId: authorId,
            relatedEventId: eventId);
        Announcement sameAuthorAndEvent = AnnouncementMother.With(
            Now.AddMinutes(3),
            schoolId: schoolId,
            authorId: authorId,
            relatedEventId: eventId);
        Announcement second = AnnouncementMother.With(Now.AddMinutes(2), schoolId: schoolId);
        Guid secondAuthorId = second.AuthorId;
        Announcement cut = AnnouncementMother.With(
            Now.AddMinutes(1),
            schoolId: schoolId,
            relatedEventId: GuidMother.Random());
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldHaveClassMembers();
        this.ShouldFindCalendarEvents();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        this.ShouldSearchAnnouncements(first, sameAuthorAndEvent, second, cut);

        await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, 3),
            CancellationToken.None);

        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserClassMemberFinderByIds.Query>(query => query.ClassId == classId
                    && query.UserIds.Count == 2
                    && query.UserIds.Contains(authorId)
                    && query.UserIds.Contains(secondAuthorId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<CalendarEventFinderByIds.Query>(query => query.CalendarEventIds.Count == 1
                    && query.CalendarEventIds.Contains(eventId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task merge_both_branches_newest_first_and_return_the_next_cursor_when_there_are_more_than_the_limit()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldSearchAnnouncementsSuccessively(
            [SchoolNotice(schoolId, authorId, titlePrefix, 3), SchoolNotice(schoolId, authorId, titlePrefix, 1)],
            [ClassNotice(classId, authorId, titlePrefix, 4), ClassNotice(classId, authorId, titlePrefix, 2)]);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Select(item => item.Title)
            .Should().Equal(TitleOf(titlePrefix, 4), TitleOf(titlePrefix, 3));
        PageCursor.TryDecode(result.Value.NextCursor, out PageCursor cursor).Should().BeTrue();
        cursor.Key.Should().Be(Now.AddMinutes(3).UtcTicks);
    }

    [Test]
    public async Task return_no_next_cursor_when_there_are_as_many_announcements_as_the_limit()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldSearchAnnouncementsSuccessively(
            [SchoolNotice(schoolId, authorId, titlePrefix, 1)],
            [ClassNotice(classId, authorId, titlePrefix, 2)]);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, 2),
            CancellationToken.None);

        result.Value.Items.Should().HaveCount(2);
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_only_what_comes_after_the_boundary_when_following_the_next_cursor()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldSearchAnnouncements(
            SchoolNotice(schoolId, authorId, titlePrefix, 1),
            ClassNotice(classId, authorId, titlePrefix, 3),
            SchoolNotice(schoolId, authorId, titlePrefix, 2),
            ClassNotice(classId, authorId, titlePrefix, 4));
        Result<PagedResponse<AnnouncementResponse>> firstPage = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, 2),
            CancellationToken.None);

        Result<PagedResponse<AnnouncementResponse>> secondPage = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithCursor(classId, 2, firstPage.Value.NextCursor!),
            CancellationToken.None);

        firstPage.Value.Items.Select(item => item.Title)
            .Should().Equal(TitleOf(titlePrefix, 4), TitleOf(titlePrefix, 3));
        secondPage.Value.Items.Select(item => item.Title)
            .Should().Equal(TitleOf(titlePrefix, 2), TitleOf(titlePrefix, 1));
        secondPage.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task paginate_announcements_created_at_the_same_time_without_duplicates_or_gaps()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldAllowClassAccess(authorId, classId);
        Announcement[] tied =
        [
            SchoolNotice(schoolId, authorId, titlePrefix, 5),
            ClassNotice(classId, authorId, titlePrefix, 5),
            SchoolNotice(schoolId, authorId, titlePrefix, 5),
        ];
        this.ShouldSearchAnnouncements(tied);
        List<Guid> seen = [];
        string? cursor = null;

        for (int requested = 0; requested <= tied.Length; requested++)
        {
            Result<PagedResponse<AnnouncementResponse>> page = await this._useCase.ExecuteAsync(
                AnnouncementFinderByClassQueryMother.With(classId, 1, cursor),
                CancellationToken.None);
            seen.AddRange(page.Value.Items.Select(item => item.Id));
            cursor = page.Value.NextCursor;
            if (cursor is null)
            {
                break;
            }
        }

        cursor.Should().BeNull();
        seen.Should().BeEquivalentTo(tied.Select(announcement => announcement.Id));
    }

    [TestCase("garbage!")]
    [TestCase("")]
    [TestCase("bm9wZQ")]
    public async Task return_invalid_cursor_without_reading_anything_when_the_cursor_is_invalid(string cursor)
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor),
            CancellationToken.None);

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
    }

    [TestCase("abc|f0000000-0000-0000-0000-000000000001")]
    [TestCase("123|not-a-guid")]
    public async Task return_invalid_cursor_when_the_cursor_key_or_id_cannot_be_parsed(string payload)
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        string cursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).TrimEnd('=');

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor),
            CancellationToken.None);

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_invalid_cursor_when_the_cursor_key_is_out_of_range_for_a_date()
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        string cursor = PageCursorMother.Create(long.MaxValue, GuidMother.Random()).Encode();

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithCursor(classId, VALID_LIMIT, cursor),
            CancellationToken.None);

        result.Error!.Code.Should().Be("Pagination.InvalidCursor");
        this.ShouldNotHaveSearched();
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(51)]
    public async Task return_invalid_limit_when_the_limit_is_out_of_range(int limit)
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, limit),
            CancellationToken.None);

        result.Error!.Code.Should().Be("Pagination.InvalidLimit");
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_the_default_number_of_announcements_when_no_limit_is_given()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldSearchAnnouncements(Enumerable.Range(1, Pagination.DefaultLimit + 1)
            .Select(minutes => SchoolNotice(schoolId, authorId, titlePrefix, minutes))
            .ToArray());

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Value.Items.Should().HaveCount(Pagination.DefaultLimit);
        result.Value.NextCursor.Should().NotBeNull();
    }

    [Test]
    public async Task order_announcements_created_at_the_same_time_by_descending_id()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        Announcement low = AnnouncementMother.With(
            id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            schoolId: schoolId,
            createdAt: Now);
        Announcement high = AnnouncementMother.With(
            id: Guid.Parse("ff000000-0000-0000-0000-000000000000"),
            schoolId: schoolId,
            createdAt: Now);
        this.ShouldSearchAnnouncements(low, high);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Value.Items.Select(item => item.Id).Should().Equal(high.Id, low.Id);
    }

    [Test]
    public async Task throw_the_school_error_without_reading_when_the_school_of_the_class_cannot_be_found()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailFindingSchoolOfSchoolYear(schoolYearId, new SchoolYearExceptions.NotFound());
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(GuidMother.Random()),
            CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Error.Should().Be(UserErrors.ClassAccessForbidden);
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_when_the_class_is_unknown()
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
    }

    private static Announcement SchoolNotice(Guid schoolId, Guid authorId, string titlePrefix, int minutes)
        => AnnouncementMother.SchoolWide(schoolId, TitleOf(titlePrefix, minutes), Now.AddMinutes(minutes), authorId);

    private static Announcement ClassNotice(Guid classId, Guid authorId, string titlePrefix, int minutes)
        => AnnouncementMother.ForClass(classId, TitleOf(titlePrefix, minutes), Now.AddMinutes(minutes), authorId);

    private static string TitleOf(string titlePrefix, int minutes) => $"{titlePrefix}-{minutes}";

    private void ShouldFindNoMembers()
        => this.ShouldHaveClassMembers();

    private void ShouldFindNoCalendarEvents()
        => this.ShouldFindCalendarEvents();

    private void ShouldHaveClassMembers(params UserClassMemberResponse[] members)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserClassMemberFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserClassMemberFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<UserClassMemberResponse>)members
                    .Where(member => query.UserIds.Contains(member.UserId))
                    .ToList());

    private void ShouldFindCalendarEvents(params CalendarEventSummaryResponse[] calendarEvents)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<CalendarEventFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CalendarEventFinderByIds.Query query, CancellationToken _)
                => (IReadOnlyCollection<CalendarEventSummaryResponse>)calendarEvents
                    .Where(calendarEvent => query.CalendarEventIds.Contains(calendarEvent.Id))
                    .ToList());
}
