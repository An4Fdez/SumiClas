namespace SumiClas.Tests.WeeklySummaries.Domain;

using AwesomeAssertions;
using SumiClas.Tests.Shared;
using SumiClas.WeeklySummaries.Domain;

[TestFixture]
public class WeeklySummaryShould : UnitTestCase
{
    private const int DAYS_IN_A_WEEK = 7;
    private const int FIRST_DAY_AFTER_MONDAY = 1;
    private const int MINUTES_LATER = 30;

    [Test]
    public void keep_what_it_is_created_with_and_the_instant_it_was_generated()
    {
        Guid id = GuidMother.Random();
        Guid classId = GuidMother.Random();
        string content = StringMother.Random("summary");
        string inputHash = WeeklySummaryMother.InputHash();

        WeeklySummary summary = WeeklySummary.Create(
            id,
            classId,
            WeeklySummaryMother.Monday,
            content,
            inputHash,
            this.Clock.Object);

        summary.Id.Should().Be(id);
        summary.ClassId.Should().Be(classId);
        summary.WeekStart.Should().Be(WeeklySummaryMother.Monday);
        summary.Content.Should().Be(content);
        summary.InputHash.Should().Be(inputHash);
        summary.GeneratedAt.Should().Be(Now);
    }

    [Test]
    public void drop_the_blanks_around_the_content()
    {
        string content = StringMother.Random("summary");

        WeeklySummary summary = this.CreateWithContent($"  {content}\n");

        summary.Content.Should().Be(content);
    }

    [Test]
    public void throw_id_required_when_the_id_is_empty()
    {
        Func<WeeklySummary> act = () => WeeklySummary.Create(
            Guid.Empty,
            GuidMother.Random(),
            WeeklySummaryMother.Monday,
            StringMother.Random("summary"),
            WeeklySummaryMother.InputHash(),
            this.Clock.Object);

        act.Should().Throw<WeeklySummaryExceptions.IdRequired>();
    }

    [Test]
    public void throw_class_required_when_the_class_is_empty()
    {
        Func<WeeklySummary> act = () => WeeklySummary.Create(
            GuidMother.Random(),
            Guid.Empty,
            WeeklySummaryMother.Monday,
            StringMother.Random("summary"),
            WeeklySummaryMother.InputHash(),
            this.Clock.Object);

        act.Should().Throw<WeeklySummaryExceptions.ClassRequired>();
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\n")]
    public void throw_content_required_when_the_content_is_blank(string content)
    {
        Func<WeeklySummary> act = () => this.CreateWithContent(content);

        act.Should().Throw<WeeklySummaryExceptions.ContentRequired>();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void throw_input_hash_required_when_the_fingerprint_is_blank(string inputHash)
    {
        Func<WeeklySummary> act = () => WeeklySummary.Create(
            GuidMother.Random(),
            GuidMother.Random(),
            WeeklySummaryMother.Monday,
            StringMother.Random("summary"),
            inputHash,
            this.Clock.Object);

        act.Should().Throw<WeeklySummaryExceptions.InputHashRequired>();
    }

    [Test]
    public void throw_week_start_invalid_for_any_other_day(
        [Range(FIRST_DAY_AFTER_MONDAY, DAYS_IN_A_WEEK - 1)] int daysAfterMonday)
    {
        DateOnly notMonday = WeeklySummaryMother.Monday.AddDays(daysAfterMonday);

        Func<WeeklySummary> act = () => WeeklySummary.Create(
            GuidMother.Random(),
            GuidMother.Random(),
            notMonday,
            StringMother.Random("summary"),
            WeeklySummaryMother.InputHash(),
            this.Clock.Object);

        act.Should().Throw<WeeklySummaryExceptions.WeekStartInvalid>();
    }

    [Test]
    public void be_rewritten_with_new_text_fingerprint_and_instant_and_stay_the_same_summary()
    {
        WeeklySummary summary = WeeklySummaryMother.With();
        Guid id = summary.Id;
        Guid classId = summary.ClassId;
        string content = StringMother.Random("rewritten");
        string inputHash = WeeklySummaryMother.InputHash();
        this.AdvanceClock(TimeSpan.FromMinutes(MINUTES_LATER));

        summary.Rewrite($" {content} ", inputHash, this.Clock.Object);

        summary.Content.Should().Be(content);
        summary.InputHash.Should().Be(inputHash);
        summary.GeneratedAt.Should().Be(Now.AddMinutes(MINUTES_LATER));
        summary.Id.Should().Be(id);
        summary.ClassId.Should().Be(classId);
        summary.WeekStart.Should().Be(WeeklySummaryMother.Monday);
    }

    [Test]
    public void refuse_to_be_rewritten_with_a_blank_text_and_keep_what_it_had()
    {
        WeeklySummary summary = WeeklySummaryMother.With();
        string content = summary.Content;
        string inputHash = summary.InputHash;

        Action act = () => summary.Rewrite("  ", WeeklySummaryMother.InputHash(), this.Clock.Object);

        act.Should().Throw<WeeklySummaryExceptions.ContentRequired>();
        summary.Content.Should().Be(content);
        summary.InputHash.Should().Be(inputHash);
    }

    [Test]
    public void refuse_to_be_rewritten_with_a_blank_fingerprint_and_keep_what_it_had()
    {
        WeeklySummary summary = WeeklySummaryMother.With();
        string content = summary.Content;
        string inputHash = summary.InputHash;

        Action act = () => summary.Rewrite(StringMother.Random("rewritten"), " ", this.Clock.Object);

        act.Should().Throw<WeeklySummaryExceptions.InputHashRequired>();
        summary.Content.Should().Be(content);
        summary.InputHash.Should().Be(inputHash);
    }

    [Test]
    public void say_whether_it_was_written_from_some_data()
    {
        string inputHash = WeeklySummaryMother.InputHash();
        WeeklySummary summary = WeeklySummaryMother.With(inputHash: inputHash);

        summary.WasWrittenFrom(inputHash).Should().BeTrue();
        summary.WasWrittenFrom(WeeklySummaryMother.InputHash()).Should().BeFalse();
    }

    [Test]
    public void find_the_monday_of_the_week_of_any_day([Range(0, DAYS_IN_A_WEEK - 1)] int daysAfterMonday)
    {
        DateOnly day = WeeklySummaryMother.Monday.AddDays(daysAfterMonday);

        WeeklySummary.WeekStartOf(day).Should().Be(WeeklySummaryMother.Monday);
    }

    [Test]
    public void put_a_sunday_in_the_week_that_started_the_monday_before()
    {
        DateOnly sunday = WeeklySummaryMother.Monday.AddDays(DAYS_IN_A_WEEK - 1);
        DateOnly nextMonday = WeeklySummaryMother.Monday.AddDays(DAYS_IN_A_WEEK);

        WeeklySummary.WeekStartOf(sunday).Should().Be(WeeklySummaryMother.Monday);
        WeeklySummary.WeekStartOf(nextMonday).Should().Be(nextMonday);
    }

    [Test]
    public void find_the_monday_across_the_change_of_year()
    {
        DateOnly newYearsDay = new(2026, 1, 1);
        DateOnly mondayOfThatWeek = new(2025, 12, 29);

        WeeklySummary.WeekStartOf(newYearsDay).Should().Be(mondayOfThatWeek);
    }

    private WeeklySummary CreateWithContent(string content)
        => WeeklySummary.Create(
            GuidMother.Random(),
            GuidMother.Random(),
            WeeklySummaryMother.Monday,
            content,
            WeeklySummaryMother.InputHash(),
            this.Clock.Object);
}
