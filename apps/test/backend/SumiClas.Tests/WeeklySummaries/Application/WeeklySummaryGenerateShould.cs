namespace SumiClas.Tests.WeeklySummaries.Application;

using AwesomeAssertions;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Domain;
using SumiClas.Tests.Announcements.Application;
using SumiClas.Tests.Assignments.Application;
using SumiClas.Tests.CalendarEvents.Application;
using SumiClas.Tests.ClassExpenses.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.WeeklySummaries.Domain;
using SumiClas.Users.Domain;
using SumiClas.WeeklySummaries.Application;
using SumiClas.WeeklySummaries.Domain;

[TestFixture]
public class WeeklySummaryGenerateShould : WeeklySummariesUnitTestCase
{
    private const int DAYS_AFTER_MONDAY = 6;
    private const int DAYS_IN_WEEK = 7;
    private const int ONE_DAY = 1;
    private const int TWO_DAYS = 2;
    private const int THREE_DAYS = 3;
    private const int BIRTHDAYS_OF_THE_WEEK = 2;
    private const int LOST_ITEMS_OF_THE_CLASS = 3;
    private const int HOURS_UNTIL_SUNDAY_NIGHT = 35;
    private const int LAST_MINUTE = 59;
    private const int LAST_SECOND = 59;

    private static readonly DateOnly MONDAY = WeeklySummaryMother.Monday;
    private static readonly DateOnly SUNDAY = MONDAY.AddDays(DAYS_AFTER_MONDAY);

    private readonly WeeklySummaryGenerate.Generate _useCase;

