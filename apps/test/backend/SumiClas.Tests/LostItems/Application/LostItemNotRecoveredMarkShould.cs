namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Classes.Domain;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LostItemNotRecoveredMarkShould : LostItemsUnitTestCase
{
    private readonly LostItemNotRecoveredMark.Mark _useCase;

    public LostItemNotRecoveredMarkShould()
    {
        this._useCase = new LostItemNotRecoveredMark.Mark(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task mark_the_school_item_as_not_recovered_without_asking_for_any_class()
    {
        LostItem lostItem = LostItemMother.With(recovered: true);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);

        Result result = await this._useCase.ExecuteAsync(
            LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        result.IsSuccess.Should().BeTrue();
        lostItem.Recovered.Should().BeFalse();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task mark_the_class_item_as_not_recovered_when_the_school_year_is_active()
    {
        Guid classId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId, recovered: true);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);

        Result result = await this._useCase.ExecuteAsync(
            LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        result.IsSuccess.Should().BeTrue();
        lostItem.Recovered.Should().BeFalse();
    }

    [Test]
    public async Task keep_the_item_as_not_recovered_when_it_was_already_not_recovered()
    {
        LostItem lostItem = LostItemMother.With();
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);

        Result result = await this._useCase.ExecuteAsync(
            LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        result.IsSuccess.Should().BeTrue();
        lostItem.Recovered.Should().BeFalse();
    }

    [Test]
    public async Task not_add_any_item()
    {
        LostItem lostItem = LostItemMother.With(recovered: true);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);
        this.ShouldCaptureAddedLostItems();

        await this._useCase.ExecuteAsync(LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_not_found_without_asking_when_the_item_does_not_exist()
    {
        this.ShouldFetchNoLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemNotRecoveredMarkCommandMother.With(GuidMother.Random()));

        result.Error.Should().Be(LostItemErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), recovered: true);
        this.ShouldNotFindClass();
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        lostItem.Recovered.Should().BeTrue();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_archived_without_marking_when_the_school_year_is_archived()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId, recovered: true);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        lostItem.Recovered.Should().BeTrue();
    }

    [Test]
    public async Task throw_school_year_not_found_without_marking_when_the_school_year_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId, recovered: true);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(lostItem.AuthorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        lostItem.Recovered.Should().BeTrue();
    }

    [Test]
    public async Task return_mark_forbidden_and_keep_the_state_when_the_user_is_not_the_author()
    {
        LostItem lostItem = LostItemMother.With(recovered: true);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Result result = await this._useCase.ExecuteAsync(LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        result.Error.Should().Be(LostItemErrors.MarkForbidden);
        lostItem.Recovered.Should().BeTrue();
    }

    [Test]
    public async Task return_unauthenticated_and_keep_the_state_when_there_is_no_current_user()
    {
        LostItem lostItem = LostItemMother.With(recovered: true);
        this.ShouldFetchLostItems(lostItem);
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(LostItemNotRecoveredMarkCommandMother.With(lostItem.Id));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        lostItem.Recovered.Should().BeTrue();
    }
}
