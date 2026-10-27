namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Application;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class LostItemClaimFinderByItemShould : LostItemsUnitTestCase
{
    private const int OLDEST_MINUTES = 1;
    private const int NEWEST_MINUTES = 2;

    private readonly LostItemClaimFinderByItem.Finder _useCase;

    public LostItemClaimFinderByItemShould()
    {
        this._useCase = new LostItemClaimFinderByItem.Finder(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task return_every_claim_to_the_author_newest_first_with_the_name_and_role_of_each_claimant()
    {
        Guid classId = GuidMother.Random();
        LostItemClaim oldest = LostItemClaimMother.With(createdAt: Now.AddMinutes(OLDEST_MINUTES));
        LostItemClaim newest = LostItemClaimMother.With(
            status: ClaimStatus.Rejected,
            createdAt: Now.AddMinutes(NEWEST_MINUTES));
        LostItem lostItem = LostItemMother.ForClass(classId, claims: [oldest, newest]);
        string oldestName = StringMother.Random("oldest");
        string newestName = StringMother.Random("newest");
        this.ShouldResolveCurrentUser(lostItem.AuthorId);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldAllowClassAccess(lostItem.AuthorId, classId);
        this.ShouldFindClassMembers(
            UserClassMemberResponseMother.Participant(oldest.ClaimantId, oldestName),
            UserClassMemberResponseMother.Representative(newest.ClaimantId, newestName));

        IReadOnlyCollection<LostItemClaimResponse> result = await this._useCase.ExecuteAsync(
            LostItemClaimFinderByItemQueryMother.With(lostItem.Id));

        result.Select(claim => claim.Id).Should().Equal(newest.Id, oldest.Id);
        LostItemClaimResponse first = result.First();
        first.ClaimantId.Should().Be(newest.ClaimantId);
        first.ClaimantName.Should().Be(newestName);
        first.ClaimantRole.Should().Be("ClassRepresentative");
        first.Message.Should().Be(newest.Message);
        first.Status.Should().Be("Rejected");
        first.CreatedAt.Should().Be(newest.CreatedAt);
        result.Should().OnlyContain(claim => !claim.IsMine);
        result.Last().ClaimantRole.Should().Be("Participant");
    }

    [Test]
    public async Task return_only_the_claims_of_the_user_to_a_member_who_is_not_the_author()
    {
        Guid classId = GuidMother.Random();
        Guid current = GuidMother.Random();
        string currentName = StringMother.Random("me");
        LostItemClaim mine = LostItemClaimMother.PendingOf(current);
        LostItemClaim someoneElse = LostItemClaimMother.PendingOf(GuidMother.Random());
        LostItem lostItem = LostItemMother.ForClass(classId, claims: [someoneElse, mine]);
        this.ShouldResolveCurrentUser(current);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldAllowClassAccess(current, classId);
        this.ShouldFindClassMembers(
            UserClassMemberResponseMother.Participant(current, currentName),
            UserClassMemberResponseMother.Participant(someoneElse.ClaimantId, StringMother.Random("other")));

        IReadOnlyCollection<LostItemClaimResponse> result = await this._useCase.ExecuteAsync(
            LostItemClaimFinderByItemQueryMother.With(lostItem.Id));

        LostItemClaimResponse claim = result.Should().ContainSingle().Subject;
        claim.Id.Should().Be(mine.Id);
        claim.ClaimantName.Should().Be(currentName);
        claim.IsMine.Should().BeTrue();
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserClassMemberFinderByIds.Query>(query => query.ClassId == classId
                    && query.UserIds.Count == result.Count
                    && query.UserIds.Contains(current)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task show_the_response_message_only_on_an_accepted_claim()
    {
        Guid classId = GuidMother.Random();
        Guid current = GuidMother.Random();
        string coordination = StringMother.Random("pickup");
        LostItemClaim accepted = LostItemClaimMother.With(
            claimantId: current,
            status: ClaimStatus.Accepted,
            responseMessage: coordination,
            createdAt: Now.AddMinutes(NEWEST_MINUTES));
        LostItemClaim rejected = LostItemClaimMother.With(
            claimantId: current,
            status: ClaimStatus.Rejected,
            responseMessage: StringMother.Random("hidden"),
            createdAt: Now.AddMinutes(OLDEST_MINUTES));
        LostItem lostItem = LostItemMother.ForClass(classId, claims: [rejected, accepted]);
        this.ShouldResolveCurrentUser(current);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldAllowClassAccess(current, classId);
        this.ShouldFindClassMembers();

        IReadOnlyCollection<LostItemClaimResponse> result = await this._useCase.ExecuteAsync(
            LostItemClaimFinderByItemQueryMother.With(lostItem.Id));

        result.Select(claim => claim.ResponseMessage).Should().Equal(coordination, null);
    }

    [Test]
    public async Task return_an_empty_name_and_no_role_when_the_claimant_is_no_longer_a_member()
    {
        Guid classId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId, claims: [LostItemClaimMother.With()]);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldAllowClassAccess(lostItem.AuthorId, classId);
        this.ShouldFindClassMembers();

        IReadOnlyCollection<LostItemClaimResponse> result = await this._useCase.ExecuteAsync(
            LostItemClaimFinderByItemQueryMother.With(lostItem.Id));

        LostItemClaimResponse claim = result.Should().ContainSingle().Subject;
        claim.ClaimantName.Should().BeEmpty();
        claim.ClaimantRole.Should().BeNull();
    }

    [Test]
    public async Task throw_not_found_when_the_item_does_not_exist()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFetchNoLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimFinderByItemQueryMother.With(GuidMother.Random()));

        await act.Should().ThrowAsync<LostItemExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_claim_class_required_when_the_item_belongs_to_the_school()
    {
        LostItem lostItem = LostItemMother.With();
        this.ShouldResolveCurrentUser(lostItem.AuthorId);
        this.ShouldFetchLostItems(lostItem);

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimFinderByItemQueryMother.With(lostItem.Id));

        await act.Should().ThrowAsync<LostItemExceptions.ClaimClassRequired>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_reading_the_members_when_the_user_is_not_a_member()
    {
        Guid classId = GuidMother.Random();
        Guid current = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId, claims: [LostItemClaimMother.With()]);
        this.ShouldResolveCurrentUser(current);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldDenyClassAccess(current, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimFinderByItemQueryMother.With(lostItem.Id));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserClassMemberFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_unauthenticated_without_looking_for_the_item_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimFinderByItemQueryMother.With(GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Repository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
