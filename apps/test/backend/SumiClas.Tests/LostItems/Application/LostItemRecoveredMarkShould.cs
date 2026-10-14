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
public class LostItemRecoveredMarkShould : LostItemsUnitTestCase
{
    private readonly LostItemRecoveredMark.Mark _useCase;

    public LostItemRecoveredMarkShould()
    {
        this._useCase = new LostItemRecoveredMark.Mark(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task mark_the_school_item_as_recovered_without_asking_for_any_class()
    {
        LostItem lostItem = LostItemMother.With();
        this.ShouldFetchLostItems(lostItem);

        Result result = await this._useCase.ExecuteAsync(LostItemRecoveredMarkCommandMother.With(lostItem.Id));

        result.IsSuccess.Should().BeTrue();
        lostItem.Recovered.Should().BeTrue();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task mark_the_class_item_as_recovered_when_the_school_year_is_active()
    {
        Guid classId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchLostItems(lostItem);

        Result result = await this._useCase.ExecuteAsync(LostItemRecoveredMarkCommandMother.With(lostItem.Id));

        result.IsSuccess.Should().BeTrue();
        lostItem.Recovered.Should().BeTrue();
    }

    [Test]
    public async Task keep_the_item_as_recovered_when_it_was_already_recovered()
    {
        LostItem lostItem = LostItemMother.With(recovered: true);
        this.ShouldFetchLostItems(lostItem);

        Result result = await this._useCase.ExecuteAsync(LostItemRecoveredMarkCommandMother.With(lostItem.Id));

        result.IsSuccess.Should().BeTrue();
        lostItem.Recovered.Should().BeTrue();
    }

    [Test]
    public async Task not_ask_for_the_current_user_nor_add_any_item()
    {
        LostItem lostItem = LostItemMother.With();
        this.ShouldFetchLostItems(lostItem);
        this.ShouldCaptureAddedLostItems();

        await this._useCase.ExecuteAsync(LostItemRecoveredMarkCommandMother.With(lostItem.Id));

        this.UserProvider.VerifyNoOtherCalls();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_not_found_without_asking_when_the_item_does_not_exist()
    {
        this.ShouldFetchNoLostItems();

        Result result = await this._useCase.ExecuteAsync(
            LostItemRecoveredMarkCommandMother.With(GuidMother.Random()));

        result.Error.Should().Be(LostItemErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random());
        this.ShouldNotFindClass();
        this.ShouldFetchLostItems(lostItem);

        Result result = await this._useCase.ExecuteAsync(LostItemRecoveredMarkCommandMother.With(lostItem.Id));

        result.Error.Should().Be(ClassErrors.NotFound);
        lostItem.Recovered.Should().BeFalse();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task return_school_year_archived_without_marking_when_the_school_year_is_archived()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);
        this.ShouldFetchLostItems(lostItem);

        Result result = await this._useCase.ExecuteAsync(LostItemRecoveredMarkCommandMother.With(lostItem.Id));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        lostItem.Recovered.Should().BeFalse();
    }

    [Test]
    public async Task return_school_year_not_found_without_marking_when_the_school_year_does_not_exist()
    {
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(classId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.NotFound);
        this.ShouldFetchLostItems(lostItem);

        Result result = await this._useCase.ExecuteAsync(LostItemRecoveredMarkCommandMother.With(lostItem.Id));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
        lostItem.Recovered.Should().BeFalse();
    }
}
