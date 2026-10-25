namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.WeeklySummaries.Domain;
using SumiClas.WeeklySummaries.Domain;
using SumiClas.WeeklySummaries.Infrastructure;

/// <summary>The summaries over a real PostgreSQL: they come back whole and a class has one per week.</summary>
[TestFixture]
public class WeeklySummaryPersistenceShould
{
    private const int ONE_WEEK_DAYS = 7;
    private const int ONE_SECOND = 1;
    private const int ONE_HOUR = 1;
    private const string WEEK_CLASS_INDEX = "IX_WeeklySummaries_ClassId_WeekStart";

    private static readonly DateOnly NextMonday = WeeklySummaryMother.Monday.AddDays(ONE_WEEK_DAYS);

    [Test]
    public async Task give_back_everything_that_was_saved()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        WeeklySummary saved = WeeklySummaryMother.With();
        await SaveAsync(database, saved);

        IReadOnlyCollection<WeeklySummary> found = await SearchAsync(
            database,
            WeeklySummarySpecificationMother.WithId(saved.Id));

        found.Should().ContainSingle().Which.Should().BeEquivalentTo(saved);
    }

    [TestCaseSource(nameof(Filters))]
    public async Task select_only_the_summary_the_filter_describes(
        Func<WeeklySummary, WeeklySummarySpecification> filterOf)
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        WeeklySummary wanted = WeeklySummaryMother.With();
        WeeklySummary other = WeeklySummaryMother.With(
            weekStart: NextMonday,
            generatedAt: UnitTestCase.Now.AddSeconds(ONE_SECOND));
        await SaveAsync(database, wanted);
        await SaveAsync(database, other);

        IReadOnlyCollection<WeeklySummary> found = await SearchAsync(database, filterOf(wanted));

        found.Select(summary => summary.Id).Should().Equal(wanted.Id);
    }

    [Test]
    public async Task raise_a_duplicate_key_naming_the_index_when_a_class_gets_two_summaries_for_the_same_week()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid classId = GuidMother.Random();
        await SaveAsync(database, WeeklySummaryMother.With(classId: classId));

        Func<Task> act = () => SaveAsync(database, WeeklySummaryMother.With(classId: classId));

        (await act.Should().ThrowAsync<DuplicateKeyException>()).Which.ConstraintName.Should().Be(WEEK_CLASS_INDEX);
    }

    [Test]
    public async Task save_a_summary_for_another_week_of_the_same_class_and_for_the_same_week_of_another_class()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid classId = GuidMother.Random();
        await SaveAsync(database, WeeklySummaryMother.With(classId: classId));

        Func<Task> otherWeek = () => SaveAsync(
            database,
            WeeklySummaryMother.With(classId: classId, weekStart: NextMonday));
        Func<Task> otherClass = () => SaveAsync(database, WeeklySummaryMother.With());

        await otherWeek.Should().NotThrowAsync();
        await otherClass.Should().NotThrowAsync();
    }

    [Test]
    public async Task keep_the_rewritten_text_fingerprint_and_instant_in_the_same_row()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        WeeklySummary saved = WeeklySummaryMother.With();
        await SaveAsync(database, saved);
        string content = StringMother.Random("rewritten");
        string inputHash = WeeklySummaryMother.InputHash();
        DateTimeOffset rewrittenAt = UnitTestCase.Now.AddHours(ONE_HOUR);
        await RewriteAsync(database, saved.Id, content, inputHash, rewrittenAt);

        IReadOnlyCollection<WeeklySummary> all = await SearchAsync(
            database,
            WeeklySummarySpecificationMother.Unfiltered());

        WeeklySummary rewritten = all.Should().ContainSingle().Which;
        rewritten.Id.Should().Be(saved.Id);
        rewritten.Content.Should().Be(content);
        rewritten.InputHash.Should().Be(inputHash);
        rewritten.GeneratedAt.Should().Be(rewrittenAt);
    }

    private static IEnumerable<TestCaseData> Filters()
    {
        yield return Filter("id", wanted => WeeklySummarySpecificationMother.WithId(wanted.Id));
        yield return Filter("class", wanted => WeeklySummarySpecificationMother.OfClass(wanted.ClassId));
        yield return Filter("week", wanted => WeeklySummarySpecificationMother.OfWeek(wanted.WeekStart));
        yield return Filter("content", wanted => WeeklySummarySpecificationMother.WithContent(wanted.Content));
        yield return Filter("input_hash", wanted => WeeklySummarySpecificationMother.WithInputHash(wanted.InputHash));
        yield return Filter("generated_at", wanted => WeeklySummarySpecificationMother.GeneratedAt(wanted.GeneratedAt));
    }

    private static TestCaseData Filter(string name, Func<WeeklySummary, WeeklySummarySpecification> filterOf)
        => new TestCaseData(filterOf).SetName($"the_{name}");

    private static async Task SaveAsync(TestDatabase database, WeeklySummary summary)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.WeeklySummaries.Add(summary);
        await context.SaveChangesAsync();
    }

    private static async Task RewriteAsync(
        TestDatabase database,
        Guid id,
        string content,
        string inputHash,
        DateTimeOffset at)
    {
        Mock<IClock> clock = new();
        clock.Setup(source => source.UtcNow).Returns(at);
        await using SumiClasDbContext context = database.NewContext();
        WeeklySummary summary = (await new WeeklySummaryRepository(context)
            .SearchAsync(WeeklySummarySpecificationMother.WithId(id))).Single();
        summary.Rewrite(content, inputHash, clock.Object);
        await context.SaveChangesAsync();
    }

    private static async Task<IReadOnlyCollection<WeeklySummary>> SearchAsync(
        TestDatabase database,
        WeeklySummarySpecification specification)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await new WeeklySummaryRepository(context).SearchAsync(specification);
    }
}
