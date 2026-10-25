namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ClassExpensePendingSummaryResponseMother
{
    public static ClassExpensePendingSummaryResponse Owing(string item) => new(Pending: true, Item: item);

    public static ClassExpensePendingSummaryResponse NotOwing() => new(Pending: false, Item: null);
}
