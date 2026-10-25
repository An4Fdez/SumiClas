namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class AnnouncementUnreadSummaryFinderByClassShould : AnnouncementsUnitTestCase
{
    private const int REQUESTED_DAYS = 7;
    private const int ONE_DAY = 1;
    private const int TWO_DAYS = 2;
    private const int ONE_SECOND = 1;

    private static readonly TimeSpan YESTERDAY = TimeSpan.FromDays(ONE_DAY);
    private static readonly TimeSpan TWO_DAYS_AGO = TimeSpan.FromDays(TWO_DAYS);
    private static readonly TimeSpan JUST_OUTSIDE = TimeSpan.FromSeconds(ONE_SECOND);

    private readonly AnnouncementUnreadSummaryFinderByClass.Finder _useCase;

    public AnnouncementUnreadSummaryFinderByClassShould()
    {
        this._useCase = new AnnouncementUnreadSummaryFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task count_the_unread_announcements_of_the_school_and_of_the_class()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        Announcement[] unread =
        [
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("school"), Now - YESTERDAY),
            AnnouncementMother.ForClass(classId, StringMother.Random("class"), Now - TWO_DAYS_AGO),
            AnnouncementMother.ForClass(classId, StringMother.Random("other"), Now),
        ];
        this.ShouldSearchAnnouncements(unread);

        AnnouncementUnreadSummaryResponse result = await this.SummarizeAsync(classId);

        result.Unread.Should().Be(unread.Length);
    }

    [Test]
    public async Task not_count_the_announcements_the_current_user_has_read_but_count_the_ones_others_have_read()
    {
        Guid currentUserId = GuidMother.Random();
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser(currentUserId);
        Announcement[] unreadByTheUser =
        [
            AnnouncementMother.ReadBy(schoolId, GuidMother.Random(), Now - YESTERDAY),
            AnnouncementMother.ClassReadBy(classId, GuidMother.Random(), Now - YESTERDAY),
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("unread"), Now - YESTERDAY),
        ];
        this.ShouldSearchAnnouncements(
        [
            AnnouncementMother.ReadBy(schoolId, currentUserId, Now - YESTERDAY),
            AnnouncementMother.ClassReadBy(classId, currentUserId, Now - YESTERDAY),
            .. unreadByTheUser,
        ]);

        AnnouncementUnreadSummaryResponse result = await this.SummarizeAsync(classId);

        result.Unread.Should().Be(unreadByTheUser.Length);
    }

    [Test]
    public async Task not_count_the_announcements_the_current_user_published_but_count_the_ones_of_others()
    {
        Guid currentUserId = GuidMother.Random();
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser(currentUserId);
        Announcement[] ofOthers =
        [
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("theirs"), Now, GuidMother.Random()),
            AnnouncementMother.ForClass(classId, StringMother.Random("theirs too"), Now, GuidMother.Random()),
        ];
        this.ShouldSearchAnnouncements(
        [
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("mine"), Now, currentUserId),
            AnnouncementMother.ForClass(classId, StringMother.Random("mine too"), Now, currentUserId),
            .. ofOthers,
        ]);

        AnnouncementUnreadSummaryResponse result = await this.SummarizeAsync(classId);

        result.Unread.Should().Be(ofOthers.Length);
    }

    [Test]
    public async Task count_as_urgent_only_the_unread_announcements_of_high_priority()
    {
        Guid currentUserId = GuidMother.Random();
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser(currentUserId);
        Announcement[] unreadUrgent =
        [
            AnnouncementMother.With(Now, schoolId: schoolId, priority: Priority.High),
            AnnouncementMother.With(Now, scope: AnnouncementScope.Class, classId: classId, priority: Priority.High),
        ];
        Announcement[] unreadNotUrgent =
        [
            AnnouncementMother.With(Now, schoolId: schoolId, priority: Priority.Normal),
            AnnouncementMother.With(Now, schoolId: schoolId, priority: Priority.Low),
        ];
        this.ShouldSearchAnnouncements(
        [
            AnnouncementMother.ReadBy(schoolId, currentUserId, Now, Priority.High),
            .. unreadUrgent,
            .. unreadNotUrgent,
        ]);

        AnnouncementUnreadSummaryResponse result = await this.SummarizeAsync(classId);

        result.Unread.Should().Be(unreadUrgent.Length + unreadNotUrgent.Length);
        result.UnreadUrgent.Should().Be(unreadUrgent.Length);
    }

    [Test]
    public async Task count_the_announcements_of_exactly_the_last_thirty_days_but_not_the_older_ones()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        TimeSpan window = TimeSpan.FromDays(SummaryPeriod.DefaultDays);
        Announcement[] inside =
        [
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("school edge"), Now - window),
            AnnouncementMother.ForClass(classId, StringMother.Random("class edge"), Now - window),
        ];
        this.ShouldSearchAnnouncements(
        [
            .. inside,
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("school old"), Now - window - JUST_OUTSIDE),
            AnnouncementMother.ForClass(classId, StringMother.Random("class old"), Now - window - JUST_OUTSIDE),
        ]);

        AnnouncementUnreadSummaryResponse result = await this.SummarizeAsync(classId);

        result.Unread.Should().Be(inside.Length);
    }

    [Test]
    public async Task look_back_only_as_many_days_as_requested()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        TimeSpan window = TimeSpan.FromDays(REQUESTED_DAYS);
        Announcement[] inside =
        [
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("edge"), Now - window),
            AnnouncementMother.ForClass(classId, StringMother.Random("edge too"), Now - window),
        ];
        this.ShouldSearchAnnouncements(
        [
            .. inside,
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("older"), Now - window - JUST_OUTSIDE),
            AnnouncementMother.ForClass(classId, StringMother.Random("older too"), Now - window - JUST_OUTSIDE),
        ]);

        AnnouncementUnreadSummaryResponse result = await this._useCase.ExecuteAsync(
            AnnouncementUnreadSummaryFinderByClassQueryMother.WithDays(classId, REQUESTED_DAYS),
            CancellationToken.None);

        result.Unread.Should().Be(inside.Length);
    }

    [TestCase(SummaryPeriod.MinDays - ONE_DAY)]
    [TestCase(-REQUESTED_DAYS)]
    [TestCase(SummaryPeriod.MaxDays + ONE_DAY)]
    public async Task throw_invalid_days_without_reading_when_the_period_is_out_of_range(int days)
    {
        (Guid classId, _) = this.ShouldResolveClassOfCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AnnouncementUnreadSummaryFinderByClassQueryMother.WithDays(classId, days),
            CancellationToken.None);

        await act.Should().ThrowAsync<SummaryPeriodExceptions.InvalidDays>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task not_count_the_announcements_of_other_classes_or_other_schools()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        Announcement[] mine = [AnnouncementMother.SchoolWide(schoolId, StringMother.Random("mine"), Now)];
        this.ShouldSearchAnnouncements(
        [
            .. mine,
            AnnouncementMother.SchoolWide(GuidMother.Random(), StringMother.Random("other school"), Now),
            AnnouncementMother.ForClass(GuidMother.Random(), StringMother.Random("other class"), Now),
        ]);

        AnnouncementUnreadSummaryResponse result = await this.SummarizeAsync(classId);

        result.Unread.Should().Be(mine.Length);
    }

    [Test]
    public async Task return_zeros_when_there_are_no_announcements()
    {
        (Guid classId, _) = this.ShouldResolveClassOfCurrentUser();
        this.ShouldSearchAnnouncements();

        AnnouncementUnreadSummaryResponse result = await this.SummarizeAsync(classId);

        result.Should().Be(AnnouncementUnreadSummaryResponseMother.None());
    }

    [Test]
    public async Task throw_unauthenticated_without_asking_or_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this.SummarizeAsync(GuidMother.Random());

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Func<Task> act = () => this.SummarizeAsync(classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_the_class_error_when_the_class_is_unknown()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Func<Task> act = () => this.SummarizeAsync(GuidMother.Random());

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
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

        Func<Task> act = () => this.SummarizeAsync(classId);

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    private Task<AnnouncementUnreadSummaryResponse> SummarizeAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            AnnouncementUnreadSummaryFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

    private (Guid ClassId, Guid SchoolId) ShouldResolveClassOfCurrentUser(Guid? userId = null)
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        Guid currentUserId = userId ?? GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFindSchoolOfSchoolYear(schoolYearId, schoolId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassAccess(currentUserId, classId);
        return (classId, schoolId);
    }
}
