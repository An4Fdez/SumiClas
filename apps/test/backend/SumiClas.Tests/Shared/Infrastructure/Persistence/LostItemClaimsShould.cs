namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using SumiClas.LostItems.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.LostItems.Domain;

/// <summary>
/// The claims of a lost item over a real PostgreSQL: a claim added to a stored item must be inserted, resolving them
/// must keep every status and the recovered mark, deleting the item must take its claims with it, and the database
/// must refuse a second pending claim of the same user even if the domain check is skipped.
/// </summary>
[TestFixture]
public class LostItemClaimsShould : UnitTestCase
{
    private const string PENDING_CLAIM_INDEX = "IX_LostItemClaims_LostItemId_ClaimantId";

    [Test]
    public async Task keep_a_claim_added_to_a_stored_item()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        LostItem stored = LostItemMother.ForClass(GuidMother.Random());
        Guid claimId = Guid.NewGuid();
        Guid claimantId = Guid.NewGuid();
        string message = StringMother.Random("detail");
        await SaveAsync(database, stored);
        await ChangeAsync(database, stored.Id, item => item.AddClaim(claimId, claimantId, message, this.Clock.Object));

        LostItem reloaded = await LoadAsync(database, stored.Id);

        LostItemClaim claim = reloaded.Claims.Should().ContainSingle().Subject;
        claim.Id.Should().Be(claimId);
        claim.ClaimantId.Should().Be(claimantId);
        claim.Message.Should().Be(message);
        claim.Status.Should().Be(ClaimStatus.Pending);
        claim.ResponseMessage.Should().BeNull();
        claim.CreatedAt.Should().Be(Now);
    }

    [Test]
    public async Task keep_the_statuses_the_recovered_mark_and_the_response_message_after_accepting_a_claim()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        LostItemClaim accepted = LostItemClaimMother.With();
        LostItemClaim other = LostItemClaimMother.With();
        LostItem stored = LostItemMother.ForClass(GuidMother.Random(), claims: [accepted, other]);
        string coordination = StringMother.Random("pickup");
        await SaveAsync(database, stored);
        await ChangeAsync(database, stored.Id, item => item.AcceptClaim(accepted.Id, coordination));

        LostItem reloaded = await LoadAsync(database, stored.Id);

        reloaded.Recovered.Should().BeTrue();
        LostItemClaim reloadedAccepted = reloaded.Claims.Single(claim => claim.Id == accepted.Id);
        reloadedAccepted.Status.Should().Be(ClaimStatus.Accepted);
        reloadedAccepted.ResponseMessage.Should().Be(coordination);
        reloaded.Claims.Single(claim => claim.Id == other.Id).Status.Should().Be(ClaimStatus.Rejected);
    }

    [Test]
    public async Task delete_the_claims_together_with_their_item_and_only_those()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        LostItem deleted = LostItemMother.ForClass(GuidMother.Random(), claims: [LostItemClaimMother.With()]);
        LostItem kept = LostItemMother.ForClass(GuidMother.Random(), claims: [LostItemClaimMother.With()]);
        await SaveAsync(database, deleted);
        await SaveAsync(database, kept);
        await using (SumiClasDbContext context = database.NewContext())
        {
            LostItem toDelete = await context.LostItems.SingleAsync(item => item.Id == deleted.Id);
            context.LostItems.Remove(toDelete);
            await context.SaveChangesAsync();
        }

        LostItem reloaded = await LoadAsync(database, kept.Id);

        reloaded.Claims.Should().ContainSingle();
        await using SumiClasDbContext reading = database.NewContext();
        reading.LostItems.Include(item => item.Claims).SelectMany(item => item.Claims)
            .Select(claim => claim.Id).ToList()
            .Should().Equal(kept.Claims.Select(claim => claim.Id));
    }

    [Test]
    public async Task raise_a_duplicate_key_naming_the_index_when_a_user_has_two_pending_claims_on_an_item()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid claimantId = Guid.NewGuid();
        LostItem duplicated = LostItemMother.ForClass(
            GuidMother.Random(),
            claims: [LostItemClaimMother.PendingOf(claimantId), LostItemClaimMother.PendingOf(claimantId)]);

        Func<Task> act = () => SaveAsync(database, duplicated);

        (await act.Should().ThrowAsync<DuplicateKeyException>())
            .Which.ConstraintName.Should().Be(PENDING_CLAIM_INDEX);
    }

    [Test]
    public async Task allow_a_new_pending_claim_after_the_previous_one_of_the_user_was_rejected()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid claimantId = Guid.NewGuid();
        LostItemClaim previous = LostItemClaimMother.PendingOf(claimantId);
        LostItem stored = LostItemMother.ForClass(GuidMother.Random(), claims: [previous]);
        await SaveAsync(database, stored);
        await ChangeAsync(database, stored.Id, item => item.RejectClaim(previous.Id));

        await ChangeAsync(
            database,
            stored.Id,
            item => item.AddClaim(Guid.NewGuid(), claimantId, StringMother.Random("again"), this.Clock.Object));

        LostItem reloaded = await LoadAsync(database, stored.Id);
        reloaded.Claims.Select(claim => claim.Status).Should()
            .BeEquivalentTo([ClaimStatus.Rejected, ClaimStatus.Pending]);
    }

    private static async Task SaveAsync(TestDatabase database, LostItem lostItem)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.LostItems.Add(lostItem);
        await context.SaveChangesAsync();
    }

    private static async Task ChangeAsync(TestDatabase database, Guid lostItemId, Action<LostItem> change)
    {
        await using SumiClasDbContext context = database.NewContext();
        LostItem lostItem = await context.LostItems.SingleAsync(candidate => candidate.Id == lostItemId);
        change(lostItem);
        await context.SaveChangesAsync();
    }

    private static async Task<LostItem> LoadAsync(TestDatabase database, Guid lostItemId)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await context.LostItems.SingleAsync(candidate => candidate.Id == lostItemId);
    }
}
