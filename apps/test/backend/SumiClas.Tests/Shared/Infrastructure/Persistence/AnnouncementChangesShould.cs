namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using SumiClas.Announcements.Domain;
using SumiClas.Announcements.Infrastructure;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.Announcements.Domain;
using SumiClas.Tests.Shared;

/// <summary>
/// The changes an author makes to an announcement over a real PostgreSQL: the update time comes back as saved,
/// updating a stored announcement deletes the rows of its read receipts, and deleting it deletes them too.
/// </summary>
[TestFixture]
public class AnnouncementChangesShould : UnitTestCase
{
    private static readonly TimeSpan ONE_HOUR = TimeSpan.FromHours(1);

    private const string RECEIPT_COUNT = "SELECT count(*)::int AS \"Value\" FROM \"AnnouncementReadReceipts\"";

    [Test]
    public async Task give_back_the_update_time_that_was_saved_and_none_when_it_was_never_updated()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Announcement updated = AnnouncementMother.With(Now, updatedAt: Now + ONE_HOUR);
        Announcement untouched = AnnouncementMother.With(Now);
        await SaveAsync(database, updated);
        await SaveAsync(database, untouched);

        Announcement reloadedUpdated = await LoadAsync(database, updated.Id);
        Announcement reloadedUntouched = await LoadAsync(database, untouched.Id);

        reloadedUpdated.UpdatedAt.Should().Be(Now + ONE_HOUR);
        reloadedUntouched.UpdatedAt.Should().BeNull();
    }

    [Test]
    public async Task keep_the_new_content_and_delete_the_receipts_of_a_stored_announcement_that_is_updated()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Announcement stored = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        stored.MarkAsReadBy(GuidMother.Random(), this.Clock.Object);
        Announcement other = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        await SaveAsync(database, stored);
        await SaveAsync(database, other);
        string title = StringMother.Random("New title");
        string body = StringMother.Random("New body");
        this.AdvanceClock(ONE_HOUR);
        await ChangeAsync(
            database,
            stored.Id,
            announcement => announcement.Update(title, body, Priority.High, null, this.Clock.Object));

        Announcement reloaded = await LoadAsync(database, stored.Id);
        Announcement reloadedOther = await LoadAsync(database, other.Id);

        reloaded.Title.Should().Be(title);
        reloaded.Body.Should().Be(body);
        reloaded.Priority.Should().Be(Priority.High);
        reloaded.UpdatedAt.Should().Be(Now + ONE_HOUR);
        reloaded.ReadReceipts.Should().BeEmpty();
        reloadedOther.ReadReceipts.Should().ContainSingle();
        (await CountReceiptsAsync(database)).Should().Be(1);
    }

    [Test]
    public async Task delete_the_announcement_with_its_receipts_and_keep_the_others()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Announcement removed = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        removed.MarkAsReadBy(GuidMother.Random(), this.Clock.Object);
        Announcement kept = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        await SaveAsync(database, removed);
        await SaveAsync(database, kept);
        await RemoveAsync(database, removed.Id);

        await using SumiClasDbContext context = database.NewContext();
        List<Guid> remaining = await context.Set<Announcement>().Select(announcement => announcement.Id).ToListAsync();

        remaining.Should().Equal(kept.Id);
        (await CountReceiptsAsync(database)).Should().Be(kept.ReadReceipts.Count);
    }

    private static async Task SaveAsync(TestDatabase database, Announcement announcement)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.Set<Announcement>().Add(announcement);
        await context.SaveChangesAsync();
    }

    private static async Task<Announcement> LoadAsync(TestDatabase database, Guid id)
    {
        await using SumiClasDbContext context = database.NewContext();
        return (await new AnnouncementRepository(context).GetByIdAsync(id))!;
    }

    private static async Task ChangeAsync(TestDatabase database, Guid id, Action<Announcement> change)
    {
        await using SumiClasDbContext context = database.NewContext();
        Announcement announcement = (await new AnnouncementRepository(context).GetByIdAsync(id))!;
        change(announcement);
        await context.SaveChangesAsync();
    }

    private static async Task RemoveAsync(TestDatabase database, Guid id)
    {
        await using SumiClasDbContext context = database.NewContext();
        AnnouncementRepository repository = new(context);
        Announcement announcement = (await repository.GetByIdAsync(id))!;
        repository.Remove(announcement);
        await context.SaveChangesAsync();
    }

    private static async Task<int> CountReceiptsAsync(TestDatabase database)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await context.Database.SqlQueryRaw<int>(RECEIPT_COUNT).SingleAsync();
    }
}