    public WeeklySummaryGenerateShould()
    {
        this._useCase = new WeeklySummaryGenerate.Generate(
            this.Repository.Object,
            this.Writer.Object,
            this.QueryBus.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task return_the_text_written_for_the_week_when_the_class_has_no_summary_yet()
    {
        Guid classId = GuidMother.Random();
        string written = StringMother.Random("written");
        this.ShouldSearchWeeklySummaries();
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(classId, MONDAY, SUNDAY, announcements: [AnnouncementPeriodResponseMother.With()]);
        this.ShouldWrite(written);

        WeeklySummaryResponse result = await this.GenerateAsync(classId);

        result.Should().Be(WeeklySummaryResponseMother.Written(MONDAY, SUNDAY, written, Now));
    }

    [Test]
    public async Task store_the_text_for_the_class_and_the_week_with_the_fingerprint_of_what_it_was_written_from()
    {
        Guid classId = GuidMother.Random();
        string written = StringMother.Random("written");
        this.ShouldSearchWeeklySummaries();
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(
            classId,
            MONDAY,
            SUNDAY,
            announcements: [AnnouncementPeriodResponseMother.With()],
            lostItems: LOST_ITEMS_OF_THE_CLASS);
        this.ShouldWrite(written);

        await this.GenerateAsync(classId);

        WeeklySummary stored = this.Added.Should().ContainSingle().Subject;
        stored.ClassId.Should().Be(classId);
        stored.WeekStart.Should().Be(MONDAY);
        stored.Content.Should().Be(written);
        stored.InputHash.Should().Be(FingerprintOf(this.WrittenFrom.Single()));
        stored.GeneratedAt.Should().Be(Now);
        stored.Id.Should().NotBeEmpty();
    }

    [Test]
    public async Task give_the_writer_the_news_of_the_week_counting_the_birthdays_instead_of_naming_them()
    {
        Guid classId = GuidMother.Random();
        string urgent = StringMother.Random("urgent");
        string trip = StringMother.Random("trip");
        string meeting = StringMother.Random("meeting");
        string activity = StringMother.Random("activity");
        string homework = StringMother.Random("homework");
        string expense = StringMother.Random("expense");
        this.ShouldSearchWeeklySummaries();
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(
            classId,
            MONDAY,
            SUNDAY,
            announcements: [AnnouncementPeriodResponseMother.With(urgent, urgent: true)],
            events:
            [
                CalendarEventPeriodResponseMother.With(nameof(EventType.Trip), MONDAY.AddDays(ONE_DAY), trip),
                CalendarEventPeriodResponseMother.With(nameof(EventType.Birthday), MONDAY),
                CalendarEventPeriodResponseMother.With(nameof(EventType.Birthday), SUNDAY),
                CalendarEventPeriodResponseMother.With(nameof(EventType.Meeting), MONDAY.AddDays(TWO_DAYS), meeting),
                CalendarEventPeriodResponseMother.With(
                    nameof(EventType.Activity),
                    MONDAY.AddDays(THREE_DAYS),
                    activity),
            ],
            assignments: [AssignmentPeriodResponseMother.With(SUNDAY, homework)],
            expenses: [ClassExpenseOpenResponseMother.With(expense)],
            lostItems: LOST_ITEMS_OF_THE_CLASS);
        this.ShouldWrite(StringMother.Random("written"));

        await this.GenerateAsync(classId);

        this.WrittenFrom.Should().ContainSingle().Which.Should().BeEquivalentTo(
            WeeklySummaryInputMother.With(
                announcements: [WeeklySummaryInputMother.Announcement(urgent, urgent: true)],
                events:
                [
                    WeeklySummaryInputMother.Event(MONDAY.AddDays(ONE_DAY), trip),
                    WeeklySummaryInputMother.Event(MONDAY.AddDays(TWO_DAYS), meeting),
                    WeeklySummaryInputMother.Event(MONDAY.AddDays(THREE_DAYS), activity),
                ],
                assignments: [WeeklySummaryInputMother.Assignment(SUNDAY, homework)],
                pendingExpenses: [expense],
                birthdays: BIRTHDAYS_OF_THE_WEEK,
                unrecoveredLostItems: LOST_ITEMS_OF_THE_CLASS,
                weekStart: MONDAY));
    }

    [Test]
    public async Task read_a_sunday_as_part_of_the_week_that_started_the_monday_before()
    {
        Guid classId = GuidMother.Random();
        this.AdvanceClock(new TimeSpan(HOURS_UNTIL_SUNDAY_NIGHT, LAST_MINUTE, LAST_SECOND));
        this.ShouldSearchWeeklySummaries();
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(classId, MONDAY, SUNDAY, announcements: [AnnouncementPeriodResponseMother.With()]);
        this.ShouldWrite(StringMother.Random("written"));

        WeeklySummaryResponse result = await this.GenerateAsync(classId);

        result.WeekStart.Should().Be(MONDAY);
        result.WeekEnd.Should().Be(SUNDAY);
    }

    [Test]
    public async Task write_the_summary_again_when_the_week_changed_since_the_stored_one_was_written()
    {
        Guid classId = GuidMother.Random();
        string written = StringMother.Random("written");
        WeeklySummary stored = WeeklySummaryMother.With(
            classId: classId,
            generatedAt: Now - TimeSpan.FromDays(ONE_DAY));
        this.ShouldSearchWeeklySummaries(stored);
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(classId, MONDAY, SUNDAY, announcements: [AnnouncementPeriodResponseMother.With()]);
        this.ShouldWrite(written);

        WeeklySummaryResponse result = await this.GenerateAsync(classId);

        result.Should().Be(WeeklySummaryResponseMother.Written(MONDAY, SUNDAY, written, Now));
        stored.Content.Should().Be(written);
        stored.InputHash.Should().Be(FingerprintOf(this.WrittenFrom.Single()));
        stored.GeneratedAt.Should().Be(Now);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_the_stored_summary_without_asking_the_writer_when_the_week_has_not_changed()
    {
        Guid classId = GuidMother.Random();
        string announcement = StringMother.Random("announcement");
        WeeklySummaryInput week = WeeklySummaryInputMother.With(
            announcements: [WeeklySummaryInputMother.Announcement(announcement)],
            weekStart: MONDAY);
        DateTimeOffset writtenAt = Now - TimeSpan.FromDays(ONE_DAY);
        WeeklySummary stored = WeeklySummaryMother.With(
            classId: classId,
            inputHash: FingerprintOf(week),
            generatedAt: writtenAt);
        this.ShouldSearchWeeklySummaries(stored);
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(
            classId,
            MONDAY,
            SUNDAY,
            announcements: [AnnouncementPeriodResponseMother.With(announcement)]);

        WeeklySummaryResponse result = await this.GenerateAsync(classId);

        result.Should().Be(WeeklySummaryResponseMother.Written(MONDAY, SUNDAY, stored.Content, writtenAt));
        this.ShouldNotHaveWritten();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task tell_that_there_is_no_text_without_asking_the_writer_when_the_week_has_no_news()
    {
        Guid classId = GuidMother.Random();
        this.ShouldSearchWeeklySummaries();
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(classId, MONDAY, SUNDAY);

        WeeklySummaryResponse result = await this.GenerateAsync(classId);

        result.Should().Be(WeeklySummaryResponseMother.WithoutNews(MONDAY, SUNDAY));
        this.ShouldNotHaveWritten();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task leave_the_stored_summary_as_it_is_when_the_week_no_longer_has_news()
    {
        Guid classId = GuidMother.Random();
        WeeklySummary stored = WeeklySummaryMother.With(classId: classId);
        string content = stored.Content;
        string inputHash = stored.InputHash;
        this.ShouldSearchWeeklySummaries(stored);
        this.ShouldReadWeek(classId, MONDAY, SUNDAY);

        WeeklySummaryResponse result = await this.GenerateAsync(classId);

        result.Content.Should().BeNull();
        stored.Content.Should().Be(content);
        stored.InputHash.Should().Be(inputHash);
    }

    [Test]
    public async Task look_only_at_the_summary_of_the_class_and_of_the_week_when_others_exist()
    {
        Guid classId = GuidMother.Random();
        string written = StringMother.Random("written");
        WeeklySummary ofAnotherClass = WeeklySummaryMother.With();
        WeeklySummary ofAnotherWeek = WeeklySummaryMother.With(
            classId: classId,
            weekStart: MONDAY.AddDays(-DAYS_IN_WEEK));
        string anotherClassContent = ofAnotherClass.Content;
        string anotherWeekContent = ofAnotherWeek.Content;
        this.ShouldSearchWeeklySummaries(ofAnotherClass, ofAnotherWeek);
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(classId, MONDAY, SUNDAY, announcements: [AnnouncementPeriodResponseMother.With()]);
        this.ShouldWrite(written);

        await this.GenerateAsync(classId);

        this.Added.Should().ContainSingle().Which.Content.Should().Be(written);
        ofAnotherClass.Content.Should().Be(anotherClassContent);
        ofAnotherWeek.Content.Should().Be(anotherWeekContent);
    }

    [Test]
    public async Task let_the_failure_of_the_writer_through_and_store_nothing_when_the_class_has_no_summary()
    {
        Guid classId = GuidMother.Random();
        this.ShouldSearchWeeklySummaries();
        this.ShouldCaptureAddedWeeklySummaries();
        this.ShouldReadWeek(classId, MONDAY, SUNDAY, announcements: [AnnouncementPeriodResponseMother.With()]);
        this.ShouldFailWriting(new WeeklySummaryWriterException("The AI service is down."));

        Func<Task> act = () => this.GenerateAsync(classId);

        await act.Should().ThrowAsync<WeeklySummaryWriterException>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task let_the_failure_of_the_writer_through_and_keep_the_stored_summary_as_it_was()
    {
        Guid classId = GuidMother.Random();
        WeeklySummary stored = WeeklySummaryMother.With(classId: classId);
        string content = stored.Content;
        string inputHash = stored.InputHash;
        this.ShouldSearchWeeklySummaries(stored);
        this.ShouldReadWeek(classId, MONDAY, SUNDAY, announcements: [AnnouncementPeriodResponseMother.With()]);
        this.ShouldFailWriting(new WeeklySummaryWriterException("The AI service is down."));

        Func<Task> act = () => this.GenerateAsync(classId);

        await act.Should().ThrowAsync<WeeklySummaryWriterException>();
        stored.Content.Should().Be(content);
        stored.InputHash.Should().Be(inputHash);
    }

    [TestCase(typeof(UserExceptions.ClassAccessForbidden))]
    public async Task let_the_error_of_reading_the_week_through_without_asking_the_writer(Type exceptionType)
    {
        this.ShouldFailReadingWeek((Exception)Activator.CreateInstance(exceptionType)!);
        this.ShouldSearchWeeklySummaries();
        this.ShouldCaptureAddedWeeklySummaries();

        Func<Task> act = () => this.GenerateAsync(GuidMother.Random());

        await act.Should().ThrowAsync<Exception>().Where(exception => exception.GetType() == exceptionType);
        this.ShouldNotHaveWritten();
        this.ShouldNotHaveSearched();
        this.Added.Should().BeEmpty();
    }

    private static string FingerprintOf(WeeklySummaryInput input)
        => WeeklySummaryFingerprint.Of(WeeklySummaryPromptBuilder.Build(input));

    private Task<WeeklySummaryResponse> GenerateAsync(Guid classId)
        => this._useCase.ExecuteAsync(new WeeklySummaryGenerate.Command(classId), CancellationToken.None);
}
