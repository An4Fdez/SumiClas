namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Shared.Application;
using SumiClas.Users.Application;
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
    public async Task merge_both_branches_newest_first_and_return_the_next_cursor_when_there_are_more_than_the_limit()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        this.ShouldFindClass(classId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldSearchAnnouncementsSuccessively(
            [SchoolNotice(authorId, titlePrefix, 3), SchoolNotice(authorId, titlePrefix, 1)],
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
        this.ShouldFindClass(classId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldSearchAnnouncementsSuccessively(
            [SchoolNotice(authorId, titlePrefix, 1)],
            [ClassNotice(classId, authorId, titlePrefix, 2)]);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, 2),
            CancellationToken.None);

        result.Value.Items.Should().HaveCount(2);
        result.Value.NextCursor.Should().BeNull();
    }

    [Test]
    public async Task return_the_newest_announcements_first_and_a_next_cursor_on_the_first_page()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        this.ShouldFindClass(classId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldSearchAnnouncements(
            SchoolNotice(authorId, titlePrefix, 1),
            ClassNotice(classId, authorId, titlePrefix, 3),
            SchoolNotice(authorId, titlePrefix, 2),
            ClassNotice(classId, authorId, titlePrefix, 4));

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, 3),
            CancellationToken.None);

        result.Value.Items.Select(item => item.Title)
            .Should().Equal(TitleOf(titlePrefix, 4), TitleOf(titlePrefix, 3), TitleOf(titlePrefix, 2));
        result.Value.NextCursor.Should().NotBeNull();
    }

    [Test]
    public async Task return_only_what_comes_after_the_boundary_when_following_the_next_cursor()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string titlePrefix = StringMother.Random("notice");
        this.ShouldFindClass(classId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldSearchAnnouncements(
            SchoolNotice(authorId, titlePrefix, 1),
            ClassNotice(classId, authorId, titlePrefix, 3),
            SchoolNotice(authorId, titlePrefix, 2),
            ClassNotice(classId, authorId, titlePrefix, 4));
        Result<PagedResponse<AnnouncementResponse>> firstPage = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithLimit(classId, 2),
            CancellationToken.None);

        Result<PagedResponse<AnnouncementResponse>> secondPage = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithCursor(classId, 2, firstPage.Value.NextCursor!),
            CancellationToken.None);

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
        this.ShouldFindClass(classId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        Announcement[] tied =
        [
            SchoolNotice(authorId, titlePrefix, 5),
            ClassNotice(classId, authorId, titlePrefix, 5),
            SchoolNotice(authorId, titlePrefix, 5),
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
    [TestCase(500)]
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
        this.ShouldFindClass(classId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldSearchAnnouncements(Enumerable.Range(1, Pagination.DefaultLimit + 1)
            .Select(minutes => SchoolNotice(authorId, titlePrefix, minutes))
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
        this.ShouldFindClass(classId);
        this.ShouldFindNoMembers();
        this.ShouldFindNoCalendarEvents();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        Announcement low = AnnouncementMother.With(
            id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            createdAt: Now);
        Announcement high = AnnouncementMother.With(
            id: Guid.Parse("ff000000-0000-0000-0000-000000000000"),
            createdAt: Now);
        this.ShouldSearchAnnouncements(low, high);

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Value.Items.Select(item => item.Id).Should().Equal(high.Id, low.Id);
    }

    [Test]
    public async Task return_the_class_error_when_the_class_is_unknown()
    {
        Guid classId = GuidMother.Random();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Result<PagedResponse<AnnouncementResponse>> result = await this._useCase.ExecuteAsync(
            AnnouncementFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

        result.Error!.Code.Should().Be("Class.NotFound");
    }

    private static Announcement SchoolNotice(Guid authorId, string titlePrefix, int minutes)
        => AnnouncementMother.SchoolWide(TitleOf(titlePrefix, minutes), Now.AddMinutes(minutes), authorId);

    private static Announcement ClassNotice(Guid classId, Guid authorId, string titlePrefix, int minutes)
        => AnnouncementMother.ForClass(classId, TitleOf(titlePrefix, minutes), Now.AddMinutes(minutes), authorId);

    private static string TitleOf(string titlePrefix, int minutes) => $"{titlePrefix}-{minutes}";

    private void ShouldFindNoMembers()
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<UserClassMemberResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserClassMemberResponse>());

    private void ShouldFindNoCalendarEvents()
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<IQuery<IReadOnlyCollection<CalendarEventSummaryResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEventSummaryResponse>());
}
