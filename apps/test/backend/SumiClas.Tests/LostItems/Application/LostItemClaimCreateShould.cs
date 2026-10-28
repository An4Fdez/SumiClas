namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Domain;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class LostItemClaimCreateShould : LostItemsUnitTestCase
{
    private readonly LostItemClaimCreate.Create _useCase;

    public LostItemClaimCreateShould()
    {
        this._useCase = new LostItemClaimCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_a_pending_claim_of_the_current_user_after_checking_membership_and_the_school_year()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid current = GuidMother.Random();
        Guid claimId = GuidMother.Random();
        string message = StringMother.Random("detail");
        LostItem lostItem = LostItemMother.ForClass(classId);
        this.ShouldResolveCurrentUser(current);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(current, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        await this._useCase.ExecuteAsync(LostItemClaimCreateCommandMother.With(claimId, lostItem.Id, message));

        LostItemClaim claim = lostItem.Claims.Should().ContainSingle().Subject;
        claim.Id.Should().Be(claimId);
        claim.ClaimantId.Should().Be(current);
        claim.Message.Should().Be(message);
        claim.Status.Should().Be(ClaimStatus.Pending);
        claim.CreatedAt.Should().Be(Now);
        this.ShouldHaveEnsuredClassAccess(current, classId);
        this.ShouldHaveEnsuredSchoolYearIsActive(schoolYearId);
    }

    [Test]
    public async Task throw_not_found_without_asking_when_the_item_does_not_exist()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFetchNoLostItems();

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimCreateCommandMother.For(GuidMother.Random()));

        await act.Should().ThrowAsync<LostItemExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_claim_class_required_without_asking_when_the_item_belongs_to_the_school()
    {
        LostItem lostItem = LostItemMother.With();
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFetchLostItems(lostItem);

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimCreateCommandMother.For(lostItem.Id));

        await act.Should().ThrowAsync<LostItemExceptions.ClaimClassRequired>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_claiming_when_the_user_is_not_a_member_of_the_class()
    {
        Guid classId = GuidMother.Random();
        Guid current = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId);
        this.ShouldResolveCurrentUser(current);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldFindClass(classId);
        this.ShouldDenyClassAccess(current, classId);

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimCreateCommandMother.For(lostItem.Id));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        lostItem.Claims.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_archived_without_claiming_when_the_school_year_is_archived()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid current = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId);
        this.ShouldResolveCurrentUser(current);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(current, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimCreateCommandMother.For(lostItem.Id));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        lostItem.Claims.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_not_found_when_the_class_of_the_item_does_not_exist()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random());
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldFetchLostItems(lostItem);
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimCreateCommandMother.For(lostItem.Id));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
    }

    [Test]
    public async Task throw_unauthenticated_without_looking_for_the_item_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemClaimCreateCommandMother.For(GuidMother.Random()));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.Repository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
