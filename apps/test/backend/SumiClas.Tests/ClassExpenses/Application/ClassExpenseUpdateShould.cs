namespace SumiClas.Tests.ClassExpenses.Application;

using AwesomeAssertions;
using SumiClas.ClassExpenses.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Tests.ClassExpenses.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassExpenseUpdateShould : ClassExpensesUnitTestCase
{
    private readonly ClassExpenseUpdate.Update _useCase;

    public ClassExpenseUpdateShould()
    {
        this._useCase = new ClassExpenseUpdate.Update(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task throw_item_required_without_asking_the_bus_when_the_new_concept_is_blank()
    {
        Guid authorId = GuidMother.Random();
        ClassExpense classExpense = ClassExpenseMother.With(authorId: authorId);
        string item = classExpense.Item;
        this.ShouldFetchClassExpenses(classExpense);
        this.ShouldResolveCurrentUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseUpdateCommandMother.WithRelatedEvent(classExpense.Id, item: " "));

        await act.Should().ThrowAsync<ClassExpenseExceptions.ItemRequired>();
        classExpense.Item.Should().Be(item);
        this.QueryBus.VerifyNoOtherCalls();
    }
}
