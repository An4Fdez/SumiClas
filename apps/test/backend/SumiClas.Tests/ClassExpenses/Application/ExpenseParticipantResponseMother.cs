namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ExpenseParticipantResponseMother
{
    public static ExpenseParticipantResponse With(Guid guardianId, string fullName, bool hasPaid)
        => new(guardianId, fullName, hasPaid);
}
