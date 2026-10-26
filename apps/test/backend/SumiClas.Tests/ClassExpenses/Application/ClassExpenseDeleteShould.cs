namespace SumiClas.Tests.ClassExpenses.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.ClassExpenses.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Tests.ClassExpenses.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassExpenseDeleteShould : ClassExpensesUnitTestCase
{
    private readonly ClassExpenseDelete.Delete _useCase;

    public ClassExpenseDeleteShould()
    {
        this._useCase = new ClassExpenseDelete.Delete(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task remove_only_the_requested_expense_when_its_author_deletes_it_and_nobody_has_paid()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        ClassExpense classExpense = ClassExpenseMother.With(
            classId: classId,
            authorId: authorId,
            participants: [ExpenseParticipantMother.Unpaid(GuidMother.Random())]);
        ClassExpense other = ClassExpenseMother.With(authorId: authorId);
        this.ShouldFetchClassExpenses(other, classExpense);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);

        await this._useCase.ExecuteAsync(ClassExpenseDeleteCommandMother.WithClassExpenseId(classExpense.Id));

        this.ShouldHaveRemoved(classExpense);
        this.Repository.Verify(repository => repository.Remove(other), Times.Never);
        this.ShouldHaveEnsuredSchoolYearIsActive(schoolYearId);
    }

    [Test]
    public async Task throw_has_payments_without_removing_when_any_participant_has_already_paid()
    {
        Guid authorId = GuidMother.Random();
        ClassExpense classExpense = ClassExpenseMother.With(
            authorId: authorId,
            participants:
            [
                ExpenseParticipantMother.Unpaid(GuidMother.Random()),
                ExpenseParticipantMother.Paid(GuidMother.Random()),
            ]);
        this.ShouldFetchClassExpenses(classExpense);
        this.ShouldResolveCurrentUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseDeleteCommandMother.WithClassExpenseId(classExpense.Id));

        await act.Should().ThrowAsync<ClassExpenseExceptions.HasPayments>();
        this.ShouldNotHaveRemoved();
    }

    [Test]
    public async Task throw_change_forbidden_before_looking_at_the_payments_when_the_current_user_is_not_the_author()
    {
        ClassExpense classExpense = ClassExpenseMother.With(
            participants: [ExpenseParticipantMother.Paid(GuidMother.Random())]);
        this.ShouldFetchClassExpenses(classExpense);
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            ClassExpenseDeleteCommandMother.WithClassExpenseId(classExpense.Id));

        await act.Should().ThrowAsync<ClassExpenseExceptions.ChangeForbidden>();
        this.ShouldNotHaveRemoved();
    }
}
