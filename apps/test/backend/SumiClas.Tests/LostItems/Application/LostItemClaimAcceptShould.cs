namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LostItemClaimAcceptShould : LostItemsUnitTestCase
{
    private readonly LostItemClaimAccept.Accept _useCase;

    public LostItemClaimAcceptShould()
    {
        this._useCase = new LostItemClaimAccept.Accept(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task accept_the_claim_for_the_author_recovering_the_item_and_rejecting_the_other_pending_claims()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        LostItemClaim accepted = LostItemClaimMother.With();
        LostItemClaim other = LostItemClaimMother.With();
        LostItem lostItem = LostItemMother.ForClass(classId, claims: [accepted, other]);
        string coordination = StringMother.Random("pickup");
        this.ShouldResolveCurrentUser(lostItem.AuthorId);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        await this._useCase.ExecuteAsync(LostItemClaimAcceptCommandMother.With(lostItem.Id, accepted.Id, coordination));

        accepted.Status.Should().Be(ClaimStatus.Accepted);
        accepted.ResponseMessage.Should().Be(coordination);
        other.Status.Should().Be(ClaimStatus.Rejected);
        lostItem.Recovered.Should().BeTrue();
        this.ShouldHaveEnsuredSchoolYearIsActive(schoolYearId);
    }

    [Test]
    public async Task accept_the_claim_of_a_school_item_without_asking_for_any_class()
    {
        LostItemClaim claim = LostItemClaimMother.With();
        LostItem lostItem = LostItemMother.With(claims: [claim]);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);
        this.ShouldFetchLostItems(lostItem);

        await this._useCase.ExecuteAsync(LostItemClaimAcceptCommandMother.With(lostItem.Id, claim.Id));

        claim.Status.Should().Be(ClaimStatus.Accepted);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_not_found_when_the_item_does_not_exist()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFetchNoLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemClaimAcceptCommandMother.With(GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<LostItemExceptions.NotFound>();
    }

    [Test]
    public async Task throw_claim_resolve_forbidden_and_keep_everything_when_the_user_is_not_the_author()
    {
        LostItemClaim claim = LostItemClaimMother.With();
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [claim]);
        this.ShouldResolveCurrentUser(claim.ClaimantId);
        this.ShouldFetchLostItems(lostItem);

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimAcceptCommandMother.With(lostItem.Id, claim.Id));

        await act.Should().ThrowAsync<LostItemExceptions.ClaimResolveForbidden>();
        claim.Status.Should().Be(ClaimStatus.Pending);
        lostItem.Recovered.Should().BeFalse();
    }

    [Test]
    public async Task throw_school_year_archived_and_keep_everything_when_the_school_year_is_archived()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        LostItemClaim claim = LostItemClaimMother.With();
        LostItem lostItem = LostItemMother.ForClass(classId, claims: [claim]);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimAcceptCommandMother.With(lostItem.Id, claim.Id));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        claim.Status.Should().Be(ClaimStatus.Pending);
        lostItem.Recovered.Should().BeFalse();
    }

    [Test]
    public async Task throw_unauthenticated_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemClaimAcceptCommandMother.With(GuidMother.Random(), GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
    }
}
