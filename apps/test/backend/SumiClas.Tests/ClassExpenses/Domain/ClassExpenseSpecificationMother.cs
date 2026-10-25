namespace SumiClas.Tests.ClassExpenses.Domain;

using SumiClas.ClassExpenses.Domain;

public static class ClassExpenseSpecificationMother
{
    public static ClassExpenseSpecification OwedBy(Guid guardianId)
        => new(participantId: guardianId, participantHasPaid: false);
}
