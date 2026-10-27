namespace SumiClas.Tests.LostItems.Domain;

using AwesomeAssertions;
using SumiClas.LostItems.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LostItemShould : UnitTestCase
{
    private const int LATER_MINUTES = 1;
    private const int LATEST_MINUTES = 2;

    [Test]
    public void throw_school_required_when_a_school_item_has_no_school()
    {
        Func<LostItem> act = () => this.CreateWithScope(ItemScope.School, GuidMother.Random(), null);

        act.Should().Throw<LostItemExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void throw_school_required_when_a_school_item_has_an_empty_school()
    {
        Func<LostItem> act = () => this.CreateWithScope(ItemScope.School, GuidMother.Random(), Guid.Empty);

        act.Should().Throw<LostItemExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void keep_only_the_class_when_the_item_is_of_the_class_scope()
    {
        Guid classId = GuidMother.Random();

        LostItem lostItem = this.CreateWithScope(ItemScope.Class, classId, GuidMother.Random());

        lostItem.ClassId.Should().Be(classId);
        lostItem.SchoolId.Should().BeNull();
    }

    [Test]
    public void keep_only_the_school_when_the_item_is_of_the_school_scope()
    {
        Guid schoolId = GuidMother.Random();

        LostItem lostItem = this.CreateWithScope(ItemScope.School, GuidMother.Random(), schoolId);

        lostItem.SchoolId.Should().Be(schoolId);
        lostItem.ClassId.Should().BeNull();
    }

    [Test]
    public void add_a_pending_claim_with_the_trimmed_message_and_the_date_of_the_clock()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random());
        Guid claimId = GuidMother.Random();
        Guid claimantId = GuidMother.Random();
        string message = StringMother.Random("detail");

        lostItem.AddClaim(claimId, claimantId, $"  {message}  ", this.Clock.Object);

        LostItemClaim claim = lostItem.Claims.Should().ContainSingle().Subject;
        claim.Id.Should().Be(claimId);
        claim.ClaimantId.Should().Be(claimantId);
        claim.Message.Should().Be(message);
        claim.Status.Should().Be(ClaimStatus.Pending);
        claim.ResponseMessage.Should().BeNull();
        claim.CreatedAt.Should().Be(Now);
    }

    [Test]
    public void throw_claim_id_required_when_the_claim_has_an_empty_id()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random());

        Action act = () => lostItem.AddClaim(Guid.Empty, GuidMother.Random(), StringMother.Random("detail"), this.Clock.Object);

        act.Should().Throw<LostItemExceptions.ClaimIdRequired>();
    }

    [Test]
    public void throw_claimant_required_when_the_claimant_is_empty()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random());

        Action act = () => lostItem.AddClaim(GuidMother.Random(), Guid.Empty, StringMother.Random("detail"), this.Clock.Object);

        act.Should().Throw<LostItemExceptions.ClaimantRequired>();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void throw_claim_message_required_when_the_message_is_blank(string message)
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random());

        Action act = () => lostItem.AddClaim(GuidMother.Random(), GuidMother.Random(), message, this.Clock.Object);

        act.Should().Throw<LostItemExceptions.ClaimMessageRequired>();
        lostItem.Claims.Should().BeEmpty();
    }

    [Test]
    public void throw_claim_own_item_forbidden_when_the_author_claims_their_own_item()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random());

        Action act = () => lostItem.AddClaim(GuidMother.Random(), lostItem.AuthorId, StringMother.Random("detail"), this.Clock.Object);

        act.Should().Throw<LostItemExceptions.ClaimOwnItemForbidden>();
        lostItem.Claims.Should().BeEmpty();
    }

    [Test]
    public void throw_already_recovered_when_the_item_is_recovered()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), recovered: true);

        Action act = () => lostItem.AddClaim(GuidMother.Random(), GuidMother.Random(), StringMother.Random("detail"), this.Clock.Object);

        act.Should().Throw<LostItemExceptions.AlreadyRecovered>();
        lostItem.Claims.Should().BeEmpty();
    }

    [Test]
    public void throw_claim_already_exists_when_the_claimant_has_a_pending_claim()
    {
        Guid claimantId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(
            GuidMother.Random(),
            claims: [LostItemClaimMother.PendingOf(claimantId)]);

        Action act = () => lostItem.AddClaim(GuidMother.Random(), claimantId, StringMother.Random("detail"), this.Clock.Object);

        act.Should().Throw<LostItemExceptions.ClaimAlreadyExists>();
        lostItem.Claims.Should().ContainSingle();
    }

    [Test]
    public void add_a_new_claim_when_the_previous_ones_of_the_claimant_were_resolved()
    {
        Guid claimantId = GuidMother.Random();
        LostItemClaim rejected = LostItemClaimMother.With(claimantId: claimantId, status: ClaimStatus.Rejected);
        LostItemClaim ofSomeoneElse = LostItemClaimMother.PendingOf(GuidMother.Random());
        Guid newClaimId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [rejected, ofSomeoneElse]);

        lostItem.AddClaim(newClaimId, claimantId, StringMother.Random("detail"), this.Clock.Object);

        lostItem.Claims.Select(claim => claim.Id).Should().BeEquivalentTo([rejected.Id, ofSomeoneElse.Id, newClaimId]);
    }

    [Test]
    public void accept_a_claim_marking_the_item_recovered_and_rejecting_only_the_other_pending_claims()
    {
        LostItemClaim accepted = LostItemClaimMother.With();
        LostItemClaim otherPending = LostItemClaimMother.With();
        LostItemClaim alreadyRejected = LostItemClaimMother.With(status: ClaimStatus.Rejected);
        LostItem lostItem = LostItemMother.ForClass(
            GuidMother.Random(),
            claims: [otherPending, accepted, alreadyRejected]);
        string coordination = StringMother.Random("pickup");

        lostItem.AcceptClaim(accepted.Id, $"  {coordination}  ");

        accepted.Status.Should().Be(ClaimStatus.Accepted);
        accepted.ResponseMessage.Should().Be(coordination);
        otherPending.Status.Should().Be(ClaimStatus.Rejected);
        otherPending.ResponseMessage.Should().BeNull();
        alreadyRejected.Status.Should().Be(ClaimStatus.Rejected);
        lostItem.Recovered.Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("   ")]
    public void accept_a_claim_without_response_message_when_it_is_missing_or_blank(string? responseMessage)
    {
        LostItemClaim claim = LostItemClaimMother.With();
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [claim]);

        lostItem.AcceptClaim(claim.Id, responseMessage);

        claim.ResponseMessage.Should().BeNull();
    }

    [TestCase(ClaimStatus.Accepted)]
    [TestCase(ClaimStatus.Rejected)]
    public void throw_claim_not_pending_without_changing_anything_when_accepting_a_resolved_claim(ClaimStatus status)
    {
        LostItemClaim resolved = LostItemClaimMother.With(status: status);
        LostItemClaim pending = LostItemClaimMother.With();
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [resolved, pending]);

        Action act = () => lostItem.AcceptClaim(resolved.Id, null);

        act.Should().Throw<LostItemExceptions.ClaimNotPending>();
        pending.Status.Should().Be(ClaimStatus.Pending);
        lostItem.Recovered.Should().BeFalse();
    }

    [Test]
    public void throw_claim_not_found_when_accepting_a_claim_the_item_does_not_have()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [LostItemClaimMother.With()]);

        Action act = () => lostItem.AcceptClaim(GuidMother.Random(), null);

        act.Should().Throw<LostItemExceptions.ClaimNotFound>();
    }

    [Test]
    public void reject_a_claim_leaving_the_other_claims_and_the_item_as_they_were()
    {
        LostItemClaim rejected = LostItemClaimMother.With();
        LostItemClaim other = LostItemClaimMother.With();
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [rejected, other]);

        lostItem.RejectClaim(rejected.Id);

        rejected.Status.Should().Be(ClaimStatus.Rejected);
        other.Status.Should().Be(ClaimStatus.Pending);
        lostItem.Recovered.Should().BeFalse();
    }

    [TestCase(ClaimStatus.Accepted)]
    [TestCase(ClaimStatus.Rejected)]
    public void throw_claim_not_pending_when_rejecting_a_resolved_claim(ClaimStatus status)
    {
        LostItemClaim resolved = LostItemClaimMother.With(status: status);
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [resolved]);

        Action act = () => lostItem.RejectClaim(resolved.Id);

        act.Should().Throw<LostItemExceptions.ClaimNotPending>();
        resolved.Status.Should().Be(status);
    }

    [Test]
    public void throw_claim_not_found_when_rejecting_a_claim_the_item_does_not_have()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), claims: [LostItemClaimMother.With()]);

        Action act = () => lostItem.RejectClaim(GuidMother.Random());

        act.Should().Throw<LostItemExceptions.ClaimNotFound>();
    }

    [Test]
    public void count_only_the_pending_claims()
    {
        LostItemClaim[] pending = [LostItemClaimMother.With(), LostItemClaimMother.With()];
        LostItem lostItem = LostItemMother.ForClass(
            GuidMother.Random(),
            claims:
            [
                .. pending,
                LostItemClaimMother.With(status: ClaimStatus.Accepted),
                LostItemClaimMother.With(status: ClaimStatus.Rejected),
            ]);

        lostItem.CountPendingClaims().Should().Be(pending.Length);
    }

    [Test]
    public void give_the_status_of_the_latest_claim_of_a_claimant_and_nothing_for_who_never_claimed()
    {
        Guid claimantId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(
            GuidMother.Random(),
            claims:
            [
                LostItemClaimMother.With(claimantId: claimantId, status: ClaimStatus.Pending, createdAt: Now.AddMinutes(LATER_MINUTES)),
                LostItemClaimMother.With(claimantId: claimantId, status: ClaimStatus.Rejected, createdAt: Now),
                LostItemClaimMother.With(status: ClaimStatus.Accepted, createdAt: Now.AddMinutes(LATEST_MINUTES)),
            ]);

        lostItem.LatestClaimStatusOf(claimantId).Should().Be(ClaimStatus.Pending);
        lostItem.LatestClaimStatusOf(GuidMother.Random()).Should().BeNull();
    }

    private LostItem CreateWithScope(ItemScope scope, Guid? classId, Guid? schoolId)
        => LostItem.Create(
            GuidMother.Random(),
            scope,
            classId,
            schoolId,
            ItemStatus.Lost,
            StringMother.Random("Item"),
            null,
            false,
            GuidMother.Random(),
            this.Clock.Object);
}
