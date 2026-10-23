namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using SumiClas.Announcements.Domain;
using SumiClas.Announcements.Infrastructure;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.Announcements.Domain;

/// <summary>
/// The cursor pagination over a real PostgreSQL: walking the pages must give every row once and in the order that the
/// in-memory merge of <see cref="Pagination"/> assumes, even when many rows share the same instant.
/// </summary>
[TestFixture]
public class KeysetPaginationShould
{
    private const int ANNOUNCEMENTS = 40;
    private const int PAGE_SIZE = 7;
    private const int RANDOM_IDS = 200;

    [TestCase(true)]
    [TestCase(false)]
    public async Task walk_every_announcement_once_and_in_the_order_of_the_pagination(bool allAtTheSameInstant)
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        List<Announcement> saved = await SaveAnnouncementsAsync(database, allAtTheSameInstant);

        Walk walk = await WalkAsync(database);

        walk.Items.Select(announcement => announcement.Id).Should().Equal(
            InPaginationOrder(saved).Select(announcement => announcement.Id));
        walk.PageSizes.Should().OnlyContain(size => size <= PAGE_SIZE);
        walk.PageSizes.Should().HaveCount((int)Math.Ceiling((double)(ANNOUNCEMENTS + 1) / PAGE_SIZE));
    }

    [Test]
    public async Task return_nothing_after_the_smallest_id_of_an_instant_when_walking_newest_first()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        await SaveAnnouncementsAsync(database, allAtTheSameInstant: true);

        IReadOnlyCollection<Announcement> lastPage = await PageAsync(
            database,
            new KeysetBoundary<DateTimeOffset>(UnitTestCase.Now, Guid.Empty));

        lastPage.Should().BeEmpty("no row sorts after the smallest possible id of the last instant.");
    }

    [Test]
    public async Task order_the_ids_in_the_database_as_the_comparer_of_the_pagination_does()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        List<Guid> ids = Enumerable.Range(0, RANDOM_IDS).Select(_ => Guid.NewGuid()).ToList();
        await using (SumiClasDbContext writer = database.NewContext())
        {
            writer.Announcements.AddRange(ids.Select(id => AnnouncementMother.WithId(id)));
            await writer.SaveChangesAsync();
        }

        await using SumiClasDbContext reader = database.NewContext();
        List<Guid> inTheDatabase = await reader.Announcements.OrderBy(announcement => announcement.Id)
            .Select(announcement => announcement.Id)
            .ToListAsync();

        inTheDatabase.Should().Equal(ids.Order(Pagination.IdComparer));
    }

    private static IEnumerable<Announcement> InPaginationOrder(IEnumerable<Announcement> announcements)
        => announcements
            .OrderByDescending(announcement => announcement.CreatedAt)
            .ThenByDescending(announcement => announcement.Id, Pagination.IdComparer);

    private static async Task<List<Announcement>> SaveAnnouncementsAsync(
        TestDatabase database,
        bool allAtTheSameInstant)
    {
        List<Announcement> announcements = Enumerable.Range(0, ANNOUNCEMENTS)
            .Select(index => AnnouncementMother.With(
                UnitTestCase.Now.AddMinutes(allAtTheSameInstant ? 0 : index / 4)))
            .ToList();
        await using SumiClasDbContext context = database.NewContext();
        context.Announcements.AddRange(announcements);
        await context.SaveChangesAsync();
        return announcements;
    }

    private static async Task<Walk> WalkAsync(TestDatabase database)
    {
        List<Announcement> walked = [];
        List<int> pageSizes = [];
        KeysetBoundary<DateTimeOffset>? boundary = null;
        for (int requests = 0; requests <= ANNOUNCEMENTS; requests++)
        {
            IReadOnlyCollection<Announcement> page = await PageAsync(database, boundary);
            walked.AddRange(page);
            pageSizes.Add(page.Count);
            if (page.Count < PAGE_SIZE)
            {
                return new Walk(walked, pageSizes);
            }

            Announcement last = page.Last();
            boundary = new KeysetBoundary<DateTimeOffset>(last.CreatedAt, last.Id);
        }

        return new Walk(walked, pageSizes);
    }

    private sealed record Walk(List<Announcement> Items, List<int> PageSizes);

    private static async Task<IReadOnlyCollection<Announcement>> PageAsync(
        TestDatabase database,
        KeysetBoundary<DateTimeOffset>? boundary)
    {
        await using SumiClasDbContext context = database.NewContext();
        AnnouncementRepository repository = new(context);
        return await repository.SearchAsync(
            new AnnouncementSpecification(page: new KeysetPage<DateTimeOffset>(PAGE_SIZE, boundary)));
    }
}
