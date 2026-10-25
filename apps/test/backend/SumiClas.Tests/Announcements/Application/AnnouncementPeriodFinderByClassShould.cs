namespace SumiClas.Tests.Announcements.Application;

using AwesomeAssertions;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class AnnouncementPeriodFinderByClassShould : AnnouncementsUnitTestCase
{
    private const int ONE_DAY = 1;
    private const int TWO_DAYS = 2;
    private const int THREE_DAYS = 3;
    private const int WEEK_LENGTH_AFTER_FIRST_DAY = 6;
    private const int ONE_SECOND = 1;

    private static readonly DateOnly FROM = new(2026, 10, 5);
    private static readonly DateOnly TO = FROM.AddDays(WEEK_LENGTH_AFTER_FIRST_DAY);
    private static readonly DateTimeOffset START = StartOf(FROM);
    private static readonly DateTimeOffset END = StartOf(TO.AddDays(ONE_DAY));
    private static readonly TimeSpan JUST_OUTSIDE = TimeSpan.FromSeconds(ONE_SECOND);

    private readonly AnnouncementPeriodFinderByClass.Finder _useCase;

    public AnnouncementPeriodFinderByClassShould()
    {
        this._useCase = new AnnouncementPeriodFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_the_title_and_urgency_of_the_announcements_of_the_school_and_of_the_class_oldest_first()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        string schoolTitle = StringMother.Random("school");
        string classTitle = StringMother.Random("class");
        this.ShouldSearchAnnouncements(
            AnnouncementMother.ForClass(classId, classTitle, START.AddDays(TWO_DAYS)),
            AnnouncementMother.SchoolWide(schoolId, schoolTitle, START.AddDays(ONE_DAY)));

        IReadOnlyCollection<AnnouncementPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().Equal(
            new AnnouncementPeriodResponse(schoolTitle, Urgent: false),
            new AnnouncementPeriodResponse(classTitle, Urgent: false));
    }

    [Test]
    public async Task mark_as_urgent_only_the_announcements_of_high_priority()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        string urgentTitle = StringMother.Random("urgent");
        string normalTitle = StringMother.Random("normal");
        string lowTitle = StringMother.Random("low");
        this.ShouldSearchAnnouncements(
            AnnouncementMother.With(START, schoolId: schoolId, title: urgentTitle, priority: Priority.High),
            AnnouncementMother.With(
                START.AddDays(ONE_DAY), schoolId: schoolId, title: normalTitle, priority: Priority.Normal),
            AnnouncementMother.With(
                START.AddDays(TWO_DAYS), schoolId: schoolId, title: lowTitle, priority: Priority.Low));

        IReadOnlyCollection<AnnouncementPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().Equal(
            new AnnouncementPeriodResponse(urgentTitle, Urgent: true),
            new AnnouncementPeriodResponse(normalTitle, Urgent: false),
            new AnnouncementPeriodResponse(lowTitle, Urgent: false));
    }

    [Test]
    public async Task order_the_announcements_published_at_the_same_instant_by_id()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        Guid[] ids = [GuidMother.Random(), GuidMother.Random()];
        Guid first = ids.OrderBy(id => id, Pagination.IdComparer).First();
        Guid second = ids.OrderBy(id => id, Pagination.IdComparer).Last();
        string firstTitle = StringMother.Random("first");
        string secondTitle = StringMother.Random("second");
        this.ShouldSearchAnnouncements(
            AnnouncementMother.With(START, id: second, schoolId: schoolId, title: secondTitle),
            AnnouncementMother.With(START, id: first, schoolId: schoolId, title: firstTitle));

        IReadOnlyCollection<AnnouncementPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(announcement => announcement.Title).Should().Equal(firstTitle, secondTitle);
    }

    [Test]
    public async Task include_what_was_published_from_the_first_instant_of_the_first_day_to_the_last_of_the_last_day()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        string first = StringMother.Random("first");
        string last = StringMother.Random("last");
        this.ShouldSearchAnnouncements(
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("school before"), START - JUST_OUTSIDE),
            AnnouncementMother.ForClass(classId, StringMother.Random("class before"), START - JUST_OUTSIDE),
            AnnouncementMother.SchoolWide(schoolId, first, START),
            AnnouncementMother.ForClass(classId, last, END - JUST_OUTSIDE),
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("school after"), END),
            AnnouncementMother.ForClass(classId, StringMother.Random("class after"), END));

        IReadOnlyCollection<AnnouncementPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(announcement => announcement.Title).Should().Equal(first, last);
    }

    [Test]
    public async Task cover_the_whole_day_when_the_period_is_a_single_day()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        DateOnly day = FROM.AddDays(THREE_DAYS);
        string morning = StringMother.Random("morning");
        string night = StringMother.Random("night");
        this.ShouldSearchAnnouncements(
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("day before"), StartOf(day) - JUST_OUTSIDE),
            AnnouncementMother.SchoolWide(schoolId, morning, StartOf(day)),
            AnnouncementMother.SchoolWide(schoolId, night, StartOf(day.AddDays(ONE_DAY)) - JUST_OUTSIDE),
            AnnouncementMother.SchoolWide(schoolId, StringMother.Random("day after"), StartOf(day.AddDays(ONE_DAY))));

        IReadOnlyCollection<AnnouncementPeriodResponse> result = await this._useCase.ExecuteAsync(
            AnnouncementPeriodFinderByClassQueryMother.With(classId, day, day),
            CancellationToken.None);

        result.Select(announcement => announcement.Title).Should().Equal(morning, night);
    }

    [Test]
    public async Task give_the_same_answer_to_whoever_asks_whether_they_wrote_or_read_the_announcements_or_not()
    {
        Guid currentUserId = GuidMother.Random();
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser(currentUserId);
        string written = StringMother.Random("written");
        string read = StringMother.Random("read");
        this.ShouldSearchAnnouncements(
            AnnouncementMother.SchoolWide(schoolId, written, START, currentUserId),
            AnnouncementMother.ReadBy(schoolId, currentUserId, START.AddDays(ONE_DAY), title: read));

        IReadOnlyCollection<AnnouncementPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(announcement => announcement.Title).Should().Equal(written, read);
    }

    [Test]
    public async Task not_include_the_announcements_of_other_classes_or_other_schools()
    {
        (Guid classId, Guid schoolId) = this.ShouldResolveClassOfCurrentUser();
        string mine = StringMother.Random("mine");
        this.ShouldSearchAnnouncements(
            AnnouncementMother.SchoolWide(schoolId, mine, START),
            AnnouncementMother.SchoolWide(GuidMother.Random(), StringMother.Random("other school"), START),
            AnnouncementMother.ForClass(GuidMother.Random(), StringMother.Random("other class"), START));

        IReadOnlyCollection<AnnouncementPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(announcement => announcement.Title).Should().Equal(mine);
    }

    [Test]
    public async Task throw_class_access_forbidden_without_reading_when_the_user_does_not_belong_to_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFindClass(classId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassAccess(currentUserId, classId);

        Func<Task> act = () => this.InPeriodAsync(classId);

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    private static DateTimeOffset StartOf(DateOnly day) => new(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private Task<IReadOnlyCollection<AnnouncementPeriodResponse>> InPeriodAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            AnnouncementPeriodFinderByClassQueryMother.With(classId, FROM, TO),
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
