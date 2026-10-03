using SumiClas.Shared.Domain;

namespace SumiClas.ClassExpenses.Domain;

public static class ClassExpenseErrors
{
    public static readonly Error IdRequired = new("ClassExpense.IdRequired", "El identificador del gasto no puede estar vacío.");
    public static readonly Error ParticipantNotFound = new("ClassExpense.ParticipantNotFound", "Alguno de los participantes no existe.");
    public static readonly Error ClassRequired = new("ClassExpense.ClassRequired", "El gasto debe pertenecer a una clase.");
    public static readonly Error ItemRequired = new("ClassExpense.ItemRequired", "El concepto del gasto no puede estar vacío.");
    public static readonly Error TotalAmountMustBePositive = new("ClassExpense.TotalAmountMustBePositive", "El importe total debe ser mayor que cero.");
    public static readonly Error AtLeastOneParticipantRequired = new("ClassExpense.AtLeastOneParticipantRequired", "El gasto debe tener al menos un participante.");
    public static readonly Error AuthorRequired = new("ClassExpense.AuthorRequired", "El gasto debe tener un autor.");
    public static readonly Error NotAParticipant = new("ClassExpense.NotAParticipant", "Este usuario no participa en el gasto.");
}
