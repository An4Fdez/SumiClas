namespace SumiClas.Tests.ClassExpenses.Domain;

using AwesomeAssertions;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ClassExpenseShould
{
    [Test]
    public void have_payments_as_soon_as_one_participant_has_paid()
    {
        ClassExpense classExpense = ClassExpenseMother.With(
            participants:
            [
                ExpenseParticipantMother.Unpaid(GuidMother.Random()),
                ExpenseParticipantMother.Paid(GuidMother.Random()),
                ExpenseParticipantMother.Unpaid(GuidMother.Random()),
            ]);

        classExpense.HasPayments.Should().BeTrue();
    }

    [Test]
    public void have_no_payments_while_every_participant_is_pending()
    {
        ClassExpense classExpense = ClassExpenseMother.With(
            participants:
            [
                ExpenseParticipantMother.Unpaid(GuidMother.Random()),
                ExpenseParticipantMother.Unpaid(GuidMother.Random()),
            ]);

        classExpense.HasPayments.Should().BeFalse();
    }

    [Test]
    public void have_no_payments_again_when_the_only_payment_is_undone()
    {
        Guid guardianId = GuidMother.Random();
        ClassExpense classExpense = ClassExpenseMother.With(participants: [ExpenseParticipantMother.Paid(guardianId)]);

        classExpense.MarkAsUnpaid(guardianId);

        classExpense.HasPayments.Should().BeFalse();
    }
}
