namespace SumiClas.Tests.LostItems.Application;

using AwesomeAssertions;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.Tests.LostItems.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LostItemUpdateShould : LostItemsUnitTestCase
{
    private readonly LostItemUpdate.Update _useCase;

    public LostItemUpdateShould()
    {
        this._useCase = new LostItemUpdate.Update(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task throw_description_required_without_asking_the_bus_when_the_new_description_is_blank()
    {
        Guid authorId = GuidMother.Random();
        LostItem lostItem = LostItemMother.ForClass(GuidMother.Random(), authorId: authorId);
        string description = lostItem.Description;
        this.ShouldFetchLostItems(lostItem);
        this.ShouldResolveCurrentUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(LostItemUpdateCommandMother.With(lostItem.Id, " "));

        await act.Should().ThrowAsync<LostItemExceptions.DescriptionRequired>();
        lostItem.Description.Should().Be(description);
        this.QueryBus.VerifyNoOtherCalls();
    }
}
