namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Domain;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class LostItemClaimPendingSummaryFinderByClassShould : LostItemsUnitTestCase
{
    private readonly LostItemClaimPendingSummaryFinderByClass.Finder _useCase;

    public LostItemClaimPendingSummaryFinderByClassShould()
    {
        this._useCase = new LostItemClaimPendingSummaryFinderByClass.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task count_only_the_pending_claims_on_the_open_class_items_of_the_user()
    {
        Guid classId = GuidMother.Random();
        Guid current = GuidMother.Random();
        LostItem firstOwn = LostItemMother.ForClass(
            classId,
            authorId: current,
            claims: [LostItemClaimMother.With(), LostItemClaimMother.With(status: ClaimStatus.Rejected)]);
        LostItem secondOwn = LostItemMother.ForClass(classId, authorId: current, claims: [LostItemClaimMother.With()]);
        LostItem recovered = LostItemMother.ForClass(
            classId,
            authorId: current,
            recovered: true,
            claims: [LostItemClaimMother.With()]);
        LostItem ofSomeoneElse = LostItemMother.ForClass(classId, claims: [LostItemClaimMother.With()]);
        LostItem ofAnotherClass = LostItemMother.ForClass(
            GuidMother.Random(),
            authorId: current,
            claims: [LostItemClaimMother.With()]);
        this.ShouldResolveCurrentUser(current);
        this.ShouldFindClass(classId);
        this.ShouldAllowClassAccess(current, classId);
        this.ShouldSearchLostItems(firstOwn, secondOwn, recovered, ofSomeoneElse, ofAnotherClass);

        LostItemClaimPendingSummaryResponse result = await this._useCase.ExecuteAsync(
            LostItemClaimPendingSummaryFinderByClassQueryMother.WithClassId(classId));

        result.Count.Should().Be(firstOwn.CountPendingClaims() + secondOwn.CountPendingClaims());
    }

    [Test]
    public async Task count_none_when_the_user_has_no_open_items_with_claims()
    {
        Guid classId = GuidMother.Random();
        Guid current = GuidMother.Random();
        this.ShouldResolveCurrentUser(current);
        this.ShouldFindClass(classId);
        this.ShouldAllowClassAccess(current, classId);
        this.ShouldSearchNoLostItems();

        LostItemClaimPendingSummaryResponse result = await this._useCase.ExecuteAsync(
            LostItemClaimPendingSummaryFinderByClassQueryMother.WithClassId(classId));

        result.Count.Should().Be(0);
    }

    [Test]
    public async Task throw_class_access_forbidden_without_reading_when_the_user_is_not_a_member()
    {
        Guid classId = GuidMother.Random();
        Guid current = GuidMother.Random();
        this.ShouldResolveCurrentUser(current);
        this.ShouldFindClass(classId);
        this.ShouldDenyClassAccess(current, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemClaimPendingSummaryFinderByClassQueryMother.WithClassId(classId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_class_not_found_without_reading_when_the_class_does_not_exist()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemClaimPendingSummaryFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_unauthenticated_without_reading_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemClaimPendingSummaryFinderByClassQueryMother.WithClassId(GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveSearched();
    }
}
