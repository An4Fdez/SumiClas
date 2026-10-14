namespace SumiClas.Tests.ClassExpenses.Domain;

using SumiClas.ClassExpenses.Domain;

public static class ExpenseParticipantMother
{
    public static ExpenseParticipant Unpaid(Guid guardianId) => new(guardianId, false);

    public static ExpenseParticipant Paid(Guid guardianId) => new(guardianId, true);
}
